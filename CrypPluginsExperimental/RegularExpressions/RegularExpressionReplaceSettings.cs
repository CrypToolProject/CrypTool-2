using System;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System.ComponentModel;

namespace RegularExpressions
{
    public class RegularExpressionReplaceSettings :ISettings
    {

        #region taskpane
        
        private String patternValue;
        [TaskPane("RegexPattern", "Pattern to be replaced.", null, 0, false, ControlType.TextBox, ValidationType.RegEx, null)]
        public String PatternValue
        {
            get { return this.patternValue; }
            set
            {
                if (value != this.patternValue)
                {
                    this.patternValue = value;
                    OnPropertyChanges("PatternValue");
                }
            }
        }

        private String replaceValue;
        [TaskPane("Replacement", "Word to replace the pattern.", null, 0, false, ControlType.TextBox)]
        public String ReplaceValue
        {
            get { return this.replaceValue; }
            set
            {
                if (value != this.replaceValue)
                {
                    this.replaceValue = value;
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
