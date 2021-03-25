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
    /// Interaktionslogik für ResourceManagementView.xaml
    /// </summary>
    public partial class ResourceManagementView : UserControl
    {
        public MainWindow MainWindow { get; set; }

        private ObservableCollection<Resource> ResourcesList = new ObservableCollection<Resource>();

        private Configuration Config = Configuration.GetConfiguration();

        public ResourceManagementView()
        {
            InitializeComponent();
            ResourcesListView.ItemsSource = ResourcesList;
            ResourcesList.Clear();
            IsVisibleChanged += ResourceManagementView_IsVisibleChanged;
        }

        /// <summary>
        /// Called, when the UI changes to visible starte
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResourceManagementView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsVisible)
            {
                return;
            }

            //we fetch the Resource list in a separate thread, thus, the ui is not blocked during download of the list
            Thread fetchResourceListThread = new Thread(FetchResourceList);
            fetchResourceListThread.IsBackground = true;
            fetchResourceListThread.Start();
        }

        /// <summary>
        /// Method requests a Resource list and stores it in the list of the GUI
        /// </summary>
        private void FetchResourceList()
        {
            try
            {
                CrypToolStoreClient client = new CrypToolStoreClient();
                client.ServerCertificate = MainWindow.ServerCertificate;
                client.ServerAddress = Config.GetConfigEntry("ServerAddress");
                client.ServerPort = int.Parse(Config.GetConfigEntry("ServerPort"));
                client.Connect();
                client.Login(MainWindow.Username, MainWindow.Password);
                DataModificationOrRequestResult result = client.GetResourceList(MainWindow.IsAdmin ? "*" : MainWindow.Username);
                List<Resource> resources = (List<Resource>)result.DataObject;
                client.Disconnect();
                Dispatcher.BeginInvoke(new ThreadStart(() =>
                {
                    try
                    {
                        ResourcesList.Clear();
                        foreach (Resource Resource in resources)
                        {
                            if (Resource.Username == MainWindow.Username || MainWindow.IsAdmin)
                            {
                                ResourcesList.Add(Resource);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("Exception during adding Resources to list: {0}", ex.Message), "Exception");
                    }
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Exception during retrieving list of Resources: {0}", ex.Message), "Exception");
            }
        }

        /// <summary>
        /// Deletes the Resource defined by the clicked button
        /// Then, updates the Resource list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int id = (int)button.CommandParameter;

            MessageBoxResult messageBoxResult = MessageBox.Show(string.Format("Do you really want to delete the Resource {0}?", id), string.Format("Delete {0}", id), MessageBoxButton.YesNo);

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
                    DataModificationOrRequestResult result = client.DeleteResource(id);
                    client.Disconnect();

                    if (result.Success)
                    {
                        MessageBox.Show(string.Format("Successfully deleted {0}", id), "Resource deleted");
                        FetchResourceList();
                    }
                    else
                    {
                        MessageBox.Show(string.Format("Could not delete Resource: {0}", result.Message), "Deletion not possible");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Exception during deletion of Resource: {0}", ex.Message), "Exception");
                }
            }
        }

        /// <summary>
        /// Shows a window for updating a Resource
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Update_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int id = (int)button.CommandParameter;
            UpdateResourceWindow updateResourceWindow = new UpdateResourceWindow(id);
            updateResourceWindow.MainWindow = MainWindow;
            updateResourceWindow.ShowDialog();
            //we fetch the Resource list in a separate thread, thus, the ui is not blocked during download of the list
            Thread fetchResourceListThread = new Thread(FetchResourceList);
            fetchResourceListThread.IsBackground = true;
            fetchResourceListThread.Start();
        }

        /// <summary>
        /// Shows a window for creating a new Resource
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateNewResourceButton_Click(object sender, RoutedEventArgs e)
        {
            CreateNewResourceWindow createNewResourceWindow = new CreateNewResourceWindow();
            createNewResourceWindow.MainWindow = MainWindow;
            createNewResourceWindow.ShowDialog();
            //we fetch the Resource list in a separate thread, thus, the ui is not blocked during download of the list
            Thread fetchResourceListThread = new Thread(FetchResourceList);
            fetchResourceListThread.IsBackground = true;
            fetchResourceListThread.Start();
        }

        /// <summary>
        /// Switches to the source view showing the datas of the defined resource
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResourceData_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int id = (int)button.CommandParameter;
            MainWindow.ResourceDataManagementView.ResourceId = id;
            MainWindow.ChangeScreen(UiState.ResourceDataManagement);
        }

        /// <summary>
        /// Resets the view
        /// Clears the Resource list
        /// </summary>
        public void Reset()
        {
            Dispatcher.BeginInvoke(new ThreadStart(() =>
            {
                try
                {
                    ResourcesList.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Exception during reset of Resource list: {0}", ex.Message), "Exception");
                }
            }));
        }
    }
}