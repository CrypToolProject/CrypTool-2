/*
 * $Id: sts_runs.c 237 2006-08-23 01:33:46Z rgb $
 *
 * See copyright in copyright.h and the accompanying file COPYING
 * See also accompanying file STS.COPYING
 *
 */

/*
 *========================================================================
 * This is a the monobit test, rewritten from the description in the
 * STS suite.
 *
 * Rewriting means that I can standardize the interface to gsl-encapsulated
 * routines more easily.  It also makes this my own code.
 *========================================================================
 */

#include "libdieharder.h"

int sts_runs(Test **test, int irun)
{

 int b,t;
 uint value;
 uint *rand_int;
 Xtest ptest;
 double pones,c00,c01,c10,c11;;

 /*
  * for display only.  2 means sts_runs tests 2-tuples.
  */
 test[0]->ntuple = 2;

 /*
  * Allocate the space needed by the test.
  */
 rand_int = (uint *)malloc(test[0]->tsamples*sizeof(uint));

 /*
  * Number of total bits from -t test[0]->tsamples = size of rand_int[]
  */
 bits = rmax_bits*test[0]->tsamples;

 /*
  * We have to initialize these a bit differently this time
  */
 ptest.x = 0.0;

 /*
  * Create entire bitstring to be tested
  */
 for(t=0;t<test[0]->tsamples;t++){
   rand_int[t] = gsl_rng_get(rng);
 }

 /*
  * Fill vector of "random" integers with selected generator.
  * NOTE WELL:  This can also be done by reading in a file!
  */
 pones = 0.0;
 c00 = 0.0;
 c01 = 0.0;
 c10 = 0.0;  /* Equal to c01 by obvious periodic symmetry */
 c11 = 0.0;
 for(b=0;b<bits;b++){
   /*
    * This gets the integer value of the ntuple at index position
    * n in the current bitstring, from a window with cyclic wraparound.
    */
   value = get_bit_ntuple(rand_int,test[0]->tsamples,2,b);
   switch(value){
     case 0:   /* 00 no new ones */
       c00++;
       break;
     case 1:   /* 01 no new ones */
       c01++;
       ptest.x++;
       break;
     case 2:   /* 10 one new one (from the left) */
       c10++;
       ptest.x++;
       pones++;
       break;
     case 3:   /* 11 one new one (from the left) */
       c11++;
       pones++;
       break;
   }
   MYDEBUG(D_STS_RUNS) {
     printf("# sts_runs(): ptest.x = %f, pone = %f\n",ptest.x,pones);
   }
 }
 /*
  * form the probability of getting a one in the entire sample
  */
 pones /= (double) test[0]->tsamples*rmax_bits;
 c00 /= (double) test[0]->tsamples*rmax_bits;
 c01 /= (double) test[0]->tsamples*rmax_bits;
 c10 /= (double) test[0]->tsamples*rmax_bits;
 c11 /= (double) test[0]->tsamples*rmax_bits;

 /*
  * Now we can finally compute the targets for the problem.
  */
 ptest.y = 2.0*bits*pones*(1.0-pones);
 ptest.sigma = 2.0*sqrt((double) bits)*pones*(1.0-pones);

 MYDEBUG(D_STS_RUNS) {
   printf(" p = %f c00 = %f c01 = %f c10 = %f c11 = %f\n",pones,c00,c01,c10,c11);
 }

 Xtest_eval(&ptest);
 test[0]->pvalues[irun] = ptest.pvalue;

 MYDEBUG(D_STS_RUNS) {
   printf("# sts_runs(): test[0]->pvalues[%u] = %10.5f\n",irun,test[0]->pvalues[irun]);
 }

 free(rand_int);

 return(0);

}

