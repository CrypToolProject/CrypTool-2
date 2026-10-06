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
    public class PurpleTest
    {
        public PurpleTest()
        {
        }

        [TestMethod]
        public void PurpleMatchesExpectedVectors()
        {
            CrypTool.PluginBase.ICrypComponent pluginInstance = TestHelpers.GetPluginInstance("Purple");
            PluginTestScenario scenario = new PluginTestScenario(pluginInstance, new[] { "Text", ".Action", ".Alphabet", ".Motion", ".Sixes", ".Twenties", ".Twenties2", ".Twenties3", ".UnknownSymbolHandling", ".CaseHandling", ".OutputFormatting" }, new[] { "OutputString" });
            object[] output;

            foreach (TestVector vector in testvectors)
            {
                // encrypt
                output = scenario.GetOutputs(new object[] { vector.input, 0, vector.alphabet, vector.motion, vector.sixes, vector.twenties, vector.twenties2, vector.twenties3, vector.unknownSymbolHandling, vector.caseHandling, vector.outputFormatting });
                Assert.AreEqual(vector.output.ToUpper(), output[0], "Unexpected value in test #" + vector.n + ".");
                // decrypt
                output = scenario.GetOutputs(new object[] { vector.output, 1, vector.alphabet, vector.motion, vector.sixes, vector.twenties, vector.twenties2, vector.twenties3, vector.unknownSymbolHandling, vector.caseHandling, vector.outputFormatting });
                Assert.AreEqual(vector.input.ToUpper(), output[0], "Unexpected value in test #" + vector.n + ".");
            }

        }

        private struct TestVector
        {
            public string input, output, alphabet;
            public int n, sixes, twenties, twenties2, twenties3, motion, unknownSymbolHandling, caseHandling, outputFormatting;
        }

        //
        // Source of the test vectors: http://cryptocellar.org/simula/purple/index.html
        //
        private readonly TestVector[] testvectors = new TestVector[] {
            new TestVector () { n=0,
                sixes = 9,
                twenties = 1,
                twenties2 = 24,
                twenties3 = 6,
                alphabet="NOKTYUXEQLHBRMPDICJASVWGZF",
                motion=231,
                unknownSymbolHandling = 1,
                caseHandling = 0,
                outputFormatting = 0,
                output="ZTXOD NWKCC MAVNZ XYWEE TUQTC IMNVE UVIWB LUAXR RTLVA RGNTP CNOIU PJLCI VRTPJ KAUHV MUDTH KTXYZ ELQTV WGBUH FAWSH ULBFB HEXMY HFLOW D-KWH KKNXE BVPYH HGHEK XIOHQ HUHWI KYJYH PPFEA LNNAK IBOOZ NFRLQ CFLJT TSSDD O-OCV T-ZCK QTSHX TIJCN WXOKU FNQR- TAOIH WTATW VHOTG CGAKV ANKZA NMUIN",
                input="FOVTA TAKID ASINI MUIMI NOMOX IWOIR UBESI FYXXF CKZZR DXOOV BTNFY XFAEM EMORA NDUMF IOFOV OOMOJ IBAKA RIFYX RAICC YLFCB BCFCT HEGOV E-NME NTOFJ APANL FLPRO MPTED BYAGE NUINE DESIR ETOCO METOA NAMIC ABLEU NDERS T-NDI N-WIT HTHEG OVERN MENTO FTHE- NITED STATE SINOR DERTH ATTHE TWOCO"
            },
        };

    }
}