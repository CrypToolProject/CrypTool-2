/*
   Copyright 2018 Dominik Vogt <ct2contact@CrypTool.org>

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
using common;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace ADFGVXAnalyzer
{
    public class Alphabet36Vector : Vector
    {
        private static readonly ADFGVXANalyzerSettings settings = new ADFGVXANalyzerSettings();
        private static readonly string ALPHABET = settings.Alphabet;
        private static readonly int ALPHABET_SIZE = ALPHABET.Length;
        private static readonly StringBuilder ALPHABET_BUILDER = new StringBuilder(ALPHABET);

        public Alphabet36Vector(string s, bool withStats)
            : base(ALPHABET_BUILDER, s, withStats)
        { }

        public Alphabet36Vector(int length, bool withStats)

            : base(ALPHABET_BUILDER, length, withStats)
        { }

        public Alphabet36Vector(bool withStats)

            : this(ALPHABET_SIZE, withStats)
        { }

        public Alphabet36Vector()

            : this(ALPHABET_SIZE, false)
        { }

        public string ToCleanString()
        {
            return Regex.Replace(ToString(), "[0-9XY]", " ");
        }
        public string ToCleanString(int length)
        {
            string buffer = Regex.Replace(ToString(), "[0-9XY]", " ");
            return buffer.Substring(0, Math.Min(length, ToString().Length));
        }
    }
}
