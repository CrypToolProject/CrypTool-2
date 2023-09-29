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
using CrypTool.Core;
using CrypTool.CrypWin.Properties;
using CrypTool.CrypWin.Resources;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Attributes;
using CrypTool.PluginBase.Miscellaneous;
using CrypWin.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Threading;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace CrypTool.CrypWin
{
    public partial class MainWindow
    {
        private FileStream _logFileStream;
        private StreamWriter _logFileWriter;

        #region PluginManager events
        private void pluginManager_OnExceptionOccured(object sender, PluginManagerEventArgs args)
        {
            OnGuiLogNotificationOccured(Resource.pluginManager, new GuiLogEventArgs(args.Exception.Message, null, NotificationLevel.Error));
        }

        private void pluginManager_OnDebugMessageOccured(object sender, PluginManagerEventArgs args)
        {
            OnGuiLogNotificationOccured(Resource.pluginManager, new GuiLogEventArgs(args.Exception.Message, null, NotificationLevel.Debug));
        }

        private void pluginManager_OnPluginLoaded(object sender, PluginLoadedEventArgs args)
        {
            splashWindow.ShowStatus(string.Format(Properties.Resources.Assembly___0___loaded_, args.AssemblyName),
              ((args.CurrentPluginNumber / (double)args.NumberPluginsFound * 100) / 2));
        }
        #endregion PluginManager events

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, null, logLevel));
        }

        private int statusBarTextCounter, errorCounter, warningCounter, infoCounter, debugCounter, balloonCounter;

        public void OnGuiLogNotificationOccured(object sender, GuiLogEventArgs arg)
        {
            if (silent)
            {
                return;
            }

            if (Dispatcher.CheckAccess())
            {
                OnGuiLogNotificationOccuredTS(sender, arg);
            }
            else
            {
                GuiLogNotificationDelegate guiLogDelegate = new GuiLogNotificationDelegate(OnGuiLogNotificationOccuredTS);
                Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, guiLogDelegate, sender, arg);
            }
        }

        /// <summary>
        /// The method shows a new log entry in the GUI. It must be called only from the GUI thread! For 
        /// log messages from other threads use GuiLogMessage()
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args">Information about the log message</param>
        public void OnGuiLogNotificationOccuredTS(object sender, GuiLogEventArgs args)
        {
            try
            {
                if (Settings.Default.WriteLogFile)
                {
                    WriteLogToFile(args);
                }
            }
            catch (Exception)
            {
                //do nothing
            }

            try
            {
                //check log level setting
                NotificationLevel level = (NotificationLevel)Settings.Default.LogLevel;
                if (AssemblyHelper.BuildType != Ct2BuildType.Developer && //to CT2 developers we always show each log
                    args.NotificationLevel < level)
                {
                    return;
                }
            }
            catch (Exception)
            {
                //do nothing
            }

            try
            {
                if (textBlockDebugsCount == null ||
                    textBlockInfosCount == null ||
                    textBlockWarningsCount == null ||
                    textBlockErrosCount == null ||
                    textBlockBalloonsCount == null)
                {
                    return;
                }
                statusBarTextCounter++;
                LogMessage logMessage = new LogMessage
                {

                    // Attention: The string value of enum will be used to find appropritate imgage. 
                    LogLevel = args.NotificationLevel,
                    Nr = statusBarTextCounter
                };

                if (args.Plugin != null)
                {
                    logMessage.Plugin = args.Plugin.GetPluginInfoAttribute().Caption;
                    logMessage.Title = args.Title;
                }
                else if (sender is string)
                {
                    logMessage.Plugin = sender as string;
                    logMessage.Title = "-";
                }
                else
                {
                    logMessage.Plugin = Resource.CrypTool;
                    logMessage.Title = "-";
                }

                logMessage.Time =
                  args.DateTime.Hour.ToString("00") + ":" + args.DateTime.Minute.ToString("00") + ":" +
                  args.DateTime.Second.ToString("00") + ":" + args.DateTime.Millisecond.ToString("000");

                logMessage.Message = args.Message;

                if (listFilter.Contains(args.NotificationLevel))
                {
                    TextBlock textBlock = new TextBlock
                    {
                        Text = logMessage.LogLevel + ": " + logMessage.Time + ": " + args.Message
                    };
                    statusBarItem.Content = textBlock;
                    if (args.Message.Length >= 64)
                    {
                        notifyIcon.Text = args.Message.Substring(0, 63);
                    }
                    else
                    {
                        notifyIcon.Text = args.Message;
                    }
                }

                collectionLogMessages.Add(logMessage);

                //Not more than 1000 messages allowed:
                if (collectionLogMessages.Count > 1000)
                {
                    LogMessage firstLogMessage = collectionLogMessages[0];
                    switch (firstLogMessage.LogLevel)
                    {
                        case NotificationLevel.Debug:
                            debugCounter--;
                            textBlockDebugsCount.Text = string.Format("{0:0,0}", debugCounter);
                            break;
                        case NotificationLevel.Info:
                            infoCounter--;
                            textBlockInfosCount.Text = string.Format("{0:0,0}", infoCounter);
                            break;
                        case NotificationLevel.Warning:
                            warningCounter--;
                            textBlockWarningsCount.Text = string.Format("{0:0,0}", warningCounter);
                            break;
                        case NotificationLevel.Error:
                            errorCounter--;
                            textBlockErrosCount.Text = string.Format("{0:0,0}", errorCounter);
                            break;
                        case NotificationLevel.Balloon:
                            balloonCounter--;
                            textBlockBalloonsCount.Text = string.Format("{0:0,0}", balloonCounter);
                            break;
                    }
                    collectionLogMessages.Remove(firstLogMessage);
                }

                SetMessageCount();

                switch (args.NotificationLevel)
                {
                    case NotificationLevel.Debug:
                        debugCounter++;
                        textBlockDebugsCount.Text = string.Format("{0:0,0}", debugCounter);
                        break;
                    case NotificationLevel.Info:
                        infoCounter++;
                        textBlockInfosCount.Text = string.Format("{0:0,0}", infoCounter);
                        break;
                    case NotificationLevel.Warning:
                        warningCounter++;
                        textBlockWarningsCount.Text = string.Format("{0:0,0}", warningCounter);
                        break;
                    case NotificationLevel.Error:
                        errorCounter++;
                        textBlockErrosCount.Text = string.Format("{0:0,0}", errorCounter);
                        break;
                    case NotificationLevel.Balloon:
                        balloonCounter++;
                        textBlockBalloonsCount.Text = string.Format("{0:0,0}", balloonCounter);
                        break;
                }

                ScrollToLast();

                //Balloon:
                if (WindowState == WindowState.Minimized)
                {
                    int ms = Settings.Default.BallonVisibility_ms;
                    if (args.NotificationLevel == NotificationLevel.Balloon && Settings.Default.ShowBalloonLogMessagesInBalloon)
                    {
                        notifyIcon.ShowBalloonTip(ms, Properties.Resources.Balloon_Message, args.Message, ToolTipIcon.Info);
                    }
                    else if (args.NotificationLevel == NotificationLevel.Error && Settings.Default.ShowErrorLogMessagesInBalloon)
                    {
                        notifyIcon.ShowBalloonTip(ms, Properties.Resources.Error_Message, args.Message, ToolTipIcon.Error);
                    }
                    else if (args.NotificationLevel == NotificationLevel.Info && Settings.Default.ShowInfoLogMessagesInBalloon)
                    {
                        notifyIcon.ShowBalloonTip(ms, Properties.Resources.Information_Message, args.Message, ToolTipIcon.Info);
                    }
                    else if (args.NotificationLevel == NotificationLevel.Warning && Settings.Default.ShowWarningLogMessagesInBalloon)
                    {
                        notifyIcon.ShowBalloonTip(ms, Properties.Resources.Warning_Message, args.Message, ToolTipIcon.Warning);
                    }
                    else if (args.NotificationLevel == NotificationLevel.Debug && Settings.Default.ShowDebugLogMessagesInBalloon)
                    {
                        notifyIcon.ShowBalloonTip(ms, Properties.Resources.Debug_Message, args.Message, ToolTipIcon.Info);
                    }
                }
            }
            catch (Exception)
            {
                //OnGuiLogNotificationOccuredTS(this, new GuiLogEventArgs(exception.Message, null, NotificationLevel.Error)); <--- causes recursion (StackOverflowException)
            }
        }

        /// <summary>
        /// Writes a log entry to the logfile 
        /// </summary>
        /// <param name="args"></param>
        private void WriteLogToFile(GuiLogEventArgs args)
        {
            if (_logFileStream == null)
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                //1. check, if CrypTool 2 folder exists, if not, create it
                if (!Directory.Exists(appDataPath + @"\CrypTool2"))
                {
                    Directory.CreateDirectory(appDataPath + @"\CrypTool2");
                }
                //2. check, if CrypTool 2 logs folder exists; if not, create it
                if (!Directory.Exists(appDataPath + @"\CrypTool2\Logs"))
                {
                    Directory.CreateDirectory(appDataPath + @"\CrypTool2\Logs");
                }
                //3. create logfile path
                string logfileName = appDataPath + @"\CrypTool2\Logs\CrypTool_2_log_" + DateTime.Now.Ticks + ".csv";
                _logFileStream = new FileStream(logfileName, FileMode.Create);
            }

            if (_logFileWriter == null)
            {
                _logFileWriter = new StreamWriter(_logFileStream);
                _logFileWriter.WriteLine("date time;log level;plugin name;title;message");
                _logFileWriter.Flush();
            }

            string level = "no_log_level_defined";
            switch (args.NotificationLevel)
            {
                case NotificationLevel.Debug:
                    level = "debug";
                    break;
                case NotificationLevel.Info:
                    level = "info";
                    break;
                case NotificationLevel.Warning:
                    level = "warning";
                    break;
                case NotificationLevel.Error:
                    level = "error";
                    break;
                case NotificationLevel.Balloon:
                    level = "baloon";
                    break;
            }
            string pluginname = "no_plugin_defined";
            IPlugin plugin = args.Plugin;
            if (plugin != null)
            {
                PluginInfoAttribute attribute = plugin.GetPluginInfoAttribute();
                if (attribute != null)
                {
                    pluginname = attribute.Caption;
                }
            }
            _logFileWriter.WriteLine(DateTime.Now + ";\"" + level + "\";\"" + pluginname + "\";\"" + args.Title + "\";\"" + args.Message + "\"");
            _logFileWriter.Flush();
        }

        /// <summary>
        /// Closes the logFile
        /// </summary>
        private void CloseLogFile()
        {
            try
            {
                if (_logFileWriter != null)
                {
                    _logFileWriter.Close();
                    //writer also closes stream
                }
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private void SetMessageCount()
        {
            if (listViewLogList != null || collectionLogMessages != null)
            {
                dockWindowLogMessages.Header = Properties.Resources.LogWindow + " " + string.Format("{0:0,0}", listViewLogList.Items.Count) + string.Format(Properties.Resources._Messages___0__filtered_, string.Format("{0:0,0}", collectionLogMessages.Count - listViewLogList.Items.Count));
            }
        }

        private void ScrollToLast()
        {
            if (listViewLogList.Items.Count > 0)
            {
                listViewLogList.ScrollIntoView(listViewLogList.Items[listViewLogList.Items.Count - 1]);
            }
        }

        private void ButtonDeleteMessages_Click(object sender, RoutedEventArgs e)
        {
            deleteAllMessages();
        }

        private void deleteAllMessages()
        {
            collectionLogMessages.Clear();
            statusBarTextCounter = 0;
            dockWindowLogMessages.Header = Properties.Resources.LogWindow + " 00" + Properties.Resources._Messages;
            errorCounter = 0;
            textBlockErrosCount.Text = errorCounter.ToString();
            warningCounter = 0;
            textBlockWarningsCount.Text = warningCounter.ToString();
            infoCounter = 0;
            textBlockInfosCount.Text = infoCounter.ToString();
            debugCounter = 0;
            textBlockDebugsCount.Text = debugCounter.ToString();
            balloonCounter = 0;
            textBlockBalloonsCount.Text = balloonCounter.ToString();
        }

        public void DeleteAllMessagesInGuiThread()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    deleteAllMessages();
                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format("Exception during DeleteAllMessagesInGuiThread: {0}", ex.Message), NotificationLevel.Error);
                }
            }, null);
        }

        public IList<string> GetAllMessagesFromGuiThread(params NotificationLevel[] levels)
        {
            IList<string> logList = new List<string>();

            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    foreach (LogMessage msg in collectionLogMessages)
                    {
                        if (levels.Contains(msg.LogLevel))
                        {
                            logList.Add(msg.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format("Exception during GetAllMessagesFromGuiThread: {0}", ex.Message), NotificationLevel.Error);
                }
            }, null);

            return logList;
        }

        private void buttonError_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(listViewLogList.ItemsSource);
            if (listFilter.Contains(NotificationLevel.Error))
            {
                listFilter.Remove(NotificationLevel.Error);
            }
            else
            {
                listFilter.Add(NotificationLevel.Error);
            }

            view.Filter = new Predicate<object>(FilterCallback);

            ToggleButton tb = sender as ToggleButton;
            if (tb != null)
            {
                if (tb.IsChecked == true)
                {
                    tb.ToolTip = Properties.Resources.Hide_Errors;
                }
                else
                {
                    tb.ToolTip = Properties.Resources.Show_Errors;
                }
            }
            SetMessageCount();
        }

        private void buttonWarning_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(listViewLogList.ItemsSource);
            if (listFilter.Contains(NotificationLevel.Warning))
            {
                listFilter.Remove(NotificationLevel.Warning);
            }
            else
            {
                listFilter.Add(NotificationLevel.Warning);
            }

            view.Filter = new Predicate<object>(FilterCallback);

            ToggleButton tb = sender as ToggleButton;
            if (tb != null)
            {
                if (tb.IsChecked == true)
                {
                    tb.ToolTip = Properties.Resources.Hide_Warnings;
                }
                else
                {
                    tb.ToolTip = Properties.Resources.Show_Warnings;
                }
            }
            SetMessageCount();
        }

        private void buttonInfo_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(listViewLogList.ItemsSource);
            if (listFilter.Contains(NotificationLevel.Info))
            {
                listFilter.Remove(NotificationLevel.Info);
            }
            else
            {
                listFilter.Add(NotificationLevel.Info);
            }

            view.Filter = new Predicate<object>(FilterCallback);

            ToggleButton tb = sender as ToggleButton;
            if (tb != null)
            {
                if (tb.IsChecked == true)
                {
                    tb.ToolTip = Properties.Resources.Hide_Infos;
                }
                else
                {
                    tb.ToolTip = Properties.Resources.Show_Infos;
                }
            }
            SetMessageCount();
        }

        private void buttonDebug_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(listViewLogList.ItemsSource);
            if (listFilter.Contains(NotificationLevel.Debug))
            {
                listFilter.Remove(NotificationLevel.Debug);
            }
            else
            {
                listFilter.Add(NotificationLevel.Debug);
            }

            view.Filter = new Predicate<object>(FilterCallback);

            ToggleButton tb = sender as ToggleButton;
            if (tb != null)
            {
                if (tb.IsChecked == true)
                {
                    tb.ToolTip = Properties.Resources.Hide_Debugs;
                }
                else
                {
                    tb.ToolTip = Properties.Resources.Show_Debugs;
                }
            }
            SetMessageCount();
        }

        private void buttonBalloon_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(listViewLogList.ItemsSource);
            if (listFilter.Contains(NotificationLevel.Balloon))
            {
                listFilter.Remove(NotificationLevel.Balloon);
            }
            else
            {
                listFilter.Add(NotificationLevel.Balloon);
            }

            view.Filter = new Predicate<object>(FilterCallback);

            ToggleButton tb = sender as ToggleButton;
            if (tb != null)
            {
                if (tb.IsChecked == true)
                {
                    tb.ToolTip = Properties.Resources.Hide_Balloons;
                }
                else
                {
                    tb.ToolTip = Properties.Resources.Show_Balloons;
                }
            }
            SetMessageCount();
        }

        private void ButtonExportToHTML_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog
                {
                    InitialDirectory = Settings.Default.LastPath,
                    Filter = Resource.HTML_fileFilter,
                    FileName = Resource.fileName_LogMessages
                };

                if (dlg.ShowDialog() == true)
                {
                    IEnumerable<string> messages = collectionLogMessages
                        .Where(m => listFilter.Contains(m.LogLevel))
                        .Select(m => string.Format(Resource.row_template, new object[] { LogMessage.Color(m.LogLevel), m.Nr.ToString(), m.LogLevel.ToString(), m.Time, m.Plugin, m.Title, m.Message }));

                    string html = Resource.table_template.Replace("{0}", messages.Count().ToString()).Replace("{1}", string.Join("", messages));

                    FileStream stream = File.Open(dlg.FileName, FileMode.Create);
                    StreamWriter sWriter = new StreamWriter(stream);
                    sWriter.Write(html);
                    sWriter.Close();
                }
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Filters the LogMessages.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private bool FilterCallback(object item)
        {
            return listFilter.Contains(((LogMessage)item).LogLevel);
        }
    }
}
