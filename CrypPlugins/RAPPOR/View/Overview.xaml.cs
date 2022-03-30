using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace CrypTool.Plugins.RAPPOR.View
{
    /// <summary>
    /// Interaction logic for Overview.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("CrypTool.Plugins.RAPPOR.Properties.Resources")]
    public partial class Overview : UserControl
    {
        //private ArrayDrawer arrayDrawer;
        //private RAPPOR rappor;
        /// <summary>
        /// Initializes the Ovewview view model.
        /// </summary>
        public Overview()
        {
            InitializeComponent();
        }
        /// <summary>
        /// This method is used to validate the input of the textboxes, insuring that only strings 
        /// are entered which can be transformed into integers.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text composition event arg.</param>
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}