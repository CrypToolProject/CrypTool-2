using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CrypTool.PluginBase.IO;
using CrypTool.Plugins.Cryptography.Encryption;

namespace Tests.TemplateAndPluginTests
{
    [TestClass]
    public class DESVisualizationTest
    {
        public DESVisualizationTest()
        {
        }

        [TestMethod]
        public void DESVisualizationTestMethod()
        {
            var pluginInstance = TestHelpers.GetPluginInstance("DESVisualization");
            var scenario = new PluginTestScenario(pluginInstance, new[] { "Text", "Key" }, new[] { "Ciphertext" });

            foreach (TestVector vector in testvectors)
            {
                object[] output = scenario.GetOutputs(new object[] { vector.text.Replace(" ","").HexToByteArray(), vector.key.Replace(" ", "").HexToByteArray() });
                Assert.AreEqual(vector.output.Replace(" ", ""), output[0].ToHex(), "Unexpected value in test #" + vector.n + ".");
            }
        }

        struct TestVector
        {
            public string key, text, output;
            public int n;
        }

        //
        // Source of the test vectors: http://csrc.nist.gov/publications/nistpubs/800-17/800-17.pdf
        //
        TestVector[] testvectors = new TestVector[] {

            //Appendix B Table 1 Round 0 to 13
            //Values for the Known Answer Test (#0xx)
            new TestVector () { n=0, key = "01 01 01 01 01 01 01 01", text = "80 00 00 00 00 00 00 00", output = "95 F8 A5 E5 DD 31 D9 00"},
            new TestVector () { n=1, key = "01 01 01 01 01 01 01 01", text = "40 00 00 00 00 00 00 00", output = "DD 7F 12 1C A5 01 56 19"},
            new TestVector () { n=2, key = "01 01 01 01 01 01 01 01", text = "20 00 00 00 00 00 00 00", output = "2E 86 53 10 4F 38 34 EA"},
            new TestVector () { n=3, key = "01 01 01 01 01 01 01 01", text = "10 00 00 00 00 00 00 00", output = "4B D3 88 FF 6C D8 1D 4F"},
            new TestVector () { n=4, key = "01 01 01 01 01 01 01 01", text = "08 00 00 00 00 00 00 00", output = "20 B9 E7 67 B2 FB 14 56"},
            new TestVector () { n=5, key = "01 01 01 01 01 01 01 01", text = "04 00 00 00 00 00 00 00", output = "55 57 93 80 D7 71 38 EF"},
            new TestVector () { n=6, key = "01 01 01 01 01 01 01 01", text = "02 00 00 00 00 00 00 00", output = "6C C5 DE FA AF 04 51 2F"},
            new TestVector () { n=7, key = "01 01 01 01 01 01 01 01", text = "01 00 00 00 00 00 00 00", output = "0D 9F 27 9B A5 D8 72 60"},
            new TestVector () { n=8, key = "01 01 01 01 01 01 01 01", text = "00 80 00 00 00 00 00 00", output = "D9 03 1B 02 71 BD 5A 0A"},
            new TestVector () { n=9, key = "01 01 01 01 01 01 01 01", text = "00 40 00 00 00 00 00 00", output = "42 42 50 B3 7C 3D D9 51"},
            new TestVector () { n=10, key = "01 01 01 01 01 01 01 01", text = "00 20 00 00 00 00 00 00", output = "B8 06 1B 7E CD 9A 21 E5"},
            new TestVector () { n=11, key = "01 01 01 01 01 01 01 01", text = "00 10 00 00 00 00 00 00", output = "F1 5D 0F 28 6B 65 BD 28"},
            new TestVector () { n=12, key = "01 01 01 01 01 01 01 01", text = "00 08 00 00 00 00 00 00", output = "AD D0 CC 8D 6E 5D EB A1"},
            new TestVector () { n=13, key = "01 01 01 01 01 01 01 01", text = "00 04 00 00 00 00 00 00 ", output = "E6 D5 F8 27 52 AD 63 D1"},

            //Appendix B Table 2 Round 0 to 14
            //Values for the Variable Key Known Answer Test (#1xx)
            new TestVector () { n=100, key = "80 01 01 01 01 01 01 01", text = "00 00 00 00 00 00 00 00", output = "95 A8 D7 28 13 DA A9 4D"},
            new TestVector () { n=101, key = "40 01 01 01 01 01 01 01", text = "00 00 00 00 00 00 00 00", output = "0E EC 14 87 DD 8C 26 D5"},
            new TestVector () { n=102, key = "20 01 01 01 01 01 01 01", text = "00 00 00 00 00 00 00 00", output = "7A D1 6F FB 79 C4 59 26"},
            new TestVector () { n=103, key = "10 01 01 01 01 01 01 01", text = "00 00 00 00 00 00 00 00", output = "D3 74 62 94 CA 6A 6C F3"},
            new TestVector () { n=104, key = "08 01 01 01 01 01 01 01", text = "00 00 00 00 00 00 00 00", output = "80 9F 5F 87 3C 1F D7 61"},
            new TestVector () { n=105, key = "04 01 01 01 01 01 01 01", text = "00 00 00 00 00 00 00 00", output = "C0 2F AF FE C9 89 D1 FC"},
            new TestVector () { n=106, key = "02 01 01 01 01 01 01 01", text = "00 00 00 00 00 00 00 00", output = "46 15 AA 1D 33 E7 2F 10"},
            new TestVector () { n=107, key = "01 80 01 01 01 01 01 01", text = "00 00 00 00 00 00 00 00", output = "20 55 12 33 50 C0 08 58"},
            new TestVector () { n=108, key = "01 40 01 01 01 01 01 01", text = "00 00 00 00 00 00 00 00", output = "DF 3B 99 D6 57 73 97 C8"},
            new TestVector () { n=109, key = "01 20 01 01 01 01 01 01", text = "00 00 00 00 00 00 00 00", output = "31 FE 17 36 9B 52 88 C9"},
            new TestVector () { n=110, key = "01 10 01 01 01 01 01 01", text = "00 00 00 00 00 00 00 00", output = "DF DD 3C C6 4D AE 16 42"},
            new TestVector () { n=111, key = "01 08 01 01 01 01 01 01", text = "00 00 00 00 00 00 00 00", output = "17 8C 83 CE 2B 39 9D 94"},
            new TestVector () { n=112, key = "01 04 01 01 01 01 01 01", text = "00 00 00 00 00 00 00 00", output = "50 F6 36 32 4A 9B 7F 80"},
            new TestVector () { n=113, key = "01 02 01 01 01 01 01 01", text = "00 00 00 00 00 00 00 00", output = "A8 46 8E E3 BC 18 F0 6D"},
            new TestVector () { n=114, key = "01 01 80 01 01 01 01 01", text = "00 00 00 00 00 00 00 00", output = "A2 DC 9E 92 FD 3C DE 92"},

            //Appendix B Table 4 Round 0 to 18
            //Values for the  Substitution Table Known Answer Test (#2xx)
            new TestVector () { n=200, key = "7C A1 10 45 4A 1A 6E 57", text = "01 A1 D6 D0 39 77 67 42", output = "69 0F 5B 0D 9A 26 93 9B"},
            new TestVector () { n=201, key = "01 31 D9 61 9D C1 37 6E", text = "5C D5 4C A8 3D EF 57 DA", output = "7A 38 9D 10 35 4B D2 71"},
            new TestVector () { n=202, key = "07 A1 13 3E 4A 0B 26 86", text = "02 48 D4 38 06 F6 71 72", output = "86 8E BB 51 CA B4 59 9A"},
            new TestVector () { n=203, key = "38 49 67 4C 26 02 31 9E", text = "51 45 4B 58 2D DF 44 0A", output = "71 78 87 6E 01 F1 9B 2A"},
            new TestVector () { n=204, key = "04 B9 15 BA 43 FE B5 B6", text = "42 FD 44 30 59 57 7F A2", output = "AF 37 FB 42 1F 8C 40 95"},
            new TestVector () { n=205, key = "01 13 B9 70 FD 34 F2 CE", text = "05 9B 5E 08 51 CF 14 3A", output = "86 A5 60 F1 0E C6 D8 5B"},
            new TestVector () { n=206, key = "01 70 F1 75 46 8F B5 E6", text = "07 56 D8 E0 77 47 61 D2", output = "0C D3 DA 02 00 21 DC 09"},
            new TestVector () { n=207, key = "43 29 7F AD 38 E3 73 FE", text = "76 25 14 B8 29 BF 48 6A", output = "EA 67 6B 2C B7 DB 2B 7A"},
            new TestVector () { n=208, key = "07 A7 13 70 45 DA 2A 16", text = "3B DD 11 90 49 37 28 02", output = "DF D6 4A 81 5C AF 1A 0F"},
            new TestVector () { n=209, key = "04 68 91 04 C2 FD 3B 2F", text = "26 95 5F 68 35 AF 60 9A", output = "5C 51 3C 9C 48 86 C0 88"},
            new TestVector () { n=210, key = "37 D0 6B B5 16 CB 75 46", text = "16 4D 5E 40 4F 27 52 32", output = "0A 2A EE AE 3F F4 AB 77"},
            new TestVector () { n=211, key = "1F 08 26 0D 1A C2 46 5E", text = "6B 05 6E 18 75 9F 5C CA", output = "EF 1B F0 3E 5D FA 57 5A"},
            new TestVector () { n=212, key = "58 40 23 64 1A BA 61 76", text = "00 4B D6 EF 09 17 60 62", output = "88 BF 0D B6 D7 0D EE 56"},
            new TestVector () { n=213, key = "02 58 16 16 46 29 B0 07", text = "48 0D 39 00 6E E7 62 F2", output = "A1 F9 91 55 41 02 0B 56"},
            new TestVector () { n=214, key = "49 79 3E BC 79 B3 25 8F", text = "43 75 40 C8 69 8F 3C FA", output = "6F BF 1C AF CF FD 05 56"},
            new TestVector () { n=215, key = "4F B0 5E 15 15 AB 73 A7", text = "07 2D 43 A0 77 07 52 92", output = "2F 22 E4 9B AB 7C A1 AC"},
            new TestVector () { n=216, key = "49 E9 5D 6D 4C A2 29 BF", text = "02 FE 55 77 81 17 F1 2A", output = "5A 6B 61 2C C2 6C CE 4A"},
            new TestVector () { n=217, key = "01 83 10 DC 40 9B 26 D6", text = "1D 9D 5C 50 18 F7 28 C2", output = "5F 4C 03 8E D1 2B 2E 41"},
            new TestVector () { n=218, key = "1C 58 7F 1C 13 92 4F EF", text = "30 55 32 28 6D 6F 29 5A", output = "63 FA C0 D0 34 D9 F7 93"},
        };
    }
}
