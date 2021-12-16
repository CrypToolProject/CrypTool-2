namespace LatticeCrypto.Models
{
    internal class ObsoleteClasses
    {
        //public class Lattice2D
        //{
        //    public Vector Vector1 { get; set; }
        //    public Vector Vector2 { get; set; }
        //    public Vector Vector1Reduced { get; set; }
        //    public Vector Vector2Reduced { get; set; }
        //    public double Determinant { get; set; }
        //    public double Volume { get; set; }

        //    public Lattice2D()
        //    {
        //        GenerateRandomVectors();
        //    }

        //    public Lattice2D(Vector v1, Vector v2)
        //    {
        //        Vector1 = v1;
        //        Vector2 = v2;
        //        Determinant = Vector.Determinant(Vector1, Vector2);
        //        Volume = Math.Abs(Determinant);
        //        Vector[] reduced = GaussianReduce(new[] { v1, v2 });
        //        Vector1Reduced = reduced[0];
        //        Vector2Reduced = reduced[1];
        //    }

        //    public LatticeND ConvertToLatticeND()
        //    {
        //        return new LatticeND(2)
        //        {
        //            Vectors = new[,]
        //                                          {
        //                                              {(int) Vector1.X, (int) Vector1.Y},
        //                                              {(int) Vector2.X, (int) Vector2.Y}
        //                                          }
        //        };
        //    }

        //    public void GenerateRandomVectors()
        //    {
        //        Random random = new Random();
        //        while (true)
        //        {
        //            int x1 = random.Next(-30, 30);
        //            int x2 = random.Next(-30, 30);
        //            int y1 = random.Next(-30, 30);
        //            int y2 = random.Next(-30, 30);
        //            Vector1 = new Vector(x1, y1);
        //            Vector2 = new Vector(x2, y2);
        //            Determinant = Vector.Determinant(Vector1, Vector2);
        //            Volume = Math.Abs(Determinant);

        //            if (Determinant != 0 && IsGoodBasis(Vector1, Vector2))
        //                break;
        //        }
        //        Vector[] reduced = GaussianReduce(new[] { Vector1, Vector2 });
        //        Vector1Reduced = reduced[0];
        //        Vector2Reduced = reduced[1];
        //    }

        //    public string LatticeToString()
        //    {
        //        return FormatSettings.LatticeTagOpen + FormatSettings.VectorTagOpen + Vector1.X + FormatSettings.CoordinateSeparator + Vector1.Y + FormatSettings.VectorTagClosed + FormatSettings.VectorSeparator + FormatSettings.VectorTagOpen + Vector2.X + FormatSettings.CoordinateSeparator + Vector2.Y + FormatSettings.VectorTagClosed + FormatSettings.LatticeTagClosed;
        //    }

        //    public string LatticeReducedToString()
        //    {
        //        return FormatSettings.LatticeTagOpen + FormatSettings.VectorTagOpen + Vector1Reduced.X + FormatSettings.CoordinateSeparator + Vector1Reduced.Y + FormatSettings.VectorTagClosed + FormatSettings.VectorSeparator + FormatSettings.VectorTagOpen + Vector2Reduced.X + FormatSettings.CoordinateSeparator + Vector2Reduced.Y + FormatSettings.VectorTagClosed + FormatSettings.LatticeTagClosed;
        //    }

        //    public string VectorLengthToString()
        //    {
        //        return FormatSettings.LatticeTagOpen + FormatSettings.VectorTagOpen + String.Format("{0:f}", Vector1.Length) + FormatSettings.VectorTagClosed + FormatSettings.VectorSeparator + FormatSettings.VectorTagOpen + String.Format("{0:f}", Vector2.Length) + FormatSettings.VectorTagClosed + FormatSettings.LatticeTagClosed;
        //    }

        //    public string VectorReducedLengthToString()
        //    {
        //        return FormatSettings.LatticeTagOpen + FormatSettings.VectorTagOpen + String.Format("{0:f}", Vector1Reduced.Length) + FormatSettings.VectorTagClosed + FormatSettings.VectorSeparator + FormatSettings.VectorTagOpen + String.Format("{0:f}", Vector2Reduced.Length) + FormatSettings.VectorTagClosed + FormatSettings.LatticeTagClosed;
        //    }

        //    public static Vector[] GaussianReduce(Vector[] vectors)
        //    {
        //        Vector v1 = vectors[0];
        //        Vector v2 = vectors[1];

        //        do
        //        {
        //            if (v1.Length > v2.Length)
        //            {
        //                Vector temp = v1;
        //                v1 = v2;
        //                v2 = temp;
        //            }
        //            int t = (int)Math.Round(Vector.Multiply(v1, v2) / Math.Pow(v1.Length, 2));
        //            v2 = Vector.Subtract(v2, Vector.Multiply(t, v1));

        //        } while (v2.Length < v1.Length);

        //        return new[] { v1, v2 };
        //    }

        //    public static bool IsGoodBasis(Vector v1, Vector v2)
        //    {
        //        //Entscheidung, ob eine Basis gut aussieht, über Determinante und Winkel
        //        double det = Vector.Determinant(v1, v2);
        //        double angle = Math.Abs(Math.Round(Vector.AngleBetween(v1, v2))) % 360;

        //        return Math.Abs(det) > 20 && angle > 5;
        //    }
        //}


        //[Obsolete("Use LLLReduce")]
        //public void LLLReduceOld()
        //{
        //    //Zunächst Referenz auflösen
        //    VectorND[] b = Vectors.Select(vectorNd => new VectorND(vectorNd.values.Clone() as int[])).ToArray();

        //    const double c = 2;
        //    int m = b.Length;
        //    VectorND[] bStar = new VectorND[m];
        //    int[,] u = new int[m, m];
        //    int[] B = new int[m];

        //    bStar[0] = b[0];
        //    B[0] = VectorND.DotProduct(bStar[0], bStar[0]);

        //    int k = 0;
        //    while (k < m - 1)
        //    {
        //        for (int i = k; i < m; i++)
        //        {
        //            bStar[i] = b[i];

        //            for (int j = 0; j < i; j++)
        //            {
        //                u[i, j] = VectorND.DotProduct(b[i], bStar[j]) / B[j];
        //                bStar[i] = VectorND.Subtract(bStar[i], u[i, j] * bStar[j]);
        //            }

        //            B[i] = VectorND.DotProduct(bStar[i], bStar[i]);

        //            Reduce(i, ref b, ref u);
        //        }

        //        if (B[k] <= c * B[k + 1])
        //        {
        //            k++;
        //        }
        //        else
        //        {
        //            Swap(ref bStar[k], ref bStar[k + 1]);
        //            Swap(ref b[k], ref b[k + 1]);
        //            k = Math.Max(k - 1, 0);
        //        }
        //    }

        //    ReducedVectors = b;
        //    Debug.WriteLine(ReducedToString());
        //}

        //[Obsolete("Use LLLReduce")]
        //private static void Reduce(int i, ref VectorND[] b, ref int[,] u)
        //{
        //    for (int j = i - 1; j >= 0; j--)
        //    {
        //        b[i] = VectorND.Subtract(b[i], u[i, j] * b[j]);
        //        for (int k = 0; k < b.Length; k++)
        //            u[k, i] = u[k, i] - (u[i, j] * u[k, j]);
        //    }
        //}

        //[Obsolete("Use LLLReduce")]
        //public Boolean IsLLLReduced()
        //{
        //    VectorND[] b = (VectorND[])Vectors.Clone();
        //    const double c = 2;
        //    int m = b.Length;
        //    VectorND[] bStar = new VectorND[m];
        //    int[,] u = new int[m, m];

        //    for (int i = 0; i < m; i++)
        //    {
        //        u[i, i] = 1;
        //        bStar[i] = b[i];
        //        for (int j = 0; j < i; j++)
        //        {
        //            u[j, i] = (VectorND.DotProduct(b[i], bStar[j]) / VectorND.DotProduct(bStar[j], bStar[j]));
        //            bStar[i] = VectorND.Subtract(bStar[i], u[j, i] * bStar[j]);
        //        }
        //    }

        //    for (int k = 0; k < m - 1; k++)
        //        if (VectorND.DotProduct(bStar[k], bStar[k]) > c * VectorND.DotProduct(bStar[k + 1], bStar[k + 1]))
        //            return false;
        //    return true;
        //}
    }
}
