//////////////////////////////////////////////////////////////////////////////////////////////////
// CrypTool V2
// © 2008 - Gerhard Junker
// Apache License see http://www.apache.org/licenses/
//
// $HeadURL: https://svn.cryptool.org/CrypTool2/trunk/CrypPlugins/PKCS5/PKCS5Settings.cs $
//////////////////////////////////////////////////////////////////////////////////////////////////
// $Revision:: 8983                                                                           $://
// $Author:: kopal                                                                            $://
// $Date:: 2021-03-24 14:51:34 +0100 (Mi, 24 Mrz 2021)                                        $://
//////////////////////////////////////////////////////////////////////////////////////////////////

using CrypTool.PluginBase;
using System.ComponentModel;
using System.Security.Cryptography;

namespace PKCS5
{
    /// <summary>
    /// Settings for PKCS#5 v2
    /// </summary>
    public class PKCS5Settings : ISettings
    {

        #region ISettings Member

        public enum EncodingTypes
        {
            Default = 0,
            Unicode = 1,
            UTF7 = 2,
            UTF8 = 3,
            UTF32 = 4,
            ASCII = 5,
            BigEndianUnicode = 6
        };
        private EncodingTypes encoding = EncodingTypes.UTF8;

        /// <summary>
        /// selected internal hash HMAC function
        /// </summary>
        private PKCS5MaskGenerationMethod.ShaFunction selectedShaFunction
                        = PKCS5MaskGenerationMethod.ShaFunction.SHA256;

        [ContextMenu("SHAFunctionCaption", "SHAFunctionTooltip", 0,
          ContextMenuControlType.ComboBox, null,
          new string[] { "SHAFunctionList1", "SHAFunctionList2", "SHAFunctionList3", "SHAFunctionList4", "SHAFunctionList5", "SHAFunctionList6", "SHAFunctionList7" })]
        [TaskPane("SHAFunctionTPCaption", "SHAFunctionTPTooltip", null, 0, true,
          ControlType.ComboBox,
          new string[] { "SHAFunctionList1", "SHAFunctionList2", "SHAFunctionList3", "SHAFunctionList4", "SHAFunctionList5", "SHAFunctionList6", "SHAFunctionList7" })]
        public int SHAFunction
        {
            get => (int)selectedShaFunction;
            set
            {
                selectedShaFunction = (PKCS5MaskGenerationMethod.ShaFunction)value;
                // set to max hash length
                length = PKCS5MaskGenerationMethod.GetHashLength(selectedShaFunction) * 8;
                OnPropertyChanged("SHAFunction");
                OnPropertyChanged("Length");
            }
        }

        /// <summary>
        /// count of hash loops
        /// </summary>
        private int count = 1000;
        [TaskPane("CountCaption", "CountTooltip", null, 1, false,
          ControlType.TextBox, ValidationType.RangeInteger, 1, 9999)]
        public int Count
        {
            get => count;
            set
            {
                count = value;
                if (count == 0)
                {
                    count = 1000;
                }

                OnPropertyChanged("Count");
            }
        }

        /// <summary>
        /// length of calculated hash in bits
        /// </summary>
        private int length = 256;
        [TaskPane("LengthCaption", "LengthTooltip", null, 2, false,
          ControlType.TextBox, ValidationType.RangeInteger, -64, 2048)]
        public int Length
        {
            get => length;
            set
            {
                length = value;
                if (length < 0) // change from bytes to bits [hack]
                {
                    length *= -8;
                }

                //while ((length & 0x07) != 0) // go to the next multiple of 8
                //  length++;

                OnPropertyChanged("Length");
            }
        }



        /// <summary>
        /// Encoding property used in the Settings pane. 
        /// </summary>
        [ContextMenu("EncodingSettingCaption", "EncodingSettingTooltip", 1, ContextMenuControlType.ComboBox, null,
      new string[] { "EncodingSettingList1", "EncodingSettingList2", "EncodingSettingList3", "EncodingSettingList4", "EncodingSettingList5", "EncodingSettingList6", "EncodingSettingList7" })]
        [TaskPane("EncodingSettingCaption", "EncodingSettingTooltip",
      null, 1, false, ControlType.RadioButton,
      new string[] { "EncodingSettingList1", "EncodingSettingList2", "EncodingSettingList3", "EncodingSettingList4", "EncodingSettingList5", "EncodingSettingList6", "EncodingSettingList7" })]
        public EncodingTypes Encoding
        {
            get => encoding;
            set
            {
                if (encoding != value)
                {
                    encoding = value;
                    OnPropertyChanged("Encoding");
                }
            }
        }


        /// <summary>
        /// Checks the length.
        /// </summary>
        private void CheckLength()
        {
            // get max length of this hash
            int newlen = PKCS5MaskGenerationMethod.GetHashLength(selectedShaFunction) * 8;
            if (newlen < length)
            {
                length = newlen; // reduce it to max length
            }
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="name">The name.</param>
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
