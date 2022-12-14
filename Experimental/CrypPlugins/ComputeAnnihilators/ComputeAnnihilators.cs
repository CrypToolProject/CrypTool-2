using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace CrypTool.ComputeAnnihilators
{
    [Author("Abdeljalil Bourbahh", "bourbahh@yahoo.de", "Ruhr-Universitaet Bochum, Chair for System Security", "http://www.trust.rub.de/")]
    [PluginInfo("Alg.Attack: Compute annihilators", "compute annihilators of Function, Set of bitsquence or a set XZ (Z-functions)", "ComputeAnnihilators/DetailedDescription/Description.xaml", new[] { "ComputeAnnihilators/Images/ann.png" })]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class ComputeAnnihilators : ICrypComponent
    {
        #region Private variables
        private ComputeAnnihilatorsSettings settings;
        private object input;
        private Hashtable outputFZ;
        private string outputstring;
        private int dimention;
        private bool stop = false;
        private bool start = false;
        #endregion
        public ComputeAnnihilators()
        {
            settings = new ComputeAnnihilatorsSettings();

            settings.LogMessage += ComputeAnnihilators_LogMessage;
        }
        public ISettings Settings
        {
            get => settings;
            set => settings = (ComputeAnnihilatorsSettings)value;
        }
        [PropertyInfo(Direction.InputData, "input as Object", "boolean function (string),set of bisequences(string)or Hashtable (Z,XZ) delivred from pluin copmute th sets XZ", false)]
        public object Input
        {
            get => input;
            set
            {
                if (value != input)
                {
                    input = value;
                    OnPropertyChanged("Input");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "annihilators  as string", "to display annihilators or Z-functions in Textoutput", false)]
        public string OutputString
        {
            get => outputstring;
            set
            {
                outputstring = value;
                OnPropertyChanged("OutputString");
            }

        }
        [PropertyInfo(Direction.OutputData, " annihilators as (Hashtable(Z,F_Z))", "to use as Input of Pugin System of equation", false)]
        public Hashtable OutputFZ
        {
            get => outputFZ;
            set
            {
                outputFZ = value;
                OnPropertyChanged("OutputFZ");
            }
        }
        #region IPlugin members
        public void Initialize()
        {
        }
        public void Dispose()
        {
        }
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }
        public UserControl Presentation => null;
        public void Stop()
        {
            if (start && !settings.ComputeEnded && settings.ActionSetting == 0)
            {
                stop = true;
            }
        }
        public void PostExecution()
        {
            Dispose();
        }

        public void PreExecution()
        {
            Dispose();
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
        private void ComputeAnnihilators_LogMessage(string msg, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(msg, this, logLevel));
            }
        }
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }
        #endregion
        #region IPlugin Members

#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore
        public void Execute()
        {
            try
            {
                switch (settings.ActionSetting)
                {
                    case 0:
                        CombinerZFunctions();
                        break;
                    case 1:
                        FunctionAnnihilators();
                        break;
                    case 2:
                        BitsequencesetAnnihilators();
                        break;
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
        public void SaveParameter(Hashtable XZ)
        {
            ArrayList parameter = (ArrayList)XZ[XZ.Count - 1];
            string outputfunc = (string)parameter[0];
            string memUpdfunc = (string)parameter[1];
            int runlenght = (int)parameter[2];
            parameter.Remove(XZ.Count - 1);
            if (IsParameterNew(outputfunc, memUpdfunc, runlenght))
            {
                settings.ComputeEnded = false;
                settings.Savedoutputfunction = outputfunc;
                settings.Savedmemoryupdatefunction = memUpdfunc;
                settings.Savedrunlength = runlenght;
                settings.Saveddegree = settings.Degree;
            }
            else
            {
                if ((settings.SavedZfunctions != null))
                {
                    foreach (DictionaryEntry dicEntry in settings.SavedZfunctions)
                    {
                        outputFZ.Add((int)dicEntry.Key, (ArrayList)dicEntry.Value);
                    }
                }
            }
        }
        public void CombinerZFunctions()
        {
            if (input != null && input.GetType().Equals(typeof(Hashtable)) && settings.OutputSet != null)
            {
                if (IsOutputSetvalid(settings.OutputSet))
                {
                    Hashtable h = (Hashtable)input;
                    int[] Zvalue = GetOutputs(settings.OutputSet);
                    int Xlength = ((bool[])((ArrayList)h[0])[0]).Length;
                    outputFZ = new Hashtable();
                    start = true;
                    stop = false;
                    SaveParameter(h);
                    for (int i = 0; i < Zvalue.Length; i++)
                    {
                        if (stop)
                        {
                            settings.SavedZfunctions = outputFZ;
                            break;
                        }
                        int z = Zvalue[i];
                        ArrayList Xi = (ArrayList)h[z];
                        if (!outputFZ.Contains(z))
                        {
                            outputstring = "compute F_=" + Bitpresentation(z, settings.OutputSet.Length);
                            OnPropertyChanged("OutputString");
                            ArrayList Ahnilators = Computeannihilators(Xi, settings.Degree);
                            outputFZ.Add(z, Ahnilators);
                        }
                    }
                    if (!stop)
                    {
                        switch (settings.OutputSetting)
                        {
                            case 0:
                                DisplayFZ(Zvalue);
                                break;
                            case 1:
                                OnPropertyChanged("OutputFZ");
                                break;
                            case 2:
                                OnPropertyChanged("OutputFZ");
                                DisplayFZ(Zvalue);
                                break;
                        }
                        settings.ComputeEnded = true;
                        settings.SavedZfunctions = outputFZ;
                    }
                    start = false;
                }
            }
            else
            {
                GuiLogMessage("not all mandatory parameters are provided", NotificationLevel.Error);
            }
        }
        public void DisplayFZ(int[] Zvalues)
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine();
            for (int i = 0; i < Zvalues.Length; i++)
            {
                int z = Zvalues[i];
                b.AppendLine("F_" + Bitpresentation(z, settings.OutputSet.Length));
                string s = (ToStringAnnihilators((ArrayList)outputFZ[z]));
                if (s == "")
                {
                    b.AppendLine("F_" + Bitpresentation(z, settings.OutputSet.Length) + " of degree " + settings.Degree + " don't exist ");
                }
                else
                {
                    b.AppendLine(s);
                }
            }
            outputstring = b.ToString();
            OnPropertyChanged("OutputString");
        }
        public bool IsParameterNew(string outputfunction, string memoryupdatefunction, int outputlength)
        {
            if (memoryupdatefunction == "")
            {
                if (settings.Savedoutputfunction == outputfunction && settings.Savedrunlength == outputlength
                    && settings.Saveddegree == settings.Degree)
                {
                    return false;
                }
            }
            else
            {
                if (settings.Savedoutputfunction == outputfunction && settings.Savedmemoryupdatefunction ==
                    memoryupdatefunction && settings.Savedrunlength == outputlength && settings.Saveddegree == settings.Degree)
                {
                    return false;
                }
            }
            return true;
        }
        private void BitsequencesetAnnihilators()
        {
            if (input != null && input.GetType().Equals(typeof(string)))
            {
                string seqsetstring = (string)input;
                if (IsBitSeqSetValid(seqsetstring))
                {
                    ArrayList bitseqlist = new ArrayList();
                    string[] seqsetarray = seqsetstring.Split(',');
                    int bitseq_length = seqsetarray[0].Length;
                    for (int k = 0; k < seqsetarray.Length; k++)
                    {
                        bool[] bitseq = new bool[bitseq_length];
                        for (int j = 0; j < bitseq.Length; j++)
                        {
                            if (seqsetarray[k].ToCharArray()[j] == '1')
                            {
                                bitseq[j] = true;
                            }
                            else
                            {
                                bitseq[j] = false;
                            }
                        }
                        bitseqlist.Add(bitseq);
                    }
                    ArrayList annihilators = Computeannihilators(bitseqlist, settings.Degree);
                    string s = ToStringAnnihilators(annihilators);
                    if (s == "")
                    {
                        outputstring = "no annihilator of degree <= " + settings.Degree + " is located";
                    }
                    else
                    {
                        outputstring = "annihilators of the given Set of degree =< " + settings.Degree + "\r\n" + s;
                    }

                    OnPropertyChanged("OutputString");
                }
            }
            else
            {
                GuiLogMessage("not all mandatory parameters are provided", NotificationLevel.Error);
            }
        }
        private bool IsOutputSetvalid(string expression)
        {
            Regex regExpression1 = new Regex("^(1|[\\*]|0)+$");
            Hashtable h = (Hashtable)input;
            ArrayList parameter = (ArrayList)h[h.Count - 1];
            int runlenght = (int)parameter[2];
            if (runlenght != settings.OutputSet.Length)
            {
                GuiLogMessage("the runlength (considered clocks) must the same in both plugins", NotificationLevel.Error);
                return false;
            }
            if (!regExpression1.IsMatch(expression))
            {
                GuiLogMessage("the expression of the set of outputs " + expression + " is not legal", NotificationLevel.Error);
                return false;
            }
            return true;
        }
        private bool IsBitSeqSetValid(string bitseqset)
        {
            string[] bitseqs = bitseqset.Split(',');
            int seqlength = bitseqs[0].Length;
            Regex objBoolExpression = new Regex("^[0-1]+$");
            for (int i = 0; i < bitseqs.Length; i++)
            {
                if (bitseqs[i].Length != seqlength)
                {
                    GuiLogMessage(" not all Bitsequence are  the same length ", NotificationLevel.Error);
                    return false;
                }
                if (!objBoolExpression.IsMatch(bitseqs[i]))
                {
                    GuiLogMessage("The bitsquence expression is not legal ", NotificationLevel.Error);
                    return false;
                }
            }
            return true;
        }
        public ArrayList Computeannihilators(ArrayList S, int deg)
        {
            int Xlength = ((bool[])S[0]).Length;
            ArrayList monomials = GenerateMonomials(Xlength, deg);
            ArrayList F = monomials;
            foreach (bool[] X in S)
            {
                ArrayList F0 = new ArrayList();
                ArrayList F1 = new ArrayList();
                foreach (ArrayList f in F)
                {
                    if (EvaluateFunction(f, X))
                    {
                        F1.Add(f);
                    }
                    else
                    {
                        F0.Add(f);
                    }
                }
                if (F1.Count > 1)
                {
                    ArrayList g = (ArrayList)F1[0];
                    for (int i = 1; i < F1.Count; i++)
                    {
                        foreach (bool[] monomial in g)
                        {
                            if (((ArrayList)F1[i]).Contains(monomial))
                            {
                                ((ArrayList)F1[i]).Remove(monomial);
                            }
                            else
                            {
                                ((ArrayList)F1[i]).Add(monomial);
                            }
                        }
                        if (!F0.Contains(((ArrayList)F1[i])))
                        {
                            F0.Add(((ArrayList)F1[i]));//G0 U F0
                        }
                    }
                }
                F = F0;
            }
            return F;
        }
        public ArrayList GenerateMonomials(int dim, int deg)
        {
            ArrayList monomials = new ArrayList();
            for (int i = 0; i < exp(2, dim); i++)// 
            {
                bool[] monomial = ConstructBitsequence(i, dim);
                int wight = 0;
                for (int j = 0; j < monomial.Length; j++)
                {
                    if (monomial[j])
                    {
                        wight = wight + 1;
                    }
                }
                if (wight <= deg)
                {
                    ArrayList fmonomial = new ArrayList
                    {
                        monomial
                    };
                    monomials.Add(fmonomial);
                }
            }
            return monomials;
        }
        private void FunctionAnnihilators()
        {
            if (input != null && input.GetType().Equals(typeof(string)))
            {
                string function = (string)input;
                if (IsFunctionExpValid(function))
                {
                    ArrayList suppf = new ArrayList();
                    string simplef = SimplifyFunction(function);
                    ArrayList convertf = ConvertSimpleFunction(simplef, dimention);
                    for (int x = 0; x < exp(2, dimention); x++)
                    {
                        bool[] X = ConstructBitsequence(x, dimention);
                        bool res = EvaluateFunction(convertf, X);
                        if (res)
                        {
                            suppf.Add(X);
                        }
                    }
                    ArrayList annihilators = Computeannihilators(suppf, settings.Degree);
                    string s = ToStringAnnihilators(annihilators);
                    if (s == "")
                    {
                        outputstring = "no annihilator of degree  = " + settings.Degree + " is located";
                    }
                    else
                    {
                        outputstring = "annihilators of the given function of degree smaller or equal " + settings.Degree + "\r\n" + s;
                    }

                    OnPropertyChanged("OutputString");
                }
            }
            else
            {
                GuiLogMessage("not all mandatory parameters are provided", NotificationLevel.Error);
            }
        }
        private bool EvaluteMonomial(bool[] monom, bool[] input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (monom[i] && !input[i])
                {
                    return false;
                }
            }
            return true;

        }
        public bool EvaluateFunction(ArrayList function, bool[] input)
        {
            bool res = EvaluteMonomial((bool[])function[0], input);
            for (int j = 1; j < function.Count; j++)
            {
                res = res ^ (EvaluteMonomial((bool[])function[j], input));
            }
            return res;
        }
        public string ToStringAnnihilators(ArrayList annihilators)
        {
            StringBuilder strBuilder = new StringBuilder();
            if (annihilators.Count == 0)
            {
                return "";
            }
            else
            {
                int index = 1;
                foreach (ArrayList annihilator in annihilators)
                {
                    string function = "";
                    foreach (bool[] monomial in annihilator)
                    {
                        string s = "";
                        for (int i = 0; i < monomial.Length; i++)
                        {
                            if (monomial[i])
                            {
                                s = s + "x" + (i + 1) + "*";
                            }
                        }
                        if (s == "")
                        {
                            s = "" + 1;
                        }
                        else
                        {
                            s = s.Remove(s.LastIndexOf("*"));
                        }

                        function = function + s + "+";
                    }
                    function = function.Remove(function.LastIndexOf("+"));
                    function = index + "-  " + function;
                    strBuilder.AppendLine(function);
                    index++;
                }
            }
            return strBuilder.ToString();
        }






        private ArrayList ConvertSimpleFunction(string function, int dim)
        {
            ArrayList convertfunction = new ArrayList();
            string[] monomials = function.Split('+');
            for (int i = 0; i < monomials.Length; i++)
            {
                string[] literals = monomials[i].Split('*');
                bool[] monom = new bool[dim];
                for (int j = 0; j < monom.Length; j++)
                {
                    monom[j] = false;
                }
                for (int j = 0; j < literals.Length; j++)
                {
                    string literal = literals[j];
                    if (literal.StartsWith("x"))
                    {
                        if (literal.StartsWith("x"))
                        {
                            int index = Convert.ToInt16(literal.Substring(1, literal.Length - 1));
                            if (index <= dim)
                            {
                                monom[index - 1] = true;
                            }
                            else
                            {
                                GuiLogMessage("the indexes of variable must begin  from 1 and to be successiv ", NotificationLevel.Error);
                            }
                        }
                    }
                    //monom[Convert.ToInt16(literal.Substring(1, literal.Length - 1)) - 1] = true;

                }
                convertfunction.Add(monom);
            }
            return convertfunction;
        }



        #endregion

        //private int CountLFSROutputs(string function)
        //{
        //    ArrayList lfsrvar = new ArrayList();
        //    string[] monom = function.Split('+');
        //    for (int i = 0; i < monom.Length; i++)
        //    {
        //        string[] literals = monom[i].Split('*');
        //        for (int j = 0; j < literals.Length; j++)
        //        {
        //            string literal = literals[j];
        //            if (literal.StartsWith("x"))
        //            {
        //                if (!lfsrvar.Contains(literal)) lfsrvar.Add(literal);
        //            }
        //        }
        //    }
        //    return lfsrvar.Count;
        //}

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
        //public int ComputeAI(ArrayList annihilators)
        //{
        //    if (annihilators.Count == 0) return 0;
        //    else
        //    {
        //        int AI=1000;
        //        foreach (ArrayList annihilator in annihilators)
        //        {
        //           int f_ai=0;
        //           foreach (bool[] monomials in annihilator)
        //           {
        //                int monom_ai = 0;
        //                for (int i = 0; i < monomials.Length; i++)
        //                {
        //                    if (monomials[i]) monom_ai++;
        //                }
        //                if (monom_ai>f_ai) f_ai = monom_ai;
        //            }
        //           if (f_ai<AI) AI=f_ai;
        //        }
        //        return AI;
        //    }  
        //}


        #region function parser
        private string SimplifyFunction(string function)
        {
            return ReduceFunction(SimplifyParentheses(NegateTerms(function)));
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
                    // die zu negierende Term selektieren
                    string subf = f.Substring(notindex + 1);
                    string[] subterms = subf.Split('+');
                    string[] subliterals = subterms[0].Split('*');
                    string tonegatliteral = subliterals[0];
                    //die zu negierende Term selektieren löschen und ersetzen
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
                    string topar = f.Substring(0, p1);//string bis die Parenthese
                    int begin = topar.LastIndexOf("+") + 1;// index des Anfang des Termes ohne plus,
                    int end = p1 + FindEndOfTerm(f.Substring(p1));// Ende des Termes:Index vom plus
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
        public string ExpandOneTerm(string term)//devlopet ein Term, das zwei oder mehr als zwei Faktoren enthält
        {//func besteht aus Terme mit * verknüpft die Terme nur die Operator+ beinhalten (k+k+k)*(k+..+k)*..*(k+k+k)*(k+..+k)
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
                res = (string)terms[0];//mit ()
            }
            else
            {
                res = "(" + LiftRightExpand(terms[0] + "*" + terms[1]) + ")";//mit ()
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
            else//(res)
            {
                res = res.Remove(0, 1);
                res = res.Remove(res.Length - 1, 1);
            }
            return res;
        }
        public string RightExpand(string term)
        {//k1*k2*(k1+..+k8)=>k1*k2*k1+..+k1*k2*k8
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
        {//(k1+..+k8)*k7*k8=>k1*k7*k8+..+k8*k7*k8
            int p1 = term.IndexOf("(");
            int p2 = term.IndexOf(")");
            string fac1 = term.Substring(p2 + 1, term.Length - p2 - 1);//factor zu multipluzieren
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
        {// (k1..k6)*(k1...k12)=>(k1..k6)*k1+..+(k1..k6)*k12
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
        public static string ReduceFunction(string function)// alle Terme reduzieren
        {
            //1*X=X*1=X
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
            //X*X=X
            string[] monomials = function.Split('+');
            string s = "";
            for (int i = 0; i < monomials.Length; i++)
            {
                s = s + ReduceTerm(monomials[i]) + "+";
            }
            s = s.Remove(s.LastIndexOf("+"));
            return s;
        }
        private static string ReduceTerm(string term)// wenn zwei literale eine wird gelöscht wenn xi und !xi exist Term wird gelöscht
        {
            string[] literals = term.Split('*');
            string s = "";
            for (int i = 0; i < literals.Length - 1; i++)
            {
                if (literals[i] != "")
                {
                    for (int j = i + 1; j < literals.Length; j++)
                    {
                        if (literals[i] == literals[j])//xi*xi=xi
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
        public string SimplifyParentheses(string f)//löst ein Schachtelung
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
                        else
                        {
                            overt++;
                        }
                    }
                    if (c == ')')
                    {
                        if (overt == 1)
                        {
                            overt = 0;
                        }
                        else //es gibt Schachtelung
                        {
                            closed++;
                            if (closed == overt)// Ende der Schachtelung
                            {// löst die Schachtelung
                                overt = 0;
                                closed = 0;
                                lastclosed = index;
                                string toexpand = f.Substring(firstovert + 1, lastclosed - firstovert - 1);
                                f = f.Remove(firstovert + 1, lastclosed - firstovert - 1);
                                f = f.Insert(firstovert + 1, SimplifyParentheses(toexpand));
                                f = SimplifyParentheses(f);
                            }// löst die Schachtelung
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
        private string[] Splitfunction(string function)
        {
            string er = function;
            int index = er.IndexOf("(");
            while (index != -1)
            {
                er = er.Remove(index, 1);
                index = er.IndexOf("(");
            }
            index = er.IndexOf(")");
            while (index != -1)
            {
                er = er.Remove(index, 1);
                index = er.IndexOf(")");
            }
            index = er.IndexOf("!");
            while (index != -1)
            {
                er = er.Remove(index, 1);
                index = er.IndexOf("!");
            }
            string[] monom = function.Split('+');
            return monom;
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
            dimention = lfsrvar.Count;// Anzahl der LFSRs-Ausgänge wird berechnet
            foreach (string lit in lfsrvar)
            {
                int index = Convert.ToInt16(lit.Substring(1, lit.Length - 1));
                if (!(1 <= index && index <= dimention))
                {
                    GuiLogMessage("the indexes of variable x_i must begin  from 1 and to be successiv ", NotificationLevel.Error);
                    return false;
                }
            }
            return true;
        }
        public bool IsFunctionExpValid(string fstring)
        {
            fstring = fstring.Replace(" ", "");
            Regex objBoolExpression = new Regex("^(([\\!]?[\\(])*(((\\!?)(x))(\\d+)|1)[\\)]*(\\*|\\+))*(((\\!?)(x))(\\d+)|1)[\\)]*$");
            if (!IsParenthesesvalid(fstring))
            {
                GuiLogMessage("the  Parentheses in " + fstring + " is not valid", NotificationLevel.Error);
                return false;
            }
            if (!objBoolExpression.IsMatch(fstring))
            {
                GuiLogMessage(fstring + "is not a legal function", NotificationLevel.Error);
                return false;
            }
            if (!IsSimpleIndicesValid(fstring))
            {
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
        public int[] GetOutputs(string outputsexp)
        {
            char[] c = outputsexp.ToCharArray();
            int[] Z;
            int Zlength = 0;
            int sum = 0;
            ArrayList sumlist = new ArrayList();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == '*')//*
                {
                    Zlength++;
                    sumlist.Add(exp(2, c.Length - 1 - i));
                }
                else//0 oder 1
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

        #endregion





    }// class ahnilatorscompute


}// namesspaces

