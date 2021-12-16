using System;
using System.Linq;
using System.Numerics;

namespace LatticeCrypto.Models
{
    public class VectorND
    {
        public readonly BigInteger[] values;
        public int dim;

        public VectorND(BigInteger[] values)
        {
            this.values = values;
            dim = values.Length;
        }

        public VectorND(int length)
        {
            values = new BigInteger[length];
            dim = length;
        }

        private double length = -1;
        public double Length
        {
            get
            {
                if (length == -1)
                {
                    length = Math.Sqrt((double)LengthSquared);
                }

                return length;
            }
        }

        private BigInteger lengthSquared = new BigInteger(-1);
        public BigInteger LengthSquared
        {
            get
            {
                if (lengthSquared == -1)
                {
                    lengthSquared = values.Aggregate<BigInteger, BigInteger>(0, (current, v) => current + BigInteger.Pow(v, 2));
                }

                return lengthSquared;
            }
        }

        public static BigInteger Sqrt(BigInteger n)
        {
            if (n == 0)
            {
                return 0;
            }

            if (n > 0)
            {
                int bitLength = Convert.ToInt32(Math.Ceiling(BigInteger.Log(n, 2)));
                BigInteger root = BigInteger.One << (bitLength / 2);

                while (!IsSqrt(n, root))
                {
                    root += n / root;
                    root /= 2;
                }

                return root;
            }

            throw new ArithmeticException("NaN");
        }

        private static bool IsSqrt(BigInteger n, BigInteger root)
        {
            BigInteger lowerBound = root * root;
            BigInteger upperBound = (root + 1) * (root + 1);

            return (n >= lowerBound && n < upperBound);
        }

        public static VectorND operator +(VectorND v1, VectorND v2)
        {
            return v1.Add(v2);
        }

        public VectorND Add(VectorND v2)
        {
            VectorND result = new VectorND(values.Length);
            for (int i = 0; i < dim; i++)
            {
                result.values[i] = values[i] + v2.values[i];
            }

            return result;
        }

        public static VectorND operator -(VectorND v1, VectorND v2)
        {
            return v1.Subtract(v2);
        }

        public VectorND Subtract(VectorND v2)
        {
            VectorND result = new VectorND(values.Length);
            for (int i = 0; i < dim; i++)
            {
                result.values[i] = values[i] - v2.values[i];
            }

            return result;
        }

        public static BigInteger operator *(VectorND v1, VectorND v2)
        {
            return v1.Multiply(v2);
        }

        public BigInteger Multiply(VectorND v2)
        {
            BigInteger result = 0;
            for (int i = 0; i < dim; i++)
            {
                result += values[i] * v2.values[i];
            }

            return result;
        }

        public static VectorND operator *(VectorND v1, BigInteger scalar)
        {
            return v1.Multiply(scalar);
        }

        public VectorND Multiply(BigInteger scalar)
        {
            VectorND result = new VectorND(values.Length);
            for (int i = 0; i < dim; i++)
            {
                result.values[i] = values[i] * scalar;
            }

            return result;
        }

        public double AngleBetween(VectorND v2)
        {
            return Math.Acos(((double)Multiply(v2) / (Length * v2.Length))) * (180 / Math.PI);
        }

        public override string ToString()
        {
            string vector = FormatSettings.VectorTagOpen;
            for (int i = 0; i < values.Length; i++)
            {
                vector += values[i];
                if (i < values.Length - 1)
                {
                    vector += FormatSettings.VectorSeparator + " ";
                }
            }
            vector += FormatSettings.VectorTagClosed;
            return vector;
        }
    }
}
