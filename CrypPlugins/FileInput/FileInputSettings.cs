/*
   Copyright 2012 Julian Weyes, University Duisburg-Essen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using CrypTool.PluginBase;
using System.ComponentModel;
using System.Windows;

namespace FileInput
{
    public class FileInputSettings : ISettings
    {
        private string openFilename;
        private string saveAndRestoreState;

        public string SaveAndRestoreState
        {
            get => saveAndRestoreState;
            set
            {
                saveAndRestoreState = value;
                OnPropertyChanged("SaveAndRestoreState");
            }
        }

        [TaskPane("OpenFilenameCaption", "OpenFilenameTooltip", null, 1, false, ControlType.OpenFileDialog, FileExtension = "All Files (*.*)|*.*")]
        public string OpenFilename
        {
            get => openFilename;
            set
            {
                if (value != openFilename)
                {
                    openFilename = value;
                    OnPropertyChanged("OpenFilename");
                }
            }
        }

        #region ISettings Members

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        #endregion

        [TaskPane("CloseFileCaption", "CloseFileTooltip", null, 2, false, ControlType.Button)]
        public void CloseFile()
        {
            OpenFilename = null;
            OnPropertyChanged("CloseFile");
        }

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public void SettingChanged(string setting, Visibility vis)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(setting, vis)));
            }
        }
    }
}