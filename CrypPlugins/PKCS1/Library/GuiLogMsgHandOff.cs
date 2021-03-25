using CrypTool.PluginBase;

namespace PKCS1.Library
{
    class GuiLogMsgHandOff
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
