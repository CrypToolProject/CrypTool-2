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

namespace CrypTool.Plugins.ObliviousTransfer1
{
    [Author("Ondřej Skowronek, Armin Krauß", "xskowr00@stud.fit.vutbr.cz", "Brno University of Technology", "https://www.vutbr.cz")]
    [PluginInfo("ObliviousTransfer1.Properties.Resources", "PluginCaption", "PluginTooltip", "ObliviousTransfer1/userdoc.xml", new[] { "ObliviousTransfer1/icon.png" })]
    [ComponentCategory(ComponentCategory.Protocols)]
    public class ObliviousTransfer1 : ICrypComponent
    {
        #region Data Properties

        [PropertyInfo(Direction.InputData, "xCaption", "xTooltip")]
        public BigInteger[] x
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "bCaption", "bTooltip")]
        public int b
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "eCaption", "eTooltip")]
        public BigInteger e
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "NCaption", "NTooltip")]
        public BigInteger N
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "vCaption", "vTooltip")]
        public BigInteger v
        {
            get;
            set;
        }
        [PropertyInfo(Direction.OutputData, "kCaption", "kTooltip")]
        public BigInteger K
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

            if (b < 0 || b >= x.Length)
            {
                GuiLogMessage("Requested message index " + b + " is illegal, it must be bigger than 0 and smaller than " + x.Length, NotificationLevel.Error);
                return;
            }

            K = BigIntegerHelper.RandomIntLimit(N);
            v = (x[b] + BigInteger.ModPow(K, e, N)) % N;

            OnPropertyChanged("K");
            OnPropertyChanged("v");

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