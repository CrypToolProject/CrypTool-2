using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class VigenereTest
    {
        public VigenereTest()
        {
        }

        [TestMethod]
        public void VigenereTestMethod()
        {
            CrypTool.PluginBase.ICrypComponent pluginInstance = TestHelpers.GetPluginInstance("Vigenere");
            PluginTestScenario scenario = new PluginTestScenario(pluginInstance, new[] { "InputString", "ShiftValue" }, new[] { "OutputString" });
            object[] output;

            foreach (TestVector vector in testvectors)
            {
                output = scenario.GetOutputs(new object[] { vector.input, vector.key });
                Assert.AreEqual(vector.output, (string)output[0], "Unexpected value in test #" + vector.n + ".");
            }

        }

        private struct TestVector
        {
            public string input, output, key;
            public int n;
        }

        //
        // Sources of the test vectors:
        //  http://courses.ece.ubc.ca/412/previous_years/2004/modules/sessions/EECE_412-03-crypto_intro-viewable.pdf
        //  http://en.wikipedia.org/wiki/Vigenère_cipher
        //  CrypTool1-Testvectors
        //
        private readonly TestVector[] testvectors = new TestVector[] {
            new TestVector () { n=0, key="LEMON", input="ATTACKATDAWN", output="LXFOPVEFRNHR" },
            new TestVector () { n=0, key="ABCD", input="CRYPTOISSHORTFORCRYPTOGRAPHY", output="CSASTPKVSIQUTGQUCSASTPIUAQJB" },
            new TestVector () { n=0, key="RELATIONS", input="TOBEORNOTTOBETHATISTHEQUESTION", output="KSMEHZBBLKSMEMPOGAJXSEJCSFLZSY" },
            new TestVector () { n=0, key="QWERTZUIOPASDFGHJKLYXCVBNM", input="BEIDERDISKUSSIONDERSICHERHEITEINESVERSCHLUESSELUNGSVERFAHRENSSOLLTEMANIMMERDAVONAUSGEHENDASSDERANGREIFERDASVERFAHRENKENNTABERNICHTDENSCHLUESSELDIEGOLDENEREGELDERKRYPTOGRAPHIEHEISSTUNTERSCHAETZENIEMALSDENKRYPTOANALYTIKEREINVERFAHRENFUERDESSENSICHERHEITMANAUFDIEGEHEIMHALTUNGDESALGORITHMUSANGEWIESENISTHATSCHWEREMAENGEL", output="RAMUXQXQGZUKVNUUMOCQFECFETUEXVBMYAJTRKFMRBNCDCIWIHFHUNJRAQYVGHODOYKTJXTKJGMENHEJELLFYPSCDSVXJLAKYEOGDGRDTWWMXQZIVGEFNJTUCKMCOPDDUFTARJVGFCSHSWOIOLPYWBBPZSRSUHHVKJLGDIOYUFVOROSCFUNUHZJAVJVGUMHOEFLJSHUCOCKMMZCFEWRREXNQYTRWLSBLAPLFOGIGHQHZIJLDHAWRHWUMKPCWLLXWAEVQWALVBLBIZIUFJIKZJVRMOKOIZGIWRXXVCMGTNAVYNHCCNFTGMFZMUJKVE" },
        };

    }
}