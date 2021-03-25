using System;
using System.Collections.Generic;
using System.Linq;

namespace SigabaKnownPlaintext{
    public class SigabaCoreFastPhaseI
    {

        public int sc = 14; //stamp challenge
        public int sc2 = 14; // stamp challenge

        public RotorByte[] ControlRotors;
        public RotorByte[] CipherRotors;
        public RotorByte[] IndexRotors;
        public byte[] IndexMaze = new byte[26];
        public RotorByte[] CodeWheels;

        private int[][] indexMaze;

        RotorByte[] CodeWheels2 = new RotorByte[5];
        int[] realWheels = new int[5];
        string[] keys;

        public SigabaCoreFastPhaseI()
        {
            InitializeRotors();
            indexMaze = control();
        }

        public byte Cipher(byte c)
        {
            return CipherRotors.Aggregate(c, (current, rotor) => rotor.DeCiph(current));
        }

        private int counter = 0;
        
        public void setCodeWheels(int[] phase1Wheels)
        {
            int count = 0; 
            for(int ccr = 1; ccr< ConstantsByte.ControlCipherRotors.Length;ccr++)
            {
                if(!phase1Wheels.Contains(ccr))
                {
                    CodeWheels2[count] = new RotorByte(ConstantsByte.ControlCipherRotors[ccr], 0, false);
                    realWheels[count] = ccr;
                    count++;
                }
            }

           //this.keys = keys;
        }

        private int[][] control()
        {
            int[][] ret = new int[113400][];
            int i = 0;
            int[] arr = new int[] { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4 };
            do
            {

                ret[i] = new int[26];
                for (int x = 0; x < 26;x++ )
                {
                    ret[i][x] = arr[ConstantsByte.Transform[0][x]];
                }
                i++;
            } while (NextPermutation(arr));
            return ret;
        }

        public bool stepOneCompact(int[][] input2, byte[] cipher, int[] types)
        {
            int[] arr = new[] {0, 1, 2, 3, 4};

            

            List<object[]> retob = new List<object[]>();

            do
            {
                ControlRotors[0] = CodeWheels2[arr[0]];
                ControlRotors[1] = CodeWheels2[arr[1]];
                ControlRotors[2] = CodeWheels2[arr[2]];
                ControlRotors[3] = CodeWheels2[arr[3]];
                ControlRotors[4] = CodeWheels2[arr[4]];
                
                for (int ix = 0; ix < 32; ix++)
                {
                    String s = GetIntBinaryString(ix);
                    if (s[0] == '1')
                    {
                        ControlRotors[0].Reverse = true;
                    }
                    else
                    {
                        ControlRotors[0].Reverse = false;
                    }
                    if (s[1] == '1')
                    {
                        ControlRotors[1].Reverse = true;
                    }
                    else
                    {
                        ControlRotors[1].Reverse = false;
                    }
                    if (s[2] == '1')
                    {
                        ControlRotors[2].Reverse = true;
                    }
                    else
                    {
                        ControlRotors[2].Reverse = false;
                    }
                    if (s[3] == '1')
                    {
                        ControlRotors[3].Reverse = true;
                    }
                    else
                    {
                        ControlRotors[3].Reverse = false;
                    }
                    if (s[4] == '1')
                    {
                        ControlRotors[4].Reverse = true;
                    }
                    else
                    {
                        ControlRotors[4].Reverse = false;
                    }

                    ControlRotors[0].Position = 21;
                    ControlRotors[1].Position = 22;
                    ControlRotors[2].Position = 23;
                    ControlRotors[3].Position = 24;
                    ControlRotors[4].Position = 25;

                    sc = (ControlRotors[2].Position - 12 + 26)%26;
                    sc2 = (ControlRotors[1].Position - 12 + 26)%26;

                    int[][] tmplst = new int[input2.Length][];

                    Boolean halt = false;
                    int[][] pseudo = new int[26][];
                    for (int letters = 0; letters < input2.Length; letters++)
                    {
                        int tempf = 5;
                        int tempg = 6;
                        int temph = 7;
                        int tempi = 8;

                        for (int i = 0; i < 5; i++)
                        {


                            //converted to:
                            if (ControlRotors[i].Reverse)
                            {
                                tempf =
                                    ControlRotors[i].RotSubMat[ControlRotors[i].Position, tempf];
                                tempg =
                                    ControlRotors[i].RotSubMat[ControlRotors[i].Position, tempg];
                                temph =
                                    ControlRotors[i].RotSubMat[ControlRotors[i].Position, temph];
                                tempi =
                                    ControlRotors[i].RotSubMat[ControlRotors[i].Position, tempi];
                            }
                            else
                            {
                                tempf = ControlRotors[i].RotSubMatBack[ControlRotors[i].Position, tempf];
                                tempg = ControlRotors[i].RotSubMatBack[ControlRotors[i].Position, tempg];
                                temph = ControlRotors[i].RotSubMatBack[ControlRotors[i].Position, temph];
                                tempi = ControlRotors[i].RotSubMatBack[ControlRotors[i].Position, tempi];
                            }
                        }

                        if (pseudo[tempf] == null)
                            pseudo[tempf] = input2[letters];
                        else
                        {
                            pseudo[tempf] = pseudo[tempf].Intersect(input2[letters]).ToArray();
                            if (pseudo[tempf].Count() == 0)
                            {
                               // Console.WriteLine("bingo");
                                halt = true;
                                break;
                            }
                        }
                        if (pseudo[tempg] == null)
                            pseudo[tempg] = input2[letters];
                        else
                        {
                            pseudo[tempg] = pseudo[tempg].Intersect(input2[letters]).ToArray();
                            if (pseudo[tempg].Count() == 0)
                            {
                                //Console.WriteLine("bingo");
                                halt = true;
                                break;
                            }
                        }
                        if (pseudo[temph] == null)
                            pseudo[temph] = input2[letters];
                        else
                        {
                            pseudo[temph] = pseudo[temph].Intersect(input2[letters]).ToArray();
                            if (pseudo[temph].Count() == 0)
                            {
                                //Console.WriteLine("bingo");
                                halt = true;
                                break;
                            }
                        }
                        if (pseudo[tempi] == null)
                            pseudo[tempi] = input2[letters];
                        else
                        {
                            pseudo[tempi] = pseudo[tempi].Intersect(input2[letters]).ToArray();
                            if (pseudo[tempi].Count() == 0)
                            {
                                //Console.WriteLine("bingo");
                                halt = true;
                                break;
                            }
                        }

                        tmplst[letters] = new int[] { tempf, tempg, temph, tempi };

                        if (ControlRotors[2].Position == sc)
                        {
                            if (ControlRotors[1].Position == sc2)
                            {
                                ControlRotors[3].IncrementPosition();
                            }
                            ControlRotors[1].IncrementPosition();
                        }
                        ControlRotors[2].IncrementPosition();
                    }

                    if(!halt)
                    for (int indM = 0; indM < 113400; indM++)
                    {
                        for (int letters = 0; letters < tmplst.Length; letters++)
                        {
                            int[] tmp = new int[]
                                            {
                                                indexMaze[indM][tmplst[letters][0]], indexMaze[indM][tmplst[letters][1]],
                                                indexMaze[indM][tmplst[letters][2]], indexMaze[indM][tmplst[letters][3]]
                                            };

                            if (input2[letters].Except(tmp).Count() != 0 || tmp.Except(input2[letters]).Count() != 0)
                            {
                                break;
                            }

                            if (letters == input2.Length - 2)
                            {

                                Console.WriteLine("We've got a winner: " + "IndexMaze Index:" + indM + "  Rotors:  " +
                                                  realWheels[arr[0]] + ControlRotors[0].Reverse + "  " + realWheels[arr[1]] + ControlRotors[1].Reverse + "  " +
                                                  realWheels[arr[2]] + ControlRotors[2].Reverse + "  " + realWheels[arr[3]] + ControlRotors[3].Reverse + "  " +
                                                  realWheels[arr[4]]+ ControlRotors[4].Reverse);
                                object[] retobtmp = new object[3];

                                ControlRotors[0].Position = 21;
                                ControlRotors[1].Position = 22;
                                ControlRotors[2].Position = 23;
                                ControlRotors[3].Position = 24;
                                ControlRotors[4].Position = 25;
                                byte[] positions = new byte[5] {0, 1, 2, 3, 4};
                                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                                Console.WriteLine(enc.GetString(Encrypt(cipher, types, positions, indM)));
                                



                                /*retobtmp[0] = indM;
                                    retobtmp[1] = new int[] { realWheels[arr[0]] ,realWheels[arr[1]] ,realWheels[arr[2]] ,realWheels[arr[3]] ,realWheels[arr[4]] };
                                    retobtmp[2] = new[] { ControlRotors[0].Reverse, ControlRotors[1].Reverse, ControlRotors[2].Reverse, ControlRotors[3].Reverse, ControlRotors[4].Reverse};
                                    retob.Add(retobtmp);*/

                            }
                        }
                    }



                  }


                }
                while (NextPermutation(arr)) ;
                return true;
            }

        public List<int[][]> PhaseI3(byte[] cipher, byte[] crib, int[] types, byte[] positions)
        {
            setInternalConfig(types, positions);

            int[] foo = new int[5] { 0, 1, 2, 3, 4 };

            counter = 0;

            int[][] posarr = new int[crib.Length][];

            List<int[][]> retlst = new List<int[][]>();

            recursiveMethod(cipher, crib, foo, types, positions, posarr, 0, retlst);

            //Console.WriteLine(counter);
            return retlst;
        }

        private int[][] recursiveMethod(byte[] cipher, byte[] crib, int[] foo, int[] types, byte[] positions, int[][] posarr, int i, List<int[][]> retlst)
        {
            setInternalConfig(types, positions);

            byte b = (byte)(Cipher((byte)(cipher[i] - 65)) + 65);

            if (b == crib[i])
            {
                for (int x = 1; x < 5; x++)
                {
                    IEnumerable<IEnumerable<int>> combis = PermutationFactory.Combinations(foo, x);
                    foreach (IEnumerable<int> enumerable in combis)
                    {
                        int[] arr = enumerable.ToArray();

                        /*foreach (int i1 in arr)
                        {
                            CipherRotors[i1].IncrementPosition();
                        }*/
                        byte[] positions2 = (byte[])positions.Clone();

                        int[][] posarr2 = (int[][])posarr.Clone();
                        posarr2[i] = arr;

                        foreach (int i1 in arr)
                        {
                            if (!CipherRotors[4 - i1].Reverse)
                                positions2[i1] = (byte)(((positions[i1] - 1) + 26) % 26);
                            else
                                positions2[i1] = (byte)((positions[i1] + 1) % 26);
                        }
                        if (i < crib.Length - 1)
                            recursiveMethod(cipher, crib, foo, types, positions2, posarr2, i + 1, retlst);
                        else
                        {
                            posarr[i] = arr;
                        }
                    }

                }
            }
            /*
            if (i > 1)
            {
                for (int ix = 0; ix < i; ix++)
                {
                    Console.Write((char)crib[ix]);
                }
                Console.WriteLine((char) b ); 
            }
            */
            if (b == crib.Last() && i == crib.Length - 1)
            {
                /*foreach (byte b1 in positions)
                {
                    Console.Write((char)(b1+65) +"  ");
                }*/
                //Console.WriteLine("we've got a winner");
                counter++;
                retlst.Add(posarr);
                return null;
            }

            else
            {
                return null;
            }
        }

        public byte[] Encrypt(byte[] cipher, int[] types, byte[] positions,int indM)
        {
            byte[] repeat = new byte[cipher.Length];

            setInternalConfig(types, positions);

            var incremented = new bool[CipherRotors.Length];

            for (int ix = 0; ix < cipher.Length; ix++)
            {
                //StringBuilder s = new StringBuilder();

                //repeat = String.Concat(repeat, (char)(Cipher(c - 65) + 65) + "");

                repeat[ix] = (byte)(Cipher((byte)(cipher[ix] - 65)) + 65);

                //Set all incremented booleans to false
                for (int i = 0; i < incremented.Length; i++)
                {
                    incremented[i] = false;
                }
                foreach (int i in Control(indM))
                {
                    if (incremented[i] == false)
                    {
                        CipherRotors[4 - i].IncrementPosition();
                        incremented[i] = true; // we incremented this rotor in this run
                    }
                }

                if (ControlRotors[2].Position == sc) // stamp challenge
                {
                    if (ControlRotors[1].Position == sc2) // stamp challenge
                    {
                        ControlRotors[3].IncrementPosition();
                    }
                    ControlRotors[1].IncrementPosition();
                }
                ControlRotors[2].IncrementPosition();
            }


            return repeat;
        }

        public byte[] PseudoEncrypt(byte[] cipher, int[] types, byte[] positions)
        {
            byte[] repeat = new byte[cipher.Length];

            setInternalConfig(types, positions);

            var incremented = new bool[CipherRotors.Length];

            for (int ix = 0; ix < cipher.Length; ix++)
            {
                //StringBuilder s = new StringBuilder();

                //repeat = String.Concat(repeat, (char)(Cipher(c - 65) + 65) + "");

                repeat[ix] = (byte)(Cipher((byte)(cipher[ix] - 65)) + 65);

                //Set all incremented booleans to false
                for (int i = 0; i < incremented.Length; i++)
                {
                    incremented[i] = false;
                }


                /*foreach (int i in Control(indM))
                {
                    if (incremented[i] == false)
                    {
                        CipherRotors[4 - i].IncrementPosition();
                        incremented[i] = true; // we incremented this rotor in this run
                    }
                }*/



                if (ControlRotors[2].Position == 14)
                {
                    if (ControlRotors[1].Position == 14)
                    {
                        ControlRotors[3].IncrementPosition();
                    }
                    ControlRotors[1].IncrementPosition();
                }
                ControlRotors[2].IncrementPosition();
            }


            return repeat;
        }

        public int[] PseudoControl()
        {
            byte tempf = 5;
            byte tempg = 6;
            byte temph = 7;
            byte tempi = 8;

            for (int i = 0; i < 4; i++)
            {
                //tempf = ControlRotors[i].DeCiph(tempf);
                //tempg = ControlRotors[i].DeCiph(tempg);
                //temph = ControlRotors[i].DeCiph(temph);
                //tempi = ControlRotors[i].DeCiph(tempi);
                //converted to:
                if (ControlRotors[i].Reverse)
                {
                    tempf = ControlRotors[i].RotSubMat[ControlRotors[i].Position, tempf]; //stamp Challenge
                    tempg = ControlRotors[i].RotSubMat[ControlRotors[i].Position, tempg]; //stamp Challenge
                    temph = ControlRotors[i].RotSubMat[ControlRotors[i].Position, temph]; //stamp Challenge
                    tempi = ControlRotors[i].RotSubMat[ControlRotors[i].Position, tempi]; //stamp Challenge
                }
                else
                {
                    tempf = ControlRotors[i].RotSubMatBack[ControlRotors[i].Position, tempf];
                    tempg = ControlRotors[i].RotSubMatBack[ControlRotors[i].Position, tempg];
                    temph = ControlRotors[i].RotSubMatBack[ControlRotors[i].Position, temph];
                    tempi = ControlRotors[i].RotSubMatBack[ControlRotors[i].Position, tempi];
                }

            }
            return new int[] { IndexMaze[tempf], IndexMaze[tempg], IndexMaze[temph], IndexMaze[tempi] };
        }

        public int[] Control(int indM)
        {
            //TODO: Control Rotor 1 kann abgekürzt werden, ein loojup zu viel
            byte tempf = 5;
            byte tempg = 6;
            byte temph = 7;
            byte tempi = 8;

            for (int i = 0; i < 5; i++)
            {
                //tempf = ControlRotors[i].DeCiph(tempf);
                //tempg = ControlRotors[i].DeCiph(tempg);
                //temph = ControlRotors[i].DeCiph(temph);
                //tempi = ControlRotors[i].DeCiph(tempi);
                //converted to:
                if (ControlRotors[i].Reverse)
                {
                    tempf = ControlRotors[i].RotSubMat[ControlRotors[i].Position, tempf];//stamp
                    tempg = ControlRotors[i].RotSubMat[ControlRotors[i].Position, tempg];//stamp
                    temph = ControlRotors[i].RotSubMat[ControlRotors[i].Position, temph];//stamp
                    tempi = ControlRotors[i].RotSubMat[ControlRotors[i].Position, tempi];//stamp
                }
                else
                {
                    tempf = ControlRotors[i].RotSubMatBack[ControlRotors[i].Position, tempf];
                    tempg = ControlRotors[i].RotSubMatBack[ControlRotors[i].Position, tempg];
                    temph = ControlRotors[i].RotSubMatBack[ControlRotors[i].Position, temph];
                    tempi = ControlRotors[i].RotSubMatBack[ControlRotors[i].Position, tempi];
                }

            }
            return new int[] { indexMaze[indM][tempf], indexMaze[indM][tempg], indexMaze[indM][temph], indexMaze[indM][tempi] };
        }

        public void setCipherRotors(int i, byte a)
        {

            CipherRotors[4 - i] = CodeWheels[a];

        }

        public void setControlRotors(byte i, byte b)
        {

            ControlRotors[i - 5] = CodeWheels[b];

        }

        public void setIndexRotors(byte i, byte c)
        {

            IndexRotors[i] = CodeWheels[c + 10];

        }

        public void setBool(byte ix, byte i, bool rev)
        {

            if (i > 4)
            {
                ControlRotors[i - 5].Reverse = rev;
            }
            else
            {
                CipherRotors[4 - i].Reverse = rev;
            }

            CodeWheels[ix].Reverse = rev;

        }

        public void setPositionsControl(byte ix, byte i, byte position)
        {
            ControlRotors[i - 5].Position = position;
            CodeWheels[ix].Position = position;
        }

        public void setPositionsIndex(byte ix, byte i, byte position)
        {
            IndexRotors[i].Position = position;
            CodeWheels[ix + 10].Position = position;

        }

        public void setInternalConfig(int[] a, byte[] positions)
        {
            for (int i = 0; i < a.Length; i++)
            {
                CodeWheels[a[i]].Position = positions[i];
            }
        }

        public void setIndexMaze()
        {

            for (byte i = 0; i < 26; i++)
            {
                byte tempf = i;

                //tempf = ControlRotors[4].DeCiph(tempf);
                //converted to:
                if (ControlRotors[4].Reverse)
                {
                    tempf = ControlRotors[4].RotSubMatRevBack[ControlRotors[4].Position, tempf];
                }
                else
                {
                    tempf = ControlRotors[4].RotSubMatBack[ControlRotors[4].Position, tempf];
                }

                tempf = ConstantsByte.Transform[0][tempf];

                foreach (var rotor in IndexRotors)
                {
                    if (tempf != -1)
                    {
                        //tempf = rotor.Ciph(tempf);
                        //converted to

                        if (rotor.Reverse)
                        {
                            tempf = rotor.RotSubMatRev[rotor.Position, tempf];
                        }
                        else
                        {
                            tempf = rotor.RotSubMat[rotor.Position, tempf];
                        }
                    }
                }

                IndexMaze[i] = ConstantsByte.Transform2[tempf];
            }
        }

        public void setIndexMaze(int[] steppingmaze)
        {

            for (byte i = 0; i < 26; i++)
            {
                byte tempf = i;

                //tempf = ControlRotors[4].DeCiph(tempf);
                //converted to:
                if (ControlRotors[4].Reverse)
                {
                    tempf = ControlRotors[4].RotSubMatRevBack[ControlRotors[4].Position, tempf];
                }
                else
                {
                    tempf = ControlRotors[4].RotSubMatBack[ControlRotors[4].Position, tempf];
                }

                tempf = ConstantsByte.Transform[0][tempf];

                IndexMaze[i] = (byte)steppingmaze[(int)tempf];
            }
        }

        public void InitializeRotors()
        {
            CodeWheels = new RotorByte[16];

            CipherRotors = new RotorByte[5];
            ControlRotors = new RotorByte[5];
            IndexRotors = new RotorByte[5];
            
            CodeWheels[0] = new RotorByte(ConstantsByte.ControlCipherRotors[0], 0, false);
            CodeWheels[1] = CipherRotors[0] = new RotorByte(ConstantsByte.ControlCipherRotors[1], 0, false);
            CodeWheels[2] = CipherRotors[1] = new RotorByte(ConstantsByte.ControlCipherRotors[2], 0, false);
            CodeWheels[3] = CipherRotors[2] = new RotorByte(ConstantsByte.ControlCipherRotors[3], 0, false);
            CodeWheels[4] = CipherRotors[3] = new RotorByte(ConstantsByte.ControlCipherRotors[4], 0, false);
            CodeWheels[5] = CipherRotors[4] = new RotorByte(ConstantsByte.ControlCipherRotors[5], 0, false);

            CodeWheels[6]  = new RotorByte(ConstantsByte.ControlCipherRotors[6], 0, false);
            CodeWheels[7] = new RotorByte(ConstantsByte.ControlCipherRotors[7], 0, false);
            CodeWheels[8] = new RotorByte(ConstantsByte.ControlCipherRotors[8], 0, false);
            CodeWheels[9] = new RotorByte(ConstantsByte.ControlCipherRotors[9], 0, false);
            CodeWheels[10] = new RotorByte(ConstantsByte.ControlCipherRotors[10], 0, false);
            /*
            CodeWheels[11] = IndexRotors[0] = new RotorByte(ConstantsByte.IndexRotors[1], 0, false);
            CodeWheels[12] = IndexRotors[1] = new RotorByte(ConstantsByte.IndexRotors[2], 0, false);
            CodeWheels[13] = IndexRotors[2] = new RotorByte(ConstantsByte.IndexRotors[3], 0, false);
            CodeWheels[14] = IndexRotors[3] = new RotorByte(ConstantsByte.IndexRotors[4], 0, false);
            CodeWheels[15] = IndexRotors[4] = new RotorByte(ConstantsByte.IndexRotors[5], 0, false);*/
        }

        public static string GetIntBinaryString(int n)
        {
            var b = new char[5];
            int pos = 4;
            int i = 0;

            while (i < 5)
            {
                if ((n & (1 << i)) != 0)
                {
                    b[pos] = '1';
                }
                else
                {
                    b[pos] = '0';
                }
                pos--;
                i++;
            }
            return new string(b);
        }

        private static bool NextPermutation(int[] numList)
        {
            /*
             Knuths
             1. Find the largest index j such that a[j] < a[j + 1]. If no such index exists, the permutation is the last permutation.
             2. Find the largest index l such that a[j] < a[l]. Since j + 1 is such an index, l is well defined and satisfies j < l.
             3. Swap a[j] with a[l].
             4. Reverse the sequence from a[j + 1] up to and including the final element a[n].

             */
            var largestIndex = -1;
            for (var i = numList.Length - 2; i >= 0; i--)
            {
                if (numList[i] < numList[i + 1])
                {
                    largestIndex = i;
                    break;
                }
            }

            if (largestIndex < 0) return false;

            var largestIndex2 = -1;
            for (var i = numList.Length - 1; i >= 0; i--)
            {
                if (numList[largestIndex] < numList[i])
                {
                    largestIndex2 = i;
                    break;
                }
            }

            var tmp = numList[largestIndex];
            numList[largestIndex] = numList[largestIndex2];
            numList[largestIndex2] = tmp;

            for (int i = largestIndex + 1, j = numList.Length - 1; i < j; i++, j--)
            {
                tmp = numList[i];
                numList[i] = numList[j];
                numList[j] = tmp;
            }

            return true;
        }
    }

  

    public static class PermutationFactory
    {
        // combinationgenerator like http://stackoverflow.com/questions/127704/algorithm-to-return-all-combinations-of-k-elements-from-n
        public static IEnumerable<IEnumerable<T>> Combinations<T>(this IEnumerable<T> elements, int k)
        {
            return k == 0 ? new[] { new T[0] } :
              elements.SelectMany((e, i) =>
                elements.Skip(i + 1).Combinations(k - 1).Select(c => (new[] { e }).Concat(c)));
        }

    }
}

/*    public int PhaseI(byte[] cipher, byte[] crib, int[] types, byte[] positions)
      {

          //byte[] repeat = new byte[cipher.Length];

          setInternalConfig(types, positions);

          int[] foo = new int[5] {0, 1, 2, 3, 4 };

         foreach (int type in types)
          {
              Console.Write(type);
          }
          Console.WriteLine();

          counter = 0;

          recursiveShit(cipher, crib, foo, types, positions,0);
          //Console.WriteLine(counter);
          return counter;
      }

      private bool recursiveShit(byte[] cipher, byte[] crib, int[] foo,int[] types, byte[] positions, int i)
      {
          setInternalConfig(types, positions);

          byte b = (byte) (Cipher((byte) (cipher[i] - 65)) + 65);
            
            
          if ( b == crib[i])
          {
              for (int x = 1; x < 5; x++)
              {
                  IEnumerable<IEnumerable<int>> combis = Blupp.Combinations(foo, x);
                  foreach (IEnumerable<int> enumerable in combis)
                  {
                      int[] arr = enumerable.ToArray();

                      /*foreach (int i1 in arr)
                      {
                          CipherRotors[i1].IncrementPosition();
                      }
                      byte[] positions2 = (byte[])positions.Clone();

                      foreach (int i1 in arr)
                      {
                          positions2[i1] = (byte)(((positions[i1] - 1)+26) %26);
                      }
                      if(i<crib.Length-1)
                          recursiveShit(cipher, crib, foo, types, positions2, i + 1);
                  }

              }
          }
            
          if (i > 1)
          {
              for (int ix = 0; ix < i; ix++)
              {
                  Console.Write((char)crib[ix]);
              }
              Console.WriteLine((char) b ); 
          }
            
          if (b == crib.Last()&& i==crib.Length-1)
                  {
                      /*foreach (byte b1 in positions)
                      {
                          Console.Write((char)(b1+65) +"  ");
                      }
                     //Console.WriteLine("we've got a winner");
                      counter++;
                      return true;
                  }
                  else
                  {
                      return false;
                  }
      }
    public int PhaseI2(byte[] cipher, byte[] crib, int[] types, byte[] positions)
        {

            //byte[] repeat = new byte[cipher.Length];

            setInternalConfig(types, positions);

            int[] foo = new int[5] { 0, 1, 2, 3, 4 };

            /*  foreach (int type in types)
              {
                  Console.Write(type);
              }
              Console.WriteLine();

            counter = 0;

            byte[][] posarr = new byte[crib.Length + 1][];
            posarr[0] = positions;
            recursiveShit2(cipher, crib, foo, types, posarr, 0);
            //Console.WriteLine(counter);
            return counter;
        }

        private bool recursiveShit2(byte[] cipher, byte[] crib, int[] foo, int[] types, byte[][] positions, int i)
        {
            setInternalConfig(types, positions[i]);

            byte b = (byte)(Cipher((byte)(cipher[i] - 65)) + 65);


            if (b == crib[i])
            {
                for (int x = 1; x < 5; x++)
                {
                    IEnumerable<IEnumerable<int>> combis = Blupp.Combinations(foo, x);
                    foreach (IEnumerable<int> enumerable in combis)
                    {
                        int[] arr = enumerable.ToArray();

                        /*foreach (int i1 in arr)
                        {
                            CipherRotors[i1].IncrementPosition();
                        }
                        byte[][] positions2 = (byte[][])positions.Clone();

                        positions2[i + 1] = (byte[])positions[i].Clone();

                        foreach (int i1 in arr)
                        {
                            positions2[i + 1][i1] = (byte)(((positions[i][i1] - 1) + 26) % 26);
                        }
                        if (i < crib.Length - 1)
                            recursiveShit2(cipher, crib, foo, types, positions2, i + 1);
                    }

                }
            }
            /*
            if (i > 1)
            {
                for (int ix = 0; ix < i; ix++)
                {
                    Console.Write((char)crib[ix]);
                }
                Console.WriteLine((char) b ); 
            }
            
            if (b == crib.Last() && i == crib.Length - 1)
            {
                foreach (byte b1 in positions[i - 1])
                {
                    Console.Write((char)(b1 + 65) + "  ");
                }
                Console.WriteLine("we've got a winner");
                counter++;
                return true;
            }
            else
            {
                return false;
            }
        }

 
 */

