using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using FormatPreservingEncryptionWeydstone;

namespace FPETests
{
    [TestClass]
    public class DFFTest
    {
        public DFFTest(){
        }

        [TestMethod]
        public void ff2EquivalentToOFF1()
        {

            int radix = 10;
            int tweakRadix = 255;

            byte[] key = { (byte) 0x2B, (byte) 0x7E, (byte) 0x15, (byte) 0x16, (byte) 0x28, (byte) 0xAE, (byte) 0xD2,
                    (byte) 0xA6, (byte) 0xAB, (byte) 0xF7, (byte) 0x15, (byte) 0x88, (byte) 0x09, (byte) 0xCF, (byte) 0x4F,
                    (byte) 0x3C };
            byte[] Tweak = { (byte)0x2B, (byte)0x7E, (byte)0x15 };

            int[] plaintext = { 0, 2, 2, 2, 2 };
            int[] ciphertext;


            FF2 ff2 = new FF2(radix, tweakRadix);

            ciphertext = ff2.encrypt(key, Tweak, plaintext);

            Console.WriteLine(ciphertext);



            DFF dff = new DFF(radix, tweakRadix, new OFF1());

            int[] ciphertext2 = dff.encrypt(key, Tweak, plaintext);

            CollectionAssert.AreEqual(ciphertext, ciphertext2);

        }



        [TestMethod]
        public void stressTestOFF1()
        {

            byte[] validKey = { (byte) 0x2B, (byte) 0x7E, (byte) 0x15, (byte) 0x16, (byte) 0x28, (byte) 0xAE, (byte) 0xD2,
                    (byte) 0xA6, (byte) 0xAB, (byte) 0xF7, (byte) 0x15, (byte) 0x88, (byte) 0x09, (byte) 0xCF, (byte) 0x4F,
                    (byte) 0x3C };

            byte[] T = new byte[] { 0, 0, 1, 0 };
            int tweakRadix = 2;
            int radix;
            int[] PT;

            // part 1: each supported radix and plaintext length combined with fixed tweak and fixed tweakRadix
            // only for power of 2 radix (else it would take too long)
            for (radix = Constants.MINRADIX_FF2; radix <= Constants.MAXRADIX_FF2; radix *= 2)
            {
                DFF dff = new DFF(radix, tweakRadix, new OFF1());
                PT = new int[] { 0 };

                // for each permitted plaintext length
                for (int testLength = dff.getMinlen(); testLength <= dff.getMaxlen(); testLength++)
                {
                    // extend PT to the length testLength
                    while (PT.Length != testLength && PT.Length < testLength)
                    {
                        int[] PExtension = { radix - 1 };
                        PT = Common.concatenate(PT, PExtension);
                    }

                    //skip if format is too small 
                    if (Math.Pow(radix, testLength) < 100) continue;
                    // encrypt the plaintext
                    int[] CT = dff.encrypt(validKey, T, PT);

                    // verify decrypted ciphertext against original plaintext
                    CollectionAssert.AreEqual(PT, dff.decrypt(validKey, T, CT));

                    // verify format has been preserved. CT has same length as PT and consists of integers < radix
                    Assert.AreEqual(PT.Length, CT.Length);
                    foreach (int ctchar in CT)
                    {
                        Assert.IsTrue(ctchar < radix && ctchar >= 0);
                    }
                    // use the ciphertext as the new plaintext
                    PT = CT;
                }

            }

            //part 2: each supported tweakRadix and tweakLength combined with fixed radix and fixed PlaintextLength
            radix = 10;
            PT = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            //for each tweakRadix
            for (tweakRadix = Constants.MINRADIX_FF2; tweakRadix <= Constants.MAXRADIX_FF2; tweakRadix *= 2)
            {
                DFF dff = new DFF(radix, tweakRadix, new OFF1());
                T = new byte[] { };

                // for each permitted tweak length
                for (int testLength = 0; testLength < dff.getMaxTlen(); testLength++)
                {
                    // extend Tweak to the length testLength
                    while (T.Length != testLength && T.Length < testLength)
                    {
                        byte[] TExtension = { (byte)(tweakRadix - 1) };
                        T = Common.concatenate(T, TExtension);
                    }

                    // encrypt the plaintext
                    int[] CT = dff.encrypt(validKey, T, PT);

                    // verify decrypted ciphertext against original plaintext
                    CollectionAssert.AreEqual(PT, dff.decrypt(validKey, T, CT));

                    // verify format has been preserved. CT has same length as PT and consists of integers < radix
                    Assert.AreEqual(PT.Length, CT.Length);
                    foreach (int ctchar in CT)
                    {
                        Assert.IsTrue(ctchar < radix && ctchar >= 0);
                    }
                    // use the ciphertext as the new plaintext
                    PT = CT;
                }

            }

        }


        [TestMethod]
        public void stressTestOFF2()
        {

            byte[] validKey = { (byte) 0x2B, (byte) 0x7E, (byte) 0x15, (byte) 0x16, (byte) 0x28, (byte) 0xAE, (byte) 0xD2,
                    (byte) 0xA6, (byte) 0xAB, (byte) 0xF7, (byte) 0x15, (byte) 0x88, (byte) 0x09, (byte) 0xCF, (byte) 0x4F,
                    (byte) 0x3C };

            byte[] T = new byte[] { 0, 0, 1, 0 };
            int tweakRadix = 2;
            int radix;
            int[] PT;

            // part 1: each supported radix and plaintext length combined with fixed tweak and fixed tweakRadix
            // only for power of 2 radix (else it would take too long)
            for (radix = Constants.MINRADIX_FF2; radix <= Constants.MAXRADIX_FF2; radix *= 2)
            {
                DFF dff = new DFF(radix, tweakRadix, new OFF2());
                PT = new int[] { 0 };

                // for each permitted plaintext length
                for (int testLength = dff.getMinlen(); testLength <= dff.getMaxlen(); testLength++)
                {
                    // extend PT to the length testLength
                    while (PT.Length != testLength && PT.Length < testLength)
                    {
                        int[] PExtension = { radix - 1 };
                        PT = Common.concatenate(PT, PExtension);
                    }

                    //skip if format is too small 
                    if (Math.Pow(radix, testLength) < 100) continue;
                    // encrypt the plaintext
                    int[] CT = dff.encrypt(validKey, T, PT);

                    // verify decrypted ciphertext against original plaintext
                    CollectionAssert.AreEqual(PT, dff.decrypt(validKey, T, CT));

                    // verify format has been preserved. CT has same length as PT and consists of integers < radix
                    Assert.AreEqual(PT.Length, CT.Length);
                    foreach (int ctchar in CT)
                    {
                        Assert.IsTrue(ctchar < radix && ctchar >= 0);
                    }
                    // use the ciphertext as the new plaintext
                    PT = CT;
                }

            }

            //part 2: each supported tweakRadix and tweakLength combined with fixed radix and fixed PlaintextLength
            radix = 10;
            PT = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            //for each tweakRadix
            for (tweakRadix = Constants.MINRADIX_FF2; tweakRadix <= Constants.MAXRADIX_FF2; tweakRadix *= 2)
            {
                DFF dff = new DFF(radix, tweakRadix, new OFF2());
                T = new byte[] { };

                // for each permitted tweak length
                for (int testLength = 0; testLength < dff.getMaxTlen(); testLength++)
                {
                    // extend Tweak to the length testLength
                    while (T.Length != testLength && T.Length < testLength)
                    {
                        byte[] TExtension = { (byte)(tweakRadix - 1) };
                        T = Common.concatenate(T, TExtension);
                    }

                    // encrypt the plaintext
                    int[] CT = dff.encrypt(validKey, T, PT);

                    // verify decrypted ciphertext against original plaintext
                    CollectionAssert.AreEqual(PT, dff.decrypt(validKey, T, CT));

                    // verify format has been preserved. CT has same length as PT and consists of integers < radix
                    Assert.AreEqual(PT.Length, CT.Length);
                    foreach (int ctchar in CT)
                    {
                        Assert.IsTrue(ctchar < radix && ctchar >= 0);
                    }
                    // use the ciphertext as the new plaintext
                    PT = CT;
                }
            }
        }

    }   
}
