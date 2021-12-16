/*
   Copyright 2019 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using System.Collections.Generic;

namespace CrypTool.Plugins.DECRYPTTools.Util
{
    public class TokenBuilder
    {
        private readonly List<Symbol> _symbols = new List<Symbol>();

        /// <summary>
        /// Appends a deep copy of the given symbol
        /// </summary>
        /// <param name="symbol"></param>
        public void Append(Symbol symbol)
        {
            _symbols.Add((Symbol)symbol.Clone());
        }

        /// <summary>
        /// Returns the length of the internal list
        /// </summary>
        public int Length => _symbols.Count;

        /// <summary>
        /// Returns a deep copy of the symbol or adds a deep copy
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Symbol this[int index]
        {
            get => (Symbol)_symbols[index].Clone();
            set => _symbols[index] = (Symbol)value.Clone();
        }

        /// <summary>
        /// Returns a new token with deep-copied symbols list
        /// from index "index" with "count" symbols
        /// </summary>
        /// <param name="index"></param>
        /// <param name="length"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public Token GetToken(int index, int count, Line line = null)
        {
            List<Symbol> symbols = new List<Symbol>();
            Token token = new Token(line);
            for (int i = index; i < index + count; i++)
            {
                symbols.Add((Symbol)_symbols[i].Clone());
            }
            token.Symbols = symbols;
            return token;
        }

        /// <summary>
        /// Removes the "count" symbols beginning with index "index"
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        public void Remove(int index, int count)
        {
            _symbols.RemoveRange(index, count);
        }

        /// <summary>
        /// Returns a deep-copied clone of the internal symbol list
        /// </summary>
        /// <returns></returns>
        public List<Symbol> ToList()
        {
            List<Symbol> copyList = new List<Symbol>();
            foreach (Symbol symbol in _symbols)
            {
                copyList.Add((Symbol)symbol.Clone());
            }
            return copyList;
        }

        /// <summary>
        /// Clears the internal list
        /// </summary>
        public void Clear()
        {
            _symbols.Clear();
        }
    }
}
