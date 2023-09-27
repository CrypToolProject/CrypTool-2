/*
   Copyright CrypTool 2 Team josef.matwich@gmail.com

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;

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
