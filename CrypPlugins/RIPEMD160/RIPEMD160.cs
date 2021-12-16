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
using System.Windows.Controls;
using System.Windows.Documents;

namespace RIPEMD160
{
    [Author("Sebastian Przybylski", "sebastian@przybylski.org", "Uni Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("RIPEMD160.Properties.Resources", "PluginCaption", "PluginTooltip", "RIPEMD160/DetailedDescription/doc.xml", "RIPEMD160/RMD160.png")]
    [ComponentCategory(ComponentCategory.HashFunctions)]
    public class RIPEMD160 : ICrypComponent
    {
        #region Private variables
        private ICrypToolStream inputData;
        private byte[] outputData;
        #endregion

        #region Public interface

        public ISettings Settings => null;

        [PropertyInfo(Direction.InputData, "InputDataCaption", "InputDataTooltip", true)]
        public ICrypToolStream InputData
        {
            get => inputData;
            set
            {
                if (value != inputData)
                {
                    inputData = value;
                    OnPropertyChanged("InputData");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputDataStreamCaption", "OutputDataStreamTooltip", true)]
        public ICrypToolStream OutputDataStream
        {
            get
            {
                if (OutputData != null)
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
                if (value != outputData)
                {
                    outputData = value;
                    OnPropertyChanged("OutputData");
                    OnPropertyChanged("OutputDataStream");
                }
            }
        }

        public void Execute()
        {
            Progress(0.5, 1.0);

            if (inputData != null && inputData.Length >= 0)
            {
                System.Security.Cryptography.RIPEMD160 ripeMd160Hash = System.Security.Cryptography.RIPEMD160.Create();
                using (CStreamReader reader = inputData.CreateReader())
                {
                    OutputData = ripeMd160Hash.ComputeHash(reader);
                }

                GuiLogMessage("Hash created.", NotificationLevel.Info);
                Progress(1, 1);
            }
            else
            {
                if (inputData == null)
                {
                    GuiLogMessage("Received null value for CrypToolStream.", NotificationLevel.Warning);
                }
                else
                {
                    GuiLogMessage("No input stream.", NotificationLevel.Warning);
                }
            }

            Progress(1.0, 1.0);
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
            inputData = null;
        }
        #endregion

        #region IPlugin Members       

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public UserControl Presentation => null;

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

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        #endregion
    }
}
