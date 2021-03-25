using Microsoft.VisualStudio.TestTools.UnitTesting;
using CrypTool.PluginBase.Miscellaneous;
using System.Numerics;

namespace Tests.TemplateAndPluginTests
{
    [TestClass]
    public class PrimesGeneratorTest
    {
        public PrimesGeneratorTest()
        {
        }

        [TestMethod]
        public void PrimesGeneratorTestMethod()
        {
            var pluginInstance = TestHelpers.GetPluginInstance("PrimesGenerator");
            var scenario = new PluginTestScenario(pluginInstance, new[] { "n", ".Mode" }, new[] { "OutputString" });
            
            foreach (TestVector vector in testvectors)
            {
                if (vector.mode <= 2)
                {
                    for (int i = 0; i < vector.count; i++)
                    {
                        object[] output = scenario.GetOutputs(new object[] { vector.input, vector.mode });
                        Assert.IsTrue(BigIntegerHelper.IsProbablePrime((BigInteger)output[0]), (BigInteger)output[0] + " is not a prime number in test #" + vector.n + ".");
                    }
                }
                else
                {
                    object[] output = scenario.GetOutputs(new object[] { vector.input, vector.mode });
                    Assert.AreEqual(vector.output, output[0], "Unexpected value in test #" + vector.n + ".");
                }
            }


        }

        struct TestVector
        {
            public BigInteger input, output;
            public int mode, n, count;
        }

        //
        // Source of the test vectors: Mathematica
        //
        TestVector[] testvectors = new TestVector[] {
            new TestVector () { n=1, mode=3, input=7, output=5 },
            new TestVector () { n=2, mode=4, input=7, output=11 },
            new TestVector () { n=3, mode=3, input=BigInteger.Parse("1267650600228229401496703205376"), output=BigInteger.Parse("1267650600228229401496703205361") },
            new TestVector () { n=4, mode=4, input=BigInteger.Parse("1267650600228229401496703205376"), output=BigInteger.Parse("1267650600228229401496703205653") },
            new TestVector () { n=5, mode=0, input=100, count=1000 },       // generate count numbers with input bits 
            new TestVector () { n=5, mode=0, input=1000, count=1 },         // generate count numbers with input bits
            new TestVector () { n=6, mode=1, input=100, count=1000 },       // generate count numbers with input bits, MSB set
            new TestVector () { n=7, mode=2, input=1000000, count=1000 },   // generate count numbers <= input
        };

    }
}