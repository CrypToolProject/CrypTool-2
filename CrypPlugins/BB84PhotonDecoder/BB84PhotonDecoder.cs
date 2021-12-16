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
using BB84PhotonDecoder;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.Plugins.BB84PhotonDecoder
{
    [Author("Benedict Beuscher", "benedict.beuscher@stud.uni-due.de", "Uni Duisburg-Essen", "http://www.uni-due.de/")]

    [PluginInfo("CrypTool.Plugins.BB84PhotonDecoder.Properties.Resources", "res_photonDecodingCaption", "res_photonDecodingTooltip", "BB84PhotonDecoder/userdoc.xml", new[] { "BB84PhotonDecoder/images/icon.png" })]

    [ComponentCategory(ComponentCategory.Protocols)]
    public class BB84PhotonDecoder : ICrypComponent
    {
        #region Private Variables

        public bool synchron;

        private readonly BB84PhotonDecoderSettings settings = new BB84PhotonDecoderSettings();
        private string inputPhotons;
        private string inputBases;
        private string outputKey;
        private System.Random newRandom;
        private readonly BB84PhotonDecoderPresentation myPresentation;

        private RNGCryptoServiceProvider sRandom;

        #endregion

        public BB84PhotonDecoder()
        {
            synchron = true;
            myPresentation = new BB84PhotonDecoderPresentation();
            myPresentation.UpdateProgess += new EventHandler(update_progress);
            Presentation = myPresentation;
        }
        private void update_progress(object sender, EventArgs e)
        {
            ProgressChanged(myPresentation.Progress, 3000);
        }
        #region Data Properties

        [PropertyInfo(Direction.InputData, "res_photonInputCaption", "res_photonInputTooltip", true)]
        public string InputPhotons
        {
            get => inputPhotons;
            set => inputPhotons = value;
        }

        [PropertyInfo(Direction.InputData, "res_basesInputCaption", "res_basesInputTooltip", true)]
        public string InputBases
        {
            get => inputBases;
            set => inputBases = value;
        }

        [PropertyInfo(Direction.OutputData, "res_keyOutputCaption", "res_keyOutputTooltip", true)]
        public string OutputKey
        {
            get => outputKey;
            set
            { } //readonly
        }
        #endregion

        #region IPlugin Members

        public UserControl Presentation
        {
            get;
            private set;
        }

        public ISettings Settings => settings;


        public void PreExecution()
        {
            myPresentation.SpeedFactor = settings.SpeedSetting;
        }

        public void Execute()
        {
            ProgressChanged(0, 1);
            if (settings.ErrorsEnabled == 0 || (int)Math.Round(inputPhotons.Length * (((double)settings.ErrorRatio) / 100)) == 0)
            {
                doNormalDecoding();
            }
            else
            {
                doDecodingWithErrors();
            }

            OnPropertyChanged("OutputKey");
            startPresentationIfVisible();
            ProgressChanged(1, 1);
        }

        private void startPresentationIfVisible()
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


                            StringBuilder waitingStringBuilder = new StringBuilder();
                            for (int i = 0; i < settings.WaitingIterations; i++)
                            {
                                waitingStringBuilder.Append('W');
                            }

                            string waitingString = waitingStringBuilder.ToString();


                            myPresentation.StartPresentation(waitingString + outputKey, waitingString + inputPhotons, waitingString + inputBases);

                        }
                        catch (Exception e)
                        {
                            GuiLogMessage("Problem beim Ausführen des Dispatchers :" + e.Message, NotificationLevel.Error);
                        }

                    }, null);


                while (!myPresentation.hasFinished)
                {
                    ProgressChanged(myPresentation.animationRepeats, inputBases.Length);
                }



            }
        }



        private void doDecodingWithErrors()
        {
            StringBuilder tempOutput = new StringBuilder();
            newRandom = new System.Random(DateTime.Now.Millisecond);
            newRandom.Next(2);
            char[] tempBases = inputBases.ToCharArray();
            char[] tempPhotons = inputPhotons.ToCharArray();

            // int fail = (int)Math.Round(inputPhotons.Length * (((double)settings.ErrorRatio)/100));
            int fail = 100 / settings.ErrorRatio;
            for (int i = 0; i < inputPhotons.Length; i++)
            {

                if (i % fail == 0)
                {
                    if (tempBases.Length > i && tempPhotons.Length > i)
                    {
                        if (tempBases[i].Equals('+'))
                        {
                            if (tempPhotons[i].Equals('|'))
                            {
                                tempOutput.Append(settings.PlusHorizontallyDecoding);
                            }
                            else if (tempPhotons[i].Equals('-'))
                            {
                                tempOutput.Append(settings.PlusVerticallyDecoding);
                            }
                            else
                            {
                                tempOutput.Append(getRandomBinary());
                            }
                        }
                        else if (tempBases[i].Equals('x'))
                        {
                            if (tempPhotons[i].Equals('\\'))
                            {
                                tempOutput.Append(settings.XTopRightDiagonallyDecoding);
                            }
                            else if (tempPhotons[i].Equals('/'))
                            {
                                tempOutput.Append(settings.XTopLeftDiagonallyDecoding);
                            }
                            else
                            {
                                tempOutput.Append(getRandomBinary());
                            }
                        }
                    }
                }
                else
                {
                    if (tempBases.Length > i && tempPhotons.Length > i)
                    {
                        if (tempBases[i].Equals('+'))
                        {
                            if (tempPhotons[i].Equals('|'))
                            {
                                tempOutput.Append(settings.PlusVerticallyDecoding);
                            }
                            else if (tempPhotons[i].Equals('-'))
                            {
                                tempOutput.Append(settings.PlusHorizontallyDecoding);
                            }
                            else
                            {
                                tempOutput.Append(getRandomBinary());
                            }
                        }
                        else if (tempBases[i].Equals('x'))
                        {
                            if (tempPhotons[i].Equals('\\'))
                            {
                                tempOutput.Append(settings.XTopLeftDiagonallyDecoding);
                            }
                            else if (tempPhotons[i].Equals('/'))
                            {
                                tempOutput.Append(settings.XTopRightDiagonallyDecoding);
                            }
                            else
                            {
                                tempOutput.Append(getRandomBinary());
                            }
                        }
                    }
                }
            }
            outputKey = tempOutput.ToString();
        }

        private void doNormalDecoding()
        {
            StringBuilder tempOutput = new StringBuilder();
            newRandom = new System.Random(DateTime.Now.Millisecond);
            newRandom.Next(2);
            char[] tempBases = inputBases.ToCharArray();
            char[] tempPhotons = inputPhotons.ToCharArray();

            for (int i = 0; i < inputPhotons.Length; i++)
            {
                if (tempBases.Length > i && tempPhotons.Length > i)
                {
                    if (tempBases[i].Equals('+'))
                    {
                        if (tempPhotons[i].Equals('|'))
                        {
                            tempOutput.Append(settings.PlusVerticallyDecoding);
                        }
                        else if (tempPhotons[i].Equals('-'))
                        {
                            tempOutput.Append(settings.PlusHorizontallyDecoding);
                        }
                        else
                        {
                            tempOutput.Append(getRandomBinary());
                        }
                    }
                    else if (tempBases[i].Equals('x'))
                    {
                        if (tempPhotons[i].Equals('\\'))
                        {
                            tempOutput.Append(settings.XTopLeftDiagonallyDecoding);
                        }
                        else if (tempPhotons[i].Equals('/'))
                        {
                            tempOutput.Append(settings.XTopRightDiagonallyDecoding);
                        }
                        else
                        {
                            tempOutput.Append(getRandomBinary());
                        }
                    }
                }
            }
            outputKey = tempOutput.ToString();
        }

        private string getRandomBinary()
        {
            sRandom = new RNGCryptoServiceProvider();
            byte[] buffer = new byte[4];
            sRandom.GetBytes(buffer);
            int result = BitConverter.ToInt32(buffer, 0);
            string returnString = "" + new Random(result).Next(2);
            return returnString;

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
            settings.PlusVerticallyDecoding = "0";
            settings.XTopRightDiagonallyDecoding = "0";
            settings.XTopLeftDiagonallyDecoding = "1";
            settings.PlusHorizontallyDecoding = "1";
            settings.SpeedSetting = 1;
            //settings.WaitingIterations = 2;
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
