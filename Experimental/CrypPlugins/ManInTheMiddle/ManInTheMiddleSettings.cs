using CrypTool.PluginBase;
using System.ComponentModel;

namespace ManInTheMiddle
{
    internal class ManInTheMiddleSettings : ISettings
    {
        private bool send;

        [TaskPane("Insert own SOAP-Body", "If checked the plugin will send the output message on play", null, 0, false, ControlType.CheckBox)]
        public bool insertBody
        {
            get => send;
            set
            {
                send = value;
                OnPropertyChanged("insertBody");
            }
        }

        #region variables

        private string soap;
        public string Soap
        {
            get => soap;
            set
            {
                soap = value;
                OnPropertyChanged("Soap");
            }
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}
