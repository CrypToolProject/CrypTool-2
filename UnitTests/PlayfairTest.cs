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
    public class PlayfairTest
    {
        public PlayfairTest()
        {
        }

        [TestMethod]
        public void PlayfairMatchesExpectedVectors()
        {
            CrypTool.PluginBase.ICrypComponent pluginInstance = TestHelpers.GetPluginInstance("Playfair");
            PluginTestScenario scenario = new PluginTestScenario(pluginInstance, new[] { "InputString", ".KeyPhraseString", ".MatrixSize" }, new[] { "OutputString" });
            object[] output;

            foreach (TestVector vector in testvectors)
            {
                output = scenario.GetOutputs(new object[] { vector.input, vector.key, vector.size });
                Assert.AreEqual(vector.output, (string)output[0], "Unexpected value in test #" + vector.n + ".");
            }

        }

        private struct TestVector
        {
            public string input, key, output;
            public int size;
            public int n;
        }

        //
        // Source of the test vectors: http://en.wikipedia.org/wiki/Talk%3APlayfair_cipher
        //
        private readonly TestVector[] testvectors = new TestVector[] {
            new TestVector () { n=0, size=0, key="playfairexample", input="Hide the gold in the tree stump", output="BMODZBXDNABEKUDMUIXMMOUVIF" },
            //new TestVector () { n=1, size=0, key="playfair example", input="Hide the gold in the tree stump", output="BMODZBXDNABEKUDMUIXMMOUVIF" },
        };

    }
}