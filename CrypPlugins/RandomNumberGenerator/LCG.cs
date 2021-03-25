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

namespace CrypTool.Plugins.RandomNumberGenerator
{
    class LCG : IrndNum
    {
        public LCG(BigInteger Seed, BigInteger Modul, BigInteger a, BigInteger b, BigInteger OutputLength) : base()
        {
            this.Seed = Seed;
            this.Modulus = Modul;
            this.A = a;
            this.B = b;
            this.OutputLength = OutputLength;
            //RandNo takes value of the seed
            this.RandNo = this.Seed;
        }

        /// <summary>
        /// generates the output
        /// </summary>
        /// <returns></returns>
        public override byte[] generateRNDNums()
        {
            byte[] res = new byte[(int)OutputLength];
            for (int j = 0; j < OutputLength; j++)
            {
                int curByte = 0;
                int tmp = 128;
                for (int i = 0; i < 8; i++)
                {
                    this.randomize();
                    if (randBit() != 0)
                    {
                        curByte += tmp;
                    }
                    tmp /= 2;
                }
                res[j] = Convert.ToByte(curByte);
            }
            return res;
        }

        /// <summary>
        /// returns next random bit
        /// </summary>
        /// <returns></returns>
        public override BigInteger randBit()
        {
            return (RandNo > Modulus / 2) ? 1 : 0;
        }

        /// <summary>
        /// randomize RandNo
        /// </summary>
        public override void randomize()
        {
            RandNo = (A * RandNo + B) % Modulus;
        }
    }
}
