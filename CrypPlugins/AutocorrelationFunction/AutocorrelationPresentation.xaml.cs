using System.Windows.Controls;


namespace CrypTool.Plugins.AutocorrelationFunction
{
    /// <summary>
    /// Interaction logic for AutocorrelationPresentation.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("AutocorrelationFunction.Properties.Resources")]
    public partial class AutocorrelationPresentation : UserControl
    {
        public AutocorrelationPresentation()
        {
            InitializeComponent();
        }
    }

    //INFO TO ALL DEVELOPERS WORKING ON PRESENTATIONS:
    //To resize the quickview the same way as this plugin does just use a <Viewbow> around all your other Presentation Elements!
}
