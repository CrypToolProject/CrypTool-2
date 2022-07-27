using Microsoft.VisualStudio.TestTools.UnitTesting;
using PKCS5;

namespace UnitTests
{
    /// <summary>
    /// test methods for PKCS#5 Plugin
    /// </summary>
    [TestClass]
    public class pkcsTest
    {
        public pkcsTest()
        {
            // nothing to do
        }

        private TestContext testContextInstance;
        public TestContext TestContext
        {
            get => testContextInstance;
            set => testContextInstance = value;
        }

        /// <summary>
        /// Converts the hex to byte.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <returns></returns>
        private byte[] ConvertHexToByte(string str)
        {
            int len = str.Length / 2;
            byte[] hex = new byte[len];
            for (int j = 0; j < len; j++)
            {
                hex[j] = byte.Parse(str.Substring(0, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
                str = str.Substring(2);
            }

            return hex;
        }

        /// <summary>
        /// Converts the byte to hex.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns></returns>
        private string ConvertByteToHex(byte[] bytes)
        {
            string tmp = "";
            foreach (byte b in bytes)
            {
                if (b < 0x10)
                {
                    tmp += "0";
                }

                tmp += b.ToString("X");
            }
            return tmp;
        }


        private void pkcs5part(byte[] key, byte[] salt, int hmac, int count, byte[] result)
        {
            PKCS5Settings set = new PKCS5Settings
            {
                Count = count,
                SHAFunction = hmac,
                Length = result.Length * 8 // Length must be in bits and not in bytes
            };

            PKCS5.PKCS5 p = new PKCS5.PKCS5
            {
                Settings = set,
                Key = key,
                Salt = salt
            };

            testContextInstance.WriteLine("key is  " + ConvertByteToHex(key));
            testContextInstance.WriteLine("salt is " + ConvertByteToHex(salt));

            p.Hash();
            byte[] h = p.HashOutputData;
            p.Dispose();

            // both arrays of same size?
            Assert.AreEqual(h.Length, result.Length, "Different hash sizes found");

            testContextInstance.WriteLine("expected hash is  " + ConvertByteToHex(result));
            testContextInstance.WriteLine("calculated hash is " + ConvertByteToHex(h));

            // the next compares references etc ... but not the array content :-(
            // Assert.AreEqual<byte[]>(result, h, "Different hash values found");
            // compare by hand ...
            for (int i = 0; i < h.Length; i++)
            {
                Assert.AreEqual(result[i], h[i], "Different hash values found");
            }

            testContextInstance.WriteLine("----");
        }

        [TestMethod]
        public void pkcs5TestMethodMD5()
        {
            byte[] key = { 0x70, 0x61, 0x73, 0x73, 0x77, 0x6f, 0x72, 0x64 };   // "password"
            byte[] salt = { 0x78, 0x57, 0x8E, 0x5A, 0x5D, 0x63, 0xCB, 0x06 };

            ///
            /// referenced test values taken from
            /// http://cryptosys.net/cgi-bin/manual.cgi?m=api&name=PBE_Kdf2 
            /// 
            //Derived key {HMAC-MD5}    = 66991b7f8010a0ba5d8a2e1e1a38341007f2eda8a79619d6 // reference needed
            //Derived key {HMAC-SHA1}   = BFDE6BE94DF7E11DD409BCE20A0255EC327CB936FFE93643
            //Derived key {HMAC-SHA256} = 97B5A91D35AF542324881315C4F849E327C4707D1BC9D322
            //Derived key {HMAC-SHA384} = bd6078731cef2cf5bdc48748a9da182ddc7b48a3cc28069e // reference needed
            //Derived key {HMAC-SHA512} = e6fa68fec0a2be2477809f8983e2719eb29415c61efacf34 // reference needed

            byte[] result_MD5 = { 0x66, 0x99, 0x1b, 0x7f, 0x80, 0x10, 0xa0, 0xba, 0x5d, 0x8a, 0x2e, 0x1e,
                               0x1a, 0x38, 0x34, 0x10, 0x07, 0xf2, 0xed, 0xa8, 0xa7, 0x96, 0x19, 0xd6 };
            byte[] result_SHA1 = { 0xBF, 0xDE, 0x6B, 0xE9, 0x4D, 0xF7, 0xE1, 0x1D, 0xD4, 0x09, 0xBC, 0xE2,
                               0x0A, 0x02, 0x55, 0xEC, 0x32, 0x7C, 0xB9, 0x36, 0xFF, 0xE9, 0x36, 0x43 };
            byte[] result_SHA256 = { 0x97, 0xB5, 0xA9, 0x1D, 0x35, 0xAF, 0x54, 0x23, 0x24, 0x88, 0x13, 0x15,
                               0xC4, 0xF8, 0x49, 0xE3, 0x27, 0xC4, 0x70, 0x7D, 0x1B, 0xC9, 0xD3, 0x22 };
            byte[] result_SHA384 = { 0xbd, 0x60, 0x78, 0x73, 0x1c, 0xef, 0x2c, 0xf5, 0xbd, 0xc4, 0x87, 0x48,
                               0xa9, 0xda, 0x18, 0x2d, 0xdc, 0x7b, 0x48, 0xa3, 0xcc, 0x28, 0x06, 0x9e };
            byte[] result_SHA512 = { 0xe6, 0xfa, 0x68, 0xfe, 0xc0, 0xa2, 0xbe, 0x24, 0x77, 0x80, 0x9f, 0x89,
                               0x83, 0xe2, 0x71, 0x9e, 0xb2, 0x94, 0x15, 0xc6, 0x1e, 0xfa, 0xcf, 0x34 };

            pkcs5part(key, salt, (int)System.Security.Cryptography.PKCS5MaskGenerationMethod.ShaFunction.MD5, 2048, result_MD5);
            pkcs5part(key, salt, (int)System.Security.Cryptography.PKCS5MaskGenerationMethod.ShaFunction.SHA1, 2048, result_SHA1);
            pkcs5part(key, salt, (int)System.Security.Cryptography.PKCS5MaskGenerationMethod.ShaFunction.SHA256, 2048, result_SHA256);
            pkcs5part(key, salt, (int)System.Security.Cryptography.PKCS5MaskGenerationMethod.ShaFunction.SHA384, 2048, result_SHA384);
            pkcs5part(key, salt, (int)System.Security.Cryptography.PKCS5MaskGenerationMethod.ShaFunction.SHA512, 2048, result_SHA512);

            // http://www.ietf.org/rfc/rfc3962.txt
            // Appendix B.  Sample Test Vectors, page 9

            byte[] salt2 = { 0x41, 0x54, 0x48, 0x45, 0x4e, 0x41, 0x2e, 0x4d, 0x49, 0x54, 0x2e,
                       0x45, 0x44, 0x55, 0x72, 0x61, 0x65, 0x62, 0x75, 0x72, 0x6e }; // "ATHENA.MIT.EDUraeburn"

            byte[] result1a = { 0xcd, 0xed, 0xb5, 0x28, 0x1b, 0xb2, 0xf8, 0x01,
                          0x56, 0x5a, 0x11, 0x22, 0xb2, 0x56, 0x35, 0x15 };

            byte[] result1b = { 0xcd, 0xed, 0xb5, 0x28, 0x1b, 0xb2, 0xf8, 0x01, 0x56, 0x5a, 0x11,
                          0x22, 0xb2, 0x56, 0x35, 0x15, 0x0a, 0xd1, 0xf7, 0xa0, 0x4b, 0xb9,
                          0xf3, 0xa3, 0x33, 0xec, 0xc0, 0xe2, 0xe1, 0xf7, 0x08, 0x37 };

            pkcs5part(key, salt2, (int)System.Security.Cryptography.PKCS5MaskGenerationMethod.ShaFunction.SHA1, 1, result1a);
            pkcs5part(key, salt2, (int)System.Security.Cryptography.PKCS5MaskGenerationMethod.ShaFunction.SHA1, 1, result1b);

            byte[] result2a = { 0x01, 0xdb, 0xee, 0x7f, 0x4a, 0x9e, 0x24, 0x3e,
                          0x98, 0x8b, 0x62, 0xc7, 0x3c, 0xda, 0x93, 0x5d };
            byte[] result2b = { 0x01, 0xdb, 0xee, 0x7f, 0x4a, 0x9e, 0x24, 0x3e, 0x98, 0x8b, 0x62,
                          0xc7, 0x3c, 0xda, 0x93, 0x5d, 0xa0, 0x53, 0x78, 0xb9, 0x32, 0x44,
                          0xec, 0x8f, 0x48, 0xa9, 0x9e, 0x61, 0xad, 0x79, 0x9d, 0x86 };

            pkcs5part(key, salt2, (int)System.Security.Cryptography.PKCS5MaskGenerationMethod.ShaFunction.SHA1, 2, result2a);
            pkcs5part(key, salt2, (int)System.Security.Cryptography.PKCS5MaskGenerationMethod.ShaFunction.SHA1, 2, result2b);

        }
    }
}
