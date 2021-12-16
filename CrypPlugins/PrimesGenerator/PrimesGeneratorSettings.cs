using CrypTool.PluginBase;

namespace CrypTool.PrimesGenerator
{
    public class PrimesGeneratorSettings : ISettings
    {
        #region Properties

        private int m_SelectedMode = 0;
        [PropertySaveOrder(1)]
        [ContextMenu("ModeCaption", "ModeTooltip", 1, ContextMenuControlType.ComboBox, null, new string[] { "ModeList1", "ModeList2", "ModeList3", "ModeList4", "ModeList5" })]
        [TaskPane("ModeCaption", "ModeTooltip", null, 1, false, ControlType.ComboBox, new string[] { "ModeList1", "ModeList2", "ModeList3", "ModeList4", "ModeList5" })]
        public int Mode
        {
            get => m_SelectedMode;
            set
            {
                if (value != m_SelectedMode)
                {
                    m_SelectedMode = value;
                    FirePropertyChangedEvent("Mode");
                }
            }
        }

        private string m_Input = "100";
        [PropertySaveOrder(2)]
        [TaskPane("InputCaption", "InputTooltip", null, 2, false, ControlType.TextBox, ValidationType.RegEx, "^[0-9]+$")]
        public string Input
        {
            get => m_Input;
            set
            {
                if (value != m_Input)
                {
                    m_Input = value;
                    FirePropertyChangedEvent("Input");
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        private void FirePropertyChangedEvent(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
