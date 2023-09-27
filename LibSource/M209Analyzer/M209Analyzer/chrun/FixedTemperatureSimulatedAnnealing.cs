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
    public class FixedTemperatureSimulatedAnnealing
    {
        private Random random = new Random();

        // Fixed temperature optimized for hexagram scoring
        private static readonly double FIXED_TEMPERATURE = 20_000.0;

        /**
         * Simulated annealing acceptance function.
         *
         * @param newKeyScore - score for the ney key
         * @param currentKeyScore - score for the current key
         * @return true if new key should be accepted
         */
        bool accept(double newKeyScore, double currentKeyScore)
        {

            // Always accept better keys
            if (newKeyScore > currentKeyScore)
            {
                return true;
            }

            // Degradation between current key and new key.
            double degradation = currentKeyScore - newKeyScore;

            double acceptanceProbability =
                    Math.Pow(Math.E, -degradation / FIXED_TEMPERATURE);

            return random.NextDouble() < acceptanceProbability;
        }
    }
}
