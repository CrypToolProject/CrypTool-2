using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System.ComponentModel;

namespace CrypTool.Plugins.UbchiPermutationCryptanalysis
{
    public class UbchiPermutationAnalysisSettings : ISettings
    {
        private int _minKeyLength = 8;
        private int _maxKeyLength = 32;
        private int _minNulls = 0;
        private int _maxNulls = 5;
        private int _maxCandidates = 25;
        private int _timeoutSeconds = 30;

        [TaskPane("MinKeyLength_Caption", "MinKeyLength_Tooltip", null, 1, false,
            ControlType.NumericUpDown, ValidationType.RangeInteger, 2, 40)]
        public int MinKeyLength
        {
            get { return _minKeyLength; }
            set { if (_minKeyLength != value) { _minKeyLength = value; OnPropertyChanged("MinKeyLength"); } }
        }

        [TaskPane("MaxKeyLength_Caption", "MaxKeyLength_Tooltip", null, 2, false,
            ControlType.NumericUpDown, ValidationType.RangeInteger, 2, 40)]
        public int MaxKeyLength
        {
            get { return _maxKeyLength; }
            set { if (_maxKeyLength != value) { _maxKeyLength = value; OnPropertyChanged("MaxKeyLength"); } }
        }

        [TaskPane("MinNulls_Caption", "MinNulls_Tooltip", null, 3, false,
            ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 10)]
        public int MinNulls
        {
            get { return _minNulls; }
            set { if (_minNulls != value) { _minNulls = value; OnPropertyChanged("MinNulls"); } }
        }

        [TaskPane("MaxNulls_Caption", "MaxNulls_Tooltip", null, 4, false,
            ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 10)]
        public int MaxNulls
        {
            get { return _maxNulls; }
            set { if (_maxNulls != value) { _maxNulls = value; OnPropertyChanged("MaxNulls"); } }
        }

        [TaskPane("MaxCandidates_Caption", "MaxCandidates_Tooltip", null, 5, false,
            ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 200)]
        public int MaxCandidates
        {
            get { return _maxCandidates; }
            set { if (_maxCandidates != value) { _maxCandidates = value; OnPropertyChanged("MaxCandidates"); } }
        }

        [TaskPane("TimeoutSeconds_Caption", "TimeoutSeconds_Tooltip", null, 6, false,
            ControlType.NumericUpDown, ValidationType.RangeInteger, 5, 600)]
        public int TimeoutSeconds
        {
            get { return _timeoutSeconds; }
            set { if (_timeoutSeconds != value) { _timeoutSeconds = value; OnPropertyChanged("TimeoutSeconds"); } }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        public void Initialize() { }
    }
}
