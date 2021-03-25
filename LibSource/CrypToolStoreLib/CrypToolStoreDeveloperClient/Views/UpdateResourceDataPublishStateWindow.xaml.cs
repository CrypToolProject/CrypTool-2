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
using System.Linq;
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
    /// Interaktionslogik für UpdateResourceDataPublishState.xaml
    /// </summary>
    public partial class UpdateResourceDataPublishStateWindow : Window
    {
        private Configuration Config = Configuration.GetConfiguration();
        
        private int ResourceId 
        {
            get;
            set;
        }

        private int ResourceVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Reference to main window
        /// </summary>
        public MainWindow MainWindow
        {
            get;
            set;
        }

        public UpdateResourceDataPublishStateWindow(int pluginId, int pluginVersion, string oldState)
        {
            InitializeComponent();
            ResourceId = pluginId;
            ResourceVersion = pluginVersion;

            //Set to old state
            switch (oldState)
            {
                default:
                case "NOTPUBLISHED":
                    PublishStateComboBox.SelectedIndex = 0;
                    break;
                case "DEVELOPER":
                    PublishStateComboBox.SelectedIndex = 1;
                    break;
                case "NIGHTLY":
                    PublishStateComboBox.SelectedIndex = 2;
                    break;
                case "BETA":
                    PublishStateComboBox.SelectedIndex = 3;
                    break;
                case "RELEASE":
                    PublishStateComboBox.SelectedIndex = 4;
                    break;                
            }
            this.Title = string.Format("Update Publish State of ResourceData-{0}-{1}", ResourceId, ResourceVersion);
        }

        /// <summary>
        /// Updates the publish state of a given resourcedata
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PublishState publishState;
                switch ((string)((ComboBoxItem)PublishStateComboBox.SelectedValue).Content)
                {                    
                    case "Developer":
                        publishState = PublishState.DEVELOPER;
                        break;
                    case "Nightly":
                        publishState = PublishState.NIGHTLY;
                        break;
                    case "Beta":
                        publishState = PublishState.BETA;
                        break;
                    case "Release":
                        publishState = PublishState.RELEASE;
                        break;
                    default:
                    case "Not published":
                        publishState = PublishState.NOTPUBLISHED;
                        break;                    
                }

                CrypToolStoreClient client = new CrypToolStoreClient();
                client.ServerCertificate = MainWindow.ServerCertificate;
                client.ServerAddress = Config.GetConfigEntry("ServerAddress");
                client.ServerPort = int.Parse(Config.GetConfigEntry("ServerPort"));
                client.Connect();
                client.Login(MainWindow.Username, MainWindow.Password);
                DataModificationOrRequestResult result = client.UpdateResourceDataPublishState(new ResourceData() { ResourceId = ResourceId, ResourceVersion = ResourceVersion }, publishState);
                client.Disconnect();

                if (result.Success)
                {
                    MessageBox.Show("Successfully updated resourcedata's publish state", "Source updated");
                    Close();
                }
                else
                {
                    MessageBox.Show(string.Format("Could not update: {0}", result.Message), "Update not possible");
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Exception during update of resourcedata's publish state: {0}", ex.Message), "Exception");
            }         
        }
    }
}
