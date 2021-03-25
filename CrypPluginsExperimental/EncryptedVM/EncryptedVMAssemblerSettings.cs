using System;
using System.ComponentModel;
using System.Windows;

using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

namespace CrypTool.Plugins.EncryptedVM
{
    public class EncryptedVMAssemblerSettings : ISettings
    {
        #region Private Variables

        private bool source = false; // false = input, true = file
        private String sourcePath = "";

        #endregion

        #region TaskPane Settings

        [TaskPane("EncryptedVM_Assembler_SourcePath_Name", "EncryptedVM_Assembler_SourcePath_Tooltip", null, 1, false, ControlType.OpenFileDialog, FileExtension = "All Files (*.*)|*.*")]
        public String SourcePath
        {
            get
            {
                return sourcePath;
            }
            set
            {
                if (sourcePath != value)
                {
                    sourcePath = value;
                    OnPropertyChanged("SourcePath");
                }
            }
        }

        [TaskPane("EncryptedVM_Assembler_Mode_Name", "EncryptedVM_Assembler_Mode_Tooltip", null, 1, false, ControlType.CheckBox)]
        public bool Source
        {
            get
            {
                return source;
            }
            set
            {
                if (source != value)
                {
                    source = value;
                    OnPropertyChanged("Source");

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

            if (source)
                settingChanged("SourcePath", Visibility.Visible);
            else
                settingChanged("SourcePath", Visibility.Collapsed);
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
