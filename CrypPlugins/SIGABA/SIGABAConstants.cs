/*
   Copyright 2021 by George Lasry, converted from Java to C# by Nils Kopal

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

namespace CrypTool.Plugins.SIGABA
{
    public static class SIGABAConstants
    {
        public static readonly string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static readonly int[] INDEX_IN_CSP889 = { 9, 1, 2, 3, 3, 4, 4, 4, 5, 5, 5, 6, 6, 6, 6, 7, 7, 7, 7, 7, 8, 8, 8, 8, 8, 8 };
        public static readonly int[] INDEX_IN_CSP2900 = { 9, 1, 2, 3, 3, 4, 4, 4, 5, 5, 5, 6, 6, 6, 6, -1, -1, -1, 7, 7, 0, 0, 8, 8, 8, 8 };
        public static readonly int[] INDEX_OUT = { 1, 5, 5, 4, 4, 3, 3, 2, 2, 1 };  // rotor stepping magnet

        public static readonly string[] CIPHER_CONTROL_ROTOR_WIRINGS =
        {
            "YCHLQSUGBDIXNZKERPVJTAWFOM",
            "INPXBWETGUYSAOCHVLDMQKZJFR",
            "WNDRIOZPTAXHFJYQBMSVEKUCGL",
            "TZGHOBKRVUXLQDMPNFWCJYEIAS",
            "YWTAHRQJVLCEXUNGBIPZMSDFOK",
            "QSLRBTEKOGAICFWYVMHJNXZUDP",
            "CHJDQIGNBSAKVTUOXFWLEPRMZY",
            "CDFAJXTIMNBEQHSUGRYLWZKVPO",
            "XHFESZDNRBCGKQIJLTVMUOYAPW",
            "EZJQXMOGYTCSFRIUPVNADLHWBK"
        };

        public static readonly int[][] INDEX_ROTOR_WIRINGS =
        {
            new int[] {7, 5, 9, 1, 4, 8, 2, 6, 3, 0},
            new int[] {3, 8, 1, 0, 5, 9, 2, 7, 6, 4},
            new int[] {4, 0, 8, 6, 1, 5, 3, 2, 9, 7},
            new int[] {3, 9, 8, 0, 5, 2, 6, 1, 7, 4},
            new int[] {6, 4, 9, 7, 1, 3, 5, 2, 8, 0}
        };
    }
}
