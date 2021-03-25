using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TemplateEditor
{
    /// <summary>
    /// Interaction logic for AllKeywordsWindow.xaml
    /// </summary>
    public partial class AllKeywordsWindow : Window
    {
        private Dictionary<string, List<string>> _allKeywords;

        public AllKeywordsWindow()
        {
            InitializeComponent();
        }

        public List<string> SelectKeywords(Dictionary<string, List<string>> allKeywords, string lang)
        {
            CancelButton.Visibility = Visibility.Visible;
            _allKeywords = allKeywords;
            LangBox.ItemsSource = allKeywords.Keys;

            if ((lang == null) || (!LangBox.Items.Contains(lang)))
            {
                LangBox.SelectedIndex = 0;
            }
            else
            {
                LangBox.SelectedItem = lang;
            }

            var res = this.ShowDialog();
            if (res.HasValue && res.Value)
            {
                return new List<string>(keywordsList.SelectedItems.Cast<string>());
            }
            else
            {
                return null;
            }
        }

        public List<string> SelectKeywords(Dictionary<string, List<string>> allKeywords)
        {
            return SelectKeywords(allKeywords, null);
        }

        public void ShowAllKeywords(Dictionary<string, List<string>> allKeywords)
        {
            _allKeywords = allKeywords;
            CancelButton.Visibility = Visibility.Hidden;
            LangBox.DataContext = allKeywords.Keys;
            LangBox.SelectedIndex = 0;
            var res = this.ShowDialog();
        }

        private void LangBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            keywordsList.DataContext = _allKeywords[(string) LangBox.SelectedItem];
        }

        private void AllLanguages_Checked(object sender, RoutedEventArgs e)
        {
            var allKeywords = new List<string>();
            foreach (var keywords in _allKeywords.Values)
            {
                allKeywords.AddRange(keywords);
            }
            allKeywords.Sort();
            keywordsList.DataContext = allKeywords;
        }

        private void AllLanguages_Unchecked(object sender, RoutedEventArgs e)
        {
            LangBox_SelectionChanged(null, null);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
