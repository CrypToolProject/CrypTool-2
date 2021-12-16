using Org.BouncyCastle.Math;
using System;

namespace PKCS1.Library
{
    internal class MyFloat
    {
        private static int prec = 1024;
        private BigInteger x;
        private int e;

        public MyFloat(MyFloat z)
        {
            x = z.x;
            e = z.e;
        }

        public MyFloat()
        {
            x = BigInteger.ValueOf(0);
            e = 0;
        }


        public MyFloat(double d) //fehlerhaft
        {
            if (d != 0.0)
            {
                int l = Convert.ToInt32(Math.Log(d) / Math.Log(2.0));
                if (l < 64)
                {
                    x = to_ZZ((d * Math.Pow(2.0, 64.0 - l)));
                    e = l - 64;
                }
                else
                {
                    x = to_ZZ(d);
                    e = 0;
                }
                e += 1024;
            }
            else
            {
                x = to_ZZ(0.0);
                e = 0;
            }
            normalize();
        }

        public static void setPrec(int iPrec)
        {
            prec = iPrec;
        }

        public void normalize()
        {
            //int l = NumBits(x); // Bitlänge l von x, l(0) = 0
            int l = x.BitLength;

            if (l == 0)
            {
                e = 0;
            }
            else
            {
                int d = prec - l;

                if (d > 0)
                {
                    //x <<= d;
                    x = x.ShiftLeft(d);
                    e -= d;
                }
                if (d < 0)
                {
                    //x >>= -d;
                    x = x.ShiftRight(-d);
                    e -= d;
                }
            }
        }

        public static BigInteger to_ZZ(ref MyFloat x)
        {
            BigInteger res = x.x;

            if (x.e <= prec)
            {
                //res <<= prec + x.e;
                res = res.ShiftRight(prec - x.e);
            }
            else
            {
                //res >>= prec - x.e;
                res = res.ShiftLeft(prec - x.e);
            }

            return res;
        }

        private BigInteger to_ZZ(double x) //fehlerhaft
        {
            long bits = BitConverter.DoubleToInt64Bits(x);
            int exponent = (int)((bits >> 52) & 0x7ffL);
            long mantissa = bits & 0xfffffffffffffL;

            BigInteger res = BigInteger.ValueOf(mantissa);
            if (exponent >= 0)
            {
                res = res.ShiftLeft(prec + exponent);
            }
            else
            {
                res = res.ShiftRight(prec - exponent);
            }

            return res;
        }

        public static void to_Float(ref MyFloat res, ref BigInteger x)
        {
            res.x = x;
            //res.e = 1024;
            res.e = prec;
            res.normalize();
        }

        public static void add(ref MyFloat res, ref MyFloat op1, ref MyFloat op2)
        {
            int d = op1.e - op2.e;
            if (d >= 0)
            {
                res.x = op1.x.Add((op2.x.ShiftRight(d)));
                res.e = op1.e;
            }
            else
            {
                res.x = (op1.x.ShiftRight(-d)).Add(op2.x);
                res.e = op2.e;
            }
            res.normalize();
        }

        public void sub(ref MyFloat res, ref MyFloat op1, ref MyFloat op2)
        {
            int d = op1.e - op2.e;
            if (d >= 0)
            {
                res.x = op1.x.Subtract((op2.x.ShiftRight(d)));
                res.e = op1.e;
            }
            else
            {
                res.x = (op1.x.ShiftRight(-d)).Subtract(op1.x);
                res.e = op2.e;
            }
            res.normalize();
        }

        public static void mul(ref MyFloat res, ref MyFloat op1, ref MyFloat op2)
        {
            res.x = op1.x.Multiply(op2.x);
            res.e = op1.e + op2.e - prec;
            res.normalize();
        }

        public static void div(ref MyFloat res, ref MyFloat op1, ref MyFloat op2)
        {
            res.x = (op1.x.ShiftLeft(prec)).Divide(op2.x);
            res.e = op1.e - op2.e;
            res.normalize();
        }
    }
}
