using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace RAPPOR.View
{
    /// <summary>
    /// Interaction logic for RandomizedResponses.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("CrypTool.Plugins.RAPPOR.Properties.Resources")]
    public partial class RandomizedResponses : UserControl
    {
        public RandomizedResponses()
        {
            InitializeComponent();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
