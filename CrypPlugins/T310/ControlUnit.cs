using System;
using System.Security.Cryptography;

namespace CrypTool.Plugins.T310
{

    /// <summary>
    /// This class handles the keys and checks for integrity.
    /// It represents the T310 control and block unit (Prüf- und Blockiereinheit)
    /// </summary>
    /// This unit mainly checks keys for correct parity of certain elements. That is the main way the T310 checks for
    /// integrity of certain elements. There are 10 registers in which the key S is stored. After the input the
    /// key is checked here. Also it would contain a second complex unit which would produce the exact same output
    /// as the default. If it was not the same or other errors occured the control unit would abort the operation.
    public class ControlUnit
    {

        /// The array represents the 5 key registers of S1 with a length of 24 bits each.
        /// The complete 5 registers must have an odd parity.
        public int[] S1 = new int[5];
        public int[] originalS1 = new int[5];

        /// The array represents the 5 key registers of S1 with a length of 24 bits each.
        /// The complete 5 registers must have an odd parity.
        public int[] S2 = new int[5];
        public int[] originalS2 = new int[5];

        private RandomNumberGenerator rand;
        private T310 t310Master;
        private BitSelectorEnum selector;

        public const int keyLength = 15;

        /// <summary>
        /// Initialize an instance of the T310 control unit (purpose: key and parity checks)
        /// </summary>
        /// <param name="t310Master"> the calling plugin function, needed for error messages</param>
        /// <param name="bitSelector"> selects if most or least significant bits are selected</param>
        public ControlUnit(T310 t310Master, BitSelectorEnum bitSelector)
        {
            this.t310Master = t310Master;
            rand = RandomNumberGenerator.Create();
            selector = bitSelector;
        }

        /// <summary>
        /// Receive a bit from S1.
        /// </summary>
        /// <returns>a byte containing a single bit of the key S1.</returns>
        public byte GetS1Bit()
        {
            byte value = selector == BitSelectorEnum.High ?
                (byte)(S1[4] & 0x01) : (byte)((S1[0] >> 23) & 0x01);
            return value;
        }

        /// <summary>
        /// Receive a bit from S2.
        /// </summary>
        /// <returns>a byte containing a single bit of the key S2.</returns>
        public byte GetS2Bit()
        {
            byte value = selector == BitSelectorEnum.High ?
               (byte)(S2[4] & 0x01) : (byte)((S2[0] >> 23) & 0x01);
            return value;
        }

        /// <summary>
        /// Shift the key S1
        /// </summary>
        public void ShiftS1()
        {
            S1 = ShiftKey(S1);
        }

        /// <summary>
        /// Shift the key S2
        /// </summary>
        public void ShiftS2()
        {
            S2 = ShiftKey(S2);
        }

        /// <summary>
        /// Shift the 5 registers of a T310 key.
        /// </summary>
        /// <param name="key">an int[] of length 5 holding a key S1 orS2</param>
        /// <returns>a int[] containing the shifted key</returns>
        private int[] ShiftKey(int[] key)
        {
            byte rightBound, leftBound = (byte)(key[4] & 0x01);

            for (int i = 0; i < key.Length; i++)
            {
                rightBound = (byte)(key[i] >> 23 & 0x01);
                key[i] >>= 1;
                key[i] |= ((int)leftBound << 23 & 0x800000);
                int tmp = ((int)leftBound << 23 & 0x01);
                leftBound = rightBound;
            }

            return key;
        }

        /// <summary>
        /// Check the validity of both T310 keys 
        /// </summary>
        /// <returns>true on valid key, false otherwise</returns>
        public bool CheckKeys()
        {
            byte keyIntegrity = 0;
            if (!CheckKeyParity(S1))
                keyIntegrity |= 1;
            if (!CheckKeyParity(S2))
                keyIntegrity |= 1;

            return keyIntegrity == 0 ? true : false;
        }

        /// <summary>
        /// Reset the keys to their original value
        /// </summary>
        public void ResetKeys()
        {
            Array.Copy(originalS1, S1, S2.Length);
            Array.Copy(originalS2, S2, S2.Length);
        }

        /// <summary>
        /// Low level check of a key for parity (odd = valid, even = invalid)
        /// </summary>
        /// <param name="key">a T310 key as int[] of length 5</param>
        /// <returns>true on valid key, false otherwise</returns>
        private bool CheckKeyParity(int[] key)
        {
            byte parity = 0;
            foreach (int element in key)
            {
                parity ^= (byte)(element % 2);
            }
            return parity == 0 ? false : true; //odd key
        }

        /// <summary>
        /// Reads keys from a byte[]
        /// </summary>
        /// <param name="inputKey">a byte[] of length 30 with containing the two keys</param>
        /// <returns>true on success, false otherwise</returns>
        public bool KeyFromBytes(byte[] inputKey, KeyIndex index)
        {
            if (inputKey.Length < keyLength)
                return false;

            if (index == KeyIndex.S1)
            {
                for (int i = 0, j = 0; i < 15 && j < 5; ++j)
                {
                    for (int k = 0; k < 3; ++k)
                        S1[j] |= (inputKey[i++] << (8 * k));
                }
                return CheckKeyParity(S1);
            }

            if (index == KeyIndex.S2)
            {
                for (int i = 0, j = 0; i < 15 && j < 5; ++j)
                {
                    for (int k = 0; k < 3; ++k)
                        S2[j] |= (inputKey[i++] << (8 * k));
                }
                return CheckKeyParity(S2);
            }

            return false;
        }

        /// <summary>
        /// Converts both keys to a single byte[] 
        /// </summary>
        /// <param name="outputBytes">the output keys as byte[]</param>
        /// <returns>true on success, false otherwise </returns>
        public bool KeysToBytes(ref byte[] outputBytes)
        {

            if (!CheckKeys())
                return false;
            if (outputBytes == null || outputBytes.Length < 30)
                return false;


            int i = 0;
            for (int j = 0; i < 15 && j < 5; ++j)
            {
                for (int k = 0; k < 3; ++k)
                    outputBytes[i++] =(byte)(S1[j] >> (8 * k));
            }

            for (int j = 0; i < 30 && j < 5; ++j)
            {
                for (int k = 0; k < 3; ++k)
                    outputBytes[i++] = (byte)(S2[j] >> (8 * k));
            }

            return CheckKeys();
        }

        /// <summary>
        ///  Create the T310 key S1 and check its parity
        /// </summary>
        /// 
        /// The key is written into an an integer array of size 5, representing
        /// the 5 key registers in the machine. After the process of creation it is
        /// parity checked.
        /// 
        /// <returns>true on success, false otherwise</returns>
        public bool CreateKeyS1()
        {
            bool validKey = false;
            for (int i = 0; i < 30 && !validKey; ++i)
            {
                S1 = GenerateKeyBytes();
                if (CheckKeyParity(S1))
                    validKey = true;
            }


            /*DEBUGGING CODE BEGINNING*/
            S1[0] = 0x1777d3;
            S1[1] = 0x8617a3;
            S1[2] = 0x183e59;
            S1[3] = 0xfc2388;
            S1[4] = 0x1fe1d2;
            /*DEBUGGING CODE ENDING*/

            Array.Copy(S1, originalS1, originalS1.Length);

            return validKey;
        }

        /// <summary>
        ///  Create the T310 key S2 and check its parity
        /// </summary>
        /// 
        /// The key is written into an an integer array of size 5, representing
        /// the 5 key registers in the machine. After the process of creation it is
        /// parity checked.
        /// 
        /// <returns>true on success, false otherwise</returns>
        public bool CreateKeyS2()
        {
            bool validKey = false;
            for (int i = 0; i < 30 && !validKey; ++i)
            {
                S2 = GenerateKeyBytes();
                if (CheckKeyParity(S2))
                    validKey = true;
            }

            if (!validKey)
                //Console.WriteLine("Key parity is incorrect (even)");

            /*DEBUGGING CODE BEGINNING*/
            S2[0] = 0x4c4f0d;
            S2[1] = 0x19eba3;
            S2[2] = 0xac7a7e;
            S2[3] = 0x5d01fb;
            S2[4] = 0x9e1d84;
            /*DEBUGGING CODE ENDING*/

            Array.Copy(S2, originalS2, originalS2.Length);
            return validKey;
        }

        /// <summary>
        /// Generates a key by random, there is no guarantee those are a valid key
        /// </summary>
        /// <returns>an int[] with random bytes</returns>
        private int[] GenerateKeyBytes()
        {
            
            byte[] tmpBytes = new byte[30];
            int[] keyBytes = new int[5];


            rand.GetNonZeroBytes(tmpBytes);

            for (int i = 0, k = 0; i < S1.Length; ++i)
            {
                keyBytes[i] |= (int)(tmpBytes[k++]);
                keyBytes[i] |= (int)(tmpBytes[k++]) << 8;
                keyBytes[i] |= (int)(tmpBytes[k++]) << 16;
                keyBytes[i] &= 0xFFFFFF;
            }
            return keyBytes;
        }
    }
}



