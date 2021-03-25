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
    /// <summary>
    /// X^2 Mod N randomumber generator
    /// </summary>
    class X2 : IrndNum
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Seed"></param>
        /// <param name="Modul"></param>
        public X2(BigInteger Seed, BigInteger Modul, BigInteger OutputLength) : base()
        {
            //B is fixed to 2
            this.B = 2;
            this.Seed = Seed;
            this.Modulus = Modul;
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

            for (int i = 0; i < res.Length; i++)
            {
                int curByte = 0;
                int tmp = 128;
                for (int j = 0; j < 8; j++)
                {
                    this.randomize();
                    if (randBit() != 0)
                    {
                        curByte += tmp;
                    }
                    tmp /= 2;
                }
                res[i] = Convert.ToByte(curByte);
            }
            return res;
        }

        /// <summary>
        /// returns next random bit
        /// </summary>
        /// <returns></returns>
        public override BigInteger randBit()
        {
            return RandNo % 2;
        }

        /// <summary>
        /// randomize RandNo
        /// </summary>
        public override void randomize()
        {
            BigInteger tmp = RandNo;
            RandNo = BigInteger.Pow(tmp, (int)B) % Modulus;
        }
    }
}
