using LatticeCrypto.Properties;
using NTL;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace LatticeCrypto.Models
{
    public class RSAModel
    {
        private RsaPrivateCrtKeyParameters privateKey;
        private RsaKeyParameters publicKey;

        public RSAModel(int bitSize)
        {
            GenerateNewRSA(bitSize, "3");
        }

        static RSAModel()
        {
            //Workaround to disable "small modulus" verification in "RsaKeyParameters.Validate":
            FieldInfo smallPrimesProduct = typeof(RsaKeyParameters).GetField("SmallPrimesProduct", BindingFlags.Static | BindingFlags.NonPublic);
            smallPrimesProduct.SetValue(null, Org.BouncyCastle.Math.BigInteger.One);
        }

        public void GenerateNewRSA(int bitSize)
        {
            RsaKeyPairGenerator r = new RsaKeyPairGenerator();
            r.Init(new KeyGenerationParameters(new SecureRandom(), bitSize));
            AsymmetricCipherKeyPair keys = r.GenerateKeyPair();
            privateKey = (RsaPrivateCrtKeyParameters)keys.Private;
            publicKey = (RsaKeyParameters)keys.Public;
        }

        public void GenerateNewRSA(int bitSize, string publicExponent)
        {
            RsaKeyPairGenerator r = new RsaKeyPairGenerator();
            r.Init(new RsaKeyGenerationParameters(new Org.BouncyCastle.Math.BigInteger(publicExponent), new SecureRandom(), bitSize, 80));
            AsymmetricCipherKeyPair keys = r.GenerateKeyPair();
            privateKey = (RsaPrivateCrtKeyParameters)keys.Private;
            publicKey = (RsaKeyParameters)keys.Public;
        }

        public Org.BouncyCastle.Math.BigInteger GetModulusN()
        {
            return publicKey.Modulus;
        }

        public string GetModulusNToString()
        {
            return GetModulusN().ToString();
        }

        public Org.BouncyCastle.Math.BigInteger GetPublicExponent()
        {
            return publicKey.Exponent;
        }

        public string GetPublicExponentToString()
        {
            return GetPublicExponent().ToString();
        }

        public Org.BouncyCastle.Math.BigInteger GetPrivateExponent()
        {
            return privateKey.Exponent;
        }

        public string GetPrivateExponentToString()
        {
            return GetPrivateExponent().ToString();
        }

        public byte[] GetPrimP()
        {
            RSAParameters rsa = DotNetUtilities.ToRSAParameters(privateKey);
            return rsa.P;
        }

        public string GetPrimPToString()
        {
            BigInteger bigInteger = ParseBinaryBE(GetPrimP());
            return bigInteger.ToString();
        }

        public byte[] GetPrimQ()
        {
            RSAParameters rsa = DotNetUtilities.ToRSAParameters(privateKey);
            return rsa.Q;
        }

        public string GetPrimQToString()
        {
            BigInteger bigInteger = ParseBinaryBE(GetPrimQ());
            return bigInteger.ToString();
        }

        public int GetBlockSize()
        {
            RsaEngine rsa = new RsaEngine();
            rsa.Init(true, publicKey);
            return rsa.GetInputBlockSize();
        }

        public byte[] Encrypt(byte[] message)
        {
            RsaEngine rsa = new RsaEngine();
            rsa.Init(true, publicKey);

            int blockSize = rsa.GetInputBlockSize();
            List<byte> output = new List<byte>();
            for (int chunkPosition = 0; chunkPosition < message.Length; chunkPosition += blockSize)
            {
                int chunkSize = Math.Min(blockSize, message.Length - (chunkPosition * blockSize));
                byte[] tmp = rsa.ProcessBlock(message, chunkPosition, chunkSize);
                output.AddRange(tmp);
            }
            return output.ToArray();
        }

        public BigInteger Encrypt(string message)
        {
            return ParseBinaryBE(Encrypt(Encoding.UTF8.GetBytes(message)));
        }

        public byte[] Decrypt(byte[] cipher)
        {
            RsaEngine rsa = new RsaEngine();
            rsa.Init(false, privateKey);

            int blockSize = rsa.GetInputBlockSize();
            List<byte> output = new List<byte>();
            for (int chunkPosition = 0; chunkPosition < cipher.Length; chunkPosition += blockSize)
            {
                int chunkSize = Math.Min(blockSize, cipher.Length - chunkPosition);
                byte[] tmp = cipher.ToList().GetRange(chunkPosition, chunkSize).ToArray().Reverse().ToArray();
                output.AddRange(rsa.ProcessBlock(tmp, 0, tmp.Length));
            }
            return output.ToArray();
        }

        public string Decrypt(BigInteger cipher)
        {
            return Encoding.UTF8.GetString(Decrypt(cipher.ToByteArray()));
        }

        public string StereotypedAttack(string leftText, string rightText, int unknownLength, BigInteger cipher, string h)
        {
            RsaEngine rsa = new RsaEngine();
            rsa.Init(true, publicKey);

            string solution;
            BigInteger left = ParseBinaryBE(Encoding.Default.GetBytes(leftText));
            BigInteger right = ParseBinaryBE(Encoding.Default.GetBytes(rightText));

            using (StereotypedAttack_Wrapper stereotypedAttack = new StereotypedAttack_Wrapper(GetModulusNToString(), GetPublicExponentToString(), left.ToString(), right.ToString(), unknownLength, cipher.ToString(), h))
            {
                stereotypedAttack.Attack();
                solution = stereotypedAttack.GetSolution();
            }

            if (string.IsNullOrEmpty(solution))
            {
                throw new Exception(Languages.errorNoSolutionFound);
            }

            BigInteger.TryParse(solution, out BigInteger bigInteger);
            solution = Encoding.Default.GetString(bigInteger.ToByteArray().Reverse().ToArray());
            return solution;
        }

        private static BigInteger ParseBinaryBE(IEnumerable<byte> raw)
        {
            return new BigInteger(raw.Reverse().Concat(new byte[] { 0 }).ToArray());
        }

        //static BigInteger ParseBinaryLE(IEnumerable<byte> raw)
        //{
        //    return new BigInteger(raw.Concat(new byte[] { 0 }).ToArray());
        //}
    }
}
