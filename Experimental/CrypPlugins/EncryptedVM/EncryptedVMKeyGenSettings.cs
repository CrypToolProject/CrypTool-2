using System;
using System.ComponentModel;

using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

namespace CrypTool.Plugins.EncryptedVM
{
    public class EncryptedVMKeyGenSettings : ISettings
    {
        #region Private Variables

        private int powerOfPolyModulus = 0; // 0 = 1024, 1 = 2048, 2 = 4096, 3 = 8192, 4 = 16384

        #endregion

        #region TaskPane Settings

        [TaskPane("EncryptedVM_Keygen_KeySize_Name", "EncryptedVM_Keygen_KeySize_Tooltip", null, 1, false, ControlType.ComboBox, new string[] { "1024", "2048", "4096", "8192", "16384" })]
        public int PowerOfPolyModulus
        {
            get
            {
                return powerOfPolyModulus;
            }
            set
            {
                if (powerOfPolyModulus != value)
                {
                    powerOfPolyModulus = value;
                    OnPropertyChanged("PowerOfPolyModulus");
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
