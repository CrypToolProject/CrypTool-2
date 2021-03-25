using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyOneCiphers
{
    static class Cipher16Bit16Configuration
    {
        public static readonly double PROBABILITYBOUNDBESTCHARACTERISTICSEARCH = 0.001;
        public static readonly double PROBABILITYBOUNDDIFFERENTIALSEARCH = 0.0001;

        public static readonly int BITWIDTHCIPHERFOUR = 4;
        public static readonly int SBOXNUM = 4;
                                           // 00, 01, 02, 03, 04, 05, 06, 07, 08, 09, 10, 11, 12, 13, 14, 15
        public static readonly int[] SBOX = { 10, 05, 15, 08, 11, 00, 03, 07, 01, 13, 09, 12, 06, 14, 02, 04 };
        public static readonly int[] SBOXREVERSE = { 05, 08, 14, 06, 15, 01, 12, 07, 03, 10, 00, 04, 11, 09, 13, 02 };
                                           // 00, 01, 02, 03, 04, 05, 06, 07, 08, 09, 10, 11, 12, 13, 14, 15
        public static readonly int[] PBOX = { 06, 13, 02, 11, 09, 10, 07, 04, 08, 14, 12, 03, 15, 01, 00, 05 };
 public static readonly int[] PBOXREVERSE = { 14, 13, 02, 11, 07, 15, 00, 06, 08, 04, 05, 03, 10, 01, 09, 12 };
    }
}
