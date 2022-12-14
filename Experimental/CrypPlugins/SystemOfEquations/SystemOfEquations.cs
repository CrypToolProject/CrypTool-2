using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace CrypTool.SystemOfEquations
{
    [Author("Abdeljalil Bourbahh", "bourbahh@yahoo.de", "Ruhr-Universitaet Bochum, Chair for System Security", "http://www.trust.rub.de/")]
    [PluginInfo("Alg. attack: System of equations", "generate a System of equation for algebric attack against Combiner of streamcipher", "SystemOfEquations/DetailedDescription/Description.xaml",
 new[] { "SystemOfEquations/Images/soe.png" })]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class SystemOfEquations : ICrypComponent
    {
        #region Private variables
        private SystemOfEquationsSettings settings;
        private Hashtable inputZfunctions;// Input
        private string[] keystreams;
        private string[] feedbackpolynomials;
        private string[] lfsrsoutputs;
        private string outputString;
        #endregion
        public SystemOfEquations()
        {
            settings = new SystemOfEquationsSettings();
            settings.LogMessage += SystemOfEquations_LogMessage;
        }
        public ISettings Settings
        {
            get => settings;
            set => settings = (SystemOfEquationsSettings)value;
        }
        [PropertyInfo(Direction.InputData, "Z-functions ", "Z-functions as  Hashtable(Z ,FZ) delivred from plugin compute annihilators", true)]
        public Hashtable InputAhnilators
        {
            get => inputZfunctions;
            set
            {
                if (value != inputZfunctions)
                {
                    inputZfunctions = value;
                    OnPropertyChanged("InputAhnilators");
                }
            }
        }
        [PropertyInfo(Direction.OutputData, "System of equation function", " the variables are die Keys Bits (k1,k2..kn)=initial statue of LFSRs", false)]
        public string OutputString
        {
            get => outputString;
            set
            {
                outputString = value;
                OnPropertyChanged("OutputString");
            }
        }
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
        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }
        private void SystemOfEquations_LogMessage(string msg, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(msg, this, logLevel));
            }
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
                if (inputZfunctions != null && settings.Feedbackpolynomials != null && settings.Lfsrsoutputs != null && settings.Keystream != null)
                {
                    if (IsLFSRsParameterlvalid() && IsKeyStreamValid())
                    {
                        int runlength = GetRunlength(inputZfunctions, lfsrsoutputs);
                        if (runlength != 0)
                        {
                            Combiner combiner = new Combiner(feedbackpolynomials, lfsrsoutputs);
                            string output = "";
                            ArrayList[] keystream = StructureKeyStream(runlength);
                            int begintime = 1;
                            ArrayList ts = keystream[0];
                            ArrayList keys = keystream[1];
                            for (int j = 0; j < ts.Count; j++)
                            {
                                int t = (int)ts[j];
                                int time = t - begintime;
                                combiner.Clock(time);
                                int[] Zset = (int[])keys[j];
                                begintime = t + Zset.Length * runlength;
                                for (int i = 0; i < Zset.Length; i++)
                                {
                                    ArrayList[][] Kt = combiner.Outputs(runlength);
                                    ArrayList Ktstring = combiner.SortOutput(Kt);
                                    int outpusize = Ktstring.Count / runlength;
                                    if (inputZfunctions[Zset[i]] != null)
                                    {
                                        string s = ReplaceVariables((ArrayList)inputZfunctions[Zset[i]], Ktstring);
                                        output = output + s;
                                    }
                                    else
                                    {
                                        output = output + "ahnilators correspond to Z= " + Bitpresentation(Zset[i], runlength) + "is not provided" + "\r\n";
                                    }
                                }
                            }
                            outputString = output;
                            OnPropertyChanged("OutputString");
                        }
                        else
                        {
                            GuiLogMessage("No Z-function is provided", NotificationLevel.Error);
                        }
                    }
                }
                else
                {
                    GuiLogMessage("not all mandatory Input is provided", NotificationLevel.Error);
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
        public string ReplaceVariables(ArrayList Zfunctions, ArrayList lfsroutput)
        {
            StringBuilder outputstring = new StringBuilder();
            foreach (ArrayList Zfunction in Zfunctions)
            {
                string equation = "";
                foreach (bool[] monomial in Zfunction)
                {
                    string s = "";
                    for (int i = 0; i < monomial.Length; i++)
                    {
                        if (monomial[i])
                        {
                            s = s + (string)lfsroutput[i] + "*";
                        }
                    }
                    if (s == "") { s = "" + 1; }
                    else { s = s.Remove(s.LastIndexOf("*")); }
                    equation = equation + s + "+";
                }
                equation = SimplifyFunction(equation.Remove(equation.LastIndexOf("+")));
                outputstring.AppendLine(equation + "=0");
            }
            return outputstring.ToString();
        }
        public ArrayList[] StructureKeyStream(int run)
        {
            ArrayList timeseq = new ArrayList();
            ArrayList Zseq = new ArrayList();
            for (int j = 0; j < keystreams.Length; j++)
            {
                string[] streami = keystreams[j].Split(',');
                int t_i = Convert.ToInt16(streami[0]);
                timeseq.Add(t_i);
                string streamseq = streami[1];
                int index = 0;
                int seqlength = streamseq.Length;
                int Zcount = seqlength / run;
                int[] Z = new int[Zcount];
                for (int i = 0; i < Zcount; i++)
                {
                    string Zstring = streamseq.Substring(index, run);
                    int Zi = IntValue(Zstring);
                    Z[i] = Zi;
                    index = index + run;
                }
                Zseq.Add(Z);
            }
            ArrayList[] KeyStream = new ArrayList[2];
            KeyStream[0] = timeseq;
            KeyStream[1] = Zseq;
            return KeyStream;
        }
        public bool IsLFSRsParameterlvalid()
        {
            feedbackpolynomials = settings.Feedbackpolynomials.Replace(" ", "").Split(';');
            lfsrsoutputs = settings.Lfsrsoutputs.Replace(" ", "").Split(';');
            Regex objBoolExpression = new Regex("^(1|0)+$");
            if (feedbackpolynomials.Length != feedbackpolynomials.Length)
            {
                GuiLogMessage("The count of feedbackpolynomials and LSFRsoutputs is not equal ", NotificationLevel.Error);
                return false;

            }
            for (int i = 0; i < feedbackpolynomials.Length; i++)
            {
                if (feedbackpolynomials[i].Length != lfsrsoutputs[i].Length)
                {
                    GuiLogMessage("the length of feedbackpolynomial and LSFRsoutput of " + (i + 1) + "th LFSR  is not equal", NotificationLevel.Error);
                    return false;
                }
                if (!objBoolExpression.IsMatch(feedbackpolynomials[i]))
                {
                    GuiLogMessage("the feedbackpolynomial" + feedbackpolynomials[i] + "is not a legal ", NotificationLevel.Error);
                    return false;
                }
                if (!objBoolExpression.IsMatch(lfsrsoutputs[i]))
                {
                    GuiLogMessage("the LSFRsoutput" + lfsrsoutputs[i] + " is not a legal ", NotificationLevel.Error);
                    return false;
                }
            }
            return true;
        }
        public bool IsKeyStreamValid()
        {
            keystreams = settings.Keystream.Replace(" ", "").Split(';');
            Regex objBoolExpression = new Regex("^(1|0)+$");
            int lasttime = 1;
            for (int i = 0; i < keystreams.Length; i++)
            {
                string[] keyseq = keystreams[i].Split(',');
                char[] dig = keyseq[0].ToCharArray();
                for (int j = 0; j < dig.Length; j++)
                {
                    if (!char.IsDigit(dig[j]))
                    {
                        GuiLogMessage("time in" + (i + 1) + "th keytreamsequence number is not a dezimal number", NotificationLevel.Error);
                        return false;
                    }
                }
                if (!objBoolExpression.IsMatch(keyseq[1]))
                {
                    GuiLogMessage("keystream in " + (i + 1) + "the keytreamsequence number  is not a not ligal", NotificationLevel.Error);
                    return false;
                }
                if (Convert.ToInt16(keyseq[0]) < lasttime)
                {
                    GuiLogMessage("the time t_(i+1) must > t_i + length of ith keystreamsequence ", NotificationLevel.Error);
                    return false;
                }
                lasttime = Convert.ToInt16(keyseq[0]) + keyseq[1].Length;
            }
            return true;
        }
        private int GetRunlength(Hashtable zfunctions, string[] outputcells)
        {
            int lfsroutputcount = 0;
            int Xsize = 0;
            for (int i = 0; i < zfunctions.Count; i++)
            {
                ArrayList l2 = (ArrayList)zfunctions[i];
                if (l2.Count != 0)
                {
                    ArrayList l = (ArrayList)l2[0];
                    bool[] monom0 = (bool[])l[0];
                    Xsize = monom0.Length;
                    break;
                }
            }
            for (int i = 0; i < outputcells.Length; i++)
            {
                char[] C = outputcells[i].ToCharArray();
                for (int j = 0; j < C.Length; j++)
                {
                    if (C[j] == '1')
                    {
                        lfsroutputcount++;
                    }
                }
            }
            return Xsize / lfsroutputcount;
        }
        private int IntValue(string bitsequence)
        {
            int value = 0;
            int index = bitsequence.Length - 1;
            for (int i = 0; i < bitsequence.Length; i++)
            {
                index = index - i;
                char bit = bitsequence[i];
                if (bit == '1')
                {
                    value = value + exp(2, index);
                }
            }
            return value;
        }
        private string SimplifyFunction(string function)
        {
            while (function.IndexOf("(") != -1)
            {
                int z1 = function.IndexOf("(");
                int z2 = function.IndexOf(")");
                string bispar = function.Substring(0, z1);
                int plus1 = bispar.LastIndexOf("+") + 1;
                int plus2 = z1 + FindEndOfTerm(function.Substring(z1));
                string Term = function.Substring(plus1, plus2 - plus1);
                function = function.Remove(plus1, Term.Length);
                Term = ExpandOneTerm(Term);
                function = function.Insert(plus1, Term);
            }
            return ReduceEquation(function);
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
        private string ReduceEquation(string function)
        {
            string[] monomials = function.Split('+');
            for (int i = 0; i < monomials.Length; i++)
            {
                string monomial = OrderMonomial(monomials[i]);
                monomials[i] = monomial;
            }
            for (int i = 0; i < monomials.Length - 1; i++)
            {
                if (monomials[i] != null)
                {
                    bool delete = false;
                    for (int j = i + 1; j < monomials.Length; j++)
                    {
                        if (monomials[i] == monomials[j])
                        {
                            monomials[j] = null;
                            delete = !delete;
                        }

                    }
                    if (delete)
                    {
                        monomials[i] = null;
                    }
                }
            }
            string equation = "";
            for (int i = 0; i < monomials.Length; i++)
            {
                if (monomials[i] != null)
                {
                    equation = equation + monomials[i] + "+";
                }
            }
            equation = equation.Remove(equation.LastIndexOf("+"));
            return equation;
        }
        private string OrderMonomial(string monomial)
        {
            string[] literals = monomial.Split('*');
            int i = 0;
            while (i < literals.Length && literals[i] != null)
            {
                for (int j = i + 1; j < literals.Length; j++)
                {
                    if (string.Compare(literals[j], literals[i]) < 0)
                    {
                        string temp = literals[j];
                        literals[j] = literals[i];
                        literals[i] = temp;
                    }
                    else
                    {
                        if (string.Compare(literals[j], literals[i]) == 0)
                        {
                            literals[j] = null;
                        }
                    }
                }
                i++;
            }
            string s = "";
            for (int k = 0; k < literals.Length; k++)
            {
                if (literals[k] != null)
                {
                    s = s + literals[k] + "*";
                }
            }
            s = s.Remove(s.LastIndexOf("*"));
            return s;
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
        //xor durchführen zwei gleiche Terme reduzieren ( das wird nicht gemacht im XZ compute)

        #endregion

        public int exp(int b, int e)
        {
            int erg = 1;
            for (int i = 1; i <= e; i++)
            {
                erg = erg * b;
            }
            return erg;
        }
    }// class System ofequation

    // Class Combiner

    //public class Combiner2
    //{
    //    public LFSR[] lfsrs;
    //    public Combiner(string[] caracpolynoms, string[] outputcells)
    //    {
    //        lfsrs = new LFSR[caracpolynoms.Length];
    //        for (int i = 0; i < caracpolynoms.Length; i++)
    //        {
    //            LFSR lfsr = new LFSR(caracpolynoms[i]);
    //            lfsr.outputcells = outputcells[i].ToCharArray();
    //            lfsrs[i] = lfsr;
    //        }
    //    }
    //    public ArrayList[] OneClockOutput()
    //    {
    //        ArrayList[] Xi = new ArrayList[lfsrs.Length];
    //        for (int i = 0; i < lfsrs.Length; i++)
    //        {
    //            Xi[i] = lfsrs[i].Output();
    //        }
    //        return Xi;
    //    }
    //    public void Clock(int time)
    //    {
    //       for (int i = 0; i < lfsrs.Length; i++)
    //       {
    //           lfsrs[i].Clock(time);

    //       }            
    //    }
    //    public void OneClock()
    //    {
    //        for (int i = 0; i < lfsrs.Length; i++)
    //        {
    //                lfsrs[i].OneClock();
    //        }             
    //    }
    //    public ArrayList[][] RunClockOutput(int run)
    //    {
    //        ArrayList[][] runoutput= new ArrayList[run][];
    //        for (int i = 0; i < run; i++)
    //        {
    //            ArrayList[] Xi = OneClockOutput();
    //            runoutput[i] = Xi;
    //            OneClock();
    //        }
    //        return runoutput;
    //    }
    //    public ArrayList SortOutput(ArrayList[][] outputs)
    //    {
    //        ArrayList sortedoutput = new ArrayList();
    //        for (int i = 0; i < outputs.Length; i++)
    //        {
    //            int startindex = 1;
    //            for (int j = 0; j < outputs[i].Length; j++)
    //            {
    //                ArrayList lsfroutput = outputs[i][j];
    //                int lfsrlength = lfsrs[j].length;
    //                foreach (bool[] cell in lsfroutput)
    //                {
    //                    string cellstring= SubstituteOutput(cell,startindex);
    //                    sortedoutput.Add(cellstring);                   
    //                }
    //                startindex = startindex + lfsrlength;
    //            }
    //        }
    //        return sortedoutput;
    //    }
    //   public string SubstituteOutput(bool[] celloutput, int index)
    //   {
    //       string s = "";
    //       for (int j = 0; j < celloutput.Length; j++)
    //       {
    //           if (celloutput[j]) s =s+"k"+(j+index)+"+";
    //       }
    //       s = s.Remove(s.Length - 1);
    //       if (s.Contains("+")) s = "(" + s + ")";    
    //       return s;
    //   }
    //}

    //Class LfSR

    //public class LfSR
    //{
    //    public int length;
    //    string characteristicpolynom;
    //    public bool[][] internalstate;
    //    public char[] outputcells;       
    //    public LfSR(string charpolynom)
    //    {
    //        characteristicpolynom = charpolynom;
    //        length = charpolynom.Length;
    //        internalstate = new bool[length][];
    //        for (int i = 0; i < internalstate.Length; i++)
    //        {
    //            internalstate[i] = new bool[length];
    //            for (int j = 0; j < internalstate.Length; j++)
    //            {
    //                if (i == j) internalstate[i][j] = true;
    //                else internalstate[i][j] = false;
    //            }
    //        }
    //    }
    //    public bool[] XorCells(bool[] cell1, bool[] cell2)
    //    {
    //        bool[] res = new bool[cell2.Length];
    //        for (int i = 0; i < cell2.Length; i++)
    //        {
    //            res[i] = cell1[i] ^ cell2[i];
    //        }
    //        return res;
    //    }
    //    public void OneClock() 
    //    {
    //        char[] c = characteristicpolynom.ToCharArray();
    //        bool[] res = new bool[length];
    //        res = internalstate[0];
    //        for (int k =1;  k<length; k++)
    //        {
    //            if (c[k] == '1') res = XorCells(res,internalstate[k]);
    //        }
    //        for (int j = 0; j<internalstate.Length-1; j++)
    //        {
    //            internalstate[j] = internalstate[j + 1];
    //        }
    //        internalstate[length-1] = res;
    //    }
    //    public void Clock(int time)
    //    {
    //        for (int i = 0; i < time; i++)
    //        {
    //            OneClock();
    //        }
    //    }
    //    public ArrayList Output()
    //    {
    //        ArrayList lfsroutput = new ArrayList();
    //        for (int i = 0; i < outputcells.Length; i++)
    //        {
    //            if (outputcells[i] == '1')
    //            lfsroutput.Add(internalstate[i]);
    //        }
    //        return lfsroutput;
    //    }
    //}




}

