using System;
using System.Collections;
using System.ComponentModel;
using CrypTool.PluginBase;

namespace CrypTool.ComputeXZ
{
    class ComputeXZSettings : ISettings
    {
        #region Private variables
        private int savedstartX = 0;
        private Hashtable savedXZ = null;
        private string savedoutputfunction = "";
        private string savedmemoryupdatefunction = "";
        private int savedrunlength = 0;
        private bool isxzcomputed = false;
        string outputs;
        private OutputTypes outputtypes = OutputTypes.todisplay;
        public enum OutputTypes { todisplay = 0, plugininput = 1, both = 2 };
        #endregion

        #region ISettings Members
        public OutputTypes Output
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
        [TaskPane("OutputTypes of XZ", "Choose Outputtype of th sets XZ", null, 2, false, ControlType.RadioButton, new string[] { "Display in TextOutput ", "Input of other Plug-in", "both" })]
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
        [TaskPane("Outputs Z", "express a sets of output Z to determine the set XZ to output", null, 3, false, ControlType.TextBox, ValidationType.RegEx, null)]
        public string SetOfOutputs
        {
            get { return this.outputs; }
            set
            {
                if (((string)value) != outputs)
                {
                    this.outputs = value;
                    OnPropertyChanged("SetOfOutputs");   
                }
            }
        }
        public string Saveoutputfunction
        {
            get { return savedoutputfunction; }
            set
            {
                if (value != savedoutputfunction)
                {
                    savedoutputfunction = value; 
                    OnPropertyChanged("Saveoutputfunction");
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
        public int SavedstartX
        {
            get { return savedstartX; }
            set
            {
                if (value != savedstartX)
                {
                    savedstartX = value;
                    OnPropertyChanged("Savedstartx");
                }
            }
        }
        public bool IsXZcomputed
        {
            get { return isxzcomputed; }
            set
            {
                if (value != isxzcomputed)
                {
                    isxzcomputed = value;
                    OnPropertyChanged("IsXZcomputed");
                }
            }
        }
        public Hashtable SavedXZ
        {
            get { return savedXZ; }
            set
            {
                if (value != savedXZ)
                {
                    savedXZ = value;
                    OnPropertyChanged("SavedXZ");
                }
            }
        }
        public delegate void ComputeXZLogMessage(string msg, NotificationLevel logLevel);
        public event ComputeXZLogMessage LogMessage;

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
