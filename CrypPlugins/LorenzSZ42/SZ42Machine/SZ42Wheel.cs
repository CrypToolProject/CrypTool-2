/*
   Copyright 2023 Nils Kopal <Nils.Kopal<at>CrypTool.org

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

namespace CrypTool.LorenzSZ42.SZ42Machine
{
    /// <summary>
    /// Implementation of a wheel of the SZ42
    /// </summary>
    public class SZ42Wheel
    {
        // we use the notation ...x...x...xx.. for the definition of the pins                
        public const char ACTIVE_PIN = 'x';
        public const char INACTIVE_PIN = '.';

        /// <summary>
        /// Pin defintions
        /// </summary>
        public char[] Pins { get; }

        /// <summary>
        /// Name of the wheel
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Current position of the wheel
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Creates a wheel with pinCount pins
        /// </summary>
        /// <param name="pinCount"></param>
        /// <param name="name"></param>
        public SZ42Wheel(int pinCount, string name, int position = 0)
        {
            Pins = new char[pinCount];
            Name = name;
            Position = position;

            for (int i = 0; i < pinCount; i++)
            {
                Pins[i] = INACTIVE_PIN;
            }
        }

        /// <summary>
        /// Increments the position (= steps 1)
        /// </summary>
        public void Step()
        {
            Position = Mod(Position + 1, Pins.Length);
        }

        /// <summary>
        /// Is the current pin active?
        /// </summary>
        /// <returns></returns>
        public byte PinActive()
        {
            return (byte)(Pins[Position] == ACTIVE_PIN ? 0b1 : 0b0);
        }

        /// <summary>
        /// Gets/sets the one back pin
        /// </summary>
        /// <returns></returns>
        public byte OneBack { get; set; }

        /// <summary>
        /// Returns the number of crosses
        /// </summary>
        /// <returns></returns>
        public int GetCrossCount()
        {
            int count = 0;
            foreach (char pin in Pins)
            {
                if (pin == ACTIVE_PIN)
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Returns the number of dots
        /// </summary>
        /// <returns></returns>
        public int GetDotCount()
        {
            int count = 0;
            foreach (char pin in Pins)
            {
                if (pin == INACTIVE_PIN)
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Mathematical modulo operator
        /// </summary>
        /// <param name="number"></param>
        /// <param name="mod"></param>
        /// <returns></returns>
        private int Mod(int number, int mod)
        {
            return (number % mod + mod) % mod;
        }
    }
}
