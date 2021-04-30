using System.Windows;

namespace TemplateEditor
{
    /// <summary>
    /// Interaction logic for InputDialog.xaml
    /// </summary>
    public partial class InputDialog : Window
    {
        public InputDialog(bool showTwoInputs = false)
        {            
            InitializeComponent();
            if (showTwoInputs)
            {
                Height = 170;
                InputBox2.Visibility = Visibility.Visible;
                Label2.Visibility = Visibility.Visible;
            }
            InputBox.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
