using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrypTool.PluginBase.IO;

namespace CrypTool.Plugins.HomophonicAnalyzer
{
    class Frequencies
    {
        #region Private Variables

        private double[] prob1gram;
        private double[][] prob2gram;
        private double[][][] prob3gram;
        private double[][][][] prob4gram;
        private double[][][][][] prob5gram;
        private double[][][][][][] prob6gram;
        private double[][][][][][][] prob7gram;

        private Alphabet alpha;
        private int size;
        private int ngram;

        private CalculateFitness calculateFitnessOfKey;

        #endregion

        #region Constructor

        public Frequencies(Alphabet alphabet)
        {
            this.alpha = alphabet;
            this.size = alphabet.Length;
        }

        #endregion

        #region Properties

        public int NGram
        {
            get { return this.ngram; }
            set { ; }
        }

        public double[] Prob1Gram
        {
            get { return this.prob1gram; }
            set { ; }
        }

        public double[][] Prob2Gram
        {
            get { return this.prob2gram; }
            set { ; }
        }

        public double[][][] Prob3Gram
        {
            get { return this.prob3gram; }
            set { ; }
        }

        public double[][][][] Prob4Gram
        {
            get { return this.prob4gram; }
            set { ; }
        }

        public double[][][][][] Prob5Gram
        {
            get { return this.prob5gram; }
            set { ; }
        }

        public double[][][][][][] Prob6Gram
        {
            get { return this.prob6gram; }
            set { ; }
        }

        public double[][][][][][][] Prob7Gram
        {
            get { return this.prob7gram; }
            set { ; }
        }

        public CalculateFitness CalculateFitnessOfKey
        {
            get { return this.calculateFitnessOfKey; }
            set { ; }
        }

        #endregion

        #region ReadFromNGramFile

        /// <summary>
        /// Generate n-gram frequencies of a text and store them 
        /// </summary>
        /// 
        public void ReadProbabilities1Gram(String filename)
        {
            ngram = 1;
            prob1gram = new double[size];

            using (var fileStream = new FileStream(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, filename), FileMode.Open, FileAccess.Read))
            {
                using (var reader = new BinaryReader(fileStream))
                {
                    for (int i = 0; i < this.prob1gram.Length; i++)
                    {
                        var bytes = reader.ReadBytes(8);
                        prob1gram[i] = BitConverter.ToDouble(bytes,0);
                    }
                }
            }
            
        }

        public void ReadProbabilities2Gram(String filename)
        {
            this.ngram = 2;
            this.prob2gram = new double[this.size][];

            for (int i = 0; i < this.size; i++)
            {
                this.prob2gram[i] = new double[this.size];
            }

            using (var fileStream = new FileStream(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, filename), FileMode.Open, FileAccess.Read))
            {
                using (var reader = new BinaryReader(fileStream))
                {
                    for (int i = 0; i < this.prob2gram.Length; i++)
                    {
                        for (int j = 0; j < this.prob2gram.Length; j++)
                        {
                            var bytes = reader.ReadBytes(8);
                            this.prob2gram[i][j] = BitConverter.ToDouble(bytes, 0); ;
                        }
                    }                   
                }
            }

        }

        public void ReadProbabilities3Gram(String filename)
        {
            this.ngram = 3;
            this.prob3gram = new double[this.size][][];

            for (int i = 0; i < this.size; i++)
            {
                this.prob3gram[i] = new double[this.size][];
                for (int j = 0; j < this.size; j++)
                {
                    this.prob3gram[i][j] = new double[this.size];
                }
            }

            using (var fileStream = new FileStream(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, filename), FileMode.Open, FileAccess.Read))
            {
                using (var reader = new BinaryReader(fileStream))
                {
                    for (int i = 0; i < this.prob3gram.Length; i++)
                    {
                        for (int j = 0; j < this.prob3gram.Length; j++)
                        {
                            for (int k = 0; k < this.prob3gram.Length; k++)
                            {
                                var bytes = reader.ReadBytes(8);
                                this.prob3gram[i][j][k] = BitConverter.ToDouble(bytes, 0);
                            }
                        }
                    }
                }
            }
        }

        public void ReadProbabilities4Gram(String filename)
        {
            this.ngram = 4;
            this.prob4gram = new double[this.size][][][];

            for (int i = 0; i < this.size; i++)
            {
                this.prob4gram[i] = new double[this.size][][];
                for (int j = 0; j < this.size; j++)
                {
                    this.prob4gram[i][j] = new double[this.size][];
                    for (int k = 0; k < this.size; k++)
                    {
                        this.prob4gram[i][j][k] = new double[this.size];
                    }
                }
            }
            using (var fileStream = new FileStream(Path.Combine(DirectoryHelper.DirectoryLanguageStatistics, filename), FileMode.Open, FileAccess.Read))
            {
                using (var reader = new BinaryReader(fileStream))
                {
                    for (int i = 0; i < this.prob4gram.Length; i++)
                    {
                        for (int j = 0; j < this.prob4gram.Length; j++)
                        {
                            for (int k = 0; k < this.prob4gram.Length; k++)
                            {
                                for (int l = 0; l < this.prob4gram.Length; l++)
                                {
                                    var bytes = reader.ReadBytes(8);
                                    this.prob4gram[i][j][k][l] = BitConverter.ToDouble(bytes, 0);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ReadProbabilities5Gram(String filename)
        {
            this.ngram = 5;
            this.prob5gram = new double[this.size][][][][];
            
            for (int i = 0; i < this.size; i++)
            {
                this.prob5gram[i] = new double[this.size][][][];
                for (int j = 0; j < this.size; j++)
                {
                    this.prob5gram[i][j] = new double[this.size][][];
                    for (int k = 0; k < this.size; k++)
                    {
                        this.prob5gram[i][j][k] = new double[this.size][];
                        for (int l = 0; l < this.size; l++)
                        {
                            this.prob5gram[i][j][k][l] = new double[this.size];
                        }
                    }
                }
            }
           
            using (var fileStream = new FileStream(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, filename), FileMode.Open))
            {
                using (var reader = new BinaryReader(fileStream))
                {
                    for (int i = 0; i < this.prob5gram.Length; i++)
                    {
                        for (int j = 0; j < this.prob5gram.Length; j++)
                        {
                            for (int k = 0; k < this.prob5gram.Length; k++)
                            {
                                for (int l = 0; l < this.prob5gram.Length; l++)
                                {
                                    for (int m = 0; m < this.prob5gram.Length; m++)
                                    {
                                        var bytes = reader.ReadBytes(8);
                                        this.prob5gram[i][j][k][l][m] = BitConverter.ToDouble(bytes, 0);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ReadProbabilities6Gram(String filename)
        {
            this.ngram = 6;
            this.prob6gram = new double[this.size][][][][][];

            for (int i = 0; i < this.size; i++)
            {
                this.prob6gram[i] = new double[this.size][][][][];
                for (int j = 0; j < this.size; j++)
                {
                    this.prob6gram[i][j] = new double[this.size][][][];
                    for (int k = 0; k < this.size; k++)
                    {
                        this.prob6gram[i][j][k] = new double[this.size][][];
                        for (int l = 0; l < this.size; l++)
                        {
                            this.prob6gram[i][j][k][l] = new double[this.size][];
                            for (int m = 0; m < this.size; m++)
                            {
                                this.prob6gram[i][j][k][l][m] = new double[this.size];
                            }
                        }
                    }
                }
            }

            using (var fileStream = new FileStream(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, filename), FileMode.Open, FileAccess.Read))
            {
                using (var reader = new BinaryReader(fileStream))
                {
                    for (int i = 0; i < this.prob6gram.Length; i++)
                    {
                        for (int j = 0; j < this.prob6gram.Length; j++)
                        {
                            for (int k = 0; k < this.prob6gram.Length; k++)
                            {
                                for (int l = 0; l < this.prob6gram.Length; l++)
                                {
                                    for (int m = 0; m < this.prob6gram.Length; m++)
                                    {
                                        for (int n = 0; n < this.prob6gram.Length; n++)
                                        {
                                            var bytes = reader.ReadBytes(8);
                                            this.prob6gram[i][j][k][l][m][n] = BitConverter.ToDouble(bytes, 0);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ReadProbabilities7Gram(String filename)
        {
            this.ngram = 7;
            this.prob7gram = new double[this.size][][][][][][];

            for (int i = 0; i < this.size; i++)
            {
                this.prob7gram[i] = new double[this.size][][][][][];
                for (int j = 0; j < this.size; j++)
                {
                    this.prob7gram[i][j] = new double[this.size][][][][];
                    for (int k = 0; k < this.size; k++)
                    {
                        this.prob7gram[i][j][k] = new double[this.size][][][];
                        for (int l = 0; l < this.size; l++)
                        {
                            this.prob7gram[i][j][k][l] = new double[this.size][][];
                            for (int m = 0; m < this.size; m++)
                            {
                                this.prob7gram[i][j][k][l][m] = new double[this.size][];
                                for (int n = 0; n < this.size; n++)
                                {
                                    this.prob7gram[i][j][k][l][m][n] = new double[this.size];
                                }
                            }
                        }
                    }
                }
            }
            using (var fileStream = new FileStream(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, filename), FileMode.Open, FileAccess.Read))
            {
                using (var reader = new BinaryReader(fileStream))
                {
                    for (int i = 0; i < this.prob7gram.Length; i++)
                    {
                        for (int j = 0; j < this.prob7gram.Length; j++)
                        {
                            for (int k = 0; k < this.prob7gram.Length; k++)
                            {
                                for (int l = 0; l < this.prob7gram.Length; l++)
                                {
                                    for (int m = 0; m < this.prob7gram.Length; m++)
                                    {
                                        for (int n = 0; n < this.prob7gram.Length; n++)
                                        {
                                            for (int o = 0; o < this.prob7gram.Length; o++)
                                            {
                                                var bytes = reader.ReadBytes(8);
                                                this.prob7gram[i][j][k][l][m][n][o] = BitConverter.ToDouble(bytes, 0);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
         
        }

        public void ReadProbabilitiesFromNGramFile(String filename)
        {
            int ngram;
            
            Int32.TryParse(filename.Substring(3, 1), out ngram);

            switch (ngram)
            {
                case 1:
                    ReadProbabilities1Gram(filename);
                    this.calculateFitnessOfKey = this.CalculateFitness1gram;
                    break;
                case 2:
                    ReadProbabilities2Gram(filename);
                    this.calculateFitnessOfKey = this.CalculateFitness2gram;
                    break;
                case 3:
                    ReadProbabilities3Gram(filename);
                    this.calculateFitnessOfKey = this.CalculateFitness3gram;
                    break;
                case 4:
                    ReadProbabilities4Gram(filename);
                    this.calculateFitnessOfKey = this.CalculateFitness4gram;
                    break;
                case 5:
                    ReadProbabilities5Gram(filename);
                    this.calculateFitnessOfKey = this.CalculateFitness5gram;
                    break;
                case 6:
                    ReadProbabilities6Gram(filename);
                    this.calculateFitnessOfKey = this.CalculateFitness6gram;
                    break;
                case 7:
                    ReadProbabilities7Gram(filename);
                    this.calculateFitnessOfKey = this.CalculateFitness7gram;
                    break;
            }
        }

        private double CalculateFitness1gram(Text plaintext)
        {
            double res = 0;
            double[] prob = this.Prob1Gram;
            int[] text = plaintext.ValidLetterArray;

            for (int pos0 = 0; pos0 < text.Length; pos0++)
            {
                res += prob[text[pos0]];
            }
            return res;
        }

        private double CalculateFitness2gram(Text plaintext)
        {
            double res = 0;
            double[][] prob = this.Prob2Gram;
            int[] text = plaintext.ValidLetterArray;

            for (int pos0 = 0; pos0 < text.Length - 1; pos0++)
            {
                res += prob[text[pos0]][text[pos0 + 1]];
            }
            return res;
        }

        private double CalculateFitness3gram(Text plaintext)
        {
            double res = 0;
            double[][][] prob = this.Prob3Gram;
            int[] text = plaintext.ValidLetterArray;

            if (text.Length == 1)
            {
                for (int i = 0; i < this.alpha.Length; i++)
                {
                    for (int j = 0; j < this.alpha.Length; j++)
                    {
                        res += prob[text[0]][i][j];
                    }
                }
            }
            else if (text.Length == 2)
            {
                for (int i=0; i < this.alpha.Length; i++)
                {
                    res += prob[text[0]][text[1]][i];
                }
            }
            else
            {
                for (int pos0 = 0; pos0 < text.Length - 2; pos0++)
                {
                    res += prob[text[pos0]][text[pos0 + 1]][text[pos0 + 2]];
                }
            }
            return res;
        }

        private double CalculateFitness4gram(Text plaintext)
        {
            double res = 0;
            double[][][][] prob = this.Prob4Gram;
            int[] text = plaintext.ValidLetterArray;

            if (text.Length == 1)
            {
                for (int i = 0; i < this.alpha.Length; i++)
                {
                    for (int j = 0; j < this.alpha.Length; j++)
                    {
                        for (int t=0; t < this.alpha.Length; t++)
                        {
                            res += prob[text[0]][i][j][t];
                        }
                    }
                }
            }
            else if (text.Length == 2)
            {
                for (int i = 0; i < this.alpha.Length; i++)
                {
                    for (int j = 0; j < this.alpha.Length; j++)
                    {
                        res += prob[text[0]][text[1]][i][j];
                    }
                }
            }
            else if (text.Length == 3)
            {
                for (int i = 0; i < this.alpha.Length; i++)
                {
                    res += prob[text[0]][text[1]][text[2]][i];
                }
            }
            else
            {
                for (int pos0 = 0; pos0 < text.Length - 3; pos0++)
                {
                    res += prob[text[pos0]][text[pos0 + 1]][text[pos0 + 2]][text[pos0 + 3]];
                }
            }
            return res;
        }

        private double CalculateFitness5gram(Text plaintext)
        {
            double res = 0;
            double[][][][][] prob = this.Prob5Gram;
            int[] text = plaintext.ValidLetterArray;

            for (int pos0 = 0; pos0 < text.Length - 4; pos0++)
            {
                res += prob[text[pos0]][text[pos0 + 1]][text[pos0 + 2]][text[pos0 + 3]][text[pos0 + 4]];
            }
            return res;
        }

        private double CalculateFitness6gram(Text plaintext)
        {
            double res = 0;
            double[][][][][][] prob = this.Prob6Gram;
            int[] text = plaintext.ValidLetterArray;

            for (int pos0 = 0; pos0 < text.Length - 5; pos0++)
            {
                res += prob[text[pos0]][text[pos0 + 1]][text[pos0 + 2]][text[pos0 + 3]][text[pos0 + 4]][text[pos0 + 5]];
            }
            return res;
        }

        private double CalculateFitness7gram(Text plaintext)
        {
            double res = 0;
            double[][][][][][][] prob = this.Prob7Gram;
            int[] text = plaintext.ValidLetterArray;

            for (int pos0 = 0; pos0 < text.Length - 6; pos0++)
            {
                res += prob[text[pos0]][text[pos0 + 1]][text[pos0 + 2]][text[pos0 + 3]][text[pos0 + 4]][text[pos0 + 5]][text[pos0 + 5]];
            }
            return res;
        }

        #endregion

        #region CreateProbabilitiesFromReferenceText
        
        public void CreateProbabilitiesFromReferenceText(Text text)
        {/*
            this.ngram = 3;

            int ratio3gram = 0;
            double[][][] freq3gram;

            for (int i = 0; i < freq3gram.Length; i++)
            {
                for (int j = 0; j < freq3gram.Length; j++)
                {
                    for (int k = 0; k < freq3gram.Length; k++)
                    {
                        freq3gram[i][j][k] = 0;
                    }
                }
            }

            // Extract frequencies
            int pos1, pos2, pos3, pos4;
            for (int pos0 = 0; pos0 < text.Length; pos0++)
            {
                while ((pos0 < text.Length) && (text.GetLetterAt(pos0) < 0))
                {
                    pos0++;
                }
                if (pos0 >= text.Length)
                {
                    break;
                }
                pos1 = pos0 + 1;
                while ((pos1 < text.Length) && (text.GetLetterAt(pos1) < 0))
                {
                    pos1++;
                }
                if (pos1 >= text.Length)
                {
                    continue;
                }

                pos2 = pos1 + 1;
                while ((pos2 < text.Length) && (text.GetLetterAt(pos2) < 0))
                {
                    pos2++;
                }
                if (pos2 >= text.Length)
                {
                    continue;
                }
                pos3 = pos2 + 1;
                while ((pos3 < text.Length) && (text.GetLetterAt(pos3) < 0))
                {
                    pos3++;
                }
                if (pos3 >= text.Length)
                {
                    continue;
                }
                pos4 = pos3 + 1;
                while ((pos4 < text.Length) && (text.GetLetterAt(pos4) < 0))
                {
                    pos4++;
                }
                if (pos4 >= text.Length)
                {
                    continue;
                }
                this.freq5gram[text.GetLetterAt(pos0)][text.GetLetterAt(pos1)][text.GetLetterAt(pos2)][text.GetLetterAt(pos3)][text.GetLetterAt(pos4)]++;
                this.ratio5gram++;
            }

            // Generate probabilities with Simple-Good-Turing algorithm
            Dictionary<int, int> table_rn = new Dictionary<int, int>();
            for (int i = 0; i < this.freq5gram.Length; i++)
            {
                for (int j = 0; j < this.freq5gram.Length; j++)
                {
                    for (int k = 0; k < this.freq5gram.Length; k++)
                    {
                        for (int l = 0; l < this.freq5gram.Length; l++)
                        {
                            for (int m = 0; m < this.freq5gram.Length; m++)
                            {
                                if (table_rn.ContainsKey(this.freq5gram[i][j][k][l][m]))
                                {
                                    table_rn[this.freq5gram[i][j][k][l][m]]++;
                                }
                                else
                                {
                                    table_rn.Add(this.freq5gram[i][j][k][l][m],1);
                                }
                            }
                        }
                    }
                }
            }
            int unseen = table_rn[0];
            table_rn.Remove(0);

            int N = this.ratio5gram;
            double N_1, a, b;
            
            int[] t_r = new int[table_rn.Count];
            int[] t_n = new int[table_rn.Count];
            double[] t_z = new double[table_rn.Count];
            double[] t_logr = new double[table_rn.Count];
            double[] t_logz = new double[table_rn.Count];
            double[] t_rstar = new double[table_rn.Count];
            double[] t_p = new double[table_rn.Count];

            // fill r and n

            List<int> keylist = table_rn.Keys.ToList<int>();
            keylist.Sort();
            for (int i = 0; i < keylist.Count; i++)
            {
                t_r[i] = keylist[i];
                t_n[i] = table_rn[keylist[i]];
            }

            double P0 = (double)t_n[getIndexOfField(t_r,1)]/N;
            // fill Z
            int var_i, var_k = 0;
            for (int index_j=0;index_j<t_r.Length;index_j++){
                if (index_j==0){
                    var_i = 0;
                    var_k = t_r[index_j + 1];
                } else if (index_j == t_r.Length - 1){
                    var_i = t_r[index_j - 1];
                    var_k = 2 * t_r[index_j] - var_i;
                } else {
                    var_i = t_r[index_j - 1];
                    var_k = t_r[index_j + 1];
                }
                t_z[index_j] = ((double)2*t_n[index_j])/(var_k - var_i);
            }
            // fill logr and logz
            for (int j=0;j<t_logr.Length;j++)
            {
                t_logr[j] = Math.Log(t_r[j]);
                t_logz[j] = Math.Log(t_z[j]);
            }
            // find a and b
            LinReg(t_logr,t_logz, out a, out b);

            // fill r*
            bool useY = false;
            for (int i = 0; i < t_r.Length; i++)
            {
                double y = ((double) (t_r[i] + 1)) * (Math.Exp(a* Math.Log(t_r[i] + 1) + b) / Math.Exp(a* Math.Log(t_r[i]) + b));
                
                // if r+1 not in t_r
                if (useY)
                {
                    t_rstar[i] = y;
                }
                else
                {
                    double x = (t_r[i] + 1) * ((double) t_n[getIndexOfField(t_r, (t_r[i] + 1))] / t_n[getIndexOfField(t_r, t_r[i])]);

                    double lside = Math.Abs(x - y);

                    double n_r1 = t_n[getIndexOfField(t_r, (t_r[i] + 1))];
                    double n_r = t_n[getIndexOfField(t_r, t_r[i])];
                    double rside = 1.96 * Math.Sqrt((t_r[i] + 1) * (t_r[i] + 1) * ((double)n_r1/(n_r*n_r)) * (1 + ((double)n_r1/n_r))); 
                            
                    if (lside > rside && !useY)
                    {
                        t_rstar[i] = x;
                    }
                    else
                    {
                        t_rstar[i] = y;
                        useY = true;
                    }
                }  
            }

            N_1 = 0;
            
            for (int i=0;i<t_rstar.Length;i++)
            {
                N_1 += t_n[i]*t_rstar[i];
            }
            for (int i = 0; i < t_p.Length; i++)
            {
                t_p[i] = (1 - P0) * (t_rstar[i] / N_1);
            }

            // fill prob array
            double prob_unseen = P0/unseen;
            for (int i = 0; i < this.freq5gram.Length; i++)
            {
                for (int j = 0; j < this.freq5gram.Length; j++)
                {
                    for (int k = 0; k < this.freq5gram.Length; k++)
                    {
                        for (int l = 0; l < this.freq5gram.Length; l++)
                        {
                            for (int m = 0; m < this.freq5gram.Length; m++)
                            {
                                if (this.freq5gram[i][j][k][l][m] == 0)
                                {
                                    this.prob5gram[i][j][k][l][m] = Math.Log(prob_unseen);
                                }
                                else
                                {
                                    int index = getIndexOfField(t_r, this.freq5gram[i][j][k][l][m]);
                                    this.prob5gram[i][j][k][l][m] = Math.Log(t_p[index]);
                                }
                            }
                        }
                    }
                }
            }
      */  }
        
        #endregion

        #region Helper Functions

        /// <summary>
        /// Linear regression
        /// </summary>
        public void LinReg(double[] xValues, double[] yValues, out double a, out double b)
        {
            double sumX = 0;
            double sumY = 0;
            double sumXX = 0;
            double sumYY = 0;
            double ssX = 0;
            double ssY = 0;
            double sumCodeviates = 0;
            double sCo = 0;
            double count = xValues.Length;

            for (int i = 0; i < xValues.Length; i++)
            {
                sumCodeviates += xValues[i] * yValues[i];
                sumX += xValues[i];
                sumY += yValues[i];
                sumXX += xValues[i] * xValues[i];
                sumYY += yValues[i] * yValues[i];
            }

            ssX = sumXX - ((sumX * sumX) / count);
            ssY = sumYY - ((sumY * sumY) / count);

            sCo = sumCodeviates - ((sumX * sumY) / count);

            a = sCo / ssX;
            b = (sumY / count) - ((sCo / ssX) * (sumX / count));
        }

        private int getIndexOfField(int[] ar, int value)
        {
            for (int i = 0; i < ar.Length; i++)
            {
                if (ar[i] == value)
                {
                    return i;
                }
            }
            return -1;
        }
        /// <summary>
        /// Get position of letters in frequency array
        /// </summary>
        private int GetFreqArrayPos(params string[] lets)
        {
            int res = 0;
            int fac = 0;

            res += this.alpha.GetPositionOfLetter(lets[4]);
            if (lets[3]!=null)
            {
                fac = this.alpha.GetPositionOfLetter(lets[3]); 
                if (fac == 0)
                {
                    fac = 1;
                }
                res += this.Pow(this.alpha.Length, (1)) * (fac);
            }
            if (lets[2]!=null)
            {
                fac = this.alpha.GetPositionOfLetter(lets[2]);
                if (fac == 0)
                {
                    fac = 1;
                }
                res += this.Pow(this.alpha.Length, (2)) * (fac);
            }
            if (lets[1]!=null)
            {
                fac = this.alpha.GetPositionOfLetter(lets[1]);
                if (fac == 0)
                {
                    fac = 1;
                }
                res += this.Pow(this.alpha.Length, (3)) * (fac);
            }
            if (lets[0]!=null)
            {
                fac = this.alpha.GetPositionOfLetter(lets[0]);
                if (fac == 0)
                {
                    fac = 1;
                }
                res += this.Pow(this.alpha.Length, (4)) * (fac);
            }
            
            return res;
        }

        /// <summary>
        /// Power
        /// </summary>
        private int Pow(int x, int e)
        {
            int res = x;

            if (e == 0)
            {
                return 1;
            }
            for (int i = 0; i < e-1; i++)
            {
                res *= x;
            }
            return res;
        }
        
        #endregion
    }
}