using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class ADFGVXTest
    {
        public ADFGVXTest()
        {
        }

        [TestMethod]
        public void ADFGVXTestMethod()
        {
            CrypTool.PluginBase.ICrypComponent pluginInstance = TestHelpers.GetPluginInstance("ADFGVX");
            PluginTestScenario scenario = new PluginTestScenario(pluginInstance, new[] { "InputString", ".CipherType", ".SubstitutionPass", ".TranspositionPass" }, new[] { "OutputString" });
            object[] output;

            foreach (TestVector vector in testvectors)
            {
                output = scenario.GetOutputs(new object[] { vector.input, vector.cipher, vector.subkey, vector.transkey });
                Assert.AreEqual(vector.output, (string)output[0], "Unexpected value in test #" + vector.n + ".");
            }

        }

        private struct TestVector
        {
            public string input, output, subkey, transkey;
            public int n, cipher;
        }

        //
        // Sources of the test vectors:
        //  http://de.wikipedia.org/wiki/ADFGX
        //  CrypTool1-Testvectors
        //
        private readonly TestVector[] testvectors = new TestVector[] {
            new TestVector () { n=0, cipher=0, subkey="WIKPEDAZYXVUTSRQONMLHGFCB", transkey="BEOBACHTUNGSLISTE", input="Munitionierung beschleunigen Punkt Soweit nicht eingesehen auch bei Tag", output="GXGGADDDGDXXAFADDFAAXAFDFFXFDGDXGAGGAAXFAGADFAAADGFAXXADADFFFDDADFGAXGXAFXGXFXDAFAGFXXFAXGFDXFFDFAGXXGXXADGXGFXDFFDGAXXFFFFGDX" },
            new TestVector () { n=1, cipher=1, subkey="8P3D1NLT4OAH7KBC5ZJU6WGMXSVIR29EY0FQ", transkey="MARK", input="ANGRIFFUMX0UHR", output="VVGVXGXXVVDADVDGVXGXDAVXGVGV" },
            new TestVector () { n=2, cipher=1, subkey="8P3D1NLT4OAH7KBC5ZJU6WGMXSVIR29EY0FQ", transkey="RHIEN", input="CAN YOU ATTACK THE LEFT FLANK OF THE ARMY DURING THE SECOND HOUR TOMORROW. WE WILL BE ABLE TO SEND REINFORCEMENTS BY NOON. HOW MANY MEN DO YOU HAVE? DO YOU NEED SUPPLIES? SEND YOUR REPLY TO THE RIVER.", output="VDVDDXDXVDDDXGGDDFXDVGVGDDFFDVXXXVDADAGDXXDDGDDXDXDAGVXDVAFDDVVGXDDGDADAFVXVAVGXXGDDDGDGVADADDVGDGXDXGDGAXAFDFDGXGAAVDXDXADXVDDFDDFXXDDDDDGGVVXDAXVGVGXGFVXGAVAGXDVFDXGXXGDXXGGDVDVDAFVDXGXGVAGDVDDXVAGDVXDADVGAGDXVGGAXFDDADXVGXFXAGDFAGGVAFADGDXDAGVDDDVFFXGDFDDVDXXXVFVXDDDGGDDVGGDDDDXGVVFXDFDXGVGXXDVGDXADDDXGGVDDDVX" },
        };

    }
}