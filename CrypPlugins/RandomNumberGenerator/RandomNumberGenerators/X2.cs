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
    /// <summary>
    /// X^2 Mod N randomumber generator
    /// </summary>
    internal class X2 : RandomGenerator
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="modul"></param>
        public X2(BigInteger seed, BigInteger modul, int outputLength) : base()
        {            
            B = 2; //B is fixed to 2
            Seed = seed;
            RandNo = seed;
            Modulus = modul;
            OutputLength = outputLength;
        }       

        /// <summary>
        /// randomize RandNo
        /// </summary>
        public override void ComputeNextRandomNumber()
        {
            BigInteger tmp = RandNo;
            RandNo = BigInteger.Pow(tmp, (int)B) % Modulus;
        }
    }
}
