/*
   Copyright Mikail Sarier 2023, University of Mannheim

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
using System;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.Plugins.Anonymity
{
    [Author("Mikail Sarier", "mikail.sarier@students.uni-mannheim.de", "Universität Mannheim", "https://www.wim.uni-mannheim.de/")]
    [PluginInfo("CrypTool.Plugins.Anonymity.Properties.Resources", "AnonymityCaption", "AnonymityTooltip", "Anonymity/userdoc.xml", new[] { "Anonymity/Anonymity.png" })]
    [ComponentCategory(ComponentCategory.Protocols)]
    public class Anonymity : ICrypComponent
    {
        #region Private Variables
        public string _csv;
        public string _output;
        private readonly AnonymitySettings _settings = new AnonymitySettings();
        private AnonymityPresentation _presentation = new AnonymityPresentation();
        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputCaption", "InputTooltip")]
        public string InputCSV
        {
            get
            {
                return _csv;
            }
            set
            {
                _csv = value;
                OnPropertyChanged("InputCSV");
            }
        }
        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip")]
        public string OutputData
        {
            get
            {
                return _output;
            }
            set
            {
                _output = value;
                OnPropertyChanged("OutputData");

            }
        }
        #endregion

        #region IPlugin Members
        public ISettings Settings
        {
            get { return _settings; }
        }
        public UserControl Presentation
        {
            get { return _presentation; }
        }

        public void PreExecution()
        {

        }

        public void Execute()
        {

            // if input is empty EmptyMessage is shown
            if (string.IsNullOrWhiteSpace(_csv))
            {
                GuiLogMessage(Properties.Resources.EmptyMessage, NotificationLevel.Warning);
                return;
            }
            try
            {
                string rowSeperator = ProcessEscapeSymbols(_settings.RowSeparator);
                string columnSeperator = ProcessEscapeSymbols(_settings.ColumnSeparator);
                string clearedCsv = _csv.Trim();
                OutputData = _csv;
                // checks if rowSeperator is in the input, if not OneRowMessage is shown
                if (!clearedCsv.Contains(rowSeperator))
                {
                    GuiLogMessage(Properties.Resources.OneRowMessage, NotificationLevel.Warning);
                    return;
                }

                // If either row separator or column seperator are not in the input warning shown
                if (!_csv.Contains(rowSeperator) || !_csv.Contains(columnSeperator))
                {
                    GuiLogMessage(Properties.Resources.NoSeperatorMessage, NotificationLevel.Warning);
                    return;
                }
                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    _presentation.ClearPresentation();
                    _presentation.CreateDataTableAndComboboxes(_csv, rowSeperator, columnSeperator);
                    _presentation.DataTableChanged += DataTableChanged;

                }, null);
            }
            catch (Exception ex)
            {

                GuiLogMessage(string.Format(Properties.Resources.ExceptionMessage, ex.Message), NotificationLevel.Error);
            }
            ProgressChanged(0, 1);
            OnPropertyChanged("");
            ProgressChanged(1, 1);
        }

        /// <summary>
        ///  if the datatable changes send the new table state to the output
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataTableChanged(object sender, EventArgs e)
        {
            var dataTable = _presentation.table;
            StringBuilder csv = new StringBuilder();

            foreach (DataGridColumn column in dataTable.Columns)
            {
                if (column.Visibility == Visibility.Visible)
                {
                    csv.Append(column.Header);
                    csv.Append(",");
                }
            }

            csv.AppendLine();

            foreach (DataRowView rowView in dataTable.Items)
            {
                DataRow row = rowView.Row;
                foreach (DataColumn column in row.Table.Columns)
                {
                    if (column.Ordinal < dataTable.Columns.Count && dataTable.Columns[column.Ordinal].Visibility == Visibility.Visible)
                    {
                        csv.Append(row[column.ColumnName]);
                        csv.Append(",");
                    }
                }

                csv.AppendLine();
            }
            OutputData = csv.ToString();
        }

        /// <summary>
        ///  escape symbols
        /// </summary>
        /// <param name="p"></param>
        private string ProcessEscapeSymbols(string p)
        {
            return p.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\b", "\b").Replace("\\t", "\t").Replace("\\v", "\v").Replace("\\", "\\");
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _presentation.table.Columns.Clear();
                _presentation.ClearPresentation();
            }, null);

        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}