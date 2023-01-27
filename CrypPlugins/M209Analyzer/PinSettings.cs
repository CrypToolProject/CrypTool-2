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

        public void Randomize()
        {
            for (int i = 0; i < Wheels.Length; i++)
            {
                Wheels[i].RandomizeAllPinValues();
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
                count += wheel.CountOfEffectivePins();
            }
            return count;
        }

        public void InverseWheelBitmap(int v)
        {
            for (int w = 0; w < Wheels.Length; w++)
            {
                if(GetWheelBit(v, w))
                {
                    InverseWheel(w);
                }
            }
        }

        public bool GetWheelBit(int v, int w)
        {
            return ((v >> (w - 1)) & 0x1) == 0x1;
        }

        public void InverseWheel(int w)
        {
            Wheels[w].ToggleAllPinValues();
        }
    }
}
