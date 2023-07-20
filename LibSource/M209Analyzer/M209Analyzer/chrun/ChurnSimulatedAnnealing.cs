using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M209AnalyzerLib.Chrun
{
    public class ChurnSimulatedAnnealing
    {


        private Random random = new Random();

        // Fixed temperature optimized for hexagram scoring
        private static readonly double FIXED_TEMPERATURE = 20_000.0;

        // Size of degradation threshold lookup table.
        private static readonly int LOOKUP_TABLE_SIZE = 100;

        // The churn algorithm lookup table of degradation thresholds.
        private readonly double[] degradationLookupTable = new double[LOOKUP_TABLE_SIZE];

        // Compute the churn algorithm lookup table of degradation thresholds.
        void computeDegradationLookupTable()
        {
            for (int index = 0; index < LOOKUP_TABLE_SIZE; index++)
                degradationLookupTable[index] =
                        FIXED_TEMPERATURE * Math.Log(LOOKUP_TABLE_SIZE / (index + 1));
        }

        /**
         * Simulated Annealing acceptance function - Churn implementation.
         *
         * @param newKeyScore - score for the ney key
         * @param currentKeyScore - score for the current key
         * @return true if new key should be accepted.
         */
        bool accept(double newKeyScore, double currentKeyScore)
        {

            // Always accept better keys
            if (newKeyScore > currentKeyScore) return true;

            // Fetch a random degradation threshold from the lookup table.
            int randomIndex = random.Next(LOOKUP_TABLE_SIZE);
            double degradationRandomThreshold = degradationLookupTable[randomIndex];

            // Degradation between current key and new key.
            double degradation = currentKeyScore - newKeyScore;

            return degradation < degradationRandomThreshold;
        }
    }
}
