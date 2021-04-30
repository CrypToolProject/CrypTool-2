using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Path = System.IO.Path;

namespace TemplateEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<TemplateInfo> _templates = new ObservableCollection<TemplateInfo>();
        private string _templateDir;

        public static readonly DependencyProperty IsOverviewProperty = DependencyProperty.Register("IsOverview",
                                                                                                   typeof (Boolean),
                                                                                                   typeof (MainWindow),
                                                                                                   new PropertyMetadata(
                                                                                                       true));

        private List<Replacement> _deletedReplacements = new List<Replacement>();
        private List<Replacement> _addedReplacements = new List<Replacement>();

        private string _selectedLanuage = null;


        public bool IsOverview
        {
            get { return (bool) GetValue(IsOverviewProperty); }
            set { SetValue(IsOverviewProperty, value); }
        }

        public static readonly DependencyProperty IsDirtyProperty = DependencyProperty.Register("IsDirty",
                                                                                                   typeof(Boolean),
                                                                                                   typeof(MainWindow),
                                                                                                   new PropertyMetadata(
                                                                                                       false));
        private bool IsDirty
        {
            get { return (bool)GetValue(IsDirtyProperty); }
            set { SetValue(IsDirtyProperty, value); }
        }

        public MainWindow()
        {
            InitializeComponent();
            var templateFolder = Directory.GetCurrentDirectory() + @"\Templates";
            if (!Directory.Exists(templateFolder))
            {
                var templateFolderDialog = new FolderBrowserDialog();
                templateFolderDialog.Description = "Template directory not found. Please select your template directory.";
                templateFolderDialog.SelectedPath = Directory.GetCurrentDirectory();
                if (templateFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _templateDir = templateFolderDialog.SelectedPath;                   
                }
                else
                {
                    Close();
                    return;
                }
            }
            else
            {
                _templateDir = templateFolder;
            }
            
            LoadTemplates(".");
            AllTemplatesList.DataContext = _templates;
            AllTemplatesList2.DataContext = _templates;
        }

        private Dictionary<string, List<string>> GetAllKeywords()
        {
            var result = new Dictionary<string, List<string>>();
            foreach (var template in _templates)
            {
                if (template.LocalizedTemplateData != null)
                {
                    foreach (var locTemp in template.LocalizedTemplateData)
                    {
                        if (locTemp.Value.Keywords != null)
                        {
                            if (!result.ContainsKey(locTemp.Key))
                            {
                                result.Add(locTemp.Key, new List<string>());
                            }
                            foreach (var keyword in locTemp.Value.Keywords)
                            {
                                if( !result[locTemp.Key].Contains(keyword) )
                                    result[locTemp.Key].Add(keyword);
                            }
                        }
                    }
                }
            }
            foreach (var res in result)
            {
                res.Value.Sort();
            }
            return result;
        }

        private void LoadTemplates(string dir)
        {
            var dirPath = Path.Combine(_templateDir, dir);

            foreach (var file in Directory.GetFiles(dirPath))
            {
                if (file.ToLower().EndsWith("cwm") || file.ToLower().EndsWith("component"))
                {
                    _templates.Add(new TemplateInfo(_templateDir, Path.Combine(dir, Path.GetFileName(file))));
                }
            }

            foreach (var subdir in Directory.GetDirectories(dirPath))
            {
                var subd = new DirectoryInfo(subdir);
                LoadTemplates(Path.Combine(dir, subd.Name));
            }
        }

        private void SwitchButton_Click(object sender, RoutedEventArgs e)
        {
            IsOverview = !IsOverview;
        }

        private void AllKeywordsButton_Click(object sender, RoutedEventArgs e)
        {
            var akw = new AllKeywordsWindow();
            akw.ShowAllKeywords(GetAllKeywords());
        }

        private void AllTemplatesList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AllTemplatesList2.SelectedIndex = AllTemplatesList.SelectedIndex;
            IsOverview = false;
        }

        private void AllTemplatesList2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (AllTemplatesList2.SelectedItem != null)
                {
                    var tempInfo = AllTemplatesList2.SelectedItem as TemplateInfo;
                    ShowTemplateDetails(tempInfo);

                    RelevantPluginsListBox.Items.Clear();
                    if (tempInfo.RelevantPlugins != null)
                    {
                        foreach (var plugin in tempInfo.RelevantPlugins)
                        {
                            RelevantPluginsListBox.Items.Add(plugin);
                        }
                    }
                    IconTextBox.Text = tempInfo.IconFile;
                }
                else
                {
                    ClearBoxes();
                }
            }
            finally
            {
                IsDirty = false;
            }
        }

        private void ClearBoxes()
        {
            KeywordsListBox.Items.Clear();
            TitleTextBox.Text = "";
            DescriptionTextBox.Text = "";
            IconTextBox.Text = "";
            RelevantPluginsListBox.Items.Clear();
        }

        private void ShowTemplateDetails(TemplateInfo templateInfo)
        {
            LanguageBox.Items.Clear();
            foreach (var localization in templateInfo.LocalizedTemplateData)
            {
                LanguageBox.Items.Add(localization);
            }

            if (_selectedLanuage != null)
            {
                var index = 0;
                foreach(var keyValuePair in LanguageBox.Items)
                {
                    var language = ((KeyValuePair<string, LocalizedTemplateData>)keyValuePair).Key;

                    if (_selectedLanuage.Equals(language))
                    {
                        break; 
                    }
                    index++;
                }
                LanguageBox.SelectedIndex = index;
                return;
            }            
            LanguageBox.SelectedIndex = 0;
            
        }

        private void LanguageBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (LanguageBox.SelectedItem == null)
                {
                    ClearBoxes();
                    return;
                }

                var localizedTemplateData = ((KeyValuePair<string, LocalizedTemplateData>)LanguageBox.SelectedItem).Value;
                TitleTextBox.Text = localizedTemplateData.Title;
                SummaryTextBox.Text = localizedTemplateData.Summary;
                DescriptionTextBox.Text = localizedTemplateData.Description;

                ReplacementsListView.Items.Clear();
                _addedReplacements.Clear();
                _deletedReplacements.Clear();

                if (localizedTemplateData.Replacements != null)
                {
                    foreach (var replacement in localizedTemplateData.Replacements)
                    {
                        ReplacementsListView.Items.Add(replacement);
                    }
                }

                KeywordsListBox.Items.Clear();
                if (localizedTemplateData.Keywords != null)
                {
                    foreach (var keyword in localizedTemplateData.Keywords)
                    {
                        KeywordsListBox.Items.Add(keyword);
                    }
                }
            }
            finally
            {
                IsDirty = false;
                if (LanguageBox.SelectedItem != null)
                {
                    _selectedLanuage = ((KeyValuePair<string, LocalizedTemplateData>)LanguageBox.SelectedItem).Key;
                }
            }
        }

        private void AddNewReplacementButton_Click(object sender, RoutedEventArgs e)
        {
            if (AllTemplatesList2.SelectedItem == null)
            {
                return;
            }                            
            var id = new InputDialog(true);
            var res = id.ShowDialog();
            if (res.HasValue && res.Value)
            {
                var localizedTemplateData = ((KeyValuePair<string, LocalizedTemplateData>)LanguageBox.SelectedItem).Value;
                var replacement = new Replacement() { Key = id.InputBox.Text, Value = id.InputBox2.Text };
                localizedTemplateData.Replacements.Add(replacement);
                ReplacementsListView.Items.Add(replacement);
                _addedReplacements.Add(replacement);
                IsDirty = true;
            }             
        }
        private void EditSelectedReplacement_Click(object sender, RoutedEventArgs e)
        {
            if (AllTemplatesList2.SelectedItem == null)
            {
                return;            
            }

            var oldreplacement = (Replacement)ReplacementsListView.Items.GetItemAt(ReplacementsListView.SelectedIndex);            
            var localizedTemplateData = ((KeyValuePair<string, LocalizedTemplateData>)LanguageBox.SelectedItem).Value;

            var id = new InputDialog(true);
            id.InputBox.Text = oldreplacement.Key;
            id.InputBox2.Text = oldreplacement.Value;

            var res = id.ShowDialog();
            if (res.HasValue && res.Value)
            {                
                var replacement = new Replacement() { Key = id.InputBox.Text, Value = id.InputBox2.Text };
                
                localizedTemplateData.Replacements.Add(replacement);
                localizedTemplateData.Replacements.Remove(oldreplacement);

                _addedReplacements.Add(replacement);
                _deletedReplacements.Add(oldreplacement);

                ReplacementsListView.Items.Add(replacement);
                ReplacementsListView.Items.Remove(oldreplacement);                               
                
                IsDirty = true;
            }
        }
        private void DeleteSelectedReplacement_Click(object sender, RoutedEventArgs e)
        {
            if (AllTemplatesList2.SelectedItem == null)
            {
                return;
            }
            if (ReplacementsListView.SelectedIndex >= 0)
            {
                var replacement = (Replacement)ReplacementsListView.Items.GetItemAt(ReplacementsListView.SelectedIndex);
                var localizedTemplateData = ((KeyValuePair<string, LocalizedTemplateData>)LanguageBox.SelectedItem).Value;
                _deletedReplacements.Add(replacement);
                localizedTemplateData.Replacements.Remove(replacement);
                ReplacementsListView.Items.Remove(replacement);
                IsDirty = true;
            }
        }

        private void PluginAddButton_Click(object sender, RoutedEventArgs e)
        {
            var id = new InputDialog();
            var res = id.ShowDialog();
            if (res.HasValue && res.Value)
            {
                RelevantPluginsListBox.Items.Add(id.InputBox.Text);
                IsDirty = true;
            }
        }

        private void PluginDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (RelevantPluginsListBox.SelectedIndex >= 0)
            {
                RelevantPluginsListBox.Items.RemoveAt(RelevantPluginsListBox.SelectedIndex);
                IsDirty = true;
            }
        }

        private void AddKnownKeywordButton_Click(object sender, RoutedEventArgs e)
        {
            var akw = new AllKeywordsWindow();
            string currLang = null;
            if (LanguageBox.SelectedItem != null)
            {
                currLang = ((KeyValuePair<string, LocalizedTemplateData>) LanguageBox.SelectedItem).Key;
            }

            var res = akw.SelectKeywords(GetAllKeywords(), currLang);
            if (res != null)
            {
                foreach (var keyword in res)
                {
                    if (!KeywordsListBox.Items.Contains(keyword))
                    {
                        KeywordsListBox.Items.Add(keyword);
                    }
                }
                IsDirty = true;
            }
        }

        private void AddNewKeywordButton_Click(object sender, RoutedEventArgs e)
        {
            var id = new InputDialog();
            var res = id.ShowDialog();
            if (res.HasValue && res.Value)
            {
                KeywordsListBox.Items.Add(id.InputBox.Text);
                IsDirty = true;
            }
        }

        private void DeleteKeywordButton_Click(object sender, RoutedEventArgs e)
        {
            if (KeywordsListBox.SelectedIndex >= 0)
            {
                KeywordsListBox.Items.RemoveAt(KeywordsListBox.SelectedIndex);
                IsDirty = true;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (AllTemplatesList2.SelectedItem != null)
            {
                var tempInfo = AllTemplatesList2.SelectedItem as TemplateInfo;
                
                //plugins:
                tempInfo.RelevantPlugins = new List<string>();
                foreach (var plugin in RelevantPluginsListBox.Items)
                {
                    tempInfo.RelevantPlugins.Add(plugin.ToString());
                }

                //icon:
                tempInfo.IconFile = IconTextBox.Text;

                //localized data:
                if (LanguageBox.SelectedItem != null)
                {
                    var localizedTemplateData = ((KeyValuePair<string, LocalizedTemplateData>) LanguageBox.SelectedItem);
                    if (!tempInfo.LocalizedTemplateData.ContainsKey(localizedTemplateData.Key))
                    {
                        tempInfo.LocalizedTemplateData.Add(localizedTemplateData.Key, new LocalizedTemplateData());
                    }
                    var md = tempInfo.LocalizedTemplateData[localizedTemplateData.Key];
                    md.Title = TitleTextBox.Text;
                    md.Summary = SummaryTextBox.Text;
                    md.Description = DescriptionTextBox.Text;
                    md.Lang = localizedTemplateData.Key;
                    md.Keywords = new List<string>();
                    if (KeywordsListBox.Items.Count > 0)
                    {
                        foreach (var keyword in KeywordsListBox.Items)
                        {
                            md.Keywords.Add((string) keyword);
                        }
                    }
                }

                _deletedReplacements.Clear();
                _addedReplacements.Clear();

                tempInfo.Save();
                UpdateTemplatesList();
                
            }
            IsDirty = false;
        }

        private void UpdateTemplatesList()
        {
            var index = AllTemplatesList2.SelectedIndex;
            var tmp = AllTemplatesList2.DataContext;
            AllTemplatesList2.DataContext = null;
            AllTemplatesList2.DataContext = tmp;
            AllTemplatesList2.SelectedIndex = index;

            index = AllTemplatesList2.SelectedIndex;
            tmp = AllTemplatesList.DataContext;
            AllTemplatesList.DataContext = null;
            AllTemplatesList.DataContext = tmp;
            AllTemplatesList.SelectedIndex = index;
        }

        private void AddLanguageButton_Click(object sender, RoutedEventArgs e)
        {
            var id = new InputDialog();
            var res = id.ShowDialog();
            if (res.HasValue && res.Value)
            {
                if (!LanguageBox.Items.Contains(id.InputBox.Text))
                {
                    var newEntry = new KeyValuePair<string, LocalizedTemplateData>(id.InputBox.Text, new LocalizedTemplateData());
                    newEntry.Value.Lang = id.InputBox.Text;
                    LanguageBox.Items.Add(newEntry);
                }
                LanguageBox.SelectedItem = id.InputBox.Text;
            }
        }

        private void DeleteLanguageButton_Click(object sender, RoutedEventArgs e)
        {
            if (LanguageBox.SelectedIndex >= 0)
            {
                var res = System.Windows.MessageBox.Show(this, "Are you sure you want to delete this language in this sample? This change will be saved immediately",
                                                        "Delete Language", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (res == MessageBoxResult.Yes)
                {
                    if (AllTemplatesList2.SelectedItem != null)
                    {
                        var tempInfo = AllTemplatesList2.SelectedItem as TemplateInfo;
                        var lang =  ((KeyValuePair<string, LocalizedTemplateData>)(LanguageBox.SelectedItem)).Key;
                        tempInfo.LocalizedTemplateData.Remove(lang);
                        tempInfo.Save();
                        UpdateTemplatesList();
                    }

                    LanguageBox.Items.RemoveAt(LanguageBox.SelectedIndex);
                    LanguageBox.SelectedIndex = 0;
                }
            }
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            IsDirty = true;
        }

        private void OpenInCT2_Click(object sender, RoutedEventArgs e)
        {
            var tempInfo = AllTemplatesList2.SelectedItem as TemplateInfo;
            if(tempInfo == null)
            {
                return;
            }
            try
            {
                Process.Start(tempInfo.FullCWMPath);
            }
            catch (Exception)
            {
                //do nothing if it can't be opened
            }
        }

        private void RevertButton_Click(object sender, RoutedEventArgs e)
        {
            var result = System.Windows.MessageBox.Show("Do you really want to revert all changes you made?", "Revert changes?", MessageBoxButton.YesNo);
            if(result == MessageBoxResult.No)
            {
                return;
            }

            if (AllTemplatesList2.SelectedItem != null)
            {
                //revert all replacements
                var localizedTemplateData = ((KeyValuePair<string, LocalizedTemplateData>)LanguageBox.SelectedItem).Value;                
                localizedTemplateData.Replacements.AddRange(_deletedReplacements);

                foreach (var r in _addedReplacements)
                {
                    localizedTemplateData.Replacements.Remove(r);
                }
            }
            LanguageBox_SelectionChanged(null, null);
            IsDirty = false;
        }
    }
}
