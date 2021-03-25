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
    /// Interaktionslogik für PluginManagementView.xaml
    /// </summary>
    public partial class PluginManagementView : UserControl
    {        
        public MainWindow MainWindow { get; set; }

        private ObservableCollection<Plugin> Plugins = new ObservableCollection<Plugin>();

        private Configuration Config = Configuration.GetConfiguration();

        public PluginManagementView()
        {
            InitializeComponent();
            PluginsListView.ItemsSource = Plugins;
            Plugins.Clear();
            IsVisibleChanged += PluginManagementView_IsVisibleChanged;
        }

        /// <summary>
        /// Called, when the UI changes to visible starte
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PluginManagementView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsVisible)
            {
                return;
            }

            //we fetch the plugin list in a separate thread, thus, the ui is not blocked during download of the list
            Thread fetchPluginListThread = new Thread(FetchPluginList);
            fetchPluginListThread.IsBackground = true;
            fetchPluginListThread.Start();
            
        }

        /// <summary>
        /// Method requests a plugin list and stores it in the list of the GUI
        /// </summary>
        private void FetchPluginList()
        {
            try
            {
                CrypToolStoreClient client = new CrypToolStoreClient();
                client.ServerCertificate = MainWindow.ServerCertificate;
                client.ServerAddress = Config.GetConfigEntry("ServerAddress");
                client.ServerPort = int.Parse(Config.GetConfigEntry("ServerPort"));
                client.Connect();
                client.Login(MainWindow.Username, MainWindow.Password);
                DataModificationOrRequestResult result = client.GetPluginList(MainWindow.IsAdmin ? "*" :  MainWindow.Username);
                List<Plugin> plugins = (List<Plugin>)result.DataObject;
                client.Disconnect();
                Dispatcher.BeginInvoke(new ThreadStart(() =>
                {
                    try
                    {
                        Plugins.Clear();
                        foreach (Plugin plugin in plugins)
                        {
                            if (plugin.Username == MainWindow.Username || MainWindow.IsAdmin)
                            {
                                Plugins.Add(plugin);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("Exception during adding plugins to list: {0}", ex.Message), "Exception");
                    }
                }));                
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Exception during retrieving list of plugins: {0}", ex.Message), "Exception");
            }         
        }

        /// <summary>
        /// Deletes the plugin defined by the clicked button
        /// Then, updates the plugin list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int id = (int)button.CommandParameter;

            MessageBoxResult messageBoxResult = MessageBox.Show(string.Format("Do you really want to delete the plugin {0}?", id), string.Format("Delete {0}", id), MessageBoxButton.YesNo);

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
                    DataModificationOrRequestResult result = client.DeletePlugin(id);
                    client.Disconnect();

                    if (result.Success)
                    {
                        MessageBox.Show(string.Format("Successfully deleted {0}", id), "Plugin deleted");
                        FetchPluginList();
                    }
                    else
                    {
                        MessageBox.Show(string.Format("Could not delete plugin: {0}", result.Message), "Deletion not possible");
                    }                                        
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Exception during deletion of plugin: {0}", ex.Message), "Exception");
                }         
            }
        }

        /// <summary>
        /// Shows a window for updating a plugin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Update_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int id = (int)button.CommandParameter;
            UpdatePluginWindow updatePluginWindow = new UpdatePluginWindow(id);
            updatePluginWindow.MainWindow = MainWindow;
            updatePluginWindow.ShowDialog();
            //we fetch the plugin list in a separate thread, thus, the ui is not blocked during download of the list
            Thread fetchPluginListThread = new Thread(FetchPluginList);
            fetchPluginListThread.IsBackground = true;
            fetchPluginListThread.Start();
        }

        /// <summary>
        /// Shows a window for creating a new plugin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateNewPluginButton_Click(object sender, RoutedEventArgs e)
        {
            CreateNewPluginWindow createNewPluginWindow = new CreateNewPluginWindow();
            createNewPluginWindow.MainWindow = MainWindow;
            createNewPluginWindow.ShowDialog();
            //we fetch the plugin list in a separate thread, thus, the ui is not blocked during download of the list
            Thread fetchPluginListThread = new Thread(FetchPluginList);
            fetchPluginListThread.IsBackground = true;
            fetchPluginListThread.Start();
        }

        /// <summary>
        /// Switches to the source view showing the sources of the defined plugin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Source_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int id = (int)button.CommandParameter;
            MainWindow.SourceManagementView.PluginId = id;
            MainWindow.ChangeScreen(UiState.SourceManagement);
        }
        
        /// <summary>
        /// Resets the view
        /// Clears the plugin list
        /// </summary>
        public void Reset()
        {
            Dispatcher.BeginInvoke(new ThreadStart(() =>
            {
                try
                {
                    Plugins.Clear();                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Exception during reset of plugin list: {0}", ex.Message), "Exception");
                }
            }));
        }
    }
}