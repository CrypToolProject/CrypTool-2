/*
 * $Id: sample.c 223 2006-08-17 06:19:38Z rgb $
 *
 * See copyright in copyright.h and the accompanying file COPYING
 * See also accompanying file STS.COPYING
 *
 */

/*
 *========================================================================
 * This routine generates "samples" p-values from the test routines, where
 * each test routine is a double function that returns a single
 * p-value.  The p-value returned can be made "independent" by optionally
 * reseeding the rng for each test call according to the -i flag.
 *
 * The p-values are packed into a vector.  This vector is subjected to
 * analysis to ensure that it is uniform.  Minimally this analysis
 * consists of a straightforward Kuiper Kolmogorov-Smirnov test (the
 * Kuiper form is chosen because of its relative simplicity and because
 * it manages the tails of the interval symmetrically).  However,
 * this is where alternative tests can also be applied if/when it is
 * determined necessary and an appropriate control structure is built
 * to permit their selection.  Possibilities for tests include binning the
 * p-values and applying a discrete (Pearson chisq) analysis to determine
 * an overall p value (possibly accompanied by plotting of the binned
 * histogram of p-values for visual validation), using a confidence
 * interval test, using other KS tests (the ordinary KS test, the
 * Anderson-Darling KS test).
 *
 * However, it is to be expected that >>none of this will matter<< in
 * assessing the quality of an rng.  The issue of robustness and
 * sensitivity come into play here.  For each test, it should be
 * simple to find a region of test parameters (e.g. number of random
 * numbers tested per test) that makes a weak generator fail
 * unambiguously and consistently, generating an aggregate p-value of
 * 0.0000 (zero to four decimals) from any sensible assessment tool
 * that generates a p-value.
 *========================================================================
 */

#include "libdieharder.h"

double sample(void *testfunc())
{

 int p;
 double pks;

 if(verbose == D_SAMPLE || verbose == D_ALL){
   printf("# samples():    sample\n");
 }
 for(p=0;p<psamples;p++){

   /*
    * Reseed every sample IF input isn't from a file AND if no seed was
    * specified on the command line.
    */
   if(fromfile == 0 && Seed == 0){
     seed = random_seed();
     gsl_rng_set(rng,seed);
   }

     
   if(verbose == D_SAMPLE || verbose == D_ALL){
     printf("# sample():  %6u\n",p);
   }
   (*testfunc)();

 }

 /*
  * pvalue now holds a vector of p-values from test testfunc().
  * We perform a KS test, or (with an appropriate case switch
  * or other control mechanism) perform other tests as well.
  */
 pks = kstest_kuiper(ks_pvalue,kspi);
 if(verbose == D_SAMPLE || verbose == D_ALL){
   printf("# sample(): p = %6.3f from Kuiper Kolmogorov-Smirnov test on %u pvalue.\n",pks,kspi);
 }

 return(pks);

}

