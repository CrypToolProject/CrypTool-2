/*
   Copyright 2008 - 2022 CrypTool Team

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

namespace CrypTool.PluginBase.Control
{
    public interface IControlEncryption : IControl, IDisposable
    {
        byte[] Encrypt(byte[] plaintext, byte[] key);
        byte[] Encrypt(byte[] plaintext, byte[] key, byte[] iv);
        byte[] Encrypt(byte[] plaintext, byte[] key, byte[] iv, int bytesToUse);

        byte[] Decrypt(byte[] ciphertext, byte[] key);
        byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] iv);
        byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] iv, int bytesToUse);

        /// <summary>
        /// Returns the short name of the blockcipher, e.g. AES oder 3DES.
        /// </summary>
        /// <returns>The blockcipher's short name</returns>
        string GetCipherShortName();

        /// <summary>
        /// Returns the number of bytes of a block that is fixed for the cipher or being currently configured.
        /// </summary>
        /// <returns>The blocksize</returns>
        int GetBlockSizeAsBytes();

        /// <summary>
        /// Returns the number of bytes of the key that is fixed for the cipher or being currently configured.
        /// </summary>
        /// <returns>The keysize</returns>
        int GetKeySizeAsBytes();

        /// <summary>
        /// Returns the pattern that the corresponding encryption plugin expects for the abstract key.
        /// </summary>
        /// <returns>The pattern</returns>
        string GetKeyPattern();

        /// <summary>
        /// Returns the KeyTranslator which can be used to map abstract keys to concrete key for this encryption plugin.
        /// </summary>
        /// <returns>An implementation of IKeyTranslator.</returns>
        IKeyTranslator GetKeyTranslator();

        void ChangeSettings(string setting, object value);

        IControlEncryption Clone();

        event KeyPatternChanged KeyPatternChanged;
    }
}
