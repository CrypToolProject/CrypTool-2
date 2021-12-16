using LatticeCrypto.Properties;
using LatticeCrypto.Utilities;
using NTL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows.Documents;

namespace LatticeCrypto.Models
{
    public class MerkleHellmanModel
    {
        public VectorND privateKey;
        public VectorND publicKey;
        public int dim;
        public BigInteger r;
        public BigInteger rI;
        public BigInteger mod;
        private const int generatingMultiplicator = 2;
        private const int maxBitLength = 8;

        public MerkleHellmanModel()
        { }

        public MerkleHellmanModel(int dim)
        {
            this.dim = dim;
            GenerateSuperincreasingSequence();
            GenerateModAndR();
            ComputePublicKey();
        }

        public MerkleHellmanModel(VectorND privateKey, BigInteger r, BigInteger mod)
        {
            dim = privateKey.dim;
            this.privateKey = privateKey;
            this.r = r;
            this.mod = mod;
            rI = ModInverse(r, mod);
            ComputePublicKey();
        }

        public string Cryptanalysis(VectorND cipher, Paragraph paragraph)
        {
            string messageBinary = "";

            foreach (BigInteger value in cipher.values)
            {
                //BigInteger tempValue = value;

                LatticeND lattice = TransformToLattice(value);

                //Anmerkung: Der zweite Versuch nach LAGARIAS/ODLYZKO, S.232 funktioniert leider nicht,
                //daher wurde dieser Teil auskommentiert

                //for (int j = 0; j <= 1; j++)
                //{
                lattice.LLLReduce();

                BigInteger[] vector = FindBinaryVector(lattice.ReducedVectors, value);

                //    if (vector != null)
                //        break;

                //    //Zweiter Versuch
                //    tempValue = Util.Sum(publicKey.ToList()) - tempValue;
                //    lattice = TransformToLattice(tempValue);
                //}

                if (vector == null)
                {
                    throw new Exception();
                }

                if (paragraph != null)
                {
                    paragraph.Inlines.Add("--------------------\r\n");
                    paragraph.Inlines.Add(new Bold(new Run(Languages.labelCurrentBlock)));
                    paragraph.Inlines.Add(" " + value + "\r\n");
                    paragraph.Inlines.Add(new Bold(new Run(Languages.labelLatticeBasis)));
                    paragraph.Inlines.Add(" " + lattice.LatticeToString() + "\r\n");
                    paragraph.Inlines.Add(new Bold(new Run(Languages.labelReducedLatticeBasis)));
                    paragraph.Inlines.Add(" " + lattice.LatticeReducedToString() + "\r\n");
                    paragraph.Inlines.Add(new Bold(new Run(Languages.labelFoundVector)));
                    paragraph.Inlines.Add(" " + new VectorND(vector) + "\r\n");
                }

                for (int k = 0; k < dim; k++)
                {
                    messageBinary += vector[k];
                }
            }

            int redudandBits = (messageBinary.Length) % maxBitLength;
            if (redudandBits > 0)
            {
                messageBinary = messageBinary.Remove(messageBinary.Length - redudandBits);
            }

            if (messageBinary.Length >= maxBitLength && messageBinary.Substring(messageBinary.Length - maxBitLength).IndexOf('1') == -1)
            {
                messageBinary = messageBinary.Remove(messageBinary.Length - maxBitLength);
            }

            if (paragraph != null)
            {
                paragraph.Inlines.Add("--------------------\r\n");
                paragraph.Inlines.Add(new Bold(new Run(Languages.labelPlainTextBinary)));
                paragraph.Inlines.Add(" " + messageBinary + "\r\n");
            }

            List<byte> bytes = new List<byte>();

            for (int i = 0; i < messageBinary.Length; i += maxBitLength)
            {
                bytes.Add(Convert.ToByte(messageBinary.Substring(i, maxBitLength), 2));
            }

            string messageUTF8 = Encoding.UTF8.GetString(bytes.ToArray());

            if (paragraph != null)
            {
                paragraph.Inlines.Add(new Bold(new Run(Languages.labelPlainTextUTF8)));
                paragraph.Inlines.Add(" " + messageUTF8 + "\r\n");
            }

            return messageUTF8;
        }

        private BigInteger[] FindBinaryVector(IList<VectorND> reducedVectors, BigInteger sum)
        {
            BigInteger[] vector = new BigInteger[publicKey.dim + 1];
            bool found = true;

            for (int i = 0; i < dim; i++)
            {
                for (int j = 0; j < dim; j++)
                {
                    //Suche nach dem Vektor mit dem richtigen Format {0, 1}
                    if (reducedVectors[i].values[j] == 0 || reducedVectors[i].values[j] == 1)
                    {
                        continue;
                    }

                    found = false;
                    break;
                }

                if (!found)
                {
                    continue;
                }

                //Teste Vektor
                BigInteger testSum = 0;
                for (int l = 0; l < dim; l++)
                {
                    testSum += reducedVectors[i].values[l] * publicKey.values[l];
                }

                if (testSum != sum)
                {
                    continue;
                }

                for (int k = 0; k < dim; k++)
                {
                    vector[k] = reducedVectors[i].values[k];
                }

                return vector;
            }

            return null;
        }


        public LatticeND TransformToLattice(BigInteger sum)
        {
            LatticeND lattice = new LatticeND(publicKey.dim + 1, publicKey.dim + 1, false);

            for (int i = 0; i < publicKey.dim; i++)
            {
                lattice.Vectors[i] = new VectorND(publicKey.dim + 1);
                lattice.Vectors[i].values[i] = 1;
                lattice.Vectors[i].values[publicKey.dim] = publicKey.values[i];
            }
            lattice.Vectors[publicKey.dim] = new VectorND(publicKey.dim + 1);
            lattice.Vectors[publicKey.dim].values[publicKey.dim] = sum * (-1);

            return lattice;
        }

        public VectorND Encrypt(string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            string messageBinary = messageBytes.Aggregate("", (current, messageByte) => current + Convert.ToString(messageByte, 2).PadLeft(maxBitLength, '0'));

            int blockCount = (int)Math.Ceiling((double)messageBinary.Length / dim);
            BigInteger[] encryptVector = new BigInteger[blockCount];

            for (int i = 0; i < messageBinary.Length; i++)
            {
                if (messageBinary[i].Equals('1'))
                {
                    encryptVector[i / dim] += publicKey.values[i % dim];
                }
            }

            return new VectorND(encryptVector);
        }

        public string Decrypt(string vectorString)
        {
            string[] array = vectorString.Split(',');
            BigInteger[] vector = new BigInteger[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                vector[i] = BigInteger.Parse(array[i]);
            }
            return Decrypt(new VectorND(vector));
        }

        public string Decrypt(VectorND vector)
        {
            BigInteger[] decryptVector = new BigInteger[vector.dim];
            for (int i = 0; i < vector.dim; i++)
            {
                BigInteger bigInt = (vector.values[i] * rI) % mod;
                decryptVector[i] = bigInt;
            }

            string messageBinary = "";
            for (int i = 0; i < decryptVector.Length; i++)
            {
                string blockPart = "";
                for (int j = privateKey.dim - 1; j >= 0; j--)
                {
                    if (decryptVector[i] >= privateKey.values[j])
                    {
                        decryptVector[i] -= privateKey.values[j];
                        blockPart += "1";
                    }
                    else
                    {
                        blockPart += "0";
                    }
                }

                //Beim letzten Block kürzen
                if (i == decryptVector.Length - 1)
                {
                    int redudandBits = (messageBinary.Length + blockPart.Length) % maxBitLength;
                    if (redudandBits > 0)
                    {
                        blockPart = blockPart.Remove(0, redudandBits);
                    }

                    if (blockPart.Length >= maxBitLength && blockPart.Substring(0, maxBitLength).IndexOf('1') == -1)
                    {
                        blockPart = blockPart.Remove(0, maxBitLength);
                    }
                }

                blockPart = new string(blockPart.Reverse().ToArray());
                messageBinary += blockPart;
            }

            List<byte> bytes = new List<byte>();

            for (int i = 0; i < messageBinary.Length; i += maxBitLength)
            {
                bytes.Add(Convert.ToByte(messageBinary.Substring(i, maxBitLength), 2));
            }

            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        private static BigInteger ModInverse(BigInteger a, BigInteger mod)
        {
            BigInteger b;
            using (NTL_Wrapper nativeObject = new NTL_Wrapper())
            {
                b = nativeObject.ModInverse(a % mod, mod);
            }
            return b;
        }


        private void ComputePublicKey()
        {
            publicKey = new VectorND(dim);

            for (int i = 0; i < dim; i++)
            {
                BigInteger bigInt = (privateKey.values[i] * r) % mod;
                publicKey.values[i] = bigInt;
            }

            Debug.WriteLine(publicKey);
        }

        private void GenerateModAndR()
        {
            BigInteger sum = Util.Sum(privateKey.values.ToList());

            mod = Util.ComputeRandomBigInt(sum, sum * 2);
            r = Util.ComputeRandomBigInt(2, sum * 2);
            while (Euclid(r, mod) != 1)
            {
                r = Util.ComputeRandomBigInt(2, sum * 2);
            }

            rI = ModInverse(r, mod);
            Debug.WriteLine(mod + " " + r + " " + rI);
        }

        public static BigInteger Euclid(BigInteger a, BigInteger b)
        {
            while (b != 0)
            {
                BigInteger h = a % b;
                a = b;
                b = h;
            }
            return a;
        }

        private void GenerateSuperincreasingSequence()
        {
            List<BigInteger> values = new List<BigInteger>(dim);

            BigInteger tempGenMult = generatingMultiplicator;
            bool sequenceFound = false;

            while (!sequenceFound)
            {

                for (int i = 0; i < dim; i++)
                {
                    BigInteger sum = Util.Sum(values);
                    if (sum == 0)
                    {
                        sum = 1;
                    }

                    values.Add(Util.ComputeRandomBigInt(sum + 1, sum * tempGenMult));
                }
                double density = dim / BigInteger.Log(values[dim - 1], 2);

                if (density > 0.646 || !IsSuperincreasingSequence(values))
                {
                    tempGenMult *= 2;
                    values = new List<BigInteger>(dim);
                }
                else
                {
                    sequenceFound = true;
                }
            }

            //Könnte hier weg und stattdessen in der dazugehörigen Test-Methode verwendet werden
            //if (!IsSuperincreasingSequence(values))
            //    throw new Exception(Languages.errorGeneratingSuperincSequence);

            privateKey = new VectorND(values.ToArray());
            Debug.WriteLine(privateKey);
        }

        public static bool IsSuperincreasingSequence(IEnumerable<BigInteger> values)
        {
            BigInteger sum = 0;

            foreach (BigInteger value in values)
            {
                if (value <= sum)
                {
                    return false;
                }

                sum += value;
            }
            return true;
        }
    }
}
