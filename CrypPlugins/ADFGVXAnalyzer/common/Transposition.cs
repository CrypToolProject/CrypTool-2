namespace common
{
    public class Transposition
    {
        public static void encodeWithInverseKey(Vector inverseKey, Vector plain, Vector cipher)
        {

            int keyLength = inverseKey.length;
            int fullRows = (plain.length + keyLength - 1) / keyLength;
            int longColumns = plain.length % keyLength;

            cipher.length = 0;
            for (int cCol = 0; cCol < keyLength; cCol++)
            {
                int pCol = inverseKey.TextInInt[cCol];
                int colLength = ((longColumns != 0) && (pCol >= longColumns)) ? fullRows - 1 : fullRows;
                for (int row = 0; row < colLength; row++)
                {
                    int pPos = row * keyLength + pCol;
                    cipher.append(plain.TextInInt[pPos]);
                }
            }

        }
        public static void encode(Vector key, Vector inverseKey, Vector plain, Vector cipher)
        {
            inverseKey.inverseOf(key);
            encodeWithInverseKey(inverseKey, plain, cipher);
        }
        public static void encode(Vector key, Vector plain, Vector cipher)
        {
            Vector inverseKey = new AlphabetVector(key.length, false);
            inverseKey.inverseOf(key);
            encodeWithInverseKey(inverseKey, plain, cipher);
        }
        public static void decodeWithInverseKey(Vector inverseKey, Vector cipher, Vector plain)
        {
            int keyLength = inverseKey.length;
            int fullRows = (cipher.length + keyLength - 1) / keyLength;
            int longColumns = cipher.length % keyLength;

            int cPos = 0;
            for (int cCol = 0; cCol < keyLength; cCol++)
            {
                int col = inverseKey.TextInInt[cCol];
                int colLength = ((longColumns != 0) && (col >= longColumns)) ? fullRows - 1 : fullRows;
                for (int row = 0; row < colLength; row++)
                {
                    int pPos = row * keyLength + col;
                    plain.TextInInt[pPos] = cipher.TextInInt[cPos++];
                }
            }
            plain.length = cipher.length;

        }
        public static void decode(Vector key, Vector inverseKey, Vector cipher, Vector plain)
        {
            inverseKey.inverseOf(key);
            decodeWithInverseKey(inverseKey, cipher, plain);
        }
        public static void decode(Vector key, Vector cipher, Vector plain)
        {
            Vector inverseKey = new AlphabetVector(key.length, false);
            inverseKey.inverseOf(key);
            decodeWithInverseKey(inverseKey, cipher, plain);
        }
    }
}
