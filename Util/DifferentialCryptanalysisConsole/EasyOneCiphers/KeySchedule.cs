/*
   Copyright 2019 Nils Kopal, nils.kopal<at>cryptool.org

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
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EasyCipher
{
    public class KeySchedule
    {
        /// <summary>
        /// Creates 4 4bit round keys based on a 32bit key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int[] Create_4_4BitRoundKeys(int key)
        {
            //multiply key with big prime numbers
            key = (int)(((BigInteger)key * 212217287) % 2147483647);
            key = (int)(((BigInteger)key * 121586177) % 2147483647);

            //extract all bits
            var bit31 = 1 & (key >> 31);
            var bit30 = 1 & (key >> 30);
            var bit29 = 1 & (key >> 29);
            var bit28 = 1 & (key >> 28);
            var bit27 = 1 & (key >> 27);
            var bit26 = 1 & (key >> 26);
            var bit25 = 1 & (key >> 25);
            var bit24 = 1 & (key >> 24);
            var bit23 = 1 & (key >> 23);
            var bit22 = 1 & (key >> 22);
            var bit21 = 1 & (key >> 21);
            var bit20 = 1 & (key >> 20);
            var bit19 = 1 & (key >> 19);
            var bit18 = 1 & (key >> 18);
            var bit17 = 1 & (key >> 17);
            var bit16 = 1 & (key >> 16);
            var bit15 = 1 & (key >> 15);
            var bit14 = 1 & (key >> 14);
            var bit13 = 1 & (key >> 13);
            var bit12 = 1 & (key >> 12);
            var bit11 = 1 & (key >> 11);
            var bit10 = 1 & (key >> 10);
            var bit9 = 1 & (key >> 9);
            var bit8 = 1 & (key >> 8);
            var bit7 = 1 & (key >> 7);
            var bit6 = 1 & (key >> 6);
            var bit5 = 1 & (key >> 5);
            var bit4 = 1 & (key >> 4);
            var bit3 = 1 & (key >> 3);
            var bit2 = 1 & (key >> 2);
            var bit1 = 1 & (key >> 1);
            var bit0 = 1 & (key >> 0);

            //create keys
            var keys = new int [4];

            keys[2] = bit0 << 3 | bit4 << 2 | bit8 << 1 | bit12 << 0;
            keys[1] = bit1 << 3 | bit5 << 2 | bit9 << 1 | bit13 << 0;
            keys[3] = bit2 << 3 | bit6 << 2 | bit10 << 1 | bit14 << 0;
            keys[0] = bit3 << 3 | bit7 << 2 | bit11 << 1 | bit15 << 0;

            keys[3] = keys[0] ^ (bit16 << 3 | bit20 << 2 | bit24 << 1 | bit28 << 0);
            keys[2] = keys[1] ^ (bit17 << 3 | bit21 << 2 | bit25 << 1 | bit29 << 0);
            keys[0] = keys[2] ^ (bit18 << 3 | bit22 << 2 | bit26 << 1 | bit30 << 0);
            keys[1] = keys[3] ^ (bit19 << 3 | bit23 << 2 | bit27 << 1 | bit31 << 0);

            return keys;
        }

        /// <summary>
        /// Creates 4 8bit round keys based on a 32bit key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int[] Create_4_8_BitRoundKeys(int key)
        {
            //multiply key with big prime numbers
            key = (int)(((BigInteger)key * 212217287) % 2147483647);
            key = (int)(((BigInteger)key * 121586177) % 2147483647);

            //extract all bits
            var bit31 = 1 & (key >> 31);
            var bit30 = 1 & (key >> 30);
            var bit29 = 1 & (key >> 29);
            var bit28 = 1 & (key >> 28);
            var bit27 = 1 & (key >> 27);
            var bit26 = 1 & (key >> 26);
            var bit25 = 1 & (key >> 25);
            var bit24 = 1 & (key >> 24);
            var bit23 = 1 & (key >> 23);
            var bit22 = 1 & (key >> 22);
            var bit21 = 1 & (key >> 21);
            var bit20 = 1 & (key >> 20);
            var bit19 = 1 & (key >> 19);
            var bit18 = 1 & (key >> 18);
            var bit17 = 1 & (key >> 17);
            var bit16 = 1 & (key >> 16);
            var bit15 = 1 & (key >> 15);
            var bit14 = 1 & (key >> 14);
            var bit13 = 1 & (key >> 13);
            var bit12 = 1 & (key >> 12);
            var bit11 = 1 & (key >> 11);
            var bit10 = 1 & (key >> 10);
            var bit9 = 1 & (key >> 9);
            var bit8 = 1 & (key >> 8);
            var bit7 = 1 & (key >> 7);
            var bit6 = 1 & (key >> 6);
            var bit5 = 1 & (key >> 5);
            var bit4 = 1 & (key >> 4);
            var bit3 = 1 & (key >> 3);
            var bit2 = 1 & (key >> 2);
            var bit1 = 1 & (key >> 1);
            var bit0 = 1 & (key >> 0);

            //create keys
            var keys = new int[4];

            keys[2] = bit0 << 3 | bit4 << 2 | bit8 << 1 | bit12 << 0 | keys[0] ^ (bit16 << 6 | bit20 << 5 | bit24 << 4 | bit28 << 7);
            keys[1] = bit1 << 3 | bit5 << 2 | bit9 << 1 | bit13 << 0 | keys[2] ^ (bit18 << 6 | bit22 << 5 | bit26 << 4 | bit30 << 7);
            keys[3] = bit2 << 3 | bit6 << 2 | bit10 << 1 | bit14 << 0 | keys[1] ^ (bit17 << 6 | bit21 << 5 | bit25 << 4 | bit29 << 7);
            keys[0] = bit3 << 3 | bit7 << 2 | bit11 << 1 | bit15 << 0 | keys[3] ^ (bit19 << 6 | bit23 << 5 | bit27 << 4 | bit31 << 7);

            return keys;
        }

        /// <summary>
        /// Creates 4 16bit round keys based on a 64bit key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int[] Create_4_16_BitRoundKeys(Int64 key)
        {
            //multiply key with big prime numbers
            key = (Int64)(((BigInteger)key * 785894450710714057) % 9223372036854775807);
            key = (Int64)(((BigInteger)key * 191373772482246931) % 9223372036854775807);

            //extract all bits
            var bit63 = 1 & (key >> 63);
            var bit62 = 1 & (key >> 62);
            var bit61 = 1 & (key >> 61);
            var bit60 = 1 & (key >> 60);
            var bit59 = 1 & (key >> 59);
            var bit58 = 1 & (key >> 58);
            var bit57 = 1 & (key >> 57);
            var bit56 = 1 & (key >> 56);
            var bit55 = 1 & (key >> 55);
            var bit54 = 1 & (key >> 54);
            var bit53 = 1 & (key >> 53);
            var bit52 = 1 & (key >> 52);
            var bit51 = 1 & (key >> 51);
            var bit50 = 1 & (key >> 50);
            var bit49 = 1 & (key >> 49);
            var bit48 = 1 & (key >> 48);
            var bit47 = 1 & (key >> 47);
            var bit46 = 1 & (key >> 47);
            var bit45 = 1 & (key >> 45);
            var bit44 = 1 & (key >> 44);
            var bit43 = 1 & (key >> 43);
            var bit42 = 1 & (key >> 42);
            var bit41 = 1 & (key >> 41);
            var bit40 = 1 & (key >> 40);
            var bit39 = 1 & (key >> 39);
            var bit38 = 1 & (key >> 38);
            var bit37 = 1 & (key >> 37);
            var bit36 = 1 & (key >> 36);
            var bit35 = 1 & (key >> 35);
            var bit34 = 1 & (key >> 34);
            var bit33 = 1 & (key >> 33);
            var bit32 = 1 & (key >> 32);
            var bit31 = 1 & (key >> 31);
            var bit30 = 1 & (key >> 30);
            var bit29 = 1 & (key >> 29);
            var bit28 = 1 & (key >> 28);
            var bit27 = 1 & (key >> 27);
            var bit26 = 1 & (key >> 26);
            var bit25 = 1 & (key >> 25);
            var bit24 = 1 & (key >> 24);
            var bit23 = 1 & (key >> 23);
            var bit22 = 1 & (key >> 22);
            var bit21 = 1 & (key >> 21);
            var bit20 = 1 & (key >> 20);
            var bit19 = 1 & (key >> 19);
            var bit18 = 1 & (key >> 18);
            var bit17 = 1 & (key >> 17);
            var bit16 = 1 & (key >> 16);
            var bit15 = 1 & (key >> 15);
            var bit14 = 1 & (key >> 14);
            var bit13 = 1 & (key >> 13);
            var bit12 = 1 & (key >> 12);
            var bit11 = 1 & (key >> 11);
            var bit10 = 1 & (key >> 10);
            var bit9 = 1 & (key >> 9);
            var bit8 = 1 & (key >> 8);
            var bit7 = 1 & (key >> 7);
            var bit6 = 1 & (key >> 6);
            var bit5 = 1 & (key >> 5);
            var bit4 = 1 & (key >> 4);
            var bit3 = 1 & (key >> 3);
            var bit2 = 1 & (key >> 2);
            var bit1 = 1 & (key >> 1);
            var bit0 = 1 & (key >> 0);

            //create keys
            var keys = new int[4];

            keys[2] = (int)(bit0 << 3 | bit4 << 2 | bit8 << 1 | bit12 << 0 | keys[0] ^ (bit16 << 6 | bit20 << 5 | bit24 << 4 | bit28 << 7));
            keys[1] = (int)(bit1 << 3 | bit5 << 2 | bit9 << 1 | bit13 << 0 | keys[2] ^ (bit17 << 6 | bit21 << 5 | bit25 << 4 | bit29 << 7));
            keys[3] = (int)(bit2 << 3 | bit6 << 2 | bit10 << 1 | bit14 << 0 | keys[1] ^ (bit18 << 6 | bit22 << 5 | bit26 << 4 | bit30 << 7));
            keys[0] = (int)(bit3 << 3 | bit7 << 2 | bit11 << 1 | bit15 << 0 | keys[3] ^ (bit19 << 6 | bit23 << 5 | bit27 << 4 | bit31 << 7));

            keys[3] |= (int)(bit32 << 9 | bit36 << 8 | bit40 << 12 | bit44 << 10 | keys[0] ^ (bit48 << 11 | bit52 << 13 | bit56 << 15 | bit60 << 14));
            keys[0] |= (int)(bit33 << 9 | bit37 << 8 | bit41 << 12 | bit45 << 10 | keys[2] ^ (bit49 << 11 | bit53 << 13 | bit57 << 15 | bit61 << 14));
            keys[2] |= (int)(bit34 << 9 | bit38 << 8 | bit42 << 12 | bit46 << 10 | keys[1] ^ (bit50 << 11 | bit54 << 13 | bit58 << 15 | bit62 << 14));
            keys[1] |= (int)(bit35 << 9 | bit39 << 8 | bit43 << 12 | bit47 << 10 | keys[3] ^ (bit51 << 11 | bit55 << 13 | bit59 << 15 | bit63 << 14));

            return keys;
        }
    }
}
