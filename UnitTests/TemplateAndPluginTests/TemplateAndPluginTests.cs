using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using WorkspaceManager.Model;
using Application = System.Windows.Forms.Application;

namespace UnitTests
{
    [TestClass]
    public class TemplateAndPluginTests
    {
        private static DirectoryInfo _templateDirectory;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            DirectoryInfo dir = Directory.GetParent(System.Environment.CurrentDirectory);
            while (dir != null && dir.GetDirectories("Templates").Length == 0)
            {
                dir = dir.Parent;
            }
            if (dir == null)
            {
                Assert.Fail("Template directory not found!");
            }
            _templateDirectory = dir.GetDirectories("Templates")[0];
        }

        [TestMethod]
        public void CasearTest()
        {
            CrypTool.PluginBase.ICrypComponent pluginInstance = TestHelpers.GetPluginInstance("Caesar");
            PluginTestScenario scenario = new PluginTestScenario(pluginInstance, new[] { "ShiftKey", "AlphabetSymbols", "InputString", ".CaseSensitive" }, new[] { "OutputString" });

            //Test 1:
            if (!scenario.Test(new object[] { 24, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", "Franz jagt im komplett verwahrlosten Taxi quer durch Bayern", false }, new[] { "Dpylx hyer gk imknjcrr tcpuyfpjmqrcl Ryvg oscp bspaf Zywcpl".ToUpper() }))
            {
                TestHelpers.TestFail(1);
            }

            //Test 2:
            if (!scenario.Test(new object[] { 24, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", "Franz jagt im komplett verwahrlosten Taxi quer durch Bayern", true }, new[] { "dPyLX HyER GK IMKNJCRR TCPUyFPJMQRCL ryVG OSCP BSPAF ZyWCPL" }))
            {
                TestHelpers.TestFail(2);
            }
        }

        //[TestMethod]
        public void CasearTemplateTest()
        {
            TestForm form = new TestForm();
            Thread uiThread = new Thread(() => Application.Run(form));
            uiThread.SetApartmentState(ApartmentState.STA);
            uiThread.Start();

            //This test doesn't work yet!

            uiThread.Join();
        }

        public class TestForm : Form
        {
            public TestForm()
            {
                WorkspaceModel model = GetWorkspaceModel(@"Cryptography\Classic\Caesar.cwm");
                TemplateTestScenario scenario = new TemplateTestScenario(model, new[] { "Message>.Text" }, new[] { "Result>CurrentValue" });

                //Test 1:
                if (!scenario.Test(new object[] { "Input your message here." }, new[] { "Nsuzy dtzw rjxxflj mjwj." }))
                {
                    //TestFail(1);
                }
            }
        }
        [TestMethod]
        public void ADFGVXTest()
        {
            CrypTool.PluginBase.ICrypComponent pluginInstance = TestHelpers.GetPluginInstance("ADFGVX");
            PluginTestScenario scenario = new PluginTestScenario(pluginInstance, new[] { ".CipherType", ".SubstitutionPass", ".TranspositionPass", "InputString" }, new[] { "OutputString" });

            //Test 1:
            if (!scenario.Test(new[] { (object)CrypTool.ADFGVX.ADFGVXSettings.CipherTypeEnum.ADFGX, "WIKPEDAZYXVUTSRQONMLHGFCB", "BEOBACHTUNGSLISTE", "Munitionierung beschleunigen Punkt Soweit nicht eingesehen auch bei Tag" }, new[] { "GXGGADDDGDXXAFADDFAAXAFDFFXFDGDXGAGGAAXFAGADFAAADGFAXXADADFFFDDADFGAXGXAFXGXFXDAFAGFXXFAXGFDXFFDFAGXXGXXADGXGFXDFFDGAXXFFFFGDX" }))
            {
                TestHelpers.TestFail(1);
            }
        }

        #region private

        private static WorkspaceModel GetWorkspaceModel(string fileName)
        {
            ModelPersistance modelLoader = new ModelPersistance();
            WorkspaceModel model = modelLoader.loadModel(Path.Combine(_templateDirectory.FullName, fileName));
            return model;
        }

        #endregion
    }
}
