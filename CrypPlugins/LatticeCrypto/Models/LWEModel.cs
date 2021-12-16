using LatticeCrypto.Utilities;
using System;

namespace LatticeCrypto.Models
{
    public class LWEModel
    {
        public int n;
        public int m;
        public int l;
        public int q;
        public MatrixND S;
        public MatrixND A;
        public MatrixND B;
        public MatrixND e;
        public MatrixND r;
        public MatrixND u;
        public double alpha;
        public double std;

        public LWEModel()
        { }

        public LWEModel(int n, int l, int q, bool isSquare)
        {
            this.n = n;
            m = isSquare ? n : (int)Math.Round(1.1 * n * Math.Log(q));
            this.l = l;
            this.q = q;
            Random random = new Random();

            S = new MatrixND(n, l);
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < l; j++)
                {
                    S[i, j] = random.Next(q);
                }
            }

            A = new MatrixND(m, n);
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    A[i, j] = random.Next(q);
                }
            }

            alpha = 1 / (Math.Sqrt(n) * Math.Pow(Math.Log(n), 2));
            std = (alpha * q) / Math.Sqrt(2 * Math.PI);

            e = new MatrixND(m, l);
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < l; j++)
                {
                    e[i, j] = (int)Math.Round(random.NextGaussian(0, std)) % q;
                }
            }

            B = (A * S + e) % q;
        }

        public LWEModel(MatrixND S, MatrixND A, MatrixND e, int q, int l)
        {
            this.S = S;
            this.A = A;
            this.e = e;
            this.q = q;
            this.l = l;

            B = (A * S + e) % q;
        }

        public void GenerateNewRandomVector()
        {
            Random random = new Random();
            r = new MatrixND(1, m);
            for (int i = 0; i < m; i++)
            {
                r[0, i] = random.Next(2);
            }

            u = r * A;
        }

        public void SetRandomVector(MatrixND randomVector)
        {
            r = randomVector;
            u = r * A;
        }

        public MatrixND Encrypt(MatrixND message)
        {
            return (r * B + message * Math.Floor((double)q / 2)) % q;
        }

        public MatrixND Decrypt(MatrixND cipher)
        {
            MatrixND result = (cipher - (u * S)) % q;
            MatrixND message = new MatrixND(1, l);

            for (int i = 0; i < l; i++)
            {
                double disZero = Math.Min(Math.Abs(q - result[0, i]), result[0, i]);
                double disOne = Math.Abs((Math.Floor((double)q / 2) - result[0, i]) % q);
                message[0, i] = disOne < disZero ? 1 : 0;
            }
            return message;
        }
    }

    //public class EncryptLWETupel
    //{
    //    public MatrixND c;
    //    public MatrixND u;

    //    public EncryptLWETupel(MatrixND c, MatrixND u)
    //    {
    //        this.c = c;
    //        this.u = u;
    //    }
    //}
}
