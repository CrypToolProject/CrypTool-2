/*
   Copyright 2008 Sebastian Przybylski, University of Siegen

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
using System.Security.Cryptography;
using System.Windows.Documents;

namespace SHA
{
    [Author("Sebastian Przybylski", "sebastian@przybylski.org", "Uni Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("SHA.Properties.Resources", "PluginCaption", "PluginTooltip", "SHA/DetailedDescription/doc.xml", "SHA/SHA.png")]
    [FunctionList("SHA-1")]
    [FunctionList("SHA-256")]
    [FunctionList("SHA-384")]
    [FunctionList("SHA-512")]
    [ComponentCategory(ComponentCategory.HashFunctions)]
    public class SHA : ICrypComponent
    {
        private SHASettings settings;
        private ICrypToolStream inputData;
        private byte[] outputData;


        public SHA()
        {
            settings = new SHASettings();
        }

        #region IHashAlgorithm Members

        public void Execute()
        {
            Progress(0.5, 1.0);

            HashAlgorithm hash = GetHashAlgorithm(settings.SHAFunction);

            if (inputData == null)
            {
                GuiLogMessage("Received null value for ICrypToolStream.", NotificationLevel.Warning);
            }
            else if (hash == null)
            {
                GuiLogMessage("No valid SHA algorithm instance.", NotificationLevel.Error);
            }
            else
            {
                using (CStreamReader reader = inputData.CreateReader())
                {
                    OutputData = hash.ComputeHash(reader);
                    //GuiLogMessage("Hash created.", NotificationLevel.Info);
                }
            }
            Progress(1.0, 1.0);
        }

        private HashAlgorithm GetHashAlgorithm(int shaType)
        {
            switch ((SHASettings.ShaFunction)shaType)
            {
                case SHASettings.ShaFunction.SHA1:
                    return new SHA1Managed();
                case SHASettings.ShaFunction.SHA256:
                    return new SHA256Managed();
                case SHASettings.ShaFunction.SHA384:
                    return new SHA384Managed();
                case SHASettings.ShaFunction.SHA512:
                    return new SHA512Managed();
            }

            return null;
        }

        public ISettings Settings
        {
          get => settings;
            set => settings = (SHASettings)value;
        }

        #endregion

        [PropertyInfo(Direction.InputData, "InputDataCaption", "InputDataTooltip", true)]
        public ICrypToolStream InputData
        {
            get => inputData;
            set
            {
                if (value != inputData)
                {
                    inputData = value;
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputDataStreamCaption", "OutputDataStreamTooltip", true)]
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
            set { } //readonly
        }

        [PropertyInfo(Direction.OutputData, "OutputDataCaption", "OutputDataTooltip", true)]
        public byte[] OutputData
        {
            get => outputData;
            set
            {
                if (outputData != value)
                {
                    outputData = value;
                    OnPropertyChanged("OutputData");
                    OnPropertyChanged("OutputDataStream");
                }
            }
        }

        #region IPlugin Members

        public void Dispose()
        {
            if (inputData != null)
            {
                inputData = null;
            }
        }

        public void Initialize()
        {
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public System.Windows.Controls.UserControl Presentation => null;

        public void Stop()
        {
        }

        public void PostExecution()
        {
            Dispose();
        }

        public void PreExecution()
        {
        }
        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region IHashAlgorithm Members

        public FlowDocument DetailedDescription => null;

        #endregion

        private void Progress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        #region IPlugin Members

        #endregion
    }
}
