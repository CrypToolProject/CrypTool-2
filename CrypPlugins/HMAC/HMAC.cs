/*
   Copyright 2009 Holger Pretzsch, University of Duisburg-Essen

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
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using System.ComponentModel;
using System.Windows.Controls;

namespace CrypTool.HMAC
{
    [Author("Holger Pretzsch", "CrypTool@holger-pretzsch.de", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("CrypTool.HMAC.Properties.Resources", "PluginCaption", "PluginTooltip", "HMAC/DetailedDescription/doc.xml", "HMAC/HMAC.png")]
    [ComponentCategory(ComponentCategory.HashFunctions)]
    public class HMAC : ICrypComponent
    {
        #region Private variables

        private HMACSettings settings;
        private ICrypToolStream inputData;
        private byte[] key;
        private byte[] outputData;

        #endregion

        #region Public interface

        public HMAC()
        {
            settings = new HMACSettings();
        }

        public ISettings Settings
        {
            get => settings;

            set => settings = (HMACSettings)value;
        }

        [PropertyInfo(Direction.InputData, "InputDataCaption", "InputDataTooltip", true)]
        public ICrypToolStream InputData
        {
            get => inputData;

            set
            {
                inputData = value;
                OnPropertyChanged("InputData");
            }
        }

        [PropertyInfo(Direction.InputData, "KeyCaption", "KeyTooltip", true)]
        public byte[] Key
        {
            get => key;

            set
            {
                key = value;
                OnPropertyChanged("Key");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputDataStreamCaption", "OutputDataStreamTooltip", false)]
        public ICrypToolStream OutputDataStream
        {
            get
            {
                if (outputData != null)
                {
                    return new CStreamWriter(outputData);
                }

                return null;
            }

            set { } // readonly
        }

        [PropertyInfo(Direction.OutputData, "OutputDataCaption", "OutputDataTooltip", false)]
        public byte[] OutputData
        {
            get => outputData;

            set
            {
                outputData = value;
                OnPropertyChanged("OutputData");
                OnPropertyChanged("OutputDataStream");
            }
        }

        public void Execute()
        {
            ProgressChanged(0, 1);

            System.Security.Cryptography.HMAC hmacAlgorithm;

            switch ((HMACSettings.HashFunction)settings.SelectedHashFunction)
            {
                case HMACSettings.HashFunction.MD5:
                    hmacAlgorithm = new System.Security.Cryptography.HMACMD5();
                    break;
                case HMACSettings.HashFunction.RIPEMD160:
                    hmacAlgorithm = new System.Security.Cryptography.HMACRIPEMD160();
                    break;
                case HMACSettings.HashFunction.SHA1:
                    hmacAlgorithm = new System.Security.Cryptography.HMACSHA1();
                    break;
                case HMACSettings.HashFunction.SHA256:
                    hmacAlgorithm = new System.Security.Cryptography.HMACSHA256();
                    break;
                case HMACSettings.HashFunction.SHA384:
                    hmacAlgorithm = new System.Security.Cryptography.HMACSHA384();
                    break;
                case HMACSettings.HashFunction.SHA512:
                    hmacAlgorithm = new System.Security.Cryptography.HMACSHA512();
                    break;
                default:
                    GuiLogMessage("No hash algorithm for HMAC selected, using MD5.", NotificationLevel.Warning);
                    hmacAlgorithm = new System.Security.Cryptography.HMACMD5();
                    break;
            }

            hmacAlgorithm.Key = key;

            OutputData = (inputData != null) ? hmacAlgorithm.ComputeHash(inputData.CreateReader()) : hmacAlgorithm.ComputeHash(new byte[] { });

            GuiLogMessage(string.Format("HMAC computed. (using hash algorithm {0}: {1})", settings.SelectedHashFunction, hmacAlgorithm.GetType().Name), NotificationLevel.Info);

            ProgressChanged(1, 1);
        }


        public void Initialize() { }

        public void Dispose()
        {
            inputData = null;
        }
        #endregion

        #region IPlugin Members

        public UserControl Presentation => null;

        public void Stop() { }

        public void PostExecution()
        {
            Dispose();
        }

        public void PreExecution()
        {
            Dispose();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region IPlugin Members

#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}
