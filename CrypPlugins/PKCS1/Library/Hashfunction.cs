using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System.Text;

namespace PKCS1.Library
{
    internal class Hashfunction
    {
        private static IDigest hashFunctionDigest = DigestUtilities.GetDigest(HashFuncIdentHandler.SHA1.diplayName); // default SHA1

        public static byte[] generateHashDigest(string input, HashFunctionIdent hashIdent)
        {
            byte[] bInput = Encoding.ASCII.GetBytes(input);
            return generateHashDigest(ref bInput, ref hashIdent);
        }

        public static byte[] generateHashDigest(ref byte[] input, ref HashFunctionIdent hashIdent)
        {
            hashFunctionDigest = DigestUtilities.GetDigest(hashIdent.diplayName);
            byte[] hashDigest = new byte[hashFunctionDigest.GetDigestSize()];
            hashFunctionDigest.BlockUpdate(input, 0, input.Length);
            hashFunctionDigest.DoFinal(hashDigest, 0);

            return hashDigest;
        }

        // gibt länge in bytes zurück!
        public static int getDigestSize()
        {
            return hashFunctionDigest.GetDigestSize();
        }

        public static string getAlgorithmName()
        {
            return hashFunctionDigest.AlgorithmName;
        }
    }
}
