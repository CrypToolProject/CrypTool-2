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