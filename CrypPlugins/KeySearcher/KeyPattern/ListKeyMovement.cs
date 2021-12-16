/*                              
   Copyright 2010 Sven Rech (svenrech at googlemail dot com), Uni Duisburg-Essen

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

namespace KeySearcher.KeyPattern
{
    public class ListKeyMovement : IKeyMovement
    {
        public List<int> KeyList
        {
            get;
            private set;
        }

        public ListKeyMovement(List<int> keyList)
        {
            KeyList = keyList;
            KeyList.Sort();
        }

        public int Count()
        {
            return KeyList.Count;
        }
    }
}
