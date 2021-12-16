using CrypTool.PluginBase;
using System.ComponentModel;

namespace WordPatterns
{
    public enum Case
    {
        Sensitive,
        Insensitive
    }

    public class WordPatternsSettings : ISettings
    {
        private Case caseSelection = Case.Insensitive;
        private string separators = "";
        private bool homophonic = false;

        [TaskPane("CaseSelectionCaption", "CaseSelectionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "CaseSelectionList1", "CaseSelectionList2" })]
        public Case CaseSelection
        {
            get => caseSelection;
            set
            {
                if (caseSelection != value)
                {
                    caseSelection = value;
                    OnPropertyChanged("CaseSelection");
                }
            }
        }

        [TaskPane("HomophonicCaption", "HomophonicTooltip", null, 2, false, ControlType.CheckBox)]
        public bool Homophonic
        {
            get => homophonic;
            set
            {
                if (homophonic != value)
                {
                    homophonic = value;
                    OnPropertyChanged("Homophonic");
                }
            }
        }

        private bool sort = false;
        //[TaskPane("SortCaption", "SortTooltip", null, 2, false, ControlType.CheckBox)]
        public bool Sort
        {
            get => sort;
            set
            {
                if (sort != value)
                {
                    sort = value;
                    OnPropertyChanged("Sort");
                }
            }
        }

        /// <summary>
        /// Separator characters used to split the input
        /// </summary>
        [TaskPane("SeparatorsSettingCaption", "SeparatorsSettingTooltip", null, 4, false, ControlType.TextBox)]
        public string Separators
        {
            get => separators;
            set
            {
                if (separators != value)
                {
                    separators = value;
                    OnPropertyChanged("Separators");
                }
            }
        }

        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        #endregion
    }
}
