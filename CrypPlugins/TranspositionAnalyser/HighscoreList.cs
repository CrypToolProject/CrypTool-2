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
using System.Collections.Generic;

namespace TranspositionAnalyser
{
    public class HighscoreList : List<ValueKey>
    {
        private readonly ValueKeyComparer comparer;

        public HighscoreList(ValueKeyComparer comparer, int Capacity)
        {
            this.comparer = comparer;
            this.Capacity = Capacity;
        }

        public bool isBetter(ValueKey v)
        {
            if (Count == 0)
            {
                return true;
            }

            return comparer.Compare(v, this[0]) > 0;
        }

        public bool isPresent(ValueKey v, out int i)
        {
            if (Count == 0 || comparer.Compare(v, this[Count - 1]) < 0) { i = Count; return false; }

            for (i = 0; i < Count; i++)
            {
                int cmp = comparer.Compare(v, this[i]);
                if (cmp > 0)
                {
                    return false;
                }

                if (cmp == 0)
                {
                    return true;
                }
            }

            return false;
        }

        public new bool Add(ValueKey v)
        {
            if (isPresent(v, out int i))
            {
                return false;
            }

            return Add(v, i);
        }

        public bool Add(ValueKey v, int i)
        {
            if (i >= Capacity)
            {
                return false;
            }

            if (Count >= Capacity)
            {
                RemoveAt(Capacity - 1);
            }

            Insert(i, (ValueKey)v.Clone());
            return true;
        }

        public void Merge(HighscoreList list)
        {
            foreach (ValueKey v in list)
            {
                if (!Add(v))
                {
                    return;
                }
            }
        }
    }
}