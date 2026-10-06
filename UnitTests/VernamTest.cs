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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CrypTool.PluginBase.IO;
using CrypTool.Plugins.Cryptography.Encryption;

namespace Tests.TemplateAndPluginTests
{
    [TestClass]
    public class VernamTest
    {
        public VernamTest()
        {
        }

        [TestMethod]
        public void VernamMatchesExpectedVectors()
        {
            var pluginInstance = TestHelpers.GetPluginInstance("Vernam");
            var scenario = new PluginTestScenario(pluginInstance, new[] { "InputString", "KeyString", "CipherMode", "UnknownSymbolHandling"}, new[] { "OutputString" });
            object[] output;

            foreach (TestVector vector in testvectors)
            {
                output = scenario.GetOutputs(new object[] { vector.input, vector.key, vector.cipherMode, vector.UnknownSymbolHandling });
                Assert.AreEqual(vector.output, (string)output[0], "Unexpected value in test #" + vector.n + ".");
            }
        }

        struct TestVector
        {
            public string input, key, output;
            public int n, cipherMode, UnknownSymbolHandling;
        }

        TestVector[] testvectors = new TestVector[] {
            new TestVector () { input="Franz jagt im komplett verwahrlosten Taxi quer durch Bayern", key = "das ist ein Key mit erlaubten Zeichen", output = "IrsmHrC kBmhWdInyxEdxKkvysPeuq sAvlrmWaPhhINdvhqt1gF9NiRdvE", cipherMode = 0, UnknownSymbolHandling = 0},
        };

    }
}
