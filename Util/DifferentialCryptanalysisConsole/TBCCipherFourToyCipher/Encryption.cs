using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;

namespace TBCCipherFourToyCipher
{
    class Encryption : IEncryption
    {
        private int[] _sbox = {6, 4, 12, 5, 0, 7, 2, 14, 1, 15, 3, 13, 8, 10, 9, 11};
        private int[] _pbox = {0, 4, 8, 12, 1, 5, 9, 13, 2, 6, 10, 14, 3, 7, 11, 15};
        private Random _random = new Random();
        public int[] _keys;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Encryption()
        {
            GenerateRandomKeys();
        }

        public int EncryptBlock(int data)
        {
            //First four rounds: Keymix, SBox, PBox
            for (int i = 0; i < 4; i++)
            {
#if DEBUG
                /*
                if (i == 3)
                {
                    Console.WriteLine("Encryption before k3: " + data);
                }   
                */
#endif
                data = KeyMix(data, _keys[i]);
#if DEBUG
                /*
                if (i == 3)
                {
                    Console.WriteLine("Encryption before applied SBoxes k3: " + data);
                } 
                */
#endif
                data = SBox(data);
#if DEBUG
                /*
                if (i == 3)
                {
                    Console.WriteLine("Encryption before applied PBoxes k3: " + data);
                }
                */
#endif
                data = PBox(data);
            }

#if DEBUG
            //Console.WriteLine("Encryption before k4: " + data);
#endif
            //Last round: Keymix, SBox, Keymix
            data = KeyMix(data, _keys[4]);
            data = SBox(data);
#if DEBUG
            //Console.WriteLine("Encryption before k5: " + data);
#endif
            data = KeyMix(data, _keys[5]);

            return data;
        }

        public void GenerateRandomKeys()
        {
            _keys = new[]
            {
                _random.Next(0, 65535),
                _random.Next(0, 65535),
                _random.Next(0, 65535),
                _random.Next(0, 65535),
                _random.Next(0, 65535),
                _random.Next(0, 65535)
                /*
                0x5b92,
                0x064b,
                0x1e03,
                0xa55f,
                0xecbd,
                0x7ca5
                 */
            };
        }

        public int KeyMix(int data, int key)
        {
            return data ^ key;
        }

        public int PBox(int data)
        {
            BitArray bits = new BitArray(BitConverter.GetBytes(data));
            BitArray pboxedArray = new BitArray(16);

            //use pbox
            for (int i = 0; i < 16; i++)
            {
                pboxedArray[_pbox[i]] = bits[i];
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

        public string PrintKeys()
        {
            return (_keys[0] + ", " + _keys[1] + ", " + _keys[2] + ", " + _keys[3] + ", " + _keys[4] + ", " + _keys[5]);
        }

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
            zeroToThreeInt = _sbox[zeroToThreeInt];
            fourToSevenInt = _sbox[fourToSevenInt];
            eightToElevenInt = _sbox[eightToElevenInt];
            twelveToFifteenInt = _sbox[twelveToFifteenInt];

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
    }
}