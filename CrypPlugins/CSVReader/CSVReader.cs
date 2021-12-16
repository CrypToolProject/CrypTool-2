/*                              
   Copyright 2013 Nils Kopal, Universität Kassel

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
using System.Linq;
using System.Text;

namespace CSVReader
{
    [Author("Nils Kopal", "Nils.Kopal@CrypTool.org", "Universität Kassel", "http://www.ais.uni-kassel.de")]
    [PluginInfo("CSVReader.Properties.Resources", "PluginCaption", "PluginTooltip", "CSVReader/DetailedDescription/doc.xml", "CSVReader/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataflow)]
    public class CSVReader : ICrypComponent
    {
        private string _csv;
        private string _output;

        private readonly CSVReaderSettings _settings = new CSVReaderSettings();

        [PropertyInfo(Direction.InputData, "InputCSVCaption", "InputCSVTooltip", true)]
        public string InputCSV
        {
            get => _csv;
            set
            {
                _csv = value;
                OnPropertyChanged("InputCSV");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputDataCaption", "OutputDataTooltip", true)]
        public string OutputData
        {
            get => _output;
            set
            {
                _output = value;
                OnPropertyChanged("OutputData");
            }
        }

        public void PreExecution()
        {

        }

        public void PostExecution()
        {

        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public ISettings Settings => _settings;

        public System.Windows.Controls.UserControl Presentation => null;

        public void Execute()
        {
            if (string.IsNullOrEmpty(_csv))
            {
                GuiLogMessage("Empty CSV. Can not process.", NotificationLevel.Warning);
                return;
            }

            try
            {
                string rowSeperator = ProcessEscapeSymbols(_settings.RowSeparator);
                string columnSeperator = ProcessEscapeSymbols(_settings.ColumnSeparator);
                StringBuilder output = new StringBuilder();
                string[] rows = _csv.Split(rowSeperator.ToArray(), StringSplitOptions.RemoveEmptyEntries);

                int noColumnCounter = 0;
                int rowid = 0;
                foreach (string row in rows)
                {
                    string[] columns = row.Split(columnSeperator.ToArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (columns.Length - 1 < _settings.ComlumnID)
                    {
                        noColumnCounter++;
                        if (noColumnCounter < 10)
                        {
                            GuiLogMessage(string.Format("Row {0} has no column with id {1}: \"{2}\"", rowid, _settings.ComlumnID, row), NotificationLevel.Warning);
                        }
                        else if (noColumnCounter == 10)
                        {
                            GuiLogMessage(string.Format("More than 10 rows have no column with id {0}.", _settings.ComlumnID), NotificationLevel.Warning);
                        }
                    }
                    else
                    {
                        output.Append(columns[_settings.ComlumnID]);
                        if (rowid < rows.Length - 1)
                        {
                            output.Append(ProcessEscapeSymbols(_settings.ResultSeparator));
                        }
                    }
                    rowid++;
                }
                OutputData = output.ToString();
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception occured during processing of CSV: {0}", ex.Message), NotificationLevel.Error);
            }

        }

        private string ProcessEscapeSymbols(string p)
        {
            return p.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\b", "\b").Replace("\\t", "\t").Replace("\\v", "\v").Replace("\\", "\\");
        }

        public void Stop()
        {

        }

        public void Initialize()
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Dispose()
        {

        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }
    }
}
