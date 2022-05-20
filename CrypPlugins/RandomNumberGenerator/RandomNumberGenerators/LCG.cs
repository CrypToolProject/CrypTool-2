/*
   Copyright 2018 CrypTool 2 Team <ct2contact@CrypTool.org>
   Author: Christian Bender, Universität Siegen

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
    internal class LCG : RandomGenerator
    {
        public LCG(BigInteger seed, BigInteger modul, BigInteger a, BigInteger b, int outputLength) : base()
        {
            Seed = seed;
            RandNo = seed;
            Modulus = modul;
            A = a;
            B = b;
            OutputLength = outputLength;
        }
        
        /// <summary>
        /// randomize RandNo
        /// </summary>
        public override void ComputeNextRandomNumber()
        {
            RandNo = (A * RandNo + B) % Modulus;
        }
    }
}
