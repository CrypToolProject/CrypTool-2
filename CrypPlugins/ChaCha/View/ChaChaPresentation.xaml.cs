using CrypTool.Plugins.ChaCha.ViewModel;
using System.Windows.Controls;

namespace CrypTool.Plugins.ChaCha.View
{
    /// <summary>
    /// Interaction logic for ChaChaPresentation.xaml
    /// </summary>
    [PluginBase.Attributes.Localization("CrypTool.Plugins.ChaCha.Properties.Resources")]
    public partial class ChaChaPresentation : UserControl
    {
        public ChaChaPresentation(ChaCha chachaVisualization)
        {
            InitializeComponent();
            DataContext = new ChaChaPresentationViewModel(chachaVisualization);
        }
    }
}