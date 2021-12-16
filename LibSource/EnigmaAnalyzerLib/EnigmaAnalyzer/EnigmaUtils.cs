/*
   Copyright 2020 George Lasry
   Converted in 2020 from Java to C# by Nils Kopal

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
using System.IO;

namespace EnigmaAnalyzerLib
{
    public class EnigmaUtils
    {
        public static short getIndex(char c)
        {
            short val;
            if ((c >= 'a') && (c <= 'z'))
            {
                val = (short)(c - 'a');
            }
            else if ((c >= 'A') && (c <= 'Z'))
            {
                val = (short)(c - 'A');
            }
            else
            {
                val = -1;
            }
            return val;

        }

        public static int getDigitIndex(char c)
        {

            int val;
            if ((c >= '0') && (c <= '9'))
            {
                val = c - '0';
            }
            else
            {
                val = -1;
            }
            return val;

        }

        public static char getChar(int i)
        {
            if ((i >= 0) && (i <= 25))
            {
                return ((char)('A' + i));
            }
            else
            {
                return '?';
            }
        }

        public static string getstring(short[] crV, int clen)
        {

            string m = "";

            for (int i = 0; i < clen; i++)
            {
                m += getChar(crV[i]);
            }

            return m;
        }

        public static string getCiphertextstringInGroups(short[] crV, int clen)
        {
            string m = "";
            for (int p = 0; p < clen; p++)
            {
                if ((p % 5) == 0)
                {
                    m += " ";
                }
                m += getChar(crV[p]);
                if ((p % 25) == 24)
                {
                    m += "";
                }

            }
            return m;
        }


        public static string getCiphertextstringNoXJ(short[] crV, int clen)
        {
            string m = "";
            int X = getIndex('X');
            int J = getIndex('J');
            int Z = getIndex('Z');

            for (int i = 0; i < clen; i++)
            {
                int c = crV[i];
                if ((c == X) || (c == J))
                {
                    m += " ";
                }
                else if ((c == Z) && ((i + 1) < clen) && (crV[i + 1] == Z))
                {
                    m += "  ";
                    i++;
                }
                else
                {
                    m += getChar(c);
                }

            }

            return m;
        }

        public static int getText(string s, short[] crV)
        {

            int len = 0;
            for (int i = 0; (i < 1000) && (i < s.Length); i++)
            {
                crV[len++] = getIndex(s[i]);
            }
            return len;

        }

        public static string estimatedTimestring(long time)
        {
            DateTime dtime = DateTime.Now.AddSeconds(time);
            string timestring;
            if (time > 3600 * 24)
            {
                timestring = string.Format("{0} Days {1} Hours ({2}) ", (time / 3600) / 24, (time / 3600) % 24, dtime.ToString("kk:mm:ss"));
            }
            else if (time > 3600)
            {
                timestring = string.Format("{0} Hours ({1}) ", time / 3600.0, dtime.ToString("d / M kk: mm:ss"));
            }
            else if (time > 60)
            {
                timestring = string.Format("{0} Minutes", time / 60.0);
            }
            else
            {
                timestring = string.Format("{0} Seconds", time / 1.0);
            }
            return timestring;
        }

        public static string getEstimatedTimestring(long cases, int minRate, int maxRate)
        {
            long minSeconds = cases / maxRate;
            long maxSeconds = cases / minRate;

            if (maxSeconds < 10)
            {
                return "Less than 10 seconds";
            }
            string minS = estimatedTimestring(minSeconds);
            string maxS = estimatedTimestring(maxSeconds);
            return "" + minS + " - " + maxS;
        }

        public static int loadCipherText(string fileName, short[] text, bool print)
        {

            string line;
            int k = 0;

            int[] lc = new int[26];

            try
            {
                // FileStream reads text files in the default encoding.
                FileStream FileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);

                // Always wrap FileStream in StreamReader.
                StreamReader bufferedReader = new StreamReader(FileStream);

                while (((line = bufferedReader.ReadLine()) != null) && (k < Key.MAXLEN))
                {
                    for (int i = 0; (i < line.Length) && (k < Key.MAXLEN); i++)
                    {
                        short index = getIndex(line[i]);
                        if (index != -1)
                        {
                            text[k] = index;
                            k++;
                            lc[index]++;
                        }
                    }
                }
                // Always close files.
                bufferedReader.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine("Unable to read file {0} - {1}", fileName, ex.ToString());
            }

            if (print)
            {
                Console.WriteLine("Read file {0}", fileName);
            }

            return k;
        }

        public static int loadRandomText(string fileName, short[] randomText, int len, bool generateXs, int garbledLettersPercentage)
        {
            Random random = new Random();
            string file = "";
            string line;
            int BOOKSIZE = 50000;
            int[] text = new int[BOOKSIZE];
            int k = 0;

            try
            {
                // FileStream reads text files in the default encoding.
                FileStream FileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);

                // Always wrap FileStream in StreamReader.
                StreamReader bufferedReader = new StreamReader(FileStream);

                while (((line = bufferedReader.ReadLine()) != null) && (k < BOOKSIZE))
                {
                    file += line;
                    line = line + "X";
                    for (int i = 0; (i < line.Length) && (k < BOOKSIZE); i++)
                    {
                        if (line[i] == ' ')
                        {
                            continue;
                        }
                        int index = getIndex(line[i]);
                        if (index == -1)
                        {
                            index = getIndex('X');
                        }
                        if (index == getIndex('X'))
                        {
                            if (k == 0)
                            {
                                continue;
                            }
                            if (text[k - 1] == getIndex('X'))
                            {
                                continue;
                            }
                        }
                        text[k++] = index;
                    }
                }

                // Always close files.
                bufferedReader.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine("Failed to read {0} - {1}", fileName, ex.ToString());
            }

            if (len >= k)
            {
                Console.WriteLine("Input file too short to extract " + len + " characters " + fileName + "'");
                return 0;
            }

            int X = getIndex('X');

            int pos = random.Next(k - len - 1);

            int start = -1;
            for (int i = pos; i >= 0; i--)
            {
                if (i == 0)
                {
                    start = 0;
                    break;
                }
                else if (text[i] == X)
                {
                    start = i + 1;
                    break;
                }
            }
            int end = start + len;
            for (int i = k - 1; i > (start + len); i--)
            {
                if (text[i] == X)
                {
                    end = i;
                }
            }

            if ((end - start) < len / 2)
            {
                Console.WriteLine("Could not create a coherent message (from X to X, or from beginning of a line to end of another line) with " + len + " characters from " + fileName + "'");
            }

            int Length = end - start + 1;

            string cleanText = "";
            string garbledText = "";
            for (int i = 0; i < Length; i++)
            {

                int letter = text[start + i];
                cleanText += getChar(letter);
                if (garbledLettersPercentage > 0)
                {
                    int rand = random.Next(100);
                    if (rand < garbledLettersPercentage)
                    {
                        rand = random.Next(25);
                        letter = (letter + rand) % 26;
                        string garbledLetter = "" + getChar(letter);
                        garbledText += garbledLetter.ToLower();

                    }
                    else
                    {
                        garbledText += getChar(letter);
                    }
                }
                else
                {
                    garbledText += getChar(letter);
                }

                randomText[i] = (short)letter;

            }

            Console.WriteLine("Random text file extracted from {0} (length: {1}, start: {2}, end: {3})",
                    fileName, Length, start, end);
            if (garbledLettersPercentage > 0)
            {
                Console.WriteLine("Generated {0} percent of garbled letters. Clean Version:{1} With Garbles:{2}",
                        garbledLettersPercentage, cleanText, garbledText);
            }
            return Length;
        }

        public static void saveToFile(string fileName, string str)
        {

            try
            {
                // Assume default encoding.
                FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                // Always wrap FileStream in StreamWriter.
                StreamWriter streamWriter = new StreamWriter(fileStream);
                streamWriter.Write(str);
                streamWriter.WriteLine();
                streamWriter.Flush();
                streamWriter.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing file {0} - {0}", fileName, ex.ToString());
            }
        }

        public static bool isTextEncrypted(Key key, short[] text1, int clen, string indicatorS)
        {
            short[] text2 = new short[Key.MAXLEN];
            Key key2 = new Key(key);
            string indicPlainS;

            if (indicatorS.Length != 0)
            {
                short[] indicCiphertext = new short[3];
                short[] indicPlain = new short[3];

                int ilen = EnigmaUtils.getText(indicatorS, indicCiphertext);
                key.encipherDecipherAll(indicCiphertext, indicPlain, ilen);
                indicPlainS = EnigmaUtils.getstring(indicPlain, ilen);
                key2.setMesg(indicPlainS);
            }

            key2.encipherDecipherAll(text1, text2, clen);

            long score1 = 0, score2 = 0;
            for (int i = 0; i < clen; i++)
            {
                for (int j = 0; j < clen; j++)
                {
                    if (text1[i] == text1[j])
                    {
                        score1++;
                    }
                    if (text2[i] == text2[j])
                    {
                        score2++;
                    }
                }
            }
            return score1 < score2;
        }
    }
}