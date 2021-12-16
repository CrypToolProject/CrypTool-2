/*
   Copyright CrypTool 2 Team <ct2contact@cryptool.org>

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
using CrypTool.StraddlingCheckerboard.Properties;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.StraddlingCheckerboard
{
    [Author("Niklas Weimann", "niklas.weimann@student.uni-siegen.de", "CrypTool 2 Team", "https://www.cryptool.org")]
    [PluginInfo("CrypTool.StraddlingCheckerboard.Properties.Resources", "StraddlingCheckerboard",
        "StraddlingCheckerboardTooltip", "StraddlingCheckerboard/userdoc.xml",
        new[] { "StraddlingCheckerboard/Images/icon.png" })]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class StraddlingCheckerboard : ICrypComponent
    {
        #region Private Variables

        private readonly StraddlingCheckerboardSettings _settings = new StraddlingCheckerboardSettings();
        private readonly StraddlingCheckerboardPresentation _straddlingCheckerboardPresentation;
        private Checkerboard _checkerboard;
        private double _process;
        private string _outputText;
        private bool _running;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputTextCaption", "InputTextTooltip", true)]
        public string InputText { get; set; }

        [PropertyInfo(Direction.InputData, "InputContentCaption", "InputContentTooltip")]
        public string Content
        {
            get => _settings.Content;
            set
            {
                if (value == null || value == _settings.Content)
                {
                    return;
                }

                _settings.Content = value;
                OnPropertyChanged(nameof(Content));
            }
        }

        [PropertyInfo(Direction.InputData, "InputRowsColumnsCaption", "InputRowsColumnsTooltip")]
        public string RowsColumns
        {
            get => _settings.RowsColumns;
            set
            {
                if (value == null || value == _settings.RowsColumns)
                {
                    return;
                }

                _settings.RowsColumns = value;
                OnPropertyChanged(nameof(RowsColumns));
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputTextCaption", "OutputTextTooltip")]
        public string OutputText
        {
            get => _outputText;
            set
            {
                _outputText = value;
                OnPropertyChanged(nameof(OutputText));
            }
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings => _settings;

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation => _straddlingCheckerboardPresentation;

        public StraddlingCheckerboard()
        {
            _straddlingCheckerboardPresentation = new StraddlingCheckerboardPresentation();
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            _running = true;
        }

        private void UpdatePresentation(DataTable tableContent, List<int?> checkerboardRows)
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal,
                (SendOrPostCallback)delegate
               {
                   _straddlingCheckerboardPresentation.ShowCheckerboard(tableContent, checkerboardRows);
               }, null);
        }


        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);

            string rows = GetRowValue();
            string columns = GetColumnValue();
            string table = GetContentValue();

            (bool isValid, string message) = ValidateBoardConfiguration(columns, rows, table);
            if (!isValid)
            {
                GuiLogMessage(message, NotificationLevel.Error);
                return;
            }

            if (!_running)
            {
                return;
            }

            _checkerboard = new Checkerboard(rows, columns);
            _checkerboard.FillTable(table);
            UpdatePresentation(_checkerboard.TableContent, _checkerboard.Rows);

            ProgressChanged(0.5, 1);
            if (!_running)
            {
                return;
            }
            string result = _settings.Cipher == StraddlingCheckerboardSettings.StraddlingCheckerBoardMode.Encrypt
                ? Encipher(InputText)
                : Decipher(InputText);

            if (_running)
            {
                OutputText = result;
            }
            ProgressChanged(1, 1);
        }

        private (bool isValid, string Message) ValidateBoardConfiguration(string columns, string rows, string table)
        {
            bool IsUnique(string s)
            {
                return s.Length != s.Distinct().Count();
            }

            bool ContainsOnlyNumbers(string s)
            {
                return !s.All(x => int.TryParse(x.ToString(), out _));
            }

            bool IsNullOrEmpty(params string[] s)
            {
                return s.Any(string.IsNullOrEmpty);
            }

            if (IsNullOrEmpty(columns, rows))
            {
                return (false, string.Format(Resources.CouldNotParseValues, columns, rows));
            }

            if (IsUnique(columns))
            {
                return (false, Resources.DuplicateCharInColumnNotAllowed);
            }

            if (IsUnique(rows))
            {
                return (false, Resources.DuplicateCharInRowNotAllowed);
            }

            if (IsUnique(table))
            {
                return (false, Resources.DuplicateCharNotAllowed);
            }

            if (IsNullOrEmpty(table))
            {
                return (false, Resources.ContentCanNotBeEmpty);
            }

            int theoreticalMaximumAmountOfSpace = (rows.Length + 1) * columns.Length - rows.Length;
            if (theoreticalMaximumAmountOfSpace < table.Length)
            {
                return (false, Resources.NotEnoughSpace);
            }

            if (ContainsOnlyNumbers(rows))
            {
                return (false, Resources.OnlyNumbersForRows);
            }

            if (ContainsOnlyNumbers(columns))
            {
                return (false, Resources.OnlyNumbersForColumns);
            }

            return (true, string.Empty);
        }

        public string Encipher(string s)
        {
            StringBuilder res = new StringBuilder();
            char[] charArray = s.ToCharArray();
            foreach (char c in charArray)
            {
                string cs = c.ToString();

                if (!_running)
                {
                    return string.Empty;
                }

                if (_checkerboard.GetRowAndColumnByChar(cs, out (string row, string column) result))
                {
                    res.Append($"{result.row}{result.column}");
                }
                else
                {
                    // We can not encipher this char so we append it as is in the plain text and give a
                    // warning message about it.
                    res.Append(cs);
                    GuiLogMessage(string.Format(Resources.CharNotInBoard, c), NotificationLevel.Warning);
                }

                ProgressChanged(UpdateProcess(charArray.Length), 1);
            }

            return res.ToString();
        }

        /// <summary>
        /// Deciphers a given cipher text based on the Straddling Checkerboard associated with the object.
        /// </summary>
        /// <param name="cipherText">The cipher text that should be deciphered.</param>
        /// <returns>The result of the deciphering process.</returns>
        public string Decipher(string cipherText)
        {
            StringBuilder res = new StringBuilder();
            string[] cipher = cipherText.Where(c => !char.IsControl(c)).Select(x => x.ToString()).ToArray();
            for (int i = 0; i < cipher.Length && _running; i++)
            {
                string c = cipher[i];
                string k;
                if (!int.TryParse(c, out int char1))
                {
                    GuiLogMessage(string.Format(Resources.CharNotInBoard, c), NotificationLevel.Warning);
                    continue;
                }

                if (_checkerboard.Rows.Contains(char1))
                {
                    k = _checkerboard[c, cipher[i + 1]];
                    i++;
                }
                else
                {
                    k = _checkerboard[c];
                }

                res.Append(k);
                ProgressChanged(UpdateProcess(cipher.Length), 1);
            }

            return res.ToString();
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal,
                (SendOrPostCallback)delegate { _straddlingCheckerboardPresentation.DisableCheckerboard(); }, null);
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            _checkerboard = null;
            _running = false;
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

        private double UpdateProcess(int cipherLength)
        {
            double oldProcess = _process;
            _process += GetStepSize(cipherLength);
            return oldProcess;
        }

        private double GetStepSize(int count)
        {
            return (1 - _process) / count;
        }

        #endregion

        #region DetermineValue

        private string GetRowValue()
        {
            return SplitRowsColumns(string.IsNullOrEmpty(RowsColumns) ? _settings.RowsColumns : RowsColumns).Rows;
        }

        private string GetColumnValue()
        {
            return SplitRowsColumns(string.IsNullOrEmpty(RowsColumns) ? _settings.RowsColumns : RowsColumns).Columns;
        }

        private string GetContentValue()
        {
            return string.IsNullOrEmpty(Content) ? _settings.Content : Content;
        }

        private (string Rows, string Columns) SplitRowsColumns(string src)
        {
            char[] spitCharArray = new[] { '\n', ';', ' ' };
            string[] rowsColumns = src.Split(spitCharArray);
            return rowsColumns.Length == 2 ? (rowsColumns[0], rowsColumns[1]) : (null, null);
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
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