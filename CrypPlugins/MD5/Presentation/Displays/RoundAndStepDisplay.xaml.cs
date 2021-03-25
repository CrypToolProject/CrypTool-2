using System.Windows.Controls;

namespace CrypTool.MD5.Presentation.Displays
{
    /// <summary>
    /// Interaktionslogik für RoundAndStepDisplay.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("CrypTool.Plugins.MD5.Properties.Resources")]
    public partial class RoundAndStepDisplay : UserControl
    {
        public RoundAndStepDisplay()
        {
            InitializeComponent();

            Width = double.NaN;
            Height = double.NaN;
        }
    }
}
