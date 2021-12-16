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
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Controls;

namespace CrypTool.Plugins.Yao3
{
    [Author("Ondřej Skowronek, Armin Krauß", "xskowr00@stud.fit.vutbr.cz", "Brno University of Technology", "https://www.vutbr.cz")]
    [PluginInfo("Yao3.Properties.Resources", "PluginCaption", "PluginTooltip", "Yao3/userdoc.xml", new[] { "Yao3/icon.png" })]
    [ComponentCategory(ComponentCategory.Protocols)]
    public class Yao3 : ICrypComponent
    {
        #region Data Properties

        [PropertyInfo(Direction.InputData, "pCaption", "pTooltip")]
        public BigInteger p
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "ZCaption", "ZTooltip")]
        public List<BigInteger> Zs
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "xCaption", "xTooltip")]
        public BigInteger x
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "BCaption", "BTooltip")]
        public int B
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "BisRicherCaption", "BisRicherTooltip")]
        public bool BisRicher
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings => null;

        public UserControl Presentation => null;

        public void PreExecution()
        {
        }

        public void Execute()
        {
            ProgressChanged(0, 1);

            if (B >= Zs.Count)
            {
                GuiLogMessage("B's amount of money (" + B + ") must be smaller than the maximum amount (" + Zs.Count + ").", NotificationLevel.Error);
                return;
            }

            BisRicher = (Zs[B] != x % p);
            OnPropertyChanged("BisRicher");

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