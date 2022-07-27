using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace UnitTests
{
    [TestClass]
    public class MorseCodeTest
    {
        [TestMethod]
        public void MorseCodeTestMethod()
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

