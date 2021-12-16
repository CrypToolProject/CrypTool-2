/*
   Copyright 2019 Christian Bender christian1.bender@student.uni-siegen.de

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

using DCAPathVisualiser.Logic;
using DCAPathVisualiser.UI.Cipher1;
using DCAPathVisualiser.UI.Cipher2;
using DCAPathVisualiser.UI.Cipher3;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace DCAPathVisualiser.UI
{
    /// <summary>
    /// Interaktionslogik für PathVisualiserPres.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("DCAPathVisualiser.Properties.Resources")]
    public partial class PathVisualiserPres : INotifyPropertyChanged
    {
        private UserControl _cipherControl;
        private UserControl _tableControl;
        private bool _workspaceRunning;
        private Algorithms _currentAlgorithm;
        private DifferentialAttackRoundConfiguration _currentConfigurationToDisplay;

        /// <summary>
        /// Constructor
        /// </summary>
        public PathVisualiserPres()
        {
            InitializeComponent();
        }

        public void RenderView()
        {
            string activeSBoxes;

            switch (CurrentAlgorithm)
            {
                case Algorithms.Cipher1:
                    break;

                case Algorithms.Cipher2:
                    ((Cipher2Table)_tableControl).Characteristics.Clear();

                    activeSBoxes = "";
                    for (int i = _currentConfigurationToDisplay.ActiveSBoxes.Length - 1; i >= 0; i--)
                    {
                        if (_currentConfigurationToDisplay.ActiveSBoxes[i])
                        {
                            activeSBoxes += "1";
                        }
                        else
                        {
                            activeSBoxes += "0";
                        }
                    }

                    //if we are not in the last round
                    if (CurrentConfigurationToDisplay.Round > 1)
                    {
                        //add data into table header
                        ((Cipher2Table)_tableControl).CurrentActiveSBoxes = activeSBoxes;
                        ((Cipher2Table)_tableControl).CurrentCountOfCharacteristics =
                            _currentConfigurationToDisplay.Characteristics.Count;
                        ((Cipher2Table)_tableControl).CurrentInputDiff = Convert
                            .ToString(_currentConfigurationToDisplay.InputDifference, 2).PadLeft(16, '0')
                            .Insert(8, " ");
                        ((Cipher2Table)_tableControl).CurrentExpectedDiff = Convert
                            .ToString(_currentConfigurationToDisplay.ExpectedDifference, 2).PadLeft(16, '0')
                            .Insert(8, " ");
                        ((Cipher2Table)_tableControl).CurrentProbability =
                            string.Format("{0:0.0000}", _currentConfigurationToDisplay.Probability);
                        ((Cipher2Table)_tableControl).CurrentRound = _currentConfigurationToDisplay.Round;

                        switch (CurrentConfigurationToDisplay.Round)
                        {
                            case 3:
                                {
                                    ((Cipher2Table)_tableControl).DataGridCharacteristicsOutputDiffR2Col.Visibility =
                                        Visibility.Visible;
                                    ((Cipher2Table)_tableControl).DataGridCharacteristicsInputDiffR3Col.Visibility =
                                        Visibility.Visible;

                                    //add data into table
                                    foreach (Characteristic characteristic in _currentConfigurationToDisplay.Characteristics)
                                    {
                                        UI.Models.Cipher2CharacteristicUI charTable =
                                            new Models.Cipher2CharacteristicUI()
                                            {
                                                InputDiff = Convert.ToString(characteristic.InputDifferentials[0], 2)
                                                    .PadLeft(16, '0').Insert(8, " "),
                                                InputDiffR1 = Convert.ToString(characteristic.InputDifferentials[0], 2)
                                                    .PadLeft(16, '0').Insert(8, " "),
                                                OutputDiffR1 = Convert.ToString(characteristic.OutputDifferentials[0], 2)
                                                    .PadLeft(16, '0').Insert(8, " "),
                                                InputDiffR2 = Convert.ToString(characteristic.InputDifferentials[1], 2)
                                                    .PadLeft(16, '0').Insert(8, " "),
                                                OutputDiffR2 = Convert.ToString(characteristic.OutputDifferentials[1], 2)
                                                    .PadLeft(16, '0').Insert(8, " "),
                                                ExpectedDiff = Convert.ToString(characteristic.InputDifferentials[2], 2)
                                                    .PadLeft(16, '0').Insert(8, " "),

                                                InputDiffInt = characteristic.InputDifferentials[0],
                                                InputDiffR1Int = characteristic.InputDifferentials[0],
                                                OutputDiffR1Int = characteristic.OutputDifferentials[0],
                                                InputDiffR2Int = characteristic.InputDifferentials[1],
                                                OutputDiffR2Int = characteristic.OutputDifferentials[1],
                                                ExpectedDiffInt = characteristic.InputDifferentials[2]
                                            };

                                        ((Cipher2Table)_tableControl).Characteristics.Add(charTable);
                                    }
                                }
                                break;
                            case 2:
                                {
                                    ((Cipher2Table)_tableControl).DataGridCharacteristicsOutputDiffR2Col.Visibility =
                                        Visibility.Hidden;
                                    ((Cipher2Table)_tableControl).DataGridCharacteristicsInputDiffR3Col.Visibility =
                                        Visibility.Hidden;

                                    //add data into table
                                    foreach (Characteristic characteristic in _currentConfigurationToDisplay.Characteristics)
                                    {
                                        Models.Cipher2CharacteristicUI charTable =
                                            new UI.Models.Cipher2CharacteristicUI()
                                            {
                                                InputDiff = Convert.ToString(characteristic.InputDifferentials[0], 2)
                                                    .PadLeft(16, '0').Insert(8, " "),
                                                InputDiffR1 = Convert.ToString(characteristic.InputDifferentials[0], 2)
                                                    .PadLeft(16, '0').Insert(8, " "),
                                                OutputDiffR1 = Convert.ToString(characteristic.OutputDifferentials[0], 2)
                                                    .PadLeft(16, '0').Insert(8, " "),
                                                InputDiffR2 = Convert.ToString(characteristic.InputDifferentials[1], 2)
                                                    .PadLeft(16, '0').Insert(8, " "),

                                                InputDiffInt = characteristic.InputDifferentials[0],
                                                InputDiffR1Int = characteristic.InputDifferentials[0],
                                                OutputDiffR1Int = characteristic.OutputDifferentials[0],
                                                InputDiffR2Int = characteristic.InputDifferentials[1],
                                                OutputDiffR2Int = characteristic.OutputDifferentials[1],
                                                ExpectedDiffInt = characteristic.InputDifferentials[2]
                                            };

                                        ((Cipher2Table)_tableControl).Characteristics.Add(charTable);
                                    }
                                }
                                break;
                        }

                        //select first element
                        Models.Cipher2CharacteristicUI firstElem = ((Cipher2Table)_tableControl).Characteristics.FirstOrDefault();
                        if (firstElem != null)
                        {
                            ((Cipher2Table)_tableControl).DataGridCharacteristics.SelectedItem = firstElem;
                            Cipher2CharacteristicToShowChanged(this, new Cipher2CharacteristicSelectionEventArgs()
                            {
                                SelectedCharacteristic = firstElem
                            });
                        }
                    }
                    else
                    {
                        //add data into table header
                        ((Cipher2Table)_tableControl).CurrentActiveSBoxes = activeSBoxes;
                        ((Cipher2Table)_tableControl).CurrentCountOfCharacteristics = 0;
                        ((Cipher2Table)_tableControl).CurrentInputDiff =
                            Properties.Resources.TableHeaderLastRoundInputDiff;
                        ((Cipher2Table)_tableControl).CurrentExpectedDiff =
                            Properties.Resources.TableHeaderLastRoundExpectedDiff;
                        ((Cipher2Table)_tableControl).CurrentProbability =
                            Properties.Resources.TableHeaderLastRoundProbability;
                        ((Cipher2Table)_tableControl).CurrentRound = _currentConfigurationToDisplay.Round;
                    }

                    break;

                case Algorithms.Cipher3:

                    ((Cipher3Table)_tableControl).Characteristics.Clear();

                    activeSBoxes = "";
                    for (int i = _currentConfigurationToDisplay.ActiveSBoxes.Length - 1; i >= 0; i--)
                    {
                        if (_currentConfigurationToDisplay.ActiveSBoxes[i])
                        {
                            activeSBoxes += "1";
                        }
                        else
                        {
                            activeSBoxes += "0";
                        }
                    }

                    //if we are not in the last round
                    if (CurrentConfigurationToDisplay.Round > 1)
                    {
                        //add data into table header
                        ((Cipher3Table)_tableControl).CurrentActiveSBoxes = activeSBoxes;
                        ((Cipher3Table)_tableControl).CurrentCountOfCharacteristics =
                            _currentConfigurationToDisplay.Characteristics.Count;
                        ((Cipher3Table)_tableControl).CurrentInputDiff = Convert
                            .ToString(_currentConfigurationToDisplay.InputDifference, 2).PadLeft(16, '0')
                            .Insert(8, " ");
                        ((Cipher3Table)_tableControl).CurrentExpectedDiff = Convert
                            .ToString(_currentConfigurationToDisplay.ExpectedDifference, 2).PadLeft(16, '0')
                            .Insert(8, " ");
                        ((Cipher3Table)_tableControl).CurrentProbability =
                            string.Format("{0:0.0000}", _currentConfigurationToDisplay.Probability);
                        ((Cipher3Table)_tableControl).CurrentRound = _currentConfigurationToDisplay.Round;

                        switch (CurrentConfigurationToDisplay.Round)
                        {
                            case 5:
                                ((Cipher3Table)_tableControl).DataGridCharacteristicsInputDiffR5Col.Visibility =
                                    Visibility.Visible;
                                ((Cipher3Table)_tableControl).DataGridCharacteristicsOutputDiffR4Col.Visibility =
                                    Visibility.Visible;
                                ((Cipher3Table)_tableControl).DataGridCharacteristicsInputDiffR4Col.Visibility =
                                    Visibility.Visible;
                                ((Cipher3Table)_tableControl).DataGridCharacteristicsOutputDiffR3Col.Visibility =
                                    Visibility.Visible;
                                ((Cipher3Table)_tableControl).DataGridCharacteristicsInputDiffR3Col.Visibility =
                                    Visibility.Visible;
                                ((Cipher3Table)_tableControl).DataGridCharacteristicsOutputDiffR2Col.Visibility =
                                    Visibility.Visible;
                                ((Cipher3Table)_tableControl).DataGridCharacteristicsInputDiffR2Col.Visibility =
                                    Visibility.Visible;

                                //add data into table
                                foreach (Characteristic characteristic in _currentConfigurationToDisplay.Characteristics)
                                {
                                    Models.Cipher3CharacteristicUI charTable =
                                        new Models.Cipher3CharacteristicUI()
                                        {
                                            InputDiff = Convert.ToString(characteristic.InputDifferentials[0], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            InputDiffR1 = Convert.ToString(characteristic.InputDifferentials[0], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            OutputDiffR1 = Convert.ToString(characteristic.OutputDifferentials[0], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            InputDiffR2 = Convert.ToString(characteristic.InputDifferentials[1], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            OutputDiffR2 = Convert.ToString(characteristic.OutputDifferentials[1], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            InputDiffR3 = Convert.ToString(characteristic.InputDifferentials[2], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            OutputDiffR3 = Convert.ToString(characteristic.OutputDifferentials[2], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            InputDiffR4 = Convert.ToString(characteristic.InputDifferentials[3], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            OutputDiffR4 = Convert.ToString(characteristic.OutputDifferentials[3], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            InputDiffR5 = Convert.ToString(characteristic.InputDifferentials[4], 2)
                                                .PadLeft(16, '0').Insert(8, " "),

                                            InputDiffInt = characteristic.InputDifferentials[0],
                                            InputDiffR1Int = characteristic.InputDifferentials[0],
                                            OutputDiffR1Int = characteristic.OutputDifferentials[0],
                                            InputDiffR2Int = characteristic.InputDifferentials[1],
                                            OutputDiffR2Int = characteristic.OutputDifferentials[1],
                                            InputDiffR3Int = characteristic.InputDifferentials[2],
                                            OutputDiffR3Int = characteristic.OutputDifferentials[2],
                                            InputDiffR4Int = characteristic.InputDifferentials[3],
                                            OutputDiffR4Int = characteristic.OutputDifferentials[3],
                                            InputDiffR5Int = characteristic.InputDifferentials[4],
                                        };

                                    ((Cipher3Table)_tableControl).Characteristics.Add(charTable);
                                }

                                break;
                            case 4:
                                ((Cipher3Table)_tableControl).DataGridCharacteristicsInputDiffR5Col.Visibility =
                                    Visibility.Hidden;
                                ((Cipher3Table)_tableControl).DataGridCharacteristicsOutputDiffR4Col.Visibility =
                                    Visibility.Hidden;
                                ((Cipher3Table)_tableControl).DataGridCharacteristicsInputDiffR4Col.Visibility =
                                    Visibility.Visible;
                                ((Cipher3Table)_tableControl).DataGridCharacteristicsOutputDiffR3Col.Visibility =
                                    Visibility.Visible;
                                ((Cipher3Table)_tableControl).DataGridCharacteristicsInputDiffR3Col.Visibility =
                                    Visibility.Visible;
                                ((Cipher3Table)_tableControl).DataGridCharacteristicsOutputDiffR2Col.Visibility =
                                    Visibility.Visible;
                                ((Cipher3Table)_tableControl).DataGridCharacteristicsInputDiffR2Col.Visibility =
                                    Visibility.Visible;

                                //add data into table
                                foreach (Characteristic characteristic in _currentConfigurationToDisplay.Characteristics)
                                {
                                    Models.Cipher3CharacteristicUI charTable =
                                        new Models.Cipher3CharacteristicUI()
                                        {
                                            InputDiff = Convert.ToString(characteristic.InputDifferentials[0], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            InputDiffR1 = Convert.ToString(characteristic.InputDifferentials[0], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            OutputDiffR1 = Convert.ToString(characteristic.OutputDifferentials[0], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            InputDiffR2 = Convert.ToString(characteristic.InputDifferentials[1], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            OutputDiffR2 = Convert.ToString(characteristic.OutputDifferentials[1], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            InputDiffR3 = Convert.ToString(characteristic.InputDifferentials[2], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            OutputDiffR3 = Convert.ToString(characteristic.OutputDifferentials[2], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            InputDiffR4 = Convert.ToString(characteristic.InputDifferentials[3], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            OutputDiffR4 = Convert.ToString(characteristic.OutputDifferentials[3], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            InputDiffR5 = Convert.ToString(characteristic.InputDifferentials[4], 2)
                                                .PadLeft(16, '0').Insert(8, " "),

                                            InputDiffInt = characteristic.InputDifferentials[0],
                                            InputDiffR1Int = characteristic.InputDifferentials[0],
                                            OutputDiffR1Int = characteristic.OutputDifferentials[0],
                                            InputDiffR2Int = characteristic.InputDifferentials[1],
                                            OutputDiffR2Int = characteristic.OutputDifferentials[1],
                                            InputDiffR3Int = characteristic.InputDifferentials[2],
                                            OutputDiffR3Int = characteristic.OutputDifferentials[2],
                                            InputDiffR4Int = characteristic.InputDifferentials[3],
                                            OutputDiffR4Int = characteristic.OutputDifferentials[3],
                                            InputDiffR5Int = characteristic.InputDifferentials[4],
                                        };

                                    ((Cipher3Table)_tableControl).Characteristics.Add(charTable);
                                }

                                break;
                            case 3:
                                ((Cipher3Table)_tableControl).DataGridCharacteristicsInputDiffR5Col.Visibility =
                                    Visibility.Hidden;
                                ((Cipher3Table)_tableControl).DataGridCharacteristicsOutputDiffR4Col.Visibility =
                                    Visibility.Hidden;
                                ((Cipher3Table)_tableControl).DataGridCharacteristicsInputDiffR4Col.Visibility =
                                    Visibility.Hidden;
                                ((Cipher3Table)_tableControl).DataGridCharacteristicsOutputDiffR3Col.Visibility =
                                    Visibility.Hidden;
                                ((Cipher3Table)_tableControl).DataGridCharacteristicsInputDiffR3Col.Visibility =
                                    Visibility.Visible;
                                ((Cipher3Table)_tableControl).DataGridCharacteristicsOutputDiffR2Col.Visibility =
                                    Visibility.Visible;
                                ((Cipher3Table)_tableControl).DataGridCharacteristicsInputDiffR2Col.Visibility =
                                    Visibility.Visible;

                                //add data into table
                                foreach (Characteristic characteristic in _currentConfigurationToDisplay.Characteristics)
                                {
                                    Models.Cipher3CharacteristicUI charTable =
                                        new Models.Cipher3CharacteristicUI()
                                        {
                                            InputDiff = Convert.ToString(characteristic.InputDifferentials[0], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            InputDiffR1 = Convert.ToString(characteristic.InputDifferentials[0], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            OutputDiffR1 = Convert.ToString(characteristic.OutputDifferentials[0], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            InputDiffR2 = Convert.ToString(characteristic.InputDifferentials[1], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            OutputDiffR2 = Convert.ToString(characteristic.OutputDifferentials[1], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            InputDiffR3 = Convert.ToString(characteristic.InputDifferentials[2], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            OutputDiffR3 = Convert.ToString(characteristic.OutputDifferentials[2], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            InputDiffR4 = Convert.ToString(characteristic.InputDifferentials[3], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            OutputDiffR4 = Convert.ToString(characteristic.OutputDifferentials[3], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            InputDiffR5 = Convert.ToString(characteristic.InputDifferentials[4], 2)
                                                .PadLeft(16, '0').Insert(8, " "),

                                            InputDiffInt = characteristic.InputDifferentials[0],
                                            InputDiffR1Int = characteristic.InputDifferentials[0],
                                            OutputDiffR1Int = characteristic.OutputDifferentials[0],
                                            InputDiffR2Int = characteristic.InputDifferentials[1],
                                            OutputDiffR2Int = characteristic.OutputDifferentials[1],
                                            InputDiffR3Int = characteristic.InputDifferentials[2],
                                            OutputDiffR3Int = characteristic.OutputDifferentials[2],
                                            InputDiffR4Int = characteristic.InputDifferentials[3],
                                            OutputDiffR4Int = characteristic.OutputDifferentials[3],
                                            InputDiffR5Int = characteristic.InputDifferentials[4],
                                        };

                                    ((Cipher3Table)_tableControl).Characteristics.Add(charTable);
                                }

                                break;
                            case 2:
                                ((Cipher3Table)_tableControl).DataGridCharacteristicsInputDiffR5Col.Visibility =
                                    Visibility.Hidden;
                                ((Cipher3Table)_tableControl).DataGridCharacteristicsOutputDiffR4Col.Visibility =
                                    Visibility.Hidden;
                                ((Cipher3Table)_tableControl).DataGridCharacteristicsInputDiffR4Col.Visibility =
                                    Visibility.Hidden;
                                ((Cipher3Table)_tableControl).DataGridCharacteristicsOutputDiffR3Col.Visibility =
                                    Visibility.Hidden;
                                ((Cipher3Table)_tableControl).DataGridCharacteristicsInputDiffR3Col.Visibility =
                                    Visibility.Hidden;
                                ((Cipher3Table)_tableControl).DataGridCharacteristicsOutputDiffR2Col.Visibility =
                                    Visibility.Hidden;
                                ((Cipher3Table)_tableControl).DataGridCharacteristicsInputDiffR2Col.Visibility =
                                    Visibility.Visible;

                                //add data into table
                                foreach (Characteristic characteristic in _currentConfigurationToDisplay.Characteristics)
                                {
                                    Models.Cipher3CharacteristicUI charTable =
                                        new Models.Cipher3CharacteristicUI()
                                        {
                                            InputDiff = Convert.ToString(characteristic.InputDifferentials[0], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            InputDiffR1 = Convert.ToString(characteristic.InputDifferentials[0], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            OutputDiffR1 = Convert.ToString(characteristic.OutputDifferentials[0], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            InputDiffR2 = Convert.ToString(characteristic.InputDifferentials[1], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            OutputDiffR2 = Convert.ToString(characteristic.OutputDifferentials[1], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            InputDiffR3 = Convert.ToString(characteristic.InputDifferentials[2], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            OutputDiffR3 = Convert.ToString(characteristic.OutputDifferentials[2], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            InputDiffR4 = Convert.ToString(characteristic.InputDifferentials[3], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            OutputDiffR4 = Convert.ToString(characteristic.OutputDifferentials[3], 2)
                                                .PadLeft(16, '0').Insert(8, " "),
                                            InputDiffR5 = Convert.ToString(characteristic.InputDifferentials[4], 2)
                                                .PadLeft(16, '0').Insert(8, " "),

                                            InputDiffInt = characteristic.InputDifferentials[0],
                                            InputDiffR1Int = characteristic.InputDifferentials[0],
                                            OutputDiffR1Int = characteristic.OutputDifferentials[0],
                                            InputDiffR2Int = characteristic.InputDifferentials[1],
                                            OutputDiffR2Int = characteristic.OutputDifferentials[1],
                                            InputDiffR3Int = characteristic.InputDifferentials[2],
                                            OutputDiffR3Int = characteristic.OutputDifferentials[2],
                                            InputDiffR4Int = characteristic.InputDifferentials[3],
                                            OutputDiffR4Int = characteristic.OutputDifferentials[3],
                                            InputDiffR5Int = characteristic.InputDifferentials[4],
                                        };

                                    ((Cipher3Table)_tableControl).Characteristics.Add(charTable);
                                }

                                break;
                        }

                        //select first element
                        Models.Cipher3CharacteristicUI firstElem = ((Cipher3Table)_tableControl).Characteristics.FirstOrDefault();
                        if (firstElem != null)
                        {
                            ((Cipher3Table)_tableControl).DataGridCharacteristics.SelectedItem = firstElem;
                            Cipher3CharacteristicToShowChanged(this, new Cipher3CharacteristicSelectionEventArgs()
                            {
                                SelectedCharacteristic = firstElem
                            });
                        }
                    }
                    else
                    {
                        //add data into table header
                        ((Cipher3Table)_tableControl).CurrentActiveSBoxes = activeSBoxes;
                        ((Cipher3Table)_tableControl).CurrentCountOfCharacteristics = 0;
                        ((Cipher3Table)_tableControl).CurrentInputDiff =
                            Properties.Resources.TableHeaderLastRoundInputDiff;
                        ((Cipher3Table)_tableControl).CurrentExpectedDiff =
                            Properties.Resources.TableHeaderLastRoundExpectedDiff;
                        ((Cipher3Table)_tableControl).CurrentProbability =
                            Properties.Resources.TableHeaderLastRoundProbability;
                        ((Cipher3Table)_tableControl).CurrentRound = _currentConfigurationToDisplay.Round;

                        ((Cipher3Table)_tableControl).DataGridCharacteristicsInputDiffR5Col.Visibility =
                            Visibility.Hidden;
                        ((Cipher3Table)_tableControl).DataGridCharacteristicsOutputDiffR4Col.Visibility =
                            Visibility.Hidden;
                        ((Cipher3Table)_tableControl).DataGridCharacteristicsInputDiffR4Col.Visibility =
                            Visibility.Hidden;
                        ((Cipher3Table)_tableControl).DataGridCharacteristicsOutputDiffR3Col.Visibility =
                            Visibility.Hidden;
                        ((Cipher3Table)_tableControl).DataGridCharacteristicsInputDiffR3Col.Visibility =
                            Visibility.Hidden;
                        ((Cipher3Table)_tableControl).DataGridCharacteristicsOutputDiffR2Col.Visibility =
                            Visibility.Hidden;
                        ((Cipher3Table)_tableControl).DataGridCharacteristicsInputDiffR2Col.Visibility =
                            Visibility.Hidden;
                    }
                    break;
            }
        }

        #region Properties

        /// <summary>
        /// Property for _currentConfigurationToDisplay
        /// </summary>
        public DifferentialAttackRoundConfiguration CurrentConfigurationToDisplay
        {
            set
            {
                _currentConfigurationToDisplay = value;
                OnPropertyChanged();
            }
            get => _currentConfigurationToDisplay;
        }

        /// <summary>
        /// Property for _workspaceRunning
        /// </summary>
        public bool WorkspaceRunning
        {
            get => _workspaceRunning;
            set
            {
                _workspaceRunning = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for current algorithm
        /// </summary>
        public Algorithms CurrentAlgorithm
        {
            get => _currentAlgorithm;
            set
            {
                _currentAlgorithm = value;

                switch (_currentAlgorithm)
                {
                    case Algorithms.Cipher1:
                        {
                            /* */
                            _cipherControl = new Cipher1Characteristic();
                            _tableControl = new Cipher1Table();
                            TabItemCipher.Content = _cipherControl;
                            TabItemTable.Child = _tableControl;

                        }
                        break;
                    case Algorithms.Cipher2:
                        {
                            _cipherControl = new Cipher2Characteristic();
                            _tableControl = new Cipher2Table();
                            TabItemCipher.Content = _cipherControl;
                            TabItemTable.Child = _tableControl;

                            ((Cipher2Table)_tableControl).SelectionChanged += Cipher2CharacteristicToShowChanged;
                        }
                        break;
                    case Algorithms.Cipher3:
                        {
                            _cipherControl = new Cipher3Characteristic();
                            _tableControl = new Cipher3Table();
                            TabItemCipher.Content = _cipherControl;
                            TabItemTable.Child = _tableControl;

                            ((Cipher3Table)_tableControl).SelectionChanged += Cipher3CharacteristicToShowChanged;
                        }
                        break;
                }

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Handles change of the characteristic to display in the cipher view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cipher3CharacteristicToShowChanged(object sender, Cipher3CharacteristicSelectionEventArgs e)
        {
            if (_currentConfigurationToDisplay == null || e.SelectedCharacteristic == null)
            {
                bool[] d = new bool[16];
                for (int i = 0; i < 16; i++)
                {
                    d[i] = false;
                }

                ((Cipher3Characteristic)_cipherControl).BitsKeyRoundOne = d;
                ((Cipher3Characteristic)_cipherControl).PermutationBitsRoundOne = d;


                ((Cipher3Characteristic)_cipherControl).BitsKeyRoundTwo = d;
                ((Cipher3Characteristic)_cipherControl).PermutationBitsRoundTwo = d;

                ((Cipher3Characteristic)_cipherControl).BitsKeyRoundThree = d;
                ((Cipher3Characteristic)_cipherControl).PermutationBitsRoundThree = d;

                ((Cipher3Characteristic)_cipherControl).BitsKeyRoundFour = d;
                ((Cipher3Characteristic)_cipherControl).PermutationBitsRoundFour = d;

                ((Cipher3Characteristic)_cipherControl).BitsKeyRoundFive = d;
                ((Cipher3Characteristic)_cipherControl).BitsKeyRoundSix = d;

                ((Cipher3Characteristic)_cipherControl).InputDiff = "";
                ((Cipher3Characteristic)_cipherControl).Round1InputDiff = "";
                ((Cipher3Characteristic)_cipherControl).Round1OutputDiff = "";
                ((Cipher3Characteristic)_cipherControl).Round2InputDiff = "";
                ((Cipher3Characteristic)_cipherControl).Round2OutputDiff = "";
                ((Cipher3Characteristic)_cipherControl).Round3InputDiff = "";
                ((Cipher3Characteristic)_cipherControl).Round3OutputDiff = "";
                ((Cipher3Characteristic)_cipherControl).Round4InputDiff = "";
                ((Cipher3Characteristic)_cipherControl).Round4OutputDiff = "";
                ((Cipher3Characteristic)_cipherControl).Round5InputDiff = "";

                ((Cipher3Characteristic)_cipherControl).ActiveSBoxes = new bool[4];

                return;
            }

            foreach (Characteristic curCharacteristic in _currentConfigurationToDisplay.Characteristics)
            {
                //find selected one
                if (curCharacteristic.InputDifferentials[0] == e.SelectedCharacteristic.InputDiffInt &&
                    curCharacteristic.InputDifferentials[0] == e.SelectedCharacteristic.InputDiffR1Int &&
                    curCharacteristic.OutputDifferentials[0] == e.SelectedCharacteristic.OutputDiffR1Int &&
                    curCharacteristic.InputDifferentials[1] == e.SelectedCharacteristic.InputDiffR2Int &&
                    curCharacteristic.OutputDifferentials[1] == e.SelectedCharacteristic.OutputDiffR2Int &&
                    curCharacteristic.InputDifferentials[2] == e.SelectedCharacteristic.InputDiffR3Int &&
                    curCharacteristic.OutputDifferentials[2] == e.SelectedCharacteristic.OutputDiffR3Int &&
                    curCharacteristic.InputDifferentials[3] == e.SelectedCharacteristic.InputDiffR4Int &&
                    curCharacteristic.OutputDifferentials[3] == e.SelectedCharacteristic.OutputDiffR4Int &&
                    curCharacteristic.InputDifferentials[4] == e.SelectedCharacteristic.InputDiffR5Int)
                {
                    if (curCharacteristic is Logic.Cipher3.Cipher3Characteristic charToDisplay)
                    {
                        BitArray bits = new BitArray(BitConverter.GetBytes(charToDisplay.InputDifferentials[0]));
                        bool[] d = new bool[16];
                        for (int i = 0; i < 16; i++)
                        {
                            d[i] = bits[i];
                        }

                        ((Cipher3Characteristic)_cipherControl).BitsKeyRoundOne = d;

                        bits = new BitArray(BitConverter.GetBytes(charToDisplay.OutputDifferentials[0]));
                        d = new bool[16];
                        for (int i = 0; i < 16; i++)
                        {
                            d[i] = bits[i];
                        }

                        ((Cipher3Characteristic)_cipherControl).PermutationBitsRoundOne = d;

                        bits = new BitArray(BitConverter.GetBytes(charToDisplay.InputDifferentials[1]));
                        d = new bool[16];
                        for (int i = 0; i < 16; i++)
                        {
                            d[i] = bits[i];
                        }

                        ((Cipher3Characteristic)_cipherControl).BitsKeyRoundTwo = d;

                        bits = new BitArray(BitConverter.GetBytes(charToDisplay.OutputDifferentials[1]));
                        d = new bool[16];
                        for (int i = 0; i < 16; i++)
                        {
                            d[i] = bits[i];
                        }

                        ((Cipher3Characteristic)_cipherControl).PermutationBitsRoundTwo = d;

                        bits = new BitArray(BitConverter.GetBytes(charToDisplay.InputDifferentials[2]));
                        d = new bool[16];
                        for (int i = 0; i < 16; i++)
                        {
                            d[i] = bits[i];
                        }

                        ((Cipher3Characteristic)_cipherControl).BitsKeyRoundThree = d;

                        bits = new BitArray(BitConverter.GetBytes(charToDisplay.OutputDifferentials[2]));
                        d = new bool[16];
                        for (int i = 0; i < 16; i++)
                        {
                            d[i] = bits[i];
                        }

                        ((Cipher3Characteristic)_cipherControl).PermutationBitsRoundThree = d;

                        bits = new BitArray(BitConverter.GetBytes(charToDisplay.InputDifferentials[3]));
                        d = new bool[16];
                        for (int i = 0; i < 16; i++)
                        {
                            d[i] = bits[i];
                        }

                        ((Cipher3Characteristic)_cipherControl).BitsKeyRoundFour = d;

                        bits = new BitArray(BitConverter.GetBytes(charToDisplay.OutputDifferentials[3]));
                        d = new bool[16];
                        for (int i = 0; i < 16; i++)
                        {
                            d[i] = bits[i];
                        }

                        ((Cipher3Characteristic)_cipherControl).PermutationBitsRoundFour = d;

                        bits = new BitArray(BitConverter.GetBytes(charToDisplay.InputDifferentials[4]));
                        d = new bool[16];
                        for (int i = 0; i < 16; i++)
                        {
                            d[i] = bits[i];
                        }

                        ((Cipher3Characteristic)_cipherControl).BitsKeyRoundFive = d;

                        ((Cipher3Characteristic)_cipherControl).InputDiff = Convert
                            .ToString(charToDisplay.InputDifferentials[0], 2).PadLeft(16, '0').Insert(8, " ");
                        ((Cipher3Characteristic)_cipherControl).Round1InputDiff =
                            "=" + Convert.ToString(charToDisplay.InputDifferentials[0], 2).PadLeft(16, '0')
                                .Insert(8, " ");
                        ((Cipher3Characteristic)_cipherControl).Round1OutputDiff =
                            "=" + Convert.ToString(charToDisplay.OutputDifferentials[0], 2).PadLeft(16, '0')
                                .Insert(8, " ");
                        ((Cipher3Characteristic)_cipherControl).Round2InputDiff =
                            "=" + Convert.ToString(charToDisplay.InputDifferentials[1], 2).PadLeft(16, '0')
                                .Insert(8, " ");

                        if (CurrentConfigurationToDisplay.Round == 5)
                        {
                            ((Cipher3Characteristic)_cipherControl).Round2OutputDiff =
                                "=" + Convert.ToString(charToDisplay.OutputDifferentials[1], 2).PadLeft(16, '0')
                                    .Insert(8, " ");
                            ((Cipher3Characteristic)_cipherControl).Round3InputDiff =
                                "=" + Convert.ToString(charToDisplay.InputDifferentials[2], 2).PadLeft(16, '0')
                                    .Insert(8, " ");
                            ((Cipher3Characteristic)_cipherControl).Round3OutputDiff =
                                "=" + Convert.ToString(charToDisplay.OutputDifferentials[2], 2).PadLeft(16, '0')
                                    .Insert(8, " ");
                            ((Cipher3Characteristic)_cipherControl).Round4InputDiff =
                                "=" + Convert.ToString(charToDisplay.InputDifferentials[3], 2).PadLeft(16, '0')
                                    .Insert(8, " ");
                            ((Cipher3Characteristic)_cipherControl).Round4OutputDiff =
                                "=" + Convert.ToString(charToDisplay.OutputDifferentials[3], 2).PadLeft(16, '0')
                                    .Insert(8, " ");
                            ((Cipher3Characteristic)_cipherControl).Round5InputDiff =
                                "=" + Convert.ToString(charToDisplay.InputDifferentials[4], 2).PadLeft(16, '0')
                                    .Insert(8, " ");
                        }
                        else if (CurrentConfigurationToDisplay.Round == 4)
                        {
                            ((Cipher3Characteristic)_cipherControl).Round2OutputDiff =
                                "=" + Convert.ToString(charToDisplay.OutputDifferentials[1], 2).PadLeft(16, '0')
                                    .Insert(8, " ");
                            ((Cipher3Characteristic)_cipherControl).Round3InputDiff =
                                "=" + Convert.ToString(charToDisplay.InputDifferentials[2], 2).PadLeft(16, '0')
                                    .Insert(8, " ");
                            ((Cipher3Characteristic)_cipherControl).Round3OutputDiff =
                                "=" + Convert.ToString(charToDisplay.OutputDifferentials[2], 2).PadLeft(16, '0')
                                    .Insert(8, " ");
                            ((Cipher3Characteristic)_cipherControl).Round4InputDiff =
                                "=" + Convert.ToString(charToDisplay.InputDifferentials[3], 2).PadLeft(16, '0')
                                    .Insert(8, " ");
                            ((Cipher3Characteristic)_cipherControl).Round4OutputDiff = "";
                            ((Cipher3Characteristic)_cipherControl).Round5InputDiff = "";
                        }
                        else if (CurrentConfigurationToDisplay.Round == 3)
                        {
                            ((Cipher3Characteristic)_cipherControl).Round2OutputDiff =
                                "=" + Convert.ToString(charToDisplay.OutputDifferentials[1], 2).PadLeft(16, '0')
                                    .Insert(8, " ");
                            ((Cipher3Characteristic)_cipherControl).Round3InputDiff =
                                "=" + Convert.ToString(charToDisplay.InputDifferentials[2], 2).PadLeft(16, '0')
                                    .Insert(8, " ");
                            ((Cipher3Characteristic)_cipherControl).Round3OutputDiff = "";
                            ((Cipher3Characteristic)_cipherControl).Round4InputDiff = "";
                            ((Cipher3Characteristic)_cipherControl).Round4OutputDiff = "";
                            ((Cipher3Characteristic)_cipherControl).Round5InputDiff = "";
                        }
                        else if (CurrentConfigurationToDisplay.Round == 2)
                        {
                            ((Cipher3Characteristic)_cipherControl).Round2OutputDiff = "";
                            ((Cipher3Characteristic)_cipherControl).Round3InputDiff = "";
                            ((Cipher3Characteristic)_cipherControl).Round3OutputDiff = "";
                            ((Cipher3Characteristic)_cipherControl).Round4InputDiff = "";
                            ((Cipher3Characteristic)_cipherControl).Round4OutputDiff = "";
                            ((Cipher3Characteristic)_cipherControl).Round5InputDiff = "";
                        }

                        ((Cipher3Characteristic)_cipherControl).Round = _currentConfigurationToDisplay.Round;
                        ((Cipher3Characteristic)_cipherControl).ActiveSBoxes =
                            _currentConfigurationToDisplay.ActiveSBoxes;
                    }
                }
            }
        }

        /// <summary>
        /// Handles change of the characteristic to display in the cipher view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cipher2CharacteristicToShowChanged(object sender, Cipher2CharacteristicSelectionEventArgs e)
        {
            if (_currentConfigurationToDisplay == null || e.SelectedCharacteristic == null)
            {
                bool[] d = new bool[16];
                for (int i = 0; i < 16; i++)
                {
                    d[i] = false;
                }

                ((Cipher2Characteristic)_cipherControl).BitsKeyRoundOne = d;

                d = new bool[16];
                for (int i = 0; i < 16; i++)
                {
                    d[i] = false;
                }

                ((Cipher2Characteristic)_cipherControl).PermutationBitsRoundOne = d;

                d = new bool[16];
                for (int i = 0; i < 16; i++)
                {
                    d[i] = false;
                }

                ((Cipher2Characteristic)_cipherControl).BitsKeyRoundTwo = d;

                d = new bool[16];
                for (int i = 0; i < 16; i++)
                {
                    d[i] = false;
                }

                ((Cipher2Characteristic)_cipherControl).PermutationBitsRoundTwo = d;

                d = new bool[16];
                for (int i = 0; i < 16; i++)
                {
                    d[i] = false;
                }

                ((Cipher2Characteristic)_cipherControl).BitsKeyRoundThree = d;

                ((Cipher2Characteristic)_cipherControl).InputDiff = "";
                ((Cipher2Characteristic)_cipherControl).Round1InputDiff = "";
                ((Cipher2Characteristic)_cipherControl).Round1OutputDiff = "";
                ((Cipher2Characteristic)_cipherControl).Round2InputDiff = "";
                ((Cipher2Characteristic)_cipherControl).Round2OutputDiff = "";
                ((Cipher2Characteristic)_cipherControl).Round3InputDiff = "";
                ((Cipher2Characteristic)_cipherControl).ActiveSBoxes = new bool[4];

                return;
            }

            foreach (Characteristic curCharacteristic in _currentConfigurationToDisplay.Characteristics)
            {
                if (curCharacteristic.InputDifferentials[0] == e.SelectedCharacteristic.InputDiffInt &&
                    curCharacteristic.InputDifferentials[0] == e.SelectedCharacteristic.InputDiffR1Int &&
                    curCharacteristic.OutputDifferentials[0] == e.SelectedCharacteristic.OutputDiffR1Int &&
                    curCharacteristic.InputDifferentials[1] == e.SelectedCharacteristic.InputDiffR2Int &&
                    curCharacteristic.OutputDifferentials[1] == e.SelectedCharacteristic.OutputDiffR2Int &&
                    curCharacteristic.InputDifferentials[2] == e.SelectedCharacteristic.ExpectedDiffInt)
                {
                    if (curCharacteristic is Logic.Cipher2.Cipher2Characteristic charToDisplay)
                    {
                        BitArray bits = new BitArray(BitConverter.GetBytes(charToDisplay.InputDifferentials[0]));
                        bool[] d = new bool[16];
                        for (int i = 0; i < 16; i++)
                        {
                            d[i] = bits[i];
                        }

                        ((Cipher2Characteristic)_cipherControl).BitsKeyRoundOne = d;

                        bits = new BitArray(BitConverter.GetBytes(charToDisplay.OutputDifferentials[0]));
                        d = new bool[16];
                        for (int i = 0; i < 16; i++)
                        {
                            d[i] = bits[i];
                        }

                        ((Cipher2Characteristic)_cipherControl).PermutationBitsRoundOne = d;

                        bits = new BitArray(BitConverter.GetBytes(charToDisplay.InputDifferentials[1]));
                        d = new bool[16];
                        for (int i = 0; i < 16; i++)
                        {
                            d[i] = bits[i];
                        }

                        ((Cipher2Characteristic)_cipherControl).BitsKeyRoundTwo = d;

                        bits = new BitArray(BitConverter.GetBytes(charToDisplay.OutputDifferentials[1]));
                        d = new bool[16];
                        for (int i = 0; i < 16; i++)
                        {
                            d[i] = bits[i];
                        }

                        ((Cipher2Characteristic)_cipherControl).PermutationBitsRoundTwo = d;

                        bits = new BitArray(BitConverter.GetBytes(charToDisplay.InputDifferentials[2]));
                        d = new bool[16];
                        for (int i = 0; i < 16; i++)
                        {
                            d[i] = bits[i];
                        }

                        ((Cipher2Characteristic)_cipherControl).BitsKeyRoundThree = d;

                        ((Cipher2Characteristic)_cipherControl).InputDiff = Convert
                            .ToString(charToDisplay.InputDifferentials[0], 2).PadLeft(16, '0').Insert(8, " ");
                        ((Cipher2Characteristic)_cipherControl).Round1InputDiff =
                            "=" + Convert.ToString(charToDisplay.InputDifferentials[0], 2).PadLeft(16, '0')
                                .Insert(8, " ");
                        ((Cipher2Characteristic)_cipherControl).Round1OutputDiff =
                            "=" + Convert.ToString(charToDisplay.OutputDifferentials[0], 2).PadLeft(16, '0')
                                .Insert(8, " ");
                        ((Cipher2Characteristic)_cipherControl).Round2InputDiff =
                            "=" + Convert.ToString(charToDisplay.InputDifferentials[1], 2).PadLeft(16, '0')
                                .Insert(8, " ");
                        if (CurrentConfigurationToDisplay.Round == 3)
                        {
                            ((Cipher2Characteristic)_cipherControl).Round2OutputDiff =
                                "=" + Convert.ToString(charToDisplay.OutputDifferentials[1], 2).PadLeft(16, '0')
                                    .Insert(8, " ");
                            ((Cipher2Characteristic)_cipherControl).Round3InputDiff =
                                "=" + Convert.ToString(charToDisplay.InputDifferentials[2], 2).PadLeft(16, '0')
                                    .Insert(8, " ");
                        }
                        else
                        {
                            ((Cipher2Characteristic)_cipherControl).Round2OutputDiff = "";
                            ((Cipher2Characteristic)_cipherControl).Round3InputDiff = "";
                        }

                        ((Cipher2Characteristic)_cipherControl).Round = _currentConfigurationToDisplay.Round;
                        ((Cipher2Characteristic)_cipherControl).ActiveSBoxes =
                            _currentConfigurationToDisplay.ActiveSBoxes;
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Method to call if data changes
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged
                    .Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}