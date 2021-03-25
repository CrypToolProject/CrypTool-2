/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System;
using System.ComponentModel;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

namespace SigabaPhaseII
{
    // HOWTO: rename class (click name, press F2)
    public class SigabaPhaseIISettings : ISettings
    {
        #region Private Variables

        private int _plainText = 0;
        private int _unknownSymbolHandling = 0;
        private int _caseHandling = 0;

        private string _alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private bool _all = true;

        #endregion

        public void Initialize()
        {

        }

        #region TaskPane Settings

        [TaskPane("Amount of plain text", "The amount of plain text that is uesed by the component", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int PlainText
        {
            get
            {
                return _plainText;
            }
            set
            {
                if (_plainText != value)
                {
                    _plainText = value;
                    OnPropertyChanged("PlainText");
                }
            }
        }

        [TaskPane("CipherRotor1ReverseCaption", "CipherRotor2ReverseTooltip", null, 2, false, ControlType.CheckBox, "Full length")]
        public Boolean AllPlainText
        {
            get { return _all; }
            set
            {
                if (((Boolean)value) != _all)
                {
                    _all = value;
                    OnPropertyChanged("AllPlainText");
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

        #region Public properties

        public string Alphabet
        {
            get { return _alphabet; }
            set { _alphabet = value; }
        }

        #endregion
    }
}
