using System;

namespace PlayfairAnalysis
{
    /// <summary>
    /// This class provides some methods equivalent to Java standard lib methods.
    /// </summary>
    public static class Arrays
    {
        public static int[] copyOf(int[] original, int newLength)
        {
            var copy = new int[newLength];
            Array.Copy(original, 0, copy, 0, newLength);
            return copy;
        }

        internal static void fill(int[] a, int val)
        {
            for (var i = 0; i < a.Length; i++)
            {
                a[i] = val;
            }
        }
        internal static void arraycopy(Array src, int srcPos, Array dest, int destPos, int length)
        {
            Array.Copy(src, srcPos, dest, destPos, length);
        }
    }
}
