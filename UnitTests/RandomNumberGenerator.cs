using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTests
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
            ICG = 4,
            SubtractiveGenerator = 5,
            XORShift = 6
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
            new TestVector () { n=0, t=AlgorithmType.X2modN, o=OutputType.ByteArray, Seed="19", Modulo="3", A="", B="", OutputAmount="8", Result="0101010101010101" },
            new TestVector () { n=1, t=AlgorithmType.X2modN, o=OutputType.ByteArray, Seed="3", Modulo="17", A="", B="", OutputAmount="8", Result="090D100101010101" },
            new TestVector () { n=2, t=AlgorithmType.X2modN, o=OutputType.ByteArray, Seed="11", Modulo="19457325865870232370589789343309096381", A="", B="", OutputAmount="8", Result="79313961DBC60CC1" },

            new TestVector () { n=3, t=AlgorithmType.LCG, o=OutputType.ByteArray, Seed="11", Modulo="256", A="2", B="23", OutputAmount="8", Result="2D71F900092969E9" },
            new TestVector () { n=4, t=AlgorithmType.LCG, o=OutputType.ByteArray, Seed="11", Modulo="256", A="3", B="23", OutputAmount="8", Result="38BF00541350072C" },
            new TestVector () { n=5, t=AlgorithmType.LCG, o=OutputType.ByteArray, Seed="11", Modulo="19457325865870232370589789343309096381", A="3", B="23", OutputAmount="8", Result="38BF005402130750" },

            new TestVector () { n=6, t=AlgorithmType.ICG, o=OutputType.ByteArray, Seed="11", Modulo="256", A="21", B="2", OutputAmount="8", Result="59AF00F500137177" },
            new TestVector () { n=7, t=AlgorithmType.ICG, o=OutputType.ByteArray, Seed="11", Modulo="256", A="2", B="21", OutputAmount="8", Result="830013F30033B300" },
            new TestVector () { n=8, t=AlgorithmType.ICG, o=OutputType.ByteArray, Seed="17", Modulo="19457325865870232370589789343309096381", A="3", B="23", OutputAmount="8", Result="DE3424A9D78D5960" },

            new TestVector () { n=9, t=AlgorithmType.SubtractiveGenerator, o=OutputType.ByteArray, Seed="17",Result="DED2881CB82AB51A" },
            new TestVector () { n=10, t=AlgorithmType.SubtractiveGenerator, o=OutputType.ByteArray, Seed="171",Result="540CBD1B989C8C1C" },
            new TestVector () { n=11, t=AlgorithmType.SubtractiveGenerator, o=OutputType.ByteArray, Seed="12031984",Result="D7770A370894D30A" },

            new TestVector () { n=12, t=AlgorithmType.XORShift, o=OutputType.ByteArray, Seed="1",Result="2120040001060804" },
            new TestVector () { n=13, t=AlgorithmType.XORShift, o=OutputType.ByteArray, Seed="42",Result="2845AD00AC340AA9" },
            new TestVector () { n=14, t=AlgorithmType.XORShift, o=OutputType.ByteArray, Seed="12031984",Result="543474BBCBFF2B82" },
        };
    }
}