/*
   Copyright (C) CrypTool 2 Team

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
using System.Numerics; 
using KeySearcher.Helper;
using KeySearcher.P2P.Exceptions;
using KeySearcher.P2P.Storage;

namespace KeySearcher.P2P.Tree
{
    abstract class NodeBase
    {
        protected internal readonly BigInteger From;
        protected internal readonly BigInteger To;
        protected internal readonly string DistributedJobIdentifier;
        protected readonly KeyQualityHelper KeyQualityHelper;

        protected long id;
        protected string hostname;

        protected internal DateTime LastUpdate;

        public readonly Node ParentNode;
        public LinkedList<KeySearcher.ValueKey> Result;

        //Dictionary Tests        
        public String Avatarname = "CrypTool2"; 
        
        public Dictionary<String, Dictionary<long, Information>> Activity;
        protected bool integrated;

        protected NodeBase( KeyQualityHelper keyQualityHelper, Node parentNode, BigInteger @from, BigInteger to, string distributedJobIdentifier)
        {
            KeyQualityHelper = keyQualityHelper;
            ParentNode = parentNode;
            From = @from;
            To = to;
            DistributedJobIdentifier = distributedJobIdentifier;

            LastUpdate = DateTime.MinValue;
            Result = new LinkedList<KeySearcher.ValueKey>();

            Activity = new Dictionary<string, Dictionary<long, Information>>();
            integrated = false;

        }

        protected void UpdateDht()
        {
            
        }

        public void PushToResults(KeySearcher.ValueKey valueKey)
        {
            if (Result.Contains(valueKey))
                return;

            var node = Result.First;
            while (node != null)
            {
                if (KeyQualityHelper.IsBetter(valueKey.value, node.Value.value))
                {
                    Result.AddBefore(node, valueKey);
                    if (Result.Count > 10)
                        Result.RemoveLast();
                    return;
                }
                node = node.Next;
            } 

            if (Result.Count < 10)
                Result.AddLast(valueKey);
        } 

        protected void UpdateActivity(Int64 id, String hostname)
        {
            var Maschine = new Dictionary<long, Information> { {id , new Information() { Count = 1, Hostname = hostname,Date = DateTime.UtcNow } } };
            if (!Activity.ContainsKey(Avatarname))
            {
                Activity.Add(Avatarname, Maschine);
            }
        }

        public abstract bool IsReserved();

        public abstract Leaf CalculatableLeaf(bool useReservedNodes);

        public abstract bool IsCalculated();

        public abstract void Reset();

        public abstract void UpdateCache();

        public override string ToString()
        {
            return "NodeBase " + GetType() + ", from " + From + " to " + To;
        }
    }
}
