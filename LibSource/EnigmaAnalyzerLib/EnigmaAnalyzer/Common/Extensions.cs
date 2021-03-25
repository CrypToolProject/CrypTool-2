/*
   Copyright 2020 George Lasry
   Converted in 2020 from Java to C# by Nils Kopal

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

namespace EnigmaAnalyzerLib.Common
{
    public static class Extensions
    {
        public static bool EqualsIgnoreCase(this string a, string b)
        {
            return a.ToLower().Equals(b.ToLower());
        }

        public static string JavaSubstring(this string s, int index1, int index2)
        {
            return s.Substring(index1, index2 - index1);
        }
    }
}
