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
using System.Text;
using System.Threading.Tasks;

namespace EasyCipherOne
{
    public class EasyCipherOne
    {
        #region sbox definitions
                                                      // 00, 01, 02, 03, 04, 05, 06, 07, 08, 09, 10, 11, 12, 13, 14, 15
        public static readonly int[] SBox4 = new int[] { 10, 05, 15, 08, 11, 00, 03, 07, 01, 13, 09, 12, 06, 14, 02, 04 };
        public static readonly int[] SBox4r = new int[]{ 05, 08, 14, 06, 15, 01, 12, 07, 03, 10, 00, 04, 11, 09, 13, 02 };
        #endregion

        #region 4bit roundkeys 4bit block cipher

        /// <summary>
        /// Encrypts a plaintext using roundkeys
        /// 4bit roundkeys 4bit block cipher
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="roundkeys"></param>
        /// <returns></returns>
        public static int Encrypt4Bit4(int plaintext, int[] roundkeys)
        {
            plaintext = (plaintext & 15);   // we only use 4bit, thus, we remove the others
            var ciphertext = plaintext;

            for(var round=0;round < roundkeys.Length - 1; round++)
            {
                ciphertext = EncryptSingleRound4Bit4(ciphertext, roundkeys[round], round == roundkeys.Length - 2);
            }

            var lastkey = (roundkeys[roundkeys.Length - 1] & 15); //we only use 4 bits of roundkey
            ciphertext = (ciphertext ^ lastkey);

            return ciphertext;
        }

        /// <summary>
        /// Decrypts a ciphertext using roundkeys
        /// 4bit roundkeys 4bit block cipher
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="roundkeys"></param>
        /// <returns></returns>
        public static int Decrypt4Bit4(int ciphertext, int[] roundkeys)
        {
            ciphertext = (ciphertext & 15);   // we only use 4bit, thus, we remove the others
            var plaintext = ciphertext;

            var lastkey = (roundkeys[roundkeys.Length - 1] & 15); //we only use 4 bits of roundkey
            plaintext = (plaintext ^ lastkey);

            for (var round = roundkeys.Length - 2; round >= 0; round--)
            {
                plaintext = DecryptSingleRound4Bit4(plaintext, roundkeys[round], round == roundkeys.Length -2);
            }
            return plaintext;
        }

        /// <summary>
        /// Encrypts a single round using a single round key
        /// 4bit roundkeys 4bit block cipher
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="roundkey"></param>
        /// <param name="lastRound"></param>
        /// <returns></returns>
        private static int EncryptSingleRound4Bit4(int plaintext, int roundkey, bool lastRound = false)
        {
            //step 1: XOR roundkey
            roundkey = (roundkey & 15); //we only use 4 bits of roundkey
            var ciphertext = (plaintext ^ roundkey);

            //step 2: do substitution with sbox
            ciphertext = SBox4[ciphertext];

            //step 3: do permutation
            if (!lastRound)
            {
                var bit3 = 1 & (ciphertext >> 3);
                var bit2 = 1 & (ciphertext >> 2);
                var bit1 = 1 & (ciphertext >> 1);
                var bit0 = 1 & (ciphertext >> 0);
                ciphertext =  ((bit0 << 3) | 
                               (bit1 << 0) | 
                               (bit2 << 2) | 
                               (bit3 << 1));
            }
            return ciphertext;
        }

        /// <summary>
        /// Decrypts a single round using a single round key
        /// 4bit roundkeys 4bit block cipher
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="roundkey"></param>
        /// <param name="lastRound"></param>
        /// <returns></returns>
        private static int DecryptSingleRound4Bit4(int ciphertext, int roundkey, bool lastRound = false)
        {
            int plaintext = ciphertext;

            //step 1: reverse permutation
            if (!lastRound)
            {
                var bit3 = 1 & (plaintext >> 3);
                var bit2 = 1 & (plaintext >> 2);
                var bit1 = 1 & (plaintext >> 1);
                var bit0 = 1 & (plaintext >> 0);
                plaintext = ((bit0 << 1) | 
                             (bit1 << 3) | 
                             (bit2 << 2) | 
                             (bit3 << 0));
            }
            
            //step 3: reverse substitution with reversed sbox
            plaintext = SBox4r[plaintext];

            //step 1: XOR roundkey
            roundkey = (roundkey & 15); //we only use 4 bits of roundkey
            plaintext = (plaintext ^ roundkey);

            return plaintext;
        }

        #endregion

        #region 4bit roundkeys 8bit block cipher

        /// <summary>
        /// Encrypts a plaintext using roundkeys
        /// 4bit roundkeys 8bit block cipher
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="roundkeys"></param>
        /// <returns></returns>
        public static int Encrypt4Bit8(int plaintext, int[] roundkeys)
        {
            var ciphertext = plaintext;
            for (var round = 0; round < roundkeys.Length - 1; round++)
            {
                ciphertext = EncryptSingleRound4Bit8(ciphertext, roundkeys[round], round == roundkeys.Length - 2);
            }
            var lastkey = (roundkeys[roundkeys.Length - 1 ] & 15); //we only use 4 bits of roundkey
            var doubleroundkey = (lastkey << 4 | lastkey);
            ciphertext = (ciphertext ^ doubleroundkey);
            return ciphertext;
        }

        /// <summary>
        /// Decrypts a ciphertext using roundkeys
        /// 4bit roundkeys 8bit block cipher
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="roundkeys"></param>
        /// <returns></returns>
        public static int Decrypt4Bit8(int ciphertext, int[] roundkeys)
        {
            var plaintext = ciphertext;

            var lastkey = (roundkeys[roundkeys.Length - 1] & 15); //we only use 4 bits of roundkey
            var doubleroundkey = (lastkey << 4 | lastkey);
            plaintext = (plaintext ^ doubleroundkey);

            for (var round = roundkeys.Length - 2; round >= 0; round--)
            {
                plaintext = DecryptSingleRound4Bit8(plaintext, roundkeys[round], round == roundkeys.Length - 2);
            }
            return plaintext;
        }

        /// <summary>
        /// Encrypts a single round using a single round key
        /// 4bit roundkeys 8bit block cipher
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="roundkey"></param>
        /// <param name="lastRound"></param>
        /// <returns></returns>
        private static int EncryptSingleRound4Bit8(int plaintext, int roundkey, bool lastRound = false)
        {
            //step 1: XOR roundkey
            roundkey = (roundkey & 15); //we only use 4 bits of roundkey
            var doubleroundkey = (roundkey << 4 | roundkey);
            var ciphertext = (plaintext ^ doubleroundkey);

            //step 2: do substitution with 2 sboxes in parallel
            ciphertext = (SBox4[ciphertext >> 4] << 4 | SBox4[ciphertext & 15]);

            //step 3: do permutation
            if (!lastRound)
            {
                var bit7 = 1 & (ciphertext >> 7);
                var bit6 = 1 & (ciphertext >> 6);
                var bit5 = 1 & (ciphertext >> 5);
                var bit4 = 1 & (ciphertext >> 4);
                var bit3 = 1 & (ciphertext >> 3);
                var bit2 = 1 & (ciphertext >> 2);
                var bit1 = 1 & (ciphertext >> 1);
                var bit0 = 1 & (ciphertext >> 0);

                ciphertext = ((bit0 << 4) | 
                              (bit1 << 6) | 
                              (bit2 << 7) | 
                              (bit3 << 5) | 
                              (bit4 << 2) | 
                              (bit5 << 1) | 
                              (bit6 << 0) | 
                              (bit7 << 3));
            }
            return ciphertext;
        }

        /// <summary>
        /// Decrypts a single round using a single round key
        /// 4bit roundkeys 8bit block cipher
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="roundkey"></param>
        /// <param name="lastRound"></param>
        /// <returns></returns>
        private static int DecryptSingleRound4Bit8(int ciphertext, int roundkey, bool lastRound = false)
        {
            var plaintext = ciphertext;

            //step 1: reverse permutation
            if (!lastRound)
            {
                var bit7 = 1 & (plaintext >> 7);
                var bit6 = 1 & (plaintext >> 6);
                var bit5 = 1 & (plaintext >> 5);
                var bit4 = 1 & (plaintext >> 4);
                var bit3 = 1 & (plaintext >> 3);
                var bit2 = 1 & (plaintext >> 2);
                var bit1 = 1 & (plaintext >> 1);
                var bit0 = 1 & (plaintext >> 0);
                plaintext = ((bit0 << 6) |
                             (bit1 << 5) |
                             (bit2 << 4) |
                             (bit3 << 7) |
                             (bit4 << 0) |
                             (bit5 << 3) |
                             (bit6 << 1) |
                             (bit7 << 2));
            }

            //step 2: reverse substitution with reversed sbox
            plaintext = (SBox4r[plaintext >> 4] << 4 | SBox4r[plaintext & 15]);

            //step 3: XOR roundkey
            roundkey = (roundkey & 15); //we only use 4 bits of roundkey
            var doubleroundkey = (roundkey << 4 | roundkey);
            plaintext = (plaintext ^ doubleroundkey);

            return plaintext;
        }

        #endregion

        #region 8bit roundkeys 8bit block cipher

        /// <summary>
        /// Encrypts a plaintext using roundkeys
        /// 8bit roundkeys 8bit block cipher
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="roundkeys"></param>
        /// <returns></returns>
        public static int Encrypt8Bit8(int plaintext, int[] roundkeys)
        {
            var ciphertext = plaintext;

            for (var round = 0; round < roundkeys.Length - 1; round++)
            {
                ciphertext = EncryptSingleRound8Bit8(ciphertext, roundkeys[round], round == roundkeys.Length - 2);
            }

            var lastkey = roundkeys[roundkeys.Length - 1];
            ciphertext = (ciphertext ^ lastkey);

            return ciphertext;
        }

        /// <summary>
        /// Decrypts a ciphertext using roundkeys
        /// 8bit roundkeys 8bit block cipher
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="roundkeys"></param>
        /// <returns></returns>
        public static int Decrypt8Bit8(int ciphertext, int[] roundkeys)
        {
            var plaintext = ciphertext;

            var lastkey = roundkeys[roundkeys.Length - 1];
            plaintext = (plaintext ^ lastkey);

            for (var round = roundkeys.Length - 2; round >= 0; round--)
            {
                plaintext = DecryptSingleRound8Bit8(plaintext, roundkeys[round], round == roundkeys.Length - 2);
            }
            return plaintext;
        }

        /// <summary>
        /// Encrypts a single round using a single round key
        /// 8bit roundkeys 8bit block cipher
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="roundkey"></param>
        /// <param name="lastRound"></param>
        /// <returns></returns>
        private static int EncryptSingleRound8Bit8(int plaintext, int roundkey, bool lastRound = false)
        {
            //step 1: XOR roundkey
            var ciphertext = (plaintext ^ roundkey);

            //step 2: do substitution with 2 sboxes in parallel
            ciphertext = (SBox4[ciphertext >> 4] << 4 | SBox4[ciphertext & 15]);

            //step 3: do permutation
            if (!lastRound)
            {
                var bit7 = 1 & (ciphertext >> 7);
                var bit6 = 1 & (ciphertext >> 6);
                var bit5 = 1 & (ciphertext >> 5);
                var bit4 = 1 & (ciphertext >> 4);
                var bit3 = 1 & (ciphertext >> 3);
                var bit2 = 1 & (ciphertext >> 2);
                var bit1 = 1 & (ciphertext >> 1);
                var bit0 = 1 & (ciphertext >> 0);

                ciphertext = ((bit0 << 4) |
                              (bit1 << 2) |
                              (bit2 << 6) |
                              (bit3 << 3) |
                              (bit4 << 7) |
                              (bit5 << 1) |
                              (bit6 << 0) |
                              (bit7 << 5));
            }

            return ciphertext;
        }

        /// <summary>
        /// Decrypts a single round using a single round key
        /// 8bit roundkeys 8bit block cipher
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="roundkey"></param>
        /// <param name="lastRound"></param>
        /// <returns></returns>
        private static int DecryptSingleRound8Bit8(int ciphertext, int roundkey, bool lastRound = false)
        {
            var plaintext = ciphertext;
            //step 1: reverse permutation
            if (!lastRound)
            {
                var bit7 = 1 & (plaintext >> 7);
                var bit6 = 1 & (plaintext >> 6);
                var bit5 = 1 & (plaintext >> 5);
                var bit4 = 1 & (plaintext >> 4);
                var bit3 = 1 & (plaintext >> 3);
                var bit2 = 1 & (plaintext >> 2);
                var bit1 = 1 & (plaintext >> 1);
                var bit0 = 1 & (plaintext >> 0);
                plaintext = ((bit0 << 6) |
                             (bit1 << 5) |
                             (bit2 << 1) |
                             (bit3 << 3) |
                             (bit4 << 0) |
                             (bit5 << 7) |
                             (bit6 << 2) |
                             (bit7 << 4));
            }

            //step 3: reverse substitution with reversed sbox
            plaintext = (SBox4r[plaintext >> 4] << 4 | SBox4r[plaintext & 15]);

            //step 1: XOR roundkey
            plaintext = (plaintext ^ roundkey);
            
            return plaintext;
        }

        #endregion

        #region 4bit roundkeys 16bit block cipher

        /// <summary>
        /// Encrypts a plaintext using roundkeys
        /// 4bit roundkeys 16bit block cipher
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="roundkeys"></param>
        /// <returns></returns>
        public static int Encrypt4Bit16(int plaintext, int[] roundkeys)
        {
            var ciphertext = plaintext;

            for (var round = 0; round < roundkeys.Length -1; round++)
            {
                ciphertext = EncryptSingleRound4Bit16(ciphertext, roundkeys[round], round == roundkeys.Length - 2);
            }

            var quadruplekey = ((15 & roundkeys[roundkeys.Length - 1]) << 12 | 
                                (15 & roundkeys[roundkeys.Length - 1]) << 8 |
                                (15 & roundkeys[roundkeys.Length - 1]) << 4 | 
                                (15 & roundkeys[roundkeys.Length - 1]));

            ciphertext = (ciphertext ^ quadruplekey);

            return ciphertext;
        }

        /// <summary>
        /// Decrypts a ciphertext using roundkeys
        /// 4bit roundkeys 16bit block cipher
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="roundkeys"></param>
        /// <returns></returns>
        public static int Decrypt4Bit16(int ciphertext, int[] roundkeys)
        {
            var plaintext = ciphertext;

            var quadruplekey = ((15 & roundkeys[roundkeys.Length - 1]) << 12 |
                                (15 & roundkeys[roundkeys.Length - 1]) << 8 |
                                (15 & roundkeys[roundkeys.Length - 1]) << 4 |
                                (15 & roundkeys[roundkeys.Length - 1]));

            plaintext = (plaintext ^ quadruplekey);

            for (var round = roundkeys.Length - 2; round >= 0; round--)
            {
                plaintext = DecryptSingleRound4Bit16(plaintext, roundkeys[round], round == roundkeys.Length - 2);
            }
            return plaintext;
        }

        /// <summary>
        /// Encrypts a single round using a single round key
        /// 4bit roundkeys 16bit block cipher
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="roundkey"></param>
        /// <param name="lastRound"></param>
        /// <returns></returns>
        private static int EncryptSingleRound4Bit16(int plaintext, int roundkey, bool lastRound = false)
        {
            //step 1: XOR roundkey
            var quadruplekey = ((15 & roundkey) << 12 |
                                (15 & roundkey) << 8 |
                                (15 & roundkey) << 4 |
                                (15 & roundkey));

            var ciphertext = (plaintext ^ quadruplekey);

            //step 1: do substitution with 4 sboxes in parallel
            ciphertext = (SBox4[15 & (ciphertext >> 12)] << 12 |
                          SBox4[15 & (ciphertext >> 8)] << 8 |
                          SBox4[15 & (ciphertext >> 4)] << 4 |
                          SBox4[ciphertext & 15]);

            //step 2: do permutation
            if (!lastRound)
            {
                var bit15 = 1 & (ciphertext >> 15);
                var bit14 = 1 & (ciphertext >> 14);
                var bit13 = 1 & (ciphertext >> 13);
                var bit12 = 1 & (ciphertext >> 12);
                var bit11 = 1 & (ciphertext >> 11);
                var bit10 = 1 & (ciphertext >> 10);
                var bit9 = 1 & (ciphertext >> 9);
                var bit8 = 1 & (ciphertext >> 8);
                var bit7 = 1 & (ciphertext >> 7);
                var bit6 = 1 & (ciphertext >> 6);
                var bit5 = 1 & (ciphertext >> 5);
                var bit4 = 1 & (ciphertext >> 4);
                var bit3 = 1 & (ciphertext >> 3);
                var bit2 = 1 & (ciphertext >> 2);
                var bit1 = 1 & (ciphertext >> 1);
                var bit0 = 1 & (ciphertext >> 0);

                ciphertext = ((bit0 << 6) |
                              (bit1 << 8) |
                              (bit2 << 1) |
                              (bit3 << 9) |
                              (bit4 << 3) |
                              (bit5 << 4) |
                              (bit6 << 0) |
                              (bit7 << 7)|
                              (bit8 << 15) |
                              (bit9 << 12) |
                              (bit10 << 5) |
                              (bit11 << 14) |
                              (bit12 << 10) |
                              (bit13 << 13) |
                              (bit14 << 2) |
                              (bit15 << 11));
            }
            return ciphertext;
        }

        /// <summary>
        /// Decrypts a single round using a single round key
        /// 4bit roundkeys 16bit block cipher
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="roundkey"></param>
        /// <param name="lastRound"></param>
        /// <returns></returns>
        private static int DecryptSingleRound4Bit16(int ciphertext, int roundkey, bool lastRound = false)
        {
            int plaintext = ciphertext;

            //step 1: reverse permutation
            if (!lastRound)
            {
                var bit15 = 1 & (ciphertext >> 15);
                var bit14 = 1 & (ciphertext >> 14);
                var bit13 = 1 & (ciphertext >> 13);
                var bit12 = 1 & (ciphertext >> 12);
                var bit11 = 1 & (ciphertext >> 11);
                var bit10 = 1 & (ciphertext >> 10);
                var bit9 = 1 & (ciphertext >> 9);
                var bit8 = 1 & (ciphertext >> 8);
                var bit7 = 1 & (ciphertext >> 7);
                var bit6 = 1 & (ciphertext >> 6);
                var bit5 = 1 & (ciphertext >> 5);
                var bit4 = 1 & (ciphertext >> 4);
                var bit3 = 1 & (ciphertext >> 3);
                var bit2 = 1 & (ciphertext >> 2);
                var bit1 = 1 & (ciphertext >> 1);
                var bit0 = 1 & (ciphertext >> 0);

                plaintext = ((bit0 << 6) |
                             (bit1 << 2) |
                             (bit2 << 14) |
                             (bit3 << 4) |
                             (bit4 << 5) |
                             (bit5 << 10) |
                             (bit6 << 0) |
                             (bit7 << 7) |
                             (bit8 << 1) |
                             (bit9 << 3) |
                             (bit10 << 12) |
                             (bit11 << 15) |
                             (bit12 << 9) |
                             (bit13 << 13) |
                             (bit14 << 11) |
                             (bit15 << 8));
            }

            //step 2: reverse substitution with reversed sbox
            plaintext = (SBox4r[15 & (plaintext >> 12)] << 12 |
                         SBox4r[15 & (plaintext >> 8)] << 8 |
                         SBox4r[15 & (plaintext >> 4)] << 4 |
                         SBox4r[plaintext & 15]);

            //step 3: XOR roundkey
            var quadruplekey = (roundkey << 12 | roundkey << 8 | roundkey << 4 | roundkey);
            plaintext = (plaintext ^ quadruplekey);


            return plaintext;
        }

        #endregion

        #region 8bit roundkeys 16bit block cipher

        /// <summary>
        /// Encrypts a plaintext using roundkeys
        /// 8bit roundkeys 16bit block cipher
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="roundkeys"></param>
        /// <returns></returns>
        public static int Encrypt8bit16(int plaintext, int[] roundkeys)
        {
            int ciphertext = plaintext & 65535;
            for (var round = 0; round < roundkeys.Length - 1; round++)
            {
                ciphertext = EncryptSingleRound8bit16(ciphertext, roundkeys[round], round == roundkeys.Length - 2);
            }

            var doublekey = ((roundkeys[roundkeys.Length - 1] & 255) << 8 |
                             (roundkeys[roundkeys.Length - 1] & 255));

            ciphertext = (ciphertext ^ doublekey);

            return ciphertext;
        }

        /// <summary>
        /// Decrypts a ciphertext using roundkeys
        /// 8bit roundkeys 16bit block cipher
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="roundkeys"></param>
        /// <returns></returns>
        public static int Decrypt8bit16(int ciphertext, int[] roundkeys)
        {
            int plaintext = ciphertext & 65535;

            var doublekey = ((roundkeys[roundkeys.Length - 1] & 255) << 8 |
                             (roundkeys[roundkeys.Length - 1] & 255));

            plaintext = (plaintext ^ doublekey);

            for (var round = roundkeys.Length - 2; round >= 0; round--)
            {
                plaintext = DecryptSingleRound8bit16(plaintext, roundkeys[round], round == roundkeys.Length - 2);
            }
            return plaintext;
        }

        /// <summary>
        /// Encrypts a single round using a single round key
        /// 8bit roundkeys 16bit block cipher
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="roundkey"></param>
        /// <param name="lastRound"></param>
        /// <returns></returns>
        private static int EncryptSingleRound8bit16(int plaintext, int roundkey, bool lastRound = false)
        {
            //step 1: XOR roundkey
            var doublekey = ((roundkey & 255) << 8 |
                             (roundkey & 255));
            var ciphertext = (plaintext ^ doublekey);

            //step 1: do substitution with 4 sboxes in parallel
            ciphertext = (SBox4[15 & (ciphertext >> 12)] << 12 |
                          SBox4[15 & (ciphertext >> 8)] << 8 |
                          SBox4[15 & (ciphertext >> 4)] << 4 |
                          SBox4[ciphertext & 15]);

            //step 2: do permutation
            if (!lastRound)
            {
                var bit15 = 1 & (ciphertext >> 15);
                var bit14 = 1 & (ciphertext >> 14);
                var bit13 = 1 & (ciphertext >> 13);
                var bit12 = 1 & (ciphertext >> 12);
                var bit11 = 1 & (ciphertext >> 11);
                var bit10 = 1 & (ciphertext >> 10);
                var bit9 = 1 & (ciphertext >> 9);
                var bit8 = 1 & (ciphertext >> 8);
                var bit7 = 1 & (ciphertext >> 7);
                var bit6 = 1 & (ciphertext >> 6);
                var bit5 = 1 & (ciphertext >> 5);
                var bit4 = 1 & (ciphertext >> 4);
                var bit3 = 1 & (ciphertext >> 3);
                var bit2 = 1 & (ciphertext >> 2);
                var bit1 = 1 & (ciphertext >> 1);
                var bit0 = 1 & (ciphertext >> 0);

                ciphertext = ((bit0 << 3)  |
                              (bit1 << 0)  |
                              (bit2 << 12) |
                              (bit3 << 1)  |
                              (bit4 << 6)  |
                              (bit5 << 8)  |
                              (bit6 << 15) |
                              (bit7 << 9)  |
                              (bit8 << 7)  |
                              (bit9 << 14) |
                              (bit10 << 10)|
                              (bit11 << 5) |
                              (bit12 << 11)|
                              (bit13 << 2) |
                              (bit14 << 13)|
                              (bit15 << 4));
            }
            return ciphertext;
        }

        /// <summary>
        /// Decrypts a single round using a single round key
        /// 8bit roundkeys 16bit block cipher
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="roundkey"></param>
        /// <param name="lastRound"></param>
        /// <returns></returns>
        private static int DecryptSingleRound8bit16(int ciphertext, int roundkey, bool lastRound = false)
        {
            int plaintext = ciphertext;

            //step 1: reverse permutation
            if (!lastRound)
            {
                var bit15 = 1 & (ciphertext >> 15);
                var bit14 = 1 & (ciphertext >> 14);
                var bit13 = 1 & (ciphertext >> 13);
                var bit12 = 1 & (ciphertext >> 12);
                var bit11 = 1 & (ciphertext >> 11);
                var bit10 = 1 & (ciphertext >> 10);
                var bit9 = 1 & (ciphertext >> 9);
                var bit8 = 1 & (ciphertext >> 8);
                var bit7 = 1 & (ciphertext >> 7);
                var bit6 = 1 & (ciphertext >> 6);
                var bit5 = 1 & (ciphertext >> 5);
                var bit4 = 1 & (ciphertext >> 4);
                var bit3 = 1 & (ciphertext >> 3);
                var bit2 = 1 & (ciphertext >> 2);
                var bit1 = 1 & (ciphertext >> 1);
                var bit0 = 1 & (ciphertext >> 0);

                plaintext = ((bit0 << 1)  |
                             (bit1 << 3)  |
                             (bit2 << 13) |
                             (bit3 << 0)  |
                             (bit4 << 15) |
                             (bit5 << 11) |
                             (bit6 << 4)  |
                             (bit7 << 8)  |
                             (bit8 << 5)  |
                             (bit9 << 7)  |
                             (bit10 << 10)|
                             (bit11 << 12)|
                             (bit12 << 2) |
                             (bit13 << 14)|
                             (bit14 << 9) |
                             (bit15 << 6));
            }

            //step 2: reverse substitution with reversed sbox
            plaintext = (SBox4r[15 & (plaintext >> 12)] << 12 |
                         SBox4r[15 & (plaintext >> 8)] << 8 |
                         SBox4r[15 & (plaintext >> 4)] << 4 |
                         SBox4r[plaintext & 15]);

            //step 3: XOR roundkey
            var doublekey = ((roundkey & 255) << 8 |
                             (roundkey & 255));
            plaintext = (plaintext ^ doublekey);


            return plaintext;
        }

        #endregion

        #region 16bit roundkeys 16bit block cipher

        /// <summary>
        /// Encrypts a plaintext using roundkeys
        /// 16bit roundkeys 16bit block cipher
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="roundkeys"></param>
        /// <returns></returns>
        public static int Encrypt16bit16(int plaintext, int[] roundkeys)
        {
            int ciphertext = plaintext & 65535;
            for (var round = 0; round < roundkeys.Length - 1; round++)
            {
                ciphertext = EncryptSingleRound16bit16(ciphertext, roundkeys[round], round == roundkeys.Length - 2);
            }

            ciphertext = (ciphertext ^ (roundkeys[roundkeys.Length - 1] & 65535));

            return ciphertext;
        }

        /// <summary>
        /// Decrypts a ciphertext using roundkeys
        /// 16bit roundkeys 16bit block cipher
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="roundkeys"></param>
        /// <returns></returns>
        public static int Decrypt16bit16(int ciphertext, int[] roundkeys)
        {
            int plaintext = ciphertext & 65535;
            plaintext = (plaintext ^ (roundkeys[roundkeys.Length - 1] & 65535));

            for (var round = roundkeys.Length - 2; round >= 0; round--)
            {
                plaintext = DecryptSingleRound16bit16(plaintext, roundkeys[round], round == roundkeys.Length - 2);
            }
            return plaintext;
        }

        /// <summary>
        /// Encrypts a single round using a single round key
        /// 16bit roundkeys 16bit block cipher
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="roundkey"></param>
        /// <param name="lastRound"></param>
        /// <returns></returns>
        private static int EncryptSingleRound16bit16(int plaintext, int roundkey, bool lastRound = false)
        {
            //step 1: XOR roundkey
            var ciphertext = (plaintext ^ (roundkey & 65535));

            //step 1: do substitution with 4 sboxes in parallel
            ciphertext = (SBox4[15 & (ciphertext >> 12)] << 12 |
                          SBox4[15 & (ciphertext >> 8)] << 8 |
                          SBox4[15 & (ciphertext >> 4)] << 4 |
                          SBox4[ciphertext & 15]);

            //step 2: do permutation
            if (!lastRound)
            {
                var bit15 = 1 & (ciphertext >> 15);
                var bit14 = 1 & (ciphertext >> 14);
                var bit13 = 1 & (ciphertext >> 13);
                var bit12 = 1 & (ciphertext >> 12);
                var bit11 = 1 & (ciphertext >> 11);
                var bit10 = 1 & (ciphertext >> 10);
                var bit9 = 1 & (ciphertext >> 9);
                var bit8 = 1 & (ciphertext >> 8);
                var bit7 = 1 & (ciphertext >> 7);
                var bit6 = 1 & (ciphertext >> 6);
                var bit5 = 1 & (ciphertext >> 5);
                var bit4 = 1 & (ciphertext >> 4);
                var bit3 = 1 & (ciphertext >> 3);
                var bit2 = 1 & (ciphertext >> 2);
                var bit1 = 1 & (ciphertext >> 1);
                var bit0 = 1 & (ciphertext >> 0);

                ciphertext = ((bit0 << 6) |
                              (bit1 << 13)|
                              (bit2 << 2) |
                              (bit3 << 11)|
                              (bit4 << 9) |
                              (bit5 << 10)|
                              (bit6 << 7) |
                              (bit7 << 4) |
                              (bit8 << 8) |
                              (bit9 << 14) |
                              (bit10 << 12)|
                              (bit11 << 3) |
                              (bit12 << 15)|
                              (bit13 << 1) |
                              (bit14 << 0) |
                              (bit15 << 5));
            }
            return ciphertext;
        }

        /// <summary>
        /// Decrypts a single round using a single round key
        /// 16bit roundkeys 16bit block cipher
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="roundkey"></param>
        /// <param name="lastRound"></param>
        /// <returns></returns>
        private static int DecryptSingleRound16bit16(int ciphertext, int roundkey, bool lastRound = false)
        {
            int plaintext = ciphertext;

            //step 1: reverse permutation
            if (!lastRound)
            {
                var bit15 = 1 & (ciphertext >> 15);
                var bit14 = 1 & (ciphertext >> 14);
                var bit13 = 1 & (ciphertext >> 13);
                var bit12 = 1 & (ciphertext >> 12);
                var bit11 = 1 & (ciphertext >> 11);
                var bit10 = 1 & (ciphertext >> 10);
                var bit9 = 1 & (ciphertext >> 9);
                var bit8 = 1 & (ciphertext >> 8);
                var bit7 = 1 & (ciphertext >> 7);
                var bit6 = 1 & (ciphertext >> 6);
                var bit5 = 1 & (ciphertext >> 5);
                var bit4 = 1 & (ciphertext >> 4);
                var bit3 = 1 & (ciphertext >> 3);
                var bit2 = 1 & (ciphertext >> 2);
                var bit1 = 1 & (ciphertext >> 1);
                var bit0 = 1 & (ciphertext >> 0);

                plaintext = ((bit0 << 14) |
                             (bit1 << 13) |
                             (bit2 << 2) |
                             (bit3 << 11) |
                             (bit4 << 7) |
                             (bit5 << 15) |
                             (bit6 << 0) |
                             (bit7 << 6) |
                             (bit8 << 8) |
                             (bit9 << 4) |
                             (bit10 << 5) |
                             (bit11 << 3) |
                             (bit12 << 10) |
                             (bit13 << 1) |
                             (bit14 << 9) |
                             (bit15 << 12));
            }

            //step 2: reverse substitution with reversed sbox
            plaintext = (SBox4r[15 & (plaintext >> 12)] << 12 |
                         SBox4r[15 & (plaintext >> 8)] << 8 |
                         SBox4r[15 & (plaintext >> 4)] << 4 |
                         SBox4r[plaintext & 15]);

            //step 3: XOR roundkey
            plaintext = (plaintext ^ (roundkey & 65535));


            return plaintext;
        }

        #endregion
    }
}
