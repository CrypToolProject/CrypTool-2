/*
   Copyright 2018 CrypTool 2 Team <ct2contact@CrypTool.org>
   Author: Christian Bender, Universität Siegen

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
using CrypTool.PluginBase.Miscellaneous;
using System.ComponentModel;

namespace CrypTool.Plugins.KKDFSHAKE256
{
    public class KKDFSHAKE256Settings : ISettings
    {
        #region Private Variables

        private bool _saveToFile;
        private bool _displayPres;
        private string _filePath;

        #endregion

        #region TaskPane Settings

        /// <summary>
        /// Specifies, if presentation is displayed or not
        /// </summary>
        [TaskPane("ConfigPresCaption", "ConfigPresTooltip", "ProcessGroup", 0, false, ControlType.CheckBox)]
        public bool DisplayPres
        {
            get => _displayPres;
            set => _displayPres = value;
        }

        /// <summary>
        /// Specifies, if the output of the calculation shall be saved to a file or not
        /// </summary>
        [TaskPane("ConfigPrintKMToFileCaption", "ConfigPrintKMToFileTooltip", "PrintToFileGroup", 0, false, ControlType.CheckBox)]
        public bool SaveToFile
        {
            get => _saveToFile;
            set => _saveToFile = value;
        }

        /// <summary>
        /// If the output shall be saved to a file, the file can be specified in this dialog
        /// </summary>
        [TaskPane("SaveFileDialogCaption", "SaveFileDialogTooltip", "PrintToFileGroup", 1, false, ControlType.SaveFileDialog, FileExtension = "Text Files (*.txt)|*.txt")]
        public string FilePath
        {
            get => _filePath;
            set => _filePath = value;
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
