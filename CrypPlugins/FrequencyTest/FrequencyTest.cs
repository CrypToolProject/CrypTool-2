/*  
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace CrypTool.FrequencyTest
{
    [Author("Georgi Angelov, Danail Vazov, Matthäus Wander, Nils Kopal", "angelov@CrypTool.org", "Uni Duisburg", "http://www.uni-duisburg-essen.de")]
    [PluginInfo("CrypTool.FrequencyTest.Properties.Resources", "PluginCaption", "PluginTooltip", "FrequencyTest/DetailedDescription/doc.xml", "FrequencyTest/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
    public class FrequencyTest : ICrypComponent
    {
        #region Const and variable definitions

        public const int ABSOLUTE = 0;
        public const int PERCENTAGED = 1;
        public const int LOG2 = 2;
        public const int SINKOV = 3;

        private string stringInput;
        private readonly IDictionary<string, double[]> _grams = new SortedDictionary<string, double[]>();
        private readonly DataSource _datta = new DataSource();
        private double _presentationScaler = 1.0; // the initial zoom value
        private readonly double _presentationBarWidth = 38.0; // the width in pixel of a single chart bar
        private readonly double _presentationBarHeightAdd = 8.0 + 2.0 * 26.0; // the additional heigth to a chart bar, comprised of two rectangles (3px, 5px) and two textblocks

        private const string DEFAULT_ALPHABET = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private string alphabet = DEFAULT_ALPHABET;

        #endregion

        #region Properties (Inputs/Outputs)

        [PropertyInfo(Direction.InputData, "StringInputCaption", "StringInputTooltip", true)]
        public string StringInput
        {
            get => stringInput;
            set
            {
                stringInput = value;
                OnPropertyChanged("StringInput");
            }
        }

        [PropertyInfo(Direction.InputData, "AlphabetCaption", "AlphabetTooltip", false)]
        public string Alphabet
        {
            get => alphabet;
            set
            {
                alphabet = value;
                OnPropertyChanged("Alphabet");
            }
        }

        [PropertyInfo(Direction.OutputData, "StringOutputCaption", "StringOutputTooltip", false)]
        public string StringOutput { get; private set; } = "";

        [PropertyInfo(Direction.OutputData, "ArrayOutputCaption", "ArrayOutputTooltip", false)]
        public int[] ArrayOutput { get; private set; } = new int[0];

        #endregion

        #region IPlugin Members

        private FrequencyTestSettings settings;
        public ISettings Settings
        {
            get => settings;
            set => settings = (FrequencyTestSettings)value;
        }
        private readonly FrequencyTestPresentation presentation;
        public FrequencyTest()
        {
            settings = new FrequencyTestSettings();
            presentation = new FrequencyTestPresentation();
            Presentation = presentation;
            presentation.SizeChanged += new System.Windows.SizeChangedEventHandler(presentation_SizeChanged);
            settings.PropertyChanged += new PropertyChangedEventHandler(settings_PropertyChanged);
        }

        public UserControl Presentation
        {
            get;
            private set;
        }

        public void PreExecution()
        {
            alphabet = DEFAULT_ALPHABET;
        }

        public void Execute()
        {
            Progress(0.0, 0.0);

            if (stringInput == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(alphabet))
            {
                GuiLogMessage(Properties.Resources.EmptyAlphabetInvalidMessage, NotificationLevel.Warning);
                return;
            }

            presentation.SetBackground(Brushes.LightYellow);

            string workstring = stringInput;

            // Any change in the word discards and recalculates the output. This is not that effective.
            lock (_grams)
            {
                _grams.Clear();

                if (settings.BoundaryFragments == 1)
                {
                    foreach (string word in WordTokenizer.tokenize(workstring))
                    {
                        ProcessWord(word);
                    }
                }
                else
                {
                    ProcessWord(workstring);
                }

                double sum = _grams.Values.Sum(item => item[ABSOLUTE]);

                // calculate scaled values
                foreach (double[] g in _grams.Values)
                {
                    g[PERCENTAGED] = g[ABSOLUTE] / sum;
                    g[LOG2] = Math.Log(g[ABSOLUTE], 2);
                    g[SINKOV] = Math.Log(g[PERCENTAGED], Math.E);
                }

                // OUTPUT
                StringBuilder sb = new StringBuilder();
                ArrayOutput = new int[_grams.Count];

                //here, we sort by frequency occurrence if the user wants so
                if (settings.SortFrequencies)
                {
                    List<KeyValuePair<string, double[]>> list = _grams.ToList();
                    list.Sort(delegate (KeyValuePair<string, double[]> a, KeyValuePair<string, double[]> b)
                    {
                        return a.Value[ABSOLUTE] > b.Value[ABSOLUTE] ? -1 : 1;
                    });

                    _grams.Clear();

                    foreach (KeyValuePair<string, double[]> i in list)
                    {
                        _grams.Add(i.Key, i.Value);
                    }
                }

                for (int i = 0; i < _grams.Count; i++)
                {
                    KeyValuePair<string, double[]> item = _grams.ElementAt(i);

                    sb.Append(item.Key + ":");
                    sb.Append(item.Value[ABSOLUTE] + ":");
                    sb.Append(item.Value[PERCENTAGED] + Environment.NewLine);

                    ArrayOutput[i] = (int)item.Value[ABSOLUTE];
                }
                StringOutput = sb.ToString();

                // update the presentation data
                UpdatePresentation();
            }

            OnPropertyChanged("StringOutput");
            OnPropertyChanged("ArrayOutput");

            // Show progress finished.
            Progress(1.0, 1.0);
        }

        /// <summary>
        /// Processes a single word.
        /// </summary>
        /// <param name="workstring"></param>
        private void ProcessWord(string workstring)
        {
            if(settings.CaseSensitivity == 0)
            {
                workstring = workstring.ToUpper();
                alphabet = alphabet.ToUpper();
            }

            if (settings.ProcessUnknownSymbols == 0)
            {
                workstring = StringUtil.StripUnknownSymbols(alphabet + (settings.AnalysisMode == AnalysisMode.SymbolSeparated ? settings.SymbolSeparators : ""), workstring);
            }

            if (workstring.Length == 0)
            {
                return;
            }

            if (settings.CaseSensitivity == 0)
            {
                workstring = workstring.ToUpper();
            }

            if(settings.AnalysisMode == AnalysisMode.NGrams)
            {
                int stepsize = 1;
                if (!settings.CountOverlapping)
                {
                    stepsize = settings.GrammLength;
                }

                foreach (string g in GramTokenizer.tokenize(workstring, settings.GrammLength, settings.BoundaryFragments == 1, stepsize))
                {
                    if (!_grams.ContainsKey(g))
                    {
                        _grams[g] = new double[] { 1, 0, 0, 0 };
                    }
                    else
                    {
                        _grams[g][ABSOLUTE]++;
                    }
                }
            }
            else
            {
                string separators = ".";
                if (!string.IsNullOrEmpty(settings.SymbolSeparators))
                {
                    separators = settings.SymbolSeparators;
                }
                foreach (string g in SymbolTokenizer.tokenize(workstring, separators))
                {
                    if (!_grams.ContainsKey(g))
                    {
                        _grams[g] = new double[] { 1, 0, 0, 0 };
                    }
                    else
                    {
                        _grams[g][ABSOLUTE]++;
                    }
                }
            }         
        }

        /// <summary>
        /// Updates the presentation data.
        /// </summary>
        private void UpdatePresentation()
        {

            // remove all entries
            _datta.ValueCollection.Clear();

            //create header text
            string valueType = Properties.Resources.InPercent;
            if (settings.ShowAbsoluteValues)
            {
                valueType = Properties.Resources.AbsoluteValues;
            }
            if (settings.AnalysisMode == AnalysisMode.NGrams)
            {
                switch (settings.GrammLength)
                {
                    case 1:
                        presentation.SetHeadline(Properties.Resources.UnigramFrequencies + " " + valueType);
                        break;
                    case 2:
                        presentation.SetHeadline(Properties.Resources.BigramFrequencies + " " + valueType);
                        break;
                    case 3:
                        presentation.SetHeadline(Properties.Resources.TrigramFrequencies + " " + valueType);
                        break;
                    case 4:
                        presentation.SetHeadline(Properties.Resources.TetragramFrequencies + " " + valueType);
                        break;
                    case 5:
                        presentation.SetHeadline(Properties.Resources.PentagramFrequencies + " " + valueType);
                        break;
                    case 6:
                        presentation.SetHeadline(Properties.Resources.HexagramFrequencies + " " + valueType);
                        break;
                    case 7:
                        presentation.SetHeadline(Properties.Resources.HeptagramFrequencies + " " + valueType);
                        break;
                    case 8:
                        presentation.SetHeadline(Properties.Resources.OctagramFrequencies + " " + valueType);
                        break;
                    default:
                        presentation.SetHeadline(settings.GrammLength + Properties.Resources.nGram + " " + valueType);
                        break;
                }
            }
            else
            {
                presentation.SetHeadline(Properties.Resources.SymbolSeparated + Properties.Resources.Frequencies);
            }

            //update bars
            if (_grams.Count > 0 && presentation.ActualWidth > 0)
            {
                // retrieve the maximum value from all grams
                double max = _grams.Values.Max(item => item[PERCENTAGED]);

                // calculate the needed width for the chart (unscaled) in pixel
                double unscaledChartWidth = (_grams.Count < 10 ? 10 : _grams.Count + (settings.ShowTotal ? 1 : 0)) * _presentationBarWidth + 3;
                if (_grams.Count > settings.MaxNumberOfShownNGramms + (settings.ShowTotal ? 1 : 0))
                {
                    unscaledChartWidth = (settings.MaxNumberOfShownNGramms + (settings.ShowTotal ? 1 : 0)) * _presentationBarWidth + 3;
                }

                // retrieve the maximum bar height from settings in pixel
                double maxBarHeight = settings.ChartHeight;
                if (settings.Autozoom)
                {
                    // calculate the scaling-value depeding on the needed width and the current presentation width
                    _presentationScaler = presentation.ActualWidth / unscaledChartWidth;
                    settings.Scale = (int)(_presentationScaler * 10000.0);

                    //set the maximum bar height to the current heigth of chart-area in presentation (best fill)
                    //maxBarHeight = presentation.chartBars.ActualHeight - presentationBarHeightAdd;
                    maxBarHeight = (presentation.ActualHeight / _presentationScaler) - (presentation.chartHeadline.ActualHeight + _presentationBarHeightAdd);
                }

                //count all grams and create a total bar
                if (settings.ShowTotal)
                {
                    int sum = (int)_grams.Values.Sum(item => item[ABSOLUTE]);
                    CollectionElement element = new CollectionElement(1.0001 * max * (maxBarHeight / max), sum, 100, "Σ", true)
                    {
                        ColorA = Colors.LightGreen,
                        ColorB = Colors.DarkGreen
                    };
                    _datta.ValueCollection.Add(element);
                }

                // calculate presentation bars height and add the to our local DataSource
                foreach (KeyValuePair<string, double[]> item in _grams)
                {
                    double height = item.Value[PERCENTAGED] * (maxBarHeight / max);
                    CollectionElement row = new CollectionElement(height, (int)item.Value[ABSOLUTE], Math.Round(item.Value[PERCENTAGED] * 100, 2), item.Key, settings.ShowAbsoluteValues);
                    _datta.ValueCollection.Add(row);
                }

                //add dummy bars
                while (_datta.ValueCollection.Count + (settings.ShowTotal ? 1 : 0) < 10)
                {
                    _datta.ValueCollection.Add(new CollectionElement(0, 0, 0, string.Empty, false, System.Windows.Visibility.Visible));
                }
            }

            //finally, update ui
            presentation.ShowData(_datta, settings.SortFrequencies, settings.MaxNumberOfShownNGramms + (settings.ShowTotal ? 1 : 0));

        }

        private void presentation_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            UpdatePresentation();
        }

        private void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Autozoom":
                case "ChartHeight":
                    UpdatePresentation();
                    break;
                case "Scale":
                    presentation.SetScaler(settings.Scale / 10000.0);
                    break;
            }
        }


        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
            presentation.SetBackground(Brushes.LightGray);
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void Progress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion

    }
}