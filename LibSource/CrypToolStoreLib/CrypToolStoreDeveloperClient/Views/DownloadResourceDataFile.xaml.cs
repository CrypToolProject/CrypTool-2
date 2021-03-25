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
    /// Interaktionslogik für DownloadResourceDataFileWindow.xaml
    /// </summary>
    public partial class DownloadResourceDataFileWindow : Window
    {
        public MainWindow MainWindow { get; set; }

        private Configuration Config = Configuration.GetConfiguration();

        private int ResourceId { get; set; }
        private int ResourceVersion { get; set; }

        private bool Stop = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public DownloadResourceDataFileWindow(int pluginid, int pluginversion)
        {
            InitializeComponent();
            ResizeMode = ResizeMode.NoResize;
            ResourceId = pluginid;
            ResourceVersion = pluginversion;
            Closing += DownloadResourceDataFileWindow_Closing;
            this.Title = string.Format("Downoad ResourceData File: ResourceData-{0}-{1}.bin", pluginid, pluginversion);
        }

        /// <summary>
        /// When the window is closed, it sets "Stop" to true
        /// Then, if a download is currently running, it is automatically stopped
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadResourceDataFileWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Stop = true;
        }

        /// <summary>
        /// Tries to download a zip
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {

            //we fetch the source list in a separate thread, thus, the ui is not blocked during download of the list
            Thread UploadSourceZipFileThread = new Thread(DownloadAssembyZipFile);
            UploadSourceZipFileThread.IsBackground = true;
            UploadSourceZipFileThread.Start();

            DownloadButton.IsEnabled = false;
        }

        /// <summary>
        /// Downloads the selected zip file
        /// stops, if the window is closed
        /// </summary>
        private void DownloadAssembyZipFile()
        {
            try
            {
                CrypToolStoreClient client = new CrypToolStoreClient();
                client.ServerCertificate = MainWindow.ServerCertificate;
                client.ServerAddress = Config.GetConfigEntry("ServerAddress");
                client.ServerPort = int.Parse(Config.GetConfigEntry("ServerPort"));
                client.Connect();
                client.Login(MainWindow.Username, MainWindow.Password);

                ResourceData resourceData = new ResourceData();
                resourceData.ResourceId = ResourceId;
                resourceData.ResourceVersion = ResourceVersion;

                string filename = "ResourceData-" + ResourceId + "-" + ResourceVersion + ".bin";

                client.UploadDownloadProgressChanged += client_UploadDownloadProgressChanged;
                DataModificationOrRequestResult result = client.DownloadResourceDataFile(resourceData, Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + filename, ref Stop);

                client.Disconnect();

                if (result.Success)
                {
                    Dispatcher.BeginInvoke(new ThreadStart(() =>
                    {
                        try
                        {
                            ProgressBar.Maximum = 1;
                            ProgressBar.Value = 1;
                            ProgressText.Text = "100 %";
                        }
                        catch (Exception ex)
                        {
                            //wtf?
                        }
                    }));
                    MessageBox.Show("Successfully download ResourceData file", "ResourceData file downloaded");
                    Dispatcher.BeginInvoke(new ThreadStart(() =>
                    {
                        try
                        {
                            Close();
                        }
                        catch (Exception ex)
                        {
                            //wtf?
                        }
                    }));
                }
                else
                {
                    if (result.Message != "USERSTOP")
                    {
                        MessageBox.Show(string.Format("Could not download ResourceData file: {0}", result.Message), "ResourceData file download not possible");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Exception during download of ResourceData zip file: {0}", ex.Message), "Exception");
            }

            Dispatcher.BeginInvoke(new ThreadStart(() =>
            {
                try
                {
                    DownloadButton.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    //wtf?
                }
            }));
        }

        /// <summary>
        /// Updates progress bar every second
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_UploadDownloadProgressChanged(object sender, UploadDownloadProgressEventArgs e)
        {
            Dispatcher.Invoke(new ThreadStart(() =>
            {
                try
                {
                    ProgressBar.Maximum = e.FileSize;
                    ProgressBar.Value = e.DownloadedUploaded;
                    double progress = e.DownloadedUploaded / (double)e.FileSize * 100;

                    ProgressText.Text = Math.Round(progress, 2) + " % (" + Tools.FormatSpeedString(e.BytePerSecond) + " - " + Tools.RemainingTime(e.BytePerSecond, e.FileSize, e.DownloadedUploaded) + ")";
                }
                catch (Exception ex)
                {
                    //wtf?
                }
            }));
        }
    }
}
