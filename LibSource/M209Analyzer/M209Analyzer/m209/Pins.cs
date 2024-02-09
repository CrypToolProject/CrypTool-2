/*
   Copyright CrypTool 2 Team josef.matwich@gmail.com

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
using M209AnalyzerLib.Common;
using System;
using System.Text;

namespace M209AnalyzerLib.M209
{
    public class Pins
    {
        //ISOMORPHIC pins, take into account the indicator and the active offset
        //for a given message at pos P and for wheel W, look at
        // isoPinsReal[W][POS%WHEEL_SIZE[W]]
        public bool[][] IsoPins;

        public string Indicator = Key.NULL_INDICATOR;
        private Key _parentKey = null;

        public bool[] WheelPins1;
        public bool[] WheelPins2;
        public bool[] WheelPins3;
        public bool[] WheelPins4;
        public bool[] WheelPins5;
        public bool[] WheelPins6;

        private static long startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        public Random random = new Random((int)startTime);


        private Pins(Key parentKey)
        {
            _parentKey = parentKey;

            IsoPins = new bool[_parentKey.WHEELS + 1][];

            // Workaround for not possible initialization: new boolean[Key.WHEELS + 1][26];
            for (int i = 0; i < IsoPins.Length; i++)
            {
                IsoPins[i] = new bool[26];
            }

            for (int w = 1; w <= _parentKey.WHEELS; w++)
            {
                IsoPins[w] = new bool[_parentKey.WHEELS_SIZE[w]];
            }
            WheelPins1 = IsoPins[1];
            WheelPins2 = IsoPins[2];
            WheelPins3 = IsoPins[3];
            WheelPins4 = IsoPins[4];
            WheelPins5 = IsoPins[5];
            WheelPins6 = IsoPins[6];
        }

        // Absolute PIN settings, as defined in the key
        public Pins(Key parentKey, string[] absolutePins, string indicator) : this(parentKey)
        {
            // Workaround for not possible initialization: new boolean[Key.WHEELS + 1][26];
            if (IsoPins[0].Length != 26)
            {
                for (int i = 0; i < IsoPins.Length; i++)
                {
                    IsoPins[i] = new bool[26];
                }
            }

            Set(absolutePins, indicator);
        }

        public void Toggle(int w, int pos)
        {
            IsoPins[w][pos] ^= true;
        }

        public void Toggle(int w, int pos1, int pos2)
        {
            IsoPins[w][pos1] ^= true;
            IsoPins[w][pos2] ^= true;
        }

        public bool Compare(int w, int pos1, int pos2)
        {
            return IsoPins[w][pos1] == IsoPins[w][pos2];
        }

        private string AbsolutePinString(int w)
        {
            bool[] pinsW = IsoPins[w];
            StringBuilder s = new StringBuilder();
            for (int index = 0; index < _parentKey.WHEELS_SIZE[w]; index++)
            {
                int isoIndex = IsoIndex(w, index);
                if (pinsW[isoIndex])
                {
                    s.Append(_parentKey.WHEEL_LETTERS[w][index]);
                }
            }
            return s.ToString();
        }

        private string AbsolutePinString0or1(int w)
        {
            StringBuilder s = new StringBuilder();
            for (int index = 0; index < _parentKey.WHEELS_SIZE[w]; index++)
            {
                int isoIndex = IsoIndex(w, index);
                if (IsoPins[w][isoIndex])
                {
                    s.Append("1");
                }
                else
                {
                    s.Append("0");
                }
            }
            return s.ToString();
        }

        public string AbsolutePinStringAll()
        {
            StringBuilder s = new StringBuilder();
            s.Append(Indicator);
            s.Append(" ");
            for (int w = 1; w <= _parentKey.WHEELS; w++)
            {
                s.Append(" ").Append(w).Append(": ");
                s.Append(AbsolutePinString(w));
                s.Append(" (").Append(AbsolutePinString0or1(w)).Append(") ");
                s.Append(",");
            }
            return s.ToString();
        }
        public string AbsolutePinStringAll01()
        {
            StringBuilder s = new StringBuilder();
            s.Append(Indicator);
            s.Append(" ");
            for (int w = 1; w <= _parentKey.WHEELS; w++)
            {
                s.Append(" ").Append(w).Append(": ");
                s.Append(AbsolutePinString0or1(w)).Append(" ");
            }
            return s.ToString();
        }

        public string[] AbsolutePinsStringArray()
        {
            string[] pins = new string[_parentKey.WHEELS];
            for (int w = 1; w <= _parentKey.WHEELS; w++)
            {
                pins[w - 1] = AbsolutePinString(w);
            }
            return pins;

        }

        public bool[][] CreateCopy()
        {
            bool[][] isoPins = new bool[_parentKey.WHEELS + 1][];
            for (int w = 1; w <= _parentKey.WHEELS; w++)
            {
                isoPins[w] = new bool[IsoPins[w].Length];
                Array.Copy(IsoPins[w], 0, isoPins[w], 0, IsoPins[w].Length);
            }
            return isoPins;
        }

        public void Get(bool[][] isoPins)
        {
            for (int w = 1; w <= _parentKey.WHEELS; w++)
            {
                Array.Copy(IsoPins[w], 0, isoPins[w], 0, IsoPins[w].Length);
            }
        }

        public void Set(bool[][] isoPins)
        {
            for (int w = 1; w <= _parentKey.WHEELS; w++)
            {
                Array.Copy(isoPins[w], 0, IsoPins[w], 0, IsoPins[w].Length);
            }
            if (_parentKey != null)
            {
                _parentKey.InvalidateDecryption();
            }
        }

        public bool[] CreateCopy(int w)
        {
            bool[] isoPinsW = new bool[IsoPins[w].Length];
            Array.Copy(IsoPins[w], 0, isoPinsW, 0, IsoPins[w].Length);
            return isoPinsW;
        }

        public void Get(int w, bool[] isoPinsW)
        {
            Array.Copy(IsoPins[w], 0, isoPinsW, 0, IsoPins[w].Length);
        }

        public void Set(int w, bool[] isoPinsW)
        {
            Array.Copy(isoPinsW, 0, IsoPins[w], 0, IsoPins[w].Length);
            if (_parentKey != null)
            {
                _parentKey.InvalidateDecryption();
            }
        }

        private void Set(string[] absolutePinsStringArray, string indicator)
        {
            if (indicator.Length != _parentKey.WHEELS)
            {
                throw new Exception($"Wrong indicator length: {indicator}");
            }
            for (int w = 1; w <= _parentKey.WHEELS; w++)
            {
                char ic = indicator[w - 1];
                int indicatorIndex = _parentKey.WHEEL_LETTERS[w].IndexOf(ic);
                if (indicatorIndex == -1)
                {
                    throw new Exception($"Invalid indicator letter [{ic}] for wheel: {w} - Does not appear in Wheel letters");
                }
            }

            Indicator = indicator;

            if (absolutePinsStringArray == null)
            {
                return;
            }

            if (absolutePinsStringArray.Length == _parentKey.WHEELS)
            {
                for (int w = 1; w <= _parentKey.WHEELS; w++)
                {

                    // need clean!
                    for (int i = 0; i < IsoPins[w].Length; i++)
                    {
                        IsoPins[w][i] = false;
                    }

                    StringBuilder pinw = new StringBuilder();

                    foreach (char c in absolutePinsStringArray[w - 1].ToCharArray())
                    {
                        if (c != ' ')
                        {
                            pinw.Append(c);
                        }
                    }
                    if (pinw.Length > _parentKey.WHEELS_SIZE[w])
                    {
                        throw new Exception($"Too many isoPinsReal for wheel: {w} ({absolutePinsStringArray[w]})");
                    }
                    foreach (char c in pinw.ToString().ToCharArray())
                    {
                        int index = _parentKey.WHEEL_LETTERS[w].IndexOf(c);
                        if (index == -1)
                        {
                            throw new Exception($"Invalid letter [{c}]for wheel: {w} ({absolutePinsStringArray[w]})");
                        }

                        int isoIndex = IsoIndex(w, index);

                        if (IsoPins[w][isoIndex])
                        {
                            throw new Exception($"Duplicate letter [{c} ]for wheel: {w} ({absolutePinsStringArray[w]})");
                        }
                        IsoPins[w][isoIndex] = true;
                    }
                }
            }
            else if (absolutePinsStringArray.Length == _parentKey.WHEELS_SIZE[1])
            {
                for (int w = 1; w <= _parentKey.WHEELS; w++)
                {
                    // need clean!
                    for (int i = 0; i < IsoPins[w].Length; i++)
                    {
                        IsoPins[w][i] = false;
                    }

                    StringBuilder pinw = new StringBuilder();

                    foreach (string s in absolutePinsStringArray)
                    {
                        if ((s.Length >= w) && (s[w - 1] != '-'))
                        {
                            pinw.Append(s[w - 1]);
                        }
                    }

                    if (pinw.Length > _parentKey.WHEELS_SIZE[w])
                    {
                        throw new Exception($"Too many isoPinsReal for wheel: {w} ({pinw})");
                    }
                    foreach (char c in pinw.ToString().ToCharArray())
                    {
                        int index = _parentKey.WHEEL_LETTERS[w].IndexOf(c);
                        if (index == -1)
                        {
                            throw new Exception($"Invalid letter [{c}]for wheel: {w} ({pinw})");
                        }

                        int isoIndex = IsoIndex(w, index);

                        if (IsoPins[w][isoIndex])
                        {
                            throw new Exception($"Duplicate letter [{c}]for wheel: {w} ({pinw})");
                        }
                        IsoPins[w][isoIndex] = true;
                    }
                }
            }
            else
            {
                throw new Exception($"Wrong pins - size of array : {absolutePinsStringArray.Length}");
            }
            if (_parentKey != null)
            {
                _parentKey.InvalidateDecryption();
            }
        }

        private int IsoIndex(int w, int index)
        {
            char ic = Indicator[w - 1];
            int indicatorIndex = _parentKey.WHEEL_LETTERS[w].IndexOf(ic);

            char activeC = _parentKey.WHEELS_ACTIVE_PINS[w];
            int activeIndex = _parentKey.WHEEL_LETTERS[w].IndexOf(activeC);

            return (index - activeIndex - indicatorIndex + 2 * _parentKey.WHEELS_SIZE[w]) % _parentKey.WHEELS_SIZE[w];
        }

        /// <summary>
        /// Randomize the state of all pins on wheel w.
        /// </summary>
        /// <param name="w">Wheel No.</param>
        public void Randomize(int w)
        {

            bool[] isoPinsW = IsoPins[w];
            int total = IsoPins[w].Length;
            int maxActive = (Global.MAX_PERCENT_ACTIVE_PINS * total) / 100;
            int maxInactive = ((100 - Global.MIN_PERCENT_ACTIVE_PINS) * total) / 100;
            bool good;
            do
            {
                for (int i = 0; i < isoPinsW.Length; i++)
                {
                    isoPinsW[i] = false;
                }
                good = true;
                int countActive = 0;
                int countInactive = 0;
                int rand = random.Next();
                int consecutiveActive = 0;
                int consecutiveInactive = 0;
                for (int p = 0; p < total; p++)
                {
                    bool newVal = (rand & 0x1) == 0x1;
                    bool cannotBeActive = consecutiveActive == Global.MAX_CONSECUTIVE_SAME_PINS || countActive == maxActive;
                    bool cannotBeInactive = consecutiveInactive == Global.MAX_CONSECUTIVE_SAME_PINS || countInactive == maxInactive;
                    if (cannotBeActive && cannotBeInactive)
                    {
                        good = false;
                        break;
                    }
                    else if (cannotBeActive)
                    {
                        newVal = false;
                    }
                    else if (cannotBeInactive)
                    {
                        newVal = true;
                    }
                    if (newVal)
                    {
                        countActive++;
                        consecutiveActive++;
                        consecutiveInactive = 0;
                    }
                    else
                    {
                        countInactive++;
                        consecutiveInactive++;
                        consecutiveActive = 0;
                    }
                    isoPinsW[p] = newVal;
                    rand >>= 1;
                }

                if (!good)
                {
                    continue;
                }
                if (Global.MAX_CONSECUTIVE_SAME_PINS < 10)
                {
                    int rounds = 0;
                    for (int p = 0, unchanged = 0; unchanged <= Global.MAX_CONSECUTIVE_SAME_PINS; p = (p == total - 1) ? 0 : (p + 1))
                    {
                        rounds++;
                        if (rounds > 100)
                        {
                            good = false;
                            break;
                        }
                        bool currentVal = isoPinsW[p];
                        bool newVal = currentVal;
                        bool cannotBeActive = consecutiveActive == Global.MAX_CONSECUTIVE_SAME_PINS;
                        bool cannotBeInactive = consecutiveInactive == Global.MAX_CONSECUTIVE_SAME_PINS;
                        if (cannotBeActive && cannotBeInactive)
                        {
                            good = false;
                            break;
                        }
                        else if (cannotBeActive)
                        {
                            newVal = false;
                        }
                        else if (cannotBeInactive)
                        {
                            newVal = true;
                        }
                        if (newVal == currentVal)
                        {
                            // No change - only update consecutive counters.
                            if (currentVal)
                            {
                                consecutiveActive++;
                                consecutiveInactive = 0;
                            }
                            else
                            {
                                consecutiveInactive++;
                                consecutiveActive = 0;
                            }
                            unchanged++;
                        }
                        else
                        {
                            // Change the sign.
                            cannotBeActive = countActive == maxActive;
                            cannotBeInactive = countInactive == maxInactive;
                            if ((newVal && cannotBeActive) || (!newVal && cannotBeInactive))
                            {
                                good = false;
                                break;
                            }
                            if (newVal)
                            {
                                countActive++;
                                countInactive--; // We changed the sign!.
                                consecutiveActive++;
                                consecutiveInactive = 0;
                            }
                            else
                            {
                                countInactive++;
                                countActive--; // We changed the sign.
                                consecutiveInactive++;
                                consecutiveActive = 0;
                            }
                            isoPinsW[p] = newVal;
                            unchanged = 0;
                        }
                    }

                }
            } while (!good);


            if (_parentKey != null)
            {
                _parentKey.InvalidateDecryption();
            }
        }

        public bool LongSeq(int w, int centerPos1, int centerPos2)
        {
            return LongSeq(w, centerPos1) || LongSeq(w, centerPos2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="w">Wheel No.</param>
        /// <param name="centerPos"></param>
        /// <returns>boolean</returns>
        public bool LongSeq(int w, int centerPos)
        {
            long maxSame = Global.MAX_CONSECUTIVE_SAME_PINS;
            bool[] isoPinsW = IsoPins[w];
            int total = isoPinsW.Length;
            if (maxSame > total)
            {
                return false;
            }
            maxSame++; // Not strict.
            bool centerVal = isoPinsW[centerPos];

            int same;
            int pos;
            for (same = 1, pos = centerPos + 1; same <= maxSame; same++, pos++)
            {
                if (pos == total)
                {
                    pos = 0;
                }
                if (isoPinsW[pos] != centerVal)
                {
                    break;
                }
            }

            for (pos = centerPos - 1; same <= maxSame; same++, pos--)
            {
                if (pos < 0)
                {
                    pos = total - 1;
                }
                if (isoPinsW[pos] != centerVal)
                {
                    break;
                }
            }
            return same > maxSame;
        }

        /// <summary>
        /// Generates random pin settings for all wheels.
        /// </summary>
        public void Randomize()
        {
            do
            {
                for (int w = 1; w <= _parentKey.WHEELS; w++)
                {
                    Randomize(w);
                }
            } while ((CountActivePins() < MinCount()) || (CountActivePins() > MaxCount()));
            if (_parentKey != null)
            {
                _parentKey.InvalidateDecryption();
            }
        }
        public int MaxCount()
        {
            return Utils.Sum(_parentKey.WHEELS_SIZE) * Global.MAX_PERCENT_ACTIVE_PINS / 100;
        }
        public int MinCount()
        {
            return Utils.Sum(_parentKey.WHEELS_SIZE) * Global.MIN_PERCENT_ACTIVE_PINS / 100;
        }
        public void Inverse(int w)
        {
            bool[] isoPinsW = IsoPins[w];
            for (int p = 0; p < _parentKey.WHEELS_SIZE[w]; p++)
            {
                isoPinsW[p] ^= true;
            }
            if (_parentKey != null)
            {
                _parentKey.InvalidateDecryption();
            }
        }

        public void InverseWheelBitmap(int v)
        {
            for (int w = 1; w <= _parentKey.WHEELS; w++)
            {
                if (_parentKey.GetWheelBit(v, w))
                {
                    Inverse(w);
                }
            }
            if (_parentKey != null)
            {
                _parentKey.InvalidateDecryption();
            }
        }

        public int CountActivePins()
        {
            int count = 0;
            var isoPins = IsoPins;
            foreach (bool[] isoPinsW in isoPins)
            {
                foreach (bool val in isoPinsW)
                {
                    if (val)
                    {
                        count++;
                    }
                }
            }
            return count;
        }
    }
}
