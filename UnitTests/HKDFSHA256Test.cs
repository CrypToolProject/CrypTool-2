using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Numerics;
using System.Threading;

namespace UnitTests
{

    /// <summary>
    /// TestClass for HKDFSHA256
    /// </summary>
    [TestClass]
    public class HKDFSHA256Test
    {
        /// <summary>
        /// defines the structure of the test vectors
        /// </summary>
        private struct TestVector
        {
            public string SKM, CTXInfo, Salt, KeyMaterial;
            public BigInteger OutputBytes;
            public int n;
        }

        private byte[] outputData = null;
        private CrypTool.Plugins.HKDFSHA256.HKDFSHA256 pluginInstance;

        /// <summary>
        /// The method contains the unittests for the RFC implementation of HKDFSHA256
        /// </summary>
        [TestMethod]
        public void HKDFSHA256TestMethod_RFC_8bCTR()
        {
            pluginInstance = (CrypTool.Plugins.HKDFSHA256.HKDFSHA256)TestHelpers.GetPluginInstance("HKDFSHA256");
            pluginInstance.PropertyChanged += PluginInstance_PropertyChanged;
            PluginTestScenario scenario = new PluginTestScenario(pluginInstance, new[] { "SKM", "CTXInfo", "Salt", "OutputBytes", ".InfinityOutput", ".DisplayPres" }, new[] { "KeyMaterial" });
            object[] output;

            //loop over each test vector and test it
            foreach (TestVector vector in testvectors_RFC)
            {
                DateTime startTime = DateTime.Now;
                outputData = null;
                output = scenario.GetOutputs(new object[] { vector.SKM.ToLower().HexToByteArray(), vector.CTXInfo.ToLower().HexToByteArray(), vector.Salt.ToLower().HexToByteArray(), vector.OutputBytes, true, false });
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
        /// The method contains the unittests for the implementation with 32bit counter of HKDFSHA256
        /// </summary>
        [TestMethod]
        public void HKDFSHA256TestMethod_nonRFC_32bCTR()
        {
            pluginInstance = (CrypTool.Plugins.HKDFSHA256.HKDFSHA256)TestHelpers.GetPluginInstance("HKDFSHA256");
            pluginInstance.PropertyChanged += PluginInstance_PropertyChanged;
            PluginTestScenario scenario = new PluginTestScenario(pluginInstance, new[] { "SKM", "CTXInfo", "Salt", "OutputBytes", ".InfinityOutput", ".DisplayPres" }, new[] { "KeyMaterial" });
            object[] output;

            foreach (TestVector vector in testvectors_nonRFC)
            {
                DateTime startTime = DateTime.Now;
                outputData = null;
                output = scenario.GetOutputs(new object[] { vector.SKM.ToLower().HexToByteArray(), vector.CTXInfo.ToLower().HexToByteArray(), vector.Salt.ToLower().HexToByteArray(), vector.OutputBytes, false, false });

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

        //
        // Sources of the test vectors:
        //  https://tools.ietf.org/html/rfc5869
        //
        private readonly TestVector[] testvectors_RFC = new TestVector[] {
            new TestVector () { n=0, SKM="0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b", CTXInfo="f0f1f2f3f4f5f6f7f8f9", Salt="000102030405060708090a0b0c", KeyMaterial="3cb25f25faacd57a90434f64d0362f2a2d2d0a90cf1a5a4c5db02d56ecc4c5bf34007208d5b887185865", OutputBytes=42 },
            new TestVector () { n=1, SKM="000102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f202122232425262728292a2b2c2d2e2f303132333435363738393a3b3c3d3e3f404142434445464748494a4b4c4d4e4f", CTXInfo="b0b1b2b3b4b5b6b7b8b9babbbcbdbebfc0c1c2c3c4c5c6c7c8c9cacbcccdcecfd0d1d2d3d4d5d6d7d8d9dadbdcdddedfe0e1e2e3e4e5e6e7e8e9eaebecedeeeff0f1f2f3f4f5f6f7f8f9fafbfcfdfeff", Salt="606162636465666768696a6b6c6d6e6f707172737475767778797a7b7c7d7e7f808182838485868788898a8b8c8d8e8f909192939495969798999a9b9c9d9e9fa0a1a2a3a4a5a6a7a8a9aaabacadaeaf", KeyMaterial="b11e398dc80327a1c8e7f78c596a49344f012eda2d4efad8a050cc4c19afa97c59045a99cac7827271cb41c65e590e09da3275600c2f09b8367793a9aca3db71cc30c58179ec3e87c14c01d5c1f3434f1d87", OutputBytes=82 },
            new TestVector () { n=2, SKM="0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b", CTXInfo="", Salt="", KeyMaterial="8da4e775a563c18f715f802a063c5a31b8a11f5c5ee1879ec3454e5f3c738d2d9d201395faa4b61a96c8", OutputBytes=42 }
        };

        // <summary>
        // Sources of the test vectors:
        //  https://tools.ietf.org/html/rfc5869
        // </summary>
        private readonly TestVector[] testvectors_nonRFC = new TestVector[] {
            new TestVector () { n=0, SKM="0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b", CTXInfo="f0f1f2f3f4f5f6f7f8f9", Salt="000102030405060708090a0b0c", KeyMaterial="079c2e4b5552c1f0c77e879ec340c9e5f9b2a41b9d440907a2a726d09a2c30d25742dc02624cbaddd3e8", OutputBytes=42 },
            new TestVector () { n=1, SKM="000102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f202122232425262728292a2b2c2d2e2f303132333435363738393a3b3c3d3e3f404142434445464748494a4b4c4d4e4f", CTXInfo="b0b1b2b3b4b5b6b7b8b9babbbcbdbebfc0c1c2c3c4c5c6c7c8c9cacbcccdcecfd0d1d2d3d4d5d6d7d8d9dadbdcdddedfe0e1e2e3e4e5e6e7e8e9eaebecedeeeff0f1f2f3f4f5f6f7f8f9fafbfcfdfeff", Salt="606162636465666768696a6b6c6d6e6f707172737475767778797a7b7c7d7e7f808182838485868788898a8b8c8d8e8f909192939495969798999a9b9c9d9e9fa0a1a2a3a4a5a6a7a8a9aaabacadaeaf", KeyMaterial="dd7dde978a8f78ea872654841c19c444c18ac64ab7efc70b4f4e4bbffdab0eb7fab1ef47716c5fa91451fc764c61ec5f84010ba092d9df0aba67ea12b69966393e92ef2c90249a3f19dda1956fd71d55c86c", OutputBytes=82 },
            new TestVector () { n=2, SKM="0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b", CTXInfo="", Salt="", KeyMaterial="28febe6314bb794bc4f622f0b90f3842131c60c73a2dd93980eec0ba9e44872529befcc6f36f360a3044", OutputBytes=42 }
        };
    }
}