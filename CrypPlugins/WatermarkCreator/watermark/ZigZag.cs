namespace net.watermark
{
    /* Original Project can be found at https://code.google.com/p/dct-watermark/
    * Ported to C# to be used within CrypTool 2 by Nils Rehwald
    * Thanks to cgaffa, ZXing and everyone else who worked on the original Project for making the original Java sources available publicly
    * Thanks to Nils Kopal for Support and Bugfixing */

    // Program name: zigZag.java
    // Program features: zigZag categories, including two2one (turn one-dimensional two-dimensional) and one2two
    // (One-dimensional two-dimensional rotation) in two zigzag scan method
    // Ported to C# by Nils Rehwald

    internal class ZigZag
    {
        internal static int N = 128;

        internal virtual void one2two(int[] input, int[][] output)
        {
            int n = 0, x = 0, y = 0;
            output[x][y] = input[n];
            n++;
            while (true)
            {
                if (x == 0 && y <= N - 2)
                {
                    y++;
                    output[x][y] = input[n];
                    n++;
                    while (true)
                    {
                        x++;
                        y--;
                        output[x][y] = input[n];
                        n++;

                        if (y == 0)
                        {
                            break;
                        }
                    }
                }

                if (y == 0 && x <= N - 2)
                {
                    x++;
                    output[x][y] = input[n];
                    n++;
                    while (true)
                    {
                        x--;
                        y++;
                        output[x][y] = input[n];
                        n++;

                        if (x == 0)
                        {
                            break;
                        }
                    }
                }

                if (x == N - 1 && y < N - 2)
                {
                    y++;
                    output[x][y] = input[n];
                    n++;
                    while (true)
                    {
                        x--;
                        y++;
                        output[x][y] = input[n];
                        n++;

                        if (y == N - 1)
                        {
                            break;
                        }
                    }
                }

                if (y == N - 1 && x < N - 2)
                {
                    x++;
                    output[x][y] = input[n];
                    n++;
                    while (true)
                    {
                        x++;
                        y--;
                        output[x][y] = input[n];
                        n++;

                        if (x == N - 1)
                        {
                            break;
                        }
                    }
                }

                if (x == N - 1 && y == N - 2)
                {
                    y++;
                    output[x][y] = input[n];
                    break;
                }

            } // while

        } // 1 Dimension --> 2 Dimension

        internal virtual void two2one(int[][] input, int[] output)
        {
            int n = 0, x = 0, y = 0;
            output[n] = input[x][y];
            n++;
            while (true)
            {

                if (x == 0 && y <= N - 2)
                {
                    y++;
                    output[n] = input[x][y];
                    n++;
                    while (true)
                    {
                        x++;
                        y--;
                        output[n] = input[x][y];
                        n++;
                        if (y == 0)
                        {
                            break;
                        }
                    }
                }

                if (y == 0 && x <= N - 2)
                {
                    x++;
                    output[n] = input[x][y];
                    n++;
                    while (true)
                    {
                        x--;
                        y++;
                        output[n] = input[x][y];
                        n++;
                        if (x == 0)
                        {
                            break;
                        }
                    }
                }

                if (x == N - 1 && y < N - 2)
                {
                    y++;
                    output[n] = input[x][y];
                    n++;
                    while (true)
                    {
                        x--;
                        y++;
                        output[n] = input[x][y];
                        n++;
                        if (y == N - 1)
                        {
                            break;
                        }
                    }
                }

                if (y == N - 1 && x < N - 2)
                {
                    x++;
                    output[n] = input[x][y];
                    n++;
                    while (true)
                    {
                        x++;
                        y--;
                        output[n] = input[x][y];
                        n++;

                        if (x == N - 1)
                        {
                            break;
                        }
                    }
                }

                if (x == N - 1 && y == N - 2)
                {
                    y++;
                    output[n] = input[x][y];
                    break;
                }

            } // while
        } // 2 Dimension --> 1 Dimension
    }

}