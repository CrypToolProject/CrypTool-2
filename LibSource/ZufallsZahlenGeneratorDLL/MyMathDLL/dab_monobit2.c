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
 * Block-monobit test.
 * Since we don't know what block size to use, try a bunch of block sizes.
 * In particular, try all blocks sizes of 2^k words, where k={0..n}.
 * Not sure what n should be. Especially considering the difficulty with
 * getting a statistic out of the test.
 * ========================================================================
 */

#include "libdieharder.h"
#define BLOCK_MAX (16)

/* The evalMostExtreme function is in dab_dct.c */
extern double evalMostExtreme(double *pvalue, uint num);

int dab_monobit2(Test **test, int irun)
{
 uint i, j;
 uint blens = rmax_bits;
 uint ntup = ntuple;
 double *counts;
 uint *tempCount;
 double pvalues[BLOCK_MAX];

 /* First, find out the maximum block size to use.
  * The maximum size will be 2^ntup words.
  */
 if (ntup == 0) {
   for (j=0;j<BLOCK_MAX;j++) {
     uint nmax = blens * (2<<j);
     uint nsamp = test[0]->tsamples / (2<<j);
     if ( nsamp*gsl_ran_binomial_pdf(nmax/2,0.5,nmax) < 20 ) break;
   }
   ntup = j;
 }

 test[0]->ntuple = ntup;

 counts = (double *) malloc(sizeof(*counts) * blens * (2<<ntup));  // 1 << (ntup+1)
 memset(counts, 0, sizeof(*counts) * blens * (2<<ntup));

 tempCount = (uint *) malloc(sizeof(*tempCount) * ntup);
 memset(tempCount, 0, sizeof(*tempCount) * ntup);

 for(i=0;i<test[0]->tsamples;i++) {
   uint n = gsl_rng_get(rng);
   uint t = 1;

   // Begin: count bits
   n -= (n >> 1) & 0x55555555;
   n = (n & 0x33333333) + ((n >> 2) & 0x33333333);
   n = (n + (n >> 4)) & 0x0f0f0f0f;

   if (0) {
     n = (n * 0x01010101) >> 24;
  } else {
     n = n + (n >> 8);
     n = (n + (n >> 16)) & 0x3f;
  }
  // End: count bits

  for (j = 0; j < ntup; j++) {
    tempCount[j] += n;  // Update block count

    if ((t & i) && !(t & (i-1))) {  // If this is the start of a new block
      counts[blens * ((2<<j)-1) + tempCount[j]]++;  // Save the count
      tempCount[j] = 0;  // Reset the counter
    }
    t <<=1;
  }
 }

 /* Calculate the p-value for each block size. */
 for (j = 0; j < ntup; j++) {
   double p = chisq_binomial(counts + (blens * ((2<<j)-1)), 0.5, blens * (2<<j),
       test[0]->tsamples / (2<<j));
   pvalues[j] = p;
 }

 /* Take only the most extreme p-value, and correct its value to
  * account for the number of p-values considered.
  */
 test[0]->pvalues[irun] = evalMostExtreme(pvalues, ntup);

 nullfree(counts);
 nullfree(tempCount);

 return(0);
}

