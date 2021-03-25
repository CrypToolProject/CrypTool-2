namespace CrypTool.Plugins.ChaCha.Helper
{
    internal static class Digits
    {
        /// <summary>
        /// Return the amount of digits this number has in the decimal system.
        /// For example, 154 has 3 digits.
        /// </summary>
        public static int GetAmountOfDigits(int n)
        {
            int count = 0;
            while (n != 0)
            {
                count++;
                n /= 10;
            }
            return count;
        }
    }
}