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
namespace CrypTool.Plugins.ChaCha.Helper
{
    internal static class Digits
    {
        /// <summary>
        /// Return the amount of digits this number has in the decimal system.
        /// For example, 154 has 3 digits.
        /// </summary>
        public static int GetAmountOfDigits(int n)
        {
            int count = 0;
            while (n != 0)
            {
                count++;
                n /= 10;
            }
            return count;
        }
    }
}