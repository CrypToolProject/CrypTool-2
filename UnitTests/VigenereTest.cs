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

namespace UnitTests
{
    [TestClass]
    public class VigenereTest
    {
        public VigenereTest()
        {
        }

        [TestMethod]
        public void VigenereMatchesExpectedVectors()
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