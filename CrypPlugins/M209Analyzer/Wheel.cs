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

        public bool EvaluateCurrentPin()
        {
            return this._pinSettings[this._position];
        }

        public bool EvaluatePinAtPosition(int position)
        {
            return this._pinSettings[position];
        }

        public char GetCurrentLetter()
        {
            return this._letters[this._position];
        }

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

        public void TogglePinValue(int position)
        {
            this._pinSettings[position] = !this._pinSettings[position];
        }

        public void ToggleAllPinValues()
        {
            for (int i = 0; i < this._pinSettings.Length; i++)
            {
                this.TogglePinValue(i);
            }
        }

        public int GetRandomPinPosition()
        {
            return this._randomizer.Next(0, this._pinSettings.Length - 1);
        }

        public void RandomizeAllPinValues()
        {
            for (int i = 0; i < this._pinSettings.Length; i++)
            {
                this._pinSettings[i] = this._randomizer.Next(0, 1) == 1;
            }
        }
    }
}
