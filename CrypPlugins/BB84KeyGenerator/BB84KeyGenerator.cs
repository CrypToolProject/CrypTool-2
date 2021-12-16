/*
   Copyright 2011 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using BB84KeyGenerator;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.Plugins.BB84KeyGenerator
{
    [Author("Benedict Beuscher", "benedict.beuscher@stud.uni-due.de", "Uni Duisburg-Essen", "http://www.uni-due.de/")]

    [PluginInfo("CrypTool.Plugins.BB84KeyGenerator.Properties.Resources", "res_GeneratorCaption", "res_GeneratorTooltip", "BB84KeyGenerator/userdoc.xml", new[] { "BB84KeyGenerator/images/icon.png" })]
    [ComponentCategory(ComponentCategory.Protocols)]
    public class BB84KeyGenerator : ICrypComponent
    {
        #region Private Variables

        private readonly BB84KeyGeneratorSettings settings = new BB84KeyGeneratorSettings();
        private string inputKey;
        private string inputBasesFirst;
        private string inputBasesSecond;
        private string outputCommonKey;
        private readonly BB84KeyGeneratorPresentation myPresentation;

        #endregion


        public BB84KeyGenerator()
        {
            myPresentation = new BB84KeyGeneratorPresentation();
            myPresentation.UpdateProgess += new EventHandler(update_progress);
            Presentation = myPresentation;

        }
        private void update_progress(object sender, EventArgs e)
        {
            ProgressChanged(myPresentation.Progress, 3000);
        }

        #region Data Properties

        [PropertyInfo(Direction.InputData, "res_InputKeyCaption", "res_InputKeyTooltip")]
        public string InputKey
        {
            get => inputKey;

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }
                string newInput = filterValidInput(value);
                inputKey = newInput;
            }
        }

        private string filterValidInput(string value)
        {
            StringBuilder outputString = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i].Equals('0') || value[i].Equals('1'))
                {
                    outputString.Append(value[i]);
                }
            }

            return outputString.ToString();
        }

        [PropertyInfo(Direction.InputData, "res_InputFirstBasesCaption", "res_InputFirstBasesTooltip")]
        public string InputBasesFirst
        {
            get => inputBasesFirst;

            set => inputBasesFirst = value;
        }

        [PropertyInfo(Direction.InputData, "res_InputSecondBasesCaption", "res_InputSecondBaseTooltip")]
        public string InputBasesSecond
        {
            get => inputBasesSecond;

            set => inputBasesSecond = value;
        }

        [PropertyInfo(Direction.InputData, "res_PhotonInputCaption", "res_PhotonInputTooltip", true)]
        public string InputPhotons
        {
            get => null;
            set
            {
            }
        }

        [PropertyInfo(Direction.OutputData, "res_CommonKeyCaption", "res_CommonKeyTooltip")]
        public string OutputCommonKey
        {
            get => outputCommonKey;
            set { } //read-only

        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings => settings;

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation
        {
            get;
            private set;
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            myPresentation.speed = settings.SpeedSetting;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);
            generateCommonKey();
            showPresentationIfVisible();
            notifyOutput();
            ProgressChanged(1, 1);
        }


        private void generateCommonKey()
        {
            StringBuilder tempOutput = new StringBuilder();

            for (int i = 0; i < inputKey.Length; i++)
            {
                if (inputBasesFirst.Length > i && inputBasesSecond.Length > i && inputKey.Length > i)
                {
                    if (inputBasesFirst[i].Equals(inputBasesSecond[i]))
                    {
                        tempOutput.Append(inputKey[i]);
                    }
                }
            }
            outputCommonKey = tempOutput.ToString();
        }

        private void showPresentationIfVisible()
        {
            if (Presentation.IsVisible)
            {
                Presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        if (!myPresentation.hasFinished)
                        {
                            myPresentation.StopPresentation();
                        }
                        myPresentation.StartPresentation(inputBasesFirst, inputBasesSecond, inputKey);
                    }
                    catch (Exception e)
                    {
                        GuiLogMessage("Problem beim Ausführen des Dispatchers :" + e.Message, NotificationLevel.Error);
                    }
                }, null);
            }

        }


        private void notifyOutput()
        {
            OnPropertyChanged("OutputCommonKey");
        }

        public void PostExecution()
        {
            Presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    myPresentation.StopPresentation();
                }
                catch (Exception e)
                {
                    GuiLogMessage("Problem beim Ausführen des Dispatchers :" + e.Message, NotificationLevel.Error);
                }
            }, null);
        }

        public void Stop()
        {
            Presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    myPresentation.StopPresentation();
                }
                catch (Exception e)
                {
                    GuiLogMessage("Problem beim Ausführen des Dispatchers :" + e.Message, NotificationLevel.Error);
                }
            }, null);
        }


        public void Initialize()
        {
            settings.SpeedSetting = 1.0;
        }

        public void Dispose()
        {
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
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
