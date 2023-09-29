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

using System.ComponentModel;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

namespace VisualDecoder_ocr
{
    public class VisualDecoderSettings : ISettings
    {
        public enum DimCodeType { AUTO, EAN13, EAN8, Code39, Code128, QRCode, DataMatrix, 
                                  PDF417, MaxiCode, Aztec, CodaBar, RSS, UPC_A, UPC_E};
        public enum DecoderTypes {Dimcode, OCR};
        public enum OCRLanguages { eng, de, unkown };

        private DimCodeType decodingType;
        private DecoderTypes decoderType;

        private bool stopOnSuccess;

        private bool numericMode;
        private OCRLanguages ocrLanguage;

        #region TaskPane Settings

        [TaskPane("DecoderCaption", "DecoderTooltip", null, 1, true, ControlType.ComboBox, new[] {"Visual Code/Barcode" , "Optical Character Recocnition" })]
        public DecoderTypes DecoderType
        {
            get { return decoderType; }
            set
            {
                if (value != decoderType)
                {
                    decoderType = value;
                    OnPropertyChanged("DecodingType");
                }
            }
        }

        [TaskPane("CodeTypeCaption", "CodeTypeTooltip", null, 1, true, ControlType.ComboBox, new[] { "AUTO", "EAN13", "EAN8", "Code39", "Code128", "QRCode", 
                                                                                                     "DataMatrix", "PDF417", "MaxiCode",  "Aztec", "CodaBar",
                                                                                                     "RSS", "UPC_A", "UPC_E" })]
        public DimCodeType CodeType
        {
            get { return decodingType; }
            set
            {
                if (value != decodingType)
                {
                    decodingType = value;
                    OnPropertyChanged("DecodingType");
                }
            }
        }

        [TaskPane("StopOnSuccessCaption", "StopOnSuccessCaptionTooltip", null, 2, false, ControlType.CheckBox)]
        public bool StopOnSuccess
        {
            get
            {
                return stopOnSuccess;
            }
            set
            {
                if (stopOnSuccess != value)
                {
                    stopOnSuccess = value;
                    OnPropertyChanged("StopOnSuccess");
                }
            }
        }

        [TaskPane("NumericModeCaption", "NumericModeCaptionTooltip", null, 2, false, ControlType.CheckBox)]
        public bool NumericMode
        {
            get
            {
                return numericMode;
            }
            set
            {
                if (numericMode != value)
                {
                    numericMode = value;
                    OnPropertyChanged("NumericMode");
                }
            }
        }

        [TaskPane("LanguageCaption", "LanguageTooltip", null, 1, false, ControlType.ComboBox, new[] { "english", "german", "unknown" })]
        public OCRLanguages OCRLanguage
        {
            get { return ocrLanguage; }
            set
            {
                if (value != ocrLanguage)
                {
                    ocrLanguage = value;
                    OnPropertyChanged("OCRLanguage");
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
    }
}
