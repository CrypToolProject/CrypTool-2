using CrypTool.PluginBase;

namespace LatticeCrypto.Utilities
{
    public delegate void Navigate(NavigationCommandType type);
    //public delegate void ParamChanged(ParameterChangeType type);
    //public delegate void SigGenerated(SignatureType type);
    //public delegate void VoidDelegate();
    public delegate void GuiLogHandler(string message, NotificationLevel logLevel);
}
