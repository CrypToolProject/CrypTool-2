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
