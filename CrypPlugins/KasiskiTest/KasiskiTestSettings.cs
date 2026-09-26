/*
   Copyright (C) CrypTool 2 Team

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

namespace CrypTool.KasiskiTest
{
    public class KasiskiTestSettings : ISettings
    {
        #region ISettings Members

        #endregion

        public int caseSensitivity = 0;
        public int unknownSymbolHandling = 1;
        public int grammLength = 3;
        public int factorSize = 20;


        [PropertySaveOrder(1)]
        [TaskPane("GrammLengthCaption", "GrammLengthTooltip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 100)]
        public int GrammLength
        {
            get => grammLength;
            set
            {
                if (value != grammLength)
                {
                    grammLength = value;
                }
            }
        }
        [PropertySaveOrder(2)]
        [ContextMenu("RemoveUnknownSymbolsCaption", "RemoveUnknownSymbolsTooltip", 4, ContextMenuControlType.ComboBox, null, new string[] { "RemoveUnknownSymbolsList1", "RemoveUnknownSymbolsList2" })]
        [TaskPane("RemoveUnknownSymbolsCaption", "RemoveUnknownSymbolsTooltip", null, 4, false, ControlType.ComboBox, new string[] { "RemoveUnknownSymbolsList1", "RemoveUnknownSymbolsList2" })]
        public int RemoveUnknownSymbols
        {
            get => unknownSymbolHandling;
            set
            {
                if (value != unknownSymbolHandling)
                {
                    unknownSymbolHandling = value;
                }

                OnPropertyChanged("RemoveUnknownSymbols");
            }
        }
        [PropertySaveOrder(3)]
        [TaskPane("FactorSizeCaption", "FactorSizeTooltip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 2, 100)]
        public int FactorSize
        {
            get => factorSize;
            set
            {
                if (value != factorSize)
                {
                    factorSize = value;
                }
            }
        }


        [PropertySaveOrder(4)]
        [ContextMenu("CaseSensitivityCaption", "CaseSensitivityTooltip", 4, ContextMenuControlType.ComboBox, null, new string[] { "CaseSensitivityList1", "CaseSensitivityList2" })]
        [TaskPane("CaseSensitivityCaption", "CaseSensitivityTooltip", null, 4, false, ControlType.ComboBox, new string[] { "CaseSensitivityList1", "CaseSensitivityList2" })]
        public int CaseSensitivity
        {
            get => caseSensitivity;
            set
            {
                if (value != caseSensitivity)
                {
                    caseSensitivity = value;
                }

                OnPropertyChanged("Case Sensitive");
            }
        }






        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
