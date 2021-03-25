using System;
using System.Collections;
using System.ComponentModel;
using CrypTool.PluginBase;

namespace CrypTool.ComputeAnnihilators
{
    class ComputeAnnihilatorsSettings : ISettings
    {
        #region Private variables
        private Hashtable savedZfunctions = null;
        private string savedoutputfunction = "";
        private string savedmemoryupdatefunction = "";
        private int saveddegree = 0;
        private bool computeended = false;
        private int savedrunlength = 0;
        string outputset;
        private int degree;
        private ActionTypes actiontypes = ActionTypes.Combiner;
        public enum ActionTypes { Combiner = 0, function = 1, setofSequence = 2 };
        private OutputTypes outputtypes = OutputTypes.todisplay;
        public enum OutputTypes { todisplay = 0, plugininput = 1, both = 2 };
        #endregion
        #region ISettings Members
        public ActionTypes Actiontypes
        {
            get { return this.actiontypes; }
            set
            {
                if (this.actiontypes != value)
                {
                    this.actiontypes = value;
                    OnPropertyChanged("ActionSetting");   
                }
            }
        }
        [ContextMenu("Action", "Choose application.", 1, ContextMenuControlType.ComboBox, null, new string[] { "Combiner", "function", "setofSequence" })]
        [TaskPane("Action", "Choose application", null, 1, false, ControlType.RadioButton, new string[] { "Z-functions of combiner", "Annihilators of Boolean function", "Annihilators of sets of BitsSequences" })]
        public int ActionSetting
        {
            get
            {
                return (int)this.actiontypes;
            }
            set
            {
                if (this.actiontypes != (ActionTypes)value)
                {
                    this.actiontypes = (ActionTypes)value;
                    OnPropertyChanged("ActionSetting");   
                }
            }
        }
        [TaskPane("Degree ", "most degree of the searched annihilator", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger,0, int.MaxValue)]
        public int Degree
        {
            get { return this.degree; }
            set
            {
                if (value != degree)
                {
                    this.degree = value;
                    OnPropertyChanged("Degree");   
                }
            }
        }
        public OutputTypes Outputtypes
        {
            get { return this.outputtypes; }
            set
            {
                if (this.outputtypes != value)
                {
                    this.outputtypes = value;
                    OnPropertyChanged("OutputSetting");   
                }
            }
        }
        [ContextMenu("Output Type", "display in Textoutput or delivre to plugin system of equation", 4, ContextMenuControlType.ComboBox, null, new string[] { "todisplay", "plugininput", "both" })]
        [TaskPane("Output Type", "display in Textoutput or delivre to plugin system of equation", "required only in Z-functions", 4, false, ControlType.RadioButton, new string[] { "Display in Textoutput ", "Input of other Plug-in", "both" })]
        public int OutputSetting
        {
            get
            {
                return (int)this.outputtypes;
            }
            set
            {
                if (this.outputtypes != (OutputTypes)value)
                {
                    this.outputtypes = (OutputTypes)value;
                    OnPropertyChanged("OutputSetting");   
                }
            }
        }
        [TaskPane("Outputs Z", "express a set of output Z to determine Z-function to output", "required only in Z-functions", 5, false, ControlType.TextBox, ValidationType.RegEx, "^(1|[\\*]|0)*")]
        public string OutputSet
        {
            get { return this.outputset; }
            set
            {
                if (value != outputset)
                {
                    this.outputset = value;
                    OnPropertyChanged("OutputSet");   
                }
            }
        }
        public Hashtable SavedZfunctions
        {
            get { return savedZfunctions; }
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
            get { return savedoutputfunction; }
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
            get { return savedmemoryupdatefunction; }
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
            get { return savedrunlength; }
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
            get { return saveddegree; }
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
            get { return computeended; }
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

        protected void OnPropertyChanged(String name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion
    }
}
