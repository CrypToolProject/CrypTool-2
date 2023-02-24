/*                              
   Copyright 2023 Nils Kopal <nils.kopal@cryptool.org>
 
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
using System.Collections.Generic;

namespace WorkspaceManagerModel.Model.Tools
{
    /// <summary>
    /// This class takes care that each object serialized by the XMLSerializer gets a unique identifier. It seems that we had a HashCode collision when 
    /// saving huge workspace files. Therefore, we replaced the GetHashCode call with this construct here:
    /// </summary>
    internal class ObjectIDGenerator
    {
        private long _id = -1; // ids start with 0 (we increment the first id)
        private readonly List<(object obj, long id)> _objects = new List<(object, long)>();

        /// <summary>
        /// Returns a unique id for a given object. 
        /// Returns the same id when called again with the same object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public long GetID(object obj)
        {
            foreach ((object obj, long id) tuple in _objects)
            {
                //we check for reference equality, thus two exact copies of the same object will get different ids
                if (ReferenceEquals(tuple.obj, obj))                 
                {
                    return tuple.id;
                }
            }
            _id++;
            _objects.Add((obj, _id));            
            return _id;
        }
    }
}
