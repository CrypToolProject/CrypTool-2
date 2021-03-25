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
using CrypToolStoreLib.Server;
using CrypToolStoreLib.Tools;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
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
using System.Windows.Shapes;

namespace CrypToolStoreDeveloperClient.Views
{
    /// <summary>
    /// Interaktionslogik für UpdateResourceWindow.xaml
    /// </summary>
    public partial class UpdateResourceWindow : Window
    {
        public MainWindow MainWindow { get; set; }
        private int ResourceId { get; set; }

        private Configuration Config = Configuration.GetConfiguration();

        /// <summary>
        /// Contrucor, needs a Resource id to know which Resource to show
        /// </summary>
        /// <param name="resourceid"></param>
        public UpdateResourceWindow(int resourceid)
        {
            InitializeComponent();
            ResizeMode = ResizeMode.NoResize;
            Loaded += UpdateResourceWindow_Loaded;
            ResourceId = resourceid;
        }

        /// <summary>
        /// Called, when window is loaded; retrieves Resource data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void UpdateResourceWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                CrypToolStoreClient client = new CrypToolStoreClient();
                client.ServerCertificate = MainWindow.ServerCertificate;
                client.ServerAddress = Config.GetConfigEntry("ServerAddress");
                client.ServerPort = int.Parse(Config.GetConfigEntry("ServerPort"));
                client.Connect();
                client.Login(MainWindow.Username, MainWindow.Password);

                DataModificationOrRequestResult result = client.GetResource(ResourceId);
                Resource Resource = (Resource)result.DataObject;
                client.Disconnect();
                if (Resource == null)
                {
                    return;
                }
                Dispatcher.BeginInvoke(new ThreadStart(() =>
                {
                    try
                    {
                        NameTextBox.Text = Resource.Name;
                        DescriptionTextBox.Text = Resource.Description;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("Exception during retrieval of Resource data: {0}", ex.Message), "Exception");
                    }
                }));

            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Exception during retrieval of Resource data: {0}", ex.Message), "Exception");
            }
        }


        /// <summary>
        /// Tries to update a Resource with the data entered in the UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            string username = MainWindow.Username;
            string name = NameTextBox.Text;
            string description = DescriptionTextBox.Text;

            try
            {
                CrypToolStoreClient client = new CrypToolStoreClient();
                client.ServerCertificate = MainWindow.ServerCertificate;
                client.ServerAddress = Config.GetConfigEntry("ServerAddress");
                client.ServerPort = int.Parse(Config.GetConfigEntry("ServerPort"));
                client.Connect();
                client.Login(MainWindow.Username, MainWindow.Password);

                Resource Resource = new Resource();
                Resource.Id = ResourceId;
                Resource.Username = username;
                Resource.Name = name;
                Resource.Description = description;

                DataModificationOrRequestResult result = client.UpdateResource(Resource);
                client.Disconnect();

                if (result.Success)
                {
                    MessageBox.Show("Successfully updated Resource", "Resource updated");
                    Close();
                }
                else
                {
                    MessageBox.Show(string.Format("Could not update: {0}", result.Message), "Update not possible");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Exception during update of Resource: {0}", ex.Message), "Exception");
            }
        }
    }
}