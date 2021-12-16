/*
   Copyright 2019 Nils Kopal <kopal<AT>CrypTool.de>

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
using CrypTool.Core;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Attributes;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using CrypToolStoreLib.Client;
using CrypToolStoreLib.DataObjects;
using CrypToolStoreLib.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml;

namespace CrypTool.CrypToolStore
{
    /// <summary>
    /// Interaktionslogik für CrypToolStorePresentation.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("CrypTool.CrypToolStore.Properties.Resources")]
    public partial class CrypToolStorePresentation : UserControl, INotifyPropertyChanged
    {
        //reference totthe editor
        private readonly CrypToolStoreEditor _CrypToolStoreEditor;
        //list of all plugins available in the store
        private ObservableCollection<PluginWrapper> Plugins { get; set; }
        //Current selected plugin in the store
        private PluginWrapper SelectedPlugin { get; set; }
        //Pending changes means, something has been installed or uninstalled
        //thus, CrypTool 2 needs to be restarted to have changes take any effect
        public static bool PendingChanges { get; set; }
        //The reference to the window is needed for modal message boxes
        private readonly Window _windowToBlockForMessageBoxes;

        private readonly SynchronizationContext _context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="CrypToolStoreEditor"></param>
        public CrypToolStorePresentation(CrypToolStoreEditor CrypToolStoreEditor)
        {
            InitializeComponent();
            _CrypToolStoreEditor = CrypToolStoreEditor;
            Plugins = new ObservableCollection<PluginWrapper>();
            PluginListView.ItemsSource = Plugins;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(PluginListView.ItemsSource);
            view.Filter = UserFilter;
            view.SortDescriptions.Add(new SortDescription("UpdateAvailable", ListSortDirection.Descending));
            view.SortDescriptions.Add(new SortDescription("IsInstalled", ListSortDirection.Descending));
            view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            DataContext = this;
            _windowToBlockForMessageBoxes = Application.Current.MainWindow;
            _context = SynchronizationContext.Current;
        }

        /// <summary>
        /// Filters the plugin list
        /// Only shows plugins that have the search text (given by user in search field) in
        /// Name, ShortDescription, LongDescription, Authornames, or Authorinstitutes
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool UserFilter(object item)
        {
            if (string.IsNullOrEmpty(Filter.Text))
            {
                return true;
            }
            else
            {
                PluginWrapper plugin = (PluginWrapper)item;
                string searchtext = plugin.Name +
                                    plugin.ShortDescription +
                                    plugin.LongDescription +
                                    plugin.Authornames +
                                    plugin.Authorinstitutes;
                return (searchtext.IndexOf(Filter.Text, StringComparison.OrdinalIgnoreCase) >= 0);
            }
        }

        /// <summary>
        /// Called, when ui has been loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Task updateStorePluginListTask = new Task(UpdateStorePluginList);
            updateStorePluginListTask.Start();
        }

        /// <summary>
        /// Starts a thread to retrieve the newest list of plugins from the CrypToolStoreServer
        /// </summary>
        private void UpdateStorePluginList()
        {
            try
            {
                CrypToolStoreClient client = new CrypToolStoreClient
                {
                    ServerCertificate = new X509Certificate2(Properties.Resources.CTStoreTLS),
                    ServerAddress = CrypTool.CrypToolStore.Constants.ServerAddress,
                    ServerPort = CrypTool.CrypToolStore.Constants.ServerPort
                };

                //Translate the Ct2BuildType to PublishState
                PublishState publishState;
                switch (AssemblyHelper.BuildType)
                {
                    case Ct2BuildType.Developer:
                        publishState = PublishState.DEVELOPER;
                        break;
                    case Ct2BuildType.Nightly:
                        publishState = PublishState.NIGHTLY;
                        break;
                    case Ct2BuildType.Beta:
                        publishState = PublishState.BETA;
                        break;
                    case Ct2BuildType.Stable:
                        publishState = PublishState.RELEASE;
                        break;
                    default: //if no known version is given, we assume release
                        publishState = PublishState.RELEASE;
                        break;
                }

                //Connect to CrypToolStoreServer
                client.Connect();

                //get list of published plugins and resources
                DataModificationOrRequestResult result_plugins = client.GetPublishedPluginList(publishState);
                DataModificationOrRequestResult result_resources = client.GetPublishedResourceList(publishState);

                //Disconnect from CrypToolStoreServer
                client.Disconnect();

                //Display result or in case of error an error message
                if (result_plugins.Success && result_resources.Success)
                {
                    List<PluginAndSource> pluginsAndSources = ((List<PluginAndSource>)result_plugins.DataObject);
                    List<ResourceAndResourceData> resourcesAndData = ((List<ResourceAndResourceData>)result_resources.DataObject);

                    //add elements to observed list to show them in the UI
                    _context.Send(callback =>
                    {
                        try
                        {
                            Plugins.Clear();
                            foreach (PluginAndSource pluginAndSource in pluginsAndSources)
                            {
                                PluginWrapper wrapper = new PluginWrapper(pluginAndSource);
                                CheckIfAlreadyInstalledPlugin(wrapper);
                                Plugins.Add(wrapper);
                            }

                            if (ShowResources.IsChecked == true)
                            {
                                foreach (ResourceAndResourceData pluginAndSource in resourcesAndData)
                                {
                                    ResourceWrapper wrapper = new ResourceWrapper(pluginAndSource);
                                    CheckIfAlreadyInstalledResource(wrapper);
                                    Plugins.Add(wrapper);
                                }
                            }

                            //Search for old selected plugin and select it
                            if (SelectedPlugin != null)
                            {
                                int counter = 0;
                                foreach (PluginWrapper wrapper in PluginListView.Items)
                                {
                                    if (wrapper.PluginId == SelectedPlugin.PluginId)
                                    {
                                        PluginListView.SelectedIndex = counter;
                                        break;
                                    }
                                    counter++;
                                }
                            }
                            else
                            {
                                //otherwise, select the first plugin in list
                                PluginListView.SelectedIndex = 0;
                            }
                        }
                        catch (Exception ex)
                        {
                            _CrypToolStoreEditor.GuiLogMessage(string.Format(Properties.Resources.CrypToolStorePresentation_UpdateStorePluginList_Exception_occured_during_adding_of_current_list_of_plugins_to_the_user_interface___0_, ex.Message), NotificationLevel.Error);
                        }
                    }, null);
                }
                else
                {
                    _CrypToolStoreEditor.GuiLogMessage(string.Format(Properties.Resources.CrypToolStorePresentation_UpdateStorePluginList_Error_occured_during_retrieval_of_current_list_of_plugins_from_CrypToolStore___0_, result_plugins.Message), NotificationLevel.Error);
                }
            }
            catch (Exception ex)
            {
                _CrypToolStoreEditor.GuiLogMessage(string.Format(Properties.Resources.CrypToolStorePresentation_UpdateStorePluginList_Exception_occured_during_retrieval_of_current_list_of_plugins_from_CrypToolStore___0_, ex.Message), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Called when text of search field has been changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(PluginListView.ItemsSource).Refresh();
        }

        /// <summary>
        /// Selection in the PluginListView changed, i.e., the user selected a plugin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PluginListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                //the user selected a new plugin in the PluginListView
                PluginWrapper plugin = (PluginWrapper)e.AddedItems[0];
                SelectedPlugin = plugin;

                //Show selected plugin in the right box of the CrypToolStore UI
                _context.Send(callback =>
                {
                    try
                    {
                        SelectedPluginName.Content = plugin.Name;
                        SelectedPluginShortDescription.Text = plugin.ShortDescription;
                        SelectedPluginLongDescription.Text = plugin.LongDescription;
                        SelectedPluginIcon.Source = plugin.Icon;
                        SelectedPluginAuthorsName.Content = plugin.Authornames;
                        SelectedPluginAuthorsEmail.Content = plugin.Authoremails;
                        SelectedPluginAuthorsInstitutes.Content = plugin.Authorinstitutes;
                        SelectedPluginVersion.Content = plugin.PluginVersion + "." + plugin.BuildVersion + " (" + plugin.BuildDate + ")";
                        SelectedPluginFileSize.Content = plugin.FileSize;
                        if (SelectedPlugin.IsInstalled)
                        {
                            InstallButton.IsEnabled = false;
                            DeleteButton.IsEnabled = true;
                        }
                        else
                        {
                            InstallButton.IsEnabled = true;
                            DeleteButton.IsEnabled = false;
                        }
                        if (SelectedPlugin.UpdateAvailable)
                        {
                            UpdateButton.IsEnabled = true;
                        }
                        else
                        {
                            UpdateButton.IsEnabled = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        _CrypToolStoreEditor.GuiLogMessage(string.Format(Properties.Resources.CrypToolStorePresentation_PluginListView_SelectionChanged_Exception_occured_during_showing_of_selected_plugin_in_the_right_box_of_the_CrypToolStore_UI___0_, ex.Message), NotificationLevel.Error);
                    }
                }, null);
            }
        }

        /// <summary>
        /// Checks, if a plugin is already installed
        /// </summary>
        /// <param name="plugin"></param>
        private void CheckIfAlreadyInstalledPlugin(PluginWrapper plugin)
        {
            string xmlfilename = System.IO.Path.Combine(GetPluginsFolder(), string.Format("install-{0}-{1}.xml", plugin.PluginId, plugin.PluginVersion));
            if (Directory.Exists(GetPluginFolder(plugin)) || File.Exists(xmlfilename))
            {
                try
                {
                    string metaxmlfilename = System.IO.Path.Combine(GetPluginFolder(plugin), "pluginmetainfo.xml");
                    if (File.Exists(metaxmlfilename))
                    {
                        XmlDocument xml = new XmlDocument();
                        xml.Load(metaxmlfilename);

                        XmlNode rootNode = xml.SelectSingleNode("xml");
                        XmlNode pluginNode = rootNode.SelectSingleNode("Plugin");
                        XmlNode versionNode = pluginNode.SelectSingleNode("Version");
                        XmlNode buildVersionNode = pluginNode.SelectSingleNode("BuildVersion");

                        int pluginversion = int.Parse(versionNode.InnerText);
                        int buildversion = int.Parse(buildVersionNode.InnerText);

                        if (plugin.PluginVersion != pluginversion || plugin.BuildVersion != buildversion)
                        {
                            plugin.UpdateAvailable = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _CrypToolStoreEditor.GuiLogMessage(string.Format(Properties.Resources.CrypToolStorePresentation_CheckIfAlreadyInstalled_Exception_occured_during_check_of__0__for_updates___1_, plugin.Name, ex.Message), NotificationLevel.Error);
                }
                plugin.IsInstalled = true;
            }
            else
            {
                plugin.IsInstalled = false;
            }
        }

        /// <summary>
        /// Checks, if a resource is already installed
        /// </summary>
        /// <param name="resource"></param>
        private void CheckIfAlreadyInstalledResource(ResourceWrapper resource)
        {
            if (Directory.Exists(GetResourceFolder(resource)))
            {
                resource.IsInstalled = true;
            }
            else
            {
                resource.IsInstalled = false;
            }
        }

        /// <summary>
        /// Returns the absolute path to the plugin folder
        /// </summary>
        /// <param name="plugin"></param>
        /// <returns></returns>
        private string GetPluginFolder(PluginWrapper plugin)
        {
            return System.IO.Path.Combine(GetPluginsFolder(), "plugin-" + plugin.PluginId);
        }

        /// <summary>
        /// Returns the absolute path to the resource folder
        /// </summary>
        /// <param name="plugin"></param>
        /// <returns></returns>
        private string GetResourceFolder(ResourceWrapper resource)
        {
            return System.IO.Path.Combine(GetResourcesFolder(), "resource-" + resource.PluginId + "-" + resource.PluginVersion);
        }

        /// <summary>
        /// Returns the absolute path to the plugins folder
        /// </summary>
        private string GetPluginsFolder()
        {
            //Translate the Ct2BuildType to a folder name for CrypToolStore plugins                
            string CrypToolStoreSubFolder;
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
            string CrypToolStorePluginFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), PluginManager.CrypToolStoreDirectory);
            CrypToolStorePluginFolder = System.IO.Path.Combine(CrypToolStorePluginFolder, CrypToolStoreSubFolder);
            CrypToolStorePluginFolder = System.IO.Path.Combine(CrypToolStorePluginFolder, "plugins");
            return CrypToolStorePluginFolder;
        }

        /// <summary>
        /// Returns the absolute path to the resources folder
        /// </summary>
        public string GetResourcesFolder()
        {
            //Translate the Ct2BuildType to a folder name for CrypToolStore plugins                
            string CrypToolStoreSubFolder;
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
            string CrypToolStorePluginFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), PluginManager.CrypToolStoreDirectory);
            CrypToolStorePluginFolder = System.IO.Path.Combine(CrypToolStorePluginFolder, CrypToolStoreSubFolder);
            CrypToolStorePluginFolder = System.IO.Path.Combine(CrypToolStorePluginFolder, "resources");
            return CrypToolStorePluginFolder;
        }

        /// <summary>
        /// User clicked InstallButton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedPlugin == null)
            {
                return;
            }
            MessageBoxResult result = MessageBox.Show(_windowToBlockForMessageBoxes, string.Format(Properties.Resources.CrypToolStorePresentation_InstallButton_Click_Do_you_really_want_to_download_and_install___0___from_CrypTool_Store_, SelectedPlugin.Name), string.Format(Properties.Resources.CrypToolStorePresentation_InstallButton_Click_Start_download_and_installation_of___0___, SelectedPlugin.Name), MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                if (SelectedPlugin is ResourceWrapper)
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            InstallResource();
                        }
                        catch (Exception ex)
                        {
                            string message = string.Format(Properties.Resources.CrypToolStorePresentation_InstallPlugin_Exception_occured_while_downloading_and_installing___0_, ex.Message);
                            _CrypToolStoreEditor.GuiLogMessage(message, NotificationLevel.Error);
                            _context.Send(callback => MessageBox.Show(_windowToBlockForMessageBoxes, message, Properties.Resources.CrypToolStorePresentation_InstallPlugin_Exception_occured_, MessageBoxButton.OK), null);
                        }
                    });
                }
                else if (SelectedPlugin is PluginWrapper)
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            InstallPlugin();
                        }
                        catch (Exception ex)
                        {
                            string message = string.Format(Properties.Resources.CrypToolStorePresentation_InstallPlugin_Exception_occured_while_downloading_and_installing___0_, ex.Message);
                            _CrypToolStoreEditor.GuiLogMessage(message, NotificationLevel.Error);
                            _context.Send(callback => MessageBox.Show(_windowToBlockForMessageBoxes, message, Properties.Resources.CrypToolStorePresentation_InstallPlugin_Exception_occured_, MessageBoxButton.OK), null);
                        }
                    });
                }
            }
        }

        /// <summary>
        /// Installs the plugin in an own thread
        /// </summary>
        private void InstallPlugin()
        {
            bool errorOccured = false;
            string assemblyfilename = System.IO.Path.Combine(GetPluginsFolder(), string.Format("assembly-{0}-{1}.zip", SelectedPlugin.PluginId, SelectedPlugin.PluginVersion));
            string xmlfilename = System.IO.Path.Combine(GetPluginsFolder(), string.Format("install-{0}-{1}.xml", SelectedPlugin.PluginId, SelectedPlugin.PluginVersion));

            //Step 0: delete files before download
            try
            {
                if (File.Exists(assemblyfilename))
                {
                    File.Delete(assemblyfilename);
                }
                if (File.Exists(assemblyfilename + ".part"))
                {
                    File.Delete(assemblyfilename + ".part");
                }
                if (File.Exists(xmlfilename))
                {
                    File.Delete(xmlfilename);
                }
            }
            catch (Exception ex)
            {
                _CrypToolStoreEditor.GuiLogMessage(string.Format(Properties.Resources.CrypToolStorePresentation_InstallPlugin_Exception_occured_while_deleting_old_installation_files___0_, ex.Message), NotificationLevel.Error);
                return;
            }

            //Step 1: Lock everything in the UI, thus, the user cannot do anything while downloading
            _context.Send(callback =>
            {
                try
                {
                    PluginListView.IsEnabled = false;
                    InstallButton.IsEnabled = false;
                    DeleteButton.IsEnabled = false;
                    Filter.IsEnabled = false;

                    InstallationProgressBar.Visibility = Visibility.Visible;
                    InstallationProgressText.Visibility = Visibility.Visible;
                }
                catch (Exception ex)
                {
                    _CrypToolStoreEditor.GuiLogMessage(string.Format(Properties.Resources.CrypToolStorePresentation_InstallPlugin_Exception_occured_while_locking_of_everything___0_, ex.Message), NotificationLevel.Error);
                }
            }, null);

            //Step 2: download assembly
            try
            {
                //Download assembly zip
                CrypToolStoreClient client = new CrypToolStoreClient
                {
                    ServerCertificate = new X509Certificate2(Properties.Resources.CTStoreTLS),
                    ServerAddress = CrypTool.CrypToolStore.Constants.ServerAddress,
                    ServerPort = CrypTool.CrypToolStore.Constants.ServerPort
                };
                client.UploadDownloadProgressChanged += client_UploadDownloadProgressChanged;

                //Translate the Ct2BuildType to PublishState
                PublishState publishState;
                switch (AssemblyHelper.BuildType)
                {
                    case Ct2BuildType.Developer:
                        publishState = PublishState.DEVELOPER;
                        break;
                    case Ct2BuildType.Nightly:
                        publishState = PublishState.NIGHTLY;
                        break;
                    case Ct2BuildType.Beta:
                        publishState = PublishState.BETA;
                        break;
                    case Ct2BuildType.Stable:
                        publishState = PublishState.RELEASE;
                        break;
                    default: //if no known version is given, we assume release
                        publishState = PublishState.RELEASE;
                        break;
                }

                //Connect to CrypToolStoreServer
                client.Connect();

                //get list of published plugins
                DataModificationOrRequestResult result = client.GetPublishedPlugin(SelectedPlugin.PluginId, publishState);
                if (result.Success == false)
                {
                    client.Disconnect();
                    string message = string.Format(Properties.Resources.CrypToolStorePresentation_InstallPlugin_Could_not_download_from_CrypToolStore_Server__Message_was___0_, result.Message);
                    _CrypToolStoreEditor.GuiLogMessage(message, NotificationLevel.Error);
                    _context.Send(callback => MessageBox.Show(_windowToBlockForMessageBoxes, message, Properties.Resources.CrypToolStorePresentation_InstallPlugin_Error_during_download_, MessageBoxButton.OK), null);
                }

                PluginAndSource pluginAndSource = (PluginAndSource)result.DataObject;
                bool stop = false;
                result = client.DownloadAssemblyZipFile(pluginAndSource.Source, assemblyfilename, ref stop);
                client.Disconnect();
                if (result.Success == false)
                {
                    client.Disconnect();
                    string message = string.Format(Properties.Resources.CrypToolStorePresentation_InstallPlugin_Could_not_download_from_CrypToolStore_Server__Message_was___0_, result.Message);
                    _CrypToolStoreEditor.GuiLogMessage(message, NotificationLevel.Error);
                    _context.Send(callback => MessageBox.Show(_windowToBlockForMessageBoxes, message, Properties.Resources.CrypToolStorePresentation_InstallPlugin_Error_during_download_, MessageBoxButton.OK), null);
                    errorOccured = true;
                    return;
                }

                //Step 3: Create installation xml file                
                using (StreamWriter xmlfile = new StreamWriter(xmlfilename))
                {
                    string type = "installation";
                    xmlfile.WriteLine(string.Format("<installation type=\"{0}\">", type));
                    xmlfile.WriteLine("  <plugin>");
                    xmlfile.WriteLine(string.Format("    <name>{0}</name>", SelectedPlugin.Name));
                    xmlfile.WriteLine(string.Format("    <id>{0}</id>", SelectedPlugin.PluginId));
                    xmlfile.WriteLine(string.Format("    <version>{0}</version>", SelectedPlugin.PluginVersion));
                    xmlfile.WriteLine("  </plugin>");
                    xmlfile.WriteLine("</installation>");
                }

                //Step 4: Notify user
                _context.Send(callback =>
                {
                    //set progress bar to 100%
                    InstallationProgressBar.Maximum = 1;
                    InstallationProgressBar.Value = 1;
                    InstallationProgressText.Content = "100 %";
                    //show messagebox
                    MessageBox.Show(_windowToBlockForMessageBoxes, string.Format(Properties.Resources.CrypToolStorePresentation_InstallPlugin___0___has_been_successfully_downloaded__You_need_to_restart_CrypTool_2_to_complete_installation_, SelectedPlugin.Name), Properties.Resources.CrypToolStorePresentation_InstallPlugin_Download_succeeded_, MessageBoxButton.OK);
                }, null);

                PendingChanges = true;
                OnStaticPropertyChanged("PendingChanges");
            }
            catch (Exception ex)
            {
                string message = string.Format(Properties.Resources.CrypToolStorePresentation_InstallPlugin_Exception_occured_while_downloading_and_installing___0_, ex.Message);
                _CrypToolStoreEditor.GuiLogMessage(message, NotificationLevel.Error);
                _context.Send(callback => MessageBox.Show(_windowToBlockForMessageBoxes, message, Properties.Resources.CrypToolStorePresentation_InstallPlugin_Exception_occured_, MessageBoxButton.OK), null);
                errorOccured = true;
            }
            finally
            {
                try
                {
                    //if something went wrong, we delete the zip and xml files
                    if (errorOccured)
                    {
                        if (File.Exists(assemblyfilename))
                        {
                            File.Delete(assemblyfilename);
                        }
                        if (File.Exists(assemblyfilename + ".part"))
                        {
                            File.Delete(assemblyfilename + ".part");
                        }
                        if (File.Exists(xmlfilename))
                        {
                            File.Delete(xmlfilename);
                        }
                    }
                }
                catch (Exception)
                {
                    //wtf?
                }

                //Step 5: Unlock everything in the UI
                _context.Send(callback =>
                {
                    try
                    {
                        PluginListView.IsEnabled = true;
                        InstallButton.IsEnabled = true;
                        DeleteButton.IsEnabled = true;
                        Filter.IsEnabled = true;

                        InstallationProgressBar.Visibility = Visibility.Collapsed;
                        InstallationProgressText.Visibility = Visibility.Collapsed;
                    }
                    catch (Exception ex)
                    {
                        _CrypToolStoreEditor.GuiLogMessage(string.Format(Properties.Resources.CrypToolStorePresentation_InstallPlugin_Exception_occured_while_unlocking_of_everything___0_, ex.Message), NotificationLevel.Error);
                    }
                }, null);

                //Step 6: Update StorePluginListTask
                Task updateStorePluginListTask = new Task(UpdateStorePluginList);
                updateStorePluginListTask.Start();
            }
        }

        /// <summary>
        /// Installs the resource in an own thread
        /// </summary>
        private void InstallResource()
        {
            bool errorOccured = false;
            string resourceFileName = System.IO.Path.Combine(GetResourceFolder((ResourceWrapper)SelectedPlugin), string.Format("resource-{0}-{1}.bin", SelectedPlugin.PluginId, SelectedPlugin.PluginVersion));

            //Step 0: create folder
            if (Directory.Exists(GetResourceFolder((ResourceWrapper)SelectedPlugin)))
            {
                Directory.Delete(GetResourceFolder((ResourceWrapper)SelectedPlugin), true);
            }
            Directory.CreateDirectory(GetResourceFolder((ResourceWrapper)SelectedPlugin));

            //Step 1: Lock everything in the UI, thus, the user cannot do anything while downloading
            _context.Send(callback =>
            {
                try
                {
                    PluginListView.IsEnabled = false;
                    InstallButton.IsEnabled = false;
                    DeleteButton.IsEnabled = false;
                    Filter.IsEnabled = false;

                    InstallationProgressBar.Visibility = Visibility.Visible;
                    InstallationProgressText.Visibility = Visibility.Visible;
                }
                catch (Exception ex)
                {
                    _CrypToolStoreEditor.GuiLogMessage(string.Format(Properties.Resources.CrypToolStorePresentation_InstallPlugin_Exception_occured_while_locking_of_everything___0_, ex.Message), NotificationLevel.Error);
                }
            }, null);

            //Step 2: download resource file
            try
            {
                //Download resource file
                CrypToolStoreClient client = new CrypToolStoreClient
                {
                    ServerCertificate = new X509Certificate2(Properties.Resources.CTStoreTLS),
                    ServerAddress = CrypTool.CrypToolStore.Constants.ServerAddress,
                    ServerPort = CrypTool.CrypToolStore.Constants.ServerPort
                };
                client.UploadDownloadProgressChanged += client_UploadDownloadProgressChanged;

                //Translate the Ct2BuildType to PublishState
                PublishState publishState;
                switch (AssemblyHelper.BuildType)
                {
                    case Ct2BuildType.Developer:
                        publishState = PublishState.DEVELOPER;
                        break;
                    case Ct2BuildType.Nightly:
                        publishState = PublishState.NIGHTLY;
                        break;
                    case Ct2BuildType.Beta:
                        publishState = PublishState.BETA;
                        break;
                    case Ct2BuildType.Stable:
                        publishState = PublishState.RELEASE;
                        break;
                    default: //if no known version is given, we assume release
                        publishState = PublishState.RELEASE;
                        break;
                }

                //Connect to CrypToolStoreServer
                client.Connect();

                //get list of published resources
                DataModificationOrRequestResult result = client.GetPublishedResource(SelectedPlugin.PluginId, publishState);
                if (result.Success == false)
                {
                    client.Disconnect();
                    string message = string.Format(Properties.Resources.CrypToolStorePresentation_InstallPlugin_Could_not_download_from_CrypToolStore_Server__Message_was___0_, result.Message);
                    _CrypToolStoreEditor.GuiLogMessage(message, NotificationLevel.Error);
                    _context.Send(callback => MessageBox.Show(_windowToBlockForMessageBoxes, message, Properties.Resources.CrypToolStorePresentation_InstallPlugin_Error_during_download_, MessageBoxButton.OK), null);
                }

                ResourceAndResourceData resourceAndResourceData = (ResourceAndResourceData)result.DataObject;
                bool stop = false;
                result = client.DownloadResourceDataFile(resourceAndResourceData.ResourceData, resourceFileName, ref stop);
                client.Disconnect();
                if (result.Success == false)
                {
                    client.Disconnect();
                    string message = string.Format(Properties.Resources.CrypToolStorePresentation_InstallPlugin_Could_not_download_from_CrypToolStore_Server__Message_was___0_, result.Message);
                    _CrypToolStoreEditor.GuiLogMessage(message, NotificationLevel.Error);
                    _context.Send(callback => MessageBox.Show(_windowToBlockForMessageBoxes, message, Properties.Resources.CrypToolStorePresentation_InstallPlugin_Error_during_download_, MessageBoxButton.OK), null);
                    errorOccured = true;
                    return;
                }

                //Step 3: Notify user
                _context.Send(callback =>
                {
                    //set progress bar to 100%
                    InstallationProgressBar.Maximum = 1;
                    InstallationProgressBar.Value = 1;
                    InstallationProgressText.Content = "100 %";
                    //show messagebox
                    MessageBox.Show(_windowToBlockForMessageBoxes, string.Format(Properties.Resources.CrypToolStorePresentation_ResourceSuccessfullyInstalled, SelectedPlugin.Name), Properties.Resources.CrypToolStorePresentation_ResourceSuccessfullyInstalled, MessageBoxButton.OK);
                }, null);
            }
            catch (Exception ex)
            {
                string message = string.Format(Properties.Resources.CrypToolStorePresentation_InstallPlugin_Exception_occured_while_downloading_and_installing___0_, ex.Message);
                _CrypToolStoreEditor.GuiLogMessage(message, NotificationLevel.Error);
                _context.Send(callback => MessageBox.Show(_windowToBlockForMessageBoxes, message, Properties.Resources.CrypToolStorePresentation_InstallPlugin_Exception_occured_, MessageBoxButton.OK), null);
                errorOccured = true;
            }
            finally
            {
                try
                {
                    //if something went wrong, we delete the folder
                    if (errorOccured)
                    {
                        if (Directory.Exists(GetResourceFolder((ResourceWrapper)SelectedPlugin)))
                        {
                            Directory.Delete(GetResourceFolder((ResourceWrapper)SelectedPlugin), true);
                        }
                    }
                }
                catch (Exception)
                {
                    //wtf?
                }

                //Step 5: Unlock everything in the UI
                _context.Send(callback =>
                {
                    try
                    {
                        PluginListView.IsEnabled = true;
                        InstallButton.IsEnabled = true;
                        DeleteButton.IsEnabled = true;
                        Filter.IsEnabled = true;

                        InstallationProgressBar.Visibility = Visibility.Collapsed;
                        InstallationProgressText.Visibility = Visibility.Collapsed;
                    }
                    catch (Exception ex)
                    {
                        _CrypToolStoreEditor.GuiLogMessage(string.Format(Properties.Resources.CrypToolStorePresentation_InstallPlugin_Exception_occured_while_unlocking_of_everything___0_, ex.Message), NotificationLevel.Error);
                    }
                }, null);

                //Step 6: Update StorePluginListTask
                Task updateStorePluginListTask = new Task(UpdateStorePluginList);
                updateStorePluginListTask.Start();
            }
        }

        /// <summary>
        /// Updates the download progress bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_UploadDownloadProgressChanged(object sender, UploadDownloadProgressEventArgs e)
        {
            if (e.FileSize <= 0)
            {
                return;
            }
            _context.Send(callback =>
            {
                InstallationProgressBar.Maximum = e.FileSize;
                InstallationProgressBar.Value = e.DownloadedUploaded;
                double progress = e.DownloadedUploaded / (double)e.FileSize * 100;
                InstallationProgressText.Content = Math.Round(progress, 2) + " % (" + Tools.FormatSpeedString(e.BytePerSecond) + " - " + Tools.RemainingTime(e.BytePerSecond, e.FileSize, e.DownloadedUploaded) + ")";
            }, null);
        }

        /// <summary>
        /// Deletes the selected plugin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {

            if (SelectedPlugin == null)
            {
                return;
            }
            try
            {
                MessageBoxResult result = MessageBox.Show(_windowToBlockForMessageBoxes, string.Format(Properties.Resources.CrypToolStorePresentation_DeleteButton_Click_Do_you_really_want_to_uninstall___0___from_CrypTool_Store_, SelectedPlugin.Name), Properties.Resources.CrypToolStorePresentation_DeleteButton_Click_Unistallation_, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    if (SelectedPlugin is ResourceWrapper)
                    {
                        DeleteResource();
                    }
                    else if (SelectedPlugin is PluginWrapper)
                    {
                        DeletePlugin();
                    }
                }
            }
            catch (Exception ex)
            {
                string message = string.Format(Properties.Resources.CrypToolStorePresentation_DeleteButton_Click_Could_not_uninstall__Exception_was___0_, ex.Message);
                _CrypToolStoreEditor.GuiLogMessage(message, NotificationLevel.Error);
                MessageBox.Show(_windowToBlockForMessageBoxes, message, Properties.Resources.CrypToolStorePresentation_DeleteButton_Click_Exception_during_uninstallation_, MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Deletes a plugin
        /// </summary>
        private void DeletePlugin()
        {
            string assemblyfilename = System.IO.Path.Combine(GetPluginsFolder(), string.Format("assembly-{0}-{1}.zip", SelectedPlugin.PluginId, SelectedPlugin.PluginVersion));
            string xmlfilename = System.IO.Path.Combine(GetPluginsFolder(), string.Format("install-{0}-{1}.xml", SelectedPlugin.PluginId, SelectedPlugin.PluginVersion));

            try
            {
                if (File.Exists(assemblyfilename))
                {
                    File.Delete(assemblyfilename);
                }
                if (File.Exists(xmlfilename))
                {
                    File.Delete(xmlfilename);
                }
            }
            catch (Exception ex)
            {
                _CrypToolStoreEditor.GuiLogMessage(string.Format(Properties.Resources.CrypToolStorePresentation_InstallPlugin_Exception_occured_while_deleting_old_installation_files___0_, ex.Message), NotificationLevel.Error);
                return;
            }

            using (StreamWriter xmlfile = new StreamWriter(xmlfilename))
            {
                string type = "deletion";
                xmlfile.WriteLine(string.Format("<installation type=\"{0}\">", type));
                xmlfile.WriteLine("  <plugin>");
                xmlfile.WriteLine(string.Format("    <name>{0}</name>", SelectedPlugin.Name));
                xmlfile.WriteLine(string.Format("    <id>{0}</id>", SelectedPlugin.PluginId));
                xmlfile.WriteLine(string.Format("    <version>{0}</version>", SelectedPlugin.PluginVersion));
                xmlfile.WriteLine("  </plugin>");
                xmlfile.WriteLine("</installation>");
            }

            MessageBox.Show(_windowToBlockForMessageBoxes, string.Format(Properties.Resources.CrypToolStorePresentation_DeleteButton_Click___0___has_been_marked_for_uninstallation__You_need_to_restart_CrypTool_2_to_complete_installation_, SelectedPlugin.Name), "Marked for uninstallation.", MessageBoxButton.OK);

            PendingChanges = true;
            OnStaticPropertyChanged("PendingChanges");

            //update StorePluginList
            Task updateStorePluginListTask = new Task(UpdateStorePluginList);
            updateStorePluginListTask.Start();
        }

        /// <summary>
        /// Deletes a resource
        /// </summary>
        private void DeleteResource()
        {
            try
            {
                string resourceDirectory = GetResourceFolder((ResourceWrapper)SelectedPlugin);
                if (Directory.Exists(resourceDirectory))
                {
                    Directory.Delete(resourceDirectory, true);
                }
            }
            catch (Exception ex)
            {
                _CrypToolStoreEditor.GuiLogMessage(string.Format(Properties.Resources.CrypToolStorePresentation_InstallPlugin_Exception_occured_while_deleting_old_installation_files___0_, ex.Message), NotificationLevel.Error);
                return;
            }

            //update StorePluginList
            Task updateStorePluginListTask = new Task(UpdateStorePluginList);
            updateStorePluginListTask.Start();
        }


        /// <summary>
        /// User pressed RestartButton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int processID = Process.GetCurrentProcess().Id;
                string exePath = Process.GetCurrentProcess().MainModule.FileName;
                string CrypToolFolderPath = System.IO.Path.GetDirectoryName(exePath);
                string updaterPath = System.IO.Path.Combine(DirectoryHelper.BaseDirectory, "CrypUpdater.exe");
                string parameters = "\"dummy\" " + "\"" + CrypToolFolderPath + "\" " + "\"" + exePath + "\" " + "\"" + processID + "\" -JustRestart";
                Process.Start(updaterPath, parameters);
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                string message = string.Format(Properties.Resources.CrypToolStorePresentation_RestartButton_Click_Exception_occured_while_trying_to_restart_CrypTool_2___0_, ex.Message);
                _CrypToolStoreEditor.GuiLogMessage(message, NotificationLevel.Error);
                MessageBox.Show(_windowToBlockForMessageBoxes, message, Properties.Resources.CrypToolStorePresentation_RestartButton_Click_Exception_during_restart_, MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Static property changed
        /// </summary>
        /// <param name="name"></param>
        private void OnStaticPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(StaticPropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public static event PropertyChangedEventHandler StaticPropertyChanged;

        private void ShowResources_Checked(object sender, RoutedEventArgs e)
        {
            //update StorePluginList
            Task updateStorePluginListTask = new Task(UpdateStorePluginList);
            updateStorePluginListTask.Start();
        }

        private void ShowResources_Unchecked(object sender, RoutedEventArgs e)
        {
            //update StorePluginList
            Task updateStorePluginListTask = new Task(UpdateStorePluginList);
            updateStorePluginListTask.Start();
        }

        /// <summary>
        /// Copies all infos about the currently selected plugin to the clipboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedPlugin != null)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine(string.Format("Name: {0}", SelectedPlugin.Name));
                    builder.AppendLine(string.Format("AuthorNames: {0}", SelectedPlugin.Authornames));
                    builder.AppendLine(string.Format("AuthorInstitutes: {0}", SelectedPlugin.Authorinstitutes));
                    builder.AppendLine(string.Format("AuthorEmails: {0}", SelectedPlugin.Authoremails));
                    builder.AppendLine(string.Format("BuildDate: {0}", SelectedPlugin.BuildDate));
                    builder.AppendLine(string.Format("PluginId: {0}", SelectedPlugin.PluginId));
                    builder.AppendLine(string.Format("PluginVersion: {0}", SelectedPlugin.PluginVersion));
                    builder.AppendLine(string.Format("BuildVersion: {0}", SelectedPlugin.BuildVersion));
                    builder.AppendLine(string.Format("ShortDescription: {0}", SelectedPlugin.ShortDescription));
                    builder.AppendLine(string.Format("LongDescription: {0}", SelectedPlugin.LongDescription));
                    builder.AppendLine(string.Format("FileSize: {0}", SelectedPlugin.FileSize));
                    SetClipboard(builder.ToString());
                }
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private void SetClipboard(string text)
        {
            text = text.Replace(Convert.ToChar(0x0).ToString(), "");    //Remove null chars in order to avoid problems in clipboard
            Clipboard.SetText(text);
        }
    }

    /// <summary>
    /// Converts a boolean to Visibility (inverse)
    /// false => Visible
    /// true => Collapsed
    /// </summary>
    public class InverseBooleanToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool && (bool)value)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Visibility && (Visibility)value == Visibility.Visible)
            {
                return false;
            }
            return true;
        }
    }
}
