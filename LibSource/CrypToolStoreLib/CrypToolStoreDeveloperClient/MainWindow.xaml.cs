using CrypToolStoreLib.Tools;
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
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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

namespace CrypToolStoreDeveloperClient
{
    public enum UiState
    {
        LoginScreen,
        MainMenu,
        UserManagement,
        PluginManagement,
        SourceManagement,
        ResourceManagement,
        ResourceDataManagement
    }

    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public X509Certificate2 ServerCertificate;
        public string Password;
        public string Username;
        public bool IsLoggedIn;
        public bool IsAdmin;
        public UiState UiState;

        public MainWindow()
        {
            InitializeComponent();

            Username = null;
            Password = null;
            IsLoggedIn = false;
            IsAdmin = false;
           
            ResizeMode = ResizeMode.CanMinimize;

            LoginView.MainWindow = this;
            HeaderView.MainWindow = this;
            MainMenuView.MainWindow = this;
            UserManagementView.MainWindow = this;
            PluginManagementView.MainWindow = this;
            SourceManagementView.MainWindow = this;
            ResourceManagementView.MainWindow = this;
            ResourceDataManagementView.MainWindow = this;

            ChangeScreen(UiState.LoginScreen);
            ServerCertificate = new X509Certificate2(Configuration.GetConfiguration().GetConfigEntry("ServerCertificate"));
        }

        /// <summary>
        /// Changes the current screen that CrypToolStoreDeveloperClient displays
        /// </summary>
        /// <param name="uiState"></param>
        public void ChangeScreen(UiState uiState)
        {
            Reset();
            UiState = uiState;

            HeaderView.Visibility = Visibility.Hidden;
            LoginView.Visibility = Visibility.Hidden;
            MainMenuView.Visibility = Visibility.Hidden;
            UserManagementView.Visibility = Visibility.Hidden;
            PluginManagementView.Visibility = Visibility.Hidden;
            SourceManagementView.Visibility = Visibility.Hidden;
            ResourceManagementView.Visibility = Visibility.Hidden;
            ResourceDataManagementView.Visibility = Visibility.Hidden;

            switch (uiState)
            {
                case UiState.LoginScreen:
                    LoginView.Visibility = Visibility.Visible;
                    Width = 400;
                    Height = 200;
                    LoginView.FocusOnUsername();
                    HeaderView.UiTitel = "Login";
                    break;
                case UiState.MainMenu:                    
                    Width = 400;
                    Height = 400;
                    HeaderView.Visibility = Visibility.Visible;
                    MainMenuView.Visibility = Visibility.Visible;
                    HeaderView.UiTitel = "Main Menu";
                    break;
                case UiState.UserManagement:
                    Width = 900;
                    Height = 600;
                    HeaderView.Visibility = Visibility.Visible;
                    UserManagementView.Visibility = Visibility.Visible;
                    HeaderView.UiTitel = "User Management";
                    break;
                case UiState.PluginManagement:
                    Width = 1100;
                    Height = 600;
                    HeaderView.Visibility = Visibility.Visible;
                    PluginManagementView.Visibility = Visibility.Visible;
                    HeaderView.UiTitel = "Plugin Management";
                    break;
                case UiState.SourceManagement:
                    Width = 1100;
                    Height = 600;
                    HeaderView.Visibility = Visibility.Visible;
                    SourceManagementView.Visibility = Visibility.Visible;
                    HeaderView.UiTitel = "Source Management";
                    break;
                case UiState.ResourceManagement:
                    Width = 1100;
                    Height = 600;
                    HeaderView.Visibility = Visibility.Visible;
                    ResourceManagementView.Visibility = Visibility.Visible;
                    HeaderView.UiTitel = "Resource Management";
                    break;
                case UiState.ResourceDataManagement:
                    Width = 1100;
                    Height = 600;
                    HeaderView.Visibility = Visibility.Visible;
                    ResourceDataManagementView.Visibility = Visibility.Visible;
                    HeaderView.UiTitel = "Resource Data Management";
                    break;
            }
        }

        /// <summary>
        /// Resets all views by emptying the list of each view
        /// </summary>
        public void Reset()
        {
            UserManagementView.Reset();
            PluginManagementView.Reset();
            SourceManagementView.Reset();
            ResourceManagementView.Reset();
            ResourceDataManagementView.Reset();
        }
    }
}
