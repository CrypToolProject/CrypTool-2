using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using CrypTool.Core;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using Path = System.IO.Path;

namespace Startcenter
{
    /// <summary>
    /// Interaction logic for LastOpenedFilesList.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("Startcenter.Properties.Resources")]
    public partial class LastOpenedFilesList : UserControl
    {
        public event OpenEditorHandler OnOpenEditor;
        public event OpenTabHandler OnOpenTab;
        private readonly List<RecentFileInfo> _recentFileInfos = new List<RecentFileInfo>();
        private readonly RecentFileList _recentFileList = RecentFileList.GetSingleton();
        public event EventHandler<TemplateOpenEventArgs> TemplateLoaded;

        public LastOpenedFilesList()
        {
            ReadRecentFileList();
            InitializeComponent();
            RecentFileListBox.DataContext = _recentFileInfos;

            _recentFileList.ListChanged += delegate
                                               {
                                                   try
                                                   {
                                                       ReadRecentFileList();
                                                       RecentFileListBox.DataContext = null;
                                                       RecentFileListBox.DataContext = _recentFileInfos;
                                                   }
                                                   catch (Exception)
                                                   {
                                                       //Not critical.. Do nothing
                                                   }
                                               };
        }

        private void ReadRecentFileList()
        {
            var recentFiles = _recentFileList.GetRecentFiles();
            _recentFileInfos.Clear();

            foreach (var rfile in recentFiles)
            {
                if (!File.Exists(rfile))
                    continue; // ignore non-existing files

                var file = new FileInfo(rfile);
                var fileExt = file.Extension.ToLower().Substring(1);
                if (ComponentInformations.EditorExtension != null && ComponentInformations.EditorExtension.ContainsKey(fileExt))
                {
                    bool cte = (fileExt == "cte");
                    Type editorType = ComponentInformations.EditorExtension[fileExt];
                    string xmlFile = Path.Combine(file.Directory.FullName, Path.GetFileNameWithoutExtension(file.Name) + ".xml");
                    string iconFile = null;
                    Span description = new Span();
                    string title = null;

                    if (File.Exists(xmlFile))
                    {
                        try
                        {
                            XElement xml = XElement.Load(xmlFile);
                            var titleElement = XMLHelper.GetGlobalizedElementFromXML(xml, "title");
                            if (titleElement != null)
                                title = titleElement.Value;

                            var summaryElement = XMLHelper.GetGlobalizedElementFromXML(xml, "summary");
                            var descriptionElement = XMLHelper.GetGlobalizedElementFromXML(xml, "description");
                            if (summaryElement != null)
                            {
                                description.Inlines.Add(new Bold(XMLHelper.ConvertFormattedXElement(summaryElement)));
 
                            }
                            if (descriptionElement != null && descriptionElement.Value.Length > 1)
                            {
                                description.Inlines.Add(new LineBreak());
                                description.Inlines.Add(new LineBreak());
                                description.Inlines.Add(XMLHelper.ConvertFormattedXElement(descriptionElement));
                            }

                            if (xml.Element("icon") != null && xml.Element("icon").Attribute("file") != null)
                                iconFile = Path.Combine(file.Directory.FullName, xml.Element("icon").Attribute("file").Value);
                        }
                        catch (Exception)
                        {
                            //we do nothing if the loading of an description xml fails => this is not a hard error
                        }
                    }

                    if ((title == null) || (title.Trim() == ""))
                    {
                        title = Path.GetFileNameWithoutExtension(file.Name).Replace("-", " ").Replace("_", " ");
                    }
                    if (description.Inlines.Count == 0)
                    {
                        string desc;
                        if (cte)
                            desc = Properties.Resources.This_is_an_AnotherEditor_file_;
                        else
                            desc = Properties.Resources.This_is_a_WorkspaceManager_file_;
                        description.Inlines.Add(new Run(desc));
                    }

                    if (iconFile == null || !File.Exists(iconFile))
                        iconFile = Path.Combine(file.Directory.FullName, Path.GetFileNameWithoutExtension(file.Name) + ".png");
                    var image = File.Exists(iconFile) ? ImageLoader.LoadImage(new Uri(iconFile)) : editorType.GetImage(0).Source;

                    _recentFileInfos.Add(new RecentFileInfo()
                        {
                            File = rfile,
                            Title = title,
                            Description = new TextBlock(description) { MaxWidth = 400, TextWrapping = TextWrapping.Wrap},
                            Icon = image,
                            EditorType = editorType
                        });
                }
            }
            _recentFileInfos.Reverse();
        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSelectedTemplate();
        }

        private void OpenSelectedTemplate()
        {
            if (RecentFileListBox.SelectedItem == null)
                return;

            var selectedItem = (RecentFileInfo) RecentFileListBox.SelectedItem;

            if (TemplateLoaded != null)
            {
                var info = new TabInfo()
                {
                    Filename = new FileInfo(selectedItem.File)
                };
                TemplateLoaded.Invoke(this, new TemplateOpenEventArgs() { Type = typeof(WorkspaceManager.WorkspaceManagerClass), Info = info });
            }
            _recentFileList.AddRecentFile(selectedItem.File);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _recentFileList.Clear();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenSelectedTemplate();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (RecentFileListBox == null || RecentFileListBox.SelectedItem == null)
            {
                return;
            }
            var file = ((RecentFileInfo) RecentFileListBox.SelectedItem).File;
            _recentFileList.RemoveFile(file);
        }
    }

    struct RecentFileInfo
    {
        public string File { get; set; }
        public string Title { get; set; }
        public TextBlock Description { get; set; }
        public ImageSource Icon { get; set; }
        public Type EditorType { get; set; }
    }
}
