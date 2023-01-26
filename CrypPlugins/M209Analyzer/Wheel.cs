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
            _pinSettings = new bool[letters.Length];
            _position = initialPosition;
            Length = letters.Length;
        }

        private string _letters;
        private int _position = 0;
        private bool[] _pinSettings;
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
            return this._pinSettings[this._position];
        }

        /// <summary>
        /// Returns the bool value of the pin in the given position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool EvaluatePinAtPosition(int position)
        {
            return this._pinSettings[position];
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
                if (this._pinSettings[i])
                {
                    effectiveLetters += this._letters[i];
                }
            }
            return effectiveLetters;
        }

        public void SetPinValue(int position, bool value)
        {
            this._pinSettings[position] = value;
        }

        /// <summary>
        /// Use the string representation of pin settings to set the pin setting.
        /// </summary>
        /// <param name="pinSetting"></param>
        public void SetAllPinValuesUsingString(string pinSetting)
        {
            int shift = 0;
            int length = pinSetting.Length;
            for (int i = 0; i < this._pinSettings.Length; i++)
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
                    this._pinSettings[i] = true;
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
            this._pinSettings[position] = !this._pinSettings[position];
        }

        /// <summary>
        /// Toggle the effective state of all pins
        /// </summary>
        public void ToggleAllPinValues()
        {
            for (int i = 0; i < this._pinSettings.Length; i++)
            {
                this.TogglePinValue(i);
            }
        }

        /// <summary>
        /// Returns a random position on the current wheel.
        /// </summary>
        /// <returns></returns>
        public int GetRandomPinPosition()
        {
            return this._randomizer.Next(0, this._pinSettings.Length - 1);
        }

        /// <summary>
        /// Randomize the effective state on an pin settings.
        /// </summary>
        public void RandomizeAllPinValues()
        {
            for (int i = 0; i < this._pinSettings.Length; i++)
            {
                this._pinSettings[i] = this._randomizer.Next(0, 1) == 1;
            }
        }

        /// <summary>
        /// Returns the amount of effectivePins
        /// </summary>
        /// <returns></returns>
        public int CountOfEffectivePins()
        {
            int count = 0;
            for (int i = 0; i < this._pinSettings.Length; i++)
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
