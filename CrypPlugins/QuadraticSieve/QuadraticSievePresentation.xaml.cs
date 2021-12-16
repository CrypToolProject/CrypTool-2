using System.Windows.Controls;

namespace CrypTool.Plugins.QuadraticSieve
{
    /// <summary>
    /// Interaction logic for QuadraticSievePresentation.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("QuadraticSieve.Properties.Resources")]
    public partial class QuadraticSievePresentation : UserControl
    {
        private readonly ProgressRelationPackages progressRelationPackages;
        public ProgressRelationPackages ProgressRelationPackages => progressRelationPackages;

        public QuadraticSievePresentation()
        {
            InitializeComponent();

            progressRelationPackages = new ProgressRelationPackages(peer2peerScrollViewer);
            peer2peerScrollViewer.Content = progressRelationPackages;
            progressRelationPackages.MaxWidth = 620 - 30;
        }

        public void SelectFirstComposite()
        {
            foreach (string item in factorList.Items)
            {
                if (item.StartsWith("Composite"))
                {
                    factorList.SelectedItem = item;
                    factorList.ScrollIntoView(item);
                    return;
                }
            }
        }
    }
}
