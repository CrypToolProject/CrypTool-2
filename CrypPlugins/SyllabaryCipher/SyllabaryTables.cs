/*
   Copyright 2022 Nils Kopal <kopal<AT>cryptool.org>

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

namespace SyllabaryCipher
{

    /// <summary>
    /// This static class contains tables for the "Syllabary cipher".
    /// The English table is the original table shown in "Military Cryptanalytics Part 1 Chapter XI, April 1956 by William F. Friedman and Lambros D."
    /// The other tables are from the American Cryptogram Association (ACA), see: https://www.cryptogram.org/downloads/SYLLABARYMASTERCHART.pdf
    /// </summary>
    public static class SyllabaryTables
    {         
        public static readonly string[,] English =
        {
            // 1      2      3      4      5      6      7      8      9       0
      /* 1 */ {"A"  , "1"  , "AL" , "AN" , "AND", "AR" , "ARE", "AS" , "AT" , "ATE" }, 
      /* 2 */ {"ATI", "B"  , "2"  , "BE" , "C"  , "3"  , "CA" , "CE" , "CO" , "COM" }, 
      /* 3 */ {"D"  , "4"  , "DA" , "DE" , "E"  , "5"  , "EA" , "ED" , "EN" , "ENT" },
      /* 4 */ {"ER" , "ERE", "ERS", "ES" , "EST", "F"  , "6"  , "G"  , "7"  , "H"   },
      /* 5 */ {"8"  , "HAS", "HE" , "I"  , "9"  , "IN" , "ING", "ION", "IS" , "IT"  },
      /* 6 */ {"IVE", "J"  , "0"  , "K"  , "L"  , "LA" , "LE" , "M"  , "ME" , "N"   },
      /* 7 */ {"ND" , "NE" , "NT" , "O"  , "OF" , "ON" , "OR" , "OU" , "P"  , "Q"   },
      /* 8 */ {"R"  , "RA" , "RE" , "RED", "RES", "RI" , "RO" , "S"  , "SE" , "SH"  },
      /* 9 */ {"ST" , "STO", "T"  , "TE" , "TED", "TER", "TH" , "THE", "THI", "THR" },
      /* 0 */ {"TI" , "TO" , "U"  , "V"  , "VE" , "W"  , "WE" , "X"  , "Y"  , "Z"   }
        };

        public static readonly string[,] Italian =
        {
             // 1      2      3      4      5      6      7      8      9       0
      /* 1 */ {"A"  , "1"  , "AL" , "AN" , "AR" , "ATO", "B"  , "2"  , "C"  , "3"  }, 
      /* 2 */ {"CA" , "CHE", "CI" , "CO" , "D"  , "4"  , "DA" , "DE" , "DI" , "E"  }, 
      /* 3 */ {"5"  , "EL" , "EN" , "ER" , "ES" , "ET" , "F"  , "6"  , "G"  , "7"  },
      /* 4 */ {"GI" , "H"  , "8"  , "I"  , "9"  , "IA" , "IC" , "IL" , "IN" , "ION"},
      /* 5 */ {"IS" , "IT" , "J"  , "0"  , "K"  , "L"  , "LA" , "LE" , "LI" , "LL" },
      /* 6 */ {"LO" , "M"  , "MA" , "ME" , "MI" , "MO" , "N"  , "NA" , "NE" , "NI" },
      /* 7 */ {"NO" , "NTE", "O"  , "OL" , "ON" , "OR" , "OS" , "P"  , "PA" , "PER"},
      /* 8 */ {"PO" , "PR" , "Q"  , "R"  , "RA" , "RE" , "RI" , "RO" , "S"  , "SA" },
      /* 9 */ {"SE" , "SI" , "SO" , "SS" , "ST" , "T"  , "TA" , "TE" , "TI" , "TO" },
      /* 0 */ {"TR" , "TT" , "U"  , "UN" , "V"  , "W"  , "X"  , "Y"  , "Z"  , "ZIO"}
        };

        public static readonly string[,] French =
        {
             // 1      2      3      4      5      6      7      8      9       0
      /* 1 */ {"A"  , "1"  , "AI" , "AIS", "AIT", "AN" , "ANS", "AR" , "AS" , "B"  },
      /* 2 */ {"2"  , "C"  , "3"  , "CE" , "D"  , "4"  , "DAN", "DE" , "DEL", "DES"},
      /* 3 */ {"DU" , "E"  , "5"  , "ED" , "EDE", "EL" , "ELL", "EM" , "EME", "EN" },
      /* 4 */ {"ENT", "ER" , "ES" , "ESE", "EST", "EUR", "F"  , "6"  , "G"  , "7"  },
      /* 5 */ {"GE" , "H"  , "8"  , "I"  , "9"  , "IE" , "ION", "IT" , "J"  , "0"  },
      /* 6 */ {"K"  , "L"  , "LA" , "LE" , "LES", "LLE", "M"  , "ME" , "MEN", "N"  },
      /* 7 */ {"NE" , "NO" , "NON", "NS" , "NT" , "O"  , "ON" , "ONT", "OU" , "OUI"},
      /* 8 */ {"OUR", "OUS", "P"  , "PAR", "Q"  , "QU" , "QUE", "QUI", "R"  , "RE" },
      /* 9 */ {"RES", "S"  , "SE" , "SSE", "T"  , "TE" , "TI" , "TIO", "TRE", "TTE"},
      /* 0 */ {"U"  , "UI" , "UN" , "UNE", "UR" , "V"  , "W"  , "X"  , "Y"  , "Z"  }
        };

        public static readonly string[,] German =
        {
             // 1      2      3      4      5      6      7      8      9       0
      /* 1 */ {"A"  , "1"  , "AB" , "AHT", "ALS", "AM" , "AN" , "AU" , "AUF", "B"  },
      /* 2 */ {"2"  , "BE" , "BEN", "BER", "C"  , "3"  , "CH" , "CHE", "CHT", "D"  },
      /* 3 */ {"4"  , "DA" , "DE" , "DEN", "DER", "DES", "DI" , "DIE", "DU" , "E"  },
      /* 4 */ {"5"  , "EI" , "EIN", "EL" , "EN" , "END", "ER" , "F"  , "6"  , "G"  },
      /* 5 */ {"7"  , "GE" , "GEN", "H"  , "8"  , "HA" , "HE" , "HEN", "I"  , "9"  },
      /* 6 */ {"ICH", "IE" , "IN" , "ISC", "IST", "IT" , "J"  , "0"  , "K"  , "L"  },
      /* 7 */ {"M"  , "MI" , "MIT", "N"  , "ND" , "NDE", "NE" , "NO" , "NS" , "NUR"},
      /* 8 */ {"O"  , "OB" , "P"  , "Q"  , "R"  , "RCH", "RE" , "S"  , "SCH", "SE" },
      /* 9 */ {"ST" , "T"  , "TE" , "TEN", "U"  , "UE" , "UM" , "UN" , "UND", "UNG"},
      /* 0 */ {"V"  , "VON", "W"  , "WAR", "WAS", "WO" , "X"  , "Y"  , "Z"  , "ZU" }
        };

        public static readonly string[,] Spanish =
        {
             // 1      2      3      4      5      6      7      8      9       0
      /* 1 */ {"A"  , "1"  , "AD" , "ADO", "AL" , "AQU", "AR" , "ARA", "AS" , "B"  },
      /* 2 */ {"2"  , "C"  , "3"  , "CI" , "CIO", "CO" , "CON", "D"  , "4"  , "DE" },
      /* 3 */ {"DEL", "DI" , "E"  , "5"  , "EDE", "EL" , "EN" , "ER" , "ES" , "EST"},
      /* 4 */ {"F"  , "6"  , "G"  , "7"  , "H"  , "8"  , "HAY", "I"  , "9"  , "IO" },
      /* 5 */ {"IST", "J"  , "0"  , "K"  , "L"  , "LA" , "LAS", "LO" , "LOS", "M"  },
      /* 6 */ {"MAS", "ME" , "MI" , "MUY", "N"  , "NEI", "NO" , "NON", "NTE", "O"  },
      /* 7 */ {"ON" , "OR" , "OS" , "OSA", "P"  , "PER", "POR", "Q"  , "QU" , "QUE"},
      /* 8 */ {"R"  , "RA" , "RE" , "RES", "S"  , "SDE", "SE" , "SER", "SI" , "SIN"},
      /* 9 */ {"SON", "ST" , "SU" , "SUS", "T"  , "TA" , "TE" , "TI" , "TU" , "U"  },
      /* 0 */ {"UE" , "UN" , "UNA", "UNO", "V"  , "VA" , "W"  , "X"  , "Y"  , "Z"  }
        };

        public static readonly string[,] Latin =
        {
             // 1      2      3      4      5      6      7      8      9       0
      /* 1 */ {"A"  , "1"  , "AD" , "AE" , "AM" , "ANT", "AS" , "AT" , "ATI", "ATU"},
      /* 2 */ {"B"  , "2"  , "BUS", "C"  , "3"  , "CON", "CUM", "D"  , "4"  , "E"  },
      /* 3 */ {"5"  , "EM" , "ENT", "EQU", "ER" , "ERA", "ERI", "ES" , "ET" , "EX" },
      /* 4 */ {"F"  , "6"  , "G"  , "7"  , "H"  , "8"  , "I"  , "9"  , "IA" , "IBU"},
      /* 5 */ {"IN" , "IO" , "ION", "IS" , "ISS", "IT" , "ITA", "ITU", "J"  , "0"  },
      /* 6 */ {"K"  , "L"  , "M"  , "N"  , "NE" , "NT" , "O"  , "OS" , "P"  , "PER"},
      /* 7 */ {"PRO", "Q"  , "QUA", "QUE", "QUI", "QUO", "R"  , "RA" , "RAT", "RE" },
      /* 8 */ {"RI" , "RUM", "S"  , "SE" , "SI" , "SSE", "STR", "T"  , "TA" , "TAT"},
      /* 9 */ {"TE" , "TER", "TI" , "TIS", "TO" , "TUM", "TUR", "U"  , "UA" , "UI" },
      /* 0 */ {"UM" , "UNT", "UR" , "US" , "UT" , "V"  , "W"  , "X"  , "Y"  , "Z"  }
        };
    }
}