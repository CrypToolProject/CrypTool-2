using CrypTool.PluginBase;

namespace PKCS1.Library
{
    interface IGuiLogMsg
    {
        event GuiLogHandler OnGuiLogMsgSend;

        void registerHandOff();

        void SendGuiLogMsg(string message, NotificationLevel logLevel);
    }
}
