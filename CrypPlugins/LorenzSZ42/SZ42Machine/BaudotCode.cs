/*
   Copyright 2023 Nils Kopal <Nils.Kopal<at>CrypTool.org
   Converted from George Lasry's SZ42 Java implementation (and adapted)

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
using CrypTool.Core.Properties;
using System;
using System.Text;

namespace CrypTool.LorenzSZ42.SZ42Machine
{
    public enum BaudotNotation
    {
        Raw,
        Readable,
        British
    }

    public class BaudotCode
    {
        // Figure symbols - they are the same for all notations. ENQ and BEL have no printable equivalend, so using @ and * instead
        private const string FIGURES = "$3$-$'87$@4*,!:(5+)2£6019?&$./;$";  // $ = special symbols
        // @ = ENQ
        // * = BEL

        private const byte BYTE_CR = 2;
        private const byte BYTE_LF = 8;
        private const byte BYTE_LTRS = 31;
        private const byte BYTE_SP = 4;
        private const byte BYTE_FIGS = 27;
        private const byte BYTE_NUL = 0;

        // Raw plaintext alphabet - all valid symbols, with additional symbols represented with closest equivalents
        private const string RAW_PLAINTEXT_LETTERS = " ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÃÅΆĄÂªªÇČÐĎΛĚÊÈÉĘËĮÎÌÍÏŁŇŃÑØÒÓÔŐÕΘº°ǪΦÞŘŔŠ§ŤÚŰÙÛŮ×ÝŻŽŹ\t";
        private const string PLAINTEXT_LETTERS_MAP = " ABCDEFGHIJKLMNOPQRSTUVWXYZAAAAAAAAACCDDDEEEEEEIIIIILNNNOOOOOOOOOOPPRRSSTUUUUUXYZZZ ";
        private const string RAW_PLAINTEXT_FIGURES = "3-'87@4*,!:(5+)2£6019?&./;{}[]<>~%–—_=$#|«»’^“”·̨" + "\"" + "\\";
        private const string PLAINTEXT_FIGURES_MAP = "3-'87@4*,!:(5+)2£6019?&./;()()()&&----£&/''''''.," + "'" + "/";

        // British notation, used at Bletchley Park
        private const char BRITISH_CR = '3';
        private const char BRITISH_LF = '4';
        private const char BRITISH_LTRS = '8';
        private const char BRITISH_FIGS = '+';
        private const char BRITISH_SP = '9';
        private const char BRITISH_NUL = '/';

        private static readonly string BRITISH_LETTERS = Letters(BRITISH_CR, BRITISH_LF, BRITISH_LTRS, BRITISH_FIGS, BRITISH_SP, BRITISH_NUL);

        // Readable notation - each symbol represented by one symbol (no loss of information), properly reproducing figure shift symbols
        // Useful when matching symbol by symbol against ciphertext in single line (while being able to read figure symbols)
        private const char READABLE_CR = '#';
        private const char READABLE_LF = '|';
        private const char READABLE_LTRS = '>';
        private const char READABLE_FIGS = '<';
        private const char READABLE_SP = ' ';
        private const char READABLE_NUL = '_';
        private static readonly string READABLE_LETTERS = Letters(READABLE_CR, READABLE_LF, READABLE_LTRS, READABLE_FIGS, READABLE_SP, READABLE_NUL);

        // Build the letter mode symbols
        private static string Letters(char cr, char lf, char ltrs, char figs, char sp, char nul)
        {
            return ("" + nul + "E" + lf + "A" + sp + "SIU" + cr + "DRJNFCKTZLWHYPQOBG" + figs + "MXV" + ltrs);
        }

        // Parameters to format raw text
        public bool LETTER_SHIFT_BEFORE_SPACE { get; set; } = false; // 9 --> 89 (if DOUBLE_LETTER_OR_FIGURE_SHIFT = true then 9 -> 889)
        public bool DOUBLE_LETTER_OR_FIGURE_SHIFT { get; set; } = false;  // when false: full stop . --> +N8 ; when true . -> ++N88    (note that usually there is a space after that: ++N889)

        /// <summary>
        /// Parse raw plaintext. Replace unsupported characters with closest equivalents in Baudot (some unknown symbols may be discarded)
        /// Apply the rules for adding additional/double shift symbols
        /// </summary>
        /// <param name="plaintextstring"></param>
        /// <returns></returns>
        public byte[] FromRawPlaintext(string plaintextstring)
        {
            // Convert special German symbols
            plaintextstring = plaintextstring
                    .Replace("ß", "SS")
                    .ToUpper()   // uppercase only after replacing ß
                    .Replace("Ä", "AE")
                    .Replace("Ö", "OE")
                    .Replace("Ü", "UE");

            StringBuilder stringBuilder = new StringBuilder();
            bool figureShift = false;

            foreach (char c in plaintextstring)
            {
                if (c > 65000)
                {
                    continue;
                }

                switch (c)
                {
                    case ' ':
                        if (figureShift || LETTER_SHIFT_BEFORE_SPACE)
                        {
                            stringBuilder.Append(READABLE_LTRS);
                            if (DOUBLE_LETTER_OR_FIGURE_SHIFT)
                            {
                                stringBuilder.Append(READABLE_LTRS);
                            }
                        }
                        figureShift = false;
                        stringBuilder.Append(READABLE_SP);
                        break;
                    case '\n':
                        if (figureShift)
                        {
                            stringBuilder.Append(READABLE_LTRS);
                            if (DOUBLE_LETTER_OR_FIGURE_SHIFT)
                            {
                                stringBuilder.Append(READABLE_LTRS);
                            }
                            figureShift = false;
                        }
                        stringBuilder.Append(READABLE_LF);
                        break;
                    case '\r':
                        if (figureShift)
                        {
                            stringBuilder.Append(READABLE_LTRS);
                            if (DOUBLE_LETTER_OR_FIGURE_SHIFT)
                            {
                                stringBuilder.Append(READABLE_LTRS);
                            }
                            figureShift = false;
                        }
                        stringBuilder.Append(READABLE_CR);
                        break;
                    default:
                        int index = RAW_PLAINTEXT_LETTERS.IndexOf(c);
                        if (index != -1)
                        {
                            if (figureShift)
                            {
                                stringBuilder.Append(READABLE_LTRS);
                                if (DOUBLE_LETTER_OR_FIGURE_SHIFT)
                                {
                                    stringBuilder.Append(READABLE_LTRS);
                                }
                                figureShift = false;
                            }
                            stringBuilder.Append(PLAINTEXT_LETTERS_MAP[index]);
                            continue;
                        }
                        index = RAW_PLAINTEXT_FIGURES.IndexOf(c);
                        if (index != -1)
                        {
                            if (!figureShift)
                            {
                                stringBuilder.Append(READABLE_FIGS);
                                if (DOUBLE_LETTER_OR_FIGURE_SHIFT)
                                {
                                    stringBuilder.Append(READABLE_FIGS);
                                }
                                figureShift = true;
                            }
                            stringBuilder.Append(PLAINTEXT_FIGURES_MAP[index]);
                            continue;
                        }
                        break;
                }
            }
            return FromReadableNotation(stringBuilder.ToString());
        }

        /// <summary>
        /// Converts Baudot code to raw plaintext
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public string ToRawPlaintext(byte[] bytes)
        {
            StringBuilder stringBuilder = new StringBuilder();
            bool figureShift = false;
            int offset = 0;

            foreach (byte b in bytes)
            {
                switch (b)
                {
                    case BYTE_FIGS:
                        figureShift = true;
                        break;
                    case BYTE_LTRS:
                        figureShift = false;
                        break;
                    case BYTE_CR:
                        break;
                    case BYTE_LF:
                        stringBuilder.Append('\n');
                        break;
                    case BYTE_SP:
                        stringBuilder.Append(' ');
                        break;
                    case BYTE_NUL:
                        break;
                    default:
                        if (b > 0b00011111)
                        {
                            throw new Exception(string.Format(Properties.Resources.CannotConvertToRawPlaintext, ToBinaryString(b), (char)b, offset));
                        }
                        if (figureShift)
                        {

                            stringBuilder.Append(FIGURES[b]);
                        }
                        else
                        {
                            stringBuilder.Append(READABLE_LETTERS[b]);
                        }
                        break;
                }
                offset++;
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Parse/create readable plaintext
        /// </summary>
        /// <param name="readableNotationstring"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public byte[] FromReadableNotation(string readableNotationstring)
        {
            int len = readableNotationstring.Length;
            byte[] bytes = new byte[len];
            bool figureShift = false;
            for (int i = 0; i < len; i++)
            {
                char c = readableNotationstring[i];
                byte b;
                switch (c)
                {
                    case READABLE_SP:
                        b = BYTE_SP;
                        break;
                    case READABLE_NUL:
                        b = BYTE_NUL;
                        break;
                    case READABLE_LTRS:
                        figureShift = false;
                        b = BYTE_LTRS;
                        break;
                    case READABLE_FIGS:
                        figureShift = true;
                        b = BYTE_FIGS;
                        break;
                    case READABLE_LF:
                        b = BYTE_LF;
                        break;
                    case READABLE_CR:
                        b = BYTE_CR;
                        break;
                    default:
                        int index;
                        if (figureShift)
                        {
                            index = FIGURES.IndexOf(c);
                        }
                        else
                        {
                            index = READABLE_LETTERS.IndexOf(c);
                        }
                        if (index == -1)
                        {
                            throw new Exception(string.Format(Properties.Resources.InvalidTextInReadableNotation, readableNotationstring[i], figureShift, i));
                        }
                        b = (byte)index;
                        break;
                }

                bytes[i] = b;
            }
            return bytes;
        }

        /// <summary>
        /// Converts Baudot code to readable notation
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public string ToReadableNotation(byte[] bytes)
        {
            StringBuilder stringBuilder = new StringBuilder();
            bool figureShift = false;
            int offset = 0;

            foreach (byte b in bytes)
            {
                switch (b)
                {
                    case BYTE_FIGS:
                        stringBuilder.Append(READABLE_FIGS);
                        figureShift = true;
                        break;
                    case BYTE_LTRS:
                        stringBuilder.Append(READABLE_LTRS);
                        figureShift = false;
                        break;
                    case BYTE_CR:
                        stringBuilder.Append(READABLE_CR);
                        break;
                    case BYTE_LF:
                        stringBuilder.Append(READABLE_LF);
                        break;
                    case BYTE_SP:
                        stringBuilder.Append(READABLE_SP);
                        break;
                    case BYTE_NUL:
                        stringBuilder.Append(READABLE_NUL);
                        break;
                    default:
                        if (b > 0b00011111)
                        {
                            throw new Exception(string.Format(Properties.Resources.CannotConvertToReadableNotation, ToBinaryString(b), (char)b, offset));
                        }
                        if (figureShift)
                        {
                            stringBuilder.Append(FIGURES[b]);
                        }
                        else
                        {
                            stringBuilder.Append(READABLE_LETTERS[b]);
                        }
                        break;
                }
                offset++;
            }
            return stringBuilder.ToString();

        }

        /// <summary>
        /// Parse/create Baudot code with British notation
        /// </summary>
        /// <param name="britishNotationstring"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public byte[] FromBritishNotation(string britishNotationstring)
        {
            int len = britishNotationstring.Length;
            byte[] bytes = new byte[len];
            for (int i = 0; i < len; i++)
            {
                int index = BRITISH_LETTERS.IndexOf(britishNotationstring[i]);
                if (index == -1)
                {
                    throw new Exception(string.Format(Properties.Resources.InvalidTextInBritishNotation, britishNotationstring[i], i));
                }
                bytes[i] = (byte)index;
            }
            return bytes;
        }

        /// <summary>
        /// Converts Baudot code to British notation
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public string ToBritishNotation(byte[] bytes)
        {
            StringBuilder stringBuilder = new StringBuilder();
            int offset = 0;
            foreach (byte b in bytes)
            {
                if (b > 0b00011111)
                {
                    throw new Exception(string.Format(Properties.Resources.CannotConvertToBritishNotation, ToBinaryString(b), (char)b, offset));
                }
                stringBuilder.Append(BRITISH_LETTERS[b]);
                offset++;
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Converts a given byte to a binary string
        /// e.g. 17 -> 0010001
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        private static string ToBinaryString(byte b)
        {
            return Convert.ToString(b, 2).PadLeft(8, '0');
        }
    }
}
