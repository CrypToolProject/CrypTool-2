using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace CrypTool.ComputeXZ
{
    [Author("Abdeljalil Bourbahh", "bourbahh@yahoo.de", "Ruhr-Universitaet Bochum, Chair for System Security", "http://www.trust.rub.de/")]
    [PluginInfo("Alg. attack: Compute XZ", "compute the sets XZ for a Combiner of streamcipher", "ComputeXZ/DetailedDescription/Description.xaml", new[] { "ComputeXZ/Images/xzz.png" })]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class ComputeXZ : ICrypComponent
    {
        #region Private variables
        private Hashtable outputXZ;
        private Hashtable XZ;
        private string outputString;
        private ComputeXZSettings settings;
        private string outputfunction;
        private string memoryupdatefunction;
        private int k;// Anzahl der LFSRSs-Ausgänge bei (k,l)-Combiner
        private int startX = 0;
        private bool isXZloaded = false;
        private bool stop = false;
        #endregion
        #region Public interface
        public ComputeXZ()
        {
            settings = new ComputeXZSettings();
            settings.LogMessage += ComputeXZ_LogMessage;
        }
        public ISettings Settings
        {
            get => settings;
            set => settings = (ComputeXZSettings)value;
        }
        [PropertyInfo(Direction.InputData, "outputfunction", "outputfunction of Combiner", true)]
        public string Outputfunction
        {
            get => outputfunction;
            set
            {
                if (value != outputfunction)
                {
                    outputfunction = value;
                    OnPropertyChanged("Outputfunction");
                }
            }
        }
        [PropertyInfo(Direction.InputData, "memoryupdatefunction of combiner", " to input if combiner has memory ", false)]
        public string Memoryupdatefunction
        {
            get => memoryupdatefunction;
            set
            {
                if (value != memoryupdatefunction)
                {
                    memoryupdatefunction = value;
                    OnPropertyChanged("Memoryupdatefunction");
                }
            }
        }
        [PropertyInfo(Direction.OutputData, "the stes XZ as string", "to display XZ in Textoutput", false)]
        public string OutputString
        {
            get => outputString;
            set
            {
                outputString = value;
                OnPropertyChanged("OutputString");
            }

        }
        [PropertyInfo(Direction.OutputData, " the sets XZ as (Hashtable(Z,XZ))", " to use as Input of Pugin compute Annihiators", false)]
        public Hashtable OutputXZ
        {
            get => outputXZ;
            set
            {
                outputXZ = value;
                OnPropertyChanged("OutputXZ");
            }
        }
        #endregion
        #region IPlugin members
        public void Initialize()
        {
        }
        public void Dispose()
        {
        }
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }
        public UserControl Presentation => null;
        public void Stop()
        {
            try
            {
                if (!settings.IsXZcomputed)
                {
                    stop = true;
                }
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }
        public void PostExecution()
        {
        }

        public void PreExecution()
        {
        }
        #endregion
        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        private void ComputeXZ_LogMessage(string msg, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(msg, this, logLevel));
            }
        }
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }
        #endregion


#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore
        #region private functions
        public void Execute()
        {
            try
            {
                if (outputfunction != null && settings.SetOfOutputs != null)
                {
                    if (IsFunctionsExpValid() && (IsOutputsExpValid(settings.SetOfOutputs)))
                    {
                        stop = false;
                        if (IsParameterNew() || !settings.IsXZcomputed)
                        {
                            if (memoryupdatefunction == null)
                            {
                                ComputeSimpleXZ();
                            }
                            else
                            {
                                ComputeSetXZ();
                            }
                        }
                        else
                        {
                            if (!isXZloaded)
                            {
                                outputXZ = settings.SavedXZ;
                                isXZloaded = true;
                            }
                        }
                        if (!stop)
                        {
                            switch (settings.OutputSetting)
                            {
                                case 0:
                                    DisplayXZ();
                                    break;
                                case 1:
                                    PassXZ();
                                    break;
                                case 2:
                                    DisplayXZ();
                                    PassXZ();
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    GuiLogMessage("not all mandatory parameters are provided", NotificationLevel.Error);
                }
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
            finally
            {
                ProgressChanged(1, 1);
            }
        }
        public void ComputeSetXZ()
        {
            int run = settings.SetOfOutputs.Length;
            string simplef = SimplifyFunction(outputfunction);
            string[] initY = memoryupdatefunction.Split(';');
            int l = initY.Length;//l=memorysize
            int Xlength = k * run;
            ArrayList f = ConvertCombinerFunction(simplef, k, l);
            ArrayList[] Y = new ArrayList[l];
            for (int q = 0; q < l; q++)
            {
                string simpleY_k = SimplifyFunction(initY[q]);
                Y[q] = ConvertCombinerFunction(simpleY_k, k, l);
            }
            InitializeXZ();
            int Z;
            for (int x = startX; x < exp(2, Xlength); x++)
            {
                if (stop)
                {
                    settings.SavedstartX = x;
                    settings.SavedXZ = XZ;
                    break;
                }
                bool[] X = ConstructBitsequence(x, Xlength);
                for (int m = 0; m < exp(2, l); m++)
                {
                    Z = 0;
                    bool[] M_i = ConstructBitsequence(m, l);
                    bool[] savemi = new bool[l];
                    for (int r = 0; r < run; r++)
                    {// r
                        bool z_i = EvaluateCombinerFunction(f, X, r, M_i);
                        if (z_i)
                        {
                            Z = Z + exp(2, run - r - 1);
                        }

                        for (int i = 0; i < l; i++)
                        {
                            savemi[i] = M_i[i];
                        }
                        for (int j = 0; j < l; j++)
                        {
                            M_i[j] = EvaluateCombinerFunction(Y[j], X, r, savemi);
                        }
                    }
                    ((ArrayList)XZ[Z]).Add(X);
                }
            }
            if (!stop)
            {
                settings.IsXZcomputed = true;
                outputXZ = new Hashtable();
                foreach (DictionaryEntry dicEntry in XZ)
                {
                    int z = (int)dicEntry.Key;
                    ArrayList xz = ReduceXZ((ArrayList)dicEntry.Value);
                    outputXZ.Add(z, xz);
                }
                settings.SavedXZ = outputXZ;
            }
        }
        public void ComputeSimpleXZ()
        {
            int run = settings.SetOfOutputs.Length;
            string simplef = SimplifyFunction(outputfunction);
            int Xlength = k * run;
            ArrayList f = ConvertSimpleFunction(simplef, k);
            InitializeXZ();
            int Z;
            for (int x = startX; x < exp(2, Xlength); x++)// für alle X
            {
                if (stop)
                {
                    settings.SavedstartX = x;
                    settings.SavedXZ = XZ;
                    break;
                }
                bool[] X = ConstructBitsequence(x, Xlength);
                Z = 0;
                for (int r = 0; r < run; r++)
                {// r
                    bool z = EvaluateOutputFunction(f, X, r);
                    if (z)
                    {
                        Z = Z + exp(2, run - r - 1);
                    }
                }
                ((ArrayList)XZ[Z]).Add(X);
            }
            if (!stop)
            {
                settings.IsXZcomputed = true;
                outputXZ = new Hashtable();
                foreach (DictionaryEntry dicEntry in XZ)
                {
                    int z = (int)dicEntry.Key;
                    ArrayList xz = ReduceXZ((ArrayList)dicEntry.Value);
                    outputXZ.Add(z, xz);
                }
                settings.SavedXZ = outputXZ;
            }
        }
        public bool IsParameterNew()
        {
            if (memoryupdatefunction != null)
            {
                if (settings.Saveoutputfunction == outputfunction && settings.Savedmemoryupdatefunction == memoryupdatefunction && settings.Savedrunlength == settings.SetOfOutputs.Length)
                {
                    return false;
                }
            }
            else
            {
                if (settings.Saveoutputfunction == outputfunction && settings.Savedrunlength == settings.SetOfOutputs.Length)
                {
                    return false;
                }
            }
            return true;
        }
        public void DisplayXZ()
        {
            int[] outputsZ = GetOutputs(settings.SetOfOutputs);
            StringBuilder b = new StringBuilder();
            for (int i = 0; i < outputsZ.Length; i++)
            {
                int Z = outputsZ[i];
                ArrayList XZ = (ArrayList)outputXZ[Z];
                b.AppendLine("X_" + Bitpresentation(Z, settings.SetOfOutputs.Length));
                b.AppendLine(TostringXZ(XZ));
            }
            outputString = b.ToString();
            OnPropertyChanged("OutputString");
        }
        public void PassXZ()
        {
            int index = exp(2, settings.SetOfOutputs.Length);
            ArrayList parameter = new ArrayList
            {
                settings.Saveoutputfunction
            };
            if (memoryupdatefunction != null)
            {
                parameter.Add(settings.Savedmemoryupdatefunction);
            }
            else
            {
                parameter.Add("");
            }

            parameter.Add(settings.SetOfOutputs.Length);
            if (outputXZ.Contains(index))
            {
                outputXZ.Remove(index);
            }

            outputXZ.Add(index, parameter);
            OnPropertyChanged("OutputXZ");
        }
        public int[] GetOutputs(string outputsexp)
        {
            char[] c = outputsexp.ToCharArray();
            int[] Z;
            int Zlength = 0;
            int sum = 0;
            ArrayList sumlist = new ArrayList();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == '*')
                {
                    Zlength++;
                    sumlist.Add(exp(2, c.Length - 1 - i));
                }
                else
                {
                    if (c[i] == '1')
                    {
                        sum = sum + exp(2, c.Length - 1 - i);
                    }
                }
            }
            Z = new int[exp(2, Zlength)];
            for (int i = 0; i < exp(2, Zlength); i++)
            {
                bool[] bitseq = ConstructBitsequence(i, Zlength);
                int dez_v = 0;
                for (int j = 0; j < bitseq.Length; j++)
                {
                    if (bitseq[j])
                    {
                        dez_v = dez_v + (int)sumlist[j];
                    }
                }
                Z[i] = dez_v + sum;
            }
            return Z;
        }
        public void InitializeXZ()
        {
            int run = settings.SetOfOutputs.Length;
            XZ = new Hashtable();
            if (IsParameterNew())
            {
                startX = 0;
                settings.IsXZcomputed = false;
                for (int k = 0; k < exp(2, run); k++)
                {
                    ArrayList list = new ArrayList();
                    XZ.Add(k, list);
                }
                settings.Saveoutputfunction = outputfunction;
                settings.Savedmemoryupdatefunction = memoryupdatefunction;
                settings.Savedrunlength = settings.SetOfOutputs.Length;
            }
            else
            {
                startX = settings.SavedstartX;
                foreach (DictionaryEntry dicEntry in settings.SavedXZ)
                {
                    ArrayList list = (ArrayList)dicEntry.Value;
                    XZ.Add((int)dicEntry.Key, list);
                }
            }
        }
        public ArrayList ReduceXZ(ArrayList primXZ)
        {
            ArrayList reducedXZ = new ArrayList();
            int i = 0;
            bool[] X = (bool[])primXZ[0];
            while (i < primXZ.Count)
            {
                if (X.Equals((bool[])primXZ[i]))
                {
                    i++;
                }
                else
                {
                    reducedXZ.Add(X);
                    X = (bool[])primXZ[i];
                    i++;
                }
            }
            reducedXZ.Add((bool[])primXZ[i - 1]);
            return reducedXZ;
        }
        private bool EvaluateCombinerMonomial(bool[] monom, bool[] X, int i, bool[] M)
        {
            int Xindex = i * k;
            for (int j = 0; j < k; j++)
            {
                if (monom[j] && !X[Xindex])
                { return false; }
                Xindex++;
            }
            for (int j = 0; j < M.Length; j++)
            {
                if (monom[k + j] && !M[j])
                {
                    return false;
                }
            }
            return true;
        }
        public bool EvaluateCombinerFunction(ArrayList function, bool[] X, int i, bool[] M)
        {
            bool erg = EvaluateCombinerMonomial((bool[])function[0], X, i, M);
            for (int j = 1; j < function.Count; j++)
            {
                erg = erg ^ (EvaluateCombinerMonomial((bool[])function[j], X, i, M));
            }
            return erg;
        }
        private bool EvaluateSimpleMonomial(bool[] monom, bool[] X, int i)
        {
            int Xindex = i * k;
            for (int j = 0; j < k; j++)
            {
                if (monom[j] && !X[Xindex])
                { return false; }
                Xindex++;
            }
            return true;
        }
        public bool EvaluateOutputFunction(ArrayList function, bool[] X, int i)
        {
            bool erg = EvaluateSimpleMonomial((bool[])function[0], X, i);
            for (int j = 1; j < function.Count; j++)
            {
                erg = erg ^ (EvaluateSimpleMonomial((bool[])function[j], X, i));
            }
            return erg;
        }
        private ArrayList ConvertCombinerFunction(string function, int p, int q)
        {
            ArrayList convertfunction = new ArrayList();
            int totalinput = p + q;
            string[] monmials = function.Split('+');
            for (int i = 0; i < monmials.Length; i++)
            {
                string[] literals = monmials[i].Split('*');
                bool[] monom = new bool[totalinput];
                for (int j = 0; j < monom.Length; j++)
                {
                    monom[j] = false;
                }
                for (int j = 0; j < literals.Length; j++)
                {
                    string literal = literals[j];
                    if (literal.StartsWith("x"))
                    {
                        int index = Convert.ToInt16(literal.Substring(1, literal.Length - 1));
                        if (index <= p)
                        {
                            monom[index - 1] = true;
                        }
                        else
                        {
                            GuiLogMessage("the indexes of variable must begin  from 1 and to be successiv ", NotificationLevel.Error);

                        }
                    }
                    else
                    {
                        if (literal.StartsWith("m"))
                        {
                            int index = Convert.ToInt16(literal.Substring(1, literal.Length - 1));
                            if (index <= p)
                            {
                                monom[p + index - 1] = true;
                            }
                            else
                            {
                                GuiLogMessage("the indexes of variable must begin  from 1 and to be successiv ", NotificationLevel.Error);

                            }

                        }
                    }
                }
                bool containmonom = false;
                for (int j = 0; j < convertfunction.Count; j++)
                {
                    if (monom.SequenceEqual((bool[])convertfunction[j]))
                    {
                        convertfunction.RemoveAt(j);
                        containmonom = true;
                        break;
                    }
                }
                if (!containmonom)
                {
                    convertfunction.Add(monom);
                }
            }
            return convertfunction;
        }
        private ArrayList ConvertSimpleFunction(string function, int p)
        {
            ArrayList convertfunction = new ArrayList();
            string[] monomials = function.Split('+');
            for (int i = 0; i < monomials.Length; i++)
            {
                string[] literals = monomials[i].Split('*');
                bool[] monom = new bool[p];
                for (int j = 0; j < monom.Length; j++)
                {
                    monom[j] = false;
                }
                for (int j = 0; j < literals.Length; j++)
                {
                    string literal = literals[j];
                    if (literal.StartsWith("x"))
                    {
                        int index = Convert.ToInt16(literal.Substring(1, literal.Length - 1));
                        if (index <= p)
                        {
                            monom[index - 1] = true;
                        }
                        else
                        {
                            GuiLogMessage("the indexes of variable must begin  from 1 and to be successiv ", NotificationLevel.Error);

                        }
                    }

                }
                convertfunction.Add(monom);
            }
            return convertfunction;
        }
        public bool IsFunctionsExpValid()// eingegebene Funktionen
        {
            if (memoryupdatefunction == null)
            {
                return IsFunctionExpValid(outputfunction);
            }
            else
            {
                return IsCombinerFunctionsExpvalid();
            }
        }
        public bool IsFunctionExpValid(string f)//einfache combiner 
        {
            Regex validExpression = new Regex("^(([\\!]?[\\(])*(((\\!?)(x))(\\d+)|1)[\\)]*(\\*|\\+))*(((\\!?)(x))(\\d+)|1)[\\)]*$");
            f = f.Replace(" ", "");
            if (!IsParenthesesvalid(f))
            {
                GuiLogMessage("the  Parentheses in " + f + "are not valid", NotificationLevel.Error);
                return false;

            }
            if (!validExpression.IsMatch(f))
            {
                GuiLogMessage(f + "is not a legal  function expression", NotificationLevel.Error);
                return false;
            }
            if (!IsSimpleIndicesValid(f))
            {
                return false;
            }

            return true;
        }
        public bool IsCombinerFunctionsExpvalid()//alle Combinerfunktionen 
        {
            if (!IsCombinerFunctionExpValid(outputfunction))
            {
                return false;
            }
            memoryupdatefunction = memoryupdatefunction.Replace("\r\n", "");
            if (memoryupdatefunction.EndsWith(";"))
            { memoryupdatefunction = memoryupdatefunction.Remove(memoryupdatefunction.Length - 1, 1); }
            string[] memoryupdatefunctionArray = memoryupdatefunction.Split(';');
            for (int i = 0; i < memoryupdatefunctionArray.Length; i++)
            {
                if (!IsCombinerFunctionExpValid(memoryupdatefunctionArray[i]))
                {
                    return false;
                }
            }
            if (!IsIndicesValid())
            {
                return false;
            }

            return true;

        }
        private bool IsOutputsExpValid(string expression)
        {
            Regex regExpression1 = new Regex("^(1|[\\*]|0)+$");
            if (!regExpression1.IsMatch(expression))
            {
                GuiLogMessage("The expression of Set of output" + expression + "is not legal", NotificationLevel.Error);
                return false;
            }
            return true;
        }
        public bool IsCombinerFunctionExpValid(string function)//eine Combinerfunktion 
        {
            function = function.Replace(" ", "");
            Regex validExpression = new Regex("^(([\\!]?[\\(])*(((\\!?)(x|m))(\\d+)|1)[\\)]*(\\*|\\+))*(((\\!?)(x|m))(\\d+)|1)[\\)]*$");
            if (!IsParenthesesvalid(function))
            {
                GuiLogMessage("the  Parentheses in " + function + "is not valid", NotificationLevel.Error);
                return false;
            }
            if (!validExpression.IsMatch(function))
            {
                GuiLogMessage(function + "is not a legal  function expression", NotificationLevel.Error);
                return false;
            }
            return true;
        }
        private bool IsParenthesesvalid(string s)
        {
            CharEnumerator cn = s.GetEnumerator();
            int overt = 0;
            int closed = 0;
            while (cn.MoveNext())
            {
                char c = cn.Current;
                if (c == '(')
                {
                    overt++;
                }
                else
                {
                    if (c == ')')
                    {
                        closed++;
                    }

                    if (closed > overt)
                    {
                        return false;
                    }
                }
            }
            if (closed < overt)
            {
                return false;
            }

            return true;
        }
        private bool IsIndicesValid()
        {
            string[] memoryupdatefunctionArray = memoryupdatefunction.Split(';');
            int q = memoryupdatefunctionArray.Length;
            string[] functions = new string[q + 1];
            Array.Copy(memoryupdatefunctionArray, functions, q);
            functions[q] = outputfunction;
            ArrayList lfsrvar = new ArrayList();
            ArrayList memvar = new ArrayList();
            for (int n = 0; n < functions.Length; n++)
            {
                string[] monomials = Splitfunction(functions[n]);
                for (int i = 0; i < monomials.Length; i++)
                {
                    string[] literals = monomials[i].Split('*');
                    for (int j = 0; j < literals.Length; j++)
                    {
                        string literal = literals[j];
                        if (literal.StartsWith("x"))
                        {
                            if (!lfsrvar.Contains(literal))
                            {
                                lfsrvar.Add(literal);
                            }
                        }
                        else
                        {
                            if (literal.StartsWith("m"))
                            {
                                if (!memvar.Contains(literal))
                                {
                                    memvar.Add(literal);
                                }
                            }
                        }
                    }

                }
            }
            k = lfsrvar.Count;
            foreach (string lit in lfsrvar)
            {
                int index = Convert.ToInt16(lit.Substring(1, lit.Length - 1));
                if (!(1 <= index && index <= k))
                {
                    GuiLogMessage("the indexes of variable x_i must begin  from 1 and to be successiv ", NotificationLevel.Error);
                    return false;
                }
            }
            foreach (string mlit in memvar)
            {
                int index = Convert.ToInt16(mlit.Substring(1, mlit.Length - 1));
                if (!(1 <= index && index <= q))
                {
                    GuiLogMessage("the indexes of variable m_i must begin  from 1 and to be successiv ", NotificationLevel.Error);
                    return false;
                }
            }
            return true;
        }
        private bool IsSimpleIndicesValid(string function)
        {
            ArrayList lfsrvar = new ArrayList();
            string[] monomials = Splitfunction(function);
            for (int i = 0; i < monomials.Length; i++)
            {
                string[] literals = monomials[i].Split('*');
                for (int j = 0; j < literals.Length; j++)
                {
                    string literal = literals[j];
                    if (literal.StartsWith("x"))
                    {
                        if (!lfsrvar.Contains(literal))
                        {
                            lfsrvar.Add(literal);
                        }
                    }
                }
            }
            k = lfsrvar.Count;
            foreach (string lit in lfsrvar)
            {
                int index = Convert.ToInt16(lit.Substring(1, lit.Length - 1));
                if (!(1 <= index && index <= k))
                {
                    GuiLogMessage("the indexes of variable x_i must begin  from 1 and to be successiv ", NotificationLevel.Error);
                    return false;
                }
            }
            return true;
        }
        public string TostringXZ(ArrayList xzset)
        {
            StringBuilder xzstring = new StringBuilder();
            int Xlength = ((bool[])xzset[0]).Length;
            foreach (bool[] X in xzset)
            {
                for (int j = 0; j < Xlength; j++)
                {
                    if (X[j])
                    {
                        xzstring.Append("1");
                    }
                    else
                    {
                        xzstring.Append("0");
                    }
                }
                xzstring.Append(",");
            }
            xzstring.Remove(xzstring.Length - 1, 1);
            return xzstring.ToString();
        }
        #region functionparser
        private string SimplifyFunction(string function)
        {
            return ReduceFunction(SimplifyParentheses(NegateTerms(function)));
        }
        public string SimplifyParentheses(string f)
        {
            CharEnumerator fchar = f.GetEnumerator();
            int overt = 0;
            int closed = 0;
            int firstovert = 0;
            int lastclosed = 0;
            int index = 0;
            if (!IsParenthesesNested(f))
            {
                return Expandproducts(f);
            }
            else
            {
                while (fchar.MoveNext())
                {
                    char c = fchar.Current;
                    if (c == '(')
                    {
                        if (overt == 0)
                        {
                            firstovert = index;
                            overt++;
                        }
                        else { overt++; }
                    }
                    if (c == ')')
                    {
                        if (overt == 1)
                        {
                            overt = 0;
                        }
                        else
                        {
                            closed++;
                            if (closed == overt)
                            {
                                overt = 0;
                                closed = 0;
                                lastclosed = index;
                                string toexpand = f.Substring(firstovert + 1, lastclosed - firstovert - 1);
                                f = f.Remove(firstovert + 1, lastclosed - firstovert - 1);
                                f = f.Insert(firstovert + 1, SimplifyParentheses(toexpand));
                                f = SimplifyParentheses(f);
                            }
                        }
                    }
                    index++;
                }
                return f;
            }
        }
        public bool IsParenthesesNested(string f)
        {
            CharEnumerator fchar = f.GetEnumerator();
            bool overt = false;
            while (fchar.MoveNext())
            {
                char c = fchar.Current;
                if (c == '(')
                {
                    if (overt)
                    {
                        return true;
                    }
                    else
                    {
                        overt = true;
                    }
                }
                if (c == ')' && overt)
                {
                    overt = false;
                }
            }
            return false;

        }
        private string NegateTerms(string f)
        {
            int notindex = f.IndexOf("!");
            while (notindex != -1)
            {
                string tonegate = f.Substring(notindex + 1, 1);
                if (tonegate == "(")
                {
                    f = f.Remove(notindex, 1);
                    f = f.Insert(notindex + 1, "1+");
                }
                else
                {
                    string subf = f.Substring(notindex + 1);
                    string[] subterms = subf.Split('+');
                    string[] subliterals = subterms[0].Split('*');
                    string tonegatliteral = subliterals[0];
                    f = f.Remove(notindex, 1 + tonegatliteral.Length);
                    f = f.Insert(notindex, "(1+" + tonegatliteral + ")");
                }
                notindex = f.IndexOf("!");
            }
            return f;
        }
        private string Expandproducts(string f)
        {
            while (f.IndexOf("(") != -1)
            {
                int p1 = f.IndexOf("(");
                int p2 = f.IndexOf(")");
                if (!f.Substring(p1, p2 - p1 + 1).Contains('+'))
                {
                    f = f.Remove(p1, 1);
                    f = f.Remove(p2 - 1, 1);
                }
                else
                {
                    string topar = f.Substring(0, p1);
                    int begin = topar.LastIndexOf("+") + 1;
                    int end = p1 + FindEndOfTerm(f.Substring(p1));
                    string Term = f.Substring(begin, end - begin);
                    f = f.Remove(begin, Term.Length);
                    Term = ExpandOneTerm(Term);
                    f = f.Insert(begin, Term);
                }
            }
            return f;
        }
        public int FindEndOfTerm(string f)
        {
            CharEnumerator f_char = f.GetEnumerator();
            int index = 0;
            bool overt = false;
            while (f_char.MoveNext())
            {
                char c = f_char.Current;
                if (c == '(')
                {
                    overt = true;
                }

                if (c == ')')
                {
                    overt = false;
                }

                if (c == '+' && !overt)
                {
                    return index;
                }

                index++;
            }
            return f.Length;
        }
        public string ExpandOneTerm(string term)
        {
            int p1 = term.IndexOf('(');
            int p2 = term.IndexOf(')');
            ArrayList terms = new ArrayList();
            while (p1 != -1)
            {
                string factor = term.Substring(p1, p2 - p1 + 1);
                if (p2 != term.Length - 1) { term = term.Remove(p1, p2 - p1 + 2); }
                else { term = term.Remove(p1, p2 - p1 + 1); }
                terms.Add(factor);
                p1 = term.IndexOf('(');
                p2 = term.IndexOf(')');
            }
            string res = "";
            if (terms.Count == 1)
            {
                res = (string)terms[0];
            }
            else
            {
                res = "(" + LiftRightExpand(terms[0] + "*" + terms[1]) + ")";
                for (int i = 2; i < terms.Count; i++)
                {
                    res = "(" + LiftRightExpand(res + "*" + terms[i]) + ")";
                }
            }
            if (term != "")
            {
                if (term.EndsWith("*"))
                {
                    res = RightExpand(term + res);
                }
                else
                {
                    res = RightExpand(term + "*" + res);
                }
            }
            else
            {
                res = res.Remove(0, 1);
                res = res.Remove(res.Length - 1, 1);
            }
            return res;
        }
        public string RightExpand(string term)
        {
            int p1 = term.IndexOf("(");
            int p2 = term.IndexOf(")");
            string fact1 = term.Substring(0, p1);
            string fact2 = term.Substring(p1 + 1, p2 - p1 - 1);
            string[] sumands = fact2.Split('+');
            string s = "";
            for (int i = 0; i < sumands.Length; i++)
            {
                s = s + fact1 + sumands[i] + "+";
            }
            s = s.Remove(s.Length - 1);
            return s;
        }
        public string LiftExpand(string term)
        {
            int p1 = term.IndexOf("(");
            int p2 = term.IndexOf(")");
            string fac1 = term.Substring(p2 + 1, term.Length - p2 - 1);
            string fac2 = term.Substring(p1 + 1, p2 - p1 - 1);
            string[] sumands = fac2.Split('+');
            string s = "";
            for (int i = 0; i < sumands.Length; i++)
            {
                s = s + sumands[i] + fac1 + "+";
            }
            s = s.Remove(s.Length - 1);
            return s;
        }
        public string LiftRightExpand(string term)
        {
            int p1 = term.IndexOf("(");
            int p2 = term.IndexOf(")");
            string fac1 = term.Substring(0, p2 + 2);
            string fac2 = term.Substring(p2 + 3, term.Length - p2 - 4);
            string[] sumands = fac2.Split('+');
            string s = "";
            for (int i = 0; i < sumands.Length; i++)
            {
                s = s + LiftExpand(fac1 + sumands[i]) + "+";
            }
            s = s.Remove(s.Length - 1);
            return s;
        }
        public string ReduceFunction(string function)
        {
            int index = function.IndexOf("+1*");
            while (index != -1)
            {
                function = function.Remove(index + 1, 2);
                index = function.IndexOf("+1*");
            }
            index = function.IndexOf("1*");
            if (index == 0)
            {
                function = function.Remove(index + 1, 2);
            }

            index = function.IndexOf("*1");
            while (index != -1)
            {
                function = function.Remove(index, 2);
                index = function.IndexOf("*1");
            }
            string[] monomials = function.Split('+');
            string s = "";
            for (int i = 0; i < monomials.Length; i++)
            {
                s = s + ReduceTerm(monomials[i]) + "+";
            }
            s = s.Remove(s.LastIndexOf("+"));
            return s;
        }
        private string ReduceTerm(string term)
        {
            string[] literals = term.Split('*');
            string s = "";
            for (int i = 0; i < literals.Length - 1; i++)
            {
                if (literals[i] != "")
                {
                    for (int j = i + 1; j < literals.Length; j++)
                    {
                        if (literals[i] == literals[j])
                        {
                            literals[j] = "";
                        }
                    }
                    s = s + literals[i] + "*";
                }
            }
            s = s + literals[literals.Length - 1];
            return s;
        }
        #endregion
        public int exp(int b, int e)
        {
            int res = 1;
            for (int i = 1; i <= e; i++)
            {
                res = res * b;
            }
            return res;
        }
        public bool[] ConstructBitsequence(int value, int length)
        {
            bool[] bitseq = new bool[length];
            if (value == 0)
            {
                for (int j = 0; j < length; j++)
                {
                    bitseq[j] = false;
                }
            }
            else
            {
                int i = length;
                while (value != 0)
                {
                    int div = value % 2;
                    if (div == 0)
                    {
                        bitseq[i - 1] = false;
                    }
                    else
                    {
                        bitseq[i - 1] = true;
                    }

                    i = i - 1;
                    value = value / 2;
                }
                if (i > 0)
                {
                    for (int j = 0; j < i; j++)
                    {
                        bitseq[j] = false;
                    }
                }
            }
            return bitseq;
        }
        public string Bitpresentation(int value, int length)
        {
            string s = "";
            int i = length;
            if (value == 0)
            {
                for (int j = 0; j < length; j++)
                {
                    s = s + "0";
                }
            }
            else
            {
                while (value != 0)
                {
                    int div = value % 2;
                    if (div == 0)
                    {
                        s = "0" + s;
                    }
                    else
                    {
                        s = "1" + s;
                    }

                    i = i - 1;
                    value = value / 2;
                }
                if (i > 0)
                {
                    for (int j = 0; j < i; j++)
                    {
                        s = "0" + s;
                    }
                }
            }
            return s;
        }
        private string[] Splitfunction(string function)
        {
            string s = function;
            int index = s.IndexOf('(');
            while (index != -1)
            {
                s = s.Remove(index, 1);
                index = s.IndexOf('(');
            }
            index = s.IndexOf(')');
            while (index != -1)
            {
                s = s.Remove(index, 1);
                index = s.IndexOf(')');
            }
            index = s.IndexOf('!');
            while (index != -1)
            {
                s = s.Remove(index, 1);
                index = s.IndexOf('!');
            }
            string[] monom = function.Split('+');
            return monom;
        }

        #endregion
    }
}