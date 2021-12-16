/*
   Copyright 2019 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using CrypTool.PluginBase.Miscellaneous;
using CrypTool.Plugins.DECRYPTTools.Util;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace CrypTool.Plugins.DECRYPTTools
{
    [PluginBase.Attributes.Localization("CrypTool.Plugins.DECRYPTTools.Properties.Resources")]
    public partial class DECRYPTDownloaderPresentation : UserControl
    {
        private readonly DECRYPTDownloader Plugin;
        public ObservableCollection<RecordsRecord> RecordsList = new ObservableCollection<RecordsRecord>();
        private GridViewColumnHeader _lastHeaderClicked;
        private ListSortDirection _lastDirection;
        private bool _downloadingList = false;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public DECRYPTDownloaderPresentation(DECRYPTDownloader plugin)
        {
            InitializeComponent();
            Plugin = plugin;
            ListView.ItemsSource = RecordsList;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListView.ItemsSource);
            view.Filter = UserFilter;
        }

        /// <summary>
        /// Filter name using the entered text
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool UserFilter(object item)
        {
            if (string.IsNullOrEmpty(Filter.Text))
            {
                return true;
            }
            else
            {
                return ((item as RecordsRecord).name.IndexOf(Filter.Text, StringComparison.OrdinalIgnoreCase) >= 0);
            }
        }

        /// <summary>
        /// User double-clicked for downloading a record
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void HandleDoubleClick(object sender, EventArgs eventArgs)
        {
            try
            {
                ListViewItem lvi = sender as ListViewItem;
                if (lvi != null)
                {
                    RecordsRecord record = lvi.Content as RecordsRecord;
                    if (record != null)
                    {
                        if (OnPluginProgressChanged != null)
                        {
                            OnPluginProgressChanged.Invoke(null, new PluginProgressEventArgs(0, 1));
                        }

                        Plugin.Download(record);

                        if (OnPluginProgressChanged != null)
                        {
                            OnPluginProgressChanged.Invoke(null, new PluginProgressEventArgs(1, 1));
                        }
                    }
                }
            }
            catch (Exception)
            {
                //wtf?
            }
        }

        /// <summary>
        /// Text of filter textfield changed; thus, update the ListView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Filter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(ListView.ItemsSource).Refresh();
        }

        /// <summary>
        /// Sets the content of the LoginLabel
        /// </summary>
        /// <param name="text"></param>
        public void SetLoginNameLabel(string text)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                LoginNameLabel.Content = text;
            }, null);
        }

        /// <summary>
        /// Sorts the list view based on the clicked column header
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    if (headerClicked != _lastHeaderClicked)
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if (_lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }

                    Binding columnBinding = headerClicked.Column.DisplayMemberBinding as Binding;
                    string sortBy = (columnBinding != null ? (columnBinding.Path.Path != null ? columnBinding.Path.Path : headerClicked.Column.Header) : null) as string;

                    Sort(sortBy, direction);

                    if (direction == ListSortDirection.Ascending)
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowUp"] as DataTemplate;
                    }
                    else
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowDown"] as DataTemplate;
                    }

                    // Remove arrow from previously sorted header
                    if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked)
                    {
                        _lastHeaderClicked.Column.HeaderTemplate = null;
                    }

                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;
                }
            }
        }

        private void Sort(string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(ListView.ItemsSource);
            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }

        /// <summary>
        /// Downloads the current filtered list record by record
        /// </summary>
        public void DownloadCurrentRecordList()
        {
            lock (this)
            {
                if (_downloadingList)
                {
                    return;
                }
                _downloadingList = true;
            }

            string filterText = string.Empty;

            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                IsEnabled = false;
                filterText = Filter.Text;
            }, null);
            try
            {
                double total = 0;
                //count matching record to generate total number
                foreach (RecordsRecord record in RecordsList)
                {
                    if (string.IsNullOrEmpty(filterText) || record.name.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        total++;
                    }
                }

                double counter = 0;
                //output each record
                foreach (RecordsRecord record in RecordsList)
                {
                    if (string.IsNullOrEmpty(filterText) || record.name.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (Plugin.Running == false)
                        {
                            return;
                        }
                        Plugin.Download(record);
                        //show download progress using PluginProgressChanged
                        counter += 1;
                        if (OnPluginProgressChanged != null)
                        {
                            OnPluginProgressChanged.Invoke(null, new PluginProgressEventArgs(counter, total));
                        }
                    }
                }
                if (OnPluginProgressChanged != null)
                {
                    OnPluginProgressChanged.Invoke(null, new PluginProgressEventArgs(1, 1));
                }

            }
            finally
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    IsEnabled = true;
                }, null);
                lock (this)
                {
                    _downloadingList = false;
                }
            }
        }
        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, null, new GuiLogEventArgs(message, null, logLevel));
        }
    }
}
