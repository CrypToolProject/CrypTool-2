/*
   Copyright 2023 Nils Kopal <kopal<AT>cryptool.org>

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
using static CrypTool.Typex.TypexMachine.Util;

namespace CrypTool.Typex.TypexMachine
{
    /// <summary>
    /// A single Rotor of the Rotor Machine
    /// A rotor can encrypt and decrypt letters
    /// </summary>
    public class Rotor
    {
        protected int[] _rotor;
        protected int[] _inverseRotor;
        private readonly int[] _notches;
        private int _rotation;

        public bool HasAnomaly
        {
            get; private set;
        }

        public bool IsReversed
        {
            get; private set;
        }

        /// <summary>
        /// Creates a rotor with the given parameters
        /// </summary>
        /// <param name="rotor"></param>
        /// <param name="notches"></param>
        /// <param name="ringPosition"></param>
        /// <param name="rotation"></param>
        /// <param name="isReversed"></param>
        public Rotor(int[] rotor, int[] notches, int ringPosition, int rotation, bool hasAnomaly = false, bool isReversed = false)
        {
            _rotor = rotor;
            if (isReversed)
            {
                int[] reversed = new int[_rotor.Length];
                for (int i = 0; i < _rotor.Length; i++)
                {
                    reversed[Util.Mod(_rotor.Length - _rotor[i], _rotor.Length)] = Util.Mod(_rotor.Length - i, _rotor.Length);
                }
                _rotor = reversed;
            }
            if (notches != null)
            {
                _notches = (int[])notches.Clone();
                for (int i = 0; i < _notches.Length; i++)
                {
                    _notches[i] = Mod(_notches[i] + ringPosition, _rotor.Length);
                }
            }

            _inverseRotor = GenerateInverseRotor(_rotor);
            Rotation = Mod(rotation - ringPosition, _rotor.Length);
            HasAnomaly = hasAnomaly;
            IsReversed = isReversed;
        }

        /// <summary>
        /// Current rotation of this rotor
        /// </summary>
        public int Rotation
        {
            get => _rotation;
            set => _rotation = Mod(value, _rotor.Length);
        }

        /// <summary>
        /// Creates the inverse rotor for decryption
        /// </summary>
        /// <param name="rotor"></param>
        /// <returns></returns>
        private int[] GenerateInverseRotor(int[] rotor)
        {
            int[] inverseRotor = new int[rotor.Length];
            for (int i = 0; i < rotor.Length; i++)
            {
                inverseRotor[i] = rotor.IndexOf(i);
            }
            return inverseRotor;
        }

        /// <summary>
        /// Move rotor one step
        /// </summary>
        public virtual void Step()
        {
            _rotation = Mod(_rotation + 1, _rotor.Length);
        }

        /// <summary>
        /// Returns true, if rotor is currently at a notch position
        /// </summary>
        /// <returns></returns>
        public virtual bool IsAtNotchPosition()
        {
            return _notches.Contains(Rotation);
        }

        /// <summary>
        /// Returns true, if rotor is currently at a notch position
        /// </summary>
        /// <returns></returns>
        public virtual bool WillBeAtNotchPosition()
        {
            return _notches.Contains(Util.Mod(Rotation + 1, _rotor.Length));
        }

        /// <summary>
        /// Encrypts a single letter
        /// </summary>
        /// <param name="plaintextLetter"></param>
        /// <returns></returns>
        public int Encrypt(int plaintextLetter)
        {
            return Mod(_rotor[Mod(plaintextLetter + Rotation, _rotor.Length)] - Rotation, _rotor.Length);
        }

        /// <summary>
        /// Decrypts a single letter
        /// </summary>
        /// <param name="ciphertextLetter"></param>
        /// <returns></returns>
        public int Decrypt(int ciphertextLetter)
        {
            return Mod(_inverseRotor[Mod(ciphertextLetter + Rotation, _inverseRotor.Length)] - Rotation, _inverseRotor.Length);
        }
    }
}
