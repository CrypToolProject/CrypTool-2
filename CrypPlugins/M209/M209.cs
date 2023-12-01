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
using CrypTool.M209;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.Plugins.M209
{
    [Author("Martin Jedrychowski, Martin Switek", "jedry@gmx.de, Martin_Switek@gmx.de", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("CrypTool.M209.Properties.Resources", "PluginCaption", "PluginTooltip", "M209/DetailedDescription/doc.xml",
      "M209/Images/M-209.jpg", "M209/Images/encrypt.png", "M209/Images/decrypt.png")]

    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class M209 : ICrypComponent
    {
        #region Private Variables

        private readonly M209Settings settings = new M209Settings();
        private M209Presentation _presentation = new M209Presentation();

        #endregion

        #region Data Properties

        private readonly string[] rotoren = new string[6] {
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
            "ABCDEFGHIJKLMNOPQRSTUVXYZ",    // no W
            "ABCDEFGHIJKLMNOPQRSTUVX",      // no WYZ
            "ABCDEFGHIJKLMNOPQRSTU",        // no V-Z
            "ABCDEFGHIJKLMNOPQRS",          // no T-Z
            "ABCDEFGHIJKLMNOPQ"             // no R-Z
        };

        private bool _gridsToBeCreated = true;

        private readonly bool[,] pins = new bool[6, 27];
        private readonly int[,] StangeSchieber = new int[27, 2];
        private readonly int[,] rotorpos = new int[6, 26];
        private readonly int[] rotorofs = new int[6] { 15, 14, 13, 12, 11, 10 };  // position of 'active' pin wrt upper pin

        public string[,] _shownWheelPositions = new string[6, 5];
        public string[] _activeWheelPositions = new string[6];
        public bool[] _wheelsWithActivePin = new bool[6];

        private readonly Random rnd = new Random();

        public M209()
        {
            settings.m209 = this;

            // invert array 'rotoren' for faster access
            for (int r = 0; r < 6; r++)
            {
                for (int c = 0; c < rotoren[r].Length; c++)
                {
                    rotorpos[r, rotoren[r][c] - 'A'] = c;
                }
            }
        }

        [PropertyInfo(Direction.InputData, "TextCaption", "TextTooltip", true)]
        public string Text
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "InputInternalKeyCaption", "InputInternalKeyTooltip", false)]
        public string InputInternalKey
        {
            set
            {
                if (value == null)
                {
                    return;
                }

                string[] s = value.Split(new char[] { ',' });

                if (s.Length != 1 + settings.Stangen + settings.Rotoren)
                {
                    GuiLogMessage("The given internal key is not valid.", NotificationLevel.Error);
                    return;
                }

                string[] startwert = s.Take(1).ToArray();
                string[] pins = s.Skip(1).Take(settings.Rotoren).ToArray();
                string[] lugs = s.Skip(1).Skip(settings.Rotoren).Take(settings.Stangen).ToArray();
                updatePins(pins);
                updateLugs(lugs);

                settings.Startwert = startwert[0];

                OnPropertyChanged("OutputInternalKey");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", false)]
        public string OutputString
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputInternalKeyCaption", "OutputInternalKeyTooltip", false)]
        public string OutputInternalKey
        {
            get => settings.InternalKey;
            set
            {
                settings.InternalKey = value;
                OnPropertyChanged("OutputInternalKey");
            }
        }

        [PropertyInfo(Direction.OutputData, "KeyCheckCaption", "KeyCheckTooltip", false)]
        public string KeyCheck
        {
            get
            {
                //string keycheck = cipherText("AAAAAAAAAAAAAAAAAAAAAAAAAA", "AAAAAA", 0, false);
                //keycheck = BlockFormat(keycheck, 5);

                //if (!settings.FormattedCheck)
                //{
                //    return keycheck;
                //}

                //string sep = "-------------------------------\n";

                //return sep + "NR LUGS  1  2  3  4  5  6\n" + sep + settings.FormattedInternalKey + "\n" + sep + "26 LETTER CHECK\n\n" + keycheck + "\n" + sep;
                return "TEst";
            }
        }

        /*
             Diese Methode Verschlüsselt einen einzelnen Buchstaben

             In Cipher mode, the output is printed in groups of five letters. 
             Use the letter Z in the plaintext to replace spaces between words. 

             In Decipher mode, the output is printed continuously. If the deciphered plaintext letter is
             Z, a space is printed.
         */

        public char calculateOffset(string extKey, char c)
        {
            bool[] aktiveArme = new bool[6];

            // Durch alle Rotoren
            for (int r = 0; r < settings.Rotoren; r++)
            {
                int i = getRotorPostion(extKey[r], r);
                i = (i + rotorofs[r]) % rotoren[r].Length;   // position of 'active' pin
                aktiveArme[r] = pins[r, i];
            }

            int verschiebung = (countStangen(aktiveArme) + ('Z' - c)) % 26;
            return (char)('A' + verschiebung);
        }

        //Zähle die Verschiebung (wo aktive Arme und Schieber)
        public int countStangen(bool[] active)
        {
            int count = 0;

            for (int i = 0; i < settings.Stangen; i++)
            {
                for (int c = 0; c < settings.Rotoren; c++)
                {
                    if (active[c])
                    {
                        if ((StangeSchieber[i, 0] == (c + 1)) || (StangeSchieber[i, 1] == (c + 1)))
                        {
                            count++;
                            break;  // bar is set, leave inner loop
                        }
                    }
                }
            }

            return count;
        }

        public int countStangen(int active)
        {
            bool[] aktiveArme = new bool[6];

            for (int c = 0; c < settings.Rotoren; c++)
            {
                aktiveArme[c] = ((active & 1) == 1);
                active >>= 1;
            }

            return countStangen(aktiveArme);
        }

        // Schieber von der GUI in Array 'StangeSchieber'
        public void setBars()
        {
            clearBars();

            for (int i = 0; i < settings.Stangen; i++)
            {
                string temp = settings.bar[i];

                // Wir müssen den String splitten und in int umwandeln
                if (temp != null && temp.Length > 0)
                {
                    StangeSchieber[i, 0] = Convert.ToInt32(temp[0].ToString());
                    if (temp.Length > 1)    // Falls 2 Zahlen --> splitten, konvertieren und speichern 
                    {
                        StangeSchieber[i, 1] = Convert.ToInt32(temp[1].ToString());
                    }
                }
            }
        }

        // Benutzereingaben werden in Pin Array reingeschrieben
        public void setPins()
        {
            clearPins();

            if (settings.Rotor1 != null)
            {
                for (int i = 0; i < settings.Rotor1.Length; i++)
                {
                    pins[0, getRotorPostion(settings.Rotor1[i], 0)] = true;
                }
            }

            if (settings.Rotor2 != null)
            {
                for (int i = 0; i < settings.Rotor2.Length; i++)
                {
                    pins[1, getRotorPostion(settings.Rotor2[i], 1)] = true;
                }
            }

            if (settings.Rotor3 != null)
            {
                for (int i = 0; i < settings.Rotor3.Length; i++)
                {
                    pins[2, getRotorPostion(settings.Rotor3[i], 2)] = true;
                }
            }

            if (settings.Rotor4 != null)
            {
                for (int i = 0; i < settings.Rotor4.Length; i++)
                {
                    pins[3, getRotorPostion(settings.Rotor4[i], 3)] = true;
                }
            }

            if (settings.Rotor5 != null)
            {
                for (int i = 0; i < settings.Rotor5.Length; i++)
                {
                    pins[4, getRotorPostion(settings.Rotor5[i], 4)] = true;
                }
            }

            if (settings.Model == 0)
            {
                if (settings.Rotor6 != null)
                {
                    for (int i = 0; i < settings.Rotor6.Length; i++)
                    {
                        pins[5, getRotorPostion(settings.Rotor6[i], 5)] = true;
                    }
                }
            }
        }

        // Pins werden genullt
        public void clearPins()
        {
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 27; j++)
                {
                    pins[i, j] = false;
                }
            }
        }

        // Schieber werden genullt
        public void clearBars()
        {
            for (int i = 0; i < 27; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    StangeSchieber[i, j] = 0;
                }
            }
        }

        // Sucht zum Buchstaben passenden Index
        public int getRotorPostion(char c, int rotor)
        {
            return rotorpos[rotor, c - 'A'];
        }

        // Sammelt die verschlüsselten Buchstaben zu einem String und passt den String an (cipher / decipher)
        public string cipherText(string text, string key, int action, bool zspace)
        {
            Regex objNotNaturalPattern = new Regex("[A-Z]");
            string tempOutput = "";

            if (action == 0 && zspace)
            {
                text = text.Replace(' ', 'Z');
            }
            UpdateWheelPosition(key);

            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _presentation.ShowDisplayedPositionsInGrid(_shownWheelPositions);
                _presentation.ShowActivePositionsInGrid(_activeWheelPositions);
                //_presentation.ShowWheelPinActivityInGrid(_wheelsWithActivePin);
                _presentation.ShowWheelPositionsinGrid(_shownWheelPositions);
                _presentation.labelInput.Content = string.Empty;
                _presentation.labelOutput.Content = string.Empty;
            }, null);

            string upperText = text.ToUpper();

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                char uc = upperText[i];
                bool isValid = objNotNaturalPattern.IsMatch(uc.ToString());
                if (isValid)
                {
                    char enc = calculateOffset(key, uc);
                    key = rotateWheel(key);
                    c = char.IsUpper(c) ? enc : enc.ToString().ToLower()[0];

                    UpdateWheelPosition(key);

                    Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        _presentation.ShowDisplayedPositionsInGrid(_shownWheelPositions);
                        _presentation.ShowActivePositionsInGrid(_activeWheelPositions);
                        _presentation.ShowWheelPinActivityInGrid(_wheelsWithActivePin);
                        _presentation.ShowWheelPositionsinGrid(_shownWheelPositions);
                        _presentation.labelInput.Content = text[i];
                        _presentation.labelOutput.Content = c;
                    }, null);

                }

                switch (settings.UnknownSymbolHandling)
                {
                    case 0: // Ignore
                        tempOutput += c;
                        break;
                    case 1: // Remove
                        if (isValid)
                        {
                            tempOutput += c;
                        }

                        break;
                    default: // Replace with X
                        if (!isValid)
                        {
                            c = 'X';
                        }

                        tempOutput += c;
                        break;
                }
            }


            //CaseHandling
            switch (settings.CaseHandling)
            {
                case 1:
                    tempOutput = tempOutput.ToUpper();
                    break;
                case 2:
                    tempOutput = tempOutput.ToLower();
                    break;
                default:
                    // do nothing
                    break;
            }

            if (action == 1 && zspace)
            {
                tempOutput = tempOutput.Replace('Z', ' ');
            }

            return tempOutput;
        }

        public string BlockFormat(string s, int blocksize)
        {
            for (int i = blocksize; i < s.Length; i += blocksize + 1)
            {
                s = s.Insert(i, " ");
            }

            return s;
        }

        // Die Methode wird für jeden Buchstaben aufgerufen um die Rotoren zu drehen

        // Rein AAAAAA Raus BBBBBB                                    
        public string rotateWheel(string pos)
        {
            char[] neuePos = new char[6];

            //Durch alle Rotoren
            for (int r = 0; r < settings.Rotoren; r++)
            {
                neuePos[r] = rotoren[r][(getRotorPostion(pos[r], r) + 1) % rotoren[r].Length];
            }

            return new string(neuePos);
        }

        public void UpdateWheelPosition(string key)
        {
            for (int i = 0; i < settings.Rotoren; i++)
            {
                string[] WheelPositionsArray = settings.initrotors[i].ToCharArray().Select(c => c.ToString()).ToArray();
                _shownWheelPositions[i, 0] = WheelPositionsArray[Mod((getRotorPostion(key[i], i) - 2), settings.initrotors[i].Length)];
                _shownWheelPositions[i, 1] = WheelPositionsArray[Mod((getRotorPostion(key[i], i) - 1), settings.initrotors[i].Length)];
                _shownWheelPositions[i, 2] = WheelPositionsArray[Mod((getRotorPostion(key[i], i)), settings.initrotors[i].Length)];
                _shownWheelPositions[i, 3] = WheelPositionsArray[Mod((getRotorPostion(key[i], i) + 1), settings.initrotors[i].Length)];
                _shownWheelPositions[i, 4] = WheelPositionsArray[Mod((getRotorPostion(key[i], i) + 2), settings.initrotors[i].Length)];
                int offset = 0;

                switch (settings.initrotors[i].Length)
                {
                    case 26:
                        offset = 15;
                        break;
                    case 25:
                        offset = 14;
                        break;
                    case 23:
                        offset = 13;
                        break;
                    case 21:
                        offset = 12;
                        break;
                    case 19:
                        offset = 11;
                        break;
                    case 17:
                        offset = 10;
                        break;
                    default:
                        break;
                }

                string curPosition = WheelPositionsArray[(getRotorPostion(key[i], i) + offset) % settings.initrotors[i].Length];
                _activeWheelPositions[i] = curPosition;

                var position = getRotorPostion(key[i], i);

                _wheelsWithActivePin[i] = pins[i, getRotorPostion(key[i], i)];
            }
        }

        private int Mod(int x, int m)
        {
            return (x % m + m) % m;
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings => settings;

        public UserControl Presentation => _presentation;

        public UserControl QuickWatchPresentation => null;

        public void PreExecution()
        {
        }

        public void Execute()
        {
            ProgressChanged(0, 1);

            if (Text == null)
            {
                GuiLogMessage("Input is not set. Execution is stopped!", NotificationLevel.Warning);
                return;
            }

            if (settings.Startwert.Length != settings.Rotoren)
            {
                GuiLogMessage(string.Format("The key does not have the expected length ({0} instead of {1}).", settings.Startwert.Length, settings.Rotoren), NotificationLevel.Error);
                return;
            }

            for (int i = 0; i < settings.Startwert.Length; i++)
            {
                if (!settings.initrotors[i].Contains(settings.Startwert[i]))
                {
                    GuiLogMessage(string.Format("The key contains the illegal character '{0}' at position {1}.", settings.Startwert[i], i + 1), NotificationLevel.Error);
                    return;
                }
            }

            setPins();
            setBars();

            if (_gridsToBeCreated)
            {
                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    _presentation.CreateDinamicInfoGrid();
                    _presentation.CreateWheelsInfoGrids();
                    _gridsToBeCreated = false;
                }, null);
            }

            // check if each rotor has at least one active pin
            bool found = false;
            for (int i = 0; i < settings.Rotoren; i++)
            {
                found = false;
                for (int j = 0; j < settings.initrotors[i].Length; j++)
                {
                    if (pins[i, j]) { found = true; break; }
                }

                if (!found)
                {
                    break;
                }
            }
            if (!found)
            {
                GuiLogMessage("The internal key is weak. At least one rotor has no active pins.", NotificationLevel.Warning);
            }

            // check if the lug setting is capable of generating all offsets
            int ofs = CheckLugs();
            if (ofs != 26)
            {
                GuiLogMessage(string.Format("The internal key is weak. It generates only {0} of the 26 possible offsets.", ofs - 1), NotificationLevel.Warning);
            }


            //string check = BlockFormat(cipherText("AAAAAAAAAAAAAAAAAAAAAAAAAA", "AAAAAA", 0, true), 5);
            //GuiLogMessage(string.Format("26 letters check: {0}", check), NotificationLevel.Debug);

            OutputString = cipherText(Text, settings.Startwert, settings.Action, settings.ZSpace);
            if (settings.BlockOutput)
            {
                OutputString = BlockFormat(OutputString, 5);  // in Fünfergruppen ausgeben
            }


            OnPropertyChanged("OutputString");
            OnPropertyChanged("OutputInternalKey");
            OnPropertyChanged("KeyCheck");

            ProgressChanged(1, 1);
        }

        public void PostExecution()
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

        #region KeyGeneration

        private readonly List<List<int>> pairs = new List<List<int>> { };

        public void RandomKey()
        {
            for (int i = 0; i < 6; i++)
            {
                for (int j = i + 1; j < 6; j++)
                {
                    pairs.Add(new List<int> { i, j });
                }
            }

            for (int iterations = 0; iterations < 10; iterations++)
            {
                // set pins

                bool[] pins = new bool[156];
                int tries = 0;
                do
                {
                    SetRandomPins(pins);
                    tries++;
                } while (!CheckForRuns(pins, 6));

                // set lugs

                int[,] lugset = (rnd.Next(10) > 0) ? LugSettings.SetA : LugSettings.SetB;
                int r = rnd.Next(lugset.Length / 6);
                int[] set = new int[6];
                for (int i = 0; i < 6; i++)
                {
                    set[i] = lugset[r, i];
                }

                set = Shuffle(set);

                int overlap = set.Sum() - 27;

                List<List<int>> overlaps = null;
                int maxtries = 10000;
                for (int i = 0; i < maxtries; i++)
                {
                    overlaps = GenerateOverlaps(set, overlap);
                    if (overlaps != null && CheckOverlaps(overlaps))
                    {
                        LugsFromOverlaps(set, overlaps);
                        if (CheckLugs() == 26)
                        {   // if valid key was found, transfer it to the settings
                            updatePins(pins);
                            updateLugs();

                            int activepins = settings.ActivePins;
                            int totalpins = settings.TotalPins;
                            double percentage = (100.0 * activepins) / totalpins;
                            GuiLogMessage(string.Format("{0} of {1} pins are active ({2:0.00}%). Found after {3} tries.", activepins, totalpins, percentage, tries), NotificationLevel.Debug);

                            OnPropertyChanged("OutputInternalKey");
                            return;
                        }
                    }
                }
            }

            GuiLogMessage("The key generation heuristic failed. Please try again.", NotificationLevel.Warning);
        }

        public void updatePins(bool[] pins)
        {
            string[] pinstrings = new string[6];

            for (int i = 0; i < 6; i++)
            {
                pinstrings[i] = "";
                for (int j = 0; j < settings.initrotors[i].Length; j++)
                {
                    if (pins[26 * i + j])
                    {
                        pinstrings[i] += settings.initrotors[i][j];
                    }
                }
            }

            updatePins(pinstrings);
        }

        public void updatePins(string[] pinstrings)
        {
            settings.Rotor1 = pinstrings[0];
            settings.Rotor2 = pinstrings[1];
            settings.Rotor3 = pinstrings[2];
            settings.Rotor4 = pinstrings[3];
            settings.Rotor5 = pinstrings[4];
            settings.Rotor6 = pinstrings[5];
        }

        // Schieber von Array 'StangeSchieber' in GUI
        public void updateLugs()
        {
            string[] lugs = new string[settings.Stangen];

            for (int i = 0; i < settings.Stangen; i++)
            {
                lugs[i] = ((StangeSchieber[i, 0] > 0) ? StangeSchieber[i, 0].ToString() : "")
                        + ((StangeSchieber[i, 1] > 0) ? StangeSchieber[i, 1].ToString() : "");
            }

            updateLugs(lugs);
        }

        public void updateLugs(string[] lugs)
        {
            settings.Bar1 = lugs[0];
            settings.Bar2 = lugs[1];
            settings.Bar3 = lugs[2];
            settings.Bar4 = lugs[3];
            settings.Bar5 = lugs[4];
            settings.Bar6 = lugs[5];
            settings.Bar7 = lugs[6];
            settings.Bar8 = lugs[7];
            settings.Bar9 = lugs[8];
            settings.Bar10 = lugs[9];
            settings.Bar11 = lugs[10];
            settings.Bar12 = lugs[11];
            settings.Bar13 = lugs[12];
            settings.Bar14 = lugs[13];
            settings.Bar15 = lugs[14];
            settings.Bar16 = lugs[15];
            settings.Bar17 = lugs[16];
            settings.Bar18 = lugs[17];
            settings.Bar19 = lugs[18];
            settings.Bar20 = lugs[19];
            settings.Bar21 = lugs[20];
            settings.Bar22 = lugs[21];
            settings.Bar23 = lugs[22];
            settings.Bar24 = lugs[23];
            settings.Bar25 = lugs[24];
            settings.Bar26 = lugs[25];
            settings.Bar27 = lugs[26];
        }

        private void LugsFromOverlaps(int[] set, List<List<int>> overlaps)
        {
            int[] remaining = (int[])set.Clone();

            List<int[]> l_single = new List<int[]>();
            List<int[]> l_double = new List<int[]>();

            foreach (List<int> o in overlaps)
            {
                for (int j = 0; j < o[2]; j++)
                {
                    l_double.Add(new int[2] { o[0] + 1, o[1] + 1 });
                }

                remaining[o[0]] -= o[2];
                remaining[o[1]] -= o[2];
            }

            for (int i = 0; i < 6; i++)
            {
                for (int l = 0; l < remaining[i]; l++)
                {
                    l_single.Add(new int[1] { i + 1 });
                }
            }

            l_single.AddRange(l_double);
            if (l_single.Count > settings.Stangen)
            {
                GuiLogMessage("Generated more bars than are available. The excess is ignored.", NotificationLevel.Warning);
                l_single = (List<int[]>)l_single.Take(settings.Stangen);
            }

            clearBars();

            for (int i = 0; i < l_single.Count; i++)
            {
                for (int j = 0; j < l_single[i].Length; j++)
                {
                    StangeSchieber[i, j] = l_single[i][j];
                }
            }
        }

        // Test, if all offsets from 0 to 25 can be generated with the given lugs.
        private int CheckLugs()
        {
            HashSet<int> numbers = new HashSet<int>();

            for (int i = 0; i < 64; i++)
            {
                numbers.Add(countStangen(i) % 26);
                if (numbers.Count == 26)
                {
                    break;
                }
            }

            return numbers.Count;
        }

        private List<List<int>> GenerateOverlaps(int[] set, int overlap)
        {
            List<List<int>> shuffledPairs = Shuffle(pairs).ToList();

            int[] remaining = (int[])set.Clone();

            int divisor = (overlap <= 3) ? 1 : ((overlap <= 8) ? 2 : 3);

            int chunk_limit = Math.Max(1, Math.Min(4, overlap / divisor));

            List<List<int>> overlaps = new List<List<int>>();
            foreach (List<int> pair in shuffledPairs)
            {
                int x = pair[0];
                int y = pair[1];
                if (overlap <= 0)
                {
                    break;
                }

                int max_chunk = Math.Min(Math.Min(Math.Min(remaining[x], remaining[y]), overlap), chunk_limit);
                if (max_chunk <= 0)
                {
                    continue;
                }

                int chunk = rnd.Next(1, max_chunk);

                overlap -= chunk;
                remaining[x] -= chunk;
                remaining[y] -= chunk;

                overlaps.Add((new List<int> { x, y, chunk }));
            }

            return (overlap == 0) ? overlaps : null;
        }

        private bool CheckOverlaps(List<List<int>> overlaps)
        {
            if (overlaps.Count >= 3)
            {
                HashSet<int> numbers = new HashSet<int>();
                foreach (List<int> o in overlaps)
                {
                    numbers.Add(o[0]);
                    numbers.Add(o[1]);
                }
                if (numbers.Count <= 3)
                {
                    return false;
                }
            }

            if (overlaps.Count >= 2)
            {
                bool found = false;
                foreach (List<int> o in overlaps)
                {
                    if (o[1] - o[0] > 1) { found = true; break; }
                }

                if (!found)
                {
                    return false;
                }

                found = false;
                foreach (List<int> o in overlaps)
                {
                    if (o[1] - o[0] == 1) { found = true; break; }
                }

                if (!found)
                {
                    return false;
                }
            }

            return true;
        }

        private T[] Shuffle<T>(IEnumerable<T> source)
        {
            T[] elements = source.ToArray();
            for (int i = elements.Length - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                T tmp = elements[i]; elements[i] = elements[j]; elements[j] = tmp;
            }
            return elements;
        }

        private void SetRandomPins(bool[] pins)
        {
            for (int i = 0; i < pins.Count(); i++)
            {
                pins[i] = (i < 78);
            }

            for (int i = 0; i < pins.Count(); i++)
            {
                int j = rnd.Next(pins.Count());
                bool tmp = pins[i];
                pins[i] = pins[j];
                pins[j] = tmp;
            }
        }

        private bool CheckForRuns(bool[] pins, int maxrunlength)
        {
            for (int i = 0; i < 6; i++)
            {
                bool cur = false;
                int runlength = 0;
                for (int j = 0; j < settings.initrotors[i].Length + maxrunlength - 1; j++)
                {
                    bool p = pins[26 * i + (j % settings.initrotors[i].Length)];
                    if (cur == p)
                    {
                        runlength++;
                        if (runlength > maxrunlength)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        cur = p;
                        runlength = 1;
                    }
                }
            }

            return true;
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
