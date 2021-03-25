using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeysToyCipher
{
    class HeysToyCipherConfiguration
    {
        public static readonly int BITWIDTHCIPHERFOUR = 4;
        public static readonly int SBOXNUM = 4;
        public static readonly int KEYNUM = 6;
        public static readonly double PROBABILITYBOUNDBESTCHARACTERISTICSEARCH = 0.001;
        public static readonly double PROBABILITYBOUNDDIFFERENTIALSEARCH = 0.0001;
        public static readonly int[] SBOX = { 14, 4, 13, 1, 2, 15, 11, 8, 3, 10, 6, 12, 5, 9, 0, 7 };
        public static readonly int[] PBOX = { 0, 4, 8, 12, 1, 5, 9, 13, 2, 6, 10, 14, 3, 7, 11, 15 };
        public static readonly int[] SBOXREVERSE = { 14, 3, 4, 8, 1, 12, 10, 15, 7, 13, 9, 6, 11, 2, 0, 5 };
        public static readonly int[] PBOXREVERSE = { 0, 4, 8, 12, 1, 5, 9, 13, 2, 6, 10, 14, 3, 7, 11, 15 };
        public static readonly int PAIRMULTIPLIER = 1000;
    }
}
