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