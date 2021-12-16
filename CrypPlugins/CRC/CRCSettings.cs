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

using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System.ComponentModel;

namespace CrypTool.CRC
{
    public struct CRCInfo
    {
        public string name;
        public ulong polynomial;                       /* CRC generator polynomial in normal representation: e.g. x^4+x^2+x+1 = 10111 => 0111 = 0x7 */
        public ulong polynomial_reversed;              /* CRC generator polynomial in reversed representation: e.g. x^4+x^2+x+1 = 10111 => 1110 = 0xe */
        public ulong polynomial_reversedreciprocal;    /* CRC generator polynomial in reversed reciprocal representation: e.g. x^4+x^2+x+1 = 10111 => 1011 = 0xb */
        public int width;       /* size of the LFSR */
        public ulong mask;     /* mask corresponding to the width of the CRC method */
        public ulong init;     /* initial value for LFSR */
        public ulong xorout;   /* XOR the CRC result with this value */
        public bool refin;   /* true: input is little-endian, false: input is big-endian */
        public bool refout;  /* true: output is little-endian, false: output is big-endian */
        public ulong check;    /* check is the expected crc value for the input string "123456789" rsp. hex input 31 32 33 34 35 36 37 38 39 */
    }

    public class CRCSettings : ISettings
    {
        #region private variables

        public CRCInfo[] crcspecs = new CRCInfo[] {
            new CRCInfo () { name = "CRC-1/Partiy", width=1, polynomial=0x01, init=0x00, xorout=0x00, refin=false, refout=false, check=0x01 },
            new CRCInfo () { name = "CRC-3/ROHC", width=3, polynomial=0x3, init=0x7, xorout=0x0, refin=true, refout=true, check=0x6 },
            new CRCInfo () { name = "CRC-4/ITU", width=4, polynomial=0x3, init=0x0, xorout=0x0, refin=true, refout=true, check=0x7 },
            new CRCInfo () { name = "CRC-5/EPC", width=5, polynomial=0x09, init=0x09, xorout=0x00, refin=false, refout=false, check=0x00 },
            new CRCInfo () { name = "CRC-5/ITU", width=5, polynomial=0x15, init=0x00, xorout=0x00, refin=true, refout=true, check=0x07 },
            new CRCInfo () { name = "CRC-5/USB", width=5, polynomial=0x05, init=0x1F, xorout=0x1F, refin=true, refout=true, check=0x19 },
            new CRCInfo () { name = "CRC-6/DARC", width=6, polynomial=0x19, init=0x00, xorout=0x00, refin=true, refout=true, check=0x26 },
            new CRCInfo () { name = "CRC-6/ITU", width=6, polynomial=0x03, init=0x00, xorout=0x00, refin=true, refout=true, check=0x06 },
            new CRCInfo () { name = "CRC-7", width=7, polynomial=0x09, init=0x00, xorout=0x00, refin=false, refout=false, check=0x75 },
            new CRCInfo () { name = "CRC-7/ROHC", width=7, polynomial=0x4F, init=0x7F, xorout=0x00, refin=true, refout=true, check=0x53 },
            new CRCInfo () { name = "CRC-8", width=8, polynomial=0x07, init=0x00, xorout=0x00, refin=false, refout=false, check=0xF4 },
            new CRCInfo () { name = "CRC-8/ITU", width=8, polynomial=0x07, init=0x00, xorout=0x55, refin=false, refout=false, check=0xA1 },
            new CRCInfo () { name = "CRC-8/ROHC", width=8, polynomial=0x07, init=0xFF, xorout=0x00, refin=true, refout=true, check=0xD0 },
            new CRCInfo () { name = "CRC-8/DARC", width=8, polynomial=0x39, init=0x00, xorout=0x00, refin=true, refout=true, check=0x15 },
            new CRCInfo () { name = "CRC-8/I-CODE", width=8, polynomial=0x1D, init=0xFD, xorout=0x00, refin=false, refout=false, check=0x7E },
            new CRCInfo () { name = "CRC-8/J1850", width=8, polynomial=0x1D, init=0xFF, xorout=0xFF, refin=false, refout=false, check=0x4B },
            new CRCInfo () { name = "CRC-8/MAXIM", width=8, polynomial=0x31, init=0x00, xorout=0x00, refin=true, refout=true, check=0xA1 },
            new CRCInfo () { name = "CRC-8/WCDMA", width=8, polynomial=0x9B, init=0x00, xorout=0x00, refin=true, refout=true, check=0x25 },
            new CRCInfo () { name = "CRC-8/CCITT", width=8, polynomial=0x8D, init=0x00, xorout=0x00, refin=false, refout=false, check=0xD2 },
            new CRCInfo () { name = "CRC-8/EBU", width=8, polynomial=0x1D, init=0xFF, xorout=0x00, refin=true, refout=true, check=0x97 },
            new CRCInfo () { name = "CRC-10", width=10, polynomial=0x233, init=0x000, xorout=0x000, refin=false, refout=false, check=0x199 },
            new CRCInfo () { name = "CRC-11", width=11, polynomial=0x385, init=0x01A, xorout=0x000, refin=false, refout=false, check=0x5A3 },
            new CRCInfo () { name = "CRC-12/3GPP", width=12, polynomial=0x80F, init=0x000, xorout=0x000, refin=false, refout=true, check=0xDAF },
            new CRCInfo () { name = "CRC-12/DECT", width=12, polynomial=0x80F, init=0x000, xorout=0x000, refin=false, refout=false, check=0xF5B },
            new CRCInfo () { name = "CRC-14/DARC", width=14, polynomial=0x0805, init=0x0000, xorout=0x0000, refin=true, refout=true, check=0x082D },
            new CRCInfo () { name = "CRC-15", width=15, polynomial=0x4599, init=0x0000, xorout=0x0000, refin=false, refout=false, check=0x059E },
            new CRCInfo () { name = "CRC-15/MPT1327", width=15, polynomial=0x6815, init=0x0000, xorout=0x0001, refin=false, refout=false, check=0x2566 },
            new CRCInfo () { name = "CRC-16", width=16, polynomial=0x8005, init=0x0000, xorout=0x0000, refin=true, refout=true, check=0xBB3D },
            new CRCInfo () { name = "CRC-16/AUG-CCITT", width=16, polynomial=0x1021, init=0x1D0F, xorout=0x0000, refin=false, refout=false, check=0xE5CC },
            new CRCInfo () { name = "CRC-16/BUYPASS", width=16, polynomial=0x8005, init=0x0000, xorout=0x0000, refin=false, refout=false, check=0xFEE8 },
            new CRCInfo () { name = "CRC-16/CCITT-FALSE", width=16, polynomial=0x1021, init=0xFFFF, xorout=0x0000, refin=false, refout=false, check=0x29B1 },
            new CRCInfo () { name = "CRC-16/DDS-110", width=16, polynomial=0x8005, init=0x800D, xorout=0x0000, refin=false, refout=false, check=0x9ECF },
            new CRCInfo () { name = "CRC-16/DECT-R", width=16, polynomial=0x0589, init=0x0000, xorout=0x0001, refin=false, refout=false, check=0x007E },
            new CRCInfo () { name = "CRC-16/DECT-X", width=16, polynomial=0x0589, init=0x0000, xorout=0x0000, refin=false, refout=false, check=0x007F },
            new CRCInfo () { name = "CRC-16/DNP", width=16, polynomial=0x3D65, init=0x0000, xorout=0xFFFF, refin=true, refout=true, check=0xEA82 },
            new CRCInfo () { name = "CRC-16/EN-13757", width=16, polynomial=0x3D65, init=0x0000, xorout=0xFFFF, refin=false, refout=false, check=0xC2B7 },
            new CRCInfo () { name = "CRC-16/GENIBUS", width=16, polynomial=0x1021, init=0xFFFF, xorout=0xFFFF, refin=false, refout=false, check=0xD64E },
            new CRCInfo () { name = "CRC-16/MAXIM", width=16, polynomial=0x8005, init=0x0000, xorout=0xFFFF, refin=true, refout=true, check=0x44C2 },
            new CRCInfo () { name = "CRC-16/MCRF4XX", width=16, polynomial=0x1021, init=0xFFFF, xorout=0x0000, refin=true, refout=true, check=0x6F91 },
            new CRCInfo () { name = "CRC-16/RIELLO", width=16, polynomial=0x1021, init=0xB2AA, xorout=0x0000, refin=true, refout=true, check=0x63D0 },
            new CRCInfo () { name = "CRC-16/T10-DIF", width=16, polynomial=0x8BB7, init=0x0000, xorout=0x0000, refin=false, refout=false, check=0xD0DB },
            new CRCInfo () { name = "CRC-16/TELEDISK", width=16, polynomial=0xA097, init=0x0000, xorout=0x0000, refin=false, refout=false, check=0x0FB3 },
            new CRCInfo () { name = "CRC-16/TMS37157", width=16, polynomial=0x1021, init=0x89EC, xorout=0x0000, refin=true, refout=true, check=0x26B1 },
            new CRCInfo () { name = "CRC-16/USB", width=16, polynomial=0x8005, init=0xFFFF, xorout=0xFFFF, refin=true, refout=true, check=0xB4C8 },
            new CRCInfo () { name = "CRC-A", width=16, polynomial=0x1021, init=0xC6C6, xorout=0x0000, refin=true, refout=true, check=0xBF05 },
            new CRCInfo () { name = "KERMIT", width=16, polynomial=0x1021, init=0x0000, xorout=0x0000, refin=true, refout=true, check=0x2189 },
            new CRCInfo () { name = "MODBUS", width=16, polynomial=0x8005, init=0xFFFF, xorout=0x0000, refin=true, refout=true, check=0x4B37 },
            new CRCInfo () { name = "CRC-16/IBM-SDLC", width=16, polynomial=0x1021, init=0xFFFF, xorout=0xFFFF, refin=true, refout=true, check=0x906E },
            new CRCInfo () { name = "XMODEM", width=16, polynomial=0x1021, init=0x0000, xorout=0x0000, refin=false, refout=false, check=0x31C3 },
            new CRCInfo () { name = "CRC-24/OPENPGP", width=24, polynomial=0x864CFB, init=0xB704CE, xorout=0x000000, refin=false, refout=false, check=0x21CF02 },
            new CRCInfo () { name = "CRC-24/FLEXRAY-A", width=24, polynomial=0x5D6DCB, init=0xFEDCBA, xorout=0x000000, refin=false, refout=false, check=0x7979BD },
            new CRCInfo () { name = "CRC-24/FLEXRAY-B", width=24, polynomial=0x5D6DCB, init=0xABCDEF, xorout=0x000000, refin=false, refout=false, check=0x1F23B8 },
            new CRCInfo () { name = "CRC-31/PHILIPS", width=31, polynomial=0x04C11DB7, init=0x7FFFFFFF, xorout=0x7FFFFFFF, refin=false, refout=false, check=0x0CE9E46C },
            new CRCInfo () { name = "CRC-32", width=32, polynomial=0x04C11DB7, init=0xFFFFFFFF, xorout=0xFFFFFFFF, refin=true, refout=true, check=0xCBF43926 },
            new CRCInfo () { name = "CRC-32/BZIP2", width=32, polynomial=0x04C11DB7, init=0xFFFFFFFF, xorout=0xFFFFFFFF, refin=false, refout=false, check=0xFC891918 },
            new CRCInfo () { name = "CRC-32/MPEG-2", width=32, polynomial=0x04C11DB7, init=0xFFFFFFFF, xorout=0x00000000, refin=false, refout=false, check=0x0376E6E7 },
            new CRCInfo () { name = "CRC-32/POSIX", width=32, polynomial=0x04C11DB7, init=0x00000000, xorout=0xFFFFFFFF, refin=false, refout=false, check=0x765E7680 },
            new CRCInfo () { name = "JAMCRC", width=32, polynomial=0x04C11DB7, init=0xFFFFFFFF, xorout=0x00000000, refin=true, refout=true, check=0x340BC6D9 },
            new CRCInfo () { name = "CRC-32C", width=32, polynomial=0x1EDC6F41, init=0xFFFFFFFF, xorout=0xFFFFFFFF, refin=true, refout=true, check=0xE3069283 },
            new CRCInfo () { name = "CRC-32D", width=32, polynomial=0xA833982B, init=0xFFFFFFFF, xorout=0xFFFFFFFF, refin=true, refout=true, check=0x87315576 },
            new CRCInfo () { name = "CRC-32Q", width=32, polynomial=0x814141AB, init=0x00000000, xorout=0x00000000, refin=false, refout=false, check=0x3010BF7F },
            new CRCInfo () { name = "XFER", width=32, polynomial=0x000000AF, init=0x00000000, xorout=0x00000000, refin=false, refout=false, check=0xBD0BE338 },
            new CRCInfo () { name = "CRC-40/GSM", width=40, polynomial=0x0004820009, init=0x0000000000, xorout=0x0000000000, refin=false, refout=false, check=0x2BE9B039B9 },
            new CRCInfo () { name = "CRC-64", width=64, polynomial=0x42F0E1EBA9EA3693, init=0x0000000000000000, xorout=0x0000000000000000, refin=false, refout=false, check=0x6C40DF5F0B497347 },
            new CRCInfo () { name = "CRC-64/WE", width=64, polynomial=0x42F0E1EBA9EA3693, init=0xFFFFFFFFFFFFFFFF, xorout=0xFFFFFFFFFFFFFFFF, refin=false, refout=false, check=0x62EC59E3F1A4F00A },
            new CRCInfo () { name = "CRC-64/1B", width=64, polynomial=0x000000000000001B, init=0x0000000000000000, xorout=0x0000000000000000, refin=true, refout=true, check=0x46A5A9388A5BEFFE },
            new CRCInfo () { name = "CRC-64/Jones", width=64, polynomial=0xAD93D23594C935A9, init=0xFFFFFFFFFFFFFFFF, xorout=0x0000000000000000, refin=true, refout=true, check=0xCAA717168609F281 }
        };

        #endregion

        public CRCSettings()
        {
            CRCMethod = 53; /* CRC-32 */
        }

        #region taskpane

        private int _CRCMethod;
        [TaskPane("CRCMethodCaption", "CRCMethodTooltip", null, 1, false, ControlType.ComboBox, new string[] {
            "CRC-1/Partiy", "CRC-3/ROHC", "CRC-4/ITU", "CRC-5/EPC", "CRC-5/ITU", "CRC-5/USB", "CRC-6/DARC", "CRC-6/ITU", "CRC-7", "CRC-7/ROHC", "CRC-8", "CRC-8/ITU", "CRC-8/ROHC", "CRC-8/DARC", "CRC-8/I-CODE", "CRC-8/J1850", "CRC-8/MAXIM",
            "CRC-8/WCDMA", "CRC-8/CCITT", "CRC-8/EBU", "CRC-10", "CRC-11", "CRC-12/3GPP", "CRC-12/DECT", "CRC-14/DARC", "CRC-15", "CRC-15/MPT1327", "CRC-16", "CRC-16/AUG-CCITT", "CRC-16/BUYPASS", "CRC-16/CCITT-FALSE", "CRC-16/DDS-110",
            "CRC-16/DECT-R", "CRC-16/DECT-X", "CRC-16/DNP", "CRC-16/EN-13757", "CRC-16/GENIBUS", "CRC-16/MAXIM", "CRC-16/MCRF4XX", "CRC-16/RIELLO", "CRC-16/T10-DIF", "CRC-16/TELEDISK", "CRC-16/TMS37157", "CRC-16/USB", "CRC-A", "KERMIT",
            "MODBUS", "CRC-16/IBM-SDLC", "XMODEM", "CRC-24/OPENPGP", "CRC-24/FLEXRAY-A", "CRC-24/FLEXRAY-B", "CRC-31/PHILIPS", "CRC-32", "CRC-32/BZIP2", "CRC-32/MPEG-2", "CRC-32/POSIX", "JAMCRC", "CRC-32C", "CRC-32D", "CRC-32Q", "XFER",
            "CRC-40/GSM", "CRC-64", "CRC-64/WE", "CRC-64/1B", "CRC-64/Jones" })]
        public int CRCMethod
        {
            get => _CRCMethod;
            set
            {
                _CRCMethod = value;
                Width = crcspecs[CRCMethod].width;
                string fmt = "x" + ((Width + 3) / 4);
                Polynomial = crcspecs[CRCMethod].polynomial.ToString(fmt);
                Init = crcspecs[CRCMethod].init.ToString(fmt);
                XorOut = crcspecs[CRCMethod].xorout.ToString(fmt);
                RefIn = crcspecs[CRCMethod].refin;
                RefOut = crcspecs[CRCMethod].refout;
                OnPropertyChanged("CRCMethod");
            }
        }

        //private int _mode = 0;
        //[TaskPane("ModeCaption", "ModeTooltip", null, 2, false, ControlType.ComboBox, new string[] { "calculate CRC", "check integrity" })]
        //public int Mode
        //{
        //    get { return _mode; }
        //    set
        //    {
        //        if (value != _mode)
        //        {
        //            _mode = value;
        //            OnPropertyChanged("Mode");
        //        }
        //    }
        //}

        private int _width;
        [TaskPane("WidthCaption", "WidthTooltip", "CRCSpecsGroup", 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 64)]
        public int Width
        {
            get => _width;
            set
            {
                if (value != _width)
                {
                    _width = value;
                    OnPropertyChanged("Width");
                }
            }
        }

        private string _polynomial;
        [TaskPaneAttribute("PolynomialCaption", "PolynomialTooltip", "CRCSpecsGroup", 3, false, ControlType.TextBox, ValidationType.RegEx, "^[0-9a-fA-F]{1,64}$")]
        public string Polynomial
        {
            get => _polynomial;
            set
            {
                if (value != _polynomial)
                {
                    _polynomial = value;
                    OnPropertyChanged("Polynomial");
                }
            }
        }

        private string _init;
        [TaskPaneAttribute("InitCaption", "InitTooltip", "CRCSpecsGroup", 4, false, ControlType.TextBox, ValidationType.RegEx, "^[0-9a-fA-F]{1,64}$")]
        public string Init
        {
            get => _init;
            set
            {
                if (value != _init)
                {
                    _init = value;
                    OnPropertyChanged("Init");
                }
            }
        }

        private string _xorout;
        [TaskPaneAttribute("XorOutCaption", "XorOutTooltip", "CRCSpecsGroup", 5, false, ControlType.TextBox, ValidationType.RegEx, "^[0-9a-fA-F]{1,64}$")]
        public string XorOut
        {
            get => _xorout;
            set
            {
                if (value != _xorout)
                {
                    _xorout = value;
                    OnPropertyChanged("XorOut");
                }
            }
        }

        private bool _refin;
        [TaskPaneAttribute("RefInCaption", "RefInTooltip", "CRCSpecsGroup", 6, false, ControlType.CheckBox)]
        public bool RefIn
        {
            get => _refin;
            set
            {
                if (_refin != value)
                {
                    _refin = value;
                    OnPropertyChanged("RefIn");
                }
            }
        }

        private bool _refout;
        [TaskPaneAttribute("RefOutCaption", "RefOutTooltip", "CRCSpecsGroup", 7, false, ControlType.CheckBox)]
        public bool RefOut
        {
            get => _refout;
            set
            {
                if (_refout != value)
                {
                    _refout = value;
                    OnPropertyChanged("RefOut");
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        protected void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;
        private void ChangePluginIcon(int iconIndex)
        {
            if (OnPluginStatusChanged != null)
            {
                OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, iconIndex));
            }
        }

        #endregion
    }
}
