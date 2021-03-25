using System.Windows.Controls;

namespace CrypTool.Plugins.ChaCha.View
{
    /// <summary>
    /// Interaction logic for Overview.xaml
    /// </summary>
    [PluginBase.Attributes.Localization("CrypTool.Plugins.ChaCha.Properties.Resources")]
    public partial class Overview : UserControl
    {
        public Overview()
        {
            InitializeComponent();
            ActionViewBase.LoadLocaleResources(this);
        }
    }
}