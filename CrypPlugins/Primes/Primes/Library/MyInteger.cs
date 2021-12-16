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

namespace Primes.Library
{
    public class MyInteger
    {
        private readonly long m_Value;

        public MyInteger(long value)
        {
            m_Value = value;
            m_BitCount = -1;
        }

        private int m_BitCount;

        public int BitCount
        {
            get
            {
                if (m_BitCount == -1)
                {
                    m_BitCount = GetBitCount(m_Value);
                }
                return m_BitCount;
            }
        }

        public static int GetBitCount(long n)
        {
            int result = 0;
            long tmp = n;
            for (int i = 0; i < 64; i++)
            {
                if ((tmp & 1) == 1)
                {
                    result++;
                }

                tmp = tmp >> 1;
            }
            return result;
        }

        public bool TestBit(int bit)
        {
            long tmp = m_Value >> bit;
            return (tmp & 1) == 1;
        }

        public int[] GetIndices()
        {
            int[] indices = new int[BitCount];
            long n = m_Value;

            for (int i = 0, j = 0; n != 0; i++, n >>= 1)
            {
                if ((n & 1) == 1)
                {
                    indices[j++] = i;
                }
            }

            return indices;
        }
    }
}
