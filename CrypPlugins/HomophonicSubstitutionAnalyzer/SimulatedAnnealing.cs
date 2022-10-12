/*
   Copyright 2020 Nils Kopal <Nils.Kopal<at>CrypTool.org
   Simulated Annealing algorithms by George Lasry

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

namespace CrypTool.Plugins.HomophonicSubstitutionAnalyzer
{
    public class SimulatedAnnealing
    {
        private readonly Random _random = new Random();
        private readonly double _startTemperature = 0;
        private double _currentTemperature = 0;
        private readonly double _steps;
        private readonly double _stepSize;

        public SimulatedAnnealing(double temperature, double steps)
        {
            _currentTemperature = temperature;
            _startTemperature = temperature;
            _steps = steps;
            _stepSize = _startTemperature / _steps;
        }

        /// <summary>
        /// Simulated Annealing Acceptance Function
        /// </summary>
        /// <param name="newKeyScore"></param>
        /// <param name="currentKeyScore"></param>
        /// <returns></returns>
        public bool AcceptWithTemperature(double newKeyScore, double currentKeyScore)
        {
            _currentTemperature = _currentTemperature - _stepSize;            
            // Always accept better keys
            if (newKeyScore >= currentKeyScore)
            {
                return true;
            }
            double degradation = -Math.Abs(currentKeyScore - newKeyScore);
            double acceptanceProbability = Math.Exp(degradation / _currentTemperature);
            return acceptanceProbability > 0.0085 && _random.NextDouble() < acceptanceProbability;
        }

        /// <summary>
        /// Returns true as long as we did not reach the end Temperature 
        /// </summary>
        /// <returns></returns>
        public bool IsHot()
        {
            return _currentTemperature > 0;
        }

        /// <summary>
        /// Returns the current progress of the simulated annealing process
        /// </summary>
        /// <returns></returns>
        public double GetProgress()
        {
            return (_startTemperature - _currentTemperature) / _startTemperature;
        }
    }
}
