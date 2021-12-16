using CrypTool.PluginBase;

namespace LatticeCrypto.Utilities
{
    internal class GuiLogMsgHandOff
    {
        #region singleton
        private static GuiLogMsgHandOff instance;

        private GuiLogMsgHandOff() { }

        public static GuiLogMsgHandOff getInstance()
        {
            return instance ?? (instance = new GuiLogMsgHandOff());
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
