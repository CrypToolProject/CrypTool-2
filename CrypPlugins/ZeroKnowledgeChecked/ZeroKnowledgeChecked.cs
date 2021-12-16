/*
   Copyright 2011 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using System.ComponentModel;
using System.Numerics;
using System.Windows.Controls;

namespace CrypTool.Plugins.ZeroKnowledgeChecked
{
    [Author("Ondřej Skowronek", "xskowr00@stud.fit.vutbr.cz", "Brno University of Technology", "https://www.vutbr.cz")]
    [PluginInfo("ZeroKnowledgeChecked.Properties.Resources", "PluginCaption", "PluginTooltip", "ZeroKnowledgeChecked/userdoc.xml", new[] { "ZeroKnowledgeChecked/icon.png" })]
    [ComponentCategory(ComponentCategory.Protocols)]
    public class ZeroKnowledgeChecked : ICrypComponent
    {
        #region Private Variables

        private readonly ZeroKnowledgeCheckedSettings settings = new ZeroKnowledgeCheckedSettings();

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputCaption", "InputTooltip")]
        public BigInteger Input
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "AmountOfOptionsCaption", "AmountOfOptionsTooltip")]
        public BigInteger AmountOfOptions
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip")]
        public BigInteger Output
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members


        public ISettings Settings => settings;

        public UserControl Presentation => null;

        public void PreExecution()
        {
        }

        public void Execute()
        {
            ProgressChanged(0, 1);

            if (AmountOfOptions <= 0)
            {
                GuiLogMessage("AmountOfOptions must be a positive integer.", NotificationLevel.Error);
                return;
            }

            if (settings.Secret)
            {
                // If the prover knows the secret, he can generate any requested number.
                // This is represented by him simply forwarding the requested number 'Input' to the 'Output'.
                Output = Input;
            }
            else
            {
                // If the prover doesn't know he secret, he doesn't know how to produce the requested number.
                // He can only hope his random guess equals the requested number.
                // The more rounds and the more choices there are, it will become more and more unlikely that he always guesses right.
                Output = BigIntegerHelper.RandomIntLimit(AmountOfOptions);
            }

            OnPropertyChanged("Output");

            ProgressChanged(1, 1);
        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
        }

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