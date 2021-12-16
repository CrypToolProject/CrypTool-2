using System;

namespace PlayfairAnalysis.Common
{
    public class SimulatedAnnealing
    {
        private readonly Utils utils;

        public SimulatedAnnealing(Utils utils)
        {
            this.utils = utils;
        }

        public bool acceptHexaScore(long newScore, long currLocalScore, int multiplier)
        {

            //return accept(newScore, currLocalScore, 275.0 * multiplier / 20.0);
            return accept(newScore, currLocalScore, 13.75 * multiplier);

        }

        private readonly double minRatio = Math.Log(0.0085);

        public bool accept(long newScore, long currLocalScore, double temperature)
        {

            long diffScore = newScore - currLocalScore;
            if (diffScore > 0)
            {
                return true;
            }
            if (temperature == 0.0)
            {
                return false;
            }
            double ratio = diffScore / temperature;
            return ratio > minRatio && Math.Pow(Math.E, ratio) > utils.randomNextDouble();

        }
    }

}
