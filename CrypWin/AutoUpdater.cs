/*
   Copyright 2008-2013 CrypTool Team

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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;
using CrypTool.CrypWin.Properties;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Attributes;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using Microsoft.Win32;

namespace CrypTool.CrypWin
{
    class AutoUpdater
    {
        #region Fields and properties
        public delegate void UpdaterStateChangedHandler(State newStatus);
        public delegate void UpdateDownloadProgressChangedHandler(int progress);
        public event UpdaterStateChangedHandler OnUpdaterStateChanged;
        public event UpdateDownloadProgressChangedHandler OnUpdateDownloadProgressChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public enum State { Idle, Checking, UpdateAvailable, Downloading, UpdateReady };
            
        private static AutoUpdater autoUpdater = null;        
        private const string XmlPath = "https://www.cryptool.org/ct2download/Builds/CT2_Versions.xml";
        private readonly string TempPath = DirectoryHelper.DirectoryLocalTemp;


        private XElement onlineUpdateVersions;
        private Version onlineUpdateVersion = new Version();
        private Version currentlyRunningVersion = AssemblyHelper.Version;
        private bool serverAvailable = true;
        private string serverNotAvailableMessage;
        private WebClient wc;
        /// <summary>
        /// Changed URI to ct2 trac subdomain to allow nightly builds show changelogs again
        /// </summary>
        private string changelogTemplate = "https://trac.ct2.cryptool.org/log/trunk?format=rss&action=stop_on_copy&mode=stop_on_copy&rev=§&stop_rev=$"; //§ new $ current build
        private string changelog;
        private string changelogText;
        private string updateName;
        private System.Timers.Timer checkTimer = new System.Timers.Timer(1000 * 60 * Settings.Default.CheckInterval);
        private System.Timers.Timer progressTimer;

        private DateTime lastTime = DateTime.Now;
        private DateTime lastGuiUpdateTime = DateTime.Now;
        double bytesReceived = 0;

        private int downloadRetry = 0;

        private State currentState = State.Idle;

        public State CurrentState
        {
            get { return currentState; }
            private set 
            {
                if (currentState != value)
                {
                    currentState = value;
                    OnUpdaterStateChanged(currentState);
                    AutoUpdater_OnUpdaterStatusChanged(currentState);
                }
            }
        }

        public string FilePath
        {
            get
            {
                switch (AssemblyHelper.InstallationType)
                {
                    case Ct2InstallationType.MSI:
                        return Path.Combine(TempPath, "CT2Update.msi");
                    case Ct2InstallationType.NSIS:
                        return Path.Combine(TempPath, "CT2Update.exe");
                    //case Ct2InstallationType.Developer: // uncomment this for letting the updater succeed with the developer version - NOTE: after a successfull (test) update, you must clean your build directory!
                    case Ct2InstallationType.ZIP:
                        return Path.Combine(TempPath, "CT2Update.zip");
                    default:
                        return null;
                }
            }
        }

        public string FilePathTemporary
        {
            get
            {
                return Path.Combine(TempPath, "CT2Update.part");
            }
        }

        public bool IsUpdateReady
        {
            get
            {
                return File.Exists(FilePath);
            }
        }

        #endregion

        #region Implementation

        public static AutoUpdater GetSingleton()
        {
            if (autoUpdater == null)
                autoUpdater = new AutoUpdater();

            return autoUpdater;
        }

        private AutoUpdater()
        {
            // show only changes newer than the current version
            changelogTemplate = changelogTemplate.Replace("$", (currentlyRunningVersion.Build + 1).ToString()); 
            
            // listen for system suspend/resume
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        }

        private string GetBuildTypeXmlString()
        {
            switch (AssemblyHelper.BuildType)
            {
                case Ct2BuildType.Developer:
                    // for testing/simulating autoupdater with the developer edition use nightly, beta or stable here
                    return "nightly";
                case Ct2BuildType.Nightly:
                    return "nightly";
                case Ct2BuildType.Beta:
                    return "beta";
                case Ct2BuildType.Stable:
                    return "stable";
                default:
                    return null;
            }
        }        

        private string GetUserAgentString(char checkingReference)
        {
            string arch = Environment.Is64BitOperatingSystem ? "x64" : "x86";
            return "CrypTool/" + AssemblyHelper.Version + " (" + checkingReference + "; " + CultureInfo.CurrentUICulture.Name + "; "
                                    + AssemblyHelper.BuildType + "; " + Environment.OSVersion.ToString() + "; "
                                    + Environment.OSVersion.Platform + "; " + arch + "; .NET/" + Environment.Version + ")";
        }

        private void AutoUpdater_OnUpdaterStatusChanged(AutoUpdater.State newStatus)
        {
            UpdaterPresentation.GetSingleton().Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                                        {
                                                            UpdaterStateChanged(newStatus);
                                                        }, null);
            if (newStatus == State.UpdateAvailable && Settings.Default.AutoDownload)
            {
                if (downloadRetry < 3)
                {
                    Download();
                    downloadRetry++;
                }
                else
                    GuiLogMessage("AutoUpdate: Auto download failed, try again later.", NotificationLevel.Warning);
            }
        }

        private void UpdaterStateChanged(State newStatus)
        {
            try
            {
                var presentation = UpdaterPresentation.GetSingleton();

                switch (newStatus)
                {
                    case State.Idle:
                        presentation.updateButton.Visibility = Visibility.Visible;
                        presentation.updateButton.IsEnabled = true;
                        presentation.updateButton.Content = Properties.Resources.Check_for_updates_now;
                        presentation.label1.Content = serverAvailable ?
                            Properties.Resources.You_have_currently_the_latest_version_installed_ :
                            string.Format(Properties.Resources.Checking_failed__cannot_contact_server__0, serverNotAvailableMessage);
                        presentation.smallRightImage.Visibility = Visibility.Collapsed;
                        break;
                    case State.Checking:
                        presentation.updateButton.Visibility = Visibility.Collapsed;
                        presentation.updateButton.IsEnabled = false;
                        presentation.label1.Content = Properties.Resources.Checking_for_updates___;
                        break;
                    case State.UpdateAvailable:
                        presentation.updateButton.IsEnabled = true;
                        presentation.updateButton.Content = Properties.Resources.Download_update_now;
                        presentation.label1.Content = (updateName == null) ?
                            Properties.Resources.Update_available_ :
                            string.Format(Properties.Resources.Update_available___0, updateName);
                        presentation.updateButton.Visibility = Visibility.Visible;
                        presentation.progressBar1.Visibility = Visibility.Collapsed;
                        presentation.text.Visibility = Visibility.Collapsed;
                        presentation.smallRightImage.Source = (ImageSource)presentation.FindResource("Update");
                        presentation.smallRightImage.Visibility = Visibility.Visible;
                        presentation.ChangelogBorder.Visibility = Visibility.Visible;
                        FillRSS(presentation);
                        break;
                    case State.Downloading:
                        presentation.updateButton.IsEnabled = false;
                        presentation.updateButton.Visibility = Visibility.Collapsed;
                        presentation.progressBar1.Visibility = Visibility.Visible;
                        presentation.text.Visibility = Visibility.Visible;
                        presentation.label1.Content = (updateName == null) ?
                            Properties.Resources.Downloading_update___ :
                            string.Format(Properties.Resources.Downloading_update___0_____, updateName);
                        presentation.smallRightImage.Source = (ImageSource)presentation.FindResource("Update");
                        presentation.ChangelogBorder.Visibility = Visibility.Visible;
                        presentation.smallRightImage.Visibility = Visibility.Visible;
                        FillRSS(presentation);
                        break;
                    case State.UpdateReady:
                        presentation.updateButton.IsEnabled = true;
                        presentation.updateButton.Content = Properties.Resources.Restart_and_install_now;
                        presentation.updateButton.Visibility = Visibility.Visible;
                        presentation.progressBar1.Visibility = Visibility.Collapsed;
                        presentation.text.Visibility = Visibility.Collapsed;
                        presentation.label1.Content = (updateName == null) ?
                            Properties.Resources.Update_ready_to_install_ :
                            string.Format(Properties.Resources.Update___0___ready_to_install_, updateName);
                        presentation.smallRightImage.Source = (ImageSource)presentation.FindResource("UpdateReady");
                        presentation.ChangelogBorder.Visibility = Visibility.Visible;
                        presentation.smallRightImage.Visibility = Visibility.Visible;
                        FillRSS(presentation);
                        break;
                }
            }
            catch (Exception)
            {
                GuiLogMessage("AutoUpdate: Error occurred while trying to get update information.", NotificationLevel.Warning);
            }
        }

        private void FillRSS(UpdaterPresentation presentation)
        {
            switch (AssemblyHelper.BuildType)
            {
                // for Developer and Nightly fill the presentation with the SVN-changelog
                case Ct2BuildType.Developer:
                case Ct2BuildType.Nightly:
                    if (changelog != null)
                    {
                        presentation.ReadAndFillRSSChangelog(changelog);
                    }
                    break;

                // for Beta and Stable use dedicated text in presentation
                case Ct2BuildType.Beta:
                case Ct2BuildType.Stable:
                    if (changelogText != null)
                    {
                        presentation.FillChangelogText(changelogText);
                    }
                    break;
                
                default:
                // Unknown build type - do nothing.
                    break;
            }
        }

        private void progressTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (!wc.IsBusy)
                {
                    // TODO: how to get the error cause here?
                    GuiLogMessage("AutoUpdate: Download failed.", NotificationLevel.Warning);
                    wc.CancelAsync();
                    progressTimer.Stop();
                    CurrentState = State.UpdateAvailable;
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage("AutoUpdate: Error during download: " + ex.Message, NotificationLevel.Error);
            }
        }

        public void StartCheckTimer()
        {
            try
            {
                checkTimer.Elapsed += new System.Timers.ElapsedEventHandler(delegate
                                                                                {
                                                                                    downloadRetry = 0;
                                                                                    CheckForUpdates('P');
                                                                                });
                checkTimer.Start();
            }
            catch (Exception ex)
            {
                GuiLogMessage("AutoUpdate: Error starting the checking timer: " + ex.Message, NotificationLevel.Error);
            }
        }

        public void BeginCheckingForUpdates(char reference, int waitSecs = 0)
        {
            new Thread(delegate()
                           {
                               if (waitSecs > 0)
                               {
                                   Thread.Sleep(waitSecs * 1000);
                               }
                               CheckForUpdates(reference);
                           }).Start();
        }

        private void CheckForUpdates(char reference)
        {
            try
            {
                CurrentState = State.Checking;

                GuiLogMessage("AutoUpdate: Checking online for new updates ...", NotificationLevel.Info);

                ReadXml(reference); // sets onlineUpdateVersion

                Version downloadedVersion = ReadDownloadedUpdateVersion();

                // if this is the developer version, prepare the rss-changelog with the last 10 entries:
                if (AssemblyHelper.BuildType == Ct2BuildType.Developer)
                {
                    // replace the detected fileversion with a version, which is 10 builds older than the online version in the template for retrieving the rss
                    changelogTemplate = changelogTemplate.Replace((currentlyRunningVersion.Build + 1).ToString(), (onlineUpdateVersion.Build - 10).ToString());
                    //artificially make this version older than the online update, hence an update is always detected
                    currentlyRunningVersion = new Version(2, 0, onlineUpdateVersion.Build - 10);
                }

                if (IsOnlineUpdateAvailable(downloadedVersion))
                {
                    changelog = changelogTemplate.Replace("§", onlineUpdateVersion.Build.ToString());
                    var versionElement = onlineUpdateVersions.Element(GetBuildTypeXmlString());
                    if (versionElement.Element("name") != null)
                    {
                        updateName = versionElement.Element("name").Value;
                    }
                    if (versionElement.Element("changelog") != null)
                    {
                        changelogText = versionElement.Element("changelog").Value;
                    }

                    if (File.Exists(FilePath))
                    {
                        File.Delete(FilePath);
                    }

                    CurrentState = State.UpdateAvailable; // this also starts the download
                    GuiLogMessage("AutoUpdate: New version found online: " + updateName, NotificationLevel.Info);
                    GuiLogMessage("A new version of CrypTool 2 is available: " + updateName, NotificationLevel.Balloon);
                }
                else if (IsUpdateReady) // downloaded update ready
                {
                    if (CurrentState != State.UpdateReady)
                    {
                        if (downloadedVersion > new Version()) // always true with NSIS update
                        {
                            changelog = changelogTemplate.Replace("§", downloadedVersion.Build.ToString());
                            updateName = downloadedVersion.ToString();
                            GuiLogMessage("AutoUpdate: Found already downloaded update ready to install: " + updateName, NotificationLevel.Info);
                        }
                        else
                            // may happen with ZIP update, but is unusual -- ZIP updates are mostly installed at startup or redownloaded again
                        {
                            changelog = null;
                            updateName = null;
                        }

                        CurrentState = State.UpdateReady;
                        
                    }
                }
                else
                {
                    GuiLogMessage("AutoUpdate: No updates found – you are on the most current version.", NotificationLevel.Info);
                    CurrentState = State.Idle;
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage("AutoUpdate: Error occured while checking for updates: " + ex.Message, NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Reads available update version from temporary download location (if anything found).
        /// </summary>
        /// <returns>empty version if nothing found</returns>
        public Version ReadDownloadedUpdateVersion()
        {
            if (!File.Exists(FilePath))
                return new Version();

            var versionInfo = FileVersionInfo.GetVersionInfo(FilePath);
            if (versionInfo.FileMajorPart < 1 && versionInfo.FileMinorPart < 1) // happens with ZIP updates
                return new Version(); // creates Version 0.0 which is greater than 0.0.0.0!
            else
                return new Version(versionInfo.FileMajorPart, versionInfo.FileMinorPart, versionInfo.FileBuildPart, versionInfo.FilePrivatePart);
        }

        private bool IsOnlineUpdateAvailable(Version downloadedVersion)
        {
            return onlineUpdateVersion > currentlyRunningVersion    // newer than running
                && onlineUpdateVersion > downloadedVersion          // newer than downloaded update (always true for ZIP)
                && serverAvailable;                                 // online check succeeded
        }

        private void ReadXml(char userAgentRef)
        {
            WebClient client = null;

            try
            {
                client = new WebClient();
                client.Headers["User-Agent"] = GetUserAgentString(userAgentRef);

                Stream s = client.OpenRead(XmlPath);
                XElement xml = XElement.Load(s);

                onlineUpdateVersions = xml.Element("x64");

                // Retrieve the current version from the server (for nightly, beta and stable)
                Version.TryParse(onlineUpdateVersions.Element(GetBuildTypeXmlString()).Attribute("version").Value, out onlineUpdateVersion);

                if (!serverAvailable)
                {
                    serverAvailable = true;
                    serverNotAvailableMessage = null;
                    GuiLogMessage("AutoUpdate: Checking for updates successful, connection to server available.", NotificationLevel.Debug);
                }

            }
            catch (Exception ex)
            {
                if (serverAvailable)
                {
                    serverAvailable = false;
                }

                serverNotAvailableMessage = ex.Message;
                GuiLogMessage("AutoUpdate: Cannot check for updates: " + ex.Message, NotificationLevel.Warning);
            }
            finally
            {
                if (client != null) client.Dispose();
            }
        }

        public void GuiLogMessage(string message, NotificationLevel loglevel)
        {
            if (OnGuiLogNotificationOccured != null)
                OnGuiLogNotificationOccured(null, new GuiLogEventArgs(message, null, loglevel));
        }

        public void Download()
        {
            try
            {
                progressTimer = new System.Timers.Timer(1000 * 10);

                progressTimer.Elapsed += new System.Timers.ElapsedEventHandler(progressTimer_Elapsed);

                if (!Directory.Exists(TempPath))
                {
                    Directory.CreateDirectory(TempPath);
                }

                wc = new System.Net.WebClient();

                wc.DownloadProgressChanged += new System.Net.DownloadProgressChangedEventHandler(wc_DownloadProgressChanged);
                wc.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(wc_DownloadFileCompleted);

                Uri downloadUri = null;
                switch(AssemblyHelper.InstallationType)
                {
                    case Ct2InstallationType.MSI:
                        downloadUri = new Uri(onlineUpdateVersions.Element(GetBuildTypeXmlString()).Attribute("msidownload").Value);
                        break;
                    case Ct2InstallationType.NSIS:
                        downloadUri = new Uri(onlineUpdateVersions.Element(GetBuildTypeXmlString()).Attribute("nsisdownload").Value);
                        break;
                    case Ct2InstallationType.ZIP:
                        downloadUri = new Uri(onlineUpdateVersions.Element(GetBuildTypeXmlString()).Attribute("zipdownload").Value);
                        break;
                    case Ct2InstallationType.Developer:
                        // For testing dowloads uncomment the next three codelines and comment the 3 codelins afterwards
                        //GuiLogMessage("AutoUpdate: Downloading the ZIP package for testing purpose (this is the developer edition..)", NotificationLevel.Info);
                        //downloadUri = new Uri(onlineUpdateVersions.Element(GetBuildTypeXmlString()).Attribute("zipdownload").Value);
                        //break;

                        GuiLogMessage("AutoUpdate: Not downloading anything for the developer version. For testing the download comment this in AutoUpdates.cs!", NotificationLevel.Info);
                        CurrentState = State.Idle;
                        return;
                    default:
                        GuiLogMessage("AutoUpdate: Unknown installation type ("+AssemblyHelper.InstallationType.ToString()+"). Cannot download appropiate update package.", NotificationLevel.Error);
                        return;
                }

                lastTime = DateTime.Now;
                lastGuiUpdateTime = DateTime.Now;
                bytesReceived = 0;

                wc.DownloadFileAsync(downloadUri, FilePathTemporary);

                CurrentState = State.Downloading;
                
                progressTimer.Start();

                GuiLogMessage("AutoUpdate: Downloading update...", NotificationLevel.Info);

                if (!serverAvailable)
                {
                    serverAvailable = true;
                    serverNotAvailableMessage = null;
                    GuiLogMessage("AutoUpdate: Downloading update... (Retry)", NotificationLevel.Info);
                }
                     
            }
            catch (Exception e)
            {
                if (serverAvailable)
                {
                    serverAvailable = false;
                    serverNotAvailableMessage = e.Message;
                    GuiLogMessage("AutoUpdate: Download failed (" + GetBuildTypeXmlString() + "). " + e.Message, NotificationLevel.Warning);
                }
                CurrentState = State.UpdateAvailable;
            }
        }

        private void wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
                PrepareUpdate();
        }

        private void wc_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            OnUpdateDownloadProgressChanged(e.ProgressPercentage);
            try
            {                
                progressTimer.Stop();                
                UpdaterPresentation.GetSingleton().Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        UpdaterPresentation.GetSingleton().progressBar1.Value = e.ProgressPercentage;                        
                        if (DateTime.Now > lastGuiUpdateTime.AddMilliseconds(750))
                        {
                            double interval = DateTime.Now.Subtract(lastTime).TotalMilliseconds;
                            lastTime = DateTime.Now;
                            double bytes = e.BytesReceived - bytesReceived;
                            bytesReceived = e.BytesReceived;
                            double bytesPerSecond = (bytes / interval ) * 1000.1;
                            
                            if (bytesPerSecond < 1024)
                            {
                                UpdaterPresentation.GetSingleton().text.Text = string.Format("{0:0.00} Bytes/sec", bytesPerSecond);
                            }
                            else if (bytesPerSecond / 1024 < 1024)
                            {
                                UpdaterPresentation.GetSingleton().text.Text = string.Format("{0:0.00} KiB/sec", (bytesPerSecond / 1024.0));
                            }
                            else
                            {
                                UpdaterPresentation.GetSingleton().text.Text = string.Format("{0:0.00} MiB/sec", (bytesPerSecond / (1024.0 * 1024.0)));
                            }
                            lastGuiUpdateTime = DateTime.Now;
                        }                        
                    }
                    catch (Exception ex)
                    {
                        //wtf?
                    }
                }, e.ProgressPercentage);
                if (wc.IsBusy)
                    progressTimer.Start();
            }
            catch (Exception ex)
            {
                GuiLogMessage("Error during download: "+ex.Message, NotificationLevel.Error);
            }
        }

        private void PrepareUpdate()
        {
            try
            {
                progressTimer.Stop();
                string exepath = Assembly.GetExecutingAssembly().Location;
                string exedir = Path.GetDirectoryName(exepath);

                File.Copy(Path.Combine(exedir, "Lib\\Ionic.Zip.Reduced.dll"), Path.Combine(TempPath, "Ionic.Zip.Reduced.dll"), true);
                File.Move(FilePathTemporary, FilePath);

                GuiLogMessage("AutoUpdate: Update ready to install (" + GetBuildTypeXmlString() + ").", NotificationLevel.Info);

                CurrentState = State.UpdateReady;

            }
            catch (Exception)
            {
                GuiLogMessage("AutoUpdate: Cannot prepare update procedure (" + GetBuildTypeXmlString() + ").", NotificationLevel.Error);
                CurrentState = State.UpdateAvailable;
            }
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch(e.Mode)
            {
                case PowerModes.Suspend:
                    checkTimer.Stop(); // avoid timer being triggered at system resume (before network is up)
                    break;
                case PowerModes.Resume:
                    // with periodic intervals > 5 min, also check 120 sec after resume
                    if (Settings.Default.CheckInterval > 5)
                        BeginCheckingForUpdates('R', 120);

                    checkTimer.Start();
                    break;
            }
        }

        #endregion

    }

}
