/* HOWTO: Change year, author name and organization.
   Copyright 2010 Your Name, University of Duckburg

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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace CrypTool.Plugins.M209
{
    public class M209Settings : ISettings
    {
        #region Public M209 specific interface

        public delegate void M209ReExecute();
        public event M209ReExecute ReExecute;
        #endregion

        #region Private Variables
        private readonly ObservableCollection<string> actionStrings = new ObservableCollection<string>();
        private readonly ObservableCollection<string> rotorAStrings = new ObservableCollection<string>();
        private readonly ObservableCollection<string> rotorBStrings = new ObservableCollection<string>();
        private readonly ObservableCollection<string> reflectorStrings = new ObservableCollection<string>();
        private int model = 0;

        private int selectedAction = 0;

        public M209 m209;

        private string startwert = "AAAAAA";

        // aktive Pins an den Rotoren
        public string[] initrotors = new string[6] {
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
            "ABCDEFGHIJKLMNOPQRSTUVXYZ",    // no W
            "ABCDEFGHIJKLMNOPQRSTUVX",      // no WYZ
            "ABCDEFGHIJKLMNOPQRSTU",        // no V-Z
            "ABCDEFGHIJKLMNOPQRS",          // no T-Z
            "ABCDEFGHIJKLMNOPQ"             // no R-Z
        };

        // Default-Einstellungen
        private readonly string[] rotor = new string[6] { "ABDHIKMNSTVW", "ADEGJKLORSUX", "ABGHJLMNRSTUX", "CEFHIMNPSTU", "BDEFHIMNPS", "ABDHKNOQ" };

        public string[] bar = new string[27] {
            "36","06","16","15","45","04","04","04","04",
            "20","20","20","20","20","20","20","20","20",
            "20","25","25","05","05","05","05","05","05"
        };

        private int unknownSymbolHandling = 0;  // 0=ignore, leave unmodified
        private int caseHandling = 0;           // 0=preserve, 1=convert all to upper, 2=convert all to lower
        private bool blockOutput = false;       // output data in blocks of 5 characters
        private bool zspace = false;            // replace spaces with 'Z'
        private bool formattedCheck = true;     // false: only ouput check value, true: output formatted internal key + check value

        #endregion

        #region

        public int Stangen => (Model == 1) ? 25 : 27;

        public int Rotoren => (Model == 1) ? 5 : 6;

        public int ActivePins
        {
            get
            {
                int res = 0;
                for (int i = 0; i < Rotoren; i++)
                {
                    res += rotor[i].Length;
                }

                return res;
            }
        }

        public int TotalPins
        {
            get
            {
                int res = 0;
                for (int i = 0; i < Rotoren; i++)
                {
                    res += initrotors[i].Length;
                }

                return res;
            }
        }

        public string InternalKey
        {
            get => Startwert + "," + string.Join(",", rotor) + "," + string.Join(",", bar);
            set
            {
                string[] s = value.Split(new char[] { ',' });
                if (s.Length != 1 + Stangen + Rotoren)
                {
                    return;
                }
            }
        }

        public string FormattedInternalKey
        {
            get
            {
                string[] s = new string[27];

                for (int i = 0; i < 27; i++)
                {
                    char l0 = (bar[i].Length >= 1) ? bar[i][0] : '0';
                    char l1 = (bar[i].Length >= 2) ? bar[i][1] : '0';
                    if (bar[i].Length == 1 && l0 >= '4') { l1 = l0; l0 = '0'; }
                    s[i] = string.Format("{0:00} {1}-{2} ", i + 1, l0, l1);
                }

                for (int i = 0; i < Rotoren; i++)
                {
                    for (int j = 0; j < initrotors[i].Length; j++)
                    {
                        s[j] += string.Format("  {0}", (rotor[i].Contains(initrotors[i][j])) ? initrotors[i][j] : '-');
                    }
                }

                return string.Join("\n", s);
            }
        }

        #endregion

        #region TaskPane Settings

        [ContextMenu("ModelCaption", "ModelTooltip", 0, ContextMenuControlType.ComboBox, null, new string[] { "ModelList1", "ModelList2" })]
        [TaskPane("ModelCaption", "ModelTooltip", null, 0, false, ControlType.ComboBox, new string[] { "ModelList1", "ModelList2" })]
        [PropertySaveOrder(1)]
        public int Model
        {
            get => model;
            set
            {
                if (value != model)
                {
                    model = value;
                    OnPropertyChanged("Model");
                }
            }
        }

        [ContextMenu("ActionCaption", "ActionTooltip", 1, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, "ActionList1", "ActionList2")]
        [TaskPane("ActionCaption", "ActionTooltip", null, 1, true, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
        public int Action
        {
            get => selectedAction;
            set
            {
                if (value != selectedAction)
                {
                    selectedAction = value;
                    OnPropertyChanged("Action");

                    //if (ReExecute != null) ReExecute();   
                }
            }
        }

        [TaskPaneAttribute("StartwertCaption", "StartwertTooltip", null, 2, true, ControlType.TextBox, ValidationType.RegEx, "^[A-Z][A-VX-Z][A-VX][A-U][A-S][A-Q]?$")]
        public string Startwert
        {
            get => startwert;
            set
            {
                if (startwert != value)
                {
                    startwert = value;
                    OnPropertyChanged("Startwert");
                }
            }
        }

        [TaskPane("RandomKeyCaption", "RandomKeyTooltip", "RandomKeyGroup", 3, false, ControlType.Button)]
        public void RandomKey()
        {
            m209.RandomKey();
        }

        #region Wheel options
        [TaskPane("Rotor1Caption", "Rotor1Tooltip", "WheelGroup", 4, true, ControlType.TextBox, ValidationType.RegEx, "^[A-Z]{0,26}$")]
        public string Rotor1
        {
            get => rotor[0];
            set
            {
                //char[] c = value.ToCharArray().Distinct().ToArray();
                //Array.Sort(c);
                //value = new String(c);

                if (rotor[0] != value)
                {
                    rotor[0] = value;
                    OnPropertyChanged("Rotor1");
                }
            }
        }
        [TaskPane("Rotor2Caption", "Rotor2Tooltip", "WheelGroup", 4, true, ControlType.TextBox, ValidationType.RegEx, "^[A-VX-Z]{0,25}$")]
        public string Rotor2
        {
            get => rotor[1];
            set
            {
                if (rotor[1] != value)
                {
                    rotor[1] = value;
                    OnPropertyChanged("Rotor2");
                }
            }
        }
        [TaskPane("Rotor3Caption", "Rotor3Tooltip", "WheelGroup", 4, true, ControlType.TextBox, ValidationType.RegEx, "^[A-VX]{0,23}$")]
        public string Rotor3
        {
            get => rotor[2];
            set
            {
                if (rotor[2] != value)
                {
                    rotor[2] = value;
                    OnPropertyChanged("Rotor3");
                }
            }
        }
        [TaskPane("Rotor4Caption", "Rotor4Tooltip", "WheelGroup", 4, true, ControlType.TextBox, ValidationType.RegEx, "^[A-U]{0,21}$")]
        public string Rotor4
        {
            get => rotor[3];
            set
            {
                if (rotor[3] != value)
                {
                    rotor[3] = value;
                    OnPropertyChanged("Rotor4");
                }
            }
        }
        [TaskPane("Rotor5Caption", "Rotor5Tooltip", "WheelGroup", 4, true, ControlType.TextBox, ValidationType.RegEx, "^[A-S]{0,19}$")]
        public string Rotor5
        {
            get => rotor[4];
            set
            {
                if (rotor[4] != value)
                {
                    rotor[4] = value;
                    OnPropertyChanged("Rotor5");
                }
            }
        }
        [TaskPane("Rotor6Caption", "Rotor6Tooltip", "WheelGroup", 4, true, ControlType.TextBox, ValidationType.RegEx, "^[A-Q]{0,17}$")]
        public string Rotor6
        {
            get => rotor[5];
            set
            {
                if (rotor[5] != value)
                {
                    rotor[5] = value;
                    OnPropertyChanged("Rotor6");
                }
            }
        }

        //WheelOptions
        #endregion

        #region Bar options
        [TaskPane("Bar1Caption", "Bar1Tooltip", "BarGroup", 11, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar1
        {
            get => bar[0];
            set
            {
                if (bar[0] != value)
                {
                    bar[0] = value;
                    OnPropertyChanged("Bar1");
                }
            }
        }
        [TaskPane("Bar2Caption", "Bar2Tooltip", "BarGroup", 12, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar2
        {
            get => bar[1];
            set
            {
                if (bar[1] != value)
                {
                    bar[1] = value;
                    OnPropertyChanged("Bar2");
                }
            }
        }

        [TaskPane("Bar3Caption", "Bar3Tooltip", "BarGroup", 13, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar3
        {
            get => bar[2];
            set
            {
                if (bar[2] != value)
                {
                    bar[2] = value;
                    OnPropertyChanged("Bar3");
                }
            }
        }

        [TaskPane("Bar4Caption", "Bar4Tooltip", "BarGroup", 14, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar4
        {
            get => bar[3];
            set
            {
                if (bar[3] != value)
                {
                    bar[3] = value;
                    OnPropertyChanged("Bar4");
                }
            }
        }

        [TaskPane("Bar5Caption", "Bar5Tooltip", "BarGroup", 15, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar5
        {
            get => bar[4];
            set
            {
                if (bar[4] != value)
                {
                    bar[4] = value;
                    OnPropertyChanged("Bar5");
                }
            }
        }

        [TaskPane("Bar6Caption", "Bar6Tooltip", "BarGroup", 16, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar6
        {
            get => bar[5];
            set
            {
                if (bar[5] != value)
                {
                    bar[5] = value;
                    OnPropertyChanged("Bar6");
                }
            }
        }

        [TaskPane("Bar7Caption", "Bar7Tooltip", "BarGroup", 17, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar7
        {
            get => bar[6];
            set
            {
                if (bar[6] != value)
                {
                    bar[6] = value;
                    OnPropertyChanged("Bar7");
                }
            }
        }

        [TaskPane("Bar8Caption", "Bar8Tooltip", "BarGroup", 18, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar8
        {
            get => bar[7];
            set
            {
                if (bar[7] != value)
                {
                    bar[7] = value;
                    OnPropertyChanged("Bar8");
                }
            }
        }

        [TaskPane("Bar9Caption", "Bar9Tooltip", "BarGroup", 19, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar9
        {
            get => bar[8];
            set
            {
                if (bar[8] != value)
                {
                    bar[8] = value;
                    OnPropertyChanged("Bar9");
                }
            }
        }

        [TaskPane("Bar10Caption", "Bar10Tooltip", "BarGroup", 20, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar10
        {
            get => bar[9];
            set
            {
                if (bar[9] != value)
                {
                    bar[9] = value;
                    OnPropertyChanged("Bar10");
                }
            }
        }

        [TaskPane("Bar11Caption", "Bar11Tooltip", "BarGroup", 21, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar11
        {
            get => bar[10];
            set
            {
                if (bar[10] != value)
                {
                    bar[10] = value;
                    OnPropertyChanged("Bar11");
                }
            }
        }

        [TaskPane("Bar12Caption", "Bar12Tooltip", "BarGroup", 22, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar12
        {
            get => bar[11];
            set
            {
                if (bar[11] != value)
                {
                    bar[11] = value;
                    OnPropertyChanged("Bar12");
                }
            }
        }

        [TaskPane("Bar13Caption", "Bar13Tooltip", "BarGroup", 23, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar13
        {
            get => bar[12];
            set
            {
                if (bar[12] != value)
                {
                    bar[12] = value;
                    OnPropertyChanged("Bar13");
                }
            }
        }

        [TaskPane("Bar14Caption", "Bar14Tooltip", "BarGroup", 24, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar14
        {
            get => bar[13];
            set
            {
                if (bar[13] != value)
                {
                    bar[13] = value;
                    OnPropertyChanged("Bar14");
                }
            }
        }

        [TaskPane("Bar15Caption", "Bar15Tooltip", "BarGroup", 25, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar15
        {
            get => bar[14];
            set
            {
                if (bar[14] != value)
                {
                    bar[14] = value;
                    OnPropertyChanged("Bar15");
                }
            }
        }

        [TaskPane("Bar16Caption", "Bar16Tooltip", "BarGroup", 26, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar16
        {
            get => bar[15];
            set
            {
                if (bar[15] != value)
                {
                    bar[15] = value;
                    OnPropertyChanged("Bar16");
                }
            }
        }

        [TaskPane("Bar17Caption", "Bar17Tooltip", "BarGroup", 27, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar17
        {
            get => bar[16];
            set
            {
                if (bar[16] != value)
                {
                    bar[16] = value;
                    OnPropertyChanged("Bar17");
                }
            }
        }

        [TaskPane("Bar18Caption", "Bar18Tooltip", "BarGroup", 28, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar18
        {
            get => bar[17];
            set
            {
                if (bar[17] != value)
                {
                    bar[17] = value;
                    OnPropertyChanged("Bar18");
                }
            }
        }

        [TaskPane("Bar19Caption", "Bar19Tooltip", "BarGroup", 29, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar19
        {
            get => bar[18];
            set
            {
                if (bar[18] != value)
                {
                    bar[18] = value;
                    OnPropertyChanged("Bar19");
                }
            }
        }

        [TaskPane("Bar20Caption", "Bar20Tooltip", "BarGroup", 30, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar20
        {
            get => bar[19];
            set
            {
                if (bar[19] != value)
                {
                    bar[19] = value;
                    OnPropertyChanged("Bar20");
                }
            }
        }

        [TaskPane("Bar21Caption", "Bar21Tooltip", "BarGroup", 31, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar21
        {
            get => bar[20];
            set
            {
                if (bar[20] != value)
                {
                    bar[20] = value;
                    OnPropertyChanged("Bar21");
                }
            }
        }

        [TaskPane("Bar22Caption", "Bar22Tooltip", "BarGroup", 32, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar22
        {
            get => bar[21];
            set
            {
                if (bar[21] != value)
                {
                    bar[21] = value;
                    OnPropertyChanged("Bar22");
                }
            }
        }

        [TaskPane("Bar23Caption", "Bar23Tooltip", "BarGroup", 33, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar23
        {
            get => bar[22];
            set
            {
                if (bar[22] != value)
                {
                    bar[22] = value;
                    OnPropertyChanged("Bar23");
                }
            }
        }

        [TaskPane("Bar24Caption", "Bar24Tooltip", "BarGroup", 34, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar24
        {
            get => bar[23];
            set
            {
                if (bar[23] != value)
                {
                    bar[23] = value;
                    OnPropertyChanged("Bar24");
                }
            }
        }

        [TaskPane("Bar25Caption", "Bar25Tooltip", "BarGroup", 35, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar25
        {
            get => bar[24];
            set
            {
                if (bar[24] != value)
                {
                    bar[24] = value;
                    OnPropertyChanged("Bar25");
                }
            }
        }

        [TaskPane("Bar26Caption", "Bar26Tooltip", "BarGroup", 36, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar26
        {
            get => bar[25];
            set
            {
                if (bar[25] != value)
                {
                    bar[25] = value;
                    OnPropertyChanged("Bar26");
                }
            }
        }

        [TaskPane("Bar27Caption", "Bar27Tooltip", "BarGroup", 37, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar27
        {
            get => bar[26];
            set
            {
                if (bar[26] != value)
                {
                    bar[26] = value;
                    OnPropertyChanged("Bar27");
                }
            }
        }

        // Bar Setting
        #endregion

        #region Text options

        [TaskPane("UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip", "TextOptionsGroup", 50, false, ControlType.ComboBox, new string[] { "UnknownSymbolHandlingList1", "UnknownSymbolHandlingList2", "UnknownSymbolHandlingList3" })]
        public int UnknownSymbolHandling
        {
            get => unknownSymbolHandling;
            set
            {
                if (value != unknownSymbolHandling)
                {
                    unknownSymbolHandling = value;
                    OnPropertyChanged("UnknownSymbolHandling");
                }
            }
        }

        [TaskPane("CaseHandlingCaption", "CaseHandlingTooltip", "TextOptionsGroup", 51, false, ControlType.ComboBox, new string[] { "CaseHandlingList1", "CaseHandlingList2", "CaseHandlingList3" })]
        public int CaseHandling
        {
            get => caseHandling;
            set
            {
                if (value != caseHandling)
                {
                    caseHandling = value;
                    OnPropertyChanged("CaseHandling");
                }
            }
        }

        [TaskPane("ZSpaceCaption", "ZSpaceTooltip", "TextOptionsGroup", 52, false, ControlType.CheckBox)]
        public bool ZSpace
        {
            get => zspace;
            set
            {
                if (value != zspace)
                {
                    zspace = value;
                    OnPropertyChanged("ZSpace");
                }
            }
        }

        [TaskPane("BlockCaption", "BlockTooltip", "TextOptionsGroup", 53, false, ControlType.CheckBox)]
        public bool BlockOutput
        {
            get => blockOutput;
            set
            {
                if (value != blockOutput)
                {
                    blockOutput = value;
                    OnPropertyChanged("BlockOutput");
                }
            }
        }

        [TaskPane("FormattedCheckCaption", "FormattedCheckTooltip", "TextOptionsGroup", 54, false, ControlType.CheckBox)]
        public bool FormattedCheck
        {
            get => formattedCheck;
            set
            {
                if (value != formattedCheck)
                {
                    formattedCheck = value;
                    OnPropertyChanged("FormattedCheck");
                }
            }
        }

        #endregion

        //Taskpane ende
        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
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