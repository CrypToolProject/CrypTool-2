using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class PRESENTTest
    {
        public PRESENTTest()
        {
        }

        [TestMethod]
        public void PRESENTTestMethod()
        {
            CrypTool.PluginBase.ICrypComponent pluginInstance = TestHelpers.GetPluginInstance("PRESENT");
            PluginTestScenario scenario = new PluginTestScenario(pluginInstance, new[] { "InputStream", "InputKey", ".Action", ".Mode", ".Padding" }, new[] { "OutputStream" });
            object[] output;

            foreach (TestVector vector in testvectors)
            {
                output = scenario.GetOutputs(new object[] { vector.input.HexToStream(), vector.key.HexToByteArray(), 0, 0, 0 });
                Assert.AreEqual(vector.output.ToUpper(), output[0].ToHex(), "Unexpected value in test #" + vector.n + ".");
            }

        }

        private struct TestVector
        {
            public string input, key, output;
            public int n;
        }

        //
        // Source of the test vectors: http://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.80.8447&rep=rep1&type=pdf
        //
        private readonly TestVector[] testvectors = new TestVector[] {
            new TestVector () { n=0, key="00000000000000000000", input="0000000000000000", output="5579C1387B228445" },
            new TestVector () { n=1, key="FFFFFFFFFFFFFFFFFFFF", input="0000000000000000", output="E72C46C0F5945049" },
            new TestVector () { n=2, key="00000000000000000000", input="FFFFFFFFFFFFFFFF", output="A112FFC72F68417B" },
            new TestVector () { n=3, key="FFFFFFFFFFFFFFFFFFFF", input="FFFFFFFFFFFFFFFF", output="3333DCD3213210D2" },
        };


    }
}