/*
   Copyright 2008-2013 Arno Wacker, University of Kassel

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

namespace CrypTool.Enigma
{
    internal partial class EnigmaCore
    {
        private const string A3 = "ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const int alen = 26; // Alphabet Length


        public static readonly string[] stators = {
                                            // Kommerzielle Enigma A/B
                                            "JWULCMNOHPQZYXIRADKEGVBTSF", // no reference found for this!! This is just a copy from Enigma D
                                            // Kommerzielle Enigma D
                                            "JWULCMNOHPQZYXIRADKEGVBTSF",  
                                            // Enigma der Reichsbahn („Rocket“), ab 7. Feb 1941
                                            "QWERTZUIOASDFGHJKPYXCVBNML", // see reference: "D. H. Hamer, G. Sullivan, and F. Weierud, “ENIGMA VARIATIONS : AN EXTENDED FAMILY OF MACHINES,” vol. 140, no. 3, pp. 1–17, 1993."
                                            // Enigma I, ab 1930, Walzen IV ab 1938, Walzen V-VII ab 1938
                                            "ABCDEFGHIJKLMNOPQRSTUVWXYZ", 
                                            // Enigma M4 "Shark"
                                            "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
                                            };

        public static readonly string[,] rotors = new string[,]{
                                        { // Kommerzielle Enigma A/B, ab 1924
                                            "DMTWSILRUYQNKFEJCAZBPGXOHV",
                                            "HQZGPJTMOBLNCIFDYAWVEUSRXL",
                                            "UQNTLSZFMREHDPLKIBVYGJCWGA",
                                            "",
                                            "",
                                            "",
                                            "",
                                            "",
                                        },
                                        { // Kommerzielle Enigma D
                                          // ETW: JWULCMNOHPQZYXIRADKEGVBTSF
                                            "LPGSZMHAEOQKVXRFYBUTNICJDW",
                                            "SLVGBTFXJQOHEWIRZYAMKPCNDU",
                                            "CJGDPSHKTURAWZXFMYNQOBVLIE",
                                            "",
                                            "",
                                            "",
                                            "",
                                            "",
                                        },
                                        { // Enigma der Reichsbahn („Rocket“), ab 7. Feb 1941
                                          // ETW: QWERTZUIOASDFGHJKPYXCVBNML
                                            "JGDQOXUSCAMIFRVTPNEWKBLZYH",
                                            "NTZPSFBOKMWRCJDIVLAEYUXHGQ",
                                            "JVIUBHTCDYAKEQZPOSGXNRMWFL",
                                            "", //"CIWTBKXNRESPFLYDAGVHQUOJZM", //a rotor from Enigma T - just used for the challenge on MTC3 (Cascading encryption - Part 3/3)
                                            "",
                                            "",
                                            "",
                                            "",
                                        },
                                        {   // Enigma I, ab 1930, Walzen IV ab 1938, Walzen V-VII ab 1938
                                            // ETW: ABCDEFGHIJKLMNOPQRSTUVWXYZ
                                            "EKMFLGDQVZNTOWYHXUSPAIBRCJ", // I
                                            "AJDKSIRUXBLHWTMCQGZNPYFVOE", // II
                                            "BDFHJLCPRTXVZNYEIWGAKMUSQO", // III
                                            "ESOVPZJAYQUIRHXLNFTGKDCMWB", // IV
                                            "VZBRGITYUPSDNHLXAWMJQOFECK", // V
                                            "JPGVOUMFYQBENHZRDKASXLICTW", // VI
                                            "NZJHGRCXMYSWBOUFAIVLPEKQDT", // VII
                                            "FKQHTLXOCBJSPDZRAMEWNIUYGV"  // VIII
                                        },
                                        {   // Enigma M4 "Shark"
                                            // ETW: ABCDEFGHIJKLMNOPQRSTUVWXYZ
                                            "LEYJVCNIXWPBQMDRTAKZGFUHOS", // Beta, ab 1 Feb. 1942
                                            "FSOKANUERHMBTIYCWLQPZXVGJD", // Gamma, ab 1. Juli 1943
                                            "",
                                            "",
                                            "",
                                            "",
                                            "",
                                            "",
                                        }
                                  };
        public static readonly string[,] reflectors = {
                                                    {  // Kommerzielle Enigma A/B - there was no reflector
                                                        "",
                                                        "",
                                                        ""
                                                    },
                                                    {  // Kommerzielle Enigma D
                                                        "IMETCGFRAYSQBZXWLHKDVUPOJN",
                                                        "",
                                                        ""
                                                    },
                                                    {  // Enigma der Reichsbahn („Rocket“), ab 7. Feb 1941
                                                        "QYHOGNECVPUZTFDJAXWMKISRBL",  // see reference: "D. H. Hamer, G. Sullivan, and F. Weierud, “ENIGMA VARIATIONS : AN EXTENDED FAMILY OF MACHINES,” vol. 140, no. 3, pp. 1–17, 1993."
                                                        "",
                                                        ""
                                                    },
                                                    {  // Enigma I, ab 1930, Walzen IV ab 1938, Walzen V-VII ab 1938
                                                        "EJMZALYXVBWFCRQUONTSPIKHGD", // UKW A
                                                        "YRUHQSLDPXNGOKMIEBFZCWVJAT", // UKW B
                                                        "FVPJIAOYEDRZXWGCTKUQSBNMHL"  // UKW C
                                                    },
                                                    {  // Enigma M4 "Shark"
                                                        "",
                                                        "ENKQAUYWJICOPBLMDXZVFTHRGS",
                                                        "RDOBJNTKVEHMLFCWZAXGYIPSUQ"
                                                    },
                                                };

        public static readonly string[,] notches = {
                                                {  // Kommerzielle Enigma A/B - work in progress - notches needed
                                                   "",  // I
                                                   "",  // II
                                                   "",  // III
                                                   "",  // IV
                                                   "",  // V
                                                   "", // VI
                                                   "", // VII
                                                   ""  // VIII
                                                },
                                                 {  // Kommerzielle Enigma D work in progress - notches needed
                                                   "",  // I
                                                   "",  // II
                                                   "",  // III
                                                   "",  // IV
                                                   "",  // V
                                                   "", // VI
                                                   "", // VII
                                                   ""  // VIII
                                                },
                                                {  // Enigma der Reichsbahn („Rocket“), ab 7. Feb 1941 // notches: V,M,G see reference: "D. H. Hamer, G. Sullivan, and F. Weierud, “ENIGMA VARIATIONS : AN EXTENDED FAMILY OF MACHINES,” vol. 140, no. 3, pp. 1–17, 1993."
                                                   "V",  // I
                                                   "M",  // II
                                                   "G",  // III
                                                   "", //"EHNTZ",  // notches for a rotor from Enigma T - just used for the challenge MTC3 (Cascading encryption - Part 3/3)
                                                   "",
                                                   "",
                                                   "",
                                                   ""
                                                },
                                                {  // Enigma I, ab 1930, Walzen IV ab 1938, Walzen V-VII ab 1938
                                                   "Q",  // I
                                                   "E",  // II
                                                   "V",  // III
                                                   "J",  // IV
                                                   "Z",  // V
                                                   "ZM", // VI
                                                   "ZM", // VII
                                                   "ZM"  // VIII
                                                },
                                                {  // Enigma M4 "Shark" - work in progress - notches need to be verified
                                                   "Q",  // I
                                                   "E",  // II
                                                   "V",  // III
                                                   "J",  // IV
                                                   "Z",  // V
                                                   "ZM", // VI
                                                   "ZM", // VII
                                                   "ZM"  // VIII
                                                },
                                             };

    }
}
