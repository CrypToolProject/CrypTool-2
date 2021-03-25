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
    /// Interaktionslogik für ResourceDataManagementView.xaml
    /// </summary>
    public partial class ResourceDataManagementView : UserControl
    {
        public MainWindow MainWindow { get; set; }

        private ObservableCollection<ResourceData> ResourceDatas = new ObservableCollection<ResourceData>();
        public int ResourceId { get; set; }

        private Configuration Config = Configuration.GetConfiguration();

        public ResourceDataManagementView()
        {
            InitializeComponent();
            ResourceDatasListView.ItemsSource = ResourceDatas;
            ResourceDatas.Clear();
            IsVisibleChanged += ResourceDataManagementView_IsVisibleChanged;
        }

        /// <summary>
        /// Called, when the UI changes to visible state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResourceDataManagementView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsVisible)
            {
                return;
            }

            //we fetch the ResourceData list in a separate thread, thus, the ui is not blocked during download of the list
            Thread fetchResourceDataListThread = new Thread(FetchResourceDataList);
            fetchResourceDataListThread.IsBackground = true;
            fetchResourceDataListThread.Start();
        }

        /// <summary>
        /// Method requests a ResourceData list and stores it in the list of the GUI
        /// </summary>
        private void FetchResourceDataList()
        {
            try
            {
                CrypToolStoreClient client = new CrypToolStoreClient();
                client.ServerCertificate = MainWindow.ServerCertificate;
                client.ServerAddress = Config.GetConfigEntry("ServerAddress");
                client.ServerPort = int.Parse(Config.GetConfigEntry("ServerPort"));
                client.Connect();
                client.Login(MainWindow.Username, MainWindow.Password);
                DataModificationOrRequestResult result = client.GetResourceDataList(ResourceId);
                List<ResourceData> resourceDatas = (List<ResourceData>)result.DataObject;
                client.Disconnect();
                Dispatcher.BeginInvoke(new ThreadStart(() =>
                {
                    try
                    {
                        ResourceDatas.Clear();
                        foreach (ResourceData ResourceData in resourceDatas)
                        {
                            ResourceDatas.Add(ResourceData);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("Exception during adding ResourceDatas to list: {0}", ex.Message), "Exception");
                    }
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Exception during retrieving list of ResourceDatas: {0}", ex.Message), "Exception");
            }
        }

        /// <summary>
        /// Deletes the ResourceData defined by the clicked button
        /// Then, updates the ResourceData list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int pluginversion = (int)button.CommandParameter;

            MessageBoxResult messageBoxResult = MessageBox.Show(string.Format("Do you really want to delete the ResourceData {0}-{1}?", ResourceId, pluginversion), string.Format("Delete {0}-{1}", ResourceId, pluginversion), MessageBoxButton.YesNo);

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
                    DataModificationOrRequestResult result = client.DeleteResourceData(ResourceId, pluginversion);
                    client.Disconnect();

                    if (result.Success)
                    {
                        MessageBox.Show(string.Format("Successfully deleted ResourceData {0}-{1}", ResourceId, pluginversion), "ResourceData deleted");
                        //we fetch the ResourceData list in a separate thread, thus, the ui is not blocked during download of the list
                        Thread fetchResourceDataListThread = new Thread(FetchResourceDataList);
                        fetchResourceDataListThread.IsBackground = true;
                        fetchResourceDataListThread.Start();
                    }
                    else
                    {
                        MessageBox.Show(string.Format("Could not delete ResourceData: {0}", result.Message), "Deletion not possible");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Exception during deletion of ResourceData: {0}", ex.Message), "Exception");
                }
            }
        }

        /// <summary>
        /// Shows a window for uploading a file of the ResourceData
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UploadResourceDataZipFile_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int resourceversion = (int)button.CommandParameter;
            UploadResourceDataFileWindow uploadResourceDataFileWindow = new UploadResourceDataFileWindow(ResourceId, resourceversion);
            uploadResourceDataFileWindow.MainWindow = MainWindow;
            uploadResourceDataFileWindow.ShowDialog();
            //we fetch the ResourceData list in a separate thread, thus, the ui is not blocked during download of the list
            Thread fetchResourceDataListThread = new Thread(FetchResourceDataList);
            fetchResourceDataListThread.IsBackground = true;
            fetchResourceDataListThread.Start();
        }

        /// <summary>
        /// Shows a window for downloading a file of the ResourceData
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadResourceDataZipFile_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int resourceversion = (int)button.CommandParameter;
            DownloadResourceDataFileWindow downloadResourceDataFileWindow = new DownloadResourceDataFileWindow(ResourceId, resourceversion);
            downloadResourceDataFileWindow.MainWindow = MainWindow;
            downloadResourceDataFileWindow.ShowDialog();
            //we fetch the ResourceData list in a separate thread, thus, the ui is not blocked during download of the list
            Thread fetchResourceDataListThread = new Thread(FetchResourceDataList);
            fetchResourceDataListThread.IsBackground = true;
            fetchResourceDataListThread.Start();
        }
     
        /// <summary>
        /// Updates the publish state of a ResourceData
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdatePublishState_Click(object sender, RoutedEventArgs e)
        {
            if (!MainWindow.IsAdmin)
            {
                MessageBox.Show("Only administrators are allowed to change the publish state of a ResourceData", "Update publish state not possible");
                return;
            }
            Button button = (Button)sender;
            int resourceversion = (int)button.CommandParameter;

            //retrieve old state for default selection of combo box in UpdateResourceDataPublishStateWindow
            string oldstate = "NOTPUBLISHED";
            try
            {
                CrypToolStoreClient client = new CrypToolStoreClient();
                client.ServerCertificate = MainWindow.ServerCertificate;
                client.ServerAddress = Config.GetConfigEntry("ServerAddress");
                client.ServerPort = int.Parse(Config.GetConfigEntry("ServerPort"));
                client.Connect();
                client.Login(MainWindow.Username, MainWindow.Password);
                DataModificationOrRequestResult result = client.GetResourceData(ResourceId, resourceversion);
                ResourceData ResourceData = (ResourceData)result.DataObject;

                client.Disconnect();
                oldstate = ResourceData.PublishState;
            }
            catch (Exception)
            {
                //wtf?
            }
            UpdateResourceDataPublishStateWindow updateResourceDataPublishStateWindow = new UpdateResourceDataPublishStateWindow(ResourceId, resourceversion, oldstate);
            updateResourceDataPublishStateWindow.MainWindow = MainWindow;
            updateResourceDataPublishStateWindow.ShowDialog();
            //we fetch the ResourceData list in a separate thread, thus, the ui is not blocked during download of the list
            Thread fetchResourceDataListThread = new Thread(FetchResourceDataList);
            fetchResourceDataListThread.IsBackground = true;
            fetchResourceDataListThread.Start();
        }

        /// <summary>
        /// Shows a window for creating a new ResourceData
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateNewResourceDataButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult messageBoxResult = MessageBox.Show(string.Format("Do you really want to create a new ResourceData for Resource {0}?", ResourceId), "Create new ResourceData", MessageBoxButton.YesNo);

                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    CrypToolStoreClient client = new CrypToolStoreClient();
                    client.ServerCertificate = MainWindow.ServerCertificate;
                    client.ServerAddress = Config.GetConfigEntry("ServerAddress");
                    client.ServerPort = int.Parse(Config.GetConfigEntry("ServerPort"));
                    client.Connect();
                    client.Login(MainWindow.Username, MainWindow.Password);

                    //1. compute highest version number for new ResourceData
                    DataModificationOrRequestResult result = client.GetResourceDataList(ResourceId);

                    if (result.Success == false)
                    {
                        client.Disconnect();
                        MessageBox.Show(string.Format("Could not get ResourceData list for computing new version number: {0}", result.Message), "Creation not possible");
                        return;
                    }

                    List<ResourceData> ResourceDatas = (List<ResourceData>)result.DataObject;

                    int highestVersionNumber = 0;
                    foreach (ResourceData ResourceData in ResourceDatas)
                    {
                        if (ResourceData.ResourceVersion > highestVersionNumber)
                        {
                            highestVersionNumber = ResourceData.ResourceVersion;
                        }
                    }

                    //2: create new ResourceData
                    ResourceData newResourceData = new ResourceData();
                    newResourceData.ResourceId = ResourceId;
                    newResourceData.ResourceVersion = highestVersionNumber + 1;

                    result = client.CreateResourceData(newResourceData);
                    client.Disconnect();

                    //3. Show result to user
                    if (result.Success == true)
                    {
                        MessageBox.Show(string.Format("Created new ResourceData: {0}-{1}", newResourceData.ResourceId, newResourceData.ResourceVersion), "ResourceData created");
                        //we fetch the ResourceData list in a separate thread, thus, the ui is not blocked during download of the list
                        Thread fetchResourceDataListThread = new Thread(FetchResourceDataList);
                        fetchResourceDataListThread.IsBackground = true;
                        fetchResourceDataListThread.Start();
                    }
                    else
                    {
                        MessageBox.Show(string.Format("Creation not possible: {0}", result.Message), "ResourceData created");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Exception during creation of new ResourceData: {0}", ex.Message), "Exception");
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
        /// Clears the ResourceData list
        /// </summary>
        public void Reset()
        {
            Dispatcher.BeginInvoke(new ThreadStart(() =>
            {
                try
                {
                    ResourceDatas.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Exception during reset of ResourceData list: {0}", ex.Message), "Exception");
                }
            }));
        }

    }
}