using System.Text;
using System.Text.RegularExpressions;

namespace CrypTool.ACACiphersLib
{
    public class VigenereAlgorithm : Cipher
    {
        public override int[] Encrypt(int[] plaintext, int[] key)
        {
            return EncryptWithDifferentAlphabet(plaintext, key, Tools.MapTextIntoNumberSpace(LATIN_ALPHABET, LATIN_ALPHABET));
        }
        public int[] EncryptWithDifferentAlphabet(int[] plaintext, int[] key, int[] alphabet)
        {
            int plaintextlength = plaintext.Length; // improves the speed because length has not to be calculated in the loop
            int[] ciphertext = new int[plaintextlength];
            int keylength = key.Length; // improves the speed because length has not to be calculated in the loop
            int alphabetlength = alphabet.Length; // improves the speed because length has not to be calculated in the loop
            int[] lookup = new int[alphabetlength]; // improves the speed because length has not to be calculated in the loop
            for (int position = 0; position < alphabetlength; position++)
            {
                lookup[alphabet[position]] = position;
            }
            for (int position = 0; position < plaintextlength; position++)
            {
                ciphertext[position] = alphabet[(lookup[plaintext[position]] + lookup[key[position % keylength]]) % alphabetlength];
            }
            return ciphertext;
        }

        public override int[] Decrypt(int[] ciphertext, int[] key)
        {
            return DecryptWithDifferentAlphabet(ciphertext, key, Tools.MapTextIntoNumberSpace(LATIN_ALPHABET, LATIN_ALPHABET));
        }
        public int[] DecryptWithDifferentAlphabet(int[] ciphertext, int[] key, int[] alphabet)
        {
            int plaintextlength = ciphertext.Length; // improves the speed because length has not to be calculated in the loop
            int[] plaintext = new int[plaintextlength];
            int keylength = key.Length; // improves the speed because length has not to be calculated in the loop
            int alphabetlength = alphabet.Length; // improves the speed because length has not to be calculated in the loop
            int[] lookup = new int[alphabetlength]; // improves the speed because length has not to be calculated in the loop
            for (int position = 0; position < alphabetlength; position++)
            {
                lookup[alphabet[position]] = position;
            }
            for (int position = 0; position < plaintextlength; position++)
            {
                plaintext[position] = alphabet[(lookup[ciphertext[position]] - lookup[key[position % keylength]] + alphabetlength) % alphabetlength];
            }
            return plaintext;
        }

    }
}
