using CrypTool.PluginBase.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class MD5Test
    {
        public MD5Test()
        {
        }

        [TestMethod]
        public void MD5TestMethod()
        {
            CrypTool.PluginBase.ICrypComponent pluginInstance = TestHelpers.GetPluginInstance("MD5");
            PluginTestScenario scenario1 = new PluginTestScenario(pluginInstance, new[] { "InputData" }, new[] { "OutputDataStream" });
            PluginTestScenario scenario2 = new PluginTestScenario(pluginInstance, new[] { "InputData" }, new[] { "OutputData" });
            object[] output;

            foreach (TestVector vector in testvectors)
            {
                output = scenario1.GetOutputs(new object[] { vector.input.ToStream() });
                Assert.AreEqual(vector.output.ToUpper(), output[0].ToHex(), "Unexpected value in test #" + vector.n + ".");
                output = scenario2.GetOutputs(new object[] { vector.input.ToStream() });
                Assert.AreEqual(vector.output.ToUpper(), output[0].ToHex(), "Unexpected value in test #" + vector.n + ".");
            }

            ICrypToolStream input = new string('\0', 16).ToStream();
            for (int i = 0; i < 100000; i++)
            {
                input = (ICrypToolStream)scenario1.GetOutputs(new object[] { input })[0];
            }

            Assert.AreEqual("1A83F51285E4D89403D00C46EF8508FE", input.ToHex(), "Unexpected value in iteration test");
        }

        private struct TestVector
        {
            public string input, output;
            public int n;
        }

        //
        // Source of the test vectors: https://www.cosic.esat.kuleuven.be/nessie/testvectors/hash/md5/Md5-128.unverified.test-vectors, http://de.wikipedia.org/wiki/Message-Digest_Algorithm_5
        //
        private readonly TestVector[] testvectors = new TestVector[] {
            new TestVector () { n=0, input="", output="d41d8cd98f00b204e9800998ecf8427e" },
            new TestVector () { n=1, input="a", output="0cc175b9c0f1b6a831c399e269772661" },
            new TestVector () { n=2, input="abc", output="900150983cd24fb0d6963f7d28e17f72" },
            new TestVector () { n=3, input="message digest", output="f96b697d7cb7938d525a2f31aaf161d0" },
            new TestVector () { n=4, input="abcdefghijklmnopqrstuvwxyz", output="c3fcd3d76192e4007dfb496cca67e13b" },
            new TestVector () { n=5, input="abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq", output="8215EF0796A20BCAAAE116D3876C664A" },
            new TestVector () { n=6, input="ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", output="d174ab98d277d9f5a5611c2c9f419d9f" },
            new TestVector () { n=7, input="12345678901234567890123456789012345678901234567890123456789012345678901234567890", output="57edf4a22be3c955ac49da2e2107b67a" },
            new TestVector () { n=8, input=new string('a',1000000), output="7707d6ae4e027c70eea2a935c2296f21" },
            new TestVector () { n=9, input=new string('\0',16), output="4AE71336E44BF9BF79D2752E234818A5" },
            new TestVector () { n=10, input="Franz jagt im komplett verwahrlosten Taxi quer durch Bayern", output="a3cca2b2aa1e3b5b3b5aad99a8529074" },
            new TestVector () { n=11, input="Frank jagt im komplett verwahrlosten Taxi quer durch Bayern", output="7e716d0e702df0505fc72e2b89467910" },
            new TestVector () { n=12, input="38", output="a5771bce93e200c36f7cd9dfd0e5deaa" }
        };
    }
}
