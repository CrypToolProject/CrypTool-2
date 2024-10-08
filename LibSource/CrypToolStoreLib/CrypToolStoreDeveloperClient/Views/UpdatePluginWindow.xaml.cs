﻿using CrypToolStoreLib;
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
    /// Interaktionslogik für UpdatePluginWindow.xaml
    /// </summary>
    public partial class UpdatePluginWindow : Window
    {
        public MainWindow MainWindow { get; set; }
        private int PluginId { get; set; }

        private Configuration Config = Configuration.GetConfiguration();

        private byte[] icon;
        private byte[] Icon
        {
            get
            {
                return icon;
            }
            set
            {
                icon = value;
                UpdateIconInUi();
            }
        }

        /// <summary>
        /// Updates the icon image in the ui to show the icon image
        /// </summary>
        private void UpdateIconInUi()
        {
            try
            {
                if (icon == null || icon.Length == 0)
                {
                    return;
                }
                BitmapImage bitmapImage = new BitmapImage();
                using (var mem = new MemoryStream(icon))
                {
                    mem.Position = 0;
                    bitmapImage.BeginInit();
                    bitmapImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.UriSource = null;
                    bitmapImage.StreamSource = mem;
                    bitmapImage.EndInit();
                }
                bitmapImage.Freeze();
                IconImage.Source = bitmapImage;
            }
            catch (Exception)
            {
                //wtf?
            }
        }

        /// <summary>
        /// Contrucor, needs a plugin id to know which plugin to show
        /// </summary>
        /// <param name="pluginid"></param>
        public UpdatePluginWindow(int pluginid)
        {
            InitializeComponent();
            ResizeMode = ResizeMode.NoResize;
            Loaded += UpdatePluginWindow_Loaded;
            PluginId = pluginid;
            Icon = new byte[0];
        }

        /// <summary>
        /// Called, when window is loaded; retrieves plugin data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void UpdatePluginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                CrypToolStoreClient client = new CrypToolStoreClient();
                client.ServerCertificate = MainWindow.ServerCertificate;
                client.ServerAddress = Config.GetConfigEntry("ServerAddress");
                client.ServerPort = int.Parse(Config.GetConfigEntry("ServerPort"));
                client.Connect();
                client.Login(MainWindow.Username, MainWindow.Password);

                DataModificationOrRequestResult result = client.GetPlugin(PluginId);
                Plugin plugin = (Plugin)result.DataObject;
                client.Disconnect();
                if (plugin == null)
                {
                    return;
                }
                Dispatcher.BeginInvoke(new ThreadStart(() =>
                {
                    try
                    {
                        NameTextBox.Text = plugin.Name;
                        ShortDescriptionTextBox.Text = plugin.ShortDescription;
                        LongDescriptionTextBox.Text = plugin.LongDescription;
                        AuthorNamesTextBox.Text = plugin.Authornames;
                        AuthorEmailsTextBox.Text = plugin.Authoremails;
                        AuthorInstitutesTextBox.Text = plugin.Authorinstitutes;
                        Icon = plugin.Icon;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("Exception during retrieval of plugin data: {0}", ex.Message), "Exception");
                    }
                }));

            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Exception during retrieval of plugin data: {0}", ex.Message), "Exception");
            }
        }


        /// <summary>
        /// Tries to update a plugin with the data entered in the UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            string username = MainWindow.Username;
            string name = NameTextBox.Text;
            string shortdescription = ShortDescriptionTextBox.Text;
            string longdescription = LongDescriptionTextBox.Text;
            string authornames = AuthorNamesTextBox.Text;
            string authoremails = AuthorEmailsTextBox.Text;
            string authorinstitutes = AuthorInstitutesTextBox.Text;
         
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter a name", "Name missing");
                return;
            }
            if (string.IsNullOrEmpty(shortdescription))
            {
                MessageBox.Show("Please enter a short description", "Short description missing");
                return;
            }
            if (string.IsNullOrEmpty(longdescription))
            {
                MessageBox.Show("Please enter a long description", "Long description missing");
                return;
            }
            if (string.IsNullOrEmpty(authornames))
            {
                MessageBox.Show("Please enter a author names", "Author names missing");
                return;
            }
            if (string.IsNullOrEmpty(authoremails))
            {
                MessageBox.Show("Please enter a author emails", "Author emails missing");
                return;
            }
            if (string.IsNullOrEmpty(authorinstitutes))
            {
                MessageBox.Show("Please enter a author institutes", "Author institutes missing");
                return;
            }
            string[] emails = authoremails.Split(';');
            foreach (string email in emails)
            {
                if (!IsValidEmail(email))
                {
                    MessageBox.Show(string.Format("Invalid email address: {0}", email), "Invalid email");
                    return;
                }
            }

            try
            {
                CrypToolStoreClient client = new CrypToolStoreClient();
                client.ServerCertificate = MainWindow.ServerCertificate;
                client.ServerAddress = Config.GetConfigEntry("ServerAddress");
                client.ServerPort = int.Parse(Config.GetConfigEntry("ServerPort"));
                client.Connect();
                client.Login(MainWindow.Username, MainWindow.Password);

                Plugin plugin = new Plugin();
                plugin.Id = PluginId;
                plugin.Username = username;
                plugin.Name = name;
                plugin.ShortDescription = shortdescription;
                plugin.LongDescription = longdescription;
                plugin.Authornames = authornames;
                plugin.Authoremails = authoremails;
                plugin.Authorinstitutes = authorinstitutes;
                plugin.Icon = Icon;

                DataModificationOrRequestResult result = client.UpdatePlugin(plugin);
                client.Disconnect();

                if (result.Success)
                {
                    MessageBox.Show("Successfully updated plugin", "Plugin updated");
                    Close();
                }
                else
                {
                    MessageBox.Show(string.Format("Could not update: {0}", result.Message), "Update not possible");
                }                
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Exception during update of plugin: {0}", ex.Message), "Exception");
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

        /// <summary>
        /// Shows an open file dialog to open an icon file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectIconButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Select Icon for the Plugin";
                openFileDialog.Filter = "png files (*.png)|*.png|jpg files (*.jpg)|*.jpg";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                bool? dialogResult = openFileDialog.ShowDialog();
                if (dialogResult == true)
                {
                    byte[] image = File.ReadAllBytes(openFileDialog.FileName);
                    if (image.Length > Constants.CLIENTHANDLER_MAX_ICON_FILE_SIZE)
                    {
                        MessageBox.Show(string.Format("File size of icons can only be less or equal to {0} byte!", Constants.CLIENTHANDLER_MAX_ICON_FILE_SIZE), "Invalid icon file size");
                        return;
                    }
                    Icon = image;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Exception during selecting of icon: {0}", ex.Message), "Exception");
            }
        }
    }
}
