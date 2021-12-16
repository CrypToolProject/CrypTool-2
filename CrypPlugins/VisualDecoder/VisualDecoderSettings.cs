/*
    Copyright 2013 Christopher Konze, University of Kassel
 
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

namespace CrypTool.Plugins.VisualDecoder
{
    public class VisualDecoderSettings : ISettings
    {
        public enum DimCodeType { AUTO, EAN13, EAN8, Code39, Code128, QRCode, DataMatrix, PDF417, MaxiCode, Aztec, CodaBar, RSS, UPC_A, UPC_E };
        private DimCodeType decodingType;
        private bool stopOnSuccess;

        #region TaskPane Settings

        [TaskPane("DecodingTypeCaption", "DecodingTypeTooltip", null, 1, true, ControlType.ComboBox, new[] { "AUTO", "EAN13", "EAN8", "Code39", "Code128", "QRCode",
                                                                                                     "DataMatrix", "PDF417", "MaxiCode",  "Aztec", "CodaBar",
                                                                                                     "RSS", "UPC_A", "UPC_E" })]
        public DimCodeType DecodingType
        {
            get => decodingType;
            set
            {
                if (value != decodingType)
                {
                    decodingType = value;
                    OnPropertyChanged("DecodingType");
                }
            }
        }

        [TaskPane("StopOnSuccessCaption", "StopOnSuccessTooltip", null, 2, false, ControlType.CheckBox)]
        public bool StopOnSuccess
        {
            get => stopOnSuccess;
            set
            {
                if (stopOnSuccess != value)
                {
                    stopOnSuccess = value;
                    OnPropertyChanged("StopOnSuccess");
                }
            }
        }


        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}
