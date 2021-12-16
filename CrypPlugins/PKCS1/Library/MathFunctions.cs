using Org.BouncyCastle.Math;

//using Emil.GMP;

namespace PKCS1.Library
{
    internal class MathFunctions
    {
        // Heron Algorithmus
        public static BigInteger cuberoot(BigInteger radicant)
        {
            int i = 0;
            BigInteger biStart = BigInteger.ValueOf(1000);
            BigInteger biFix2 = BigInteger.Two;
            BigInteger biFix3 = BigInteger.Three;
            BigInteger biFromBevor = BigInteger.Zero;

            while (!biStart.Equals(biFromBevor))
            {
                biFromBevor = biStart;
                // (2 * biStart + (x/ biStart^2)) / 3
                biStart = biFix2.Multiply(biStart).Add(radicant.Divide(biStart.Pow(2))).Divide(biFix3);
                i++;
            }
            return biStart;
        }

        public static bool compareBigInt(BigInteger value1, BigInteger value2, int length)
        {
            byte[] array1 = value1.ToByteArray();
            byte[] array2 = value2.ToByteArray();

            return compareByteArray(ref array1, ref array2, length);
        }

        public static bool compareByteArray(ref byte[] array1, ref byte[] array2, int length)
        {
            for (int i = length - 1; i > 0; i--)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static BigInteger cuberoot4(BigInteger BigIntRad, int prec)
        {
            //BigInteger x;                    // ZZ: Langzahl-Integer
            MyFloat a = new MyFloat();
            MyFloat xi = new MyFloat();
            MyFloat x3 = new MyFloat();
            MyFloat two = new MyFloat(); // RR: Gleitkommazahlen beliebiger Präzision
            MyFloat three = new MyFloat(); // RR: Gleitkommazahlen beliebiger Präzision

            MyFloat.setPrec(prec);

            //x = BigIntRad;

            BigInteger BigInt2 = BigInteger.Two;
            MyFloat.to_Float(ref two, ref BigInt2);

            BigInteger BigInt3 = BigInteger.Three;
            MyFloat.to_Float(ref three, ref BigInt3);

            // 1. Startwert für die Approximation berechnen (mit double)
            //appr_cr_x = exp( 1.0/3.0 * log(x) );  


            // 2. Startwert (xi) und Ausgangswert (a=x) in Gleitkommazahl mit hoher Präzision überführen
            //a  = to_RR(x);
            MyFloat.to_Float(ref a, ref BigIntRad);

            MyFloat tmp = new MyFloat();
            BigInteger tmp2 = BigInteger.ValueOf(BigIntRad.BitLength);
            MyFloat.to_Float(ref tmp, ref tmp2);
            //xi = to_RR(appr_cr_x);
            //xi = new MyFloat(appr_cr_x);
            //MyFloat.div(ref xi, ref a,ref tmp);
            BigInteger start = BigIntRad.ShiftRight(BigIntRad.BitLength * 2 / 3);
            MyFloat.to_Float(ref xi, ref start);


            // 3. Halley's konvergierende Folge (x[i+1] = xi*(xi^3 + 2*a)/(2*xi^3 + a) --> x^(1/3)) mit 200 Iterationen -- *nicht optimiert*
            //two = to_RR(2.0);
            //two = new MyFloat(2.0);

            for (int i = 0; i < 200; i++)
            {
                //x3 = xi*xi*xi;
                MyFloat.mul(ref x3, ref xi, ref xi);
                MyFloat.mul(ref x3, ref x3, ref xi);
                //xi = (xi*(x3 + two * a)) / ( two * x3 + a );

                //xi = xi*( (x3 + two * a) / ( two * x3 +a ) );
                MyFloat twoA = new MyFloat();
                MyFloat.mul(ref twoA, ref two, ref a);

                MyFloat left = new MyFloat();
                MyFloat.add(ref left, ref x3, ref twoA);


                MyFloat twoX3 = new MyFloat();
                MyFloat.mul(ref twoX3, ref two, ref x3);

                MyFloat right = new MyFloat();
                MyFloat.add(ref right, ref twoX3, ref a);

                MyFloat division = new MyFloat();
                MyFloat.div(ref division, ref left, ref right);

                MyFloat.mul(ref xi, ref xi, ref division);
            }

            return MyFloat.to_ZZ(ref xi);
        }
    }
}