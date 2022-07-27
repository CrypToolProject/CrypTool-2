using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class RC2Test
    {
        public RC2Test()
        {
        }

        [TestMethod]
        public void RC2TestMethod()
        {
            CrypTool.PluginBase.ICrypComponent pluginInstance = TestHelpers.GetPluginInstance("RC2");
            PluginTestScenario scenario = new PluginTestScenario(pluginInstance, new[] { "InputStream", "InputIV", "InputKey", ".Action", ".Padding" }, new[] { "OutputStream" });

            foreach (TestVector vector in testvectors)
            {
                object[] output = scenario.GetOutputs(new object[] { vector.input.HexToStream(), vector.IV.HexToByteArray(), vector.key.HexToByteArray(), vector.mode, 0 });
                Assert.AreEqual(vector.output.ToUpper(), output[0].ToHex(), "Unexpected value in test #" + vector.n + ".");
            }
        }

        private struct TestVector
        {
            public string key, IV, input, output;
            public int n, mode;
        }

        //
        // Source of the test vectors: http://tools.ietf.org/html/rfc2268
        //
        private readonly TestVector[] testvectors = new TestVector[] {
            new TestVector () { n=0, mode=0, key="0000000000000000", IV="0000000000000000", input="0000000000000000", output="ebb773f993278eff" },
            new TestVector () { n=1, mode=0, key="ffffffffffffffff", IV="0000000000000000", input="ffffffffffffffff", output="278b27e42e2f0d49" },
            new TestVector () { n=2, mode=0, key="3000000000000000", IV="0000000000000000", input="1000000000000001", output="30649edf9be7d2c2" }, 
            //new TestVector () { n=3, mode=0, key="88", IV="0000000000000000", input="0000000000000000", output="61a8a244adacccf0" }, // key too short
            //new TestVector () { n=4, mode=0, key="88bca90e90875a", IV="0000000000000000", input="0000000000000000", output="6ccf4308974c267f" }, 
            //new TestVector () { n=5, mode=0, key="88bca90e90875a7f0f79c384627bafb2", IV="0000000000000000", input="0000000000000000", output="1a807d272bbe5db1" }, 
            new TestVector () { n=6, mode=0, key="88bca90e90875a7f0f79c384627bafb2", IV="0000000000000000", input="0000000000000000", output="2269552ab0f85ca6" }, 
            //new TestVector () { n=7, mode=0, key="88bca90e90875a7f0f79c384627bafb216f80a6f85920584c42fceb0be255daf1e", IV="0000000000000000", input="0000000000000000", output="5b78d3a43dfff1f1" },
        };
    }
}