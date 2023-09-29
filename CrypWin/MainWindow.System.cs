/*
   Copyright 2008 - 2022 CrypTool Team

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
using CrypTool.CrypWin.Helper;
using CrypTool.CrypWin.Properties;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Attributes;
using CrypTool.PluginBase.Editor;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Trinet.Core.IO.Ntfs;
using MessageBox = System.Windows.MessageBox;
using PowerLineStatus = System.Windows.Forms.PowerLineStatus;

namespace CrypTool.CrypWin
{
    public partial class MainWindow
    {
        private ProcessPriorityClass oldPriority = Process.GetCurrentProcess().PriorityClass;
        private readonly List<IEditor> runningEditorsOnSuspend = new List<IEditor>();
        private bool runOnBattery = false;

        #region System Events

        private void MainCrypToolWindow_StateChanged(object sender, EventArgs e)
        {
            try
            {
                if (WindowState == WindowState.Minimized)
                {
                    if (closingCausedMinimization)
                    {
                        ShowInTaskbar = false;
                        notifyIcon.ShowBalloonTip(1000 * 5, Properties.Resources.Information, Properties.Resources.CrypTool_2_0_has_been_backgrounded_due_to_running_tasks_, ToolTipIcon.Info);
                        oldPriority = Process.GetCurrentProcess().PriorityClass;
                        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Idle;
                    }
                    closingCausedMinimization = false;
                }
                else
                {
                    oldWindowState = WindowState;
                    ShowInTaskbar = true;
                    Visibility = Visibility.Visible;
                    Process.GetCurrentProcess().PriorityClass = oldPriority;
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Error during StateChange event of CrypTool 2 Window: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Resume:
                    GuiLogMessage("Computer resuming!", NotificationLevel.Info);
                    ReexecuteSuspendedEditors();
                    break;

                case PowerModes.Suspend:
                    GuiLogMessage("Computer suspending!", NotificationLevel.Info);

                    //Stop all workspaces:
                    runningEditorsOnSuspend.Clear();
                    foreach (IEditor editor in editorToFileMap.Keys)
                    {
                        if (editor.CanStop)
                        {
                            editor.Stop();
                            runningEditorsOnSuspend.Add(editor);
                        }
                    }
                    break;

                case PowerModes.StatusChange:
                    if (Settings.Default.StopWorkspaces)
                    {
                        bool isRunningOnBattery = (System.Windows.Forms.SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline);
                        if (isRunningOnBattery)
                        {
                            if (!runOnBattery)
                            {
                                bool stopped = false;
                                foreach (IEditor editor in editorToFileMap.Keys)
                                {
                                    if (editor.CanStop)
                                    {
                                        stopped = true;
                                        editor.Stop();
                                    }
                                }
                                if (stopped)
                                {
                                    GuiLogMessage("Power mode offline: Stopped workspaces to save power.", NotificationLevel.Info);
                                }

                                runOnBattery = true;
                            }
                        }
                        else
                        {
                            runOnBattery = false;
                        }
                    }
                    break;
            }
        }

        private void ReexecuteSuspendedEditors()
        {
            foreach (IEditor editor in runningEditorsOnSuspend)
            {
                if (editor.CanExecute)
                {
                    editor.Execute();
                }
            }
        }

        private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            shutdown = true;
            Close();
        }

        private void CreateNotifyIcon()
        {
            try
            {
                notifyIcon = new System.Windows.Forms.NotifyIcon
                {
                    Text = "CrypTool 2",
                    Icon = Properties.Resources.CrypTool,
                    Visible = true
                };

                ContextMenu notifyContextMenu = new System.Windows.Forms.ContextMenu();
                notifyIcon.ContextMenu = notifyContextMenu;

                MenuItem openMenuItem = new System.Windows.Forms.MenuItem(Properties.Resources._Open_CrypTool_2_0);
                openMenuItem.Click += new EventHandler(notifyIcon_DoubleClick);

                MenuItem normalPriority = new System.Windows.Forms.MenuItem(Properties.Resources._Normal_Priority)
                {
                    Checked = true
                };
                normalPriority.Click += new EventHandler(delegate
                {
                    Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;
                });
                normalPriority.Tag = ProcessPriorityClass.Normal;

                MenuItem idlePriority = new System.Windows.Forms.MenuItem(Properties.Resources._Idle_Priority);
                idlePriority.Click += new EventHandler(delegate
                {
                    Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Idle;
                });
                idlePriority.Tag = ProcessPriorityClass.Idle;

                MenuItem highPriority = new System.Windows.Forms.MenuItem(Properties.Resources._High_Priority);
                highPriority.Click += new EventHandler(delegate
                {
                    Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
                });
                highPriority.Tag = ProcessPriorityClass.High;

                MenuItem realtimePriority = new System.Windows.Forms.MenuItem(Properties.Resources._Realtime_Priority);
                realtimePriority.Click += new EventHandler(delegate
                {
                    Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
                });
                realtimePriority.Tag = ProcessPriorityClass.RealTime;

                MenuItem belowNormalPriority = new System.Windows.Forms.MenuItem(Properties.Resources._Below_Normal_Priority);
                belowNormalPriority.Click += new EventHandler(delegate
                {
                    Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
                });
                belowNormalPriority.Tag = ProcessPriorityClass.BelowNormal;

                MenuItem aboveNormalPriority = new System.Windows.Forms.MenuItem(Properties.Resources._Above_Normal_Priority);
                aboveNormalPriority.Click += new EventHandler(delegate
                {
                    Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;
                });
                aboveNormalPriority.Tag = ProcessPriorityClass.AboveNormal;

                MenuItem exitMenuItem = new System.Windows.Forms.MenuItem(Properties.Resources._Exit);
                exitMenuItem.Click += new EventHandler(exitMenuItem_Click);

                MenuItem priorityItems = new System.Windows.Forms.MenuItem(Properties.Resources.Change__Priority);

                playStopMenuItem = new System.Windows.Forms.MenuItem();
                playStopMenuItem.Click += new EventHandler(PlayStopMenuItemClicked);
                playStopMenuItem.Text = "Play";
                playStopMenuItem.Tag = true;

                notifyContextMenu.MenuItems.Add(openMenuItem);
                notifyContextMenu.MenuItems.Add("-");
                notifyContextMenu.MenuItems.Add(playStopMenuItem);
                notifyContextMenu.MenuItems.Add("-");
                notifyContextMenu.MenuItems.Add(priorityItems);
                notifyContextMenu.MenuItems.Add("-");
                notifyContextMenu.MenuItems.Add(exitMenuItem);

                priorityItems.MenuItems.Add(realtimePriority);
                priorityItems.MenuItems.Add(highPriority);
                priorityItems.MenuItems.Add(aboveNormalPriority);
                priorityItems.MenuItems.Add(normalPriority);
                priorityItems.MenuItems.Add(belowNormalPriority);
                priorityItems.MenuItems.Add(idlePriority);

                notifyIcon.DoubleClick += new EventHandler(notifyIcon_DoubleClick);

                PriorityChangedListener.PriorityChanged += delegate (ProcessPriorityClass newPriority)
                {
                    GuiLogMessage(string.Format("Changed CrypTool 2 process priority to '{0}'!", newPriority), NotificationLevel.Info);

                    foreach (System.Windows.Forms.MenuItem item in priorityItems.MenuItems)
                    {
                        if (item.Tag != null && item.Tag is ProcessPriorityClass)
                        {
                            item.Checked = ((ProcessPriorityClass)item.Tag == newPriority);
                        }
                    }
                };
                GuiLogMessage("NotifiyIcon successfully created", NotificationLevel.Debug);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception occured during creation of NotifyIcon: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        #endregion

        #region AutoUpdater

        private ImageSource update;
        private ImageSource noUpdate;
        private ImageSource updateReady;

        private bool autoUpdaterIconImageRotating = false;
        public bool AutoUpdaterIconImageRotating
        {
            get => autoUpdaterIconImageRotating;
            set
            {
                autoUpdaterIconImageRotating = value;

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    Storyboard autoUpdaterBigIconImageRotateStoryboard = (Storyboard)FindResource("AutoUpdaterBigIconImageRotateStoryboard");
                    Storyboard autoUpdaterSmallIconImageRotateStoryboard = (Storyboard)FindResource("AutoUpdaterSmallIconImageRotateStoryboard");
                    if (autoUpdaterIconImageRotating)
                    {
                        autoUpdaterBigIconImageRotateStoryboard.Begin();
                        autoUpdaterSmallIconImageRotateStoryboard.Begin();
                    }
                    else
                    {
                        autoUpdaterBigIconImageRotateStoryboard.Stop();
                        autoUpdaterSmallIconImageRotateStoryboard.Stop();
                    }
                }, null);
            }
        }

        private void InitUpdater()
        {
            update = (ImageSource)FindResource("Update");
            noUpdate = (ImageSource)FindResource("NoUpdate");
            updateReady = (ImageSource)FindResource("UpdateReady");
            autoUpdateButton.ToolTip = Properties.Resources.No_update_available_;

            AutoUpdater.GetSingleton().OnGuiLogNotificationOccured += new GuiLogNotificationEventHandler(OnGuiLogNotificationOccured);
            AutoUpdater.GetSingleton().OnUpdaterStateChanged += new AutoUpdater.UpdaterStateChangedHandler(MainWindow_OnUpdaterStateChanged);
            AutoUpdater.GetSingleton().OnUpdateDownloadProgressChanged += new AutoUpdater.UpdateDownloadProgressChangedHandler(MainWindow_OnUpdateDownloadProgressChanged);
            UpdaterPresentation.GetSingleton().OnRestartClicked += new UpdaterPresentation.RestartClickedHandler(RestartCrypTool);

            if (Settings.Default.CheckPeriodically)
            {
                AutoUpdater.GetSingleton().BeginCheckingForUpdates('S');
                AutoUpdater.GetSingleton().StartCheckTimer();
            }
            else if (Settings.Default.CheckOnStartup)
            {
                AutoUpdater.GetSingleton().BeginCheckingForUpdates('S');
            }
        }

        private bool IsUpdateFileAvailable()
        {
            if (!AutoUpdater.GetSingleton().IsUpdateReady)
            {
                return false; // nothing there
            }

            // check whether update is obsolete, i.e. older than running version
            Version updateVersion = AutoUpdater.GetSingleton().ReadDownloadedUpdateVersion(); // may return 0.0 in some cases
            if (updateVersion > new Version() // only if update version is known, i.e. > 0.0
                && updateVersion <= AssemblyHelper.Version) // and if update file is older currently running version
            {
                GuiLogMessage("Found obsolete update file, attempting to remove...", NotificationLevel.Debug);

                // obsolete update, attempt to remove
                try
                {
                    File.Delete(AutoUpdater.GetSingleton().FilePath);
                }
                catch (Exception e)
                {
                    GuiLogMessage("Obsolete update file found but removal failed: " + e.Message, NotificationLevel.Warning);
                }

                return false; // don't install
            }

            return true; // else: go ahead
        }

        private void MainWindow_OnUpdateDownloadProgressChanged(int progress)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                updateDownloadBar.Value = progress;
            }, progress);
        }

        private void MainWindow_OnUpdaterStateChanged(AutoUpdater.State newStatus)
        {
            switch (newStatus)
            {
                case AutoUpdater.State.Idle:
                    NoUpdateButtonImage();
                    AutoUpdaterIconImageRotating = false;
                    SetUpdaterToolTip(Properties.Resources.No_update_available_);
                    break;
                case AutoUpdater.State.Checking:
                    NoUpdateButtonImage();
                    AutoUpdaterIconImageRotating = true;
                    SetUpdaterToolTip(Properties.Resources.Checking_for_updates___);
                    break;
                case AutoUpdater.State.UpdateAvailable:
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        updateDownloadBar.Visibility = Visibility.Hidden;
                    }, null);
                    UpdateButtonImage();
                    AutoUpdaterIconImageRotating = false;
                    SetUpdaterToolTip(Properties.Resources.Update_available_);
                    MainWindow_OnUpdateDownloadProgressChanged(0);
                    if (!Settings.Default.AutoDownload)
                    {
                        Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            AutoUpdater_Executed(null, null);
                        }, null);
                    }
                    break;
                case AutoUpdater.State.Downloading:
                    UpdateButtonImage();
                    AutoUpdaterIconImageRotating = true;
                    SetUpdaterToolTip(Properties.Resources.Downloading_update___);
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        updateDownloadBar.Visibility = Visibility.Visible;
                    }, null);
                    break;
                case AutoUpdater.State.UpdateReady:
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        updateDownloadBar.Visibility = Visibility.Hidden;
                    }, null);
                    UpdateReadyButtonImage();
                    AutoUpdaterIconImageRotating = false;
                    SetUpdaterToolTip(Properties.Resources.Update_ready_to_install_);
                    if (!Settings.Default.AutoInstall)
                    {
                        Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            AutoUpdater_Executed(null, null);
                        }, null);
                    }
                    break;
            }
        }

        /// <summary>
        /// Ask user whether to install update or not. If not, ask also whether to remove update (if yes, then delete file).
        /// </summary>
        /// <returns>true if update shall be installed, false else</returns>
        private bool AskForInstall()
        {
            MessageBoxButton b = MessageBoxButton.YesNo;
            MessageBoxResult res = MessageBox.Show(Properties.Resources.Update_ready_to_install__would_you_like_to_install_it_now_, Properties.Resources.CrypTool_2_0_Update, b);
            if (res == MessageBoxResult.Yes)
            {
                return true;
            }
            else
            {
                // flomar, 04/20/2011: in case the user opts to *not* install CrypTool2 right now, we offer to remove the existing update file; 
                // this is a workaround regarding tracker item #255 (corrupted update file wouldn't let the auto updater do its job)
                MessageBoxResult removeExistingUpdateFile = MessageBox.Show(Properties.Resources.Update_ready_to_install__would_you_like_to_remove_it, Properties.Resources.CrypTool_2_0_Update, MessageBoxButton.YesNo);
                if (removeExistingUpdateFile == MessageBoxResult.Yes)
                {
                    // flomar, 04/20/2011: determine the existing update file and remove it (see above, related to bug-fix for tracker item #255)
                    try
                    {
                        File.Delete(AutoUpdater.GetSingleton().FilePath);
                    }
                    catch (Exception e)
                    {
                        GuiLogMessage("Removing update file failed: " + e.Message, NotificationLevel.Warning);
                    }
                }

                return false;
            }
        }

        private void RestartCrypTool()
        {
            try
            {
                restart = true;
                Close();
            }
            catch (Exception ex)
            {
                //we had an exception IN the Close() operation of the window
                //since we cannot fix the code of the Close() method we catch
                //the exception and write it to the log:
                GuiLogMessage(string.Format("Exception in RestartCrypTool method: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Attempts to initiate CrypUpdater process.
        /// </summary>
        /// <returns>true if succeeded, false if something went wrong</returns>
        private bool OnUpdate()
        {
            int processID = Process.GetCurrentProcess().Id;
            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            string CrypToolFolderPath = Path.GetDirectoryName(exePath);
            string updaterPath = Path.Combine(DirectoryHelper.DirectoryLocalTemp, "CrypUpdater.exe");
            string filePath = AutoUpdater.GetSingleton().FilePath;

            try
            {
                File.Copy(Path.Combine(CrypToolFolderPath, "CrypUpdater.exe"), Path.Combine(DirectoryHelper.DirectoryLocalTemp, "CrypUpdater.exe"), true);

                if (File.Exists(filePath) && File.Exists(updaterPath) && File.Exists(Path.Combine(DirectoryHelper.DirectoryLocalTemp, "Ionic.Zip.Reduced.dll")))
                {
                    string parameters = "\"" + filePath + "\" " + "\"" + CrypToolFolderPath + "\" " + "\"" + exePath + "\" " + "\"" + processID + "\" \"" + bool.TrueString + "\"";
                    Process.Start(updaterPath, parameters);
                    if (!restart)
                    {
                        App.Current.Shutdown();
                    }

                    // success
                    return true;
                }
                else
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch (Exception)
                    {
                        //Just ignore
                    }
                    GuiLogMessage("AutoUpdate: Update failed, one or more files cannot be found.", NotificationLevel.Error);
                    return false;
                }

            }
            catch (Exception e)
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception)
                {
                    //Just ignore
                }
                GuiLogMessage("AutoUpdate: Update failed: " + e.Message, NotificationLevel.Error);
                return false;
            }
        }

        private void UpdateButtonImage()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ((System.Windows.Controls.Image)(autoUpdateButton.Image)).Source = update;
                ((System.Windows.Controls.Image)(autoUpdateButton.Image)).Source = update;
            }, null);
        }

        private void NoUpdateButtonImage()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ((System.Windows.Controls.Image)(autoUpdateButton.Image)).Source = noUpdate;
                ((System.Windows.Controls.Image)(autoUpdateButton.ImageSmall)).Source = noUpdate;
            }, null);
        }

        private void UpdateReadyButtonImage()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ((System.Windows.Controls.Image)(autoUpdateButton.Image)).Source = updateReady;
                ((System.Windows.Controls.Image)(autoUpdateButton.ImageSmall)).Source = updateReady;
            }, null);
        }

        private void SetUpdaterToolTip(string text)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                autoUpdateButton.ToolTip = text;
            }, text);
        }

        #endregion

        private const string ZoneName = "Zone.Identifier";

        private void UnblockDLLs()
        {
            try
            {
                if (AssemblyHelper.InstallationType != Ct2InstallationType.ZIP)
                {
                    return;
                }
                IEnumerable<string> files = Directory.EnumerateFiles(DirectoryHelper.BaseDirectory, "*", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    bool result = FileSystem.AlternateDataStreamExists(file, ZoneName);
                    if (result)
                    {
                        // Clear the read-only attribute, if set:
                        FileAttributes attributes = File.GetAttributes(file);
                        if (FileAttributes.ReadOnly == (FileAttributes.ReadOnly & attributes))
                        {
                            attributes &= ~FileAttributes.ReadOnly;
                            File.SetAttributes(file, attributes);
                        }

                        //UnblocK:
                        FileSystem.DeleteAlternateDataStream(file, ZoneName);
                    }
                }
                GuiLogMessage("DLLs unblocked", NotificationLevel.Debug);
            }
            catch (Exception)
            {
                GuiLogMessage("Exception during unblocking of DLLs. If everything works fine, exception can be ignored", NotificationLevel.Warning);
            }
        }
    }
}
