using System;

namespace KeySearcher
{
    /// <summary>
    /// Represents one entry in our maschine statistic list
    /// </summary>
    public class MachInfo
    {
        public int Sum { get; set; }
        public string Hostname { get; set; }
        public string Users { get; set; }
        public DateTime Date { get; set; }
        public bool Current { get; set; }

        private bool dead = true;
        public bool Dead { get => dead; set => dead = value; }
    }
}