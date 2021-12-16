using System;
using System.Collections.Generic;
using System.Numerics;

namespace LatticeCrypto.Models
{
    public class GGHModel
    {
        public MatrixND privateKeyR;
        public MatrixND privateKeyR1;
        public MatrixND publicKeyB;
        public MatrixND publicKeyB1;
        public MatrixND transU;
        //public MatrixND transU1;
        public LatticeND lattice;
        public int l = 4;
        public const int maxValueForTransU = 10;
        public int k;
        public int dim;
        public const int sigma = 1;
        public VectorND errorVector;
        private MatrixND errorVectorIntern;

        public GGHModel()
        { }

        public GGHModel(int dim, int l) : this(dim, l, null)
        { }

        public GGHModel(int dim, int l, VectorND errorVector)
        {
            this.dim = dim;
            this.l = l;
            k = (int)Math.Round(l * Math.Sqrt(dim));

            do
            {
                GeneratePrivateKey();
                GeneratePublicKey();
            } while (privateKeyR.Det() == 0 || publicKeyB.Det() == 0);

            privateKeyR1 = privateKeyR.Invert();
            publicKeyB1 = publicKeyB.Invert();

            GenerateLattice();

            if (errorVector == null || errorVector.dim != dim)
            {
                GenerateErrorVector();
            }
            else
            {
                this.errorVector = errorVector;
                errorVectorIntern = new MatrixND(dim, 1);
                for (int i = 0; i < dim; i++)
                {
                    errorVectorIntern[i, 0] = (double)errorVector.values[i];
                }
            }
        }

        public GGHModel(int dim, MatrixND privateKeyR, MatrixND publicKeyB, VectorND errorVector)
        {
            this.dim = dim;
            this.privateKeyR = privateKeyR;
            this.publicKeyB = publicKeyB;
            privateKeyR1 = privateKeyR.Invert();
            publicKeyB1 = publicKeyB.Invert();

            GenerateLattice();

            this.errorVector = errorVector;
            errorVectorIntern = new MatrixND(dim, 1);
            for (int i = 0; i < dim; i++)
            {
                errorVectorIntern[i, 0] = (double)errorVector.values[i];
            }
        }

        private void GeneratePrivateKey()
        {
            Random random = new Random();
            MatrixND S = new MatrixND(dim, dim);
            for (int i = 0; i < dim; i++)
            {
                for (int j = 0; j < dim; j++)
                {
                    S[i, j] = random.Next(-l, l);
                }
            }

            MatrixND kI = k * MatrixND.IdentityMatrix(dim, dim);

            privateKeyR = kI + S;
        }

        public void SetPrivateKeyManually(MatrixND newPrivateKeyR, bool generatePublicKey)
        {
            privateKeyR = newPrivateKeyR;
            privateKeyR1 = privateKeyR.Invert();

            if (!generatePublicKey)
            {
                return;
            }

            do
            {
                GeneratePublicKey();
            } while (publicKeyB.Det() == 0);

            publicKeyB1 = publicKeyB.Invert();
        }

        public void SetPublicKeyManually(MatrixND newPublicKeyB)
        {
            publicKeyB = newPublicKeyB;
            publicKeyB1 = publicKeyB.Invert();
        }

        public void SetErrorVectorManually(VectorND newErrorVector)
        {
            errorVector = newErrorVector;
            dim = newErrorVector.dim;
            errorVectorIntern = new MatrixND(dim, 1);
            for (int i = 0; i < dim; i++)
            {
                errorVectorIntern[i, 0] = (int)errorVector.values[i];
            }
        }

        public void GeneratePublicKey()
        {
            //Generiere zunächst eine unimodulare Matrix mittels einer oberen Dreiecksmatrix
            Random random = new Random();
            transU = new MatrixND(dim, dim);
            for (int i = 0; i < dim; i++)
            {
                for (int j = 0; j < dim; j++)
                {
                    if (i == j)
                    {
                        transU[i, j] = random.NextDouble() < 0.5 ? 1 : -1;
                    }
                    else if (i > j)
                    {
                        transU[i, j] = random.Next(-maxValueForTransU, maxValueForTransU);
                    }
                }
            }

            //transU1 = transU.Invert();

            publicKeyB = privateKeyR * transU;
        }

        public void GenerateLattice()
        {
            lattice = new LatticeND(dim, dim, false);

            //Umwandlung der Matrizeneinträge von double zu BigInt und damit Umwandlung der Matrizen zu einem Gitter

            VectorND[] privateVectors = new VectorND[dim];
            for (int i = 0; i < dim; i++)
            {
                BigInteger[] privateBigInts = new BigInteger[dim];
                for (int j = 0; j < dim; j++)
                {
                    privateBigInts[j] = new BigInteger(privateKeyR[j, i]);
                }

                privateVectors[i] = new VectorND(privateBigInts);
            }
            lattice.ReducedVectors = privateVectors;

            VectorND[] publicVectors = new VectorND[dim];
            for (int i = 0; i < dim; i++)
            {
                BigInteger[] publicBigInts = new BigInteger[dim];
                for (int j = 0; j < dim; j++)
                {
                    publicBigInts[j] = new BigInteger(publicKeyB[j, i]);
                }

                publicVectors[i] = new VectorND(publicBigInts);
            }
            lattice.Vectors = publicVectors;

            if (transU == null)
            {
                return;
            }

            VectorND[] transVectors = new VectorND[dim];
            for (int i = 0; i < dim; i++)
            {
                BigInteger[] transBigInts = new BigInteger[dim];
                for (int j = 0; j < dim; j++)
                {
                    transBigInts[j] = new BigInteger(transU[j, i]);
                }

                transVectors[i] = new VectorND(transBigInts);
            }
            lattice.UnimodTransVectors = transVectors;
        }

        public void GenerateErrorVector()
        {
            Random random = new Random();
            errorVector = new VectorND(dim);
            errorVectorIntern = new MatrixND(dim, 1);

            for (int i = 0; i < dim; i++)
            {
                int randomSigma = random.NextDouble() < 0.5 ? sigma : -sigma;
                errorVector.values[i] = randomSigma;
                errorVectorIntern[i, 0] = randomSigma;
            }
        }

        public VectorND Encrypt(string message)
        {
            char[] chars = message.ToCharArray();
            int blockCount = Math.DivRem(chars.Length, dim, out int rem);
            if (rem > 0)
            {
                blockCount++;
            }

            VectorND cipher = new VectorND(dim * blockCount);

            for (int i = 0; i < blockCount; i++)
            {
                MatrixND messagePart = new MatrixND(dim, 1);
                for (int j = 0; j < dim && chars.Length > i * dim + j; j++)
                {
                    messagePart[j, 0] = chars[i * dim + j];
                }

                MatrixND cipherPart = publicKeyB * messagePart + errorVectorIntern;
                for (int j = 0; j < dim; j++)
                {
                    cipher.values[i * dim + j] = (int)cipherPart[j, 0];
                }
            }

            return cipher;
        }

        public string Decrypt(VectorND cipher)
        {
            List<char> result = new List<char>();
            int blockCount = cipher.dim / dim;

            for (int i = 0; i < blockCount; i++)
            {
                MatrixND cipherVector = new MatrixND(dim, 1);
                for (int j = 0; j < dim; j++)
                {
                    cipherVector[j, 0] = (double)cipher.values[i * dim + j];
                }

                MatrixND babai = privateKeyR1 * cipherVector;
                for (int j = 0; j < dim; j++)
                {
                    babai[j, 0] = Math.Round(babai[j, 0]);
                }

                MatrixND messageVector = publicKeyB1 * privateKeyR * babai;

                for (int j = 0; j < dim; j++)
                {
                    int messageInt = (int)Math.Round(messageVector[j, 0]);
                    if (messageInt == 0)
                    {
                        break;
                    }

                    result.Add(Convert.ToChar(messageInt));
                }
            }

            return new string(result.ToArray());
        }

        public bool DoTheKeysFit()
        {
            try
            {
                transU = (publicKeyB * privateKeyR1);
                return Math.Abs(Math.Round(transU.Det(), 5)) == 1.00000;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
