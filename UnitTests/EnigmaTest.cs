using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class EnigmaTest
    {
        public EnigmaTest()
        {
        }

        [TestMethod]
        public void EnigmaTestMethod()
        {
            CrypTool.PluginBase.ICrypComponent pluginInstance = TestHelpers.GetPluginInstance("Enigma");
            PluginTestScenario scenario = new PluginTestScenario(pluginInstance, new[] { "TextInput", ".Model", ".InitialRotorPos", ".Rotor1", ".Rotor2", ".Rotor3", ".Rotor4", ".Reflector", ".Ring1", ".Ring2", ".Ring3", ".Ring4", ".PlugBoard" }, new[] { "TextOutput" });
            object[] output;

            foreach (TestVector vector in testvectors)
            {
                pluginInstance.PreExecution();
                output = scenario.GetOutputs(new object[] { vector.input, vector.model, vector.key, vector.rot1, vector.rot2, vector.rot3, vector.rot4, vector.ukw, vector.ring1, vector.ring2, vector.ring3, vector.ring4, vector.plugBoard }, false);
                Assert.AreEqual(vector.output.ToUpper(), (string)output[0], "Unexpected value in test #" + vector.n + ".");
                pluginInstance.PostExecution();
            }
        }

        private struct TestVector
        {
            public string input, output, key;
            public string plugBoard;
            public int model, ukw, rot1, rot2, rot3, rot4, ring1, ring2, ring3, ring4;
            public int n;
        }

        private readonly TestVector[] testvectors = new TestVector[] {
            // Testvektor from Wikipedia: http://de.wikipedia.org/wiki/Enigma_(Maschine)
            new TestVector () { n=0, model=3, key="RTZ", ukw=1, rot1=2, rot2=3, rot3=0, ring1=8, ring2=26, ring3=16, plugBoard="DBNATLIHGVZFMCOUYRSEPJXWQK",
                input  = "DASOBERKOMMANDODERWEHRMAQTGIBTBEKANNTXAACHENXAACHENXISTGERETTETXDURQGEBUENDELTENEINSATZDERHILFSKRAEFTEKONNTEDIEBEDROHUNGABGEWENDETUNDDIERETTUNGDERSTADTGEGENXEINSXAQTXNULLXNULLXUHRSIQERGESTELLTWERDENX",
                output = "LJPQHSVDWCLYXZQFXHIUVWDJOBJNZXRCWEOTVNJCIONTFQNSXWISXKHJDAGDJVAKUKVMJAJHSZQQJHZOIAVZOWMSCKASRDNXKKSRFHCXCMPJGXYIJCCKISYYSHETXVVOVDQLZYTNJXNUWKZRXUJFXMBDIBRVMJKRHTCUJQPTEEIYNYNJBEAQJCLMUODFWMARQCFOBWN",
            },

            // Source of the test vectors: test vectors created with D.Rijmenants' Enigma Simulator v6.4
            new TestVector () { n=1, model=3, key="AAA", ukw=1, rot1=2, rot2=1, rot3=0, ring1=1, ring2=1, ring3=1, plugBoard="ABCDEFGHIJKLMNOPQRSTUVWXYZ",
                input  = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
                output = "BDZGOWCXLTKSBTMCDLPBMUQOFXYHCXTGYJFLINHNXSHIUNTHEORXPQPKOVHCBUBTZSZSOOSTGOTFSODBBZZLXLCYZXIFGWFDZEEQIBMGFJBWZFCKPFMGBXQCIVIBBRNCOCJUVYDKMVJPFMDRMTGLWFOZLXGJEYYQPVPBWNCKVKLZTCBDLDCTSNRCOOVPTGBVBBISGJSOYHDENCTNUUKC" ,
            },
        };
    }
}
