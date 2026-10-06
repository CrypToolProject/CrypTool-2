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
    public class ScytaleTest
    {
        public ScytaleTest()
        {
        }

        [TestMethod]
        public void ScytaleEncryptsAndDecryptsKnownMessage()
        {
            CrypTool.PluginBase.ICrypComponent pluginInstance = TestHelpers.GetPluginInstance("Scytale");
            PluginTestScenario scenario = new PluginTestScenario(pluginInstance, new[] { "InputString", ".StickSize", ".Action" }, new[] { "OutputString" });
            object[] output;

            foreach (TestVector vector in testvectors)
            {
                output = scenario.GetOutputs(new object[] { vector.input, vector.size, vector.action });
                Assert.AreEqual(vector.output, (string)output[0], "Unexpected value in test #" + vector.n + ".");
            }
        }

        private struct TestVector
        {
            public string input, output;
            public int size, action;
            public int n;
        }

        // Fixed CT2-format vectors. The old CT1 layout is intentionally not used.
        private readonly TestVector[] testvectors = new TestVector[] {
            new TestVector () { n=0, action=0, size=4, input="HELPMEIAMUNDERATTACK", output="HENTEIDTLAEAPMRCMUAK" },
            new TestVector () { n=1, action=1, size=4, input="HENTEIDTLAEAPMRCMUAK", output="HELPMEIAMUNDERATTACK" },
        };

    }
}
