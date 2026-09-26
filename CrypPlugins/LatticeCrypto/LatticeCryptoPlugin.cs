/*
   Copyright (C) CrypTool 2 Team

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
using LatticeCrypto.Views;

namespace LatticeCrypto
{
    [Author("Eric Schmeck", "eric.schmeck@gmx.de", "Universität Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("LatticeCrypto.Properties.Resources", "PluginCaption", "PluginTooltip", "LatticeCrypto/DetailedDescription/doc.xml", "LatticeCrypto/LatticeCryptoPlugin.png")]
    [FunctionList("FL_F_cryptanalysis", "FL_P_merkle")]
    [FunctionList("FL_F_cryptanalysis", "FL_P__RSA")]
    [FunctionList("FL_F_cryptography", "FL_P_GGH")]
    [FunctionList("FL_F_cryptography", "FL_P_LWE")]
    [FunctionList("FL_F_problems", "FL_P_gauss")]
    [FunctionList("FL_F_problems", "FL_P_LLL")]
    [FunctionList("FL_F_problems", "FL_P_next")]

    public class LatticeCryptoPlugin : ICrypTutorial
    {
        private LatticeCryptoMain latticeCryptoMain;

        //public LatticeCryptoPlugin()
        //{
        //    //GuiLogMsgHandOff.getInstance().OnGuiLogMsgSend += GuiLogMessage; // bei weiterleitung registrieren
        //}

        #region EventHandler

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        //private void GuiLogMessage(string message, NotificationLevel logLevel)
        //{            
        //    EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, message, logLevel); 
        //}

        #region IPlugin Member

        public void Dispose()
        {
            //if (latticeCryptoPlugin != null)
            //    latticeCryptoPlugin.Dispose();
        }

        public void Execute()
        {
            //throw new NotImplementedException();
        }

        public void Initialize()
        {
            //throw new NotImplementedException();
        }

        public void Stop()
        {
            //throw new NotImplementedException();
        }

        public System.Windows.Controls.UserControl Presentation => latticeCryptoMain ?? (latticeCryptoMain = new LatticeCryptoMain());

        public ISettings Settings => null;

        #endregion
    }
}
