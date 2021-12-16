/*
   Copyright 2019 Nils Kopal <Nils.Kopal<AT>Uni-Kassel.de>

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
using CrypTool.PluginBase;
using CrypToolStoreLib.Client;
using CrypToolStoreLib.DataObjects;
using CrypToolStoreLib.Tools;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Windows;

namespace CrypTool.CrypToolStore
{
    public partial class DownloadResourceDataFileWindow : Window
    {
        private int ResourceId { get; set; }
        private int ResourceVersion { get; set; }
        private IPlugin Plugin { get; set; }

        public string Path { get; set; }

        private bool Stop = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public DownloadResourceDataFileWindow(int resourceId, int resourceVersion, IPlugin plugin, string description)
        {
            InitializeComponent();
            ResizeMode = ResizeMode.NoResize;
            ResourceId = resourceId;
            ResourceVersion = resourceVersion;
            Plugin = plugin;
            DescriptionText.Text = description;
            Closing += DownloadResourceDataFileWindow_Closing;
            Title = string.Format("A {0} component requested to download a resource file", plugin.GetType().Name);
            Path = null;
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
        /// Tries to download a resource
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {

            //we fetch the source list in a separate thread, thus, the ui is not blocked during download of the list
            Thread uploadSourceZipFileThread = new Thread(DownloadResourceZipFile)
            {
                IsBackground = true
            };
            uploadSourceZipFileThread.Start();

            DownloadButton.IsEnabled = false;
        }

        /// <summary>
        /// Closes the window and aborts the download
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AbortButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Downloads the selected file
        /// stops, if the window is closed
        /// </summary>
        private void DownloadResourceZipFile()
        {
            try
            {
                CrypToolStoreClient client = new CrypToolStoreClient
                {
                    ServerCertificate = new X509Certificate2(Properties.Resources.CTStoreTLS),
                    ServerAddress = Constants.ServerAddress,
                    ServerPort = Constants.ServerPort
                };
                client.Connect();

                ResourceData resourceData = new ResourceData
                {
                    ResourceId = ResourceId,
                    ResourceVersion = ResourceVersion
                };
                //delete all old versions including the current version
                for (int i = 0; i <= ResourceVersion; i++)
                {
                    string dir = System.IO.Path.Combine(ResourceHelper.GetResourcesFolder(), string.Format("resource-{0}-{1}", ResourceId, i));
                    if (Directory.Exists(dir))
                    {
                        Directory.Delete(dir, true);
                    }
                }
                string filename = System.IO.Path.Combine(ResourceHelper.GetResourcesFolder(), string.Format("resource-{0}-{1}", ResourceId, ResourceVersion));
                Directory.CreateDirectory(filename);
                filename = System.IO.Path.Combine(filename, string.Format("Resource-{0}-{1}.bin", ResourceId, ResourceVersion));
                client.UploadDownloadProgressChanged += client_UploadDownloadProgressChanged;
                DataModificationOrRequestResult result = client.DownloadResourceDataFile(resourceData, filename, ref Stop);
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
                        catch (Exception)
                        {
                            //wtf?
                        }
                    }));
                    MessageBox.Show("Successfully downloaded resource file", "Resource file downloaded");
                    Path = filename;
                    Dispatcher.BeginInvoke(new ThreadStart(() =>
                    {
                        try
                        {
                            Close();
                        }
                        catch (Exception)
                        {
                            //wtf?
                        }
                    }));
                }
                else
                {
                    if (result.Message != "USERSTOP")
                    {
                        MessageBox.Show(string.Format("Could not download resource file: {0}", result.Message), "Resource file download not possible");
                    }
                    try
                    {
                        string dir = System.IO.Path.Combine(ResourceHelper.GetResourcesFolder(), string.Format("resource-{0}-{1}", ResourceId, ResourceVersion));
                        if (Directory.Exists(dir))
                        {
                            Directory.Delete(dir, true);
                        }
                    }
                    catch (Exception)
                    {
                        //wtf?
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Exception during download of resource file: {0}", ex.Message), "Exception");

                try
                {
                    string dir = System.IO.Path.Combine(ResourceHelper.GetResourcesFolder(), string.Format("resource-{0}-{1}", ResourceId, ResourceVersion));
                    if (Directory.Exists(dir))
                    {
                        Directory.Delete(dir, true);
                    }
                }
                catch (Exception)
                {
                    //wtf?
                }
            }

            Dispatcher.BeginInvoke(new ThreadStart(() =>
            {
                try
                {
                    DownloadButton.IsEnabled = true;
                }
                catch (Exception)
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
                catch (Exception)
                {
                    //wtf?
                }
            }));
        }
    }
}
