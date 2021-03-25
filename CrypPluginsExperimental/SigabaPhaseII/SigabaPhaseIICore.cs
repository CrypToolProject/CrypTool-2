using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SigabaPhaseII
{
    public class SigabaPhaseIICore
    {
        SigabaPhaseII facade;
        RotorByte[] CodeWheels = new RotorByte[5];
        int[] realWheels = new int[5];

        string[] keys;

        public SigabaPhaseIICore(SigabaPhaseII facade) 
        {
            this.facade = facade;
        }
    
        public void setCodeWheels(int[] phase1Wheels, string[] keys)
        {
            int count = 0; 
            for(int ccr = 1; ccr< ConstantsByte.ControlCipherRotors.Length;ccr++)
            {
                if(!phase1Wheels.Contains(ccr))
                {
                    CodeWheels[count] = new RotorByte(ConstantsByte.ControlCipherRotors[ccr], 0, false);
                    realWheels[count] = ccr;
                    count++;
                }
            }

            this.keys = keys;
        }

        public List<Candidate> stepOneCompact(int[][] input2)
        {
            int[] foo = new int[26];

            List<Candidate> winnerList2 = new List<Candidate>();

            for (int i = 0; i < foo.Count(); i++)
            {
                foo[i] = i;
            }

            List<Candidate> winnerList = new List<Candidate>();
            //List<Candidate> winnerList2 = new List<Candidate>();
            //List<Candidate> winnerList3 = new List<Candidate>();


            IEnumerable<IEnumerable<int>> combis2 = CombinationFactory.Combinations(new[] { 0, 1, 2, 3, 4 }, 3);

            RotorByte[] ControlRotors = new RotorByte[3];
            int counter = 0;

            Boolean br = false;

            for (int co = 0; co < combis2.Count(); co++)
            {

                int[] arr = combis2.ElementAt(co).ToArray();

                do
                {
                    counter++;
                    ControlRotors[0] = CodeWheels[arr[0]];
                    ControlRotors[1] = CodeWheels[arr[1]];
                    ControlRotors[2] = CodeWheels[arr[2]];
                    Console.Write(realWheels[arr[0]]);
                    Console.Write(realWheels[arr[1]]);
                    Console.WriteLine(realWheels[arr[2]]);

                    Console.WriteLine("Combis.count");

                    for (int ix = 0; ix < 8; ix++)
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
                        for (int co1 = 0; co1 < 26; co1++)
                        {
                            for (int co2 = 0; co2 < 26; co2++)
                            {
                                for (int co3 = 0; co3 < 26; co3++)
                                {
                                    ControlRotors[0].Position = co1;
                                    ControlRotors[1].Position = co2;
                                    ControlRotors[2].Position = co3;
                                    int[][] pseudo = new int[26][];
                                    int[][] temp = new int[input2.Length][];


                                    for (int letters = 0; letters < input2.Length; letters++)
                                    {
                                        /* int tempf = combis.ElementAt(ix).ElementAt(0);
                                     int tempg = combis.ElementAt(ix).ElementAt(1);
                                     int temph = combis.ElementAt(ix).ElementAt(2);
                                     int tempi = combis.ElementAt(ix).ElementAt(3);*/
                                        int tempf = 5;
                                        int tempg = 6;
                                        int temph = 7;
                                        int tempi = 8;



                                        for (int i = 0; i < 3; i++)
                                        {
                                            //converted to:
                                            if (ControlRotors[i].Reverse)
                                            {
                                                tempf =
                                                    ControlRotors[i].RotSubMatRevBack[ControlRotors[i].Position, tempf];
                                                tempg =
                                                    ControlRotors[i].RotSubMatRevBack[ControlRotors[i].Position, tempg];
                                                temph =
                                                    ControlRotors[i].RotSubMatRevBack[ControlRotors[i].Position, temph];
                                                tempi =
                                                    ControlRotors[i].RotSubMatRevBack[ControlRotors[i].Position, tempi];
                                            }
                                            else
                                            {
                                                tempf = ControlRotors[i].RotSubMatBack[ControlRotors[i].Position, tempf];
                                                tempg = ControlRotors[i].RotSubMatBack[ControlRotors[i].Position, tempg];
                                                temph = ControlRotors[i].RotSubMatBack[ControlRotors[i].Position, temph];
                                                tempi = ControlRotors[i].RotSubMatBack[ControlRotors[i].Position, tempi];
                                            }

                                        }

                                        int[] tmp = new int[] {tempf,tempg,temph,tempi,0};

                                        temp[letters] = tmp;

                                        /*
                                    Console.Write((char)(tempf+65)+"");
                                    Console.Write((char)(tempg + 65)+"");
                                    Console.Write((char)(temph + 65)+"");
                                    Console.Write((char)(tempi + 65)+"" + letters);
                                    Console.WriteLine();*/
                                        //Console.ReadKey();

                                        if (pseudo[tempf] == null)
                                            pseudo[tempf] = input2[letters];
                                        else
                                        {
                                            pseudo[tempf] = pseudo[tempf].Intersect(input2[letters]).ToArray();
                                            if (pseudo[tempf].Count() == 0)
                                            {
                                                //Console.WriteLine("bingo");
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
                                                break;
                                            }
                                        }
                                        if (letters == input2.Length - 1)
                                        {
                                            /*Console.WriteLine("we have got a winner");
                                            Console.WriteLine((char)(co1 + 65) + "" + (char)(co2 + 65) + "" + (char)(co3 + 65));
                                            Console.WriteLine("ReverseControlRotor1:" + ControlRotors[0].Reverse + " ReverseControlRotor2:" + ControlRotors[1].Reverse + " ReverseControlRotor3:" + ControlRotors[2].Reverse);
                                            */
                                            for (int cn = 0; cn < pseudo.Length; cn++)
                                            {
                                                if (pseudo[cn] == null)
                                                    pseudo[cn] = new int[] { 0, 1, 2, 3, 4 };
                                            }

                                            Candidate winner = new Candidate()
                                            {
                                                Positions = new int[] { co1, co2, co3 },
                                                Reverse =
                                                    new bool[]
                                                                               {
                                                                                   ControlRotors[0].Reverse,
                                                                                   ControlRotors[1].Reverse,
                                                                                   ControlRotors[2].Reverse
                                                                               },
                                                RotorType = new int[] { arr[0], arr[1], arr[2] },
                                                RotorTypeReal = new int[] { realWheels[arr[0]], realWheels[arr[1]], realWheels[arr[2]] },
                                                Pseudo = pseudo
                                            };

                                            Candidate winnerReal = new Candidate()
                                            {
                                                Positions = new int[] { co1, co2, co3 },
                                                Reverse =
                                                    new bool[]
                                                                               {
                                                                                   ControlRotors[0].Reverse,
                                                                                   ControlRotors[1].Reverse,
                                                                                   ControlRotors[2].Reverse
                                                                               },
                                                RotorType = new int[] { realWheels[arr[0]], realWheels[arr[1]], realWheels[arr[2]] },
                                                Pseudo = pseudo
                                            };

                                            // Console.WriteLine("");
                                            // br = true;
                                            //int[][] winner = pseudo;

                                            //facade.AddPresentationEntry(winner);

                                            facade.AddEntryCandidate(winner);

                                           if( WinnerConfirmCompact(input2, winner,temp));
                                            {
                                                
                                                Console.WriteLine("WCC");
                                            }
                                            //winnerList.Add(winner);
                                            break;
                                        }



                                        if (ControlRotors[2].Position == 14)
                                        {
                                            if (ControlRotors[1].Position == 14)
                                            {
                                                pseudo = new int[26][];
                                                temp[letters][4] = 1;
                                            }
                                            ControlRotors[1].IncrementPosition();
                                        }
                                        ControlRotors[2].IncrementPosition();
                                    }                                               //if (br) break;
                                }                                                   //if (br) break;
                            }                                                       //if (br) break;
                        }                                                           //if (br) break;
                    }                                                               //if (br) break;
                } while (NextPermutation(arr));                                     //if (br) break;
            }

            return winnerList;
        }

        public Boolean WinnerConfirmCompact(int[][] input2, Candidate winner, int [][] temp)
        {
                List<Candidate> winnerList2 = new List<Candidate>();
                int[] rest = new int[2];
                bool b = true;
                for (int i = 0; i < 5; i++)
                {
                    if (!winner.RotorType.Contains(i))
                    {
                        if (b)
                        {
                            rest[0] = i;
                            b = false;
                        }
                        else
                        {
                            rest[1] = i;
                        }

                    }
                }

                for (int re = 0; re < 2; re++)
                {
                    RotorByte testRotor = CodeWheels[rest[re]];

                    for (int rev = 0; rev < 2; rev++)
                    {
                        testRotor.Reverse = (rev == 1);
                        for (int pos = 0; pos < 26; pos++)
                        {
                            testRotor.Position = pos;
                            int[][] pseudo = new int[26][];
                            if (pos == 0)
                                Console.Write("");
                            int[][] temp2 = new int[input2.Length][];
                            for (int letters = 0; letters < input2.Length; letters++)
                            {
                                int tempf = temp[letters][0]; ;
                                int tempg = temp[letters][1]; ;
                                int temph = temp[letters][2];;
                                int tempi = temp[letters][3]; ;

                                if (testRotor.Reverse)
                                {
                                    tempf =
                                        testRotor.RotSubMatRevBack[testRotor.Position, tempf];
                                    tempg =
                                        testRotor.RotSubMatRevBack[testRotor.Position, tempg];
                                    temph =
                                        testRotor.RotSubMatRevBack[testRotor.Position, temph];
                                    tempi =
                                        testRotor.RotSubMatRevBack[testRotor.Position, tempi];
                                }
                                else
                                {
                                    tempf = testRotor.RotSubMatBack[testRotor.Position, tempf];
                                    tempg = testRotor.RotSubMatBack[testRotor.Position, tempg];
                                    temph = testRotor.RotSubMatBack[testRotor.Position, temph];
                                    tempi = testRotor.RotSubMatBack[testRotor.Position, tempi];
                                }

                                int[] tmp = new int[] { tempf, tempg, temph, tempi, 0 };

                                temp2[letters] = tmp;

                                /*Console.Write((char)(tempf + 65) + "");
                                Console.Write((char)(tempg + 65) + "");
                                Console.Write((char)(temph + 65) + "");
                                Console.Write((char)(tempi + 65) + "" + letters);
                                Console.WriteLine();
                                Console.ReadKey();*/

                                if (pseudo[tempf] == null)
                                    pseudo[tempf] = input2[letters];
                                else
                                {
                                    pseudo[tempf] = pseudo[tempf].Intersect(input2[letters]).ToArray();
                                    if (pseudo[tempf].Count() == 0)
                                    {
                                        //Console.WriteLine("bingo");
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
                                        break;
                                    }
                                }
                                if (letters == input2.Length - 1)
                                {
                                    //Console.WriteLine("we have got a winner");
                                    //Console.WriteLine("Rotor 4: " + pos + testRotor.Reverse + rest[re] + wi);
                                    /*Console.WriteLine("Rotor1: " + winner.RotorType[0] + " REverse: " +
                                                      winner.Reverse[0] + " POsition: " + winner.Positions[0] +
                                                      " Rotor2: " + winner.RotorType[1] + " REverse: " +
                                                      winner.Reverse[1] + " POsition: " + winner.Positions[1] +
                                                      " Rotor3: " + winner.RotorType[2] + " REverse: " +
                                                      winner.Reverse[2] + " POsition: " + winner.Positions[2]);
                                    */

                                    facade.AddEntryConfirmed(winner,realWheels[rest[re]],realWheels[rest[re==0? 1:0]]);

                                    if (!winnerList2.Contains(winner))
                                    {
                                        winnerList2.Add(winner);
                                        if(SteppingMazeCompletionCompact(input2, winner, temp))
                                        {
                                            Console.WriteLine("SCC");
                                        }
                                        
                                    }
                                    
                                    break;
                                }


                                
                                if(temp[letters][4]==1)
                                {
                                    testRotor.IncrementPosition();
                                }
                                

                            }
                        }
                    }
                }
            return true;
        }

        public Boolean SteppingMazeCompletionCompact(int[][] input2, Candidate winner, int[][] temp)
        {
            
                List<int[]> alreadyTested = new List<int[]>();
                int[] test = new int[26];
                int[][] test2 = new int[26][];
                test2 = winner.Pseudo;
            
                for (int i0 = 0; i0 < test2[0].Length; i0++)
                for (int i1 = 0; i1 < test2[1].Length; i1++)
                for (int i2 = 0; i2 < test2[2].Length; i2++)
                for (int i3 = 0; i3 < test2[3].Length; i3++)
                for (int i4 = 0; i4 < test2[4].Length; i4++)
                for (int i5 = 0; i5 < test2[5].Length; i5++)
                for (int i6 = 0; i6 < test2[6].Length; i6++)
                for (int i7 = 0; i7 < test2[7].Length; i7++)
                for (int i8 = 0; i8 < test2[8].Length; i8++)
                for (int i9 = 0; i9 < test2[9].Length; i9++)
                for (int i10 = 0; i10 < test2[10].Length; i10++)
                for (int i11 = 0; i11 < test2[11].Length; i11++)
                for (int i12 = 0; i12 < test2[12].Length; i12++)
                for (int i13 = 0; i13 < test2[13].Length; i13++)
                for (int i14 = 0; i14 < test2[14].Length; i14++)
                for (int i15 = 0; i15 < test2[15].Length; i15++)
                for (int i16 = 0; i16 < test2[16].Length; i16++)
                for (int i17 = 0; i17 < test2[17].Length; i17++)
                for (int i18 = 0; i18 < test2[18].Length; i18++)
                for (int i19 = 0; i19 < test2[19].Length; i19++)
                for (int i20 = 0; i20 < test2[20].Length; i20++)
                for (int i21 = 0; i21 < test2[21].Length; i21++)
                for (int i22 = 0; i22 < test2[22].Length; i22++)
                for (int i23 = 0; i23 < test2[23].Length; i23++)
                for (int i24 = 0; i24 < test2[24].Length; i24++)
                for (int i25 = 0; i25 < test2[25].Length; i25++)

                {
                    /*Console.WriteLine(test2[0][i0] + "" + test2[1][i1] + "" + test2[2][i2] + "" + test2[3][i3] + "" +
                                      test2[4][i4] + "" + test2[5][i5] + "" + test2[6][i6] + "" + test2[7][i7] + "" +
                                      test2[8][i8] + "" + test2[9][i9] + "" + test2[10][i10] + "" + test2[11][i11] + "" +
                                      test2[12][i12] + "" + test2[13][i13] + "" + test2[14][i14] + "" + test2[15][i15] +
                                      "" + test2[16][i16] + "" + test2[17][i17] + "" + test2[18][i18] + "" +
                                      test2[19][i19] + "" + test2[20][i20] + "" + test2[21][i21] + "" + test2[22][i22] +
                                      "" + test2[23][i23] + "" + test2[24][i24] + "" + test2[25][i25] + "");*/

                    test[0] = test2[0][i0];
                    test[1] = test2[1][i1];
                    test[2] = test2[2][i2];
                    test[3] = test2[3][i3];
                    test[4] = test2[4][i4];
                    test[5] = test2[5][i5];
                    test[6] = test2[6][i6];
                    test[7] = test2[7][i7];
                    test[8] = test2[8][i8];
                    test[9] = test2[9][i9];
                    test[10] = test2[10][i10];
                    test[11] = test2[11][i11];
                    test[12] = test2[12][i12];
                    test[13] = test2[13][i13];
                    test[14] = test2[14][i14];
                    test[15] = test2[15][i15];
                    test[16] = test2[16][i16];
                    test[17] = test2[17][i17];
                    test[18] = test2[18][i18];
                    test[19] = test2[19][i19];
                    test[20] = test2[20][i20];
                    test[21] = test2[21][i21];
                    test[22] = test2[22][i22];
                    test[23] = test2[23][i23];
                    test[24] = test2[24][i24];
                    test[25] = test2[25][i25];

                    int counter1 = 0;
                    int counter2 = 0;
                    int counter3 = 0;
                    int counter4 = 0;
                    int counter5 = 0;



                    foreach (int ix1 in test)
                    {
                        if (ix1 == 0)
                            counter1++;
                        if (ix1 == 1)
                            counter2++;
                        if (ix1 == 2)
                            counter3++;
                        if (ix1 == 3)
                            counter4++;
                        if (ix1 == 4)
                            counter5++;
                    }
                    /*
                    Console.WriteLine("0: " + counter1);
                    Console.WriteLine("1: " + counter2);
                    Console.WriteLine("2: " + counter3);
                    Console.WriteLine("3: " + counter4);
                    Console.WriteLine("4: " + counter5);
                    */
                    int[] countarr = new int[5]{ counter1, counter2, counter3, counter4, counter5 };
                    Array.Sort(countarr);
                    

                    Boolean brB = false;
                    for (int cm = 0; cm < alreadyTested.Count; cm++)
                    {
                        Boolean bev = false;
                        for (int cn = 0; cn < alreadyTested[cm].Length; cn++)
                        {
                            if (alreadyTested[cm][cn] != countarr[4-cn])
                            {
                                bev = true;
                            }
                        }
                        if (!bev)
                        {
                            brB = true;
                            break;
                        }
                    }

                    if (brB)
                    {
                        break;
                    }
                    
                    alreadyTested.Add(countarr);

                    if (counter1 + counter2 + counter3 + counter4 + counter5 != 26)
                    {
                        break;
                    }

                    List<int[][]> l3 = IndexPermutations(countarr);

                    int[] rest = new int[2];

                    bool b = true;
                    for (int i = 0; i < 5; i++)
                    {
                        if (!winner.RotorType.Contains(i))
                        {
                            if (b)
                            {
                                rest[0] = i;
                                b = false;
                            }
                            else
                            {
                                rest[1] = i;
                            }

                        }
                    }



                    for (int re = 0; re < 2; re++)
                    {
                        RotorByte[] testRotor = new RotorByte[2];
                        testRotor[0] = CodeWheels[rest[re]];
                        Console.WriteLine("");
                        if (re == 1)
                        {
                            testRotor[1] = CodeWheels[rest[0]];
                            Console.Write(rest[0]);
                            Console.Write(rest[1]);
                        }
                        else
                        {
                            testRotor[1] = CodeWheels[rest[1]];
                            Console.Write(rest[1]);
                            Console.Write(rest[0]);
                        }

                        Console.WriteLine("Combis.count");
                        for (int indpo = 0; indpo < l3.Count; indpo++)
                        {
                            int[] steppingmaze = new int[10];

                            for (int i = 0; i < 10; i++)
                            {
                                for (int ix = 0; ix < 5; ix++)
                                {
                                    if (l3[indpo][ix].Contains(i))
                                    {
                                        steppingmaze[i] = ix;
                                    }
                                }

                            }
                        for (int rev1 = 0; rev1 < 2; rev1++)
                        {
                            testRotor[0].Reverse = (rev1 == 1);
                            for (int rev2 = 0; rev2 < 2; rev2++)
                            {
                                testRotor[1].Reverse = (rev2 == 1);
                                for (int pos = 0; pos < 26; pos++)
                                {
                                    for (int pos2 = 0; pos2 < 26; pos2++)
                                    {
                                        
                                            testRotor[0].Position = pos;
                                            testRotor[1].Position = pos2;

                                            

                                            for (int letters = 0; letters < input2.Length; letters++)
                                            {
                                                int tempf = temp[letters][0];
                                                int tempg = temp[letters][1];
                                                int temph = temp[letters][2];
                                                int tempi = temp[letters][3];

                                                for (int i = 0; i < 2; i++)
                                                {
                                                    if (testRotor[i].Reverse)
                                                    {
                                                        tempf =
                                                            testRotor[i].RotSubMatRevBack[
                                                                testRotor[i].Position, tempf];
                                                        tempg =
                                                            testRotor[i].RotSubMatRevBack[
                                                                testRotor[i].Position, tempg];
                                                        temph =
                                                            testRotor[i].RotSubMatRevBack[
                                                                testRotor[i].Position, temph];
                                                        tempi =
                                                            testRotor[i].RotSubMatRevBack[
                                                                testRotor[i].Position, tempi];
                                                    }
                                                    else
                                                    {
                                                        tempf =
                                                            testRotor[i].RotSubMatBack[testRotor[i].Position, tempf];
                                                        tempg =
                                                            testRotor[i].RotSubMatBack[testRotor[i].Position, tempg];
                                                        temph =
                                                            testRotor[i].RotSubMatBack[testRotor[i].Position, temph];
                                                        tempi =
                                                            testRotor[i].RotSubMatBack[testRotor[i].Position, tempi];
                                                    }
                                                }

                                                /*Console.Write((char)(tempf + 65) + "");
                                                    Console.Write((char)(tempg + 65) + "");
                                                    Console.Write((char)(temph + 65) + "");
                                                    Console.Write((char)(tempi + 65) + "" + letters);
                                                    Console.WriteLine();
                                                    Console.ReadKey();*/

                                                

                                                tempf = ConstantsByte.Transform[0][tempf];
                                                tempg = ConstantsByte.Transform[0][tempg];
                                                temph = ConstantsByte.Transform[0][temph];
                                                tempi = ConstantsByte.Transform[0][tempi];

                                                tempf = steppingmaze[tempf];
                                                tempg = steppingmaze[tempg];
                                                temph = steppingmaze[temph];
                                                tempi = steppingmaze[tempi];

                                                if (!input2[letters].Contains(tempf) ||
                                                    !input2[letters].Contains(tempg) ||
                                                    !input2[letters].Contains(temph) ||
                                                    !input2[letters].Contains(tempi))
                                                    break;


                                                if (letters == input2.Length - 1)
                                                {
                                                    Console.WriteLine("we have got a winner");
                                                    //Console.WriteLine("Rotor 4: " + pos + testRotor.Reverse + rest[re] + wi);
                                                    /*Console.WriteLine("Rotor1: " + winner.RotorType[0] +
                                                                      " REverse: " +
                                                                      winner.Reverse[0] + " POsition: " +
                                                                      winner.Positions[0] +
                                                                      " Rotor2: " + winner.RotorType[1] +
                                                                      " REverse: " +
                                                                      winner.Reverse[1] + " POsition: " +
                                                                      winner.Positions[1] +
                                                                      " Rotor3: " + winner.RotorType[2] +
                                                                      " REverse: " +
                                                                      winner.Reverse[2] + " POsition: " +
                                                                      winner.Positions[2] +
                                                                      " Rotor4: " + rest[0] + " REverse: " +
                                                                      testRotor[0].Reverse + " POsition: " + pos +
                                                                      " Rotor5: " + rest[1] + " REverse: " +
                                                                      testRotor[1].Reverse + " POsition: " + pos2
                                                        );*/
                                                    facade.AddEntryComplete(winner, steppingmaze, pos, pos2, realWheels[rest[re]], realWheels[rest[re == 0 ? 1 : 0]],rev1==1,rev2==1);

                                                    break;
                                                    }

                                                    if (temp[letters][4] == 1)
                                                        {
                                                            testRotor[0].IncrementPosition();
                                                        }

                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }



            return true;
        }

        public List<Candidate> stepOne(int[][] input2)
        {
            int[] foo = new int[26];

            List<Candidate> winnerList2 = new List<Candidate>();

            for (int i = 0; i < foo.Count(); i++)
            {
                foo[i] = i;
            }

            List<Candidate> winnerList = new List<Candidate>();
            //List<Candidate> winnerList2 = new List<Candidate>();
            //List<Candidate> winnerList3 = new List<Candidate>();

            
            IEnumerable<IEnumerable<int>> combis2 = CombinationFactory.Combinations(new[] { 0, 1, 2, 3, 4 }, 3);

            RotorByte[] ControlRotors = new RotorByte[3];
            int counter = 0;

            Boolean br = false;

            for (int co = 0; co < combis2.Count(); co++)
            {

                int[] arr = combis2.ElementAt(co).ToArray();

                do
                {
                    counter++;
                    ControlRotors[0] = CodeWheels[arr[0]];
                    ControlRotors[1] = CodeWheels[arr[1]];
                    ControlRotors[2] = CodeWheels[arr[2]];
                    Console.Write(arr[0]);
                    Console.Write(arr[1]);
                    Console.WriteLine(arr[2]);

                    Console.WriteLine("Combis.count");

                    for (int ix = 0; ix < 8; ix++)
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
                        for (int co1 = 0; co1 < 26; co1++)
                        {
                            for (int co2 = 0; co2 < 26; co2++)
                            {
                                for (int co3 = 0; co3 < 26; co3++)
                                {
                                    ControlRotors[0].Position = co1;
                                    ControlRotors[1].Position = co2;
                                    ControlRotors[2].Position = co3;
                                    int[][] pseudo = new int[26][];

                                    for (int letters = 0; letters < input2.Length; letters++)
                                    {
                                        /* int tempf = combis.ElementAt(ix).ElementAt(0);
                                     int tempg = combis.ElementAt(ix).ElementAt(1);
                                     int temph = combis.ElementAt(ix).ElementAt(2);
                                     int tempi = combis.ElementAt(ix).ElementAt(3);*/
                                        int tempf = 5;
                                        int tempg = 6;
                                        int temph = 7;
                                        int tempi = 8;



                                        for (int i = 0; i < 3; i++)
                                        {
                                            //converted to:
                                            if (ControlRotors[i].Reverse)
                                            {
                                                tempf =
                                                    ControlRotors[i].RotSubMatRevBack[ControlRotors[i].Position, tempf];
                                                tempg =
                                                    ControlRotors[i].RotSubMatRevBack[ControlRotors[i].Position, tempg];
                                                temph =
                                                    ControlRotors[i].RotSubMatRevBack[ControlRotors[i].Position, temph];
                                                tempi =
                                                    ControlRotors[i].RotSubMatRevBack[ControlRotors[i].Position, tempi];
                                            }
                                            else
                                            {
                                                tempf = ControlRotors[i].RotSubMatBack[ControlRotors[i].Position, tempf];
                                                tempg = ControlRotors[i].RotSubMatBack[ControlRotors[i].Position, tempg];
                                                temph = ControlRotors[i].RotSubMatBack[ControlRotors[i].Position, temph];
                                                tempi = ControlRotors[i].RotSubMatBack[ControlRotors[i].Position, tempi];
                                            }

                                        }
                                        /*
                                    Console.Write((char)(tempf+65)+"");
                                    Console.Write((char)(tempg + 65)+"");
                                    Console.Write((char)(temph + 65)+"");
                                    Console.Write((char)(tempi + 65)+"" + letters);
                                    Console.WriteLine();*/
                                        //Console.ReadKey();

                                        if (pseudo[tempf] == null)
                                            pseudo[tempf] = input2[letters];
                                        else
                                        {
                                            pseudo[tempf] = pseudo[tempf].Intersect(input2[letters]).ToArray();
                                            if (pseudo[tempf].Count() == 0)
                                            {
                                                //Console.WriteLine("bingo");
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
                                                break;
                                            }
                                        }
                                        if (letters == input2.Length-1)
                                        {
                                            /*Console.WriteLine("we have got a winner");
                                            Console.WriteLine((char)(co1 + 65) + "" + (char)(co2 + 65) + "" + (char)(co3 + 65));
                                            Console.WriteLine("ReverseControlRotor1:" + ControlRotors[0].Reverse + " ReverseControlRotor2:" + ControlRotors[1].Reverse + " ReverseControlRotor3:" + ControlRotors[2].Reverse);
                                            */
                                            for (int cn = 0; cn < pseudo.Length; cn++)
                                            {
                                                if (pseudo[cn] == null)
                                                    pseudo[cn] = new int[] { 0, 1, 2, 3, 4 };
                                            }

                                            Candidate winner = new Candidate()
                                            {
                                                Positions = new int[] { co1, co2, co3 },
                                                Reverse =
                                                    new bool[]
                                                                               {
                                                                                   ControlRotors[0].Reverse,
                                                                                   ControlRotors[1].Reverse,
                                                                                   ControlRotors[2].Reverse
                                                                               },
                                                RotorType = new int[] { arr[0], arr[1], arr[2] },
                                                Pseudo = pseudo
                                            };

                                           // Console.WriteLine("");
                                            // br = true;
                                            //int[][] winner = pseudo;

                                            //facade.AddPresentationEntry(winner);

                                            winnerList.Add(winner);
                                            break;
                                        }



                                        if (ControlRotors[2].Position == 14)
                                        {
                                            if (ControlRotors[1].Position == 14)
                                            {
                                                pseudo = new int[26][];
                                            }
                                            ControlRotors[1].IncrementPosition();
                                        }
                                        ControlRotors[2].IncrementPosition();
                                    }                                               //if (br) break;
                                }                                                   //if (br) break;
                            }                                                       //if (br) break;
                        }                                                           //if (br) break;
                    }                                                               //if (br) break;
                } while (NextPermutation(arr));                                     //if (br) break;
            }

            return winnerList;
        }

        public List<Candidate> WinnerConfirm(int[][] input2, List<Candidate> winnerList)
        {
            List<Candidate> winnerList2 = new List<Candidate>();

            for (int wi = 0; wi < winnerList.Count; wi++)
            {

                Candidate winner = winnerList[wi];

                RotorByte[] ControlRotors = new RotorByte[3];

                ControlRotors[0] = CodeWheels[winner.RotorType[0]];
                ControlRotors[1] = CodeWheels[winner.RotorType[1]];
                ControlRotors[2] = CodeWheels[winner.RotorType[2]];

                ControlRotors[0].Position = winner.Positions[0];

                ControlRotors[0].Reverse = winner.Reverse[0];
                ControlRotors[1].Reverse = winner.Reverse[1];
                ControlRotors[2].Reverse = winner.Reverse[2];


                int[] rest = new int[2];
                bool b = true;
                for (int i = 0; i < 5; i++)
                {
                    if (!winner.RotorType.Contains(i))
                    {
                        if (b)
                        {
                            rest[0] = i;
                            b = false;
                        }
                        else
                        {
                            rest[1] = i;
                        }

                    }
                }

                for (int re = 0; re < 2; re++)
                {
                    RotorByte testRotor = CodeWheels[rest[re]];

                    for (int rev = 0; rev < 2; rev++)
                    {
                        testRotor.Reverse = (rev == 1);
                        for (int pos = 0; pos < 26; pos++)
                        {
                            ControlRotors[1].Position = winner.Positions[1];
                            ControlRotors[2].Position = winner.Positions[2];
                            testRotor.Position = pos;
                            int[][] pseudo = new int[26][];
                            if (pos == 0)
                                Console.Write("");

                            for (int letters = 0; letters < input2.Length; letters++)
                            {
                                int tempf = 5;
                                int tempg = 6;
                                int temph = 7;
                                int tempi = 8;

                                for (int i = 0; i < 3; i++)
                                {
                                    //converted to:
                                    if (ControlRotors[i].Reverse)
                                    {
                                        tempf =
                                            ControlRotors[i].RotSubMatRevBack[ControlRotors[i].Position, tempf];
                                        tempg =
                                            ControlRotors[i].RotSubMatRevBack[ControlRotors[i].Position, tempg];
                                        temph =
                                            ControlRotors[i].RotSubMatRevBack[ControlRotors[i].Position, temph];
                                        tempi =
                                            ControlRotors[i].RotSubMatRevBack[ControlRotors[i].Position, tempi];
                                    }
                                    else
                                    {
                                        tempf = ControlRotors[i].RotSubMatBack[ControlRotors[i].Position, tempf];
                                        tempg = ControlRotors[i].RotSubMatBack[ControlRotors[i].Position, tempg];
                                        temph = ControlRotors[i].RotSubMatBack[ControlRotors[i].Position, temph];
                                        tempi = ControlRotors[i].RotSubMatBack[ControlRotors[i].Position, tempi];
                                    }

                                }

                                /*Console.Write((char)(tempf+65)+"");
                                    Console.Write((char)(tempg + 65)+"");
                                    Console.Write((char)(temph + 65)+"");
                                    Console.Write((char)(tempi + 65)+"" + letters);
                                    Console.WriteLine();
                                    Console.ReadKey();*/

                                if (testRotor.Reverse)
                                {
                                    tempf =
                                        testRotor.RotSubMatRevBack[testRotor.Position, tempf];
                                    tempg =
                                        testRotor.RotSubMatRevBack[testRotor.Position, tempg];
                                    temph =
                                        testRotor.RotSubMatRevBack[testRotor.Position, temph];
                                    tempi =
                                        testRotor.RotSubMatRevBack[testRotor.Position, tempi];
                                }
                                else
                                {
                                    tempf = testRotor.RotSubMatBack[testRotor.Position, tempf];
                                    tempg = testRotor.RotSubMatBack[testRotor.Position, tempg];
                                    temph = testRotor.RotSubMatBack[testRotor.Position, temph];
                                    tempi = testRotor.RotSubMatBack[testRotor.Position, tempi];
                                }
                                /*Console.Write((char)(tempf + 65) + "");
                                Console.Write((char)(tempg + 65) + "");
                                Console.Write((char)(temph + 65) + "");
                                Console.Write((char)(tempi + 65) + "" + letters);
                                Console.WriteLine();
                                Console.ReadKey();*/

                                if (pseudo[tempf] == null)
                                    pseudo[tempf] = input2[letters];
                                else
                                {
                                    pseudo[tempf] = pseudo[tempf].Intersect(input2[letters]).ToArray();
                                    if (pseudo[tempf].Count() == 0)
                                    {
                                        //Console.WriteLine("bingo");
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
                                        break;
                                    }
                                }
                                if (letters == input2.Length-1)
                                {
                                    //Console.WriteLine("we have got a winner");
                                    //Console.WriteLine("Rotor 4: " + pos + testRotor.Reverse + rest[re] + wi);
                                    /*Console.WriteLine("Rotor1: " + winner.RotorType[0] + " REverse: " +
                                                      winner.Reverse[0] + " POsition: " + winner.Positions[0] +
                                                      " Rotor2: " + winner.RotorType[1] + " REverse: " +
                                                      winner.Reverse[1] + " POsition: " + winner.Positions[1] +
                                                      " Rotor3: " + winner.RotorType[2] + " REverse: " +
                                                      winner.Reverse[2] + " POsition: " + winner.Positions[2]);
                                    */
                                    if(!winnerList2.Contains(winner))
                                        winnerList2.Add(winner);

                                    break;
                                }



                                if (ControlRotors[2].Position == 14)
                                {
                                    if (ControlRotors[1].Position == 14)
                                    {
                                        testRotor.IncrementPosition();
                                    }
                                    ControlRotors[1].IncrementPosition();
                                }
                                ControlRotors[2].IncrementPosition();

                            }
                        }
                    }
                }
            }

            return winnerList2;
        }

        public List<Candidate> SteppingMazeCompletion(int[][] input2, List<Candidate> winnerList3)
        {
            for (int wi = 0; wi < winnerList3.Count; wi++)
            {
                List<int[]> alreadyTested = new List<int[]>();
                int[] test = new int[26];
                int[][] test2 = new int[26][];
                test2 = winnerList3[wi].Pseudo;
            
                for (int i0 = 0; i0 < test2[0].Length; i0++)
                for (int i1 = 0; i1 < test2[1].Length; i1++)
                for (int i2 = 0; i2 < test2[2].Length; i2++)
                for (int i3 = 0; i3 < test2[3].Length; i3++)
                for (int i4 = 0; i4 < test2[4].Length; i4++)
                for (int i5 = 0; i5 < test2[5].Length; i5++)
                for (int i6 = 0; i6 < test2[6].Length; i6++)
                for (int i7 = 0; i7 < test2[7].Length; i7++)
                for (int i8 = 0; i8 < test2[8].Length; i8++)
                for (int i9 = 0; i9 < test2[9].Length; i9++)
                for (int i10 = 0; i10 < test2[10].Length; i10++)
                for (int i11 = 0; i11 < test2[11].Length; i11++)
                for (int i12 = 0; i12 < test2[12].Length; i12++)
                for (int i13 = 0; i13 < test2[13].Length; i13++)
                for (int i14 = 0; i14 < test2[14].Length; i14++)
                for (int i15 = 0; i15 < test2[15].Length; i15++)
                for (int i16 = 0; i16 < test2[16].Length; i16++)
                for (int i17 = 0; i17 < test2[17].Length; i17++)
                for (int i18 = 0; i18 < test2[18].Length; i18++)
                for (int i19 = 0; i19 < test2[19].Length; i19++)
                for (int i20 = 0; i20 < test2[20].Length; i20++)
                for (int i21 = 0; i21 < test2[21].Length; i21++)
                for (int i22 = 0; i22 < test2[22].Length; i22++)
                for (int i23 = 0; i23 < test2[23].Length; i23++)
                for (int i24 = 0; i24 < test2[24].Length; i24++)
                for (int i25 = 0; i25 < test2[25].Length; i25++)

                {
                    Console.WriteLine(test2[0][i0] + "" + test2[1][i1] + "" + test2[2][i2] + "" + test2[3][i3] + "" +
                                      test2[4][i4] + "" + test2[5][i5] + "" + test2[6][i6] + "" + test2[7][i7] + "" +
                                      test2[8][i8] + "" + test2[9][i9] + "" + test2[10][i10] + "" + test2[11][i11] + "" +
                                      test2[12][i12] + "" + test2[13][i13] + "" + test2[14][i14] + "" + test2[15][i15] +
                                      "" + test2[16][i16] + "" + test2[17][i17] + "" + test2[18][i18] + "" +
                                      test2[19][i19] + "" + test2[20][i20] + "" + test2[21][i21] + "" + test2[22][i22] +
                                      "" + test2[23][i23] + "" + test2[24][i24] + "" + test2[25][i25] + "");

                    test[0] = test2[0][i0];
                    test[1] = test2[1][i1];
                    test[2] = test2[2][i2];
                    test[3] = test2[3][i3];
                    test[4] = test2[4][i4];
                    test[5] = test2[5][i5];
                    test[6] = test2[6][i6];
                    test[7] = test2[7][i7];
                    test[8] = test2[8][i8];
                    test[9] = test2[9][i9];
                    test[10] = test2[10][i10];
                    test[11] = test2[11][i11];
                    test[12] = test2[12][i12];
                    test[13] = test2[13][i13];
                    test[14] = test2[14][i14];
                    test[15] = test2[15][i15];
                    test[16] = test2[16][i16];
                    test[17] = test2[17][i17];
                    test[18] = test2[18][i18];
                    test[19] = test2[19][i19];
                    test[20] = test2[20][i20];
                    test[21] = test2[21][i21];
                    test[22] = test2[22][i22];
                    test[23] = test2[23][i23];
                    test[24] = test2[24][i24];
                    test[25] = test2[25][i25];

                    int counter1 = 0;
                    int counter2 = 0;
                    int counter3 = 0;
                    int counter4 = 0;
                    int counter5 = 0;



                    foreach (int ix1 in test)
                    {
                        if (ix1 == 0)
                            counter1++;
                        if (ix1 == 1)
                            counter2++;
                        if (ix1 == 2)
                            counter3++;
                        if (ix1 == 3)
                            counter4++;
                        if (ix1 == 4)
                            counter5++;
                    }

                    Console.WriteLine("0: " + counter1);
                    Console.WriteLine("1: " + counter2);
                    Console.WriteLine("2: " + counter3);
                    Console.WriteLine("3: " + counter4);
                    Console.WriteLine("4: " + counter5);

                    int[] countarr = { counter1, counter2, counter3, counter4, counter5 };
                    Array.Sort(countarr);


                    Boolean brB = false;
                    for (int cm = 0; cm < alreadyTested.Count; cm++)
                    {
                        Boolean bev = false;
                        for (int cn = 0; cn < alreadyTested[cm].Length; cn++)
                        {
                            if (alreadyTested[cm][cn] != countarr[cn])
                            {
                                bev = true;
                            }
                        }
                        if (!bev)
                        {
                            brB = true;
                            break;
                        }
                    }
                    if (brB)
                    { break; }
                    alreadyTested.Add(countarr);

                    if (counter1 + counter2 + counter3 + counter4 + counter5 != 26)
                    {
                        break;
                    }

                    List<int[][]> l3 = IndexPermutations(new[] { counter1, counter2, counter3, counter4, counter5 });

                    Candidate winner = winnerList3[wi];

                    RotorByte[] ControlRotors = new RotorByte[3];

                    ControlRotors[0] = CodeWheels[winner.RotorType[0]];
                    ControlRotors[1] = CodeWheels[winner.RotorType[1]];
                    ControlRotors[2] = CodeWheels[winner.RotorType[2]];

                    ControlRotors[0].Position = winner.Positions[0];

                    ControlRotors[0].Reverse = winner.Reverse[0];
                    ControlRotors[1].Reverse = winner.Reverse[1];
                    ControlRotors[2].Reverse = winner.Reverse[2];



                    int[] rest = new int[2];

                    bool b = true;
                    for (int i = 0; i < 5; i++)
                    {
                        if (!winner.RotorType.Contains(i))
                        {
                            if (b)
                            {
                                rest[0] = i;
                                b = false;
                            }
                            else
                            {
                                rest[1] = i;
                            }

                        }
                    }

                    for (int re = 0; re < 2; re++)
                    {
                        RotorByte[] testRotor = new RotorByte[2];
                        testRotor[0] = CodeWheels[rest[re]];
                        Console.WriteLine("");
                        if (re == 1)
                        {
                            testRotor[1] = CodeWheels[rest[0]];
                            Console.Write(rest[0]);
                            Console.Write(rest[1]);
                        }
                        else
                        {
                            testRotor[1] = CodeWheels[rest[1]];
                            Console.Write(rest[1]);
                            Console.Write(rest[0]);
                        }

                        Console.WriteLine("Combis.count");

                        for (int rev = 0; rev < 2; rev++)
                        {
                            testRotor[0].Reverse = (rev == 1);
                            for (int rev1 = 0; rev1 < 2; rev1++)
                            {
                                testRotor[1].Reverse = (rev1 == 1);
                                for (int pos = 0; pos < 26; pos++)
                                {
                                    for (int pos2 = 0; pos2 < 26; pos2++)
                                    {
                                        for (int indpo = 0; indpo < l3.Count; indpo++)
                                        {
                                            ControlRotors[1].Position = winner.Positions[1];
                                            ControlRotors[2].Position = winner.Positions[2];
                                            testRotor[0].Position = pos;
                                            testRotor[1].Position = pos2;

                                            int[][] pseudo = new int[26][];
                                            if (pos == 0)
                                                Console.Write("");

                                            for (int letters = 0; letters < input2.Length; letters++)
                                            {
                                                int tempf = 5;
                                                int tempg = 6;
                                                int temph = 7;
                                                int tempi = 8;

                                                for (int i = 0; i < 3; i++)
                                                {
                                                    //converted to:
                                                    if (ControlRotors[i].Reverse)
                                                    {
                                                        tempf =
                                                            ControlRotors[i].RotSubMatRevBack[
                                                                ControlRotors[i].Position, tempf];
                                                        tempg =
                                                            ControlRotors[i].RotSubMatRevBack[
                                                                ControlRotors[i].Position, tempg];
                                                        temph =
                                                            ControlRotors[i].RotSubMatRevBack[
                                                                ControlRotors[i].Position, temph];
                                                        tempi =
                                                            ControlRotors[i].RotSubMatRevBack[
                                                                ControlRotors[i].Position, tempi];
                                                    }
                                                    else
                                                    {
                                                        tempf =
                                                            ControlRotors[i].RotSubMatBack[
                                                                ControlRotors[i].Position, tempf];
                                                        tempg =
                                                            ControlRotors[i].RotSubMatBack[
                                                                ControlRotors[i].Position, tempg];
                                                        temph =
                                                            ControlRotors[i].RotSubMatBack[
                                                                ControlRotors[i].Position, temph];
                                                        tempi =
                                                            ControlRotors[i].RotSubMatBack[
                                                                ControlRotors[i].Position, tempi];
                                                    }

                                                }

                                                /*Console.Write((char)(tempf+65)+"");
                                          Console.Write((char)(tempg + 65)+"");
                                          Console.Write((char)(temph + 65)+"");
                                          Console.Write((char)(tempi + 65)+"" + letters);
                                          Console.WriteLine();
                                          Console.ReadKey();*/
                                                for (int i = 0; i < 2; i++)
                                                {
                                                    if (testRotor[i].Reverse)
                                                    {
                                                        tempf =
                                                            testRotor[i].RotSubMatRevBack[
                                                                testRotor[i].Position, tempf];
                                                        tempg =
                                                            testRotor[i].RotSubMatRevBack[
                                                                testRotor[i].Position, tempg];
                                                        temph =
                                                            testRotor[i].RotSubMatRevBack[
                                                                testRotor[i].Position, temph];
                                                        tempi =
                                                            testRotor[i].RotSubMatRevBack[
                                                                testRotor[i].Position, tempi];
                                                    }
                                                    else
                                                    {
                                                        tempf =
                                                            testRotor[i].RotSubMatBack[testRotor[i].Position, tempf];
                                                        tempg =
                                                            testRotor[i].RotSubMatBack[testRotor[i].Position, tempg];
                                                        temph =
                                                            testRotor[i].RotSubMatBack[testRotor[i].Position, temph];
                                                        tempi =
                                                            testRotor[i].RotSubMatBack[testRotor[i].Position, tempi];
                                                    }
                                                }
                                                /*Console.Write((char)(tempf + 65) + "");
                                        Console.Write((char)(tempg + 65) + "");
                                        Console.Write((char)(temph + 65) + "");
                                        Console.Write((char)(tempi + 65) + "" + letters);
                                        Console.WriteLine();
                                        Console.ReadKey();*/
                                                int[] steppingmaze = new int[10];
                                                for (int i = 0; i < 10; i++)
                                                {
                                                    for (int ix = 0; ix < 5; ix++)
                                                    {
                                                        if (l3[indpo][ix].Contains(i))
                                                        {
                                                            steppingmaze[i] = ix;
                                                        }
                                                    }

                                                }

                                                tempf = ConstantsByte.Transform[0][tempf];
                                                tempg = ConstantsByte.Transform[0][tempg];
                                                temph = ConstantsByte.Transform[0][temph];
                                                tempi = ConstantsByte.Transform[0][tempi];

                                                tempf = steppingmaze[tempf];
                                                tempg = steppingmaze[tempg];
                                                temph = steppingmaze[temph];
                                                tempi = steppingmaze[tempi];

                                                if (!input2[letters].Contains(tempf) ||
                                                    !input2[letters].Contains(tempg) ||
                                                    !input2[letters].Contains(temph) ||
                                                    !input2[letters].Contains(tempi))
                                                    break;


                                                if (letters == input2.Length-1)
                                                {
                                                    Console.WriteLine("we have got a winner");
                                                    //Console.WriteLine("Rotor 4: " + pos + testRotor.Reverse + rest[re] + wi);
                                                    Console.WriteLine("Rotor1: " + winner.RotorType[0] +
                                                                      " REverse: " +
                                                                      winner.Reverse[0] + " POsition: " +
                                                                      winner.Positions[0] +
                                                                      " Rotor2: " + winner.RotorType[1] +
                                                                      " REverse: " +
                                                                      winner.Reverse[1] + " POsition: " +
                                                                      winner.Positions[1] +
                                                                      " Rotor3: " + winner.RotorType[2] +
                                                                      " REverse: " +
                                                                      winner.Reverse[2] + " POsition: " +
                                                                      winner.Positions[2] +
                                                                      " Rotor4: " + rest[0] + " REverse: " +
                                                                      testRotor[0].Reverse + " POsition: " + pos +
                                                                      " Rotor5: " + rest[1] + " REverse: " +
                                                                      testRotor[1].Reverse + " POsition: " + pos2
                                                        );
                                                    break;
                                                }



                                                if (ControlRotors[2].Position == 14)
                                                {
                                                    if (ControlRotors[1].Position == 14)
                                                    {
                                                        testRotor[0].IncrementPosition();
                                                    }
                                                    ControlRotors[1].IncrementPosition();
                                                }
                                                ControlRotors[2].IncrementPosition();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }}
            return winnerList3;
        }

        private static List<int[][]> IndexPermutations(int[] test)
        {
            List<int[][]> l1 = new List<int[][]>();
            int[] x = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            do
            {
                if (x[0] < x[1] && x[2] < x[3] && x[4] < x[5] && x[6] < x[7] && x[8] < x[9])
                {
                    int[][] x1 = new int[][] { new[] { x[0], x[1] }, new[] { x[2], x[3] }, new[] { x[4], x[5] }, new[] { x[6], x[7] }, new[] { x[8], x[9] } };
                    l1.Add(x1);

                }


            }
            while (NextPermutation(x));

            int[][] l2 = new int[l1.Count][];

            for (int i = 0; i < l2.Length; i++)
            {
                int[] w = new int[5];

                for (int ix = 0; ix < 5; ix++)
                {
                    w[ix] = ConstantsByte.CountTable[l1[i][ix][0]] +
                            ConstantsByte.CountTable[l1[i][ix][1]];
                }
                l2[i] = w;
            }


            int info = 0;

            List<int[][]> l3 = new List<int[][]>();
            do
                for (int i = 0; i < l2.Length; i++)
                {
                    Boolean b = true;
                    for (int ci = 0; ci < l2[i].Length; ci++)
                    {
                        if (l2[i][ci] != test[ci])
                        {
                            b = false;
                        }
                    }

                    if (b)
                        l3.Add(l1[i]);
                } while (NextPermutation(test));


            Console.WriteLine(l3.Count);

            /*for (int i = 0; i < l3.Count; i++)
            {
                for (int ci = 0; ci < l3[i].Length; ci++)
                {
                    for (int co = 0; co < l3[i][ci].Length; co++)
                    {
                        Console.Write(l3[i][ci][co]);
                    }
                    Console.Write("; ");
                }
                Console.WriteLine("  ");
                Console.ReadKey();
            }
            Console.WriteLine();

            Console.WriteLine(info);
            Console.ReadKey();
            */
            return l3;
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

        private static string GetIntBinaryString(int n)
        {
            var b = new char[3];
            int pos = 2;
            int i = 0;

            while (i < 3)
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
    }

    public class Candidate
    {
        private int[] positions = new int[3];
        private int[] rotortype = new int[3];
        private int[] rotortypereal = new int[3];
        private bool[] reverse = new bool[3];
        private int[][] pseudo = new int[26][];

        public int[][] Pseudo
        {
            get
            {
                return pseudo;
            }
            set { pseudo = value; }
        }

        public int[] Positions
        {
            get
            {
                return positions;
            }
            set { positions = value; }
        }

        public int[] RotorType
        {
            get
            {
                return rotortype;
            }
            set { rotortype = value; }
        }

        public int[] RotorTypeReal
        {
            get
            {
                return rotortypereal;
            }
            set { rotortypereal = value; }
        }

        public bool[] Reverse
        {
            get
            {
                return reverse;
            }
            set { reverse = value; }
        }

    }

    internal static class CombinationFactory
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
