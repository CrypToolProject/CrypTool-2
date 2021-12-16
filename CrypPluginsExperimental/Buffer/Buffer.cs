/*
   Copyright 2020 Nils Kopal <kopal<at>CrypTool.org>

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

namespace CrypTool.Plugins.Buffer
{
    [Author("Nils Kopal", "kopal@CrypTool.org", "CrypTool 2 Team", "https://www.CrypTool.org")]
    [PluginInfo("Buffer", "Buffers input data and returns these as a block of output data", "Buffer/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    [ComponentCategory(ComponentCategory.ToolsDataflow)]
    public class Buffer : ICrypComponent
    {
        #region Private Variables

        private readonly BufferSettings settings = new BufferSettings();
        private byte[] _buffer;
        private int _bufferOffset = 0;
        private bool _stop = false;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputStream", "Input data stream to buffer", true)]
        public ICrypToolStream InputStream
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputStream", "Output data stream", true)]
        public ICrypToolStream OutputStream
        {
            get;
            set;
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
        public UserControl Presentation => null;

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            _buffer = new byte[settings.BufferSize];
            _bufferOffset = 0;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);

            CStreamReader reader = InputStream.CreateReader();
            int lengthOfReadData;
            _stop = false;

            while ((lengthOfReadData = reader.Read(_buffer, _bufferOffset, _buffer.Length - _bufferOffset)) > 0)
            {
                _bufferOffset += lengthOfReadData;
                if (_bufferOffset == _buffer.Length)
                {
                    CStreamWriter writer = new CStreamWriter();
                    writer.Write(_buffer);
                    writer.Close();
                    OutputStream = writer;
                    OnPropertyChanged("OutputStream");
                    _bufferOffset = 0;
                }
                if (_stop)
                {
                    return;
                }
            }
            reader.Close();
            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            _stop = true;
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
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
