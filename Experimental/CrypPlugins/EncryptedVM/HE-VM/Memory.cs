using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Research.SEAL;
using System.IO;

namespace CrypTool.Plugins.EncryptedVM
{
    public class Program
    {
        public BigPolyArray[] ac = new BigPolyArray[8];
        public BigPolyArray[] pc = new BigPolyArray[8];
        public System.Collections.ArrayList memory = new System.Collections.ArrayList(); // BigPolyArray
    }

    class Memory
    {
        private Util sealutil;
        private Functions functions;
        
        public const int WORD_SIZE = 13;
        public const int ARRAY_ROWS = 256;
        public const int ARRAY_COLS = 8;

        public BigPolyArray[,] cellarray { get; private set; } = new BigPolyArray[ARRAY_ROWS, WORD_SIZE];

        private BigPolyArray ZERO;
        private BigPolyArray[] r = new BigPolyArray[ARRAY_ROWS];

        public Memory(Util p_sealutil, Functions p_functions)
        {
            sealutil = p_sealutil;
            functions = p_functions;
            ZERO = sealutil.enc_enc(0);
            for (int i = 0; i < ARRAY_ROWS; i++)
                for (int j = 0; j < WORD_SIZE; j++)
                    cellarray[i, j] = ZERO;
        }

        /*
         * a: Address
         * reg: Register
         * rw: Read/Write
         * b1: Word at address
         * return: (b2) Word at address
         */
        public BigPolyArray[] access(BigPolyArray[] a, BigPolyArray[] reg, BigPolyArray rw, BigPolyArray[] b1)
        {
            BigPolyArray nam, nam2, nrw, read, write, nsel;
            BigPolyArray[] b2 = new BigPolyArray[WORD_SIZE];
            for (int i = 0; i < WORD_SIZE; i++)
                b2[i] = ZERO;

            nrw = functions.not(rw);

            int mask, m;
            for (int row = 0; row < 4 /* ARRAY_ROWS */; row++)
            {
                mask = 4;

                nam = functions.not(a[0]);
                nam2 = functions.not(a[1]);
                r[row] = functions.and2((row & 1) > 0 ? a[0] : nam, (row & 2) > 0 ? a[1] : nam2);

                for (m = 2; m < ARRAY_COLS; m++, mask <<= 1)
                {
                    nam = functions.not(a[m]);
                    r[row] = functions.and2(r[row], (row & mask) > 0 ? a[m] : nam);
                }

                nam = functions.not(r[row]);

                for (m = 0; m < WORD_SIZE; m++)
                {
                    read = functions.and2(cellarray[row, m], r[row]);
                    write = functions.and2(b1[m], r[row]);

                    if (m < 8)
                    {
                        read = functions.and3(cellarray[row, m], r[row], rw);
                        write = functions.and3(b1[m], r[row], nrw);
                        nsel = functions.and2(cellarray[row, m], nam);
                        cellarray[row, m] = functions.or3(read, write, nsel);
                    }

                    b2[m] = functions.or3(b2[m], read, write);
                }
            }

            return b2;
        }

        public bool loadmemory(System.Collections.ArrayList memory)
        {
            if (memory.Count == 0 || memory.Count > ARRAY_ROWS * WORD_SIZE || memory[0].GetType() != typeof(BigPolyArray))
                return false;

            int i = 0, j = 0;
            foreach (BigPolyArray mem in memory)
            {
                if (j == WORD_SIZE)
                {
                    j = 0;
                    i++;
                }
                cellarray[i, j] = mem;
                j++;
            }

            return true;
        }
    }
}
