using System;
using System.Numerics;

namespace LatticeCrypto.Models
{
    public class LCGModel
    {
        private readonly int a;
        private readonly int c;
        private readonly int mod;
        private readonly int x0;

        public LCGModel()
        {
            a = 16807;
            c = 0;
            mod = (int)Math.Pow(2, 31) - 1;
            x0 = (int)(DateTime.Now.Ticks % mod);
        }

        public LCGModel(int a, int mod)
        {
            this.a = a;
            this.mod = mod;
            c = 0;
            x0 = (int)(DateTime.Now.Ticks % mod);
        }

        public LCGModel(int a, int c, int mod)
        {
            this.a = a;
            this.c = c;
            this.mod = mod;
            x0 = (int)(DateTime.Now.Ticks % mod);
        }

        public LCGModel(int a, int c, int mod, int x0)
        {
            this.a = a;
            this.c = c;
            this.mod = mod;
            this.x0 = x0;
        }

        public int[] GetRandomNumbers(int length)
        {
            int[] result = new int[length];
            BigInteger currentX = x0;

            for (int i = 0; i < length; i++)
            {
                result[i] = (int)((a * currentX + c) % mod);
                currentX = result[i];
            }
            return result;
        }
    }
}
