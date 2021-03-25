using System;
using System.Collections.Generic;
using System.Linq;
using CrypTool.PluginBase.Control;
using CrypTool.PluginBase.Miscellaneous;

namespace KeySearcher
{
    /// <summary>
    /// This class is used to optimize the execution of the OpenCL kernel by trying to converge against the optimal amount of concurrent OpenCL workitems.
    /// This is done by time meassuring.
    /// This optimization is only used by KeySearcher if "high load" is set.
    /// 
    /// The optimization is realised by means of subbatches.
    /// A subbatch is a part of the whole OpenCL batch. The size of the whole OpenCL batch was determined at code generation time.
    /// Subbatches are an instrument to be more flexibel with the amount of concurrent workitems.
    /// </summary>
    class KeySearcherOpenCLSubbatchOptimizer
    {
        private IKeyTranslator keyTranslator = null;
        private List<int> batchSizeFactors = null;
        private List<int> amountOfSubbatchesFactors = null;
        private int amountOfSubbatches;
        private DateTime begin;
        private TimeSpan lastDuration;
        private bool optimisticDecrease;
        private bool lastStepIncrease;
        private const long TOLERANCE = 500;
        private readonly int maxNumberOfThreads;
        private int openCLMode;

        public KeySearcherOpenCLSubbatchOptimizer(int openCLMode, int maxNumberOfThreads)
        {
            this.maxNumberOfThreads = maxNumberOfThreads;
            this.openCLMode = openCLMode;
        }

        /// <summary>
        /// This method returns the calculated amount of subbatches which is assumed to be the opimum.
        /// To determine all possible subbatches, the batchsize must be factorized into its primes.
        /// </summary>
        /// <param name="keyTranslator"></param>
        /// <returns></returns>
        public int GetAmountOfSubbatches(IKeyTranslator keyTranslator)
        {
            if (this.keyTranslator != keyTranslator)
            {
                //init code:
                this.keyTranslator = keyTranslator;

                //Find factors of OpenCL batch size:
                List<Msieve.Factor> factors = Msieve.TrivialFactorization(keyTranslator.GetOpenCLBatchSize());
                amountOfSubbatchesFactors = new List<int>();
                foreach (var fac in factors)
                {
                    for (int i = 0; i < fac.count; i++)
                        amountOfSubbatchesFactors.Add((int)fac.factor);
                }
                amountOfSubbatches = keyTranslator.GetOpenCLBatchSize();

                batchSizeFactors = new List<int>();
                DecreaseAmountOfSubbatches();

                lastDuration = TimeSpan.MaxValue;
                optimisticDecrease = false;
                lastStepIncrease = false;

                if (openCLMode == 1)    //normal load
                    DecreaseAmountOfSubbatches();
            }

            return amountOfSubbatches;
        }

        /// <summary>
        /// This method should be called immediately before the execution.
        /// </summary>
        public void BeginMeasurement()
        {
            begin = DateTime.Now;
        }

        /// <summary>
        /// This method sould be called immediately after the execution.
        /// It then calculates the new optimal subbatch size.
        /// </summary>
        public void EndMeasurement()
        {
            if (openCLMode != 2)
                return;

            var thisduration = DateTime.Now - begin;

            if (Math.Abs((thisduration - lastDuration).TotalMilliseconds) > TOLERANCE)
            {
                if (lastDuration > thisduration)
                {
                    DecreaseAmountOfSubbatches();
                    lastStepIncrease = false;
                }
                else
                {
                    if (!lastStepIncrease)
                    {
                        IncreaseAmountOfSubbatches();
                        lastStepIncrease = true;
                    }
                    else
                        lastStepIncrease = false;
                }
            }
            else
            {
                lastStepIncrease = false;
                if (optimisticDecrease)
                {
                    DecreaseAmountOfSubbatches();
                    optimisticDecrease = false;
                }
                else
                    optimisticDecrease = true;
            }

            lastDuration = thisduration;
        }

        private void DecreaseAmountOfSubbatches()
        {
            if (amountOfSubbatchesFactors.Count < 1)
                return;

            if (keyTranslator.GetOpenCLBatchSize() / amountOfSubbatches >= maxNumberOfThreads)   //not more than MAXNUMBEROFTHREADS threads concurrently
                return;

            do
            {
                int maxElement = amountOfSubbatchesFactors.Max();
                batchSizeFactors.Add(maxElement);
                amountOfSubbatchesFactors.Remove(maxElement);

                amountOfSubbatches = amountOfSubbatchesFactors.Aggregate(1, (current, i) => current * i);
            } while (amountOfSubbatchesFactors.Any() && keyTranslator.GetOpenCLBatchSize() / amountOfSubbatches < (256 * 256));        //each batch should have at least size 256*256
        }

        private void IncreaseAmountOfSubbatches()
        {
            if (batchSizeFactors.Count < 1)
                return;

            if (keyTranslator.GetOpenCLBatchSize() / amountOfSubbatches <= (256 * 256)) //each batch should have at least size 256*256
                return;

            do
            {
                int minElement = batchSizeFactors.Min();
                amountOfSubbatchesFactors.Add(minElement);
                batchSizeFactors.Remove(minElement);

                amountOfSubbatches = amountOfSubbatchesFactors.Aggregate(1, (current, i) => current * i);
            } while (keyTranslator.GetOpenCLBatchSize() / amountOfSubbatches > maxNumberOfThreads);        //not more than MAXNUMBEROFTHREADS threads concurrently
        }
    }
}
