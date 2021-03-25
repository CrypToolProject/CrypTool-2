/*                              
   Copyright 2009 Team CrypTool (Sven Rech,Dennis Nolte,Raoul Falk,Nils Kopal), Uni Duisburg-Essen

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
using System.Windows;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

namespace CrypTool.Plugins.Converter
{
    public class ConverterSettings : ISettings
    {
        #region private variables

        public enum EncodingTypes { UTF8, UTF7, UTF16, UTF32, ASCII, ISO8859_15, Windows1252 };
        public enum PresentationFormat { Text, Hex, Base64, Decimal }

        private OutputTypes converter = OutputTypes.StringType;

        private bool numeric = false;
        private string format = "";
        private bool formatAmer = false;
        private bool reverseOrder = false;
        private bool BigEndian = false;

        private int digitsdefinition = 0;   // 0 = get value of input digit from the digits string, 1 = from its byte value (minus digitsoffset)
        private int digitsbase = 256;
        private int digitsoffset = 0;
        private string digits = "0123456789ABCDEF";
        private int digitsgroup = 8;
        private int digitsEndianness = 0; // 0==LittleEndian, 1=BigEndian

        private EncodingTypes inputencoding = EncodingTypes.UTF8;
        private EncodingTypes outputencoding = EncodingTypes.UTF8;
        private PresentationFormat presentation = PresentationFormat.Text;

        public PresentationFormat Presentation
        {
            get { return this.presentation; }
            set
            {
                if (this.presentation != value)
                {
                    this.presentation = value;
                    OnPropertyChanged("Presentation");
                }
            }
        }
        #endregion

        #region taskpane

        [TaskPane("ConverterCaption", "ConverterTooltip", null, 1, true, ControlType.ComboBox, new string[] { "TypesList1", "TypesList2", "TypesList3", "TypesList4", "TypesList5", "TypesList6", "TypesList7", "TypesList8", "TypesList9", "TypesList10" })]
        public OutputTypes Converter
        {
            get { return this.converter; }
            set
            {
                if (value != this.converter)
                {
                    this.converter = value;
                    UpdateTaskPaneVisibility();
                    OnPropertyChanged("Converter");

                    UpdateIcon();
                }
            }
        }

        [TaskPane("NumericCaption", "NumericTooltip", null, 2, true, ControlType.CheckBox)]
        public bool Numeric
        {
            get { return this.numeric; }
            set
            {
                if (value != this.numeric)
                {
                    this.numeric = value;
                    OnPropertyChanged("Numeric");
                }
            }
        }

        [TaskPane("FormatCaption", "FormatTooltip", null, 3, true, ControlType.TextBox)]
        public string Format
        {
            get { return this.format; }
            set
            {
                if (value != this.format)
                {
                    this.format = value;
                    OnPropertyChanged("Format");
                }
            }
        }

        [TaskPane("ReverseOrderCaption", "ReverseOrderTooltip", null, 3, true, ControlType.CheckBox, null)]
        public bool ReverseOrder
        {
            get { return this.reverseOrder; }
            set
            {
                if (value != this.reverseOrder)
                {
                    this.reverseOrder = value;
                    OnPropertyChanged("ReverseOrder");
                }
            }
        }

        [TaskPane("EndiannessCaption", "EndiannessTooltip", null, 4, true, ControlType.ComboBox, new string[] { "EndiannessList1", "EndiannessList2" })]
        public bool Endianness
        {
            get { return this.BigEndian; }
            set
            {
                if (value != this.BigEndian)
                {
                    this.BigEndian = value;
                    OnPropertyChanged("Endianness");
                }
            }
        }

        [ContextMenu("InputEncodingSettingCaption", "InputEncodingSettingTooltip", 5, ContextMenuControlType.ComboBox, null, new string[] { "EncodingSettingList1", "EncodingSettingList2", "EncodingSettingList3", "EncodingSettingList4", "EncodingSettingList5", "EncodingSettingList6", "EncodingSettingList7" })]
        [TaskPane("InputEncodingSettingCaption", "InputEncodingSettingTooltip", null, 5, true, ControlType.ComboBox, new string[] { "EncodingSettingList1", "EncodingSettingList2", "EncodingSettingList3", "EncodingSettingList4", "EncodingSettingList5", "EncodingSettingList6", "EncodingSettingList7" })]
        public EncodingTypes InputEncoding
        {
            get
            {
                return this.inputencoding;
            }
            set
            {
                if (this.inputencoding != value)
                {
                    this.inputencoding = value;
                    OnPropertyChanged("InputEncoding");
                }
            }
        }

        [ContextMenu("OutputEncodingSettingCaption", "OutputEncodingSettingTooltip", 6, ContextMenuControlType.ComboBox, null, new string[] { "EncodingSettingList1", "EncodingSettingList2", "EncodingSettingList3", "EncodingSettingList4", "EncodingSettingList5", "EncodingSettingList6", "EncodingSettingList7" })]
        [TaskPane("OutputEncodingSettingCaption", "OutputEncodingSettingTooltip", null, 6, true, ControlType.ComboBox, new string[] { "EncodingSettingList1", "EncodingSettingList2", "EncodingSettingList3", "EncodingSettingList4", "EncodingSettingList5", "EncodingSettingList6", "EncodingSettingList7" })]
        public EncodingTypes OutputEncoding
        {
            get
            {
                return this.outputencoding;
            }
            set
            {
                if (this.outputencoding != value)
                {
                    this.outputencoding = value;
                    OnPropertyChanged("OutputEncoding");
                }
            }
        }

        [TaskPane("FormatAmerCaption", "FormatAmerTooltip", null, 7, true, ControlType.ComboBox, new string[] { "FormatAmerList1", "FormatAmerList2" })]
        public bool FormatAmer
        {
            get { return this.formatAmer; }
            set
            {
                if (value != this.formatAmer)
                {
                    this.formatAmer = value;
                    OnPropertyChanged("FormatAmer");
                }
            }
        }

        [TaskPane("PresentationFormatSettingCaption", "PresentationFormatSettingTooltip", null, 8, true, ControlType.ComboBox, new string[] { "PresentationFormatSettingList1", "PresentationFormatSettingList2", "PresentationFormatSettingList3" })]
        public int PresentationFormatSetting
        {
            get
            {
                return (int)this.presentation;
            }
            set
            {
                if (this.presentation != (PresentationFormat)value)
                {
                    this.presentation = (PresentationFormat)value;
                    OnPropertyChanged("PresentationFormatSetting");   
                }
            }
        }

        [TaskPane("DigitsDefinitionCaption", "DigitsDefinitionTooltip", "DigitsGroup", 9, true, ControlType.ComboBox, new string[] { "DigitsDefinitionList1", "DigitsDefinitionList2" })]
        public int DigitsDefinition
        {
            get { return this.digitsdefinition; }
            set
            {
                if (value != this.digitsdefinition)
                {
                    this.digitsdefinition = value;
                    UpdateTaskPaneVisibility();
                    OnPropertyChanged("DigitsDefinition");
                }
            }
        }

        [TaskPane("DigitsOffsetCaption", "DigitsOffsetTooltip", "DigitsGroup", 10, true, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 255)]
        public int DigitsOffset
        {
            get { return this.digitsoffset; }
            set
            {
                if (value != this.digitsoffset)
                {
                    this.digitsoffset = value;
                    OnPropertyChanged("DigitsOffset");
                }
            }
        }

        [TaskPane("DigitsBaseCaption", "DigitsBaseTooltip", "DigitsGroup", 11, true, ControlType.NumericUpDown, ValidationType.RangeInteger, 2, 256)]
        public int DigitsBase
        {
            get { return this.digitsbase; }
            set
            {
                if (value != this.digitsbase)
                {
                    this.digitsbase = value;
                    OnPropertyChanged("DigitsBase");
                }
            }
        }

        [TaskPane("DigitsCaption", "DigitsTooltip", "DigitsGroup", 12, true, ControlType.TextBox)]
        public string Digits
        {
            get { return this.digits; }
            set
            {
                if (value != this.digits)
                {
                    this.digits = value;
                    OnPropertyChanged("Digits");
                }
            }
        }

        [TaskPane("DigitsGroupCaption", "DigitsGroupTooltip", "DigitsGroup", 13, true, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, int.MaxValue)]
        public int DigitsGroup
        {
            get { return this.digitsgroup; }
            set
            {
                if (value != this.digitsgroup)
                {
                    this.digitsgroup = value;
                    OnPropertyChanged("DigitsGroup");
                }
            }
        }

        [TaskPane("DigitsEndiannessCaption", "DigitsEndiannessTooltip", "DigitsGroup", 14, true, ControlType.ComboBox, new string[] { "EndiannessList1", "EndiannessList2" })]
        public int DigitsEndianness
        {
            get { return this.digitsEndianness; }
            set
            {
                if (value != this.digitsEndianness)
                {
                    this.digitsEndianness = value;
                    OnPropertyChanged("DigitsEndianness");
                }
            }
        }

        private void settingChanged(string setting, Visibility vis)
        {
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(setting, vis)));
        }

        internal void UpdateIcon()
        {
            ChangePluginIcon((int)converter + 1);
        }

        internal void UpdateTaskPaneVisibility()
        {
            if (TaskPaneAttributeChanged == null)
                return;

            switch (Converter)
            {
                case OutputTypes.StringType:
                    {
                        settingChanged("Numeric", Visibility.Visible);
                        settingChanged("Format", Visibility.Visible);
                        settingChanged("InputEncoding", Visibility.Visible);
                        settingChanged("OutputEncoding", Visibility.Collapsed);
                        settingChanged("PresentationFormatSetting", Visibility.Collapsed);
                        settingChanged("FormatAmer", Visibility.Collapsed);
                        settingChanged("ReverseOrder", Visibility.Collapsed);
                        settingChanged("Endianness", Visibility.Collapsed);
                        settingChanged("DigitsByteValue", Visibility.Visible);
                        settingChanged("DigitsGroup", Visibility.Visible);
                        settingChanged("DigitsBigEndian", Visibility.Visible);
                        if (DigitsDefinition == 0)
                        {
                            settingChanged("Digits", Visibility.Visible);
                            settingChanged("DigitsBase", Visibility.Collapsed);
                            settingChanged("DigitsOffset", Visibility.Collapsed);
                        }
                        else
                        {
                            settingChanged("Digits", Visibility.Collapsed);
                            settingChanged("DigitsBase", Visibility.Visible);
                            settingChanged("DigitsOffset", Visibility.Visible);
                        }
                        break;
                    }
                case OutputTypes.IntType:
                    {
                        settingChanged("Numeric", Visibility.Collapsed);
                        settingChanged("Format", Visibility.Collapsed);
                        settingChanged("InputEncoding", Visibility.Visible);
                        settingChanged("OutputEncoding", Visibility.Collapsed);
                        settingChanged("PresentationFormatSetting", Visibility.Collapsed);
                        settingChanged("FormatAmer", Visibility.Collapsed);
                        settingChanged("ReverseOrder", Visibility.Collapsed);
                        settingChanged("Endianness", Visibility.Visible);
                        settingChanged("DigitsByteValue", Visibility.Collapsed);
                        settingChanged("Digits", Visibility.Collapsed);
                        settingChanged("DigitsGroup", Visibility.Collapsed);
                        settingChanged("DigitsBase", Visibility.Collapsed);
                        settingChanged("DigitsOffset", Visibility.Collapsed);
                        settingChanged("DigitsBigEndian", Visibility.Collapsed);
                        break;
                    }
                case OutputTypes.ShortType:
                    {
                        settingChanged("Numeric", Visibility.Collapsed);
                        settingChanged("Format", Visibility.Collapsed);
                        settingChanged("InputEncoding", Visibility.Visible);
                        settingChanged("OutputEncoding", Visibility.Collapsed);
                        settingChanged("PresentationFormatSetting", Visibility.Collapsed);
                        settingChanged("FormatAmer", Visibility.Collapsed);
                        settingChanged("ReverseOrder", Visibility.Collapsed);
                        settingChanged("Endianness", Visibility.Visible);
                        settingChanged("DigitsByteValue", Visibility.Collapsed);
                        settingChanged("Digits", Visibility.Collapsed);
                        settingChanged("DigitsGroup", Visibility.Collapsed);
                        settingChanged("DigitsBase", Visibility.Collapsed);
                        settingChanged("DigitsOffset", Visibility.Collapsed);
                        settingChanged("DigitsBigEndian", Visibility.Collapsed);
                        break;
                    }
                case OutputTypes.ByteType:
                    {
                        settingChanged("Numeric", Visibility.Collapsed);
                        settingChanged("Format", Visibility.Collapsed);
                        settingChanged("InputEncoding", Visibility.Visible);
                        settingChanged("OutputEncoding", Visibility.Collapsed);
                        settingChanged("PresentationFormatSetting", Visibility.Collapsed);
                        settingChanged("FormatAmer", Visibility.Collapsed);
                        settingChanged("ReverseOrder", Visibility.Collapsed);
                        settingChanged("Endianness", Visibility.Collapsed);
                        settingChanged("DigitsByteValue", Visibility.Collapsed);
                        settingChanged("Digits", Visibility.Collapsed);
                        settingChanged("DigitsGroup", Visibility.Collapsed);
                        settingChanged("DigitsBase", Visibility.Collapsed);
                        settingChanged("DigitsOffset", Visibility.Collapsed);
                        settingChanged("DigitsBigEndian", Visibility.Collapsed);
                        break;
                    }
                case OutputTypes.DoubleType:
                    {
                        settingChanged("Numeric", Visibility.Collapsed);
                        settingChanged("Format", Visibility.Collapsed);
                        settingChanged("InputEncoding", Visibility.Visible);
                        settingChanged("OutputEncoding", Visibility.Collapsed);
                        settingChanged("PresentationFormatSetting", Visibility.Collapsed);
                        settingChanged("FormatAmer", Visibility.Visible);
                        settingChanged("ReverseOrder", Visibility.Collapsed);
                        settingChanged("Endianness", Visibility.Collapsed);
                        settingChanged("DigitsByteValue", Visibility.Collapsed);
                        settingChanged("Digits", Visibility.Collapsed);
                        settingChanged("DigitsGroup", Visibility.Collapsed);
                        settingChanged("DigitsBase", Visibility.Collapsed);
                        settingChanged("DigitsOffset", Visibility.Collapsed);
                        settingChanged("DigitsBigEndian", Visibility.Collapsed);
                        break;
                    }
                case OutputTypes.BigIntegerType:
                    {
                        settingChanged("Numeric", Visibility.Collapsed);
                        settingChanged("Format", Visibility.Collapsed);
                        settingChanged("InputEncoding", Visibility.Visible);
                        settingChanged("OutputEncoding", Visibility.Collapsed);
                        settingChanged("PresentationFormatSetting", Visibility.Collapsed);
                        settingChanged("FormatAmer", Visibility.Collapsed);
                        settingChanged("ReverseOrder", Visibility.Collapsed);
                        settingChanged("Endianness", Visibility.Visible);
                        settingChanged("DigitsByteValue", Visibility.Collapsed);
                        settingChanged("Digits", Visibility.Collapsed);
                        settingChanged("DigitsGroup", Visibility.Collapsed);
                        settingChanged("DigitsBase", Visibility.Collapsed);
                        settingChanged("DigitsOffset", Visibility.Collapsed);
                        settingChanged("DigitsBigEndian", Visibility.Collapsed);
                        break;
                    }
                case OutputTypes.UIntArrayType:
                    {
                        settingChanged("Numeric", Visibility.Collapsed);
                        settingChanged("Format", Visibility.Collapsed);
                        settingChanged("InputEncoding", Visibility.Visible);
                        settingChanged("OutputEncoding", Visibility.Collapsed);
                        settingChanged("PresentationFormatSetting", Visibility.Collapsed);
                        settingChanged("FormatAmer", Visibility.Collapsed);
                        settingChanged("ReverseOrder", Visibility.Collapsed);
                        settingChanged("Endianness", Visibility.Collapsed);
                        settingChanged("DigitsByteValue", Visibility.Visible);
                        settingChanged("DigitsGroup", Visibility.Visible);
                        settingChanged("DigitsBigEndian", Visibility.Visible);
                        if (DigitsDefinition == 0)
                        {
                            settingChanged("Digits", Visibility.Visible);
                            settingChanged("DigitsBase", Visibility.Collapsed);
                            settingChanged("DigitsOffset", Visibility.Collapsed);
                        }
                        else
                        {
                            settingChanged("Digits", Visibility.Collapsed);
                            settingChanged("DigitsBase", Visibility.Visible);
                            settingChanged("DigitsOffset", Visibility.Visible);
                        }
                        break;
                    }
                case OutputTypes.ByteArrayType:
                    {
                        settingChanged("Numeric", Visibility.Visible);
                        settingChanged("Format", Visibility.Collapsed);
                        settingChanged("InputEncoding", Visibility.Visible);
                        settingChanged("OutputEncoding", Visibility.Visible);
                        settingChanged("PresentationFormatSetting", Visibility.Collapsed);
                        settingChanged("FormatAmer", Visibility.Collapsed);
                        settingChanged("ReverseOrder", Visibility.Visible);
                        settingChanged("Endianness", Visibility.Collapsed);
                        settingChanged("DigitsByteValue", Visibility.Visible);
                        settingChanged("DigitsGroup", Visibility.Visible);
                        settingChanged("DigitsBigEndian", Visibility.Visible);
                        if (DigitsDefinition == 0)
                        {
                            settingChanged("Digits", Visibility.Visible);
                            settingChanged("DigitsBase", Visibility.Collapsed);
                            settingChanged("DigitsOffset", Visibility.Collapsed);
                        }
                        else
                        {
                            settingChanged("Digits", Visibility.Collapsed);
                            settingChanged("DigitsBase", Visibility.Visible);
                            settingChanged("DigitsOffset", Visibility.Visible);
                        }
                        break;
                    }
                case OutputTypes.CrypToolStreamType:
                    {
                        settingChanged("Numeric", Visibility.Collapsed);
                        settingChanged("Format", Visibility.Collapsed);
                        settingChanged("InputEncoding", Visibility.Visible);
                        settingChanged("OutputEncoding", Visibility.Visible);
                        settingChanged("PresentationFormatSetting", Visibility.Collapsed);
                        settingChanged("FormatAmer", Visibility.Collapsed);
                        settingChanged("ReverseOrder", Visibility.Visible);
                        settingChanged("Endianness", Visibility.Collapsed);
                        settingChanged("DigitsByteValue", Visibility.Visible);
                        settingChanged("DigitsGroup", Visibility.Visible);
                        settingChanged("DigitsBigEndian", Visibility.Visible);
                        if (DigitsDefinition == 0)
                        {
                            settingChanged("Digits", Visibility.Visible);
                            settingChanged("DigitsBase", Visibility.Collapsed);
                            settingChanged("DigitsOffset", Visibility.Collapsed);
                        }
                        else
                        {
                            settingChanged("Digits", Visibility.Collapsed);
                            settingChanged("DigitsBase", Visibility.Visible);
                            settingChanged("DigitsOffset", Visibility.Visible);
                        }
                        break;
                    }
            }
        }
     
        #endregion

        #region INotifyPropertyChanged Member

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            UpdateTaskPaneVisibility();
        }

        protected void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;
        private void ChangePluginIcon(int iconIndex)
        {
            if (OnPluginStatusChanged != null) OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, iconIndex));
        }
        #endregion
    }
}
