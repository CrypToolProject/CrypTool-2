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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CrypToolStoreDeveloperClient.Views
{
    /// <summary>
    /// Interaktionslogik für LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {
        public MainWindow MainWindow { get; set; }

        private Configuration Config = Configuration.GetConfiguration();

        public LoginView()
        {
            InitializeComponent();
            Loaded += LoginView_Loaded;            
        }

        /// <summary>
        /// At startup we focus on username text input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoginView_Loaded(object sender, RoutedEventArgs e)
        {
            FocusOnUsername();
        }

        /// <summary>
        /// Trys a "test login", if it succeeded, it forwards to main menu
        /// and stores username and password
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            lock (this)
            {
                try
                {
                    if (MainWindow.IsLoggedIn == true)
                    {
                        return;
                    }

                    if (string.IsNullOrEmpty(Username.Text))
                    {
                        MessageBox.Show("You have to enter a username to login.", "No username entered");
                        return;
                    }
                    if (string.IsNullOrEmpty(Password.Password))
                    {
                        MessageBox.Show("You have to enter a password to login.", "No username entered");
                        return;
                    }

                    CrypToolStoreClient client = new CrypToolStoreClient();
                    client.ServerCertificate = MainWindow.ServerCertificate;
                    client.ServerAddress = Config.GetConfigEntry("ServerAddress");
                    client.ServerPort = int.Parse(Config.GetConfigEntry("ServerPort"));
                    client.Connect();
                    //just a test login, to verify username and password
                    bool authenticated = client.Login(Username.Text, Password.Password);
                    client.Disconnect();

                    if (authenticated)
                    {
                        //store everything for later requests in ui
                        MainWindow.IsLoggedIn = true;
                        MainWindow.Username = Username.Text;
                        MainWindow.Password = Password.Password;
                        MainWindow.HeaderView.Username = Username.Text;
                        if (client.IsAdmin)
                        {
                            MainWindow.IsAdmin = true;
                        }
                        //remove everything from UI for later logout; thus, the inputs are empty for a new login
                        Username.Text = string.Empty;
                        Password.Password = string.Empty;                        
                        ((MainWindow)((Grid)this.Parent).Parent).ChangeScreen(UiState.MainMenu);                        
                    }
                    else
                    {
                        MainWindow.IsLoggedIn = false;
                        MessageBox.Show("Username or password wrong", "Login failed");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Exception during login: {0}", ex.Message), "Login failed");
                }
            }
        }

        /// <summary>
        /// Just for convenience; if the user enters a return in password field, it behaves like a click
        /// on the login button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Password_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                LoginButton_Click(sender, e);
            }
        }

        /// <summary>
        /// Sets the focus on the username text input
        /// </summary>
        public void FocusOnUsername()
        {
            Username.Focus();
        }
    }
}
