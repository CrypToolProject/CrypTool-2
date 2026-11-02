using System;
using System.Linq;
using System.Security.Cryptography;
using NativeCryptography;

// Exercise the software entry points directly, even on AES-NI-capable machines.
internal static class CbcRegression
{
    private delegate byte[] Decrypt(byte[] input, byte[] key, byte[] iv, int length);
    private static int passed;
    private static int failed;

    private static byte[] Pattern(int length, int seed)
    {
        return Enumerable.Range(0, length).Select(i => (byte)(i * 37 + seed)).ToArray();
    }

    private static void Equal(byte[] expected, byte[] actual, string description)
    {
        if (!expected.SequenceEqual(actual))
        {
            throw new Exception(description + " differs from the reference");
        }
    }

    private static void Check(string name, Func<SymmetricAlgorithm> create, byte[] key,
                              int blockBytes, Decrypt decrypt, bool supportsNullIv)
    {
        foreach (int blocks in new[] { 1, 2, 7 })
        {
            foreach (string ivKind in supportsNullIv ? new[] { "zero", "nonzero", "null" }
                                                    : new[] { "zero", "nonzero" })
            {
                string label = name + ": blocks=" + blocks + ", IV=" + ivKind;
                try
                {
                    byte[] plaintext = Pattern(blockBytes * blocks, 19);
                    byte[] iv = ivKind == "nonzero" ? Pattern(blockBytes, 1) : new byte[blockBytes];
                    byte[] ciphertext;
                    using (SymmetricAlgorithm algorithm = create())
                    {
                        algorithm.Key = key;
                        algorithm.IV = iv;
                        algorithm.Mode = CipherMode.CBC;
                        algorithm.Padding = PaddingMode.None;
                        using (ICryptoTransform encryptor = algorithm.CreateEncryptor())
                        {
                            ciphertext = encryptor.TransformFinalBlock(plaintext, 0, plaintext.Length);
                        }
                    }
                    byte[] inputCopy = (byte[])ciphertext.Clone();
                    byte[] keyCopy = (byte[])key.Clone();
                    byte[] ivCopy = ivKind == "null" ? null : (byte[])iv.Clone();
                    Equal(plaintext, decrypt(inputCopy, keyCopy, ivCopy, inputCopy.Length), "plaintext");
                    Equal(ciphertext, inputCopy, "input after decryption");
                    Equal(key, keyCopy, "key after decryption");
                    if (ivCopy != null) Equal(iv, ivCopy, "IV after decryption");
                    Equal(plaintext, decrypt(inputCopy, keyCopy, ivCopy, inputCopy.Length), "repeated decryption");
                    Console.WriteLine("PASS " + label);
                    passed++;
                }
                catch (Exception error)
                {
                    Console.WriteLine("FAIL " + label + ": " + error.Message);
                    failed++;
                }
            }
        }
    }

    public static int Main()
    {
        Check("AES-128", () => new AesCryptoServiceProvider(), Pattern(16, 11), 16,
              Crypto.decryptAES128_CBC, true);
        Check("AES-192", () => new AesCryptoServiceProvider(), Pattern(24, 11), 16,
              Crypto.decryptAES192_CBC, true);
        Check("AES-256", () => new AesCryptoServiceProvider(), Pattern(32, 11), 16,
              Crypto.decryptAES256_CBC, true);
        Check("DES", () => new DESCryptoServiceProvider(), Pattern(8, 11), 8,
              Crypto.decryptDES_CBC, true);
        // The native 3DES implementation uses two keys (K1, K2, K1).
        Check("2-key 3DES", () => new TripleDESCryptoServiceProvider(), Pattern(16, 11), 8,
              Crypto.decrypt3DES_CBC, false);
        Check("RC2-128", () => new RC2CryptoServiceProvider(), Pattern(16, 11), 8,
              (input, key, iv, length) => Crypto.decryptRC2(input, key, iv, length, 1), false);
        Console.WriteLine("Results: " + passed + " passed, " + failed + " failed");
        return failed == 0 ? 0 : 1;
    }
}
