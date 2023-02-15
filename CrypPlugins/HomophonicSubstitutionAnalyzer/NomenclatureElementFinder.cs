using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrypTool.Plugins.HomophonicSubstitutionAnalyzer
{
    public class NomenclatureElementFinder
    {
        private readonly Text _ciphertext;        
        private Dictionary<int[], int> frequencyDictionary = new Dictionary<int[], int>(new IntArrayEqualityComparer());

        public NomenclatureElementFinder(Text ciphertext)
        {
            _ciphertext = ciphertext;
            GenerateFrequencyStatistic();
        }

        /// <summary>
        /// Generates a frequency statistic of all ciphertext symbols
        /// </summary>
        private void GenerateFrequencyStatistic()
        {
            for (int offset = 0; offset < _ciphertext.GetSymbolsCount(); offset++)
            {
                int[] symbol = _ciphertext[offset];
                if (!frequencyDictionary.ContainsKey(symbol))
                {
                    frequencyDictionary.Add(symbol, 1);
                    continue;
                }
                frequencyDictionary[symbol]++;
            }
        }

        /// <summary>
        /// Finds all homophones that are less frequent than set threshold
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, int> FindNomenclatureElements(int threshold)
        {
            Dictionary<int, int> positions = new Dictionary<int, int>();

            for (int offset = 0; offset < _ciphertext.GetSymbolsCount(); offset++)
            {
                int[] symbol = _ciphertext[offset];
                if(frequencyDictionary[symbol] <= threshold)
                {
                    positions.Add(offset, 1);
                }  
            }
            return positions;
        }

        /// <summary>
        /// Code snipplet taken from: https://stackoverflow.com/questions/14663168/an-integer-array-as-a-key-for-dictionary
        /// </summary>
        public class IntArrayEqualityComparer : IEqualityComparer<int[]>
        {
            public bool Equals(int[] array1, int[] array2)
            {
                if (array1.Length != array2.Length)
                {
                    return false;
                }
                for (int i = 0; i < array1.Length; i++)
                {
                    if (array1[i] != array2[i])
                    {
                        return false;
                    }
                }
                return true;
            }

            public int GetHashCode(int[] obj)
            {
                int result = 17;
                for (int i = 0; i < obj.Length; i++)
                {
                    unchecked
                    {
                        result = result * 23 + obj[i];
                    }
                }
                return result;
            }
        }
    }
}
