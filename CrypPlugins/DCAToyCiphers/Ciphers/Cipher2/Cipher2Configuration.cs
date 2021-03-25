/*
   Copyright 2019 Christian Bender christian1.bender@student.uni-siegen.de

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

namespace DCAToyCiphers.Ciphers.Cipher2
{
    public static class Cipher2Configuration
    {
        public static readonly int[] SBOX = { 10, 0, 9, 14, 6, 3, 15, 5, 1, 13, 12, 7, 11, 4, 2, 8 };
        public static readonly int[] SBOXREVERSE = { 1, 8, 14, 5, 13, 7, 4, 11, 15, 2, 0, 12, 10, 9, 3, 6 };
        public static readonly int[] PBOX = { 12, 9, 6, 3, 0, 13, 10, 7, 4, 1, 14, 11, 8, 5, 2, 15 };
        public static readonly int[] PBOXREVERSE = { 4, 9, 14, 3, 8, 13, 2, 7, 12, 1, 6, 11, 0, 5, 10, 15 };
        public static readonly int BITWIDTHCIPHERFOUR = 4;
        public static readonly int SBOXNUM = 4;
        public static readonly int KEYNUM = 4;
    }
}
