using CrypTool.M209Analyzer.Enum;
using System;
using System.Runtime.InteropServices;

namespace M209Analyzer
{
    internal class PinSettings
    {
        public static int MIN_PERCENT_ACTIVE_PINS;
        public static int MAX_PERCENT_ACTIVE_PINS;

        public PinSettings(string versionOfInstruction)
        {
            // TODO: this values depends on the versionOfInstruction
            MAX_PERCENT_ACTIVE_PINS = 70;
            MIN_PERCENT_ACTIVE_PINS = 30;
        }

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

        public PinSettings[] GetNeighborPins()
        {
            PinSettings[] pins = new PinSettings[] { 
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
        public void ToggleSingleWheelSinglePinTransformation(int wheel = -1)
        {
            if(wheel == -1)
            {
                wheel = this.Randomizer.Next(0, 5);
            }

            int randomPin = this.Wheels[wheel].GetRandomPinPosition();
            this.Wheels[wheel].TogglePinValue(randomPin);
        }

        /// <summary>
        /// Toggle the state of a pair of pins, from the same wheel, which have different states (one is 0
        /// and the other is 1, or vice versa).
        /// </summary>
        public void ToggleSingleWheelTwoPinsTransformation(int wheel = -1)
        {
            if(wheel == -1)
            {
                wheel = this.Randomizer.Next(0, 5);
            }
            int randomPin1 = this.Wheels[wheel].GetRandomPinPosition();
            this.Wheels[wheel].TogglePinValue(randomPin1);

            int randomPin2 = 0;
            // Maybe it could be better to run systematically throug the pins...
            //while (this.Wheels[randomWheel].EvaluatePinAtPosition(randomPin1) != this.Wheels[randomWheel].EvaluatePinAtPosition(randomPin2))
            //{
            //    randomPin2 = this.Wheels[randomWheel].GetRandomPinPosition();
            //}
            for (int i = 0; i < this.Wheels[wheel].Length; i++)
            {
                if (this.Wheels[wheel].EvaluatePinAtPosition(randomPin1) != this.Wheels[wheel].EvaluatePinAtPosition(randomPin2))
                {
                    randomPin2 = i;
                    break;
                }
            }
            this.Wheels[wheel].TogglePinValue(randomPin2);
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


        public void SetPinValueOfWheel(int wheel, int pin, bool value)
        {
            this.Wheels[wheel].SetPinValue(pin, value);
        }

        public int maxCount()
        {
            int sum = 0;
            for (int i = 0; i < Wheels.Length; i++)
            {
                sum += Wheels[i].Length;
            }
            return sum * MAX_PERCENT_ACTIVE_PINS / 100;
        }

        public int minCount()
        {
            int sum = 0;
            for (int i = 0; i < Wheels.Length; i++)
            {
                sum += Wheels[i].Length;
            }
            return sum * MIN_PERCENT_ACTIVE_PINS / 100;
        }

        public int count()
        {
            int count = 0;
            foreach (Wheel wheel in this.Wheels)
            {
                count += wheel.CountOfPositivePins();
            }
            return count;
        }
    }
}
