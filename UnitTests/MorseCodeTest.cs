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
    public class MorseCodeTest
    {
        [TestMethod]
        public void MorseCodeMatchesExpectedVectors()
        {
            CrypTool.PluginBase.ICrypComponent pluginInstance = TestHelpers.GetPluginInstance("MorseCode");
            PluginTestScenario scenario = new PluginTestScenario(pluginInstance, new[] { "InputText", ".Action" }, new[] { "OutputText" });

            foreach (TestVector vector in testvectors)
            {
                object[] output = scenario.GetOutputs(new object[] { vector.input, vector.action });
                Assert.AreEqual(vector.output, output[0], "Unexpected value in test #" + vector.n + ".");
            }
        }

        private struct TestVector
        {
            public string input, output;
            public int action, n;
        }

        //
        // Generated Testvectors with: http://morsecode.scphillips.com/jtranslator.html
        //
        private readonly TestVector[] testvectors =
        {
            //test vectors for encoding:
            new TestVector () { n=1, action=0, input="ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890", output=".- -... -.-. -.. . ..-. --. .... .. .--- -.- .-.. -- -. --- .--. --.- .-. ... - ..- ...- .-- -..- -.-- --.. .---- ..--- ...-- ....- ..... -.... --... ---.. ----. -----" },
            new TestVector () { n=2, action=0, input="FRANZ JAGT IM KOMPLETT VERWAHRLOSTEN TAXI QUER DURCH BAYERN", output="..-. .-. .- -. --.. / .--- .- --. - / .. -- / -.- --- -- .--. .-.. . - - / ...- . .-. .-- .- .... .-. .-.. --- ... - . -. / - .- -..- .. / --.- ..- . .-. / -.. ..- .-. -.-. .... / -... .- -.-- . .-. -." },
            new TestVector () { n=3, action=0, input="THE QUICK BROWN FOX JUMPS OVER THE LAZY DOG", output="- .... . / --.- ..- .. -.-. -.- / -... .-. --- .-- -. / ..-. --- -..- / .--- ..- -- .--. ... / --- ...- . .-. / - .... . / .-.. .- --.. -.-- / -.. --- --." },
            //test vectors for decoding:
            new TestVector () { n=4, action=1, input=".- -... -.-. -.. . ..-. --. .... .. .--- -.- .-.. -- -. --- .--. --.- .-. ... - ..- ...- .-- -..- -.-- --.. .---- ..--- ...-- ....- ..... -.... --... ---.. ----. -----", output="ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890" },
            new TestVector () { n=5, action=1, input="..-. .-. .- -. --.. / .--- .- --. - / .. -- / -.- --- -- .--. .-.. . - - / ...- . .-. .-- .- .... .-. .-.. --- ... - . -. / - .- -..- .. / --.- ..- . .-. / -.. ..- .-. -.-. .... / -... .- -.-- . .-. -.", output="FRANZ JAGT IM KOMPLETT VERWAHRLOSTEN TAXI QUER DURCH BAYERN" },
            new TestVector () { n=6, action=1, input="- .... . / --.- ..- .. -.-. -.- / -... .-. --- .-- -. / ..-. --- -..- / .--- ..- -- .--. ... / --- ...- . .-. / - .... . / .-.. .- --.. -.-- / -.. --- --.", output="THE QUICK BROWN FOX JUMPS OVER THE LAZY DOG" }
        };
    }
}

