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
using AlphabetPermutator;
using CrypTool.Plugins.Decimalization;
using CrypTool.Plugins.EllipticCurveCryptography;
using CrypTool.Plugins.HomophonicSubstitutionAnalyzer;
using CrypTool.Plugins.T310;
using KeySearcher.CrypCloud;
using LatticeCrypto.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace UnitTests
{
    [TestClass]
    public class CrypPluginsAuditRegressionTests
    {
        [TestMethod]
        public void EllipticCurveScalarIsNotReducedModuloFieldPrime()
        {
            WeierstraßCurve curve = new WeierstraßCurve(0, 1, 5);
            CrypTool.Plugins.EllipticCurveCryptography.Point point =
                new CrypTool.Plugins.EllipticCurveCryptography.Point { X = 0, Y = 1, Curve = curve };

            CrypTool.Plugins.EllipticCurveCryptography.Point result = curve.Multiply(5, point);

            Assert.IsFalse(result.IsInfinity);
            Assert.AreEqual(new BigInteger(0), result.X);
            Assert.AreEqual(new BigInteger(4), result.Y);
        }

        [TestMethod]
        public void VisaDecimalizationUsesTheLowNibble()
        {
            CrypTool.Plugins.Decimalization.Decimalization plugin =
                new CrypTool.Plugins.Decimalization.Decimalization { BinaryNumber = new byte[] { 0x10 } };
            DecimalizationSettings settings = (DecimalizationSettings)plugin.Settings;
            settings.Mode = 0;
            settings.Quant = 2;

            plugin.Execute();

            CollectionAssert.AreEqual(new[] { 1, 0 }, plugin.DecimalNumberInt);
        }

        [TestMethod]
        public void AlphabetKeywordCharactersAreTreatedLiterally()
        {
            AlphabetPermutator.AlphabetPermutator plugin = new AlphabetPermutator.AlphabetPermutator();
            string result = plugin.GenerateAlphabet("ABCDEFGHIJKLMNOPQRSTUVWXYZ", 0, "A-Z", AlphabetOrder.LeaveAsIs);

            Assert.IsTrue(result.StartsWith("A-Z"));
            Assert.IsTrue(result.Contains("B"));
            Assert.IsTrue(result.Contains("Y"));
        }

        [TestMethod]
        public void WordFinderDoesNotAcceptDictionaryPrefixes()
        {
            WordFinder finder = new WordFinder(new[] { "ABCD" }, 2, 4, "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            Assert.IsFalse(finder.IsInDictionary(new[] { 0, 1, 2 }));
            Assert.IsTrue(finder.IsInDictionary(new[] { 0, 1, 2, 3 }));
        }

        [TestMethod]
        public void WordFinderChecksTheLastPossibleStartPosition()
        {
            WordFinder finder = new WordFinder(new[] { "CD" }, 2, 2, "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            var positions = finder.FindWords(new[] { 0, 1, 2, 3 });
            Assert.AreEqual(2, positions.Single().Key);
        }

        [TestMethod]
        public void T310ResetRestoresConnectorKey()
        {
            ControlUnit control = new ControlUnit(null, BitSelectorEnum.Low);
            byte[] key = new byte[15];
            key[0] = 1;
            Assert.IsTrue(control.KeyFromBytes(key, KeyIndex.S1));

            control.S1[0] = 0;
            control.ResetKeys();

            Assert.AreEqual(1, control.S1[0]);
        }

        [TestMethod]
        public void T310ParityChecksEveryKeyBit()
        {
            ControlUnit control = new ControlUnit(null, BitSelectorEnum.Low);
            byte[] evenParityKey = new byte[15];
            evenParityKey[0] = 3;
            Assert.IsFalse(control.KeyFromBytes(evenParityKey, KeyIndex.S1));
        }

        [TestMethod]
        public void T310SynchronizationVectorAdvances()
        {
            SynchronizationUnit unit = new SynchronizationUnit(null, BitSelectorEnum.Low) { initVector = 1 };
            Assert.AreEqual((byte)1, unit.GetFBit());
            Assert.AreEqual((byte)0, unit.GetFBit());
        }

        [TestMethod]
        public void VisualCryptographySupportsOddImageWidths()
        {
            byte[] pixels = Enumerable.Range(0, 30).Select(i => (byte)i).ToArray();
            using (Bitmap bitmap = CrypTool.Plugins.VisualCryptography.VisualCryptography.CreateBitmap(pixels, 3, 10))
            {
                Assert.AreEqual(3, bitmap.Width);
                Assert.AreEqual(10, bitmap.Height);
            }
        }

        [TestMethod]
        public void TextSteganographyRejectsNegativeOffsetsInSettings()
        {
            TextSteganography.TextSteganographySettings settings =
                new TextSteganography.TextSteganographySettings();
            settings.Offset = 5;
            settings.Offset = -1;
            Assert.AreEqual(5, settings.Offset);
        }

        [TestMethod]
        public void PbkdfRejectsNonPositiveIterationCounts()
        {
            CrypTool.PBKDF.PBKDFSettings settings = new CrypTool.PBKDF.PBKDFSettings();
            settings.Iterations = 10;
            settings.Iterations = 0;
            Assert.AreEqual(10, settings.Iterations);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void LatticeRandomRangeRejectsReversedBounds()
        {
            Util.ComputeRandomBigInt(2, 1);
        }

        [TestMethod]
        public void CloudKeyResultRoundTripsLargePayloads()
        {
            KeyResultEntry entry = new KeyResultEntry
            {
                Costs = 1.25,
                KeyBytes = Enumerable.Repeat((byte)0x5a, 70000).ToArray(),
                Decryption = Enumerable.Repeat((byte)0xa5, 70001).ToArray()
            };

            KeyResultEntry copy = new KeyResultEntry(entry.Serialize());

            CollectionAssert.AreEqual(entry.KeyBytes, copy.KeyBytes);
            CollectionAssert.AreEqual(entry.Decryption, copy.Decryption);
        }

        [TestMethod]
        public void BlockchainValidationRejectsTrailingGarbageAndAcceptsSingleDigitKeys()
        {
            CrypTool.Plugins.Blockchain.Blockchain plugin = new CrypTool.Plugins.Blockchain.Blockchain();

            Assert.IsFalse(plugin.CheckTransaction("Alice;Bob;1;2 trailing"));
            Assert.IsTrue(plugin.CheckAddress("Alice;1;2;3"));
            Assert.IsFalse(plugin.CheckAddress("Alice;1;2;3 trailing"));
        }

        [TestMethod]
        public void CryptoBoxRejectsAnEmptyKeyWithoutThrowing()
        {
            CrypTool.Plugins.CryptoBoxCipher.CryptoBoxCipher plugin =
                new CrypTool.Plugins.CryptoBoxCipher.CryptoBoxCipher
                {
                    TextInput = "TEST",
                    KeyInput = string.Empty
                };

            plugin.Execute();

            Assert.IsNull(plugin.TextOutput);
        }

        [TestMethod]
        public void CostFunctionRejectsNegativeOffsetsWithoutThrowing()
        {
            CrypTool.Plugins.CostFunction.CostFunction plugin =
                new CrypTool.Plugins.CostFunction.CostFunction { InputText = new byte[] { 1, 2, 3 } };
            CrypTool.Plugins.CostFunction.CostFunctionSettings settings =
                (CrypTool.Plugins.CostFunction.CostFunctionSettings)plugin.Settings;
            settings.BytesOffset = "-1";
            settings.BytesToUse = "1";

            plugin.Execute();
        }

        [TestMethod]
        public void HkdfStopIsSafeBeforeExecution()
        {
            new CrypTool.Plugins.HKDFSHA256.HKDFSHA256().Stop();
        }

        [TestMethod]
        public void PaillierPreservesTrailingZeroBytesWithANonDefaultGenerator()
        {
            byte[] plaintext = { 0x41, 0x00, 0x00 };
            BigInteger n = 17 * 19;
            BigInteger lambda = 144;
            BigInteger generator = 1 + (2 * n);

            CrypTool.Plugins.Paillier.Paillier encryptor = new CrypTool.Plugins.Paillier.Paillier
            {
                InputN = n,
                InputG = generator,
                InputM = plaintext
            };
            ((CrypTool.Plugins.Paillier.PaillierSettings)encryptor.Settings).Action = 0;
            encryptor.Execute();

            CrypTool.Plugins.Paillier.Paillier decryptor = new CrypTool.Plugins.Paillier.Paillier
            {
                InputN = n,
                InputG = generator,
                InputLambda = lambda,
                InputM = encryptor.OutputC2
            };
            ((CrypTool.Plugins.Paillier.PaillierSettings)decryptor.Settings).Action = 1;
            decryptor.Execute();

            CollectionAssert.AreEqual(plaintext, decryptor.OutputC2);
        }
    }
}
