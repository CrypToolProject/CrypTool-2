using CrypTool.PluginBase.Control;
using KeySearcher.CrypCloud;
using KeySearcher.KeyPattern;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using VoluntLib2.ComputationLayer;

namespace KeySearcher
{
    /// <summary>
    /// Worker that performs keysearching
    /// </summary>
    internal class Worker : AWorker
    {
        private const double Epsilon = 0.01d;

        private readonly JobDataContainer jobData;
        private readonly KeyPatternPool keyPool;
        private readonly RelationOperator relationOperator;

        public Worker(JobDataContainer jobData, KeyPatternPool keyPool, KeySearcher keysearcher)
        {
            this.jobData = jobData;
            this.keyPool = keyPool;

            relationOperator = jobData.CostAlgorithm.GetRelationOperator();           
        }       

        public override CalculationResult DoWork(byte[] jobPayload, BigInteger blockId, CancellationToken cancelToken)
        {
            IKeyTranslator keySet = GetKeySetForBlock(blockId);
            IEnumerable<KeyResultEntry> bestKeys = null;

            bestKeys = FindBestKeysInBlock(keySet, cancelToken, blockId);
            return CreateCalculationResult(blockId, bestKeys);
        }


        private IKeyTranslator GetKeySetForBlock(BigInteger blockId)
        {
            KeyPattern.KeyPattern keysForBlock = keyPool[blockId];
            IKeyTranslator keyTranslator = jobData.CryptoAlgorithm.GetKeyTranslator();
            keyTranslator.SetKeys(keysForBlock);
            return keyTranslator;
        }

        #region find best keys in block

        private IEnumerable<KeyResultEntry> FindBestKeysInBlock(IKeyTranslator keyTranslator, CancellationToken cancelToken, BigInteger blockId)
        {
            IControlEncryption controlEncryption = jobData.CryptoAlgorithm;
            IControlCost costAlgorithm = jobData.CostAlgorithm;
            byte[] ciphertext = jobData.Ciphertext;
            byte[] initVector = jobData.InitVector;
            int bytesToUse = jobData.BytesToUse;

            List<KeyResultEntry> top10Keys = InitTop10Keys();
            int index = 0;
            while (keyTranslator.NextKey())
            {
                byte[] key = keyTranslator.GetKey();
                byte[] decryption = controlEncryption.Decrypt(ciphertext, key, initVector, bytesToUse);
                double costs = costAlgorithm.CalculateCost(decryption);

                if (IsTopKey(costs, top10Keys[9].Costs))
                {
                    top10Keys = CreateNewTopList(top10Keys, costs, decryption, key);
                }

                index++;
                if (index % 100000 == 0)
                {
                    cancelToken.ThrowIfCancellationRequested();
                    if (index % 500000 == 0)
                    {
                        OnProgressChanged(blockId, 500000);
                    }
                }
            }

            OnProgressChanged(blockId, index % 1500000);
            return top10Keys;
        }
     
        /// <summary>
        /// Toplist is filled with 10 entrys that will be overriden by every key
        /// This reduces the need of a couple of if-operations an so increases performance
        /// </summary>
        /// <returns></returns>
        private List<KeyResultEntry> InitTop10Keys()
        {
            List<KeyResultEntry> top10Keys = new List<KeyResultEntry>(11);
            double defaultCosts = relationOperator == RelationOperator.LargerThen ? double.MinValue : double.MaxValue;
            for (int i = 0; i < 10; i++)
            {
                top10Keys.Add(new KeyResultEntry { Costs = defaultCosts, Decryption = new byte[1], KeyBytes = new byte[1] });
            }
            return top10Keys;
        }

        private bool IsTopKey(double fst, double snd)
        {
            if (relationOperator == RelationOperator.LargerThen)
            {
                return fst - snd > Epsilon;
            }
            return fst - snd < Epsilon;
        }

        private List<KeyResultEntry> CreateNewTopList(List<KeyResultEntry> top10Keys, double costs,
            byte[] decryption, byte[] key)
        {
            byte[] copyOfKey = new byte[key.Length];
            key.CopyTo(copyOfKey, 0);
            KeyResultEntry item = new KeyResultEntry { Costs = costs, KeyBytes = copyOfKey, Decryption = decryption };
            top10Keys.Add(item);

            IOrderedEnumerable<KeyResultEntry> orderByDescending = (relationOperator == RelationOperator.LessThen)
                ? top10Keys.OrderBy(it => it)
                : top10Keys.OrderByDescending(it => it);

            return orderByDescending.Take(10).ToList();
        }

        private static CalculationResult CreateCalculationResult(BigInteger blockId,
            IEnumerable<KeyResultEntry> bestKeys)
        {
            return new CalculationResult
            {
                BlockID = blockId,
                LocalResults = bestKeys.Select(entry => entry.Serialize()).ToList()
            };
        }

        #endregion
    }
}