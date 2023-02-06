using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptool.Plugins.M209Analyzer
{
    internal class SimulatedAnnealing
    {
        private static double minRatio = Math.Log(0.0085);
        private static Random Randomizer = new Random();
        public static bool accept(double newScore, double currLocalScore, double temperatrue)
        {
            double diffScore = newScore - currLocalScore;
            if(diffScore > 0) { 
                return true; 
            }
            if (temperatrue == 0)
            {
                return false;
            }
            double ratio = diffScore / temperatrue;
            return ratio > minRatio && Math.Pow(Math.E, ratio) > Randomizer.NextDouble();
        }
    }
}
