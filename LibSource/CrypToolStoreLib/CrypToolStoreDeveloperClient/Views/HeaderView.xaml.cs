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
    /// Interaktionslogik für HeaderView.xaml
    /// </summary>
    public partial class HeaderView : UserControl, INotifyPropertyChanged
    {
        private string username;
        private string uititel;

        public MainWindow MainWindow { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public String Username
        {
            get
            {
                return username;
            }
            set
            {
                if (value != username)
                {
                    username = value;
                    OnPropertyChanged("Username");
                }
            }
        }

        public String UiTitel
        {
            get
            {
                return uititel;
            }
            set
            {
                if (value != uititel)
                {
                    uititel = value;
                    OnPropertyChanged("UiTitel");
                }
            }
        }

        public HeaderView()
        {
            InitializeComponent();
            IsVisibleChanged += HeaderView_IsVisibleChanged;
        }

        /// <summary>
        /// Shows, depending on the view, the back button in the header
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HeaderView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (MainWindow.UiState == UiState.MainMenu)
            {
                BackButtonRow.Height = new GridLength(0);
                BackButton.Visibility = Visibility.Hidden;
            }
            else
            {
                BackButtonRow.Height = new GridLength(50);
                BackButton.Visibility = Visibility.Visible;
            }
        }
      
        /// <summary>
        /// Changes back to previous window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.UiState == UiState.SourceManagement)
            {
                MainWindow.ChangeScreen(UiState.PluginManagement);
            }
            else if (MainWindow.UiState == UiState.ResourceDataManagement)
            {
                MainWindow.ChangeScreen(UiState.ResourceManagement);
            }
            else
            {
                MainWindow.ChangeScreen(UiState.MainMenu);
            }
        }        

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

    }
}
