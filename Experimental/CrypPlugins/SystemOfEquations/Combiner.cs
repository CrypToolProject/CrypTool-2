using System.Collections;
namespace CrypTool.SystemOfEquations
{
    internal class Combiner
    {
        public LFSR[] lfsrs;
        public Combiner(string[] fbpolynomials, string[] outputcells)
        {
            lfsrs = new LFSR[fbpolynomials.Length];
            for (int i = 0; i < fbpolynomials.Length; i++)
            {
                LFSR lfsr = new LFSR(fbpolynomials[i])
                {
                    outputcells = outputcells[i].ToCharArray()
                };
                lfsrs[i] = lfsr;
            }
        }
        public ArrayList[] LFSRsOutput()
        {
            ArrayList[] Xi = new ArrayList[lfsrs.Length];
            for (int i = 0; i < lfsrs.Length; i++)
            {
                Xi[i] = lfsrs[i].Output();
            }
            return Xi;
        }
        public void Clock(int time)
        {
            for (int i = 0; i < lfsrs.Length; i++)
            {
                lfsrs[i].Clock(time);

            }
        }
        public void OneClock()
        {
            for (int i = 0; i < lfsrs.Length; i++)
            {
                lfsrs[i].OneClock();
            }
        }
        public ArrayList[][] Outputs(int run)
        {
            ArrayList[][] runoutput = new ArrayList[run][];
            for (int i = 0; i < run; i++)
            {
                ArrayList[] Xi = LFSRsOutput();
                runoutput[i] = Xi;
                OneClock();
            }
            return runoutput;
        }
        public ArrayList SortOutput(ArrayList[][] outputs)
        {
            ArrayList sortedoutput = new ArrayList();
            for (int i = 0; i < outputs.Length; i++)
            {
                int startindex = 1;
                for (int j = 0; j < outputs[i].Length; j++)
                {
                    ArrayList lsfroutput = outputs[i][j];
                    int lfsrlength = lfsrs[j].length;
                    foreach (bool[] cell in lsfroutput)
                    {
                        string cellstring = ConvertCellContent(cell, startindex);
                        sortedoutput.Add(cellstring);
                    }
                    startindex = startindex + lfsrlength;
                }
            }
            return sortedoutput;
        }
        public string ConvertCellContent(bool[] content, int index)
        {
            string s = "";
            for (int j = 0; j < content.Length; j++)
            {
                if (content[j])
                {
                    s = s + "k" + (j + index) + "+";
                }
            }
            s = s.Remove(s.Length - 1);
            if (s.Contains("+"))
            {
                s = "(" + s + ")";
            }

            return s;
        }
    }
}
