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
using System.Windows.Controls;

namespace CrypTool.Plugins.CoinFlipping
{
    [Author("Ondřej Skowronek, Armin Krauß", "xskowr00@stud.fit.vutbr.cz", "Brno University of Technology", "https://www.vutbr.cz")]
    [PluginInfo("CoinFlipping.Properties.Resources", "PluginCaption", "PluginTooltip", "CoinFlipping/userdoc.xml", new[] { "CoinFlipping/icon.png" })]
    [ComponentCategory(ComponentCategory.Protocols)]
    public class CoinFlipping : ICrypComponent
    {
        #region Private Variables

        private readonly CoinFlippingSettings settings = new CoinFlippingSettings();

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "CoinFlipACaption", "CoinFlipATooltip", true)]
        public bool CoinFlipA
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "CoinFlipBCaption", "CoinFlipBTooltip", true)]
        public bool CoinFlipB
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "SuccessCaption", "SuccessTooltip")]
        public bool Success
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "CoinResultCaption", "CoinResultTooltip")]
        public bool CoinResult
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

            if (settings.SettingsHonest == 1)
            {
                CoinResult = CoinFlipA;             // Alice honestly announces her result
                Success = (CoinFlipA == CoinFlipB); // Bob wins if he correctly predicts Alices result
            }
            else
            {
                CoinResult = !CoinFlipB;            // Alice manipulates her result to her advantage
                Success = false;                    // Bob looses always
            }

            OnPropertyChanged("CoinResult");
            OnPropertyChanged("Success");

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