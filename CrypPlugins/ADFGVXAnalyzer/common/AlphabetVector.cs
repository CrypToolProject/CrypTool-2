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
using System.Text;

namespace common
{
    public class AlphabetVector : Vector
    {
        public static string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static StringBuilder ALPHABET_BUILDER = new StringBuilder(ALPHABET);

        public static int ALPHABET_SIZE = ALPHABET.Length;

        public AlphabetVector(string s, bool withStats)

            : base(ALPHABET_BUILDER, s, withStats)
        { }
        public AlphabetVector(int length, bool withStats)

            : base(ALPHABET_BUILDER, length, withStats)
        { }
        public AlphabetVector(bool withStats) : this(ALPHABET_SIZE, withStats) { }
        public AlphabetVector() : this(ALPHABET_SIZE, false) { }

    }

}
