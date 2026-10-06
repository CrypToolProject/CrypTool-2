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
    public class TranspositionTest
    {
        public TranspositionTest()
        {
        }

        [TestMethod]
        public void TranspositionMatchesExpectedVectors()
        {
            CrypTool.PluginBase.ICrypComponent pluginInstance = TestHelpers.GetPluginInstance("Transposition");
            PluginTestScenario scenario = new PluginTestScenario(pluginInstance, new[] { "Input", "Keyword", ".ReadIn", ".Permutation", ".ReadOut" }, new[] { "Output" });
            object[] output;

            foreach (TestVector vector in testvectors)
            {
                output = scenario.GetOutputs(new object[] { vector.input, vector.key, vector.readin, vector.perm, vector.readout });
                Assert.AreEqual(vector.output, output[0], "Unexpected value in test #" + vector.n + ".");
            }

        }

        private struct TestVector
        {
            public string input, output, key;
            public int readin, readout, perm;
            public int n;
        }

        //
        // Source of the test vectors: http://crackthebbccode.wikispaces.com/Useful+code
        //
        private readonly TestVector[] testvectors = new TestVector[] {
            new TestVector () { n=0, readin=0, perm=1, readout=1, key="ZEBRAS", input="WEAREDISCOVEREDFLEEATONCE", output="EVLNACDTESEAROFODEECWIREE" },
            new TestVector () { n=1, readin=0, perm=1, readout=1, key="ZEBRAS", input="Wir wurden entdeckt, flieh wenn du kannst", output="w cle tred  dnidt,h n nefwusuekinkWrntena" },
            new TestVector () { n=2, readin=1, perm=1, readout=1, key="ZEBRAS", input="Wir wurden entdeckt, flieh wenn du kannst", output="enn du deckt, den entflieh wkannstWir wur" },
            new TestVector () { n=3, readin=1, perm=0, readout=1, key="ZEBRAS", input="Wir wurden entdeckt, flieh wenn du kannst", output="wri uW ednercdteknl ,fite hwne d ukntnnsa" },
            new TestVector () { n=4, readin=0, perm=0, readout=1, key="ZEBRAS", input="Wir wurden entdeckt, flieh wenn du kannst", output="eddfkWneelaincninr k en dtehswu,n tu  twr" },
            new TestVector () { n=5, readin=0, perm=1, readout=1, key="HUHN", input="A", output="A" },
            new TestVector () { n=6, readin=1, perm=1, readout=1, key="HUHN", input="A", output="A" },
            new TestVector () { n=7, readin=1, perm=0, readout=1, key="HUHN", input="A", output="A" },
            new TestVector () { n=8, readin=0, perm=0, readout=1, key="HUHN", input="A", output="A" },
        };

    }
}