/*                              
   Copyright 2022 CrypToolTeam

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
using System.Collections;

namespace TranspositionAnalyser
{
    public class ValueKey : IComparable, ICloneable
    {
        public string key;
        public string word;
        public double score;
        public string plaintext;
        public string mode;

        public int CompareTo(object obj)
        {
            ValueKey v = (ValueKey)obj;

            int result;

            if ((result = score.CompareTo(v.score)) == 0)
            {
                if ((result = key.Length.CompareTo(v.key.Length)) == 0)
                {
                    for (int i = 0; i < key.Length; i++)
                    {
                        if ((result = key[i].CompareTo(v.key[i])) != 0)
                        {
                            break;
                        }
                    }
                }
            }

            return result;
        }

        public object Clone()
        {
            ValueKey v = new ValueKey
            {
                score = score,
                word = word,
                key = (string)key.Clone(),
                plaintext = (string)plaintext.Clone(),
                mode = mode
            };

            return v;
        }
    }

    public class ValueKeyComparer : IComparer
    {
        private readonly bool sortDescending;

        public ValueKeyComparer(bool sortDescending = true)
        {
            this.sortDescending = sortDescending;
        }

        public int Compare(object x, object y)
        {
            int result = ((ValueKey)x).CompareTo((ValueKey)y);
            return sortDescending ? -result : result;
        }
    }
}
