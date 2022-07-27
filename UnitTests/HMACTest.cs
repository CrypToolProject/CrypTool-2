using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class HMACTest
    {
        public HMACTest()
        {
        }

        [TestMethod]
        public void HMACTestMethod()
        {
            CrypTool.PluginBase.ICrypComponent pluginInstance = TestHelpers.GetPluginInstance("HMAC");
            PluginTestScenario[] scenarios = new PluginTestScenario[] {
                new PluginTestScenario(pluginInstance, new[] { "InputData", "Key", ".SelectedHashFunction" }, new[] { "OutputData" }),
                new PluginTestScenario(pluginInstance, new[] { "InputData", "Key", ".SelectedHashFunction" }, new[] { "OutputDataStream" })
            };

            object[] output;

            foreach (PluginTestScenario scenario in scenarios)
            {
                foreach (TestVector vector in testvectors1)
                {
                    output = scenario.GetOutputs(new object[] { vector.input.ToStream(), vector.key.ToByteArray(), vector.mode });
                    Assert.AreEqual(vector.output.ToUpper(), output[0].ToHex(), "Unexpected value in test #" + vector.n + ".");
                }

                foreach (TestVector vector in testvectors2)
                {
                    output = scenario.GetOutputs(new object[] { vector.input.ToStream(), vector.key.HexToByteArray(), vector.mode });
                    Assert.AreEqual(vector.output.ToUpper(), output[0].ToHex(), "Unexpected value in test #" + vector.n + ".");
                }

                foreach (TestVector vector in testvectors3)
                {
                    output = scenario.GetOutputs(new object[] { vector.input.HexToStream(), vector.key.HexToByteArray(), vector.mode });
                    Assert.AreEqual(vector.output.ToUpper(), output[0].ToHex(), "Unexpected value in test #" + vector.n + ".");
                }
            }
        }

        private struct TestVector
        {
            public string key, input, output;
            public int n, mode;
        }

        //
        // Source of the test vectors: http://en.wikipedia.org/wiki/HMAC
        //
        private readonly TestVector[] testvectors1 = new TestVector[] {
            new TestVector () { n=0, mode=0, key="", input="", output="74e6f7298a9c2d168935f58c001bad88" },   // MD5
            new TestVector () { n=1, mode=2, key="", input="", output="fbdb1d1b18aa6c08324b7d64b71fb76370690e1d" },   // SHA1
            new TestVector () { n=2, mode=3, key="", input="", output="b613679a0814d9ec772f95d778c35fc5ff1697c493715653c6c712144292c5ad" },   // SHA256  
            new TestVector () { n=3, mode=0, key="key", input="The quick brown fox jumps over the lazy dog", output="80070713463e7749b90c2dc24911e275" },   // MD5
            new TestVector () { n=4, mode=2, key="key", input="The quick brown fox jumps over the lazy dog", output="de7c9b85b8b78aa6bc8a7a36f70a90701c9db4d9" },   // SHA1
            new TestVector () { n=5, mode=3, key="key", input="The quick brown fox jumps over the lazy dog", output="f7bc83f430538424b13298e6aa6fb143ef4d59a14946175997479dbc2d1a3cd8" },   // SHA256
        };

        //
        // Source of the test vectors: http://csrc.nist.gov/publications/fips/fips198/fips-198a.pdf
        //
        private readonly TestVector[] testvectors2 = new TestVector[] {
            new TestVector () { n=6, mode=2, key="000102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f202122232425262728292a2b2c2d2e2f303132333435363738393a3b3c3d3e3f", input="Sample #1", output="4f4ca3d5d68ba7cc0a1208c9c61e9c5da0403c0a" },   // SHA1
            new TestVector () { n=7, mode=2, key="303132333435363738393a3b3c3d3e3f40414243", input="Sample #2", output="0922d3405faa3d194f82a45830737d5cc6c75d24" },   // SHA1
            new TestVector () { n=8, mode=2, key="505152535455565758595a5b5c5d5e5f606162636465666768696a6b6c6d6e6f707172737475767778797a7b7c7d7e7f808182838485868788898a8b8c8d8e8f909192939495969798999a9b9c9d9e9fa0a1a2a3a4a5a6a7a8a9aaabacadaeafb0b1b2b3", input="Sample #3", output="bcf41eab8bb2d802f3d05caf7cb092ecf8d1a3aa" },   // SHA1
            new TestVector () { n=9, mode=2, key="707172737475767778797a7b7c7d7e7f808182838485868788898a8b8c8d8e8f909192939495969798999a9b9c9d9e9fa0", input="Sample #4", output="9ea886efe268dbecce420c7524df32e0751a2a26" },   // SHA1
        };

        //
        // Source of the test vectors: http://tools.ietf.org/html/rfc2104
        //
        private readonly TestVector[] testvectors3 = new TestVector[] {
            new TestVector () { n=10, mode=0, key="0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b", input="Hi There".ToHex(), output="9294727a3638bb1c13f48ef8158bfc9d" },   // MD5
            new TestVector () { n=11, mode=0, key="Jefe".ToHex(), input="what do ya want for nothing?".ToHex(), output="750c783e6ab0b503eaa86e310a5db738" },   // MD5
            new TestVector () { n=12, mode=0, key="AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", input="DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD", output="56be34521d144c88dbb8c733f0e8b3f6" },   // MD5
        };
    }
}
