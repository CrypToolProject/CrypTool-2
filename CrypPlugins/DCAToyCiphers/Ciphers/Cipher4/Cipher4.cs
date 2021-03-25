/*
   Copyright 2019 Christian Bender christian1.bender@student.uni-siegen.de

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
using System.Collections;

namespace DCAToyCiphers.Ciphers.Cipher4
{
    public class Cipher4 : IEncryption
    {
        private int[] _keys = new int[Cipher4Configuration.KEYNUM];

        /// <summary>
        /// Decrypts a single 4 bit block
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int DecryptBlock(int data)
        {
            int result = KeyMix(data, _keys[3]);
            result = ReverseSBox(result);
            result = KeyMix(result, _keys[2]);

            result = ReverseSBox(result);
            result = KeyMix(result, _keys[1]);

            result = ReverseSBox(result);
            result = KeyMix(result, _keys[0]);

            return result;
        }

        /// <summary>
        /// Encrypts a single 4 bit block
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int EncryptBlock(int data)
        {
            //round1
            int result = KeyMix(data, _keys[0]);
            result = SBox(result);

            //round2
            result = KeyMix(result, _keys[1]);
            result = SBox(result);

            //round3
            result = KeyMix(result, _keys[2]);
            result = SBox(result);
            result = KeyMix(result, _keys[3]);

            return result;
        }

        /// <summary>
        /// Applies the key-mixing
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public int KeyMix(int data, int key)
        {
            return (data ^ key);
        }

        /// <summary>
        /// Cipher4 has no permutation
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int PBox(int data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Cipher4 has no permutation
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int ReversePBox(int data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reverses the SBox
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int ReverseSBox(int data)
        {
            BitArray bitsOfBlock = new BitArray(BitConverter.GetBytes(data));

            BitArray zeroToThree = new BitArray(4);
            BitArray fourToSeven = new BitArray(4);

            for (int i = 0; i < 4; i++)
            {
                zeroToThree[i] = bitsOfBlock[i];
                fourToSeven[i] = bitsOfBlock[i + 4];
            }

            byte[] zeroToThreeBytes = new byte[4];
            zeroToThree.CopyTo(zeroToThreeBytes, 0);

            byte[] fourToSevenBytes = new byte[4];
            fourToSeven.CopyTo(fourToSevenBytes, 0);

            int zeroToThreeInt = BitConverter.ToInt32(zeroToThreeBytes, 0);
            int fourToSevenInt = BitConverter.ToInt32(fourToSevenBytes, 0);

            //use sbox
            zeroToThreeInt = Cipher4Configuration.SBOXREVERSE[zeroToThreeInt];
            fourToSevenInt = Cipher4Configuration.SBOXREVERSE[fourToSevenInt];

            bitsOfBlock = new BitArray(16);

            zeroToThree = new BitArray(BitConverter.GetBytes(zeroToThreeInt));
            fourToSeven = new BitArray(BitConverter.GetBytes(fourToSevenInt));

            for (int i = 0; i < 4; i++)
            {
                bitsOfBlock[i] = zeroToThree[i];
                //bitsOfBlock[4 + i] = fourToSeven[i];
            }

            byte[] bytes = new byte[4];
            bitsOfBlock.CopyTo(bytes, 0);
            int combined = BitConverter.ToInt32(bytes, 0);

            return combined;
        }

        /// <summary>
        /// Applies the SBox to a 4 bit block
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int SBox(int data)
        {
            BitArray bitsOfBlock = new BitArray(BitConverter.GetBytes(data));

            BitArray zeroToThree = new BitArray(4);

            for (int i = 0; i < 4; i++)
            {
                zeroToThree[i] = bitsOfBlock[i];
            }

            byte[] zeroToThreeBytes = new byte[4];
            zeroToThree.CopyTo(zeroToThreeBytes, 0);

            int zeroToThreeInt = BitConverter.ToInt32(zeroToThreeBytes, 0);

            //use sbox
            zeroToThreeInt = Cipher4Configuration.SBOX[zeroToThreeInt];

            bitsOfBlock = new BitArray(16);

            zeroToThree = new BitArray(BitConverter.GetBytes(zeroToThreeInt));

            for (int i = 0; i < 4; i++)
            {
                bitsOfBlock[i] = zeroToThree[i];
            }

            byte[] bytes = new byte[4];
            bitsOfBlock.CopyTo(bytes, 0);
            int combined = BitConverter.ToInt32(bytes, 0);

            return combined;
        }

        /// <summary>
        /// Setter for the keys, which will be supplied by the user
        /// </summary>
        /// <param name="keys"></param>
        public void SetKeys(int[] keys)
        {
            _keys = keys;
        }
    }
}
