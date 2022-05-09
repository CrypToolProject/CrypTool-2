/*
   Copyright 2008-2020 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using CrypTool.PluginBase.Control;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Transposition
{
    [Author("Daniel Kohnen, Julian Weyers, Simon Malischewski, Armin Wiefels", "kohnen@CrypTool.org, weyers@CrypTool.org, malischewski@CrypTool.org, wiefels@CrypTool.org", "Universität Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("Transposition.Properties.Resources", "PluginCaption", "PluginTooltip", "Transposition/DetailedDescription/doc.xml", "Transposition/Images/icon.png", "Transposition/Images/encrypt.png", "Transposition/Images/decrypt.png")]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class Transposition : ICrypComponent
    {
        # region Private variables

        private string _keyword = string.Empty;
        private string _input;
        private string _outputvalue;
        private char[] _output;
        private TranspositionSettings _settings;
        private readonly TranspositionPresentation _presentation;
        private bool _running = false;
        private bool _stopped = false;

        # endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public Transposition()
        {
            _settings = new TranspositionSettings();
            _presentation = new TranspositionPresentation();
            Presentation = _presentation;
            _presentation.Transposition = this;
            _presentation.feuerEnde += new EventHandler(presentation_finished);
            _presentation.updateProgress += new EventHandler(update_progress);
            _settings.PropertyChanged += settings_OnPropertyChange;
        }

        private void update_progress(object sender, EventArgs e)
        {
            ProgressChanged(_presentation.progress, 3000);
        }

        private void presentation_finished(object sender, EventArgs e)
        {
            if (!_presentation.Stop)
            {
                Output = new string(_output);
            }

            ProgressChanged(1, 1);

            _running = false;
        }

        private void settings_OnPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            _presentation.UpdateSpeed(_settings.PresentationSpeed);
        }
        /// <summary>
        /// Get or set all settings for this algorithm.
        /// </summary>
        public ISettings Settings
        {
            get => _settings;
            set => _settings = (TranspositionSettings)value;
        }

        # region getter methods

        /// <summary>
        /// Get read in matrix
        /// </summary>
        public char[,] Read_in_matrix { get; private set; }

        /// <summary>
        /// Get permuted matrix
        /// </summary>
        public char[,] Permuted_matrix { get; private set; }

        /// <summary>
        /// Get numerical key order
        /// </summary>
        public int[] Key { get; private set; }
        # endregion

        # region Properties

        [PropertyInfo(Direction.InputData, "InputCaption", "InputTooltip", true)]
        public string Input
        {
            get => _input;

            set
            {
                _input = value;
                OnPropertyChange("Input");
            }
        }

        [PropertyInfo(Direction.InputData, "KeywordCaption", "KeywordTooltip", true)]
        public string Keyword
        {
            get => _keyword;

            set
            {
                _keyword = value;
                OnPropertyChange("Keyword");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip")]
        public string Output
        {
            get => _outputvalue;

            set
            {
                _outputvalue = value;
                OnPropertyChange("Output");
            }
        }

        private void OnPropertyChange(string propertyname)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(propertyname));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        # endregion

        #region IPlugin Member

        public void Dispose()
        {

        }

        public void Execute()
        {

            while (_running)
            {
                _presentation.my_Stop(this, EventArgs.Empty);
                if (_stopped)
                {
                    return;
                }
            }

            _running = true;

            try
            {
                ProcessTransposition();
            }
            catch (Exception)
            {
                Transposition_LogMessage("Keyword is not valid", NotificationLevel.Error);
                Output = null;
                return;
            }

            if (controlSlave is object && Input is object)
            {
                ((TranspositionControl)controlSlave).onStatusChanged();
            }

            if (Presentation.IsVisible && Key.Count() != 0)
            {
                Transposition_LogMessage(Read_in_matrix.GetLength(0) + " " + Read_in_matrix.GetLength(1) + " " + Input.Length, NotificationLevel.Debug);
                Presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        _presentation.main(Read_in_matrix, Permuted_matrix, Key, Keyword, Input.ToCharArray(), _output, _settings.Permutation, _settings.ReadIn, _settings.ReadOut, _settings.Action, _settings.Number, _settings.PresentationSpeed);
                    }
                    catch (Exception ex)
                    {
                        Transposition_LogMessage(string.Format("Exception during run of Transposition Presentation: {0}", ex.Message), NotificationLevel.Error);
                    }
                }
                , null);
            }
            else
            {
                Output = new string(_output);
                ProgressChanged(1, 1);
            }
        }

        public void Initialize()
        {
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public void PostExecution()
        {

        }

        public void PreExecution()
        {
            _running = false;
            _stopped = false;

        }

        public UserControl Presentation
        {
            get;
            private set;
        }

        public void Stop()
        {

            _stopped = true;

            _presentation.my_Stop(this, EventArgs.Empty);
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        # region Private Methods

        private void ProcessTransposition()
        {
            if (_keyword.Contains(','))
            {
                Key = get_Keyword_Array(_keyword);
            }
            else
            {
                Key = sortKey(_keyword);
            }

            switch (_settings.Action)
            {
                case 0:
                    _output = encrypt(Input.ToCharArray(), Key);
                    break;
                case 1:
                    _output = decrypt(Input.ToCharArray(), Key);
                    break;
                default:
                    break;
            }
        }

        private char[] encrypt(char[] input, int[] key)
        {
            if (key != null && input != null && key.Length > 0)
            {
                if (is_Valid_Keyword(key))
                {
                    char[] encrypted = null;

                    if (((TranspositionSettings.PermutationMode)_settings.Permutation).Equals(TranspositionSettings.PermutationMode.byRow))
                    {
                        switch ((TranspositionSettings.ReadInMode)_settings.ReadIn)
                        {
                            case TranspositionSettings.ReadInMode.byRow:
                                Read_in_matrix = enc_read_in_by_row_if_row_perm(input, key.Length); break;
                            case TranspositionSettings.ReadInMode.byColumn:
                                Read_in_matrix = enc_read_in_by_column_if_row_perm(input, key.Length); break;
                            default:
                                break;
                        }

                        Permuted_matrix = enc_permute_by_row(Read_in_matrix, key);

                        switch ((TranspositionSettings.ReadOutMode)_settings.ReadOut)
                        {
                            case TranspositionSettings.ReadOutMode.byRow:
                                encrypted = read_out_by_row_if_row_perm(Permuted_matrix, key.Length); break;
                            case TranspositionSettings.ReadOutMode.byColumn:
                                encrypted = read_out_by_column_if_row_perm(Permuted_matrix, key.Length); break;
                            default:
                                break;
                        }
                    }

                    // permute by column:
                    else
                    {
                        switch ((TranspositionSettings.ReadInMode)_settings.ReadIn)
                        {
                            case TranspositionSettings.ReadInMode.byRow:
                                Read_in_matrix = enc_read_in_by_row(input, key.Length); break;
                            case TranspositionSettings.ReadInMode.byColumn:
                                Read_in_matrix = enc_read_in_by_column(input, key.Length); break;
                            default:
                                break;
                        }

                        Permuted_matrix = enc_permut_by_column(Read_in_matrix, key);

                        switch ((TranspositionSettings.ReadOutMode)_settings.ReadOut)
                        {
                            case TranspositionSettings.ReadOutMode.byRow:
                                encrypted = read_out_by_row(Permuted_matrix, key.Length); break;
                            case TranspositionSettings.ReadOutMode.byColumn:
                                encrypted = read_out_by_column(Permuted_matrix, key.Length); break;
                            default:
                                break;
                        }
                    }
                    return encrypted;
                }
                else
                {
                    Transposition_LogMessage("Keyword is not valid", NotificationLevel.Error);
                    return null;
                }
            }
            else
            {
                // 2do: Anzeige "Kein gültiges Keyword
                return null;
            }
        }

        public char[] decrypt(char[] input, int[] new_key)
        {
            if (new_key == null || input == null || new_key.Length <= 0)
            {
                // 2do: Anzeige "Kein gültiges Keyword
                return null;
            }

            if (!is_Valid_Keyword(new_key))
            {
                Transposition_LogMessage("Keyword is not valid", NotificationLevel.Error);
                return null;
            }

            char[] decrypted = null;

            if (((TranspositionSettings.PermutationMode)_settings.Permutation).Equals(TranspositionSettings.PermutationMode.byRow))
            {
                switch ((TranspositionSettings.ReadOutMode)_settings.ReadOut)
                {
                    case TranspositionSettings.ReadOutMode.byRow:
                        Read_in_matrix = dec_read_in_by_row_if_row_perm(input, new_key); break;
                    case TranspositionSettings.ReadOutMode.byColumn:
                        Read_in_matrix = dec_read_in_by_column_if_row_perm(input, new_key); break;
                    default:
                        break;
                }

                Permuted_matrix = dec_permut_by_row(Read_in_matrix, new_key);

                switch ((TranspositionSettings.ReadInMode)_settings.ReadIn)
                {
                    case TranspositionSettings.ReadInMode.byRow:
                        decrypted = read_out_by_row_if_row_perm(Permuted_matrix, new_key.Length); break;
                    case TranspositionSettings.ReadInMode.byColumn:
                        decrypted = read_out_by_column_if_row_perm(Permuted_matrix, new_key.Length); break;
                    default:
                        break;
                }
            }

            // permute by column:
            else
            {
                switch ((TranspositionSettings.ReadOutMode)_settings.ReadOut)
                {
                    case TranspositionSettings.ReadOutMode.byRow:
                        Read_in_matrix = dec_read_in_by_row(input, new_key); break;
                    case TranspositionSettings.ReadOutMode.byColumn:
                        Read_in_matrix = dec_read_in_by_column(input, new_key); break;
                    default:
                        break;
                }

                Permuted_matrix = dec_permut_by_column(Read_in_matrix, new_key);

                switch ((TranspositionSettings.ReadInMode)_settings.ReadIn)
                {
                    case TranspositionSettings.ReadInMode.byRow:
                        decrypted = read_out_by_row(Permuted_matrix, new_key.Length); break;
                    case TranspositionSettings.ReadInMode.byColumn:
                        decrypted = read_out_by_column(Permuted_matrix, new_key.Length); break;
                    default:
                        break;
                }
            }

            return decrypted;
        }

        private char[,] enc_read_in_by_row(char[] input, int keyword_length)
        {
            int size = input.Length / keyword_length;

            if (input.Length % keyword_length != 0)
            {
                size++;
            }

            int pos = 0;
            char[,] matrix = new char[keyword_length, size];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < keyword_length; j++)
                {
                    if (pos < input.Length)
                    {
                        matrix[j, i] = input[pos];
                        pos++;
                    }
                }
            }
            return matrix;
        }

        private char[,] enc_read_in_by_column(char[] input, int keyword_length)
        {
            int size = input.Length / keyword_length;
            int offs = input.Length % keyword_length;
            if (offs != 0)
            {
                size++;
            }

            int pos = 0;

            char[,] matrix = new char[keyword_length, size];
            for (int i = 0; i < keyword_length; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (pos < input.Length)
                    {
                        if (offs > 0 && j == size - 1 && i >= offs) { }
                        else
                        {
                            matrix[i, j] = input[pos];
                            pos++;
                        }
                    }
                }
            }
            return matrix;
        }

        private char[,] enc_read_in_by_row_if_row_perm(char[] input, int keyword_length)
        {
            int height = keyword_length;
            int length = input.Length / keyword_length;
            int offs = input.Length % keyword_length;
            if (offs != 0)
            {
                length++;
            }

            char[,] matrix = new char[length, height];
            int pos = 0;

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    if (pos < input.Length)
                    {
                        if (j.Equals(length - 1) && offs != 0)
                        {
                            if (i < offs)
                            {
                                matrix[j, i] = input[pos];
                                pos++;
                            }
                        }
                        else
                        {
                            matrix[j, i] = input[pos];
                            pos++;
                        }
                    }
                }
            }
            return matrix;
        }

        private char[,] enc_read_in_by_column_if_row_perm(char[] input, int keyword_length)
        {
            int height = keyword_length;
            int length = input.Length / keyword_length;
            int offs = input.Length % keyword_length;
            if (offs != 0)
            {
                length++;
            }

            char[,] matrix = new char[length, height];
            int pos = 0;

            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (pos < input.Length)
                    {
                        matrix[i, j] = input[pos];
                        pos++;
                    }
                }
            }
            return matrix;
        }

        private char[,] dec_read_in_by_column(char[] input, int[] keyword)
        {
            int size = input.Length / keyword.Length;
            int offs = input.Length % keyword.Length;
            if (offs != 0)
            {
                size++;
            }

            char[,] matrix = new char[keyword.Length, size];
            int pos = 0;

            for (int i = 0; i < keyword.Length; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (pos < input.Length)
                    {
                        if ((!offs.Equals(0)) && j.Equals(size - 1))
                        {
                            bool ok = false;
                            for (int k = 0; k < offs; k++)
                            {
                                if ((keyword[k] - 1).Equals(i))
                                {
                                    ok = true;
                                }
                            }
                            if (ok)
                            {
                                matrix[i, j] = input[pos];
                                pos++;
                            }
                        }
                        else
                        {
                            matrix[i, j] = input[pos];
                            pos++;
                        }
                    }
                }
            }
            return matrix;
        }

        private char[,] dec_read_in_by_column_if_row_perm(char[] input, int[] keyword)
        {
            int size = input.Length / keyword.Length;
            int offs = input.Length % keyword.Length;
            if (offs != 0)
            {
                size++;
            }

            char[,] matrix = new char[size, keyword.Length];
            int pos = 0;

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < keyword.Length; j++)
                {
                    if (pos < input.Length)
                    {
                        if ((!offs.Equals(0)) && i.Equals(size - 1))
                        {
                            bool ok = false;
                            for (int k = 0; k < offs; k++)
                            {
                                if ((keyword[k] - 1).Equals(j))
                                {
                                    ok = true;
                                }
                            }
                            if (ok)
                            {
                                matrix[i, j] = input[pos];
                                pos++;
                            }
                        }
                        else
                        {
                            matrix[i, j] = input[pos];
                            pos++;
                        }
                    }
                }
            }
            return matrix;
        }

        private char[,] dec_read_in_by_row(char[] input, int[] keyword)
        {
            int size = input.Length / keyword.Length;
            int offs = input.Length % keyword.Length;
            if (offs != 0)
            {
                size++;
            }

            char[,] matrix = new char[keyword.Length, size];
            int pos = 0;

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < keyword.Length; j++)
                {
                    if (pos < input.Length)
                    {
                        if ((!offs.Equals(0)) && i.Equals(size - 1))
                        {
                            bool ok = false;

                            for (int k = 0; k < offs; k++)
                            {
                                if ((keyword[k] - 1).Equals(j))
                                {
                                    ok = true;
                                }
                            }
                            if (ok)
                            {
                                matrix[j, i] = input[pos];
                                pos++;
                            }
                        }
                        else
                        {
                            matrix[j, i] = input[pos];
                            pos++;
                        }
                    }
                }
            }
            return matrix;
        }

        private char[,] dec_read_in_by_row_if_row_perm(char[] input, int[] keyword)
        {
            int size = input.Length / keyword.Length;
            int offs = input.Length % keyword.Length;
            if (offs != 0)
            {
                size++;
            }

            char[,] matrix = new char[size, keyword.Length];
            int pos = 0;

            for (int i = 0; i < keyword.Length; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (pos < input.Length)
                    {
                        if ((!offs.Equals(0)) && j.Equals(size - 1))
                        {
                            bool ok = false;
                            for (int k = 0; k < offs; k++)
                            {
                                if ((keyword[k] - 1).Equals(i))
                                {
                                    ok = true;
                                }
                            }
                            if (ok)
                            {
                                matrix[j, i] = input[pos];
                                pos++;
                            }
                        }
                        else
                        {
                            matrix[j, i] = input[pos];
                            pos++;
                        }
                    }
                }
            }
            return matrix;
        }

        private char[,] enc_permut_by_column(char[,] readin_matrix, int[] keyword)
        {
            int x = keyword.Length;
            int y = readin_matrix.Length / keyword.Length;
            char[,] matrix = new char[x, y];
            int pos = 0;

            for (int i = 1; i <= keyword.Length; i++)
            {
                for (int j = 0; j < keyword.Length; j++)
                {
                    if (i.Equals(keyword[j]))
                    {
                        pos = j;
                    }
                }
                for (int j = 0; j < y; j++)
                {
                    matrix[i - 1, j] = readin_matrix[pos, j];
                }
            }
            return matrix;
        }

        private char[,] enc_permute_by_row(char[,] readin_matrix, int[] keyword)
        {
            int y = keyword.Length;
            int x = readin_matrix.Length / keyword.Length;
            char[,] matrix = new char[x, y];
            int pos = 0;

            for (int i = 1; i <= y; i++)
            {
                for (int j = 0; j < keyword.Length; j++)
                {
                    if (keyword[j].Equals(i))
                    {
                        pos = j;
                    }
                }

                for (int j = 0; j < x; j++)
                {
                    matrix[j, i - 1] = readin_matrix[j, pos];
                }
            }
            return matrix;
        }

        private char[,] dec_permut_by_column(char[,] readin_matrix, int[] keyword)
        {
            int x = keyword.Length;
            int y = readin_matrix.Length / keyword.Length;
            char[,] matrix = new char[x, y];

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    matrix[i, j] = readin_matrix[keyword[i] - 1, j];
                }
            }
            return matrix;
        }

        private char[,] dec_permut_by_row(char[,] readin_matrix, int[] keyword)
        {
            int x = keyword.Length;
            int y = readin_matrix.Length / keyword.Length;
            char[,] matrix = new char[y, x];

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    matrix[j, i] = readin_matrix[j, keyword[i] - 1];
                }
            }
            return matrix;
        }

        private char[] read_out_by_row(char[,] matrix, int keyword_length)
        {
            int x = keyword_length;
            int y = matrix.Length / keyword_length;
            char empty_byte = new char();
            int count_empty = 0;

            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    char tmp = matrix[j, i];
                    if (tmp.Equals(empty_byte))
                    {
                        count_empty++;
                    }
                }
            }
            char[] enc = new char[matrix.Length - count_empty];

            int pos = 0;
            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    char tmp = matrix[j, i];
                    if (!tmp.Equals(empty_byte))
                    {
                        enc[pos] = tmp;
                        pos++;
                    }
                }
            }
            return enc;
        }

        private char[] read_out_by_row_if_row_perm(char[,] matrix, int keyword_length)
        {
            int y = keyword_length;
            int x = matrix.Length / keyword_length;

            char empty_byte = new char();
            int empty_count = 0;
            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    char tmp = matrix[j, i];
                    if (tmp.Equals(empty_byte))
                    {
                        empty_count++;
                    }
                }
            }

            char[] enc = new char[matrix.Length - empty_count];
            int pos = 0;

            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    char tmp = matrix[j, i];
                    if (!tmp.Equals(empty_byte))
                    {
                        enc[pos] = tmp;
                        pos++;
                    }
                }
            }
            return enc;
        }

        private char[] read_out_by_column(char[,] matrix, int keyword_length)
        {
            int x = keyword_length;
            int y = matrix.Length / keyword_length;

            char empty_byte = new char();
            int empty_count = 0;

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    char tmp = matrix[i, j];
                    if (tmp.Equals(empty_byte))
                    {
                        empty_count++;
                    }
                }
            }

            char[] enc = new char[matrix.Length - empty_count];
            int pos = 0;
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    char tmp = matrix[i, j];
                    if (!tmp.Equals(empty_byte) || tmp.Equals(null))
                    {
                        enc[pos] = tmp;
                        pos++;
                    }
                }
            }
            return enc;
        }

        private char[] read_out_by_column_if_row_perm(char[,] matrix, int keyword_length)
        {
            int y = keyword_length;
            int x = matrix.Length / keyword_length;

            char empty_byte = new char();
            int empty_count = 0;
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    char tmp = matrix[i, j];
                    if (tmp.Equals(empty_byte))
                    {
                        empty_count++;
                    }
                }
            }

            char[] enc = new char[matrix.Length - empty_count];
            int pos = 0;
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    char tmp = matrix[i, j];
                    if (!tmp.Equals(empty_byte))
                    {
                        enc[pos] = tmp;
                        pos++;
                    }
                }
            }
            return enc;
        }

        private int[] get_Keyword_Array(string keyword)
        {
            try
            {
                int length = 1;
                char komma = ',';

                for (int i = 0; i < keyword.Length; i++)
                {
                    if (keyword[i].Equals(komma))
                    {
                        length++;
                    }
                }

                int[] keys = new int[length];
                string tmp = "";
                int pos = 0;
                for (int i = 0; i < keyword.Length; i++)
                {
                    if (i.Equals(keyword.Length - 1))
                    {
                        tmp += keyword[i];
                        keys[pos] = Convert.ToInt32(tmp);
                    }

                    else
                    {
                        if (keyword[i].Equals(komma))
                        {
                            keys[pos] = Convert.ToInt32(tmp);
                            tmp = "";
                            pos++;
                        }
                        else
                        {
                            tmp += keyword[i];
                        }
                    }
                }
                return keys;
            }
            catch (FormatException)
            {
                return null;
            }
        }

        private bool is_Valid_Keyword(int[] keyword)
        {
            return (new HashSet<int>(keyword)).Count == keyword.Length;
        }

        public int[] sortKey(string input)
        {
            if (input != null && !input.Equals(""))
            {
                string key = input;
                char[] keyChars = key.ToCharArray();
                char[] orgChars = key.ToCharArray();
                int[] rank = new int[keyChars.Length];
                Array.Sort(keyChars);

                for (int i = 0; i < orgChars.Length; i++)
                {
                    rank[i] = (Array.IndexOf(keyChars, orgChars[i])) + 1;
                    keyChars[Array.IndexOf(keyChars, orgChars[i])] = (char)0;
                }
                return rank;
            }
            return null;
        }

        public void Transposition_LogMessage(string msg, NotificationLevel loglevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(msg, this, loglevel));
        }

        public void changeSettings(string setting, object value)
        {
            if (setting.Equals("ReadIn"))
            {
                _settings.SetReadIn((int)value);
            }
            else if (setting.Equals("Permute"))
            {
                _settings.SetPermutation((int)value);
            }
            else if (setting.Equals("ReadOut"))
            {
                _settings.SetReadOut((int)value);
            }
        }

        public int getSettings(string setting)
        {
            if (setting.Equals("ReadIn"))
            {
                return _settings.ReadIn;
            }
            else if (setting.Equals("Permute"))
            {
                return _settings.Permutation;
            }
            else if (setting.Equals("ReadOut"))
            {
                return _settings.ReadOut;
            }

            throw new ArgumentException(string.Format("Setting {0} does not exist!", setting));
        }

        # endregion

        private IControlTranspoEncryption controlSlave;
        [PropertyInfo(Direction.ControlSlave, "ControlSlaveCaption", "ControlSlaveTooltip")]
        public IControlTranspoEncryption ControlSlave
        {
            get
            {
                if (controlSlave == null)
                {
                    controlSlave = new TranspositionControl(this);
                }

                return controlSlave;
            }
        }
    }

    public class TranspositionControl : IControlTranspoEncryption
    {
        private readonly Transposition plugin;

        public TranspositionControl(Transposition plugin)
        {
            this.plugin = plugin;
        }

        public string Decrypt(string ciphertext, string key)
        {
            int[] intKey = new int[key.Length];
            for (int i = 0; i < key.Length; i++)
            {
                intKey[i] = key[i];
            }
            return new string(plugin.decrypt(ciphertext.ToCharArray(), intKey));
        }

        public void onStatusChanged()
        {
            if (OnStatusChanged != null)
            {
                OnStatusChanged(this, true);
            }
        }

        public void changeSettings(string setting, object value)
        {
            plugin.changeSettings(setting, value);
        }

        public int getSettings(string setting)
        {
            return plugin.getSettings(setting);
        }

        public event IControlStatusChangedEventHandler OnStatusChanged;

        public void Dispose()
        {

        }
    }
}
