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
    /// Interaktionslogik für CreateNewUserWindow.xaml
    /// </summary>
    public partial class CreateNewResourceWindow : Window
    {
        public MainWindow MainWindow { get; set; }
        private Configuration Config = Configuration.GetConfiguration();

        /// <summary>
        /// Constructor
        /// </summary>
        public CreateNewResourceWindow()
        {
            InitializeComponent();
            ResizeMode = ResizeMode.NoResize;  
        }

        /// <summary>
        /// Tries to create a new Resource with the data entered in the UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            string username = MainWindow.Username;
            string name = NameTextBox.Text;
            string description = DescriptionTextBox.Text;
          

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter a name", "Name missing");
                return;
            }
            if (string.IsNullOrEmpty(description))
            {
                MessageBox.Show("Please enter a description", "Description missing");
                return;
            }
            
            try
            {
                CrypToolStoreClient client = new CrypToolStoreClient();
                client.ServerCertificate = MainWindow.ServerCertificate;
                client.ServerAddress = Config.GetConfigEntry("ServerAddress");
                client.ServerPort = int.Parse(Config.GetConfigEntry("ServerPort"));
                client.Connect();
                client.Login(MainWindow.Username, MainWindow.Password);

                Resource Resource = new Resource();
                Resource.Username = username;
                Resource.Name = name;
                Resource.Description = description;

                DataModificationOrRequestResult result = client.CreateResource(Resource);
                client.Disconnect();

                if (result.Success)
                {
                    MessageBox.Show("Successfully created a new Resource", "Resource created");
                    Close();
                }
                else
                {
                    MessageBox.Show(string.Format("Could not create new Resource: {0}", result.Message), "Creation not possible");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Exception during creation of new Resource: {0}", ex.Message), "Exception");
            }
        }
    }
}
