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
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Windows;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

namespace SigabaBruteforce
{
    public class SigabaBruteforceSettings : ISettings
    {
        #region Private Variables

        private enum OrientationChecks { Normal = 0, Reverse = 1, Both = 2 };

        class RotorAnalysisOptions
        {
            public int size, setsize, from, to, orientations;
            public bool showOptions;
            public bool[] rotorUsage;
            public string name;
            public int index;

            public RotorAnalysisOptions(int index, int size, int setsize, string name)
            {
                this.index = index;
                this.size = size;
                this.setsize = setsize;
                this.name = name;

                from = 0;
                to = size - 1;
                orientations = 2;
                showOptions = true;
                rotorUsage = Enumerable.Repeat(true, setsize).ToArray();
            }
        };

        RotorAnalysisOptions[] CipherRotorOptions = Enumerable.Range(0, 5).Select(i => new RotorAnalysisOptions(i + 1, 26, 10, "Cipher")).ToArray();
        RotorAnalysisOptions[] ControlRotorOptions = Enumerable.Range(0, 5).Select(i => new RotorAnalysisOptions(i + 1, 26, 10, "Control")).ToArray();
        RotorAnalysisOptions[] IndexRotorOptions = Enumerable.Range(0, 5).Select(i => new RotorAnalysisOptions(i + 1, 10, 5, "Index")).ToArray();

        private string keySpace = "";

        #endregion

        public SigabaBruteforceSettings()
        {
            setSettingsVisibility();
        }

        public void Initialize()
        {
            setSettingsVisibility();
        }

        #region TaskPane Settings

        #region Helper functions

        private void setShowOptions(RotorAnalysisOptions o, bool value)
        {
            if (o.showOptions != value)
            {
                o.showOptions = value;
                OnPropertyChanged(o.name + "Rotor" + o.index + "Rotors");
                setSettingsVisibility();
            }
        }

        private void setFrom(RotorAnalysisOptions o, int value)
        {
            o.from = value;
            if (o.to < value) o.to = value;
            OnPropertyChanged(o.name + "Rotor" + o.index + "From");
            setSettingsVisibility();
        }

        private void setTo(RotorAnalysisOptions o, int value)
        {
            o.to = value;
            if (o.from > value) o.from = value;
            OnPropertyChanged(o.name + "Rotor" + o.index + "To");
            setSettingsVisibility();
        }

        private void setRev(RotorAnalysisOptions o, int value)
        {
            if (o.orientations != value)
            {
                o.orientations = value;
                OnPropertyChanged(o.name + "Rotor" + o.index + "Rev");
            }
        }

        private void setRotorUsage(RotorAnalysisOptions o, int i, bool value)
        {
            if (o.rotorUsage[i] != value)
            {
                o.rotorUsage[i] = value;
                OnPropertyChanged(o.name + o.index + "AnalysisUseRotor" + ((i + 1) % 10));
            }
        }

        #endregion

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "")]
        [TaskPane("KeyspaceCaption", "KeyspaceTooltip", "", 0, false, ControlType.TextBoxReadOnly, "", null)]
        public string KeySpace
        {
            get
            {
                return keySpace;
            }
            private set
            {
                keySpace = value;
                OnPropertyChanged("KeySpace");
            }
        }

        [TaskPane("CalculateKeypaceCaption", "CalculateKeypaceTooltip", "", 1, false, ControlType.Button)]
        public void CalculateKeySpace()
        {
            KeySpace = getLogKeyspace().ToString("0.0") + " bits";
        }

        #region on/off cipherrotor

        [TaskPane("AnalyzeCipherRotors1Caption", "AnalyzeRotorsTooltip", "RotorAnalysisGroup", 1, false, ControlType.CheckBox, "", null)]
        public bool CipherRotor1Rotors
        {
            get { return CipherRotorOptions[0].showOptions; }
            set { setShowOptions(CipherRotorOptions[0], value); }
        }

        [TaskPane("AnalyzeCipherRotors2Caption", "AnalyzeRotorsTooltip", "RotorAnalysisGroup", 1, false, ControlType.CheckBox, "", null)]
        public bool CipherRotor2Rotors
        {
            get { return CipherRotorOptions[1].showOptions; }
            set { setShowOptions(CipherRotorOptions[1], value); }
        }

        [TaskPane("AnalyzeCipherRotors3Caption", "AnalyzeRotorsTooltip", "RotorAnalysisGroup", 1, false, ControlType.CheckBox, "", null)]
        public bool CipherRotor3Rotors
        {
            get { return CipherRotorOptions[2].showOptions; }
            set { setShowOptions(CipherRotorOptions[2], value); }
        }

        [TaskPane("AnalyzeCipherRotors4Caption", "AnalyzeRotorsTooltip", "RotorAnalysisGroup", 1, false, ControlType.CheckBox, "", null)]
        public bool CipherRotor4Rotors
        {
            get { return CipherRotorOptions[3].showOptions; }
            set { setShowOptions(CipherRotorOptions[3], value); }
        }

        [TaskPane("AnalyzeCipherRotors5Caption", "AnalyzeRotorsTooltip", "RotorAnalysisGroup", 1, false, ControlType.CheckBox, "", null)]
        public bool CipherRotor5Rotors
        {
            get { return CipherRotorOptions[4].showOptions; }
            set { setShowOptions(CipherRotorOptions[4], value); }
        }

        #endregion

        #region on/off control rotor

        [TaskPane("AnalyzeControlRotors1Caption", "AnalyzeRotorsTooltip", "RotorAnalysisGroup", 1, false, ControlType.CheckBox, "", null)]
        public bool ControlRotor1Rotors
        {
            get { return ControlRotorOptions[0].showOptions; }
            set { setShowOptions(ControlRotorOptions[0], value); }
        }

        [TaskPane("AnalyzeControlRotors2Caption", "AnalyzeRotorsTooltip", "RotorAnalysisGroup", 1, false, ControlType.CheckBox, "", null)]
        public bool ControlRotor2Rotors
        {
            get { return ControlRotorOptions[1].showOptions; }
            set { setShowOptions(ControlRotorOptions[1], value); }
        }

        [TaskPane("AnalyzeControlRotors3Caption", "AnalyzeRotorsTooltip", "RotorAnalysisGroup", 1, false, ControlType.CheckBox, "", null)]
        public bool ControlRotor3Rotors
        {
            get { return ControlRotorOptions[2].showOptions; }
            set { setShowOptions(ControlRotorOptions[2], value); }
        }

        [TaskPane("AnalyzeControlRotors4Caption", "AnalyzeRotorsTooltip", "RotorAnalysisGroup", 1, false, ControlType.CheckBox, "", null)]
        public bool ControlRotor4Rotors
        {
            get { return ControlRotorOptions[3].showOptions; }
            set { setShowOptions(ControlRotorOptions[3], value); }
        }

        [TaskPane("AnalyzeControlRotors5Caption", "AnalyzeRotorsTooltip", "RotorAnalysisGroup", 1, false, ControlType.CheckBox, "", null)]
        public bool ControlRotor5Rotors
        {
            get { return ControlRotorOptions[4].showOptions; }
            set { setShowOptions(ControlRotorOptions[4], value); }
        }

        #endregion

        #region on/off indexrotor

        [TaskPane("AnalyzeIndexRotors1Caption", "AnalyzeRotorsTooltip", "RotorAnalysisGroup", 1, false, ControlType.CheckBox, "", null)]
        public bool IndexRotor1Rotors
        {
            get { return IndexRotorOptions[0].showOptions; }
            set { setShowOptions(IndexRotorOptions[0], value); }
        }

        [TaskPane("AnalyzeIndexRotors2Caption", "AnalyzeRotorsTooltip", "RotorAnalysisGroup", 1, false, ControlType.CheckBox, "", null)]
        public bool IndexRotor2Rotors
        {
            get { return IndexRotorOptions[1].showOptions; }
            set { setShowOptions(IndexRotorOptions[1], value); }
        }

        [TaskPane("AnalyzeIndexRotors3Caption", "AnalyzeRotorsTooltip", "RotorAnalysisGroup", 1, false, ControlType.CheckBox, "", null)]
        public bool IndexRotor3Rotors
        {
            get { return IndexRotorOptions[2].showOptions; }
            set { setShowOptions(IndexRotorOptions[2], value); }
        }

        [TaskPane("AnalyzeIndexRotors4Caption", "AnalyzeRotorsTooltip", "RotorAnalysisGroup", 1, false, ControlType.CheckBox, "", null)]
        public bool IndexRotor4Rotors
        {
            get { return IndexRotorOptions[3].showOptions; }
            set { setShowOptions(IndexRotorOptions[3], value); }
        }

        [TaskPane("AnalyzeIndexRotors5Caption", "AnalyzeRotorsTooltip", "RotorAnalysisGroup", 1, false, ControlType.CheckBox, "", null)]
        public bool IndexRotor5Rotors
        {
            get { return IndexRotorOptions[4].showOptions; }
            set { setShowOptions(IndexRotorOptions[4], value); }
        }

        #endregion

        #region Cipher Bank

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Eins")]
        [TaskPane("CipherRotor1FromCaption", "FromTooltip", "PositionOptionsGroup", 1, false, ControlType.ComboBox, new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int CipherRotor1From
        {
            get { return CipherRotorOptions[0].from; }
            set { setFrom(CipherRotorOptions[0], value); }
        }


        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Eins")]
        [TaskPane("ToCaption", "ToTooltip", "PositionOptionsGroup", 2, false, ControlType.ComboBox, new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int CipherRotor1To
        {
            get { return CipherRotorOptions[0].to; }
            set { setTo(CipherRotorOptions[0], value); }
        }

        [TaskPane("RevCaption", "RevTooltip", "PositionOptionsGroup", 3, false, ControlType.RadioButton, new String[] { "OriNormalCaption", "OriReverseCaption", "OriBothCaption" })]
        public int CipherRotor1Rev
        {
            get { return CipherRotorOptions[0].orientations; }
            set { setRev(CipherRotorOptions[0], value); }
        }

        #region Rotor choice

        [TaskPane("AnalysisUseRotor1Caption", "Cipher1AnalysisUseRotorTooltip", "Cipher1AnalysisOptionsGroup", 1, false, ControlType.CheckBox, "", null)]
        public bool Cipher1AnalysisUseRotor1
        {
            get { return CipherRotorOptions[0].rotorUsage[0]; }
            set { setRotorUsage(CipherRotorOptions[0], 0, value); }
        }

        [TaskPane("AnalysisUseRotor2Caption", "Cipher1AnalysisUseRotorTooltip", "Cipher1AnalysisOptionsGroup", 2, false, ControlType.CheckBox, "", null)]
        public bool Cipher1AnalysisUseRotor2
        {
            get { return CipherRotorOptions[0].rotorUsage[1]; }
            set { setRotorUsage(CipherRotorOptions[0], 1, value); }
        }

        [TaskPane("AnalysisUseRotor3Caption", "Cipher1AnalysisUseRotorTooltip", "Cipher1AnalysisOptionsGroup", 3, false, ControlType.CheckBox, "", null)]
        public bool Cipher1AnalysisUseRotor3
        {
            get { return CipherRotorOptions[0].rotorUsage[2]; }
            set { setRotorUsage(CipherRotorOptions[0], 2, value); }
        }

        [TaskPane("AnalysisUseRotor4Caption", "Cipher1AnalysisUseRotorTooltip", "Cipher1AnalysisOptionsGroup", 4, false, ControlType.CheckBox, "", null)]
        public bool Cipher1AnalysisUseRotor4
        {
            get { return CipherRotorOptions[0].rotorUsage[3]; }
            set { setRotorUsage(CipherRotorOptions[0], 3, value); }
        }

        [TaskPane("AnalysisUseRotor5Caption", "Cipher1AnalysisUseRotorTooltip", "Cipher1AnalysisOptionsGroup", 5, false, ControlType.CheckBox, "", null)]
        public bool Cipher1AnalysisUseRotor5
        {
            get { return CipherRotorOptions[0].rotorUsage[4]; }
            set { setRotorUsage(CipherRotorOptions[0], 4, value); }
        }

        [TaskPane("AnalysisUseRotor6Caption", "Cipher1AnalysisUseRotorTooltip", "Cipher1AnalysisOptionsGroup", 6, false, ControlType.CheckBox, "", null)]
        public bool Cipher1AnalysisUseRotor6
        {
            get { return CipherRotorOptions[0].rotorUsage[5]; }
            set { setRotorUsage(CipherRotorOptions[0], 5, value); }
        }

        [TaskPane("AnalysisUseRotor7Caption", "Cipher1AnalysisUseRotorTooltip", "Cipher1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher1AnalysisUseRotor7
        {
            get { return CipherRotorOptions[0].rotorUsage[6]; }
            set { setRotorUsage(CipherRotorOptions[0], 6, value); }
        }

        [TaskPane("AnalysisUseRotor8Caption", "Cipher1AnalysisUseRotorTooltip", "Cipher1AnalysisOptionsGroup", 8, false, ControlType.CheckBox, "", null)]
        public bool Cipher1AnalysisUseRotor8
        {
            get { return CipherRotorOptions[0].rotorUsage[7]; }
            set { setRotorUsage(CipherRotorOptions[0], 7, value); }
        }

        [TaskPane("AnalysisUseRotor9Caption", "Cipher1AnalysisUseRotorTooltip", "Cipher1AnalysisOptionsGroup", 9, false, ControlType.CheckBox, "", null)]
        public bool Cipher1AnalysisUseRotor9
        {
            get { return CipherRotorOptions[0].rotorUsage[8]; }
            set { setRotorUsage(CipherRotorOptions[0], 8, value); }
        }

        [TaskPane("AnalysisUseRotor10Caption", "Cipher1AnalysisUseRotorTooltip", "Cipher1AnalysisOptionsGroup", 10, false, ControlType.CheckBox, "", null)]
        public bool Cipher1AnalysisUseRotor0
        {
            get { return CipherRotorOptions[0].rotorUsage[9]; }
            set { setRotorUsage(CipherRotorOptions[0], 9, value); }
        }

        #endregion

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("CipherRotor2FromCaption", "FromTooltip", "PositionOptionsGroup", 4, false, ControlType.ComboBox, new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int CipherRotor2From
        {
            get { return CipherRotorOptions[1].from; }
            set { setFrom(CipherRotorOptions[1], value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("ToCaption", "ToTooltip", "PositionOptionsGroup", 5, false, ControlType.ComboBox, new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int CipherRotor2To
        {
            get { return CipherRotorOptions[1].to; }
            set { setTo(CipherRotorOptions[1], value); }
        }

        [TaskPane("RevCaption", "RevTooltip", "PositionOptionsGroup", 6, false, ControlType.RadioButton, new String[] { "OriNormalCaption", "OriReverseCaption", "OriBothCaption" })]
        public int CipherRotor2Rev
        {
            get { return CipherRotorOptions[1].orientations; }
            set { setRev(CipherRotorOptions[1], value); }
        }

        #region Rotor choice

        [TaskPane("AnalysisUseRotor1Caption", "Cipher2AnalysisUseRotorTooltip", "Cipher2AnalysisOptionsGroup", 1, false, ControlType.CheckBox, "", null)]
        public bool Cipher2AnalysisUseRotor1
        {
            get { return CipherRotorOptions[1].rotorUsage[0]; }
            set { setRotorUsage(CipherRotorOptions[1], 0, value); }
        }

        [TaskPane("AnalysisUseRotor2Caption", "Cipher2AnalysisUseRotorTooltip", "Cipher2AnalysisOptionsGroup", 2, false, ControlType.CheckBox, "", null)]
        public bool Cipher2AnalysisUseRotor2
        {
            get { return CipherRotorOptions[1].rotorUsage[1]; }
            set { setRotorUsage(CipherRotorOptions[1], 1, value); }
        }

        [TaskPane("AnalysisUseRotor3Caption", "Cipher2AnalysisUseRotorTooltip", "Cipher2AnalysisOptionsGroup", 3, false, ControlType.CheckBox, "", null)]
        public bool Cipher2AnalysisUseRotor3
        {
            get { return CipherRotorOptions[1].rotorUsage[2]; }
            set { setRotorUsage(CipherRotorOptions[1], 2, value); }
        }

        [TaskPane("AnalysisUseRotor4Caption", "Cipher2AnalysisUseRotorTooltip", "Cipher2AnalysisOptionsGroup", 4, false, ControlType.CheckBox, "", null)]
        public bool Cipher2AnalysisUseRotor4
        {
            get { return CipherRotorOptions[1].rotorUsage[3]; }
            set { setRotorUsage(CipherRotorOptions[1], 3, value); }
        }

        [TaskPane("AnalysisUseRotor5Caption", "Cipher2AnalysisUseRotorTooltip", "Cipher2AnalysisOptionsGroup", 5, false, ControlType.CheckBox, "", null)]
        public bool Cipher2AnalysisUseRotor5
        {
            get { return CipherRotorOptions[1].rotorUsage[4]; }
            set { setRotorUsage(CipherRotorOptions[1], 4, value); }
        }

        [TaskPane("AnalysisUseRotor6Caption", "Cipher2AnalysisUseRotorTooltip", "Cipher2AnalysisOptionsGroup", 6, false, ControlType.CheckBox, "", null)]
        public bool Cipher2AnalysisUseRotor6
        {
            get { return CipherRotorOptions[1].rotorUsage[5]; }
            set { setRotorUsage(CipherRotorOptions[1], 5, value); }
        }

        [TaskPane("AnalysisUseRotor7Caption", "Cipher2AnalysisUseRotorTooltip", "Cipher2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher2AnalysisUseRotor7
        {
            get { return CipherRotorOptions[1].rotorUsage[6]; }
            set { setRotorUsage(CipherRotorOptions[1], 6, value); }
        }

        [TaskPane("AnalysisUseRotor8Caption", "Cipher2AnalysisUseRotorTooltip", "Cipher2AnalysisOptionsGroup", 8, false, ControlType.CheckBox, "", null)]
        public bool Cipher2AnalysisUseRotor8
        {
            get { return CipherRotorOptions[1].rotorUsage[7]; }
            set { setRotorUsage(CipherRotorOptions[1], 7, value); }
        }

        [TaskPane("AnalysisUseRotor9Caption", "Cipher2AnalysisUseRotorTooltip", "Cipher2AnalysisOptionsGroup", 9, false, ControlType.CheckBox, "", null)]
        public bool Cipher2AnalysisUseRotor9
        {
            get { return CipherRotorOptions[1].rotorUsage[8]; }
            set { setRotorUsage(CipherRotorOptions[1], 8, value); }
        }

        [TaskPane("AnalysisUseRotor10Caption", "Cipher2AnalysisUseRotorTooltip", "Cipher2AnalysisOptionsGroup", 10, false, ControlType.CheckBox, "", null)]
        public bool Cipher2AnalysisUseRotor0
        {
            get { return CipherRotorOptions[1].rotorUsage[9]; }
            set { setRotorUsage(CipherRotorOptions[1], 9, value); }
        }

        #endregion

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Drei")]
        [TaskPane("CipherRotor3FromCaption", "FromTooltip", "PositionOptionsGroup", 7, false, ControlType.ComboBox, new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int CipherRotor3From
        {
            get { return CipherRotorOptions[2].from; }
            set { setFrom(CipherRotorOptions[2], value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Drei")]
        [TaskPane("ToCaption", "ToTooltip", "PositionOptionsGroup", 8, false, ControlType.ComboBox, new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int CipherRotor3To
        {
            get { return CipherRotorOptions[2].to; }
            set { setTo(CipherRotorOptions[2], value); }
        }

        [TaskPane("RevCaption", "RevTooltip", "PositionOptionsGroup", 9, false, ControlType.RadioButton, new String[] { "OriNormalCaption", "OriReverseCaption", "OriBothCaption" })]
        public int CipherRotor3Rev
        {
            get { return CipherRotorOptions[2].orientations; }
            set { setRev(CipherRotorOptions[2], value); }
        }

        #region Rotor choice

        [TaskPane("AnalysisUseRotor1Caption", "Cipher3AnalysisUseRotorTooltip", "Cipher3AnalysisOptionsGroup", 1, false, ControlType.CheckBox, "", null)]
        public bool Cipher3AnalysisUseRotor1
        {
            get { return CipherRotorOptions[2].rotorUsage[0]; }
            set { setRotorUsage(CipherRotorOptions[2], 0, value); }
        }

        [TaskPane("AnalysisUseRotor2Caption", "Cipher3AnalysisUseRotorTooltip", "Cipher3AnalysisOptionsGroup", 2, false, ControlType.CheckBox, "", null)]
        public bool Cipher3AnalysisUseRotor2
        {
            get { return CipherRotorOptions[2].rotorUsage[1]; }
            set { setRotorUsage(CipherRotorOptions[2], 1, value); }
        }

        [TaskPane("AnalysisUseRotor3Caption", "Cipher3AnalysisUseRotorTooltip", "Cipher3AnalysisOptionsGroup", 3, false, ControlType.CheckBox, "", null)]
        public bool Cipher3AnalysisUseRotor3
        {
            get { return CipherRotorOptions[2].rotorUsage[2]; }
            set { setRotorUsage(CipherRotorOptions[2], 2, value); }
        }

        [TaskPane("AnalysisUseRotor4Caption", "Cipher3AnalysisUseRotorTooltip", "Cipher3AnalysisOptionsGroup", 4, false, ControlType.CheckBox, "", null)]
        public bool Cipher3AnalysisUseRotor4
        {
            get { return CipherRotorOptions[2].rotorUsage[3]; }
            set { setRotorUsage(CipherRotorOptions[2], 3, value); }
        }

        [TaskPane("AnalysisUseRotor5Caption", "Cipher3AnalysisUseRotorTooltip", "Cipher3AnalysisOptionsGroup", 5, false, ControlType.CheckBox, "", null)]
        public bool Cipher3AnalysisUseRotor5
        {
            get { return CipherRotorOptions[2].rotorUsage[4]; }
            set { setRotorUsage(CipherRotorOptions[2], 4, value); }
        }

        [TaskPane("AnalysisUseRotor6Caption", "Cipher3AnalysisUseRotorTooltip", "Cipher3AnalysisOptionsGroup", 6, false, ControlType.CheckBox, "", null)]
        public bool Cipher3AnalysisUseRotor6
        {
            get { return CipherRotorOptions[2].rotorUsage[5]; }
            set { setRotorUsage(CipherRotorOptions[2], 5, value); }
        }

        [TaskPane("AnalysisUseRotor7Caption", "Cipher3AnalysisUseRotorTooltip", "Cipher3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher3AnalysisUseRotor7
        {
            get { return CipherRotorOptions[2].rotorUsage[6]; }
            set { setRotorUsage(CipherRotorOptions[2], 6, value); }
        }

        [TaskPane("AnalysisUseRotor8Caption", "Cipher3AnalysisUseRotorTooltip", "Cipher3AnalysisOptionsGroup", 8, false, ControlType.CheckBox, "", null)]
        public bool Cipher3AnalysisUseRotor8
        {
            get { return CipherRotorOptions[2].rotorUsage[7]; }
            set { setRotorUsage(CipherRotorOptions[2], 7, value); }
        }

        [TaskPane("AnalysisUseRotor9Caption", "Cipher3AnalysisUseRotorTooltip", "Cipher3AnalysisOptionsGroup", 9, false, ControlType.CheckBox, "", null)]
        public bool Cipher3AnalysisUseRotor9
        {
            get { return CipherRotorOptions[2].rotorUsage[8]; }
            set { setRotorUsage(CipherRotorOptions[2], 8, value); }
        }

        [TaskPane("AnalysisUseRotor10Caption", "Cipher3AnalysisUseRotorTooltip", "Cipher3AnalysisOptionsGroup", 10, false, ControlType.CheckBox, "", null)]
        public bool Cipher3AnalysisUseRotor0
        {
            get { return CipherRotorOptions[2].rotorUsage[9]; }
            set { setRotorUsage(CipherRotorOptions[2], 9, value); }
        }

        #endregion

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Vier")]
        [TaskPane("CipherRotor4FromCaption", "FromTooltip", "PositionOptionsGroup", 10, false, ControlType.ComboBox, new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int CipherRotor4From
        {
            get { return CipherRotorOptions[3].from; }
            set { setFrom(CipherRotorOptions[3], value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Vier")]
        [TaskPane("ToCaption", "ToTooltip", "PositionOptionsGroup", 11, false, ControlType.ComboBox, new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int CipherRotor4To
        {
            get { return CipherRotorOptions[3].to; }
            set { setTo(CipherRotorOptions[3], value); }
        }

        [TaskPane("RevCaption", "RevTooltip", "PositionOptionsGroup", 12, false, ControlType.RadioButton, new String[] { "OriNormalCaption", "OriReverseCaption", "OriBothCaption" })]
        public int CipherRotor4Rev
        {
            get { return CipherRotorOptions[3].orientations; }
            set { setRev(CipherRotorOptions[3], value); }
        }

        #region Rotor choice

        [TaskPane("AnalysisUseRotor1Caption", "Cipher4AnalysisUseRotorTooltip", "Cipher4AnalysisOptionsGroup", 1, false, ControlType.CheckBox, "", null)]
        public bool Cipher4AnalysisUseRotor1
        {
            get { return CipherRotorOptions[3].rotorUsage[0]; }
            set { setRotorUsage(CipherRotorOptions[3], 0, value); }
        }

        [TaskPane("AnalysisUseRotor2Caption", "Cipher4AnalysisUseRotorTooltip", "Cipher4AnalysisOptionsGroup", 2, false, ControlType.CheckBox, "", null)]
        public bool Cipher4AnalysisUseRotor2
        {
            get { return CipherRotorOptions[3].rotorUsage[1]; }
            set { setRotorUsage(CipherRotorOptions[3], 1, value); }
        }

        [TaskPane("AnalysisUseRotor3Caption", "Cipher4AnalysisUseRotorTooltip", "Cipher4AnalysisOptionsGroup", 3, false, ControlType.CheckBox, "", null)]
        public bool Cipher4AnalysisUseRotor3
        {
            get { return CipherRotorOptions[3].rotorUsage[2]; }
            set { setRotorUsage(CipherRotorOptions[3], 2, value); }
        }

        [TaskPane("AnalysisUseRotor4Caption", "Cipher4AnalysisUseRotorTooltip", "Cipher4AnalysisOptionsGroup", 4, false, ControlType.CheckBox, "", null)]
        public bool Cipher4AnalysisUseRotor4
        {
            get { return CipherRotorOptions[3].rotorUsage[3]; }
            set { setRotorUsage(CipherRotorOptions[3], 3, value); }
        }

        [TaskPane("AnalysisUseRotor5Caption", "Cipher4AnalysisUseRotorTooltip", "Cipher4AnalysisOptionsGroup", 5, false, ControlType.CheckBox, "", null)]
        public bool Cipher4AnalysisUseRotor5
        {
            get { return CipherRotorOptions[3].rotorUsage[4]; }
            set { setRotorUsage(CipherRotorOptions[3], 4, value); }
        }

        [TaskPane("AnalysisUseRotor6Caption", "Cipher4AnalysisUseRotorTooltip", "Cipher4AnalysisOptionsGroup", 6, false, ControlType.CheckBox, "", null)]
        public bool Cipher4AnalysisUseRotor6
        {
            get { return CipherRotorOptions[3].rotorUsage[5]; }
            set { setRotorUsage(CipherRotorOptions[3], 5, value); }
        }

        [TaskPane("AnalysisUseRotor7Caption", "Cipher4AnalysisUseRotorTooltip", "Cipher4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher4AnalysisUseRotor7
        {
            get { return CipherRotorOptions[3].rotorUsage[6]; }
            set { setRotorUsage(CipherRotorOptions[3], 6, value); }
        }

        [TaskPane("AnalysisUseRotor8Caption", "Cipher4AnalysisUseRotorTooltip", "Cipher4AnalysisOptionsGroup", 8, false, ControlType.CheckBox, "", null)]
        public bool Cipher4AnalysisUseRotor8
        {
            get { return CipherRotorOptions[3].rotorUsage[7]; }
            set { setRotorUsage(CipherRotorOptions[3], 7, value); }
        }

        [TaskPane("AnalysisUseRotor9Caption", "Cipher4AnalysisUseRotorTooltip", "Cipher4AnalysisOptionsGroup", 9, false, ControlType.CheckBox, "", null)]
        public bool Cipher4AnalysisUseRotor9
        {
            get { return CipherRotorOptions[3].rotorUsage[8]; }
            set { setRotorUsage(CipherRotorOptions[3], 8, value); }
        }

        [TaskPane("AnalysisUseRotor10Caption", "Cipher4AnalysisUseRotorTooltip", "Cipher4AnalysisOptionsGroup", 10, false, ControlType.CheckBox, "", null)]
        public bool Cipher4AnalysisUseRotor0
        {
            get { return CipherRotorOptions[3].rotorUsage[9]; }
            set { setRotorUsage(CipherRotorOptions[3], 9, value); }
        }

        #endregion

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Fünf")]
        [TaskPane("CipherRotor5FromCaption", "FromTooltip", "PositionOptionsGroup", 13, false, ControlType.ComboBox, new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int CipherRotor5From
        {
            get { return CipherRotorOptions[4].from; }
            set { setFrom(CipherRotorOptions[4], value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Fünf")]
        [TaskPane("ToCaption", "ToTooltip", "PositionOptionsGroup", 14, false, ControlType.ComboBox, new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int CipherRotor5To
        {
            get { return CipherRotorOptions[4].to; }
            set { setTo(CipherRotorOptions[4], value); }
        }

        [TaskPane("RevCaption", "RevTooltip", "PositionOptionsGroup", 15, false, ControlType.RadioButton, new String[] { "OriNormalCaption", "OriReverseCaption", "OriBothCaption" })]
        public int CipherRotor5Rev
        {
            get { return CipherRotorOptions[4].orientations; }
            set { setRev(CipherRotorOptions[4], value); }
        }

        #region Rotor choice

        [TaskPane("AnalysisUseRotor1Caption", "Cipher5AnalysisUseRotorTooltip", "Cipher5AnalysisOptionsGroup", 1, false, ControlType.CheckBox, "", null)]
        public bool Cipher5AnalysisUseRotor1
        {
            get { return CipherRotorOptions[4].rotorUsage[0]; }
            set { setRotorUsage(CipherRotorOptions[4], 0, value); }
        }

        [TaskPane("AnalysisUseRotor2Caption", "Cipher5AnalysisUseRotorTooltip", "Cipher5AnalysisOptionsGroup", 2, false, ControlType.CheckBox, "", null)]
        public bool Cipher5AnalysisUseRotor2
        {
            get { return CipherRotorOptions[4].rotorUsage[1]; }
            set { setRotorUsage(CipherRotorOptions[4], 1, value); }
        }

        [TaskPane("AnalysisUseRotor3Caption", "Cipher5AnalysisUseRotorTooltip", "Cipher5AnalysisOptionsGroup", 3, false, ControlType.CheckBox, "", null)]
        public bool Cipher5AnalysisUseRotor3
        {
            get { return CipherRotorOptions[4].rotorUsage[2]; }
            set { setRotorUsage(CipherRotorOptions[4], 2, value); }
        }

        [TaskPane("AnalysisUseRotor4Caption", "Cipher5AnalysisUseRotorTooltip", "Cipher5AnalysisOptionsGroup", 4, false, ControlType.CheckBox, "", null)]
        public bool Cipher5AnalysisUseRotor4
        {
            get { return CipherRotorOptions[4].rotorUsage[3]; }
            set { setRotorUsage(CipherRotorOptions[4], 3, value); }
        }

        [TaskPane("AnalysisUseRotor5Caption", "Cipher5AnalysisUseRotorTooltip", "Cipher5AnalysisOptionsGroup", 5, false, ControlType.CheckBox, "", null)]
        public bool Cipher5AnalysisUseRotor5
        {
            get { return CipherRotorOptions[4].rotorUsage[4]; }
            set { setRotorUsage(CipherRotorOptions[4], 4, value); }
        }

        [TaskPane("AnalysisUseRotor6Caption", "Cipher5AnalysisUseRotorTooltip", "Cipher5AnalysisOptionsGroup", 6, false, ControlType.CheckBox, "", null)]
        public bool Cipher5AnalysisUseRotor6
        {
            get { return CipherRotorOptions[4].rotorUsage[5]; }
            set { setRotorUsage(CipherRotorOptions[4], 5, value); }
        }

        [TaskPane("AnalysisUseRotor7Caption", "Cipher5AnalysisUseRotorTooltip", "Cipher5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher5AnalysisUseRotor7
        {
            get { return CipherRotorOptions[4].rotorUsage[6]; }
            set { setRotorUsage(CipherRotorOptions[4], 6, value); }
        }

        [TaskPane("AnalysisUseRotor8Caption", "Cipher5AnalysisUseRotorTooltip", "Cipher5AnalysisOptionsGroup", 8, false, ControlType.CheckBox, "", null)]
        public bool Cipher5AnalysisUseRotor8
        {
            get { return CipherRotorOptions[4].rotorUsage[7]; }
            set { setRotorUsage(CipherRotorOptions[4], 7, value); }
        }

        [TaskPane("AnalysisUseRotor9Caption", "Cipher5AnalysisUseRotorTooltip", "Cipher5AnalysisOptionsGroup", 9, false, ControlType.CheckBox, "", null)]
        public bool Cipher5AnalysisUseRotor9
        {
            get { return CipherRotorOptions[4].rotorUsage[8]; }
            set { setRotorUsage(CipherRotorOptions[4], 8, value); }
        }

        [TaskPane("AnalysisUseRotor10Caption", "Cipher5AnalysisUseRotorTooltip", "Cipher5AnalysisOptionsGroup", 10, false, ControlType.CheckBox, "", null)]
        public bool Cipher5AnalysisUseRotor0
        {
            get { return CipherRotorOptions[4].rotorUsage[9]; }
            set { setRotorUsage(CipherRotorOptions[4], 9, value); }
        }

        #endregion

        #endregion

        #region Control Bank

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sechs")]
        [TaskPane("ControlRotor1FromCaption", "FromTooltip", "PositionOptionsGroup", 16, false, ControlType.ComboBox, new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int ControlRotor1From
        {
            get { return ControlRotorOptions[0].from; }
            set { setFrom(ControlRotorOptions[0], value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sechs")]
        [TaskPane("ToCaption", "ToTooltip", "PositionOptionsGroup", 17, false, ControlType.ComboBox, new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int ControlRotor1To
        {
            get { return ControlRotorOptions[0].to; }
            set { setTo(ControlRotorOptions[0], value); }
        }

        [TaskPane("RevCaption", "RevTooltip", "PositionOptionsGroup", 18, false, ControlType.RadioButton, new String[] { "OriNormalCaption", "OriReverseCaption", "OriBothCaption" })]
        public int ControlRotor1Rev
        {
            get { return ControlRotorOptions[0].orientations; }
            set { setRev(ControlRotorOptions[0], value); }
        }

        #region Rotor choice

        [TaskPane("AnalysisUseRotor1Caption", "Control1AnalysisUseRotorTooltip", "Control1AnalysisOptionsGroup", 1, false, ControlType.CheckBox, "", null)]
        public bool Control1AnalysisUseRotor1
        {
            get { return ControlRotorOptions[0].rotorUsage[0]; }
            set { setRotorUsage(ControlRotorOptions[0], 0, value); }
        }

        [TaskPane("AnalysisUseRotor2Caption", "Control1AnalysisUseRotorTooltip", "Control1AnalysisOptionsGroup", 2, false, ControlType.CheckBox, "", null)]
        public bool Control1AnalysisUseRotor2
        {
            get { return ControlRotorOptions[0].rotorUsage[1]; }
            set { setRotorUsage(ControlRotorOptions[0], 1, value); }
        }

        [TaskPane("AnalysisUseRotor3Caption", "Control1AnalysisUseRotorTooltip", "Control1AnalysisOptionsGroup", 3, false, ControlType.CheckBox, "", null)]
        public bool Control1AnalysisUseRotor3
        {
            get { return ControlRotorOptions[0].rotorUsage[2]; }
            set { setRotorUsage(ControlRotorOptions[0], 2, value); }
        }

        [TaskPane("AnalysisUseRotor4Caption", "Control1AnalysisUseRotorTooltip", "Control1AnalysisOptionsGroup", 4, false, ControlType.CheckBox, "", null)]
        public bool Control1AnalysisUseRotor4
        {
            get { return ControlRotorOptions[0].rotorUsage[3]; }
            set { setRotorUsage(ControlRotorOptions[0], 3, value); }
        }

        [TaskPane("AnalysisUseRotor5Caption", "Control1AnalysisUseRotorTooltip", "Control1AnalysisOptionsGroup", 5, false, ControlType.CheckBox, "", null)]
        public bool Control1AnalysisUseRotor5
        {
            get { return ControlRotorOptions[0].rotorUsage[4]; }
            set { setRotorUsage(ControlRotorOptions[0], 4, value); }
        }

        [TaskPane("AnalysisUseRotor6Caption", "Control1AnalysisUseRotorTooltip", "Control1AnalysisOptionsGroup", 6, false, ControlType.CheckBox, "", null)]
        public bool Control1AnalysisUseRotor6
        {
            get { return ControlRotorOptions[0].rotorUsage[5]; }
            set { setRotorUsage(ControlRotorOptions[0], 5, value); }
        }

        [TaskPane("AnalysisUseRotor7Caption", "Control1AnalysisUseRotorTooltip", "Control1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control1AnalysisUseRotor7
        {
            get { return ControlRotorOptions[0].rotorUsage[6]; }
            set { setRotorUsage(ControlRotorOptions[0], 6, value); }
        }

        [TaskPane("AnalysisUseRotor8Caption", "Control1AnalysisUseRotorTooltip", "Control1AnalysisOptionsGroup", 8, false, ControlType.CheckBox, "", null)]
        public bool Control1AnalysisUseRotor8
        {
            get { return ControlRotorOptions[0].rotorUsage[7]; }
            set { setRotorUsage(ControlRotorOptions[0], 7, value); }
        }

        [TaskPane("AnalysisUseRotor9Caption", "Control1AnalysisUseRotorTooltip", "Control1AnalysisOptionsGroup", 9, false, ControlType.CheckBox, "", null)]
        public bool Control1AnalysisUseRotor9
        {
            get { return ControlRotorOptions[0].rotorUsage[8]; }
            set { setRotorUsage(ControlRotorOptions[0], 8, value); }
        }

        [TaskPane("AnalysisUseRotor10Caption", "Control1AnalysisUseRotorTooltip", "Control1AnalysisOptionsGroup", 10, false, ControlType.CheckBox, "", null)]
        public bool Control1AnalysisUseRotor0
        {
            get { return ControlRotorOptions[0].rotorUsage[9]; }
            set { setRotorUsage(ControlRotorOptions[0], 9, value); }
        }

        #endregion

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sieben")]
        [TaskPane("ControlRotor2FromCaption", "FromTooltip", "PositionOptionsGroup", 19, false, ControlType.ComboBox, new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int ControlRotor2From
        {
            get { return ControlRotorOptions[1].from; }
            set { setFrom(ControlRotorOptions[1], value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sieben")]
        [TaskPane("ToCaption", "ToTooltip", "PositionOptionsGroup", 20, false, ControlType.ComboBox, new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int ControlRotor2To
        {
            get { return ControlRotorOptions[1].to; }
            set { setTo(ControlRotorOptions[1], value); }
        }

        [TaskPane("RevCaption", "RevTooltip", "PositionOptionsGroup", 21, false, ControlType.RadioButton, new String[] { "OriNormalCaption", "OriReverseCaption", "OriBothCaption" })]
        public int ControlRotor2Rev
        {
            get { return ControlRotorOptions[1].orientations; }
            set { setRev(ControlRotorOptions[1], value); }
        }

        #region Rotor choice

        [TaskPane("AnalysisUseRotor1Caption", "Control2AnalysisUseRotorTooltip", "Control2AnalysisOptionsGroup", 1, false, ControlType.CheckBox, "", null)]
        public bool Control2AnalysisUseRotor1
        {
            get { return ControlRotorOptions[1].rotorUsage[0]; }
            set { setRotorUsage(ControlRotorOptions[1], 0, value); }
        }

        [TaskPane("AnalysisUseRotor2Caption", "Control2AnalysisUseRotorTooltip", "Control2AnalysisOptionsGroup", 2, false, ControlType.CheckBox, "", null)]
        public bool Control2AnalysisUseRotor2
        {
            get { return ControlRotorOptions[1].rotorUsage[1]; }
            set { setRotorUsage(ControlRotorOptions[1], 1, value); }
        }

        [TaskPane("AnalysisUseRotor3Caption", "Control2AnalysisUseRotorTooltip", "Control2AnalysisOptionsGroup", 3, false, ControlType.CheckBox, "", null)]
        public bool Control2AnalysisUseRotor3
        {
            get { return ControlRotorOptions[1].rotorUsage[2]; }
            set { setRotorUsage(ControlRotorOptions[1], 2, value); }
        }

        [TaskPane("AnalysisUseRotor4Caption", "Control2AnalysisUseRotorTooltip", "Control2AnalysisOptionsGroup", 4, false, ControlType.CheckBox, "", null)]
        public bool Control2AnalysisUseRotor4
        {
            get { return ControlRotorOptions[1].rotorUsage[3]; }
            set { setRotorUsage(ControlRotorOptions[1], 3, value); }
        }

        [TaskPane("AnalysisUseRotor5Caption", "Control2AnalysisUseRotorTooltip", "Control2AnalysisOptionsGroup", 5, false, ControlType.CheckBox, "", null)]
        public bool Control2AnalysisUseRotor5
        {
            get { return ControlRotorOptions[1].rotorUsage[4]; }
            set { setRotorUsage(ControlRotorOptions[1], 4, value); }
        }

        [TaskPane("AnalysisUseRotor6Caption", "Control2AnalysisUseRotorTooltip", "Control2AnalysisOptionsGroup", 6, false, ControlType.CheckBox, "", null)]
        public bool Control2AnalysisUseRotor6
        {
            get { return ControlRotorOptions[1].rotorUsage[5]; }
            set { setRotorUsage(ControlRotorOptions[1], 5, value); }
        }

        [TaskPane("AnalysisUseRotor7Caption", "Control2AnalysisUseRotorTooltip", "Control2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control2AnalysisUseRotor7
        {
            get { return ControlRotorOptions[1].rotorUsage[6]; }
            set { setRotorUsage(ControlRotorOptions[1], 6, value); }
        }

        [TaskPane("AnalysisUseRotor8Caption", "Control2AnalysisUseRotorTooltip", "Control2AnalysisOptionsGroup", 8, false, ControlType.CheckBox, "", null)]
        public bool Control2AnalysisUseRotor8
        {
            get { return ControlRotorOptions[1].rotorUsage[7]; }
            set { setRotorUsage(ControlRotorOptions[1], 7, value); }
        }

        [TaskPane("AnalysisUseRotor9Caption", "Control2AnalysisUseRotorTooltip", "Control2AnalysisOptionsGroup", 9, false, ControlType.CheckBox, "", null)]
        public bool Control2AnalysisUseRotor9
        {
            get { return ControlRotorOptions[1].rotorUsage[8]; }
            set { setRotorUsage(ControlRotorOptions[1], 8, value); }
        }

        [TaskPane("AnalysisUseRotor10Caption", "Control2AnalysisUseRotorTooltip", "Control2AnalysisOptionsGroup", 10, false, ControlType.CheckBox, "", null)]
        public bool Control2AnalysisUseRotor0
        {
            get { return ControlRotorOptions[1].rotorUsage[9]; }
            set { setRotorUsage(ControlRotorOptions[1], 9, value); }
        }

        #endregion

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Acht")]
        [TaskPane("ControlRotor3FromCaption", "FromTooltip", "PositionOptionsGroup", 22, false, ControlType.ComboBox, new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int ControlRotor3From
        {
            get { return ControlRotorOptions[2].from; }
            set { setFrom(ControlRotorOptions[2], value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Acht")]
        [TaskPane("ToCaption", "ToTooltip", "PositionOptionsGroup", 23, false, ControlType.ComboBox, new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int ControlRotor3To
        {
            get { return ControlRotorOptions[2].to; }
            set { setTo(ControlRotorOptions[2], value); }
        }

        [TaskPane("RevCaption", "RevTooltip", "PositionOptionsGroup", 24, false, ControlType.RadioButton, new String[] { "OriNormalCaption", "OriReverseCaption", "OriBothCaption" })]
        public int ControlRotor3Rev
        {
            get { return ControlRotorOptions[2].orientations; }
            set { setRev(ControlRotorOptions[2], value); }
        }

        #region Rotor choice

        [TaskPane("AnalysisUseRotor1Caption", "Control3AnalysisUseRotorTooltip", "Control3AnalysisOptionsGroup", 1, false, ControlType.CheckBox, "", null)]
        public bool Control3AnalysisUseRotor1
        {
            get { return ControlRotorOptions[2].rotorUsage[0]; }
            set { setRotorUsage(ControlRotorOptions[2], 0, value); }
        }

        [TaskPane("AnalysisUseRotor2Caption", "Control3AnalysisUseRotorTooltip", "Control3AnalysisOptionsGroup", 2, false, ControlType.CheckBox, "", null)]
        public bool Control3AnalysisUseRotor2
        {
            get { return ControlRotorOptions[2].rotorUsage[1]; }
            set { setRotorUsage(ControlRotorOptions[2], 1, value); }
        }

        [TaskPane("AnalysisUseRotor3Caption", "Control3AnalysisUseRotorTooltip", "Control3AnalysisOptionsGroup", 3, false, ControlType.CheckBox, "", null)]
        public bool Control3AnalysisUseRotor3
        {
            get { return ControlRotorOptions[2].rotorUsage[2]; }
            set { setRotorUsage(ControlRotorOptions[2], 2, value); }
        }

        [TaskPane("AnalysisUseRotor4Caption", "Control3AnalysisUseRotorTooltip", "Control3AnalysisOptionsGroup", 4, false, ControlType.CheckBox, "", null)]
        public bool Control3AnalysisUseRotor4
        {
            get { return ControlRotorOptions[2].rotorUsage[3]; }
            set { setRotorUsage(ControlRotorOptions[2], 3, value); }
        }

        [TaskPane("AnalysisUseRotor5Caption", "Control3AnalysisUseRotorTooltip", "Control3AnalysisOptionsGroup", 5, false, ControlType.CheckBox, "", null)]
        public bool Control3AnalysisUseRotor5
        {
            get { return ControlRotorOptions[2].rotorUsage[4]; }
            set { setRotorUsage(ControlRotorOptions[2], 4, value); }
        }

        [TaskPane("AnalysisUseRotor6Caption", "Control3AnalysisUseRotorTooltip", "Control3AnalysisOptionsGroup", 6, false, ControlType.CheckBox, "", null)]
        public bool Control3AnalysisUseRotor6
        {
            get { return ControlRotorOptions[2].rotorUsage[5]; }
            set { setRotorUsage(ControlRotorOptions[2], 5, value); }
        }

        [TaskPane("AnalysisUseRotor7Caption", "Control3AnalysisUseRotorTooltip", "Control3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control3AnalysisUseRotor7
        {
            get { return ControlRotorOptions[2].rotorUsage[6]; }
            set { setRotorUsage(ControlRotorOptions[2], 6, value); }
        }

        [TaskPane("AnalysisUseRotor8Caption", "Control3AnalysisUseRotorTooltip", "Control3AnalysisOptionsGroup", 8, false, ControlType.CheckBox, "", null)]
        public bool Control3AnalysisUseRotor8
        {
            get { return ControlRotorOptions[2].rotorUsage[7]; }
            set { setRotorUsage(ControlRotorOptions[2], 7, value); }
        }

        [TaskPane("AnalysisUseRotor9Caption", "Control3AnalysisUseRotorTooltip", "Control3AnalysisOptionsGroup", 9, false, ControlType.CheckBox, "", null)]
        public bool Control3AnalysisUseRotor9
        {
            get { return ControlRotorOptions[2].rotorUsage[8]; }
            set { setRotorUsage(ControlRotorOptions[2], 8, value); }
        }

        [TaskPane("AnalysisUseRotor10Caption", "Control3AnalysisUseRotorTooltip", "Control3AnalysisOptionsGroup", 10, false, ControlType.CheckBox, "", null)]
        public bool Control3AnalysisUseRotor0
        {
            get { return ControlRotorOptions[2].rotorUsage[9]; }
            set { setRotorUsage(ControlRotorOptions[2], 9, value); }
        }

        #endregion

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Neun")]
        [TaskPane("ControlRotor4FromCaption", "FromTooltip", "PositionOptionsGroup", 25, false, ControlType.ComboBox, new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int ControlRotor4From
        {
            get { return ControlRotorOptions[3].from; }
            set { setFrom(ControlRotorOptions[3], value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Neun")]
        [TaskPane("ToCaption", "ToTooltip", "PositionOptionsGroup", 26, false, ControlType.ComboBox, new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int ControlRotor4To
        {
            get { return ControlRotorOptions[3].to; }
            set { setTo(ControlRotorOptions[3], value); }
        }

        [TaskPane("RevCaption", "RevTooltip", "PositionOptionsGroup", 27, false, ControlType.RadioButton, new String[] { "OriNormalCaption", "OriReverseCaption", "OriBothCaption" })]
        public int ControlRotor4Rev
        {
            get { return ControlRotorOptions[3].orientations; }
            set { setRev(ControlRotorOptions[3], value); }
        }

        #region Rotor choice

        [TaskPane("AnalysisUseRotor1Caption", "Control4AnalysisUseRotorTooltip", "Control4AnalysisOptionsGroup", 1, false, ControlType.CheckBox, "", null)]
        public bool Control4AnalysisUseRotor1
        {
            get { return ControlRotorOptions[3].rotorUsage[0]; }
            set { setRotorUsage(ControlRotorOptions[3], 0, value); }
        }

        [TaskPane("AnalysisUseRotor2Caption", "Control4AnalysisUseRotorTooltip", "Control4AnalysisOptionsGroup", 2, false, ControlType.CheckBox, "", null)]
        public bool Control4AnalysisUseRotor2
        {
            get { return ControlRotorOptions[3].rotorUsage[1]; }
            set { setRotorUsage(ControlRotorOptions[3], 1, value); }
        }

        [TaskPane("AnalysisUseRotor3Caption", "Control4AnalysisUseRotorTooltip", "Control4AnalysisOptionsGroup", 3, false, ControlType.CheckBox, "", null)]
        public bool Control4AnalysisUseRotor3
        {
            get { return ControlRotorOptions[3].rotorUsage[2]; }
            set { setRotorUsage(ControlRotorOptions[3], 2, value); }
        }

        [TaskPane("AnalysisUseRotor4Caption", "Control4AnalysisUseRotorTooltip", "Control4AnalysisOptionsGroup", 4, false, ControlType.CheckBox, "", null)]
        public bool Control4AnalysisUseRotor4
        {
            get { return ControlRotorOptions[3].rotorUsage[3]; }
            set { setRotorUsage(ControlRotorOptions[3], 3, value); }
        }

        [TaskPane("AnalysisUseRotor5Caption", "Control4AnalysisUseRotorTooltip", "Control4AnalysisOptionsGroup", 5, false, ControlType.CheckBox, "", null)]
        public bool Control4AnalysisUseRotor5
        {
            get { return ControlRotorOptions[3].rotorUsage[4]; }
            set { setRotorUsage(ControlRotorOptions[3], 4, value); }
        }

        [TaskPane("AnalysisUseRotor6Caption", "Control4AnalysisUseRotorTooltip", "Control4AnalysisOptionsGroup", 6, false, ControlType.CheckBox, "", null)]
        public bool Control4AnalysisUseRotor6
        {
            get { return ControlRotorOptions[3].rotorUsage[5]; }
            set { setRotorUsage(ControlRotorOptions[3], 5, value); }
        }

        [TaskPane("AnalysisUseRotor7Caption", "Control4AnalysisUseRotorTooltip", "Control4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control4AnalysisUseRotor7
        {
            get { return ControlRotorOptions[3].rotorUsage[6]; }
            set { setRotorUsage(ControlRotorOptions[3], 6, value); }
        }

        [TaskPane("AnalysisUseRotor8Caption", "Control4AnalysisUseRotorTooltip", "Control4AnalysisOptionsGroup", 8, false, ControlType.CheckBox, "", null)]
        public bool Control4AnalysisUseRotor8
        {
            get { return ControlRotorOptions[3].rotorUsage[7]; }
            set { setRotorUsage(ControlRotorOptions[3], 7, value); }
        }

        [TaskPane("AnalysisUseRotor9Caption", "Control4AnalysisUseRotorTooltip", "Control4AnalysisOptionsGroup", 9, false, ControlType.CheckBox, "", null)]
        public bool Control4AnalysisUseRotor9
        {
            get { return ControlRotorOptions[3].rotorUsage[8]; }
            set { setRotorUsage(ControlRotorOptions[3], 8, value); }
        }

        [TaskPane("AnalysisUseRotor10Caption", "Control4AnalysisUseRotorTooltip", "Control4AnalysisOptionsGroup", 10, false, ControlType.CheckBox, "", null)]
        public bool Control4AnalysisUseRotor0
        {
            get { return ControlRotorOptions[3].rotorUsage[9]; }
            set { setRotorUsage(ControlRotorOptions[3], 9, value); }
        }

        #endregion

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zehn")]
        [TaskPane("ControlRotor5FromCaption", "FromTooltip", "PositionOptionsGroup", 28, false, ControlType.ComboBox, new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int ControlRotor5From
        {
            get { return ControlRotorOptions[4].from; }
            set { setFrom(ControlRotorOptions[4], value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zehn")]
        [TaskPane("ToCaption", "ToTooltip", "PositionOptionsGroup", 29, false, ControlType.ComboBox, new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int ControlRotor5To
        {
            get { return ControlRotorOptions[4].to; }
            set { setTo(ControlRotorOptions[4], value); }
        }

        [TaskPane("RevCaption", "RevTooltip", "PositionOptionsGroup", 30, false, ControlType.RadioButton, new String[] { "OriNormalCaption", "OriReverseCaption", "OriBothCaption" })]
        public int ControlRotor5Rev
        {
            get { return ControlRotorOptions[4].orientations; }
            set { setRev(ControlRotorOptions[4], value); }
        }

        #region Rotor choice

        [TaskPane("AnalysisUseRotor1Caption", "Control5AnalysisUseRotorTooltip", "Control5AnalysisOptionsGroup", 1, false, ControlType.CheckBox, "", null)]
        public bool Control5AnalysisUseRotor1
        {
            get { return ControlRotorOptions[4].rotorUsage[0]; }
            set { setRotorUsage(ControlRotorOptions[4], 0, value); }
        }

        [TaskPane("AnalysisUseRotor2Caption", "Control5AnalysisUseRotorTooltip", "Control5AnalysisOptionsGroup", 2, false, ControlType.CheckBox, "", null)]
        public bool Control5AnalysisUseRotor2
        {
            get { return ControlRotorOptions[4].rotorUsage[1]; }
            set { setRotorUsage(ControlRotorOptions[4], 1, value); }
        }

        [TaskPane("AnalysisUseRotor3Caption", "Control5AnalysisUseRotorTooltip", "Control5AnalysisOptionsGroup", 3, false, ControlType.CheckBox, "", null)]
        public bool Control5AnalysisUseRotor3
        {
            get { return ControlRotorOptions[4].rotorUsage[2]; }
            set { setRotorUsage(ControlRotorOptions[4], 2, value); }
        }

        [TaskPane("AnalysisUseRotor4Caption", "Control5AnalysisUseRotorTooltip", "Control5AnalysisOptionsGroup", 4, false, ControlType.CheckBox, "", null)]
        public bool Control5AnalysisUseRotor4
        {
            get { return ControlRotorOptions[4].rotorUsage[3]; }
            set { setRotorUsage(ControlRotorOptions[4], 3, value); }
        }

        [TaskPane("AnalysisUseRotor5Caption", "Control5AnalysisUseRotorTooltip", "Control5AnalysisOptionsGroup", 5, false, ControlType.CheckBox, "", null)]
        public bool Control5AnalysisUseRotor5
        {
            get { return ControlRotorOptions[4].rotorUsage[4]; }
            set { setRotorUsage(ControlRotorOptions[4], 4, value); }
        }

        [TaskPane("AnalysisUseRotor6Caption", "Control5AnalysisUseRotorTooltip", "Control5AnalysisOptionsGroup", 6, false, ControlType.CheckBox, "", null)]
        public bool Control5AnalysisUseRotor6
        {
            get { return ControlRotorOptions[4].rotorUsage[5]; }
            set { setRotorUsage(ControlRotorOptions[4], 5, value); }
        }

        [TaskPane("AnalysisUseRotor7Caption", "Control5AnalysisUseRotorTooltip", "Control5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control5AnalysisUseRotor7
        {
            get { return ControlRotorOptions[4].rotorUsage[6]; }
            set { setRotorUsage(ControlRotorOptions[4], 6, value); }
        }

        [TaskPane("AnalysisUseRotor8Caption", "Control5AnalysisUseRotorTooltip", "Control5AnalysisOptionsGroup", 8, false, ControlType.CheckBox, "", null)]
        public bool Control5AnalysisUseRotor8
        {
            get { return ControlRotorOptions[4].rotorUsage[7]; }
            set { setRotorUsage(ControlRotorOptions[4], 7, value); }
        }

        [TaskPane("AnalysisUseRotor9Caption", "Control5AnalysisUseRotorTooltip", "Control5AnalysisOptionsGroup", 9, false, ControlType.CheckBox, "", null)]
        public bool Control5AnalysisUseRotor9
        {
            get { return ControlRotorOptions[4].rotorUsage[8]; }
            set { setRotorUsage(ControlRotorOptions[4], 8, value); }
        }

        [TaskPane("AnalysisUseRotor10Caption", "Control5AnalysisUseRotorTooltip", "Control5AnalysisOptionsGroup", 10, false, ControlType.CheckBox, "", null)]
        public bool Control5AnalysisUseRotor0
        {
            get { return ControlRotorOptions[4].rotorUsage[9]; }
            set { setRotorUsage(ControlRotorOptions[4], 9, value); }
        }

        #endregion

        #endregion

        #region Index Bank

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Elf")]
        [TaskPane("IndexRotor1FromCaption", "FromTooltip", "PositionOptionsGroup", 40, false, ControlType.ComboBox, new String[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" })]
        public int IndexRotor1From
        {
            get { return IndexRotorOptions[0].from; }
            set { setFrom(IndexRotorOptions[0], value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Elf")]
        [TaskPane("ToCaption", "ToTooltip", "PositionOptionsGroup", 41, false, ControlType.ComboBox, new String[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" })]
        public int IndexRotor1To
        {
            get { return IndexRotorOptions[0].to; }
            set { setTo(IndexRotorOptions[0], value); }
        }

        #region Rotor choice

        [TaskPane("IndexAnalysisUseRotor1Caption", "Index1AnalysisUseRotorTooltip", "Index1AnalysisOptionsGroup", 1, false, ControlType.CheckBox, "", null)]
        public bool Index1AnalysisUseRotor1
        {
            get { return IndexRotorOptions[0].rotorUsage[0]; }
            set { setRotorUsage(IndexRotorOptions[0], 0, value); }
        }

        [TaskPane("IndexAnalysisUseRotor2Caption", "Index1AnalysisUseRotorTooltip", "Index1AnalysisOptionsGroup", 2, false, ControlType.CheckBox, "", null)]
        public bool Index1AnalysisUseRotor2
        {
            get { return IndexRotorOptions[0].rotorUsage[1]; }
            set { setRotorUsage(IndexRotorOptions[0], 1, value); }
        }

        [TaskPane("IndexAnalysisUseRotor3Caption", "Index1AnalysisUseRotorTooltip", "Index1AnalysisOptionsGroup", 3, false, ControlType.CheckBox, "", null)]
        public bool Index1AnalysisUseRotor3
        {
            get { return IndexRotorOptions[0].rotorUsage[2]; }
            set { setRotorUsage(IndexRotorOptions[0], 2, value); }
        }

        [TaskPane("IndexAnalysisUseRotor4Caption", "Index1AnalysisUseRotorTooltip", "Index1AnalysisOptionsGroup", 4, false, ControlType.CheckBox, "", null)]
        public bool Index1AnalysisUseRotor4
        {
            get { return IndexRotorOptions[0].rotorUsage[3]; }
            set { setRotorUsage(IndexRotorOptions[0], 3, value); }
        }

        [TaskPane("IndexAnalysisUseRotor5Caption", "Index1AnalysisUseRotorTooltip", "Index1AnalysisOptionsGroup", 5, false, ControlType.CheckBox, "", null)]
        public bool Index1AnalysisUseRotor5
        {
            get { return IndexRotorOptions[0].rotorUsage[4]; }
            set { setRotorUsage(IndexRotorOptions[0], 4, value); }
        }

        #endregion

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwölf")]
        [TaskPane("IndexRotor2FromCaption", "FromTooltip", "PositionOptionsGroup", 40, false, ControlType.ComboBox, new String[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" })]
        public int IndexRotor2From
        {
            get { return IndexRotorOptions[1].from; }
            set { setFrom(IndexRotorOptions[1], value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwölf")]
        [TaskPane("ToCaption", "ToTooltip", "PositionOptionsGroup", 41, false, ControlType.ComboBox, new String[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" })]
        public int IndexRotor2To
        {
            get { return IndexRotorOptions[1].to; }
            set { setTo(IndexRotorOptions[1], value); }
        }

        #region Rotor choice

        [TaskPane("IndexAnalysisUseRotor1Caption", "Index2AnalysisUseRotorTooltip", "Index2AnalysisOptionsGroup", 1, false, ControlType.CheckBox, "", null)]
        public bool Index2AnalysisUseRotor1
        {
            get { return IndexRotorOptions[1].rotorUsage[0]; }
            set { setRotorUsage(IndexRotorOptions[1], 0, value); }
        }

        [TaskPane("IndexAnalysisUseRotor2Caption", "Index2AnalysisUseRotorTooltip", "Index2AnalysisOptionsGroup", 2, false, ControlType.CheckBox, "", null)]
        public bool Index2AnalysisUseRotor2
        {
            get { return IndexRotorOptions[1].rotorUsage[1]; }
            set { setRotorUsage(IndexRotorOptions[1], 1, value); }
        }

        [TaskPane("IndexAnalysisUseRotor3Caption", "Index2AnalysisUseRotorTooltip", "Index2AnalysisOptionsGroup", 3, false, ControlType.CheckBox, "", null)]
        public bool Index2AnalysisUseRotor3
        {
            get { return IndexRotorOptions[1].rotorUsage[2]; }
            set { setRotorUsage(IndexRotorOptions[1], 2, value); }
        }

        [TaskPane("IndexAnalysisUseRotor4Caption", "Index2AnalysisUseRotorTooltip", "Index2AnalysisOptionsGroup", 4, false, ControlType.CheckBox, "", null)]
        public bool Index2AnalysisUseRotor4
        {
            get { return IndexRotorOptions[1].rotorUsage[3]; }
            set { setRotorUsage(IndexRotorOptions[1], 3, value); }
        }

        [TaskPane("IndexAnalysisUseRotor5Caption", "Index2AnalysisUseRotorTooltip", "Index2AnalysisOptionsGroup", 5, false, ControlType.CheckBox, "", null)]
        public bool Index2AnalysisUseRotor5
        {
            get { return IndexRotorOptions[1].rotorUsage[4]; }
            set { setRotorUsage(IndexRotorOptions[1], 4, value); }
        }

        #endregion

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "DreiZehn")]
        [TaskPane("IndexRotor3FromCaption", "FromTooltip", "PositionOptionsGroup", 40, false, ControlType.ComboBox, new String[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" })]
        public int IndexRotor3From
        {
            get { return IndexRotorOptions[2].from; }
            set { setFrom(IndexRotorOptions[2], value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "DreiZehn")]
        [TaskPane("ToCaption", "ToTooltip", "PositionOptionsGroup", 41, false, ControlType.ComboBox, new String[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" })]
        public int IndexRotor3To
        {
            get { return IndexRotorOptions[2].to; }
            set { setTo(IndexRotorOptions[2], value); }
        }

        #region Rotor choice

        [TaskPane("IndexAnalysisUseRotor1Caption", "Index3AnalysisUseRotorTooltip", "Index3AnalysisOptionsGroup", 1, false, ControlType.CheckBox, "", null)]
        public bool Index3AnalysisUseRotor1
        {
            get { return IndexRotorOptions[2].rotorUsage[0]; }
            set { setRotorUsage(IndexRotorOptions[2], 0, value); }
        }

        [TaskPane("IndexAnalysisUseRotor2Caption", "Index3AnalysisUseRotorTooltip", "Index3AnalysisOptionsGroup", 2, false, ControlType.CheckBox, "", null)]
        public bool Index3AnalysisUseRotor2
        {
            get { return IndexRotorOptions[2].rotorUsage[1]; }
            set { setRotorUsage(IndexRotorOptions[2], 1, value); }
        }

        [TaskPane("IndexAnalysisUseRotor3Caption", "Index3AnalysisUseRotorTooltip", "Index3AnalysisOptionsGroup", 3, false, ControlType.CheckBox, "", null)]
        public bool Index3AnalysisUseRotor3
        {
            get { return IndexRotorOptions[2].rotorUsage[2]; }
            set { setRotorUsage(IndexRotorOptions[2], 2, value); }
        }

        [TaskPane("IndexAnalysisUseRotor4Caption", "Index3AnalysisUseRotorTooltip", "Index3AnalysisOptionsGroup", 4, false, ControlType.CheckBox, "", null)]
        public bool Index3AnalysisUseRotor4
        {
            get { return IndexRotorOptions[2].rotorUsage[3]; }
            set { setRotorUsage(IndexRotorOptions[2], 3, value); }
        }

        [TaskPane("IndexAnalysisUseRotor5Caption", "Index3AnalysisUseRotorTooltip", "Index3AnalysisOptionsGroup", 5, false, ControlType.CheckBox, "", null)]
        public bool Index3AnalysisUseRotor5
        {
            get { return IndexRotorOptions[2].rotorUsage[4]; }
            set { setRotorUsage(IndexRotorOptions[2], 4, value); }
        }

        #endregion

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "VierZehn")]
        [TaskPane("IndexRotor4FromCaption", "FromTooltip", "PositionOptionsGroup", 40, false, ControlType.ComboBox, new String[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" })]
        public int IndexRotor4From
        {
            get { return IndexRotorOptions[3].from; }
            set { setFrom(IndexRotorOptions[3], value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "VierZehn")]
        [TaskPane("ToCaption", "ToTooltip", "PositionOptionsGroup", 41, false, ControlType.ComboBox, new String[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" })]
        public int IndexRotor4To
        {
            get { return IndexRotorOptions[3].to; }
            set { setTo(IndexRotorOptions[3], value); }
        }

        #region Rotor choice

        [TaskPane("IndexAnalysisUseRotor1Caption", "Index4AnalysisUseRotorTooltip", "Index4AnalysisOptionsGroup", 1, false, ControlType.CheckBox, "", null)]
        public bool Index4AnalysisUseRotor1
        {
            get { return IndexRotorOptions[3].rotorUsage[0]; }
            set { setRotorUsage(IndexRotorOptions[3], 0, value); }
        }

        [TaskPane("IndexAnalysisUseRotor2Caption", "Index4AnalysisUseRotorTooltip", "Index4AnalysisOptionsGroup", 2, false, ControlType.CheckBox, "", null)]
        public bool Index4AnalysisUseRotor2
        {
            get { return IndexRotorOptions[3].rotorUsage[1]; }
            set { setRotorUsage(IndexRotorOptions[3], 1, value); }
        }

        [TaskPane("IndexAnalysisUseRotor3Caption", "Index4AnalysisUseRotorTooltip", "Index4AnalysisOptionsGroup", 3, false, ControlType.CheckBox, "", null)]
        public bool Index4AnalysisUseRotor3
        {
            get { return IndexRotorOptions[3].rotorUsage[2]; }
            set { setRotorUsage(IndexRotorOptions[3], 2, value); }
        }

        [TaskPane("IndexAnalysisUseRotor4Caption", "Index4AnalysisUseRotorTooltip", "Index4AnalysisOptionsGroup", 4, false, ControlType.CheckBox, "", null)]
        public bool Index4AnalysisUseRotor4
        {
            get { return IndexRotorOptions[3].rotorUsage[3]; }
            set { setRotorUsage(IndexRotorOptions[3], 3, value); }
        }

        [TaskPane("IndexAnalysisUseRotor5Caption", "Index4AnalysisUseRotorTooltip", "Index4AnalysisOptionsGroup", 5, false, ControlType.CheckBox, "", null)]
        public bool Index4AnalysisUseRotor5
        {
            get { return IndexRotorOptions[3].rotorUsage[4]; }
            set { setRotorUsage(IndexRotorOptions[3], 4, value); }
        }

        #endregion

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "FünfZehn")]
        [TaskPane("IndexRotor5FromCaption", "FromTooltip", "PositionOptionsGroup", 40, false, ControlType.ComboBox, new String[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" })]
        public int IndexRotor5From
        {
            get { return IndexRotorOptions[4].from; }
            set { setFrom(IndexRotorOptions[4], value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "FünfZehn")]
        [TaskPane("ToCaption", "ToTooltip", "PositionOptionsGroup", 41, false, ControlType.ComboBox, new String[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" })]
        public int IndexRotor5To
        {
            get { return IndexRotorOptions[4].to; }
            set { setTo(IndexRotorOptions[4], value); }
        }

        #region Rotor choice

        [TaskPane("IndexAnalysisUseRotor1Caption", "Index5AnalysisUseRotorTooltip", "Index5AnalysisOptionsGroup", 1, false, ControlType.CheckBox, "", null)]
        public bool Index5AnalysisUseRotor1
        {
            get { return IndexRotorOptions[4].rotorUsage[0]; }
            set { setRotorUsage(IndexRotorOptions[4], 0, value); }
        }

        [TaskPane("IndexAnalysisUseRotor2Caption", "Index5AnalysisUseRotorTooltip", "Index5AnalysisOptionsGroup", 2, false, ControlType.CheckBox, "", null)]
        public bool Index5AnalysisUseRotor2
        {
            get { return IndexRotorOptions[4].rotorUsage[1]; }
            set { setRotorUsage(IndexRotorOptions[4], 1, value); }
        }

        [TaskPane("IndexAnalysisUseRotor3Caption", "Index5AnalysisUseRotorTooltip", "Index5AnalysisOptionsGroup", 3, false, ControlType.CheckBox, "", null)]
        public bool Index5AnalysisUseRotor3
        {
            get { return IndexRotorOptions[4].rotorUsage[2]; }
            set { setRotorUsage(IndexRotorOptions[4], 2, value); }
        }

        [TaskPane("IndexAnalysisUseRotor4Caption", "Index5AnalysisUseRotorTooltip", "Index5AnalysisOptionsGroup", 4, false, ControlType.CheckBox, "", null)]
        public bool Index5AnalysisUseRotor4
        {
            get { return IndexRotorOptions[4].rotorUsage[3]; }
            set { setRotorUsage(IndexRotorOptions[4], 3, value); }
        }

        [TaskPane("IndexAnalysisUseRotor5Caption", "Index5AnalysisUseRotorTooltip", "Index5AnalysisOptionsGroup", 5, false, ControlType.CheckBox, "", null)]
        public bool Index5AnalysisUseRotor5
        {
            get { return IndexRotorOptions[4].rotorUsage[4]; }
            set { setRotorUsage(IndexRotorOptions[4], 4, value); }
        }

        #endregion

        #endregion

        #endregion

        private static bool[][] int2bits = Enumerable.Range(0, 1024).Select(i => Enumerable.Range(0, 10).Select(j => (i & (1 << (9 - j))) != 0).ToArray()).ToArray();

        private bool checkSetting(int[] settings, bool[] bits)
        {
            for (int i = 0; i < 10; i++)
                if (settings[i] == (int)OrientationChecks.Normal) { if (!bits[i]) return false; }
                else if (settings[i] == (int)OrientationChecks.Reverse) { if (bits[i]) return false; }

            return true;
        }

        public bool[][] getWhiteList()
        {
            int[] orientations = CipherRotorOptions.Concat(ControlRotorOptions).Select(o => o.orientations).ToArray();
            return int2bits.Where(b => checkSetting(orientations, b)).ToArray();
        }

        public double getLogKeyspace()
        {
            int cipherrotorpositions = CipherRotorOptions.Aggregate(1, (total, next) => total * (next.to - next.from + 1));
            int controlrotorpositions = ControlRotorOptions.Aggregate(1, (total, next) => total * (next.to - next.from + 1));
            int indexrotorpositions = IndexRotorOptions.Aggregate(1, (total, next) => total * (next.to - next.from + 1));
            double summax = Math.Log(cipherrotorpositions, 2) + Math.Log(controlrotorpositions, 2) + Math.Log(indexrotorpositions, 2);

            foreach (var o in CipherRotorOptions)
                if (o.orientations == (int)OrientationChecks.Both) summax++;

            foreach (var o in ControlRotorOptions)
                if (o.orientations == (int)OrientationChecks.Both) summax++;

            int[][] indexarr = indexRotorSettings();
            int[][] controlarr = rotorSettings();
            
            long sumkeyspace = 0;
            int[] arr = setStartingArr(indexarr);
            if (arr != null)
                do { sumkeyspace++; } while (NextPermutation(arr, indexarr));

            long sumkeyspace2 = 0;
            arr = setStartingArr(controlarr);
            if (arr != null)
                do { sumkeyspace2++; } while (NextPermutation(arr, controlarr));

            sumkeyspace *= sumkeyspace2;
            if (sumkeyspace > 0)
                summax += Math.Log(sumkeyspace, 2);
            else 
                summax = 0;

            return summax;
        }

        public BigInteger getKeyspaceAsLong()
        {
            BigInteger cipherrotorpositions = CipherRotorOptions.Aggregate(1, (total, next) => total * (next.to - next.from + 1));
            BigInteger controlrotorpositions = ControlRotorOptions.Aggregate(1, (total, next) => total * (next.to - next.from + 1));
            BigInteger indexrotorpositions = IndexRotorOptions.Aggregate(1, (total, next) => total * (next.to - next.from + 1));

            BigInteger bi = cipherrotorpositions * controlrotorpositions * indexrotorpositions;

            int[][] indexarr = indexRotorSettings();
            int[][] controlarr = rotorSettings();

            long sumkeyspace = 0;

            int[] arr = setStartingArr(indexarr);
            if (arr != null)
                do { sumkeyspace++; } while (NextPermutation(arr, indexarr));
            bi *= sumkeyspace;

            sumkeyspace = 0;
            arr = setStartingArr(controlarr);
            if (arr != null)
                do { sumkeyspace++; } while (NextPermutation(arr, controlarr));
            bi *= sumkeyspace;

            foreach (var o in CipherRotorOptions)
                if (o.orientations == (int)OrientationChecks.Both) bi *= 2;

            foreach (var o in ControlRotorOptions)
                if (o.orientations == (int)OrientationChecks.Both) bi *= 2;

            return bi;
        }

        public bool NextPermutation(int[] numList, int[][] controlarr)
        {
            /*
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
                        break;
                    if (i == numList.Length - 1)
                        b = false;
                }
            }

            return true;
        }

        public int[] setStartingArr(int[][] indexarr)
        {
            //int[] result = indexarr.Select((a, i) => i + 1).ToArray();
            int[] result = new int[indexarr.Length];

            for (int i = 0; i < result.Length; i++)
            {
                bool found = false;
                foreach (var ia in indexarr[i])
                    if (ia!=-1 && !result.Take(i).Contains(ia))
                    {
                        result[i] = ia;
                        found = true;
                        break;
                    }

                    //if (ia == -1) continue;

                    //Boolean before = false;
                    //for (int j = 0; j < i; j++)
                    //    if (result[j] == ia)
                    //    {
                    //        before = true;
                    //        break;
                    //    }

                    //if (!before)
                    //{
                    //    result[i] = ia;
                    //    break;
                    //}
                if (!found) return null;
            }

            return result;
        }

        public int[][] indexRotorSettings()
        {
            return IndexRotorOptions.Select(o => o.rotorUsage.Select((b, i) => b ? i + 1 : -1).ToArray()).ToArray();
        }

        public int[][] rotorSettings()
        {
            return CipherRotorOptions.Concat(ControlRotorOptions).Select(o => o.rotorUsage.Select((b, i) => b ? i + 1 : -1).ToArray()).ToArray();
        }

        private void showSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Visible)));
        }

        private void hideSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Collapsed)));
        }

        private void showhideAll(RotorAnalysisOptions o, int cnt)
        {
            if (o.showOptions)
                for (int i = 0; i < cnt; i++)
                    showSettingsElement(o.name + o.index + "AnalysisUseRotor" + i);
            else
                for (int i = 0; i < cnt; i++)
                    hideSettingsElement(o.name + o.index + "AnalysisUseRotor" + i);
        }

        private void setSettingsVisibility()
        {
            for (int i = 0; i < 5; i++)
            {
                showhideAll(CipherRotorOptions[i], 10);
                showhideAll(ControlRotorOptions[i], 10);
                showhideAll(IndexRotorOptions[i], 5);
            }
        }

        #region Events

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}