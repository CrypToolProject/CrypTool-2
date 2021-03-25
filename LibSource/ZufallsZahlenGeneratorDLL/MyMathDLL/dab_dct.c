/*
 * See copyright in copyright.h and the accompanying file COPYING
 */

/*
 *========================================================================
 *                 DAB DCT (Frequency Analysis) Test
 *
 * This test performs a Discrete Cosine Transform (DCT) on the output of
 * the RNG. More specifically, it performs tsamples transforms, each over
 * an independent block of ntuple words. If tsamples is large enough, the
 * positions of the maximum (absolute) value in each transform are
 * recorded and subjected to a chisq test for uniformity/independence. [1]
 * (A standard type II DCT is used.)
 * 
 * If tsamples is smaller than or equal to 5 times ntuple then a fallback
 * test will be used, whereby all DCT values are converted to p-values
 * and tested for uniformity via a KS test. This version is significantly
 * less sensitive, and is not recommended.
 *
 * Power: With the right parameters, this test catches more GSL
 * generators than any other; however, that count is biased by each of
 * the randomNNN generators having three copies.
 *
 * Limitations: ntuple is required to be a power of 2, because a radix 2
 * algorithm is used to calculate the DCT.
 *
 * False positives: targets are (mostly) calculated exactly, however it
 * will still return false positives when ntuple is small and tsamples is
 * very large. For the default ntuple value of 256, I get bad scores with
 * about 100 million or more tsamples (psamples set to 1).
 *
 * [1] The samples are taken as unsigned integers, and the DC coefficient
 * is adjusted to compensate for this.
 */

#include "libdieharder.h"

#define RotL(x,N)    (rmax_mask & (((x) << (N)) | ((x) >> (rmax_bits-(N)))))

void fDCT2(const unsigned int input[], double output[], size_t len);
void iDCT2(const double input[], double output[], size_t len);
void fDCT2_fft(const unsigned int input[], double output[], size_t len);
double evalMostExtreme(double *pvalue, unsigned int num);

/*
 * Discrete Cosine Transform (frequency or energy compaction) test.
 * So, it is easy to say that we want to do a DCT of the data as a test. But,
 * after we take a DCT, then what?
 * I tested a number of different methods for generating a p value from the
 * DCT.  I was actually quite surprised at the method that worked the best...
 * First off, the output of the DCT should be random-looking, normally
 * distributed with a mean of zero and standard deviation of 1, after some
 * adjustments. (The first element requires different adjustments than the
 * rest.) The adjustments are trivial to calculate.
 * There are t-sample DCTs taken; each DCT is on ntuple values.
 * 1. Generate a p value for every DCT value (t-sample * nutple of them),
 *    and run a single kstest on them all.
 *   -- Worked second best; surprisngly slow for some parameters.
 * 2. Find the single worst DCT value, generate a p-value, and compensate.
 *  -- Generally less powerful than #1. the only exception was generator #20.
 * 3. Sum each DCT vector, producing a result of length ntuple.
 *   A. Generate a p value for each sum, run ks test on them.
 *  -- Weaker than #2 even.
 *   B. Find the single worst sum, generate a p-value, and compensate.
 *  -- Also weaker than #2.
 * 4. Find the position of the most extreme value in each DCT. Run a chisq
 *    on the vector of counts of how many times each position was the most
 *    extreme.  Requires t-samples to be > 5*ntuple.
 *  -- The best! Amazingly, fails the ENTIRE random* family for the right
 *     parameters!! (Also, seems to avoid some false positives I was
 *     getting from #1.)
 *
 * On the current false positives: for the primary method the false
 * positives occur because the first and (ntuple/2)+1'th samples are the
 * largest too often (and about as often as each other).
 */

int dab_dct(Test **test,int irun)
{
 double *dct;
 unsigned int *input;
 double *pvalues = NULL;
 unsigned int i, j;
 unsigned int len = (ntuple == 0) ? 256 : ntuple;
 int rotAmount = 0;
 unsigned int v = 1<<(rmax_bits-1);
 double mean = (double) len * (v - 0.5);

 /* positionCounts is only used by the primary test, and not by the
  * fallback test.
  */
 double *positionCounts;

 /* The primary method is a chisq; we want expected counts of at least
  * five. If the number of tsamples is too low for that, use the
  * fallback method, which is doing kstest across the pvalues.
  */
 int useFallbackMethod = (test[0]->tsamples > 5 * len) ? 0 : 1;

 /* ptest, v, and sd are only used in the fall-back method, when
  * tsamples is too small compared to ntuple.
  */
 Xtest ptest;
 double sd = sqrt((1.0/6.0) * len) * v;

 dct = (double *) malloc(sizeof(double) * len);
 input = (unsigned int *) malloc(sizeof(unsigned int) * len);
 positionCounts = (double *) malloc(sizeof(double) * len);

 if (useFallbackMethod) {
   pvalues = (double *) malloc(sizeof(double) * len * test[0]->tsamples);
 }

 /* Zero out the counts initially. */
 memset(positionCounts, 0, sizeof(double) * len);

 test[0]->ntuple = len;

 /* When used, the data is normalized first. */
 ptest.y = 0.0;
 ptest.sigma = 1.0;

 /* Main loop runs tsamples times. During each iteration, a vector
  * of length ntuple will be read from the generator, so a total of
  * (tsamples * ntuple) words will be read from the RNG.
  */
 for (j=0; j<test[0]->tsamples; j++) {
   unsigned int pos = 0;
   double max = 0;

   /* Change the rotation amount after each quarter of the samples
    * have been used.
    */
   if (j != 0 && (j % (test[0]->tsamples / 4) == 0)) {
     rotAmount += rmax_bits/4;
   }

   /* Read (and rotate) the actual rng words. */
   for (i=0; i<len; i++) {
     input[i] = gsl_rng_get(rng);
     input[i] = RotL(input[i], rotAmount);
   }

   /* Perform the DCT */
   fDCT2_fft(input, dct, len);

   /* Adjust the first value (the DC coefficient). */
   dct[0] -= mean;
   dct[0] /= sqrt(2);  // Experimental + guess; seems to be correct.

   if (!useFallbackMethod) {
     /* Primary method: find the position of the largest value. */
     for (i=0; i<len; i++) {
       if (fabs(dct[i]) > max) {
         pos = i;
         max = fabs(dct[i]);
       }
     }
     /* And record it. */
     positionCounts[pos]++;
   } else {
     /* Fallback method: convert all values to pvalues. */
     for (i=0; i<len; i++) {
       ptest.x = dct[i] / sd;
       Xtest_eval(&ptest);
       pvalues[j*len + i] = ptest.pvalue;
     }
   }
 }

 if (!useFallbackMethod) {
   /* Primary method: perform a chisq test for uniformity
    * of discrete counts. */
   double p;
   double *expected = (double *) malloc(sizeof(double) * len);
   for (i=0; i<len; i++) {
     expected[i] = (double) test[0]->tsamples / len;
   }
   p = chisq_pearson(positionCounts, expected, len);
   test[0]->pvalues[irun] = p;
   free(expected);
 } else {
   /* Fallback method: perform a ks test for uniformity of the
    * continuous p-values. */
   test[0]->pvalues[irun] = kstest(pvalues, len * test[0]->tsamples);
 }

 nullfree(positionCounts);
 nullfree(pvalues);  /* Conditional; only used in fallback */
 nullfree(input);
 nullfree(dct);

 return(0);
}

void help_dab_dct()
{
  printf("%s",dab_dct_dtest.description);
}

/* Not sure why, but prototype is not included from header. */
int gsl_fft_real_radix2_transform (double data[], size_t stride, size_t n);

/*
 * Perform a type-II DCT using GSL's FFT function.
 * Assumes len is a power of 2
 */
void fDCT2_fft(const unsigned int input[], double output[], size_t len) {
 double *fft_data;
 int i;

 /*
  * David, please check this -- do you mean to call fDCT2 and then return?
  * ISO C forbids a return with expression in a void function.
  */
 if (len <= 4) {
   fDCT2(input, output, len);
   return;
 }

 /* Allocate the new vector and zero all of the elements.
  * The even elements will remain zero.
  */
 fft_data = (double *) malloc(sizeof(double) * 4 * len);
 memset(fft_data, 0, sizeof(double) * 4 * len);

 for (i = 0; i < len; i++) fft_data[2*i + 1] = input[i];
 for (i = 1; i < 2*len; i++) fft_data[4*len - i] = fft_data[i];

 gsl_fft_real_radix2_transform(fft_data, 1, 4*len);
 for (i = 0; i < len; i++) output[i] = fft_data[i] / 2;

 free(fft_data);

}

/*
 * Old, now only used if working on a very short vector.
 * Simple (direct) implementation of the DCT, type II.
 * O(n^2) run time -- TODO  replace with faster implementation.
 * Note:  the GSL library has lots of FFTs, DWT, and DHT, but not DCT!
 * DCT can be efficiently implemented using FFT, though.
 */
void fDCT2(const unsigned int input[], double output[], size_t len) {
 unsigned int i, j;
 memset(output, 0, sizeof(double) * len);

 for (i = 0; i < len; i++) {
   for (j = 0; j < len; j++) {
     output[i] += (double) input[j] * cos((PI / len) * (0.5 + j) * i);
   }
 }
}


/*
 * Old, no longer used.
 * Note: scaling of output was done empiricaclly to make the output
 * actually match the input.
 * I'm a little concerned that the scaling is different than expected,
 * but the scaling factor isn't important to how I'm expecting to
 * use it, anyway.
 * (Actually, I made it expecting to only use the forward transform.
 *  I implemented the inverse transform as a quick way to check for
 *  bugs.)
 */
void iDCT2(const double input[], double output[], size_t len) {
 unsigned int i, j;

 for (i = 0; i < len; i++) {
   double sum = 0;
   for (j = 0; j < len; j++) {
     sum += input[j] * cos(((PI * j) / len) * (0.5 + i));
   }
   output[i] = (sum - (input[0] / 2.0)) / (len / 2);
 }
}

/*
 * No longer used here; code should be saved and used elsewhere, though.
 * Given a set of p-values, this function returns a p-value indicating the
 * probability of the most extreme p-value occuring.
 */
double evalMostExtreme(double *pvalue, unsigned int num) {
 double ext = 1.0;
 int sign = 1;
 unsigned int i;
 unsigned int pos = 0;

 for (i = 0; i < num; i++) {
   double p = pvalue[i];
   int cursign = -1;
   if (p > 0.5) {
     p = 1-p;
     cursign = 1;
   }
   if (p < ext) {
     ext = p;
     sign = cursign;
     pos = i;
   }
 }

 ext = pow(1.0-ext, num);
 if (sign == 1) ext = 1.0-ext;

 return ext;
}

int main_dab_dct() {
 unsigned int input[] = { 4, 5, 6, 5, 4, 3, 2, 1, 1, 2, 3, 4, 5, 6, 7, 8 };
 double output1[16], output2[16], output3[16];
 int i;

 fDCT2(input, output1, 16);
 iDCT2(output1, output2, 16);
 fDCT2_fft(input, output3, 16);

 for (i = 0; i < 16; i++) {
   printf("%d: %d %f %f %f\n", i, input[i], output1[i], output3[i], output2[i]);
 }

 return 0;
}

