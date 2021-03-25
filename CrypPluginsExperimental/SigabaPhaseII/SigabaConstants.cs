using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sigaba
{
    class SigabaConstants
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

        public static readonly int[][] IndexRotors = new int[][]
                                                            { new int[] {0,1,2,3,4,5,6,7,8,9},
                                                              new int[] {7,5,9,1,4,8,2,6,3,0},          // actual wirings of the real csp 889, see reference http://maritime.org/tech/ecmapp.txt
                                                              new int[] {3,8,1,0,5,9,2,7,6,4},
                                                              new int[] {4,0,8,6,1,5,3,2,9,7},
                                                              new int[] {3,9,8,0,5,2,6,1,7,4},
                                                              new int[] {6,4,9,7,1,3,5,2,8,0}
                                                          };

        public static readonly int[][] Transform = {    new int[] {9, 1, 2, 3, 3, 4, 4, 4, 5, 5, 5, 6, 6, 6, 6, 7, 7, 7, 7, 7, 8, 8, 8, 8, 8, 8 },
                                                        new int[] {9, 1, 2, 3, 3, 4, 4, 4, 5, 5, 5, 6, 6, 6, 6, 9, 9, 9, 7, 7, 0, 0, 8, 8, 8, 8 }
                                                   };
        
        public static readonly int[] Transform2 = { 0, 4, 4, 3, 3, 2, 2, 1, 1, 0 };

    }
}
