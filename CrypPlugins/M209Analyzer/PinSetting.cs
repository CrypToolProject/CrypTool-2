using CrypTool.M209Analyzer.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M209Analyzer
{
    internal class PinSetting
    {
        private Wheel[] _wheels = new Wheel[6]
        {
            new Wheel("ABCDEFGHIJKLMNOPQRSTUVWXYZ", 15),
            new Wheel("ABCDEFGHIJKLMNOPQRSTUVXYZ", 14),      // no W
            new Wheel("ABCDEFGHIJKLMNOPQRSTUVX", 13),        // no WYZ
            new Wheel("ABCDEFGHIJKLMNOPQRSTU", 12),          // no V-Z
            new Wheel("ABCDEFGHIJKLMNOPQRS", 11),            // no T-Z
            new Wheel("ABCDEFGHIJKLMNOPQ", 10)               // no R-Z
        };

        private Random Randomizer = new Random();

        public void Randomize()
        {
            for (int i = 0; i < _wheels.Length; i++)
            {
                _wheels[i].RandomizeAllPinValues();
            }
        }

        public void GetNeighborPins()
        {

        }

        public void ApplyTransformation(PinSettingTransformation pinSettingTransformation)
        {
            switch (pinSettingTransformation)
            {
                case PinSettingTransformation.ToggleSingleWheelSinglePin:
                    this.ToggleSingleWheelSinglePinTransformation();
                    break;
                case PinSettingTransformation.ToggleSingleWheelTwoPins:
                    this.ToggleSingleWheelTwoPinsTransformation();
                    break;
                case PinSettingTransformation.ToggleTwoWheelsTwoPins:
                    this.ToggleTwoWheelsTwoPinsTransformation();
                    break;
                case PinSettingTransformation.ToggleSingleWheelAllPins:
                    this.ToggleSingleWheelAllPinsTransformation();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Toggle the state of a single wheel pin (change its state from 0 to 1, or from 1 to 0).
        /// </summary>
        public void ToggleSingleWheelSinglePinTransformation()
        {
            int randomWheel = this.Randomizer.Next(0, 5);
            int randomPin = _wheels[randomWheel].GetRandomPinPosition();
            _wheels[randomWheel].TogglePinValue(randomPin);
        }

        /// <summary>
        /// Toggle the state of a pair of pins, from the same wheel, which have dierent states (one is 0
        /// and the other is 1, or vice versa).
        /// </summary>
        public void ToggleSingleWheelTwoPinsTransformation()
        {
            int randomWheel = this.Randomizer.Next(0, 5);
            int randomPin1 = _wheels[randomWheel].GetRandomPinPosition();
            _wheels[randomWheel].TogglePinValue(randomPin1);

            int randomPin2 = _wheels[randomWheel].GetRandomPinPosition();
            // Maybe it could be better to run systematically throug the pins...
            while (_wheels[randomWheel].EvaluatePinAtPosition(randomPin1) != _wheels[randomWheel].EvaluatePinAtPosition(randomPin2))
            {
                randomPin2 = _wheels[randomWheel].GetRandomPinPosition();
            }
            _wheels[randomWheel].TogglePinValue(randomPin2);
        }

        /// <summary>
        /// Toggle the state of a pair of two pins, from different wheels, which have differerent states.
        /// </summary>
        public void ToggleTwoWheelsTwoPinsTransformation()
        {
            int randomWheel1 = this.Randomizer.Next(0, 5);
            int randomPin1 = _wheels[randomWheel1].GetRandomPinPosition();
            _wheels[randomWheel1].TogglePinValue(randomPin1);

            int randomWheel2 = this.Randomizer.Next(0, 5);
            while (randomWheel2 == randomWheel1)
            {
                randomWheel2 = this.Randomizer.Next(0, 5);
            }

            int randomPin2 = _wheels[randomWheel2].GetRandomPinPosition();
            while (_wheels[randomWheel1].EvaluatePinAtPosition(randomPin1) != _wheels[randomWheel2].EvaluatePinAtPosition(randomPin2))
            {
                randomPin2 = _wheels[randomWheel2].GetRandomPinPosition();
            }
            _wheels[randomWheel2].TogglePinValue(randomPin2);

        }

        /// <summary>
        /// Toggle the state of all the pins of a wheel.
        /// </summary>
        public void ToggleSingleWheelAllPinsTransformation()
        {
            for (int i = 0; i < _wheels.Length; i++)
            {
                _wheels[i].ToggleAllPinValues();
            }
        }
    }
}
