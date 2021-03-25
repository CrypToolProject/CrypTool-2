using System;
using System.ComponentModel;
using CrypTool.PluginBase;

namespace CrypTool.SystemOfEquations
{
    class SystemOfEquationsSettings : ISettings
    {
        #region Private variables
        private string keystream;
        private string feedbackpolynomials;
        private string lfsrsoutputs;
        #endregion
        #region ISettings Members
        [TaskPane("Feedback polynomials of LFSRs", "Feedback polynomials of LFSRs in bit presentation ", null, 1, false, ControlType.TextBox, ValidationType.RegEx, null)]
        public string Feedbackpolynomials
        {
            get { return this.feedbackpolynomials; }
            set
            {
                if (value != feedbackpolynomials)
                {
                    this.feedbackpolynomials = value;
                    OnPropertyChanged("Feedbackpolynomials");
                }
            }
        }
        [TaskPane("Output cells of LFSRs", " Output  cells of LFSRS in bit presentation ", null, 2, false, ControlType.TextBox, ValidationType.RegEx, null)]
        public string Lfsrsoutputs
        {
            get { return this.lfsrsoutputs; }
            set
            {
                if (value != lfsrsoutputs)
                {
                    this.lfsrsoutputs = value;
                    OnPropertyChanged("Lfsrsoutputs");
                }
            }
        }
        [TaskPane("Keystream sequences", "known keystream sequences", null, 3, false, ControlType.TextBox, ValidationType.RegEx, null)]
        public string Keystream
        {
            get { return this.keystream; }
            set
            {
                if (value != keystream)
                {
                    this.keystream = value;
                    OnPropertyChanged("Keystream");
                }
            }
        }
        public delegate void SystemOfEquationsLogMessage(string msg, NotificationLevel logLevel);
        public event SystemOfEquationsLogMessage LogMessage;

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
