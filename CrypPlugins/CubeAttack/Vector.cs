namespace CrypTool.CubeAttack
{
    public class Vector
    {
        /// <summary>
        /// Class attributes/members
        /// </summary>
        private int length;
        private readonly int[] element;

        /// <summary>
        /// Contructor
        /// </summary>
        public Vector(int length)
        {
            this.length = length;
            element = new int[length];
        }

        /// <summary>
        /// Properites
        /// </summary>
        public int Length
        {
            get => length;
            set => length = value;
        }

        /// <summary>
        /// Indexer
        /// </summary>
        public int this[int i]
        {
            get => GetElement(i);
            set => SetElement(i, value);
        }

        /// <summary>
        /// Internal functions for getting/setting values
        /// </summary>
        public int GetElement(int i)
        {
            if (i < 0 || i > Length - 1)
            {
                throw new MatrixVectorException("Invalid index specified");
            }

            return element[i];
        }

        public void SetElement(int i, int value)
        {
            if (i < 0 || i > Length - 1)
            {
                throw new MatrixVectorException("Invalid index specified");
            }

            element[i] = value;
        }

        /// <summary>
        /// The function multiplies the given matrix on a vector
        /// </summary>
        public static Vector Multiply(Matrix matrix1, Vector vec)
        {
            if (matrix1.Cols != vec.Length)
            {
                throw new MatrixVectorException("Operation not possible");
            }

            Vector result = new Vector(vec.Length);
            for (int i = 0; i < result.Length; i++)
            {
                for (int k = 0; k < matrix1.Cols; k++)
                {
                    result[i] ^= matrix1[i, k] * vec[k];
                }
            }

            return result;
        }

        /// <summary>
        /// Operator for the Vector object
        /// includes binary operator *
        /// </summary>
        public static Vector operator *(Matrix matrix1, Vector vec)
        { return (Multiply(matrix1, vec)); }
    }
}
