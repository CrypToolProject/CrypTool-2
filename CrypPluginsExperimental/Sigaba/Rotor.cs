using System;
using System.Linq;

namespace Sigaba
{
   public class Rotor
    {
        public int Position { get; set; }

        public char[] Subalpha { get; set; }
        public char[] SubalphaRev { get; set; }
        public char[] SubalphaHu { get; set; }
        public char[] SubalphaRevHu { get; set; }

        public byte[,] RotSubMat { get; set; }
        private int _rotSubMatLength0 = -1;  
        
        public byte[,] RotSubMatRev { get; set; }
        public byte[,] RotSubMatBack { get; set; }
        public byte[,] RotSubMatRevBack { get; set; }
        
        public Boolean Reverse;

        public void IncrementPosition()
        {
            if (_rotSubMatLength0 == -1)
                _rotSubMatLength0 = RotSubMat.GetLength(0);

            Position = (Position + (Reverse ? 1 : _rotSubMatLength0 - 1)) % _rotSubMatLength0;
        }
       
        public byte Ciph(int input)
        {
            return (Reverse ? RotSubMatRev : RotSubMat)[Position, input];
        }

        public byte DeCiph(int input)
        {
            return (Reverse ? RotSubMatRevBack : RotSubMatBack)[Position, input];
        }
       
        public Rotor(int[] subalpha, int position, Boolean reverse)
        {
            init(subalpha, position, reverse);
        }

        public Rotor(char[] subalpha, int position, Boolean reverse)
        {
            init(subalpha.Select(c => c - 65).ToArray(), position, reverse);
        }

        private void init(int[] subalpha, int position, Boolean reverse)
        {
            int N = subalpha.Count();

            Reverse = reverse;
            Position = position;

            RotSubMat = new byte[N, N];
            RotSubMatBack = new byte[N, N];
            RotSubMatRev = new byte[N, N];
            RotSubMatRevBack = new byte[N, N];

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    RotSubMat[i, j] = (byte)((((subalpha[(i + j) % N])) - i + N) % N);
                    RotSubMatBack[i, j] = (byte)(((Array.IndexOf(subalpha, (char)((((j + i)) % N)))) - i + N) % N);
                    RotSubMatRev[i, j] = (byte)(((i - Array.IndexOf(subalpha, (char)((((i - j + N) % N))))) + N) % N);
                    RotSubMatRevBack[i, j] = (byte)((i - (subalpha[((i - j) + N) % N]) + N) % N);
                }

                if (Sigaba.verbose)
                    Console.WriteLine(String.Join(" & ", Enumerable.Range(0, N).Select(j => RotSubMat[i, j])));
            }
        }
    }
}
