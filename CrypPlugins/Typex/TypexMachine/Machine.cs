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
    public class Machine
    {
        private readonly string _alphabet;
        private readonly Plugboard _plugboard;
        private readonly Rotor[] _rotors;
        private readonly Reflector _reflector;

        /// <summary>
        /// Creates a Typex machine with the given rotors and reflector and alphabet
        /// if alphabet is null, it uses the alphabet to create a default reflector (reversed alphabet)
        /// if reflector is null, it creates a "reversed alphabet" reflector
        /// </summary>
        /// <param name="rotors"></param>
        /// <param name="reflector"></param>
        /// <param name="plugboard"></param>
        /// <param name="alphabet"></param>
        public Machine(Rotor[] rotors, Reflector reflector = null, Plugboard plugboard = null, string alphabet = null)
        {
            _alphabet = alphabet;
            _rotors = rotors;

            if (reflector is null)
            {
                //default reflector is just the reversed alphabet
                _reflector = new Reflector(MapTextIntoNumberSpace(_alphabet, _alphabet).Reverse(), null, 0, 0);
            }
            else
            {
                _reflector = reflector;
            }

            if (plugboard is null)
            {
                //default plugboard is a plugboard where each letter maps to itself
                _plugboard = new Plugboard(_alphabet, _alphabet);
            }
            else
            {
                _plugboard = plugboard;
            }
        }

        /// <summary>
        /// The currently set key
        /// </summary>
        public int[] Key
        {
            get
            {
                int[] key = new int[_rotors.Length];
                for (int i = 0; i < _rotors.Length; i++)
                {
                    key[i] = _rotors[i].Rotation;
                }
                return key;
            }
            set
            {
                for (int i = 0; i < value.Length; i++)
                {
                    _rotors[i].Rotation = value[i];
                }
            }
        }

        /// <summary>
        /// En- or decrypts a single letter
        /// </summary>
        /// <param name="letter"></param>
        /// <returns></returns>
        public int CryptLetter(int letter)
        {
            //Step rotors
            int r = 0;
            do
            {
                _rotors[r].Step();
                if (_rotors[r].IsAtNotchPosition() ||
                (r < _rotors.Length - 1 &&
                    _rotors[r + 1].HasAnomaly &&
                    _rotors[r + 1].WillBeAtNotchPosition()))
                {
                    r++; // next rotor should step                    
                }
                else
                {
                    break; // stop 
                }
            } while (r < _rotors.Length);

            //1. Go through plugboard
            letter = _plugboard.Encrypt(letter);

            //2. Go forward through the rotors            
            for (int i = 0; i < _rotors.Length; i++)
            {
                letter = _rotors[i].Encrypt(letter);
            }

            //3. Go through reflector
            letter = _reflector.Encrypt(letter);

            //4. Go backwards through the rotors
            for (int i = _rotors.Length - 1; i >= 0; i--)
            {
                letter = _rotors[i].Decrypt(letter);
            }

            //5. Go through plugboard
            letter = _plugboard.Decrypt(letter);

            return letter;
        }
    }
}
