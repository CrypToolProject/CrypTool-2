using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System.ComponentModel;

namespace CrypTool.Plugins.GrainV0.Attack
{
    public class GrainV0AttackSettings : ISettings
    {
        #region Private Variables
        //variable for NFSR source (false->external, true-> C# random number generator)
        private readonly bool generator = false;

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

