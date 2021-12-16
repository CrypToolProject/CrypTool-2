using LatticeCrypto.Properties;
using LatticeCrypto.Utilities;
using NTL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace LatticeCrypto.Models
{
    public class LatticeND
    {
        public VectorND[] Vectors { get; set; }
        public VectorND[] ReducedVectors { get; set; }
        public List<VectorND[]> ReductionSteps { get; set; }
        public ReductionMethods ReductionMethod { get; set; }
        public bool UseRowVectors { get; set; }
        public VectorND[] UnimodTransVectors { get; set; }
        public int N { get; private set; }
        public int M { get; private set; }
        public BigInteger Determinant { get; set; }
        public double AngleBasisVectors { get; set; }
        public double AngleReducedVectors { get; set; }
        public double Density { get; set; }
        public double DensityRelToOptimum { get; set; }
        private static readonly double optimalDensity = Math.PI / Math.Sqrt(12);

        public LatticeND()
        { }

        public LatticeND(int n, int m, bool transpose)
        {
            Vectors = new VectorND[n];
            ReducedVectors = new VectorND[n];
            N = n;
            M = m;
            for (int i = 0; i < N; i++)
            {
                Vectors[i] = new VectorND(M);
            }

            UseRowVectors = transpose;
        }

        public LatticeND(VectorND[] vectors, bool useRowVectors)
        {
            Vectors = vectors;
            UseRowVectors = useRowVectors;
            N = vectors.Length;
            M = vectors[0].dim;
            ReducedVectors = new VectorND[N];
            if (N == M)
            {
                Determinant = CalculateDeterminant(Vectors);
            }

            if (Vectors.Length == 2)
            {
                AngleBasisVectors = Vectors[0].AngleBetween(Vectors[1]);
            }
        }

        public void GenerateRandomVectors(bool checkForGoodBasis, BigInteger codomainStart, BigInteger codomainEnd)
        {
            VectorND[] newVectorNds = new VectorND[N];
            BigInteger det = 0;
            int counter = 0;
            while (true)
            {
                for (int i = 0; i < N; i++)
                {
                    BigInteger[] vector = new BigInteger[M];
                    newVectorNds[i] = new VectorND(M);

                    for (int j = 0; j < M; j++)
                    {
                        vector[j] = Util.ComputeRandomBigInt(codomainStart, codomainEnd);
                    }

                    for (int k = 0; k < vector.Length; k++)
                    {
                        newVectorNds[i].values[k] = vector[k];
                    }
                }

                if (N == M)
                {
                    det = CalculateDeterminant(newVectorNds);
                }

                if ((N != M || det != 0) && (!checkForGoodBasis || IsGoodBasis(newVectorNds)))
                {
                    break;
                }

                counter++;
                if (counter > 1024)
                {
                    throw new Exception(Languages.errorFailedToGenerateLattice);
                }
            }

            Vectors = (VectorND[])newVectorNds.Clone();
            Determinant = det;
            if (Vectors.Length == 2)
            {
                AngleBasisVectors = Vectors[0].AngleBetween(Vectors[1]);
            }
        }

        public string LatticeToString()
        {
            string lattice = "";
            for (int i = 0; i < N; i++)
            {
                lattice += FormatSettings.VectorTagOpen;
                for (int j = 0; j < M; j++)
                {
                    lattice += Vectors[i].values[j];
                    if (j < M - 1)
                    {
                        lattice += FormatSettings.CoordinateSeparator;
                    }
                }
                lattice += FormatSettings.VectorTagClosed;
                if (i < N - 1)
                {
                    lattice += FormatSettings.VectorSeparator;
                }
            }
            return FormatSettings.LatticeTagOpen + lattice + FormatSettings.LatticeTagClosed;
        }

        public string LatticeReducedToString()
        {
            string lattice = "";
            for (int i = 0; i < N; i++)
            {
                lattice += FormatSettings.VectorTagOpen;
                for (int j = 0; j < M; j++)
                {
                    lattice += ReducedVectors[i].values[j];
                    if (j < M - 1)
                    {
                        lattice += FormatSettings.CoordinateSeparator;
                    }
                }
                lattice += FormatSettings.VectorTagClosed;
                if (i < N - 1)
                {
                    lattice += FormatSettings.VectorSeparator;
                }
            }
            return FormatSettings.LatticeTagOpen + lattice + FormatSettings.LatticeTagClosed;
        }

        public string LatticeTransformationToString()
        {
            string lattice = "";
            for (int i = 0; i < N; i++)
            {
                lattice += FormatSettings.VectorTagOpen;
                for (int j = 0; j < N; j++)
                {
                    lattice += UnimodTransVectors[i].values[j];
                    if (j < N - 1)
                    {
                        lattice += FormatSettings.CoordinateSeparator;
                    }
                }
                lattice += FormatSettings.VectorTagClosed;
                if (i < N - 1)
                {
                    lattice += FormatSettings.VectorSeparator;
                }
            }
            return FormatSettings.LatticeTagOpen + lattice + FormatSettings.LatticeTagClosed;
        }

        public List<string> LatticeReductionStepsToString()
        {
            List<string> steps = new List<string>();

            foreach (VectorND[] reductionStep in ReductionSteps)
            {
                string step = "";
                for (int i = 0; i < N; i++)
                {
                    step += FormatSettings.VectorTagOpen;
                    for (int j = 0; j < M; j++)
                    {
                        step += reductionStep[i].values[j];
                        if (j < M - 1)
                        {
                            step += FormatSettings.CoordinateSeparator;
                        }
                    }
                    step += FormatSettings.VectorTagClosed;
                    if (i < N - 1)
                    {
                        step += FormatSettings.VectorSeparator;
                    }
                }
                steps.Add(FormatSettings.LatticeTagOpen + step + FormatSettings.LatticeTagClosed);
            }

            return steps;
        }

        public string VectorLengthToString()
        {
            string lengths = "";
            for (int i = 0; i < N; i++)
            {
                lengths += FormatSettings.VectorTagOpen;
                BigInteger length = 0;
                for (int j = 0; j < M; j++)
                {
                    length += BigInteger.Pow(Vectors[i].values[j], 2);
                }

                lengths += string.Format("{0:f}", Math.Sqrt((double)length));
                lengths += FormatSettings.VectorTagClosed;
                if (i < N - 1)
                {
                    lengths += FormatSettings.VectorSeparator;
                }
            }
            return FormatSettings.LatticeTagOpen + lengths + FormatSettings.LatticeTagClosed;
        }

        public string VectorReducedLengthToString()
        {
            string lengths = "";
            for (int i = 0; i < N; i++)
            {
                lengths += FormatSettings.VectorTagOpen;
                BigInteger length = 0;
                for (int j = 0; j < M; j++)
                {
                    length += BigInteger.Pow(ReducedVectors[i].values[j], 2);
                }

                lengths += string.Format("{0:f}", Math.Sqrt((double)length));
                lengths += FormatSettings.VectorTagClosed;
                if (i < N - 1)
                {
                    lengths += FormatSettings.VectorSeparator;
                }
            }
            return FormatSettings.LatticeTagOpen + lengths + FormatSettings.LatticeTagClosed;
        }

        public List<string> GetAllLatticeInfosAsStringList()
        {
            List<string> latticeInfos = new List<string>
                                            {
                                                Languages.labelLatticeBasis + ": " + LatticeToString(),
                                                Languages.labelLengthBasisVectors + " " + VectorLengthToString()
                                            };

            if (N == 2 && M == 2)
            {
                latticeInfos.Add(Languages.labelAngleBasisVectors + " " + AngleBasisVectors);
            }

            latticeInfos.Add(Languages.labelReducedLatticeBasis + ": " + LatticeReducedToString());

            if (ReductionSteps != null && ReductionSteps.Count != 0)
            {
                List<string> reductionSteps = LatticeReductionStepsToString();
                for (int i = 1; i <= reductionSteps.Count; i++)
                {
                    latticeInfos.Add(" " + string.Format(Languages.labelReductionStep, i, reductionSteps.Count) + " " + reductionSteps[i - 1]);
                }
            }

            latticeInfos.Add(Languages.labelSuccessiveMinima + " " + VectorReducedLengthToString());

            if (N == 2 && M == 2)
            {
                latticeInfos.Add(Languages.labelAngleReducedVectors + " " + AngleReducedVectors);
                latticeInfos.Add(Languages.labelDensity + " " + Density + " / " + DensityRelToOptimum);
            }

            latticeInfos.Add(Languages.labelUnimodularTransformationMatrix + ": " + LatticeTransformationToString());

            if (N == M)
            {
                latticeInfos.Add(Languages.labelDeterminant + " " + Determinant);
            }

            return latticeInfos;
        }

        public BigInteger CalculateDeterminant(VectorND[] newVectorNds)
        {
            //Es sind nur quatratische Matrizen erlaubt, daher wird N = M vorausgesetzt
            if (N != M)
            {
                throw new Exception("Non-square matrix!");
            }

            BigInteger[,] basisArray = new BigInteger[N, N];

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    basisArray[i, j] = newVectorNds[i].values[j];
                }
            }

            BigInteger det;
            using (NTL_Wrapper nativeObject = new NTL_Wrapper())
            {
                det = nativeObject.Determinant(basisArray, N);
            }
            //Absolut, da Gitterdeterminanten immer positiv sind
            return BigInteger.Abs(det);
        }

        public void LLLReduce()
        {
            ReductionMethod = ReductionMethods.reduceLLL;
            BigInteger[,] basisArray = new BigInteger[N, M];
            BigInteger[,] transArray = new BigInteger[N, N];
            List<BigInteger[,]> stepList = new List<BigInteger[,]>();

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < M; j++)
                {
                    basisArray[i, j] = Vectors[i].values[j];
                }
            }

            using (NTL_Wrapper nativeObject = new NTL_Wrapper())
            {
                nativeObject.LLLReduce(basisArray, transArray, stepList, N, M, 0.99);
            }

            ReducedVectors = new VectorND[N];
            for (int i = 0; i < N; i++)
            {
                ReducedVectors[i] = new VectorND(M);
                for (int j = 0; j < M; j++)
                {
                    ReducedVectors[i].values[j] = basisArray[i, j];
                }
            }

            if (M == 2 && N == 2)
            {
                AngleReducedVectors = ReducedVectors[0].AngleBetween(ReducedVectors[1]);
                CalculateDensity();
            }

            UnimodTransVectors = new VectorND[N];
            for (int i = 0; i < N; i++)
            {
                UnimodTransVectors[i] = new VectorND(N);
                for (int j = 0; j < N; j++)
                {
                    UnimodTransVectors[i].values[j] = transArray[i, j];
                }
            }

            ReductionSteps = new List<VectorND[]>();
            foreach (BigInteger[,] step in stepList)
            {
                VectorND[] stepVectors = new VectorND[N];
                for (int i = 0; i < N; i++)
                {
                    stepVectors[i] = new VectorND(M);
                    for (int j = 0; j < M; j++)
                    {
                        stepVectors[i].values[j] = step[i, j];
                    }
                }
                ReductionSteps.Add(stepVectors);
            }
        }

        public void GaussianReduce()
        {
            ReductionMethod = ReductionMethods.reduceGauss;
            VectorND v1 = Vectors[0];
            VectorND v2 = Vectors[1];
            ReductionSteps = new List<VectorND[]>();
            UnimodTransVectors = new VectorND[2];
            VectorND u1 = new VectorND(new BigInteger[] { 1, 0 });
            VectorND u2 = new VectorND(new BigInteger[] { 0, 1 });

            BigInteger rem;
            do
            {
                if (v1.Length > v2.Length)
                {
                    Util.Swap(ref v1, ref v2);
                    Util.Swap(ref u1, ref u2);
                    ReductionSteps.Add(new[] { v1, v2 });
                }
                BigInteger t = BigInteger.DivRem(v1 * v2, v1.LengthSquared, out rem);
                //Bei der Division von BigIntegers muss noch auf korrektes Runden geprüft werden
                if (BigInteger.Abs(rem) > v1.LengthSquared / 2)
                {
                    t += rem.Sign;
                }

                v2 = v2 - v1 * t;
                u2 = u2 - u1 * t;
                ReductionSteps.Add(new[] { v1, v2 });
            } while (v1.Length > v2.Length);

            //Damit ein spitzer Winkel entsteht
            if (Settings.Default.forceAcuteAngle)
            {
                BigInteger.DivRem(v1 * v2, v1.LengthSquared, out rem);
                if (rem.Sign == -1)
                {
                    v2 = v2 * -1;
                    u2 = u2 * -1;
                    ReductionSteps.Add(new[] { v1, v2 });
                }
            }

            ReducedVectors[0] = v1;
            ReducedVectors[1] = v2;
            UnimodTransVectors = new[] { u1, u2 };
            AngleReducedVectors = ReducedVectors[0].AngleBetween(ReducedVectors[1]);
            CalculateDensity();
        }

        private void CalculateDensity()
        {
            Density = Math.PI * Math.Pow(ReducedVectors[0].Length / 2, 2) / (double)Determinant;
            DensityRelToOptimum = Density / optimalDensity;
        }

        private static bool IsGoodBasis(IList<VectorND> vectors)
        {
            if (vectors.Count != 2)
            {
                return true;
            }
            //Entscheidung, ob eine Basis gut aussieht, über Winkel (im Bogenmaß) und über die Längen
            if ((vectors[0].Length > vectors[1].Length && vectors[0].Length > 1000 * vectors[1].Length)
                || vectors[1].Length > vectors[0].Length && vectors[1].Length > 1000 * vectors[0].Length)
            {
                return false;
            }

            return vectors[0].AngleBetween(vectors[1]) > 5;
        }

        public VectorND GetMinimalReducedVector()
        {
            VectorND minimalVector = ReducedVectors[0];
            foreach (VectorND reducedVector in ReducedVectors.Where(reducedVector => reducedVector.Length < minimalVector.Length))
            {
                minimalVector = reducedVector;
            }

            return minimalVector;
        }

        public void Transpose()
        {
            VectorND[] tempVectors = (VectorND[])Vectors.Clone();

            int tempN = N;
            N = M;
            M = tempN;

            Vectors = new VectorND[N];
            for (int i = 0; i < N; i++)
            {
                Vectors[i] = new VectorND(M);
                for (int j = 0; j < M; j++)
                {
                    Vectors[i].values[j] = tempVectors[j].values[i];
                }
            }
        }

        public MatrixND ToMatrixND()
        {
            MatrixND matrixND = new MatrixND(M, N);
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < M; j++)
                {
                    matrixND[j, i] = (double)Vectors[i].values[j];
                }
            }

            return matrixND;
        }

        public override bool Equals(object obj)
        {
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < M; j++)
                {
                    if (Vectors[i].values[j] != ((LatticeND)obj).Vectors[i].values[j])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            int hash = 0;
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < M; j++)
                {
                    hash ^= Vectors[i].values[j].GetHashCode();
                }
            }

            return hash;
        }
    }
}
