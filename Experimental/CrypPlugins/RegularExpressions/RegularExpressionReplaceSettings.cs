using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System.ComponentModel;

namespace RegularExpressions
{
    public class RegularExpressionReplaceSettings : ISettings
    {

        #region taskpane

        private string patternValue;
        [TaskPane("RegexPattern", "Pattern to be replaced.", null, 0, false, ControlType.TextBox, ValidationType.RegEx, null)]
        public string PatternValue
        {
            get => patternValue;
            set
            {
                if (value != patternValue)
                {
                    patternValue = value;
                    OnPropertyChanges("PatternValue");
                }
            }
        }

        private string replaceValue;
        [TaskPane("Replacement", "Word to replace the pattern.", null, 0, false, ControlType.TextBox)]
        public string ReplaceValue
        {
            get => replaceValue;
            set
            {
                if (value != replaceValue)
                {
                    replaceValue = value;
                    OnPropertyChanges("ReplaceValue");
                }
            }
        }

        private void OnPropertyChanges(string p)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, p);
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        #endregion
    }
}
