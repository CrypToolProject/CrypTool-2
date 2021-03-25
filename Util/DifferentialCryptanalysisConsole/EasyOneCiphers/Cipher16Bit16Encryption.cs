using Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyOneCiphers
{
    class Cipher16Bit16Encryption : IEncryption
    {
        public int[] roundkeys = new int[] { 35454, 14202, 40783, 35297 };
        private readonly Random _random = new Random();

        public int EncryptBlock(int data)
        {
            
            int ciphertext = data & 65535;
            for (var round = 0; round < roundkeys.Length - 1; round++)
            {
                ciphertext = EncryptSingleRound16bit16(ciphertext, roundkeys[round], round == roundkeys.Length - 2);
            }

            ciphertext = (ciphertext ^ (roundkeys[roundkeys.Length - 1] & 65535));

            return ciphertext;
            /*
            int result = 0;
            //Round 1
            result = KeyMix(data, roundkeys[0]);
            result = SBox(result);
            result = PBox(result);

            //Round 2
            result = KeyMix(result, roundkeys[1]);
            result = SBox(result);
            result = PBox(result);

            //Round 3
            result = KeyMix(result, roundkeys[2]);
            result = SBox(result);
            result = KeyMix(result, roundkeys[3]);

            return result;
            */

        }

        private int EncryptSingleRound16bit16(int plaintext, int roundkey, bool lastRound = false)
        {
            //step 1: XOR roundkey
            var ciphertext = (plaintext ^ (roundkey & 65535));
            var ciphertext2 = (plaintext ^ roundkey);

            //step 1: do substitution with 4 sboxes in parallel
            ciphertext = (Cipher16Bit16Configuration.SBOX[15 & (ciphertext >> 12)] << 12 |
                          Cipher16Bit16Configuration.SBOX[15 & (ciphertext >> 8)] << 8 |
                          Cipher16Bit16Configuration.SBOX[15 & (ciphertext >> 4)] << 4 |
                          Cipher16Bit16Configuration.SBOX[ciphertext & 15]);

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
                              (bit1 << 13) |
                              (bit2 << 2) |
                              (bit3 << 11) |
                              (bit4 << 9) |
                              (bit5 << 10) |
                              (bit6 << 7) |
                              (bit7 << 4) |
                              (bit8 << 8) |
                              (bit9 << 14) |
                              (bit10 << 12) |
                              (bit11 << 3) |
                              (bit12 << 15) |
                              (bit13 << 1) |
                              (bit14 << 0) |
                              (bit15 << 5));
            }
            return ciphertext;
        }

        public void GenerateRandomKeys()
        {
            roundkeys = new[]{

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
                pboxedArray[Cipher16Bit16Configuration.PBOX[i]] = bits[i];
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

            BitArray keyBits = new BitArray(BitConverter.GetBytes(roundkeys[keyNum]));

            for (int i = keyBits.Length - 17; i >= 0; i--)
            {
                if (((i + 1) % 4) == 0)
                    sb.Append(" ");

                char c = keyBits[i] ? '1' : '0';
                sb.Append(c);
            }

            sb.Append(" Decimal K" + keyNum + ": " + roundkeys[keyNum]);

            return sb.ToString();
        }

        public string PrintKeys()
        {
            throw new NotImplementedException();
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
            zeroToThreeInt = Cipher16Bit16Configuration.SBOX[zeroToThreeInt];
            fourToSevenInt = Cipher16Bit16Configuration.SBOX[fourToSevenInt];
            eightToElevenInt = Cipher16Bit16Configuration.SBOX[eightToElevenInt];
            twelveToFifteenInt = Cipher16Bit16Configuration.SBOX[twelveToFifteenInt];

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
