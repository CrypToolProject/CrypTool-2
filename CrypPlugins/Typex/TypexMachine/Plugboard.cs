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
using System;
using System.Collections.Generic;
using static CrypTool.Typex.TypexMachine.Util;

namespace CrypTool.Typex.TypexMachine
{
    public class Plugboard
    {
        private readonly int[] _plugs;
        private readonly int[] _inversePlugs;

        /// <summary>
        /// Creates a Plugboard with given plugs
        /// </summary>
        /// <param name="plugs"></param>
        /// <param name="alphabet"></param>
        public Plugboard(string plugs, string alphabet)
        {
            HashSet<char> letters = new HashSet<char>();
            foreach (char c in plugs)
            {
                if (!alphabet.Contains(string.Format("{0}", c)))
                {
                    throw new Exception(string.Format("Invalid plugs: \"{0}\". The \"{1}\" is not part of the alphabet", plugs, c));
                }
                if (letters.Contains(c))
                {
                    throw new Exception(string.Format("Invalid plugs: \"{0}\". The \"{1}\" is used more than once", plugs, c));
                }
                letters.Add(c);
            }
            _plugs = MapTextIntoNumberSpace(plugs, alphabet);
            _inversePlugs = new int[plugs.Length];
            for (int i = 0; i < _plugs.Length; i++)
            {
                _inversePlugs[_plugs[i]] = i;
            }

        }

        /// <summary>
        /// Encrypts the given letter
        /// </summary>
        /// <param name="letter"></param>
        /// <returns></returns>
        public int Encrypt(int letter)
        {
            return _plugs[letter];
        }
        /// <summary>
        /// Decrypts the given letter
        /// </summary>
        /// <param name="letter"></param>
        /// <returns></returns>
        public int Decrypt(int letter)
        {
            return _inversePlugs[letter];
        }
    }
}
