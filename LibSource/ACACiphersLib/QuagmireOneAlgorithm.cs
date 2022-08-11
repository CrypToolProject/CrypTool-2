using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrypTool.ACACiphersLib
{
    internal class QuagmireOneAlgorithm : Cipher
    {

        public QuagmireOneAlgorithm(string[] parameters)
        {
            Parameters = Tools.MapTextIntoNumberSpace(parameters[1], LATIN_ALPHABET);
        }
        public int[] Parameters { get; }
        public override int[] Decrypt(int[] ciphertext, int[] key)
        {
            throw new NotImplementedException();
        }

        public override int[] Encrypt(int[] plaintext, int[] key)
        {
            List<int> result = new List<int>();

            int[] pt_alphabet = key;
            List<List<int>> ct_alphabet = generate_ct_alphabet(key);

            foreach (var it in plaintext.Select((x, i) => new { Value = x, Index = i }))
            {
                int a = Tools.mod(it.Index, Parameters.Length);
                int b = Tools.npWhere(pt_alphabet, it.Value);
                result.Add(ct_alphabet[Tools.mod(it.Index,Parameters.Length)][Tools.npWhere(pt_alphabet,it.Value)]);

            }
        

            return result.ToArray();
        }

        public List<List<int>> generate_ct_alphabet(int[] key)
        {
            List<List<int>>ct_alphabet = new List<List<int>>();

            foreach (var c in Parameters) {
                List<int> lst = new List<int>();
                int distance = 0;
                distance = Tools.npWhere(key,0) - c;
                for (int i = 0; i < LATIN_ALPHABET.Length; i++) {
                    lst.Add(Tools.mod(i-distance,LATIN_ALPHABET.Length));
                }
                ct_alphabet.Add(lst);
            }

            return ct_alphabet;
        }

    }
}
