using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CrypTool.Plugins.ChaCha.Model
{
    /// <summary>
    /// Abstract class for all values which are calculated during the ChaCha hash function.
    /// </summary>
    internal abstract class ChaChaHashValue : INotifyPropertyChanged
    {
        public ChaChaHashValue(uint? value)
        {
            Value = value;
        }

        public ChaChaHashValue()
        {
            Value = null;
        }

        /// <summary>
        /// The actual UInt32 value. Can be null in case no value should be shown in the visualization.
        /// </summary>
        private uint? _value; public uint? Value

        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// True if the element containing the value should be marked, for example by setting the background to a specific color.
        /// </summary>
        private bool _mark; public bool Mark

        {
            get => _mark;
            set
            {
                _mark = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Reset the field.
        /// </summary>
        public virtual void Reset()
        {
            Value = null;
            Mark = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }
    }
}