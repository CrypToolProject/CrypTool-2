/*
   Copyright CrypTool 2 Team josef.matwich@gmail.com

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

namespace M209AnalyzerLib.Common
{
    public static class RandomGen
    {
        private static Random _global = new Random();
        [ThreadStatic]
        private static Random _local;

        public static int NextInt()
        {
            Random inst = _local;
            if (inst == null)
            {
                int seed;
                lock (_global) seed = _global.Next();
                _local = inst = new Random(seed);
            }
            return inst.Next();
        }

        public static int NextInt(int range)
        {
            Random inst = _local;
            if (inst == null)
            {
                int seed;
                lock (_global) seed = _global.Next(range);
                _local = inst = new Random(seed);
            }
            return inst.Next(range);
        }

        public static double NextDouble()
        {

            double seed;
            lock (_global) seed = _global.NextDouble();

            return seed;
        }

        public static float NextFloat()
        {
            var result = (NextDouble()
                  * (Single.MaxValue - (double)Single.MinValue))
                  + Single.MinValue;
            return (float)result;
        }
    }
}
