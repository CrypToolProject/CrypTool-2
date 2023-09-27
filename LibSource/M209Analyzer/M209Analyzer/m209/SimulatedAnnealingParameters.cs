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

namespace M209AnalyzerLib.M209
{
    /// <summary>
    /// Parameters for the Simulated Annealing
    /// </summary>
    public class SimulatedAnnealingParameters
    {
        /// <summary>
        /// Default value: Math.Log(0.0085)
        /// </summary>
        public double MinRatio { get; set; } = Math.Log(0.0085);

        /// <summary>
        /// Default value: 1000.
        /// </summary>
        public double StartTemperature { get; set; } = 1000.0;

        /// <summary>
        /// Default value: 1.0
        /// </summary>
        public double EndTemperature { get; set; } = 1.0;

        /// <summary>
        /// Default value: 1.1
        /// </summary>
        public double Decrement { get; set; } = 1.1;
    }
}
