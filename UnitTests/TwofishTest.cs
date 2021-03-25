using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.TemplateAndPluginTests
{
    [TestClass]
    public class TwofishTest
    {
        public TwofishTest()
        {
        }

        [TestMethod]
        public void TwofishTestMethod()
        {
            var pluginInstance = TestHelpers.GetPluginInstance("Twofish");
            var scenario = new PluginTestScenario(pluginInstance, new[] { "InputStream", "KeyData", ".Action", ".Mode", ".KeySize", ".Mode", ".Padding" }, new[] { "OutputStream" });
            object[] output;

            foreach (TestVector vector in testvectors)
            {
                output = scenario.GetOutputs(new object[] { vector.input.HexToStream(), vector.key.HexToByteArray(), 0, 0, vector.keysize, 0, 0 });
                Assert.AreEqual(vector.output.ToUpper(), output[0].ToHex(), "Unexpected value in test #" + vector.n + ".");
            }

            foreach (TestVector vector in testvectors_loop)
            {
                string input, key, cipher;
                input = cipher = key = "00000000000000000000000000000000";

                for (int i = 0; i < 49; i++)
                {
                    key = (input + key).Substring(0, (128 + vector.keysize * 64) / 4);
                    input = cipher;
                    output = scenario.GetOutputs(new object[] { input.HexToStream(), key.HexToByteArray(), 0, 0, vector.keysize, 0, 0 });
                    cipher = output[0].ToHex();
                }
                Assert.AreEqual(vector.output.ToUpper(), cipher, "Unexpected value in test loop #" + vector.n + ".");
            }
        }

        struct TestVector
        {
            public string input, key, output;
            public int n, mode, action, keysize;
        }
        
        TestVector[] testvectors = new TestVector[] {
            // Source of the test vectors: http://www.schneier.com/code/ecb_ival.txt
            new TestVector () { n=0, action=0, mode=0, keysize=0, key="00000000000000000000000000000000", input="00000000000000000000000000000000", output="9f589f5cf6122c32b6bfec2f2ae8c35a" },
            new TestVector () { n=1, action=0, mode=0, keysize=0, key="BCA724A54533C6987E14AA827952F921", input="6B459286F3FFD28D49F15B1581B08E42", output="5D9D4EEFFA9151575524F115815A12E0" },
            new TestVector () { n=3, action=0, mode=0, keysize=1, key="0123456789ABCDEFFEDCBA98765432100011223344556677", input="00000000000000000000000000000000", output="cfd1d2e5a9be9cdf501f13b892bd2248" },
            new TestVector () { n=4, action=0, mode=0, keysize=1, key="FB66522C332FCC4C042ABE32FA9E902FDEA4F3DA75EC7A8E", input="F0AB73301125FA21EF70BE5385FB76B6", output="E75449212BEEF9F4A390BD860A640941" },
            new TestVector () { n=5, action=0, mode=0, keysize=2, key="0123456789ABCDEFFEDCBA987654321000112233445566778899AABBCCDDEEFF", input="00000000000000000000000000000000", output="37527be0052334b89f0cfccae87cfa20" },
            new TestVector () { n=6, action=0, mode=0, keysize=2, key="248A7F3528B168ACFDD1386E3F51E30C2E2158BC3E5FC714C1EEECA0EA696D48", input="431058F4DBC7F734DA4F02F04CC4F459", output="37FE26FF1CF66175F5DDF4C33B97A205" },

            // Source of the test vectors: https://github.com/zooko/cryptopp/blob/master/tags/c50-fixes-merged/c5/twofishv.dat
            new TestVector () { n=7, action=0, mode=0, keysize=0, key="00000000000000000000000000000000", input="9F589F5CF6122C32B6BFEC2F2AE8C35A", output="D491DB16E7B1C39E86CB086B789F5419" },
            new TestVector () { n=8, action=0, mode=0, keysize=0, key="9F589F5CF6122C32B6BFEC2F2AE8C35A", input="D491DB16E7B1C39E86CB086B789F5419", output="019F9809DE1711858FAAC3A3BA20FBC3" },
            new TestVector () { n=9, action=0, mode=0, keysize=0, key="D491DB16E7B1C39E86CB086B789F5419", input="019F9809DE1711858FAAC3A3BA20FBC3", output="6363977DE839486297E661C6C9D668EB" },
            new TestVector () { n=10, action=0, mode=0, keysize=1, key="000000000000000000000000000000000000000000000000", input="00000000000000000000000000000000", output="EFA71F788965BD4453F860178FC19101" },
            new TestVector () { n=11, action=0, mode=0, keysize=1, key="EFA71F788965BD4453F860178FC191010000000000000000", input="88B2B2706B105E36B446BB6D731A1E88", output="39DA69D6BA4997D585B6DC073CA341B2" },
            new TestVector () { n=12, action=0, mode=0, keysize=1, key="88B2B2706B105E36B446BB6D731A1E88EFA71F788965BD44", input="39DA69D6BA4997D585B6DC073CA341B2", output="182B02D81497EA45F9DAACDC29193A65" },
            new TestVector () { n=13, action=0, mode=0, keysize=2, key="0000000000000000000000000000000000000000000000000000000000000000", input="00000000000000000000000000000000", output="57FF739D4DC92C1BD7FC01700CC8216F" },
            new TestVector () { n=14, action=0, mode=0, keysize=2, key="D43BB7556EA32E46F2A282B7D45B4E0D57FF739D4DC92C1BD7FC01700CC8216F", input="90AFE91BB288544F2C32DC239B2635E6", output="6CB4561C40BF0A9705931CB6D408E7FA" },

        };

        TestVector[] testvectors_loop = new TestVector[] {
            // Source of the test vectors: http://www.schneier.com/code/ecb_ival.txt
            new TestVector () { n=0, action=0, mode=0, keysize=0, output="5D9D4EEFFA9151575524F115815A12E0" },
            new TestVector () { n=1, action=0, mode=0, keysize=1, output="E75449212BEEF9F4A390BD860A640941" },
            new TestVector () { n=2, action=0, mode=0, keysize=2, output="37FE26FF1CF66175F5DDF4C33B97A205" },
       };

    }
}  