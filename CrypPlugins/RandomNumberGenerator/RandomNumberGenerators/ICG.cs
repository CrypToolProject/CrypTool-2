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
    internal class ICG : RandomGenerator
    {
        private int _count;

        /// <summary>
        /// Counter for calculation
        /// </summary>
        public int Count
        {
            set => _count = value;
            get => _count;
        }

        public ICG(BigInteger seed, BigInteger modulus, BigInteger a, BigInteger b, int outputLength) : base()
        {
            Seed = seed;
            RandNo = seed;
            Modulus = modulus;
            A = a;
            B = b;
            OutputLength = outputLength;
        }
     
        /// <summary>
        /// randomize RandNo
        /// </summary>
        public override void ComputeNextRandomNumber()
        {
            BigInteger tmp;
            if (Modulus < (A * (RandNo + Count) + B))
            {
                tmp = ((Ext_EA((A * (RandNo + Count) + B), Modulus)[1] + Modulus) % Modulus);
            }
            else
            {
                tmp = ((Ext_EA(Modulus, (A * (RandNo + Count) + B))[2] + Modulus) % Modulus);
            }
            RandNo = ((tmp + Modulus) % Modulus);
        }

        /// <summary>
        /// implementation of extended euclidean algorithm for solving a*x + b*y = gcd(a,b)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private BigInteger[] Ext_EA(BigInteger a, BigInteger b)
        {
            BigInteger[] result = new BigInteger[3];
            BigInteger tmp;

            //a must be > b
            if (a < b)
            {
                tmp = a;
                a = b;
                b = tmp;
            }

            BigInteger r = b;
            BigInteger q = 0;

            BigInteger prevx = 1;
            BigInteger prevy = 0;
            BigInteger x = 0;
            BigInteger y = 1;

            //calcs: gcd(a,b) = a*x + b*y
            while (r > 1)
            {
                r = a % b;
                q = a / b;

                tmp = x;
                x = prevx - q * x;
                prevx = tmp;

                tmp = y;
                y = prevy - q * y;
                prevy = tmp;

                a = b;
                b = r;
            }
            result[0] = r;
            result[1] = x;
            result[2] = y;

            return result;
        }
    }
}
