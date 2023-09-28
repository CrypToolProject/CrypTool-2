/*
   Copyright 2023 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using System.Collections.Generic;

namespace CrypTool.Plugins.HomophonicSubstitutionAnalyzer
{
    public class NomenclatureElementFinder
    {
        private readonly Text _ciphertext;        
        private readonly Dictionary<int[], int> _frequencyDictionary = new Dictionary<int[], int>(new IntArrayEqualityComparer());

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
                if (!_frequencyDictionary.ContainsKey(symbol))
                {
                    _frequencyDictionary.Add(symbol, 1);
                    continue;
                }
                _frequencyDictionary[symbol]++;
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
                if(_frequencyDictionary[symbol] <= threshold)
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
