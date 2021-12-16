/*                              
   Copyright 2020 Team CrypTool (Nils Kopal, Sven Rech)

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
using System;
using System.ComponentModel;
using System.Text;
using System.Windows;

namespace CrypTool.Plugins.CostFunction
{
    public class CostFunctionSettings : ISettings
    {
        public enum CostFunctionType
        {
            IOC = 0,
            Entropy = 1,
            NGramsLog2 = 2,
            RegEx = 3
        };

        #region private variables

        private CostFunctionType functionType = CostFunctionType.Entropy;

        //for all
        private string bytesToUse = "256";
        private int bytesToUseInteger = 256;
        private string bytesOffset = "0";
        private int bytesOffsetInteger;

        //for IOC and entropy
        private int blocksize = 1;

        //for ngrams
        private int language;
        private int ngramSize;
        private bool useSpaces = false;

        //for regex
        private string regExText;
        private bool caseInsensitive;
        #endregion

        public void Initialize()
        {
            UpdateTaskPaneVisibility();
        }

        [TaskPane("FunctionTypeCaption", "FunctionTypeTooltip", null, 1, false, ControlType.ComboBox, new string[] { "IOC", "Entropy", "NGramsLog2", "RegEx" })]
        public CostFunctionType FunctionType
        {
            get => functionType;
            set
            {
                functionType = value;
                UpdateTaskPaneVisibility();
                OnPropertyChanged("FunctionType");
            }
        }

        [TaskPane("BlocksizeCaption", "BlocksizeTooltip", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 10)]
        public int BlockSize
        {
            get => blocksize;
            set
            {
                blocksize = value;
                OnPropertyChanged("BlockSize");
            }
        }

        [TaskPane("BytesToUseCaption", "BytesToUseTooltip", null, 3, false, ControlType.TextBox)]
        public string BytesToUse
        {
            get => bytesToUse;
            set
            {
                int old = bytesToUseInteger;
                if (!int.TryParse(value, out bytesToUseInteger))
                {
                    bytesToUseInteger = old;
                }
                else
                {
                    bytesToUse = value;
                }

                OnPropertyChanged("BytesToUse");
            }
        }

        public int BytesToUseInteger => bytesToUseInteger;

        [TaskPane("BytesOffsetCaption", "BytesOffsetTooltip", null, 4, false, ControlType.TextBox)]
        public string BytesOffset
        {
            get => bytesOffset;
            set
            {
                int old = bytesOffsetInteger;
                if (!int.TryParse(value, out bytesOffsetInteger))
                {
                    bytesOffsetInteger = old;
                }
                else
                {
                    bytesOffset = value;
                }

                OnPropertyChanged("BytesOffset");
            }
        }

        [TaskPane("LanguageCaption", "LanguageTooltip", null, 5, false, ControlType.LanguageSelector)]
        public int Language
        {
            get => language;
            set
            {
                if (value != language)
                {
                    language = value;
                    OnPropertyChanged("Language");
                }
            }
        }

        [TaskPane("NGramSizeCaption", "NGramSizeTooltip", null, 6, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 5)]
        public int NGramSize
        {
            get => ngramSize;
            set
            {
                if (value != ngramSize)
                {
                    ngramSize = value;
                    OnPropertyChanged("NGramSize");
                }
            }
        }

        [TaskPane("UseSpacesCaption", "UseSpacesTooltip", null, 7, false, ControlType.CheckBox)]
        public bool UseSpaces
        {
            get => useSpaces;
            set
            {
                if (value != useSpaces)
                {
                    useSpaces = value;
                    OnPropertyChanged("UseSpaces");
                }
            }
        }

        public int entropyselect;
        [TaskPane("entropyCaption", "entropyTooltip", null, 6, false, ControlType.ComboBox, new string[] { "entropyList1", "entropyList2" })]
        public int EntropySelection
        {
            get => entropyselect;

            set
            {
                entropyselect = value;
                OnPropertyChanged("EntropySelection");
            }
        }


        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        internal void UpdateTaskPaneVisibility()
        {
            if (TaskPaneAttributeChanged == null)
            {
                return;
            }

            if (functionType == CostFunctionType.RegEx)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("RegEx", Visibility.Visible)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("RegExHex", Visibility.Visible)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("CaseInsensitive", Visibility.Visible)));
            }
            else
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("RegEx", Visibility.Collapsed)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("RegExHex", Visibility.Collapsed)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("CaseInsensitive", Visibility.Collapsed)));
            }

            if (functionType == CostFunctionType.NGramsLog2)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Language", Visibility.Visible)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NGramSize", Visibility.Visible)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("UseSpaces", Visibility.Visible)));

            }
            else
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Language", Visibility.Collapsed)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NGramSize", Visibility.Collapsed)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("UseSpaces", Visibility.Collapsed)));
            }

            if (functionType == CostFunctionType.Entropy)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("EntropySelection", Visibility.Visible)));
            }
            else
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("EntropySelection", Visibility.Collapsed)));
            }

            if (functionType == CostFunctionType.Entropy || functionType == CostFunctionType.IOC)
            {
                //only entropy and ioc support a block size
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BlockSize", Visibility.Visible)));
            }
            else
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BlockSize", Visibility.Collapsed)));
            }
        }

        [TaskPane("RegExCaption", "RegExTooltip", null, 5, false, ControlType.TextBox)]
        public string RegEx
        {
            get => regExText;
            set
            {
                regExText = value;
                OnPropertyChanged("RegEx");

                regExHex = convertTextToHexString(regExText);
                OnPropertyChanged("RegExHex");
            }
        }

        private static string convertTextToHexString(string text)
        {
            if (text == null)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder();
            foreach (byte b in Encoding.ASCII.GetBytes(text))
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }

        [TaskPane("CaseInsensitivCaption", "CaseInsensitivTooltip", null, 6, false, ControlType.CheckBox)]
        public bool CaseInsensitive
        {
            get => caseInsensitive;
            set
            {
                if (value != caseInsensitive)
                {
                    caseInsensitive = value;
                    OnPropertyChanged("CaseInsensitive");
                }
            }
        }

        private string regExHex;
        [TaskPane("RegExHexCaption", "RegExHexTooltip", null, 7, false, ControlType.TextBox)]
        public string RegExHex
        {
            get => regExHex;
            set
            {
                regExHex = value;
                OnPropertyChanged("RegExHex");

                regExText = convertHexStringToText(regExHex);
                OnPropertyChanged("RegEx");
            }
        }

        private static string convertHexStringToText(string hexString)
        {
            if (hexString == null)
            {
                return null;
            }

            StringBuilder cleanHexString = new StringBuilder();

            //cleanup the input
            foreach (char c in hexString)
            {
                if (Uri.IsHexDigit(c))
                {
                    cleanHexString.Append(c);
                }
            }

            int numberChars = cleanHexString.Length % 2 == 0 ? cleanHexString.Length : cleanHexString.Length - 1;

            byte[] bytes = new byte[numberChars / 2];

            for (int i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(cleanHexString.ToString().Substring(i, 2), 16);
            }
            return Encoding.ASCII.GetString(bytes);
        }

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

        #endregion

        #region testing
        public void changeFunctionType(CostFunctionType type)
        {
            functionType = type;
            UpdateTaskPaneVisibility();
            OnPropertyChanged("FunctionType");
        }
        #endregion
    }


}
