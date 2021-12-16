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
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace CrypTool.Plugins.Purple
{
    [Author("Martin Jedrychowski, Martin Switek", "jedry@gmx.de, Martin_Switek@gmx.de", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("Purple.Properties.Resources", false, "PluginCaption", "PluginTooltip", "Purple/DetailedDescription/doc.xml", "Purple/Images/Purple.PNG")]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class Purple : ICrypComponent
    {
        #region Private Variables


        private readonly PurpleSettings settings = new PurpleSettings();
        private int Pos6 = 0;
        private readonly int[] Pos20 = new int[3] { 0, 0, 0 };
        private readonly int[] speeds = new int[3] { 0, 1, 2 };
        private const int fast = 0, medium = 1, slow = 2;
        private readonly int[] plugboardPermutation = new int[26];
        private readonly int[] plugboardPermutation_inv = new int[26];
        private readonly int[,] sixes_inv = new int[25, 6];
        private readonly int[,] twenties1_inv = new int[25, 20], twenties2_inv = new int[25, 20], twenties3_inv = new int[25, 20];
        private readonly int[,] sixes = new int[25, 6] {
                                         {2,1,3,5,4,6},
                                         {6,3,5,2,1,4},
                                         {1,5,4,6,2,3},
                                         {4,3,2,1,6,5},
                                         {3,6,1,4,5,2},
                                         {2,1,6,5,3,4},
                                         {6,5,4,2,1,3},
                                         {3,6,1,4,5,2},
                                         {5,4,2,6,3,1},
                                         {4,5,3,2,1,6},
                                         {2,1,4,5,6,3},
                                         {5,4,6,3,2,1},
                                         {3,1,2,6,4,5},
                                         {4,2,5,1,3,6},
                                         {1,6,2,3,5,4},
                                         {5,4,3,6,1,2},
                                         {6,2,5,3,4,1},
                                         {2,3,4,1,5,6},
                                         {1,2,3,5,6,4},
                                         {3,1,6,4,2,5},
                                         {6,5,1,2,4,3},
                                         {1,3,6,4,2,5},
                                         {6,4,5,1,3,2},
                                         {4,6,1,2,5,3},
                                         {5,2,4,3,6,1}
        };
        private readonly int[,] twenties1 = new int[25, 20] {
                                         {6,19,14,1,10,4,2,7,13,9,8,16,3,18,15,11,5,12,20,17},
                                         {4,5,16,17,14,1,20,15,3,8,18,11,12,13,10,19,2,6,9,7},
                                         {17,1,13,6,15,11,19,12,16,18,10,3,7,14,8,20,4,9,2,5},
                                         {3,14,20,4,6,16,8,19,2,12,17,9,5,1,11,10,7,13,15,18},
                                         {19,6,8,20,13,5,18,4,10,3,16,15,14,12,7,2,17,11,1,9},
                                         {2,11,9,14,7,19,6,3,18,13,12,8,10,15,16,17,20,4,5,1},
                                         {16,7,6,18,9,10,13,1,17,2,5,4,11,19,20,14,8,15,3,12},
                                         {1,20,7,16,12,14,5,18,15,10,13,6,8,3,4,9,11,17,19,2},
                                         {17,9,11,8,20,18,7,14,1,16,15,5,19,2,6,12,4,10,13,3},
                                         {12,8,17,9,3,20,4,10,14,5,7,18,2,16,13,6,1,19,15,11},
                                         {20,1,16,11,2,17,9,4,8,15,10,13,3,18,14,5,6,7,12,19},
                                         {5,4,15,2,13,19,6,16,12,14,8,7,17,10,18,3,9,1,11,20},
                                         {15,17,10,19,16,2,11,8,9,7,3,14,18,13,12,1,5,20,6,4},
                                         {11,12,7,3,8,15,16,6,4,20,2,5,1,9,19,18,10,14,17,13},
                                         {12,16,2,7,4,8,15,19,5,1,11,9,20,17,6,14,13,3,18,10},
                                         {8,15,18,1,12,11,17,14,20,16,13,19,9,7,3,4,2,5,10,6},
                                         {7,3,5,18,17,13,19,20,14,11,9,10,2,6,1,15,12,16,4,8},
                                         {10,13,4,14,18,3,2,17,11,19,20,1,6,12,9,7,15,8,5,16},
                                         {13,7,9,12,20,16,14,10,19,6,1,2,11,4,5,3,18,17,8,15},
                                         {1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20},
                                         {9,20,12,5,10,17,1,13,7,15,4,3,16,8,18,11,19,2,14,6},
                                         {18,15,2,13,1,7,10,5,19,17,6,20,9,11,12,8,3,4,16,14},
                                         {16,18,19,10,11,20,5,9,1,4,12,13,7,6,17,2,14,15,3,8},
                                         {5,8,1,15,19,9,12,2,6,3,14,17,4,20,16,13,18,10,7,11},
                                         {14,10,4,8,9,12,3,11,17,20,19,6,15,5,2,18,16,7,1,13}
        };
        private readonly int[,] twenties2 = new int[25, 20] {
                                        {15,9,1,5,17,19,3,2,10,8,11,18,12,16,6,13,20,4,14,7},
                                        {12,6,15,2,4,9,8,16,19,17,5,11,20,7,10,18,1,14,13,3},
                                        {4,18,5,8,16,1,12,15,20,14,13,17,11,2,7,9,6,3,10,19},
                                        {6,11,2,20,14,7,18,12,15,3,8,5,10,1,17,19,9,16,4,13},
                                        {7,2,13,3,9,4,17,14,1,12,18,20,6,11,16,15,5,8,19,10},
                                        {5,17,14,7,10,9,19,20,8,13,1,2,16,3,4,12,11,18,6,15},
                                        {8,4,3,11,19,13,2,9,12,16,10,17,14,15,20,6,18,1,7,5},
                                        {20,1,16,10,15,8,14,11,18,5,3,7,13,17,19,4,2,9,12,6},
                                        {9,8,7,15,5,2,4,13,17,1,11,6,19,18,14,10,3,20,16,12},
                                        {10,12,11,18,8,16,20,17,5,6,9,3,4,19,13,7,1,14,15,2},
                                        {11,7,14,4,18,20,6,1,13,19,12,15,5,9,16,2,17,10,8,3},
                                        {2,3,9,10,13,14,15,16,7,11,20,12,18,6,1,5,8,17,19,4},
                                        {16,10,15,1,17,3,13,9,4,7,6,8,2,14,5,11,12,19,18,20},
                                        {19,16,18,12,3,13,9,10,6,2,17,14,11,4,7,20,15,5,1,8},
                                        {18,14,12,19,1,7,10,6,11,15,5,9,8,20,17,4,3,13,2,16},
                                        {20,3,19,2,4,5,11,14,9,10,18,16,15,12,8,7,13,6,17,1},
                                        {3,6,4,14,2,12,16,5,18,20,7,19,1,15,9,8,10,11,13,17},
                                        {5,15,20,9,10,17,1,19,13,12,4,2,7,6,11,14,16,8,3,18},
                                        {14,20,13,17,5,18,8,4,2,15,16,1,9,19,3,6,7,10,12,11},
                                        {8,11,1,6,19,14,5,18,17,3,10,13,12,20,15,16,4,2,7,9},
                                        {17,19,6,1,12,15,20,7,16,9,3,11,13,10,2,18,8,4,5,14},
                                        {1,5,12,20,6,11,14,8,9,7,19,4,3,13,10,17,18,16,15,2},
                                        {16,8,10,13,11,6,19,5,3,4,15,20,17,2,18,1,14,7,9,12},
                                        {19,13,8,16,20,10,7,1,2,18,14,6,9,5,12,3,17,15,11,4},
                                        {13,1,17,15,7,4,16,3,14,5,2,10,18,8,11,9,19,12,20,6}
        };
        private readonly int[,] twenties3 = new int[25, 20] {
                                        {7,19,11,3,20,1,10,6,16,12,17,13,8,9,4,18,5,14,15,2},
                                        {15,17,14,2,12,13,8,3,1,19,9,4,10,7,11,20,16,6,18,5},
                                        {2,11,20,12,1,19,4,10,9,14,6,15,13,3,7,16,18,8,5,17},
                                        {16,3,12,9,4,20,6,19,18,2,5,8,14,11,10,1,15,17,13,7},
                                        {12,18,16,4,9,3,15,13,6,20,8,2,7,10,5,19,14,1,17,11},
                                        {13,9,5,6,8,7,12,17,14,18,20,10,2,19,11,15,4,3,1,16},
                                        {4,7,2,15,17,10,19,5,8,16,1,12,3,13,6,14,20,9,11,18},
                                        {9,6,4,10,18,16,8,14,5,12,17,1,20,15,13,19,2,11,7,3},
                                        {5,14,18,17,13,15,11,12,7,8,3,6,1,2,20,4,9,10,16,19},
                                        {11,16,9,18,3,12,5,15,10,1,14,17,2,4,19,6,8,7,13,20},
                                        {19,8,3,15,14,5,1,11,2,10,12,16,18,20,17,7,13,4,9,6},
                                        {1,12,17,13,9,7,14,2,15,4,5,11,6,16,3,8,18,19,20,10},
                                        {3,4,10,12,1,18,2,8,14,13,19,7,16,6,15,9,17,20,5,11},
                                        {9,11,6,5,10,4,17,19,13,15,7,2,12,18,14,20,1,16,8,3},
                                        {8,13,14,16,19,12,20,7,10,3,15,9,4,17,1,11,5,2,6,18},
                                        {18,16,15,4,2,17,13,12,6,11,20,19,14,5,9,1,8,7,3,10},
                                        {14,1,7,20,6,13,16,18,12,9,4,17,5,11,2,3,10,15,19,8},
                                        {17,19,1,11,7,2,18,4,3,8,10,5,15,12,16,9,6,13,20,14},
                                        {10,15,2,14,11,6,7,1,16,20,13,3,9,8,18,17,19,5,12,4},
                                        {20,9,8,6,12,11,2,5,4,7,16,14,17,3,15,10,13,19,18,1},
                                        {11,20,13,8,16,10,18,14,19,6,15,4,1,17,7,5,3,9,2,12},
                                        {16,5,10,19,4,18,15,17,1,3,2,20,11,6,8,13,7,12,14,9},
                                        {6,10,19,16,5,9,1,20,17,4,11,18,7,14,13,2,12,8,3,15},
                                        {8,7,5,1,15,14,9,16,11,17,18,6,19,20,3,12,4,2,10,13},
                                        {13,2,17,7,14,8,3,9,20,5,16,10,6,1,12,15,11,18,4,19}
        };

        #endregion

        #region Constructor

        public Purple()
        {
            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    sixes[i, j]--;
                }

                for (int j = 0; j < 20; j++)
                {
                    twenties1[i, j]--;
                    twenties2[i, j]--;
                    twenties3[i, j]--;
                }

                for (int j = 0; j < 6; j++)
                {
                    sixes_inv[i, sixes[i, j]] = j;
                }

                for (int j = 0; j < 20; j++)
                {
                    twenties1_inv[i, twenties1[i, j]] = j;
                    twenties2_inv[i, twenties2[i, j]] = j;
                    twenties3_inv[i, twenties3[i, j]] = j;
                }
            }

            for (int i = 0; i < 26; i++)
            {
                plugboardPermutation[i] = plugboardPermutation_inv[i] = i;
            }
        }

        #endregion

        #region Data Properties

        /// <summary>
        /// HOWTO: Input interface to read the input data. 
        /// You can add more input properties of other type if needed.
        /// </summary>

        //  Pfeil in Programmiersprache Eingabe
        [PropertyInfo(Direction.InputData, "TextCaption", "TextTooltip", null, true, false, QuickWatchFormat.Text, null)]
        public string Text
        {
            get;
            set;
        }

        // Pfeil in Programmiersprache Ausgabe
        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", "", false, false, QuickWatchFormat.Text, null)]
        public string OutputString
        {
            get;
            set;
        }
        #endregion

        #region IPlugin Members

        public ISettings Settings => settings;

        /// <summary>
        /// HOWTO: You can provide a custom (tabbed) presentation to visualize your algorithm.
        /// Return null if you don't provide one.
        /// </summary>
        public UserControl Presentation => null;

        public void PreExecution()
        {
        }

        public void Execute()
        {
            ProgressChanged(0, 1);
            if (checkSettings())
            {
                decipher();
            }

            ProgressChanged(1, 1);

            OnPropertyChanged("OutputString");
        }

        private bool checkSettings()
        {
            if (Text == null)
            {
                GuiLogMessage("Input is not set. Execution is stopped!", NotificationLevel.Warning);
                return false;
            }

            // check starting positions of the switches
            Pos6 = settings.sechserPos - 1;
            Pos20[0] = settings.zwanzigerPos1 - 1;
            Pos20[1] = settings.zwanzigerPos2 - 1;
            Pos20[2] = settings.zwanzigerPos3 - 1;

            if (Pos6 < 0 || Pos6 > 24 || Pos20[0] < 0 || Pos20[0] > 24 || Pos20[1] < 0 || Pos20[1] > 24 || Pos20[2] < 0 || Pos20[2] > 24)
            {
                GuiLogMessage("Illegal starting position of a switch (allowed is 1-25)", NotificationLevel.Warning);
                return false;
            }

            // check switch movements
            string movement = settings.Motion.ToString();
            List<string> movements = new List<string>() { "123", "132", "213", "231", "312", "321" };

            if (!movements.Contains(movement))
            {
                GuiLogMessage("Illegal motion '" + movement + "'", NotificationLevel.Warning);
                return false;
            }

            // speed[0]=fast switch, speed[2]=slow switch
            for (int i = 0; i < 3; i++)
            {
                speeds[i] = movement[i] - '1';
            }

            // check plugboard
            char[] abc = settings.Alphabet.ToCharArray();
            Array.Sort(abc);
            if (new string(abc) != "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
            {
                GuiLogMessage("Illegal plugboard '" + settings.Alphabet + "'", NotificationLevel.Warning);
                return false;
            }

            for (int i = 0; i < 26; i++)
            {
                plugboardPermutation[i] = settings.Alphabet[i] - 'A';
                plugboardPermutation_inv[plugboardPermutation[i]] = i;
            }

            return true;
        }

        public void decipher()
        {
            char plain, cipher;

            if (settings.OutputFormatting != 0)
            {
                Text = new Regex("\\s").Replace(Text, "");
            }

            StringBuilder tempOutput = new StringBuilder(Text.Length);

            foreach (char c in Text)
            {
                plain = char.ToUpper(c);

                if (plain >= 'A' && plain <= 'Z')
                {
                    cipher = permuteCharacter(plain);
                    if (settings.CaseHandling == 0 && c != plain)
                    {
                        cipher = char.ToLower(cipher);
                    }
                }
                else if (c == ' ' || c == '\n' || c == '\t' || c == '\r')
                {
                    tempOutput.Append(c);
                    continue;
                }
                else
                {
                    switch (settings.UnknownSymbolHandling)
                    {
                        case 0: continue; // ignore
                        case 1: cipher = c; break; // copy
                        default: cipher = '_'; break; // placeholder
                    }
                }

                tempOutput.Append(cipher);
                stepSwitches();
            }

            // CaseHandling
            OutputString = tempOutput.ToString();
            if (settings.CaseHandling == 1)
            {
                OutputString = OutputString.ToUpper();
            }
            else if (settings.CaseHandling == 2)
            {
                OutputString = OutputString.ToLower();
            }

            // In Fünfergruppen ausgeben
            if (settings.OutputFormatting == 2)
            {
                for (int i = 5; i < OutputString.Length; i += 6)
                {
                    OutputString = OutputString.Insert(i, " ");
                }
            }
        }

        private char permuteCharacter(char c)
        {
            int z = c - 'A';
            z = plugboardPermutation_inv[z];

            if (settings.Action == PurpleSettings.actions.decrypt)
            {
                if (z < 6)
                {
                    z = sixes[Pos6, z];
                }
                else
                {
                    z -= 6;
                    z = twenties3[Pos20[2], z];
                    z = twenties2[Pos20[1], z];
                    z = twenties1[Pos20[0], z];
                    z += 6;
                }
            }
            else
            {
                if (z < 6)
                {
                    z = sixes_inv[Pos6, z];
                }
                else
                {
                    z -= 6;
                    z = twenties1_inv[Pos20[0], z];
                    z = twenties2_inv[Pos20[1], z];
                    z = twenties3_inv[Pos20[2], z];
                    z += 6;
                }
            }

            z = plugboardPermutation[z];
            return (char)(z + 'A');
        }

        private void stepSwitches()
        {
            if (Pos6 == 24)
            {
                Pos20[speeds[medium]] = (Pos20[speeds[medium]] + 1) % 25;
            }
            else
            {
                if (Pos6 == 23 && Pos20[speeds[medium]] == 24)
                {
                    Pos20[speeds[slow]] = (Pos20[speeds[slow]] + 1) % 25;
                }
                else
                {
                    Pos20[speeds[fast]] = (Pos20[speeds[fast]] + 1) % 25;
                }
            }

            Pos6 = (Pos6 + 1) % 25;
        }

        public void PostExecution()
        {
        }

        public void Pause()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}