/*
   Copyright 2021 Nils Kopal <Nils.Kopal<at>CrypTool.org

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

namespace CrypTool.Plugins.VisualCryptography
{
    public enum VisualPattern
    {
        Horizontal,
        Vertical,
        Diagonal,
        HorizontalVertical,
        HorizontalDiagonal,
        VerticalDiagonal,
        HorizontalVerticalDiagonal
    }

    public class VisualCryptographySettings : ISettings
    {
        #region Private Variables

        private int _charactersPerRow = 15;
        private VisualPattern _visualPattern = VisualPattern.Diagonal;
        private int _threshold = 128;

        #endregion

        #region TaskPane Settings

        [TaskPane("CharactersPerRowCaption", "CharactersPerRowTooltip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 100)]
        public int CharactersPerRow
        {
            get => _charactersPerRow;
            set
            {
                if (_charactersPerRow != value)
                {
                    _charactersPerRow = value;
                    OnPropertyChanged("CharactersPerRow");
                }
            }
        }

        [TaskPane("VisualPatternCaption", "VisualPatternTooltip", null, 2, false, ControlType.ComboBox,
            new string[] {
                "Pattern_Horizontal",
                "Pattern_Vertical",
                "Pattern_Diagonal",
                "Pattern_HorizontalVertical",
                "Pattern_HorizontalDiagonal",
                "Pattern_VerticalDiagonal",
                "Pattern_HorizontalVerticalDiagonal"
        })]
        public VisualPattern VisualPattern
        {
            get => _visualPattern;
            set
            {
                if (_visualPattern != value)
                {
                    _visualPattern = value;
                    OnPropertyChanged("VisualPattern");
                }
            }
        }

        [TaskPane("ThresholdCaption", "ThresholdTooltip", null, 3, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 255)]
        public int Threshhold
        {
            get => _threshold;
            set
            {
                if (_threshold != value)
                {
                    _threshold = value;
                    OnPropertyChanged("Threshhold");
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
