using System;
using System.Numerics;

namespace FormatPreservingEncryptionWeydstone
{
    internal class FF2Parameters : FFXParameters
    {
        /**
         * Construct a new FF2Parameters instance with the specified radix and tweakRadix.
         * 
         * @param radix
         *            the radix for FF2 operations
         * @param tweakRadix
         *            the tweakRadix for FF2 operations
         */
        public FF2Parameters(int radix, int tweakRadix)
        {
            ciphers = new Ciphers();
            this.radix = radix;
            ff2Round = new FF2RoundFunction(radix, ciphers, tweakRadix);
        }

        /**
	     * The radix specified in this parameter set.
	     */
        private readonly int radix;

        /**
	     * Instances of AES ciphers for CIPH algorithms.
	     */
        private readonly Ciphers ciphers;

        /**
         * The tweakRadix specified in this parameter set.
         */
        private readonly int tweakRadix;

        /**
	     * Split function for FF2.
	     */
        private readonly SplitFunction ff2Splitter = new FF2SplitFunction();

        private class FF2SplitFunction : SplitFunction
        {
            public int split(int n)
            {
                // validate n
                if (n < Constants.MINLEN || n > Constants.MAXLEN)
                {
                    throw new ArgumentException(
                            "n must be in the range [" + Constants.MINLEN + ".." + Constants.MAXLEN + "].");
                }

                return Common.floor(n / 2.0);
            }
        }

        /**
	     * Function to determine the number of Feistel rounds for FF2.
	     */
        private readonly RoundCounter ff2RoundCounter = new FF2RoundCounter();

        private class FF2RoundCounter : RoundCounter
        {
            public int rnds(int n)
            {
                return 10;
            }
        }

        /**
	     * Round function F for FF2, derived from NIST SP 800-38G draft version and VAES3 specifications.
	     */

        private readonly RoundFunction ff2Round;

        private class FF2RoundFunction : RoundFunction
        {
            public FF2RoundFunction(int radix, Ciphers ciphers, int tweakRadix)
            {
                this.ciphers = ciphers;
                this.radix = radix;
                this.tweakRadix = tweakRadix;
            }

            private readonly int radix;

            private readonly int tweakRadix;

            private readonly Ciphers ciphers;

            public bool validKey(byte[] K)
            {
                // validate K
                if (K == null)
                {
                    return false;
                }

                return true;
            }


            public int[] F(byte[] K, int n, byte[] T, int i, int[] B)
            {

                /* The FF2 Encrypt Algorithm
                 * 1.  Let  u =  Common.floor( n /2 ) ;  v  =  n  –  u . 
                 * 2.   Let  A  =  X [1 ..  u ]; B  =   X [ u  + 1 ..  n ] .  
                 * 3.  If  t >0,  P = [ radix ] 1  || [ t ] 1  || [ n ] 1  || [ NUM tweak radix ( T )] 13 ; else P = [ radix ] 1  || [ 0] 1  || [ n ] 1  || [ 0] 13 .  
                 * 4.  Let  J =   CIPH K ( P )  
                 * 5.  For i from 0 to 9: 
                 *      i.  Let  Q ←  [ i ] 1  ||  [ NUM  radix  ( B )] 15 
                 *      ii.  Let  Y  ← CIPH J ( Q ).  
                 *      iii  Let  y  ←  NUM 2 ( Y ).  
                 *      iv.  If  i  is  even, let  m =  u ; else, let  m  =  v .   
                 *      v.   Let  c  =  ( NUM radix ( A ) +  y ) mod  radix  m .  
                 *      vi.  Let  C  =  STR m radix ( c ). 
                 *      vii.   Let  A  =  B .  
                 *      viii.    Let  B  =  C .  
                 * 6.  Return A  ||  B .   
                 * 
                 */

                // Converts the Tweak byte array to an integer array, to be able to use integer specific methodes of the class Common (e.g. Common.num(int[],int)).
                // Alternativly these methods could be overloaded to process byte[] inputs. This conversion only occurs once per encryption, hence it shouldnt have a noticeable effect on the performance.
                int[] Ti = Array.ConvertAll(T, c => (int)c);
                int t = T.Length;

                if (Constants.CONFORMANCE_OUTPUT)
                {
                    Console.WriteLine("Round #" + i + "\n");
                }

                // 1. Let u = Common.floor(n/2); v = n - u.
                int u = Common.floor(n / 2.0);
                int v = n - u;
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    Console.WriteLine("Step 1\n\tu is " + u + ", v is " + v);
                }

                // 2. Let A = X[1..u]; B = X[u + 1..n].
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    Console.WriteLine("Step 2\n\tB is " + Common.intArrayToString(B));
                }

                // 3.If  t > 0,  P = [radix]^1 || [t]^1 || [n]^1 || [NUM tweakRadix(T)]^13
                //          else P = [radix]^1 || [0]^1 || [n]^1 || [0]^13 . 
                byte[] tbr = Common.bytestring(radix, 1);
                byte[] fbn = Common.bytestring(n, 1);

                byte[] P = { tbr[0] };
                if (T.Length > 0)
                {
                    byte[] fbt = Common.bytestring(t, 1);
                    P = Common.concatenate(P, new byte[] { fbt[0], fbn[0] });
                    P = Common.concatenate(P, Common.bytestring(Common.num(Ti, tweakRadix), 13));
                }
                else
                {
                    P = Common.concatenate(P, new byte[] { 0x00, fbn[0] });
                    P = Common.concatenate(P, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00
                , 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00});
                }

                if (Constants.CONFORMANCE_OUTPUT)
                {
                    Console.WriteLine("Step 3\n\tP is " + Common.unsignedByteArrayToString(P));
                }

                // 4. Let  J = CIPH (K,P)
                // CIPHK is applied to P in  Step  4  to produce  a  128-bit subkey, J

                byte[] J = ciphers.ciph(K, P);

                if (Constants.CONFORMANCE_OUTPUT)
                {
                    Console.WriteLine("Step 4\n\tsubkey J is " + Common.unsignedByteArrayToString(J));
                }

                // i. Let  Q ←  [ i ] 1  ||  [ NUM  radix  ( B )]^15 
                byte[] Q = Common.bytestring(i, 1);
                Q = Common.concatenate(Q, Common.bytestring(Common.num(B, radix), 15));
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    Console.WriteLine("\tStep 4.ii\n\t\tQ is " + Common.unsignedByteArrayToString(Q));
                }

                // ii.  Let  Y  ← CIPH J ( Q ).  
                byte[] Y = ciphers.ciph(J, Q);
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    Console.WriteLine("\tStep 4.ii\n\t\tY is " + Common.unsignedByteArrayToString(Y));
                }

                // iii  Let  y  ←  NUM 2 ( Y ).  
                BigInteger y = Common.num(Y);
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    Console.WriteLine("\tStep 4.iii\n\t\ty is " + y);
                }

                // iv.  If  i  is  even, let  m =  u ; else, let  m  =  v .   
                int m = i % 2 == 0 ? u : v;
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    Console.WriteLine("\tStep 4.iv\n\t\tm is " + m);
                }

                // constrain y to the range [0..radix^m]
                y = Common.mod(y, BigInteger.Pow(radix, m));


                // 5.  Let  C  =  STR m radix ( y ). 
                int[] C = Common.str(y, radix, m);
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    Console.WriteLine("\tStep 4.vi\n\t\tC is " + Common.intArrayToString(C));
                }

                return C;
            }
        };



        public int getRadix()
        {
            return radix;
        }


        public int getMinLen()
        {
            return Constants.MINLEN;
        }


        public int getMaxLen()
        {
            return Constants.MAXLEN;
        }


        public int getMinTLen()
        {
            return 0;
        }


        public int getMaxTLen()
        {
            return Constants.MAXLEN;
        }


        public ArithmeticFunction getArithmeticFunction()
        {
            return FFX.getBlockwiseArithmeticFunction(radix);
        }


        public FeistelMethod getFeistelMethod()
        {
            return FeistelMethod.TWO;
        }


        public SplitFunction getSplitter()
        {
            return ff2Splitter;
        }


        public RoundCounter getRoundCounter()
        {
            return ff2RoundCounter;
        }


        public RoundFunction getRoundFunction()
        {
            return ff2Round;
        }
    }
}
