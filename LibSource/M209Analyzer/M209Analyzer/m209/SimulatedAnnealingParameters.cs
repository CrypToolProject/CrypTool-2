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
