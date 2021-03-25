/*
   Copyright 2018 Nils Kopal <Nils.Kopal<AT>Uni-Kassel.de>

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
using CrypToolStoreLib.Client;
using CrypToolStoreLib.DataObjects;
using CrypToolStoreLib.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CrypToolStoreDeveloperClient.Views
{
    /// <summary>
    /// Interaktionslogik für SourceManagementView.xaml
    /// </summary>
    public partial class SourceManagementView : UserControl
    {
        public MainWindow MainWindow { get; set; }

        private ObservableCollection<Source> Sources = new ObservableCollection<Source>();
        public int PluginId { get; set; }

        private Configuration Config = Configuration.GetConfiguration();

        public SourceManagementView()
        {
            InitializeComponent();
            SourcesListView.ItemsSource = Sources;
            Sources.Clear();
            IsVisibleChanged += SourceManagementView_IsVisibleChanged;
        }

        /// <summary>
        /// Called, when the UI changes to visible state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SourceManagementView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsVisible)
            {
                return;
            }            

            //we fetch the source list in a separate thread, thus, the ui is not blocked during download of the list
            Thread fetchSourceListThread = new Thread(FetchSourceList);
            fetchSourceListThread.IsBackground = true;
            fetchSourceListThread.Start();
        }

        /// <summary>
        /// Method requests a source list and stores it in the list of the GUI
        /// </summary>
        private void FetchSourceList()
        {
            try
            {
                CrypToolStoreClient client = new CrypToolStoreClient();
                client.ServerCertificate = MainWindow.ServerCertificate;
                client.ServerAddress = Config.GetConfigEntry("ServerAddress");
                client.ServerPort = int.Parse(Config.GetConfigEntry("ServerPort"));
                client.Connect();
                client.Login(MainWindow.Username, MainWindow.Password);
                DataModificationOrRequestResult result = client.GetSourceList(PluginId);
                List<Source> sources = (List<Source>)result.DataObject;
                client.Disconnect();
                Dispatcher.BeginInvoke(new ThreadStart(() =>
                {
                    try
                    {
                        Sources.Clear();
                        foreach (Source source in sources)
                        {
                            Sources.Add(source);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("Exception during adding sources to list: {0}", ex.Message), "Exception");
                    }
                }));                
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Exception during retrieving list of sources: {0}", ex.Message), "Exception");
            }
        }

        /// <summary>
        /// Deletes the source defined by the clicked button
        /// Then, updates the source list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int pluginversion = (int)button.CommandParameter;

            MessageBoxResult messageBoxResult = MessageBox.Show(string.Format("Do you really want to delete the source {0}-{1}?", PluginId, pluginversion), string.Format("Delete {0}-{1}", PluginId, pluginversion), MessageBoxButton.YesNo);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                try
                {
                    CrypToolStoreClient client = new CrypToolStoreClient();
                    client.ServerCertificate = MainWindow.ServerCertificate;
                    client.ServerAddress = Config.GetConfigEntry("ServerAddress");
                    client.ServerPort = int.Parse(Config.GetConfigEntry("ServerPort"));
                    client.Connect();
                    client.Login(MainWindow.Username, MainWindow.Password);
                    DataModificationOrRequestResult result = client.DeleteSource(PluginId, pluginversion);
                    client.Disconnect();

                    if (result.Success)
                    {
                        MessageBox.Show(string.Format("Successfully deleted source {0}-{1}", PluginId, pluginversion), "Source deleted");
                        //we fetch the source list in a separate thread, thus, the ui is not blocked during download of the list
                        Thread fetchSourceListThread = new Thread(FetchSourceList);
                        fetchSourceListThread.IsBackground = true;
                        fetchSourceListThread.Start();
                    }
                    else
                    {
                        MessageBox.Show(string.Format("Could not delete source: {0}", result.Message), "Deletion not possible");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Exception during deletion of source: {0}", ex.Message), "Exception");
                }
            }
        }

        /// <summary>
        /// Shows a window for uploading a zip of the source
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UploadSourceZipFile_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int pluginversion = (int)button.CommandParameter;
            UploadSourceZipFileWindow uploadSourceZipFileWindow = new UploadSourceZipFileWindow(PluginId, pluginversion);
            uploadSourceZipFileWindow.MainWindow = MainWindow;
            uploadSourceZipFileWindow.ShowDialog();
            //we fetch the source list in a separate thread, thus, the ui is not blocked during download of the list
            Thread fetchSourceListThread = new Thread(FetchSourceList);
            fetchSourceListThread.IsBackground = true;
            fetchSourceListThread.Start();
        }

        /// <summary>
        /// Shows a window for downloading a zip of the source
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadSourceZipFile_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int pluginversion = (int)button.CommandParameter;
            DownloadSourceZipFileWindow downloadSourceZipFileWindow = new DownloadSourceZipFileWindow(PluginId, pluginversion);
            downloadSourceZipFileWindow.MainWindow = MainWindow;
            downloadSourceZipFileWindow.ShowDialog();
            //we fetch the source list in a separate thread, thus, the ui is not blocked during download of the list
            Thread fetchSourceListThread = new Thread(FetchSourceList);
            fetchSourceListThread.IsBackground = true;
            fetchSourceListThread.Start();
        }

        /// <summary>
        /// Shows a window for downloading a zip of an assembly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadAssembly_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int pluginversion = (int)button.CommandParameter;
            DownloadAssemblyZipFileWindow downloadAssemblyZipFileWindow = new DownloadAssemblyZipFileWindow(PluginId, pluginversion);
            downloadAssemblyZipFileWindow.MainWindow = MainWindow;
            downloadAssemblyZipFileWindow.ShowDialog();
            //we fetch the source list in a separate thread, thus, the ui is not blocked during download of the list
            Thread fetchSourceListThread = new Thread(FetchSourceList);
            fetchSourceListThread.IsBackground = true;
            fetchSourceListThread.Start();
        }

        /// <summary>
        /// Updates the publish state of a source
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdatePublishState_Click(object sender, RoutedEventArgs e)
        {
            if (!MainWindow.IsAdmin)
            {
                MessageBox.Show("Only administrators are allowed to change the publish state of a source", "Update publish state not possible");
                return;
            }
            Button button = (Button)sender;
            int pluginversion = (int)button.CommandParameter;

            //retrieve old state for default selection of combo box in UpdateSourcePublishStateWindow
            string oldstate = "NOTPUBLISHED";
            try
            {
                CrypToolStoreClient client = new CrypToolStoreClient();
                client.ServerCertificate = MainWindow.ServerCertificate;
                client.ServerAddress = Config.GetConfigEntry("ServerAddress");
                client.ServerPort = int.Parse(Config.GetConfigEntry("ServerPort"));
                client.Connect();
                client.Login(MainWindow.Username, MainWindow.Password);
                DataModificationOrRequestResult result = client.GetSource(PluginId, pluginversion);
                Source source = (Source)result.DataObject;
                
                client.Disconnect();
                oldstate = source.PublishState;                
            }
            catch (Exception)
            {
                //wtf?
            }            
            UpdateSourcePublishStateWindow updateSourcePublishStateWindow = new UpdateSourcePublishStateWindow(PluginId, pluginversion, oldstate);
            updateSourcePublishStateWindow.MainWindow = MainWindow;
            updateSourcePublishStateWindow.ShowDialog();
            //we fetch the source list in a separate thread, thus, the ui is not blocked during download of the list
            Thread fetchSourceListThread = new Thread(FetchSourceList);
            fetchSourceListThread.IsBackground = true;
            fetchSourceListThread.Start();
        }

        /// <summary>
        /// Shows a window for creating a new source
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateNewSourceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult messageBoxResult = MessageBox.Show(string.Format("Do you really want to create a new source for plugin {0}?", PluginId), "Create new source", MessageBoxButton.YesNo);

                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    CrypToolStoreClient client = new CrypToolStoreClient();
                    client.ServerCertificate = MainWindow.ServerCertificate;
                    client.ServerAddress = Config.GetConfigEntry("ServerAddress");
                    client.ServerPort = int.Parse(Config.GetConfigEntry("ServerPort"));
                    client.Connect();
                    client.Login(MainWindow.Username, MainWindow.Password);

                    //1. compute highest version number for new source
                    DataModificationOrRequestResult result = client.GetSourceList(PluginId);

                    if (result.Success == false)
                    {
                        client.Disconnect();
                        MessageBox.Show(string.Format("Could not get source list for computing new version number: {0}", result.Message), "Creation not possible");
                        return;
                    }

                    List<Source> sources = (List<Source>)result.DataObject;

                    int highestVersionNumber = 0;
                    foreach (Source source in sources)
                    {
                        if (source.PluginVersion > highestVersionNumber)
                        {
                            highestVersionNumber = source.PluginVersion;
                        }
                    }

                    //2: create new source
                    Source newsource = new Source();
                    newsource.PluginId = PluginId;
                    newsource.PluginVersion = highestVersionNumber + 1;
                    newsource.BuildState = BuildState.CREATED.ToString();
                    newsource.BuildLog = string.Format("Created by {0}", MainWindow.Username);

                    result = client.CreateSource(newsource);
                    client.Disconnect();

                    //3. Show result to user
                    if (result.Success == true)
                    {
                        MessageBox.Show(string.Format("Created new source: {0}-{1}", newsource.PluginId, newsource.PluginVersion), "Source created");
                        //we fetch the source list in a separate thread, thus, the ui is not blocked during download of the list
                        Thread fetchSourceListThread = new Thread(FetchSourceList);
                        fetchSourceListThread.IsBackground = true;
                        fetchSourceListThread.Start();
                    }
                    else
                    {
                        MessageBox.Show(string.Format("Creation not possible: {0}", result.Message), "Source created");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Exception during creation of new source: {0}", ex.Message), "Exception");
            }
        }

        
        /// <summary>
        /// Shows a window containing the buildlog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Buildlog_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string buildlog = (string)button.CommandParameter;
            ShowBuildLogWindow showBuildlogWindow = new ShowBuildLogWindow(buildlog);
            showBuildlogWindow.MainWindow = MainWindow;
            showBuildlogWindow.ShowDialog();          
        }
        

        /// <summary>
        /// Resets the view
        /// Clears the source list
        /// </summary>
        public void Reset()
        {
            Dispatcher.BeginInvoke(new ThreadStart(() =>
            {
                try
                {
                    Sources.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Exception during reset of source list: {0}", ex.Message), "Exception");
                }
            }));
        }

    }
}