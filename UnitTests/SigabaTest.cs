using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Tests.TemplateAndPluginTests
{
    [TestClass]
    public class SigabaTest
    {
        public SigabaTest()
        {
        }

        [TestMethod]
        public void SigabaTestMethod()
        {
            var pluginInstance = TestHelpers.GetPluginInstance("Sigaba");
            var scenario = new PluginTestScenario(pluginInstance, new[] { "InputString", ".Type", ".Action",
                ".CipherRotor1", ".CipherRotor2", ".CipherRotor3", ".CipherRotor4", ".CipherRotor5",
                ".CipherRotor1Reverse", ".CipherRotor2Reverse", ".CipherRotor3Reverse", ".CipherRotor4Reverse", ".CipherRotor5Reverse",
                ".CipherKey",
                ".ControlRotor1", ".ControlRotor2", ".ControlRotor3", ".ControlRotor4", ".ControlRotor5",
                ".ControlRotor1Reverse", ".ControlRotor2Reverse", ".ControlRotor3Reverse", ".ControlRotor4Reverse", ".ControlRotor5Reverse",
                ".ControlKey",
                ".IndexRotor1", ".IndexRotor2", ".IndexRotor3", ".IndexRotor4", ".IndexRotor5",
                ".IndexRotor1Reverse", ".IndexRotor2Reverse", ".IndexRotor3Reverse", ".IndexRotor4Reverse", ".IndexRotor5Reverse",
                ".IndexKey"
            }, new[] { "OutputString" });

            foreach (TestVector vector in testvectors)
            {
                // Encryption
                object[] output = scenario.GetOutputs(new object[] { vector.input.ToUpper().Replace(" ", ""), 0, 0,
                    vector.cipherrotors[0], vector.cipherrotors[1], vector.cipherrotors[2], vector.cipherrotors[3], vector.cipherrotors[4],
                    vector.cipherorientation[0]=='1', vector.cipherorientation[1]=='1', vector.cipherorientation[2]=='1', vector.cipherorientation[3]=='1', vector.cipherorientation[4]=='1',
                    vector.cipherposition,
                    vector.controlrotors[0], vector.controlrotors[1], vector.controlrotors[2], vector.controlrotors[3], vector.controlrotors[4],
                    vector.controlorientation[0]=='1', vector.controlorientation[1]=='1', vector.controlorientation[2]=='1', vector.controlorientation[3]=='1', vector.controlorientation[4]=='1',
                    vector.controlposition,
                    vector.indexrotors[0], vector.indexrotors[1], vector.indexrotors[2], vector.indexrotors[3], vector.indexrotors[4],
                    vector.indexorientation[0]=='1', vector.indexorientation[1]=='1', vector.indexorientation[2]=='1', vector.indexorientation[3]=='1', vector.indexorientation[4]=='1',
                    vector.indexposition
                }, false);
                Assert.AreEqual(vector.output.ToUpper().Replace(" ", ""), output[0], "Unexpected value in encryption test #" + vector.n + ".");

                // Decryption
                output = scenario.GetOutputs(new object[] { vector.output.ToUpper().Replace(" ", ""), 0, 1, /* decrypt */
                    vector.cipherrotors[0], vector.cipherrotors[1], vector.cipherrotors[2], vector.cipherrotors[3], vector.cipherrotors[4],
                    vector.cipherorientation[0]=='1', vector.cipherorientation[1]=='1', vector.cipherorientation[2]=='1', vector.cipherorientation[3]=='1', vector.cipherorientation[4]=='1',
                    vector.cipherposition,
                    vector.controlrotors[0], vector.controlrotors[1], vector.controlrotors[2], vector.controlrotors[3], vector.controlrotors[4],
                    vector.controlorientation[0]=='1', vector.controlorientation[1]=='1', vector.controlorientation[2]=='1', vector.controlorientation[3]=='1', vector.controlorientation[4]=='1',
                    vector.controlposition,
                    vector.indexrotors[0], vector.indexrotors[1], vector.indexrotors[2], vector.indexrotors[3], vector.indexrotors[4],
                    vector.indexorientation[0]=='1', vector.indexorientation[1]=='1', vector.indexorientation[2]=='1', vector.indexorientation[3]=='1', vector.indexorientation[4]=='1',
                    vector.indexposition
                }, false);
                Assert.AreEqual(vector.input.ToUpper().Replace(" ", ""), output[0], "Unexpected value in decryption test #" + vector.n + ".");
            }
        }

        struct TestVector
        {
            public int[] cipherrotors, controlrotors, indexrotors;
            public string cipherposition, controlposition, indexposition;
            public string cipherorientation, controlorientation, indexorientation;
            public string input, output;
            public int n;
        }

        //
        // Source of the test vectors: "The SIGABA Simulator" (Version 1.0.1 build 001) by Geoff Sullivan
        //
        TestVector[] testvectors = new TestVector[] { 
            new TestVector () { n=1, cipherrotors = new int[] {10,1,2,3,4}, cipherposition = "AAAAA", cipherorientation = "00000", controlrotors = new int[] {9,8,7,6,5}, controlposition = "AAAAA", controlorientation = "00000", indexrotors = new int[] {1,2,3,4,5}, indexposition = "00000", indexorientation = "00000", input = new string('A',110), output = "XEWLG AEBVD KOBNS UUZBG FHCSM FLCZS TOVIX CZXFN KYYRD URUHD VUHIK ZEJLD OENKP PEUPH AXCBW DNYOQ YAUSS WWYOZ NKDXD IQFMP CLLFU TSXQL" },
            new TestVector () { n=2, cipherrotors = new int[] {10,1,2,3,4}, cipherposition = "AAAAA", cipherorientation = "00000", controlrotors = new int[] {9,8,7,6,5}, controlposition = "AAAAA", controlorientation = "00000", indexrotors = new int[] {1,2,3,4,5}, indexposition = "00000", indexorientation = "10000", input = new string('A',110), output = "XIXUK MCFOF ETQRF FSPRW BAYVW QYWVL HUCYL OPQTA CFQMF XBDRC PBDAG POFOT QYTUK YGWGD XCNUH TOAJE RVQIK SQPTI ZWRQU LIZXU WEFLJ IMCZH" },
            new TestVector () { n=3, cipherrotors = new int[] {10,1,2,3,4}, cipherposition = "AAAAA", cipherorientation = "00000", controlrotors = new int[] {9,8,7,6,5}, controlposition = "AAAAA", controlorientation = "00000", indexrotors = new int[] {1,2,3,4,5}, indexposition = "00000", indexorientation = "11111", input = new string('A',110), output = "XPCYI VPMQK HVKXK KXFZR FNJHI NUQIG EQADF ZWNTI OKBMF JZSVO TLAGR UAAIY IFPIT KUTRO VFVRC KIXOX PFHDJ HIKMO BQFLI EEAJT IPEFD KXMYW" },
            new TestVector () { n=4, cipherrotors = new int[] {10,1,2,3,4}, cipherposition = "AAAAB", cipherorientation = "00000", controlrotors = new int[] {9,8,7,6,5}, controlposition = "AAAAA", controlorientation = "00000", indexrotors = new int[] {1,2,3,4,5}, indexposition = "00000", indexorientation = "00000", input = new string('A',100), output = "TFORH PBTNV QGTTM DOPTE OZPYS VDWAK LJNCP OODLW CWQXV KXKDN GAWXG EWBHV KJFCH HKQZU BUDHL JDNBG IGZFD BMQPR TGYGI ANKSL" },
            new TestVector () { n=5, cipherrotors = new int[] {10,1,2,3,4}, cipherposition = "AAAAA", cipherorientation = "00000", controlrotors = new int[] {9,8,7,6,5}, controlposition = "AAAAB", controlorientation = "00000", indexrotors = new int[] {1,2,3,4,5}, indexposition = "00000", indexorientation = "00000", input = new string('A',100), output = "XBRZU AMFDW GXWEP NOBPS AMAUQ EENIL ZUQHG SICSS OXZIJ DAOJL UEHHX KINYU LNIGX HBPNN DZJRG LVCON UCJCZ PCEJN CNQAR JNZSQ" },
            new TestVector () { n=6, cipherrotors = new int[] {10,1,2,3,4}, cipherposition = "AAAAA", cipherorientation = "00000", controlrotors = new int[] {9,8,7,6,5}, controlposition = "AAAAA", controlorientation = "00000", indexrotors = new int[] {1,2,3,4,5}, indexposition = "10000", indexorientation = "00000", input = new string('A',100), output = "XPBJS RIGYZ MFBPH GDYRO ZAJGJ CCSLR ZBOGT EFFCK YEZYX XRNPI NHZPB LDXYI WGDTB IGTGL VBLOR UFACG IHSVM YEJMF PDMPQ YIOTY" },
            new TestVector () { n=7, cipherrotors = new int[] {10,1,2,3,4}, cipherposition = "AAAAA", cipherorientation = "00001", controlrotors = new int[] {9,8,7,6,5}, controlposition = "AAAAA", controlorientation = "00000", indexrotors = new int[] {1,2,3,4,5}, indexposition = "00000", indexorientation = "00000", input = new string('A',100), output = "CQOWY KDTOV YSYND MFJUH ZECGT PINRK LQNTP ZJETF OZVCV ECEMO KFRSP YBBWW TDFHH HPZAH SWOBG DXIOA JHTSH VGRGS BVFPC FPETA" },
            new TestVector () { n=8, cipherrotors = new int[] {10,1,2,3,4}, cipherposition = "AAAAA", cipherorientation = "00000", controlrotors = new int[] {9,8,7,6,5}, controlposition = "AAAAA", controlorientation = "10000", indexrotors = new int[] {1,2,3,4,5}, indexposition = "00000", indexorientation = "00000", input = new string('A',100), output = "XBRGY EDUJS RKRNN EWNPO YURLZ FEFQM MJGBI UPWIP SEALR CQOUR PKXFF CAQIS JGYWG GEUZY HKXSQ RDWLP NATSL TCHMX KABNF FHRGF" },
            new TestVector () { n=9, cipherrotors = new int[] {10,1,2,3,4}, cipherposition = "AAAAA", cipherorientation = "00001", controlrotors = new int[] {9,8,7,6,5}, controlposition = "AAAAA", controlorientation = "10000", indexrotors = new int[] {1,2,3,4,5}, indexposition = "00000", indexorientation = "00000", input = new string('A',100), output = "CPLSQ BINJS CYCNN FLBPL NTRDR EOPAJ JIRTT BIDMZ LDSKR UISUC BPMCZ MPNJG GFKKV VPEWD GVPUA BOYXA ZULRK SVIWW LLMYU QWCRJ" },
            new TestVector () { n=10, cipherrotors = new int[] {10,1,2,3,4}, cipherposition = "OOOOO", cipherorientation = "00000", controlrotors = new int[] {9,8,7,6,5}, controlposition = "OOOOO", controlorientation = "00000", indexrotors = new int[] {1,2,3,4,5}, indexposition = "00000", indexorientation = "00000", input = new string('A',100), output = "OKSHK NYRLP JYAWM NSDIW HCQWX NVQKA MYTTW SRXIR NGFPL CJQHL BDMMA YBBQK QXHDY ACWZH ARYRS CZIZE OWCZX QFUBM BHZMG QEKQK" },
            new TestVector () { n=11, cipherrotors = new int[] {1,10,4,3,6}, cipherposition = "ZDJWF", cipherorientation = "01101", controlrotors = new int[] {8,2,7,9,5}, controlposition = "GTHSR", controlorientation = "01101", indexrotors = new int[] {5,1,2,4,3}, indexposition = "93224", indexorientation = "00000", input = string.Concat(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYX", 5)), output = "XPSMB JVRXL PILEP VRTKK AXZDL DQBOY JWAYM JDXLN FSXNF AGNPA DNDCD HZKVO FAMMB OKQVR KOXNR ZMECK ZIIPX JYONE IDDFC KWOIK XDJTI DRYKM YBPID PTVOF JPAOT TJZIA" },
            new TestVector () { n=12, cipherrotors = new int[] {1,10,4,3,6}, cipherposition = "ZDJWF", cipherorientation = "01101", controlrotors = new int[] {8,2,7,9,5}, controlposition = "GTHSR", controlorientation = "01101", indexrotors = new int[] {5,1,2,4,3}, indexposition = "93224", indexorientation = "11000", input = string.Concat(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYX", 5)), output = "XVJDT HEIIH CXPGE MCPJD UGKLZ AAGHM KVMXN NDBIZ DATLL PHMFB DEAKT PYYMY HQURZ VWSMX DWXKD QWBCD OJPNV IVPXW HNJLU VYXMQ YUJMW FOXYT MSTMD VFQQN XQJNQ TNSVO" },
        };
    }
}