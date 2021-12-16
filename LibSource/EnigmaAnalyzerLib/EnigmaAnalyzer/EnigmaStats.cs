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
using System.Reflection;

namespace EnigmaAnalyzerLib
{
    public class EnigmaStats
    {
        public int[] triflat = new int[32 * 32 * 32];
        public int[] biflat = new int[32 * 32];
        public int[] unidict = { 609, 220, 72, 290, 1291, 303, 281, 188, 616, 41, 199, 390, 272, 841, 442, 147, 202, 687, 623, 541, 447, 138, 168, 698, 89, 205 };
        private double triMult = 0;
        private bool newTrigrams = false;

        public void unidictConvertToLog()
        {
            int minUni = int.MaxValue;

            for (int i = 0; i < 26; i++)
            {
                minUni = Math.Min(unidict[i], minUni);
            }

            for (int i = 0; i < 26; i++)
            {
                unidict[i] = (int)(10000.0 * Math.Log((Math.E * unidict[i]) / minUni));
            }
        }

        public int loadBidictConvertToLog(string fileName, bool print)
        {

            string line;
            int items = 0;


            for (int l1 = 0; l1 < 26; l1++)
            {

                for (int l2 = 0; l2 < 26; l2++)
                {
                    //bidict[l1][l2] =0;
                    biflat[(l1 << 5) + l2] = 0;

                }
            }

            try
            {
                // FileStream reads text files in the default encoding.
                FileStream fileReader = new FileStream(fileName, FileMode.Open, FileAccess.Read);

                // Always wrap FileStream in StreamReader.
                StreamReader bufferedReader = new StreamReader(fileReader);

                while (((line = bufferedReader.ReadLine()) != null) && (line.Length > 0) && (items < 400000))
                {
                    int freq = 0;
                    int l1 = EnigmaUtils.getIndex(line[0]);
                    int l2 = EnigmaUtils.getIndex(line[1]);
                    if ((l1 == -1) || (l2 == -1))
                    {
                        continue;
                    }

                    for (int i = 3; i < line.Length; i++)
                    {
                        freq = freq * 10 + EnigmaUtils.getDigitIndex(line[i]);
                    }

                    //bidict[l1][l2] = freq;
                    biflat[(l1 << 5) + l2] = freq;
                    items++;
                    //Console.WriteLine("Bigram {0} ({1}) {2} ({3}) = {4}",l1, line.charAt(0),l2, line.charAt(1),freq);
                }

                // Always close files.
                bufferedReader.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine("Unable to read bigram file {0} - {1}", fileName, ex.ToString());
            }

            if (print)
            {
                Console.WriteLine("Bigram file read: {0}, items = {1}, converted to log frequencies  ", fileName, items);
            }

            long minbi = 1000000000;
            for (int l1 = 0; l1 < 26; l1++)
            {
                for (int l2 = 0; l2 < 26; l2++)
                {
                    //long currbi= bidict[l1][l2];
                    long currbi = biflat[(l1 << 5) + l2];
                    if ((currbi != 0) && (currbi < minbi))
                    {
                        minbi = currbi;
                    }

                }
            }
            for (int l1 = 0; l1 < 26; l1++)
            {
                for (int l2 = 0; l2 < 26; l2++)
                {
                    long currbi = biflat[(l1 << 5) + l2];
                    if (currbi != 0)
                    {
                        biflat[(l1 << 5) + l2] = (int)(10000.0 * Math.Log((Math.E * currbi) / minbi));
                    }
                }
            }

            return items;

        }

        public int loadTridictConvertToLog(string fileName, bool print)
        {

            string line;
            int items = 0;

            for (int l1 = 0; l1 < 26; l1++)
            {
                for (int l2 = 0; l2 < 26; l2++)
                {
                    for (int l3 = 0; l3 < 26; l3++)
                    {
                        triflat[triIndex(l1, l2, l3)] = 0;
                    }
                }
            }

            try
            {
                // FileStream reads text files in the default encoding.
                FileStream fileReader = new FileStream(fileName, FileMode.Open, FileAccess.Read);

                // Always wrap FileStream in StreamReader.
                StreamReader bufferedReader = new StreamReader(fileReader);
                int count = 0;
                while (((line = bufferedReader.ReadLine()) != null) && (line.Length > 0) && (count++ < 20000000))
                {
                    int freq = 0;
                    int l1 = EnigmaUtils.getIndex(line[0]);
                    int l2 = EnigmaUtils.getIndex(line[1]);
                    int l3 = EnigmaUtils.getIndex(line[2]);
                    if ((l1 == -1) || (l2 == -1) || (l3 == -1))
                    {
                        continue;
                    }
                    for (int i = 4; i < line.Length; i++)
                    {
                        freq = freq * 10 + EnigmaUtils.getDigitIndex(line[i]);
                    }
                    triflat[triIndex(l1, l2, l3)] = freq;
                    items++;
                }

                // Always close files.
                bufferedReader.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine("Unable to read trigram file {0} - {1}", fileName, ex.ToString());
            }

            if (print)
            {
                Console.WriteLine("Trigram file read: {0}, items  = {1}, converted to log frequencies  ", fileName, items);
            }

            long mintri = 1000000000;
            for (int l1 = 0; l1 < 26; l1++)
            {
                for (int l2 = 0; l2 < 26; l2++)
                {
                    for (int l3 = 0; l3 < 26; l3++)
                    {
                        long currtri = triflat[triIndex(l1, l2, l3)];
                        if ((currtri != 0) && (currtri < mintri))
                        {
                            mintri = currtri;
                        }
                    }
                }
            }
            for (int l1 = 0; l1 < 26; l1++)
            {
                for (int l2 = 0; l2 < 26; l2++)
                {
                    for (int l3 = 0; l3 < 26; l3++)
                    {
                        long currtri = triflat[triIndex(l1, l2, l3)];
                        if (currtri != 0)
                        {
                            triflat[triIndex(l1, l2, l3)] = (int)(10000.0 * Math.Log((Math.E * currtri) / mintri));
                        }
                    }
                }
            }
            return items;

        }

        public int loadTridictFromResource(string resource)
        {

            //newTrigrams = fileName.EndsWith("3WH.txt");

            long minNonZero = long.MaxValue;

            string line;
            int items = 0;
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                StringReader stringReader = new StringReader(resource);

                while (((line = stringReader.ReadLine()) != null))
                {
                    int freq = 0;
                    int l1 = EnigmaUtils.getIndex(line[0]);
                    int l2 = EnigmaUtils.getIndex(line[1]);
                    int l3 = EnigmaUtils.getIndex(line[2]);
                    if ((l1 == -1) || (l2 == -1) || (l3 == -1))
                    {
                        continue;
                    }
                    for (int i = 4; i < line.Length; i++)
                    {
                        int dig = EnigmaUtils.getDigitIndex(line[i]);
                        if (dig != -1)
                        {
                            freq = freq * 10 + dig;
                        }
                    }
                    items++;
                    triflat[triIndex(l1, l2, l3)] = freq;
                    if ((freq > 0) && (freq < minNonZero))
                    {
                        minNonZero = freq;
                    }
                }
                // Always close files.
                stringReader.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine("Unable to read tridict from resources: {0}", ex.ToString());
            }

            if (minNonZero < 1000)
            {
                triMult = (newTrigrams ? 1500.0 : 1000.0) / minNonZero;
                //triMult = 1;
                for (int l1 = 0; l1 < 26; l1++)
                {
                    for (int l2 = 0; l2 < 26; l2++)
                    {
                        for (int l3 = 0; l3 < 26; l3++)
                        {
                            int freq = triflat[triIndex(l1, l2, l3)];
                            if (freq != 0)
                            {
                                freq = (int)(freq * triMult);
                                triflat[triIndex(l1, l2, l3)] = freq;
                            }
                        }
                    }
                }
            }
            return 1;
        }

        public int loadTridict(string fileName)
        {

            newTrigrams = fileName.EndsWith("3WH.txt");

            long minNonZero = long.MaxValue;

            string line;
            int items = 0;
            try
            {
                // FileStream reads text files in the default encoding.
                FileStream fileReader = new FileStream(fileName, FileMode.Open, FileAccess.Read);

                // Always wrap FileStream in StreamReader.
                StreamReader bufferedReader = new StreamReader(fileReader);
                while (((line = bufferedReader.ReadLine()) != null))
                {
                    int freq = 0;
                    int l1 = EnigmaUtils.getIndex(line[0]);
                    int l2 = EnigmaUtils.getIndex(line[1]);
                    int l3 = EnigmaUtils.getIndex(line[2]);
                    if ((l1 == -1) || (l2 == -1) || (l3 == -1))
                    {
                        continue;
                    }
                    for (int i = 4; i < line.Length; i++)
                    {
                        int dig = EnigmaUtils.getDigitIndex(line[i]);
                        if (dig != -1)
                        {
                            freq = freq * 10 + dig;
                        }
                    }
                    items++;
                    triflat[triIndex(l1, l2, l3)] = freq;
                    if ((freq > 0) && (freq < minNonZero))
                    {
                        minNonZero = freq;
                    }
                }
                // Always close files.
                bufferedReader.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine("Unable to read trigram file {0} - {0}", fileName, ex.ToString());
            }

            if (minNonZero < 1000)
            {
                triMult = (newTrigrams ? 1500.0 : 1000.0) / minNonZero;
                //triMult = 1;
                for (int l1 = 0; l1 < 26; l1++)
                {
                    for (int l2 = 0; l2 < 26; l2++)
                    {
                        for (int l3 = 0; l3 < 26; l3++)
                        {
                            int freq = triflat[triIndex(l1, l2, l3)];
                            if (freq != 0)
                            {
                                freq = (int)(freq * triMult);
                                triflat[triIndex(l1, l2, l3)] = freq;
                            }
                        }
                    }
                }
            }
            Console.WriteLine("Trigram file read: {0}  ({1} items)", fileName, items);
            return 1;
        }

        public double triSchwelle(double length)
        {
            if (newTrigrams)
            {
                if (length <= 50)
                {
                    return 13000;
                }
                if (length <= 75)
                {
                    return 11000;
                }
                if (length <= 100)
                {
                    return 11000;
                }
                return 10000;
            }

            return 10000;
        }

        private int triIndex(int l1, int l2, int l3)
        {
            return (((l1 << 5) + l2) << 5) + l3;
        }

        public int loadBidict(string fileName)
        {
            long minNonZero = long.MaxValue;
            string line;
            try
            {
                // FileStream reads text files in the default encoding.
                FileStream fileReader = new FileStream(fileName, FileMode.Open, FileAccess.Read);

                // Always wrap FileStream in StreamReader.
                StreamReader bufferedReader = new StreamReader(fileReader);
                while ((line = bufferedReader.ReadLine()) != null)
                {
                    int freq = 0;
                    int l1 = EnigmaUtils.getIndex(line[0]);
                    int l2 = EnigmaUtils.getIndex(line[1]);
                    if ((l1 == -1) || (l2 == -1))
                    {
                        continue;
                    }
                    for (int i = 3; i < line.Length; i++)
                    {
                        int dig = EnigmaUtils.getDigitIndex(line[i]);
                        if (dig != -1)
                        {
                            freq = freq * 10 + dig;
                        }
                    }
                    //bidict[l1][l2] = freq;
                    biflat[(l1 << 5) + l2] = freq;
                    if ((freq > 0) && (freq < minNonZero))
                    {
                        minNonZero = freq;
                    }
                }
                // Always close files.
                bufferedReader.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine("Unable to read bigram file {0} - {1}", fileName, ex.ToString());
            }

            if (minNonZero < 1000)
            {
                for (int l1 = 0; l1 < 26; l1++)
                {
                    for (int l2 = 0; l2 < 26; l2++)
                    {
                        long freq = biflat[(l1 << 5) + l2];
                        if (freq != 0)
                        {
                            if (freq == minNonZero)
                            {
                                freq = 1000;
                            }
                            else
                            {
                                freq = (freq * 1000) / minNonZero;
                            }
                        }  //do nothing
                        biflat[(l1 << 5) + l2] = (int)freq;
                    }
                }
            }
            Console.WriteLine("Bigram file read: {0}  ", fileName);
            return 1;
        }

        public int loadBidictFromResources(string resource)
        {
            long minNonZero = long.MaxValue;
            string line;
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                StringReader stringReader = new StringReader(resource);

                while ((line = stringReader.ReadLine()) != null)
                {
                    int freq = 0;
                    int l1 = EnigmaUtils.getIndex(line[0]);
                    int l2 = EnigmaUtils.getIndex(line[1]);
                    if ((l1 == -1) || (l2 == -1))
                    {
                        continue;
                    }
                    for (int i = 3; i < line.Length; i++)
                    {
                        int dig = EnigmaUtils.getDigitIndex(line[i]);
                        if (dig != -1)
                        {
                            freq = freq * 10 + dig;
                        }
                    }
                    //bidict[l1][l2] = freq;
                    biflat[(l1 << 5) + l2] = freq;
                    if ((freq > 0) && (freq < minNonZero))
                    {
                        minNonZero = freq;
                    }
                }
                // Always close files.
                stringReader.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine("Unable to read bidict from resources: {0}", ex.ToString());
            }

            if (minNonZero < 1000)
            {
                for (int l1 = 0; l1 < 26; l1++)
                {
                    for (int l2 = 0; l2 < 26; l2++)
                    {
                        long freq = biflat[(l1 << 5) + l2];
                        if (freq != 0)
                        {
                            if (freq == minNonZero)
                            {
                                freq = 1000;
                            }
                            else
                            {
                                freq = (freq * 1000) / minNonZero;
                            }
                        }  //do nothing
                        biflat[(l1 << 5) + l2] = (int)freq;
                    }
                }
            }
            return 1;
        }

    }
}