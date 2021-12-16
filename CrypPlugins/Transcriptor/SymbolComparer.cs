/*
   Copyright 2014 Olga Groh

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

namespace Transcriptor
{
    internal class SymbolComparer : IComparer<Symbol>
    {
        private readonly bool mode;

        public SymbolComparer(bool mode)
        {
            this.mode = mode;
        }

        /// <summary>
        /// Defines the Compare Method
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>If mode set to true the Sort Function will sort X coordinates
        /// If mode set to false the Y coordinates will be sorted</returns>
        public int Compare(Symbol a, Symbol b)
        {
            if (mode == true)
            {
                if (a.X < b.X)
                {
                    return -1;
                }
                else
                {
                    if (a.X == b.X)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }
            }
            else
            {
                if (a.Y < b.Y)
                {
                    return -1;
                }
                else
                {
                    if (a.X == b.X)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }

                }
            }
        }
    }
}
