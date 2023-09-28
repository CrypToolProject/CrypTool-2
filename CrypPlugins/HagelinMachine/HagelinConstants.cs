/*
   Copyright 2022 Vasily Mikhalev, CrypTool 2 Team

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

namespace HagelinMachine
{
    public class HagelinConstants
    {
        public const int minNumberOfWheels = 2;
        public const int maxNumberOfWheels = 12;
        public const int minNumberOfBars = 2;
        public const int maxNumberOfBars = 32;
        public const int minWheelSize = 2;
        public const int maxWheelSize = 100;

        public static readonly int[] WheelSIZES_CX52C = { 25, 26, 46, 42, 38, 34 };
        public static readonly int[] WHEELS_SIZES_ALL_47 = { 47, 47, 47, 47, 47, 47 };
        public static readonly int[] WheelSIZES_DEFAULT = { 29, 31, 37, 41, 43, 47 };
        public static readonly int[] WheelSIZES_M209 = { 26, 25, 23, 21, 19, 17 };
        public static readonly string[] KNOWN_WheelTYPES = { "17", "19", "21", "23", "25", "26", "29", "31", "34", "37", "38", "41", "42", "43", "46", "47" };
    }
}
