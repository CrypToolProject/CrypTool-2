/*
   Copyright 2008 - 2022 CrypTool Team

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using CrypCloud.Core;
using CrypCloud.Manager;
using CrypTool.Core;
using CrypTool.CrypWin.Helper;
using CrypTool.CrypWin.Properties;
using CrypTool.CrypWin.Resources;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Attributes;
using CrypTool.PluginBase.Editor;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using CrypWin.Helper;
using DevComponents.WpfRibbon;
using Microsoft.Win32;
using OnlineDocumentationGenerator.Generators.FunctionListGenerator;
using OnlineDocumentationGenerator.Generators.HtmlGenerator;
using OnlineDocumentationGenerator.Generators.LaTeXGenerator;
using Startcenter.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
using Control = System.Windows.Controls.Control;
using DataObject = System.Windows.DataObject;
using DragDropEffects = System.Windows.DragDropEffects;
using Exception = System.Exception;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using ListBox = System.Windows.Controls.ListBox;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Orientation = System.Windows.Controls.Orientation;
using TabControl = System.Windows.Controls.TabControl;

namespace CrypTool.CrypWin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("CrypTool.CrypWin.Properties.Resources")]
    public partial class MainWindow : DevComponents.WpfRibbon.RibbonWindow
    {
        #region private variables
        private readonly List<NotificationLevel> listFilter = new List<NotificationLevel>();
        private readonly ObservableCollection<LogMessage> collectionLogMessages = new ObservableCollection<LogMessage>();
        private PluginManager pluginManager;
        private Dictionary<string, List<Type>> loadedTypes;
        private int numberOfLoadedTypes = 0;
        private int initCounter;
        private readonly Dictionary<CTTabItem, object> tabToContentMap = new Dictionary<CTTabItem, object>();
        private readonly Dictionary<object, CTTabItem> contentToTabMap = new Dictionary<object, CTTabItem>();
        private readonly Dictionary<object, IEditor> contentToParentMap = new Dictionary<object, IEditor>();
        private TabItem lastTab = null;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private bool closingCausedMinimization = false;
        private WindowState oldWindowState;
        private bool restart = false;
        private bool shutdown = false;
        private string personalDir;
        private IEditor lastEditor = null;
        private SystemInfos systemInfos;
        private LicensesTab licenses;
        private System.Windows.Forms.MenuItem playStopMenuItem;
        private readonly EditorTypePanelManager editorTypePanelManager = new EditorTypePanelManager();
        private System.Windows.Forms.Timer hasChangesCheckTimer;
        private readonly X509Certificate serverTlsCertificate1;
        private readonly X509Certificate serverTlsCertificate2;
        private readonly EditorTypePanelManager.EditorTypePanelProperties defaultPanelProperties = new EditorTypePanelManager.EditorTypePanelProperties(true, false, false);
        private CultureInfo currentculture, currentuiculture;

        private readonly Dictionary<IEditor, string> editorToFileMap = new Dictionary<IEditor, string>();
        private string ProjectFileName
        {
            get
            {
                if (ActiveEditor != null && editorToFileMap.ContainsKey(ActiveEditor))
                {
                    return editorToFileMap[ActiveEditor];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (ActiveEditor != null)
                {
                    editorToFileMap[ActiveEditor] = value;
                }
            }
        }

        private bool dragStarted;
        private Splash splashWindow;
        private bool startUpRunning = true;
        private string defaultTemplatesDirectory = "";
        private bool silent = false;
        private readonly List<IPlugin> listPluginsAlreadyInitialized = new List<IPlugin>();
        private readonly string[] interfaceNameList = new string[] {
                typeof(CrypTool.PluginBase.ICrypComponent).FullName,
                typeof(CrypTool.PluginBase.Editor.IEditor).FullName,
                typeof(CrypTool.PluginBase.ICrypTutorial).FullName };
        #endregion

        public IEditor ActiveEditor
        {
            get
            {
                if (MainSplitPanel == null)
                {
                    return null;
                }

                if (MainSplitPanel.Children.Count == 0)
                {
                    return null;
                }

                CTTabItem selectedTab = (CTTabItem)(MainTab.SelectedItem);
                if (selectedTab == null)
                {
                    return null;
                }

                if (tabToContentMap.ContainsKey(selectedTab))
                {
                    if (tabToContentMap[selectedTab] is IEditor)
                    {
                        return (IEditor)(tabToContentMap[selectedTab]);
                    }
                    else if (contentToParentMap.ContainsKey(tabToContentMap[selectedTab]) && (contentToParentMap[tabToContentMap[selectedTab]] != null))
                    {
                        return contentToParentMap[tabToContentMap[selectedTab]];
                    }
                }

                return null;
            }

            set => AddEditor(value);
        }

        public IPlugin ActivePlugin
        {
            get
            {
                if (MainSplitPanel == null)
                {
                    return null;
                }

                if (MainSplitPanel.Children.Count == 0)
                {
                    return null;
                }

                CTTabItem selectedTab = (CTTabItem)(MainTab.SelectedItem);
                if (selectedTab == null)
                {
                    return null;
                }

                if (tabToContentMap.ContainsKey(selectedTab))
                {
                    if (tabToContentMap[selectedTab] is IPlugin)
                    {
                        return (IPlugin)(tabToContentMap[selectedTab]);
                    }
                }

                return null;
            }
        }

        private EditorTypePanelManager.EditorTypePanelProperties ActivePanelProperties
        {
            get
            {
                IEditor editor = ActiveEditor;
                return editor != null ? editorTypePanelManager.GetEditorTypePanelProperties(editor.GetType()) : defaultPanelProperties;
            }
        }

        public static readonly DependencyProperty AvailableEditorsProperty =
            DependencyProperty.Register(
            "AvailableEditors",
            typeof(List<Type>),
            typeof(MainWindow),
            new FrameworkPropertyMetadata(new List<Type>(), FrameworkPropertyMetadataOptions.None, null));

        [TypeConverter(typeof(List<Type>))]
        public List<Type> AvailableEditors
        {
            get => (List<Type>)GetValue(AvailableEditorsProperty);
            private set => SetValue(AvailableEditorsProperty, value);
        }

        public static readonly DependencyProperty VisibilityStartProperty =
              DependencyProperty.Register(
              "VisibilityStart",
              typeof(bool),
              typeof(MainWindow),
              new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, null));

        [TypeConverter(typeof(bool))]
        public bool VisibilityStart
        {
            get => (bool)GetValue(VisibilityStartProperty);
            set => SetValue(VisibilityStartProperty, value);
        }

        internal static void SaveSettingsSavely()
        {
            try
            {
                Settings.Default.Save();
            }
            catch (Exception)
            {
                try
                {
                    Settings.Default.Save();
                }
                catch (Exception)
                {
                    //try saving two times, then do not try it again
                }
            }
        }

        private bool IsUpdaterEnabled => AssemblyHelper.BuildType != Ct2BuildType.Developer && !IsCommandParameterGiven("-noupdate");

        #region Init

        /// <summary>
        /// Default constructor of the MainWindow of CrypTool 2
        /// </summary>
        public MainWindow()
        {
            GuiLogMessage("Start creation of MainWindow", NotificationLevel.Debug);

            FixSettingsByDeletingThem();

            EnforceTLS12();

            SetLanguage();

            CheckEssentialComponents();

            LoadResources();

            CreateSystemInfosAndLicencesTabs();

            UnblockDLLs();

            // will exit application after doc has been generated
            if (IsCommandParameterGiven("-GenerateDoc"))
            {
                GenerateDoc();
            }

            // will exit application after doc has been generated
            if (IsCommandParameterGiven("-GenerateDocLaTeX"))
            {
                GenerateLatexDoc();
            }

            // will exit application after list has been generated
            if (IsCommandParameterGiven("-GenerateFunctionList"))
            {
                GenerateFunctionList();
            }

            SetDefaultTemplatesDir();

            // will exit application after list has been generated
            if (IsCommandParameterGiven("-GenerateComponentConnectionStatistics"))
            {
                GenerateComponentConnectionStatistics(IsCommandParameterGiven("-storeInBaseDir"));
            }

            try
            {
                if (Settings.Default.SingletonApplication && !EstablishSingleInstance())
                {
                    Environment.Exit(0);
                }
            }
            catch (Exception)
            {
                //do nothing
            }

            //if true, exists the constructor
            if (CheckForUpdate())
            {
                return;
            }

            UpgradeConfig();

            CreateAndSetPersonalProjectsDirectory();

            SaveSettingsSavely();

            SetEventHandlers();

            CreateDemoController();

            GuiLogMessage("Call InitializeComponent now", NotificationLevel.Debug);
            InitializeComponent();
            GuiLogMessage("InitializeComponent successfully called", NotificationLevel.Debug);

            UpdateSomeUIElements();

            InitializeDemoModeIfRequested();

            SetWindowState();

            RecentFileListChanged();

            CreateNotifyIcon();

            CheckUpdater();

            SetAdditionalEventHandlers();

            if (IsCommandParameterGiven("-ResetConfig"))
            {
                ResetConfig();
            }

            SetServerCertificateValidationCallback();

            InitCloud();
            GuiLogMessage("Finished creation of MainWindow", NotificationLevel.Debug);
        }

        /// <summary>
        /// Due to renaming of namespace "Cryptool" to "CrypTool", we have to once delete
        /// all settings folders
        /// If fixed.txt file does not exist we have to perform the deletion
        /// </summary>
        private void FixSettingsByDeletingThem()
        {
            try
            {
                string localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string fixedFile = localApplicationData + @"\CrypTool2\fixed_settings.txt";
                if (File.Exists(fixedFile))
                {
                    return;
                }
                string folder = localApplicationData + @"\CrypTool2";
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                }
                folder = localApplicationData + @"\CrypTool_2_Team";
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                }
                folder = localApplicationData + @"\CrypTool_Team";
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                }
                //create new empty CrypTool2 folder
                folder = localApplicationData + @"\CrypTool2";
                Directory.CreateDirectory(folder);

                FileStream stream = File.Create(fixedFile);
                stream.Close();
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception occured during FixSettingsByDeletingThem: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Sets the callback for the validation check of tls certificates
        /// </summary>
        private void SetServerCertificateValidationCallback()
        {
            //Load our certificates &
            //Install validation callback
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = UpdateServerCertificateValidationCallback;
                GuiLogMessage("Successfully set validation callback for certificate validation", NotificationLevel.Debug);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Error while initializing the certificate callback: {0}", ex), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Resets config files to default values
        /// </summary>
        private void ResetConfig()
        {
            GuiLogMessage("\"ResetConfig\" startup parameter set. Resetting configuration of CrypTool 2 to default configuration", NotificationLevel.Info);
            try
            {
                //Reset all plugins settings
                CrypTool.PluginBase.Properties.Settings.Default.Reset();
                //Reset WorkspaceManagerModel settings
                WorkspaceManagerModel.Properties.Settings.Default.Reset();
                //reset Crypwin settings
                CrypTool.CrypWin.Properties.Settings.Default.Reset();
                //reset Crypcore settings
                CrypTool.Core.Properties.Settings.Default.Reset();
                //reset MainWindow settings
                Settings.Default.Reset();
                GuiLogMessage("Settings successfully set to default configuration", NotificationLevel.Info);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Error occured while resetting configuration: {0}", ex), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Sets some additional event handlers
        /// </summary>
        private void SetAdditionalEventHandlers()
        {
            try
            {
                OnlineHelp.ShowDocPage += ShowHelpPage;

                SettingsPresentation.GetSingleton().OnGuiLogNotificationOccured += new GuiLogNotificationEventHandler(OnGuiLogNotificationOccured);
                Settings.Default.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
                {
                    try
                    {
                        //Always save everything immediately:
                        SaveSettingsSavely();

                        //Set new button image when needed:
                        CheckPreferredButton(e);

                        //Set lastPath to personal directory when lastPath is disabled:
                        if (e.PropertyName == "useLastPath" && !Settings.Default.useLastPath)
                        {
                            Settings.Default.LastPath = personalDir;
                        }
                    }
                    catch (Exception)
                    {
                        //wtf?
                    }
                };

                SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
                SystemEvents.SessionEnding += SystemEvents_SessionEnding;

                hasChangesCheckTimer = new System.Windows.Forms.Timer();
                hasChangesCheckTimer.Tick += new EventHandler(delegate
                {
                    try
                    {
                        if (ActiveEditor != null)
                        {
                            contentToTabMap[ActiveEditor].HasChanges = ActiveEditor.HasChanges ? true : false;
                        }
                    }
                    catch (Exception)
                    {
                        //wtf?
                    }
                });
                hasChangesCheckTimer.Interval = 800;
                hasChangesCheckTimer.Start();
                GuiLogMessage("Additionaly event handlers succesfully set", NotificationLevel.Debug);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception occured during setting of additional event handlers: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Checks and inits the updater
        /// </summary>
        private void CheckUpdater()
        {
            try
            {
                if (IsUpdaterEnabled)
                {
                    InitUpdater();
                    GuiLogMessage("Update successfully initialized", NotificationLevel.Debug);
                }
                else
                {
                    autoUpdateButton.Visibility = Visibility.Collapsed; // hide update button in ribbon
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception occured during checking of update: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Resets the window state of the window
        /// </summary>
        private void SetWindowState()
        {
            try
            {
                VisibilityStart = true;
                oldWindowState = WindowState;
                GuiLogMessage("Window state successfully set", NotificationLevel.Debug);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception occured during setting of window state: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Initialized demo mode if requested
        /// </summary>
        private void InitializeDemoModeIfRequested()
        {
            try
            {
                if (IsCommandParameterGiven("-demo") || IsCommandParameterGiven("-test"))
                {
                    ribbonDemoMode.Visibility = Visibility.Visible;
                    PluginExtension.IsTestMode = true;
                    LocExtension.OnGuiLogMessageOccured += GuiLogMessage;
                    GuiLogMessage("Demo mode initialized", NotificationLevel.Debug);
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception occured during initializing of demo mode: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Updates some ui elements
        /// </summary>
        private void UpdateSomeUIElements()
        {
            try
            {
                SettingBTN.IsChecked = false;
                dockWindowAlgorithmSettings.Close();

                if ((System.Windows.Visibility)Enum.Parse(typeof(System.Windows.Visibility), Properties.Settings.Default.PluginVisibility) == System.Windows.Visibility.Visible)
                {
                    PluginBTN.IsChecked = true;
                    dockWindowNaviPaneAlgorithms.Open();
                }
                else
                {
                    PluginBTN.IsChecked = false;
                    dockWindowNaviPaneAlgorithms.Close();
                }

                if ((System.Windows.Visibility)Enum.Parse(typeof(System.Windows.Visibility), Properties.Settings.Default.LogVisibility) == System.Windows.Visibility.Visible)
                {
                    LogBTN.IsChecked = true;
                    dockWindowLogMessages.Open();
                }
                else
                {
                    LogBTN.IsChecked = false;
                    dockWindowLogMessages.Close();
                }

                if (!Settings.Default.ShowRibbonBar)
                {
                    AppRibbon.IsEnabled = false;
                }

                if (!Settings.Default.ShowAlgorithmsNavigation)
                {
                    splitPanelNaviPaneAlgorithms.Visibility = Visibility.Collapsed;
                }

                if (!Settings.Default.ShowAlgorithmsSettings)
                {
                    splitPanelAlgorithmSettings.Visibility = Visibility.Collapsed;
                }

                GuiLogMessage("Successfully updated some ui elements", NotificationLevel.Debug);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception occured during update of some ui elements: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Creates the demo controller
        /// </summary>
        private void CreateDemoController()
        {
            try
            {
                demoController = new DemoController(this);
                GuiLogMessage("Successfully created demo controller", NotificationLevel.Debug);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception occured during creation of demo controller: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Set some event handlers
        /// </summary>
        private void SetEventHandlers()
        {
            try
            {
                ComponentConnectionStatistics.OnGuiLogMessageOccured += GuiLogMessage;
                ComponentConnectionStatistics.OnStatisticReset += GenerateStatisticsFromTemplates;
                recentFileList.ListChanged += RecentFileListChanged;
                Activated += MainWindow_Activated;
                Initialized += MainWindow_Initialized;
                Loaded += MainWindow_Loaded;
                Closing += MainWindow_Closing;
                GuiLogMessage("Event handlers successfully set", NotificationLevel.Debug);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception occured during setting of event hanlders: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Creates and sets the personal directory
        /// </summary>
        private void CreateAndSetPersonalProjectsDirectory()
        {
            try
            {
                personalDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CrypTool 2 Projects");
                if (!Directory.Exists(personalDir))
                {
                    Directory.CreateDirectory(personalDir);
                    GuiLogMessage("Personal folder created", NotificationLevel.Debug);
                }

                if (string.IsNullOrEmpty(Settings.Default.LastPath) || !Settings.Default.useLastPath || !Directory.Exists(Settings.Default.LastPath))
                {
                    Settings.Default.LastPath = personalDir;
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage("Exception occured during creation of personal folder: " + ex.Message, NotificationLevel.Error);
            }
        }


        /// <summary>
        /// Updgrades the config
        /// </summary>
        private void UpgradeConfig()
        {
            try
            {
                //upgrade the config
                //and fill some defaults
                if (Settings.Default.UpdateFlag)
                {
                    Settings.Default.Upgrade();
                    CrypTool.PluginBase.Properties.Settings.Default.Upgrade();
                    //upgrade WorkspaceManagerModel settings
                    WorkspaceManagerModel.Properties.Settings.Default.Upgrade();
                    //upgrade Crypwin settings
                    CrypTool.CrypWin.Properties.Settings.Default.Upgrade();
                    //upgrade Crypcore settings
                    CrypTool.Core.Properties.Settings.Default.Upgrade();
                    //upgrade MainWindow settings
                    Settings.Default.Upgrade();
                    //remove UpdateFlag
                    Settings.Default.UpdateFlag = false;
                    GuiLogMessage(string.Format("Config successfully upgrade"), NotificationLevel.Debug);
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception during upgrade of config: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Checks for and starts updates
        /// </summary>
        /// <returns></returns>
        private bool CheckForUpdate()
        {
            try
            {
                // check whether update is available to be installed
                if (IsUpdaterEnabled
                    && CheckCommandProjectFileGiven().Count == 0 // NO project file given as command line argument
                    && IsUpdateFileAvailable()) // update file ready for install
                {
                    // really start the update process?
                    if (Settings.Default.AutoInstall || AskForInstall())
                    {
                        // start update and check whether it seems to succeed
                        if (OnUpdate())
                        {
                            return true; // looking good, exit CrypWin constructor now
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception occured during check for update: {0}", ex.Message), NotificationLevel.Warning);
            }
            return false;
        }

        /// <summary>
        /// Generates the statistics of component connections
        /// </summary>
        private void GenerateComponentConnectionStatistics(bool storeInBaseDir)
        {
            try
            {
                GeneratingWindow generatingComponentConnectionStatistic = new GeneratingWindow();
                generatingComponentConnectionStatistic.Message.Content = Properties.Resources.StatisticsWaitMessage;
                generatingComponentConnectionStatistic.Title = Properties.Resources.Generating_Statistics_Title;
                generatingComponentConnectionStatistic.Show();
                TemplatesAnalyzer.GenerateStatisticsFromTemplate(defaultTemplatesDirectory);
                SaveComponentConnectionStatistics(storeInBaseDir);
                generatingComponentConnectionStatistic.Close();
            }
            catch (Exception)
            {
                //wtf?
            }
            Environment.Exit(0);
        }

        /// <summary>
        /// Sets the default templates directory
        /// </summary>
        private void SetDefaultTemplatesDir()
        {
            try
            {
                defaultTemplatesDirectory = Path.Combine(DirectoryHelper.BaseDirectory, Settings.Default.SamplesDir);
                if (!Directory.Exists(defaultTemplatesDirectory))
                {
                    GuiLogMessage("Directory with project templates not found", NotificationLevel.Debug);
                    defaultTemplatesDirectory = personalDir;
                }

                GuiLogMessage(string.Format("Set default templates dir to: {0}", defaultTemplatesDirectory), NotificationLevel.Debug);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception occured during setting of default templates dir: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Generates a function list
        /// </summary>
        private void GenerateFunctionList()
        {
            try
            {
                GeneratingWindow generatingDocWindow = new GeneratingWindow();
                generatingDocWindow.Message.Content = Properties.Resources.GeneratingFunctionListWaitMessage;
                generatingDocWindow.Title = Properties.Resources.Generating_Documentation_Title;
                generatingDocWindow.Show();
                OnlineDocumentationGenerator.DocGenerator docGenerator = new OnlineDocumentationGenerator.DocGenerator();
                docGenerator.Generate(DirectoryHelper.BaseDirectory, new FunctionListGenerator());
                generatingDocWindow.Close();
            }
            catch (Exception)
            {
            }
            Environment.Exit(0);
        }

        /// <summary>
        /// Generates a LaTeX documentation
        /// </summary>
        private void GenerateLatexDoc()
        {
            try
            {
                bool noIcons = IsCommandParameterGiven("-NoIcons");
                bool showAuthors = IsCommandParameterGiven("-ShowAuthors");
                GeneratingWindow generatingDocWindow = new GeneratingWindow();
                generatingDocWindow.Message.Content = Properties.Resources.GeneratingLaTeXWaitMessage;
                generatingDocWindow.Title = Properties.Resources.Generating_Documentation_Title;
                generatingDocWindow.Show();
                OnlineDocumentationGenerator.DocGenerator docGenerator = new OnlineDocumentationGenerator.DocGenerator();
                docGenerator.Generate(DirectoryHelper.BaseDirectory, new LaTeXGenerator(noIcons, showAuthors));
                generatingDocWindow.Close();
            }
            catch (Exception)
            {
                //wtf?    
            }
            Environment.Exit(0);
        }

        /// <summary>
        /// Generates a html documentation
        /// </summary>
        private void GenerateDoc()
        {
            try
            {
                GeneratingWindow generatingDocWindow = new GeneratingWindow();
                generatingDocWindow.Message.Content = Properties.Resources.GeneratingWaitMessage;
                generatingDocWindow.Title = Properties.Resources.Generating_Documentation_Title;
                generatingDocWindow.Show();
                OnlineDocumentationGenerator.DocGenerator docGenerator = new OnlineDocumentationGenerator.DocGenerator();
                docGenerator.Generate(DirectoryHelper.BaseDirectory, new HtmlGenerator());
                generatingDocWindow.Close();
            }
            catch (Exception)
            {
                //wtf?    
            }
            Environment.Exit(0);
        }

        /// <summary>
        /// Creates the system info tab and licenses tab
        /// </summary>
        private void CreateSystemInfosAndLicencesTabs()
        {
            try
            {
                // systemInfos and licenses must be initialized after the language has been set, otherwise they are initialized with the wrong language
                systemInfos = new SystemInfos();
                GuiLogMessage("Created SystemInfos tab", NotificationLevel.Debug);
                licenses = new LicensesTab();
                GuiLogMessage("Created LicensesTab tab", NotificationLevel.Debug);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception occured during creation of SystemInfos tab and LicensesTab tab:{0}", ex.Message), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Encorces the usage of TLS 1.2
        /// </summary>
        private void EnforceTLS12()
        {
            //Enforce usage of TLS 1.2
            try
            {
                //Enable TLS 1.2
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                //Disable old SSL and TLS versions
                ServicePointManager.SecurityProtocol &= ~(SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11);
                GuiLogMessage("TLS 1.2 enabled and disabed old protocols", NotificationLevel.Debug);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception while enabling TLS 1.2 and disabling old protocols: {0}", ex), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// This method checks, if essential components are available
        /// If not, it shows a message box containing an error and exits CT2 with return code -1
        /// 
        /// We added this to avoid spamming of tickets of users, that directly start CT2 
        /// in the zip file of the zip installation
        /// </summary>
        private void CheckEssentialComponents()
        {
            try
            {
                //List of components that have to be available
                //add new essential compoents if needed
                //Warning: If a component is not available; CT2 WONT START!
                List<string> essentialComponents = new List<string>
                {
                    System.AppDomain.CurrentDomain.BaseDirectory + @"\CrypCore.dll",
                    System.AppDomain.CurrentDomain.BaseDirectory + @"\CrypPluginBase.dll",
                    System.AppDomain.CurrentDomain.BaseDirectory + @"\CrypProprietary.dll",
                    System.AppDomain.CurrentDomain.BaseDirectory + @"\WorkspaceManagerModel.dll",
                    System.AppDomain.CurrentDomain.BaseDirectory + @"\DevComponents.WpfDock.dll",
                    System.AppDomain.CurrentDomain.BaseDirectory + @"\DevComponents.WpfEditors.dll",
                    System.AppDomain.CurrentDomain.BaseDirectory + @"\DevComponents.WpfRibbon.dll",
                    System.AppDomain.CurrentDomain.BaseDirectory + @"\OnlineDocumentationGenerator.dll",
                    System.AppDomain.CurrentDomain.BaseDirectory + @"\CrypPlugins\WorkspaceManager.dll",
                    System.AppDomain.CurrentDomain.BaseDirectory + @"\CrypPlugins\Wizard.dll"
                };

                foreach (string file in essentialComponents)
                {
                    if (!File.Exists(file))
                    {
                        MessageBox.Show(string.Format("Missing essential component of CrypTool 2:\r\n{0}\r\nEither completely unzip the zip installation of CrypTool 2 or reinstall CrypTool 2 if not running zip installation", file),
                            "CrypTool 2 Essential Component not Found!", MessageBoxButton.OK, MessageBoxImage.Error);
                        System.Environment.Exit(-1);
                    }
                }

                GuiLogMessage("Essential components checked successfully", NotificationLevel.Debug);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception occured during checking of essential components: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Loads the component connection statistics
        /// </summary>
        private void LoadIndividualComponentConnectionStatistics()
        {
            //Load component connection statistics if available, or generate them:
            try
            {
                ComponentConnectionStatistics.LoadCurrentStatistics(Path.Combine(DirectoryHelper.DirectoryLocal, "ccs.xml"));
            }
            catch (Exception)
            {
                try
                {
                    ComponentConnectionStatistics.LoadCurrentStatistics(Path.Combine(DirectoryHelper.BaseDirectory, "ccs.xml"));
                }
                catch (Exception)
                {
                    GuiLogMessage("No component connection statistics found... Generate them.", NotificationLevel.Info);
                    GenerateStatisticsFromTemplates();
                }
            }
        }

        /// <summary>
        /// Generates statistics from a template
        /// </summary>
        private void GenerateStatisticsFromTemplates()
        {
            GeneratingWindow generatingComponentConnectionStatistic = new GeneratingWindow();
            generatingComponentConnectionStatistic.Message.Content = Properties.Resources.StatisticsWaitMessage;
            generatingComponentConnectionStatistic.Title = Properties.Resources.Generating_Statistics_Title;
            BitmapImage icon = new BitmapImage();
            icon.BeginInit();
            icon.UriSource = new Uri("pack://application:,,,/CrypWin;component/images/statistics_icon.png");
            icon.EndInit();
            generatingComponentConnectionStatistic.Icon.Source = icon;
            generatingComponentConnectionStatistic.ContentRendered += delegate
            {
                TemplatesAnalyzer.GenerateStatisticsFromTemplate(defaultTemplatesDirectory);
                SaveComponentConnectionStatistics();
                GuiLogMessage("Component connection statistics successfully generated from templates.", NotificationLevel.Info);
                generatingComponentConnectionStatistic.Close();
            };
            generatingComponentConnectionStatistic.ShowDialog();
        }

        /// <summary>
        /// Establishes CrypTool as singleton
        /// </summary>
        /// <returns></returns>
        private bool EstablishSingleInstance()
        {
            try
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                int id = BitConverter.ToInt32(md5.ComputeHash(Encoding.ASCII.GetBytes(Environment.GetCommandLineArgs()[0])), 0);
                string identifyingString = string.Format("Local\\CrypTool 2 ID {0}", id);
                _singletonMutex = new Mutex(true, identifyingString, out bool createdNew);

                if (createdNew)
                {
                    Thread queueThread = new Thread(QueueThread)
                    {
                        IsBackground = true
                    };
                    queueThread.Start(identifyingString);
                }
                else
                {
                    //CT2 instance already exists... send files to it and shutdown
                    using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", identifyingString, PipeDirection.Out))
                    {
                        pipeClient.Connect();
                        using (StreamWriter sw = new StreamWriter(pipeClient))
                        {
                            foreach (string file in CheckCommandProjectFileGiven())
                            {
                                sw.WriteLine(file);
                            }
                        }
                    }
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception occured during EstablishSingleInstance: {0}", ex.Message), NotificationLevel.Warning);
                return false;
            }
        }

        private void QueueThread(object identifyingString)
        {
            while (true)
            {
                try
                {
                    using (NamedPipeServerStream pipeServer = new NamedPipeServerStream((string)identifyingString, PipeDirection.In))
                    {

                        pipeServer.WaitForConnection();
                        BringToFront();
                        using (StreamReader sr = new StreamReader(pipeServer))
                        {
                            string file;
                            do
                            {
                                file = sr.ReadLine();
                                if (!string.IsNullOrEmpty(file))
                                {
                                    string theFile = file;
                                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                    {
                                        GuiLogMessage(string.Format(Resource.workspace_loading, theFile), NotificationLevel.Info);
                                        OpenProject(theFile, null);
                                    }, null);
                                }
                            } while (!string.IsNullOrEmpty(file));
                        }
                    }
                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format("Error while maintaining single instance: {0}", ex.Message), NotificationLevel.Error);
                }
            }
        }

        private void BringToFront()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                WindowState = WindowState.Normal;
                Activate();
            }, null);
        }

        private bool UpdateServerCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate == null)
            {
                GuiLogMessage("CrypWin: Could not validate certificate, as it is null! Aborting connection.", NotificationLevel.Error);
                return false;
            }

            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNotAvailable)
            {
                GuiLogMessage("CrypWin: Could not validate TLS certificate, as the server did not provide one! Aborting connection.", NotificationLevel.Error);
                return false;
            }


            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch)
            {
                GuiLogMessage("CrypWin: Certificate name mismatch (certificate not for www.CrypTool.org?). Aborting connection.", NotificationLevel.Error);
                return false;
            }

            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors)
            {
                GuiLogMessage("CrypWin: Certificate-chain could not be validated (using self-signed certificate?)! Aborting connection.", NotificationLevel.Error);
                return false;
            }

            // Catch any other SSLPoliy errors - should never happen, as all oerror are captured before, but to be on the safe side.
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                GuiLogMessage("CrypWin: General SSL/TLS policy error: " + sslPolicyErrors.ToString() + " Aborting connection.", NotificationLevel.Error);
                return false;
            }

            /* Removed by Nils Kopal 07th May 2018. We also use TLS for communication with DECODE database. Every time we talk to it, this message appeared.
               We only want to see messages when something BAD happens... not always in the good case...
              
              GuiLogMessage("CrypWin: The webserver serving the URL " + ((HttpWebRequest)sender).Address.ToString() + " provided a valid SSL/TLS certificate. We trust it." + Environment.NewLine + 
                "Certificate:  " + certificate.Subject + Environment.NewLine +
                "Issuer: " + certificate.Issuer, 
                NotificationLevel.Debug);
             */

            return true;
        }

        /// <summary>
        /// Sets the language of CrypTool 2
        /// </summary>
        private void SetLanguage()
        {
            try
            {
                string lang = GetCommandParameter("-lang"); //Check if language parameter is given
                if (lang != null)
                {
                    switch (lang.ToLower())
                    {
                        case "de":
                            Settings.Default.Culture = CultureInfo.CreateSpecificCulture("de-DE").TextInfo.CultureName;
                            break;
                        case "en":
                            Settings.Default.Culture = CultureInfo.CreateSpecificCulture("en-US").TextInfo.CultureName;
                            break;
                    }
                }

                string culture = Settings.Default.Culture;
                if (!string.IsNullOrEmpty(culture))
                {
                    try
                    {
                        Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
                        Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
                        currentculture = Thread.CurrentThread.CurrentCulture;
                        currentuiculture = Thread.CurrentThread.CurrentUICulture;
                        GuiLogMessage(string.Format("Set language to: {0}", culture), NotificationLevel.Debug);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception while setting language: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        private void CheckPreferredButton(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "useDefaultEditor" || e.PropertyName == "preferredEditor" || e.PropertyName == "defaultEditor")
            {
                string checkEditor;
                if (!Settings.Default.useDefaultEditor)
                {
                    checkEditor = Settings.Default.preferredEditor;
                }
                else
                {
                    checkEditor = Settings.Default.defaultEditor;
                }
                foreach (ButtonDropDown editorButtons in buttonDropDownNew.Items)
                {
                    Type type = (Type)editorButtons.Tag;
                    editorButtons.IsChecked = (type.FullName == checkEditor);
                    if (editorButtons.IsChecked)
                    {
                        ((Image)buttonDropDownNew.Image).Source = ((Image)editorButtons.Image).Source;
                    }
                }
            }
        }

        private void PlayStopMenuItemClicked(object sender, EventArgs e)
        {
            if (ActiveEditor == null)
            {
                return;
            }

            if (ActiveEditor.CanStop && !(bool)playStopMenuItem.Tag)
            {
                ActiveEditor.Stop();
                playStopMenuItem.Text = "Start";
                playStopMenuItem.Tag = true;
            }
            else if (ActiveEditor.CanExecute && (bool)playStopMenuItem.Tag)
            {
                ActiveEditor.Execute();
                playStopMenuItem.Text = "Stop";
                playStopMenuItem.Tag = false;
            }
        }

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            WindowState = oldWindowState;
        }

        private void LoadResources()
        {
            try
            {
                Application.Current.Resources.MergedDictionaries.Add((ResourceDictionary)Application.LoadComponent(
                new Uri("/CrypWin;Component/Resources/GridViewStyle.xaml", UriKind.Relative)));

                Application.Current.Resources.MergedDictionaries.Add((ResourceDictionary)Application.LoadComponent(
                new Uri("/CrypWin;Component/Resources/ValidationRules.xaml", UriKind.Relative)));

                Application.Current.Resources.MergedDictionaries.Add((ResourceDictionary)Application.LoadComponent(
                new Uri("/CrypWin;Component/Resources/BlackTheme.xaml", UriKind.Relative)));

                Application.Current.Resources.MergedDictionaries.Add((ResourceDictionary)Application.LoadComponent(
                new Uri("/CrypWin;Component/Resources/Expander.xaml", UriKind.Relative)));

                Application.Current.Resources.MergedDictionaries.Add((ResourceDictionary)Application.LoadComponent(
                new Uri("/CrypWin;Component/Resources/ToggleButton.xaml", UriKind.Relative)));
                GuiLogMessage("Resources loaded", NotificationLevel.Debug);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception during loading of resources: {0}", ex.Message), NotificationLevel.Debug);
            }
        }

        /// <summary>
        /// Called when window goes to foreground.
        /// </summary>
        private void MainWindow_Activated(object sender, EventArgs e)
        {
            if (startUpRunning && splashWindow != null)
            {
                splashWindow.Activate();
            }
            else if (!startUpRunning)
            {
                Activated -= MainWindow_Activated;
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    if (ActiveEditor != null)
                    {
                        ActiveEditor.Presentation.Focus();
                    }
                }, null);
            }
        }

        private void MainWindow_Initialized(object sender, EventArgs e)
        {
            PluginExtension.OnGuiLogNotificationOccured += OnGuiLogNotificationOccured;

            HashSet<string> disabledAssemblies = new HashSet<string>();
            if (Settings.Default.DisabledPlugins != null)
            {
                foreach (PluginInformation disabledPlugin in Settings.Default.DisabledPlugins)
                {
                    disabledAssemblies.Add(disabledPlugin.Assemblyname);
                }
            }

            //Translate the Ct2BuildType to a folder name for CrypToolStore plugins                
            string CrypToolStoreSubFolder = "";
            switch (AssemblyHelper.BuildType)
            {
                case Ct2BuildType.Developer:
                    CrypToolStoreSubFolder = "Developer";
                    break;
                case Ct2BuildType.Nightly:
                    CrypToolStoreSubFolder = "Nightly";
                    break;
                case Ct2BuildType.Beta:
                    CrypToolStoreSubFolder = "Beta";
                    break;
                case Ct2BuildType.Stable:
                    CrypToolStoreSubFolder = "Release";
                    break;
                default: //if no known version is given, we assume developer
                    CrypToolStoreSubFolder = "Developer";
                    break;
            }

            pluginManager = new PluginManager(disabledAssemblies, CrypToolStoreSubFolder);
            pluginManager.OnExceptionOccured += pluginManager_OnExceptionOccured;
            pluginManager.OnDebugMessageOccured += pluginManager_OnDebugMessageOccured;
            pluginManager.OnPluginLoaded += pluginManager_OnPluginLoaded;

            #region GUI stuff without plugin access

            naviPane.SystemText.CollapsedPaneText = Properties.Resources.Classic_Ciphers;
            RibbonControl.SystemText.QatPlaceBelowRibbonText = Resource.show_quick_access_toolbar_below_the_ribbon;

            // standard filter
            listViewLogList.ItemsSource = collectionLogMessages;
            listFilter.Add(NotificationLevel.Info);
            listFilter.Add(NotificationLevel.Warning);
            listFilter.Add(NotificationLevel.Error);
            ICollectionView view = CollectionViewSource.GetDefaultView(listViewLogList.ItemsSource);
            view.Filter = new Predicate<object>(FilterCallback);

            // Set user view
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (Settings.Default.IsWindowMaximized || Settings.Default.RelWidth >= 1 || Settings.Default.RelHeight >= 1)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                Width = System.Windows.SystemParameters.PrimaryScreenWidth * Settings.Default.RelWidth;
                Height = System.Windows.SystemParameters.PrimaryScreenHeight * Settings.Default.RelHeight;
            }
            dockWindowLogMessages.IsAutoHide = Settings.Default.logWindowAutoHide;

            IsEnabled = false;
            splashWindow = new Splash();
            if (!IsCommandParameterGiven("-nosplash"))
            {
                splashWindow.Show();
            }
            # endregion

            AsyncCallback asyncCallback = new AsyncCallback(LoadingPluginsFinished);
            LoadPluginsDelegate loadPluginsDelegate = new LoadPluginsDelegate(LoadPlugins);
            loadPluginsDelegate.BeginInvoke(asyncCallback, null);
        }

        private bool IsCommandParameterGiven(string parameter)
        {
            try
            {
                string[] args = Environment.GetCommandLineArgs();
                for (int i = 1; i < args.Length; i++)
                {
                    if (args[i].Equals(parameter, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception during checking for command parameter: {0}", ex.Message), NotificationLevel.Error);
                return false;
            }
        }

        private string GetCommandParameter(string parameter)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 1; i < args.Length - 1; i++)
            {
                if (args[i].Equals(parameter, StringComparison.InvariantCultureIgnoreCase))
                {
                    return args[i + 1];
                }
            }

            return null;
        }

        private void InitTypes(Dictionary<string, List<Type>> dicTypeLists)
        {
            // process ICrypComponent (regular editor plugins)
            InitCrypComponents(dicTypeLists[typeof(ICrypComponent).FullName]);

            // process ICrypTutorial (former standalone plugins)
            InitCrypTutorials(dicTypeLists[typeof(ICrypTutorial).FullName]);

            // process IEditor
            InitCrypEditors(dicTypeLists[typeof(IEditor).FullName]);
        }

        private void InitCrypComponents(List<Type> typeList)
        {
            foreach (Type type in typeList)
            {
                PluginInfoAttribute pia = type.GetPluginInfoAttribute();
                if (pia == null)
                {
                    GuiLogMessage(string.Format(Resource.no_plugin_info_attribute, type.Name), NotificationLevel.Error);
                    continue;
                }

                foreach (ComponentCategoryAttribute attr in type.GetComponentCategoryAttributes())
                {
                    GUIContainerElementsForPlugins cont = null;

                    switch (attr.Category)
                    {
                        case ComponentCategory.CiphersClassic:
                            cont = new GUIContainerElementsForPlugins(type, pia, navPaneItemClassic, navListBoxClassic, Properties.Resources.Classic_Ciphers);
                            break;
                        case ComponentCategory.CiphersModernSymmetric:
                            cont = new GUIContainerElementsForPlugins(type, pia, navPaneItemModernCiphers, navListBoxModernCiphersSymmetric, Properties.Resources.Symmetric);
                            break;
                        case ComponentCategory.CiphersModernAsymmetric:
                            cont = new GUIContainerElementsForPlugins(type, pia, navPaneItemModernCiphers, navListBoxModernCiphersAsymmetric, Properties.Resources.Asymmetric);
                            break;
                        case ComponentCategory.Steganography:
                            cont = new GUIContainerElementsForPlugins(type, pia, navPaneItemSteganography, navListBoxSteganography, Properties.Resources.Steganography);
                            break;
                        case ComponentCategory.HashFunctions:
                            cont = new GUIContainerElementsForPlugins(type, pia, navPaneItemHash, navListBoxHashFunctions, Properties.Resources.Hash_Functions_);
                            break;
                        case ComponentCategory.CryptanalysisSpecific:
                            cont = new GUIContainerElementsForPlugins(type, pia, navPaneItemCryptanalysis, navListBoxCryptanalysisSpecific, Properties.Resources.Specific);
                            break;
                        case ComponentCategory.CryptanalysisGeneric:
                            cont = new GUIContainerElementsForPlugins(type, pia, navPaneItemCryptanalysis, navListBoxCryptanalysisGeneric, Properties.Resources.Generic);
                            break;
                        case ComponentCategory.Protocols:
                            cont = new GUIContainerElementsForPlugins(type, pia, navPaneItemProtocols, navListBoxProtocols, Properties.Resources.Protocols);
                            break;
                        case ComponentCategory.ToolsBoolean:
                            cont = new GUIContainerElementsForPlugins(type, pia, navPaneItemTools, navListBoxToolsBoolean, Properties.Resources.Boolean);
                            break;
                        case ComponentCategory.ToolsDataflow:
                            cont = new GUIContainerElementsForPlugins(type, pia, navPaneItemTools, navListBoxToolsDataflow, Properties.Resources.Dataflow);
                            break;
                        case ComponentCategory.ToolsDataInputOutput:
                            cont = new GUIContainerElementsForPlugins(type, pia, navPaneItemTools, navListBoxToolsData, Properties.Resources.DataInputOutput);
                            break;
                        case ComponentCategory.ToolsRandomNumbers:
                            cont = new GUIContainerElementsForPlugins(type, pia, navPaneItemTools, navListBoxToolsRandomNumbers, Properties.Resources.RandomNumbers);
                            break;
                        case ComponentCategory.ToolsCodes:
                            cont = new GUIContainerElementsForPlugins(type, pia, navPaneItemTools, navListBoxToolsCodes, Properties.Resources.Codes);
                            break;
                        case ComponentCategory.ToolsMisc:
                            cont = new GUIContainerElementsForPlugins(type, pia, navPaneItemTools, navListBoxToolsMisc, Properties.Resources.Misc);
                            break;

                        default:
                            GuiLogMessage(string.Format("Category {0} of plugin {1} not handled in CrypWin", attr.Category, pia.Caption), NotificationLevel.Error);
                            break;
                    }

                    if (cont != null)
                    {
                        AddPluginToNavigationPane(cont);
                    }
                }

                SendAddedPluginToGUIMessage(pia.Caption);
            }
        }

        private void InitCrypTutorials(List<Type> typeList)
        {
            if (typeList.Count > 0)
            {
                SetVisibility(ribbonTabView, Visibility.Visible);
            }

            foreach (Type type in typeList)
            {
                PluginInfoAttribute pia = type.GetPluginInfoAttribute();
                if (pia == null)
                {
                    GuiLogMessage(string.Format(Resource.no_plugin_info_attribute, type.Name), NotificationLevel.Error);
                    continue;
                }

                Type typeClosure = type;
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ButtonDropDown button = new ButtonDropDown
                    {
                        Header = pia.Caption,
                        ToolTip = pia.ToolTip,
                        Image = typeClosure.GetImage(0, 64, 40),
                        ImageSmall = typeClosure.GetImage(0, 20, 16),
                        ImagePosition = eButtonImagePosition.Left,
                        Tag = typeClosure,
                        Style = (Style)FindResource("AppMenuCommandButton"),
                        Height = 65
                    };

                    button.Click += buttonTutorial_Click;

                    ribbonBarTutorial.Items.Add(button);
                }, null);

                SendAddedPluginToGUIMessage(pia.Caption);
            }
        }

        // CrypTutorial ribbon bar clicks
        private void buttonTutorial_Click(object sender, RoutedEventArgs e)
        {
            ButtonDropDown button = sender as ButtonDropDown;
            if (button == null)
            {
                return;
            }

            Type type = button.Tag as Type;
            if (type == null)
            {
                return;
            }

            //CrypTutorials are singletons:
            foreach (KeyValuePair<object, CTTabItem> tab in contentToTabMap.Where(x => x.Key.GetType() == type))
            {
                tab.Value.IsSelected = true;
                return;
            }

            ICrypTutorial content = type.CreateTutorialInstance();
            if (content == null)
            {
                return;
            }

            OpenTab(content, new TabInfo()
            {
                Title = type.GetPluginInfoAttribute().Caption,
                Icon = content.GetType().GetImage(0).Source,
                Tooltip = new Span(new Run(content.GetPluginInfoAttribute().ToolTip))
            }, null);
            //content.Presentation.ToolTip = type.GetPluginInfoAttribute().ToolTip;
        }

        private void InitCrypEditors(List<Type> typeList)
        {
            foreach (Type type in typeList)
            {
                PluginInfoAttribute pia = type.GetPluginInfoAttribute();

                // We dont't display a drop down button while only one editor is available
                if (typeList.Count > 1)
                {
                    EditorInfoAttribute editorInfo = type.GetEditorInfoAttribute();
                    if (editorInfo != null && !editorInfo.ShowAsNewButton)
                    {
                        continue;
                    }

                    Type typeClosure = type;
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        ButtonDropDown button = new ButtonDropDown
                        {
                            Header = typeClosure.GetPluginInfoAttribute().Caption,
                            ToolTip = typeClosure.GetPluginInfoAttribute().ToolTip
                        };
                        Image image = typeClosure.GetImage(0);
                        image.Height = 35;
                        button.Image = image;
                        button.Tag = typeClosure;
                        button.IsCheckable = true;
                        if (Settings.Default.useDefaultEditor && typeClosure.FullName == Settings.Default.defaultEditor
                            || !Settings.Default.useDefaultEditor && typeClosure.FullName == Settings.Default.preferredEditor)
                        {
                            button.IsChecked = true;
                            ((Image)buttonDropDownNew.Image).Source = ((Image)button.Image).Source;
                        }

                        button.Click += buttonEditor_Click;
                        //buttonDropDownEditor.Items.Add(btn);
                        buttonDropDownNew.Items.Add(button);
                        AvailableEditors.Add(typeClosure);
                    }, null);
                }
            }

            if (typeList.Count <= 1)
            {
                SetVisibility(buttonDropDownNew, Visibility.Collapsed);
            }
        }

        private void buttonEditor_Click(object sender, RoutedEventArgs e)
        {
            IEditor editor = AddEditorDispatched(((sender as Control).Tag as Type));
            editor.PluginManager = pluginManager;
            Settings.Default.defaultEditor = ((sender as Control).Tag as Type).FullName;
            ButtonDropDown button = sender as ButtonDropDown;

            if (Settings.Default.useDefaultEditor)
            {
                ((Image)buttonDropDownNew.Image).Source = ((Image)button.Image).Source;
                foreach (ButtonDropDown btn in buttonDropDownNew.Items)
                {
                    if (btn != button)
                    {
                        btn.IsChecked = false;
                    }
                }
            }
            else
            {
                button.IsChecked = (Settings.Default.preferredEditor == ((Type)button.Tag).FullName);
            }
        }

        private void SetVisibility(UIElement element, Visibility vis)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                element.Visibility = vis;
            }, null);
        }

        /// <summary>
        /// Method is invoked after plugin manager has finished loading plugins and 
        /// CrypWin is building the plugin entries. Hence 50% is added to progess here.
        /// </summary>
        /// <param name="plugin">The plugin.</param>
        private void SendAddedPluginToGUIMessage(string plugin)
        {
            initCounter++;
            splashWindow.ShowStatus(string.Format(Properties.Resources.Added_plugin___0__, plugin), 50 + initCounter / ((double)numberOfLoadedTypes) * 100);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Hide the native expand button of naviPane, because we use resize/hide functions of SplitPanel Element
            Button naviPaneExpandButton = naviPane.Template.FindName("ExpandButton", naviPane) as Button;
            if (naviPaneExpandButton != null)
            {
                naviPaneExpandButton.Visibility = Visibility.Collapsed;
            }
        }

        [Conditional("DEBUG")]
        private void InitDebug()
        {
            dockWindowLogMessages.IsAutoHide = false;
        }

        private readonly HashSet<Type> pluginInSearchListBox = new HashSet<Type>();
        private Mutex _singletonMutex;

        private void AddPluginToNavigationPane(GUIContainerElementsForPlugins contElements)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                Image image = contElements.Plugin.GetImage(0);
                if (image != null)
                {
                    ListBoxItem navItem = CreateNavItem(contElements, image);
                    if (!pluginInSearchListBox.Contains(contElements.Plugin))
                    {
                        ListBoxItem navItem2 = CreateNavItem(contElements, contElements.Plugin.GetImage(0));
                        navListBoxSearch.Items.Add(navItem2);
                        pluginInSearchListBox.Add(contElements.Plugin);
                    }

                    if (!contElements.PaneItem.IsVisible)
                    {
                        contElements.PaneItem.Visibility = Visibility.Visible;
                    }

                    contElements.ListBox.Items.Add(navItem);
                }
                else
                {
                    if (contElements.PluginInfo != null)
                    {
                        GuiLogMessage(string.Format(Resource.plugin_has_no_icon, contElements.PluginInfo.Caption), NotificationLevel.Error);
                    }
                    else if (contElements.PluginInfo == null && contElements.Plugin != null)
                    {
                        GuiLogMessage("Missing PluginInfoAttribute on Plugin: " + contElements.Plugin.ToString(), NotificationLevel.Error);
                    }
                }
            }, null);
        }

        private ListBoxItem CreateNavItem(GUIContainerElementsForPlugins contElements, Image image)
        {
            image.Margin = new Thickness(16, 0, 5, 0);
            image.Height = 25;
            image.Width = 25;
            TextBlock textBlock = new TextBlock
            {
                FontWeight = FontWeights.DemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                Text = contElements.PluginInfo.Caption
            };
            textBlock.Tag = textBlock.Text;

            StackPanel stackPanel = new StackPanel();
            if (CultureInfo.CurrentUICulture.Name != "en")
            {
                string englishCaption = contElements.PluginInfo.EnglishCaption;
                if (englishCaption != textBlock.Text)
                {
                    stackPanel.Tag = englishCaption;
                }
            }

            stackPanel.Orientation = Orientation.Horizontal;
            stackPanel.Margin = new Thickness(0, 2, 0, 2);
            stackPanel.VerticalAlignment = VerticalAlignment.Center;
            stackPanel.Children.Add(image);
            stackPanel.Children.Add(textBlock);
            ListBoxItem navItem = new ListBoxItem
            {
                Content = stackPanel,
                Tag = contElements.Plugin
            };

            string category = contElements.PaneItem.Header as string;
            string subcategory = contElements.GroupName;
            Span categorySpan = new Span()
            {
                Inlines =
                {
                    new Bold(new Run($"{Properties.Resources.Category}: ")),
                    new Run(category)
                }
            };
            if (category != subcategory)
            {
                categorySpan.Inlines.Add(new Run(" / "));
                categorySpan.Inlines.Add(contElements.GroupName);
            }
            navItem.ToolTip = new ToolTip()
            {
                Content = new TextBlock
                {
                    Inlines =
                    {
                        new Run(contElements.PluginInfo.ToolTip),
                        new LineBreak(),
                        new LineBreak(),
                        categorySpan
                    }
                }
            };

            // dragDrop handler
            navItem.PreviewMouseDown += navItem_PreviewMouseDown;
            navItem.PreviewMouseMove += navItem_PreviewMouseMove;
            navItem.MouseDoubleClick += navItem_MouseDoubleClick;
            return navItem;
        }

        private void LoadPlugins()
        {
            if (currentculture != null)
            {
                Thread.CurrentThread.CurrentCulture = currentculture;
            }
            if (currentuiculture != null)
            {
                Thread.CurrentThread.CurrentUICulture = currentuiculture;
            }
            Dictionary<string, List<Type>> pluginTypes = new Dictionary<string, List<Type>>();
            foreach (string interfaceName in interfaceNameList)
            {
                pluginTypes.Add(interfaceName, new List<Type>());
            }

            PluginList.AddDisabledPluginsToPluginList(Settings.Default.DisabledPlugins);

            Dictionary<string, Type>.ValueCollection loadedPluginAssemblies = pluginManager.LoadTypes(AssemblySigningRequirement.LoadAllAssemblies).Values;
            foreach (Type pluginType in loadedPluginAssemblies.Where(it => !it.IsAbstract))
            {
                ComponentInformations.AddPlugin(pluginType);

                if (pluginType.GetInterface("IEditor") == null)
                {
                    PluginList.AddTypeToPluginList(pluginType);
                }

                Type type = pluginType;
                foreach (string interfaceName in interfaceNameList.Where(it => type.GetInterface(it) != null))
                {
                    pluginTypes[interfaceName].Add(pluginType);
                    numberOfLoadedTypes++;
                }
            }

            foreach (KeyValuePair<string, List<Type>> pluginType in pluginTypes)
            {
                pluginType.Value.Sort(
                    (x, y) => string.Compare(x.GetPluginInfoAttribute().Caption, y.GetPluginInfoAttribute().Caption, StringComparison.Ordinal)
                );

            }

            loadedTypes = pluginTypes;
        }

        public void LoadingPluginsFinished(IAsyncResult ar)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    PluginList.Finished();
                }
                catch (Exception ex)
                {
                    GuiLogMessage(ex.Message, NotificationLevel.Error);
                }
            }, null);

            try
            {
                AsyncResult asyncResult = ar as AsyncResult;
                LoadPluginsDelegate exe = asyncResult.AsyncDelegate as LoadPluginsDelegate;

                // check if plugin thread threw an exception
                try
                {
                    exe.EndInvoke(ar);
                }
                catch (Exception exception)
                {
                    GuiLogMessage(exception.Message, NotificationLevel.Error);
                }
                // Init of this stuff has to be done after plugins have been loaded
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        ComponentInformations.EditorExtension = GetEditorExtension(loadedTypes[typeof(IEditor).FullName]);
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage(ex.Message, NotificationLevel.Error);
                    }
                }, null);
                AsyncCallback asyncCallback = new AsyncCallback(TypeInitFinished);
                InitTypesDelegate initTypesDelegate = new InitTypesDelegate(InitTypes);
                initTypesDelegate.BeginInvoke(loadedTypes, asyncCallback, null);
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
        }

        /// <summary>
        /// CrypWin startup finished, show window stuff.
        /// </summary>
        public void TypeInitFinished(IAsyncResult ar)
        {
            try
            {
                // check if plugin thread threw an exception
                CheckInitTypesException(ar);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        Visibility = Visibility.Visible;
                        Show();

                        #region Gui-Stuff
                        Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                        Version version = AssemblyHelper.GetVersion(assembly);
                        OnGuiLogNotificationOccuredTS(this, new GuiLogEventArgs(Resource.CrypTool + " " + version.ToString() + Resource.started_and_ready, null, NotificationLevel.Info));

                        IsEnabled = true;
                        AppRibbon.Items.Refresh();
                        splashWindow.Close();
                        Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                        #endregion Gui-Stuff

                        InitDebug();

                        LoadIndividualComponentConnectionStatistics();

                        // open projects at startup if necessary, return whether any project has been opened
                        CheckCommandOpenProject();
                       
                        AddEditorDispatched(typeof(Startcenter.StartcenterEditor));                       

                        if (IsCommandParameterGiven("-silent"))
                        {
                            silent = true;
                            statusBarItem.Content = null;
                            dockWindowLogMessages.IsAutoHide = true;
                            dockWindowLogMessages.Visibility = Visibility.Collapsed;
                        }

                        startUpRunning = false;
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage(ex.Message, NotificationLevel.Error);
                    }
                }, null);

            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
        }

        private void CheckInitTypesException(IAsyncResult ar)
        {
            AsyncResult asyncResult = ar as AsyncResult;
            if (asyncResult == null)
            {
                return;
            }

            InitTypesDelegate exe = asyncResult.AsyncDelegate as InitTypesDelegate;
            if (exe == null)
            {
                return;
            }

            try
            {
                exe.EndInvoke(ar);
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }

        private Dictionary<string, Type> GetEditorExtension(List<Type> editorTypes)
        {
            Dictionary<string, Type> editorExtension = new Dictionary<string, Type>();
            foreach (Type type in editorTypes)
            {
                if (type.GetEditorInfoAttribute() != null)
                {
                    editorExtension.Add(type.GetEditorInfoAttribute().DefaultExtension, type);
                }
            }
            return editorExtension;
        }

        /// <summary>
        /// Find workspace parameter and if found, load workspace
        /// </summary>
        /// <returns>project file name or null if none</returns>
        private List<string> CheckCommandProjectFileGiven()
        {
            List<string> res = new List<string>();
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 1; i < args.Length; i++)
            {
                string currentParameter = args[i];
                if (currentParameter.StartsWith("-"))
                {
                    continue;
                }

                if (File.Exists(currentParameter))
                {
                    res.Add(currentParameter);
                }
            }

            return res;
        }

        /// <summary>
        /// Open projects at startup.
        /// </summary>
        /// <returns>true if at least one project has been opened</returns>
        private void CheckCommandOpenProject()
        {

            try
            {
                List<string> filesPath = CheckCommandProjectFileGiven();
                foreach (string filePath in filesPath)
                {
                    GuiLogMessage(string.Format(Resource.workspace_loading, filePath), NotificationLevel.Info);
                    try
                    {
                        OpenProject(filePath, FileLoadedOnStartup);
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage(ex.Message, NotificationLevel.Error);
                        if (ex.InnerException != null)
                        {
                            GuiLogMessage(ex.InnerException.Message, NotificationLevel.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
                if (ex.InnerException != null)
                {
                    GuiLogMessage(ex.InnerException.Message, NotificationLevel.Error);
                }
            }
        }


        private bool ReopenLastTabs(List<StoredTab> lastOpenedTabs)
        {
            bool hasOpenedProject = false;
            foreach (StoredTab lastOpenedTab in lastOpenedTabs)
            {
                if (lastOpenedTab is EditorFileStoredTab)
                {
                    hasOpenedProject = OpenLastEditorFileStoredTab(lastOpenedTab);
                }
                else if (lastOpenedTab is EditorTypeStoredTab)
                {
                    OpenLastEditorTypeStoredTab(lastOpenedTab);
                }
                else if (lastOpenedTab is CommonTypeStoredTab)
                {
                    OpenLastCommonTypeStoredTab(lastOpenedTab);
                }
            }
            return hasOpenedProject;
        }

        private bool OpenLastEditorFileStoredTab(StoredTab lastOpenedTab)
        {
            string file = ((EditorFileStoredTab)lastOpenedTab).Filename;
            if (File.Exists(file))
            {
                OpenProject(file, null);
                OpenTab(ActiveEditor, lastOpenedTab.Info, null);
                return true;
            }

            GuiLogMessage(string.Format(Properties.Resources.FileLost, file), NotificationLevel.Error);
            return false;
        }

        private void OpenLastEditorTypeStoredTab(StoredTab lastOpenedTab)
        {
            if (lastOpenedTab.Info.Title.Contains(CrypCloudManager.DefaultTabName))
            {
                return;
            }
            try
            {
                Type editorType = ((EditorTypeStoredTab)lastOpenedTab).EditorType;
                TabInfo info = new TabInfo();
                if (editorType == typeof(CrypCloud.Manager.CrypCloudManager))
                {
                    info.Title = CrypCloud.Manager.Properties.Resources.Tab_Caption;
                    info.Tooltip = new Span(new Run(CrypCloud.Manager.Properties.Resources.PluginTooltip));
                }
                else if (editorType == typeof(WorkspaceManager.WorkspaceManagerClass))
                {
                    //we dont open empty WorkspaceManagers
                    return;
                }
                else
                {
                    info.Title = editorType.GetPluginInfoAttribute().Caption;
                }
                IEditor editor = AddEditorDispatched(editorType);
                OpenTab(editor, info, null); //rename
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception occured during restoring of editor tab: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        private void OpenLastCommonTypeStoredTab(StoredTab lastOpenedTab)
        {
            object tabContent = null;
            TabInfo info = new TabInfo();

            Type type = ((CommonTypeStoredTab)lastOpenedTab).Type;

            if (type == typeof(OnlineHelpTab))
            {
                tabContent = OnlineHelpTab.GetSingleton(this);
                info.Title = Properties.Resources.Online_Help;
            }
            else if (type == typeof(SettingsPresentation))
            {
                tabContent = SettingsPresentation.GetSingleton();
                info.Title = Properties.Resources.Settings;
            }
            else if (type == typeof(UpdaterPresentation))
            {
                tabContent = UpdaterPresentation.GetSingleton();
                info.Title = Properties.Resources.CrypTool_2_0_Update;
            }
            else if (type == typeof(SystemInfos))
            {
                tabContent = systemInfos;
                info.Title = Properties.Resources.System_Infos;
            }
            else if (type == typeof(LicensesTab))
            {
                tabContent = licenses;
                info.Title = Properties.Resources.Licenses;
            }
            else if (typeof(ICrypTutorial).IsAssignableFrom(type))
            {
                ConstructorInfo constructorInfo = type.GetConstructor(new Type[0]);
                if (constructorInfo != null)
                {
                    tabContent = constructorInfo.Invoke(new object[0]);
                }

                info.Title = type.GetPluginInfoAttribute().Caption;
                info.Icon = type.GetImage(0).Source;
                info.Tooltip = new Span(new Run(type.GetPluginInfoAttribute().ToolTip));
            }
            else if (type != null)
            {
                try
                {
                    ConstructorInfo constructorInfo = type.GetConstructor(new Type[0]);
                    if (constructorInfo != null)
                    {
                        tabContent = constructorInfo.Invoke(new object[0]);
                    }
                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format(Properties.Resources.Couldnt_create_tab_of_Type, type.Name, ex.Message),
                        NotificationLevel.Error);
                }

                try
                {
                    info.Title = type.GetPluginInfoAttribute().Caption;
                }
                catch (Exception)
                {
                    info = lastOpenedTab.Info;
                }
            }

            if (tabContent != null && info != null)
            {
                OpenTab(tabContent, info, null);
            }
        }
        private void FileLoadedOnStartup(IEditor editor, string filename)
        {
            // Switch to "Play"-state, if parameter is given
            if (IsCommandParameterGiven("-autostart"))
            {
                PlayProject(editor);
            }
        }

        #endregion Init

        #region Editor

        private IEditor AddEditorDispatched(Type type)
        {
            if (type == null) // sanity check
            {
                return null;
            }

            EditorInfoAttribute editorInfo = type.GetEditorInfoAttribute();
            if (editorInfo != null && editorInfo.Singleton)
            {
                foreach (object e in contentToTabMap.Keys.Where(e => e.GetType() == type))
                {
                    ActiveEditor = (IEditor)e;
                    return (IEditor)e;
                }
            }

            IEditor editor = type.CreateEditorInstance();
            if (editor == null) // sanity check
            {
                return null;
            }

            if (editor.Presentation != null)
            {
                ToolTipService.SetIsEnabled(editor.Presentation, false);
            }
            editor.SamplesDir = defaultTemplatesDirectory;

            if (editor is Startcenter.StartcenterEditor)
            {
                ((Startcenter.Controls.StartcenterControl)((Startcenter.StartcenterEditor)editor).Presentation).TemplateLoaded += new EventHandler<Startcenter.Util.TemplateOpenEventArgs>(MainWindow_TemplateLoaded);
            }

            if (Dispatcher.CheckAccess())
            {
                AddEditor(editor);
            }
            else
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    AddEditor(editor);
                }, null);
            }
            return editor;
        }

        private void MainWindow_TemplateLoaded(object sender, TemplateOpenEventArgs e)
        {
            IEditor editor = OpenEditor(e.Type, e.Info);
            editor.Open(e.Info.Filename.FullName);
            OpenTab(editor, e.Info, null);           

            //update project title to template name:
            if (contentToTabMap.ContainsKey(ActivePlugin))
            {
                ProjectTitleChanged((string)contentToTabMap[ActivePlugin].Header);
            }
        }

        private void AddEditor(IEditor editor)
        {
            editor.PluginManager = pluginManager;

            TabControl tabs = (TabControl)(MainSplitPanel.Children[0]);
            foreach (TabItem tab in tabs.Items)
            {
                if (tab.Content == editor.Presentation)
                {
                    tabs.SelectedItem = tab;
                    return;
                }
            }

            editor.OnOpenTab += OpenTab;
            editor.OnOpenEditor += OpenEditor;
            editor.OnProjectTitleChanged += EditorProjectTitleChanged;

            Span tooltip = new Span();
            tooltip.Inlines.Add(editor.GetPluginInfoAttribute().ToolTip);
            OpenTab(editor, new TabInfo() { Title = editor.GetPluginInfoAttribute().Caption, Icon = editor.GetImage(0).Source, Tooltip = tooltip }, null);

            editor.Initialize();

            editor.New();
            editor.Presentation.Focusable = true;
            editor.Presentation.Focus();
        }


        private IEditor OpenEditor(Type editorType, TabInfo info)
        {
            IEditor editor = AddEditorDispatched(editorType);
            if (info == null)
            {
                info = new TabInfo();
            }

            if (info.Filename != null)
            {
                ProjectFileName = info.Filename.FullName;
            }

            if (info != null)
            {
                OpenTab(editor, info, null);
            }

            return editor;
        }

        private void EditorProjectTitleChanged(IEditor editor, string newprojecttitle)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                if (!contentToTabMap.ContainsKey(editor))
                {
                    return;
                }

                newprojecttitle = newprojecttitle.Replace("_", "__");
                contentToTabMap[editor].Header = newprojecttitle;
                if (editor == ActiveEditor)
                {
                    ProjectTitleChanged(newprojecttitle);
                }

                SaveSession();
            }, null);
        }

        /// <summary>
        /// Opens a tab with the given content.
        /// If content is of type IEditor, this method behaves a little bit different.
        /// 
        /// If a tab with the given content already exists, only the title of it is changed.
        /// </summary>
        /// <param name="content">The content to be shown in the tab</param>
        /// <param name="title">Title of the tab</param>
        private TabItem OpenTab(object content, TabInfo info, IEditor parent)
        {
            if (contentToTabMap.ContainsKey(content))
            {
                CTTabItem tab = contentToTabMap[content];
                tab.SetTabInfo(info);
                tab.IsSelected = true;
                SaveSession();
                return tab;
            }

            TabControl tabs = (TabControl)(MainSplitPanel.Children[0]);
            lastTab = (TabItem)tabs.SelectedItem;
            CTTabItem tabitem = new CTTabItem(info);
            tabitem.RequestDistractionFreeOnOffEvent += new EventHandler(tabitem_RequestDistractionFreeOnOffEvent);
            tabitem.RequestHideMenuOnOffEvent += new EventHandler(tabitem_RequestHideMenuOnOffEvent);
            tabitem.RequestBigViewFrame += handleMaximizeTab;

            IPlugin plugin = content as IPlugin;
            if (plugin != null)
            {
                plugin.OnGuiLogNotificationOccured += OnGuiLogNotificationOccured;
                tabitem.Content = plugin.Presentation;
            }
            else
            {
                tabitem.Content = content;
            }

            IEditor editor = content as IEditor;
            if (editor != null)
            {
                tabitem.Editor = editor;
                if (Settings.Default.FixedWorkspace)
                {
                    editor.ReadOnly = true;
                }
            }

            //give the tab header his individual color:
            Attribute colorAttr = Attribute.GetCustomAttribute(content.GetType(), typeof(TabColorAttribute));
            if (colorAttr != null)
            {
                SolidColorBrush brush = (SolidColorBrush)new BrushConverter().ConvertFromString(((TabColorAttribute)colorAttr).Brush);
                Color color = new Color() { A = 45, B = brush.Color.B, G = brush.Color.G, R = brush.Color.R };
                tabitem.Background = new SolidColorBrush(color);
            }

            tabitem.OnClose += delegate
            {
                CloseTab(content, tabs, tabitem);
            };

            tabs.Items.Add(tabitem);

            tabToContentMap.Add(tabitem, content);
            contentToTabMap.Add(content, tabitem);
            if (parent != null)
            {
                contentToParentMap.Add(content, parent);
            }          

            tabs.SelectedItem = tabitem;

            SaveSession();
            return tabitem;
        }

        private void tabitem_RequestHideMenuOnOffEvent(object sender, EventArgs e)
        {
            AppRibbon.IsMinimized = !AppRibbon.IsMinimized;
        }

        private void tabitem_RequestDistractionFreeOnOffEvent(object sender, EventArgs e)
        {
            doHandleMaxTab();
        }

        private void CloseTab(object content, TabControl tabs, CTTabItem tabitem)
        {
            if (Settings.Default.FixedWorkspace)
            {
                return;
            }

            IEditor editor = content as IEditor;

            if (editor != null && SaveChangesIfNecessary(editor) == FileOperationResult.Abort)
            {
                return;
            }

            if (editor != null && contentToParentMap.ContainsValue(editor))
            {
                object[] children = contentToParentMap.Keys.Where(x => contentToParentMap[x] == editor).ToArray();
                foreach (object child in children)
                {
                    CloseTab(child, tabs, contentToTabMap[child]);
                }
            }

            tabs.Items.Remove(tabitem);
            tabToContentMap.Remove(tabitem);
            contentToTabMap.Remove(content);
            contentToParentMap.Remove(content);
            if (tabitem is CTTabItem)
            {
                tabitem.RequestDistractionFreeOnOffEvent -= tabitem_RequestDistractionFreeOnOffEvent;
                tabitem.RequestHideMenuOnOffEvent -= tabitem_RequestHideMenuOnOffEvent;
            }


            tabitem.Content = null;

            if (editor != null)
            {
                editorToFileMap.Remove(editor);
                if (editor.CanStop)
                {
                    StopProjectExecution(editor);
                }
                editor.OnOpenTab -= OpenTab;
                editor.OnOpenEditor -= OpenEditor;
                editor.OnProjectTitleChanged -= EditorProjectTitleChanged;

                SaveSession();
            }

            IPlugin plugin = content as IPlugin;
            if (plugin != null)
            {
                plugin.Dispose();
            }

            //Jump back to last tab:
            if (lastTab != null && lastTab != tabitem)
            {
                lastTab.IsSelected = true;
            }

            //Open Startcenter if tabcontrol is empty now:
            if (tabs.Items.Count == 0)
            {
                AddEditorDispatched(typeof(Startcenter.StartcenterEditor));
            }
        }

        private void SaveSession()
        {
            List<StoredTab> session = new List<StoredTab>();
            foreach (KeyValuePair<CTTabItem, object> c in tabToContentMap.Where(x => Attribute.GetCustomAttribute(x.Value.GetType(), typeof(NotStoredInSessionAttribute)) == null))
            {
                if (c.Value is IEditor && !string.IsNullOrEmpty(((IEditor)c.Value).CurrentFile))
                {
                    session.Add(new EditorFileStoredTab(c.Key.Info, ((IEditor)c.Value).CurrentFile));
                }
                else if (c.Value is IEditor)
                {
                    session.Add(new EditorTypeStoredTab(c.Key.Info, c.Value.GetType()));
                }
                else
                {
                    session.Add(new CommonTypeStoredTab(c.Key.Info, c.Value.GetType()));
                }
            }
            Settings.Default.LastOpenedTabs = session;
        }

        private void SetRibbonControlEnabled(bool enabled)
        {
            ribbonMainControls.IsEnabled = enabled;
            ribbonEditorProcess.IsEnabled = enabled;
            ribbonBarEditor.IsEnabled = enabled;
        }

        public void SetRibbonControlEnabledInGuiThread(bool enabled)
        {
            if (Dispatcher.CheckAccess())
            {
                SetRibbonControlEnabled(enabled);
            }
            else
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    SetRibbonControlEnabled(enabled);
                }, null);
            }
        }

        private void ProjectTitleChanged(string newProjectTitle = null)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                string windowTitle = AssemblyHelper.ProductName;
                if (!string.IsNullOrEmpty(newProjectTitle))
                {
                    windowTitle += " – " + newProjectTitle; // append project name if not null or empty
                }

                Title = windowTitle;
            }, null);
        }

        private Type GetDefaultEditor()
        {
            return GetEditor(Settings.Default.defaultEditor);
        }

        private Type GetEditor(string name)
        {
            foreach (Type editor in loadedTypes[typeof(IEditor).FullName])
            {
                if (editor.FullName == name)
                {
                    return editor;
                }
            }
            return null;
        }

        private void SetCurrentEditorAsDefaultEditor()
        {
            Settings.Default.defaultEditor = ActiveEditor.GetType().FullName;
        }
        #endregion Editor

        #region DragDrop, NaviPaneMethods

        private void navPaneItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PaneItem pi = sender as PaneItem;
            if (pi != null)
            {
                naviPane.SystemText.CollapsedPaneText = pi.Title;
            }
        }

        private void navItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem listBoxItem = sender as ListBoxItem;
            if (listBoxItem == null)
            {
                GuiLogMessage("Not a valid menu entry.", NotificationLevel.Error);
                return;
            }

            Type type = listBoxItem.Tag as Type;
            if (type == null)
            {
                GuiLogMessage("Not a valid menu entry.", NotificationLevel.Error);
                return;
            }

            try
            {
                if (ActiveEditor != null)
                {
                    ActiveEditor.Add(type);
                }
                else
                {
                    GuiLogMessage("Adding plugin to active workspace not possible!", NotificationLevel.Error);
                }
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }

        private void navItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            dragStarted = true;
            base.OnPreviewMouseDown(e);
        }

        private void navItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (dragStarted)
            {
                dragStarted = false;

                //create data object 
                ButtonDropDown button = sender as ButtonDropDown;
                ListBoxItem listBoxItem = sender as ListBoxItem;
                Type type = null;
                if (button != null)
                {
                    type = button.Tag as Type;
                }

                if (listBoxItem != null)
                {
                    type = listBoxItem.Tag as Type;
                }

                if (type != null)
                {
                    DataObject data = new DataObject(new DragDropDataObject(type.Assembly.FullName, type.FullName, null));
                    //trap mouse events for the list, and perform drag/drop 
                    Mouse.Capture(sender as UIElement);
                    if (button != null)
                    {
                        System.Windows.DragDrop.DoDragDrop(button, data, DragDropEffects.Copy);
                    }
                    else
                    {
                        System.Windows.DragDrop.DoDragDrop(listBoxItem, data, DragDropEffects.Copy);
                    }

                    Mouse.Capture(null);
                }
            }
            dragStarted = false;
            base.OnPreviewMouseMove(e);
        }

        private void naviPane_Collapsed(object sender, RoutedEventArgs e)
        {
            naviPane.IsExpanded = true;
        }
        #endregion OnPluginClicked, DragDrop, NaviPaneMethods

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (RunningWorkspaces() == 0 && ShowInTaskbar && !closedByMenu && !restart && !shutdown && Settings.Default.RunInBackground)
            {
                oldWindowState = WindowState;
                closingCausedMinimization = true;
                WindowState = System.Windows.WindowState.Minimized;
                e.Cancel = true;
            }
            else
            {
                if (RunningWorkspaces() > 0 && !restart && !shutdown)
                {
                    MessageBoxResult res;
                    if (RunningWorkspaces() == 1)
                    {
                        res = MessageBox.Show(Properties.Resources.There_is_still_one_running_task, Properties.Resources.Warning, MessageBoxButton.YesNo);
                    }
                    else
                    {
                        res = MessageBox.Show(Properties.Resources.There_are_still_running_tasks__templates_in_Play_mode___Do_you_really_want_to_exit_CrypTool_2__, Properties.Resources.Warning, MessageBoxButton.YesNo);
                    }
                    if (res == MessageBoxResult.Yes)
                    {
                        ClosingRoutine(e);
                    }
                    else
                    {
                        e.Cancel = true;
                    }
                }
                else
                {
                    ClosingRoutine(e);
                }

                closedByMenu = false;
            }
        }

        private void ClosingRoutine(CancelEventArgs e)
        {
            try
            {
                SaveSession();
                SaveComponentConnectionStatistics();

                if (demoController != null)
                {
                    demoController.Stop();
                }

                if (_singletonMutex != null)
                {
                    _singletonMutex.ReleaseMutex();
                }

                if (!CloseProjects())  // Editor Dispose will be called here.
                {
                    e.Cancel = true;
                    WindowState = oldWindowState;
                }
                else
                {
                    if (WindowState == WindowState.Maximized)
                    {
                        Settings.Default.IsWindowMaximized = true;
                        Settings.Default.RelHeight = 0.9;
                        Settings.Default.RelWidth = 0.9;
                    }
                    else
                    {
                        Settings.Default.IsWindowMaximized = false;
                        Settings.Default.RelHeight = Height / System.Windows.SystemParameters.PrimaryScreenHeight;
                        Settings.Default.RelWidth = Width / System.Windows.SystemParameters.PrimaryScreenWidth;
                    }
                    Settings.Default.logWindowAutoHide = dockWindowLogMessages.IsAutoHide;

                    SaveSettingsSavely();

                    notifyIcon.Visible = false;
                    notifyIcon.Dispose();

                    SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
                    SystemEvents.SessionEnding -= SystemEvents_SessionEnding;

                    if (restart)
                    {
                        OnUpdate();
                    }

                    try
                    {
                        //Log out of the CrypCloud. Thus, a GoingOfflineMessage is sent to all connected peers
                        //telling them, that we are offline
                        //If the CrypCloud is not "logged in" the method just returns
                        CrypCloudCore.Instance.Logout();
                    }
                    catch (Exception)
                    {
                        //do nothing
                    }

                    CloseLogFile();

                    Application.Current.Shutdown();
                }
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }

        private void SaveComponentConnectionStatistics(bool storeInBaseDir = false)
        {
            string directory = storeInBaseDir ? DirectoryHelper.BaseDirectory : DirectoryHelper.DirectoryLocal;
            ComponentConnectionStatistics.SaveCurrentStatistics(Path.Combine(directory, "ccs.xml"));
        }

        private int RunningWorkspaces()
        {
            return editorToFileMap.Keys.Where(e => e.CanStop).Count();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            if (ActiveEditor == lastEditor)
            {
                return;
            }

            if (lastEditor != null)
            {
                lastEditor.OnOpenProjectFile -= OpenProjectFileEvent;
            }

            if (ActiveEditor != null && ActivePlugin == ActiveEditor)
            {
                ActiveEditor.OnOpenProjectFile += OpenProjectFileEvent;
            }
            ShowEditorSpecificPanels(ActiveEditor);

            if (ActivePlugin != null)
            {
                if (contentToTabMap.ContainsKey(ActivePlugin))
                {
                    ProjectTitleChanged((string)contentToTabMap[ActivePlugin].Header);
                }
            }
            else
            {
                ProjectTitleChanged();
            }

            lastEditor = ActiveEditor;
            RecentFileListChanged();
        }

        private void ShowEditorSpecificPanels(IEditor editor)
        {
            try
            {
                EditorTypePanelManager.EditorTypePanelProperties panelProperties = (editor == null) ? defaultPanelProperties : editorTypePanelManager.GetEditorTypePanelProperties(editor.GetType());

                LogBTN.IsChecked = panelProperties.ShowLogPanel;
                LogBTN_Checked(LogBTN, null);
                PluginBTN.IsChecked = panelProperties.ShowComponentPanel;
                PluginBTN_Checked(PluginBTN, null);
                SettingBTN.IsChecked = panelProperties.ShowSettingsPanel;
                SettingBTN_Checked(SettingBTN, null);

                if (ActiveEditor is WorkspaceManager.WorkspaceManagerClass)
                {
                    WorkspaceManager.View.Visuals.EditorVisual presentation = (WorkspaceManager.View.Visuals.EditorVisual)((WorkspaceManager.WorkspaceManagerClass)ActiveEditor).Presentation;
                }
            }
            catch (Exception)
            {
                //When editor has no specific settings (or editor parameter is null), just show all panels:
                MinimizeTab();
            }
        }

        private void RecentFileListChanged(List<string> recentFiles)
        {
            buttonDropDownOpen.Items.Clear();

            for (int c = recentFiles.Count - 1; c >= 0; c--)
            {
                string file = recentFiles[c];
                ButtonDropDown btn = new ButtonDropDown
                {
                    Header = file,
                    ToolTip = Properties.Resources.Load_this_file_,
                    IsChecked = (ProjectFileName == file)
                };
                btn.Click += delegate (object sender, RoutedEventArgs e)
                {
                    OpenProject(file, null);
                };

                buttonDropDownOpen.Items.Add(btn);
            }
        }

        private void RecentFileListChanged()
        {
            try
            {
                RecentFileListChanged(recentFileList.GetRecentFiles());
                GuiLogMessage("Called successfully RecentFileListChanged", NotificationLevel.Debug);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception occured in RecentFileListChanged: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        private void PluginSearchInputChanged(object sender, TextChangedEventArgs e)
        {
            if (PluginSearchTextBox.Text == "")
            {
                if (navPaneItemSearch.IsSelected)
                {
                    navPaneItemClassic.IsSelected = true;
                }

                navPaneItemSearch.Visibility = Visibility.Collapsed;
            }
            else
            {
                navPaneItemSearch.Visibility = Visibility.Visible;
                navPaneItemSearch.IsSelected = true;

                foreach (ListBoxItem items in navListBoxSearch.Items)
                {
                    Panel panel = (System.Windows.Controls.Panel)items.Content;
                    TextBlock textBlock = (TextBlock)panel.Children[1];
                    string text = (string)textBlock.Tag;
                    string engText = null;
                    if (panel.Tag != null)
                    {
                        engText = (string)panel.Tag;
                    }

                    bool hit = text.ToLower().Contains(PluginSearchTextBox.Text.ToLower());

                    if (!hit && (engText != null))
                    {
                        bool engHit = (engText.ToLower().Contains(PluginSearchTextBox.Text.ToLower()));
                        if (engHit)
                        {
                            hit = true;
                            text = text + " (" + engText + ")";
                        }
                    }

                    Visibility visibility = hit ? Visibility.Visible : Visibility.Collapsed;
                    items.Visibility = visibility;

                    if (hit)
                    {
                        textBlock.Inlines.Clear();
                        int begin = 0;
                        int end = text.IndexOf(PluginSearchTextBox.Text, begin, StringComparison.OrdinalIgnoreCase);
                        while (end != -1)
                        {
                            textBlock.Inlines.Add(text.Substring(begin, end - begin));
                            textBlock.Inlines.Add(new Bold(new Italic(new Run(text.Substring(end, PluginSearchTextBox.Text.Length)))));
                            begin = end + PluginSearchTextBox.Text.Length;
                            end = text.IndexOf(PluginSearchTextBox.Text, begin, StringComparison.OrdinalIgnoreCase);
                        }
                        textBlock.Inlines.Add(text.Substring(begin, text.Length - begin));
                    }
                }
            }
        }

        private void PluginSearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                navPaneItemSearch.Visibility = Visibility.Collapsed;
                PluginSearchTextBox.Text = "";
            }
        }

        private void buttonSysInfo_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            OpenTab(systemInfos, new TabInfo() { Title = Properties.Resources.System_Infos }, null);
        }

        private void buttonContactUs_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ContactDevelopersDialog.ShowModalDialog();
        }

        private void buttonWebsite_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            System.Diagnostics.Process.Start(Properties.Resources.Homepage);
        }

        private void buttonLicenses_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            OpenTab(licenses, new TabInfo() { Title = Properties.Resources.Licenses }, null);

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = (ComboBox)sender;
            if (box.SelectedIndex < 0)
            {
                return;
            }

            MainTab.SelectedItem = box.Items[box.SelectedIndex];
        }

        private void SettingBTN_Checked(object sender, RoutedEventArgs e)
        {
            Visibility v = ((ButtonDropDown)sender).IsChecked ? Visibility.Visible : Visibility.Collapsed;
            Properties.Settings.Default.SettingVisibility = v.ToString();
            SaveSettingsSavely();
            dockWindowAlgorithmSettings.Close();
        }

        private void LogBTN_Checked(object sender, RoutedEventArgs e)
        {
            ActivePanelProperties.ShowLogPanel = ((ButtonDropDown)sender).IsChecked;

            Visibility v = ((ButtonDropDown)sender).IsChecked ? Visibility.Visible : Visibility.Collapsed;
            Properties.Settings.Default.LogVisibility = v.ToString();
            SaveSettingsSavely();

            if (v == Visibility.Visible)
            {
                dockWindowLogMessages.Open();
            }
            else
            {
                dockWindowLogMessages.Close();
            }
        }

        private void PluginBTN_Checked(object sender, RoutedEventArgs e)
        {
            Visibility v = ((ButtonDropDown)sender).IsChecked ? Visibility.Visible : Visibility.Collapsed;
            Properties.Settings.Default.PluginVisibility = v.ToString();
            SaveSettingsSavely();

            if (v == Visibility.Visible)
            {
                dockWindowNaviPaneAlgorithms.Open();
            }
            else
            {
                dockWindowNaviPaneAlgorithms.Close();
            }
        }

        private void statusBar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            LogBTN.IsChecked = !LogBTN.IsChecked;
            LogBTN_Checked(LogBTN, null);
        }

        private void doHandleMaxTab()
        {
            {
                EditorTypePanelManager.EditorTypePanelProperties prop = (ActiveEditor == null) ? defaultPanelProperties : editorTypePanelManager.GetEditorTypePanelProperties(ActiveEditor.GetType());
                if (ActiveEditor is WorkspaceManager.WorkspaceManagerClass)
                {
                    prop.ShowSettingsPanel = ((WorkspaceManager.View.Visuals.EditorVisual)((WorkspaceManager.WorkspaceManagerClass)ActiveEditor).Presentation).IsSettingsOpen;
                }

                if (prop.IsMaximized)
                {
                    prop.Minimize();
                }
                else
                {
                    prop.Maximize();
                }

                ShowEditorSpecificPanels(ActiveEditor);
            }
        }

        private void handleMaximizeTab(object sender, EventArgs e)
        {
            doHandleMaxTab();
        }

        private void MinimizeTab()
        {
            LogBTN.IsChecked = true;
            SettingBTN.IsChecked = false;
            PluginBTN.IsChecked = false;

            LogBTN_Checked(LogBTN, null);
            SettingBTN_Checked(SettingBTN, null);
            PluginBTN_Checked(PluginBTN, null);
        }

        private void ShowHelpPage(object docEntity)
        {
            OnlineHelpTab onlineHelpTab = OnlineHelpTab.GetSingleton(this);
            onlineHelpTab.OnOpenEditor += OpenEditor;
            onlineHelpTab.OnOpenTab += OpenTab;

            //Find out which page to show:
            string lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            if (docEntity is Type)
            {
                if (!ShowPluginHelpPage((Type)docEntity, onlineHelpTab, lang))
                {
                    return;
                }
            }
            else if (docEntity is OnlineHelp.TemplateType)
            {
                string rel = ((OnlineHelp.TemplateType)docEntity).RelativeTemplateFilePath;
                try
                {
                    onlineHelpTab.ShowHTMLFile(OnlineHelp.GetTemplateDocFilename(rel, lang));
                }
                catch (Exception)
                {
                    //Try opening index page in english:
                    try
                    {
                        onlineHelpTab.ShowHTMLFile(OnlineHelp.GetTemplateDocFilename(rel, "en"));
                    }
                    catch (Exception)
                    {
                        return;
                    }
                }
            }
            else
            {
                ShowPluginHelpPage(null, onlineHelpTab, lang);
            }

            //show tab:
            TabItem tab = OpenTab(onlineHelpTab, new TabInfo() { Title = Properties.Resources.Online_Help }, null);
            if (tab != null)
            {
                tab.IsSelected = true;
            }
        }

        private bool ShowPluginHelpPage(Type docType, OnlineHelpTab onlineHelpTab, string lang)
        {
            try
            {
                if ((docType == typeof(MainWindow)) || (docType == null)) //The doc page of MainWindow is the index page.
                {
                    try
                    {
                        onlineHelpTab.ShowHTMLFile(OnlineHelp.GetComponentIndexFilename(lang));
                    }
                    catch (Exception)
                    {
                        //Try opening index page in english:
                        onlineHelpTab.ShowHTMLFile(OnlineHelp.GetComponentIndexFilename("en"));
                    }
                }
                else if (docType.GetPluginInfoAttribute() != null)
                {
                    OnlineDocumentationGenerator.DocInformations.PluginDocumentationPage pdp = OnlineDocumentationGenerator.DocGenerator.CreatePluginDocumentationPage(docType);
                    if (pdp.AvailableLanguages.Contains(lang))
                    {
                        onlineHelpTab.ShowHTMLFile(OnlineHelp.GetPluginDocFilename(docType, lang));
                    }
                    else
                    {
                        onlineHelpTab.ShowHTMLFile(OnlineHelp.GetPluginDocFilename(docType, "en"));
                    }
                }
                else
                {
                    throw new FileNotFoundException();
                }
            }
            catch (FileNotFoundException)
            {
                //if file was not found, simply try to open the index page:
                GuiLogMessage(string.Format(Properties.Resources.MainWindow_ShowHelpPage_No_special_help_file_found_for__0__, docType),
                    NotificationLevel.Warning);
                if (docType != typeof(MainWindow))
                {
                    ShowHelpPage(typeof(MainWindow));
                }
                return false;
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format(Resource.MainWindow_ShowHelpPage_Error_trying_to_open_documentation___0__, ex.Message),
                    NotificationLevel.Error);
                return false;
            }
            return true;
        }

        private void addimg_Click(object sender, RoutedEventArgs e)
        {
            ActiveEditor.AddImage();
        }

        private void addtxt_Click(object sender, RoutedEventArgs e)
        {
            ActiveEditor.AddText();
        }

        private void dockWindowLogMessages_Closed(object sender, RoutedEventArgs e)
        {
            LogBTN.IsChecked = false;
            LogBTN_Checked(LogBTN, null);
        }

        private void dockWindowAlgorithmSettings_Closed(object sender, RoutedEventArgs e)
        {
            SettingBTN.IsChecked = false;
            SettingBTN_Checked(SettingBTN, null);
        }

        private void dockWindowNaviPaneAlgorithms_Closed(object sender, RoutedEventArgs e)
        {
            PluginBTN.IsChecked = false;
            PluginBTN_Checked(PluginBTN, null);
        }

        private void navListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            MouseWheelEventArgs e2 = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent
            };
            ((ListBox)sender).RaiseEvent(e2);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in listViewLogList.SelectedItems)
            {
                sb.AppendLine(item.ToString());
            }
            Clipboard.SetText(sb.ToString());
        }

        private void clearManagement()
        {
            ManagementRootEdit.Children.Clear();
            ManagementRootTutorials.Children.Clear();
            ManagementRootMain.Children.Clear();
        }

        private void AppRibbon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ribbonTabHome.IsSelected)
            {
                clearManagement();
                ManagementRootMain.Children.Add(ribbonManagement);
                return;
            }

            if (ribbonTabEdit.IsSelected)
            {
                clearManagement();
                ManagementRootEdit.Children.Add(ribbonManagement);
                return;
            }

            if (ribbonTabView.IsSelected)
            {
                clearManagement();
                ManagementRootTutorials.Children.Add(ribbonManagement);
                return;
            }
        }
    }

    #region helper class

    public class VisibilityToMarginHelper : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility vis = (Visibility)value;
            if (vis == Visibility.Collapsed)
            {
                return new Thickness(0, 2, 0, 0);
            }
            else
            {
                return new Thickness(15, 2, 15, 0);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Helper class with GUI elements containing the plugins after init.
    /// </summary>
    /// 
    public class GUIContainerElementsForPlugins
    {
        #region shared
        public readonly Type Plugin;
        public readonly PluginInfoAttribute PluginInfo;
        #endregion shared

        #region naviPane
        public readonly PaneItem PaneItem;
        public readonly ListBox ListBox;
        #endregion naviPane

        #region ribbon
        public readonly string GroupName;
        #endregion ribbon

        public GUIContainerElementsForPlugins(Type plugin, PluginInfoAttribute pluginInfo, PaneItem paneItem, ListBox listBox, string groupName)
        {
            Plugin = plugin;
            PluginInfo = pluginInfo;
            PaneItem = paneItem;
            ListBox = listBox;
            GroupName = groupName;
        }
    }
    #endregion helper class
}
