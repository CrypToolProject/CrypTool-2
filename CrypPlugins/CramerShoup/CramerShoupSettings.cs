using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System.ComponentModel;

namespace CrypTool.Plugins.CramerShoup
{
    public class CramerShoupSettings : ISettings
    {
        #region Private Variables

        private int keysize = 0;
        private int action = 0;

        #endregion

        public void Initialize()
        {

        }

        #region TaskPane Settings

        /// <summary>
        /// Getter/Setter for the source of the Key Data
        /// </summary>
        [TaskPane("KeySizeCaption", "KeySizeTooltip", null, 1, false, ControlType.ComboBox, new string[] { "128", "256", "512" })]
        public int KeySize
        {
            get => keysize;
            set
            {
                if (value != keysize)
                {
                    keysize = value;

                    OnPropertyChanged("KeySize");
                }
            }
        }
        /// <summary>
        /// Getter/Setter for the source of the Key Data
        /// </summary>
        [TaskPane("ActionCaption", "ActionTooltip", null, 2, false, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
        public int Action
        {
            get => action;
            set
            {
                if (value != action)
                {
                    action = value;

                    OnPropertyChanged("Action");
                }
                ChangePluginIcon(action);
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        private void ChangePluginIcon(int Icon)
        {
            if (OnPluginStatusChanged != null)
            {
                OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, Icon));
            }
        }

        #endregion
    }
}
