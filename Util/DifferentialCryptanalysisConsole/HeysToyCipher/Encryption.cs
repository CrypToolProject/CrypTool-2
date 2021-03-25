using System;
using System.Collections;
using System.Text;
using Interfaces;

namespace HeysToyCipher
{
    class Encryption : IEncryption
    {

        private readonly Random _random = new Random();
        public int[] _keys = new int[5];

        /// <summary>
        /// Default constructor
        /// </summary>
        public Encryption()
        {
            GenerateRandomKeys();
        }

        /// <summary>
        /// Encrypts a single block of data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int EncryptBlock(int data)
        {
            //First three rounds: Keymix, SBox, PBox
            for (int i = 0; i < 3; i++)
            {
                data = KeyMix(data, _keys[i]);
                data = SBox(data);
                data = PBox(data);
            }

            //Last round: Keymix, SBox, Keymix
            data = KeyMix(data, _keys[3]);
            data = SBox(data);
            data = KeyMix(data, _keys[4]);

            return data;
        }

        /// <summary>
        /// generates new keys
        /// </summary>
        public void GenerateRandomKeys()
        {
            _keys = new[]{
                _random.Next(0, 65535),
                _random.Next(0, 65535),
                _random.Next(0, 65535),
                _random.Next(0, 65535),
                _random.Next(0, 65535)
            }; 
        }

        /// <summary>
        /// Applies the key
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public int KeyMix(int data, int key)
        {
            return data ^ key;
        }

        /// <summary>
        /// Applies the PBox
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int PBox(int data)
        {
            BitArray bits = new BitArray(BitConverter.GetBytes(data));
            BitArray pboxedArray = new BitArray(32);

            //use pbox
            for (int i = 0; i < 16; i++)
            {
                pboxedArray[HeysToyCipherConfiguration.PBOX[i]] = bits[i];
            }

            byte[] bytes = new byte[4];
            pboxedArray.CopyTo(bytes, 0);

            int outputBlock = BitConverter.ToInt32(bytes, 0);

            return outputBlock;
        }

        public string PrintKeyBits(int keyNum)
        {
            var sb = new StringBuilder();
            sb.Append("Binary K" + keyNum + ":");

            BitArray keyBits = new BitArray(BitConverter.GetBytes(_keys[keyNum]));

            for (int i = keyBits.Length - 17; i >= 0; i--)
            {
                if (((i + 1) % 4) == 0)
                    sb.Append(" ");

                char c = keyBits[i] ? '1' : '0';
                sb.Append(c);
            }

            sb.Append(" Decimal K" + keyNum + ": " + _keys[keyNum]);

            return sb.ToString();
        }

        /// <summary>
        /// returns the current keys
        /// </summary>
        /// <returns></returns>
        public string PrintKeys()
        {
            return (_keys[0] + ", " + _keys[1] + ", " + _keys[2] + ", " + _keys[3] + ", " + _keys[4]);
        }

        /// <summary>
        /// Applies the SBox
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
            zeroToThreeInt = HeysToyCipherConfiguration.SBOX[zeroToThreeInt];
            fourToSevenInt = HeysToyCipherConfiguration.SBOX[fourToSevenInt];
            eightToElevenInt = HeysToyCipherConfiguration.SBOX[eightToElevenInt];
            twelveToFifteenInt = HeysToyCipherConfiguration.SBOX[twelveToFifteenInt];

            bitsOfBlock = new BitArray(32);

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
    }
}
