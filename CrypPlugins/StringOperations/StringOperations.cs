/*                              
   Copyright 2011 Nils Kopal, Uni Duisburg-Essen

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
using StringOperations.Properties;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace StringOperations
{
    [Author("Nils Kopal", "nils.kopal@CrypTool.org", "Uni Duisburg", "http://www.uni-duisburg-essen.de")]
    [PluginInfo("StringOperations.Properties.Resources", "PluginCaption", "PluginTooltip", "StringOperations/DetailedDescription/doc.xml", "StringOperations/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class StringOperations : ICrypComponent
    {
        private readonly StringOperationsSettings _settings = null;

        private string _string1;
        private string _string2;
        private string _string3;
        private int _value1;
        private int _value2;
        private string _outputString;
        private int _outputValue;
        private string[] _outputStringArray;

        public StringOperations()
        {
            _settings = new StringOperationsSettings();
        }

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public ISettings Settings => _settings;

        public System.Windows.Controls.UserControl Presentation => null;

        public void PreExecution()
        {
            _string1 = null;
            _string2 = null;
            _string3 = null;
            _value1 = -1;
            _value2 = -1;

        }

        public void Execute()
        {

            //If connector values are not set, maybe the user set these values in the settings
            //So we replace the connector values with the setting values:
            if (string.IsNullOrEmpty(_string1))
            {
                _string1 = _settings.String1;
            }
            if (string.IsNullOrEmpty(_string2))
            {
                _string2 = _settings.String2;
            }
            if (string.IsNullOrEmpty(_string3))
            {
                _string3 = _settings.String3;
            }
            if (_value1 == -1)
            {
                _value1 = _settings.Value1;
            }
            if (_value2 == -1)
            {
                _value2 = _settings.Value2;
            }

            try
            {
                switch (_settings.Operation)
                {
                    case StringOperationType.Concatenate:
                        _outputString = string.Concat(_string1, _string2);
                        OnPropertyChanged(nameof(OutputString));
                        break;
                    case StringOperationType.Substring:
                        if (_string1.Length > 0 && _value1 < 0 && _value1 >= -_string1.Length)
                        {
                            _value1 = (_value1 + _string1.Length) % _string1.Length;
                        }

                        _outputString = _string1.Substring(_value1, _value2);
                        OnPropertyChanged(nameof(OutputString));
                        break;
                    case StringOperationType.ToUppercase:
                        _outputString = _string1.ToUpper();
                        OnPropertyChanged(nameof(OutputString));
                        break;
                    case StringOperationType.ToLowercase:
                        _outputString = _string1.ToLower();
                        OnPropertyChanged(nameof(OutputString));
                        break;
                    case StringOperationType.Length:
                        _outputValue = _string1.Length;
                        OnPropertyChanged(nameof(OutputValue));
                        break;
                    case StringOperationType.CompareTo:
                        _outputValue = _string1.CompareTo(_string2);
                        OnPropertyChanged(nameof(OutputValue));
                        break;
                    case StringOperationType.Trim:
                        _outputString = _string1.Trim();
                        OnPropertyChanged(nameof(OutputString));
                        break;
                    case StringOperationType.IndexOf:
                        _outputValue = _string1.IndexOf(_string2);
                        OnPropertyChanged(nameof(OutputValue));
                        break;
                    case StringOperationType.Equals:
                        _outputValue = (_string1.Equals(_string2) ? 1 : 0);
                        OnPropertyChanged(nameof(OutputValue));
                        break;
                    case StringOperationType.Replace:
                        _outputString = _string1.Replace(_string2, _string3);
                        OnPropertyChanged(nameof(OutputString));
                        break;
                    case StringOperationType.RegexReplace:
                        _outputString = Regex.Replace(_string1, _string2, _string3);
                        OnPropertyChanged(nameof(OutputString));
                        break;
                    case StringOperationType.Split:
                        _outputStringArray = _string2 != null && _string2.Length == 0
                            ? _string1.Select(c => c + "").ToArray()
                            : _string1.Split((_string2 ?? "\r\n").ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        OnPropertyChanged(nameof(OutputStringArray));
                        _outputValue = _outputStringArray?.Length ?? 0;
                        OnPropertyChanged(nameof(OutputValue));
                        break;
                    case StringOperationType.Block:
                        if (_settings.Blocksize == 0)
                        {
                            GuiLogMessage("Blocksize is '0'. Set blocksize to '1'", NotificationLevel.Warning);
                            _settings.Blocksize = 1;
                        }
                        string str = Regex.Replace(_string1, @"\s+", "");
                        StringBuilder builder = new StringBuilder();
                        for (int i = 0; i < str.Length; i += _settings.Blocksize)
                        {
                            if (i <= str.Length - _settings.Blocksize)
                            {
                                builder.Append(str.Substring(i, _settings.Blocksize) + " ");
                            }
                        }
                        if (str.Length % _settings.Blocksize != 0)
                        {
                            builder.Append(str, str.Length - str.Length % _settings.Blocksize, str.Length % _settings.Blocksize);
                        }
                        _outputString = builder.ToString().Trim();
                        OnPropertyChanged(nameof(OutputString));
                        break;
                    case StringOperationType.Reverse:
                        char[] arr = _string1.ToCharArray();
                        Array.Reverse(arr);
                        _outputString = new string(arr);
                        OnPropertyChanged(nameof(OutputString));
                        break;
                    case StringOperationType.Sort:
                        char[] sortarr = _string1.ToCharArray();
                        Array.Sort(sortarr);
                        if (_settings.Order == 1)
                        {
                            Array.Reverse(sortarr);
                        }
                        _outputString = new string(sortarr);
                        OnPropertyChanged(nameof(OutputString));
                        break;
                    case StringOperationType.Distinct:
                        _outputString = string.Concat(_string1.Distinct());
                        OnPropertyChanged(nameof(OutputString));
                        break;
                    case StringOperationType.LevenshteinDistance:
                        _outputValue = LevenshteinDistance(_string1, _string2);
                        OnPropertyChanged(nameof(OutputValue));
                        break;
                    case StringOperationType.Shuffle:
                        _outputString = Shuffle(_string1);
                        OnPropertyChanged(nameof(OutputString));
                        break;
                }
                ProgressChanged(1, 1);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format(Resources.StringOperations_Execute_Could_not_execute_operation___0______1_, (_settings).Operation, ex.Message), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Shuffles the given string using Fisher–Yates_shuffle
        /// See https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
        /// </summary>
        /// <param name="string"></param>
        /// <returns></returns>
        private string Shuffle(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }
            Random random = new Random();
            char[] array = str.ToCharArray();
            for(int i = array.Length - 1; i >= 0; i--)
            {
                int j = random.Next(0, i);
                (array[i], array[j]) = (array[j], array[i]);
            }
            return new string(array);
        }

        /// <summary>
        /// Calculates the Levenshtein distance of 2 given strings
        /// Source from Wikipedia: http://en.wikipedia.org/wiki/Levenshtein_distance#Iterative_with_two_matrix_rows
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        private int LevenshteinDistance(string s, string t)
        {
            // degenerate cases
            if (s == t)
            {
                return 0;
            }
            if (string.IsNullOrEmpty(s) || s.Length == 0)
            {
                return t.Length;
            }
            if (string.IsNullOrEmpty(t) || t.Length == 0)
            {
                return s.Length;
            }

            // create two work vectors of integer distances
            int[] v0 = new int[t.Length + 1];
            int[] v1 = new int[t.Length + 1];

            // initialize v0 (the previous row of distances)
            // this row is A[0][i]: edit distance for an empty s
            // the distance is just the number of characters to delete from t
            for (int i = 0; i < v0.Length; i++)
            {
                v0[i] = i;
            }

            for (int i = 0; i < s.Length; i++)
            {
                // calculate v1 (current row distances) from the previous row v0

                // first element of v1 is A[i+1][0]
                //   edit distance is delete (i+1) chars from s to match empty t
                v1[0] = i + 1;

                // use formula to fill in the rest of the row
                for (int j = 0; j < t.Length; j++)
                {
                    int cost = (s[i] == t[j]) ? 0 : 1;
                    v1[j + 1] = Minimum(v1[j] + 1, v0[j + 1] + 1, v0[j] + cost);
                }

                // copy v1 (current row) to v0 (previous row) for next iteration
                for (int j = 0; j < v0.Length; j++)
                {
                    v0[j] = v1[j];
                }
            }

            return v1[t.Length];
        }

        /// <summary>
        /// Returns the minimum of 3 ints
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        private int Minimum(int a, int b, int c)
        {
            int x = a;
            if (b < x)
            {
                x = b;
            }
            if (c < x)
            {
                x = c;
            }
            return x;
        }

        public void PostExecution()
        {

        }

        public void Stop()
        {

        }

        public void Initialize()
        {
            _settings.UpdateTaskPaneVisibility();
        }

        public void Dispose()
        {

        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        [PropertyInfo(Direction.InputData, "String1Caption", "String1Tooltip", false)]
        public string String1
        {
            get => _string1;
            set => _string1 = value;
        }

        [PropertyInfo(Direction.InputData, "String2Caption", "String2Tooltip", false)]
        public string String2
        {
            get => _string2;
            set => _string2 = value;
        }

        [PropertyInfo(Direction.InputData, "String3Caption", "String3Tooltip", false)]
        public string String3
        {
            get => _string3;
            set => _string3 = value;
        }


        [PropertyInfo(Direction.InputData, "Value1Caption", "Value1Tooltip", false)]
        public int Value1
        {
            get => _value1;
            set => _value1 = value;
        }

        [PropertyInfo(Direction.InputData, "Value2Caption", "Value2Tooltip", false)]
        public int Value2
        {
            get => _value2;
            set => _value2 = value;
        }

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", false)]
        public string OutputString => _outputString;

        [PropertyInfo(Direction.OutputData, "OutputValueCaption", "OutputValueTooltip", false)]
        public int OutputValue => _outputValue;

        [PropertyInfo(Direction.OutputData, "OutputStringArrayCaption", "OutputStringArrayTooltip", false)]
        public string[] OutputStringArray => _outputStringArray;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, logLevel));
            }
        }

        public void ProgressChanged(double value, double max)
        {
            if (OnPluginProgressChanged != null)
            {
                OnPluginProgressChanged(this, new PluginProgressEventArgs(value, max));
            }
        }
    }
}
