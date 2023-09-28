/*
   Copyright 2014 Nils Rehwald

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
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace CrypTool.M138
{
    [Author("Nils Rehwald", "nilsrehwald@gmail.com", "Uni Kassel", "https://www.ais.uni-kassel.de")]
    [PluginInfo("CrypTool.M138.Properties.Resources", "PluginCaption", "PluginTooltip", "M138/userdoc.xml", "M138/icon.png")]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class M138 : ICrypComponent
    {
        #region Private Variables

        private readonly M138Settings settings = new M138Settings();
        private readonly M138Visualisation visualisation = new M138Visualisation();

        private enum Commands { Encrypt, Decrypt };
        private bool _stopped = true;
        private string[,] toVisualize;
        private string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private List<string> strips = new List<string>();
        private int[] TextNumbers;
        private int _offset;
        private int[] _stripNumbers = null;
        private List<int[]> numStrips = new List<int[]>();
        private List<string> _ignoredCharacters = new List<string>();
        private string[,] tmpToVis;
        private string[] colNames;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "TextInputCaption", "TextInputTooltip")]
        public string TextInput
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "KeyCaption", "KeyTooltip")]
        public string Key
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "StripsCaption", "StripsTooltip", false)]
        public string Strips
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "TextOutputCaption", "TextOutputTooltip")]
        public string TextOutput
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings => settings;

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation => visualisation;

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            _stopped = false;
            try
            {
                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    Presentation.Visibility = Visibility.Visible;
                }, null);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            strips = SetStrips(string.IsNullOrEmpty(Strips) ? LoadStrips() : Strips);
            if (!CheckStrips(strips))
            {
                return;
            }

            ProgressChanged(0, 1);

            //Invalid character handling
            if (settings.InvalidCharacterHandling == 0) //Remove
            {
                TextInput = RemoveInvalidChars(TextInput, Alphabet);
            }
            else
            {
                _ignoredCharacters = new List<string>();
            }

            if (!splitKey())
            {
                return;
            }

            TextNumbers = MapTextIntoNumberSpace(TextInput.ToUpper(), Alphabet, settings.InvalidCharacterHandling);

            if (_offset > Alphabet.Length)
            {
                GuiLogMessage("Offset " + _offset + " is larger than strip length " + Alphabet.Length + " and will be truncated", NotificationLevel.Warning);
                _offset %= Alphabet.Length;
            }

            DeEnCrypt(settings.Action);

            OnPropertyChanged("TextOutput");
            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            _stopped = true;
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            _stopped = true;
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
            try
            {
                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    Presentation.Visibility = Visibility.Hidden;
                }, null);
            }
            catch (Exception)
            {
            }
            visualisation.IsVisibleChanged += visibilityHasChanged;
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

        #region Helpers

        private List<string> SetStrips(string text)
        {
            return text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        private string LoadStrips()
        {
            return File.ReadAllText(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, "stripes.txt"));
        }

        private bool CheckStrips(List<string> strips)
        {
            if (strips == null || strips.Count == 0)
            {
                GuiLogMessage("The strips are undefined.", NotificationLevel.Error);
                return false;
            }

            Alphabet = string.Concat(strips[0].OrderBy(c => c).Distinct());

            foreach (string strip in strips)
            {
                string uniq = string.Concat(strip.OrderBy(c => c).Distinct());

                if (uniq.Length != strip.Length)
                {
                    GuiLogMessage("Error in strip '" + strip + "'. It contains duplicates.", NotificationLevel.Error);
                    return false;
                }

                if (uniq != Alphabet)
                {
                    GuiLogMessage("Error in strip '" + strip + "'. It uses a character set that differs from the first strip.", NotificationLevel.Error);
                    return false;
                }
            }

            return true;
        }

        private string RemoveInvalidChars(string text, string alphabet)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char c in text)
            {
                if (alphabet.Contains(c.ToString()) | alphabet.Contains(c.ToString().ToUpper()))
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }

        private int[] MapTextIntoNumberSpace(string text, string alphabet, int inv)
        {
            int[] numbers = new int[text.Length];
            int position = 0;

            if (inv == 0)
            {
                foreach (char c in text)
                {
                    numbers[position] = alphabet.IndexOf(c);
                    position++;
                }
            }
            else
            {
                foreach (char c in text)
                {
                    if (alphabet.Contains(c.ToString()))
                    {
                        numbers[position] = alphabet.IndexOf(c);
                    }
                    else
                    {
                        numbers[position] = -1;
                        if (inv == 1)
                        {
                            _ignoredCharacters.Add(c.ToString());
                        }
                    }
                    position++;
                }
            }

            return numbers;
        }

        private string MapNumbersIntoTextSpace(int[] numbers, string alphabet, int inv)
        {
            StringBuilder builder = new StringBuilder();
            int counter = 0;

            if (inv == 0)
            {
                foreach (char c in numbers)
                {
                    builder.Append(alphabet[c]);
                }
            }
            else
            {
                foreach (char c in numbers)
                {
                    if (c == 65535)
                    {
                        if (inv == 1)
                        {
                            builder.Append(_ignoredCharacters[counter]);
                            counter++;
                        }
                        else
                        {
                            builder.Append('?');
                        }
                    }
                    else
                    {
                        builder.Append(alphabet[c]);
                    }
                }
            }

            return builder.ToString();
        }

        private void DeEnCrypt(int deOrEncrypt)
        {
            int _rows = TextNumbers.Length;
            int _columns = strips[0].Length;
            int[] output = new int[_rows];
            toVisualize = new string[_rows + 1, _columns + 2];

            toVisualize[0, 1] = "Strip"; ;  //Top Left field
            toVisualize[0, 0] = "Row";      //Top right field

            //Create a List of all used Strips mapped to numbers instead of characters
            numStrips = _stripNumbers.Select(i => MapTextIntoNumberSpace(strips[i], Alphabet, settings.InvalidCharacterHandling)).ToList();

            for (int c = 0; c < _columns; c++)
            {
                toVisualize[0, c + 2] = c.ToString(); //First row of Visualisation
            }

            int r_counter = 0;
            for (int r = 0; r < _rows; r++)
            {
                int _usedStrip = r_counter % _stripNumbers.Length;
                toVisualize[r + 1, 0] = (r + 1).ToString(); //Fill first column of Visualisation
                toVisualize[r + 1, 1] = _stripNumbers[_usedStrip].ToString(); //Fill second column of Visualisation
                int[] currentStrip = numStrips[_usedStrip];

                int isAt = (TextNumbers[r] < 0) ? -1 : Array.IndexOf(currentStrip, TextNumbers[r]); // Location of the Plaintext letter

                if (isAt == -1)
                {
                    for (int c = 0; c < _columns; c++)
                    {
                        toVisualize[r + 1, c + 2] = "?";
                    }

                    output[r] = -1;
                }
                else
                {
                    if (deOrEncrypt == (int)Commands.Encrypt)
                    {
                        for (int c = 0; c < _columns; c++)
                        {
                            toVisualize[r + 1, c + 2] = Alphabet[currentStrip[(isAt + c) % currentStrip.Length]].ToString();
                        }

                        output[r] = currentStrip[(isAt + _offset) % Alphabet.Length];
                    }
                    else
                    {
                        for (int c = 0; c < _columns; c++)
                        {
                            toVisualize[r + 1, c + 2] = Alphabet[currentStrip[(isAt + Alphabet.Length - c) % currentStrip.Length]].ToString();
                        }

                        output[r] = currentStrip[(isAt + Alphabet.Length - _offset) % Alphabet.Length];
                    }
                    r_counter++;
                }
            }


            //Column Headers for Visualisation
            colNames = new string[_columns + 2];
            for (int i = 0; i < _columns + 2; i++)
            {
                colNames[i] = toVisualize[0, i];
            }

            tmpToVis = new string[_rows, _columns + 2];
            for (int i = 0; i < (_rows); i++)
            {
                for (int j = 0; j < _columns + 2; j++)
                {
                    tmpToVis[i, j] = toVisualize[i + 1, j];
                }
            }

            string tmp = MapNumbersIntoTextSpace(output, Alphabet, settings.InvalidCharacterHandling);

            if (settings.CaseSensitivity)
            {
                tmp = new string(tmp.Select((c, k) => char.IsLower(TextInput[k]) ? char.ToLower(c) : c).ToArray());
            }

            TextOutput = tmp;

            if (visualisation.IsVisible)
            {
                UpdateGUI();
            }
        }

        private void UpdateGUI()
        {
            try
            {
                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        Binding2DArrayToListView(visualisation.lvwArray, tmpToVis, colNames);
                    }
                    catch (Exception)
                    {
                        //GuiLogMessage(e.StackTrace, NotificationLevel.Error);
                    }
                }, null);
            }
            catch (Exception)
            {
            }
        }

        private void visibilityHasChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (visualisation.IsVisible)
            {
                UpdateGUI();
            }
        }

        private bool splitKey()
        {
            try
            {
                char sep = "/,."[settings.SeparatorOffChar];

                if (Key.IndexOf(sep) < 0)
                {
                    throw new Exception("The key contains no offset separator '" + sep + "'.");
                }

                string[] splitted;
                splitted = Key.Split(sep);
                _offset = Convert.ToInt32(splitted[1]);

                sep = ",./"[settings.SeparatorStripChar];

                List<int> list = new List<int>();
                foreach (string ofs in splitted[0].Split(sep))
                {
                    int n = Convert.ToInt32(ofs);
                    if (n > strips.Count)
                    {
                        GuiLogMessage("Selected strip " + n + " is larger than the number of available strips " + strips.Count + ". Using default strip 1 instead.", NotificationLevel.Warning);
                        n = 1;
                    }
                    list.Add(n);
                }

                _stripNumbers = list.ToArray();
            }
            catch (Exception ex)
            {
                GuiLogMessage("Error while parsing key: " + ex.Message, NotificationLevel.Error);
                return false;
            }

            return true;
        }

        private void Binding2DArrayToListView(DataGrid dataGrid, string[,] data, string[] columnNames)
        {
            dataGrid.AutoGeneratingColumn += dgvMailingList_AutoGeneratingColumn;
            Check2DArrayMatchColumnNames(data, columnNames);
            DataTable dt = Convert2DArrayToDataTable(data, columnNames);
            dataGrid.ItemsSource = dt.DefaultView;
        }

        private void dgvMailingList_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.Equals("0"))
            {
                e.Column.CellStyle = new Style(typeof(DataGridCell));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.BackgroundProperty, new SolidColorBrush(Colors.Green)));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.FontWeightProperty, FontWeights.Bold));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.PaddingProperty, new Thickness(10)));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.HorizontalAlignmentProperty, HorizontalAlignment.Stretch));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.VerticalAlignmentProperty, VerticalAlignment.Stretch));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.VerticalContentAlignmentProperty, VerticalAlignment.Center));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.MinHeightProperty, double.Parse("30")));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.MinWidthProperty, double.Parse("30")));
            }
            else if (e.Column.Header.Equals(_offset.ToString()))
            {
                e.Column.CellStyle = new Style(typeof(DataGridCell));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.BackgroundProperty, new SolidColorBrush(Colors.Red)));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.FontWeightProperty, FontWeights.Bold));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.PaddingProperty, new Thickness(10)));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.HorizontalAlignmentProperty, HorizontalAlignment.Stretch));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.VerticalAlignmentProperty, VerticalAlignment.Stretch));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.VerticalContentAlignmentProperty, VerticalAlignment.Center));
            }

            return;
        }

        private DataTable Convert2DArrayToDataTable(string[,] data, string[] columnNames)
        {
            int len1d = data.GetLength(0);
            int len2d = data.GetLength(1);
            Check2DArrayMatchColumnNames(data, columnNames);

            DataTable dt = new DataTable();
            for (int i = 0; i < len2d; i++)
            {
                dt.Columns.Add(columnNames[i], typeof(string));
            }

            for (int row = 0; row < len1d; row++)
            {
                DataRow dr = dt.NewRow();
                for (int col = 0; col < len2d; col++)
                {
                    dr[col] = data[row, col];
                }
                dt.Rows.Add(dr);
            }

            return dt;
        }

        private void Check2DArrayMatchColumnNames(string[,] data, string[] columnNames)
        {
            int len2d = data.GetLength(1);

            if (len2d != columnNames.Length)
            {
                throw new Exception("The second dimensional length must equals column names.");
            }
        }

        #endregion
    }
}