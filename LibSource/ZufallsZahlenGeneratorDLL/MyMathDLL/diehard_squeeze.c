/*
 * See copyright in copyright.h and the accompanying file COPYING
 */

/*
 *========================================================================
 * This is the Diehard Squeeze test, rewritten from the description
 * in tests.txt on George Marsaglia's diehard site.
 *
 *               This is the SQUEEZE test                        ::
 *  Random integers are floated to get uniforms on [0,1). Start- ::
 *  ing with k=2^31=2147483647, the test finds j, the number of  ::
 *  iterations necessary to reduce k to 1, using the reduction   ::
 *  k=ceiling(k*U), with U provided by floating integers from    ::
 *  the file being tested.  Such j's are found 100,000 times,    ::
 *  then counts for the number of times j was <=6,7,...,47,>=48  ::
 *  are used to provide a chi-square test for cell frequencies.  ::
 *
 *                   Comment on SQUEEZE
 * This adaptation fixes a number of screwy fortranisms in the
 * original code.  First of all, the actual bin probabilities were
 * presented scaled up by 10^6 (suitable for 10^6 trials).  Then
 * they were multipled by 0.1.  Finally the test was run for 10^5
 * trials.  Now we just input the vector of actual bin probabilities
 * as doubles (naturally) and scale the probabilities by tsamples.
 * This yields the expected bin frequencies much more simply and
 * in a way that permits tsamples to be varied.
 *
 * Honestly, from my limited experimentation, this test is uselessly
 * insensitive on at least the rng's in the GSL with a few notable
 * exceptions (the worst of the worst).  It passes a number of
 * generators with known, serious flaws though.  Not that a lot of
 * tests in diehard ARE terribly  good at picking out particular
 * flaws in particular generators -- they're struggling to identify
 * bad generators at all.  Sorry, but that's just the way I see it.
 *========================================================================
 */


#include "libdieharder.h"

static double sdata[]={
0.00002103, 0.00005779, 0.00017554, 0.00046732, 0.00110783,
0.00236784, 0.00460944, 0.00824116, 0.01362781, 0.02096849,
0.03017612, 0.04080197, 0.05204203, 0.06283828, 0.07205637,
0.07869451, 0.08206755, 0.08191935, 0.07844008, 0.07219412,
0.06398679, 0.05470931, 0.04519852, 0.03613661, 0.02800028,
0.02105567, 0.01538652, 0.01094020, 0.00757796, 0.00511956,
0.00337726, 0.00217787, 0.00137439, 0.00084970, 0.00051518,
0.00030666, 0.00017939, 0.00010324, 0.00005851, 0.00003269,
0.00001803, 0.00000982, 0.00001121
};

int diehard_squeeze(Test **test, int irun)
{

 int i,j,k;
 Vtest vtest;

 /*
  * Squeeze counts the iterations required to reduce 2^31 to
  * to 1 with k = ceiling(k*U) where U is a uniform deviate. It
  * does this test[0]->tsamples times, binning the result in a vector from
  * <= 6 to >= 48 (where it has nontrivial support).  A chisq test
  * on the vector (Vtest) then yields a pvalue for the test run.
  */

 /*
  * for display only.  0 means "ignored".
  */
 test[0]->ntuple = 0;

 /*
  * Allocate memory for Vtest struct vector (length 51) and initialize
  * it with the expected values.
  */
 Vtest_create(&vtest,43);
 /*
  * Initialize the expected value vector
  */
 vtest.cutoff = 5.0;
 for(i=0;i<43;i++){
   vtest.y[i] = test[0]->tsamples*sdata[i];
 }
 memset(vtest.x,0,43*sizeof(double));

 /*
  * Test this.
  */
 MYDEBUG(D_DIEHARD_SQUEEZE) {
   for(i=0;i<43;i++){
     printf("%d:   %f    %f\n",i+6,vtest.x[i],vtest.y[i]);
   }
 }

 /*
  * We now squeeze test[0]->tsamples times.
  */
 for(i=0;i<test[0]->tsamples;i++){
   k = 2147483647;
   j = 0;

   /* printf("%d:   %d\n",j,k); */
   while((k != 1) && (j < 48)){
     k = ceil(k*gsl_rng_uniform(rng));
     j++;
     /* printf("%d:   %d\n",j,k); */
   }

   /*
    * keep j in range 6-48 inclusive and increment the test/counting vector.
    */
   j = (j<6)?6:j;
   vtest.x[j-6]++;

 }

 MYDEBUG(D_DIEHARD_SQUEEZE) {
   for(i=0;i<43;i++){
     printf("%d:   %f    %f\n",i+6,vtest.x[i],vtest.y[i]);
   }
 }

 Vtest_eval(&vtest);
 test[0]->pvalues[irun] = vtest.pvalue;

 MYDEBUG(D_DIEHARD_SQUEEZE) {
   printf("# diehard_squeeze(): test[0]->pvalues[%u] = %10.5f\n",irun,test[0]->pvalues[irun]);
 }

 Vtest_destroy(&vtest);

 return(0);

}

