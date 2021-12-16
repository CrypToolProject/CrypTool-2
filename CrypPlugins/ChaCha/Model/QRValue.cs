namespace CrypTool.Plugins.ChaCha.Model
{
    /// <summary>
    /// Class for the values during a quarterround.
    /// </summary>
    internal class QRValue : ChaChaHashValue
    {
        public QRValue(uint? value) : base(value)
        {
        }

        public QRValue() : base()
        {
        }

        /// <summary>
        /// True if the input paths for this value should be marked.
        /// </summary>
        private bool _markInput; public bool MarkInput

        {
            get => _markInput;
            set
            {
                _markInput = value;
                OnPropertyChanged();
            }
        }

        public override void Reset()
        {
            base.Reset();
            MarkInput = false;
        }
    }
}