using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;

namespace _3Round16BitSPN
{
    class Encryption : IEncryption
    {
        private readonly Random _random = new Random();
        public int[] _keys = new int[4];

        public Encryption()
        {
            GenerateRandomKeys();
        }

        public int EncryptBlock(int data)
        {
            int result = 0;
            //Round 1
            result = KeyMix(data, _keys[0]);
            result = SBox(result);
            result = PBox(result);

            //Round 2
            result = KeyMix(result, _keys[1]);
            result = SBox(result);
            result = PBox(result);

            //Round 3
            result = KeyMix(result, _keys[2]);
            result = SBox(result);
            result = KeyMix(result, _keys[3]);

            return result;
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
            zeroToThreeInt = _3Round16BitSPNConfiguration.SBOX[zeroToThreeInt];
            fourToSevenInt = _3Round16BitSPNConfiguration.SBOX[fourToSevenInt];
            eightToElevenInt = _3Round16BitSPNConfiguration.SBOX[eightToElevenInt];
            twelveToFifteenInt = _3Round16BitSPNConfiguration.SBOX[twelveToFifteenInt];

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

        public int PBox(int data)
        {
            BitArray bits = new BitArray(BitConverter.GetBytes(data));
            BitArray pboxedArray = new BitArray(16);

            //use pbox
            for (int i = 0; i < 16; i++)
            {
                pboxedArray[_3Round16BitSPNConfiguration.PBOX[i]] = bits[i];
            }

            byte[] bytes = new byte[4];
            pboxedArray.CopyTo(bytes, 0);

            int outputBlock = BitConverter.ToInt32(bytes, 0);

            return outputBlock;
        }

        public int KeyMix(int data, int key)
        {
            return (data ^ key);
        }

        public void GenerateRandomKeys()
        {
            _keys = new[]{
                
                _random.Next(0, 65535),
                _random.Next(0, 65535),
                _random.Next(0, 65535),
                _random.Next(0, 65535)
                /*
                0x1e03,
                0xa55f,
                0xecbd,
                0x7ca5
                */
            };
        }

        public string PrintKeys()
        {
            return (_keys[0] + ", " + _keys[1] + ", " + _keys[2] + ", " + _keys[3]);
        }

        public string PrintKeyBits(int keyNum)
        {
            throw new NotImplementedException();
        }
    }
}
