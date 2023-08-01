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
*/
using System;
using System.Linq;
using System.Text;
using static HagelinMachine.HagelinConstants;
using static HagelinMachine.HagelinEnums;

namespace HagelinMachine
{
    public class HagelinImplementation
    {
        private readonly ModelType _model;
        private readonly ModeType _mode;

        private readonly int _numberOfWheels;
        private readonly int _numberOfBars;
        private readonly Wheel[] _wheels = new Wheel[maxNumberOfWheels];
        private readonly Bar[] _bars = new Bar[maxNumberOfBars];
        private readonly string _alphabet;
        private readonly bool _fVFeatureIsActive;

        public int _displacement;
        public string[,] _shownWheelPositions = new string[maxNumberOfWheels, 5];
        public string[] _activeWheelPositions = new string[maxNumberOfWheels];
        public int _curOffsetOfPrintWheel;
        //public StringBuilder _reportStringBuilder = new StringBuilder();
        public bool[] _wheelsWithActivePin = new bool[HagelinConstants.maxNumberOfWheels];
        public int[] _advancementsToShow = new int[maxNumberOfWheels];

        public HagelinImplementation(ModelType model, int numberOfWheels, int numberOfBars, WheelType[] wheelTypes, string[] wheelPins,
                                    string[] wheelsInitialState, int[] barsType, bool[] barsHasLugs,
                                    string[] barsCamsAsString, string[] barsLugValuesAsString, ToothType[] barsTooth, int curOffsetOfPrintWheel, ModeType mode, bool fVFeatureIsActive, string alphabet)
        {
            _model = model;
            _mode = mode;
            _numberOfWheels = numberOfWheels;
            _numberOfBars = numberOfBars;
            _curOffsetOfPrintWheel = curOffsetOfPrintWheel;
            _alphabet = alphabet;
            _fVFeatureIsActive = fVFeatureIsActive;

            for (var i = 0; i < numberOfWheels; i++)
            {
                var size = Int16.Parse(wheelTypes[i].ToString().Substring(1, 2));

                _wheels[i] = new Wheel(size, wheelsInitialState[i], wheelPins[i], wheelTypes[i]);
            }

            for (var i = 0; i < _numberOfBars; i++)
            {
                _bars[i] = new Bar(barsType[i], barsHasLugs[i], barsCamsAsString[i], barsLugValuesAsString[i], barsTooth[i]);
            }

        }

        public class Wheel
        {
            public readonly int _size;
            public readonly WheelType _wheelType;
            public readonly string _pins;
            public readonly string _initialState;

            public int _position;
            public int _advancement;

            public Wheel(int size, string wheelInitialState, string wheelPins, WheelType wheelType)
            {
                _size = size;
                _initialState = wheelInitialState;
                _pins = wheelPins;
                _wheelType = wheelType;
            }

            public void Advance()
            {
                _position = (_position + 1) % _size;

            }
        }

        public class Bar
        {
            public readonly int _barType;
            public readonly bool _barHasLugs;
            public readonly string _barCamsAsString;
            public readonly string _barLugValuesAsString;
            public readonly ToothType _barTooth;

            public Bar(int barType, bool barHasLugs, string barCamsAsString, string barLugValuesAsString, ToothType barTooth)

            {
                _barType = barType;
                _barHasLugs = barHasLugs;
                _barCamsAsString = barCamsAsString;
                _barLugValuesAsString = barLugValuesAsString;
                _barTooth = barTooth;
            }
        }
        public int Displacement
        {
            get
            {
                return _displacement;
            }
        }

        public void UpdateWheelPositions()
        {
            for (int i = 0; i < _numberOfWheels; i++)
            {
                string[] WheelPositionsArray = _wheels[i]._initialState.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                _shownWheelPositions[i, 0] = WheelPositionsArray[Mod((_wheels[i]._position - 2), _wheels[i]._size)];
                _shownWheelPositions[i, 1] = WheelPositionsArray[Mod((_wheels[i]._position - 1), _wheels[i]._size)];
                _shownWheelPositions[i, 2] = WheelPositionsArray[_wheels[i]._position];
                _shownWheelPositions[i, 3] = WheelPositionsArray[Mod((_wheels[i]._position + 1), _wheels[i]._size)];
                _shownWheelPositions[i, 4] = WheelPositionsArray[Mod((_wheels[i]._position + 2), _wheels[i]._size)];
                string[] curWheelTypeSpittedToArray = _wheels[i]._wheelType.ToString().Split('_');
                int offset = int.Parse(curWheelTypeSpittedToArray[1]);

                if (_model == ModelType.M209) // For M209 the offset in the wheels of sizes 25 and 26  between shown and active pins  are different as compared to the ones used in C52c
                {
                    if (_wheels[i]._wheelType == WheelType.W25_9_A_B_C_D_E_F_G_H_I_J_K_L_M_N_O_P_Q_R_S_T_U_V_X_Y_Z)
                        offset = 14;
                    if (_wheels[i]._wheelType == WheelType.W26_11_A_B_C_D_E_F_G_H_I_J_K_L_M_N_O_P_Q_R_S_T_U_V_W_X_Y_Z)
                        offset = 15;
                }

                string curPosition = WheelPositionsArray[(_wheels[i]._position + offset) % _wheels[i]._size];
                _activeWheelPositions[i] = curPosition;

                if (_wheels[i]._pins.IndexOf(curPosition) >= 0)
                {
                    _wheelsWithActivePin[i] = true;
                }
                else 
                {
                    _wheelsWithActivePin[i] = false; 
                }
            }
        }

        //public string ComputeDisplacementAndAdvancement()
        public void ComputeDisplacementAndAdvancement()
        {
            for (int i = 0; i < _numberOfWheels; i++)
            {
                _wheels[i]._advancement = 0;

                string[] WheelPositionsArray = _wheels[i]._initialState.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string[] curWheelTypeSpittedToArray = _wheels[i]._wheelType.ToString().Split('_');
                int offset = int.Parse(curWheelTypeSpittedToArray[1]);


                if (_model == ModelType.M209) // For M209 the offset in the wheels of sizes 25 and 26  between shown and active pins  are different as compared to the ones used in C52c
                {
                    if (_wheels[i]._wheelType == WheelType.W25_9_A_B_C_D_E_F_G_H_I_J_K_L_M_N_O_P_Q_R_S_T_U_V_X_Y_Z)
                        offset = 14;
                    if (_wheels[i]._wheelType == WheelType.W26_11_A_B_C_D_E_F_G_H_I_J_K_L_M_N_O_P_Q_R_S_T_U_V_W_X_Y_Z)
                        offset = 15;
                }


                if (WheelPositionsArray.Length != _wheels[i]._size)
                {
                    //return CrypTool.Plugins.HagelinMachine.Properties.Resources.WheelStateNotValid;
                    return;
                }

                string curPosition = WheelPositionsArray[(_wheels[i]._position + offset) % _wheels[i]._size];

                UpdateWheelPositions();
            }

            _displacement = 0;

            //_reportStringBuilder.Append(CrypTool.Plugins.HagelinMachine.Properties.Resources.PhraseInReportSteps);

            for (int i = 0; i < _numberOfBars; i++)
            {
                bool barActive = false;
                string cur = _bars[i]._barLugValuesAsString.Trim();

                string[] curs = cur.Split(',');

                for (int j = 0; j < _numberOfWheels; j++)
                {
                    if (curs.Contains((j + 1).ToString()) & _wheelsWithActivePin[j]) //how to handle for the last value?
                    {
                        barActive = true;
                        break;
                    }
                }
                for (int j = 0; j < _numberOfWheels; j++)
                {
                    switch (_bars[i]._barCamsAsString[j])
                    {
                        case '0':
                            break;
                        case 'A':
                            if (barActive)
                            {
                                _wheels[j]._advancement++;
                                //_reportStringBuilder.Append(CrypTool.Plugins.HagelinMachine.Properties.Resources.PhraseInReportBar + (i + 1).ToString() + CrypTool.Plugins.HagelinMachine.Properties.Resources.PhraseInReportActiveAgainst + (j + 1).ToString() + CrypTool.Plugins.HagelinMachine.Properties.Resources.PhraseInReportCamTypeA + "\r");
                                //_reportStringBuilder.Append(CrypTool.Plugins.HagelinMachine.Properties.Resources.PhraseInReportWheel + (j + 1).ToString() + CrypTool.Plugins.HagelinMachine.Properties.Resources.PhraseInReportIsFurtherAdvanced + _wheels[j]._advancement + "\r");
                            }
                            break;
                        case 'B':
                            if (!barActive)
                            {
                                _wheels[j]._advancement++;
                                //_reportStringBuilder.Append(CrypTool.Plugins.HagelinMachine.Properties.Resources.PhraseInReportBar + (i + 1).ToString() + CrypTool.Plugins.HagelinMachine.Properties.Resources.PhraseInReportNotActiveAgainst + (j + 1).ToString() + CrypTool.Plugins.HagelinMachine.Properties.Resources.PhraseInReportCamTypeB + "\r");
                                //_reportStringBuilder.Append(CrypTool.Plugins.HagelinMachine.Properties.Resources.PhraseInReportWheel + (j + 1).ToString() + "" + _wheels[j]._advancement + "\r");
                            }
                            break;
                        case 'C':
                            _wheels[j]._advancement++;
                            break;
                        default:
                            break;
                    }

                }

                switch (_bars[i]._barTooth)
                {
                    case HagelinEnums.ToothType.DisplaceWhenShifted:
                        if (barActive)
                        {
                            _displacement++;
                            //_reportStringBuilder.Append(CrypTool.Plugins.HagelinMachine.Properties.Resources.PhraseInReportBar + (i + 1).ToString() + CrypTool.Plugins.HagelinMachine.Properties.Resources.PhraseInReportShifted + _displacement.ToString() + "\r");
                        }
                        break;
                    case HagelinEnums.ToothType.NeverDisplace:
                        break;
                    case HagelinEnums.ToothType.DisplaceWhenNotShifted:
                        if (!barActive)
                        {
                            _displacement++;
                            //_reportStringBuilder.Append(CrypTool.Plugins.HagelinMachine.Properties.Resources.PhraseInReportBar + (i + 1).ToString() + CrypTool.Plugins.HagelinMachine.Properties.Resources.PhraseInReportNotShifted + _displacement.ToString() + "\r");
                        }
                        break;
                    default:
                        break;
                }

            }

            if (_displacement >= 26)
            {
                _displacement -= 26;
            }

            //return _reportStringBuilder.ToString();
        }

        public char EncryptDecryptOneCharacter(char symbol)
        {
            if (symbol >= 'a' && symbol <= 'z')
            {
                symbol = (char)(symbol - 32);
            }

            int i = _alphabet.IndexOf(symbol);
            if (i < 0)
            {
                return symbol;
            }

            ComputeDisplacementAndAdvancement();

            int j;
            switch (_mode)
            {
                case ModeType.Encrypt:
                    j = Mod((_alphabet.IndexOf('Z') - i + _displacement + _curOffsetOfPrintWheel), 26);
                    break;
                case ModeType.Decrypt:
                    j = Mod((_alphabet.IndexOf('Z') - i + _displacement + _curOffsetOfPrintWheel), 26);
                    break;
                default:
                    j = Mod((_alphabet.IndexOf('Z') - i + _displacement + _curOffsetOfPrintWheel), 26);
                    break;
            }

            //_reportStringBuilder.Append(CrypTool.Plugins.HagelinMachine.Properties.Resources.PhraseInReportOffset + _curOffsetOfPrintWheel.ToString());
            //_reportStringBuilder.Append(CrypTool.Plugins.HagelinMachine.Properties.Resources.PhraseInReportCoputedDisplacement + _displacement.ToString());
            //_reportStringBuilder.Append(CrypTool.Plugins.HagelinMachine.Properties.Resources.PhraseInReportInputCharacter + symbol);
            //_reportStringBuilder.Append(CrypTool.Plugins.HagelinMachine.Properties.Resources.PhraseInReportOutputCharacter + " (\'Z\' - \'" + symbol + "\' + D + O ) mod 26 = \'" + _alphabet[j].ToString() + "\'\r");
            //_reportStringBuilder.Append("\rModel: " + _model.ToString());

            for (int ii = 0; ii < _numberOfWheels; ii++)
            {
                for (int jj = 0; jj < _wheels[ii]._advancement; jj++)
                {
                    _wheels[ii].Advance();
                }
                _advancementsToShow[ii] = _wheels[ii]._advancement;
            }

            if (_fVFeatureIsActive)
            {
                _curOffsetOfPrintWheel += _displacement;
                //_reportStringBuilder.Append(CrypTool.Plugins.HagelinMachine.Properties.Resources.PhraseInReportFVFeatureActive + _curOffsetOfPrintWheel.ToString() + "\r");
            }

            return _alphabet[j];
        }
        
        private int Mod(int x, int m)
        {
            return (x % m + m) % m;
        }
    }
}
