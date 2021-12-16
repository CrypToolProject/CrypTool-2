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

namespace CrypTool.Plugins.DCAToyCiphers
{
    /// <summary>
    /// Cipher1 = 16 bit blocksize, 2 subkeys, 32 bit key
    /// Cipher2 = 16 bit blocksize, 4 subkeys, 64 bit key
    /// Cipher3 = 16 bit blocksize, 6 subkeys, 96 bit key
    /// Cipher4 = 4 bit blocksize, 4 subkeys, 16 bit key
    /// </summary>
    public enum Algorithms
    {
        Cipher1,
        Cipher2,
        Cipher3,
        Cipher4
    }
}
