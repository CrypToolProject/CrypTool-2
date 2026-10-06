/*
   Copyright (C) CrypTool 2 Team

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;

namespace UnitTests
{
    [TestClass]
    public class CipherVectorCoverageTests
    {
        [TestMethod]
        public void BaconCipherUsesBinaryAlphabetCodes()
        {
            CrypTool.PluginBase.ICrypComponent plugin = TestHelpers.GetPluginInstance("BaconCipher");
            PluginTestScenario scenario = new PluginTestScenario(plugin,
                new[] { "InputText", ".Cipher", ".OutputMode" }, new[] { "OutputText" });

            object[] encrypted = scenario.GetOutputs(new object[]
            {
                "abc", CrypTool.BaconCipher.BaconCipherSettings.BaconianMode.Encrypt,
                CrypTool.BaconCipher.BaconCipherSettings.OutputTypes.Binary
            });
            Assert.AreEqual("000000000100010", encrypted[0]);

            object[] decrypted = scenario.GetOutputs(new object[]
            {
                "000000000100010", CrypTool.BaconCipher.BaconCipherSettings.BaconianMode.Decrypt,
                CrypTool.BaconCipher.BaconCipherSettings.OutputTypes.Binary
            });
            Assert.AreEqual("abc", decrypted[0]);
        }

        [TestMethod]
        public void AffineCipherEncryptsAndDecryptsKnownMessage()
        {
            CrypTool.PluginBase.ICrypComponent plugin = TestHelpers.GetPluginInstance("AffineCipher");
            PluginTestScenario scenario = new PluginTestScenario(plugin,
                new[] { "InputText", ".A", ".B", ".Action" }, new[] { "OutputText" });

            object[] encrypted = scenario.GetOutputs(new object[]
            {
                "AFFINECIPHER", 5, 8, CrypTool.Plugins.AffineCipher.Action.Encrypt
            });
            Assert.AreEqual("IHHWVCSWFRCP", encrypted[0]);

            object[] decrypted = scenario.GetOutputs(new object[]
            {
                "IHHWVCSWFRCP", 5, 8, CrypTool.Plugins.AffineCipher.Action.Decrypt
            });
            Assert.AreEqual("AFFINECIPHER", decrypted[0]);
        }

        // Blowfish zero-key/zero-plaintext ECB vector from Schneier's published test set.
        [TestMethod]
        public void BlowfishEncryptsAndDecryptsKnownBlock()
        {
            CrypTool.PluginBase.ICrypComponent plugin = TestHelpers.GetPluginInstance("Blowfish");
            PluginTestScenario scenario = new PluginTestScenario(plugin,
                new[] { "InputStream", "InputKey", ".BlowfishAlgorithmType", ".Action", ".BlockMode", ".Padding" },
                new[] { "OutputStream" });

            object[] encrypted = scenario.GetOutputs(new object[]
            {
                "0000000000000000".HexToStream(), "0000000000000000".HexToByteArray(),
                CrypTool.Plugins.Blowfish.BlowfishAlgorithmType.Blowfish,
                CrypTool.PluginBase.Miscellaneous.BlockCipherHelper.CipherAction.Encrypt,
                CrypTool.PluginBase.Miscellaneous.BlockCipherHelper.BlockMode.ECB,
                CrypTool.PluginBase.Miscellaneous.BlockCipherHelper.PaddingType.None
            });
            Assert.AreEqual("4EF997456198DD78", encrypted[0].ToHex());

            object[] decrypted = scenario.GetOutputs(new object[]
            {
                "4EF997456198DD78".HexToStream(), "0000000000000000".HexToByteArray(),
                CrypTool.Plugins.Blowfish.BlowfishAlgorithmType.Blowfish,
                CrypTool.PluginBase.Miscellaneous.BlockCipherHelper.CipherAction.Decrypt,
                CrypTool.PluginBase.Miscellaneous.BlockCipherHelper.BlockMode.ECB,
                CrypTool.PluginBase.Miscellaneous.BlockCipherHelper.PaddingType.None
            });
            Assert.AreEqual("0000000000000000", decrypted[0].ToHex());
        }

        // SM4 standard example: key and plaintext 0123456789ABCDEFFEDCBA9876543210.
        [TestMethod]
        public void SM4EncryptsAndDecryptsStandardBlock()
        {
            const string input = "0123456789ABCDEFFEDCBA9876543210";
            const string expected = "681EDF34D206965E86B3E94F536E4246";
            CrypTool.PluginBase.ICrypComponent plugin = TestHelpers.GetPluginInstance("SM4");
            PluginTestScenario scenario = new PluginTestScenario(plugin,
                new[] { "InputStream", "InputKey", ".Action", ".BlockMode", ".Padding" },
                new[] { "OutputStream" });

            object[] encrypted = scenario.GetOutputs(new object[]
            {
                input.HexToStream(), input.HexToByteArray(),
                CrypTool.PluginBase.Miscellaneous.BlockCipherHelper.CipherAction.Encrypt,
                CrypTool.PluginBase.Miscellaneous.BlockCipherHelper.BlockMode.ECB,
                CrypTool.PluginBase.Miscellaneous.BlockCipherHelper.PaddingType.None
            });
            Assert.AreEqual(expected, encrypted[0].ToHex());

            object[] decrypted = scenario.GetOutputs(new object[]
            {
                expected.HexToStream(), input.HexToByteArray(),
                CrypTool.PluginBase.Miscellaneous.BlockCipherHelper.CipherAction.Decrypt,
                CrypTool.PluginBase.Miscellaneous.BlockCipherHelper.BlockMode.ECB,
                CrypTool.PluginBase.Miscellaneous.BlockCipherHelper.PaddingType.None
            });
            Assert.AreEqual(input, decrypted[0].ToHex());
        }

        [TestMethod]
        public void TEAEncryptsAndDecryptsZeroBlockWith64Rounds()
        {
            const string zeroBlock = "0000000000000000";
            const string zeroKey = "00000000000000000000000000000000";
            const string expected = "C0A8B8983C1C3803";
            CrypTool.PluginBase.ICrypComponent plugin = TestHelpers.GetPluginInstance("TEA");
            PluginTestScenario scenario = new PluginTestScenario(plugin,
                new[] { "InputStream", "InputKey", ".Action", ".Version", ".Mode", ".Padding", ".Rounds" },
                new[] { "OutputStream" });

            object[] encrypted = scenario.GetOutputs(new object[]
            {
                zeroBlock.HexToStream(), zeroKey.HexToByteArray(), 0, 0, 0, 0, 64
            });
            Assert.AreEqual(expected, encrypted[0].ToHex());

            object[] decrypted = scenario.GetOutputs(new object[]
            {
                expected.HexToStream(), zeroKey.HexToByteArray(), 1, 0, 0, 0, 64
            });
            Assert.AreEqual(zeroBlock, decrypted[0].ToHex());
        }

        [TestMethod]
        public void HillCipherEncryptsAndDecryptsClassicExample()
        {
            CrypTool.PluginBase.ICrypComponent plugin = TestHelpers.GetPluginInstance("Hill");
            PluginTestScenario scenario = new PluginTestScenario(plugin,
                new[] { "Input", ".Matrix", ".Action" }, new[] { "Output" });
            const string matrix = "6,24,1,13,16,10,20,17,15";

            object[] encrypted = scenario.GetOutputs(new object[] { "ACT", matrix, false });
            Assert.AreEqual("POH", encrypted[0]);

            object[] decrypted = scenario.GetOutputs(new object[] { "POH", matrix, true });
            Assert.AreEqual("ACT", decrypted[0]);
        }

        [TestMethod]
        public void RSAProcessesTextbookIntegerExample()
        {
            CrypTool.PluginBase.ICrypComponent plugin = TestHelpers.GetPluginInstance("RSA");
            PluginTestScenario scenario = new PluginTestScenario(plugin,
                new[] { "InputN", "InputED", "InputMC" }, new[] { "OutputMC" });

            object[] encrypted = scenario.GetOutputs(new object[]
            {
                new BigInteger(3233), new BigInteger(17), new BigInteger(65)
            });
            Assert.AreEqual(new BigInteger(2790), encrypted[0]);

            object[] decrypted = scenario.GetOutputs(new object[]
            {
                new BigInteger(3233), new BigInteger(2753), new BigInteger(2790)
            });
            Assert.AreEqual(new BigInteger(65), decrypted[0]);
        }

        [TestMethod]
        public void HexParserRejectsMalformedVectors()
        {
            AssertInvalidHex("ABC");
            AssertInvalidHex("GG");
        }

        private static void AssertInvalidHex(string value)
        {
            try
            {
                value.HexToByteArray();
            }
            catch (System.FormatException)
            {
                return;
            }

            Assert.Fail("Expected a FormatException for malformed hex input: " + value);
        }
    }
}
