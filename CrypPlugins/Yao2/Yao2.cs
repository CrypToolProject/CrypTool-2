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

namespace CrypTool.Plugins.Yao2
{
    [Author("Ondřej Skowronek, Armin Krauß", "xskowr00@stud.fit.vutbr.cz", "Brno University of Technology", "https://www.vutbr.cz")]
    [PluginInfo("Yao2.Properties.Resources", "PluginCaption", "PluginTooltip", "Yao2/userdoc.xml", new[] { "Yao2/icon.png" })]
    [ComponentCategory(ComponentCategory.Protocols)]
    public class Yao2 : ICrypComponent
    {
        #region Data Properties

        private List<BigInteger> Ys = new List<BigInteger>();
        private bool messageflag = false;
        private bool _stopped = false;

        [PropertyInfo(Direction.InputData, "YCaption", "YTooltip")]
        public BigInteger Y
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "maxMoneyCaption", "maxMoneyTooltip")]
        public int maxMoney
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "pCaption", "pTooltip")]
        public BigInteger p
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "ACaption", "ATooltip")]
        public int A
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "ZCaption", "ZTooltip")]
        public List<BigInteger> Zs
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
            Ys = new List<BigInteger>();
            Zs = new List<BigInteger>();
            messageflag = false;
        }

        public void Execute()
        {
            ProgressChanged(0, maxMoney);
            _stopped = false;

            if (A >= maxMoney)
            {
                if (!messageflag)
                {
                    GuiLogMessage("A's amount of money (" + A + ") must be smaller than the maximum amount (" + maxMoney + ").", NotificationLevel.Error);
                }

                messageflag = true;
                return;
            }

            Ys.Add(Y);

            if (Ys.Count == maxMoney)
            {
                for (int i = 0; i < maxMoney; i++)
                {
                    BigInteger z = Ys[i] % p;
                    Zs.Add(i <= A ? z : z + 1);
                    if (_stopped)
                    {
                        return;
                    }
                }

                OnPropertyChanged("Zs");
            }

            ProgressChanged(Ys.Count, maxMoney);
        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
            _stopped = true;
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