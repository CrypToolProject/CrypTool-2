namespace _3Round16BitSPN
{
    class _3Round16BitSPNConfiguration
    {
        public static readonly int[] PBOX = { 12, 9, 6, 3, 0, 13, 10, 7, 4, 1, 14, 11, 8, 5, 2, 15 };
        public static readonly int[] PBOXREVERSE = { 4, 9, 14, 3, 8, 13, 2, 7, 12, 1, 6, 11, 0, 5, 10, 15 };




        //public static readonly int[] PBOX = { 0, 13, 10, 7, 4, 1, 14, 11, 8, 5, 2, 15, 12, 9, 6, 3 };
        //public static readonly int[] PBOXREVERSE = { 0, 5, 10, 15, 4, 9, 14, 3, 8, 13, 2, 7, 12, 1, 6, 11 };





        public static readonly int[] SBOX = { 10, 0, 9, 14, 6, 3, 15, 5, 1, 13, 12, 7, 11, 4, 2, 8 };
        public static readonly int[] SBOXREVERSE = { 1, 8, 14, 5, 13, 7, 4, 11, 15, 2, 0, 12, 10, 9, 3, 6 };


        public static readonly int BITWIDTHCIPHERFOUR = 4;
        public static readonly int SBOXNUM = 4;
        public static readonly int KEYNUM = 4;
        public static readonly double PROBABILITYBOUNDBESTCHARACTERISTICSEARCH = 0.001;
        public static readonly double PROBABILITYBOUNDDIFFERENTIALSEARCH = 0.0001;


    }
}
