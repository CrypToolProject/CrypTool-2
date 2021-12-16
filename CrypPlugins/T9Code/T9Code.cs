/*
   Copyright CrypTool 2 Team <ct2contact@cryptool.org>

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
using CrypTool.T9Code.Enums;
using CrypTool.T9Code.Properties;
using CrypTool.T9Code.Services;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.T9Code
{
    [Author("Niklas Weimann", "niklas.weimann@student.uni-siegen.de", "CrypTool 2 Team", "https://www.cryptool.org")]
    [PluginInfo("CrypTool.T9Code.Properties.Resources", "T9CodeCaption", "T9CodeToolTip", "T9Code/userdoc.xml",
        new[] { "T9Code/Images/icon.png" })]
    [ComponentCategory(ComponentCategory.ToolsCodes)]
    public class T9Code : ICrypComponent
    {
        #region Private Variables

        private readonly T9CodeSettings _settings = new T9CodeSettings();
        private readonly T9CodePresentation _t9CodePresentation;
        private readonly CryptoService _cryptoService;
        private bool _rebuildTrieNeeded = true;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputTextCaption", "InputTextTooltip", true)]
        public string InputText { get; set; }

        [PropertyInfo(Direction.InputData, "InputDictionary", "InputDictionaryTooltip")]
        public Array DictionaryContent
        {
            get => _dictionaryContent;
            set
            {
                _dictionaryContent = value;
                _rebuildTrieNeeded = true;
            }
        }

        private Array _dictionaryContent;
        private bool _running;


        [PropertyInfo(Direction.OutputData, "OutputName", "OutputTooltip")]
        public string OutputText { get; set; }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings => _settings;

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation => _t9CodePresentation;

        public T9Code()
        {
            _t9CodePresentation = new T9CodePresentation(this);
            _cryptoService = new CryptoService();
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            _running = true;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);

            if (_settings.Mode == T9Mode.Encode)
            {
                OutputText = _cryptoService.Encode(InputText);
            }
            else
            {
                _cryptoService.GramSize = _settings.GramSize;
                _cryptoService.SetGramLanguage(_settings.Language);
                ProgressChanged(0.5, 1);
                if (DictionaryContent == null)
                {
                    GuiLogMessage(Resources.UnableToDecode, NotificationLevel.Error);
                    return;
                }

                if (_rebuildTrieNeeded && DictionaryContent != null)
                {
                    if (!_running)
                    {
                        return;
                    }

                    _cryptoService.ClearDictionaryCache();
                    _cryptoService.LoadDictionary(DictionaryContent);
                    _rebuildTrieNeeded = false;
                    if (!_running)
                    {
                        return;
                    }
                }

                ProgressChanged(0.75, 1);
                if (!_running)
                {
                    return;
                }

                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal,
                    (SendOrPostCallback)(state => _t9CodePresentation.SetNumbersToDisplay(InputText)),
                    null);
                if (!_running)
                {
                    return;
                }

                string result = string.Join(" ",
                    _cryptoService.Decode(InputText));
                if (_running)
                {
                    OutputText = result;
                }
            }

            OnPropertyChanged(nameof(OutputText));
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
            _running = false;
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