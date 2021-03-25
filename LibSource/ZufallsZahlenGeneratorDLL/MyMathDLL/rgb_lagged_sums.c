/*
 * See copyright in copyright.h and the accompanying file COPYING
 */

/*
 *========================================================================
 *                         RGB Lagged Sums Test
 *
 * This is a very simple test template for a test that generates a single
 * trivial statistic -- in fact, it is the demo user test from the
 * dieharder CLI sources.  Use a GSL-wrapped generator to generate the
 * rands required to sample that number, obtaining an iid (independent,
 * identically distributed) set of samples.  find their mean.  Determine
 * the probability of obtaining that particular value by a random trial
 * (from the erf of the associated normal distribution) -- this is the
 * "p-value" of the trial.
 *
 * The interesting thing about this test is that -- simple as it is -- when
 * it is run on an rng for a series of possible lags, it suffices to show
 * that e.g. mt19937 is actually weak because it is TOO UNIFORM -- the set
 * of pvalues that result from performing the lagged sum test for certain
 * lags come out too uniformly distributed between 0 and 1 so that the
 * final KS test yields a pvalue of e.g. 0.9995... -- a two in ten thousand
 * chance -- for two or three lags in the range 0-32.  Similar weakness is
 * actually observed for rgb_permutations.  This might not affect most
 * simulations based on the generator -- many of them would if anything
 * benefit from a slight "over-uniformity" of the random number stream,
 * especially when it is only apparent for certain lags and seeds.  Others
 * might fail altogether because certain tails in the space of random
 * numbers aren't being sampled at the expected rate.  It is very much
 * caveat emptor for users of pseudo-random number generators as all of them
 * are likely weak SOMEWHERE -- dieharder just gives you a microscope to use
 * to reveal their specific weaknesses and the likely bounds where they
 * won't matter.
 *========================================================================
 */

#include "libdieharder.h"

int rgb_lagged_sums(Test **test,int irun)
{

 uint t,i,lag;
 Xtest ptest;

 /*
  * Get the lag from ntuple.  Note that a lag of zero means
  * "don't throw any away".
  */
 test[0]->ntuple = ntuple;
 lag = test[0]->ntuple;

 /*
  * ptest.x = actual sum of tsamples lagged samples from rng
  * ptest.y = tsamples*0.5 is the expected mean value of the sum
  * ptest.sigma = sqrt(tsamples/12.0) is the standard deviation
  */
 ptest.x = 0.0;  /* Initial value */
 ptest.y = (double) test[0]->tsamples*0.5;
 ptest.sigma = sqrt(test[0]->tsamples/12.0);

 if(verbose == D_RGB_LAGGED_SUMS || verbose == D_ALL){
   printf("# rgb_lagged_sums(): Doing a test with lag %u\n",lag);
 }

 for(t=0;t<test[0]->tsamples;t++){

   /*
    * A VERY SIMPLE test, but sufficient to demonstrate the
    * weaknesses in e.g. mt19937.
    */

   /* Throw away lag per sample */
   for(i=0;i<lag;i++) gsl_rng_uniform(rng);

   /* sample only every lag numbers, reset counter */
   ptest.x += gsl_rng_uniform(rng);

 }

 Xtest_eval(&ptest);
 test[0]->pvalues[irun] = ptest.pvalue;

 if(verbose == D_RGB_LAGGED_SUMS || verbose == D_ALL){
   printf("# rgb_lagged_sums(): ks_pvalue[%u] = %10.5f\n",irun,test[0]->pvalues[irun]);
 }

 return(0);

}

void help_rgb_lagged_sums()
{

  printf("%s",rgb_lagged_sums_dtest.description);

}
