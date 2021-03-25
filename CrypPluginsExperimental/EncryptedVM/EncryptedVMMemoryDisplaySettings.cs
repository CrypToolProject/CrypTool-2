using System.ComponentModel;
using System.Windows;

using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

namespace CrypTool.Plugins.EncryptedVM
{
    public class EncryptedVMMemoryDisplaySettings : ISettings
    {
        #region Private Variables

        private bool mode = false; // false = whole memory, true = selection by program
        private int rows = 0;

        #endregion

        #region TaskPane

        [TaskPane("EncryptedVM_Memory_Rows_Name", "EncryptedVM_Memory_Rows_Tooltip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, Memory.ARRAY_ROWS)]
        public int Rows
        {
            get
            {
                return rows;
            }
            set
            {
                if (rows != value)
                {
                    rows = value;
                    OnPropertyChanged("Rows");
                }
            }
        }

        [TaskPane("EncryptedVM_Memory_Mode_Name", "EncryptedVM_Memory_Mode_Tooltip", null, 1, false, ControlType.CheckBox)]
        public bool Mode
        {
            get
            {
                return mode;
            }
            set
            {
                if (mode != value)
                {
                    mode = value;
                    OnPropertyChanged("Mode");

                    UpdateTaskPaneVisibility();
                }
            }
        }

        #endregion

        #region Events

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        private void settingChanged(string setting, Visibility vis)
        {
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(setting, vis)));
        }

        internal void UpdateTaskPaneVisibility()
        {
            if (TaskPaneAttributeChanged == null)
                return;

            if (mode)
                settingChanged("Rows", Visibility.Collapsed);
            else
                settingChanged("Rows", Visibility.Visible);
        }

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
