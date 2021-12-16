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
using System.Windows.Controls;

namespace CrypTool.Plugins.BB84ErrorDetector
{
    [Author("Benedict Beuscher", "benedict.beuscher@stud.uni-due.de", "Uni Duisburg-Essen", "http://www.uni-due.de/")]

    [PluginInfo("CrypTool.Plugins.BB84ErrorDetector.Properties.Resources", "res_ErrorDetectorCaption", "res_ErrorDetectorTooltip", "BB84ErrorDetector/userdoc.xml", new[] { "BB84ErrorDetector/images/icon.png" })]

    [ComponentCategory(ComponentCategory.Protocols)]
    public class BB84ErrorDetector : ICrypComponent
    {
        #region Private Variables

        private string firstKey;
        private string secondKey;
        private string detectionMessage;
        private double errorRatio;


        private readonly BB84ErrorDetectorSettings mySettings = new BB84ErrorDetectorSettings();


        #endregion


        #region Data Properties

        [PropertyInfo(Direction.InputData, "res_FirstKeyCaption", "res_FirstKeyTooltip")]
        public string FirstKey
        {
            get => firstKey;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }
                if (!value.Equals(firstKey))
                { firstKey = value; }
            }
        }


        [PropertyInfo(Direction.InputData, "res_SecondKeyCaption", "res_SecondKeyTooltip")]
        public string SecondKey
        {
            get => secondKey;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }
                if (!value.Equals(secondKey))
                { secondKey = value; }
            }
        }

        [PropertyInfo(Direction.OutputData, "res_MessageCaption", "res_MessageTooltip")]
        public string DetectionMessage
        {
            get => detectionMessage;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }
                if (!value.Equals(detectionMessage))
                {
                    detectionMessage = value;
                }
            }
        }

        #endregion

        #region IPlugin Members


        public ISettings Settings => mySettings;

        public UserControl Presentation => null;

        public void PreExecution()
        {
        }

        public void Execute()
        {
            ProgressChanged(0, 1);
            setSequenceBounds();
            calculateErrorRatio();
            generateDetectionMessage();
            notifyOutput();
            ProgressChanged(1, 1);
        }



        private void setSequenceBounds()
        {
            if (mySettings.EndIndex >= firstKey.Length)
            {
                mySettings.EndIndex = firstKey.Length - 1;
            }

            if (mySettings.StartIndex >= firstKey.Length - 1)
            {
                mySettings.StartIndex = firstKey.Length - 1;
            }
        }

        private void generateDetectionMessage()
        {
            string message = "";

            if (errorRatio > ((double)mySettings.ThresholdValue) / 100)
            {

                message = string.Format(Properties.Resources.res_KeyUnsecure, Math.Round(errorRatio, 3) * 100);
            }
            else
            {
                message = string.Format(Properties.Resources.res_KeySecure, Math.Round(errorRatio, 3) * 100);
            }

            detectionMessage = message;
        }

        private void calculateErrorRatio()
        {
            double count = 0;
            double errors = 0;
            for (int i = mySettings.StartIndex; i <= mySettings.EndIndex; i++)
            {
                if (firstKey.Length > i && secondKey.Length > i)
                {
                    if (!firstKey[i].Equals(secondKey[i]))
                    {
                        errors++;
                    }
                }
                count++;
            }
            errorRatio = errors / count;
        }

        private void notifyOutput()
        {
            OnPropertyChanged("DetectionMessage");
        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
            mySettings.ThresholdValue = 11;
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
