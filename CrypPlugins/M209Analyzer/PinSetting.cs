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
        public Wheel[] Wheels = new Wheel[6]
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
            for (int i = 0; i < Wheels.Length; i++)
            {
                Wheels[i].RandomizeAllPinValues();
            }
        }

        public PinSetting[] GetNeighborPins()
        {
            PinSetting[] pins = new PinSetting[] { 
                this, this, this, this
            };

            pins[0].ToggleSingleWheelSinglePinTransformation();
            pins[1].ToggleSingleWheelTwoPinsTransformation();
            pins[2].ToggleTwoWheelsTwoPinsTransformation();
            pins[3].ToggleSingleWheelAllPinsTransformation();

            return pins;
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
            int randomPin = this.Wheels[randomWheel].GetRandomPinPosition();
            this.Wheels[randomWheel].TogglePinValue(randomPin);
        }

        /// <summary>
        /// Toggle the state of a pair of pins, from the same wheel, which have dierent states (one is 0
        /// and the other is 1, or vice versa).
        /// </summary>
        public void ToggleSingleWheelTwoPinsTransformation()
        {
            int randomWheel = this.Randomizer.Next(0, 5);
            int randomPin1 = this.Wheels[randomWheel].GetRandomPinPosition();
            this.Wheels[randomWheel].TogglePinValue(randomPin1);

            int randomPin2 = 0;
            // Maybe it could be better to run systematically throug the pins...
            //while (this.Wheels[randomWheel].EvaluatePinAtPosition(randomPin1) != this.Wheels[randomWheel].EvaluatePinAtPosition(randomPin2))
            //{
            //    randomPin2 = this.Wheels[randomWheel].GetRandomPinPosition();
            //}
            for (int i = 0; i < this.Wheels[randomWheel].Length; i++)
            {
                if (this.Wheels[randomWheel].EvaluatePinAtPosition(randomPin1) != this.Wheels[randomWheel].EvaluatePinAtPosition(randomPin2))
                {
                    randomPin2 = i;
                    break;
                }
            }
            this.Wheels[randomWheel].TogglePinValue(randomPin2);
        }

        /// <summary>
        /// Toggle the state of a pair of two pins, from different wheels, which have differerent states.
        /// </summary>
        public void ToggleTwoWheelsTwoPinsTransformation()
        {
            int randomWheel1 = this.Randomizer.Next(0, 5);
            int randomPin1 = this.Wheels[randomWheel1].GetRandomPinPosition();
            this.Wheels[randomWheel1].TogglePinValue(randomPin1);

            int randomWheel2 = this.Randomizer.Next(0, 5);
            while (randomWheel2 == randomWheel1)
            {
                randomWheel2 = this.Randomizer.Next(0, 5);
            }

            int randomPin2 = this.Wheels[randomWheel2].GetRandomPinPosition();
            //while (this.Wheels[randomWheel1].EvaluatePinAtPosition(randomPin1) != this.Wheels[randomWheel2].EvaluatePinAtPosition(randomPin2))
            //{
            //    randomPin2 = this.Wheels[randomWheel2].GetRandomPinPosition();
            //}
            for (int i = 0; i < this.Wheels[randomWheel2].Length; i++)
            {
                if (this.Wheels[randomWheel1].EvaluatePinAtPosition(randomPin1) != this.Wheels[randomWheel2].EvaluatePinAtPosition(randomPin2))
                {
                    randomPin2 = i;
                    break;
                }
            }
            this.Wheels[randomWheel2].TogglePinValue(randomPin2);

        }

        /// <summary>
        /// Toggle the state of all the pins of a wheel.
        /// </summary>
        public void ToggleSingleWheelAllPinsTransformation()
        {
            for (int i = 0; i < this.Wheels.Length; i++)
            {
                this.Wheels[i].ToggleAllPinValues();
            }
        }
    }
}
