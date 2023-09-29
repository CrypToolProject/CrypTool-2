using CrypTool.PluginBase;
using System.Collections;
using System.ComponentModel;

namespace CrypTool.ComputeXZ
{
    internal class ComputeXZSettings : ISettings
    {
        #region Private variables
        private int savedstartX = 0;
        private Hashtable savedXZ = null;
        private string savedoutputfunction = "";
        private string savedmemoryupdatefunction = "";
        private int savedrunlength = 0;
        private bool isxzcomputed = false;
        private string outputs;
        private OutputTypes outputtypes = OutputTypes.todisplay;
        public enum OutputTypes { todisplay = 0, plugininput = 1, both = 2 };
        #endregion

        #region ISettings Members
        public OutputTypes Output
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
        [TaskPane("OutputTypes of XZ", "Choose Outputtype of th sets XZ", null, 2, false, ControlType.RadioButton, new string[] { "Display in TextOutput ", "Input of other Plug-in", "both" })]
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
        [TaskPane("Outputs Z", "express a sets of output Z to determine the set XZ to output", null, 3, false, ControlType.TextBox, ValidationType.RegEx, null)]
        public string SetOfOutputs
        {
            get => outputs;
            set
            {
                if (value != outputs)
                {
                    outputs = value;
                    OnPropertyChanged("SetOfOutputs");
                }
            }
        }
        public string Saveoutputfunction
        {
            get => savedoutputfunction;
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
        public int SavedstartX
        {
            get => savedstartX;
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
            get => isxzcomputed;
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
            get => savedXZ;
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
