using CrypTool.PluginBase;
using System.ComponentModel;

namespace CrypTool.SystemOfEquations
{
    internal class SystemOfEquationsSettings : ISettings
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
            get => feedbackpolynomials;
            set
            {
                if (value != feedbackpolynomials)
                {
                    feedbackpolynomials = value;
                    OnPropertyChanged("Feedbackpolynomials");
                }
            }
        }
        [TaskPane("Output cells of LFSRs", " Output  cells of LFSRS in bit presentation ", null, 2, false, ControlType.TextBox, ValidationType.RegEx, null)]
        public string Lfsrsoutputs
        {
            get => lfsrsoutputs;
            set
            {
                if (value != lfsrsoutputs)
                {
                    lfsrsoutputs = value;
                    OnPropertyChanged("Lfsrsoutputs");
                }
            }
        }
        [TaskPane("Keystream sequences", "known keystream sequences", null, 3, false, ControlType.TextBox, ValidationType.RegEx, null)]
        public string Keystream
        {
            get => keystream;
            set
            {
                if (value != keystream)
                {
                    keystream = value;
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
