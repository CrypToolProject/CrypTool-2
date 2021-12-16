using CrypTool.PluginBase;
using System.ComponentModel;

namespace FriedmanTest
{
    internal class FriedmanTestSettings : ISettings
    {

        private int kappa = 0; //0="English", 1="German", 2="French", 3="Spanish", 4="Italian",5="Portugeese"
        #region ISettings Members

        [ContextMenu("KappaCaption", "KappaTooltip", 2, ContextMenuControlType.ComboBox, null, new string[] { "KappaList1", "KappaList2", "KappaList3", "KappaList4", "KappaList5", "KappaList6" })]
        [TaskPane("KappaCaption", "KappaTooltip", null, 2, false, ControlType.ComboBox, new string[] { "KappaList1", "KappaList2", "KappaList3", "KappaList4", "KappaList5", "KappaList6" })]
        public int Kappa
        {
            get => kappa;
            set
            {
                if (value != kappa)
                {
                    kappa = value;
                    OnPropertyChanged("Kappa");
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
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
