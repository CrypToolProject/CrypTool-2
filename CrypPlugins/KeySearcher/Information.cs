using System;

namespace KeySearcher
{
    /// <summary>
    /// Represents one entry in our statistic list
    /// </summary>
    public class Information
    {
        public override string ToString()
        {
            return string.Format("{0} - {1} - {2} - {3} - {4}", Count, Hostname, Date, Current, Dead);
        }

        public int Count { get; set; }
        public string Hostname { get; set; }
        public DateTime Date { get; set; }
        public bool Current { get; set; }
        public bool Dead { get; set; }
    }
}