/*
   Copyright 2023 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using CrypTool.LorenzSZ42.SZ42Machine;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;

namespace CrypTool.LorenzSZ42
{
    [Author("Nils Kopal", "nils.kopal@cryptool.org", "CrypTool project", "http://www.cryptool.org")]
    [PluginInfo("CrypTool.LorenzSZ42.Properties.Resources", "PluginCaption", "PluginTooltip", "LorenzSZ42/DetailedDescription/doc.xml", "LorenzSZ42/icon.png")]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class LorenzSZ42 : ICrypComponent
    {
        private readonly LorenzSZ42Settings _settings = new LorenzSZ42Settings();

        [PropertyInfo(Direction.InputData, "InputTextCaption", "InputTextTooltip", false)]
        public string InputText
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "KeyInputCaption", "KeyInputTooltip", false)]
        public string KeyInput
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputTextCaption", "OutputTextTooltip", false)]
        public string OutputText
        {
            get;
            set;
        }


        [PropertyInfo(Direction.OutputData, "KeyOutputCaption", "KeyOutputTooltip", false)]
        public string KeyOutput
        {
            get;
            set;
        }

        public void PreExecution()
        {
            KeyInput = string.Empty;
            InputText = string.Empty;
        }

        public void PostExecution()
        {

        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public ISettings Settings => _settings;

        public UserControl Presentation => null;

        public void Execute()
        {
            ProgressChanged(0, 1);

            switch (_settings.Action)
            {
                case SZ42Machine.Action.Encrypt:
                case SZ42Machine.Action.Decrypt:

                    //Check key
                    if (string.IsNullOrEmpty(KeyInput))
                    {
                        GuiLogMessage(Properties.Resources.NoKeyProvided, NotificationLevel.Error);
                        return;
                    }

                    //Create machine and set key
                    SZ42Machine.SZ42Machine sz42 = new SZ42Machine.SZ42Machine();
                    try
                    {
                        sz42.SetKey(KeyInput);
                    }
                    catch (ArgumentException argumentException)
                    {
                        GuiLogMessage(string.Format(Properties.Resources.InvalidKeyGiven, argumentException.Message), NotificationLevel.Error);
                        return;
                    }

                    //Warn user if rules are not fulfilled
                    StringBuilder stringBuilder = new StringBuilder();
                    if (SZ42KeyRulesChecker.CheckRules(sz42, stringBuilder) == false)
                    {
                        GuiLogMessage(Properties.Resources.EnteredKeyDoesNotFulfillAllRules, NotificationLevel.Warning);
                        foreach (string line in stringBuilder.ToString().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            GuiLogMessage(line, NotificationLevel.Warning);
                        }
                    }

                    //Output key
                    KeyOutput = sz42.ToString();
                    OnPropertyChanged(nameof(KeyOutput));

                    //En-/Decrypt
                    try
                    {
                        OutputText = sz42.Crypt(InputText, _settings.Action, _settings.Limitation, _settings.InputBaudotNotation, _settings.OutputBaudotNotation);
                    }
                    catch (Exception exception)
                    {
                        GuiLogMessage(string.Format(Properties.Resources.ErrorWhileMachinePerforms, _settings.Action, exception.Message), NotificationLevel.Error);
                        return;
                    }
                    OnPropertyChanged(nameof(OutputText));
                    break;

                case SZ42Machine.Action.GenerateKey:
                    KeyOutput = SZ42KeyGenerator.GenerateKey();
                    OnPropertyChanged(nameof(KeyOutput));
                    break;
            }
            ProgressChanged(1, 1);
        }

        public void Stop()
        {
        }

        public void Initialize()
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Dispose()
        {

        }

        /// <summary>
        /// Property of plugin has new data
        /// </summary>
        /// <param name="name"></param>
        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Log to CT2
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="logLevel"></param>
        private void GuiLogMessage(string msg, NotificationLevel logLevel)
        {
            OnGuiLogNotificationOccured?.Invoke(this, new GuiLogEventArgs(msg, this, logLevel));
        }

        /// <summary>
        /// Set the progress of this component
        /// </summary>
        /// <param name="value"></param>
        /// <param name="max"></param>
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }
    }
}