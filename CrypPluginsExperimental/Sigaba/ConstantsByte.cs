namespace Sigaba
{
    class ConstantsByte
    {
        public static readonly char[][] ControlCipherRotors = new char[][]
        {
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray(),     // testvector
            "INPXBWETGUYSAOCHVLDMQKZJFR".ToCharArray(),     // Rest imaginated rotor wirings,  
            "WNDRIOZPTAXHFJYQBMSVEKUCGL".ToCharArray(),     // non was used in the actual machine,
            "TZGHOBKRVUXLQDMPNFWCJYEIAS".ToCharArray(),     // copied from uss pampanito Sigaba Simulator, 
            "YWTAHRQJVLCEXUNGBIPZMSDFOK".ToCharArray(),     // see reference http://maritime.org/tech/ecmapp.txt
            "QSLRBTEKOGAICFWYVMHJNXZUDP".ToCharArray(),  
            "CHJDQIGNBSAKVTUOXFWLEPRMZY".ToCharArray(),
            "CDFAJXTIMNBEQHSUGRYLWZKVPO".ToCharArray(),
            "XHFESZDNRBCGKQIJLTVMUOYAPW".ToCharArray(),
            "EZJQXMOGYTCSFRIUPVNADLHWBK".ToCharArray(),
            "YCHLQSUGBDIXNZKERPVJTAWFOM".ToCharArray(),
        };

        public static readonly byte[][] IndexRotors =
        {     
            new byte[] {0,1,2,3,4,5,6,7,8,9},
            new byte[] {7,5,9,1,4,8,2,6,3,0},          // actual wirings of the real csp 889, see reference http://maritime.org/tech/ecmapp.txt
            new byte[] {3,8,1,0,5,9,2,7,6,4},
            new byte[] {4,0,8,6,1,5,3,2,9,7},
            new byte[] {3,9,8,0,5,2,6,1,7,4},
            new byte[] {6,4,9,7,1,3,5,2,8,0}
        };

        public static readonly byte[][] Transform = 
        {   
            new byte[] {9, 1, 2, 3, 3, 4, 4, 4, 5, 5, 5, 6, 6, 6, 6, 7, 7, 7, 7, 7, 8, 8, 8, 8, 8, 8 },
            new byte[] {9, 1, 2, 3, 3, 4, 4, 4, 5, 5, 5, 6, 6, 6, 6, 9, 9, 9, 7, 7, 0, 0, 8, 8, 8, 8 }
        };
        
        public static readonly byte[] Transform2 = { 0, 4, 4, 3, 3, 2, 2, 1, 1, 0 };
    }
}