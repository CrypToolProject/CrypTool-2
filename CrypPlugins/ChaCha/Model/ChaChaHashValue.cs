/*
   Copyright (C) CrypTool 2 Team

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
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