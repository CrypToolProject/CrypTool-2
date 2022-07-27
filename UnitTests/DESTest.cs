using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class DESTest
    {
        public DESTest()
        {
        }

        [TestMethod]
        public void DESTestMethod()
        {
            CrypTool.PluginBase.ICrypComponent pluginInstance = TestHelpers.GetPluginInstance("DES");
            PluginTestScenario scenario = new PluginTestScenario(pluginInstance, new[] { "InputStream", "InputIV", "InputKey", ".Action", ".Mode", ".Padding", ".TripleDES" }, new[] { "OutputStream" });

            foreach (TestVector vector in testvectors)
            {
                object[] output = scenario.GetOutputs(new object[] { vector.input.HexToStream(), vector.IV.HexToByteArray(), vector.key.HexToByteArray(), 0, 0, 0, vector.tdes });
                Assert.AreEqual(vector.output.ToUpper(), output[0].ToHex(), "Unexpected value in test #" + vector.n + ".");
            }
        }

        private struct TestVector
        {
            public string key, IV, input, output;
            public int n, mode;
            public bool tdes;
        }

        //
        // Source of the test vectors: http://caffeine-hx.googlecode.com/svn-history/r622/trunk/ext3/Tests/crypt/src/DESKeyTest.hx
        //
        private readonly TestVector[] testvectors = new TestVector[] {
            new TestVector () { n=0, mode=0, key="3b3898371520f75e", IV="0000000000000000", input="0000000000000000", output="83a1e814889253e0", tdes=false },
            new TestVector () { n=1, mode=0, key="3b3898371520f75e", IV="0000000000000000", input="6161616161616161", output="7459b5fa0741c905", tdes=false },
            //new TestVector () { n=2, mode=0, key="0101010101010101", IV="0000000000000000", input="0000000000000000", output="95f8a5e5dd31d900", tdes=false },    // weak key
            //new TestVector () { n=3, mode=0, key="0101010101010101", IV="0000000000000000", input="0000000000000000", output="dd7f121ca5015619", tdes=false },    // weak key
            new TestVector () { n=4, mode=0, key="8001010101010101", IV="0000000000000000", input="0000000000000000", output="95a8d72813daa94d", tdes=false },

            new TestVector () { n=5, mode=0, key="3b3898371520f75e", IV="0000000000000000", input="0000000000000000", output="83A1E814889253E0", tdes=false },
            new TestVector () { n=6, mode=0, key="10316E028C8F3B4A", IV="0000000000000000", input="0000000000000000", output="82DCBAFBDEAB6602", tdes=false },
            //new TestVector () { n=7, mode=0, key="0101010101010101", IV="0000000000000000", input="8000000000000000", output="95F8A5E5DD31D900", tdes=false },    // weak key
            //new TestVector () { n=8, mode=0, key="0101010101010101", IV="0000000000000000", input="4000000000000000", output="DD7F121CA5015619", tdes=false },    // weak key
            //new TestVector () { n=9, mode=0, key="0101010101010101", IV="0000000000000000", input="2000000000000000", output="2E8653104F3834EA", tdes=false },    // weak key
            //new TestVector () { n=10, mode=0, key="0101010101010101", IV="0000000000000000", input="1000000000000000", output="4BD388FF6CD81D4F", tdes=false },    // weak key
            //new TestVector () { n=11, mode=0, key="0101010101010101", IV="0000000000000000", input="0800000000000000", output="20B9E767B2FB1456", tdes=false },    // weak key
            //new TestVector () { n=12, mode=0, key="0101010101010101", IV="0000000000000000", input="0400000000000000", output="55579380D77138EF", tdes=false },    // weak key
            //new TestVector () { n=13, mode=0, key="0101010101010101", IV="0000000000000000", input="0200000000000000", output="6CC5DEFAAF04512F", tdes=false },    // weak key
            //new TestVector () { n=14, mode=0, key="0101010101010101", IV="0000000000000000", input="0100000000000000", output="0D9F279BA5D87260", tdes=false },    // weak key
            //new TestVector () { n=15, mode=0, key="0101010101010101", IV="0000000000000000", input="0000000000000000", output="D9031B0271BD5A0A", tdes=false },    // weak key
            new TestVector () { n=16, mode=0, key="8001010101010101", IV="0000000000000000", input="0000000000000000", output="95A8D72813DAA94D", tdes=false },
            new TestVector () { n=17, mode=0, key="4001010101010101", IV="0000000000000000", input="0000000000000000", output="0EEC1487DD8C26D5", tdes=false },
            new TestVector () { n=18, mode=0, key="2001010101010101", IV="0000000000000000", input="0000000000000000", output="7AD16FFB79C45926", tdes=false },
            new TestVector () { n=19, mode=0, key="1001010101010101", IV="0000000000000000", input="0000000000000000", output="D3746294CA6A6CF3", tdes=false },
            new TestVector () { n=20, mode=0, key="0801010101010101", IV="0000000000000000", input="0000000000000000", output="809F5F873C1FD761", tdes=false },
            new TestVector () { n=21, mode=0, key="0401010101010101", IV="0000000000000000", input="0000000000000000", output="C02FAFFEC989D1FC", tdes=false },
            new TestVector () { n=22, mode=0, key="0201010101010101", IV="0000000000000000", input="0000000000000000", output="4615AA1D33E72F10", tdes=false },
            new TestVector () { n=23, mode=0, key="0180010101010101", IV="0000000000000000", input="0000000000000000", output="2055123350C00858", tdes=false },
            new TestVector () { n=24, mode=0, key="0140010101010101", IV="0000000000000000", input="0000000000000000", output="DF3B99D6577397C8", tdes=false },

            new TestVector () { n=25, mode=0, key="3b3898371520f75e922fb510c71f436e", IV="0000000000000000", input="0000000000000000", output="3CF6606A0E62731A", tdes=true },
            new TestVector () { n=26, mode=0, key="3b3898371520f75e922fb510c71f436e", IV="0000000000000000", input="6161616161616161", output="0837d02f212b3d7f", tdes=true },
        };
    }
}