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
    public partial class CreateNewUserWindow : Window
    {
        public MainWindow MainWindow { get; set; }

        private Configuration Config = Configuration.GetConfiguration();

        public CreateNewUserWindow()
        {
            InitializeComponent();
            ResizeMode = ResizeMode.NoResize;
        }

        /// <summary>
        /// Tries to create a new user/developer with the data entered in the UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string firstname = FirstnameTextBox.Text;
            string lastname = LastnameTextBox.Text;
            string email = EmailTextBox.Text;
            bool isAdmin = IsAdminCheckBox.IsChecked == true;
            string password = PasswordPasswordBox.Password;
            string confirmPassword = ConfirmPasswordPasswordBox.Password;

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Please enter a username", "Username missing");
                return;
            }
            if (string.IsNullOrEmpty(firstname))
            {
                MessageBox.Show("Please enter a firstname", "Username missing");
                return;
            }
            if (string.IsNullOrEmpty(lastname))
            {
                MessageBox.Show("Please enter a lastname", "Username missing");
                return;
            }
            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Please enter a email", "Email missing");
                return;
            }
            if (!IsValidEmail(email))
            {
                MessageBox.Show("Please enter a valid email", "Invalid email");
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter a password", "Password missing");
                return;
            }
            if (string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Please confirm password", "Password confirmation missing");
                return;
            }
            if (password != confirmPassword)
            {
                MessageBox.Show("Entered passwords are not equal", "Passwords are not equal");
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

                Developer developer = new Developer();
                developer.Username = username;
                developer.Firstname = firstname;
                developer.Lastname = lastname;
                developer.Email = email;
                developer.Password = password;
                developer.IsAdmin = isAdmin;
                
                DataModificationOrRequestResult result = client.CreateDeveloper(developer);
                client.Disconnect();

                if (result.Success)
                {
                    MessageBox.Show("Successfully created a new developer", "Developer created");
                    Close();
                }
                else
                {
                    MessageBox.Show(string.Format("Could not create new developer: {0}", result.Message), "Creation not possible");
                }                                
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Exception during creation of new developer: {0}", ex.Message), "Exception");
            }         
        }

        /// <summary>
        /// Checks, if the given emailaddress is valid
        /// </summary>
        /// <param name="emailaddress"></param>
        /// <returns></returns>
        public bool IsValidEmail(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
