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
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System.Text;

namespace PKCS1.Library
{
    internal class Hashfunction
    {
        private static IDigest hashFunctionDigest = DigestUtilities.GetDigest(HashFuncIdentHandler.SHA1.diplayName); // default SHA1

        public static byte[] generateHashDigest(string input, HashFunctionIdent hashIdent)
        {
            byte[] bInput = Encoding.ASCII.GetBytes(input);
            return generateHashDigest(ref bInput, ref hashIdent);
        }

        public static byte[] generateHashDigest(ref byte[] input, ref HashFunctionIdent hashIdent)
        {
            hashFunctionDigest = DigestUtilities.GetDigest(hashIdent.diplayName);
            byte[] hashDigest = new byte[hashFunctionDigest.GetDigestSize()];
            hashFunctionDigest.BlockUpdate(input, 0, input.Length);
            hashFunctionDigest.DoFinal(hashDigest, 0);

            return hashDigest;
        }

        // gibt länge in bytes zurück!
        public static int getDigestSize()
        {
            return hashFunctionDigest.GetDigestSize();
        }

        public static string getAlgorithmName()
        {
            return hashFunctionDigest.AlgorithmName;
        }
    }
}
