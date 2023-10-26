/*
   Copyright 2022 Vasily Mikhalev, CrypTool 2 Team

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using static HagelinMachine.HagelinConstants;
using static HagelinMachine.HagelinEnums;

namespace CrypTool.Plugins.HagelinMachine
{
    public partial class HagelinMachineSettings : ISettings
    {
        #region Variables

        private bool _initializing = true;
        private bool _showAllSettings = false;
        private UnknownSymbolHandling _unknownSymbolHandling = UnknownSymbolHandling.Ignore;
        private ModeType _mode = ModeType.Encrypt;
        private ModelType _model = ModelType.CX52a;
        private int _numberOfWheels = 6;
        private int _numberOfBars = 32;

        // Wheel properties:
        public WheelType[] _wheelsTypes = new WheelType[maxNumberOfWheels];
        public string[] _wheelTypeNames = new string[maxNumberOfWheels] { "29", "29", "29", "29", "29", "29", "29", "29", "29", "29", "29", "29" };
        public int[] _wheelSizes = new int[maxNumberOfWheels];
        public string[] _wheelsPins = new string[maxNumberOfWheels];
        public string[] _wheelsInitialStates = new string[maxNumberOfWheels];

        public string _wheelsInitialPositions = "A,A,A,A,A,A";

        //Bars properties
        public int[] _barType = new int[maxNumberOfBars] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        public bool[] _barHasLugs = new bool[maxNumberOfBars] { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true };
        public string[] _barCamsAsString = new string[maxNumberOfBars];
        public string[] _barLug = new string[maxNumberOfBars] { "1", "2", "2", "3", "3", "4", "4", "4", "4", "4", "5", "5", "5", "5", "5", "5", "5", "5", "6", "6", "6", "6", "6", "6", "6", "6", "6", "1", "1,2", "1,2,3", "1,2,3,4", "1,2,3,4,5" };
        public ToothType[] _barTooth = new ToothType[maxNumberOfBars] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public PluginStates _pluginState = PluginStates.ModelSelection;
        public string _hintMessage = "1 - Select the model of Hagelin Machine and Operation Mode";
        public string _selectedModelMessage = "Selected model - CX-52a";
        //private string _selectedModel = "CX-52a";
        private string _selectedWheels;
        private string _selectedBars;
        private string _selectedInitOffset;
        private string _selectedFVFeature;

        //FV Feature
        private bool _FVfeatureIsActive = false;
        private int _initOffset = 0;

        // Text options
        private bool _useZAsSpace;

        //Visibility Properties
        private bool _wheelsShown = false;
        private bool _barsShown = false;
        private bool _onlyLugsShown;
        private bool _numberOfWheelsAndBarsShown = false;

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        #endregion

        #region Methods

        #region Functions to set initial machine state

        /// <summary>
        /// Rotation by one position
        /// </summary>
        /// <param name="wheelIndex"></param>
        public void RotateWheelDown(int wheelIndex)
        {
            string val = _wheelsInitialStates[wheelIndex];
            int indexOfFirstComma = val.IndexOf(",");
            int i = 1;
            while (val[i + indexOfFirstComma] == ' ')
            {
                i++;
            }
            int indexOfSecondValue = indexOfFirstComma + i;

            string last2characters = val.Substring(val.Length - 2).Trim();
            if (last2characters[last2characters.Length - 1] != ',')
            {
                val += ',';
            }

            _wheelsInitialStates[wheelIndex] = val.Substring(indexOfSecondValue) + val.Substring(0, indexOfSecondValue - 1);
            OnPropertyChanged("Wheel" + (wheelIndex + 1).ToString() + "InitialState");
            SetExternalKeyShow();
            OnPropertyChanged("WheelsState");
        }

        public void RotateWheelUp(int wheelIndex)
        {

            string val = _wheelsInitialStates[wheelIndex];
            if (val[val.Length - 1] == ',')
            {
                val = val.Substring(0, val.Length - 1); // removing last comma if needed

            }

            val = "," + val;

            while (val[val.Length - 1] != ',')
            {
                val = val[val.Length - 1] + val.Substring(0, val.Length - 1);
            }

            _wheelsInitialStates[wheelIndex] = val;
            OnPropertyChanged("Wheel" + (wheelIndex + 1).ToString() + "InitialState");
            SetExternalKeyShow();
            OnPropertyChanged("WheelsState");
        }

        public void RotateWheel(int wheelIndex, string toValue)
        {
            string val = _wheelsInitialStates[wheelIndex];
            if (val.IndexOf(toValue) < 0)
            {
                return;
            }
            string last2characters = val.Substring(val.Length - 2).Trim();
            if (last2characters[last2characters.Length - 1] != ',')
            {
                val += ",";
            }

            while (val.IndexOf(toValue) > 0)
            {
                val = val.Substring(1) + val.Substring(0, 1);
            }

            _wheelsInitialStates[wheelIndex] = val;

            OnPropertyChanged("Wheel" + (wheelIndex + 1).ToString() + "InitialState");
            SetExternalKeyShow();
        }

        #endregion

        #region Machine settings  
        private void ApplyModel()
        {
            switch (_model)
            {
                #region CX52A
                case ModelType.CX52a:
                    NumberOfWheels = 6;
                    OnPropertyChanged("NumberOfWheels");
                    NumberOfBars = 32;
                    OnPropertyChanged("NumberOfBars");

                    SetWheelSizes(WheelSIZES_DEFAULT);

                    for (int i = 1; i < 5; i++)
                    {
                        DefineBarByKnownType(i - 1, BarType.ldA00000num1);
                    }

                    DefineBarByKnownType(5 - 1, BarType.ldB00000num2);

                    for (int i = 6; i < 10; i++)
                    { DefineBarByKnownType(i - 1, BarType.ld0A0000num3); }

                    DefineBarByKnownType(10 - 1, BarType.ld0B0000num4);

                    for (int i = 11; i < 15; i++)
                    {
                        DefineBarByKnownType(i - 1, BarType.ld00A000num5);
                    }

                    DefineBarByKnownType(15 - 1, BarType.ld00B000num6);

                    for (int i = 16; i < 20; i++)
                    {
                        DefineBarByKnownType(i - 1, BarType.ld000A00num7);
                    }

                    DefineBarByKnownType(20 - 1, BarType.ld000B00num8);

                    for (int i = 21; i < 25; i++)
                    {
                        DefineBarByKnownType(i - 1, BarType.ld0000A0num9);
                    }

                    DefineBarByKnownType(25 - 1, BarType.ld0000B0num10);

                    for (int i = 26; i < 30; i++)
                    {
                        DefineBarByKnownType(i - 1, BarType.ld00000Anum11);
                    }

                    DefineBarByKnownType(30 - 1, BarType.ld00000Bnum12);

                    for (int i = 31; i < 33; i++)
                    {
                        DefineBarByKnownType(i - 1, BarType.ld000000num13);
                    }

                    _model = ModelType.CX52a;
                    OnPropertyChanged("Model");
                    break;
                #endregion
                #region CX52B
                case ModelType.CX52b:
                    SetWheelSizes(WheelSIZES_DEFAULT);
                    NumberOfBars = 32;
                    OnPropertyChanged("NumberOfBars");
                    DefineBarByKnownType(32 - 1, BarType.udCCCCCCnum17);
                    for (int i = 1; i < 32; i++)
                    {
                        DefineBarByKnownType(i - 1, BarType.ldBBBBBBnum14);
                    }
                    break;
                case ModelType.CX52c:

                    SetWheelSizes(WheelSIZES_CX52C);
                    NumberOfBars = 32;
                    OnPropertyChanged("NumberOfBars");
                    for (int i = 1; i < 33; i++)
                    {
                        DefineBarByKnownType(i - 1, BarType.ld000000num13);
                    }
                    DefineBarByKnownType(28 - 1, BarType.ldCCCCCCnum15);
                    DefineBarByKnownType(29 - 1, BarType.ld00CCCCnum68);

                    _model = ModelType.CX52c;
                    OnPropertyChanged("Model");
                    break;
                #endregion
                #region C52_D
                case ModelType.C52d:
                    SetWheelSizes(WheelSIZES_DEFAULT);
                    NumberOfBars = 32;
                    OnPropertyChanged("NumberOfBars");
                    for (int i = 1; i < 33; i++)
                    {
                        DefineBarByKnownType(i - 1, BarType.ld000000num13);
                    }
                    DefineBarByKnownType(1 - 1, BarType.ldCCCCCCnum15);
                    _model = ModelType.C52d;
                    OnPropertyChanged("Model");
                    break;
                #endregion
                #region CXM
                case ModelType.CXM:
                    SetWheelSizes(WHEELS_SIZES_ALL_47);
                    NumberOfBars = 32;
                    OnPropertyChanged("NumberOfBars");

                    DefineBarByKnownType(28 - 1, BarType.ldCA0000num57);
                    DefineBarByKnownType(29 - 1, BarType.ld00A000num5);
                    DefineBarByKnownType(30 - 1, BarType.ld000A00num7);
                    DefineBarByKnownType(31 - 1, BarType.ld0000A0num9);
                    DefineBarByKnownType(32 - 1, BarType.ld00000Anum11);

                    for (int i = 1; i < 28 - 1; i++)
                    {
                        DefineBarByKnownType(i - 1, BarType.ld000000num13);
                    }
                    _model = ModelType.CXM;
                    OnPropertyChanged("Model");
                    break;

                #endregion
                #region CXM_Late
                case ModelType.CXM_LATE_VERSION:
                    SetWheelSizes(WHEELS_SIZES_ALL_47);
                    NumberOfBars = 32;
                    OnPropertyChanged("NumberOfBars");

                    DefineBarByKnownType(28 - 1, BarType.lnCA0000num157);
                    DefineBarByKnownType(29 - 1, BarType.ln00A000num105);
                    DefineBarByKnownType(30 - 1, BarType.ln000A00num107);
                    DefineBarByKnownType(31 - 1, BarType.ln0000A0num109);
                    DefineBarByKnownType(32 - 1, BarType.ln00000Anum111);

                    for (int i = 1; i < 28 - 1; i++)
                    {
                        DefineBarByKnownType(i - 1, BarType.ld000000num13);
                    }
                    _model = ModelType.CXM_LATE_VERSION;
                    OnPropertyChanged("Model");
                    break;
                #endregion
                #region France
                case ModelType.FRANCE:
                    SetWheelSizes(WheelSIZES_DEFAULT);
                    NumberOfBars = 32;
                    OnPropertyChanged("NumberOfBars");
                    for (int i = 0; i < 32; i++)
                    {
                        DefineBarByKnownType(i, BarType.ld000000num13);
                    }
                    DefineBarByKnownType(15 - 1, BarType.ldCCCCCCnum15);
                    DefineBarByKnownType(21 - 1, BarType.ldAAAAAAnum54);
                    DefineBarByKnownType(27 - 1, BarType.ldBBBBBBnum14);
                    _model = ModelType.FRANCE;
                    OnPropertyChanged("Model");

                    break;
                #endregion
                #region EIRE
                case ModelType.EIRE:
                    SetWheelSizes(WheelSIZES_CX52C);
                    NumberOfBars = 32;
                    for (int i = 0; i < 32; i++)
                    {
                        DefineBarByKnownType(i, BarType.ld000000num13);
                    }
                    DefineBarByKnownType(3 - 1, BarType.ld00BBBBnum35);
                    DefineBarByKnownType(6 - 1, BarType.ldB00000num2);
                    DefineBarByKnownType(10 - 1, BarType.ld0B0000num4);
                    DefineBarByKnownType(11 - 1, BarType.ldAA0000num56);
                    DefineBarByKnownType(20 - 1, BarType.ld00AA00num60);
                    DefineBarByKnownType(28 - 1, BarType.ud00CCCCnum16);
                    DefineBarByKnownType(29 - 1, BarType.ld0000AAnum64);
                    _model = ModelType.EIRE;
                    OnPropertyChanged("Model");
                    break;
                #endregion
                #region M209
                case ModelType.M209:
                    SetWheelSizes(WheelSIZES_M209);
                    NumberOfBars = 27;
                    for (int i = 0; i < 27; i++)
                    {
                        DefineBarByKnownType(i, BarType.ld000000num13);
                    }
                    DefineBarByKnownType(1 - 1, BarType.ldCCCCCCnum15);

                    break;
                #endregion

                #region Custom
                case ModelType.Custom:
                    _numberOfWheelsAndBarsShown = true;
                    NumberOfWheels = 6;
                    OnPropertyChanged("NumberOfWheels");                  

                    SetWheelSizes(WheelSIZES_DEFAULT);
                    
                    SupportedWheelTypeNames.Clear();
                    for (int i = 0; i < KNOWN_WheelTYPES.Length; i++)
                    {
                        SupportedWheelTypeNames.Add(KNOWN_WheelTYPES[i]);
                    }



                    for (int i = 1; i < _numberOfBars; i++)
                    {
                        DefineBarByKnownType(i - 1, BarType.ldA00000num1);
                    }

                    _model = ModelType.Custom;

                    OnPropertyChanged("Model");

                    break;
                    #endregion
                    
                default:
                    throw new NotImplementedException(string.Format("Model {0} not implemented", _model));
            }

            SetAllWheelsToFirstPosition();
            OnPropertyChanged("SupportedWheelTypes");
            UpdateSelecteBarsString();
        }

        private void AdaptBarSizesToNumberOfWheels()
        {
            for (int i = 0; i < _numberOfBars; i++)
            {

                while (_barCamsAsString[i].Length < _numberOfWheels)
                {
                    _barCamsAsString[i] += "0";
                    _barType[i] = 0;
                    OnPropertyChanged("Bar" + (i + 1).ToString() + "CamTypes");
                    OnPropertyChanged("Bar" + (i + 1).ToString() + "Type");
                }

                if (_barCamsAsString[i].Length > _numberOfWheels)
                {
                    _barCamsAsString[i] = _barCamsAsString[i].Substring(0, _numberOfWheels);
                    _barType[i] = 0;
                    OnPropertyChanged("Bar" + (i + 1).ToString() + "CamTypes");
                    OnPropertyChanged("Bar" + (i + 1).ToString() + "Type");
                }
            }
        }

        private void DefineBarByKnownType(int barIndex, BarType barType)
        {
            string barTypeString = barType.ToString();

            _barType[barIndex] = Int16.Parse(barTypeString.Substring(11));

            if (barTypeString[0] == 'l')
            {
                _barHasLugs[barIndex] = true;
            }
            else
            {
                _barHasLugs[barIndex] = false;
            }
            _barCamsAsString[barIndex] = barTypeString.Substring(2, 6);

            switch (barTypeString[1])
            {
                case 'd':
                    _barTooth[barIndex] = ToothType.DisplaceWhenShifted;
                    break;
                case 'n':
                    _barTooth[barIndex] = ToothType.NeverDisplace;
                    break;
                case 'i':
                    _barTooth[barIndex] = ToothType.DisplaceWhenNotShifted;
                    break;
                default:
                    break;
            }
            string barName = "Bar" + (barIndex + 1).ToString();
            _barType[barIndex] = (int)barType;
            OnPropertyChanged(barName + "Type");
            OnPropertyChanged(barName + "HasLugs");
            OnPropertyChanged(barName + "CamTypes");
            OnPropertyChanged(barName + "ToothType");

        }

        private void SetWheelSizes(int[] wheelSizes)
        {
            SupportedWheelTypeNames.Clear();
            for (int i = 0; i < wheelSizes.Length; i++)
            {
                bool alreadyAdded = SupportedWheelTypeNames.Contains(wheelSizes[i].ToString());
                if (alreadyAdded)
                    continue;

                SupportedWheelTypeNames.Add(wheelSizes[i].ToString());
            }
          
            List<string> wheelTypes = new List<string>();

            foreach (WheelType wheelType in Enum.GetValues(typeof(WheelType)))
            {
                wheelTypes.Add(wheelType.ToString().Substring(1, 2));
            }

            for (int i = 0; i < _numberOfWheels; i++)
            {

                SetWheelType(i, wheelTypes.IndexOf(wheelSizes[i % (wheelSizes.Length)].ToString()));
            }
        }

        private void SetWheelType(int wheelIndex, int value)
        {
            if (_wheelsTypes[wheelIndex] != (WheelType)value)
            {           
                _wheelsTypes[wheelIndex] = (WheelType)value;
                _wheelTypeNames[wheelIndex] = GetWheelNameFromWheelType((WheelType)value);


                SetWheelStateToFirstPosition(wheelIndex);
                SetPinsForWheel(wheelIndex, "Default");

                StringBuilder stringBuilder = new StringBuilder("");

                for (int i = 0; i < _numberOfWheels; i++)
                {                   
                    stringBuilder.Append(_wheelsTypes[i].ToString().Substring(1, 2) + ", ");
                }
                stringBuilder.Remove(stringBuilder.Length - 2, 2);
                if (_selectedWheels != stringBuilder.ToString())
                {
                    _selectedWheels = stringBuilder.ToString();
                    OnPropertyChanged("SelectedWheels");
                }

                OnPropertyChanged("Wheel" + (wheelIndex + 1).ToString() + "TypeName");
            }

        }

        private void SetWheelStateToFirstPosition(int wheelIndex)
        {
            string typeOfWheel = ((WheelType)_wheelsTypes[wheelIndex]).ToString();
            int startPosition = typeOfWheel.IndexOf('_', 5);
            string wheelPositions = typeOfWheel.Substring(startPosition + 1).Replace("_", ",");
            _wheelsInitialStates[wheelIndex] = wheelPositions;
            OnPropertyChanged("Wheel" + (wheelIndex + 1).ToString() + "InitialState");
            SetExternalKeyShow();
        }

        private void SetAllWheelsToFirstPosition()
        {
            for (int i = 0; i < _numberOfWheels; i++)
            {
                SetWheelStateToFirstPosition(i);
            }
        }


        public void SetPinsForWheel(int wheelIndex, string activePinsPositions)
        {
            StringBuilder newPins = new StringBuilder();
            char[] separator = { ',', ' ' };
            string[] allPositionsList = _wheelsInitialStates[wheelIndex].Split(separator, StringSplitOptions.RemoveEmptyEntries);

            switch (activePinsPositions)
            {
                case "Default": //Every Second Lug is on
                    for (int i = 0; i < allPositionsList.Length; i++)
                    {
                        if ((i + wheelIndex) % 2 == 0)
                        {
                            newPins.Append(allPositionsList[i] + ",");
                        }
                    }
                    break;
                case "Random":
                    Random rnd = new Random();
                    List<int> randomNumbers = new List<int>();
                    for (int i = 0; i < allPositionsList.Length; i++)
                    {
                        int number;
                        do
                        {
                            number = rnd.Next();
                        }
                        while (randomNumbers.Contains(number));
                        randomNumbers.Add(number);

                        if (rnd.Next(0, 2) == 1)
                        {
                            newPins.Append(allPositionsList[i] + ",");

                        }
                    }
                    break;
                default:
                    newPins.Append(activePinsPositions);
                    break;
            }
            if (newPins.ToString().Substring(newPins.Length - 2, 2) == ",")
            {
                newPins.Remove(newPins.Length - 2, 2);
            }
            _wheelsPins[wheelIndex] = newPins.ToString();
            OnPropertyChanged("Wheel" + (wheelIndex + 1).ToString() + "Pins");
        }

        private void SetAllWheelsPinsToRandom()
        {
            for (int i = 0; i < _numberOfWheels; i++)
            {
                SetPinsForWheel(i, "Random");
            }
        }

        public void SetBarLugToValue(int barIndex, string val)
        {
            if (_barHasLugs[barIndex] == false)
            {
                return;
            }
            _barLug[barIndex] = val;
            OnPropertyChanged("Bar" + (barIndex + 1).ToString() + "Lugs");
        }

        private void SetAllLugsToRandom()
        {
            Random random = new Random();
            List<int> randomNumbers = new List<int>();

            for (int i = 0; i < _numberOfBars; i++)
            {
                if (_barHasLugs[i] == false)
                {
                    _barLug[i] = string.Empty;
                    continue;
                }
                else
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    for (int j = 0; j < _numberOfWheels; j++)
                    {
                        int number;
                        do
                        {
                            number = random.Next();
                        }
                        while (randomNumbers.Contains(number));

                        if (random.Next(0, 3) == 1)
                        {
                            stringBuilder.Append((j + 1).ToString() + ",");
                        }
                    }
                    _barLug[i] = stringBuilder.ToString();
                    OnPropertyChanged("Bar" + (i + 1).ToString() + "Lugs");
                }
            }
        }

        #endregion

        #region Methods for setting properties

        #region Wheel Properties

        private void setWheelState(int wheelIndex, string value)
        {
            if (_wheelsInitialStates[wheelIndex] != value)
            {
                _wheelsInitialStates[wheelIndex] = value;
                OnPropertyChanged("Wheel" + (wheelIndex + 1).ToString() + "InitialState");
            }
        }

        private void setWheelPins(int wheelIndex, string value)
        {
            if (_wheelsPins[wheelIndex] != value)
            {
                _wheelsPins[wheelIndex] = value;
                OnPropertyChanged("Wheel" + (wheelIndex + 1).ToString() + "Pins");
            }
        }

        #endregion

        #region Bar Properties

        private void UpdateSelecteBarsString()
        {
            StringBuilder stringBuilder = new StringBuilder("");

            for (int i = 0; i < _numberOfBars; i++)
            {
                int barOrder = _barType[i];
                BarType barType = (BarType)barOrder;
                string BarTypeAsString = barType.ToString();
                string BarNumberString = BarTypeAsString.Substring(11);
                stringBuilder.Append(BarNumberString + ", ");
            }
            stringBuilder.Remove(stringBuilder.Length - 2, 2); //removing last comma and space
            if (_selectedBars != stringBuilder.ToString())
            {
                _selectedBars = stringBuilder.ToString();
                OnPropertyChanged("SelectedBars");
            }
        }

        private void SetBarType(int barIndex, int value)
        {         
            DefineBarByKnownType(barIndex, (BarType)value);

            UpdateSelecteBarsString();

            OnPropertyChanged("Bar" + (barIndex + 1).ToString() + "Type");
        }

        private void SetBarHasLugs(int barIndex, bool value)
        {
            if (_barHasLugs[barIndex] != value)
            {
                _barHasLugs[barIndex] = value;
                OnPropertyChanged("Model");
                OnPropertyChanged("Bar" + (barIndex + 1).ToString() + "Lugs");
                _barType[barIndex] = 0;
                OnPropertyChanged("Bar" + (barIndex + 1).ToString() + "Type");

            }
        }

        private void SetCamTypes(int barIndex, string value)
        {
            if (_barCamsAsString[barIndex] != value)
            {
                _barCamsAsString[barIndex] = value;
                OnPropertyChanged("Model");
                OnPropertyChanged("Bar" + (barIndex + 1).ToString() + "CamTypes");
                _barType[barIndex] = 0;
                OnPropertyChanged("Bar" + (barIndex + 1).ToString() + "Type");
            }
        }

        private void SetBarLugs(int barIndex, string value)
        {
            if (_barLug[barIndex] != value)
            {
                _barLug[barIndex] = value;
                OnPropertyChanged("Bar" + (barIndex + 1).ToString() + "Lugs");
            }
        }

        private void SetBarToothType(int barIndex, ToothType value)
        {
            if (_barTooth[barIndex] != value)
            {
                _barTooth[barIndex] = value;
                OnPropertyChanged("Bar" + (barIndex + 1).ToString() + "ToothType");
            }
        }

        #endregion

        #endregion

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (_initializing)
                return;
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion

        public void Initialize() // used  to hide/show different settings
        {
            _initializing = true;
            ApplyModel();
            SetVisibility();
            SetPluginToState(PluginStates.ModelSelection);
            _initializing = false;
        }
    }
}
