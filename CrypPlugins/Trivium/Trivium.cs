/*
   Copyright 2022 Nils Kopal, CrypTool project

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

namespace CrypTool.Trivium
{
    [Author("Nils Kopal", "nils.kopal@CrypTool.org", "CrypToolTeam", "http://www.cryptool.org/")]
    [PluginInfo("CrypTool.Trivium.Properties.Resources", "PluginCaption", "PluginTooltip", "Trivium/DetailedDescription/doc.xml", "Trivium/icon.png")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class Trivium : ICrypComponent
    {
        #region IPlugin Members

        public const int BUFFER_SIZE = 1024 * 1024; //1 Mib             

        private TriviumSettings _settings;
        private ICrypToolStream _inputData;
        private byte[] _inputKey;
        private byte[] _inputIV;

        private bool _stopped = false;

        #endregion

        public Trivium()
        {
            _settings = new TriviumSettings();
        }

        public ISettings Settings
        {
            get => _settings;
            set => _settings = (TriviumSettings)value;
        }

        [PropertyInfo(Direction.InputData, "InputDataCaption", "InputDataTooltip", true)]
        public ICrypToolStream InputData
        {
            get
            {
                return _inputData;
            }
            set
            {
                _inputData = value;
            }
        }

        [PropertyInfo(Direction.InputData, "InputKeyCaption", "InputKeyTooltip", true)]
        public byte[] InputKey
        {
            get
            {
                return _inputKey;
            }
            set
            {
                _inputKey = value;
            }
        }

        [PropertyInfo(Direction.InputData, "InputIVCaption", "InputIVTooltip", true)]
        public byte[] InputIV
        {
            get
            {
                return _inputIV;
            }
            set
            {
                _inputIV = value;
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputDataCaption", "OutputDataTooltip", false)]
        public ICrypToolStream OutputData
        {
            get;
            set;
        }

        public void Dispose()
        {

        }

        public void Execute()
        {
            if (_inputData == null)
            {
                GuiLogMessage(Properties.Resources.NoDataProvided, NotificationLevel.Error);
                return;
            }
            if (_inputKey == null)
            {
                GuiLogMessage(Properties.Resources.NoKeyProvided, NotificationLevel.Error);
                return;
            }
            if (_inputIV == null)
            {
                GuiLogMessage(Properties.Resources.NoIVProvided, NotificationLevel.Error);
                return;
            }
            if (_inputKey.Length != 10)
            {
                GuiLogMessage(Properties.Resources.WrongKeySizeProvided, NotificationLevel.Error);
                return;
            }
            if (_inputIV.Length != 10)
            {
                GuiLogMessage(Properties.Resources.WrongIVSizeProvided, NotificationLevel.Error);
                return;
            }

            TriviumAlgorithm triviumAlgorithm = new TriviumAlgorithm(_inputKey, _inputIV, _settings.InitRounds);
            _stopped = false;

            byte[] buffer = new byte[BUFFER_SIZE];
            int bytesRead;
            using (CStreamReader reader = _inputData.CreateReader())
            {
                using (CStreamWriter writer = new CStreamWriter())
                {
                    OutputData = writer;
                    OnPropertyChanged(nameof(OutputData));

                    while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0 && !_stopped)
                    {
                        for (int i = 0; i < bytesRead; i++)
                        {
                            buffer[i] ^= triviumAlgorithm.GenerateNextByte();                            
                        }
                        writer.Write(buffer, 0, bytesRead);
                        writer.Flush();
                        ProgressChanged(reader.Position, reader.Length);
                    }                   
                }
            }
        }

        public void Initialize()
        {
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

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

        public void PostExecution()
        {
            InputData = null;
            InputKey = null;
            InputIV = null;
            OutputData = null;
        }

        public void PreExecution()
        {

        }

        public UserControl Presentation => null;

        public void Stop()
        {
            _stopped = true;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}