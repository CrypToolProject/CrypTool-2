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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace UnitTests
{
    [TestClass]
    public class SosemanukTest
    {
        // The complete eSTREAM vector corpus is retained as an embedded TSV.
        // A single case per key/IV length plus all offsets of the longest case
        // keeps this regular unit test fast and avoids a huge C# field initializer.
        [TestMethod]
        public void SosemanukMatchesPublishedVectorsAcrossLengthsAndOffsets()
        {
            CrypTool.PluginBase.ICrypComponent plugin = TestHelpers.GetPluginInstance("Sosemanuk");
            PluginTestScenario scenario = new PluginTestScenario(plugin,
                new[] { "InputData", "InputKey", "InputIV" }, new[] { "OutputData" });
            HashSet<string> lengthPairs = new HashSet<string>();
            HashSet<int> longerOffsets = new HashSet<int>();
            int total = 0;
            int checkedVectors = 0;

            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("UnitTests.TestVectors.Sosemanuk.tsv");
            Assert.IsNotNull(stream, "The Sosemanuk test vectors must be embedded in UnitTests.dll.");
            using (StreamReader reader = new StreamReader(stream))
            {
                Assert.AreEqual("n\tkey\tiv\toffset\toutput", reader.ReadLine());
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    total++;
                    string[] fields = line.Split('\t');
                    Assert.AreEqual(5, fields.Length, "Malformed Sosemanuk vector #" + total);
                    int number = int.Parse(fields[0], CultureInfo.InvariantCulture);
                    int offset = int.Parse(fields[3], CultureInfo.InvariantCulture);
                    int keyLength = fields[1].Length / 2;
                    int ivLength = fields[2].Length / 2;
                    string lengthPair = keyLength + "/" + ivLength;

                    bool checkLength = offset == 0 && lengthPairs.Add(lengthPair);
                    bool checkOffset = keyLength == 32 && ivLength == 16 &&
                        offset != 0 && longerOffsets.Add(offset);
                    if (!checkLength && !checkOffset)
                    {
                        continue;
                    }

                    byte[] input = new byte[offset + fields[4].Length / 2];
                    object[] output = scenario.GetOutputs(new object[]
                    {
                        input, fields[1].HexToByteArray(), fields[2].HexToByteArray()
                    });
                    string actual = output[0].ToHex();
                    Assert.AreEqual(fields[4].ToUpperInvariant(), actual.Substring(offset * 2),
                        "Unexpected output in Sosemanuk vector #" + number);
                    checkedVectors++;
                }
            }

            Assert.AreEqual(8896, total, "The eSTREAM vector corpus is incomplete.");
            Assert.AreEqual(24, lengthPairs.Count, "A key/IV length combination is missing.");
            Assert.AreEqual(6, longerOffsets.Count, "A nonzero offset is missing.");
            Assert.AreEqual(30, checkedVectors);
        }
    }
}
