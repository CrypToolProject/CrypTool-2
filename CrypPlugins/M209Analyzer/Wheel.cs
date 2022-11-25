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
        }

        private string _letters;
        private int _position = 0;
        private bool[] _pinSettings;
        private Random _randomizer = new Random();

        public void Rotate()
        {
            if (_position < _letters.Length - 1)
            {
                _position++;
            }
            else if (_position == _letters.Length - 1)
            {
                _position = 0;
            }
        }

        public bool EvaluateCurrentPin()
        {
            return _pinSettings[_position];
        }

        public bool EvaluatePinAtPosition(int position)
        {
            return _pinSettings[position];
        }

        public char GetCurrentLetter()
        {
            return _letters[_position];
        }

        public string GetEffectiveLetters()
        {
            string effectiveLetters = "";

            for (int i = 0; i < _letters.Length; i++)
            {
                if (_pinSettings[i])
                {
                    effectiveLetters += _letters[i];
                }
            }
            return effectiveLetters;
        }

        public void SetPinValue(int position, bool value)
        {
            _pinSettings[position] = value;
        }

        public void SetAllPinValuesUsingString(string pinSetting)
        {
            int shift = 0;
            int length = pinSetting.Length;
            for (int i = 0; i < _pinSettings.Length; i++)
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
                char c = _letters[i];
                char p = pinSetting[i - shift];
                if (pinSetting[i - shift] == _letters[i])
                {
                    _pinSettings[i] = true;
                }
                else
                {
                    shift++;
                }
            }

        }

        public void TogglePinValue(int position)
        {
            _pinSettings[position] = !_pinSettings[position];
        }

        public void ToggleAllPinValues()
        {
            for (int i = 0; i < _pinSettings.Length; i++)
            {
                this.TogglePinValue(i);
            }
        }

        public int GetRandomPinPosition()
        {
            return _randomizer.Next(0, _pinSettings.Length - 1);
        }

        public void RandomizeAllPinValues()
        {
            for (int i = 0; i < _pinSettings.Length; i++)
            {
                _pinSettings[i] = _randomizer.Next(0, 1) == 1;
            }
        }
    }
}
