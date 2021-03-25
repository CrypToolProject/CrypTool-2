using CrypTool.PluginBase;

namespace PKCS1.Library
{
    public delegate void Navigate(NavigationCommandType type);
    public delegate void ParamChanged(ParameterChangeType type);
    public delegate void SigGenerated(SignatureType type);
    public delegate void VoidDelegate();
    public delegate void GuiLogHandler(string message, NotificationLevel logLevel);
}
