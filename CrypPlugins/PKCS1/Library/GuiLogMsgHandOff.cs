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

namespace PKCS1.Library
{
    internal class GuiLogMsgHandOff
    {
        #region singleton
        private static GuiLogMsgHandOff instance = null;

        private GuiLogMsgHandOff() { }

        public static GuiLogMsgHandOff getInstance()
        {
            if (null == instance)
            {
                instance = new GuiLogMsgHandOff();
            }
            return instance;
        }
        #endregion

        public event GuiLogHandler OnGuiLogMsgSend;

        // Klassen, welche GuiLogMessages schicken wollen, müssen hier ihr GuiLogHandler reingeben
        public void registerAt(ref GuiLogHandler guiLogEvent)
        {
            guiLogEvent += SendGuiLogMsg;
        }

        private void SendGuiLogMsg(string message, NotificationLevel logLevel)
        {
            if (null != OnGuiLogMsgSend)
            {
                OnGuiLogMsgSend(message, logLevel);
            }
        }
    }
}
