/*
   Copyright 2019 CrypTool 2 Team <ct2contact@CrypTool.org>
   Author: Nils Kopal
   code adapted from: https://rosettacode.org/wiki/Subtractive_generator#C.23

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

using System.Numerics;

namespace CrypTool.Plugins.RandomNumberGenerator.RandomNumberGenerators
{
    internal class SubtractiveGenerator : RandomGenerator
    {
        private readonly int MAX = 1000000000;
        private int[] _State;
        private int _Position;
        private int _CurrentValue;

        public SubtractiveGenerator(BigInteger seed, int outputLength) : base()
        {
            Seed = seed;
            OutputLength = outputLength;
            Initialize();
        }

        /// <summary>
        /// Initializes using the seed
        /// </summary>
        private void Initialize()
        {
            _State = new int[55];
            int[] temp = new int[55];
            temp[0] = Mod((int)Seed);
            temp[1] = 1;
            for (int i = 2; i < 55; ++i)
            {
                temp[i] = Mod(temp[i - 2] - temp[i - 1]);
            }

            for (int i = 0; i < 55; ++i)
            {
                _State[i] = temp[(34 * (i + 1)) % 55];
            }
            _Position = 54;
            for (int i = 55; i < 220; ++i)
            {
                ComputeNextRandomNumber();
            }
        }

        public override void ComputeNextRandomNumber()
        {
            _CurrentValue = Mod(_State[(_Position + 1) % 55] - _State[(_Position + 32) % 55]);
            _Position = (_Position + 1) % 55;
            _State[_Position] = _CurrentValue;
            RandNo = _CurrentValue;
        }

        private int Mod(int n)
        {
            return ((n % MAX) + MAX) % MAX;
        }
    }
}
