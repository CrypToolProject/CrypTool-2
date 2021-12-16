/*
   Copyright 2008 Timm Korte, University of Siegen

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

using System.Security.Cryptography;

namespace CrypTool.PRESENT
{
    public sealed class PresentManaged : PresentCipher
    {
        private readonly RNGCryptoServiceProvider sRandom = new RNGCryptoServiceProvider();

        public PresentManaged()
        {
        }

        public override void GenerateIV()
        {
            IVValue = new byte[8];
            sRandom.GetBytes(IVValue);
        }

        public override void GenerateKey()
        {
            KeyValue = new byte[KeySizeValue >> 3];
            sRandom.GetBytes(KeyValue);
        }

        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
        {
            return new PresentTransform(this, false, rgbKey, rgbIV);
        }

        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
        {
            return new PresentTransform(this, true, rgbKey, rgbIV);
        }
    }
}
