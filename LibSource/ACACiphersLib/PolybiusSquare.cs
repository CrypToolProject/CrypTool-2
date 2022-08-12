using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrypTool.ACACiphersLib
{
    internal class PolybiusSquare
    {
        string alphabet;
        int side;

        PolybiusSquare(string alphabet, string key)
        {
            this.alphabet = key;
            side = (int)Math.Ceiling(Math.Sqrt(alphabet.Length));
        }

        public int find_index_in_alphabet(char c, string alphabet)
        {
            int k =0;
            for (int j=0;j<alphabet.Length;j++)
            {
                if(alphabet[j] == c) {
                    k = j;
                    break;
                }
            }
            return k;
        }

        public (int, int) getCoordinates(char c)
        {
            int j = 0;
            for (int i =0;i<alphabet.Length;i++)
            {
                if (alphabet[i] == c)
                {
                    j = i;
                    break;
                }
            }
            int row = j / side;
            int column = Tools.mod(j, side);
            return (row, column);
        }

        public char getChar(int row, int column)
        {
            return alphabet[row * side + column];
        }

        public int getColumns()
        {
            return side;
        }

        public int getRows()
        {
            return alphabet.Length / side;
        }

    }
}
