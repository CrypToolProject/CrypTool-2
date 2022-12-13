using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class NihilistTest
    {
        public NihilistTest()
        {
        }

        [TestMethod]
        public void NihilistTestMethod()
        {
            CrypTool.PluginBase.ICrypComponent pluginInstance = TestHelpers.GetPluginInstance("Nihilist");
            PluginTestScenario scenario = new PluginTestScenario(pluginInstance, new[] { ".Action", ".AlphabetVersion", ".UnknownSymbolHandling", "InputText", "Key1", "Key2", }, new[] { "OutputText" });
            object[] output;

            foreach (TestVector vector in testvectors)
            {
                output = scenario.GetOutputs(new object[] { vector.action, vector.alphabetVersion, vector.unknownSymbolHanding, vector.input, vector.keyword1, vector.keyword2 });
                Assert.AreEqual(vector.output, output[0], "Unexpected value in test #" + vector.n + ".");
            }

        }

        private struct TestVector
        {
            public int n;
            public int action;
            public int alphabetVersion;
            public int unknownSymbolHanding;
            public string input, output, keyword1, keyword2;            
        }

        private readonly TestVector[] testvectors = new TestVector[]
        {
            new TestVector () { n=0, action=0, alphabetVersion = 0, unknownSymbolHanding = 1, keyword1="RUSSIAN", keyword2="NIHILISTS", input="In the history of cryptography, the Nihilist cipher is a manually operated symmetric encryption cipher, originally used by Russian Nihilists.", output="35 35 78 47 60 47 27 58 58 63 25 87 56 66 37 24 99 56 66 56 65 25 50 57 46 99 58 54 39 54 28 68 28 48 59 26 66 37 47 57 68 39 24 59 26 36 55 48 35 47 29 48 80 67 63 57 58 25 50 59 38 69 26 75 55 74 39 80 25 27 68 38 42 37 44 68 78 59 27 87 34 44 28 76 47 60 25 55 56 27 53 28 54 29 70 49 67 57 26 46 38 55 68 46 26 26 58 27 36 35 54 28 68 28 48 59 26 66 27 " },
            new TestVector () { n=1, action=1, alphabetVersion = 0, unknownSymbolHanding = 1, keyword1="RUSSIAN", keyword2="NIHILISTS", input="35 35 78 47 60 47 27 58 58 63 25 87 56 66 37 24 99 56 66 56 65 25 50 57 46 99 58 54 39 54 28 68 28 48 59 26 66 37 47 57 68 39 24 59 26 36 55 48 35 47 29 48 80 67 63 57 58 25 50 59 38 69 26 75 55 74 39 80 25 27 68 38 42 37 44 68 78 59 27 87 34 44 28 76 47 60 25 55 56 27 53 28 54 29 70 49 67 57 26 46 38 55 68 46 26 26 58 27 36 35 54 28 68 28 48 59 26 66 27 ", output="INTHEHISTORYOFCRYPTOGRAPHYTHENIHILISTCIPHERISAMANUALLYOPERATEDSYMMETRICENCRYPTIONCIPHERORIGINALLYUSEDBYRUSSIANNIHILISTS" },
            new TestVector () { n=2, action=0, alphabetVersion = 1, unknownSymbolHanding = 1, keyword1="FUBAR", keyword2="BARFU", input="In the history of cryptography, the Nihilist cipher is a manually operated symmetric encryption cipher, originally used by Russian Nihilists.", output="38 48 58 35 34 37 39 57 54 47 28 65 50 22 28 28 65 51 54 47 36 29 29 47 36 64 57 39 33 46 38 38 40 43 37 55 57 31 36 48 37 36 30 36 54 27 47 29 45 24 27 46 47 62 47 49 36 30 25 55 35 35 57 62 45 46 36 58 26 37 29 36 49 27 27 64 50 58 36 47 47 30 40 47 36 35 29 50 26 37 36 39 49 25 44 45 65 27 53 34 34 27 66 26 24 55 56 40 25 46 47 39 39 36 44 38 56 58 53 " },
            new TestVector () { n=3, action=1, alphabetVersion = 1, unknownSymbolHanding = 1, keyword1="FUBAR", keyword2="BARFU", input="38 48 58 35 34 37 39 57 54 47 28 65 50 22 28 28 65 51 54 47 36 29 29 47 36 64 57 39 33 46 38 38 40 43 37 55 57 31 36 48 37 36 30 36 54 27 47 29 45 24 27 46 47 62 47 49 36 30 25 55 35 35 57 62 45 46 36 58 26 37 29 36 49 27 27 64 50 58 36 47 47 30 40 47 36 35 29 50 26 37 36 39 49 25 44 45 65 27 53 34 34 27 66 26 24 55 56 40 25 46 47 39 39 36 44 38 56 58 53 ", output="INTHEHISTORYOFCRYPTOGRAPHYTHENIHILISTCIPHERISAMANUALLYOPERATEDSYMMETRICENCRYPTIONCIPHERORIGINALLYUSEDBYRUSSIANNIHILISTS" }
        };        
    }
}