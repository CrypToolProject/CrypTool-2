/*
 * See copyright in copyright.h and the accompanying file COPYING
 */

/*
 *========================================================================
 *                        DAB Byte Distribution Test
 *
 * Extract n independent bytes from each of k consecutive words. Increment
 * indexed counters in each of n tables.  (Total of 256 * n counters.)
 * Currently, n=3 and is fixed at compile time.
 * If n>=2, then the lowest and highest bytes will be used, along
 * with n-2 bytes from the middle.
 * If the generator's word size is too small, overlapped bytes will
 * be used.
 * Current, k=3 and is fixed at compile time.
 * Use a basic chisq fitting test (chisq_pearson) for the p-value.
 * Previous version also used a chisq independence test (chisq2d); it
 * was found to be slightly less sensitive.
 * I envisioned this test as using a small number of samples and large
 * number of separate tests. Experiments so far show that keeping -p 1
 * and increasing -t performs best.
 *========================================================================
 */

#include "libdieharder.h"


#define SAMP_PER_WORD 3
#define SAMP_TOTAL (3*SAMP_PER_WORD)
#define TABLE_SIZE (256 * SAMP_TOTAL)

int dab_bytedistrib(Test **test,int irun) {
 Vtest vtest;
 unsigned int t,i,j;
 unsigned int counts[TABLE_SIZE];

 /* Zero the counters */
 memset(counts, 0, sizeof(unsigned int) * TABLE_SIZE);

 test[0]->ntuple = 0;  // Not used currently

 for(t=0;t<test[0]->tsamples;t++){
   for(i=0;i<(SAMP_TOTAL / SAMP_PER_WORD);i++){

     /*
      * Generate a word; this word will be used for SAMP_PER_WORD bytes.
      */
     unsigned int word = gsl_rng_get(rng);
     unsigned char currentShift = 0;
     for (j = 0; j < SAMP_PER_WORD; j++) {

       /*
        * Shifts:
        * The "shiftAmount" is how much the word needs to be shifted for the
        * *next* byte, since the current byte is read before shifting.
        * The calculation is such that the final byte read is always the most
        * significant byte. The middle bytes fall as they will.
        * The calculation is uglier than I wanted, but it handles fractional
        * shift amounts correctly (at least I think it does; confirmed for
        * SAMP_PER_WORD==3 and a variety of rmax_bits values).
        */
       unsigned char shiftAmount = ((j+1) * (rmax_bits - 8)) / (SAMP_PER_WORD - 1);
       unsigned int v = word & 0x0ff;    /* v is the byte sampled from the word */
       word >>= shiftAmount - currentShift;
       currentShift += shiftAmount;

       /*
        * Increment the appropriate count, faking a 3d array using a 1d array.
        * This should probably be changed, so that v is the final index and
        * not j, for clarity.
        */
       counts[v * SAMP_TOTAL + i * SAMP_PER_WORD + j]++;
     }
   }
 }

 Vtest_create(&vtest, TABLE_SIZE);
 vtest.ndof = 255 * SAMP_TOTAL;

 /*
  * Setup the chisq test, setting the actual and expected values for
  * each entry.
  */
 for(i=0;i<TABLE_SIZE;i++){
   vtest.x[i] = counts[i];
   vtest.y[i] = (double) test[0]->tsamples / 256;
 }

 Vtest_eval(&vtest);
 test[0]->pvalues[irun] = vtest.pvalue;
 Vtest_destroy(&vtest);

 return(0);
}

