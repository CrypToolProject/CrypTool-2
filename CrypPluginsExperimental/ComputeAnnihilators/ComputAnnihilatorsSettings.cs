using CrypTool.PluginBase;
using System.Collections;
using System.ComponentModel;

namespace CrypTool.ComputeAnnihilators
{
    internal class ComputeAnnihilatorsSettings : ISettings
    {
        #region Private variables
        private Hashtable savedZfunctions = null;
        private string savedoutputfunction = "";
        private string savedmemoryupdatefunction = "";
        private int saveddegree = 0;
        private bool computeended = false;
        private int savedrunlength = 0;
        private string outputset;
        private int degree;
        private ActionTypes actiontypes = ActionTypes.Combiner;
        public enum ActionTypes { Combiner = 0, function = 1, setofSequence = 2 };
        private OutputTypes outputtypes = OutputTypes.todisplay;
        public enum OutputTypes { todisplay = 0, plugininput = 1, both = 2 };
        #endregion
        #region ISettings Members
        public ActionTypes Actiontypes
        {
            get => actiontypes;
            set
            {
                if (actiontypes != value)
                {
                    actiontypes = value;
                    OnPropertyChanged("ActionSetting");
                }
            }
        }
        [ContextMenu("Action", "Choose application.", 1, ContextMenuControlType.ComboBox, null, new string[] { "Combiner", "function", "setofSequence" })]
        [TaskPane("Action", "Choose application", null, 1, false, ControlType.RadioButton, new string[] { "Z-functions of combiner", "Annihilators of Boolean function", "Annihilators of sets of BitsSequences" })]
        public int ActionSetting
        {
            get => (int)actiontypes;
            set
            {
                if (actiontypes != (ActionTypes)value)
                {
                    actiontypes = (ActionTypes)value;
                    OnPropertyChanged("ActionSetting");
                }
            }
        }
        [TaskPane("Degree ", "most degree of the searched annihilator", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int Degree
        {
            get => degree;
            set
            {
                if (value != degree)
                {
                    degree = value;
                    OnPropertyChanged("Degree");
                }
            }
        }
        public OutputTypes Outputtypes
        {
            get => outputtypes;
            set
            {
                if (outputtypes != value)
                {
                    outputtypes = value;
                    OnPropertyChanged("OutputSetting");
                }
            }
        }
        [ContextMenu("Output Type", "display in Textoutput or delivre to plugin system of equation", 4, ContextMenuControlType.ComboBox, null, new string[] { "todisplay", "plugininput", "both" })]
        [TaskPane("Output Type", "display in Textoutput or delivre to plugin system of equation", "required only in Z-functions", 4, false, ControlType.RadioButton, new string[] { "Display in Textoutput ", "Input of other Plug-in", "both" })]
        public int OutputSetting
        {
            get => (int)outputtypes;
            set
            {
                if (outputtypes != (OutputTypes)value)
                {
                    outputtypes = (OutputTypes)value;
                    OnPropertyChanged("OutputSetting");
                }
            }
        }
        [TaskPane("Outputs Z", "express a set of output Z to determine Z-function to output", "required only in Z-functions", 5, false, ControlType.TextBox, ValidationType.RegEx, "^(1|[\\*]|0)*")]
        public string OutputSet
        {
            get => outputset;
            set
            {
                if (value != outputset)
                {
                    outputset = value;
                    OnPropertyChanged("OutputSet");
                }
            }
        }
        public Hashtable SavedZfunctions
        {
            get => savedZfunctions;
            set
            {
                if (value != savedZfunctions)
                {
                    savedZfunctions = value;
                    OnPropertyChanged("SavedZfunctions");
                }
            }
        }
        public string Savedoutputfunction
        {
            get => savedoutputfunction;
            set
            {
                if (value != savedoutputfunction)
                {
                    savedoutputfunction = value;
                    OnPropertyChanged("Savedoutputfunction");
                }
            }
        }
        public string Savedmemoryupdatefunction
        {
            get => savedmemoryupdatefunction;
            set
            {
                if (value != savedmemoryupdatefunction)
                {
                    savedmemoryupdatefunction = value;
                    OnPropertyChanged("Savedmemoryupdatefunction");
                }

            }
        }
        public int Savedrunlength
        {
            get => savedrunlength;
            set
            {
                if (value != savedrunlength)
                {
                    savedrunlength = value;
                    OnPropertyChanged("Savedrunlength");
                }
            }
        }
        public int Saveddegree
        {
            get => saveddegree;
            set
            {
                if (value != saveddegree)
                {
                    saveddegree = value;
                    OnPropertyChanged("Saveddegree");
                }
            }
        }
        public bool ComputeEnded
        {
            get => computeended;
            set
            {
                if (value != computeended)
                {
                    computeended = value;
                    OnPropertyChanged("ComputeEnded");
                }
            }
        }
        public delegate void ComputeAnnihilatorsLogMessage(string msg, NotificationLevel logLevel);
        public event ComputeAnnihilatorsLogMessage LogMessage;

        #endregion
        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion
    }
}
