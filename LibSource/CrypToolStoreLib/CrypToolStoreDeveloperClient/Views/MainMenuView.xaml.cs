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
    /// Interaktionslogik für MainMenu.xaml
    /// </summary>
    public partial class MainMenuView : UserControl
    {
        public MainWindow MainWindow { get; set; }

        public MainMenuView()
        {
            InitializeComponent();
            IsVisibleChanged += MainMenuView_IsVisibleChanged;
        }

        private void MainMenuView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //Only admins are allowed to edit/view developers
            //thus, the user management is only shown to them
            if (MainWindow.IsAdmin)
            {
                UserManagementRow.Height = new GridLength(50);
                UserManagementButton.Visibility = Visibility.Visible;
            }
            else
            {
                UserManagementRow.Height = new GridLength(0);
                UserManagementButton.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Change screen to User Management when UserManagementButton is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserManagementButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ChangeScreen(UiState.UserManagement);
        }

        /// <summary>
        /// Updates the own user data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateMyDataButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateUserWindow updateUserWindow = new UpdateUserWindow(MainWindow.Username, false);
            updateUserWindow.MainWindow = MainWindow;
            updateUserWindow.ShowDialog();
        }

        /// <summary>
        /// "Logs" out by removing username and password and changing to login screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Username = string.Empty;
            MainWindow.Password = string.Empty;
            MainWindow.IsLoggedIn = false;
            MainWindow.IsAdmin = false;
            MainWindow.ChangeScreen(UiState.LoginScreen);
        }

        /// <summary>
        /// Change screen to Plugin Management when PluginManagementButton is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PluginManagementButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ChangeScreen(UiState.PluginManagement);
        }

        /// <summary>
        /// Change screen to Resource Management when ResourceManagementButton is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResourceManagementButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ChangeScreen(UiState.ResourceManagement);
        }
    }
}
