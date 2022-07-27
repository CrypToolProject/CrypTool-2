using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class RIPEMD160Test
    {
        public RIPEMD160Test()
        {
        }

        [TestMethod]
        public void RIPEMD160TestMethod()
        {
            CrypTool.PluginBase.ICrypComponent pluginInstance = TestHelpers.GetPluginInstance("RIPEMD160");
            PluginTestScenario scenario = new PluginTestScenario(pluginInstance, new[] { "InputData" }, new[] { "OutputData" });

            foreach (TestVector vector in testvectors)
            {
                object[] output = scenario.GetOutputs(new object[] { vector.input.ToStream() });
                Assert.AreEqual(vector.output.ToUpper(), output[0].ToHex(), "Unexpected value in test #" + vector.n + ".");
            }
        }

        private struct TestVector
        {
            public string input, output;
            public int n;
        }

        //
        // Source of the test vectors: https://www.cosic.esat.kuleuven.be/nessie/testvectors/hash/ripemd-160/Ripemd-160.test-vectors
        //
        private readonly TestVector[] testvectors = new TestVector[] {
            new TestVector () { n=0, output="9c1185a5c5e9fc54612808977ee8f548b2258d31", input="" },
            new TestVector () { n=1, output="0bdc9d2d256b3ee9daae347be6f4dc835a467ffe", input="a" },
            new TestVector () { n=2, output="8eb208f7e05d987a9b044a8e98c6b087f15a0bfc", input="abc" },
            new TestVector () { n=3, output="5d0689ef49d2fae572b881b123a85ffa21595f36", input="message digest" },
            new TestVector () { n=4, output="f71c27109c692c1b56bbdceb5b9d2865b3708dbc", input="abcdefghijklmnopqrstuvwxyz" },
            new TestVector () { n=5, output="12a053384a9c0c88e405a06c27dcf49ada62eb2b", input="abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq" },
            new TestVector () { n=6, output="b0e20b6e3116640286ed3a87a5713079b21f5189", input="ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789" },
            new TestVector () { n=7, output="9b752e45573d4b39f4dbd3323cab82bf63326bfb", input="12345678901234567890123456789012345678901234567890123456789012345678901234567890" }
        };

    }
}