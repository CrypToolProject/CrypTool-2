/*
   Copyright 2019 Nils Kopal <Nils<AT>kopaldev.de>

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
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using VoluntLib2;
using System.Security.Cryptography.X509Certificates;
using VoluntLib2.Tools;
using VoluntLib2.ManagementLayer;

namespace WellKnownPeer
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private VoluntLib _VoluntLib = new VoluntLib();
        private ObservableCollection<LogEntry> _Logs = new ObservableCollection<LogEntry>();
        private FileLogger _FileLogger = new FileLogger();

        public MainWindow()
        {
            InitializeComponent();            
            Closing += MainWindow_Closing;
            try
            {
                Logger.SetLogLevel(Logtype.Info);
                Logger.GetLogger().LoggOccured += MainWindow_LoggOccured;                

                X509Certificate2 rootCA = new X509Certificate2(Properties.Settings.Default.RootCertificate);
                X509Certificate2 ownKey = new X509Certificate2(Properties.Settings.Default.OwnKey, Properties.Settings.Default.OwnPassword);

                //we already init certificate service since it clears the lists of admin and revoced certificates
                CertificateService.GetCertificateService().Init(rootCA, ownKey);                

                _VoluntLib = new VoluntLib() { LocalStoragePath = "Jobs" };

                var wellKnownPeers = Properties.Settings.Default.WellKnownPeers.Split(';');
                if (wellKnownPeers.Length != 0)
                {                    
                    foreach (string wellKnownPeer in wellKnownPeers)
                    {
                        if(string.IsNullOrEmpty(wellKnownPeer)  || string.IsNullOrWhiteSpace(wellKnownPeer))
                        {
                            continue;
                        }
                        Logger.GetLogger().LogText(string.Format("WellKnownPeer: {0}", wellKnownPeer), this, Logtype.Info);
                        _VoluntLib.WellKnownPeers.Add(wellKnownPeer);
                    }
                }
                       
                var administrators = Properties.Settings.Default.Administrators.Split(';');
                if (administrators.Length != 0)
                {                                        
                    foreach(string administrator in administrators)
                    {
                        if (string.IsNullOrEmpty(administrator) || string.IsNullOrWhiteSpace(administrator))
                        {
                            continue;
                        }
                        Logger.GetLogger().LogText(string.Format("Administrator: {0}", administrator), this, Logtype.Info);
                        CertificateService.GetCertificateService().AdminCertificateList.Add(administrator);
                    }
                }

                var bannedCertificates = Properties.Settings.Default.BannedCertificates.Split(';');
                if (bannedCertificates.Length != 0)
                {                    
                    foreach (string bannedCertificate in bannedCertificates)
                    {
                        if (string.IsNullOrEmpty(bannedCertificate) || string.IsNullOrWhiteSpace(bannedCertificate))
                        {
                            continue;
                        }
                        Logger.GetLogger().LogText(string.Format("Banned certificate: {0}", bannedCertificate), this, Logtype.Info);
                        CertificateService.GetCertificateService().BannedCertificateList.Add(bannedCertificate);
                    }

                }

                LogListView.DataContext = _Logs;
                _VoluntLib.Start(rootCA, ownKey, 10000, false);

                ContactListView.DataContext = _VoluntLib.GetContactList();
                JobList.DataContext = _VoluntLib.GetJoblist();

                byte[] peerId = _VoluntLib.GetPeerId();
                string id = BitConverter.ToString(peerId);
                if (Application.Current != null)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        try
                        {
                            MyPeerID.Content = "My Peer ID: " + id;
                        }
                        catch (Exception)
                        {
                            //wtf?
                        }
                    }));
                }
                else
                {
                    MyPeerID.Content = "My Peer ID: " + id;
                }
                Logger.GetLogger().LogText(string.Format("My Peer ID:: {0}", id), this, Logtype.Info);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception during startup of WellKnownPeer: " + ex.Message, "Exception during startup!", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Environment.Exit(-1);
            }

        }

        void MainWindow_LoggOccured(object sender, LogEventArgs logEventArgs)
        {
            //first, write to logfile
            _FileLogger.OnLoggOccured(sender, logEventArgs);

            try
            {
                LogEntry entry = new LogEntry();
                entry.LogTime = DateTime.Now.ToString();
                entry.LogType = logEventArgs.Logtype.ToString();
                entry.Message = logEventArgs.Message;
                if (Application.Current != null)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        try
                        {
                            _Logs.Add(entry);
                            if (_Logs.Count > 100)
                            {
                                _Logs.RemoveAt(0);
                            }
                            LogListView.ScrollIntoView(LogListView.Items[LogListView.Items.Count - 1]);
                        }
                        catch (Exception)
                        {
                            //wtf?
                        }
                    }));
                }
                else
                {
                    _Logs.Add(entry);
                    if (_Logs.Count > 100)
                    {
                        _Logs.RemoveAt(0);
                    }
                    LogListView.ScrollIntoView(LogListView.Items[LogListView.Items.Count - 1]);
                }
            }catch(Exception)
            {
                //wtf;
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                _VoluntLib.Stop();
            }
            catch (Exception)
            {
                //wtf?
            }
        }

        /// <summary>
        /// Shows details of the selected job
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void JobList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            JobViewer viewer = new JobViewer();
            viewer.Job = (Job)JobList.SelectedItem;
            viewer.ShowDialog();
        }
    }

    public class LogEntry
    {
        public string LogTime { get; set; }
        public string LogType { get; set; }
        public string Message { get; set; }
    }
}
