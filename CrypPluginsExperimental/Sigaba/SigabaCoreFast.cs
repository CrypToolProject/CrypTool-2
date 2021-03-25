using System.Linq;

namespace Sigaba
{
    public class SigabaCoreFast
    {

        public RotorByte[] ControlRotors;
        public RotorByte[] CipherRotors;
        public RotorByte[] IndexRotors;
        public byte[] IndexMaze = new byte[26];
        public RotorByte[] CodeWheels;

       public SigabaCoreFast()
       {
           InitializeRotors();
       }

        public byte Cipher(byte c)
        {
            return CipherRotors.Aggregate(c, (current, rotor) => rotor.DeCiph(current));
        }

        public byte[] Encrypt(byte[] cipher, int[] types, byte[] positions)
        {
            
            byte[] repeat = new byte[cipher.Length];

            setInternalConfig(types,positions);

            var incremented = new bool[CipherRotors.Length];

            for (int ix = 0; ix < cipher.Length;ix++ )
            {
                //StringBuilder s = new StringBuilder();

                //repeat = String.Concat(repeat, (char)(Cipher(c - 65) + 65) + "");

                repeat[ix] = (byte)(Cipher((byte)(cipher[ix] - 65)) + 65);
                
                //Set all incremented booleans to false
                for (int i = 0; i < incremented.Length; i++)
                {
                    incremented[i] = false;
                }
                foreach (int i in Control())
                {
                    if (incremented[i] == false)
                    {
                        CipherRotors[4 - i].IncrementPosition();
                        incremented[i] = true; // we incremented this rotor in this run
                    }
                }

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


                foreach (int i in Control())
                {
                    if (incremented[i] == false)
                    {
                        CipherRotors[4 - i].IncrementPosition();
                        incremented[i] = true; // we incremented this rotor in this run
                    }
                }



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
                    tempf = ControlRotors[i].RotSubMatRevBack[ControlRotors[i].Position, tempf];
                    tempg = ControlRotors[i].RotSubMatRevBack[ControlRotors[i].Position, tempg];
                    temph = ControlRotors[i].RotSubMatRevBack[ControlRotors[i].Position, temph];
                    tempi = ControlRotors[i].RotSubMatRevBack[ControlRotors[i].Position, tempi];
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

        public int[] Control()
        {
            //TODO: Control Rotor 1 kann abgekürzt werden, ein loojup zu viel
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
                    tempf = ControlRotors[i].RotSubMatRevBack[ControlRotors[i].Position, tempf];
                    tempg = ControlRotors[i].RotSubMatRevBack[ControlRotors[i].Position, tempg];
                    temph = ControlRotors[i].RotSubMatRevBack[ControlRotors[i].Position, temph];
                    tempi = ControlRotors[i].RotSubMatRevBack[ControlRotors[i].Position, tempi];
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

        public void setCipherRotors(int i, byte a)
        {
            
            CipherRotors[4-i] = CodeWheels[a];
            
        }

        public void setControlRotors(byte i, byte b)
        {
            
            ControlRotors[i-5] = CodeWheels[b];
            
        }

        public void setIndexRotors(byte i ,byte c)
        {
            
            IndexRotors[i] = CodeWheels[c+10];
            
        }

        public void setBool(byte ix,byte i ,bool rev)
        {
            
            if(i>4)
            {
                ControlRotors[i-5].Reverse = rev;
            }
            else
            {
                CipherRotors[4-i].Reverse = rev;
            }

            CodeWheels[ix].Reverse = rev;
            
        }

        public void setPositionsControl(byte ix,byte i, byte position)
        {
            ControlRotors[i - 5].Position = position;
            CodeWheels[ix].Position = position;
        }

        public void setPositionsIndex(byte ix, byte i, byte position)
        {
            IndexRotors[i].Position = position;
            CodeWheels[ix+10].Position = position;

        }

        public void setInternalConfig(int[] a, byte[] positions)
        {
            for (int i = 0; i < a.Length;i++ )
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

        public void setIndexMaze2(int[] steppingmaze)
        {

            for (int i = 0; i < 26;i++ )
                IndexMaze[i] = (byte)steppingmaze[i];
            
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

            CodeWheels[6] = ControlRotors[0] = new RotorByte(ConstantsByte.ControlCipherRotors[6], 0, false);
            CodeWheels[7] = ControlRotors[1] = new RotorByte(ConstantsByte.ControlCipherRotors[7], 0, false);
            CodeWheels[8] = ControlRotors[2] = new RotorByte(ConstantsByte.ControlCipherRotors[8], 0, false);
            CodeWheels[9] = ControlRotors[3] = new RotorByte(ConstantsByte.ControlCipherRotors[9], 0, false);
            CodeWheels[10] = ControlRotors[4] = new RotorByte(ConstantsByte.ControlCipherRotors[10], 0, false);

            CodeWheels[11] = IndexRotors[0] = new RotorByte(ConstantsByte.IndexRotors[1], 0, false);
            CodeWheels[12] = IndexRotors[1] = new RotorByte(ConstantsByte.IndexRotors[2], 0, false);
            CodeWheels[13] = IndexRotors[2] = new RotorByte(ConstantsByte.IndexRotors[3], 0, false);
            CodeWheels[14] = IndexRotors[3] = new RotorByte(ConstantsByte.IndexRotors[4], 0, false);
            CodeWheels[15] = IndexRotors[4] = new RotorByte(ConstantsByte.IndexRotors[5], 0, false);
        }
    }
    
}
