using CrypTool.PluginBase.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTests
{
    [TestClass]
    public class CRCTest
    {
        public CRCTest()
        {
        }

        // convert stream to ulong
        private ulong stream2ulong(ICrypToolStream s)
        {
            byte[] tmpbuf = s.ToByteArray();
            Array.Reverse(tmpbuf);

            byte[] buf = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                buf[i] = 0;
            }

            for (int i = 0; i < tmpbuf.Length; i++)
            {
                buf[i] = tmpbuf[i];
            }

            return BitConverter.ToUInt64(buf, 0);
        }

        [TestMethod]
        public void CRCTestMethod()
        {
            CrypTool.PluginBase.ICrypComponent pluginInstance = TestHelpers.GetPluginInstance("CRC");
            PluginTestScenario scenario1 = new PluginTestScenario(pluginInstance, new[] { "InputStream", ".Width", ".Polynomial", ".Init", ".XorOut", ".RefIn", ".RefOut" }, new[] { "OutputStream" });
            PluginTestScenario scenario2 = new PluginTestScenario(pluginInstance, new[] { "InputStream", ".CRCMethod" }, new[] { "OutputStream" });
            object[] output;

            ICrypToolStream input = "313233343536373839".HexToStream(); // "123456789" as Stream
            ulong check;

            for (int method = 0; method < testvectors.Length; method++)
            {
                TestVector vector = testvectors[method];

                // set CRC parameters individually
                output = scenario1.GetOutputs(new object[] { input, vector.width, vector.polynomial.ToString("x"), vector.init.ToString("x"), vector.xorout.ToString("x"), vector.refin, vector.refout });
                check = stream2ulong(output[0] as ICrypToolStream);
                Assert.AreEqual(vector.check.ToString("x"), check.ToString("x"), "Unexpected value in test '" + vector.name + "'.");

                // set CRC parameters by selecting a CRCMethod
                output = scenario2.GetOutputs(new object[] { input, method });
                check = stream2ulong(output[0] as ICrypToolStream);
                Assert.AreEqual(vector.check.ToString("x"), check.ToString("x"), "Unexpected value in test '" + vector.name + "'.");
            }
        }

        private struct TestVector
        {
            public string name;
            public int width;
            public bool refin, refout;
            public ulong polynomial, init, xorout, check;
        }

        //
        // Sources of the test vectors:
        //  http://protocoltool.sourceforge.net/CRC%20list.html
        //  http://reveng.sourceforge.net/crc-catalogue/
        //
        private readonly TestVector[] testvectors = new TestVector[] {
            new TestVector () { name = "CRC-1/Partiy", width=1, polynomial=0x01, init=0x00, xorout=0x00, refin=false, refout=false, check=0x01 },
            new TestVector () { name = "CRC-3/ROHC", width=3, polynomial=0x3, init=0x7, xorout=0x0, refin=true, refout=true, check=0x6 },
            new TestVector () { name = "CRC-4/ITU", width=4, polynomial=0x3, init=0x0, xorout=0x0, refin=true, refout=true, check=0x7 },
            new TestVector () { name = "CRC-5/EPC", width=5, polynomial=0x09, init=0x09, xorout=0x00, refin=false, refout=false, check=0x00 },
            new TestVector () { name = "CRC-5/ITU", width=5, polynomial=0x15, init=0x00, xorout=0x00, refin=true, refout=true, check=0x07 },
            new TestVector () { name = "CRC-5/USB", width=5, polynomial=0x05, init=0x1F, xorout=0x1F, refin=true, refout=true, check=0x19 },
            new TestVector () { name = "CRC-6/DARC", width=6, polynomial=0x19, init=0x00, xorout=0x00, refin=true, refout=true, check=0x26 },
            new TestVector () { name = "CRC-6/ITU", width=6, polynomial=0x03, init=0x00, xorout=0x00, refin=true, refout=true, check=0x06 },
            new TestVector () { name = "CRC-7", width=7, polynomial=0x09, init=0x00, xorout=0x00, refin=false, refout=false, check=0x75 },
            new TestVector () { name = "CRC-7/ROHC", width=7, polynomial=0x4F, init=0x7F, xorout=0x00, refin=true, refout=true, check=0x53 },
            new TestVector () { name = "CRC-8", width=8, polynomial=0x07, init=0x00, xorout=0x00, refin=false, refout=false, check=0xF4 },
            new TestVector () { name = "CRC-8/ITU", width=8, polynomial=0x07, init=0x00, xorout=0x55, refin=false, refout=false, check=0xA1 },
            new TestVector () { name = "CRC-8/ROHC", width=8, polynomial=0x07, init=0xFF, xorout=0x00, refin=true, refout=true, check=0xD0 },
            new TestVector () { name = "CRC-8/DARC", width=8, polynomial=0x39, init=0x00, xorout=0x00, refin=true, refout=true, check=0x15 },
            new TestVector () { name = "CRC-8/I-CODE", width=8, polynomial=0x1D, init=0xFD, xorout=0x00, refin=false, refout=false, check=0x7E },
            new TestVector () { name = "CRC-8/J1850", width=8, polynomial=0x1D, init=0xFF, xorout=0xFF, refin=false, refout=false, check=0x4B },
            new TestVector () { name = "CRC-8/MAXIM", width=8, polynomial=0x31, init=0x00, xorout=0x00, refin=true, refout=true, check=0xA1 },
            new TestVector () { name = "CRC-8/WCDMA", width=8, polynomial=0x9B, init=0x00, xorout=0x00, refin=true, refout=true, check=0x25 },
            new TestVector () { name = "CRC-8/CCITT", width=8, polynomial=0x8D, init=0x00, xorout=0x00, refin=false, refout=false, check=0xD2 },
            new TestVector () { name = "CRC-8/EBU", width=8, polynomial=0x1D, init=0xFF, xorout=0x00, refin=true, refout=true, check=0x97 },
            new TestVector () { name = "CRC-10", width=10, polynomial=0x233, init=0x000, xorout=0x000, refin=false, refout=false, check=0x199 },
            new TestVector () { name = "CRC-11", width=11, polynomial=0x385, init=0x01A, xorout=0x000, refin=false, refout=false, check=0x5A3 },
            new TestVector () { name = "CRC-12/3GPP", width=12, polynomial=0x80F, init=0x000, xorout=0x000, refin=false, refout=true, check=0xDAF },
            new TestVector () { name = "CRC-12/DECT", width=12, polynomial=0x80F, init=0x000, xorout=0x000, refin=false, refout=false, check=0xF5B },
            new TestVector () { name = "CRC-14/DARC", width=14, polynomial=0x0805, init=0x0000, xorout=0x0000, refin=true, refout=true, check=0x082D },
            new TestVector () { name = "CRC-15", width=15, polynomial=0x4599, init=0x0000, xorout=0x0000, refin=false, refout=false, check=0x059E },
            new TestVector () { name = "CRC-15/MPT1327", width=15, polynomial=0x6815, init=0x0000, xorout=0x0001, refin=false, refout=false, check=0x2566 },
            new TestVector () { name = "CRC-16", width=16, polynomial=0x8005, init=0x0000, xorout=0x0000, refin=true, refout=true, check=0xBB3D },
            new TestVector () { name = "CRC-16/AUG-CCITT", width=16, polynomial=0x1021, init=0x1D0F, xorout=0x0000, refin=false, refout=false, check=0xE5CC },
            new TestVector () { name = "CRC-16/BUYPASS", width=16, polynomial=0x8005, init=0x0000, xorout=0x0000, refin=false, refout=false, check=0xFEE8 },
            new TestVector () { name = "CRC-16/CCITT-FALSE", width=16, polynomial=0x1021, init=0xFFFF, xorout=0x0000, refin=false, refout=false, check=0x29B1 },
            new TestVector () { name = "CRC-16/DDS-110", width=16, polynomial=0x8005, init=0x800D, xorout=0x0000, refin=false, refout=false, check=0x9ECF },
            new TestVector () { name = "CRC-16/DECT-R", width=16, polynomial=0x0589, init=0x0000, xorout=0x0001, refin=false, refout=false, check=0x007E },
            new TestVector () { name = "CRC-16/DECT-X", width=16, polynomial=0x0589, init=0x0000, xorout=0x0000, refin=false, refout=false, check=0x007F },
            new TestVector () { name = "CRC-16/DNP", width=16, polynomial=0x3D65, init=0x0000, xorout=0xFFFF, refin=true, refout=true, check=0xEA82 },
            new TestVector () { name = "CRC-16/EN-13757", width=16, polynomial=0x3D65, init=0x0000, xorout=0xFFFF, refin=false, refout=false, check=0xC2B7 },
            new TestVector () { name = "CRC-16/GENIBUS", width=16, polynomial=0x1021, init=0xFFFF, xorout=0xFFFF, refin=false, refout=false, check=0xD64E },
            new TestVector () { name = "CRC-16/MAXIM", width=16, polynomial=0x8005, init=0x0000, xorout=0xFFFF, refin=true, refout=true, check=0x44C2 },
            new TestVector () { name = "CRC-16/MCRF4XX", width=16, polynomial=0x1021, init=0xFFFF, xorout=0x0000, refin=true, refout=true, check=0x6F91 },
            new TestVector () { name = "CRC-16/RIELLO", width=16, polynomial=0x1021, init=0xB2AA, xorout=0x0000, refin=true, refout=true, check=0x63D0 },
            new TestVector () { name = "CRC-16/T10-DIF", width=16, polynomial=0x8BB7, init=0x0000, xorout=0x0000, refin=false, refout=false, check=0xD0DB },
            new TestVector () { name = "CRC-16/TELEDISK", width=16, polynomial=0xA097, init=0x0000, xorout=0x0000, refin=false, refout=false, check=0x0FB3 },
            new TestVector () { name = "CRC-16/TMS37157", width=16, polynomial=0x1021, init=0x89EC, xorout=0x0000, refin=true, refout=true, check=0x26B1 },
            new TestVector () { name = "CRC-16/USB", width=16, polynomial=0x8005, init=0xFFFF, xorout=0xFFFF, refin=true, refout=true, check=0xB4C8 },
            new TestVector () { name = "CRC-A", width=16, polynomial=0x1021, init=0xC6C6, xorout=0x0000, refin=true, refout=true, check=0xBF05 },
            new TestVector () { name = "KERMIT", width=16, polynomial=0x1021, init=0x0000, xorout=0x0000, refin=true, refout=true, check=0x2189 },
            new TestVector () { name = "MODBUS", width=16, polynomial=0x8005, init=0xFFFF, xorout=0x0000, refin=true, refout=true, check=0x4B37 },
            new TestVector () { name = "CRC-16/IBM-SDLC", width=16, polynomial=0x1021, init=0xFFFF, xorout=0xFFFF, refin=true, refout=true, check=0x906E },
            new TestVector () { name = "XMODEM", width=16, polynomial=0x1021, init=0x0000, xorout=0x0000, refin=false, refout=false, check=0x31C3 },
            new TestVector () { name = "CRC-24/OPENPGP", width=24, polynomial=0x864CFB, init=0xB704CE, xorout=0x000000, refin=false, refout=false, check=0x21CF02 },
            new TestVector () { name = "CRC-24/FLEXRAY-A", width=24, polynomial=0x5D6DCB, init=0xFEDCBA, xorout=0x000000, refin=false, refout=false, check=0x7979BD },
            new TestVector () { name = "CRC-24/FLEXRAY-B", width=24, polynomial=0x5D6DCB, init=0xABCDEF, xorout=0x000000, refin=false, refout=false, check=0x1F23B8 },
            new TestVector () { name = "CRC-31/PHILIPS", width=31, polynomial=0x04C11DB7, init=0x7FFFFFFF, xorout=0x7FFFFFFF, refin=false, refout=false, check=0x0CE9E46C },
            new TestVector () { name = "CRC-32", width=32, polynomial=0x04C11DB7, init=0xFFFFFFFF, xorout=0xFFFFFFFF, refin=true, refout=true, check=0xCBF43926 },
            new TestVector () { name = "CRC-32/BZIP2", width=32, polynomial=0x04C11DB7, init=0xFFFFFFFF, xorout=0xFFFFFFFF, refin=false, refout=false, check=0xFC891918 },
            new TestVector () { name = "CRC-32/MPEG-2", width=32, polynomial=0x04C11DB7, init=0xFFFFFFFF, xorout=0x00000000, refin=false, refout=false, check=0x0376E6E7 },
            new TestVector () { name = "CRC-32/POSIX", width=32, polynomial=0x04C11DB7, init=0x00000000, xorout=0xFFFFFFFF, refin=false, refout=false, check=0x765E7680 },
            new TestVector () { name = "JAMCRC", width=32, polynomial=0x04C11DB7, init=0xFFFFFFFF, xorout=0x00000000, refin=true, refout=true, check=0x340BC6D9 },
            new TestVector () { name = "CRC-32C", width=32, polynomial=0x1EDC6F41, init=0xFFFFFFFF, xorout=0xFFFFFFFF, refin=true, refout=true, check=0xE3069283 },
            new TestVector () { name = "CRC-32D", width=32, polynomial=0xA833982B, init=0xFFFFFFFF, xorout=0xFFFFFFFF, refin=true, refout=true, check=0x87315576 },
            new TestVector () { name = "CRC-32Q", width=32, polynomial=0x814141AB, init=0x00000000, xorout=0x00000000, refin=false, refout=false, check=0x3010BF7F },
            new TestVector () { name = "XFER", width=32, polynomial=0x000000AF, init=0x00000000, xorout=0x00000000, refin=false, refout=false, check=0xBD0BE338 },
            new TestVector () { name = "CRC-40/GSM", width=40, polynomial=0x0004820009, init=0x0000000000, xorout=0x0000000000, refin=false, refout=false, check=0x2BE9B039B9 },
            new TestVector () { name = "CRC-64", width=64, polynomial=0x42F0E1EBA9EA3693, init=0x0000000000000000, xorout=0x0000000000000000, refin=false, refout=false, check=0x6C40DF5F0B497347 },
            new TestVector () { name = "CRC-64/WE", width=64, polynomial=0x42F0E1EBA9EA3693, init=0xFFFFFFFFFFFFFFFF, xorout=0xFFFFFFFFFFFFFFFF, refin=false, refout=false, check=0x62EC59E3F1A4F00A },
            new TestVector () { name = "CRC-64/1B", width=64, polynomial=0x000000000000001B, init=0x0000000000000000, xorout=0x0000000000000000, refin=true, refout=true, check=0x46A5A9388A5BEFFE },
            new TestVector () { name = "CRC-64/Jones", width=64, polynomial=0xAD93D23594C935A9, init=0xFFFFFFFFFFFFFFFF, xorout=0x0000000000000000, refin=true, refout=true, check=0xCAA717168609F281 }
        };

    }
}