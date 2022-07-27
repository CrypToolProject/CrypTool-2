using CrypTool.PluginBase.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class SHATest
    {
        public SHATest()
        {
        }

        [TestMethod]
        public void SHATestMethod()
        {
            CrypTool.PluginBase.ICrypComponent pluginInstance = TestHelpers.GetPluginInstance("SHA");
            PluginTestScenario scenario1 = new PluginTestScenario(pluginInstance, new[] { "InputData", ".SHAFunction" }, new[] { "OutputDataStream" });
            PluginTestScenario scenario2 = new PluginTestScenario(pluginInstance, new[] { "InputData", ".SHAFunction" }, new[] { "OutputData" });
            object[] output;

            foreach (TestVector vector in testvectors)
            {
                output = scenario1.GetOutputs(new object[] { vector.input.ToStream(), vector.mode });
                Assert.AreEqual(vector.output.ToUpper(), output[0].ToHex(), "Unexpected value in test #" + vector.n + ".");
                output = scenario2.GetOutputs(new object[] { vector.input.ToStream(), vector.mode });
                Assert.AreEqual(vector.output.ToUpper(), output[0].ToHex(), "Unexpected value in test #" + vector.n + ".");
            }

            foreach (TestVector vector in testvectors_loop)
            {
                ICrypToolStream input = vector.input.ToStream();
                for (int i = 0; i < 100000; i++)
                {
                    input = (ICrypToolStream)scenario1.GetOutputs(new object[] { input, vector.mode })[0];
                }

                Assert.AreEqual(vector.output.ToUpper(), input.ToHex(), "Unexpected value in iteration test #" + vector.n + ".");
            }
        }

        private struct TestVector
        {
            public string input, output;
            public int n, mode;
        }

        //
        // Sources of the test vectors:
        //  https://www.cosic.esat.kuleuven.be/nessie/testvectors/hash/sha/Sha-1-160.test-vectors,
        //  https://www.cosic.esat.kuleuven.be/nessie/testvectors/hash/sha/Sha-2-256.unverified.test-vectors
        //  https://www.cosic.esat.kuleuven.be/nessie/testvectors/hash/sha/Sha-2-384.unverified.test-vectors
        //  https://www.cosic.esat.kuleuven.be/nessie/testvectors/hash/sha/Sha-2-512.unverified.test-vectors
        //  http://de.wikipedia.org/wiki/Secure_Hash_Algorithm
        //  http://www.filehash.info/
        //
        private readonly TestVector[] testvectors = new TestVector[] {
            // SHA-1
            new TestVector () { n=0, mode=0, output="da39a3ee5e6b4b0d3255bfef95601890afd80709", input="" },
            new TestVector () { n=1, mode=0, output="86f7e437faa5a7fce15d1ddcb9eaeaea377667b8", input="a" },
            new TestVector () { n=2, mode=0, output="a9993e364706816aba3e25717850c26c9cd0d89d", input="abc" },
            new TestVector () { n=3, mode=0, output="c12252ceda8be8994d5fa0290a47231c1d16aae3", input="message digest" },
            new TestVector () { n=4, mode=0, output="32d10c7b8cf96570ca04ce37f2a19d84240d3a89", input="abcdefghijklmnopqrstuvwxyz" },
            new TestVector () { n=5, mode=0, output="84983e441c3bd26ebaae4aa1f95129e5e54670f1", input="abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq" },
            new TestVector () { n=6, mode=0, output="761c457bf73b14d27e9e9265c46f4b4dda11f940", input="ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789" },
            new TestVector () { n=7, mode=0, output="50abf5706a150990a08b2c5ea40fa0e585554732", input="12345678901234567890123456789012345678901234567890123456789012345678901234567890" },
            new TestVector () { n=8, mode=0, output="34aa973cd4c4daa4f61eeb2bdbad27316534016f", input=new string('a',1000000) },
            new TestVector () { n=9, mode=0, output="6768033e216468247bd031a0a2d9876d79818f8f", input=new string('\0',20) },
            new TestVector () { n=10, mode=0, output="68ac906495480a3404beee4874ed853a037a7a8f", input="Franz jagt im komplett verwahrlosten Taxi quer durch Bayern" },
            new TestVector () { n=11, mode=0, output="d8e8ece39c437e515aa8997c1a1e94f1ed2a0e62", input="Frank jagt im komplett verwahrlosten Taxi quer durch Bayern" },
            new TestVector () { n=12, mode=0, output="5b384ce32d8cdef02bc3a139d4cac0a22bb029e8", input="38" },

            // SHA-256
            new TestVector () { n=13, mode=1, output="e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855", input="" },
            new TestVector () { n=14, mode=1, output="ca978112ca1bbdcafac231b39a23dc4da786eff8147c4e72b9807785afee48bb", input="a" },
            new TestVector () { n=15, mode=1, output="ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", input="abc" },
            new TestVector () { n=16, mode=1, output="f7846f55cf23e14eebeab5b4e1550cad5b509e3348fbc4efa3a1413d393cb650", input="message digest" },
            new TestVector () { n=17, mode=1, output="71c480df93d6ae2f1efad1447c66c9525e316218cf51fc8d9ed832f2daf18b73", input="abcdefghijklmnopqrstuvwxyz" },
            new TestVector () { n=18, mode=1, output="248d6a61d20638b8e5c026930c3e6039a33ce45964ff2167f6ecedd419db06c1", input="abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq" },
            new TestVector () { n=19, mode=1, output="db4bfcbd4da0cd85a60c3c37d3fbd8805c77f15fc6b1fdfe614ee0a7c8fdb4c0", input="ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789" },
            new TestVector () { n=20, mode=1, output="f371bc4a311f2b009eef952dd83ca80e2b60026c8e935592d0f9c308453c813e", input="12345678901234567890123456789012345678901234567890123456789012345678901234567890" },
            new TestVector () { n=21, mode=1, output="cdc76e5c9914fb9281a1c7e284d73e67f1809a48a497200e046d39ccc7112cd0", input=new string('a',1000000) },
            new TestVector () { n=22, mode=1, output="66687aadf862bd776c8fc18b8e9f8e20089714856ee233b3902a591d0d5f2925", input=new string('\0',32) },
            new TestVector () { n=23, mode=1, output="d32b568cd1b96d459e7291ebf4b25d007f275c9f13149beeb782fac0716613f8", input="Franz jagt im komplett verwahrlosten Taxi quer durch Bayern" },
            new TestVector () { n=24, mode=1, output="78206a866dbb2bf017d8e34274aed01a8ce405b69d45db30bafa00f5eeed7d5e", input="Frank jagt im komplett verwahrlosten Taxi quer durch Bayern" },
            new TestVector () { n=25, mode=1, output="aea92132c4cbeb263e6ac2bf6c183b5d81737f179f21efdc5863739672f0f470", input="38" },

            // SHA-384
            new TestVector () { n=26, mode=2, output="38b060a751ac96384cd9327eb1b1e36a21fdb71114be07434c0cc7bf63f6e1da274edebfe76f65fbd51ad2f14898b95b", input="" },
            new TestVector () { n=27, mode=2, output="54a59b9f22b0b80880d8427e548b7c23abd873486e1f035dce9cd697e85175033caa88e6d57bc35efae0b5afd3145f31", input="a" },
            new TestVector () { n=28, mode=2, output="cb00753f45a35e8bb5a03d699ac65007272c32ab0eded1631a8b605a43ff5bed8086072ba1e7cc2358baeca134c825a7", input="abc" },
            new TestVector () { n=29, mode=2, output="473ed35167ec1f5d8e550368a3db39be54639f828868e9454c239fc8b52e3c61dbd0d8b4de1390c256dcbb5d5fd99cd5", input="message digest" },
            new TestVector () { n=30, mode=2, output="feb67349df3db6f5924815d6c3dc133f091809213731fe5c7b5f4999e463479ff2877f5f2936fa63bb43784b12f3ebb4", input="abcdefghijklmnopqrstuvwxyz" },
            new TestVector () { n=31, mode=2, output="3391fdddfc8dc7393707a65b1b4709397cf8b1d162af05abfe8f450de5f36bc6b0455a8520bc4e6f5fe95b1fe3c8452b", input="abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq" },
            new TestVector () { n=32, mode=2, output="1761336e3f7cbfe51deb137f026f89e01a448e3b1fafa64039c1464ee8732f11a5341a6f41e0c202294736ed64db1a84", input="ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789" },
            new TestVector () { n=33, mode=2, output="b12932b0627d1c060942f5447764155655bd4da0c9afa6dd9b9ef53129af1b8fb0195996d2de9ca0df9d821ffee67026", input="12345678901234567890123456789012345678901234567890123456789012345678901234567890" },
            new TestVector () { n=34, mode=2, output="9d0e1809716474cb086e834e310a4a1ced149e9c00f248527972cec5704c2a5b07b8b3dc38ecc4ebae97ddd87f3d8985", input=new string('a',1000000) },
            new TestVector () { n=35, mode=2, output="8f0d145c0368ad6b70be22e41c400eea91b971d96ba220fec9fae25a58dffdaaf72dbe8f6783d55128c9df4efaf6f8a7", input=new string('\0',48) },
            new TestVector () { n=36, mode=2, output="71e8383a4cea32d6fd6877495db2ee353542f46fa44bc23100bca48f3366b84e809f0708e81041f427c6d5219a286677", input="Franz jagt im komplett verwahrlosten Taxi quer durch Bayern" },
            new TestVector () { n=37, mode=2, output="ef9cd8873a92190f68a85edccb823649e3018ab4da3aeff54215187c0972f7d77922c72f7c0d90fca01cf3e46af664d2", input="Frank jagt im komplett verwahrlosten Taxi quer durch Bayern" },
            new TestVector () { n=38, mode=2, output="c071d202ad950b6a04a5f15c24596a993af8b212467958d570a3ffd4780060638e3a3d06637691d3012bd31122071b2c", input="38" },

            // SHA-512
            new TestVector () { n=39, mode=3, output="cf83e1357eefb8bdf1542850d66d8007d620e4050b5715dc83f4a921d36ce9ce47d0d13c5d85f2b0ff8318d2877eec2f63b931bd47417a81a538327af927da3e", input="" },
            new TestVector () { n=40, mode=3, output="1f40fc92da241694750979ee6cf582f2d5d7d28e18335de05abc54d0560e0f5302860c652bf08d560252aa5e74210546f369fbbbce8c12cfc7957b2652fe9a75", input="a" },
            new TestVector () { n=41, mode=3, output="ddaf35a193617abacc417349ae20413112e6fa4e89a97ea20a9eeee64b55d39a2192992a274fc1a836ba3c23a3feebbd454d4423643ce80e2a9ac94fa54ca49f", input="abc" },
            new TestVector () { n=42, mode=3, output="107dbf389d9e9f71a3a95f6c055b9251bc5268c2be16d6c13492ea45b0199f3309e16455ab1e96118e8a905d5597b72038ddb372a89826046de66687bb420e7c", input="message digest" },
            new TestVector () { n=43, mode=3, output="4dbff86cc2ca1bae1e16468a05cb9881c97f1753bce3619034898faa1aabe429955a1bf8ec483d7421fe3c1646613a59ed5441fb0f321389f77f48a879c7b1f1", input="abcdefghijklmnopqrstuvwxyz" },
            new TestVector () { n=44, mode=3, output="204a8fc6dda82f0a0ced7beb8e08a41657c16ef468b228a8279be331a703c33596fd15c13b1b07f9aa1d3bea57789ca031ad85c7a71dd70354ec631238ca3445", input="abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq" },
            new TestVector () { n=45, mode=3, output="1e07be23c26a86ea37ea810c8ec7809352515a970e9253c26f536cfc7a9996c45c8370583e0a78fa4a90041d71a4ceab7423f19c71b9d5a3e01249f0bebd5894", input="ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789" },
            new TestVector () { n=46, mode=3, output="72ec1ef1124a45b047e8b7c75a932195135bb61de24ec0d1914042246e0aec3a2354e093d76f3048b456764346900cb130d2a4fd5dd16abb5e30bcb850dee843", input="12345678901234567890123456789012345678901234567890123456789012345678901234567890" },
            new TestVector () { n=47, mode=3, output="e718483d0ce769644e2e42c7bc15b4638e1f98b13b2044285632a803afa973ebde0ff244877ea60a4cb0432ce577c31beb009c5c2c49aa2e4eadb217ad8cc09b", input=new string('a',1000000) },
            new TestVector () { n=48, mode=3, output="7be9fda48f4179e611c698a73cff09faf72869431efee6eaad14de0cb44bbf66503f752b7a8eb17083355f3ce6eb7d2806f236b25af96a24e22b887405c20081", input=new string('\0',64) },
            new TestVector () { n=49, mode=3, output="af9ed2de700433b803240a552b41b5a472a6ef3fe1431a722b2063c75e9f07451f67a28e37d09cde769424c96aea6f8971389db9e1993d6c565c3c71b855723c", input="Franz jagt im komplett verwahrlosten Taxi quer durch Bayern" },
            new TestVector () { n=50, mode=3, output="90b30ef9902ae4c4c691d2d78c2f8fa0aa785afbc5545286b310f68e91dd2299c84a2484f0419fc5eaa7de598940799e1091c4948926ae1c9488dddae180bb80", input="Frank jagt im komplett verwahrlosten Taxi quer durch Bayern" },
            new TestVector () { n=51, mode=3, output="caae34a5e81031268bcdaf6f1d8c04d37b7f2c349afb705b575966f63e2ebf0fd910c3b05160ba087ab7af35d40b7c719c53cd8b947c96111f64105fd45cc1b2", input="38" }
        };
        private readonly TestVector[] testvectors_loop = new TestVector[] {  // 1.000.000 loops
            new TestVector () { n=0, mode=0, input=new string('\0', 20), output="aa95cc0f521432afa84a608134570caa3cd69d04" },
            new TestVector () { n=1, mode=1, input=new string('\0', 32), output="b422bc9c0646a432433c2410991c95e2d89758e3b4f540aca863389f28a11379" },
            new TestVector () { n=2, mode=2, input=new string('\0', 48), output="ebefe915f541241f3641910f393a56399f51331a67e2a73aad872c7ad7a13a494dca010d7d422ea7f4db5056b33e8292" },
            new TestVector () { n=3, mode=3, input=new string('\0', 64), output="23b8521df55569c4e55c7be36d4ad106e338b0799d5e105058aaa1a95737c25db77af240849ae7283ea0b7cbf196f5e4bd78aca19af97eb6e364ada4d12c1178" }
        };

    }
}