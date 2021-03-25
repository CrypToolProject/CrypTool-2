using System;
using System.Collections.Generic;

namespace PlayfairAnalysis.Common
{

    public class NGrams
    {
        private static Dictionary<long, long> map7 = new Dictionary<long, long>();
        private static Dictionary<long, long> map8 = new Dictionary<long, long>();
        private static long MASK7 = (long)Math.Pow(26, 6);

        private static bool[] FILTER = new bool[(int)Math.Pow(26, 6)];
        private static long MASK8 = (long)Math.Pow(26, 7);
        private readonly AnalysisInstance instance;

        public NGrams(AnalysisInstance instance)
        {
            this.instance = instance;
        }

        public long eval7(int[] text, int len)
        {
            instance.Stats.evaluations++;
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
                if (!map7.TryGetValue(idx, out var v))
                {
                    continue;
                }
                score += 400_000 * v;
            }

            return score / (len - 7 + 1);
        }

        public long eval8(int[] text, int len)
        {
            instance.Stats.evaluations++;
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
                if (!map8.TryGetValue(idx, out var v))
                {
                    continue;
                }
                score += 400_000 * v;
            }
            return score / (len - 8 + 1);
        }
    }
}
