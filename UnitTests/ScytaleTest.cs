using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class ScytaleTest
    {
        public ScytaleTest()
        {
        }

        [TestMethod]
        public void ScytaleTestMethod()
        {
            CrypTool.PluginBase.ICrypComponent pluginInstance = TestHelpers.GetPluginInstance("Scytale");
            PluginTestScenario scenario = new PluginTestScenario(pluginInstance, new[] { "InputString", "StickSize", ".Action" }, new[] { "OutputString" });
            object[] output;

            foreach (TestVector vector in testvectors)
            {
                output = scenario.GetOutputs(new object[] { vector.input, vector.size, vector.action });
                Assert.AreEqual(vector.output, (string)output[0], "Unexpected value in test #" + vector.n + ".");
            }
        }

        private struct TestVector
        {
            public string input, output;
            public int size, action;
            public int n;
        }

        //
        // Source of the test vectors: http://en.wikipedia.org/wiki/Scytale, manually created test vector
        //
        private readonly TestVector[] testvectors = new TestVector[] {
            /*new TestVector () { n=0, action=0, size=4, input="HELPMEIAMUNDERATTACK", output="HENTEIDTLAEAPMRCMUAK" },
            new TestVector () { n=1, action=1, size=4, input="HENTEIDTLAEAPMRCMUAK", output="HELPMEIAMUNDERATTACK" },
            new TestVector () { n=2, action=0, size=4, input="Help me I am under attack", output="H nteIdal ecpark m _m a_eut_" },
            new TestVector () { n=3, action=1, size=4, input="H nteIdal ecpark m _m a_eut_", output="Help me I am under attack" },
            new TestVector () { n=4, action=0, size=7, input="The quick brown fox jumps over the lazy dog.", output="Tcnuelghk mra.e fp z_ bosty_qrx h _uo oed_iwjv o_" },
            new TestVector () { n=5, action=1, size=7, input="Tcnuelghk mra.e fp z_ bosty_qrx h _uo oed_iwjv o_", output="The quick brown fox jumps over the lazy dog." },*/
            //Scytale in CT1 and CT2.0 are incompatible, so such an unit test does not make any sense...
        };

    }
}