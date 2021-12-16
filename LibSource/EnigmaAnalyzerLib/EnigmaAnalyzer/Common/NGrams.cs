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

using System;
using System.Collections.Generic;
using System.IO;

namespace EnigmaAnalyzerLib.Common
{
    public class NGrams
    {
        private static readonly Dictionary<long, long> map7 = new Dictionary<long, long>();
        private static readonly Dictionary<long, long> map8 = new Dictionary<long, long>();
        private static readonly long MASK7 = (long)Math.Pow(26, 6);

        private static readonly bool[] FILTER = new bool[(int)Math.Pow(26, 6)];
        private static readonly long MASK8 = (long)Math.Pow(26, 7);

        public static long eval7(int[] text, int len)
        {
            Stats.evaluations++;
            long idx = 0;
            long score = 0;
            for (int i = 0; i < len; i++)
            {
                idx = (idx % MASK7) * 26 + text[i];
                if (i < 7 - 1)
                {
                    continue;
                }
                if (!FILTER[(int)(idx / 26)])
                {
                    continue;
                }
                long v = map7[idx];
                if (v == 0)
                {
                    continue;
                }
                score += 400_000 * v;
            }

            return score / (len - 7 + 1);
        }

        public static long eval8(int[] text, int len)
        {
            Stats.evaluations++;
            long idx = 0;
            long score = 0;
            for (int i = 0; i < len; i++)
            {
                idx = (idx % MASK8) * 26 + text[i];
                if (i < 8 - 1)
                {
                    continue;
                }
                if (!FILTER[(int)(idx / (26 * 26))])
                {
                    continue;
                }
                long v = map8[idx];
                if (v == 0)
                {
                    continue;
                }
                score += 400_000 * v;
            }
            return score / (len - 8 + 1);
        }

        public static bool load(string statsFilename, int ngrams)
        {
            try
            {
                FileStream inputstream = new FileStream(statsFilename, FileMode.Open, FileAccess.Read);
                Dictionary<long, long> map = ngrams == 8 ? map8 : map7;
                map.Clear();
                /*ObjectInputStream inputStream = new ObjectInputStream(inputstream);
                long[] data = (long[]) inputStream.readObject();
                Console.WriteLine("Read {0} items from {1}", data.Length/2, statsFilename);
                long using = Math.Min(data.Length / 2, 1_000_000);
                for (int i = 0; i < usng; i++) {
                    long index = data[2 * i];
                    long value = data[2 * i + 1] + 1;
                    map.Add(index, (long) (Math.Log(value) / Math.Log(2)));
                    if (ngrams == 7) {
                        FILTER[(int) (index / 26)] = true;
                    } else {
                        FILTER[(int) (index / (26 * 26))] = true;
                    }
                }*/
                //Console.WriteLine("using %,d items from {0} (free shorts %,d)", usng, statsFilename, Runtime.getRuntime().freeMemory());

                inputstream.Close();


                return true;
            }
            catch (IOException e)
            {
                Console.WriteLine(e.StackTrace);
            }
            return false;
        }
    }
}