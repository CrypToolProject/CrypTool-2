/*
   Copyright 2022 George Lasry, Nils Kopal, CrypTool 2 Team

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
using System;

namespace PlayfairAnalysis
{
    /// <summary>
    /// This class provides some methods equivalent to Java standard lib methods.
    /// </summary>
    public static class Arrays
    {
        public static int[] CopyOf(int[] original, int newLength)
        {
            int[] copy = new int[newLength];
            Array.Copy(original, 0, copy, 0, newLength);
            return copy;
        }

        internal static void Fill(int[] a, int val)
        {
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = val;
            }
        }
        internal static void Arraycopy(Array src, int srcPos, Array dest, int destPos, int length)
        {
            Array.Copy(src, srcPos, dest, destPos, length);
        }
    }
}
