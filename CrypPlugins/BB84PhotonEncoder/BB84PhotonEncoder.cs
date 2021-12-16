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
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.Plugins.BB84PhotonEncoder
{

    [Author("Benedict Beuscher", "benedict.beuscher@stud.uni-due.de", "Uni Duisburg-Essen", "http://www.uni-due.de/")]

    [PluginInfo("CrypTool.Plugins.BB84PhotonEncoder.Properties.Resources", "res_photonEncodingCaption", "res_photonEncodingTooltip", "BB84PhotonEncoder/userdoc.xml", new[] { "BB84PhotonEncoder/images/icon.png" })]

    [ComponentCategory(ComponentCategory.Protocols)]
    public class BB84PhotonEncoder : ICrypComponent
    {
        #region Private Variables and Constructor

        private readonly BB84PhotonEncoderSettings settings = new BB84PhotonEncoderSettings();
        private string inputKey;
        private string inputBases;
        private string photonOutput;
        private readonly BB84PhotonEncoderPresentation myPresentation;
        private readonly int duration;

        public BB84PhotonEncoder()
        {
            myPresentation = new BB84PhotonEncoderPresentation();
            myPresentation.UpdateProgess += new EventHandler(update_progress);
            Presentation = myPresentation;
        }

        private void update_progress(object sender, EventArgs e)
        {
            ProgressChanged(myPresentation.Progress, 3000);
        }
        #endregion


        #region Data Properties

        [PropertyInfo(Direction.InputData, "res_keyInputCaption", "res_keyInputTooltip")]
        public string InputKey
        {
            get => inputKey;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    string newInput = filterValidInput(value);
                    inputKey = newInput;
                }
            }
        }

        private string filterValidInput(string value)
        {
            string outputString = "";
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i].Equals('0') || value[i].Equals('1'))
                {
                    outputString += value[i];
                }
            }

            return outputString;
        }

        [PropertyInfo(Direction.InputData, "res_basesInputCaption", "res_basesInputTooltip")]
        public string InputBases
        {
            get => inputBases;
            set => inputBases = value;
        }


        [PropertyInfo(Direction.OutputData, "res_photonOutputCaption", "res_photonOutputTooltip")]
        public string PhotonOutput
        {
            get => photonOutput;
            set
            { } //readonly
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

            encodeKeyIntoPhotons();

            startPresentationIfVisible();

            notifyOutput();

            ProgressChanged(1, 1);
        }


        private void encodeKeyIntoPhotons()
        {
            StringBuilder tempOutput = new StringBuilder();

            for (int i = 0; i < inputKey.Length; i++)
            {
                if (inputBases.Length > i && inputKey.Length > i)
                {
                    if (inputBases[i].Equals('+'))
                    {
                        if (inputKey[i].Equals('0'))
                        {
                            tempOutput.Append(getPlusBasePhoton(settings.PlusZeroEncoding));
                        }
                        else if (inputKey[i].Equals('1'))
                        {
                            tempOutput.Append(getPlusBasePhoton(settings.PlusOneEncoding));
                        }
                    }
                    else if (inputBases[i].Equals('x'))
                    {
                        if (inputKey[i].Equals('0'))
                        {
                            tempOutput.Append(getXBasePhoton(settings.XZeroEncoding));
                        }
                        else if (inputKey[i].Equals('1'))
                        {
                            tempOutput.Append(getXBasePhoton(settings.XOneEncoding));
                        }
                    }
                }
                ProgressChanged(i / (inputKey.Length - 1), 1);
            }
            photonOutput = tempOutput.ToString();
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

                        myPresentation.StartPresentation(inputKey, photonOutput, inputBases);
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
            OnPropertyChanged("PhotonOutput");
        }

        private string getXBasePhoton(int p)
        {
            if (p == 0)
            {
                return "\\";
            }
            else
            {
                return "/";
            }
        }

        private string getPlusBasePhoton(int p)
        {
            if (p == 0)
            {
                return "|";
            }
            else
            {
                return "-";
            }
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
            settings.XZeroEncoding = 1;
            settings.XOneEncoding = 0;
            settings.PlusZeroEncoding = 0;
            settings.PlusOneEncoding = 1;
            settings.SpeedSetting = 1;

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
