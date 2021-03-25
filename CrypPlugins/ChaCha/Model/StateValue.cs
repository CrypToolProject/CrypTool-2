namespace CrypTool.Plugins.ChaCha.Model
{
    /// <summary>
    /// Class for the values inside the state matrix.
    /// </summary>
    internal class StateValue : ChaChaHashValue
    {
        public StateValue(uint? value) : base(value)
        {
        }

        public StateValue() : base()
        {
        }
    }
}