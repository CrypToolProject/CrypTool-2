using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Numerics;
using System.Threading;

namespace UnitTests
{
    [TestClass]
    public class KKDFSHAKE256
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
        private CrypTool.Plugins.KKDFSHAKE256.KKDFSHAKE256 pluginInstance;

        /// <summary>
        /// The method contains the unittests
        /// </summary>
        [TestMethod]
        public void KKDFSHAKE256TestMethod()
        {
            pluginInstance = (CrypTool.Plugins.KKDFSHAKE256.KKDFSHAKE256)TestHelpers.GetPluginInstance("KKDFSHAKE256");
            pluginInstance.PropertyChanged += PluginInstance_PropertyChanged;
            PluginTestScenario scenario = new PluginTestScenario(pluginInstance, new[] { "SKM", "Key", "OutputBytes", ".DisplayPres" }, new[] { "KeyMaterial" });
            object[] output;

            //loop over each test vector and test it
            foreach (TestVector vector in testvectors)
            {
                DateTime startTime = DateTime.Now;
                outputData = null;
                output = scenario.GetOutputs(new object[] { vector.SKM.ToLower().HexToByteArray(), vector.Key.ToLower().HexToByteArray(), vector.OutputBytes, false });

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
        //  https://csrc.nist.gov/CSRC/media/Projects/Cryptographic-Algorithm-Validation-Program/documents/sha3/shakebytetestvectors.zip
        // </summary>
        private readonly TestVector[] testvectors = new TestVector[] {
            new TestVector () { n=0, SKM="445b17ce13727ae842b877c4750611a9eb79823bc5752da0a5e9d4e27bd40b94", Key="", KeyMaterial="e7708cdc22f03b0bfaca03e5d11d46cac118fded60b64bf4acffb35b0b474fbe85d270e625b95d54157d6597eb4fbdfa482e636d4a44", OutputBytes=54 },
            new TestVector () { n=1, SKM="a9eb79823bc5752da0a5e9d4e27bd40b94", Key="445b17ce13727ae842b877c4750611", KeyMaterial="e7708cdc22f03b0bfaca03e5d11d46cac118fded60b64bf4acffb35b0b474fbe85d270e625b95d54157d6597eb4fbdfa482e636d4a44", OutputBytes=54 },
            new TestVector () { n=2, SKM="e3ef127eadfafaf40408cebb28705df30b68d99dfa1893507ef3062d85461715", Key="", KeyMaterial="7314002948c057006d4fc21e3e19c258fb5bdd57728fe93c9c6ef265b6d9f559ca73da32c427e135ba0db900d9003b19c9cf116f542a760418b1a435ac75ed5ab4ef151808c3849c3bce11c3cd285dd75e5c9fd0a0b32a89640a68e6e5b270f966f33911cfdffd03488b52b4c7fd1b2219de133e77519c426a63b9d8afac2ccab273ebd23765616b04446d6ac403f46ac0c147eda629eb7583c8bd00dc7c30fcd6711b36f99f80ac94b683ebb090581970ae7e696c4c0afa9b5dafe07d1ab80877cbd09b705a0147d62d72a506732459a54142a0892c", OutputBytes=214 },
            new TestVector () { n=3, SKM="bb28705df30b68d99dfa1893507ef3062d85461715", Key="e3ef127eadfafaf40408ce", KeyMaterial="7314002948c057006d4fc21e3e19c258fb5bdd57728fe93c9c6ef265b6d9f559ca73da32c427e135ba0db900d9003b19c9cf116f542a760418b1a435ac75ed5ab4ef151808c3849c3bce11c3cd285dd75e5c9fd0a0b32a89640a68e6e5b270f966f33911cfdffd03488b52b4c7fd1b2219de133e77519c426a63b9d8afac2ccab273ebd23765616b04446d6ac403f46ac0c147eda629eb7583c8bd00dc7c30fcd6711b36f99f80ac94b683ebb090581970ae7e696c4c0afa9b5dafe07d1ab80877cbd09b705a0147d62d72a506732459a54142a0892c", OutputBytes=214 }
        };
    }
}