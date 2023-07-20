using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace M209AnalyzerLib.Common
{
    public class Octagrams
    {
        public static readonly int SIZE_4_GRAM = 26 * 26 * 26 * 26;
        public static readonly long VALID_4_GRAMS = 122650;
        public static int[] array4 = new int[SIZE_4_GRAM];
        public static byte[][] array2;
        static readonly int SUB_ARRAY_SIZE = (int)Math.Pow(2, 30);
        static readonly int CHUNK_SIZE = (int)Math.Pow(2, 18);

        public static bool read2(String filename)
        {

            long start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            array2 = new byte[16][];
            for (int i = 0; i < array2.Length; i++)
            {
                array2[i] = new byte[SUB_ARRAY_SIZE];
            }
            //if (1==1) return false ;

            byte[] bytes = new byte[CHUNK_SIZE];
            long totalRead = 0;

            try
            {

                int read;
                var byteArray = File.ReadAllBytes(filename);
                for (int i = 0; i < byteArray.Length; i++)
                {
                    read = byteArray[i];
                    if (read <= 0) break;

                    int arrayIndex = (int)(totalRead / SUB_ARRAY_SIZE);
                    int indexInArray = (int)(totalRead % SUB_ARRAY_SIZE);
                    Array.Copy(bytes, 0, array2[arrayIndex], indexInArray, read);
                    totalRead += read;
                    if (totalRead % (CHUNK_SIZE * 1024) == 0)
                    {
                        Console.WriteLine(".");
                    }
                }
                Console.WriteLine();

            }
            catch (IOException ex)
            {
                Console.WriteLine($"Unable to read  file {filename} - {ex.ToString()}");
            }
            Console.WriteLine($"Octagram stats file {filename} loaded successfully ({(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - start) / 1_000.0} seconds)," +
                $" size = {totalRead} bytes ({new ComputerInfo().AvailablePhysicalMemory} free bytes after loading)\n");

            return true;
        }


        private static void read4(String fileName)
        {

            long max = 0;
            try
            {
                List<string> lines = File.ReadAllLines(fileName).ToList();

                String line = "";

                int count = 0;
                int countNonZero = 0;

                for (int i = 0; i < lines.Count; i++)
                {
                    line = lines[i];

                    String[] split = line.Split(' ');
                    foreach (String item in split)
                    {
                        int index = int.Parse(item);
                        if (index > 0)
                        {
                            array4[count] = index;

                            countNonZero++;
                            max = Math.Max(max, index);
                        }
                        count++;
                    }
                    Console.WriteLine($"{line.Length} {split.Length} {countNonZero} {max}\n");
                }
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Error reading file '{fileName}' ");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error reading file '{fileName}' ");
            }
        }


        public static long eval(int[] plaintext, int plaintextLength)
        {

            Stats.Evaluations++;

            if (plaintextLength < 8)
            {
                return 0;
            }
            // access to arrays.
            int l1 = plaintext[0];
            int l2 = plaintext[1];
            int l3 = plaintext[2];
            int l4 = plaintext[3];
            int l5 = plaintext[4];
            int l6 = plaintext[5];
            int l7 = plaintext[6];
            int l8;
            long sum = 0;
            int q1;
            int q2;
            long index1;
            long index2;
            long index;
            int arrayIndex;
            int indexInArray;
            int val = 0;
            for (int i = 7; i < plaintextLength; i++)
            {

                l8 = plaintext[i];
                q1 = ((l1 * 26 + l2) * 26 + l3) * 26 + l4;
                index1 = array4[q1];
                if (index1 > 0)
                {
                    q2 = ((l5 * 26 + l6) * 26 + l7) * 26 + l8;
                    index2 = array4[q2];
                    if (index2 > 0)
                    {
                        index = index1 * (VALID_4_GRAMS + 1) + index2;
                        arrayIndex = (int)(index / SUB_ARRAY_SIZE);
                        indexInArray = (int)(index % SUB_ARRAY_SIZE);
                        //System.out.printf("i: %,3d q1: %,7d q2: %,7d index1: %,7d index2:%,7d index: %,15d arrayIndex: %,1d indexArray: %,12d val: %,3d\n", i, q1, q2, index1, index2, index, arrayIndex, indexInArray, val);
                        val = array2[arrayIndex][indexInArray];
                        if (val < 0)
                        {
                            val += 256;
                        }
                        sum += val;
                    }
                }

                l1 = l2;
                l2 = l3;
                l3 = l4;
                l4 = l5;
                l5 = l6;
                l6 = l7;
                l7 = l8;

            }

            return 20 * 1000 * sum / (plaintextLength - 7);


        }

        public static void main(String[] a)
        {


            load8grams();
            int[] v1 = Utils.GetText("ANYTHINGISVALID");
            Console.WriteLine($"{v1} {eval(v1, v1.Length)} \n");
            int[] v2 = Utils.GetText("YGHUASDM");
            Console.WriteLine($"{v2} {eval(v2, v2.Length)} \n");

        }

        public static void load8grams()
        {
            read4("D:\\AZDecrypt\\AZdecrypt 1.16\\array4.txt");
            read2("D:\\AZDecrypt\\AZdecrypt 1.16\\array2.txt");
        }
    }
}
