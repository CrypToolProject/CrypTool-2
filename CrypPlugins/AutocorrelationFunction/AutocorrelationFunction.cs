/* HOWTO: Change year, author name and organization.
   Copyright 2010 Your Name, University of Duckburg

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
using CrypTool.PluginBase.Utils.Graphics.Diagrams.Histogram;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace CrypTool.Plugins.AutocorrelationFunction
{
    [Author("Dennis Nolte", "nolte@CrypTool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("AutocorrelationFunction.Properties.Resources", "PluginCaption", "PluginTooltip", "AutocorrelationFunction/DetailedDescription/doc.xml", "AutocorrelationFunction/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
    public class AutocorrelationFunction : ICrypComponent
    {
        #region Private Variables
        private const int MAX_TESTED_SHIFTS = 31;

        private readonly AutocorrelationPresentation _presentation;
        private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private string _inputText;
        private int _probableLength;
        private double _probablekorr;
        private double[] _autocorrelationValues;
        private HistogramElement _histogramElement;
        private readonly HistogramDataSource _data;

        #endregion

        #region Data Properties
     
        [PropertyInfo(Direction.InputData, "InputCipherCaption", "InputCipherTooltip", true)]
        public string InputText
        {
            get => _inputText;
            set
            {
                _inputText = value;
                OnPropertyChanged("InputText");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputLengthCaption", "OutputLengthTooltip")]
        public int OutputLength
        {
            get => _probableLength;
            set
            {
                _probableLength = value;
                OnPropertyChanged("OutputLength");
            }
        }

        #endregion

        #region IPlugin Members

        public AutocorrelationFunction()
        {
            _presentation = new AutocorrelationPresentation();
            _data = new HistogramDataSource();
        }
        public ISettings Settings => null;

        public UserControl Presentation => _presentation;

        public void PreExecution()
        {
        }

        public void Execute()
        {
            if (string.IsNullOrEmpty(InputText) || string.IsNullOrWhiteSpace(InputText))
            {
                return;
            }

            ProgressChanged(0, 1);

            _probableLength = 0;
            _probablekorr = double.MinValue;
            _inputText = PrepareForAnalysis(_inputText);
            _autocorrelationValues = new double[MAX_TESTED_SHIFTS];

            _presentation.histogram.SetHeadline(typeof(AutocorrelationFunction).GetPluginStringResource("Autocorrelation_matches"));

            //for each possible shift value...
            for (int shift = 0; shift < MAX_TESTED_SHIFTS; shift++)
            {
                int match = 0;

                //...calculate how often the letters match...
                for (int offset = 0; offset < _inputText.Length - shift; offset++)
                {
                    if (_inputText[offset] == _inputText[offset + shift])
                    {
                        match++;
                    }
                }

                try
                {
                    //...and save the count for the matches at the shift position
                    _autocorrelationValues[shift] = match;
                }
                catch
                {
                }
            }

            _data.ValueCollection.Clear();

            for (int i = 1; i < MAX_TESTED_SHIFTS; i++)
            {
                //find the one with the highest match count...
                if (_autocorrelationValues[i] > _probablekorr)
                {
                    _probablekorr = _autocorrelationValues[i];
                    _probableLength = i;
                }
            }

            for (int i = 1; i < _autocorrelationValues.Length; i++)
            {
                if (_autocorrelationValues[i] > -1) 
                {
                    _histogramElement = new HistogramElement(_autocorrelationValues[i], _autocorrelationValues[i], string.Empty + i);
                    _data.ValueCollection.Add(_histogramElement);
                }
            }

            _presentation.histogram.SetHeadline(string.Format(typeof(AutocorrelationFunction).GetPluginStringResource("Highest_match_count_with_shift"), _probablekorr, _probableLength));

            if (_data != null)
            {
                _presentation.histogram.ShowData(_data);
            }

            OutputLength = _probableLength;
            OnPropertyChanged("OutputLength");
            ProgressChanged(1, 1);

        }

        public void PostExecution()
        {
        }

        public void Stop()
        {            
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Remove spaces and non-alphabet symbols from the text
        /// </summary>
        private string PrepareForAnalysis(string text)
        {
            StringBuilder stringBuilder = new StringBuilder();
            text = text.ToUpper();

            for (int offset = 0; offset < text.Length; offset++)
            {
                if (Alphabet.Contains(text.Substring(offset, 1)))
                {
                    stringBuilder.Append(text[offset]);
                }
            }
            return stringBuilder.ToString();
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