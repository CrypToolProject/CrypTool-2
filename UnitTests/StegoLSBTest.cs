using Microsoft.VisualStudio.TestTools.UnitTesting;
using CrypTool.PluginBase.IO;

namespace Tests.TemplateAndPluginTests
{
    [TestClass]
    public class StegoLSBTest
    {
        public StegoLSBTest()
        {
        }

        [TestMethod]
        public void StegoLSBTestMethod()
        {
            var pluginInstance = TestHelpers.GetPluginInstance("StegoLeastSignificantBit");
            var scenario = new PluginTestScenario(pluginInstance, new[] { "InputData", "InputCarrier", "InputPassword", ".Action", ".CustomizeRegions", ".OutputFileFormat" }, new[] { "OutputData", "OutputCarrier" });

            CStreamWriter carrierImage = new CStreamWriter("Templates\\Steganography\\CrypTool2.jpg", true);

            foreach (TestVector vector in testvectors)
            {
                object[] output1 = scenario.GetOutputs(new object[] { vector.message.ToStream(), carrierImage, vector.password.ToStream(), 0, false, 1 });
                object[] output2 = scenario.GetOutputs(new object[] { null, (ICrypToolStream)output1[1], vector.password.ToStream(), 1, false, 1 });
                // check if hiding and extracting the secret message leaves it unchanged
                Assert.AreEqual(vector.message, ((ICrypToolStream)output2[0]).ToByteArray().ToString2(), "Unexpected value in test #" + vector.n + ".");
            }
        }

        struct TestVector
        {
            public int n;
            public string message, password;
        }

        TestVector[] testvectors = new TestVector[] {
               new TestVector () { n=1, message="The quick brown fox jumps over the lazy dog.", password="12345678" },
               new TestVector () { n=2, message="Dies ist ein längerer Text mit einigen Sonderzeichen: äöüÄÖÜßé.", password="äöüabc" },
        };

    }
}