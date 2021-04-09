using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using CrypTool.Core;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using OnlineDocumentationGenerator.Generators.HtmlGenerator;
using Path = System.IO.Path;

namespace Startcenter
{
    /// <summary>
    /// Interaction logic for Templates.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("Startcenter.Properties.Resources")]
    public partial class Templates : UserControl
    {
        private readonly RecentFileList _recentFileList = RecentFileList.GetSingleton();
        private int TemplateCount = 0;

        public event OpenEditorHandler OnOpenEditor;
        public event OpenTabHandler OnOpenTab;

        public Templates()
        {
            InitializeComponent();
        }
        
        private string _templatesDir;

        public string TemplatesDir
        {
            set 
            {
                if (value != null)
                {
                    _templatesDir = value;
                    FillTemplatesNavigationPane();
                    FoundTemplateCountLabel.Content = TemplateCount;
                }
            }
        }

        public void ReloadTemplates()
        {
            FillTemplatesNavigationPane();
            TemplateSearchInputChanged(this,null);
        }

        private void FillTemplatesNavigationPane()
        {
            if (_templatesDir == null) return;
            DirectoryInfo templateDir = new DirectoryInfo(_templatesDir);

            TemplatesTreeView.Items.Clear();
            TemplatesListBox.Items.Clear();
            
            var rootItem = new CTTreeViewItem(templateDir.Name);
            if (templateDir.Exists)
            {
                foreach (var subDirectory in templateDir.GetDirectories())
                {
                    HandleTemplateDirectories(subDirectory, rootItem);
                }

                MakeTemplateInformation(templateDir, rootItem);

                // sort listbox entries alphabetically
                TemplatesListBox.Items.SortDescriptions.Add(new SortDescription("Tag.Value", ListSortDirection.Ascending));
            }

            //Add root directory entries to the treeview based on their order number:
            var counter = 0;
            var items = rootItem.Items.Cast<CTTreeViewItem>().ToList();
            rootItem.Items.Clear();
            while (items.Count > 0)
            {
                var item = items.FirstOrDefault(x => x.Order == counter);
                if (item != null)
                {
                    items.Remove(item);
                    TemplatesTreeView.Items.Add(item);
                }
                else
                {
                    TemplatesTreeView.Items.Add(new TreeViewItem() { Style = (Style) FindResource("SeparatorStyle") });

                    if (items.All(x => x.Order < 0))
                    {
                        foreach (var it in items)
                        {
                            TemplatesTreeView.Items.Add(it);
                        }
                        return;
                    }
                }
                counter++;
            }
        }

        private void HandleTemplateDirectories(DirectoryInfo directory, CTTreeViewItem parent)
        {
            if (directory == null)
                return;

            //Read directory metainfos:
            var dirName = directory.Name;
            Inline tooltip = null;
            ImageSource dirImage = null;
            var metainfo = directory.GetFiles("dir.xml");
            var order = -1;
            if (metainfo.Length > 0)
            {
                XElement metaXML = XElement.Load(metainfo[0].FullName);
                if (metaXML.Attribute("order") != null)
                {
                    order = int.Parse(metaXML.Attribute("order").Value);
                }

                var dirNameEl = XMLHelper.GetGlobalizedElementFromXML(metaXML, "name");
                if (dirNameEl.Value != null)
                {
                    dirName = dirNameEl.Value;
                }

                if (metaXML.Element("icon") != null && metaXML.Element("icon").Attribute("file") != null)
                {
                    var iconFile = Path.Combine(directory.FullName, metaXML.Element("icon").Attribute("file").Value);
                    if (File.Exists(iconFile))
                    {
                        dirImage = ImageLoader.LoadImage(new Uri(iconFile));
                    }
                }

                var summaryElement = XMLHelper.GetGlobalizedElementFromXML(metaXML, "summary");
                if (summaryElement != null)
                {
                    tooltip = XMLHelper.ConvertFormattedXElement(summaryElement);
                }
            }

            CTTreeViewItem item = new CTTreeViewItem(dirName, order, tooltip, dirImage);
            parent.Items.Add(item);

            foreach (var subDirectory in directory.GetDirectories())
            {
                HandleTemplateDirectories(subDirectory, item);
            }
            MakeTemplateInformation(directory, item);
        }

        private void MakeTemplateInformation(DirectoryInfo info, CTTreeViewItem parent)
        {
            SolidColorBrush bg = Brushes.Transparent;

            foreach (var file in info.GetFiles().Where(x => (x.Extension.ToLower() == ".cwm") || (x.Extension.ToLower() == ".component")))
            {
                if (file.Name.StartsWith("."))
                {
                    continue;
                }
                StringBuilder copyTextBuilder = new StringBuilder();
                bool component = (file.Extension.ToLower() == ".component");
                string title = null;
                Span summary1 = new Span();
                Span summary2 = new Span();
                string xmlFile = Path.Combine(file.Directory.FullName, Path.GetFileNameWithoutExtension(file.Name) + ".xml");
                string iconFile = null;
                Dictionary<string, List<string>> internationalizedKeywords = new Dictionary<string, List<string>>();
                if (File.Exists(xmlFile))
                {
                    try
                    {
                        XElement xml = XElement.Load(xmlFile);
                        var titleElement = XMLHelper.GetGlobalizedElementFromXML(xml, "title");
                        if (titleElement != null)
                        {
                            title = titleElement.Value;
                            //add title to text for copy context menu
                            copyTextBuilder.AppendLine(title);                           
                        }

                        var summaryElement = XMLHelper.GetGlobalizedElementFromXML(xml, "summary");
                        var descriptionElement = XMLHelper.GetGlobalizedElementFromXML(xml, "description");
                        if (summaryElement != null)
                        {
                            summary1.Inlines.Add(new Bold(XMLHelper.ConvertFormattedXElement(summaryElement)));
                            summary2.Inlines.Add(new Bold(XMLHelper.ConvertFormattedXElement(summaryElement)));

                            //add summary to text for copy context menu
                            copyTextBuilder.AppendLine();
                            copyTextBuilder.AppendLine(XMLHelper.ConvertFormattedXElementToString(summaryElement));  
                        }
                        if (descriptionElement != null && descriptionElement.Value.Length > 1) 
                        {
                            summary1.Inlines.Add(new LineBreak());
                            summary1.Inlines.Add(new LineBreak());
                            summary1.Inlines.Add(XMLHelper.ConvertFormattedXElement(descriptionElement));
                            summary1.Inlines.Add(new LineBreak());
                            summary1.Inlines.Add(new LineBreak());
                            summary1.Inlines.Add(Properties.Resources.Category + StripTemplatesPath(file.Directory.FullName));

                            summary2.Inlines.Add(new LineBreak());
                            summary2.Inlines.Add(new LineBreak());
                            summary2.Inlines.Add(XMLHelper.ConvertFormattedXElement(descriptionElement));
                            summary2.Inlines.Add(new LineBreak());
                            summary2.Inlines.Add(new LineBreak());
                            summary2.Inlines.Add(Properties.Resources.Category + StripTemplatesPath(file.Directory.FullName));

                            //add description to text for copy context menu
                            copyTextBuilder.AppendLine();
                            copyTextBuilder.AppendLine(XMLHelper.ConvertFormattedXElementToString(descriptionElement));
                            copyTextBuilder.AppendLine();
                            copyTextBuilder.AppendLine(Properties.Resources.Category + StripTemplatesPath(file.Directory.FullName));
                        }

                        if (xml.Element("icon") != null && xml.Element("icon").Attribute("file") != null)
                        {
                            iconFile = Path.Combine(file.Directory.FullName, xml.Element("icon").Attribute("file").Value);
                        }

                        foreach (var keywordTag in xml.Elements("keywords"))
                        {
                            var langAtt = keywordTag.Attribute("lang");
                            string lang = "en";
                            if (langAtt != null)
                            {
                                lang = langAtt.Value;
                            }
                            var keywords = keywordTag.Value;
                            if (keywords != null || keywords != "")
                            {
                                foreach (var keyword in keywords.Split(','))
                                {
                                    if (!internationalizedKeywords.ContainsKey(lang))
                                    {
                                        internationalizedKeywords.Add(lang, new List<string>());
                                    }
                                    internationalizedKeywords[lang].Add(keyword.Trim());
                                }
                            }
                        }                        
                    }
                    catch(Exception)
                    {
                        //we do nothing if the loading of an description xml fails => this is not a hard error
                    }
                }
                
                if ((title == null) || (title.Trim() == ""))
                {
                    title = component ? file.Name : Path.GetFileNameWithoutExtension(file.Name).Replace("-", " ").Replace("_", " ");
                }             

                if (summary1.Inlines.Count == 0)
                {
                    string desc = component ? Properties.Resources.This_is_a_standalone_component_ : Properties.Resources.This_is_a_WorkspaceManager_file_;
                    summary1.Inlines.Add(new Run(desc));
                    summary2.Inlines.Add(new Run(desc));
                }

                if (iconFile == null || !File.Exists(iconFile))
                {
                    iconFile = Path.Combine(file.Directory.FullName, Path.GetFileNameWithoutExtension(file.Name) + ".png");
                }
                ImageSource image = null;
                if (File.Exists(iconFile))
                {
                    try
                    {
                        image = ImageLoader.LoadImage(new Uri(iconFile));
                    }
                    catch (Exception)
                    {
                        image = null;
                    }
                }
                else
                {
                    var ext = file.Extension.Remove(0, 1);
                    if (!component && ComponentInformations.EditorExtension.ContainsKey(ext))
                    {
                        Type editorType = ComponentInformations.EditorExtension[ext];
                        image = editorType.GetImage(0).Source;
                    }
                }

                System.Collections.ArrayList list = new System.Collections.ArrayList();
                list.Add(new TabInfo()
                {
                    Filename = file,
                });

                ListBoxItem searchItem = CreateTemplateListBoxItem(file, title, summary1, image);

                if (internationalizedKeywords.Count > 0)
                {
                    list.Add(internationalizedKeywords);
                }

                ((StackPanel)searchItem.Content).Tag = list;
                TemplatesListBox.Items.Add(searchItem);

                CTTreeViewItem item = new CTTreeViewItem(file, title, summary2, image) 
                { 
                    Background = bg 
                };
                ToolTipService.SetShowDuration(item, Int32.MaxValue);
                item.MouseDoubleClick += TemplateItemDoubleClick;

                //adding context menu for opening template
                item.ContextMenu = new ContextMenu();
                MenuItem menuItem = new MenuItem();
                menuItem.Header = Properties.Resources.OpenTemplate;
                menuItem.Tag = item;
                menuItem.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(ContextMenu_OpenTemplateClick));
                item.ContextMenu.Items.Add(menuItem);                
                //adding context menu for copying the description text to this template entry                
                menuItem = new MenuItem();
                menuItem.Header = Properties.Resources.CopyDescription;
                menuItem.Tag = copyTextBuilder.ToString();
                menuItem.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(ContextMenu_CopyClick));
                item.ContextMenu.Items.Add(menuItem);
                
                parent.Items.Add(item);
                TemplateCount++;
            }
        }

        /// <summary>
        /// Removes the preceding part of the templates path
        /// i.e. "C:\program files\CrypTool 2\templates\etc" becomes "etc"
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string StripTemplatesPath(string path)
        {
            int position = path.ToLower().IndexOf("templates");
            if (position > 0) 
            {
                return path.Substring(position + 10, path.Length - (position + 10)).Replace("\\"," / ");
            }
            return string.Empty;
        }

        /// <summary>
        /// User clicked on open template in context menu of an entry of the template list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        public void ContextMenu_OpenTemplateClick(Object sender, EventArgs eventArgs)
        {
            try
            {
                //when user clicks on the "open template" entry of the context menu
                //we invoke the double click on the item, thus, opening the template
                MenuItem menuItem = (MenuItem)((RoutedEventArgs)eventArgs).Source;
                CTTreeViewItem cTTreeViewItem = (CTTreeViewItem)menuItem.Tag;
                TemplateItemDoubleClick(cTTreeViewItem, null);
            }
            catch (Exception)
            {
                //wtf ?
            }
        }

        /// <summary>
        /// User clicked on copy in context menu of an entry of the template list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        public void ContextMenu_CopyClick(Object sender, EventArgs eventArgs)
        {
            try
            {
                MenuItem menuItem = (MenuItem)((RoutedEventArgs)eventArgs).Source;
                string copytext = (string)menuItem.Tag;
                Clipboard.SetText(copytext);                
            }
            catch (Exception)
            {
                Clipboard.SetText("");
            }
        }

        private ListBoxItem CreateTemplateListBoxItem(FileInfo file, string title, Inline tooltip, ImageSource imageSource)
        {
            Image image = new Image();
            image.Source = imageSource;
            image.Margin = new Thickness(16, 0, 5, 0);
            image.Height = 25;
            image.Width = 25;
            TextBlock textBlock = new TextBlock();
            textBlock.FontWeight = FontWeights.DemiBold;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.Text = title;
            textBlock.Tag = title;
            StackPanel stackPanel = new StackPanel();
            stackPanel.Margin = new Thickness(0, 2, 0, 2);
            stackPanel.VerticalAlignment = VerticalAlignment.Center;
            stackPanel.Orientation = Orientation.Horizontal;
            stackPanel.Children.Add(image);
            stackPanel.Children.Add(textBlock);

            ListBoxItem navItem = new ListBoxItem();
            navItem.Content = stackPanel;
            navItem.Tag = new KeyValuePair<string, string>(file.FullName, title);


            navItem.MouseDoubleClick += TemplateItemDoubleClick;
            if (tooltip != null)
            {
                var tooltipBlock = new TextBlock(tooltip) {TextWrapping = TextWrapping.Wrap, MaxWidth = 400};
                navItem.ToolTip = tooltipBlock;
            }

            ToolTipService.SetShowDuration(navItem, Int32.MaxValue);
            return navItem;
        }

        public event EventHandler<TemplateOpenEventArgs> TemplateLoaded;

        private void TemplateItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var infos = ((KeyValuePair<string, string>)((FrameworkElement)sender).Tag);
            if (infos.Key != null && Path.GetExtension(infos.Key) != null)
            {
                var fileExt = Path.GetExtension(infos.Key).ToLower().Substring(1);
                if (ComponentInformations.EditorExtension != null && ComponentInformations.EditorExtension.ContainsKey(fileExt))
                {
                    Type editorType = ComponentInformations.EditorExtension[fileExt];
                    TabInfo info = new TabInfo();
                    if (sender is CTTreeViewItem)
                    {
                        var templateItem = (CTTreeViewItem)sender;

                        info = new TabInfo()
                        {
                            Filename = templateItem.File,
                        };                       
                    }
                    else if (sender is ListBoxItem)
                    {
                        var searchItem = (ListBoxItem) sender;
                        info = (TabInfo)((System.Collections.ArrayList)((FrameworkElement)searchItem.Content).Tag)[0];
                    }

                    if (TemplateLoaded != null)
                    {
                        TemplateLoaded.Invoke(this, new TemplateOpenEventArgs() { Info = info, Type = editorType });
                    }                    
                    _recentFileList.AddRecentFile(infos.Key);
                }
            }
        }

        private void TemplateSearchInputChanged(object sender, TextChangedEventArgs e)
        {
            List<string> searchWords = new List<string>();
            List<string> hitSearchWords = new List<string>();

            var searchWordsArray = SearchTextBox.Text.ToLower().Split(new char[] { ',', ' ' });
            foreach (var searchword in searchWordsArray)
            {
                var sw = searchword.Trim();
                if (sw != "" && !searchWords.Contains(sw)) searchWords.Add(sw);
            }

            if (searchWords.Count == 0)
            {
                TemplatesListBox.Visibility = Visibility.Collapsed;
                TemplatesTreeView.Visibility = Visibility.Visible;
                FoundTemplateCountLabel.Content = TemplateCount;
                return;
            }

            TemplatesListBox.Visibility = Visibility.Visible;
            TemplatesTreeView.Visibility = Visibility.Collapsed;
            
            int templateFoundCounter = 0;

            foreach (ListBoxItem item in TemplatesListBox.Items)
            {
                TextBlock textBlock = (TextBlock)((Panel)item.Content).Children[1];
                string title = (string)textBlock.Tag;

                // search template title for the search words
                hitSearchWords.Clear();
                SearchForHitWords(searchWords, hitSearchWords, new List<string>() { title });

                bool allSearchWordsFound = hitSearchWords.Count == searchWords.Count;

                // if the template title doesn't contain all search words, search also the keywords
                if (!allSearchWordsFound)
                {
                    List<string> keywords = SearchMatchingKeywords(item, searchWords, hitSearchWords);
                    allSearchWordsFound = hitSearchWords.Count == searchWords.Count;
                    if (allSearchWordsFound)
                    {
                        title += " (" + String.Join(", ", keywords) + ")";
                    }
                }

                if (allSearchWordsFound)
                {
                    item.Visibility = Visibility.Visible;
                    templateFoundCounter++;
                }
                else
                {
                    item.Visibility = Visibility.Collapsed;
                }

                // display matching text segments in bold font
                if (allSearchWordsFound)
                {
                    textBlock.Inlines.Clear();
                    int begin = 0;
                    int length;
                    int end = IndexOfFirstHit(title, searchWordsArray, begin, out length);
                    while (end != -1)
                    {
                        textBlock.Inlines.Add(title.Substring(begin, end - begin));
                        textBlock.Inlines.Add(new Bold(new Italic(new Run(title.Substring(end, length)))));
                        begin = end + length;
                        end = IndexOfFirstHit(title, searchWordsArray, begin, out length);
                    }
                    textBlock.Inlines.Add(title.Substring(begin, title.Length - begin));
                }
            }
            FoundTemplateCountLabel.Content = templateFoundCounter;
        }

        private int IndexOfFirstHit(string text, string[] searchWords, int begin, out int length)
        {
            length = 0;
            int res = -1;
            foreach (var searchWord in searchWords)
            {
                if (searchWord != "")
                {
                    int e = text.IndexOf(searchWord, begin, StringComparison.OrdinalIgnoreCase);
                    if ((e > -1) && ((e < res) || (res < 0)))
                    {
                        res = e;
                        length = searchWord.Length;
                    }
                }
            }
            return res;
        }

        private List<string> SearchMatchingKeywords(ListBoxItem item, List<string> searchWords, List<string> hitSearchWords)
        {
            List<string> hitKeyWords = new List<string>();

            var tag = ((System.Collections.ArrayList)((FrameworkElement)item.Content).Tag);

            if (tag.Count == 2)
            {
                var internationalizedKeywords = (Dictionary<string, List<string>>)tag[1];
                List<string> langs = new List<string>() { CultureInfo.CurrentCulture.TextInfo.CultureName, CultureInfo.CurrentCulture.TwoLetterISOLanguageName };
                if (!langs.Contains("en")) langs.Add("en"); // check english keywords in any case
                foreach (var lang in langs)
                    if(lang!=null && internationalizedKeywords.ContainsKey(lang))
                        SearchForHitWords(searchWords, hitSearchWords, internationalizedKeywords[lang], hitKeyWords);
            }

            return hitKeyWords;
        }

        private void SearchForHitWords(List<string> searchWords, List<string> hitSearchWords, List<string> words, List<string> hitWords=null)
        {
            foreach (var searchWord in searchWords)
            {
                if (hitSearchWords.Contains(searchWord)) continue;
                foreach (var word in words)
                    if (word.ToLower().Contains(searchWord.ToLower()))
                    {
                        if (hitWords != null && !hitWords.Contains(word)) hitWords.Add(word);
                        if (!hitSearchWords.Contains(searchWord)) hitSearchWords.Add(searchWord);
                    }
            }
        }

        private void TemplateSearchBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                SearchTextBox.Text = "";
            }
            //if only one item is left, the user may open the corresponding template by hitting the enter key
            if (e.Key == Key.Return && (int)FoundTemplateCountLabel.Content == 1)            
            {
                try
                {
                    foreach (ListBoxItem item in TemplatesListBox.Items)
                    {
                        if (item.Visibility == Visibility.Visible)
                        {
                            TemplateItemDoubleClick(item, null);
                            break;
                        }
                    }
                }
                catch (Exception)
                {
                    //wtf?
                }
            }
        }

        private string GetRelativePathBySubtracting(string path1, string path2)
        {
            var rel = path2.Substring(path1.Length);
            if (rel[0] == Path.DirectorySeparatorChar)
            {
                return rel.Substring(1);
            }
            return rel;
        }

        public void ShowHelp()
        {
            FrameworkElement item = null;
            if (TemplatesTreeView.Visibility == Visibility.Visible)
            {
                if (TemplatesTreeView.SelectedItem != null)
                {
                    item = (CTTreeViewItem) TemplatesTreeView.SelectedItem;
                }
            }
            else
            {
                if (TemplatesListBox.SelectedItem != null)
                {
                    item = (FrameworkElement) TemplatesListBox.SelectedItem;
                }
            }

            if (item == null)
            {
                return;
            }

            var infos = ((KeyValuePair<string, string>)item.Tag);
            var rel = GetRelativePathBySubtracting(_templatesDir, infos.Key);
            OnlineHelp.InvokeShowDocPage(new OnlineHelp.TemplateType(rel));
        }

        private void ExpandAll_Click(object sender, RoutedEventArgs e)
        {
            SetTreeViewItems(TemplatesTreeView, true);
        }

        private void CollapseAll_Click(object sender, RoutedEventArgs e)
        {
            SetTreeViewItems(TemplatesTreeView, false);
        }

        void SetTreeViewItems(object obj, bool expand)
        {
            if (obj is TreeViewItem)
            {
                ((TreeViewItem)obj).IsExpanded = expand;
                foreach (object obj2 in ((TreeViewItem)obj).Items)
                    SetTreeViewItems(obj2, expand);
            }
            else if (obj is ItemsControl)
            {
                foreach (object obj2 in ((ItemsControl)obj).Items)
                {
                    if (obj2 != null)
                    {
                        SetTreeViewItems(((ItemsControl)obj).ItemContainerGenerator.ContainerFromItem(obj2), expand);

                        TreeViewItem item = obj2 as TreeViewItem;
                        if (item != null)
                            item.IsExpanded = expand;
                    }
                }
            }
        }
    }

    public class TemplateOpenEventArgs : EventArgs
    {
        public TabInfo Info { get; set; }

        public Type Type { get; set; }
    }
}
