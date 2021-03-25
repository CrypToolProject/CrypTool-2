using System.Collections;

namespace CrypTool.Plugins.ChaCha.Model
{
    /// <summary>
    /// Class for each step during one quarterround with focus on visualization.
    /// Supports nullable qr values.
    /// </summary>
    internal class VisualQRStep
    {
        public QRValue Add { get; set; } = new QRValue();
        public QRValue XOR { get; set; } = new QRValue();
        public QRValue Shift { get; set; } = new QRValue();

        /// <summary>
        /// Get enumerator for all quarterround values of this step.
        /// </summary>
        /// <remarks>
        /// Needed for `foreach`.
        /// </remarks>
        public IEnumerator GetEnumerator()
        {
            return new QRValue[] { Add, XOR, Shift }.GetEnumerator();
        }

        /// <summary>
        /// Reset all values in this qr step.
        /// </summary>
        public void Reset()
        {
            Add.Reset();
            XOR.Reset();
            Shift.Reset();
        }
    }
}