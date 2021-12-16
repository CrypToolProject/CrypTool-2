using CrypTool.PluginBase;

namespace PrimeTest
{
    public class PrimeTestSettings : ISettings
    {
        #region ISettings Members

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        private void FirePropertyChangeEvent(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
