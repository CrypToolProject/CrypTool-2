using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Tests.TemplateAndPluginTests
{

    /// <summary>
    /// TestClass for RandomNumberGenerator
    /// </summary>
    [TestClass]
    public class RandomNumberGenerator
    {
        /// <summary>
        /// defines the structure of the test vectors
        /// </summary>
        private struct TestVector
        {
            public string Seed, A, B, Modulo, OutputAmount, Result;
            public int n;
            public AlgorithmType t;
            public OutputType o;
        }

        /// <summary>
        /// Algorithmtype
        /// </summary>
        public enum AlgorithmType
        {
            RandomRandom = 0,               //.net Random.Random
            RNGCryptoServiceProvider = 1,   //.net RNGCryptoServiceProvider
            X2modN = 2,
            LCG = 3,
            ICG = 4
        }

        /// <summary>
        /// Outputtype
        /// </summary>
        public enum OutputType
        {
            ByteArray = 0,
            CrypToolStream = 1,
            Number = 2,
            NumberArray = 3
        }

        private byte[] outputData = null;
        private CrypTool.Plugins.RandomNumberGenerator.RandomNumberGenerator pluginInstance;

        /// <summary>
        /// The method contains the unittests
        /// </summary>
        [TestMethod]
        public void RandomNumberGeneratorTestMethod()
        {
            pluginInstance = (CrypTool.Plugins.RandomNumberGenerator.RandomNumberGenerator)TestHelpers.GetPluginInstance("RandomNumberGenerator");
            PluginTestScenario scenario = new PluginTestScenario(pluginInstance, new[] { ".AlgorithmType", ".OutputType", ".OutputLength", ".Seed", ".a", ".b", ".Modulus" }, new[] { "Output" });
            object[] output;

            //loop over each test vector and test it
            foreach (TestVector vector in testvectors)
            {
                DateTime startTime = DateTime.Now;
                output = scenario.GetOutputs(new object[] { vector.t, vector.o, vector.OutputAmount, vector.Seed, vector.A, vector.B, vector.Modulo });
                outputData = (byte[])pluginInstance.Output;
                Assert.AreEqual(vector.Result.ToLower(), outputData.ToHex().ToLower(), "Unexpected value in test #" + vector.n + ".");
            }
        }

        // <summary>
        // Self defined by using the CT1
        // </summary>
        private readonly TestVector[] testvectors = new TestVector[] {
            new TestVector () { n=0, t=AlgorithmType.X2modN, o=OutputType.ByteArray, Seed="19", Modulo="3", A="", B="", OutputAmount="8", Result="FFFFFFFFFFFFFFFF" },
            new TestVector () { n=1, t=AlgorithmType.X2modN, o=OutputType.ByteArray, Seed="3", Modulo="17", A="", B="", OutputAmount="8", Result="DFFFFFFFFFFFFFFF" },
            new TestVector () { n=2, t=AlgorithmType.X2modN, o=OutputType.ByteArray, Seed="11", Modulo="19457325865870232370589789343309096381", A="", B="", OutputAmount="8", Result="F8CFF9E9D7EFC060" },

            new TestVector () { n=3, t=AlgorithmType.LCG, o=OutputType.ByteArray, Seed="11", Modulo="256", A="2", B="23", OutputAmount="8", Result="23FFFFFFFFFFFFFF" },
            new TestVector () { n=4, t=AlgorithmType.LCG, o=OutputType.ByteArray, Seed="11", Modulo="256", A="3", B="23", OutputAmount="8", Result="41F0EB72C3980121" },
            new TestVector () { n=5, t=AlgorithmType.LCG, o=OutputType.ByteArray, Seed="11", Modulo="19457325865870232370589789343309096381", A="3", B="23", OutputAmount="8", Result="0000000000000000" },

            new TestVector () { n=6, t=AlgorithmType.ICG, o=OutputType.ByteArray, Seed="11", Modulo="3", A="21", B="2", OutputAmount="8", Result="FFFFFFFFFFFFFFFF" },
            new TestVector () { n=7, t=AlgorithmType.ICG, o=OutputType.ByteArray, Seed="11", Modulo="7", A="2", B="21", OutputAmount="8", Result="251554D2CAAA2925" },
            new TestVector () { n=8, t=AlgorithmType.ICG, o=OutputType.ByteArray, Seed="12", Modulo="5", A="3", B="21", OutputAmount="8", Result="C66331988C466331"},
            new TestVector () { n=9, t=AlgorithmType.ICG, o=OutputType.ByteArray, Seed="12", Modulo="46633", A="3", B="21", OutputAmount="8", Result="0FFF30DA09FB9B17" }
        };
    }
}