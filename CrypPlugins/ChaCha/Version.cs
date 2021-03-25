namespace CrypTool.Plugins.ChaCha
{
    public class Version
    {
        public static readonly Version IETF = new Version("IETF", 32, 96);
        public static readonly Version DJB = new Version("DJB", 64, 64);
        public string Name { get; }
        public uint CounterBits { get; }
        public uint IVBits { get; }

        private Version(string name, uint counterBits, uint ivBits)
        {
            Name = name;
            CounterBits = counterBits;
            IVBits = ivBits;
        }
    }
}