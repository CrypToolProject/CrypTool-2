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

using CrypTool.MD5.Algorithm;
using CrypTool.MD5.Presentation;
using CrypTool.PluginBase;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Windows.Controls;

namespace CrypTool.MD5
{
    [Author("Sebastian Przybylski", "sebastian@przybylski.org", "Uni Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("CrypTool.Plugins.MD5.Properties.Resources", "PluginCaption", "PluginTooltip", "MD5/DetailedDescription/doc.xml", "MD5/MD5.png")]
    [ComponentCategory(ComponentCategory.HashFunctions)]
    public class MD5 : ICrypComponent
    {
        #region Private variables

        private ICrypToolStream inputData;
        private byte[] outputData;
        private readonly PresentableMD5 md5;
        private readonly PresentationContainer presentationContainer;

        #endregion

        #region Public interface
        public MD5()
        {
            md5 = new PresentableMD5();
            md5.AddSkippedState(MD5StateDescription.STARTING_ROUND_STEP);
            md5.AddSkippedState(MD5StateDescription.FINISHING_COMPRESSION);

            presentationContainer = new PresentationContainer(md5);

            md5.StatusChanged += Md5StatusChanged;
        }

        public ISettings Settings => null;

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

        [PropertyInfo(Direction.OutputData, "OutputDataStreamCaption", "OutputDataStreamTooltip", false)]
        public ICrypToolStream OutputDataStream
        {
            get
            {
                if (outputData != null)
                {
                    //GuiLogMessage("Got request for hash (Stream)...", NotificationLevel.Debug);
                    return new CStreamWriter(outputData);
                }
                return null; ;
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputDataCaption", "OutputDataTooltip", false)]
        public byte[] OutputData
        {

            get =>
              //GuiLogMessage("Got request for hash (Byte Array)...", NotificationLevel.Debug);
              outputData;

            set
            {
                outputData = value;
                OnPropertyChanged("OutputData");
                OnPropertyChanged("OutputDataStream");
            }
        }

        private void Md5StatusChanged()
        {
            if (md5.IsInFinishedState)
            {
                OutputData = md5.HashValueBytes;
                ProgressChanged(1.0, 1.0);
            }
        }

        public void Execute()
        {
            ProgressChanged(0.5, 1.0);
            if (inputData != null)
            {
                if (Presentation.IsVisible)
                {
                    md5.Initialize(inputData);
                }
                else
                {
                    HashAlgorithm builtinMd5 = System.Security.Cryptography.MD5.Create();

                    using (CStreamReader reader = inputData.CreateReader())
                    {
                        OutputData = builtinMd5.ComputeHash(reader);
                    }

                    ProgressChanged(1.0, 1.0);
                }
            }
            else
            {
                GuiLogMessage("Received null value for CrypToolStream.", NotificationLevel.Warning);
            }
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }
        #endregion

        #region IPlugin Members

        public UserControl Presentation => presentationContainer;

        public void Stop()
        {

        }

        public void PostExecution()
        {
            inputData = null;
        }

        public void PreExecution()
        {
            inputData = null;
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
