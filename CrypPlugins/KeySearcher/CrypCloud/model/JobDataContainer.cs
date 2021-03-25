using System.Numerics;
using CrypTool.PluginBase.Control;

namespace KeySearcher
{
    internal class JobDataContainer
    {
        public BigInteger JobId { get; set; }
        public byte[] Ciphertext { get; set; }
        public IControlEncryption CryptoAlgorithm { get; set; }
        public IControlCost CostAlgorithm { get; set; }
        public int BytesToUse { get; set; }
        public byte[] InitVector { get; set; } 
        public BigInteger NumberOfBlocks { get; set; }

    }
}