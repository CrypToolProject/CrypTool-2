/*
 * ========================================================================
 * $Id: sts_monobit.c 237 2006-08-23 01:33:46Z rgb $
 *
 * See copyright in copyright.h and the accompanying file COPYING
 * See also accompanying file STS.COPYING
 * ========================================================================
 */

/*
 * ========================================================================
 * This is a the monobit test, rewritten from the description in the
 * STS suite.
 *
 * Rewriting means that I can standardize the interface to gsl-encapsulated
 * routines more easily.  It also makes this my own code.
 *
 * The #if'd code below is from David Bauer, and is a bit difficult to
 * read (which ordinarily would be sufficient grounds for not using it,
 * according to my usual standards for non-obfuscating code).  It is,
 * however, 12% faster as it does the summing up of 1's vs 0's using
 * bitshifts and masks (faster operations than looped C sums).  You can
 * turn on the old code if you like, but we'll leave the default "fast".
 *
 * NOBITS = 0 means "use the fast bitmask code", where
 * NOBITS = 1 should turn the linear/obvious C code back on.
 * ========================================================================
 */

#include "libdieharder.h"

#define NOBITS 0

int sts_monobit(Test **test, int irun)
{

 int i;
 uint blens,nbits;
 Xtest ptest;

 /*
  * for display only.  1 means monobit tests 1-tuples.
  */
 test[0]->ntuple = 1;

 /*
  * ptest.x contains n_1's - n_0's = n_1's - (nbits - n_1's)
  *   or ptest.x = 2*n_1's - nbits;
  * ptest.y is the number we expect (2*n_1's = nbits, so ptest.y = 0)
  * ptest.sigma is the expected error, 1/sqrt(nbits).
  *
  * Note that the expected distribution is the "half normal" centered
  * on 0.0.  I need to figure out if this is responsible for the 1/sqrt(2)
  * in the pvalue = erfc(|y - x|/(sqrt(2)*sigma)).
  *
  * Another very useful thing to note is that we don't really need to
  * do "samples" here.  Or rather, we could -- for enough bits, the
  * distribution of means should be normal -- but we don't.
  *
  */
 /*
  * The number of bits per random integer tested.
  */
 blens = rmax_bits;
 nbits = blens*test[0]->tsamples;
 ptest.y = 0.0;
 ptest.sigma = sqrt((double)nbits);

 /*
  * NOTE WELL:  This can also be done by reading in a file!  Note
  * that if -b bits is specified, size will be "more than enough".
  */
 MYDEBUG(D_STS_MONOBIT) {
   printf("# rgb_bitdist(): Generating %lu bits in bitstring",test[0]->tsamples*sizeof(uint)*8);
 }
 ptest.x = 0;

 for(i=0;i<test[0]->tsamples;i++) {
#if NOBITS
   bitstring = gsl_rng_get(rng);
#else
   uint n = gsl_rng_get(rng);
#endif
   MYDEBUG(D_STS_MONOBIT) {
#if NOBITS
     printf("# rgb_bitdist() (nobits): rand_int[%d] = %u = ",i,bitstring);
     dumpbits(&bitstring,8*sizeof(uint));
#else
     printf("# rgb_bitdist() (bits): rand_int[%d] = %u = ",i,n);
     dumpbits(&n,8*sizeof(uint));
#endif
   }
#if NOBITS
   for(b=0;b<blens;b++){
     /*
      * This gets the integer value of the ntuple at index position
      * n in the current bitstring, from a window with cyclic wraparound.
      */
     bit = bitstring & 0x01;
     bitstring >>= 1;
     ptest.x += bit;
   }
#else
   n -= (n >> 1) & 0x55555555;
   n = (n & 0x33333333) + ((n >> 2) & 0x33333333);
   n = (n + (n >> 4)) & 0x0f0f0f0f;
   if(0) {
     n = (n * 0x01010101) >> 24;
   } else {
     n = n + (n >> 8);
     n = (n + (n >> 16)) & 0x3f;
   }
   ptest.x += n;
#endif

 }
 
 ptest.x = 2*ptest.x - nbits;
 MYDEBUG(D_STS_MONOBIT) {
   printf("mtext.x = %10.5f  ptest.sigma = %10.5f\n",ptest.x,ptest.sigma);
 }
 Xtest_eval(&ptest);
 test[0]->pvalues[irun] = ptest.pvalue;

 MYDEBUG(D_STS_MONOBIT) {
   printf("# sts_monobit(): test[0]->pvalues[%u] = %10.5f\n",irun,test[0]->pvalues[irun]);
 }

 return(0);

}

