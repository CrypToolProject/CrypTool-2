using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;

namespace TBCCipherThreeToyCipher
{
    class Encryption : IEncryption
    {
        private readonly Random _random = new Random();
        private int[] _keys = new int[CipherThreeConfiguration.KEYNUM];

        /// <summary>
        /// default constructor
        /// </summary>
        public Encryption()
        {
            GenerateRandomKeys();
        }

        public int DecryptBlock(int data)
        {
            int result = data;

            result = KeyMix(result, _keys[3]);
            result = ReverseSBox(result);
            result = KeyMix(result, _keys[2]);
            result = ReverseSBox(result);
            result = KeyMix(result, _keys[1]);
            result = ReverseSBox(result);
            result = KeyMix(result, _keys[0]);
            return result;
        }

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
            zeroToThreeInt = CipherThreeConfiguration.SBOXREVERSE[zeroToThreeInt];
            fourToSevenInt = CipherThreeConfiguration.SBOXREVERSE[fourToSevenInt];

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
        /// Encrypts a single block
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int EncryptBlock(int data)
        {
            int result = KeyMix(data, _keys[0]);
            result = SBox(result);
            result = KeyMix(result, _keys[1]);
            result = SBox(result);
            result = KeyMix(result, _keys[2]);
            //Console.WriteLine("Encryption after applied k3: " + result);
            /* */
            result = SBox(result);
            result = KeyMix(result, _keys[3]);
            
            return result;
        }

        /// <summary>
        /// Generates randomly chosen keys
        /// </summary>
        public void GenerateRandomKeys()
        {
            _keys = new int[]
            {
                
                _random.Next(0, 15),
                _random.Next(0, 15),
                _random.Next(0, 15),
                _random.Next(0, 15)
                /*
                14,
                10,
                8,
               
                0,
                0,
                0,
                9
                 */
            };
        }

        /// <summary>
        /// Key mixing
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public int KeyMix(int data, int key)
        {
            return data ^ key;
        }

        /// <summary>
        /// Method not needed in CipherThree
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int PBox(int data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a specified key in binary and decimal representation
        /// </summary>
        /// <param name="keyNum"></param>
        /// <returns></returns>
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
        /// returns the current keys as string
        /// </summary>
        /// <returns></returns>
        public string PrintKeys()
        {
            return (_keys[0] + ", " + _keys[1] + ", " + _keys[2] + ", " + _keys[3]);
        }

        /// <summary>
        /// Applies the SBox to the data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int SBox(int data)
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
            int fourToSevenInt = BitConverter.ToInt32(fourToSevenBytes, 0);;

            //use sbox
            zeroToThreeInt = CipherThreeConfiguration.SBOX[zeroToThreeInt];
            fourToSevenInt = CipherThreeConfiguration.SBOX[fourToSevenInt];

            bitsOfBlock = new BitArray(32);

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
    }
}
