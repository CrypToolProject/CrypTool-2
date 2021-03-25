/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System.ComponentModel;
using System.Windows.Controls;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using FleißnerGrilleGenerator;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Media;
using FleißnerGrilleGenerator.Properties;

namespace CrypTool.Plugins.FleißnerGrilleGenerator
{
    [Author("Robert Rauer", "robert_rauer@yahoo.de", "Universität Kassel", "http://cryptool2.vs.uni-due.de")]
    [PluginInfo("FleißnerGrilleGenerator.Properties.Resources", "PluginCaption", "PluginTooltip", "FleißnerGrilleGenerator/DetailedDescription/doc.xml",
        new[] { "FleißnerGrilleGenerator/Images/FleißnerGrilleGenerator.png" })]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class FleißnerGrilleGenerator : ICrypComponent
    {
        #region Private Variables

        // HOWTO: You need to adapt the settings class as well, see the corresponding file.
        private readonly FleißnerGrilleGeneratorSettings settings = new FleißnerGrilleGeneratorSettings();
        private FleißnerGrilleGeneratorPresentation myPresentation;
        private bool running = false;
        private bool stopped = false;
        private bool isPlayMode = false;
        private string output;
        #endregion

        #region Data Properties

        /// <summary>
        /// Constructor
        /// </summary>
        public FleißnerGrilleGenerator()
        {
            this.settings = new FleißnerGrilleGeneratorSettings();
            myPresentation = new FleißnerGrilleGeneratorPresentation(this);
            Presentation = myPresentation;
            myPresentation.fireEnd += new EventHandler(presentation_finished);
            myPresentation.updateProgress += new EventHandler(update_progress);
            this.settings.LogMessage += FleißnerGrilleGenerator_LogMessage;
        }

        private void update_progress(object sender, EventArgs e)
        {
            //TranspositionPresentation myhelp = new TranspositionPresentation();
            //myhelp = (TranspositionPresentation)sender;
            ProgressChanged(myPresentation.progress, 3000);
        }

        private void presentation_finished(object sender, EventArgs e)
        {
            if (!myPresentation.Stop)
                OutputString = this.output;
            ProgressChanged(1, 1);

            running = false;
        }

        /// <summary>
        /// HOWTO: Input interface to read the input data. 
        /// You can add more input properties of other type if needed.
        /// </summary>
        /// 
        private string _inputString;

        [PropertyInfo(Direction.InputData, "InputStringCaption", "InputStringTooltip", true)]
        public string InputString
        {
            get
            {
                return _inputString;
            }
            set
            {
                _inputString = value;
            }
        }


        private int minGrille(string input)
        {
            int min;
            if (Math.Sqrt(input.Length) - (int)Math.Sqrt(input.Length) > 0) // sqrt from length is odd
            {
                min = ((int)Math.Sqrt(input.Length)) + 1;
            }
            else // sqrt from lenght is even
            {
                min = (int)Math.Sqrt(input.Length);
            }
            return min;
        }

        /// <summary>
        /// HOWTO: Output interface to write the output data.
        /// You can add more output properties ot other type if needed.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", false)]
        public string OutputString
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public System.Windows.Controls.UserControl Presentation
        {
            get;
            private set;
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            while (running)
            {
                myPresentation.my_Stop(this, EventArgs.Empty);
                if (stopped)
                    return;
            }

            isPlayMode = true;

            ProgressChanged(0, 100);

            if (!string.IsNullOrEmpty(InputString))
            {
                ProcessFleißnerGenerator();
                OnPropertyChanged("OutputString"); // push output to editor

                //TODO: generateRandomGrille();
                // HOWTO: You can pass error, warning, info or debug messages to the CT2 main window.
                /*if (settings.SomeParameter < 0)
                    GuiLogMessage("SomeParameter is negative", NotificationLevel.Debug);
                */
                // HOWTO: Make sure the progress bar is at maximum when your Execute() finished successfully.
                ProgressChanged(1, 1);

            }
            else
            {
                FleißnerGrilleGenerator_LogMessage(Resources.INPUTSTRING_EMPTY , NotificationLevel.Error);
                OutputString = null;
                return;
            }

            if (Presentation.IsVisible && !string.IsNullOrEmpty(InputString))
            {
                FleißnerGrilleGenerator_LogMessage("Tester 1", NotificationLevel.Debug);
                Presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    myPresentation.main(this);
                }
               , null);
            }
            else
            {
                OutputString = this.output;
                ProgressChanged(1, 1);
            }
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region private methods
        private void ProcessFleißnerGenerator()
        {
            if (settings.ActionRandom == FleißnerGrilleGeneratorSettings.FleißnerGeneratorRandom.Random)
            {
                OutputString = generateRandomGrille();
            }
            else
            {
                OutputString = generateNotRandomGrille();
            }
        }

        private string generateRandomGrille()
        {
            int[,] grille = fillGrille(); 
            grille = setHolesRandomToGrille(grille);
            string grilleString = getGrilleString(grille);
            return grilleString;
        }

        private string generateNotRandomGrille()
        {
            string grilleString;
            if (settings.grille != null)
            {
                grilleString = getGrilleString(settings.grille);
            }
            else 
            {
                int[,] grille = fillGrille();
                grille = setHolesRandomToGrille(grille);
                grilleString = getGrilleString(grille);
            }
            return grilleString;
        }

        private int[,] setHolesRandomToGrille(int[,] grille)
        {
            int grilleLength = grille.GetLength(0);
            for (int i = 0; i < grilleLength * grilleLength; i = i + 4)
            {
                Random randomH = new Random();
                int random;
                do
                {
                    random = randomH.Next(0, grille.GetLength(0) * grille.GetLength(1) - 1); //random from 0 ... inputLength-1
                } while (getPoint(random, grille));

                int y = (int)(random / grilleLength);
                int x = random - (y * grilleLength);
                grille[x, y] = 1;
                for (int rotate = 0; rotate < 3; rotate++)
                {
                    grille = RotateStencil(grille);
                    grille[x, y] = 2;
                }
            }
            return grille;
        }

        /// <summary>
        // rotate a 2-dimensional Array
        public int[,] RotateStencil(int[,] stencil)
        {
            int stencilLength = (int)Math.Sqrt(stencil.Length);
            int[,] ret = rotate(stencil);
            return ret;
        }

        /// <summary>
        // rotate a 2-dimensional Array
        public int[,] rotate(int[,] stencil)
        {
            int stencilLength = (int)Math.Sqrt(stencil.Length);
            int[,] ret = new int[stencilLength, stencilLength];
            for (int i = 0; i < stencilLength; ++i)
            {
                for (int j = 0; j < stencilLength; ++j)
                {
                    ret[i, j] = stencil[stencilLength - j - 1, i];
                }
            }
            return ret;
        }

        private bool getPoint(int random, int[,] grille)
        {
            int grilleLength = grille.GetLength(0);
            int y = (int)(random / grilleLength);
            int x = random - (y * grilleLength);
            if (grille[x, y] == 0) 
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private int getGrilleSize()
        {
            int inputLength = _inputString.Length;
            if (Math.Sqrt(inputLength) - (int)Math.Sqrt(inputLength) == 0)
            {
                if ((int)Math.Sqrt(inputLength) % 2 == 0) //Die Anzahl der gesamten Felder ist durch 4 teilbar
                {
                    return (int)Math.Sqrt(inputLength);
                }
                else 
                {
                    return (int)Math.Sqrt(inputLength) + 1;
                }
            }
            else
            {
                if (((int)Math.Sqrt(inputLength) + 1) % 2 == 0) // Die Anzahl der gesamten Felder ist durch 4 teilbar
                {
                    return (int)Math.Sqrt(inputLength) + 1;
                }
                else
                {
                    return (int)Math.Sqrt(inputLength) + 2;
                }
            }
        }

        /// <summary>
        /// fills the GrilleWrapPanel with empty Objects
        /// </summary>
        /// <param name="input"></param>
        private int[,] fillGrille()
        {
            int grilleSize = getGrilleSize();
            int[,] grille = new int[grilleSize, grilleSize];
            for (int x = 0; x < grilleSize; x++)
            {
                for (int y = 0; y < grilleSize; y++)
                {
                    grille[x, y] = 0;
                }
            }
            return grille;
        }

        /// <summary>
        /// result of the bool[,] grille as a string
        /// </summary>
        /// <param name="grille"></param>
        /// <returns></returns>
        private string getGrilleString(int[,] grille)
        {
            string result = "";
            for (int x = 0; x < grille.GetLength(0); x++) //columnwise
            {
                for (int y = 0; y < grille.GetLength(0); y++) //rowwise
                {
                    if (grille[x, y]  == 1)
                    {
                        result = result + "1";
                    }
                    else
                    {
                        result = result + "0";
                    }
                }
                //if (y != grille.GetLength(0) - 1)
                //{
                    result = result + "\n";
                //}
            }
            return result;
        }
        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void FleißnerGrilleGenerator_LogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}
