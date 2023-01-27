using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M209Analyzer
{
    internal class Wheel
    {

        public Wheel(string letters, int initialPosition)
        {
            _letters = letters;
            _pinEffectiveStates = new bool[letters.Length];
            _position = initialPosition;
            Length = letters.Length;
        }

        private string _letters;
        private int _position = 0;
        private bool[] _pinEffectiveStates;
        private Random _randomizer = new Random();

        public int Length = 0;

        /// <summary>
        /// Increment the _position simulating a rotation and set the _position to 0 if a round is finished.
        /// </summary>
        public void Rotate()
        {
            if (this._position < this._letters.Length - 1)
            {
                this._position++;
            }
            else if (this._position == this._letters.Length - 1)
            {
                this._position = 0;
            }
        }

        /// <summary>
        /// Returns the bool value of the pin which is currently in position.
        /// </summary>
        /// <returns></returns>
        public bool EvaluateCurrentPin()
        {
            return this._pinEffectiveStates[this._position];
        }

        /// <summary>
        /// Returns the bool value of the pin in the given position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool EvaluatePinAtPosition(int position)
        {
            return this._pinEffectiveStates[position];
        }

        /// <summary>
        /// Returns the letter which is currently in position.
        /// </summary>
        /// <returns></returns>
        public char GetCurrentLetter()
        {
            return this._letters[this._position];
        }

        /// <summary>
        /// Returns all effective letters to get the string representation of the pin settings.
        /// </summary>
        /// <returns></returns>
        public string GetEffectiveLetters()
        {
            string effectiveLetters = "";

            for (int i = 0; i < this._letters.Length; i++)
            {
                if (this._pinEffectiveStates[i])
                {
                    effectiveLetters += this._letters[i];
                }
            }
            return effectiveLetters;
        }

        public void SetPinValue(int position, bool value)
        {
            this._pinEffectiveStates[position] = value;
        }

        /// <summary>
        /// Use the string representation of pin settings to set the pin setting.
        /// </summary>
        /// <param name="pinSetting"></param>
        public void SetAllPinValuesUsingString(string pinSetting)
        {
            int shift = 0;
            int length = pinSetting.Length;
            for (int i = 0; i < this._pinEffectiveStates.Length; i++)
            {
                if ((i - shift) == pinSetting.Length)
                {
                    if (this.GetEffectiveLetters() == pinSetting)
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("ERROR");
                    }
                }
                char c = this._letters[i];
                char p = pinSetting[i - shift];
                if (pinSetting[i - shift] == this._letters[i])
                {
                    this._pinEffectiveStates[i] = true;
                }
                else
                {
                    shift++;
                }
            }

        }

        /// <summary>
        /// Toggle the effective state of the pin at the given position.
        /// </summary>
        /// <param name="position"></param>
        public void TogglePinValue(int position)
        {
            this._pinEffectiveStates[position] = !this._pinEffectiveStates[position];
        }

        /// <summary>
        /// Toggle the effective state of all pins
        /// </summary>
        public void ToggleAllPinValues()
        {
            for (int i = 0; i < this._pinEffectiveStates.Length; i++)
            {
                this.TogglePinValue(i);
            }
        }

        /// <summary>
        /// Toggle the effective state of pins of position p1 and p2
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public void ToggleTwoPins(int p1, int p2)
        {
            this._pinEffectiveStates[p1] = !this._pinEffectiveStates[p1];
            this._pinEffectiveStates[p2] = !this._pinEffectiveStates[p2];
        }

        public bool longSeq(int centerPos1, int centerPos2)
        {
            return longSeq(centerPos1) || longSeq(centerPos2);
        }

        public bool longSeq(int centerPos)
        {
            long maxSame = 6; // MAX_CONSECUTIVE_SAME_PINS
            int total = this._pinEffectiveStates.Length;

            if ( maxSame > total)
            {
                return false;
            }
            maxSame++; // Not strict.
            bool centerVal = this.EvaluatePinAtPosition(centerPos);

            int same;
            int pos;
            for (same = 1, pos = centerPos + 1; same <= maxSame; same++, pos++)
            {
                if (pos == total)
                {
                    pos = 0;
                }
                if (this.EvaluatePinAtPosition(pos) != centerVal)
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
                if (this.EvaluatePinAtPosition(pos) != centerVal)
                {
                    break;
                }
            }
            return same > maxSame;
        }

        /// <summary>
        /// Returns a random position on the current wheel.
        /// </summary>
        /// <returns></returns>
        public int GetRandomPinPosition()
        {
            return this._randomizer.Next(0, this._pinEffectiveStates.Length - 1);
        }

        /// <summary>
        /// Randomize the effective state on an pin settings.
        /// </summary>
        public void RandomizeAllPinValues()
        {
            for (int i = 0; i < this._pinEffectiveStates.Length; i++)
            {
                this._pinEffectiveStates[i] = (this._randomizer.Next(0, 2) == 1);
            }
        }

        /// <summary>
        /// Returns the amount of effectivePins
        /// </summary>
        /// <returns></returns>
        public int CountOfEffectivePins()
        {
            int count = 0;
            for (int i = 0; i < this._pinEffectiveStates.Length; i++)
            {
                if (EvaluatePinAtPosition(i))
                {
                    count++;
                }
            }
            return count;
        }
    }
}
