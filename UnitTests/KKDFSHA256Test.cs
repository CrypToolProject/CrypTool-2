using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Numerics;
using System.Threading;

namespace UnitTests
{

    /// <summary>
    /// TestClass for KKDFSHA256
    /// </summary>
    [TestClass]
    public class KKDFSHA256
    {
        /// <summary>
        /// defines the structure of the test vectors
        /// </summary>
        private struct TestVector
        {
            public string SKM, Key, KeyMaterial;
            public BigInteger OutputBytes;
            public int n;
        }

        private byte[] outputData = null;
        private CrypTool.Plugins.KKDFSHA256.KKDFSHA256 pluginInstance;

        /// <summary>
        /// The method contains the unittests
        /// </summary>
        [TestMethod]
        public void KKDFSHA256TestMethod()
        {
            pluginInstance = (CrypTool.Plugins.KKDFSHA256.KKDFSHA256)TestHelpers.GetPluginInstance("KKDFSHA256");
            pluginInstance.PropertyChanged += PluginInstance_PropertyChanged;
            PluginTestScenario scenario = new PluginTestScenario(pluginInstance, new[] { "SKM", "Key", "OutputBytes", ".DisplayPres", ".InfinityOutput" }, new[] { "KeyMaterial" });
            object[] output;

            //loop over each test vector and test it
            foreach (TestVector vector in testvectors)
            {
                DateTime startTime = DateTime.Now;
                outputData = null;
                output = scenario.GetOutputs(new object[] { vector.SKM.ToLower().HexToByteArray(), vector.Key.ToLower().HexToByteArray(), vector.OutputBytes, false, false });

                // the loop is needed, because the plugin calculates the output in an seperated worker-thread, which is triggered by the execute-method of the plugin
                do
                {
                    Thread.Sleep(1);
                    if (DateTime.Now > startTime.AddSeconds(60))
                    {
                        throw new Exception("TestCase #" + vector.n + " running to long. Aborted it.");
                    }
                } while (outputData == null);
                Assert.AreEqual(vector.KeyMaterial.ToLower(), outputData.ToHex().ToLower(), "Unexpected value in test #" + vector.n + ".");
            }
        }

        /// <summary>
        /// helper method to monitor when plugin has finished to work
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PluginInstance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("KeyMaterial"))
            {
                outputData = pluginInstance.KeyMaterial;
            }
        }

        // <summary>
        // Sources of the test vectors:
        //  https://csrc.nist.gov/CSRC/media/Projects/Cryptographic-Algorithm-Validation-Program/documents/shs/shabytetestvectors.zip
        // the nist testvectors are for testing the plain hashfunction sha256. keep in mind, that we use a key and a counter, which changes the input values of the hashfunction
        // </summary>
        private readonly TestVector[] testvectors = new TestVector[] {
            new TestVector () { n=0, SKM="6ac6c63d618eaf00d91c5e2807e83c093912b8e202f78e139703498a79c6067f54497c6127a23910a6", Key="", KeyMaterial="71c8448273772969c0f57667ce9016372a3f2d8c300eeeb6348d6a07e7b26c85", OutputBytes=32 },
            new TestVector () { n=1, SKM="6ac6c63d618eaf00d91c5e2807e83c093912b8e202f78e139703498a79c6067f54497c6127a23910a6", Key="", KeyMaterial="71c8448273772969c0f57667ce9016372a3f2d8c300eeeb6348d6a07e7b26c8573566f8b983b1559356a48648a175c291ad45afb56000dc2e412da043648e57c8c301507cd1927ef366642dbee4fd5a41ed3f4a24176d1f8f4d9040deadf42c0afc033c88a9902c1a57828aa3bdb7116e35a41dabc078d368a1cba18b8c7c8e1", OutputBytes=128 },
            new TestVector () { n=2, SKM="3d83df37172c81afd0de115139fbf4390c22e098c5af4c5ab4852406510bc0e6cf741769f44430c5270fdae0cb849d71cbab", Key="", KeyMaterial="137de2712ee468cac31088b950394c4ab3b6a14a9c15cdb9f35f46606a407b24", OutputBytes=32 },
            new TestVector () { n=3, SKM="5e2807e83c093912b8e202f78e139703498a79c6067f54497c6127a23910a6", Key="6ac6c63d618eaf00d91c", KeyMaterial="07ebe2e89a9a0a68b319082d7d40e12fa101c093b55a138a5b89e5dba6018764", OutputBytes=32 },
        };
    }
}