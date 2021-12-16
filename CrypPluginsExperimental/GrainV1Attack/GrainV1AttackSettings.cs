using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System.ComponentModel;

namespace CrypTool.Plugins.GrainV1.Attack
{
    public class GrainV1AttackSettings : ISettings
    {
        #region Private Variables
        //variable for NFSR source (false->external, true-> C# random number generator)
        private bool generator = false;

        #endregion

        #region TaskPane Settings
        //property for CheckBox
        [TaskPane("Built-in random generator", "Check this to use built-in random generator to fill NFSR", null, 1, false, PluginBase.ControlType.CheckBox)]
        public bool UseGenerator
        {
            get => generator;
            set
            {
                if (generator != value)
                {
                    generator = value;
                    OnPropertyChanged("UseGenerator");
                }
            }
        }

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
