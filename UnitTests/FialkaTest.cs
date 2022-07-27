using CrypTool.Fialka;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class FialkaTest
    {
        static FialkaTest()
        {
            //make sure, that the test environment finds the Fialka.dll
            TestHelpers.SetAssemblyPaths();
        }

        #region Data structures

        private struct TestVectorSimpleInOut
        {
            public int testCase;
            public string input, output;
        }

        private struct TestVectorAdvancedSettings
        {
            public int testCase;
            public string input, output;
            public object model;
            public object layout;
            public object rTypes;
            public object rSeries;
            public object numLock;
            public int[] PunchCard, RotorOrder, RotorOffset, RingOffset, CoreOrder, CoreOrientation, CoreOffset;
        }

        #endregion

        #region Encryption and decryption test
        // simple test the PT/CT for default settings
        [TestMethod]
        public void FialkaTestMethodInOut()
        {

            // encryption
            foreach (TestVectorSimpleInOut vector in _testvectorsSimpleInOut)
            {
                CrypTool.PluginBase.ICrypComponent pluginInstance = TestHelpers.GetPluginInstance("Fialka");
                PluginTestScenario scenario = new PluginTestScenario(pluginInstance, new[] { "Input" }, new[] { "Output" });

                object[] output = scenario.GetOutputs(new object[] { vector.input.ToUpper() }, false);
                Assert.AreEqual(vector.output.ToUpper(), (string)output[0], "Unexpected value in test #" + vector.testCase + " encryption.");
            }

            // decrytpion
            foreach (TestVectorSimpleInOut vector in _testvectorsSimpleInOut)
            {
                CrypTool.PluginBase.ICrypComponent pluginInstance = TestHelpers.GetPluginInstance("Fialka");
                PluginTestScenario scenario = new PluginTestScenario(pluginInstance, new[] { "Input", ".OperationMode" }, new[] { "Output" });

                object[] output = scenario.GetOutputs(new object[] { vector.output.ToUpper(), FialkaEnums.operationMode.Decrypt }, false);
                Assert.AreEqual(vector.input.ToUpper(), (string)output[0], "Unexpected value in test #" + vector.testCase + " decryption.");
            }
        }

        private readonly TestVectorSimpleInOut[] _testvectorsSimpleInOut = new TestVectorSimpleInOut[] {

            new TestVectorSimpleInOut () { // short, no space
                testCase = 0,
                input  = "TESTTESTTESTTEST",
                output = "PH7FR2EMWW8B8XRA",
                // M-125, PROTON I, Czechoslovakian layouts, Latin print head, no Punch Card, base offsets, default order
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov
           },

            new TestVectorSimpleInOut () { // long with space
                testCase = 1,
                input  = "THE EARLIEST FORMS OF SECRET WRITING REQUIRED LITTLE MORE THAN WRITING IMPLEMENTS SINCE MOST PEOPLE COULD NOT READ MORE LITERACY OR LITERATE OPPONENTS REQUIRED ACTUAL CRYPTOGRAPHY THE MAIN CLASSICAL CIPHER TYPES ARE TRANSPOSITION CIPHERS WHICH REARRANGE THE ORDER OF LETTERS IN A MESSAGE EG HELLO WORLD BECOMES EHLOL OWRDL IN A TRIVIALLY SIMPLE REARRANGEMENT SCHEME AND SUBSTITUTION CIPHERS WHICH SYSTEMATICALLY REPLACE LETTERS OR GROUPS OF LETTERS WITH OTHER LETTERS OR GROUPS OF LETTERS EG FLY AT ONCE BECOMES GMZ BU PODF BY REPLACING EACH LETTER WITH THE ONE FOLLOWING IT IN THE LATIN ALPHABET SIMPLE VERSIONS OF EITHER HAVE NEVER OFFERED MUCH CONFIDENTIALITY FROM ENTERPRISING OPPONENTS",
                output = "PE5MPWY5MW8BDMLLWXGRJMMM7VDNREZIDBHQKYUWOEJOFEWE82PREHXIX2SLHD5TYE7NS8RIPSPHZDVCD52WCGJSWSUUGQGWGD7K7NNE5G2FPQWGOLXJVPPQLOQYBVVQZQDGNW8VHGXOLBFCWTJPXQTKN5Z22NXEPNVUTVEDYDYEOXGL2N7FWPON2CF8FPVHY2JCMZRDVPEYL5CMFVQWMQBKKERDA5YXZ2HXSSERX7PZU7DTOMJMKDMHSYEC8ZJYYTWPQRTBJAKPS8QL5G7HLKYRQOBDE8QGDZUHEKPVR7DMHELAKIRASMHFTP87W2UEPZE8AR7FSTOWITJDTMJUNVQJ2GSKKACIZHW8ZICIMNTVHDQMAIMYCQN72IX2EMPMH5OKK2YNJX2NSSHEKKE8HFI2CCAGCAKAYLZXKCGNB8BG577QXEAEYXLPJCTAIOXHJVBPMVBAJJMLVHVOL2AKXA2JFQSAX8PRHERHEYPEKYBACFFMFINC5VNNOEEV7PSEHFQZBWCRJJDE2RSITGVXA2J5EGJHKZWAYHIWG878HO7NRRUJ5VHGQCE8OZ8ZCIZCXIBQOXFLMTD7APHS7RT7K8SAGEX5RDBXOLTKLQFKXLN2ASQJRFLEAYLEKLGYXBYSKLQW85DB57F7WKG7ZLTUVACUSQENIDSQDDS7BZ5AKIWBPT8N2G",
                // M-125, PROTON I, Czechoslovakian layouts, Latin print head, no Punch Card, base offsets, default order
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov
            }


        };
        #endregion


        #region PROTON I test
        [TestMethod]
        public void FialkaTestMethodSettingsProtonI()
        {

            CrypTool.PluginBase.ICrypComponent pluginInstance = TestHelpers.GetPluginInstance("Fialka");
            PluginTestScenario scenario = new PluginTestScenario(pluginInstance, new[] { "Input", ".MachineModel", ".CountryLayout", ".RotorType", ".RotorSeries",
                    ".RotorOrder", ".RotorOffsets", ".RotorRingOffsets", ".PunchCard" }, new[] { "Output" });
            foreach (TestVectorAdvancedSettings vector in _testvectorsProtonI)
            {
                object[] output = scenario.GetOutputs(new object[]
                {
                    vector.input.ToUpper(), vector.model, vector.layout, vector.rTypes, vector.rSeries,
                    vector.RotorOrder, vector.RotorOffset, vector.RingOffset, vector.PunchCard
                }, false);
                Assert.AreEqual(vector.output.ToUpper(), (string)output[0], "Unexpected value in test #" + vector.testCase + " encryption.");
            }
        }

        private readonly TestVectorAdvancedSettings[] _testvectorsProtonI = new TestVectorAdvancedSettings[] {

            new TestVectorAdvancedSettings () {
                testCase = 2,
                input  = "WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8",
                output = "QNEAJPXLKPUP2EF75BJFBTAPEW722AIPNWDKQYA2UKGK82G7GDVV8WINKS252GHKXEKGG75GJPCSRWTWHEOBV2V",
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.Czechoslovakia,
                rTypes = FialkaEnums.rotorTypes.PROTON_I,
                rSeries = FialkaEnums.rotorSeries.K6,
                RotorOrder = FialkaConstants.baseRotorPositions(),
                RotorOffset = FialkaConstants.nullOffset(),
                RingOffset = FialkaConstants.nullOffset(),
                //CoreOrder = FialkaConstants.baseRotorPositions(),
                //CoreOrientation = new int[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                //CoreOffset = FialkaConstants.nullOffset(),
                PunchCard = FialkaConstants.punchCardIdentity()
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov         
            },
            new TestVectorAdvancedSettings () {
                testCase = 3,
                input  = "WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8",
                output = "KHAFL5HCHSVO2MMHQYE2IT7CFB2QZEJQDHWZVSJN7RQZBNU8GGKWQAEGPKIXF2YPZLLQLESCHZOSFYYEPLCGFZH",
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.Czechoslovakia,
                rTypes = FialkaEnums.rotorTypes.PROTON_I,
                rSeries = FialkaEnums.rotorSeries.K6,
                RotorOrder = FialkaConstants.baseRotorPositions(),
                RotorOffset = FialkaConstants.baseRotorPositions(),
                RingOffset = FialkaConstants.nullOffset(),
                //CoreOrder = FialkaConstants.baseRotorPositions(),
                //CoreOrientation = new int[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                //CoreOffset = FialkaConstants.nullOffset(),
                PunchCard = FialkaConstants.punchCardIdentity()
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov
            },
            new TestVectorAdvancedSettings () {
                testCase = 4,
                input  = "WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8",
                output = "RRKTEWLCKARQEEFFUMYBLPFPQQTYOYLS2DS5CMQAFKVYJBSGZEFCBCHOY5VEYETLX8NVHXUXCE8GGWWIYLLK82K",
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.Czechoslovakia,
                rTypes = FialkaEnums.rotorTypes.PROTON_I,
                rSeries = FialkaEnums.rotorSeries.K6,
                RotorOrder = FialkaConstants.baseRotorPositions(),
                RotorOffset = FialkaConstants.nullOffset(),
                RingOffset = FialkaConstants.nullOffset(),
                //CoreOrder = FialkaConstants.baseRotorPositions(),
                //CoreOrientation = new int[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                //CoreOffset = FialkaConstants.nullOffset(),
                PunchCard = new int[]{1,0, 3,2, 5,4, 7,6, 9,8, 11,10, 13,12, 15,14, 17,16, 19,18, 21,20, 23,22, 25,24, 27,26, 29,28}
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov
         
            },
            new TestVectorAdvancedSettings () {
                testCase = 5,
                input  = "WERTZUIOPQ7SDFGHJKL5YCVBNMA8WRTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8",
                output = "XDSODQEHF8HOEJXXDNPLCJPNMPLQTLKYEEWKOXTLQPGME2AJ8ZAY87MKHJEGXHRGZFIXQOAYA5ERKALN5L5V2",
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.Czechoslovakia,
                rTypes = FialkaEnums.rotorTypes.PROTON_I,
                rSeries = FialkaEnums.rotorSeries.K6,
                RotorOrder = FialkaConstants.baseRotorPositions(),
                RotorOffset = FialkaConstants.baseRotorPositions(),
                RingOffset = FialkaConstants.nullOffset(),
                //CoreOrder = FialkaConstants.baseRotorPositions(),
                //CoreOrientation = new int[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                //CoreOffset = FialkaConstants.nullOffset(),
                PunchCard = new int[]{1,0, 3,2, 5,4, 7,6, 9,8, 11,10, 13,12, 15,14, 17,16, 19,18, 21,20, 23,22, 25,24, 27,26, 29,28}
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov       
            }
        };
        #endregion


        #region PROTON II test

        [TestMethod]
        public void FialkaTestMethodSettingsProtonII()
        {
            CrypTool.PluginBase.ICrypComponent pluginInstance = TestHelpers.GetPluginInstance("Fialka");
            PluginTestScenario scenario = new PluginTestScenario(pluginInstance, new[] { "Input", ".MachineModel", ".CountryLayout", ".RotorType", ".RotorSeries",
                    ".RotorOrder", ".RotorOffsets", ".RotorRingOffsets", ".PunchCard",
                    ".RotorCoreOrders", ".RotorCoreOffsets", ".RotorCoreOrientations"
            }, new[] { "Output" });
            foreach (TestVectorAdvancedSettings vector in _testvectorsProtonII)
            {
                object[] output = scenario.GetOutputs(new object[]
                {
                    vector.input.ToUpper(), vector.model, vector.layout, vector.rTypes, vector.rSeries,
                    vector.RotorOrder, vector.RotorOffset, vector.RingOffset, vector.PunchCard,
                    vector.CoreOrder, vector.CoreOffset, vector.CoreOrientation
                }, false);
                Assert.AreEqual(vector.output.ToUpper(), (string)output[0], "Unexpected value in test #" + vector.testCase + " encryption.");
            }
        }

        private readonly TestVectorAdvancedSettings[] _testvectorsProtonII = new TestVectorAdvancedSettings[] {

            new TestVectorAdvancedSettings () {
                testCase = 6,
                input  = "WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8",
                output = "ND8ABTUUJMHNP75YHVUHYNRFMRMK77BBGQ7LHVRFUIDRRMICT8KD52YSVPADYHMAESPGAKTBEEOE77FGXMYAVDZ",
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.Czechoslovakia,
                rTypes = FialkaEnums.rotorTypes.PROTON_II,
                rSeries = FialkaEnums.rotorSeries.K6,
                RotorOrder = new int[] {9, 0, 8, 1, 7, 2, 6, 3, 5, 4},
                RotorOffset = FialkaConstants.nullOffset(),
                RingOffset = FialkaConstants.nullOffset(),
                CoreOrder = new int[] {9, 0, 8, 1, 7, 2, 6, 3, 5, 4},
                CoreOrientation = new int[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                CoreOffset = FialkaConstants.nullOffset(),
                PunchCard = FialkaConstants.punchCardIdentity()
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov         
            },
             new TestVectorAdvancedSettings () {
                testCase = 7,
                input  = "WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8",
                output = "HOONMOWEUBOEKTP8DHJBDJAEFJ5NAA5MFFEWOOO22LDYQQAOEEA5THIVTOYOE8DUOOH7IKPECYLQ2FJDLWLTEMH",
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.Czechoslovakia,
                rTypes = FialkaEnums.rotorTypes.PROTON_II,
                rSeries = FialkaEnums.rotorSeries.K6,
                RotorOrder = new int[] {9, 0, 8, 1, 7, 2, 6, 3, 5, 4},
                RotorOffset = FialkaConstants.baseRotorPositions(),
                RingOffset = FialkaConstants.nullOffset(),
                CoreOrder = new int[] {9, 0, 8, 1, 7, 2, 6, 3, 5, 4},
                CoreOrientation = new int[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                CoreOffset = FialkaConstants.nullOffset(),
                PunchCard = FialkaConstants.punchCardIdentity()
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov         
            },
             new TestVectorAdvancedSettings () {
                testCase = 8,
                input  = "WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8",
                output = "WZIVNPAZNGBRKD2VMKE8MJXSUA2EMMKMK77DWRFMWGDD2SE7EM5P7NKQEDNEQBPFDJEZ25DEDMINMBH8ZRMJN27",
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.Czechoslovakia,
                rTypes = FialkaEnums.rotorTypes.PROTON_II,
                rSeries = FialkaEnums.rotorSeries.K6,
                RotorOrder = new int[] {9, 0, 8, 1, 7, 2, 6, 3, 5, 4},
                RotorOffset = FialkaConstants.baseRotorPositions(),
                RingOffset = FialkaConstants.nullOffset(),
                CoreOrder = new int[] {9, 0, 8, 1, 7, 2, 6, 3, 5, 4},
                CoreOrientation = new int[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                CoreOffset = FialkaConstants.nullOffset(),
                PunchCard = new int[] {1,0, 3,2, 5,4, 7,6, 9,8, 11,10, 13,12, 15,14, 17,16, 19,18, 21,20, 23,22, 25,24, 27,26, 29,28}
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov         
            },
            new TestVectorAdvancedSettings () {
                testCase = 9,
                input  = "WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8",
                output = "NYOJOEP2YKX2XFJ5RFNBAYBHYE2NSSGKUTULBOTJQ8OHONCBFJRFW2V85DRDKCZHQTCGJGTWYHEA2C77OMN2NXJ",
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.Czechoslovakia,
                rTypes = FialkaEnums.rotorTypes.PROTON_II,
                rSeries = FialkaEnums.rotorSeries.K6,
                RotorOrder = new int[] {4, 5, 3, 6, 2, 7, 1, 8, 0, 9},
                RotorOffset = FialkaConstants.nullOffset(),
                RingOffset = FialkaConstants.nullOffset(),
                CoreOrder = new int[] {9, 0, 8, 1, 7, 2, 6, 3, 5, 4},
                CoreOrientation = new int[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                CoreOffset = FialkaConstants.nullOffset(),
                PunchCard = FialkaConstants.punchCardIdentity()
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov         
            },
            new TestVectorAdvancedSettings () {
                testCase = 10,
                input  = "WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8",
                output = "YNE2XIIO5RQ7JNZEVPAUJXJFLEZQIYPG8VN8EF7PAWPFIOQ7P88XHIVWNN5BUTNFRNUPVUJGGRFY8ELGQGDJ5NV",
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.Czechoslovakia,
                rTypes = FialkaEnums.rotorTypes.PROTON_II,
                rSeries = FialkaEnums.rotorSeries.K6,
                RotorOrder = FialkaConstants.baseRotorPositions(),
                RotorOffset = FialkaConstants.nullOffset(),
                RingOffset = FialkaConstants.nullOffset(),
                CoreOrder = FialkaConstants.baseRotorPositions(),
                CoreOrientation = new int[] {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                CoreOffset = FialkaConstants.nullOffset(),
                PunchCard = FialkaConstants.punchCardIdentity()
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov         
            },
            new TestVectorAdvancedSettings () {
                testCase = 11,
                input  = "WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8",
                output = "LHZHO75LWUQAWQ5XIP7UKZ5YBF8RBYTBGUFUZNGSUMDSLYEI5GP5OZTJNXTZN7CX8QZULYFPHF5NV7EDTVO5AA",
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.Czechoslovakia,
                rTypes = FialkaEnums.rotorTypes.PROTON_II,
                rSeries = FialkaEnums.rotorSeries.K6,
                RotorOrder = FialkaConstants.baseRotorPositions(),
                RotorOffset = FialkaConstants.baseRotorPositions(),
                RingOffset = FialkaConstants.nullOffset(),
                CoreOrder = FialkaConstants.baseRotorPositions(),
                CoreOrientation = new int[] {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                CoreOffset = FialkaConstants.nullOffset(),
                PunchCard = FialkaConstants.punchCardIdentity()
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov         
            },
            new TestVectorAdvancedSettings () {
                testCase = 12,
                input  = "WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8",
                output = "C8HFHWWYPA8ZXM7V8RZF2UTF88EENFDUQMZVJN8WIAQYDU7V8Z2CS28IJKS5HTYAWWKNGK8CDXEUROHNWXDSYES",
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.Czechoslovakia,
                rTypes = FialkaEnums.rotorTypes.PROTON_II,
                rSeries = FialkaEnums.rotorSeries.K6,
                RotorOrder = FialkaConstants.baseRotorPositions(),
                RotorOffset = FialkaConstants.baseRotorPositions(),
                RingOffset = FialkaConstants.nullOffset(),
                CoreOrder = new int[] {9, 0, 8, 1, 7, 2, 6, 3, 5, 4},
                CoreOrientation = new int[] {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                CoreOffset = FialkaConstants.nullOffset(),
                PunchCard = FialkaConstants.punchCardIdentity()
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov         
            },
            new TestVectorAdvancedSettings () {
                testCase = 13,
                input  = "WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8",
                output = "SRM7LGZCPJ7PW7IHN2T7CNJNQXJNUFRTZAN2MHAPBPOQWY5ARNJ7H7YEEQELGJV5HUJLDYJ2QG8WLIMDLI5WMCG",
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.Czechoslovakia,
                rTypes = FialkaEnums.rotorTypes.PROTON_II,
                rSeries = FialkaEnums.rotorSeries.K6,
                RotorOrder = new int[] {4, 5, 3, 6, 2, 7, 1, 8, 0, 9},
                RotorOffset = FialkaConstants.baseRotorPositions(),
                RingOffset = FialkaConstants.nullOffset(),
                CoreOrder = new int[] {9, 0, 8, 1, 7, 2, 6, 3, 5, 4},
                CoreOrientation = new int[] {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                CoreOffset = FialkaConstants.nullOffset(),
                PunchCard = new int[]{ 1,0, 3,2, 5,4, 7,6, 9,8, 11,10, 13,12, 15,14, 17,16, 19,18, 21,20, 23,22, 25,24, 27,26, 29,28}
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov         
            },
             new TestVectorAdvancedSettings () {
                testCase = 14,
                input  = "WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8",
                output = "QNEAJPXLKPUP2EF75BJFBTAPEW722AIPNWDKQYA2UKGK82G7GDVV8WINKS252GHKXEKGG75GJPCSRWTWHEOBV2V",
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.Poland,
                rTypes = FialkaEnums.rotorTypes.PROTON_II,
                rSeries = FialkaEnums.rotorSeries.K6,
                RotorOrder =  FialkaConstants.baseRotorPositions(),
                RotorOffset = FialkaConstants.nullOffset(),
                RingOffset = FialkaConstants.nullOffset(),
                CoreOrder = FialkaConstants.baseRotorPositions(),
                CoreOrientation = new int[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                CoreOffset = FialkaConstants.nullOffset(),
                PunchCard = FialkaConstants.punchCardIdentity()
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov         
            },

            new TestVectorAdvancedSettings () {
                testCase = 15,
                input  = "2ERTZUIOP3SDFGH4KL567CVBNM92ERTZUIOP3SDFGH4KL567CVBNM92ERTZUIOP3SDFGH4KL567CVBNM9",
                output = "3NE84P7LKP54K7CRV3B6E9ZJK78DHMCECE97O9VF9GZG6KPHT4IDFKE3JTG8KO2ZBHM3MO9ZLTU8LJLH3",
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.GDR,
                rTypes = FialkaEnums.rotorTypes.PROTON_II,
                rSeries = FialkaEnums.rotorSeries.K6,
                RotorOrder =  FialkaConstants.baseRotorPositions(),
                RotorOffset = FialkaConstants.nullOffset(),
                RingOffset = FialkaConstants.nullOffset(),
                CoreOrder = FialkaConstants.baseRotorPositions(),
                CoreOrientation = new int[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                CoreOffset = FialkaConstants.nullOffset(),
                PunchCard = FialkaConstants.punchCardIdentity()
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov   
                // !!! Fialka Simulator v5.16 by Vyacheslav Chernov BUG - KEYBOARD: A/8; PAPER OUT: 7/8; INPUT: 8/A; OUTPUT: A/8
            },
             new TestVectorAdvancedSettings () {
                testCase = 16,
                input  = "WERTYUIOPQ7SDFGHJKL5ZXCVBNMA8WERTYUIOPQ7SDFGHJKL5ZXCVBNMA8WERTYUIOPQ7SDFGHJKL5ZXCVBNMA8",
                output = "VHSNWII75JQUWON8GEJGVWIVGEXOVFN8BJV5PLGKGGWRA8BYRZODUXEYMR5V2DVOXPCOXQBFREGZEKLKU85OT77",
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.Czechoslovakia,
                rTypes = FialkaEnums.rotorTypes.PROTON_II,
                rSeries = FialkaEnums.rotorSeries.K3,
                RotorOrder =  FialkaConstants.baseRotorPositions(),
                RotorOffset = FialkaConstants.nullOffset(),
                RingOffset = FialkaConstants.nullOffset(),
                CoreOrder = FialkaConstants.baseRotorPositions(),
                CoreOrientation = new int[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                CoreOffset = FialkaConstants.nullOffset(),
                PunchCard = FialkaConstants.punchCardIdentity()
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov         
            },
            new TestVectorAdvancedSettings () {
                testCase = 17,
                input  = "WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8",
                output = "E5Z8SKP78SPDEQRSIY5XTCUM2MBXLMBVDWZE2E2LKMMR2OVYSZJ82MP8PKQ7T5WZPVZXHIESCYXNDMZVMI2QNJV",
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.Czechoslovakia,
                rTypes = FialkaEnums.rotorTypes.PROTON_II,
                rSeries = FialkaEnums.rotorSeries.K3,
                RotorOrder =  FialkaConstants.baseRotorPositions(),
                RotorOffset = FialkaConstants.nullOffset(),
                RingOffset = FialkaConstants.nullOffset(),
                CoreOrder = FialkaConstants.baseRotorPositions(),
                CoreOrientation = new int[] {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                CoreOffset = FialkaConstants.nullOffset(),
                PunchCard = FialkaConstants.punchCardIdentity()
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov         
            },
            new TestVectorAdvancedSettings () {
                testCase = 18,
                input  = "WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8",
                output = "VHSNTII75JQUWON8GEJGIWIVGEXOVFN8B5V5PLGKGGWRA8BYRAODUXEYMR5V2D7OXPCOXQBFREGZEKJKU85OT77",
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.Poland,
                rTypes = FialkaEnums.rotorTypes.PROTON_II,
                rSeries = FialkaEnums.rotorSeries.K3,
                RotorOrder =  FialkaConstants.baseRotorPositions(),
                RotorOffset = FialkaConstants.nullOffset(),
                RingOffset = FialkaConstants.nullOffset(),
                CoreOrder = FialkaConstants.baseRotorPositions(),
                CoreOrientation = new int[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                CoreOffset = FialkaConstants.nullOffset(),
                PunchCard = FialkaConstants.punchCardIdentity()
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov         
            },
            new TestVectorAdvancedSettings () {
                testCase = 19,
                input  = "2ERTZUIOP3SDFGH4KL567CVBNM92ERTZUIOP3SDFGH4KL567CVBNM92ERTZUIOP3SDFGH4KL567CVBNM9",
                output = "VHSNTIIA54O7NMKOK8O57CNKCBABTUFUV92GKVAOSFRV94T4FNDMC8BT3B8DONGHL5LJ62A8E5BCJ492G",
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.GDR,
                rTypes = FialkaEnums.rotorTypes.PROTON_II,
                rSeries = FialkaEnums.rotorSeries.K3,
                RotorOrder =  FialkaConstants.baseRotorPositions(),
                RotorOffset = FialkaConstants.nullOffset(),
                RingOffset = FialkaConstants.nullOffset(),
                CoreOrder = FialkaConstants.baseRotorPositions(),
                CoreOrientation = new int[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                CoreOffset = FialkaConstants.nullOffset(),
                PunchCard = FialkaConstants.punchCardIdentity()
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov 
                // !!! Fialka Simulator v5.16 by Vyacheslav Chernov BUG - KEYBOARD: A/8; PAPER OUT: 7/8; INPUT: 8/A; OUTPUT: A/8
            },
            new TestVectorAdvancedSettings () {
                testCase = 20,
                input  = "WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8",
                output = "I55GRSPBGMLRDGQS5SFIBJZHW5APCBJJPOZAWYPTNRWZIMNVNSUKAERFVGNMPF7T8KCDTT8GYF2A2FXX2ZXSCMB",
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.Czechoslovakia,
                rTypes = FialkaEnums.rotorTypes.PROTON_II,
                rSeries = FialkaEnums.rotorSeries.K5,
                RotorOrder =  FialkaConstants.baseRotorPositions(),
                RotorOffset = FialkaConstants.nullOffset(),
                RingOffset = FialkaConstants.nullOffset(),
                CoreOrder = FialkaConstants.baseRotorPositions(),
                CoreOrientation = new int[] {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1},//new int[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                CoreOffset = FialkaConstants.nullOffset(),
                PunchCard = FialkaConstants.punchCardIdentity()
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov  
                // !!! Actually there is a bug in the Fialka Simulator v5.16 by Vyacheslav Chernov, for 5K rotor series, the wiring for core side 1 and 2 are exchanged. 
                // !!! We used the wiring (and calculated the wiring for flipped cores) presented in http://www.cryptomuseum.com/crypto/fialka/m125_3/hu.htm.            
            },

            new TestVectorAdvancedSettings () {
                testCase = 21,
                input  = "WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8",
                output = "QMSVWNFOV2WWBRFMNOQPYUQYNIEKXABJZJQITXASZR7ZSVNTFUWXJUXFBNFRTPKWFQWXMVYCRTCSARYMPNFBYR8",
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.Czechoslovakia,
                rTypes = FialkaEnums.rotorTypes.PROTON_II,
                rSeries = FialkaEnums.rotorSeries.K5,
                RotorOrder =  FialkaConstants.baseRotorPositions(),
                RotorOffset = FialkaConstants.nullOffset(),
                RingOffset = FialkaConstants.nullOffset(),
                CoreOrder = FialkaConstants.baseRotorPositions(),
                CoreOrientation = new int[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},//new int[] {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                CoreOffset = FialkaConstants.nullOffset(),
                PunchCard = FialkaConstants.punchCardIdentity()
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov 
                // !!! Actually there is a bug in the Fialka Simulator v5.16 by Vyacheslav Chernov, for 5K rotor series, the wiring for core side 1 and 2 are exchanged. 
                // !!! We used the wiring (and calculated the wiring for flipped cores) presented in http://www.cryptomuseum.com/crypto/fialka/m125_3/hu.htm.
            },
          new TestVectorAdvancedSettings () {
                testCase = 22,
                input  = "WERTZUIOP7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7DFGHJKL5YCVBNMA8WRTZUIOPQ7DFGHJKL5YCVBNMA8",
                output = "I55GRSPBG8GOAFEWRTKKG2OJNQMSGOIXTCTW2NWM5SS5OUH7GV2WPMVXDJPBXZDFRJMKSYIW72R8XRUUU",
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.Poland,
                rTypes = FialkaEnums.rotorTypes.PROTON_II,
                rSeries = FialkaEnums.rotorSeries.K5,
                RotorOrder =  FialkaConstants.baseRotorPositions(),
                RotorOffset = FialkaConstants.nullOffset(),
                RingOffset = FialkaConstants.nullOffset(),
                CoreOrder = FialkaConstants.baseRotorPositions(),
                CoreOrientation = new int[] {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1},//new int[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                CoreOffset = FialkaConstants.nullOffset(),
                PunchCard = FialkaConstants.punchCardIdentity()
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov 
                // !!! Actually there is a bug in the Fialka Simulator v5.16 by Vyacheslav Chernov, for 5K rotor series, the wiring for core side 1 and 2 are exchanged. 
                // !!! We used the wiring (and calculated the wiring for flipped cores) presented in http://www.cryptomuseum.com/crypto/fialka/m125_3/hu.htm.
              },

        new TestVectorAdvancedSettings () {
                testCase = 23,
                input  = "2ERTZUIOP3SDFGH4KL567CVBNM92ERTZUIOP3SDFGH4KL567CVBNM92ERTZUIOP3SDFGH4KL567CVBNM9",
                output = "3MSV2NFOVJC8GN4FR8FIMIK2BBSBVL4FJBV7333JICM57HIZBHSNGC46D3ZL7E7VAI6PB9E6J7HTAG37K",
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.GDR,
                rTypes = FialkaEnums.rotorTypes.PROTON_II,
                rSeries = FialkaEnums.rotorSeries.K5,
                RotorOrder =  FialkaConstants.baseRotorPositions(),
                RotorOffset = FialkaConstants.nullOffset(),
                RingOffset = FialkaConstants.nullOffset(),
                CoreOrder = FialkaConstants.baseRotorPositions(),
                CoreOrientation = new int[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},//new int[] {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1}
                CoreOffset = FialkaConstants.nullOffset(),
                PunchCard = FialkaConstants.punchCardIdentity()
                 //Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov 
                 // !!! Fialka Simulator v5.16 by Vyacheslav Chernov BUG - KEYBOARD: A/8; PAPER OUT: 7/8; INPUT: 8/A; OUTPUT: A/8
                 // !!! Actually there is a bug in the Fialka Simulator v5.16 by Vyacheslav Chernov, for 5K rotor series, the wiring for core side 1 and 2 are exchanged. 
                 // !!! We used the wiring (and calculated the wiring for flipped cores) presented in http://www.cryptomuseum.com/crypto/fialka/m125_3/hu.htm.
            },

             new TestVectorAdvancedSettings () {
                testCase = 24,
                input  = "WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8WERTZUIOPQ7SDFGHJKL5YXCVBNMA8",
                output = "WZQ8CRK7FKUPFWUINWR2A5SQJMJTN72Z2RHNXEO2YGTHZXNIM8M5EXVN58EHYRZH2AUUORWUPPCERM8KFQGMWQX",
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.Czechoslovakia,
                rTypes = FialkaEnums.rotorTypes.PROTON_II,
                rSeries = FialkaEnums.rotorSeries.K6,
                RotorOrder =  new int[] {4, 5, 3, 6, 2, 7, 1, 8, 0, 9},
                RotorOffset = FialkaConstants.baseRotorPositions(),
                RingOffset = new int[] {9, 8, 7, 6, 5, 4, 3, 2, 1, 0},
                CoreOrder = new int[] {9, 0, 8, 1, 7, 2, 6, 3, 5, 4},
                CoreOrientation = new int[] {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                CoreOffset = new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9},
                PunchCard = new int[] {1,0, 3,2, 5,4, 7,6, 9,8, 11,10, 13,12, 15,14, 17,16, 19,18, 21,20, 23,22, 25,24, 27,26, 29,28}
                 //Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov 
               }
        };
        #endregion


        #region NUM LOCK 10 test

        [TestMethod]
        public void FialkaTestMethodNumbers()
        {
            CrypTool.PluginBase.ICrypComponent pluginInstance = TestHelpers.GetPluginInstance("Fialka");
            PluginTestScenario scenario = new PluginTestScenario(pluginInstance, new[] { "Input", ".NumLockType", ".MachineModel", ".CountryLayout", ".RotorType", ".RotorSeries",
                    ".RotorOrder", ".RotorOffsets", ".RotorRingOffsets", ".PunchCard",
                    ".RotorCoreOrders", ".RotorCoreOffsets", ".RotorCoreOrientations"
            }, new[] { "Output" });
            foreach (TestVectorAdvancedSettings vector in _testvectorsNumbers)
            {
                object[] output = scenario.GetOutputs(new object[]
                {
                    vector.input.ToUpper(), vector.numLock, vector.model, vector.layout, vector.rTypes, vector.rSeries,
                    vector.RotorOrder, vector.RotorOffset, vector.RingOffset, vector.PunchCard,
                    vector.CoreOrder, vector.CoreOffset, vector.CoreOrientation
                }, false);
                Assert.AreEqual(vector.output.ToUpper(), (string)output[0], "Unexpected value in test #" + vector.testCase + " encryption.");
            }
        }

        private readonly TestVectorAdvancedSettings[] _testvectorsNumbers = new TestVectorAdvancedSettings[]
        {

            new TestVectorAdvancedSettings()
            {
                testCase = 25,
                input = "0123456789012345678901234567890123456789001122334455667788999876543210246897531",
                output = "3715660133463688561596210999135493475042599859425419860013336849522769179536999",
                numLock = FialkaEnums.numLockType.NumLock10,
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.Poland,
                rTypes = FialkaEnums.rotorTypes.PROTON_II,
                rSeries = FialkaEnums.rotorSeries.K3,
                RotorOrder = FialkaConstants.baseRotorPositions(),
                RotorOffset = FialkaConstants.nullOffset(),
                RingOffset = FialkaConstants.nullOffset(),
                CoreOrder = FialkaConstants.baseRotorPositions(),
                CoreOrientation = new int[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                CoreOffset = FialkaConstants.nullOffset(),
                PunchCard = FialkaConstants.punchCardIdentity()
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov         
            },
            new TestVectorAdvancedSettings()
            {
                testCase = 26,
                input = "0123456789012345678901234567890011223344556677889941957217086951357133543759920655",
                output = "4211045833410036541701952892821190399281686633486297517746311270522779428816434640",
                numLock = FialkaEnums.numLockType.NumLock10,
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.Czechoslovakia,
                rTypes = FialkaEnums.rotorTypes.PROTON_II,
                rSeries = FialkaEnums.rotorSeries.K6,
                RotorOrder = FialkaConstants.baseRotorPositions(),
                RotorOffset = FialkaConstants.nullOffset(),
                RingOffset = FialkaConstants.nullOffset(),
                CoreOrder = FialkaConstants.baseRotorPositions(),
                CoreOrientation = new int[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                CoreOffset = FialkaConstants.nullOffset(),
                PunchCard = FialkaConstants.punchCardIdentity()
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov         
            },
            new TestVectorAdvancedSettings()
            {
                testCase = 27,
                input = "0011223344556677889901234567899111212149119495353135157868839719588013257559795775975123700006875",
                output = "8566013658783611962301360947746396511950198930984548084567910419150320828061779852755291192856290",
                numLock = FialkaEnums.numLockType.NumLock10,
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.GDR,
                rTypes = FialkaEnums.rotorTypes.PROTON_II,
                rSeries = FialkaEnums.rotorSeries.K5,
                RotorOrder = FialkaConstants.baseRotorPositions(),
                RotorOffset = FialkaConstants.nullOffset(),
                RingOffset = FialkaConstants.nullOffset(),
                CoreOrder = FialkaConstants.baseRotorPositions(),
                CoreOrientation = new int[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                CoreOffset = FialkaConstants.nullOffset(),
                PunchCard = FialkaConstants.punchCardIdentity()
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov         
            },
            new TestVectorAdvancedSettings()
            {
                testCase = 28,
                input = "795597447954411330088667955734147597325977116801349795522557431143688079793112299688957937759673117",
                output = "388851698314873517417469479710432056074854179064361867734936360514031735722951992372951740606624186",
                numLock = FialkaEnums.numLockType.NumLock10,
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.Czechoslovakia,
                rTypes = FialkaEnums.rotorTypes.PROTON_II,
                rSeries = FialkaEnums.rotorSeries.K6,
                RotorOrder = FialkaConstants.baseRotorPositions(),
                RotorOffset = FialkaConstants.nullOffset(),
                RingOffset = FialkaConstants.nullOffset(),
                CoreOrder = FialkaConstants.baseRotorPositions(),
                CoreOrientation = FialkaConstants.deafultCoreOrientation(),
                CoreOffset = FialkaConstants.nullOffset(),
                PunchCard = new int[] {1,0, 3,2, 5,4, 7,6, 9,8, 11,10, 13,12, 15,14, 17,16, 19,18, 21,20, 23,22, 25,24, 27,26, 29,28}
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov         
            },
             new TestVectorAdvancedSettings()
            {
                testCase = 29,
                input = "795134776688955974311475968795341755227334376688013495767319771395788552733144769574597933142208619",
                output = "269912605890784990551205346100770615227979076607973781097931582380301934113265499668923567466043627",
                numLock = FialkaEnums.numLockType.NumLock10,
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.Czechoslovakia,
                rTypes = FialkaEnums.rotorTypes.PROTON_II,
                rSeries = FialkaEnums.rotorSeries.K6,
                RotorOrder = FialkaConstants.baseRotorPositions(),
                RotorOffset = FialkaConstants.nullOffset(),
                RingOffset = FialkaConstants.nullOffset(),
                CoreOrder = FialkaConstants.baseRotorPositions(),
                CoreOrientation = new int[] {1, -1, 1, -1, 1, -1, 1, -1, 1, -1},
                CoreOffset = FialkaConstants.nullOffset(),
                PunchCard = new int[] {1,0, 3,2, 5,4, 7,6, 9,8, 11,10, 13,12, 15,14, 17,16, 19,18, 21,20, 23,22, 25,24, 27,26, 29,28}
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov         
            },
            new TestVectorAdvancedSettings()
            {
                testCase = 30,
                input = "699533008966795229993115576699431347959608313480039663225973314957662974317568103469520088699331796",
                output = "307510653173395886565970277438397325591036434882704650095743540127925834319181878351321262166467474",
                numLock = FialkaEnums.numLockType.NumLock10,
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.Czechoslovakia,
                rTypes = FialkaEnums.rotorTypes.PROTON_II,
                rSeries = FialkaEnums.rotorSeries.K6,
                RotorOrder = FialkaConstants.baseRotorPositions(),
                RotorOffset = FialkaConstants.nullOffset(),
                RingOffset = FialkaConstants.nullOffset(),
                CoreOrder =new int[] {5, 6, 7, 8, 9, 0, 1, 2, 3, 4},
                CoreOrientation = new int[] {1, -1, 1, -1, 1, -1, 1, -1, 1, -1},
                CoreOffset = FialkaConstants.nullOffset(),
                PunchCard = new int[] {1,0, 3,2, 5,4, 7,6, 9,8, 11,10, 13,12, 15,14, 17,16, 19,18, 21,20, 23,22, 25,24, 27,26, 29,28}
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov         
            },
            new TestVectorAdvancedSettings()
            {
                testCase = 31,
                input = "977311447955225933176880347531345599663439552433088955766743137297680149554349537743795911495944759",
                output = "506225238546985404133880652281379189060214785805648622783123647992246412867146080201998120577711465",
                numLock = FialkaEnums.numLockType.NumLock10,
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.Czechoslovakia,
                rTypes = FialkaEnums.rotorTypes.PROTON_II,
                rSeries = FialkaEnums.rotorSeries.K6,
                RotorOrder = FialkaConstants.baseRotorPositions(),
                RotorOffset = FialkaConstants.nullOffset(),
                RingOffset = FialkaConstants.nullOffset(),
                CoreOrder =new int[] {5, 6, 7, 8, 9, 0, 1, 2, 3, 4},
                CoreOrientation = new int[] {1, -1, 1, -1, 1, -1, 1, -1, 1, -1},
                CoreOffset = FialkaConstants.baseRotorPositions(),
                PunchCard = new int[] {1,0, 3,2, 5,4, 7,6, 9,8, 11,10, 13,12, 15,14, 17,16, 19,18, 21,20, 23,22, 25,24, 27,26, 29,28}
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov         
            } ,
             new TestVectorAdvancedSettings()
            {
                testCase = 32,
                input = "869311795743088679531175929941113779959680013499559743117953996079542297434968995334731479768175314",
                output = "463651510342025925722690183932426810185929788330163117329514052802038199648135010344194116208607670",
                numLock = FialkaEnums.numLockType.NumLock10,
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.Czechoslovakia,
                rTypes = FialkaEnums.rotorTypes.PROTON_II,
                rSeries = FialkaEnums.rotorSeries.K6,
                RotorOrder = FialkaConstants.baseRotorPositions(),
                RotorOffset = FialkaConstants.nullOffset(),
                RingOffset = new int[] {9, 8, 7, 6, 5, 4, 3, 2, 1, 0},
                CoreOrder =new int[] {5, 6, 7, 8, 9, 0, 1, 2, 3, 4},
                CoreOrientation = new int[] {1, -1, 1, -1, 1, -1, 1, -1, 1, -1},
                CoreOffset = FialkaConstants.baseRotorPositions(),
                PunchCard = new int[] {1,0, 3,2, 5,4, 7,6, 9,8, 11,10, 13,12, 15,14, 17,16, 19,18, 21,20, 23,22, 25,24, 27,26, 29,28}
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov         
            },
             new TestVectorAdvancedSettings()
            {
                testCase = 33,
                input = "433137543117665594398080317685202292137968943947528697741179731347657941376954172594310867534795431",
                output = "818690548642312502012438583972053545959362287410350454562205576574364000041319586307154445415567420",
                numLock = FialkaEnums.numLockType.NumLock10,
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.Czechoslovakia,
                rTypes = FialkaEnums.rotorTypes.PROTON_II,
                rSeries = FialkaEnums.rotorSeries.K6,
                RotorOrder = new int[] {9, 8, 7, 6, 5, 4, 3, 2, 1, 0},
                RotorOffset = FialkaConstants.nullOffset(),
                RingOffset = new int[] {9, 8, 7, 6, 5, 4, 3, 2, 1, 0},
                CoreOrder =new int[] {5, 6, 7, 8, 9, 0, 1, 2, 3, 4},
                CoreOrientation = new int[] {1, -1, 1, -1, 1, -1, 1, -1, 1, -1},
                CoreOffset = FialkaConstants.baseRotorPositions(),
                PunchCard = new int[] {1,0, 3,2, 5,4, 7,6, 9,8, 11,10, 13,12, 15,14, 17,16, 19,18, 21,20, 23,22, 25,24, 27,26, 29,28}
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov         
            },
            new TestVectorAdvancedSettings()
            {
                testCase = 34,
                input = "769541447953086110860318674389576673195229731495776817576688034497311449525931346877388710067474716",
                output = "501619465800407695661037489362351416077306401365780278269308111211108282032698873096893676752587138",
                numLock = FialkaEnums.numLockType.NumLock10,
                model = FialkaEnums.machineModel.M125_3,
                layout = FialkaEnums.countryLayout.Czechoslovakia,
                rTypes = FialkaEnums.rotorTypes.PROTON_II,
                rSeries = FialkaEnums.rotorSeries.K6,
                RotorOrder = new int[] {9, 8, 7, 6, 5, 4, 3, 2, 1, 0},
                RotorOffset = new int[] {4, 5, 3, 6, 2, 7, 1, 8, 0, 9},
                RingOffset = new int[] {9, 8, 7, 6, 5, 4, 3, 2, 1, 0},
                CoreOrder =new int[] {5, 6, 7, 8, 9, 0, 1, 2, 3, 4},
                CoreOrientation = new int[] {1, -1, 1, -1, 1, -1, 1, -1, 1, -1},
                CoreOffset = FialkaConstants.baseRotorPositions(),
                PunchCard = new int[] {1,0, 3,2, 5,4, 7,6, 9,8, 11,10, 13,12, 15,14, 17,16, 19,18, 21,20, 23,22, 25,24, 27,26, 29,28}
                // Source of the test vectors: test vectors created with Fialka Simulator v5.16 by Vyacheslav Chernov         
            }
        };
        #endregion
    }
}


