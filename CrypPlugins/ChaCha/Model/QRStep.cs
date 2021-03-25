namespace CrypTool.Plugins.ChaCha.Model
{
    /// <summary>
    /// Class for each step during one quarterround.
    /// </summary>
    public class QRStep
    {
        public QRStep(uint add, uint xor, uint shift)
        {
            Add = add;
            XOR = xor;
            Shift = shift;
        }

        public uint Add { get; }
        public uint XOR { get; }
        public uint Shift { get; }
    }
}