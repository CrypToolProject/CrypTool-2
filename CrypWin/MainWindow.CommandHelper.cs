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
using CrypTool.PluginBase.Editor;
using Startcenter;
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace CrypTool.CrypWin
{
    public partial class MainWindow
    {
        #region Private variables
        private bool closedByMenu = false;
        private Window aboutWindow;

        private DemoController demoController;
        #endregion

        #region New, Open, Save, SaveAs, Close
        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
            {
                return;
            }

            if (Settings.Default.useDefaultEditor)
            {
                NewProject(GetDefaultEditor());
            }
            else
            {
                NewProject(GetEditor(Settings.Default.preferredEditor));
            }
        }

        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
            {
                e.CanExecute = false;
                e.Handled = true;
                return;
            }
            e.CanExecute = true;
            e.Handled = true;
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
            {
                return;
            }

            OpenProject();
        }

        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
            {
                e.CanExecute = false;
                e.Handled = true;
                return;
            }
            e.CanExecute = ActiveEditor != null && ActiveEditor.CanSave;
            e.Handled = true;
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
            {
                return;
            }

            SaveProject();
        }

        private void SaveAs_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
            {
                e.CanExecute = false;
                e.Handled = true;
                return;
            }
            e.CanExecute = ActiveEditor != null && ActiveEditor.CanSave;
            e.Handled = true;
        }

        private void SaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
            {
                return;
            }

            SaveProjectAs();
        }

        private void Print_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (MainTab == null)
            {
                return;
            }

            CTTabItem tab = (CTTabItem)(MainTab.SelectedItem);
            if (tab == null)
            {
                return;
            }

            if (tabToContentMap.ContainsKey(tab))
            {
                object o = tabToContentMap[tab];
                if (o is OnlineHelpTab)
                {
                    e.CanExecute = true;
                }
                else
                {
                    e.CanExecute = ActiveEditor != null && ActiveEditor.CanPrint;
                }
            }
            e.Handled = true;
        }

        private void Print_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (MainTab == null)
            {
                return;
            }

            CTTabItem tab = (CTTabItem)(MainTab.SelectedItem);
            if (tab == null)
            {
                return;
            }

            if (tabToContentMap.ContainsKey(tab))
            {
                object o = tabToContentMap[tab];
                if (o is OnlineHelpTab)
                {
                    ((OnlineHelpTab)o).Print();
                }
                else
                {
                    if (ActiveEditor != null)
                    {
                        ActiveEditor.Print();
                    }
                }
            }
        }

        private void Close_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
            {
                e.CanExecute = false;
                e.Handled = true;
                return;
            }
            e.CanExecute = true;
            e.Handled = true;
        }

        private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
            {
                return;
            }

            closedByMenu = true;
            Close();
        }
        #endregion New, Open, Save, SaveAs, Close

        # region ContextHelp, Play, Stop, Pause, Undo, Redo, Maximize, Fullscreen
        private void ContextHelp_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ContextHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
            {
                return;
            }

            try
            {
                if (ActiveEditor != null)
                {
                    ActiveEditor.ShowSelectedEntityHelp();
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
        }

        private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
            {
                e.CanExecute = false;
                e.Handled = true;
                return;
            }
            e.CanExecute = ActiveEditor != null && ActiveEditor.CanExecute;
            if (e.CanExecute)
            {
                playStopMenuItem.Text = "Play";
                playStopMenuItem.Tag = true;
            }
        }

        private void Play_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
            {
                return;
            }

            PlayProject();
        }

        public void PlayProjectInGuiThread()
        {
            if (Dispatcher.CheckAccess())
            {
                try
                {
                    PlayProject();
                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format("Exception during playing of project in gui thread: {0}", ex.Message), NotificationLevel.Error);
                }
            }
            else
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        PlayProject();
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage(string.Format("Exception during playing of project in gui thread: {0}", ex.Message), NotificationLevel.Error);
                    }
                }, null);
            }
        }

        private void PlayProject()
        {
            PlayProject(ActiveEditor);
        }

        private void PlayProject(IEditor editor)
        {
            try
            {
                if (editor != null)
                {
                    Thread editorThread = new Thread(editor.Execute)
                    {
                        CurrentCulture = Thread.CurrentThread.CurrentCulture,
                        CurrentUICulture = Thread.CurrentThread.CurrentUICulture
                    };
                    editorThread.Start();
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
        }

        private void Stop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
            {
                e.CanExecute = false;
                e.Handled = true;
                return;
            }
            e.CanExecute = ActiveEditor != null && ActiveEditor.CanStop;
            if (e.CanExecute)
            {
                playStopMenuItem.Text = "Stop";
                playStopMenuItem.Tag = false;
            }
        }

        private void Stop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
            {
                return;
            }

            StopProjectExecution();
        }

        public void StopProjectInGuiThread()
        {
            if (Dispatcher.CheckAccess())
            {
                StopProjectExecution();
            }
            else
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    StopProjectExecution();
                }, null);
            }
        }

        public void CloseProjectInGuiThread()
        {
            try
            {
                if (Dispatcher.CheckAccess())
                {
                    CloseProject();
                    tabToContentMap.Last().Key.Close();
                }
                else
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        try
                        {
                            CloseProject();
                            tabToContentMap.Last().Key.Close();
                        }
                        catch (Exception ex)
                        {
                            GuiLogMessage(string.Format("Exception during CloseProjectInGuiThread: {0}", ex.Message), NotificationLevel.Error);
                        }
                    }, null);
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception during CloseProjectInGuiThread: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        private void StopProjectExecution(IEditor editor)
        {
            if (editor != null)
            {
                editor.Stop();
            }
        }

        private void StopProjectExecution()
        {
            StopProjectExecution(ActiveEditor);
        }

        private void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
            {
                e.CanExecute = false;
                e.Handled = true;
                return;
            }
            e.CanExecute = ActiveEditor != null && ActiveEditor.CanUndo;
        }

        private void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ActiveEditor != null)
            {
                ActiveEditor.Undo();
            }
        }

        private void Redo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
            {
                e.CanExecute = false;
                e.Handled = true;
                return;
            }
            e.CanExecute = ActiveEditor != null && ActiveEditor.CanRedo;
        }

        private void Redo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ActiveEditor != null)
            {
                ActiveEditor.Redo();
            }
        }

        private void Cut_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ActiveEditor != null && ActiveEditor.CanCut;
        }

        private void Cut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ActiveEditor != null)
            {
                ActiveEditor.Cut();
            }
        }

        private void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ActiveEditor != null && ActiveEditor.CanCopy;
        }

        private void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ActiveEditor != null)
            {
                ActiveEditor.Copy();
            }
        }

        private void Paste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ActiveEditor != null && ActiveEditor.CanPaste;
        }

        private void Paste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ActiveEditor != null)
            {
                ActiveEditor.Paste();
            }
        }

        private void Remove_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ActiveEditor != null && ActiveEditor.CanRemove;
        }

        private void Remove_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ActiveEditor != null)
            {
                ActiveEditor.Remove();
            }
        }

        private void AddImage_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ActiveEditor != null && ActiveEditor.GetEditorInfoAttribute().CanEdit;
        }

        private void AddText_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ActiveEditor != null && ActiveEditor.GetEditorInfoAttribute().CanEdit;
        }

        private void Maximize_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Fullscreen_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Fullscreen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AppRibbon.IsMinimized = !AppRibbon.IsMinimized; // toggle
        }

        private void PlayDemo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
            {
                e.CanExecute = false;
                e.Handled = true;
                return;
            }
            e.CanExecute = demoController != null && !demoController.IsRunning;
        }

        private void PlayDemo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
            {
                return;
            }

            string[] samples = OpenMultipleProjectsDialog();
            string saveFile = null;

            if (IsCommandParameterGiven("-test"))
            {
                SaveFileDialog saveSelector = new SaveFileDialog
                {
                    Filter = "Log file (*.txt)|*.txt"
                };
                if (saveSelector.ShowDialog() == true)
                {
                    saveFile = saveSelector.FileName;
                }
                else
                {
                    return;
                }
            }

            if (samples.Length > 0)
            {
                SetRibbonControlEnabled(false);
                demoController.Start(samples, saveFile); // saveFile may be null, it's okay
            }
        }

        private void StopDemo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
            {
                e.CanExecute = false;
                e.Handled = true;
                return;
            }
            e.CanExecute = demoController != null && demoController.IsRunning;
        }

        private void StopDemo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Settings.Default.FixedWorkspace)
            {
                return;
            }

            demoController.Stop();
        }

        private void ShowHideLog_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ShowHideLog_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            LogBTN.IsChecked = !LogBTN.IsChecked;
            LogBTN_Checked(LogBTN, null);
        }

        # endregion Play, Stop, Pause, Undo, Redo, Maximize, Fullscreen, Demo

        # region P2P

        private void P2P_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void P2P_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO synchronize instance with editor selection in settings tab
            //removed p2p editor: kopal 04.11.2014
            AddEditorDispatched(typeof(CrypCloud.Manager.CrypCloudManager));
        }

        # endregion P2P

        # region Startcenter

        private void Startcenter_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Startcenter_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AddEditorDispatched(typeof(StartcenterEditor));
        }

        private void CrypToolStore_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CrypToolStore_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AddEditorDispatched(typeof(CrypToolStore.CrypToolStoreEditor));
        }

        # endregion Startcenter

        # region AutoUpdater
        private void AutoUpdater_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void AutoUpdater_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            UpdaterPresentation updaterPresentation = UpdaterPresentation.GetSingleton();
            OpenTab(updaterPresentation, new TabInfo() { Title = Properties.Resources.CrypTool_2_0_Update }, null).IsSelected = true;
        }

        # endregion P2P

        #region Settings
        private void Settings_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Settings_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SettingsPresentation settingsPresentation = SettingsPresentation.GetSingleton();
            OpenTab(settingsPresentation, new TabInfo() { Title = Properties.Resources.Settings }, null).IsSelected = true;
        }

        #endregion Settings

        # region help
        private void ButtonHelp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowHelpPage(typeof(MainWindow));
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }

        private void ButtonAbout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                aboutWindow = new Splash(true);
                // does not activate window, though this may hide the modal dialog in background in certain (uncommon) situations
                aboutWindow.ShowDialog();
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }

        # endregion help
    }

}
