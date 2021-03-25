using Microsoft.Research.SEAL;

namespace CrypTool.Plugins.EncryptedVM
{
    class Functions
    {
        public int opcountand { get; private set; }
        public int opcountxor { get; private set; }
        public int opcountnot { get; private set; }
        private Util sealutil;
        private BigPolyArray ZERO, ONE;

        public Functions(Util p_sealutil)
        {
            sealutil = p_sealutil;
            ZERO = sealutil.enc_enc(0);
            ONE = sealutil.enc_enc(1);
        }

        public int decode(BigPolyArray poly)
        {
            return sealutil.dec_dec(poly);
        }

        public int decode2(BigPolyArray[] polys, int size)
        {
            int res = 0;
            for (int i = size - 1; i >= 0; i--)
            {
                res |= sealutil.dec_dec(polys[i]);
                if (i > 0)
                    res <<= 1;
            }
            return res;
        }

        public BigPolyArray[] encode(int size, int value)
        {
            int mask = 1;
            BigPolyArray[] ret = new BigPolyArray[size];
            for (int i = 0; i < size; i++, mask <<= 1)
                ret[i] = sealutil.enc_enc(((value & mask) > 0) ? 1 : 0);
            return ret;
        }

        public BigPolyArray and2(BigPolyArray value1, BigPolyArray value2)
        {
            opcountand++;
            return sealutil.mul(value1, value2);
        }

        public BigPolyArray and3(BigPolyArray value1, BigPolyArray value2, BigPolyArray value3)
        {
            opcountand += 2;
            // return sealutil.mul(sealutil.mul(value1, value2), value3);
            return sealutil.mul_many(new BigPolyArray[] { value1, value2, value3 });
        }

        public BigPolyArray and4(BigPolyArray value1, BigPolyArray value2, BigPolyArray value3, BigPolyArray value4)
        {
            opcountand += 3;
            // return sealutil.mul(sealutil.mul(sealutil.mul(value1, value2), value3), value4);
            return sealutil.mul_many(new BigPolyArray[] { value1, value2, value3, value4 });
        }

        public BigPolyArray and8(BigPolyArray value1, BigPolyArray value2, BigPolyArray value3, BigPolyArray value4,
                            BigPolyArray value5, BigPolyArray value6, BigPolyArray value7, BigPolyArray value8)
        {
            opcountand += 7;
            // return sealutil.mul(sealutil.mul(sealutil.mul(sealutil.mul(sealutil.mul(sealutil.mul(sealutil.mul(value1, value2), value3), value4), value5), value6), value7), value8);
            return sealutil.mul(and4(value1, value2, value3, value4), and4(value5, value6, value7, value8));
        }

        public BigPolyArray xor(BigPolyArray value1, BigPolyArray value2)
        {
            opcountxor++;
            return sealutil.add(value1, value2);
        }

        public BigPolyArray not(BigPolyArray value)
        {
            opcountnot++;
            return sealutil.add(value, ONE);
        }

        public BigPolyArray or2(BigPolyArray value1, BigPolyArray value2)
        {
            // return xor(and2(value1, value2), xor(value1, value2));
            return sealutil.add_many(new BigPolyArray[] { sealutil.mul(value1, value2), value1, value2 });
        }

        public BigPolyArray or3(BigPolyArray value1, BigPolyArray value2, BigPolyArray value3)
        {
            // return or2(or2(value1, value2), value3);
            return sealutil.add_many(new BigPolyArray[] { sealutil.mul(value1, value2), sealutil.mul(value1, value3), sealutil.mul(value2, value3), value1, value2, value3 });
        }

        public BigPolyArray or4(BigPolyArray value1, BigPolyArray value2, BigPolyArray value3, BigPolyArray value4)
        {
            return or2(or3(value1, value2, value3), value4);
        }

        public BigPolyArray or6(BigPolyArray value1, BigPolyArray value2, BigPolyArray value3,
                           BigPolyArray value4, BigPolyArray value5, BigPolyArray value6)
        {
            return or3(or4(value1, value2, value3, value4), value5, value6);
        }

        public BigPolyArray or15(BigPolyArray value1, BigPolyArray value2, BigPolyArray value3, BigPolyArray value4,
                            BigPolyArray value5, BigPolyArray value6, BigPolyArray value7, BigPolyArray value8,
                            BigPolyArray value9, BigPolyArray value10, BigPolyArray value11, BigPolyArray value12,
                            BigPolyArray value13, BigPolyArray value14, BigPolyArray value15)
        {
            return or3(or6(value1, value2, value3, value4, value5, value6), or6(value7, value8, value9, value10, value11, value12), or3(value13, value14, value15));
        }

        public BigPolyArray[] ha(BigPolyArray value1, BigPolyArray value2)
        {
            return new BigPolyArray[] { xor(value1, value2), and2(value1, value2) }; // sum, carry
        }

        public BigPolyArray[] fa(BigPolyArray value1, BigPolyArray value2, BigPolyArray cin)
        {
            BigPolyArray v1Xv2 = xor(value1, value2);
            return new BigPolyArray[] { xor(v1Xv2, cin), or2(and2(v1Xv2, cin), and2(value1, value2)) };  // sum, carry
        }

        public BigPolyArray[] ALU_add(BigPolyArray[] a, BigPolyArray[] b, BigPolyArray cin)
        {
            BigPolyArray[] res = new BigPolyArray[Memory.ARRAY_COLS + 1];
            BigPolyArray[] t = new BigPolyArray[2];

            for (int i = 0; i < Memory.ARRAY_COLS; i++)
            {
                t = fa(a[i], b[i], cin);
                res[i] = t[0];
                cin = t[1];
            }

            res[8] = cin;

            return res;
        }

        public BigPolyArray[] ALU_addadr(BigPolyArray[] a, BigPolyArray[] b)
        {
            BigPolyArray[] res = new BigPolyArray[Memory.ARRAY_COLS];
            BigPolyArray[] t = new BigPolyArray[2];
            BigPolyArray cin = ZERO;

            for (int i = 0; i < Memory.ARRAY_COLS; i++)
            {
                t = fa(a[i], b[i], cin);
                res[i] = t[0];
                cin = t[1];
            }

            return res;
        }
    }
}
