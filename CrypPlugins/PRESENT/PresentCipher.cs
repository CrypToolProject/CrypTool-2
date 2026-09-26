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
using System.Security.Cryptography;

namespace CrypTool.PRESENT
{
    public abstract class PresentCipher : SymmetricAlgorithm
    {
        public PresentCipher()
        {
            KeySizeValue = 80;
            BlockSizeValue = 64;
            FeedbackSizeValue = 64;

            LegalKeySizesValue = new KeySizes[] { new KeySizes(80, 128, 48) };

            LegalBlockSizesValue = new KeySizes[] { new KeySizes(64, 64, 0) };
        }
    }
}