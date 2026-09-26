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
namespace PKCS1.Library
{
    public enum HashFunctionType { SHA1 = 0, SHA256 = 1, SHA384 = 2, SHA512 = 3, MD2 = 4, MD5 = 5 };

    internal static class HashFuncIdentHandler
    {
        public static HashFunctionIdent SHA1 = new HashFunctionIdent(HashFunctionType.SHA1, "SHA-1", "3021300906052B0E03021A05000414", 160);
        public static HashFunctionIdent SHA256 = new HashFunctionIdent(HashFunctionType.SHA256, "SHA-256", "3031300D060960864801650304020105000420", 256);
        public static HashFunctionIdent SHA384 = new HashFunctionIdent(HashFunctionType.SHA384, "SHA-384", "3041300D060960864801650304020205000430", 384);
        public static HashFunctionIdent SHA512 = new HashFunctionIdent(HashFunctionType.SHA512, "SHA-512", "3051300D060960864801650304020305000440", 512);
        public static HashFunctionIdent MD2 = new HashFunctionIdent(HashFunctionType.MD2, "MD2", "3020300C06082A864886F70D020205000410", 128);
        public static HashFunctionIdent MD5 = new HashFunctionIdent(HashFunctionType.MD5, "MD5", "3020300C06082A864886F70D020505000410", 128);
    }

    public class HashFunctionIdent
    {
        public string diplayName;
        public HashFunctionType type;
        public string DERIdent;
        public int digestLength;

        public HashFunctionIdent(HashFunctionType hashFuncType, string displayName, string DERIdent, int length)
        {
            type = hashFuncType;
            diplayName = displayName;
            this.DERIdent = DERIdent;
            digestLength = length;
        }

        public override string ToString()
        {
            return diplayName;
        }
    }
}
