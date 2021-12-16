using System.Collections.Concurrent;

namespace KeySearcher.CrypCloud.statistics
{
    public class CricularBuffer<T> : ConcurrentQueue<T>
    {
        private readonly object syncObject = new object();

        public int Size { get; private set; }

        public CricularBuffer(int size)
        {
            Size = size;
        }

        public new void Enqueue(T obj)
        {
            base.Enqueue(obj);
            lock (syncObject)
            {
                while (base.Count > Size)
                {
                    base.TryDequeue(out T outObj);
                }
            }
        }
    }
}
