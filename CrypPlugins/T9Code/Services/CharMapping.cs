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
using System.Collections.Generic;
using System.Text;

namespace CrypTool.T9Code.Services
{
    public static class CharMapping
    {
        private static readonly Dictionary<string, string[]> DigitToCharMapping = new Dictionary<string, string[]>
        {
            { "2", new[] { "a", "ä", "b", "c" } },
            { "3", new[] { "d", "e", "f" } },
            { "4", new[] { "g", "h", "i" } },
            { "5", new[] { "j", "k", "l" } },
            { "6", new[] { "m", "n", "o", "ö" } },
            { "7", new[] { "p", "q", "r", "s", "ß" } },
            { "8", new[] { "t", "u", "ü", "v" } },
            { "9", new[] { "w", "x", "y", "z" } },
            { "0", new[] { " " } }
        };

        private static Dictionary<string, string> _charToDigitMapping;

        private static readonly StringBuilder StringBuilder = new StringBuilder();

        private static Dictionary<string, string> CharToDigitMapping =>
            _charToDigitMapping ?? (_charToDigitMapping = GenerateCharToDigitMapping());

        private static Dictionary<string, string> GenerateCharToDigitMapping()
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string[]> keyValuePair in DigitToCharMapping)
            {
                foreach (string value in keyValuePair.Value)
                {
                    res[value] = keyValuePair.Key;
                }
            }

            return res;
        }

        public static string StringToDigit(string letters)
        {
            StringBuilder.Clear();
            string lowerLetters = letters.ToLower();
            foreach (char c in lowerLetters)
            {
                if (CharToDigitMapping.TryGetValue(c.ToString(), out string value))
                {
                    StringBuilder.Append(value);
                }
            }

            return StringBuilder.ToString();
        }
    }
}