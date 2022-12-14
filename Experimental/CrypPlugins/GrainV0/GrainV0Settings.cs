using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System.ComponentModel;

namespace CrypTool.Plugins.GrainV0.Chipher
{
    public class GrainV0Settings : ISettings
    {
        #region Private Variables
        //variable for NFSR source (false->external, true-> C# random number generator)

        #endregion

        #region TaskPane Settings
        //property for CheckBox


        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion

        public void Initialize()
        {

        }
    }
}

