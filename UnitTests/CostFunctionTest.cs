using CrypTool.Plugins.CostFunction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace UnitTests
{
    [TestClass]
    public class CostFunctionTest
    {
        private TestContext testContextInstance;
        public TestContext TestContext
        {
            get => testContextInstance;
            set => testContextInstance = value;
        }
        [TestMethod]
        public void CostFunctionTests()
        {
            string input = "In der Kryptographie ist die Transposition ein Verschluesselungsverfahren, bei dem die Zeichen einer Botschaft (des Klartextes) umsortiert werden. Jedes Zeichen bleibt unveraendert erhalten, jedoch wird die Stelle, an der es steht, geaendert. Dies steht im Gegensatz zu den Verfahren der (monoalphabetischen oder polyalphabetischen) Substitution, bei denen jedes Zeichen des Klartextes seinen Platz behaelt, jedoch durch ein anderes Zeichen ersetzt (substituiert) wird.";
            double epsilon = 0.000001;

            System.Text.ASCIIEncoding enc = new ASCIIEncoding(); // String to Byte Conversion
            CostFunction cf = new CostFunction();
            cf.setBlocksizeToUse(1);
            cf.setTextToUse("" + input.Length);

            //Index of Conincidence
            double target = 0.0717292657591165;
            cf.Initialize();
            cf.InputText = enc.GetBytes(input);
            testContextInstance.WriteLine(enc.GetString(cf.InputText));
            cf.changeFunctionType(CostFunctionSettings.CostFunctionType.IOC);
            cf.PreExecution(); // important, wont work without this
            cf.Execute();

            Assert.AreEqual(target, cf.Value, epsilon); // This _is_ close enough. => Floating point arithmetic!

            //Entropy
            target = 4.31723445412447;
            cf.Initialize();
            cf.InputText = enc.GetBytes(input);
            cf.changeFunctionType(CostFunctionSettings.CostFunctionType.Entropy);
            cf.PreExecution();
            cf.Execute();

            Assert.AreEqual(target, cf.Value, epsilon);

            /*
            //Bigrams: log 2
            target = 4989.51650232229;
            string path = Path.Combine(Environment.CurrentDirectory, "Data\\StatisticsCorpusDE");
            this.testContextInstance.WriteLine(path);
            cf.setDataPath(path);
            cf.Initialize();
            cf.InputText = enc.GetBytes(input);
            cf.changeFunctionType((int)CostFunctionSettings.FunctionTypes.NGramsLog2);
            cf.PreExecution(); 
            cf.Execute();
            testContextInstance.WriteLine(cf.Value.ToString());
            Assert.AreEqual(target, cf.Value, epsilon); 

            //Bigrams: Sinkov
            target = -103.695213603301;
            cf.Initialize();
            cf.InputText = enc.GetBytes(input);
            cf.changeFunctionType((int)CostFunctionSettings.FunctionTypes.NgramsSinkov);
            cf.PreExecution(); 
            cf.Execute();
            testContextInstance.WriteLine(cf.Value.ToString());
            Assert.AreEqual(target, cf.Value, epsilon); 

            //Bigrams: Percentaged
            target = 30.9596836679693;
            cf.Initialize();
            cf.InputText = enc.GetBytes(input);
            cf.changeFunctionType((int)CostFunctionSettings.FunctionTypes.NGramsPercentage);
            cf.PreExecution(); 
            cf.Execute();
            testContextInstance.WriteLine(cf.Value.ToString());
            Assert.AreEqual(target, cf.Value, epsilon); 
            */

            //RegEx - Match
            target = 1.0;
            cf.Initialize();
            cf.InputText = enc.GetBytes("In der Kryptographie 1234567890");
            cf.changeFunctionType(CostFunctionSettings.CostFunctionType.RegEx);
            cf.setRegEx("[a-zA-Z0-9 ]*");
            cf.PreExecution();
            cf.Execute();
            testContextInstance.WriteLine(cf.Value.ToString());
            Assert.AreEqual(target, cf.Value, epsilon);

            //RegEx - Not a Match
            target = -469.0;
            cf.Initialize();
            cf.InputText = enc.GetBytes(input);
            cf.changeFunctionType(CostFunctionSettings.CostFunctionType.RegEx);
            cf.setRegEx("[0-9]"); // String = Number?
            cf.PreExecution();
            cf.Execute();
            testContextInstance.WriteLine(cf.Value.ToString());
            Assert.AreEqual(target, cf.Value, epsilon);

            //Weighted Bigrams/Trigrams
            /*
            target = -1827.29001210328;
            cf.Initialize();
            cf.InputText = enc.GetBytes(input);
            cf.changeFunctionType((int)CostFunctionSettings.FunctionTypes.Weighted); ;
            cf.PreExecution(); 
            cf.Execute();
            testContextInstance.WriteLine(cf.Value.ToString());
            Assert.AreEqual(target, cf.Value, epsilon); 
            */
        }
    }
}
