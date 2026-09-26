/*
   Copyright (C) CrypTool 2 Team

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
namespace CrypTool.WEP
{
    public class RC4
    {
        public RC4()
        {
        }

        /// <summary>
        /// Encryptes an inputstream with the help of a 
        /// pseudo-random generation algorithm.
        /// </summary>
        /// <param name="input">Byte-Inputstream</param>
        /// <param name="key">WEP-Key</param>
        /// <returns></returns>
        public static byte[] rc4encrypt(byte[] input, byte[] key)
        {
            byte[] cipher = new byte[input.Length];

            byte[] sbox = new byte[256];
            int i, j = 0;
            byte b, keybyte;
            for (i = 0; i < 256; i++)
            {
                sbox[i] = (byte)i;
            }

            // initialization
            for (i = j = 0; i < 256; i++)
            {
                j = (j + sbox[i] + key[i % key.Length]) % 256;
                // swapping sbox[i] and sbox[j]
                // as you can see, swapping depends on key
                b = sbox[i];
                sbox[i] = sbox[j];
                sbox[j] = b;

            }

            i = 0;
            j = 0;
            for (int c = 0; c < input.Length; c++)
            {
                i = (i + 1) % 256;
                j = (j + sbox[i]) % 256;
                b = sbox[i];
                sbox[i] = sbox[j];
                sbox[j] = b;
                keybyte = sbox[((sbox[i] + sbox[j]) % 256)];
                // XOR
                cipher[c] = (byte)((input[c]) ^ keybyte);
            }
            return cipher;
        }
    }
}
