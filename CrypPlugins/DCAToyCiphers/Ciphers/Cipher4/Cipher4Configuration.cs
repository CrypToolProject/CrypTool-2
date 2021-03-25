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

namespace DCAToyCiphers.Ciphers.Cipher4
{
    public static class Cipher4Configuration
    {
        public static readonly int BITWIDTHCIPHERFOUR = 4;
        public static readonly int SBOXNUM = 1;
        public static readonly int KEYNUM = 4;
        public static readonly int[] SBOX = { 6, 4, 12, 5, 0, 7, 2, 14, 1, 15, 3, 13, 8, 10, 9, 11 };
        public static readonly int[] SBOXREVERSE = { 4, 8, 6, 10, 1, 3, 0, 5, 12, 14, 13, 15, 2, 11, 7, 9 };
    }
}
