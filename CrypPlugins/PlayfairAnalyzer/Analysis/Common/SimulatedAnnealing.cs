/*
   Copyright 2022 George Lasry, Nils Kopal, CrypTool 2 Team

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

namespace PlayfairAnalysis.Common
{
    public class SimulatedAnnealing
    {
        private readonly Utils utils;

        public SimulatedAnnealing(Utils utils)
        {
            this.utils = utils;
        }

        public bool AcceptHexaScore(long newScore, long currLocalScore, int multiplier)
        {
            return Accept(newScore, currLocalScore, 13.75 * multiplier);

        }

        private readonly double minRatio = Math.Log(0.0085);

        public bool Accept(long newScore, long currLocalScore, double temperature)
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
            return ratio > minRatio && Math.Pow(Math.E, ratio) > utils.RandomNextDouble();
        }
    }
}
