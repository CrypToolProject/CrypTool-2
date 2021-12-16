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
using System;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Controls;

namespace CrypTool.Plugins.ObliviousTransfer2
{
    [Author("Ondřej Skowronek, Armin Krauß", "xskowr00@stud.fit.vutbr.cz", "Brno University of Technology", "https://www.vutbr.cz")]
    [PluginInfo("ObliviousTransfer2.Properties.Resources", "PluginCaption", "PluginTooltip", "ObliviousTransfer2/userdoc.xml", new[] { "ObliviousTransfer2/icon.png" })]
    [ComponentCategory(ComponentCategory.Protocols)]
    public class ObliviousTransfer2 : ICrypComponent
    {
        #region Data Properties

        [PropertyInfo(Direction.InputData, "MessagesCaption", "MessagesTooltip")]
        public string[] Messages
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "xCaption", "xTooltip")]
        public BigInteger[] x
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "vCaption", "vTooltip")]
        public BigInteger v
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "dCaption", "dTooltip")]
        public BigInteger d
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

        [PropertyInfo(Direction.OutputData, "EncryptedMessagesCaption", "EncryptedMessagesTooltip")]
        public BigInteger[] EncryptedMessages
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
            BigInteger m, k;

            ProgressChanged(0, 1);

            if (x == null || Messages == null)
            {
                GuiLogMessage("Illegal array 'x' or 'messages'.", NotificationLevel.Error);
                return;
            }

            if (x.Length != Messages.Length)
            {
                GuiLogMessage("Arrays 'x' and 'messages' must have the same number of entries.", NotificationLevel.Error);
                return;
            }

            EncryptedMessages = new BigInteger[x.Length];

            for (int i = 0; i < Messages.Length; i++)
            {
                try // can be read as parseable expression?
                {
                    m = BigIntegerHelper.ParseExpression(Messages[i]);
                }
                catch (Exception)
                {
                    GuiLogMessage("Error while converting '" + Messages[i] + "' to a number.", NotificationLevel.Error);
                    return;
                }

                k = BigInteger.ModPow(((v - x[i]) % N + N) % N, d, N);
                EncryptedMessages[i] = (m + k) % N;

                ProgressChanged(i + 1, Messages.Length);
            }

            OnPropertyChanged("EncryptedMessages");
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