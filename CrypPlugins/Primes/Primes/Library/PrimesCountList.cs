/*
   Copyright 2008 Timo Eckhardt, University of Siegen

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
using System.IO;

namespace Primes.Library
{
    public static class PrimesCountList
    {
        private static readonly string m_Filename;
        private static long[] primes;

        static PrimesCountList()
        {
            m_Filename = "primes.txt";
        }

        public static bool LoadPrimes()
        {
            if (File.Exists(m_Filename))
            {
                string[] finput = File.ReadAllLines(m_Filename);
                primes = new long[finput.LongLength];

                foreach (string s in finput)
                {
                    int indexEqual = s.IndexOf("=");
                    long n = long.Parse(s.Substring(0, indexEqual));
                    long val = long.Parse(s.Substring(indexEqual + 1, s.Length - indexEqual - 1));
                    primes[n] = val;
                }
            }

            return true;
        }

        public static bool Initialzed => primes != null && primes.Length > 0;

        public static long GetPrime(long index)
        {
            if (!Initialzed)
            {
                throw new IndexOutOfRangeException();
            }

            if (index >= primes.Length)
            {
                return 0;
            }

            return primes[index];
        }

        public static long MaxNumber => primes.LongLength - 1;
    }
}
