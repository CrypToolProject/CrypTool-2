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
using System.Text;

namespace ADFGVXAnalyzer
{
    public class ADFGVXVector : Vector
    {
        private static readonly ADFGVXANalyzerSettings settings = new ADFGVXANalyzerSettings();
        private static readonly string ALPHABET = settings.EncryptAlphabet;

        //static String ALPHABET = "ADFGVXZ";
        private static readonly StringBuilder ALPHABET_BUILDER = new StringBuilder(ALPHABET);
        private static readonly int ALPHABET_SIZE = ALPHABET.Length;

        public ADFGVXVector(string s, bool withStats)
            : base(ALPHABET_BUILDER, s.ToUpper(), withStats)
        { }

        public ADFGVXVector(int length, bool withStats)
            : base(ALPHABET_BUILDER, length, withStats)
        { }

        public ADFGVXVector(bool withStats)
           : this(ALPHABET_SIZE, withStats)
        { }

        public ADFGVXVector()
            : this(ALPHABET_SIZE, false)
        { }
    }
}
