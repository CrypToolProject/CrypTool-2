/*
   Copyright 2022 Nils Kopal <Nils.Kopal<at>CrypTool.org

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

namespace CrypTool.Plugins.SymbolCipher.CipherImplementations
{
    /// <summary>
    /// Abstract super class for symbol ciphers
    /// </summary>
    public abstract class ASymbolCipher
    {
        protected readonly int _dpi;
        public const double A4_WIDTH = 8.27; // inch
        public const double A4_HEIGHT = 11.69; // inch        

        public ASymbolCipher(int dpi = 300)
        {
            _dpi = dpi;
        }

        /// <summary>
        /// Encrypts the given plaintext using the given key into an image
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract Image Encrypt(string plaintext, string key = null);

        /// <summary>
        /// Creates a key image based on the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract Image GenerateKeyImage(string key);
    }
}
