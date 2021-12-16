/*
   Copyright 2021 by George Lasry, converted from Java to C# by Nils Kopal

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CrypTool.Plugins.SIGABA
{
    public enum SIGABAModel
    {
        CSP889,
        CSP2900
    }

    public enum Command
    {
        Key,
        Input,
        None,
        SIGABAModel
    }
    public class SIGABAImplementation
    {
        private readonly Rotor[] _cipherBank = new Rotor[5];
        private readonly Rotor[] _controlBank = new Rotor[5];
        private readonly IndexRotor[] _indexBank = new IndexRotor[5];
        private readonly SIGABAModel _model;

        public SIGABAImplementation(SIGABAModel model, string cipherRotors, string controlRotors, string indexRotors,
                                                       string cipherRotorPositions, string controlRotorPositions, string indexRotorPositions)
        {
            _model = model;
            for (int i = 0; i < 5; i++)
            {
                if (model == SIGABAModel.CSP2900 && (i == 1 || i == 3))
                {
                    _cipherBank[i] = new Rotor(cipherRotors[i * 2] - '0', cipherRotors[i * 2 + 1] == 'R', true, cipherRotorPositions[i] - 'A');
                }
                else
                {
                    _cipherBank[i] = new Rotor(cipherRotors[i * 2] - '0', cipherRotors[i * 2 + 1] == 'R', false, cipherRotorPositions[i] - 'A');
                }
                _controlBank[i] = new Rotor(controlRotors[i * 2] - '0', controlRotors[i * 2 + 1] == 'R', false, controlRotorPositions[i] - 'A');
                _indexBank[i] = new IndexRotor(indexRotors[i] - '0', indexRotorPositions[i] - '0');
            }
        }

        public string EncryptDecrypt(bool decrypt, string input)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char c in input.ToArray())
            {
                builder.Append((char)(CipherPath(decrypt, c - 'A') + 'A'));
                CipherBankUpdate();
                ControlBankUpdate();
            }
            return builder.ToString();
        }

        private void ControlBankUpdate()
        {
            if (_controlBank[2]._position == 'O' - 'A')
            {
                // medium rotor moves
                if (_controlBank[3]._position == 'O' - 'A')
                {
                    // slow rotor moves
                    _controlBank[1].Advance();
                }
                _controlBank[3].Advance();
            }
            _controlBank[2].Advance(); // fast rotor always moves
        }

        private void CipherBankUpdate()
        {
            bool[] move = new bool[5];
            if (_model == SIGABAModel.CSP889)
            {
                for (int i = 'F' - 'A'; i <= 'I' - 'A'; i++)
                {
                    move[SIGABAConstants.INDEX_OUT[IndexPath(SIGABAConstants.INDEX_IN_CSP889[ControlPath(i)])] - 1] = true;
                }
            }
            else
            {
                for (int i = 'D' - 'A'; i <= 'I' - 'A'; i++)
                {
                    int indexInput = SIGABAConstants.INDEX_IN_CSP2900[ControlPath(i)];
                    if (indexInput != -1)
                    {
                        move[SIGABAConstants.INDEX_OUT[IndexPath(indexInput)] - 1] = true;
                    }
                }
            }
            for (int i = 0; i < 5; i++)
            {
                if (move[i])
                {
                    _cipherBank[i].Advance();
                }
            }
        }

        private int CipherPath(bool decrypt, int c)
        {
            if (decrypt)
            {
                for (int r = 4; r >= 0; r--)
                {
                    c = _cipherBank[r].RightToLeft(c);
                }
            }
            else
            {
                for (int r = 0; r <= 4; r++)
                {
                    c = _cipherBank[r].LeftToRight(c);
                }
            }
            return c;
        }

        private int ControlPath(int c)
        {
            for (int r = 4; r >= 0; r--)
            {
                c = _controlBank[r].RightToLeft(c);
            }
            return c;
        }

        private int IndexPath(int c)
        {
            for (int r = 0; r <= 4; r++)
            {
                c = _indexBank[r].IndexPath(c);
            }
            return c;
        }

        public class Rotor
        {
            public const int LEFT_TO_RIGHT = 0;
            public const int RIGHT_TO_LEFT = 1;

            private readonly int[,] _wiring = new int[2, 26];
            public int _position;
            private readonly bool _reversedOrientation;
            private readonly bool _reversedMotion; // for CSP2900

            public Rotor(int wiringIndex, bool reversedOrientation, bool reversedMotion, int position)
            {
                for (int i = 0; i < 26; i++)
                {
                    _wiring[LEFT_TO_RIGHT, i] = SIGABAConstants.CIPHER_CONTROL_ROTOR_WIRINGS[wiringIndex][i] - 'A';
                    _wiring[RIGHT_TO_LEFT, _wiring[LEFT_TO_RIGHT, i]] = i;
                }
                _reversedOrientation = reversedOrientation;
                _reversedMotion = reversedMotion;
                _position = position;
            }

            public void Advance()
            {
                if (_reversedOrientation ^ _reversedMotion)
                {
                    _position = (_position + 1) % 26;
                }
                else
                {
                    _position = (_position - 1 + 26) % 26;
                }
            }

            public int LeftToRight(int str)
            {
                if (!_reversedOrientation)
                {
                    return (_wiring[LEFT_TO_RIGHT, (str + _position) % 26] - _position + 26) % 26;
                }
                return (_position - _wiring[RIGHT_TO_LEFT, (_position - str + 26) % 26] + 26) % 26;
            }

            public int RightToLeft(int str)
            {
                if (!_reversedOrientation)
                {
                    return (_wiring[RIGHT_TO_LEFT, (str + _position) % 26] - _position + 26) % 26;
                }
                return (_position - _wiring[LEFT_TO_RIGHT, (_position - str + 26) % 26] + 26) % 26;
            }
        }

        public class IndexRotor
        {
            private readonly int[] _wiring = new int[10];
            private readonly int _position;

            public IndexRotor(int wiringIndex, int pos)
            {
                Array.Copy(SIGABAConstants.INDEX_ROTOR_WIRINGS[wiringIndex], 0, _wiring, 0, 10);
                _position = pos;
            }

            public int IndexPath(int i)
            {
                return (_wiring[(i + _position) % 10] - _position + 10) % 10;
            }
        }

        private static void Usage(string error)
        {
            Console.Write("\r\n{0}\r\n\r\n", error);
            Console.WriteLine("Usage: SIGABA.exe [-m [SIGABAModel]] [-e|-d] [-k [Key]] -i [Input]");
            Console.WriteLine(" -e for encryption (default), -d for decryption");
            Console.WriteLine(" SIGABAModel: either CSP889 (default, when -m is omitted), or CSP2900");
            Console.WriteLine(" Key (for -k): [Cipher rotors,Control rotors,Index rotor,Cipher rotor positions,Control rotor positions,Index rotor positions]");
            Console.WriteLine("   Cipher/Control rotor format: [rotor number from 0 to 9,orientation N (normal) or R (reverse)], e.g., 0R1N2N3N4R");
            Console.WriteLine("   Cipher/Control rotor positions: from A to Z, e.g., ABCDE");
            Console.WriteLine("   Index rotors format: from 0 to 4, e.g., 01234");
            Console.WriteLine("   Cipher/Control rotor positions: from 0 to 9, e.g., 13579");
            Console.WriteLine("   If the key is not specified, a random key is generated");
            Console.WriteLine(" Input (for -i): Either the plaintext (for -e, str which case spaces are replaced by Z), or ciphertext (for -d)");
            Console.WriteLine();
            Console.WriteLine("Examples: sigaba.exe -m CSP889 -k 0R1N2N3N4R5N6N7R8N9N01234ABCDEFGHIJ01234 -e -i AAAAAAAAAAAAAAAAAAAA");
            Console.WriteLine("          sigaba.exe -m CSP889 -k 0R1N2N3N4R5N6N7R8N9N01234ABCDEFGHIJ01234 -d -i JTSCALXDRWOQKRXHKMVD");
            Console.WriteLine("          sigaba.exe -m CSP2900 -k 0R1N2N3N4R5N6N7R8N9N01234ABCDEFGHIJ01234 -e -i AAAAAAAAAAAAAAAAAAAA");
            Console.WriteLine("          sigaba.exe -m CSP889 -i AAAAAAAAAAAAAAAAAAAA");
            Console.WriteLine("          sigaba.exe -m CSP2900 -i AAAAAAAAAAAAAAAAAAAA");
            Environment.Exit(-1);
        }

        private static void CheckUniqueRotors(string arg, string rotorType)
        {
            HashSet<char> found = new HashSet<char>();
            foreach (char c in Regex.Replace(arg, "[^0-9]", string.Empty).ToArray())
            {
                if (!found.Add(c))
                {
                    Usage("Rotor " + c + " used more than once str " + rotorType + " rotors: " + arg);
                }
            }
        }

        private static int[] RandomPermutation(int size)
        {
            int[] permutations = new int[size];
            for (int i = 0; i < size; i++)
            {
                permutations[i] = i;
            }
            Random random = new Random();
            for (int i = 0; i < size - 2; i++)
            {
                int j = i + random.Next(size - i);
                int keep = permutations[i];
                permutations[i] = permutations[j];
                permutations[j] = keep;
            }
            return permutations;
        }

        public static void Main(string[] args)
        {
            string cipherRotors = string.Empty;
            string controlRotors = string.Empty;
            string indexRotors = string.Empty;
            string cipherRotorPositions = string.Empty;
            string controlRotorPositions = string.Empty;
            string indexRotorPositions = string.Empty;

            string input = string.Empty;
            SIGABAModel model = SIGABAModel.CSP889;
            bool decrypt = false;
            Command parameterExpectedFor = Command.None;

            foreach (string arg in args)
            {
                if (arg.StartsWith("-"))
                {
                    switch (arg.Substring(1))
                    {

                        case "k":
                        case "K":
                            parameterExpectedFor = Command.Key;
                            break;
                        case "i":
                        case "I":
                            parameterExpectedFor = Command.Input;
                            break;
                        case "e":
                        case "E":
                            decrypt = false;
                            parameterExpectedFor = Command.None;
                            break;
                        case "d":
                        case "D":
                            decrypt = true;
                            parameterExpectedFor = Command.None;
                            break;
                        case "m":
                        case "M":
                            parameterExpectedFor = Command.SIGABAModel;
                            break;
                        default:
                            Usage("Invalid command: " + arg);
                            break;
                    }
                    continue;
                }
                switch (parameterExpectedFor)
                {
                    case Command.Key:
                        if (arg.Length != 40)
                        {
                            Usage("Invalid key: " + arg);
                        }
                        cipherRotors = arg.Substring(0, 10);
                        if (!Regex.IsMatch(cipherRotors, "[0-9,NR,0-9,NR,0-9,NR,0-9,NR,0-9,NR]"))
                        {
                            Usage("Invalid Cipher rotor selection: " + cipherRotors);
                        }
                        CheckUniqueRotors(cipherRotors, "Cipher");

                        controlRotors = arg.Substring(10, 10);
                        if (!Regex.IsMatch(controlRotors, "[0-9,NR,0-9,NR,0-9,NR,0-9,NR,0-9,NR]"))
                        {
                            Usage("Invalid Control rotor selection: " + controlRotors);
                        }
                        CheckUniqueRotors(controlRotors, "Control");
                        CheckUniqueRotors(cipherRotors + controlRotors, "Cipher and Control");

                        indexRotors = arg.Substring(20, 5);
                        if (!Regex.IsMatch(indexRotors, "[0-4,0-4,0-4,0-4,0-4]"))
                        {
                            Usage("Invalid Index rotor selection: " + indexRotors);
                        }
                        CheckUniqueRotors(indexRotors, "Index");

                        cipherRotorPositions = arg.Substring(25, 5);
                        if (!Regex.IsMatch(cipherRotorPositions, "[A-Z,A-Z,A-Z,A-Z,A-Z]"))
                        {
                            Usage("Invalid Cipher rotor positions: " + cipherRotorPositions);
                        }

                        controlRotorPositions = arg.Substring(30, 5);
                        if (!Regex.IsMatch(controlRotorPositions, "[A-Z,A-Z,A-Z,A-Z,A-Z]"))
                        {
                            Usage("Invalid Control rotor positions: " + controlRotorPositions);
                        }

                        indexRotorPositions = arg.Substring(35, 5);
                        if (!Regex.IsMatch(indexRotorPositions, "[0-9,0-9,0-9,0-9,0-9]"))
                        {
                            Usage("Invalid Index rotor positions: " + indexRotorPositions);
                        }
                        parameterExpectedFor = Command.None;
                        break;
                    case Command.Input:
                        if (Regex.IsMatch(arg, "[a-zA-Z]+"))
                        {
                            input = arg.ToUpper();
                        }
                        else
                        {
                            Usage("Invalid " + (decrypt ? "plaintext" : "ciphertext") + " - only alphabetical letters allowed: " + arg);
                        }
                        parameterExpectedFor = Command.None;
                        break;
                    case Command.SIGABAModel:
                        if (arg.Equals("CSP889", StringComparison.OrdinalIgnoreCase))
                        {
                            model = SIGABAModel.CSP889;
                        }
                        else if (arg.Equals("CSP2900", StringComparison.OrdinalIgnoreCase))
                        {
                            model = SIGABAModel.CSP2900;
                        }
                        else
                        {
                            Usage("Invalid model: " + arg + " - either -m CSP889 or -m CSP2900 (if omitted, model is CSP899)");
                        }
                        break;
                    default:
                        Usage("Unexpected parameter: " + arg);
                        break;
                }
            }

            if (string.IsNullOrEmpty(input))
            {
                Usage(string.Empty + (decrypt ? "Plaintext" : "Ciphertext") + " not specified (-i)");
            }

            if (string.IsNullOrEmpty(cipherRotors))
            {
                int[] rotorsCtlCph = RandomPermutation(10);
                int[] rotorsIndex = RandomPermutation(5);

                Random random = new Random();
                for (int i = 0; i < 5; i++)
                {
                    cipherRotors += string.Format("{0}", rotorsCtlCph[i], random.NextDouble() > 0.5 ? "R" : "N");
                    controlRotors += string.Format("{0}", rotorsCtlCph[i + 5], random.NextDouble() > 0.5 ? "R" : "N");
                    indexRotors += string.Format("{0}", rotorsIndex[i]);
                    cipherRotorPositions += (char)('A' + random.Next(26));
                    controlRotorPositions += (char)('A' + random.Next(26));
                    indexRotorPositions += (char)('0' + random.Next(10));
                }
                Console.WriteLine("Generating a random key (model {0}): {1}", model, cipherRotors + controlRotors + indexRotors + cipherRotorPositions + controlRotorPositions + indexRotorPositions);
            }

            SIGABAImplementation sigabaImplementation = new SIGABAImplementation(model, cipherRotors, controlRotors, indexRotors,
                                                                       cipherRotorPositions, controlRotorPositions, indexRotorPositions);
            Console.WriteLine("Input:  {0}", input);
            string output = sigabaImplementation.EncryptDecrypt(decrypt, input);
            Console.WriteLine("Output: {0}", output);

            if (decrypt)
            {
                Console.WriteLine(output.Replace("Z", " "));
            }
        }
    }
}
