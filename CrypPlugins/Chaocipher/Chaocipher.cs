/*
   Copyright 
2 Team <ct2contact@cryptool.org>

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

using CrypTool.Chaocipher.Properties;
using CrypTool.Chaocipher.Services;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.Chaocipher
{
    [Author("Niklas Weimann", "niklas.weimann@student.uni-siegen.de", "CrypTool 2 Team", "https://www.cryptool.org")]
    [PluginInfo("CrypTool.Chaocipher.Properties.Resources", "Chaocipher", "ChaocipherToolTip", "Chaocipher/userdoc.xml",
        new[] { "Chaocipher/Images/icon.png" })]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class Chaocipher : ICrypComponent
    {
        #region Private Variables

        private readonly ChaocipherSettings _settings = new ChaocipherSettings();
        private readonly ChaocipherPresentation _chaocipherPresentation;
        private readonly CryptoService _cryptoService;
        private string _outputText;
        private bool _running;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputTextCaption", "InputTextTooltip", true)]
        public string InputText { get; set; }

        [PropertyInfo(Direction.InputData, "KeyCaption", "KeyTooltip")]
        public string Key
        {
            get => _settings.Key;
            set
            {
                if (value == null || value == _settings.Key)
                {
                    return;
                }

                _settings.Key = value;
                OnPropertyChanged(nameof(Key));
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputTextCaption", "OutputTextTooltip")]
        public string OutputText
        {
            get => _outputText;
            set
            {
                _outputText = value;
                OnPropertyChanged(nameof(OutputText));
            }
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings => _settings;

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation => _chaocipherPresentation;

        public Chaocipher()
        {
            _chaocipherPresentation = new ChaocipherPresentation();
            _cryptoService = new CryptoService();
            _settings.OnSpeedChanged += value =>
                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal,
                    (SendOrPostCallback)(state => _chaocipherPresentation.SetSpeed(value)),
                    null);
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal,
                (SendOrPostCallback)(state => _chaocipherPresentation.SetSpeed(_settings.Speed)),
                null);
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            _running = true;
            _cryptoService.Running = true;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);
            string leftDisk = GetLeftDisk();
            string rightDisk = GetRightDisk();
            if (leftDisk == null || rightDisk == null)
            {
                return;
            }
            if (leftDisk.Length != rightDisk.Length)
            {
                GuiLogMessage(Resources.AlphabetDifferInLength, NotificationLevel.Error);
                return;
            }

            switch (_settings.Cipher)
            {
                case ChaocipherSettings.ChaoCipherCodeMode.Encrypt:
                    {
                        Models.CipherResult result = _cryptoService.Encipher(InputText, leftDisk, rightDisk);
                        result = result.GenerateDescription();
                        if (!_running)
                        {
                            return;
                        }
                        Presentation.Dispatcher.Invoke(DispatcherPriority.Normal,
                            (SendOrPostCallback)async delegate { await _chaocipherPresentation.ShowEncipher(result); },
                            null);
                        OutputText = result.ResultString;
                        break;
                    }
                case ChaocipherSettings.ChaoCipherCodeMode.Decrypt:
                    {
                        Models.CipherResult result = _cryptoService.Decipher(InputText, leftDisk, rightDisk).GenerateDescription();
                        if (!_running)
                        {
                            return;
                        }
                        Presentation.Dispatcher.Invoke(DispatcherPriority.Normal,
                            (SendOrPostCallback)async delegate { await _chaocipherPresentation.ShowDecipher(result); },
                            null);
                        OutputText = result.ResultString;
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(_settings.Cipher));
            }

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
            _cryptoService.Running = false;
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal,
                (SendOrPostCallback)delegate { _chaocipherPresentation.Stop(); },
                null);
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

        private string GetLeftDisk()
        {
            return SplitString(_settings.Key)?.Left;
        }

        private string GetRightDisk()
        {
            return SplitString(_settings.Key)?.Right;
        }

        private (string Left, string Right)? SplitString(string src)
        {
            char[] spitCharArray = new[] { '\n', ';', ' ' };
            string[] rowsColumns = src.Split(spitCharArray);
            if (rowsColumns.Length == 2)
            {
                return (rowsColumns[0], rowsColumns[1]);
            }

            GuiLogMessage(
                string.Format(Resources.WarningDefaultAlphabet,
                    string.Join(",", spitCharArray.Select(x => $"\"{char.ToString(x).Replace("\n", "\\n")}\""))),
                NotificationLevel.Warning);
            return null;
        }

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