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
using System.ComponentModel;

namespace CrypTool.Plugins.SZ42
{
    public class LorenzSettings : ISettings
    {
        #region Private Variables

        private int limitation = 0;
        private readonly string[] patterns = new string[12];
        private readonly string[] positions = new string[12];
        private int action = 0;
        private bool inputParsed = false;
        private bool outputParsed = false;

        #endregion

        #region Public Interface

        /// <summary>
        /// We use this delegate to send log messages from the settings class to the Caesar plugin
        /// </summary>
        public delegate void LorenzLogMessage(string msg, NotificationLevel loglevel);
        public delegate void LorenzReExecute();
        public event LorenzReExecute ReExecute;

        public string[] Patterns => patterns;

        public string[] Positions => positions;

        #endregion

        #region TaskPane Settings

        #region General Settings

        [TaskPane("ActionCaption", "ActionTooltip", "GeneralSettingsGroup", 0, false, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
        public int Action
        {
            get => action;
            set
            {
                if (value != action)
                {
                    action = value;
                    OnPropertyChanged("Action");

                    if (ReExecute != null)
                    {
                        ReExecute();
                    }
                }
            }
        }

        [TaskPane("LimitationCaption", "LimitationTooltip", "GeneralSettingsGroup", 1, false, ControlType.ComboBox, new string[] { "LimitationList1", "LimitationList2" })]
        public int Limitation
        {
            get => limitation;
            set
            {
                if (value != limitation)
                {
                    limitation = value;
                    OnPropertyChanged("Limitation");

                    if (ReExecute != null)
                    {
                        ReExecute();
                    }
                }
            }
        }

        #endregion

        #region Wheels Patterns

        [TaskPane("Patternχ1Caption", "Patternχ1Tooltip", "PatternGroup", 2, false, ControlType.TextBox, "")]
        public string Patternχ1
        {
            get => patterns[0];
            set
            {
                if (value != patterns[0])
                {
                    patterns[0] = value;
                    OnPropertyChanged("Pattern");

                    if (ReExecute != null)
                    {
                        ReExecute();
                    }
                }
            }
        }

        [TaskPane("Patternχ2Caption", "Patternχ2Tooltip", "PatternGroup", 3, false, ControlType.TextBox, "")]
        public string Patternχ2
        {
            get => patterns[1];
            set
            {
                if (value != patterns[1])
                {
                    patterns[1] = value;
                    OnPropertyChanged("Patternχ2");

                    if (ReExecute != null)
                    {
                        ReExecute();
                    }
                }
            }
        }

        [TaskPane("Patternχ3Caption", "Patternχ3Tooltip", "PatternGroup", 4, false, ControlType.TextBox, "")]
        public string Patternχ3
        {
            get => patterns[2];
            set
            {
                if (value != patterns[2])
                {
                    patterns[2] = value;
                    OnPropertyChanged("Patternχ3");

                    if (ReExecute != null)
                    {
                        ReExecute();
                    }
                }
            }
        }

        [TaskPane("Patternχ4Caption", "Patternχ4Tooltip", "PatternGroup", 5, false, ControlType.TextBox, "")]
        public string Patternχ4
        {
            get => patterns[3];
            set
            {
                if (value != patterns[3])
                {
                    patterns[3] = value;
                    OnPropertyChanged("Patternχ4");

                    if (ReExecute != null)
                    {
                        ReExecute();
                    }
                }
            }
        }

        [TaskPane("Patternχ5Caption", "Patternχ5Tooltip", "PatternGroup", 6, false, ControlType.TextBox, "")]
        public string Patternχ5
        {
            get => patterns[4];
            set
            {
                if (value != patterns[4])
                {
                    patterns[4] = value;
                    OnPropertyChanged("Patternχ5");

                    if (ReExecute != null)
                    {
                        ReExecute();
                    }
                }
            }
        }

        [TaskPane("PatternΨ1Caption", "PatternΨ1Tooltip", "PatternGroup", 7, false, ControlType.TextBox, "")]
        public string PatternΨ1
        {
            get => patterns[5];
            set
            {
                if (value != patterns[5])
                {
                    patterns[5] = value;
                    OnPropertyChanged("PatternΨ1");

                    if (ReExecute != null)
                    {
                        ReExecute();
                    }
                }
            }
        }

        [TaskPane("PatternΨ2Caption", "PatternΨ2Tooltip", "PatternGroup", 8, false, ControlType.TextBox, "")]
        public string PatternΨ2
        {
            get => patterns[6];
            set
            {
                if (value != patterns[6])
                {
                    patterns[6] = value;
                    OnPropertyChanged("PatternΨ2");

                    if (ReExecute != null)
                    {
                        ReExecute();
                    }
                }
            }
        }

        [TaskPane("PatternΨ3Caption", "PatternΨ3Tooltip", "PatternGroup", 9, false, ControlType.TextBox, "")]
        public string PatternΨ3
        {
            get => patterns[7];
            set
            {
                if (value != patterns[7])
                {
                    patterns[7] = value;
                    OnPropertyChanged("PatternΨ3");

                    if (ReExecute != null)
                    {
                        ReExecute();
                    }
                }
            }
        }

        [TaskPane("PatternΨ4Caption", "PatternΨ4Tooltip", "PatternGroup", 10, false, ControlType.TextBox, "")]
        public string PatternΨ4
        {
            get => patterns[8];
            set
            {
                if (value != patterns[8])
                {
                    patterns[8] = value;
                    OnPropertyChanged("PatternΨ4");

                    if (ReExecute != null)
                    {
                        ReExecute();
                    }
                }
            }
        }

        [TaskPane("PatternΨ5Caption", "PatternΨ5Tooltip", "PatternGroup", 11, false, ControlType.TextBox, "")]
        public string PatternΨ5
        {
            get => patterns[9];
            set
            {
                if (value != patterns[9])
                {
                    patterns[9] = value;
                    OnPropertyChanged("PatternΨ5");

                    if (ReExecute != null)
                    {
                        ReExecute();
                    }
                }
            }
        }

        [TaskPane("Patternμ61Caption", "Patternμ61Tooltip", "PatternGroup", 12, false, ControlType.TextBox, "")]
        public string Patternμ61
        {
            get => patterns[10];
            set
            {
                if (value != patterns[10])
                {
                    patterns[10] = value;
                    OnPropertyChanged("Patternμ61");

                    if (ReExecute != null)
                    {
                        ReExecute();
                    }
                }
            }
        }

        [TaskPane("Patternμ37Caption", "Patternμ37Tooltip", "PatternGroup", 13, false, ControlType.TextBox, "")]
        public string Patternμ37
        {
            get => patterns[11];
            set
            {
                if (value != patterns[11])
                {
                    patterns[11] = value;
                    OnPropertyChanged("Patternμ37");

                    if (ReExecute != null)
                    {
                        ReExecute();
                    }
                }
            }
        }

        #endregion

        #region Wheels Positions

        [TaskPane("Positionχ1Caption", "Positionχ1Tooltip", "PositionGroup", 14, false, ControlType.TextBox, "")]
        public string Positionχ1
        {
            get => positions[0];
            set
            {
                if (value != positions[0])
                {
                    positions[0] = value;
                    OnPropertyChanged("Positionχ1");

                    if (ReExecute != null)
                    {
                        ReExecute();
                    }
                }
            }
        }

        [TaskPane("Positionχ2Caption", "Positionχ2Tooltip", "PositionGroup", 15, false, ControlType.TextBox, "")]
        public string Positionχ2
        {
            get => positions[1];
            set
            {
                if (value != positions[1])
                {
                    positions[1] = value;
                    OnPropertyChanged("Positionχ2");

                    if (ReExecute != null)
                    {
                        ReExecute();
                    }
                }
            }
        }

        [TaskPane("Positionχ3Caption", "Positionχ3Tooltip", "PositionGroup", 16, false, ControlType.TextBox, "")]
        public string Positionχ3
        {
            get => positions[2];
            set
            {
                if (value != positions[2])
                {
                    positions[2] = value;
                    OnPropertyChanged("Positionχ3");

                    if (ReExecute != null)
                    {
                        ReExecute();
                    }
                }
            }
        }

        [TaskPane("Positionχ4Caption", "Positionχ4Tooltip", "PositionGroup", 17, false, ControlType.TextBox, "")]
        public string Positionχ4
        {
            get => positions[3];
            set
            {
                if (value != positions[3])
                {
                    positions[3] = value;
                    OnPropertyChanged("Positionχ4");

                    if (ReExecute != null)
                    {
                        ReExecute();
                    }
                }
            }
        }

        [TaskPane("Positionχ5Caption", "Positionχ5Tooltip", "PositionGroup", 18, false, ControlType.TextBox, "")]
        public string Positionχ5
        {
            get => positions[4];
            set
            {
                if (value != positions[4])
                {
                    positions[4] = value;
                    OnPropertyChanged("Positionχ5");

                    if (ReExecute != null)
                    {
                        ReExecute();
                    }
                }
            }
        }

        [TaskPane("PositionΨ1Caption", "PositionΨ1Tooltip", "PositionGroup", 19, false, ControlType.TextBox, "")]
        public string PositionΨ1
        {
            get => positions[5];
            set
            {
                if (value != positions[5])
                {
                    positions[5] = value;
                    OnPropertyChanged("PositionΨ1");

                    if (ReExecute != null)
                    {
                        ReExecute();
                    }
                }
            }
        }

        [TaskPane("PositionΨ2Caption", "PositionΨ2Tooltip", "PositionGroup", 20, false, ControlType.TextBox, "")]
        public string PositionΨ2
        {
            get => positions[6];
            set
            {
                if (value != positions[6])
                {
                    positions[6] = value;
                    OnPropertyChanged("PositionΨ2");

                    if (ReExecute != null)
                    {
                        ReExecute();
                    }
                }
            }
        }

        [TaskPane("PositionΨ3Caption", "PositionΨ3Tooltip", "PositionGroup", 21, false, ControlType.TextBox, "")]
        public string PositionΨ3
        {
            get => positions[7];
            set
            {
                if (value != positions[7])
                {
                    positions[7] = value;
                    OnPropertyChanged("PositionΨ3");

                    if (ReExecute != null)
                    {
                        ReExecute();
                    }
                }
            }
        }

        [TaskPane("PositionΨ4Caption", "PositionΨ4Tooltip", "PositionGroup", 22, false, ControlType.TextBox, "")]
        public string PositionΨ4
        {
            get => positions[8];
            set
            {
                if (value != positions[8])
                {
                    positions[8] = value;
                    OnPropertyChanged("PositionΨ4");

                    if (ReExecute != null)
                    {
                        ReExecute();
                    }
                }
            }
        }

        [TaskPane("PositionΨ5Caption", "PositionΨ5Tooltip", "PositionGroup", 23, false, ControlType.TextBox, "")]
        public string PositionΨ5
        {
            get => positions[9];
            set
            {
                if (value != positions[9])
                {
                    positions[9] = value;
                    OnPropertyChanged("PositionΨ5");

                    if (ReExecute != null)
                    {
                        ReExecute();
                    }
                }
            }
        }

        [TaskPane("Positionμ61Caption", "Positionμ61Tooltip", "PositionGroup", 24, false, ControlType.TextBox, "")]
        public string Positionμ61
        {
            get => positions[10];
            set
            {
                if (value != positions[10])
                {
                    positions[10] = value;
                    OnPropertyChanged("Positionμ61");

                    if (ReExecute != null)
                    {
                        ReExecute();
                    }
                }
            }
        }

        [TaskPane("Positionμ37Caption", "Positionμ37Tooltip", "PositionGroup", 25, false, ControlType.TextBox, "")]
        public string Positionμ37
        {
            get => positions[10];
            set
            {
                if (value != positions[10])
                {
                    positions[10] = value;
                    OnPropertyChanged("Positionμ37");

                    if (ReExecute != null)
                    {
                        ReExecute();
                    }
                }
            }
        }

        #endregion

        #region Format Settings

        [TaskPane("InputParsedCaption", "InputParsedTooltip", "FormatGroup", 26, false, ControlType.CheckBox, "")]
        public bool InputParsed
        {
            get => inputParsed;
            set
            {
                if (value != inputParsed)
                {
                    inputParsed = value;
                    OnPropertyChanged("InputParsed");

                    if (ReExecute != null)
                    {
                        ReExecute();
                    }
                }
            }
        }

        [TaskPane("OutputParsedCaption", "OutputParsedTooltip", "FormatGroup", 27, false, ControlType.CheckBox, "")]
        public bool OutputParsed
        {
            get => outputParsed;
            set
            {
                if (value != outputParsed)
                {
                    outputParsed = value;
                    OnPropertyChanged("OutputParsed");

                    if (ReExecute != null)
                    {
                        ReExecute();
                    }
                }
            }
        }

        #endregion

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
