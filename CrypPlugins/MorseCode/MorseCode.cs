/*
   Copyright 2021 Nils Kopal <Nils.Kopal@cryptool.org>

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
using MorseCode;
using System.ComponentModel;
using System.Windows.Controls;

namespace CrypTool.Plugins.MorseCode
{
    [Author("Nils Kopal", "Nils.Kopal@Uni-Kassel.de", "Universität Kassel", "http://www.uc.uni-kassel.de/")]
    [PluginInfo("MorseCode.Properties.Resources", "PluginCaption", "PluginTooltip", "MorseCode/userdoc.xml", new[] { "MorseCode/icon.png" })]
    [ComponentCategory(ComponentCategory.ToolsCodes)]
    public class MorseCode : ICrypComponent
    {
        /// <summary>
        /// Constructs our mapping and creates our MorseCode object
        /// </summary>
        public MorseCode()
        {

        }

        #region Private Variables

        private readonly MorseCodeSettings _settings = new MorseCodeSettings();
        private bool _stopped = false;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputTextCaption", "InputTextTooltip")]
        public string InputText
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputTextCaption", "OutputTextTooltip")]
        public string OutputText
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "SoundOutputCaption", "SoundOutputTooltip")]
        public byte[] SoundOutput
        {
            get;
            set;
        }


        #endregion

        #region IPlugin Members

        public ISettings Settings => _settings;

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation => null;

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            SoundOutput = null;
            _stopped = false;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);
            if (string.IsNullOrEmpty(InputText) || string.IsNullOrWhiteSpace(InputText))
            {
                OutputText = "";
                OnPropertyChanged("OutputText");
            }
            else
            {
                MorseEncoder morseEncoder = null;
                switch (_settings.Code)
                {
                    case MorseCodeSettings.CodeType.American_Morse:
                        morseEncoder = new AmericanMorseEncoder();
                        break;
                    case MorseCodeSettings.CodeType.Continental:
                        morseEncoder = new ContinentalMorseEncoder();
                        break;
                    case MorseCodeSettings.CodeType.Navy:
                        morseEncoder = new NavyMorseEncoder();
                        break;
                    case MorseCodeSettings.CodeType.TapCode:
                        morseEncoder = new TapCodeEncoder();
                        break;
                    default:
                    case MorseCodeSettings.CodeType.International_ITU:
                        morseEncoder = new InternationalMorseEncoder();
                        break;
                }

                if (morseEncoder == null)
                {
                    return;
                }

                morseEncoder.OnPluginProgressChanged += Encoder_OnPluginProgressChanged;
                switch (_settings.Action)
                {
                    case MorseCodeSettings.ActionType.Encode:
                        OutputText = morseEncoder.Encode(InputText);
                        OnPropertyChanged("OutputText");
                        break;
                    case MorseCodeSettings.ActionType.Decode:
                        OutputText = morseEncoder.Decode(InputText);
                        OnPropertyChanged("OutputText");
                        break;
                    case MorseCodeSettings.ActionType.Play:
                        OnPropertyChanged("SoundOutput");
                        morseEncoder.OnWaveFileGenerated += MorseEncoder_OnPlayTone;
                        morseEncoder.Play(InputText, _settings.Frequency, _settings.TickDuration, _settings.Volume, ref _stopped);
                        break;
                }
            }
            ProgressChanged(1, 1);
        }

        private void MorseEncoder_OnPlayTone(object sender, WaveEventArgs toneEventArgs)
        {
            SoundOutput = toneEventArgs.WaveFile;
            OnPropertyChanged("SoundOutput");
        }

        private void Encoder_OnPluginProgressChanged(IPlugin sender, PluginProgressEventArgs args)
        {
            OnPluginProgressChanged(this, args);
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
            _stopped = true;
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
