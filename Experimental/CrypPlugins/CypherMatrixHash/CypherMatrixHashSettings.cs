/*
   Copyright 2011 CrypTool 2 Team <ct2contact@CrypTool.org>

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

namespace CrypTool.Plugins.CypherMatrixHash
{
    public class CypherMatrixHashSettings : ISettings
    {
        public enum Permutation { None = 0, B = 1, D = 2 };
        public enum CypherMatrixHashMode { SMX = 0, FMX = 1, Mini = 2 };

        #region Private variables and public constructor

        private Permutation selectedPerm = Permutation.B;
        private CypherMatrixHashMode selectedHash = CypherMatrixHashMode.FMX;
        private int code = 1;   // 1 bis 99, Standardwert: 1, individueller Anwender-Code
        private int basis = 77; // 35 bis 96, Standardwert: 77?, Zahlensystem für Expansionsfunktion
        private int hashBlockLen = 64;  //32-96 Bytes, Standardwert: 64?, Länge der >Hash-Sequenz<
        private bool debug = false;

        public CypherMatrixHashSettings()
        {
        }

        public void Initialize()
        {
        }

        #endregion

        #region TaskPane Settings

        [TaskPane("HashModeCaption", "HashModeTooltip", null, 1, false, ControlType.ComboBox, new string[] { "HashModeSMX", "HashModeFMX", "HashModeMini" })]
        public CypherMatrixHashMode HashMode
        {
            get => selectedHash;
            set
            {
                if (value != selectedHash)
                {

                    selectedHash = value;
                    OnPropertyChanged("HashMode");
                }
            }
        }

        [TaskPane("UserCodeCaption", "UserCodeTooltip", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 99)]
        public int Code
        {
            get => code;
            set
            {
                if (code != value)
                {
                    code = value;
                    OnPropertyChanged("Code");
                }
            }
        }

        [TaskPane("ExpansionBaseCaption", "ExpansionBaseTooltip", null, 3, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 35, 96)]
        public int Basis
        {
            get => basis;
            set
            {
                if (basis != value)
                {
                    basis = value;
                    OnPropertyChanged("Basis");
                }
            }
        }

        [TaskPane("HashBlockSizeCaption", "HashBlockSizeTooltip", null, 4, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 32, 96)]
        public int HashBlockLen
        {
            get => hashBlockLen;
            set
            {
                if (hashBlockLen != value)
                {
                    hashBlockLen = value;
                    OnPropertyChanged("HashBlockLen");
                }
            }
        }

        [TaskPane("PermCaption", "PermTooltip", null, 5, false, ControlType.ComboBox, new string[] { "PermOptionNone", "PermOptionB", "PermOptionD" })]
        public Permutation Perm
        {
            get => selectedPerm;
            set
            {
                if (value != selectedPerm)
                {

                    selectedPerm = value;
                    OnPropertyChanged("Perm");
                }
            }
        }

        [TaskPane("WriteDebugLogCaption", "WriteDebugLogTooltip", null, 6, false, ControlType.CheckBox)]
        public bool Debug
        {
            get => debug;
            set
            {
                if (debug != value)
                {
                    debug = value;
                    OnPropertyChanged("Debug");
                }
            }
        }

        #endregion

        #region private Methods

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}
