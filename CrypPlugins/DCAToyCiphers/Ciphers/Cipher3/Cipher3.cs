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

namespace DCAToyCiphers.Ciphers.Cipher3
{
    public class Cipher3 : IEncryption
    {
        private int[] _keys = new int[Cipher3Configuration.KEYNUM];

        /// <summary>
        /// Decrypts a single 16 bit block
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int DecryptBlock(int data)
        {
            int result = KeyMix(data, _keys[5]);
            result = ReverseSBox(result);
            result = KeyMix(result, _keys[4]);

            result = ReversePBox(result);
            result = ReverseSBox(result);
            result = KeyMix(result, _keys[3]);

            result = ReversePBox(result);
            result = ReverseSBox(result);
            result = KeyMix(result, _keys[2]);

            result = ReversePBox(result);
            result = ReverseSBox(result);
            result = KeyMix(result, _keys[1]);

            result = ReversePBox(result);
            result = ReverseSBox(result);
            result = KeyMix(result, _keys[0]);

            return result;
        }

        /// <summary>
        /// Encrypts a single 16 bit block
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int EncryptBlock(int data)
        {
            //First four rounds: Keymix, SBox, PBox
            //Round1
            data = KeyMix(data, _keys[0]);
            data = SBox(data);
            data = PBox(data);

            //Round2
            data = KeyMix(data, _keys[1]);
            data = SBox(data);
            data = PBox(data);

            //Round3
            data = KeyMix(data, _keys[2]);
            data = SBox(data);
            data = PBox(data);

            //Round4
            data = KeyMix(data, _keys[3]);
            data = SBox(data);
            data = PBox(data);

            //Last round: Keymix, SBox, Keymix
            data = KeyMix(data, _keys[4]);
            data = SBox(data);
            data = KeyMix(data, _keys[5]);

            return data;
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
        /// Applies the permutation to a 16 bit block
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int PBox(int data)
        {
            BitArray bits = new BitArray(BitConverter.GetBytes(data));
            BitArray pboxedArray = new BitArray(16);

            //use pbox
            for (int i = 0; i < 16; i++)
            {
                pboxedArray[Cipher3Configuration.PBOX[i]] = bits[i];
            }

            byte[] bytes = new byte[4];
            pboxedArray.CopyTo(bytes, 0);

            int outputBlock = BitConverter.ToInt32(bytes, 0);

            return outputBlock;
        }

        /// <summary>
        /// Reverses the PBox
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int ReversePBox(int data)
        {
            BitArray bitsOfBlock = new BitArray(BitConverter.GetBytes(data));
            BitArray pboxedArray = new BitArray(16);

            //use pbox
            for (int i = 0; i < 16; i++)
            {
                pboxedArray[Cipher3Configuration.PBOXREVERSE[i]] = bitsOfBlock[i];
            }

            byte[] bytes = new byte[4];
            pboxedArray.CopyTo(bytes, 0);

            int outputBlock = BitConverter.ToInt32(bytes, 0);
            return outputBlock;
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
            BitArray eightToEleven = new BitArray(4);
            BitArray twelveToFifteen = new BitArray(4);

            for (int i = 0; i < 4; i++)
            {
                zeroToThree[i] = bitsOfBlock[i];
                fourToSeven[i] = bitsOfBlock[i + 4];
                eightToEleven[i] = bitsOfBlock[i + 8];
                twelveToFifteen[i] = bitsOfBlock[i + 12];
            }

            byte[] zeroToThreeBytes = new byte[4];
            zeroToThree.CopyTo(zeroToThreeBytes, 0);

            byte[] fourToSevenBytes = new byte[4];
            fourToSeven.CopyTo(fourToSevenBytes, 0);

            byte[] eightToElevenBytes = new byte[4];
            eightToEleven.CopyTo(eightToElevenBytes, 0);

            byte[] twelveToFifteenBytes = new byte[4];
            twelveToFifteen.CopyTo(twelveToFifteenBytes, 0);

            int zeroToThreeInt = BitConverter.ToInt32(zeroToThreeBytes, 0);
            int fourToSevenInt = BitConverter.ToInt32(fourToSevenBytes, 0);
            int eightToElevenInt = BitConverter.ToInt32(eightToElevenBytes, 0);
            int twelveToFifteenInt = BitConverter.ToInt32(twelveToFifteenBytes, 0);

            //use sbox
            zeroToThreeInt = Cipher3Configuration.SBOXREVERSE[zeroToThreeInt];
            fourToSevenInt = Cipher3Configuration.SBOXREVERSE[fourToSevenInt];
            eightToElevenInt = Cipher3Configuration.SBOXREVERSE[eightToElevenInt];
            twelveToFifteenInt = Cipher3Configuration.SBOXREVERSE[twelveToFifteenInt];

            bitsOfBlock = new BitArray(16);

            zeroToThree = new BitArray(BitConverter.GetBytes(zeroToThreeInt));
            fourToSeven = new BitArray(BitConverter.GetBytes(fourToSevenInt));
            eightToEleven = new BitArray(BitConverter.GetBytes(eightToElevenInt));
            twelveToFifteen = new BitArray(BitConverter.GetBytes(twelveToFifteenInt));

            for (int i = 0; i < 4; i++)
            {
                bitsOfBlock[i] = zeroToThree[i];
                bitsOfBlock[4 + i] = fourToSeven[i];
                bitsOfBlock[8 + i] = eightToEleven[i];
                bitsOfBlock[12 + i] = twelveToFifteen[i];
            }

            byte[] bytes = new byte[4];
            bitsOfBlock.CopyTo(bytes, 0);
            int combined = BitConverter.ToInt32(bytes, 0);

            return combined;
        }

        /// <summary>
        /// Applies the SBoxes to a 16 bit block
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int SBox(int data)
        {
            BitArray bitsOfBlock = new BitArray(BitConverter.GetBytes(data));

            BitArray zeroToThree = new BitArray(4);
            BitArray fourToSeven = new BitArray(4);
            BitArray eightToEleven = new BitArray(4);
            BitArray twelveToFifteen = new BitArray(4);

            for (int i = 0; i < 4; i++)
            {
                zeroToThree[i] = bitsOfBlock[i];
                fourToSeven[i] = bitsOfBlock[i + 4];
                eightToEleven[i] = bitsOfBlock[i + 8];
                twelveToFifteen[i] = bitsOfBlock[i + 12];
            }

            byte[] zeroToThreeBytes = new byte[4];
            zeroToThree.CopyTo(zeroToThreeBytes, 0);

            byte[] fourToSevenBytes = new byte[4];
            fourToSeven.CopyTo(fourToSevenBytes, 0);

            byte[] eightToElevenBytes = new byte[4];
            eightToEleven.CopyTo(eightToElevenBytes, 0);

            byte[] twelveToFifteenBytes = new byte[4];
            twelveToFifteen.CopyTo(twelveToFifteenBytes, 0);

            int zeroToThreeInt = BitConverter.ToInt32(zeroToThreeBytes, 0);
            int fourToSevenInt = BitConverter.ToInt32(fourToSevenBytes, 0);
            int eightToElevenInt = BitConverter.ToInt32(eightToElevenBytes, 0);
            int twelveToFifteenInt = BitConverter.ToInt32(twelveToFifteenBytes, 0);

            //use sbox
            zeroToThreeInt = Cipher3Configuration.SBOX[zeroToThreeInt];
            fourToSevenInt = Cipher3Configuration.SBOX[fourToSevenInt];
            eightToElevenInt = Cipher3Configuration.SBOX[eightToElevenInt];
            twelveToFifteenInt = Cipher3Configuration.SBOX[twelveToFifteenInt];

            bitsOfBlock = new BitArray(16);

            zeroToThree = new BitArray(BitConverter.GetBytes(zeroToThreeInt));
            fourToSeven = new BitArray(BitConverter.GetBytes(fourToSevenInt));
            eightToEleven = new BitArray(BitConverter.GetBytes(eightToElevenInt));
            twelveToFifteen = new BitArray(BitConverter.GetBytes(twelveToFifteenInt));

            for (int i = 0; i < 4; i++)
            {
                bitsOfBlock[i] = zeroToThree[i];
                bitsOfBlock[4 + i] = fourToSeven[i];
                bitsOfBlock[8 + i] = eightToEleven[i];
                bitsOfBlock[12 + i] = twelveToFifteen[i];
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
