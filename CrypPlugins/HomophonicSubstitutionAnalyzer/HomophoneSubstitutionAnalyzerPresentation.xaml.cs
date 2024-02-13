/*
   Copyright 2023 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using CrypTool.CrypAnalysisViewControl;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Utils;
using LanguageStatisticsLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace CrypTool.Plugins.HomophonicSubstitutionAnalyzer
{
    [PluginBase.Attributes.Localization("CrypTool.Plugins.HomophonicSubstitutionAnalyzer.Properties.Resources")]
    public partial class HomophoneSubstitutionAnalyzerPresentation : UserControl
    {
        private const int MaxBestListEntries = 100;
        private int _keylength = 0;
        private string PlainAlphabetText = null; //obtained by language statistics
        private readonly string CipherAlphabetText;// = "ABCDEFGHIJKLMNOPQRSTUVWXYZÄÜÖabcdefghijklmnopqrstuvwxyzäüöß1234567890ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩАБВГДЂЕЄЖЗЅИІЈКЛЉМНЊОПРСТЋУФХЦЧЏШЪЫЬЭЮЯ!§$%&=?#ㄱㄴㄷㄹㅁㅂㅅㅇㅈㅊㅋㅌㅍㅎㄲㄸㅃㅆㅉㅏㅑㅓㅕㅗㅛㅜㅠㅡㅣㅐㅒㅔㅖㅚㅟㅢㅘㅝㅙㅞ";
        private HillClimber _hillClimber;
        private WordFinder _wordFinder;
        private NomenclatureElementFinder _nomenclatureElementFinder;
        private SymbolLabel[,] _ciphertextLabels = new SymbolLabel[0, 0];
        private SymbolLabel[,] _plaintextLabels = new SymbolLabel[0, 0];
        private TextBox[] _minTextBoxes = new TextBox[0];
        private TextBox[] _maxTextBoxes = new TextBox[0];

        public AnalyzerConfiguration AnalyzerConfiguration { get; private set; }
        private Grams _grams;

        //cache for loaded n-grams
        private static readonly Dictionary<string, Grams> NGramCache = new Dictionary<string, Grams>();

        private string _ciphertext = null;
        private char _separator;
        private CiphertextFormat _ciphertextFormat;
        private bool _running = false;

        public event EventHandler<ProgressChangedEventArgs> Progress;
        public event EventHandler<NewBestValueEventArgs> NewBestValue;
        public event EventHandler<UserChangedTextEventArgs> UserChangedText;
        public event TextChangedEventHandler LetterLimitsChanged;

        private ObservableCollection<ResultEntry> BestList { get; } = new ObservableCollection<ResultEntry>();
        private int _restart = 0;

        private List<string> _originalCiphertextSymbols = new List<string>();

        public HomophoneSubstitutionAnalyzerPresentation()
        {
            InitializeComponent();
            DisableUIAndStop();

            //create ciphertext alphabet symbols
            StringBuilder builder = new StringBuilder();
            for (int i = 41; i < 1041; i++)
            {
                builder.Append((char)i);
            }
            CipherAlphabetText = builder.ToString();
        }

        /// <summary>
        /// Initializes the ui with a new ciphertext
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="ciphertextFormat"></param>
        /// <param name="separator"></param>
        /// <param name="startTemperature"></param>
        public void AddCiphertext(string ciphertext, CiphertextFormat ciphertextFormat, char separator, int startTemperature, bool useNulls)
        {
            _ciphertext = ciphertext;
            _separator = separator;
            _ciphertextFormat = ciphertextFormat;
            int[] numbers = ConvertCiphertextToNumbers(_ciphertext, _separator);
            _originalCiphertextSymbols = ConvertToList(_ciphertext, _separator);
            int homophoneNumber = Tools.Distinct(numbers).Length;
            _keylength = (int)(homophoneNumber * 1.3);
            if(_keylength < 5)
            {
                _keylength = 5;
            }
            Text text_ciphertext = new Text(Tools.ChangeToConsecutiveNumbers(numbers));
            AnalyzerConfiguration = new AnalyzerConfiguration(_keylength, text_ciphertext)
            {
                PlaintextMapping = PlainAlphabetText,
                CiphertextAlphabet = CipherAlphabetText,
                TextColumns = 60,
                Steps = 50000,
                KeyLetterLimits = new List<LetterLimits>(),
                MinWordLength = 8,
                MaxWordLength = 10,
                WordCountToFind = 3,
                NomenclatureElementsThreshold = 1,
                Separator = separator,
                StartTemperature = startTemperature,
                UseNulls = useNulls
            };
            _nomenclatureElementFinder = new NomenclatureElementFinder(text_ciphertext);
            _hillClimber = new HillClimber(AnalyzerConfiguration)
            {
                Grams = _grams
            };
            _hillClimber.NewBestValue += HillClimberNewBestValue;
            _hillClimber.Progress += HillClimberProgress;
        }

        /// <summary>
        /// If the user gives a start key, this is used to lock the homophones
        /// </summary>
        /// <param name="startKey"></param>
        public void ApplyStartKey(string startKey)
        {
            if (string.IsNullOrEmpty(startKey))
            {
                return;
            }

            Dictionary<string, string> startKeyDictionary = new Dictionary<string, string>();
            string[] lines = startKey.Split(new char[] { '\r', '\n' });

            //our key is build of lines in the format [ab];[cd]
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || string.IsNullOrEmpty(line))
                {
                    //ignore empty lines in key
                    continue;
                }

                string[] split = line.Split(';');
                if (split.Count() != 2)
                {
                    throw new ArgumentException(string.Format("The line in the key is not valid: {0} ", line));
                }

                //remove [ and ]
                if (split[0].StartsWith("["))
                {
                    split[0] = split[0].Substring(1, split[0].Length - 1);
                }
                if (split[0].EndsWith("]"))
                {
                    split[0] = split[0].Substring(0, split[0].Length - 1);
                }

                //we only use symbols, which are part of our plaintext alphabet
                if (!PlainAlphabetText.Contains(split[0]))
                {
                    continue;
                }

                //remove [ and ]
                if (split[1].StartsWith("["))
                {
                    split[1] = split[1].Substring(1, split[1].Length - 1);
                }
                if (split[1].EndsWith("]"))
                {
                    split[1] = split[1].Substring(0, split[1].Length - 1);
                }

                if (split[1].Contains("|"))
                {
                    //case 1: we have a list of homophone separated by |
                    string[] symbols = split[1].Split('|');
                    foreach (string symbol in symbols)
                    {
                        if (!startKeyDictionary.ContainsKey(symbol))
                        {
                            startKeyDictionary.Add(symbol, split[0]);
                        }
                    }
                }
                else
                {
                    //case 2: we only have a single homophone
                    if (!startKeyDictionary.ContainsKey(split[1]))
                    {
                        startKeyDictionary.Add(split[1], split[0]);
                    }
                }
            }

            StringBuilder dummyAlphabetBuilder = new StringBuilder();
            for (int i = 0; i < _keylength; i++)
            {
                dummyAlphabetBuilder.Append(PlainAlphabetText[PlainAlphabetText.Length - 1]);
            }

            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                CipherAlphabetTextBox.Text = CipherAlphabetText.Substring(0, _keylength);
                PlainAlphabetTextBox.Text = dummyAlphabetBuilder.ToString();

                foreach (SymbolLabel ciphertextLabel in _ciphertextLabels)
                {
                    if (ciphertextLabel == null)
                    {
                        continue;
                    }
                    if (startKeyDictionary.ContainsKey((string)ciphertextLabel.Content))
                    {
                        _plaintextLabels[ciphertextLabel.X, ciphertextLabel.Y].Symbol = startKeyDictionary[(string)ciphertextLabel.Content];
                        _plaintextLabels[ciphertextLabel.X, ciphertextLabel.Y].Content = startKeyDictionary[(string)ciphertextLabel.Content];

                        var key = CipherAlphabetTextBox.Text;
                        var index = key.IndexOf(ciphertextLabel.Symbol);
                        if (index > -1 && _hillClimber.AnalyzerConfiguration.LockedHomophoneMappings[index] == -1)
                        {
                            _hillClimber.AnalyzerConfiguration.LockedHomophoneMappings[index] = Tools.MapIntoNumberSpace(_plaintextLabels[ciphertextLabel.X, ciphertextLabel.Y].Symbol, PlainAlphabetText)[0];
                            //Update plainalphabet textbox
                            PlainAlphabetTextBox.Text = PlainAlphabetTextBox.Text.Remove(index, 1);
                            PlainAlphabetTextBox.Text = PlainAlphabetTextBox.Text.Insert(index, (_plaintextLabels[ciphertextLabel.X, ciphertextLabel.Y].Symbol));
                        }
                    }
                }
                MarkLockedHomophones();
                OnUserChangedText(); //output start plaintext and key
            }, null);            
        }

        /// <summary>
        /// Replaces all unicode names in ciphertext labels content by the unicode char itself
        /// </summary>
        public void ReplaceCiphertextLabelsByUnicodeChars()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                foreach (SymbolLabel ciphertextLabel in _ciphertextLabels)
                {
                    if (ciphertextLabel == null)
                    {
                        continue;
                    }

                    //allow <space> syntax for DECRYPT documents
                    if (((string)ciphertextLabel.Content).ToLower().Equals("<space>"))
                    {
                        ciphertextLabel.Content = "space";
                    }

                    long unicode = UnicodeHelper.GetIdByName((string)ciphertextLabel.Content);
                    if (unicode != -1)
                    {
                        ciphertextLabel.Content = string.Format("{0}", (char)unicode);
                    }
                }
            }, null);
        }

        /// <summary>
        /// Generates the plaintext and the ciphertext grids
        /// </summary>
        public void GenerateGrids()
        {
            int[] numbers = ConvertCiphertextToNumbers(_ciphertext, _separator);
            int homophoneNumber = Tools.Distinct(numbers).Length;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                if (AnalyzerConfiguration.KeepLinebreaks)
                {
                    GenerateCiphertextGridWithLinebreaks(AnalyzerConfiguration.Ciphertext);
                    GeneratePlaintextGridWithLinebreaks(AnalyzerConfiguration.Ciphertext);
                }
                else
                {
                    GenerateCiphertextGrid(AnalyzerConfiguration.Ciphertext, AnalyzerConfiguration.TextColumns);
                    GeneratePlaintextGrid(AnalyzerConfiguration.Ciphertext, AnalyzerConfiguration.TextColumns);
                }
                ProgressBar.Value = 0;
                ProgressText.Content = string.Empty;
                BestList.Clear();
                BestListView.DataContext = BestList;
                InfoTextLabel.Content = string.Format(Properties.Resources.DifferentHomophones, numbers.Length, homophoneNumber);
            }, null);
        }

        /// <summary>
        /// Converts string text into numbers depending on ciphertextformat
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <returns></returns>
        private int[] ConvertCiphertextToNumbers(string ciphertext, char separator)
        {
            switch (_ciphertextFormat)
            {
                case CiphertextFormat.NumberGroups:
                    return Tools.MapHomophoneTextNumbersIntoNumberSpace(ciphertext);
                case CiphertextFormat.CommaSeparated:
                    return Tools.MapHomophoneCommaSeparatedIntoNumberSpace(ciphertext, separator);
                case CiphertextFormat.SingleLetters:
                default:
                    return Tools.MapHomophonesIntoNumberSpace(ciphertext);
            }
        }

        /// <summary>
        /// Converts string text to list of strings
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <returns></returns>
        private List<string> ConvertToList(string ciphertext, char separator)
        {
            switch (_ciphertextFormat)
            {
                case CiphertextFormat.NumberGroups:
                    return Tools.ConvertHomophoneTextNumbersToListOfStrings(ciphertext);
                case CiphertextFormat.CommaSeparated:
                    return Tools.ConvertHomophoneCommaSeparatedToListOfStrings(ciphertext, separator);
                case CiphertextFormat.SingleLetters:
                default:
                    return Tools.ConvertHomophonesToListOfString(ciphertext);
            }
        }

        /// <summary>
        /// Creates the wordfinder that is used during the analysis using a given list of words from a dictionary
        /// if list is null or empty, no wordfinder is created
        /// </summary>
        /// <param name="dictionary"></param>
        public void AddDictionary(string[] dictionary)
        {
            if (dictionary != null && dictionary.Length > 0)
            {
                _wordFinder = new WordFinder(dictionary, AnalyzerConfiguration.MinWordLength, AnalyzerConfiguration.MaxWordLength, PlainAlphabetText);
            }
            else
            {
                _wordFinder = null;
            }
        }

        /// <summary>
        /// Generates the Grid for the ciphertext and fills in the symbols
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="columns"></param>
        private void GenerateCiphertextGrid(Text ciphertext, int columns)
        {
            CiphertextGrid.Children.Clear();

            int rows = (int)Math.Ceiling((double)ciphertext.GetSymbolsCount() / columns);
            _ciphertextLabels = new SymbolLabel[columns, rows];
            string text = Tools.MapNumbersIntoTextSpace(ciphertext.ToIntegerArray(), CipherAlphabetText);

            for (int column = 0; column < columns; column++)
            {
                CiphertextGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            CiphertextGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(int.MaxValue, GridUnitType.Star) });

            for (int row = 0; row < rows; row++)
            {
                CiphertextGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength() });
            }
            CiphertextGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            int offset = 0;
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    if (offset == ciphertext.GetSymbolsCount())
                    {
                        break;
                    }
                    SymbolLabel label = new SymbolLabel
                    {
                        X = x,
                        Y = y,
                        SymbolOffset = offset
                    };
                    label.MouseLeftButtonDown += CiphertextLabelOnMouseLeftButtonDown;
                    label.MouseRightButtonDown += CiphertextLabelOnMouseRightButtonDown;
                    if (offset < text.Length)
                    {
                        label.Symbol = text.Substring(offset, 1);
                    }
                    else
                    {
                        label.Symbol = "?";
                    }
                    _ciphertextLabels[x, y] = label;
                    label.Width = 30 + (_originalCiphertextSymbols[offset].Length  - 1) * 5;
                    label.Height = 30;
                    label.FontSize = 20;
                    label.FontFamily = new FontFamily("Courier New");
                    label.Content = _originalCiphertextSymbols[offset];
                    label.ToolTip = _originalCiphertextSymbols[offset];
                    label.HorizontalAlignment = HorizontalAlignment.Center;
                    label.HorizontalContentAlignment = HorizontalAlignment.Center;
                    offset++;
                    Grid.SetRow(label, y);
                    Grid.SetColumn(label, x);
                    CiphertextGrid.Children.Add(label);
                }
            }
        }

        /// <summary>
        /// Generates the Grid for the ciphertext and fills in the symbols (keeping the linebreaks)
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="columns"></param>
        private void GenerateCiphertextGridWithLinebreaks(Text ciphertext)
        {
            CiphertextGrid.Children.Clear();
            int columns = 0;
            int offset = 0;
            int distance;
            foreach (int number in AnalyzerConfiguration.LinebreakPositions)
            {
                distance = number - offset;
                offset = offset + distance;
                if (distance > columns)
                {
                    columns = distance;
                }
            }
            distance = ciphertext.GetLettersCount() - offset;
            if (distance > columns)
            {
                columns = distance;
            }
            int rows = AnalyzerConfiguration.LinebreakPositions.Count + 1;

            _ciphertextLabels = new SymbolLabel[columns, rows];
            string text = Tools.MapNumbersIntoTextSpace(ciphertext.ToIntegerArray(), CipherAlphabetText);

            for (int column = 0; column < columns; column++)
            {
                CiphertextGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            CiphertextGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(int.MaxValue, GridUnitType.Star) });

            for (int row = 0; row < rows; row++)
            {
                CiphertextGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength() });
            }
            CiphertextGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            offset = 0;
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    if (offset == ciphertext.GetSymbolsCount())
                    {
                        break;
                    }
                    SymbolLabel label = new SymbolLabel
                    {
                        X = x,
                        Y = y,
                        SymbolOffset = offset
                    };
                    label.MouseLeftButtonDown += CiphertextLabelOnMouseLeftButtonDown;
                    label.MouseRightButtonDown += CiphertextLabelOnMouseRightButtonDown;
                    if (offset < text.Length)
                    {
                        label.Symbol = text.Substring(offset, 1);
                    }
                    else
                    {
                        label.Symbol = "?";
                    }
                    _ciphertextLabels[x, y] = label;
                    label.Width = 30 + (_originalCiphertextSymbols[offset].Length - 1) * 5;
                    label.Height = 30;
                    label.FontSize = 20;
                    label.FontFamily = new FontFamily("Courier New");
                    label.Content = _originalCiphertextSymbols[offset];
                    label.ToolTip = _originalCiphertextSymbols[offset];
                    label.HorizontalAlignment = HorizontalAlignment.Center;
                    label.HorizontalContentAlignment = HorizontalAlignment.Center;
                    offset++;
                    Grid.SetRow(label, y);
                    Grid.SetColumn(label, x);
                    CiphertextGrid.Children.Add(label);
                    if (AnalyzerConfiguration.LinebreakPositions.Contains(offset))
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Generates the Grid for the plaintext and fills in the symbols
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="columns"></param>
        private void GeneratePlaintextGrid(Text plaintext, int columns)
        {
            PlaintextGrid.Children.Clear();

            int rows = (int)Math.Ceiling((double)plaintext.GetSymbolsCount() / columns);
            _plaintextLabels = new SymbolLabel[columns, rows];
            string text = Tools.MapNumbersIntoTextSpace(plaintext.ToIntegerArray(), CipherAlphabetText);

            for (int column = 0; column < columns; column++)
            {
                PlaintextGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            PlaintextGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(int.MaxValue, GridUnitType.Star) });

            for (int row = 0; row < rows; row++)
            {
                PlaintextGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength() });
            }
            PlaintextGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            int offset = 0;
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    if (offset == plaintext.GetSymbolsCount())
                    {
                        break;
                    }
                    SymbolLabel label = new SymbolLabel();
                    label.MouseLeftButtonDown += PlaintextLabelOnMouseLeftButtonDown;
                    label.MouseRightButtonDown += PlaintextLabelOnMouseRightButtonDown;
                    _plaintextLabels[x, y] = label;
                    label.X = x;
                    label.Y = y;
                    label.SymbolOffset = offset;
                    label.Symbol = text.Substring(offset, 1);
                    label.Width = 30 + (_originalCiphertextSymbols[offset].Length - 1) * 5;
                    label.Height = 30;
                    label.FontSize = 20;
                    label.FontFamily = new FontFamily("Courier New");
                    label.Content = text.Substring(offset, 1);
                    label.ToolTip = _originalCiphertextSymbols[offset];
                    label.HorizontalAlignment = HorizontalAlignment.Center;
                    label.HorizontalContentAlignment = HorizontalAlignment.Center;
                    offset++;
                    Grid.SetRow(label, y);
                    Grid.SetColumn(label, x);
                    PlaintextGrid.Children.Add(label);
                }
            }
        }

        /// <summary>
        /// Generates the Grid for the ciphertext and fills in the symbols (keeping the linebreaks)
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="columns"></param>
        private void GeneratePlaintextGridWithLinebreaks(Text plaintext)
        {
            PlaintextGrid.Children.Clear();

            int columns = 0;
            int offset = 0;
            int distance;
            foreach (int number in AnalyzerConfiguration.LinebreakPositions)
            {
                distance = number - offset;
                offset = offset + distance;
                if (distance > columns)
                {
                    columns = distance;
                }
            }
            distance = plaintext.GetLettersCount() - offset;
            if (distance > columns)
            {
                columns = distance;
            }
            AnalyzerConfiguration.TextColumns = columns;
            int rows = AnalyzerConfiguration.LinebreakPositions.Count + 1;

            _plaintextLabels = new SymbolLabel[columns, rows];
            string text = Tools.MapNumbersIntoTextSpace(plaintext.ToIntegerArray(), CipherAlphabetText);

            for (int column = 0; column < columns; column++)
            {
                PlaintextGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            PlaintextGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(int.MaxValue, GridUnitType.Star) });

            for (int row = 0; row < rows; row++)
            {
                PlaintextGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength() });
            }
            PlaintextGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            offset = 0;
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    if (offset == plaintext.GetSymbolsCount())
                    {
                        break;
                    }
                    SymbolLabel label = new SymbolLabel();
                    label.MouseLeftButtonDown += PlaintextLabelOnMouseLeftButtonDown;
                    label.MouseRightButtonDown += PlaintextLabelOnMouseRightButtonDown;
                    _plaintextLabels[x, y] = label;
                    label.X = x;
                    label.Y = y;
                    label.SymbolOffset = offset;
                    label.Symbol = text.Substring(offset, 1);
                    label.Width = 30 + (_originalCiphertextSymbols[offset].Length - 1) * 5;
                    label.Height = 30;
                    label.FontSize = 20;
                    label.FontFamily = new FontFamily("Courier New");
                    label.Content = text.Substring(offset, 1);
                    label.HorizontalAlignment = HorizontalAlignment.Center;
                    label.HorizontalContentAlignment = HorizontalAlignment.Center;
                    offset++;
                    Grid.SetRow(label, y);
                    Grid.SetColumn(label, x);
                    PlaintextGrid.Children.Add(label);
                    if (AnalyzerConfiguration.LinebreakPositions.Contains(offset))
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Generates the tab for the selection of the key letter distribution
        /// </summary>
        public void GenerateKeyLetterLimitsListView()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
           {
               KeyLetterListView.Items.Clear();
               _minTextBoxes = new TextBox[AnalyzerConfiguration.KeyLetterLimits.Count];
               _maxTextBoxes = new TextBox[AnalyzerConfiguration.KeyLetterLimits.Count];

               Grid grid = new Grid
               {
                   Width = 500
               };
               grid.ColumnDefinitions.Add(new ColumnDefinition());
               grid.ColumnDefinitions.Add(new ColumnDefinition());
               grid.ColumnDefinitions.Add(new ColumnDefinition());

               Label letterLabel = new Label
               {
                   Content = Properties.Resources.LetterLabel
               };
               Grid.SetRow(letterLabel, 0);
               Grid.SetColumn(letterLabel, 0);
               letterLabel.VerticalContentAlignment = VerticalAlignment.Center;
               letterLabel.HorizontalContentAlignment = HorizontalAlignment.Center;

               Label minLabel = new Label
               {
                   Content = Properties.Resources.MinLabel
               };
               Grid.SetRow(minLabel, 0);
               Grid.SetColumn(minLabel, 1);
               minLabel.VerticalContentAlignment = VerticalAlignment.Center;
               minLabel.HorizontalContentAlignment = HorizontalAlignment.Center;

               Label maxLabel = new Label
               {
                   Content = Properties.Resources.MaxLabel
               };
               Grid.SetRow(letterLabel, 0);
               Grid.SetRow(maxLabel, 0);
               Grid.SetColumn(maxLabel, 2);
               maxLabel.VerticalContentAlignment = VerticalAlignment.Center;
               maxLabel.HorizontalContentAlignment = HorizontalAlignment.Center;

               grid.Children.Add(letterLabel);
               grid.Children.Add(minLabel);
               grid.Children.Add(maxLabel);

               KeyLetterListView.Items.Add(grid);

               int index = 0;
               foreach (LetterLimits limits in AnalyzerConfiguration.KeyLetterLimits)
               {
                   grid = new Grid
                   {
                       Width = 500
                   };
                   grid.ColumnDefinitions.Add(new ColumnDefinition());
                   grid.ColumnDefinitions.Add(new ColumnDefinition());
                   grid.ColumnDefinitions.Add(new ColumnDefinition());

                   Label label = new Label
                   {
                       FontSize = 16,
                       Width = 50,
                       Content = string.Format("\"{0}\"", Tools.MapNumbersIntoTextSpace(new int[] { limits.Letter }, AnalyzerConfiguration.PlaintextAlphabet))
                   };

                   Grid.SetRow(label, 0);
                   Grid.SetColumn(label, 0);

                   TextBox minbox = new TextBox();
                   _minTextBoxes[index] = minbox;
                   minbox.Text = string.Empty + limits.MinValue;
                   minbox.VerticalContentAlignment = VerticalAlignment.Center;
                   minbox.Width = 150;
                   minbox.Height = 25;
                   minbox.FontSize = 12;
                   minbox.TextChanged += LetterLimitsChanged;
                   Grid.SetRow(minbox, 0);
                   Grid.SetColumn(minbox, 1);

                   TextBox maxbox = new TextBox();
                   _maxTextBoxes[index] = maxbox;
                   maxbox.Text = string.Empty + limits.MaxValue;
                   maxbox.VerticalContentAlignment = VerticalAlignment.Center;
                   maxbox.Width = 150;
                   maxbox.Height = 25;
                   maxbox.FontSize = 12;
                   maxbox.TextChanged += LetterLimitsChanged;
                   Grid.SetRow(maxbox, 0);
                   Grid.SetColumn(maxbox, 2);

                   grid.Children.Add(label);
                   grid.Children.Add(minbox);
                   grid.Children.Add(maxbox);

                   KeyLetterListView.Items.Add(grid);
                   index++;
               }
           }, null);
        }

        /// <summary>
        /// Fired each time the progress changed, i.e. the analyzer finished a restart
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void HillClimberProgress(object sender, ProgressChangedEventArgs eventArgs)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ProgressBar.Value = eventArgs.Percentage * 100;
                ProgressText.Content = string.Format("{0} %", Math.Round(eventArgs.Percentage, 2) * 100);

                if (eventArgs.Terminated && AnalyzerConfiguration.AnalysisMode == AnalysisMode.SemiAutomatic)
                {
                    _running = false;
                    AnalyzeButton.Content = Properties.Resources.Analyze;
                }

                //in fullautomatic analysis mode with 100% we restart by resetting locked letters
                if (eventArgs.Terminated && AnalyzerConfiguration.AnalysisMode == AnalysisMode.FullAutomatic)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal,
                    (SendOrPostCallback)delegate
                   {
                       //reset all locked letters
                       for (int i = 0; i < _hillClimber.AnalyzerConfiguration.LockedHomophoneMappings.Length; i++)
                       {
                           _hillClimber.AnalyzerConfiguration.LockedHomophoneMappings[i] = -1;
                       }
                       MarkLockedHomophones();
                   }, null);
                }

            }, null);
            if (Progress != null)
            {
                if (AnalyzerConfiguration.AnalysisMode == AnalysisMode.FullAutomatic)
                {
                    //in fullautomatic analysis mode the progress is calculated using the restarts
                    eventArgs.Percentage = _restart / (double)AnalyzerConfiguration.Restarts;
                }

                Progress.Invoke(sender, eventArgs);
            }
        }

        /// <summary>
        /// Fired each time the analyzer found a "better" cost value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void HillClimberNewBestValue(object sender, NewBestValueEventArgs eventArgs)
        {
            AutoResetEvent waitHandle = new AutoResetEvent(false);
            bool newTopEntry = false;
            Dictionary<int, int> wordPositions = null;

            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    int column = 0;
                    int row = 0;
                    int offset = 0;
                    foreach (char letter in eventArgs.Plaintext)
                    {
                        _plaintextLabels[column, row].Content = letter;
                        _plaintextLabels[column, row].Symbol = string.Empty + letter;
                        column++;
                        offset++;
                        if ((AnalyzerConfiguration.KeepLinebreaks && AnalyzerConfiguration.LinebreakPositions.Contains(offset)) ||
                            (!AnalyzerConfiguration.KeepLinebreaks && column == AnalyzerConfiguration.TextColumns))
                        {
                            column = 0;
                            row++;
                        }
                    }
                    CipherAlphabetTextBox.Text = eventArgs.CiphertextAlphabet;
                    PlainAlphabetTextBox.Text = eventArgs.PlaintextMapping;
                    CostTextBox.Text = string.Format(Properties.Resources.CostValue_0, Math.Round(eventArgs.CostValue, 2));
                    MarkLockedHomophones();
                    wordPositions = AutoLockWords(AnalyzerConfiguration.WordCountToFind, eventArgs.PlaintextAsNumbers);
                    MarkFoundWords(wordPositions);
                    newTopEntry = AddNewBestListEntry(eventArgs.PlaintextMapping, eventArgs.CostValue, eventArgs.Plaintext);
                }
                catch (Exception)
                {
                    //if auto-lock fails, we just continue
                }
                finally
                {
                    waitHandle.Set();
                }
            }, null);

            //wait here for auto-locker to finish
            waitHandle.WaitOne();

            if (NewBestValue != null)
            {
                if (newTopEntry && wordPositions != null && wordPositions.Count > 0)
                {
                    string substitutionKey = GenerateSubstitutionKey();
                    eventArgs.SubstitutionKey = substitutionKey;
                    eventArgs.FoundWords = new List<string>();
                    //if we have a new top entry, we also output the found words
                    foreach (KeyValuePair<int, int> positionLength in wordPositions)
                    {
                        string word = eventArgs.Plaintext.Substring(positionLength.Key, positionLength.Value);
                        eventArgs.FoundWords.Add(word);
                    }
                }
                eventArgs.NewTopEntry = newTopEntry;
                NewBestValue.Invoke(sender, eventArgs);
            }
        }

        /// <summary>
        /// Generates the current substitution key which can be used by
        /// the Substitution component with the nomenclature templates
        /// </summary>
        /// <returns></returns>
        private string GenerateSubstitutionKey()
        {
            Dictionary<string, List<string>> keyDictionary = new Dictionary<string, List<string>>();
            foreach (SymbolLabel ciphertextLabel in _ciphertextLabels)
            {
                if (ciphertextLabel == null)
                {
                    continue;
                }
                int x = ciphertextLabel.X;
                int y = ciphertextLabel.Y;
                SymbolLabel plaintextLabel = _plaintextLabels[x, y];

                if (plaintextLabel != null)
                {
                    if (!plaintextLabel.Locked)
                    {
                        continue;
                    }
                    string plainletter = plaintextLabel.Symbol;
                    string cipherletter = _originalCiphertextSymbols[ciphertextLabel.SymbolOffset];
                    if (!keyDictionary.ContainsKey(plainletter))
                    {
                        keyDictionary.Add(plainletter, new List<string>());
                    }
                    if (!keyDictionary[plainletter].Contains(cipherletter))
                    {
                        keyDictionary[plainletter].Add(cipherletter);
                    }
                }
            }
            StringBuilder builder = new StringBuilder();
            
            List<string> plaintextElements = keyDictionary.Keys.ToList();
            plaintextElements.Sort(); // sort alphabetically

            foreach (string plaintextElement in plaintextElements)
            {                
                builder.Append(string.Format("[{0}];", plaintextElement));
                List<string> ciphertextElements = keyDictionary[plaintextElement];
                ciphertextElements.Sort(); // sort alphabetically

                for (int i = 0; i < ciphertextElements.Count; i++)
                {
                    string symbol = ciphertextElements[i];
                    if (i == 0)
                    {
                        builder.Append("[");
                        builder.Append(symbol);
                    }
                    else if (i < ciphertextElements.Count - 1)
                    {
                        builder.Append("|");
                        builder.Append(symbol);
                    }
                    else
                    {
                        builder.Append("|");
                        builder.Append(symbol);
                        builder.AppendLine("]");
                    }
                }
                if (ciphertextElements.Count == 1)
                {
                    //if we have only one element, we have to close the tag
                    builder.AppendLine("]");
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Marks the found words in blue color
        /// </summary>
        /// <param name="wordPositions"></param>
        private void MarkFoundWords(Dictionary<int, int> wordPositions)
        {
            if (wordPositions == null)
            {
                return;
            }
            //Color the found words in blue
            foreach (KeyValuePair<int, int> value in wordPositions)
            {
                for (int i = 0; i < value.Value; i++)
                {
                    int position = value.Key + i;
                    foreach (SymbolLabel label in _ciphertextLabels)
                    {
                        if (label == null)
                        {
                            continue;
                        }
                        if (label.SymbolOffset == position)
                        {
                            label.Background = Brushes.LightSkyBlue;
                            _plaintextLabels[label.X, label.Y].Background = Brushes.LightSkyBlue;                            
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds a new entry to the bestlist
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="text"></param>
        private bool AddNewBestListEntry(string key, double value, string text)
        {
            ResultEntry entry = new ResultEntry
            {
                Key = key,
                Text = text,
                Value = Math.Round(value, 2)
            };
            bool newTopEntry = false;
            try
            {
                if (BestList.Count > 0 && entry.Value <= BestList.Last().Value)
                {
                    return false;
                }
                if (BestList.Count > 0 && entry.Value > BestList.First().Value)
                {
                    newTopEntry = true;
                }

                //Insert new entry at correct place to sustain order of list:
                int insertIndex = BestList.TakeWhile(e => e.Value > entry.Value).Count();
                BestList.Insert(insertIndex, entry);

                if (BestList.Count > MaxBestListEntries)
                {
                    BestList.RemoveAt(MaxBestListEntries);
                }
                int ranking = 1;
                foreach (ResultEntry e in BestList)
                {
                    e.Ranking = ranking;
                    ranking++;
                }
                BestListView.ScrollIntoView(BestListView.Items[0]);
            }
            catch (Exception)
            {
                //wtf?
            }
            return newTopEntry;
        }       

        /// <summary>
        /// Left mouse button down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mouseButtonEventArgs"></param>
        private void PlaintextLabelOnMouseLeftButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (_running)
            {
                return;
            }
            try
            {
                SymbolLabel label = (SymbolLabel)sender;
                string symbol = _ciphertextLabels[label.X, label.Y].Symbol;
                LockHomophone(symbol);                
            }
            catch (Exception)
            {
                //do nothing here
            }
            mouseButtonEventArgs.Handled = true;
        }

        /// <summary>
        /// Left mouse button down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mouseButtonEventArgs"></param>
        private void CiphertextLabelOnMouseLeftButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (_running)
            {
                return;
            }
            SymbolLabel label = (SymbolLabel)sender;
            if (_plaintextLabels[label.X, label.Y] != null)
            {
                PlaintextLabelOnMouseLeftButtonDown(_plaintextLabels[label.X, label.Y], mouseButtonEventArgs);
            }
        }

        /// <summary>
        /// Right mouse button down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mouseButtonEventArgs"></param>
        private void PlaintextLabelOnMouseRightButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (_running)
            {
                return;
            }
            try
            {
                SymbolLabel label = (SymbolLabel)sender;
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    ChangeHomophone(_ciphertextLabels[label.X, label.Y].Symbol, -1);
                }
                else
                {
                    ChangeHomophone(_ciphertextLabels[label.X, label.Y].Symbol, 1);                    
                }
                OnUserChangedText();
            }
            catch (Exception)
            {
                //do nothing here
            }
            mouseButtonEventArgs.Handled = true;            
        }

        /// <summary>
        /// Right mouse button down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mouseButtonEventArgs"></param>
        private void CiphertextLabelOnMouseRightButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (_running)
            {
                return;
            }
            SymbolLabel label = (SymbolLabel)sender;
            if (_plaintextLabels[label.X, label.Y] != null)
            {
                PlaintextLabelOnMouseRightButtonDown(_plaintextLabels[label.X, label.Y], mouseButtonEventArgs);
            }
        }

        /// <summary>
        /// Changes the mapping of a homophone
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="direction"></param>
        private void ChangeHomophone(string symbol, int direction, bool nomenclatureElement = false)
        {
            var key = CipherAlphabetTextBox.Text;
            var index = key.IndexOf(symbol);
            if (index > -1)
            {
                if (_hillClimber.AnalyzerConfiguration.LockedHomophoneMappings[index] == -1)
                {
                    return;
                }
                if (!nomenclatureElement)
                {
                    _hillClimber.AnalyzerConfiguration.LockedHomophoneMappings[index] = (_hillClimber.AnalyzerConfiguration.LockedHomophoneMappings[index] + direction) % PlainAlphabetText.Length;
                }
                else
                {
                    _hillClimber.AnalyzerConfiguration.LockedHomophoneMappings[index] = PlainAlphabetText.Length - 1;
                }
                if (_hillClimber.AnalyzerConfiguration.LockedHomophoneMappings[index] < 0)
                {
                    _hillClimber.AnalyzerConfiguration.LockedHomophoneMappings[index] = PlainAlphabetText.Length - 1;
                }

                string newSymbol = Tools.MapNumbersIntoTextSpace(new int[] { _hillClimber.AnalyzerConfiguration.LockedHomophoneMappings[index] }, PlainAlphabetText);

                //Update plainalphabet textbox
                PlainAlphabetTextBox.Text = PlainAlphabetTextBox.Text.Remove(index, 1);
                PlainAlphabetTextBox.Text = PlainAlphabetTextBox.Text.Insert(index, newSymbol);

                //Update all plaintext labels               
                foreach (SymbolLabel label in _ciphertextLabels)
                {
                    if (label == null)
                    {
                        continue;
                    }
                    if (label.Symbol.Equals(symbol))
                    {
                        SymbolLabel plaintextLabel = _plaintextLabels[label.X, label.Y];
                        plaintextLabel.Symbol = newSymbol;
                        plaintextLabel.Content = newSymbol;
                    }
                }
            }
        }

        /// <summary>
        /// Generates updated plaintext and fires event that the user changed the plaintext
        /// </summary>
        private void OnUserChangedText()
        {
            //Create new plaintext from labels
            if (UserChangedText != null)
            {
                StringBuilder plaintextBuilder = new StringBuilder();
                int column = 0;
                int row = 0;
                for (int offset = 0; offset < _ciphertextLabels.Length; offset++)
                {
                    if (_plaintextLabels[column, row] != null)
                    {
                        plaintextBuilder.Append(_plaintextLabels[column, row].Content);
                    }
                    column++;
                    if (column == AnalyzerConfiguration.TextColumns)
                    {
                        column = 0;
                        row++;
                    }

                    if ((AnalyzerConfiguration.KeepLinebreaks && AnalyzerConfiguration.LinebreakPositions.Contains(offset)) ||
                        (!AnalyzerConfiguration.KeepLinebreaks && column == AnalyzerConfiguration.TextColumns))
                    {
                        if (AnalyzerConfiguration.KeepLinebreaks)
                        {
                            plaintextBuilder.AppendLine();
                        }
                    }
                }
                UserChangedTextEventArgs args = new UserChangedTextEventArgs() { Plaintext = plaintextBuilder.ToString() };
                args.SubstitutionKey = GenerateSubstitutionKey();
                UserChangedText.Invoke(this, args);
            }
        }

        /// <summary>
        /// Locks a mapping of a single mapping of a homophone
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="auto"></param>
        private void LockHomophone(string symbol, bool auto = false)
        {
            var key = CipherAlphabetTextBox.Text;
            var index = key.IndexOf(symbol);
            if (index > -1)
            {
                if (_hillClimber.AnalyzerConfiguration.LockedHomophoneMappings[index] == -1 || auto)
                {
                    _hillClimber.AnalyzerConfiguration.LockedHomophoneMappings[index] = Tools.MapIntoNumberSpace(string.Empty + PlainAlphabetTextBox.Text[index], PlainAlphabetText)[0];
                }
                else
                {
                    _hillClimber.AnalyzerConfiguration.LockedHomophoneMappings[index] = -1;
                }
                MarkLockedHomophones();
                if (!auto)
                {
                    OnUserChangedText();
                }
            }
        }

        /// <summary>
        /// Marks the locked homophones in the plaintext and ciphertext
        /// </summary>
        private void MarkLockedHomophones()
        {
            StringBuilder lockedElementsStringBuilder = new StringBuilder();
            for (int i = 0; i < _hillClimber.AnalyzerConfiguration.LockedHomophoneMappings.Length; i++)
            {
                if (_hillClimber.AnalyzerConfiguration.LockedHomophoneMappings[i] != -1)
                {
                    lockedElementsStringBuilder.Append(Tools.MapNumbersIntoTextSpace(new int[] { i }, CipherAlphabetText));
                }
            }
            string lockedElementsString = lockedElementsStringBuilder.ToString();

            foreach (SymbolLabel label in _ciphertextLabels)
            {
                if (label == null)
                {
                    continue;
                }
                if (lockedElementsString.Contains(label.Symbol))
                {
                    label.Locked = true;
                    _plaintextLabels[label.X, label.Y].Locked = true;
                    label.Background = Brushes.LightGreen;                    
                    _plaintextLabels[label.X, label.Y].Background = Brushes.LightGreen;
                    
                }
                else
                {
                    label.Locked = false;
                    _plaintextLabels[label.X, label.Y].Locked = false;
                    label.Background = Brushes.White;
                    _plaintextLabels[label.X, label.Y].Background = Brushes.White;
                }
            }
        }

        /// <summary>
        /// Starts and stops the analysis
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Analyze_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lock (this)
                {
                    if (_running == true)
                    {
                        _running = false;
                        _hillClimber.Stop();
                        AnalyzeButton.Content = Properties.Resources.Analyze;
                    }
                    else
                    {
                        AnalyzeButton.Content = Properties.Resources.Stop;
                        ProgressBar.Value = 0;
                        ProgressText.Content = string.Empty;
                        StartAnalysis();
                    }
                }
            }
            catch (Exception)
            {
                //do nothing
                _running = false;
            }
        }

        /// <summary>
        /// Starts the analysis
        /// </summary>
        public void StartAnalysis()
        {
            if (_running)
            {
                return;
            }
            _running = true;

            if (AnalyzerConfiguration.AnalysisMode == AnalysisMode.SemiAutomatic)
            {
                ThreadStart threadStart = () => _hillClimber.Execute();
                Thread thread = new Thread(threadStart)
                {
                    IsBackground = true
                };
                thread.Start();
            }
            else
            {
                ThreadStart threadStart = () =>
                {
                    for (_restart = 0; _restart < AnalyzerConfiguration.Restarts; _restart++)
                    {
                        _hillClimber.Execute();
                        if (_running == false)
                        {
                            return;
                        }
                    }

                };
                Thread thread = new Thread(threadStart)
                {
                    IsBackground = true
                };
                thread.Start();
            }
        }

        /// <summary>
        /// Updates the key letter LetterLimits in the analyzer's config
        /// Also updates the ui; if a non-integer value has been entered, it is set to 0
        /// </summary>
        public void UpdateKeyLetterLimits()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                for (int index = 0; index < AnalyzerConfiguration.KeyLetterLimits.Count; index++)
                {
                    try
                    {
                        int minvalue = 0;
                        int maxvalue = 0;
                        try
                        {
                            minvalue = int.Parse(_minTextBoxes[index].Text);
                        }
                        catch (Exception)
                        {
                            //do nothing
                        }

                        try
                        {
                            maxvalue = int.Parse(_maxTextBoxes[index].Text);
                        }
                        catch (Exception)
                        {
                            //do nothing
                        }

                        LetterLimits limits = AnalyzerConfiguration.KeyLetterLimits[index];
                        limits.MinValue = minvalue;
                        limits.MaxValue = maxvalue;

                        _minTextBoxes[index].Text = string.Empty + minvalue;
                        _maxTextBoxes[index].Text = string.Empty + maxvalue;
                    }
                    catch (Exception)
                    {
                        //do nothing                
                    }
                }
            },
            null);
        }

        /// <summary>
        /// Resets all locked homophone mappings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetLockedLettersButton_Click(object sender, RoutedEventArgs e)
        {
            if (_hillClimber.AnalyzerConfiguration.LockedHomophoneMappings == null || _running == true)
            {
                return;
            }
            for (int i = 0; i < _hillClimber.AnalyzerConfiguration.LockedHomophoneMappings.Length; i++)
            {
                _hillClimber.AnalyzerConfiguration.LockedHomophoneMappings[i] = -1;
            }
            MarkLockedHomophones();
        }

        /// <summary>
        /// Automatically finds and locks words
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FindLockWordsButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_running)
            {
                return;
            }            

            StringBuilder plaintextBuilder = new StringBuilder();
            int rows = _plaintextLabels.Length / AnalyzerConfiguration.TextColumns + (_plaintextLabels.Length % AnalyzerConfiguration.TextColumns > 0 ? 1 : 0);
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < AnalyzerConfiguration.TextColumns; x++)
                {
                    SymbolLabel label = _plaintextLabels[x, y];
                    if (label == null)
                    {
                        continue;
                    }
                    plaintextBuilder.Append(label.Symbol);
                }
            }
            Dictionary<int, int> foundWords = AutoLockWords(1, Tools.MapIntoNumberSpace(plaintextBuilder.ToString(), PlainAlphabetText));
            MarkFoundWords(foundWords);
            OnUserChangedText(); // forward key and plaintext as output
        }

        /// <summary>
        /// Automatically finds and locks nomenclature elements
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FindLockNomenclatureElements_OnClick(object sender, RoutedEventArgs e)
        {
            if (_running)
            {
                return;
            }
            AutoFindAndLockNomenclatureElements();
            OnUserChangedText(); // forward key and plaintext as output
        }

        /// <summary>
        /// Automatically locks and finds nomenclature elements
        /// </summary>
        private void AutoFindAndLockNomenclatureElements()
        {
            var key = CipherAlphabetTextBox.Text;
            
            Dictionary<int, int> nomenclatureElementPositions = _nomenclatureElementFinder.FindNomenclatureElements(AnalyzerConfiguration.NomenclatureElementsThreshold);
            foreach (KeyValuePair<int, int> value in nomenclatureElementPositions)
            {
                for (int i = 0; i < value.Value; i++)
                {
                    int position = value.Key + i;
                    foreach (SymbolLabel label in _ciphertextLabels)
                    {                        
                        if (label == null)
                        {
                            continue;
                        }
                        var index = key.IndexOf(label.Symbol);

                        if(index == -1 || _hillClimber.AnalyzerConfiguration.LockedHomophoneMappings[index] != -1)
                        {
                            continue;
                        }

                        if (label.SymbolOffset == position)
                        {                            
                            LockHomophone(label.Symbol, true);
                            ChangeHomophone(label.Symbol, 0, true); //sets the homophone plaintext to the last element of the plaintext alphabet
                            break;
                        }
                    }
                }
            }            
        }

        /// <summary>
        /// Automatically locks words
        /// Only works, if a WordFinder has previously been created
        /// </summary>
        private Dictionary<int, int> AutoLockWords(int minCount, int[] plaintext)
        {
            if (_wordFinder == null)
            {
                return null;
            }

            Dictionary<int, int> wordPositions = _wordFinder.FindWords(plaintext);
            if (wordPositions.Count < minCount)
            {
                return null;
            }
            foreach (KeyValuePair<int, int> value in wordPositions)
            {
                for (int i = 0; i < value.Value; i++)
                {
                    int position = value.Key + i;
                    foreach (SymbolLabel label in _ciphertextLabels)
                    {
                        if (label == null)
                        {
                            continue;
                        }
                        if (label.SymbolOffset == position)
                        {
                            LockHomophone(label.Symbol, true);
                            break;
                        }
                    }
                }
            }
            return wordPositions;
        }

        /// <summary>
        /// Enables the UI for the user to work with
        /// </summary>
        public void EnableUI()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                if (AnalyzerConfiguration.AnalysisMode == AnalysisMode.SemiAutomatic)
                {
                    AnalyzeButton.IsEnabled = true;
                    FindLockWordsButton.IsEnabled = true;
                    ResetLockedLettersButton.IsEnabled = true;
                    FindLockWordNomenclatureElementsButton.IsEnabled = true;

                    foreach (TextBox box in _minTextBoxes)
                    {
                        if (box == null)
                        {
                            continue;
                        }
                        box.IsEnabled = true;
                    }
                    foreach (TextBox box in _maxTextBoxes)
                    {
                        if (box == null)
                        {
                            continue;
                        }
                        box.IsEnabled = true;
                    }
                }

                foreach (SymbolLabel label in _plaintextLabels)
                {
                    if (label == null)
                    {
                        continue;
                    }
                    label.IsEnabled = true;
                }
                foreach (SymbolLabel label in _ciphertextLabels)
                {
                    if (label == null)
                    {
                        continue;
                    }
                    label.IsEnabled = true;
                }
                AnalyzeButton.Content = Properties.Resources.Analyze;
            }, null);
        }

        /// <summary>
        /// Disables editing and stops everything
        /// </summary>
        public void DisableUIAndStop()
        {
            _running = false;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                AnalyzeButton.IsEnabled = false;
                FindLockWordsButton.IsEnabled = false;
                ResetLockedLettersButton.IsEnabled = false;
                FindLockWordNomenclatureElementsButton.IsEnabled = false;

                foreach (TextBox box in _minTextBoxes)
                {
                    if (box == null)
                    {
                        continue;
                    }
                    box.IsEnabled = false;
                }
                foreach (TextBox box in _maxTextBoxes)
                {
                    if (box == null)
                    {
                        continue;
                    }
                    box.IsEnabled = false;
                }
                foreach (SymbolLabel label in _plaintextLabels)
                {
                    if (label == null)
                    {
                        continue;
                    }
                    label.IsEnabled = false;
                }
                foreach (SymbolLabel label in _ciphertextLabels)
                {
                    if (label == null)
                    {
                        continue;
                    }
                    label.IsEnabled = false;
                }
                AnalyzeButton.Content = Properties.Resources.Analyze;
            }, null);

            if (_hillClimber != null)
            {
                _hillClimber.Stop();
            }
        }

        /// <summary>
        /// Returns, if the analyzer is running
        /// </summary>
        /// <returns></returns>
        public bool IsRunning()
        {
            return _running;
        }

        /// <summary>
        /// Loads the language statistics
        /// </summary>
        /// <param name="language"></param>
        /// <param name="useSpaces"></param>
        public void LoadLangStatistics(int language, bool useSpaces, bool useNulls, int ngramsize = 6)
        {
            lock (this)
            {
                //we use a cache for each language, thus, we do not need to load and load it again
                string key = string.Format("{0}-{1}-{2}", language, useSpaces, ngramsize);
                if (!NGramCache.ContainsKey(key))
                {
                    //this is a "fallback" mechanism; it tries to load ngramsize,...,5,4,3-grams, then it fails
                    bool loaded = false;
                    while (loaded == false)
                    {
                        try
                        {
                            switch (ngramsize)
                            {
                                //we normalize the statistic values between 0 and 10_000_000
                                //then, a starting temperature of 10_000 is a good value for the simulated annealing
                                default:
                                case 6:
                                    Hexagrams hexaGrams = new Hexagrams(LanguageStatistics.LanguageCode(language), DirectoryHelper.DirectoryLanguageStatistics, useSpaces);
                                    hexaGrams.Normalize(10_000_000);
                                    NGramCache.Add(key, hexaGrams);
                                    loaded = true;
                                    break;
                                case 5:
                                    Pentagrams pentaGrams = new Pentagrams(LanguageStatistics.LanguageCode(language), DirectoryHelper.DirectoryLanguageStatistics, useSpaces);
                                    pentaGrams.Normalize(10_000_000);
                                    NGramCache.Add(key, pentaGrams);
                                    loaded = true;
                                    break;
                                case 4:
                                    Tetragrams quadGrams = new Tetragrams(LanguageStatistics.LanguageCode(language), DirectoryHelper.DirectoryLanguageStatistics, useSpaces);
                                    quadGrams.Normalize(10_000_000);
                                    NGramCache.Add(key, quadGrams);
                                    loaded = true;
                                    break;
                                case 3:
                                    Trigrams triGrams = new Trigrams(LanguageStatistics.LanguageCode(language), DirectoryHelper.DirectoryLanguageStatistics, useSpaces);
                                    triGrams.Normalize(10_000_000);
                                    NGramCache.Add(key, triGrams);
                                    loaded = true;
                                    break;
                            }
                        }
                        catch (Exception)
                        {
                            ngramsize--;
                            if (ngramsize == 2)
                            {
                                throw new ArgumentException(string.Format("Could not load any ngrams for language='{0}' useSpaces={1}", LanguageStatistics.LanguageCode(language), useSpaces));
                            }
                        }
                    }
                }
                _grams = NGramCache[key];
                PlainAlphabetText = _grams.Alphabet;
                if (useNulls)
                {
                    PlainAlphabetText = PlainAlphabetText + "#";
                }
            }
        }

        /// <summary>
        /// When ciphertext scroll viewer is scrolled, plaintext scroll viewer is adapted accordingly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CiphertextScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (CiphertextScrollViewer.VerticalOffset != PlaintextScrollViewer.VerticalOffset)
            {
                PlaintextScrollViewer.ScrollToVerticalOffset(CiphertextScrollViewer.VerticalOffset);
            }
            if (CiphertextScrollViewer.HorizontalOffset != PlaintextScrollViewer.HorizontalOffset)
            {
                PlaintextScrollViewer.ScrollToHorizontalOffset(CiphertextScrollViewer.HorizontalOffset);
            }
        }

        /// <summary>
        /// When plaintext scroll viewer is scrolled, ciphertext scroll viewer is adapted accordingly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlaintextScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (CiphertextScrollViewer.VerticalOffset != PlaintextScrollViewer.VerticalOffset)
            {
                CiphertextScrollViewer.ScrollToVerticalOffset(PlaintextScrollViewer.VerticalOffset);
            }
            if (CiphertextScrollViewer.HorizontalOffset != PlaintextScrollViewer.HorizontalOffset)
            {
                CiphertextScrollViewer.ScrollToHorizontalOffset(PlaintextScrollViewer.HorizontalOffset);
            }
        }
    }

    /// <summary>
    /// A special label that knows is x and y coordinates in the grid, the offset of its symbol in the text, and the symbol itself
    /// </summary>
    public class SymbolLabel : Label
    {
        public string Symbol { get; set; }
        public int SymbolOffset { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool Locked { get; set; }
    }

    /// <summary>
    /// ResultEntry of best list
    /// </summary>
    public class ResultEntry : ICrypAnalysisResultListEntry, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int ranking;
        public int Ranking
        {
            get => ranking;
            set
            {
                ranking = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Ranking)));
            }
        }
        public double Value { get; set; }
        public string Key { get; set; }
        public string Text { get; set; }
        public string ClipboardValue => Value.ToString();
        public string ClipboardKey => Key;
        public string ClipboardText => Text;
        public string ClipboardEntry =>
            "Rank: " + Ranking + Environment.NewLine +
            "Value: " + Value + Environment.NewLine +
            "Key: " + Key + Environment.NewLine +
            "Text: " + Text;
    }
}