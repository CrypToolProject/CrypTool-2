using System;
using System.Linq;

namespace SigabaKnownPlaintext
{
    public class RotorByte
    {
        public int Position;
        public Boolean Reverse;


        public char[] Subalpha;
        public char[] SubalphaRev;
        public char[] SubalphaHu;
        public char[] SubalphaRevHu;

        public byte[,] RotSubMat;
        private int _rotSubMatLength0 = -1;
        public byte[,] RotSubMatRev;
        public byte[,] RotSubMatBack;
        public byte[,] RotSubMatRevBack;

        public void IncrementPosition()
        {
            if (_rotSubMatLength0 == -1)
            {
                _rotSubMatLength0 = RotSubMat.GetLength(0);
            }

            if (Reverse)
            {
                Position = (Position + 1) % _rotSubMatLength0;
            }
            else
            {
                Position = (_rotSubMatLength0 + Position - 1) % _rotSubMatLength0;
            }
        }
        /*
        public byte Ciph(byte input)
        {
            if (!Reverse)
                return RotSubMat[Position, input];
            return RotSubMatRev[Position, input];
        }

        public byte DeCiph(byte input)
        {
            if (!Reverse)
                return RotSubMatBack[Position, input];
            return RotSubMat[Position, input];   //stamp
        }*/

        public byte Ciph(byte input)
        {
            if (!Reverse)
                return RotSubMat[Position, input];
            return RotSubMatRev[Position, input];
        }

        public byte DeCiph(byte input)
        {
            if (!Reverse)
                return RotSubMatBack[Position, input];
            return RotSubMatRevBack[Position, input];
        }

        public RotorByte(byte[] subalpha, int position, Boolean reverse)
        {
            var subalphaCount = subalpha.Count();
            Reverse = reverse;
            Position = position;

            RotSubMat = new byte[subalphaCount, subalphaCount];
            RotSubMatBack = new byte[subalphaCount, subalphaCount];
            RotSubMatRev = new byte[subalphaCount, subalphaCount];
            RotSubMatRevBack = new byte[subalphaCount, subalphaCount];

            for (int i = 0; i < subalphaCount; i++)
            {
                for (int j = 0; j < subalphaCount; j++)
                {
                    RotSubMat[i, j] = (byte)((((subalpha[(i + j) % subalphaCount])) - i + subalphaCount) % subalphaCount);
                    RotSubMatBack[i, j] = (byte)(((Array.IndexOf(subalpha, (char)((((j + i)) % subalphaCount)))) - i + subalphaCount) % subalphaCount);
                    RotSubMatRev[i, j] = (byte)(((i - Array.IndexOf(subalpha, (char)((((i - j + subalphaCount) % subalphaCount))))) + subalphaCount) % subalphaCount);
                    RotSubMatRevBack[i, j] = (byte)((i - (subalpha[((i - j) + subalphaCount) % subalphaCount]) + subalphaCount) % subalphaCount);
                }
            }
        }

        public RotorByte(char[] subalpha, byte position, Boolean reverse)
        {
            var subalphaCount = subalpha.Count();
            Reverse = reverse;
            Position = position;

            RotSubMat = new byte[subalphaCount, subalphaCount];
            RotSubMatBack = new byte[subalphaCount, subalphaCount];
            RotSubMatRev = new byte[subalphaCount, subalphaCount];
            RotSubMatRevBack = new byte[subalphaCount, subalphaCount];

            for (int i = 0; i < subalphaCount; i++)
            {
                for (int j = 0; j < subalphaCount; j++)
                {
                    RotSubMat[i, j] =
                        (byte)((((subalpha[(i + j) % subalphaCount] - 65)) - i + subalphaCount) % subalphaCount);
                    RotSubMatBack[i, j] =
                        (byte)
                        (((Array.IndexOf(subalpha, (char)((((j + i)) % subalphaCount) + 65))) - i + subalphaCount) %
                         subalphaCount);
                    RotSubMatRev[i, j] =
                        (byte)
                        (((i - Array.IndexOf(subalpha, (char)((((i - j + subalphaCount) % subalphaCount) + 65)))) +
                          subalphaCount) % subalphaCount);
                    RotSubMatRevBack[i, j] =
                        (byte)
                        ((i - (subalpha[((i - j) + subalphaCount) % subalphaCount] - 65) + subalphaCount) %
                         subalphaCount);
                }
            }
        }
    }
}
