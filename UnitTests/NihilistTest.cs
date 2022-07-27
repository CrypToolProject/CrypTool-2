using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class NihilistTest
    {
        public NihilistTest()
        {
        }

        [TestMethod]
        public void NihilistTestMethod()
        {
            CrypTool.PluginBase.ICrypComponent pluginInstance = TestHelpers.GetPluginInstance("Nihilist");
            PluginTestScenario scenario = new PluginTestScenario(pluginInstance, new[] { "Input", ".KeyWord", ".SecondKeyWord" }, new[] { "Output" });
            object[] output;

            foreach (TestVector vector in testvectors)
            {
                output = scenario.GetOutputs(new object[] { vector.input.ToByteArray(), vector.key1, vector.key2 });
                Assert.AreEqual(vector.output, ((byte[])output[0]).ToHex(), "Unexpected value in test #" + vector.n + ".");
            }

        }

        private struct TestVector
        {
            public string input, output, key1, key2;
            public int n;
        }

        //
        // Source of the test vectors: http://en.wikipedia.org/wiki/Nihilist_cipher
        //
        private readonly TestVector[] testvectors = new TestVector[] {
            new TestVector () { n=0, key1="ZEBRAS", key2="RUSSIAN", input="DYNAMITE WINTER PALACE", output="256A3E24432F561A68353E4D1B3739423724361B" },
        };

    }
}