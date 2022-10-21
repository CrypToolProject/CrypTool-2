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
        private string[] Wheels = new string[6] {
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
            "ABCDEFGHIJKLMNOPQRSTUVXYZ",    // no W
            "ABCDEFGHIJKLMNOPQRSTUVX",      // no WYZ
            "ABCDEFGHIJKLMNOPQRSTU",        // no V-Z
            "ABCDEFGHIJKLMNOPQRS",          // no T-Z
            "ABCDEFGHIJKLMNOPQ"             // no R-Z
        };

        private bool[][] WheelPins = new bool[][]
        {
            new bool[26] { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
            new bool[25] { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
            new bool[23] { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
            new bool[21] { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
            new bool[19] { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
            new bool[17] { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true }
        };

        private Random Randomizer = new Random();

        public string[] WheelsSettings()
        {
            string[] wheelsSettings = new string[] { };
            for (int i = 0; i < this.Wheels.Length; i++)
            {
                string wheelSetting = "";
                for (int k = 0; k < this.Wheels[i].Length; k++)
                {
                    if (this.WheelPins[i][k])
                    {
                        wheelSetting += this.Wheels[i][k];
                    }
                }
                wheelsSettings.Append(wheelSetting);
            }
            return wheelsSettings;
        }

        public void Randomize()
        {
            for (int i = 0; i < this.WheelPins.Length; i++)
            {
                for (int k = 0; k < this.WheelPins[i].Length; k++)
                {
                    this.WheelPins[i][k] = this.Randomizer.Next(0,1) == 1;
                }
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
            int randomPin = this.Randomizer.Next(0, this.WheelPins[randomWheel].Length - 1);
            this.WheelPins[randomWheel][randomPin] = !this.WheelPins[randomWheel][randomPin];
        }

        /// <summary>
        /// Toggle the state of a pair of pins, from the same wheel, which have dierent states (one is 0
        /// and the other is 1, or vice versa).
        /// </summary>
        public void ToggleSingleWheelTwoPinsTransformation()
        {
            int randomWheel = this.Randomizer.Next(0, 5);
            int randomPin1 = this.Randomizer.Next(0, this.WheelPins[randomWheel].Length - 1);
            this.WheelPins[randomWheel][randomPin1] = !this.WheelPins[randomWheel][randomPin1];

            int randomPin2 = this.Randomizer.Next(0, this.WheelPins[randomWheel].Length - 1);
            // Maybe it could be better to run systematically throug the pins...
            while (this.WheelPins[randomWheel][randomPin2] != this.WheelPins[randomWheel][randomPin1])
            {
                randomPin2 = this.Randomizer.Next(0, this.WheelPins[randomWheel].Length - 1);
            }
            this.WheelPins[randomWheel][randomPin2] = !this.WheelPins[randomWheel][randomPin2];
        }

        /// <summary>
        /// Toggle the state of a pair of two pins, from different wheels, which have differerent states.
        /// </summary>
        public void ToggleTwoWheelsTwoPinsTransformation()
        {
            int randomWheel1 = this.Randomizer.Next(0, 5);
            int randomPin1 = this.Randomizer.Next(0, this.WheelPins[randomWheel1].Length - 1);
            this.WheelPins[randomWheel1][randomPin1] = !this.WheelPins[randomWheel1][randomPin1];

            int randomWheel2 = this.Randomizer.Next(0, 5);
            while (randomWheel2 == randomWheel1)
            {
                randomWheel2 = this.Randomizer.Next(0, 5);
            }

            int randomPin2 = this.Randomizer.Next(0, this.WheelPins[randomWheel2].Length - 1);
            while (this.WheelPins[randomWheel2][randomPin2] != this.WheelPins[randomWheel1][randomPin1])
            {
                randomPin2 = this.Randomizer.Next(0, this.WheelPins[randomWheel2].Length - 1);
            }
            this.WheelPins[randomWheel2][randomPin2] = !this.WheelPins[randomWheel2][randomPin2];

        }

        /// <summary>
        /// Toggle the state of all the pins of a wheel.
        /// </summary>
        public void ToggleSingleWheelAllPinsTransformation()
        {
            for (int i = 0; i < this.WheelPins.Length; i++)
            {
                for (int k = 0; k < this.WheelPins[i].Length; k++)
                {
                    this.WheelPins[i][k] = !this.WheelPins[i][k];
                }

            }

        }
    }
}
