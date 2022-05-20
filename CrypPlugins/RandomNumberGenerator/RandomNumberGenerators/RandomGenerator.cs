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


using System;
using System.Numerics;

namespace CrypTool.Plugins.RandomNumberGenerator.RandomNumberGenerators
{

    /// <summary>
    /// abstract class for randomnumber generators
    /// </summary>
    internal abstract class RandomGenerator
    {
        private BigInteger _seed;
        private BigInteger _modulus;
        private BigInteger _randNo;
        private BigInteger _a;
        private BigInteger _b;
        private int _outputLength;

        public BigInteger Seed
        {
            set => _seed = value;
            get => _seed;
        }

        public BigInteger Modulus
        {
            set => _modulus = value;
            get => _modulus;
        }

        public BigInteger RandNo
        {
            set => _randNo = value;
            get => _randNo;
        }

        public BigInteger A
        {
            set => _a = value;
            get => _a;
        }

        public BigInteger B
        {
            set => _b = value;
            get => _b;
        }

        public int OutputLength
        {
            get => _outputLength;
            set => _outputLength = value;
        }       
        
        /// <summary>
        /// Generates a random byte array
        /// </summary>
        /// <returns></returns>
        public virtual byte[] GenerateRandomByteArray()
        {
            byte[] result = new byte[OutputLength];
            int resultoffset = 0;
            while (resultoffset < result.Length)
            {
                ComputeNextRandomNumber();
                byte[] array = RandNo.ToByteArray();
                for (int arrayoffset = 0; arrayoffset < array.Length; arrayoffset++)
                {
                    if (resultoffset + arrayoffset == result.Length)
                    {
                        break;
                    }
                    result[resultoffset + arrayoffset] = array[arrayoffset];
                }
                resultoffset = resultoffset + array.Length;
            }
            return result;
        }

        /// <summary>
        /// Returns a random bool by generating a random byte array and testing the least significant bit
        /// </summary>
        /// <returns></returns>
        public bool GenerateRandomBit()
        {
            return (GenerateRandomByteArray()[0] & 1) == 1;           
        }

        /// <summary>
        /// Has to be implemented by each random number generator
        /// After execution, RandNo has to contain the new random number
        /// </summary>
        public abstract void ComputeNextRandomNumber();
    }
}
