/*
 * $Id: rgb_kstest_test.c 250 2009-10-04 05:02:26Z rgb $
 *
 * See copyright in copyright.h and the accompanying file COPYING
 */

/*
 *========================================================================
 * A Kolmogorov-Smirnov test using Anderson-Darling tests a set of
 * deviates for uniformity.  So does Kuiper KS.  The AD in this suite has
 * been "symmetrized" so that it is not left biased, and is now very
 * similar to the algorithm in R.  Kuiper was invented in the first place
 * to be a symmetric test and has cyclic symmetry, not sensitive to
 * the end points of the test interval.
 *
 * The test is simple.  Fill a test vector with uniform deviates from the
 * default generator.  Run the selected KS test on them, and record the
 * pvalue in the test's vector of pvalues to pass back to the front end
 * for histogram graphing and/or a final KS test to a test pvalue.
 * 
 * This test has two purposes.  One is that it is a legitimate test of
 * rngs, although one that is perhaps too expensive to use for very large
 * tsamples because it can require the input data to be sorted.  The
 * other, more important one, is to allow me and other test developers
 * a platform to gain experience in interpreting the final set of
 * ks pvalues formed from a test of the uniformity of psamples pvalues
 * from individual tests.  These appear to be consistently biased high
 * for psamples = 100 even for what should be very good rngs, and I
 * want to see what happens when I form a large set of ks pvalues of (say)
 * 100 tsamples drawn directly as uniform deviates from known good
 * generators, ones that I am CERTAIN have no detectable bias of this
 * sort on only 100 samples.
 *========================================================================
 */

#include "libdieharder.h"

int rgb_kstest_test(Test **test, int irun)
{

 uint t,tsamples;
 double *testvec;

 tsamples = test[0]->tsamples;
 testvec = (double *)malloc(tsamples*sizeof(double));

 if(verbose == D_RGB_KSTEST_TEST || verbose == D_ALL){
     printf("Generating a vector of %u uniform deviates.\n",test[0]->tsamples);
 }
 for(t=0;t<tsamples;t++){

   /*
    * Generate and (conditionally) print out a point.
    */
   testvec[t] = gsl_rng_uniform_pos(rng);
   if(verbose == D_RGB_KSTEST_TEST || verbose == D_ALL){
       printf("testvec[%u] = %f",t,testvec[t]);
   }
 }

 if(ks_test >= 3){
   /*
    * This (Kuiper) can be selected with -k 3 from the command line.
    * All other values test variants of the regular kstest().
    */
   test[0]->pvalues[irun] = kstest_kuiper(testvec,tsamples);
 } else {
   /*
    * This (Symmetrized KS) is -k 0,1,2.  Default is 0.
    */
   test[0]->pvalues[irun] = kstest(testvec,tsamples);
 }

 free(testvec);

 if(verbose == D_RGB_KSTEST_TEST || verbose == D_ALL){
   printf("# rgb_kstest_test(): test[0]->pvalues[%u] = %10.5f\n",irun,test[0]->pvalues[irun]);
 }

 /*
  * I guess we return 0 on normal healthy return
  */
 return(0);

}

