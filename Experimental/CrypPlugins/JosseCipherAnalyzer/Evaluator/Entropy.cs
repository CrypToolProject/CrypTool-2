using System;
using System.Collections.Generic;
using System.Linq;

namespace CrypTool.JosseCipherAnalyzer.Evaluator
{
    public class Entropy : IEvaluator
    {
        public double Evaluate(string input)
        {
            Dictionary<char, double> table = new Dictionary<char, double>();


            foreach (char c in input)
            {
                if (table.ContainsKey(c))
                {
                    table[c]++;
                }
                else
                {
                    table.Add(c, 1);
                }
            }

            return table.Select(letter => letter.Value / input.Length).Select(freq => freq * LogTwo(freq)).Sum();
        }

        private static double LogTwo(double num)
        {
            return Math.Log(num) / Math.Log(2);
        }
    }
}