using System;
using System.ComponentModel;

using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

namespace CrypTool.Plugins.EncryptedVM
{
    public class EncryptedVMMachineSettings : ISettings
    {
        #region Private Variables

        private int cycles = 1;

        #endregion

        #region TaskPane Settings

        [TaskPane("EncryptedVM_Machine_Cycles_Name", "EncryptedVM_Machine_Cycles_Tooltip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, Int32.MaxValue)]
        public int Cycles
        {
            get
            {
                return cycles;
            }
            set
            {
                if (cycles != value)
                {
                    cycles = value;
                    OnPropertyChanged("Cycles");
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
