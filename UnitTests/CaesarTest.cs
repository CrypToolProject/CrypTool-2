using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class CaesarTest
    {
        public CaesarTest()
        {
        }

        [TestMethod]
        public void CaesarTestMethod()
        {
            CrypTool.PluginBase.ICrypComponent pluginInstance = TestHelpers.GetPluginInstance("Caesar");
            PluginTestScenario scenario = new PluginTestScenario(pluginInstance, new[] { "InputString", "AlphabetSymbols", "ShiftKey", ".CaseSensitive" }, new[] { "OutputString" });
            object[] output;

            foreach (TestVector vector in testvectors)
            {
                output = scenario.GetOutputs(new object[] { vector.input, vector.alphabet, vector.key, vector.sensitivity });
                Assert.AreEqual(vector.output, (string)output[0], "Unexpected value in test #" + vector.n + ".");
            }

        }

        private struct TestVector
        {
            public string input, output, alphabet;
            public int n, key;
            public bool sensitivity;
        }

        //
        // Sources of the test vectors:
        //  http://en.wikipedia.org/wiki/Caesar_cipher
        //  CrypTool1-Testvectors
        //
        private readonly TestVector[] testvectors = new TestVector[] {
            new TestVector () { n=0, key=24, alphabet="ABCDEFGHIJKLMNOPQRSTUVWXYZ", input="Franz jagt im komplett verwahrlosten Taxi quer durch Bayern", output="Dpylx hyer gk imknjcrr tcpuyfpjmqrcl Ryvg oscp bspaf Zywcpl".ToUpper(), sensitivity=false },
            new TestVector () { n=1, key=24, alphabet="ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", input="Franz jagt im komplett verwahrlosten Taxi quer durch Bayern", output="dPyLX HyER GK IMKNJCRR TCPUyFPJMQRCL ryVG OSCP BSPAF ZyWCPL", sensitivity=true },
            new TestVector () { n=2, key=3, alphabet="ABCDEFGHIJKLMNOPQRSTUVWXYZ", input="THE QUICK BROWN FOX JUMPS OVER THE LAZY DOG", output="WKH TXLFN EURZQ IRA MXPSV RYHU WKH ODCB GRJ", sensitivity=true },
        };

    }
}