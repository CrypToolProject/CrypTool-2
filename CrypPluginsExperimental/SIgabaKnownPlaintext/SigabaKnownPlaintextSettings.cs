/*
   Copyright 2011 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

namespace SigabaKnownPlaintext
{
    // HOWTO: rename class (click name, press F2)
    [global::CrypTool.PluginBase.Attributes.Localization("SigabaKnownPlaintext.Properties.Resources")]
    public class SigabaKnownPlaintextSettings : ISettings
    {
        public int[] cipherRotorRev;
        public int[] cipherRotorFrom;
        public int[] cipherRotorTo;

        public bool[][] AnalysisUseRotor;

        public bool[] cipher1AnalysisUseRotor;
        public bool[] cipher2AnalysisUseRotor;
        public bool[] cipher3AnalysisUseRotor;
        public bool[] cipher4AnalysisUseRotor;
        public bool[] cipher5AnalysisUseRotor;
        
        public bool action = false;

        #region Private Variables

        

        private int cipherRotor1From = 0;
        private int cipherRotor1To = 25;

        private int cipherRotor1Rev = 0;

        private int cipherRotor2From = 0;
        private int cipherRotor2To = 25;

        private int cipherRotor2Rev = 0;

        private int cipherRotor3From = 0;
        private int cipherRotor3To = 25;

        private int cipherRotor3Rev = 0;

        private int cipherRotor4From = 0;
        private int cipherRotor4To = 25;

        private int cipherRotor4Rev = 0;

        private int cipherRotor5From = 0;
        private int cipherRotor5To = 25;

        private int cipherRotor5Rev = 0;

        
        private bool cipher1AnalysisUseRotor1 = true;
        private bool cipher1AnalysisUseRotor2 = true;
        private bool cipher1AnalysisUseRotor3 = true;
        private bool cipher1AnalysisUseRotor4 = true;
        private bool cipher1AnalysisUseRotor5 = true;
        private bool cipher1AnalysisUseRotor6 = true;
        private bool cipher1AnalysisUseRotor7 = true;
        private bool cipher1AnalysisUseRotor8 = true;
        private bool cipher1AnalysisUseRotor9 = true;
        private bool cipher1AnalysisUseRotor0 = true;

        private bool cipher2AnalysisUseRotor1 = true;
        private bool cipher2AnalysisUseRotor2 = true;
        private bool cipher2AnalysisUseRotor3 = true;
        private bool cipher2AnalysisUseRotor4 = true;
        private bool cipher2AnalysisUseRotor5 = true;
        private bool cipher2AnalysisUseRotor6 = true;
        private bool cipher2AnalysisUseRotor7 = true;
        private bool cipher2AnalysisUseRotor8 = true;
        private bool cipher2AnalysisUseRotor9 = true;
        private bool cipher2AnalysisUseRotor0 = true;

        private bool cipher3AnalysisUseRotor1 = true;
        private bool cipher3AnalysisUseRotor2 = true;
        private bool cipher3AnalysisUseRotor3 = true;
        private bool cipher3AnalysisUseRotor4 = true;
        private bool cipher3AnalysisUseRotor5 = true;
        private bool cipher3AnalysisUseRotor6 = true;
        private bool cipher3AnalysisUseRotor7 = true;
        private bool cipher3AnalysisUseRotor8 = true;
        private bool cipher3AnalysisUseRotor9 = true;
        private bool cipher3AnalysisUseRotor0 = true;

        private bool cipher4AnalysisUseRotor1 = true;
        private bool cipher4AnalysisUseRotor2 = true;
        private bool cipher4AnalysisUseRotor3 = true;
        private bool cipher4AnalysisUseRotor4 = true;
        private bool cipher4AnalysisUseRotor5 = true;
        private bool cipher4AnalysisUseRotor6 = true;
        private bool cipher4AnalysisUseRotor7 = true;
        private bool cipher4AnalysisUseRotor8 = true;
        private bool cipher4AnalysisUseRotor9 = true;
        private bool cipher4AnalysisUseRotor0 = true;

        private bool cipher5AnalysisUseRotor1 = true;
        private bool cipher5AnalysisUseRotor2 = true;
        private bool cipher5AnalysisUseRotor3 = true;
        private bool cipher5AnalysisUseRotor4 = true;
        private bool cipher5AnalysisUseRotor5 = true;
        private bool cipher5AnalysisUseRotor6 = true;
        private bool cipher5AnalysisUseRotor7 = true;
        private bool cipher5AnalysisUseRotor8 = true;
        private bool cipher5AnalysisUseRotor9 = true;
        private bool cipher5AnalysisUseRotor0 = true;

        public SigabaKnownPlaintextSettings()
        {
            cipherRotorFrom = new [] {    CipherRotor1From,
                                            CipherRotor2From,
                                            CipherRotor3From,
                                            CipherRotor4From,
                                            CipherRotor5From
                                            
            };

            cipherRotorTo = new []{      CipherRotor1To,
                                            CipherRotor2To,
                                            CipherRotor3To,
                                            CipherRotor4To,
                                            CipherRotor5To
                                            
            };

            cipherRotorRev = new []{ CipherRotor1Rev,
                                        CipherRotor2Rev,
                                        CipherRotor3Rev,
                                        CipherRotor4Rev,
                                        CipherRotor5Rev,
            
            };

            AnalysisUseRotor = new [] { cipher1AnalysisUseRotor, cipher2AnalysisUseRotor, cipher3AnalysisUseRotor, cipher4AnalysisUseRotor, cipher5AnalysisUseRotor };

            cipher1AnalysisUseRotor = new [] {  Cipher1AnalysisUseRotor0, 
                                                    Cipher1AnalysisUseRotor1,
                                                    Cipher1AnalysisUseRotor2,
                                                    Cipher1AnalysisUseRotor3,
                                                    Cipher1AnalysisUseRotor4,
                                                    Cipher1AnalysisUseRotor5,
                                                    Cipher1AnalysisUseRotor6,
                                                    Cipher1AnalysisUseRotor7,
                                                    Cipher1AnalysisUseRotor8,
                                                    Cipher1AnalysisUseRotor9
            };

            cipher2AnalysisUseRotor = new [] {  Cipher2AnalysisUseRotor0, 
                                                    Cipher2AnalysisUseRotor1,
                                                    Cipher2AnalysisUseRotor2,
                                                    Cipher2AnalysisUseRotor3,
                                                    Cipher2AnalysisUseRotor4,
                                                    Cipher2AnalysisUseRotor5,
                                                    Cipher2AnalysisUseRotor6,
                                                    Cipher2AnalysisUseRotor7,
                                                    Cipher2AnalysisUseRotor8,
                                                    Cipher2AnalysisUseRotor9
            };
            cipher3AnalysisUseRotor = new [] {  Cipher3AnalysisUseRotor0, 
                                                    Cipher3AnalysisUseRotor1,
                                                    Cipher3AnalysisUseRotor2,
                                                    Cipher3AnalysisUseRotor3,
                                                    Cipher3AnalysisUseRotor4,
                                                    Cipher3AnalysisUseRotor5,
                                                    Cipher3AnalysisUseRotor6,
                                                    Cipher3AnalysisUseRotor7,
                                                    Cipher3AnalysisUseRotor8,
                                                    Cipher3AnalysisUseRotor9
            };
            cipher4AnalysisUseRotor = new [] {  Cipher4AnalysisUseRotor0, 
                                                    Cipher4AnalysisUseRotor1,
                                                    Cipher4AnalysisUseRotor2,
                                                    Cipher4AnalysisUseRotor3,
                                                    Cipher4AnalysisUseRotor4,
                                                    Cipher4AnalysisUseRotor5,
                                                    Cipher4AnalysisUseRotor6,
                                                    Cipher4AnalysisUseRotor7,
                                                    Cipher4AnalysisUseRotor8,
                                                    Cipher4AnalysisUseRotor9
            };
            cipher5AnalysisUseRotor = new [] {  Cipher5AnalysisUseRotor0, 
                                                    Cipher5AnalysisUseRotor1,
                                                    Cipher5AnalysisUseRotor2,
                                                    Cipher5AnalysisUseRotor3,
                                                    Cipher5AnalysisUseRotor4,
                                                    Cipher5AnalysisUseRotor5,
                                                    Cipher5AnalysisUseRotor6,
                                                    Cipher5AnalysisUseRotor7,
                                                    Cipher5AnalysisUseRotor8,
                                                    Cipher5AnalysisUseRotor9
            };
        }

        #endregion

        public void Initialize()
        {

        }

        #region TaskPane Settings

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [TaskPane("Verfahren", "Verfahren", "", 2, false, ControlType.ComboBox,
              new String[] { "Verfahren zur Schlüsselsuche", "Verfahren zur schnellen Dechiffrierung"})]
        public bool Action
        {
            get { return action; }
            set
            {
                action = value;
                OnPropertyChanged("Action");
               
               

            }
        }


        #endregion

        #region Cipher Bank

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Eins")]
        [TaskPane("CipherRotor1From", "PlugBoardATooltip", "PositionOptionsGroup", 1, false, ControlType.ComboBox,
            new String[] { "LetterA", "LetterB", "LetterC", "LetterD", "LetterE", "LetterF", "LetterG", "LetterH", "LetterI", "LetterJ", "LetterK", "LetterL", "LetterM", "LetterN", "LetterO", "LetterP", "LetterQ", "LetterR", "LetterS", "LetterT", "LetterU", "LetterV", "LetterW", "LetterX", "LetterY", "LetterZ" })]
        public int CipherRotor1From
        {
            get
            {
                return this.cipherRotor1From;
            }
            set
            {

                this.cipherRotor1From = (int)value;
                this.cipherRotorFrom[0] = (int)value;
                OnPropertyChanged("CipherRotor1From");
                

                if (value > cipherRotor1To)
                {
                    CipherRotor1To = value;
                    this.cipherRotorTo[0] = (int)value;

                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Eins")]
        [TaskPane("ToCaption", "PlugBoardBTooltip", "PositionOptionsGroup", 2, false, ControlType.ComboBox,
            new String[] { "LetterA", "LetterB", "LetterC", "LetterD", "LetterE", "LetterF", "LetterG", "LetterH", "LetterI", "LetterJ", "LetterK", "LetterL", "LetterM", "LetterN", "LetterO", "LetterP", "LetterQ", "LetterR", "LetterS", "LetterT", "LetterU", "LetterV", "LetterW", "LetterX", "LetterY", "LetterZ" })]
        public int CipherRotor1To
        {
            get { return cipherRotor1To; }
            set
            {
                cipherRotor1To = value;
                this.cipherRotorTo[0] = (int)value;
                OnPropertyChanged("CipherRotor1To");
                
                if (value < cipherRotor1From)
                {
                    CipherRotor1From = value;
                    this.cipherRotorFrom[0] = (int)value;
                }

            }
        }

        [TaskPane("RevCaption", "PlugBoardBTooltip", "PositionOptionsGroup", 3, false, ControlType.RadioButton, new String[] { "undefinedCaption", "normalCaption", "reverseCaption" })]
        public int CipherRotor1Rev
        {
            get { return cipherRotor1Rev; }
            set
            {
                
                if (value != cipherRotor1Rev)
                {
                    cipherRotorRev[0] = value;
                    cipherRotor1Rev = value;
                    OnPropertyChanged("CipherRotor1Rev");
                }

            }
        }

        #region Rotor choice

        [TaskPane("AnalysisUseRotorICaption", "AnalysisUseRotorITooltip",
            "Cipher1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher1AnalysisUseRotor1
        {
            get { return cipher1AnalysisUseRotor1; }
            set
            {

                if (value != cipher1AnalysisUseRotor1)
                {
                    cipher1AnalysisUseRotor[0] = value;
                    cipher1AnalysisUseRotor1 = value;
                    OnPropertyChanged("Cipher1AnalysisUseRotor1");
                }

            }
        }


        [TaskPane("AnalysisUseRotorIICaption", "AnalysisUseRotorIITooltip",
            "Cipher1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher1AnalysisUseRotor2
        {
            get { return cipher1AnalysisUseRotor2; }
            set
            {

                if (value != cipher1AnalysisUseRotor2)
                {
                    cipher1AnalysisUseRotor[1] = value;
                    cipher1AnalysisUseRotor2 = value;
                    OnPropertyChanged("Cipher1AnalysisUseRotor2");
                }
            }
        }


        [TaskPane("AnalysisUseRotorIIICaption", "AnalysisUseRotorIIITooltip",
            "Cipher1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher1AnalysisUseRotor3
        {
            get { return cipher1AnalysisUseRotor3; }
            set
            {

                if (value != cipher1AnalysisUseRotor3)
                {
                    cipher1AnalysisUseRotor[2] = value;
                    cipher1AnalysisUseRotor3 = value;
                    OnPropertyChanged("Cipher1AnalysisUseRotor3");
                }
            }
        }


        [TaskPane("AnalysisUseRotorIVCaption", "AnalysisUseRotorIVTooltip",
            "Cipher1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher1AnalysisUseRotor4
        {
            get { return cipher1AnalysisUseRotor4; }
            set
            {

                if (value != cipher1AnalysisUseRotor4)
                {
                    cipher1AnalysisUseRotor[3] = value;
                    cipher1AnalysisUseRotor4 = value;
                    OnPropertyChanged("Cipher1AnalysisUseRotor4");
                }
            }
        }



        [TaskPane("AnalysisUseRotorVCaption", "AnalysisUseRotorVTooltip",
            "Cipher1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher1AnalysisUseRotor5
        {
            get { return cipher1AnalysisUseRotor5; }
            set
            {

                if (value != cipher1AnalysisUseRotor5)
                {
                    cipher1AnalysisUseRotor[4] = value;
                    cipher1AnalysisUseRotor5 = value;
                    OnPropertyChanged("Cipher1AnalysisUseRotor5");
                }
            }
        }


        [TaskPane("AnalysisUseRotorVICaption", "AnalysisUseRotorVITooltip",
            "Cipher1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher1AnalysisUseRotor6
        {
            get { return cipher1AnalysisUseRotor6; }
            set
            {

                if (value != cipher1AnalysisUseRotor6)
                {
                    cipher1AnalysisUseRotor[5] = value;
                    cipher1AnalysisUseRotor6 = value;
                    OnPropertyChanged("Cipher1AnalysisUseRotor6");
                }
            }
        }

        [TaskPane("AnalysisUseRotorVIICaption", "AnalysisUseRotorVIITooltip",
            "Cipher1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher1AnalysisUseRotor7
        {
            get { return cipher1AnalysisUseRotor7; }
            set
            {

                if (value != cipher1AnalysisUseRotor7)
                {
                    cipher1AnalysisUseRotor[6] = value;
                    cipher1AnalysisUseRotor7 = value;
                    OnPropertyChanged("Cipher1AnalysisUseRotor7");
                }
            }
        }

        [TaskPane("AnalysisUseRotorVIIICaption", "AnalysisUseRotorVIIITooltip",
            "Cipher1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher1AnalysisUseRotor8
        {
            get { return cipher1AnalysisUseRotor8; }
            set
            {

                if (value != cipher1AnalysisUseRotor8)
                {
                    cipher1AnalysisUseRotor[7] = value;
                    cipher1AnalysisUseRotor8 = value;
                    OnPropertyChanged("Cipher1AnalysisUseRotor8");
                }
            }
        }

        [TaskPane("AnalysisUseRotorIXCaption", "AnalysisUseRotorVIIITooltip",
            "Cipher1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher1AnalysisUseRotor9
        {
            get { return cipher1AnalysisUseRotor9; }
            set
            {

                if (value != cipher1AnalysisUseRotor9)
                {
                    cipher1AnalysisUseRotor[8] = value;
                    cipher1AnalysisUseRotor9 = value;
                    OnPropertyChanged("Cipher1AnalysisUseRotor9");
                }
            }
        }
        [TaskPane("AnalysisUseRotorXCaption", "AnalysisUseRotorVIIITooltip",
            "Cipher1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher1AnalysisUseRotor0
        {
            get { return cipher1AnalysisUseRotor0; }
            set
            {

                if (value != cipher1AnalysisUseRotor0)
                {
                    cipher1AnalysisUseRotor[9] = value;
                    cipher1AnalysisUseRotor0 = value;
                    OnPropertyChanged("Cipher1AnalysisUseRotor0");
                }
            }
        }
        #endregion

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("CipherRotor2FromCaption", "PlugBoardATooltip", "PositionOptionsGroup", 4, false, ControlType.ComboBox,
            new String[] { "LetterA", "LetterB", "LetterC", "LetterD", "LetterE", "LetterF", "LetterG", "LetterH", "LetterI", "LetterJ", "LetterK", "LetterL", "LetterM", "LetterN", "LetterO", "LetterP", "LetterQ", "LetterR", "LetterS", "LetterT", "LetterU", "LetterV", "LetterW", "LetterX", "LetterY", "LetterZ" })]
        public int CipherRotor2From
        {
            get { return cipherRotor2From; }
            set
            {

                cipherRotor2From = value;
                
                this.cipherRotorFrom[1] = (int)value;
                
                OnPropertyChanged("CipherRotor2From");
                

                if (value > cipherRotor2To)
                {
                    CipherRotor2To = value;
                    this.cipherRotorTo[1] = (int)value;

                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("ToCaption", "PlugBoardBTooltip", "PositionOptionsGroup", 5, false, ControlType.ComboBox,
            new String[] { "LetterA", "LetterB", "LetterC", "LetterD", "LetterE", "LetterF", "LetterG", "LetterH", "LetterI", "LetterJ", "LetterK", "LetterL", "LetterM", "LetterN", "LetterO", "LetterP", "LetterQ", "LetterR", "LetterS", "LetterT", "LetterU", "LetterV", "LetterW", "LetterX", "LetterY", "LetterZ" })]
        public int CipherRotor2To
        {
            get { return cipherRotor2To; }
            set
            {

                cipherRotor2To = value;
                this.cipherRotorTo[1] = (int)value;
                OnPropertyChanged("CipherRotor2To");
                
                if (value < cipherRotor2From)
                {
                    CipherRotor2From = value;
                    this.cipherRotorFrom[1] = (int)value;
                }

            }
        }

        [TaskPane("RevCaption", "PlugBoardBTooltip", "PositionOptionsGroup", 6, false, ControlType.RadioButton, new String[] { "undefinedCaption", "normalCaption", "reverseCaption" })]
        public int CipherRotor2Rev
        {
            get { return cipherRotor2Rev; }
            set
            {
                
                if (value != cipherRotor2Rev)
                {
                    cipherRotorRev[1] = value;
                    cipherRotor2Rev = value;
                    OnPropertyChanged("CipherRotor2Rev");
                }

            }
        }

        #region Rotor choice

        [TaskPane("AnalysisUseRotorICaption", "AnalysisUseRotorITooltip",
            "Cipher2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher2AnalysisUseRotor1
        {
            get { return cipher2AnalysisUseRotor1; }
            set
            {

                if (value != cipher2AnalysisUseRotor1)
                {
                    cipher2AnalysisUseRotor[0] = value;
                    cipher2AnalysisUseRotor1 = value;
                    OnPropertyChanged("Cipher2AnalysisUseRotor1");
                }
            }
        }


        [TaskPane("AnalysisUseRotorIICaption", "Cipher2AnalysisUseRotorIITooltip",
            "Cipher2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher2AnalysisUseRotor2
        {
            get { return cipher2AnalysisUseRotor2; }
            set
            {

                if (value != cipher2AnalysisUseRotor2)
                {
                    cipher2AnalysisUseRotor[1] = value;
                    cipher2AnalysisUseRotor2 = value;
                    OnPropertyChanged("Cipher2AnalysisUseRotor2");
                }
            }
        }


        [TaskPane("AnalysisUseRotorIIICaption", "Cipher2AnalysisUseRotorIIITooltip",
            "Cipher2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher2AnalysisUseRotor3
        {
            get { return cipher2AnalysisUseRotor3; }
            set
            {

                if (value != cipher2AnalysisUseRotor3)
                {
                    cipher2AnalysisUseRotor[2] = value;
                    cipher2AnalysisUseRotor3 = value;
                    OnPropertyChanged("Cipher2AnalysisUseRotor3");
                }
            }
        }


        [TaskPane("AnalysisUseRotorIVCaption", "Cipher2AnalysisUseRotorIVTooltip",
            "Cipher2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher2AnalysisUseRotor4
        {
            get { return cipher2AnalysisUseRotor4; }
            set
            {

                if (value != cipher2AnalysisUseRotor4)
                {
                    cipher2AnalysisUseRotor[3] = value;
                    cipher2AnalysisUseRotor4 = value;
                    OnPropertyChanged("Cipher2AnalysisUseRotor4");
                }
            }
        }



        [TaskPane("AnalysisUseRotorVCaption", "Cipher2AnalysisUseRotorVTooltip",
            "Cipher2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher2AnalysisUseRotor5
        {
            get { return cipher2AnalysisUseRotor5; }
            set
            {

                if (value != cipher2AnalysisUseRotor5)
                {
                    cipher2AnalysisUseRotor[4] = value;
                    cipher2AnalysisUseRotor5 = value;
                    OnPropertyChanged("Cipher2AnalysisUseRotor5");
                }
            }
        }


        [TaskPane("AnalysisUseRotorVICaption", "Cipher2AnalysisUseRotorVITooltip",
            "Cipher2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher2AnalysisUseRotor6
        {
            get { return cipher2AnalysisUseRotor6; }
            set
            {

                if (value != cipher2AnalysisUseRotor6)
                {
                    cipher2AnalysisUseRotor[5] = value;
                    cipher2AnalysisUseRotor6 = value;
                    OnPropertyChanged("Cipher2AnalysisUseRotor6");
                }
            }
        }



        [TaskPane("AnalysisUseRotorVIICaption", "Cipher2AnalysisUseRotorVIITooltip",
            "Cipher2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher2AnalysisUseRotor7
        {
            get { return cipher2AnalysisUseRotor7; }
            set
            {

                if (value != cipher2AnalysisUseRotor7)
                {
                    cipher2AnalysisUseRotor[6] = value;
                    cipher2AnalysisUseRotor7 = value;
                    OnPropertyChanged("Cipher2AnalysisUseRotor7");
                }
            }
        }



        [TaskPane("AnalysisUseRotorVIIICaption", "Cipher2AnalysisUseRotorVIIITooltip",
            "Cipher2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher2AnalysisUseRotor8
        {
            get { return cipher2AnalysisUseRotor8; }
            set
            {

                if (value != cipher2AnalysisUseRotor8)
                {
                    cipher2AnalysisUseRotor[7] = value;
                    cipher2AnalysisUseRotor8 = value;
                    OnPropertyChanged("Cipher2AnalysisUseRotor8");
                }
            }
        }

        [TaskPane("AnalysisUseRotorIXCaption", "Cipher2AnalysisUseRotorVIIITooltip",
            "Cipher2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher2AnalysisUseRotor9
        {
            get { return cipher2AnalysisUseRotor9; }
            set
            {

                if (value != cipher2AnalysisUseRotor9)
                {
                    cipher2AnalysisUseRotor[8] = value;
                    cipher2AnalysisUseRotor9 = value;
                    OnPropertyChanged("Cipher2AnalysisUseRotor9");
                }
            }
        }
        [TaskPane("AnalysisUseRotorXCaption", "Cipher2AnalysisUseRotorVIIITooltip",
            "Cipher2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher2AnalysisUseRotor0
        {
            get { return cipher2AnalysisUseRotor0; }
            set
            {

                if (value != cipher2AnalysisUseRotor0)
                {
                    cipher2AnalysisUseRotor[9] = value;
                    cipher2AnalysisUseRotor0 = value;
                    OnPropertyChanged("Cipher2AnalysisUseRotor0");
                }
            }
        }
        #endregion

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Drei")]
        [TaskPane("Cipher Rotor 3: From", "PlugBoardATooltip", "PositionOptionsGroup", 7, false, ControlType.ComboBox,
            new String[] { "LetterA", "LetterB", "LetterC", "LetterD", "LetterE", "LetterF", "LetterG", "LetterH", "LetterI", "LetterJ", "LetterK", "LetterL", "LetterM", "LetterN", "LetterO", "LetterP", "LetterQ", "LetterR", "LetterS", "LetterT", "LetterU", "LetterV", "LetterW", "LetterX", "LetterY", "LetterZ" })]
        public int CipherRotor3From
        {
            get { return cipherRotor3From; }
            set
            {

                cipherRotor3From = value;
                this.cipherRotorFrom[2] = (int)value;
                OnPropertyChanged("CipherRotor3From");
                

                if (value > cipherRotor3To)
                {
                    CipherRotor3To = value;
                    this.cipherRotorTo[2] = (int)value;

                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Drei")]
        [TaskPane("ToCaption", "PlugBoardBTooltip", "PositionOptionsGroup", 8, false, ControlType.ComboBox,
            new String[] { "LetterA", "LetterB", "LetterC", "LetterD", "LetterE", "LetterF", "LetterG", "LetterH", "LetterI", "LetterJ", "LetterK", "LetterL", "LetterM", "LetterN", "LetterO", "LetterP", "LetterQ", "LetterR", "LetterS", "LetterT", "LetterU", "LetterV", "LetterW", "LetterX", "LetterY", "LetterZ" })]
        public int CipherRotor3To
        {
            get { return cipherRotor3To; }
            set
            {

                cipherRotor3To = value;
                this.cipherRotorTo[2] = (int)value;

                OnPropertyChanged("CipherRotor3To");
                
                if (value < cipherRotor3From)
                {
                    CipherRotor3From = value;
                    this.cipherRotorFrom[2] = (int)value;
                }

            }
        }

        [TaskPane("RevCaption", "PlugBoardBTooltip", "PositionOptionsGroup", 9, false, ControlType.RadioButton, new String[] { "undefinedCaption", "normalCaption", "reverseCaption" })]
        public int CipherRotor3Rev
        {
            get { return cipherRotor3Rev; }
            set
            {
                
                if (value != cipherRotor3Rev)
                {
                    cipherRotorRev[2] = value;
                    cipherRotor3Rev = value;
                    OnPropertyChanged("CipherRotor3Rev");
                }

            }
        }

        #region Rotor choice

        [TaskPane("AnalysisUseRotorICaption", "AnalysisUseRotorITooltip",
            "Cipher3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher3AnalysisUseRotor1
        {
            get { return cipher3AnalysisUseRotor1; }
            set
            {

                if (value != cipher3AnalysisUseRotor1)
                {
                    cipher3AnalysisUseRotor[0] = value;
                    cipher3AnalysisUseRotor1 = value;
                    OnPropertyChanged("Cipher3AnalysisUseRotor1");
                }
            }
        }


        [TaskPane("AnalysisUseRotorIICaption", "Cipher3AnalysisUseRotorIITooltip",
            "Cipher3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher3AnalysisUseRotor2
        {
            get { return cipher3AnalysisUseRotor2; }
            set
            {

                if (value != cipher3AnalysisUseRotor2)
                {
                    cipher3AnalysisUseRotor[1] = value;
                    cipher3AnalysisUseRotor2 = value;
                    OnPropertyChanged("Cipher3AnalysisUseRotor2");
                }
            }
        }


        [TaskPane("AnalysisUseRotorIIICaption", "Cipher3AnalysisUseRotorIIITooltip",
            "Cipher3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher3AnalysisUseRotor3
        {
            get { return cipher3AnalysisUseRotor3; }
            set
            {

                if (value != cipher3AnalysisUseRotor3)
                {
                    cipher3AnalysisUseRotor[2] = value;
                    cipher3AnalysisUseRotor3 = value;
                    OnPropertyChanged("Cipher3AnalysisUseRotor3");
                }
            }
        }


        [TaskPane("AnalysisUseRotorIVCaption", "Cipher3AnalysisUseRotorIVTooltip",
            "Cipher3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher3AnalysisUseRotor4
        {
            get { return cipher3AnalysisUseRotor4; }
            set
            {

                if (value != cipher3AnalysisUseRotor4)
                {
                    cipher3AnalysisUseRotor[3] = value;
                    cipher3AnalysisUseRotor4 = value;
                    OnPropertyChanged("Cipher3AnalysisUseRotor4");
                }
            }
        }



        [TaskPane("AnalysisUseRotorVCaption", "Cipher3AnalysisUseRotorVTooltip",
            "Cipher3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher3AnalysisUseRotor5
        {
            get { return cipher3AnalysisUseRotor5; }
            set
            {

                if (value != cipher3AnalysisUseRotor5)
                {
                    cipher3AnalysisUseRotor[4] = value;
                    cipher3AnalysisUseRotor5 = value;
                    OnPropertyChanged("Cipher3AnalysisUseRotor5");
                }
            }
        }


        [TaskPane("AnalysisUseRotorVICaption", "Cipher3AnalysisUseRotorVITooltip",
            "Cipher3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher3AnalysisUseRotor6
        {
            get { return cipher3AnalysisUseRotor6; }
            set
            {

                if (value != cipher3AnalysisUseRotor6)
                {
                    cipher3AnalysisUseRotor[5] = value;
                    cipher3AnalysisUseRotor6 = value;
                    OnPropertyChanged("Cipher3AnalysisUseRotor6");
                }
            }
        }



        [TaskPane("AnalysisUseRotorVIICaption", "Cipher3AnalysisUseRotorVIITooltip",
            "Cipher3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher3AnalysisUseRotor7
        {
            get { return cipher3AnalysisUseRotor7; }
            set
            {

                if (value != cipher3AnalysisUseRotor7)
                {
                    cipher3AnalysisUseRotor[6] = value;
                    cipher3AnalysisUseRotor7 = value;
                    OnPropertyChanged("Cipher3AnalysisUseRotor7");
                }
            }
        }



        [TaskPane("AnalysisUseRotorVIIICaption", "Cipher3AnalysisUseRotorVIIITooltip",
            "Cipher3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher3AnalysisUseRotor8
        {
            get { return cipher3AnalysisUseRotor8; }
            set
            {

                if (value != cipher3AnalysisUseRotor8)
                {
                    cipher3AnalysisUseRotor[7] = value;
                    cipher3AnalysisUseRotor8 = value;
                    OnPropertyChanged("Cipher3AnalysisUseRotor8");
                }
            }
        }

        [TaskPane("AnalysisUseRotorIXCaption", "Cipher3AnalysisUseRotorVIIITooltip",
            "Cipher3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher3AnalysisUseRotor9
        {
            get { return cipher3AnalysisUseRotor9; }
            set
            {

                if (value != cipher3AnalysisUseRotor9)
                {
                    cipher3AnalysisUseRotor[8] = value;
                    cipher3AnalysisUseRotor9 = value;
                    OnPropertyChanged("Cipher3AnalysisUseRotor9");
                }
            }
        }
        [TaskPane("AnalysisUseRotorXCaption", "Cipher3AnalysisUseRotorVIIITooltip",
            "Cipher3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher3AnalysisUseRotor0
        {
            get { return cipher3AnalysisUseRotor0; }
            set
            {

                if (value != cipher3AnalysisUseRotor0)
                {
                    cipher3AnalysisUseRotor[9] = value;
                    cipher3AnalysisUseRotor0 = value;
                    OnPropertyChanged("Cipher3AnalysisUseRotor0");
                }
            }
        }
        #endregion

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Vier")]
        [TaskPane("Cipher Rotor 4: From", "PlugBoardATooltip", "PositionOptionsGroup", 10, false, ControlType.ComboBox,
            new String[] { "LetterA", "LetterB", "LetterC", "LetterD", "LetterE", "LetterF", "LetterG", "LetterH", "LetterI", "LetterJ", "LetterK", "LetterL", "LetterM", "LetterN", "LetterO", "LetterP", "LetterQ", "LetterR", "LetterS", "LetterT", "LetterU", "LetterV", "LetterW", "LetterX", "LetterY", "LetterZ" })]
        public int CipherRotor4From
        {
            get { return cipherRotor4From; }
            set
            {

                cipherRotor4From = value;
                this.cipherRotorFrom[3] = (int)value;
                OnPropertyChanged("CipherRotor4From");
                

                if (value > cipherRotor4To)
                {
                    CipherRotor4To = value;
                    this.cipherRotorTo[3] = (int)value;


                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Vier")]
        [TaskPane("ToCaption", "PlugBoardBTooltip", "PositionOptionsGroup", 11, false, ControlType.ComboBox,
            new String[] { "LetterA", "LetterB", "LetterC", "LetterD", "LetterE", "LetterF", "LetterG", "LetterH", "LetterI", "LetterJ", "LetterK", "LetterL", "LetterM", "LetterN", "LetterO", "LetterP", "LetterQ", "LetterR", "LetterS", "LetterT", "LetterU", "LetterV", "LetterW", "LetterX", "LetterY", "LetterZ" })]
        public int CipherRotor4To
        {
            get { return cipherRotor4To; }
            set
            {

                cipherRotor4To = value;
                this.cipherRotorTo[3] = (int)value;
                OnPropertyChanged("CipherRotor4To");
                
                if (value < cipherRotor4From)
                {
                    CipherRotor4From = value;
                    this.cipherRotorFrom[3] = (int)value;
                }

            }
        }

        [TaskPane("RevCaption", "PlugBoardBTooltip", "PositionOptionsGroup", 12, false, ControlType.RadioButton, new String[] { "undefinedCaption", "normalCaption", "reverseCaption" })]
        public int CipherRotor4Rev
        {
            get { return cipherRotor4Rev; }
            set
            {
                
                if (value != cipherRotor4Rev)
                {
                    cipherRotorRev[3] = value;
                    cipherRotor4Rev = value;
                    OnPropertyChanged("CipherRotor4Rev");
                }

            }
        }

        #region Rotor choice

        [TaskPane("AnalysisUseRotorICaption", "AnalysisUseRotorITooltip",
            "Cipher4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher4AnalysisUseRotor1
        {
            get { return cipher4AnalysisUseRotor1; }
            set
            {

                if (value != cipher4AnalysisUseRotor1)
                {
                    cipher4AnalysisUseRotor[0] = value;
                    cipher4AnalysisUseRotor1 = value;
                    OnPropertyChanged("Cipher4AnalysisUseRotor1");
                }
            }
        }


        [TaskPane("AnalysisUseRotorIICaption", "Cipher4AnalysisUseRotorIITooltip",
            "Cipher4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher4AnalysisUseRotor2
        {
            get { return cipher4AnalysisUseRotor2; }
            set
            {

                if (value != cipher4AnalysisUseRotor2)
                {
                    cipher4AnalysisUseRotor[1] = value;
                    cipher4AnalysisUseRotor2 = value;
                    OnPropertyChanged("Cipher4AnalysisUseRotor2");
                }
            }
        }


        [TaskPane("AnalysisUseRotorIIICaption", "Cipher4AnalysisUseRotorIIITooltip",
            "Cipher4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher4AnalysisUseRotor3
        {
            get { return cipher4AnalysisUseRotor3; }
            set
            {

                if (value != cipher4AnalysisUseRotor3)
                {
                    cipher4AnalysisUseRotor[2] = value;
                    cipher4AnalysisUseRotor3 = value;
                    OnPropertyChanged("Cipher4AnalysisUseRotor3");
                }
            }
        }


        [TaskPane("AnalysisUseRotorIVCaption", "Cipher4AnalysisUseRotorIVTooltip",
            "Cipher4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher4AnalysisUseRotor4
        {
            get { return cipher4AnalysisUseRotor4; }
            set
            {

                if (value != cipher4AnalysisUseRotor4)
                {
                    cipher4AnalysisUseRotor[3] = value;
                    cipher4AnalysisUseRotor4 = value;
                    OnPropertyChanged("Cipher4AnalysisUseRotor4");
                }
            }
        }



        [TaskPane("AnalysisUseRotorVCaption", "Cipher4AnalysisUseRotorVTooltip",
            "Cipher4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher4AnalysisUseRotor5
        {
            get { return cipher4AnalysisUseRotor5; }
            set
            {

                if (value != cipher4AnalysisUseRotor5)
                {
                    cipher4AnalysisUseRotor[4] = value;
                    cipher4AnalysisUseRotor5 = value;
                    OnPropertyChanged("Cipher4AnalysisUseRotor5");
                }
            }
        }


        [TaskPane("AnalysisUseRotorVICaption", "Cipher4AnalysisUseRotorVITooltip",
            "Cipher4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher4AnalysisUseRotor6
        {
            get { return cipher4AnalysisUseRotor6; }
            set
            {

                if (value != cipher4AnalysisUseRotor6)
                {
                    cipher4AnalysisUseRotor[5] = value;
                    cipher4AnalysisUseRotor6 = value;
                    OnPropertyChanged("Cipher4AnalysisUseRotor6");
                }
            }
        }



        [TaskPane("AnalysisUseRotorVIICaption", "Cipher4AnalysisUseRotorVIITooltip",
            "Cipher4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher4AnalysisUseRotor7
        {
            get { return cipher4AnalysisUseRotor7; }
            set
            {

                if (value != cipher4AnalysisUseRotor7)
                {
                    cipher4AnalysisUseRotor[6] = value;
                    cipher4AnalysisUseRotor7 = value;
                    OnPropertyChanged("Cipher4AnalysisUseRotor7");
                }
            }
        }



        [TaskPane("AnalysisUseRotorVIIICaption", "Cipher4AnalysisUseRotorVIIITooltip",
            "Cipher4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher4AnalysisUseRotor8
        {
            get { return cipher4AnalysisUseRotor8; }
            set
            {

                if (value != cipher4AnalysisUseRotor8)
                {
                    cipher4AnalysisUseRotor[7] = value;
                    cipher4AnalysisUseRotor8 = value;
                    OnPropertyChanged("Cipher4AnalysisUseRotor8");
                }
            }
        }

        [TaskPane("AnalysisUseRotorIXCaption", "Cipher4AnalysisUseRotorVIIITooltip",
            "Cipher4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher4AnalysisUseRotor9
        {
            get { return cipher4AnalysisUseRotor9; }
            set
            {

                if (value != cipher4AnalysisUseRotor9)
                {
                    cipher4AnalysisUseRotor[8] = value;
                    cipher4AnalysisUseRotor9 = value;
                    OnPropertyChanged("Cipher4AnalysisUseRotor9");
                }
            }
        }
        [TaskPane("AnalysisUseRotorXCaption", "Cipher4AnalysisUseRotorVIIITooltip",
            "Cipher4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher4AnalysisUseRotor0
        {
            get { return cipher4AnalysisUseRotor0; }
            set
            {

                if (value != cipher4AnalysisUseRotor0)
                {
                    cipher4AnalysisUseRotor[9] = value;
                    cipher4AnalysisUseRotor0 = value;
                    OnPropertyChanged("Cipher4AnalysisUseRotor0");
                }
            }
        }
        #endregion

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Fünf")]
        [TaskPane("Cipher Rotor 5: From", "PlugBoardATooltip", "PositionOptionsGroup", 13, false, ControlType.ComboBox,
            new String[] { "LetterA", "LetterB", "LetterC", "LetterD", "LetterE", "LetterF", "LetterG", "LetterH", "LetterI", "LetterJ", "LetterK", "LetterL", "LetterM", "LetterN", "LetterO", "LetterP", "LetterQ", "LetterR", "LetterS", "LetterT", "LetterU", "LetterV", "LetterW", "LetterX", "LetterY", "LetterZ" })]
        public int CipherRotor5From
        {
            get { return cipherRotor5From; }
            set
            {

                cipherRotor5From = value;
                this.cipherRotorFrom[4] = (int)value;
                OnPropertyChanged("CipherRotor5From");
                

                if (value > cipherRotor5To)
                {
                    CipherRotor5To = value;
                    this.cipherRotorTo[4] = (int)value;
                    
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Fünf")]
        [TaskPane("ToCaption", "PlugBoardBTooltip", "PositionOptionsGroup", 14, false, ControlType.ComboBox,
            new String[] { "LetterA", "LetterB", "LetterC", "LetterD", "LetterE", "LetterF", "LetterG", "LetterH", "LetterI", "LetterJ", "LetterK", "LetterL", "LetterM", "LetterN", "LetterO", "LetterP", "LetterQ", "LetterR", "LetterS", "LetterT", "LetterU", "LetterV", "LetterW", "LetterX", "LetterY", "LetterZ" })]
        public int CipherRotor5To
        {
            get { return cipherRotor5To; }
            set
            {

                cipherRotor5To = value;
                this.cipherRotorTo[4] = (int)value;
                OnPropertyChanged("CipherRotor5To");
                
                if (value < cipherRotor5From)
                {
                    CipherRotor5From = value;
                    this.cipherRotorFrom[4] = (int)value;
                }

            }
        }

        [TaskPane("RevCaption", "PlugBoardBTooltip", "PositionOptionsGroup", 15, false, ControlType.RadioButton, new String[] { "undefinedCaption", "normalCaption", "reverseCaption" })]
        public int CipherRotor5Rev
        {
            get { return cipherRotor5Rev; }
            set
            {

                if (value != cipherRotor5Rev)
                {
                    cipherRotorRev[4] = value;
                    cipherRotor5Rev = value;
                    OnPropertyChanged("CipherRotor5Rev");
                }

            }
        }

        #region Rotor choice

        [TaskPane("AnalysisUseRotorICaption", "AnalysisUseRotorITooltip",
            "Cipher5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher5AnalysisUseRotor1
        {
            get { return cipher5AnalysisUseRotor1; }
            set
            {

                if (value != cipher5AnalysisUseRotor1)
                {
                    cipher5AnalysisUseRotor[0] = value;
                    cipher5AnalysisUseRotor1 = value;
                    OnPropertyChanged("Cipher5AnalysisUseRotor1");
                }
            }
        }


        [TaskPane("AnalysisUseRotorIICaption", "Cipher5AnalysisUseRotorIITooltip",
            "Cipher5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher5AnalysisUseRotor2
        {
            get { return cipher5AnalysisUseRotor2; }
            set
            {

                if (value != cipher5AnalysisUseRotor2)
                {
                    cipher5AnalysisUseRotor[1] = value;
                    cipher5AnalysisUseRotor2 = value;
                    OnPropertyChanged("Cipher5AnalysisUseRotor2");
                }
            }
        }


        [TaskPane("AnalysisUseRotorIIICaption", "Cipher5AnalysisUseRotorIIITooltip",
            "Cipher5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher5AnalysisUseRotor3
        {
            get { return cipher5AnalysisUseRotor3; }
            set
            {

                if (value != cipher5AnalysisUseRotor3)
                {
                    cipher5AnalysisUseRotor[2] = value;
                    cipher5AnalysisUseRotor3 = value;
                    OnPropertyChanged("Cipher5AnalysisUseRotor3");
                }
            }
        }


        [TaskPane("AnalysisUseRotorIVCaption", "Cipher5AnalysisUseRotorIVTooltip",
            "Cipher5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher5AnalysisUseRotor4
        {
            get { return cipher5AnalysisUseRotor4; }
            set
            {

                if (value != cipher5AnalysisUseRotor4)
                {
                    cipher5AnalysisUseRotor[3] = value;
                    cipher5AnalysisUseRotor4 = value;
                    OnPropertyChanged("Cipher5AnalysisUseRotor4");
                }
            }
        }



        [TaskPane("AnalysisUseRotorVCaption", "Cipher5AnalysisUseRotorVTooltip",
            "Cipher5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher5AnalysisUseRotor5
        {
            get { return cipher5AnalysisUseRotor5; }
            set
            {

                if (value != cipher5AnalysisUseRotor5)
                {
                    cipher5AnalysisUseRotor[4] = value;
                    cipher5AnalysisUseRotor5 = value;
                    OnPropertyChanged("Cipher5AnalysisUseRotor5");
                }
            }
        }


        [TaskPane("AnalysisUseRotorVICaption", "Cipher5AnalysisUseRotorVITooltip",
            "Cipher5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher5AnalysisUseRotor6
        {
            get { return cipher5AnalysisUseRotor6; }
            set
            {

                if (value != cipher5AnalysisUseRotor6)
                {
                    cipher5AnalysisUseRotor[5] = value;
                    cipher5AnalysisUseRotor6 = value;
                    OnPropertyChanged("Cipher5AnalysisUseRotor6");
                }
            }
        }



        [TaskPane("AnalysisUseRotorVIICaption", "Cipher5AnalysisUseRotorVIITooltip",
            "Cipher5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher5AnalysisUseRotor7
        {
            get { return cipher5AnalysisUseRotor7; }
            set
            {

                if (value != cipher5AnalysisUseRotor7)
                {
                    cipher5AnalysisUseRotor[6] = value;
                    cipher5AnalysisUseRotor7 = value;
                    OnPropertyChanged("Cipher5AnalysisUseRotor7");
                }
            }
        }



        [TaskPane("AnalysisUseRotorVIIICaption", "Cipher5AnalysisUseRotorVIIITooltip",
            "Cipher5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher5AnalysisUseRotor8
        {
            get { return cipher5AnalysisUseRotor8; }
            set
            {

                if (value != cipher5AnalysisUseRotor8)
                {
                    cipher5AnalysisUseRotor[7] = value;
                    cipher5AnalysisUseRotor8 = value;
                    OnPropertyChanged("Cipher5AnalysisUseRotor8");
                }
            }
        }

        [TaskPane("AnalysisUseRotorIXCaption", "Cipher5AnalysisUseRotorVIIITooltip",
            "Cipher5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher5AnalysisUseRotor9
        {
            get { return cipher5AnalysisUseRotor9; }
            set
            {

                if (value != cipher5AnalysisUseRotor9)
                {
                    cipher5AnalysisUseRotor[8] = value;
                    cipher5AnalysisUseRotor9 = value;
                    OnPropertyChanged("Cipher5AnalysisUseRotor9");
                }
            }
        }
        [TaskPane("AnalysisUseRotorXCaption", "Cipher5AnalysisUseRotorVIIITooltip",
            "Cipher5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher5AnalysisUseRotor0
        {
            get { return cipher5AnalysisUseRotor0; }
            set
            {

                if (value != cipher5AnalysisUseRotor0)
                {
                    cipher5AnalysisUseRotor[9] = value;
                    cipher5AnalysisUseRotor0 = value;
                    OnPropertyChanged("Cipher5AnalysisUseRotor0");
                }
            }
        }
        #endregion

        #endregion

        public int setKeyspace(String s)
        {

            int sumkeyspace2 = 1;

            int summax = (CipherRotor1To + 1 - CipherRotor1From) *
                         (CipherRotor2To + 1 - CipherRotor2From) *
                         (CipherRotor3To + 1 - CipherRotor3From) *
                         (CipherRotor4To + 1 - CipherRotor4From) *
                         (CipherRotor5To + 1 - CipherRotor5From);

            if (summax == 0)
                summax = 1;


            int[][] controlarr = rotorSettings();

            int[] arr2 = setStartingArr(controlarr);

            var foo = new int[10] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
            IEnumerable<IEnumerable<int>> combis = foo.Combinations(5);

            for (int enumi = 0; enumi < 252; enumi++)
            {
                int[] arr = combis.ElementAt(enumi).ToArray();
                if(checkWithSetting(arr))
                while (NextPermutation(arr, controlarr))
                    {
                        sumkeyspace2++;
                    }
            }
            
            summax *= sumkeyspace2;

            summax *= getWhiteList().Length;

            int[] whitelist = getWhiteList();

            return summax;
        }

        private bool checkWithSetting(int[] arr)
        {
            bool retb = true;


            if (!cipher1AnalysisUseRotor[arr[0] - 1])
            {
                retb = false;
            }
            if (!cipher2AnalysisUseRotor[arr[1] - 1])
            {
                retb = false;
            }
            if (!cipher3AnalysisUseRotor[arr[2] - 1])
            {
                retb = false;
            }
            if (!cipher4AnalysisUseRotor[arr[3] - 1])
            {
                retb = false;
            }
            if (!cipher5AnalysisUseRotor[arr[4] - 1])
            {
                retb = false;
            }
            return retb;
        }

        public int[] getWhiteList()
        {
            int[] getSettings = new[]
                            {
                                CipherRotor1Rev, CipherRotor2Rev, CipherRotor3Rev,
                                CipherRotor4Rev, CipherRotor5Rev
                            };

            List<int> value = new List<int>();

            for (int r = 0; r < 32; r++)
            {
            start:

                if (r == 32)
                    break;
                string bin = GetIntBinaryString(r);
                //reversekey = bin.Replace('1', 'R').Replace('0', ' ');

                bool b = true;
                for (int i = 0; i < bin.Length; i++)
                {
                    if (getSettings[i] == 1 && bin[i] == '1')
                    {
                        r++;
                        goto start;
                    }
                    if (getSettings[i] == 2 && bin[i] == '0')
                    {
                        r++;
                        goto start;
                    }


                }
                value.Add(r);
            }

            int[] ret = new int[value.Count];

            for (int i = 0; i < value.Count; i++)
            {
                ret[i] = value[i];
            }

            return ret;
        }

        private static string GetIntBinaryString(int n)
        {
            var b = new char[5];
            int pos = 4;
            int i = 0;

            while (i < 5)
            {
                if ((n & (1 << i)) != 0)
                {
                    b[pos] = '1';
                }
                else
                {
                    b[pos] = '0';
                }
                pos--;
                i++;
            }
            return new string(b);
        }

        public bool NextPermutation(int[] numList, int[][] controlarr)
        {
            /*
             * 
             * http://stackoverflow.com/questions/11208446/generating-permutations-of-a-set-most-efficiently
             Knuths
             1. Find the largest index j such that a[j] < a[j + 1]. If no such index exists, the permutation is the last permutation.
             2. Find the largest index l such that a[j] < a[l]. Since j + 1 is such an index, l is well defined and satisfies j < l.
             3. Swap a[j] with a[l].
             4. Reverse the sequence from a[j + 1] up to and including the final element a[n].

             */

            Boolean b = true;
            while (b)
            {

                var largestIndex = -1;
                for (var i = numList.Length - 2; i >= 0; i--)
                {
                    if (numList[i] < numList[i + 1])
                    {
                        largestIndex = i;
                        break;
                    }
                }

                if (largestIndex < 0) return false;

                var largestIndex2 = -1;
                for (var i = numList.Length - 1; i >= 0; i--)
                {
                    if (numList[largestIndex] < numList[i])
                    {
                        largestIndex2 = i;
                        break;
                    }
                }

                var tmp = numList[largestIndex];
                numList[largestIndex] = numList[largestIndex2];
                numList[largestIndex2] = tmp;

                for (int i = largestIndex + 1, j = numList.Length - 1; i < j; i++, j--)
                {
                    tmp = numList[i];
                    numList[i] = numList[j];
                    numList[j] = tmp;
                }

                for (int i = 0; i < numList.Length; i++)
                {
                    if (!controlarr[i].Contains(numList[i]))
                    {
                        break;
                    }
                    if (i == numList.Length - 1)
                    {
                        b = false;
                    }

                }
            }
            return true;
        }

        public int[] setStartingArr(int[][] indexarr)
        {

            int[] arr2 = new int[indexarr.GetLength(0)];
            for (int i = 0; i < indexarr.GetLength(0); i++)
            {
                arr2[i] = i + 1;
            }

            for (int i = 0; i < arr2.Length; i++)
            {
                for (int ix = 0; ix < indexarr[i].Length; ix++)
                {
                    if (indexarr[i][ix] != -1)
                    {
                        Boolean notbefore = false;
                        for (int j = 0; j < i; j++)
                        {

                            if (arr2[j] == indexarr[i][ix])
                            {
                                break;
                            }
                            if (j == i - 1)
                            {
                                notbefore = true;
                            }

                        }
                        if (notbefore)
                        {
                            arr2[i] = indexarr[i][ix];
                            break;
                        }
                    }
                }
            }
            return arr2;
        }

        public int[][] rotorSettings()
        {
            int[][] value = new int[][]
                                {
                                    new int[]
                                        {
                                            
                                            Cipher1AnalysisUseRotor1 ? 1 : -1,
                                            Cipher1AnalysisUseRotor2 ? 2 : -1,
                                            Cipher1AnalysisUseRotor3 ? 3 : -1,
                                            Cipher1AnalysisUseRotor4 ? 4 : -1,
                                            Cipher1AnalysisUseRotor5 ? 5 : -1,
                                            Cipher1AnalysisUseRotor6 ? 6 : -1,
                                            Cipher1AnalysisUseRotor7 ? 7 : -1,
                                            Cipher1AnalysisUseRotor8 ? 8 : -1,
                                            Cipher1AnalysisUseRotor9 ? 9 : -1,
                                            Cipher1AnalysisUseRotor0 ? 10 : -1,
                                        },
                                    new int[]
                                        {
                                            Cipher2AnalysisUseRotor1 ? 1 : -1,
                                            Cipher2AnalysisUseRotor2 ? 2 : -1,
                                            Cipher2AnalysisUseRotor3 ? 3 : -1,
                                            Cipher2AnalysisUseRotor4 ? 4 : -1,
                                            Cipher2AnalysisUseRotor5 ? 5 : -1,
                                            Cipher2AnalysisUseRotor6 ? 6 : -1,
                                            Cipher2AnalysisUseRotor7 ? 7 : -1,
                                            Cipher2AnalysisUseRotor8 ? 8 : -1,
                                            Cipher2AnalysisUseRotor9 ? 9 : -1,
                                            Cipher2AnalysisUseRotor0 ? 10: -1
                                        },
                                    new int[]
                                        {
                                            
                                            Cipher3AnalysisUseRotor1 ? 1 : -1,
                                            Cipher3AnalysisUseRotor2 ? 2 : -1,
                                            Cipher3AnalysisUseRotor3 ? 3 : -1,
                                            Cipher3AnalysisUseRotor4 ? 4 : -1,
                                            Cipher3AnalysisUseRotor5 ? 5 : -1,
                                            Cipher3AnalysisUseRotor6 ? 6 : -1,
                                            Cipher3AnalysisUseRotor7 ? 7 : -1,
                                            Cipher3AnalysisUseRotor8 ? 8 : -1,
                                            Cipher3AnalysisUseRotor9 ? 9 : -1,
                                            Cipher3AnalysisUseRotor0 ? 10: -1
                                        },
                                    new int[]
                                        {
                                            
                                            Cipher4AnalysisUseRotor1 ? 1 : -1,
                                            Cipher4AnalysisUseRotor2 ? 2 : -1,
                                            Cipher4AnalysisUseRotor3 ? 3 : -1,
                                            Cipher4AnalysisUseRotor4 ? 4 : -1,
                                            Cipher4AnalysisUseRotor5 ? 5 : -1,
                                            Cipher4AnalysisUseRotor6 ? 6 : -1,
                                            Cipher4AnalysisUseRotor7 ? 7 : -1,
                                            Cipher4AnalysisUseRotor8 ? 8 : -1,
                                            Cipher4AnalysisUseRotor9 ? 9 : -1,
                                            Cipher4AnalysisUseRotor0 ? 10: -1
                                        },
                                    new int[]
                                        {
                                            
                                            Cipher5AnalysisUseRotor1 ? 1 : -1,
                                            Cipher5AnalysisUseRotor2 ? 2 : -1,
                                            Cipher5AnalysisUseRotor3 ? 3 : -1,
                                            Cipher5AnalysisUseRotor4 ? 4 : -1,
                                            Cipher5AnalysisUseRotor5 ? 5 : -1,
                                            Cipher5AnalysisUseRotor6 ? 6 : -1,
                                            Cipher5AnalysisUseRotor7 ? 7 : -1,
                                            Cipher5AnalysisUseRotor8 ? 8 : -1,
                                            Cipher5AnalysisUseRotor9 ? 9 : -1,
                                            Cipher5AnalysisUseRotor0 ? 10: -1
                                        }
                                };


            return value;
        }

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}
