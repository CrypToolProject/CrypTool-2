using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using CrypTool.PluginBase;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Attributes;
using System.Diagnostics;
using OnlineDocumentationGenerator.Generators.HtmlGenerator;
using CrypTool.PluginBase.Miscellaneous;

namespace CrypTool.CrypWin
{
    /// <summary>
    /// Interaction logic for OnlineHelpTab.xaml
    /// </summary>
    [Localization("CrypTool.CrypWin.Properties.Resources")]
    [TabColor("White")]
    [NotStoredInSessionAttribute]
    public partial class OnlineHelpTab : UserControl
    {
        private static OnlineHelpTab _singleton = null;
        private DateTime _crypwinTime = File.GetLastWriteTime(Assembly.GetEntryAssembly().Location);
        private string _docDirectory = DirectoryHelper.BaseDirectory;
        private bool _generated = false;
        private MainWindow _mainWindow;
       
        public event OpenTabHandler OnOpenTab;
        public event OpenEditorHandler OnOpenEditor;

        private OnlineHelpTab(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            InitializeComponent();
            Tag = FindResource("Icon");            
            webBrowser.AllowDrop = false;
            webBrowser.Navigating += webBrowser_Navigating;
            
        }

        private void webBrowser_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            //The user clicked on a cwm file in the help html
            if(e.Uri.LocalPath.EndsWith(".cwm"))
            {
                Type editorType = ComponentInformations.EditorExtension["cwm"];
                string filename = e.Uri.LocalPath;
                string title = Path.GetFileNameWithoutExtension(filename);
                var editor = OnOpenEditor(editorType, new TabInfo() { Filename = new FileInfo(filename), Title = title });
                editor.Open(filename);
                e.Cancel = true;
            }
            if(e.Uri.AbsoluteUri.EndsWith("?external"))
            {                
                try
                {
                    Process.Start(e.Uri.AbsoluteUri.Remove(e.Uri.AbsoluteUri.Length-9));
                }
                catch(Exception ex)
                {
                    //wtf?
                }
                e.Cancel = true;
            }
        }

        public static OnlineHelpTab GetSingleton(MainWindow mainWindow)
        {            
            if (_singleton == null)
                _singleton = new OnlineHelpTab(mainWindow);
            //remove all event handlers from the singleton
            //to avoid opening templates more than once
            _singleton.OnOpenEditor = null;
            _singleton.OnOpenTab = null;
            return _singleton;
        }

        private void GuiLogMessage(string message, NotificationLevel level)
        {
            _mainWindow.OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, null, level));
        }

        public void ShowHTMLFile(string htmlFile)
        {
            if (!TryOpening(htmlFile))
            {
                if (!_generated)
                {
                    GenerateDoc();
                    _generated = true;
                    ShowHTMLFile(htmlFile);
                }
                else
                {
                    throw new FileNotFoundException(string.Format(Properties.Resources.OnlineHelpTab_ShowHTMLFile_Doc_page__0__not_found_, htmlFile));
                }
            }
        }

        private bool TryOpening(string htmlFile)
        {
            var pathToFile = GetPathToFile(htmlFile);
            if (UpToDateFileExists(pathToFile))
            {
                ShowPage(pathToFile);
                return true;
            }
            return false;
        }

        private void GenerateDoc()
        {
            var docGenerator = new OnlineDocumentationGenerator.DocGenerator();
            docGenerator.OnGuiLogNotificationOccured += delegate(IPlugin sender, GuiLogEventArgs args)
                                                            {
                                                                _mainWindow.OnGuiLogNotificationOccured("DocGenerator", args);
                                                            };

            var generatingDocWindow = new GeneratingWindow();
            generatingDocWindow.Message.Content = Properties.Resources.GeneratingWaitMessage;
            generatingDocWindow.Title = Properties.Resources.Generating_Documentation_Title;
            bool init = false;
            generatingDocWindow.ContentRendered += delegate
                                              {
                                                  if (!init)
                                                  {
                                                      init = true;
                                                      docGenerator.Generate(_docDirectory, new HtmlGenerator());
                                                      generatingDocWindow.Close();
                                                  }
                                              };
            generatingDocWindow.ShowDialog();
        }

        private string GetPathToFile(string htmlFile)
        {
            var helpDir = Path.Combine(_docDirectory, OnlineHelp.HelpDirectory);
            return Path.Combine(helpDir, htmlFile);
        }

        private bool UpToDateFileExists(string pathToFile)
        {
            var fileExists = File.Exists(pathToFile);
            if (AssemblyHelper.BuildType == Ct2BuildType.Developer)
            {
                //In developing mode, check if file is older than CrypWin. If that is the case, the file is potentially not up to date anymore:
                return fileExists && (File.GetLastWriteTime(pathToFile) > _crypwinTime);
            }
            return fileExists;
        }

        private void ShowPage(string pathToFile)
        {
            webBrowser.Source = new Uri(Uri.UriSchemeFile + "://" + pathToFile);
        }
        
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (webBrowser.CanGoBack)
                {
                    webBrowser.GoBack();
                }
            }
            catch(Exception)
            {
                //we do not handle errors from iexplore
            }
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (webBrowser.CanGoForward)
                {
                    webBrowser.GoForward();
                }
            }
            catch (Exception)
            {
                //we do not handle errors from iexplore
            }
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            //Find out which page to show:
            var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            try
            {
                ShowHTMLFile(OnlineHelp.GetComponentIndexFilename(lang));
            }
            catch (Exception ex)
            {
                //we do not handle errors from iexplore
                GuiLogMessage(ex.Message, NotificationLevel.Debug);
            }
        }

        public void Print()
        {
            try
            {
                var doc = webBrowser.Document as mshtml.IHTMLDocument2;
                if (doc != null)
                    doc.execCommand("Print", true, null);
            }
            catch (Exception ex)
            {
                //we do not handle errors from iexplore
            }
        }
    }
}
