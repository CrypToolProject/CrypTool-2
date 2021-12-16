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
using BB84ManInTheMiddle;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.Plugins.BB84ManInTheMiddle
{
    [Author("Benedict Beuscher", "benedict.beuscher@stud.uni-due.de", "Uni Duisburg-Essen", "http://www.uni-due.de/")]


    [PluginInfo("CrypTool.Plugins.BB84ManInTheMiddle.Properties.Resources", "res_MITMCaption", "res_MITMTooltip", "BB84ManInTheMiddle/userdoc.xml", new[] { "BB84ManInTheMiddle/images/icon.png" })]

    [ComponentCategory(ComponentCategory.Protocols)]
    public class BB84ManInTheMiddle : ICrypComponent
    {
        #region Private Variables and Constructor

        private string inputPhotons;
        private string inputBases;
        private string outputPhotons;
        private string outputKey;
        private readonly BB84ManInTheMiddlePresentation myPresentation;
        private RNGCryptoServiceProvider sRandom;


        private readonly BB84ManInTheMiddleSettings settings = new BB84ManInTheMiddleSettings();

        public BB84ManInTheMiddle()
        {
            myPresentation = new BB84ManInTheMiddlePresentation();
            myPresentation.UpdateProgress += new EventHandler(update_progress);
            Presentation = myPresentation;
        }

        private void update_progress(object sender, EventArgs e)
        {
            ProgressChanged(myPresentation.Progress, 3000);
        }

        #endregion


        #region Data Properties

        [PropertyInfo(Direction.InputData, "res_PhotonInputCaption", "res_PhotonInputTooltip", true)]
        public string InputPhotons
        {
            get => inputPhotons;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }
                if (!value.Equals(inputPhotons))
                {
                    inputPhotons = value;
                }
            }
        }

        [PropertyInfo(Direction.InputData, "res_BasesInputCaption", "res_BasesInputTooltip", true)]
        public string InputBases
        {
            get => inputBases;

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }
                if (!value.Equals(inputBases))
                {
                    inputBases = value;
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "res_KeyOutputCaption", "res_KeyOutputTooltip")]
        public string OutputKey
        {
            get => outputKey;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }
                if (!value.Equals(outputKey))
                {
                    outputKey = value;
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "res_PhotonOutputCaption", "res_PhotonOutputTooltip")]
        public string OutputPhotons
        {
            get => outputPhotons;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }
                if (!value.Equals(outputPhotons))
                {
                    outputPhotons = value;
                }
            }
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings => settings;

        public UserControl Presentation
        {
            get;
            private set;
        }

        public void PreExecution()
        {
            myPresentation.SpeedFactor = settings.SpeedSetting;
        }

        public void Execute()
        {
            ProgressChanged(0, 1);

            if (settings.IsListening == 0)
            {
                decodeIncomingPhotons();
                forwardListenedPhotons();
            }
            else
            {
                forwardReceivedPhotons();
                displaySleepMessage();
            }

            notifyOutputs();

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

                            if (settings.WaitingIterations != 0)
                            {
                                StringBuilder waitingStringBuilder = new StringBuilder();
                                for (int i = 0; i < settings.WaitingIterations; i++)
                                {
                                    waitingStringBuilder.Append('W');
                                }

                                string waitingString = waitingStringBuilder.ToString();

                                inputPhotons = waitingString + inputPhotons;
                                inputBases = waitingString + inputBases;
                                outputPhotons = waitingString + outputPhotons;

                            }


                            if (settings.IsListening == 0)
                            {
                                myPresentation.StartPresentation(inputPhotons, inputBases, outputPhotons, true);
                            }
                            else
                            {
                                myPresentation.StartPresentation(inputPhotons, inputBases, outputPhotons, false);
                            }
                        }
                        catch (Exception e)
                        {
                            GuiLogMessage("Problem beim Ausführen des Dispatchers :" + e.Message, NotificationLevel.Error);
                        }
                    }, null);
            }
        }





        private void decodeIncomingPhotons()
        {
            StringBuilder listenedKey = new StringBuilder();


            for (int i = 0; i < inputPhotons.Length; i++)
            {
                if (inputPhotons.Length > i && inputBases.Length > i)
                {
                    listenedKey.Append(decodePhoton(inputPhotons[i], inputBases[i]));
                }
            }

            outputKey = listenedKey.ToString();

        }

        private string decodePhoton(char photon, char pbase)
        {
            string returnBit = "";

            if (pbase.Equals('+'))
            {
                if (photon.Equals('|'))
                {
                    returnBit = settings.PlusVerticallyDecoding;
                }
                else if (photon.Equals('-'))
                {
                    returnBit = settings.PlusHorizontallyDecoding;
                }
                else
                {
                    returnBit = getRandomBit();
                }
            }
            else if (pbase.Equals('x'))
            {
                if (photon.Equals('/'))
                {
                    returnBit = settings.XTopRightDiagonallyDecoding;
                }
                else if (photon.Equals('\\'))
                {
                    returnBit = settings.XTopLeftDiagonallyDecoding;
                }
                else
                {
                    returnBit = getRandomBit();
                }
            }
            return returnBit;
        }

        private string getRandomBit()
        {
            sRandom = new RNGCryptoServiceProvider();
            byte[] buffer = new byte[4];
            sRandom.GetBytes(buffer);
            int result = BitConverter.ToInt32(buffer, 0);
            string returnString = "" + new Random(result).Next(2);
            return returnString;
        }

        private void forwardListenedPhotons()
        {
            string photonsToSend = "";
            for (int i = 0; i < outputKey.Length; i++)
            {
                if (outputKey.Length > i && inputBases.Length > i)
                {
                    photonsToSend += getPhotonFromBit(outputKey[i], inputBases[i]);
                }
            }

            outputPhotons = photonsToSend;

        }

        private string getPhotonFromBit(char bit, char pbase)
        {
            string returnPhoton = "";

            if (pbase.Equals('+'))
            {

                if (settings.PlusHorizontallyDecoding[0].Equals(bit))
                {

                    returnPhoton = "-";
                }
                else if (settings.PlusVerticallyDecoding[0].Equals(bit))
                {

                    returnPhoton = "|";
                }
            }
            else if (pbase.Equals('x'))
            {

                if (settings.XTopLeftDiagonallyDecoding[0].Equals(bit))
                {

                    returnPhoton = "\\";
                }
                else if (settings.XTopRightDiagonallyDecoding[0].Equals(bit))
                {

                    returnPhoton = "/";
                }
            }
            return returnPhoton;

        }

        private void forwardReceivedPhotons()
        {
            OutputPhotons = inputPhotons;

        }

        private void displaySleepMessage()
        {
            outputKey = "Man in the middle is sleeping!";
        }


        private void notifyOutputs()
        {
            OnPropertyChanged("OutputPhotons");
            OnPropertyChanged("OutputKey");
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
            settings.WaitingIterations = 1;
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
