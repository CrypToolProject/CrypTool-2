/*
   Copyright 2023 Nils Kopal <kopal<AT>cryptool.org>

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
using System.Collections.Generic;

namespace CrypTool.Typex.TypexMachine
{
    public enum TypexMachineType
    {
        CyberChef = 0,
        TestRotors = 1,
        EnigmaI = 2,
        Y296 = 3,
        SP02390 = 4,
        SP02391 = 5
    }

    public enum TypexReflector
    {
        CyberChef = 0,
        TestReflector = 1,
        EnigmaI_A = 2,
        EnigmaI_B = 3,
        EnigmaI_C = 4,
        Custom = 5
    }

    /// Typex "CyberChef"
    /// Compatible to: https://gchq.github.io/CyberChef
    /// Also see: https://github.com/gchq/CyberChef/blob/master/src/core/lib/Typex.mjs
    public static class Typex_CyberChef
    {
        /// <summary>
        /// Rotor definitions from CyberChef
        /// </summary>
        public static string[] Rotors = new string[] {
            "MCYLPQUVRXGSAOWNBJEZDTFKHI",
            "KHWENRCBISXJQGOFMAPVYZDLTU",
            "BYPDZMGIKQCUSATREHOJNLFWXV",
            "ZANJCGDLVHIXOBRPMSWQUKFYET",
            "QXBGUTOVFCZPJIHSWERYNDAMLK",
            "BDCNWUEIQVFTSXALOGZJYMHKPR",
            "WJUKEIABMSGFTQZVCNPHORDXYL",
            "TNVCZXDIPFWQKHSJMAOYLEURGB"
        };

        /// <summary>
        /// Notches defintions from CyberChef
        /// </summary>
        public static readonly int[][] Notches = new int[][]
        {
            //all eight rotors have the same notches
            new int[] { 1, 5, 7, 13, 16, 20, 22 }, //BFHNQUW         
            new int[] { 1, 5, 7, 13, 16, 20, 22 }, //BFHNQUW
            new int[] { 1, 5, 7, 13, 16, 20, 22 }, //BFHNQUW
            new int[] { 1, 5, 7, 13, 16, 20, 22 }, //BFHNQUW
            new int[] { 1, 5, 7, 13, 16, 20, 22 }, //BFHNQUW
            new int[] { 1, 5, 7, 13, 16, 20, 22 }, //BFHNQUW
            new int[] { 1, 5, 7, 13, 16, 20, 22 }, //BFHNQUW
            new int[] { 1, 5, 7, 13, 16, 20, 22 }, //BFHNQUW
        };

        /// <summary>
        /// Reflector definition from CyberChef
        /// </summary>
        public const string Reflector = "NCBKIGFMEXDUHARYWOTSLZQJPV";

        /// <summary>
        /// Generates the array of eight rotor names supported by the CyberChef typex for the settings
        /// </summary>
        /// <returns></returns>
        public static string[] GenerateRotorNameArray()
        {
            List<string> rotorsStators = new List<string>();

            for (int i = 1; i <= Rotors.Length; i++)
            {
                rotorsStators.Add(string.Format("Rotor-{0}", i));
            }
            return rotorsStators.ToArray();
        }
    }

    /// <summary>
    /// Taken from https://typex.virtualcolossus.co.uk/Typex/
    /// </summary>
    public static class Typex_TestRotors
    {
        /// <summary>
        /// Rotor definitions
        /// </summary>
        public static string[] Rotors = new string[] {
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
        };

        /// <summary>
        /// Notches defintions
        /// </summary>
        public static readonly int[][] Notches = new int[][]
        {
            //all five rotors have the same notches
            new int[] { 5, 12, 18, 25 }, //ZFMS
            new int[] { 5, 12, 18, 25 }, //ZFMS
            new int[] { 5, 12, 18, 25 }, //ZFMS
            new int[] { 5, 12, 18, 25 }, //ZFMS
            new int[] { 5, 12, 18, 25 }, //ZFMS
        };

        /// <summary>
        /// Reflector definition
        /// </summary>
        public const string Reflector = "XEUWBIKSFOGPYRJLZNHVCTDAMQ";

        /// <summary>
        /// Generates the array of five test rotor names
        /// </summary>
        /// <returns></returns>
        public static string[] GenerateRotorNamesArray()
        {
            List<string> rotorsStators = new List<string>();

            for (int i = 1; i <= Rotors.Length; i++)
            {
                rotorsStators.Add(string.Format("Test-{0}", i));
            }
            return rotorsStators.ToArray();
        }
    }

    /// <summary>
    /// Rotors of the Enigma I
    /// </summary>
    public static class Typex_EnigmaI
    {
        /// <summary>
        /// Rotor definitions
        /// </summary>
        public static string[] Rotors = new string[] {
            "EKMFLGDQVZNTOWYHXUSPAIBRCJ",
            "AJDKSIRUXBLHWTMCQGZNPYFVOE",
            "BDFHJLCPRTXVZNYEIWGAKMUSQO",
            "ESOVPZJAYQUIRHXLNFTGKDCMWB",
            "VZBRGITYUPSDNHLXAWMJQOFECK",
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
        };

        /// <summary>
        /// Notches defintions
        /// </summary>
        public static readonly int[][] Notches = new int[][]
        {
            //all five rotors have the same notches
            new int[] { 17 }, //Q
            new int[] {  5 }, //E
            new int[] { 22 }, //V
            new int[] { 10 }, //J
            new int[] { 0, 13}, //Z
            new int[] { }, //none
            new int[] { }, //none
        };

        /// <summary>
        /// Reflector definition
        /// </summary>
        public const string UKWA = "EJMZALYXVBWFCRQUONTSPIKHGD"; // UKW A
        public const string UKWB = "YRUHQSLDPXNGOKMIEBFZCWVJAT"; // UKW B
        public const string UKWC = "FVPJIAOYEDRZXWGCTKUQSBNMHL"; // UKW C

        /// <summary>
        /// Generates the array of five test rotor names
        /// </summary>
        /// <returns></returns>
        public static string[] GenerateRotorNamesArray()
        {
            return new string[] { "Rotor I", "Rotor II", "Rotor III", "Rotor IV", "Rotor V", "Static 1", "Static 2" };
        }
    }

    /// <summary>
    /// Taken from https://typex.virtualcolossus.co.uk/Typex/
    /// </summary>
    public static class Typex_Y296
    {
        /// <summary>
        /// Rotor definitions
        /// </summary>
        public static string[] Rotors = new string[] {
            "UWIRLZPEBJODKVAFMTCSHYXGNQ",
            "YGBAOWMTJRHPENFCXKUIDQZLSV",
            "HWAUTKYJONLFIQZDCBRGPEMXVS",
            "QYBUHAOJNCTLIZSWFPMGEVXDRK",
            "YILOKTBWFQNCGHPXDJSVAUMZRE",
            "EXAYBHVUSOLCMQGWNDIZTPKFJR",
            "KBOIZMTXRHDCGPYAUSVLQJEWNF",
            "HQXCNBIRUMOYAFTGKZLESWJDVP",
            "BUMPZVYKJXGTDROCSLQIAEHWFN",
            "KCHGDVUEOMBLXRFWYNQIZPJATS",
            "WJAVECIQNKZGDUBSPXMFYTHLRO",
            "IGESOYLDUJAMVHRCXFPZTQNBWK",
            "HDRZVLBTIOEWCKNSYFQXMUPGJA",
            "LCMSBEIOQJXFAWKTHDRZNVYPGU"
        };

        /// <summary>
        /// Notches defintions
        /// </summary>
        public static readonly int[][] Notches = new int[][]
        {
            //all 13 rotors have the same notches
            new int[] { 5, 12, 18, 25 }, //ZFMS
            new int[] { 5, 12, 18, 25 }, //ZFMS
            new int[] { 5, 12, 18, 25 }, //ZFMS
            new int[] { 5, 12, 18, 25 }, //ZFMS
            new int[] { 5, 12, 18, 25 }, //ZFMS
            new int[] { 5, 12, 18, 25 }, //ZFMS
            new int[] { 5, 12, 18, 25 }, //ZFMS
            new int[] { 5, 12, 18, 25 }, //ZFMS
            new int[] { 5, 12, 18, 25 }, //ZFMS
            new int[] { 5, 12, 18, 25 }, //ZFMS
            new int[] { 5, 12, 18, 25 }, //ZFMS
            new int[] { 5, 12, 18, 25 }, //ZFMS
            new int[] { 5, 12, 18, 25 }, //ZFMS
        };

        /// <summary>
        /// Generates the array of the rotor names (Rotor-A until Rotor-N)
        /// </summary>
        /// <returns></returns>
        public static string[] GenerateRotorNamesArray()
        {
            List<string> rotorsStators = new List<string>();

            for (int i = 0; i < Rotors.Length; i++)
            {
                rotorsStators.Add(string.Format("Rotor-{0}", (char)('A' + i)));
            }
            return rotorsStators.ToArray();
        }
    }

    /// <summary>
    /// Taken from https://typex.virtualcolossus.co.uk/Typex/
    /// </summary>
    public static class Typex_SP02390
    {
        /// <summary>
        /// Rotor definitions
        /// </summary>
        public static string[] Rotors = new string[] {
            "RFNVBKTIHXQGCWAEOLSMPYDZUJ",
            "OLTGENZUJDIBYPSAFWRQMCXKHV",
            "TWBHQDURMLNIEAKSVOYCJGXFPZ",
            "LYUMXSFBPZONKJCEQIATGWRHDV",
            "KGBTYSOAIVXCJPRQZNHLFWUEMD",
            "GMRUYBJZHFKTDWQCOSXAIEPNVL",
            "LUSYEITRJAPFKWCVMQHBGNXZOD"
        };

        /// <summary>
        /// Notches defintions
        /// </summary>
        public static readonly int[][] Notches = new int[][]
        {
            //all 7 rotors have the same notches
            new int[] { 5, 12, 18, 25 }, //ZFMS
            new int[] { 5, 12, 18, 25 }, //ZFMS
            new int[] { 5, 12, 18, 25 }, //ZFMS
            new int[] { 5, 12, 18, 25 }, //ZFMS
            new int[] { 5, 12, 18, 25 }, //ZFMS
            new int[] { 5, 12, 18, 25 }, //ZFMS
            new int[] { 5, 12, 18, 25 }, //ZFMS
        };

        /// <summary>
        /// Generates the array of the rotor names (Rotor-A until Rotor-G)
        /// </summary>
        /// <returns></returns>
        public static string[] GenerateRotorNamesArray()
        {
            List<string> rotorsStators = new List<string>();

            for (int i = 0; i < Rotors.Length; i++)
            {
                rotorsStators.Add(string.Format("Rotor-{0}", (char)('A' + i)));
            }
            return rotorsStators.ToArray();
        }
    }

    /// <summary>
    /// Taken from https://typex.virtualcolossus.co.uk/Typex/
    /// </summary>
    public static class Typex_SP02391
    {
        /// <summary>
        /// Rotor definitions
        /// </summary>
        public static string[] Rotors = new string[] {
            "VGUMFDRWOLHPIEAZKYXTCJQNBS",
            "JSENUYFLPBZIGARQCTXOHMKDWV",
            "PCTLQEIADOXHVUGFNBWZSYKRJM",
            "ZYDJURAHWMVEISQKXNFBLTPGCO",
            "NCSYQXJEWDMLTGKAUORIZBFVPH",
            "NLGVFUSOJCQBMKRDHWYETPIFAX",
            "YVKOZUBXMWCILZRDQTJNAESHPG"
        };

        /// <summary>
        /// Notches defintions
        /// </summary>
        public static readonly int[][] Notches = new int[][]
        {
            //all 7 rotors have the same notches
            new int[] { 5, 12, 18, 25 }, //ZFMS
            new int[] { 5, 12, 18, 25 }, //ZFMS
            new int[] { 5, 12, 18, 25 }, //ZFMS
            new int[] { 5, 12, 18, 25 }, //ZFMS
            new int[] { 5, 12, 18, 25 }, //ZFMS
            new int[] { 5, 12, 18, 25 }, //ZFMS
            new int[] { 5, 12, 18, 25 }, //ZFMS
        };

        /// <summary>
        /// Generates the array of the rotor names (Rotor-A until Rotor-G)
        /// </summary>
        /// <returns></returns>
        public static string[] GenerateRotorNamesArray()
        {
            List<string> rotorsStators = new List<string>();

            for (int i = 0; i < Rotors.Length; i++)
            {
                rotorsStators.Add(string.Format("Rotor-{0}", (char)('A' + i)));
            }
            return rotorsStators.ToArray();
        }
    }
}
