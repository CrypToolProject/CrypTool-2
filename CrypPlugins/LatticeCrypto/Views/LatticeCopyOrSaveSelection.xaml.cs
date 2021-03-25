using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LatticeCrypto.Views
{
    /// <summary>
    /// Interaktionslogik für LatticeCopyOrSaveSelection.xaml
    /// </summary>
    public partial class LatticeCopyOrSaveSelection
    {
        public int selection;

        public LatticeCopyOrSaveSelection()
        {
            InitializeComponent();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = panelSelection.Children.Cast<RadioButton>().First(x => x.IsChecked == true);
            int.TryParse(radioButton.Tag.ToString(), out selection);
            DialogResult = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
