/*
 * ========================================================================
 * $Id: diehard_birthdays.c 250 2006-10-10 05:02:26Z rgb $
 *
 * See copyright in copyright.h and the accompanying file COPYING
 * ========================================================================
 */

/*
 * ========================================================================
 * This is a version of the Diehard Birthdays test, modified from the
 * original Dieharder implementation of it.
 *
 *                 This is a BIRTHDAY SPACINGS TEST
 * Choose m birthdays in a year of n days.  List the spacings between 
 * the birthdays.  If j is the number of values that occur more than 
 * once in that list, then j is asympotically Poisson distributed with 
 * mean lambda = m^3/(4n).  A Chi-Sq test is performed comparing the 
 * seen distribution of repeated spacings to the Poisson distribution. 
 * Simulations show that the approximation is better for larger n and 
 * smaller lambda.  However, since for any given run j must be an 
 * integer, a small lambda value requires more runs to build up a good 
 * statistic.  This test uses m=1700 as the default, but it may 
 * changed (via the -n (ntuple) option), up to a maximum value of 
 * 4096.  The value of n is fixed by the choice of generator, with 
 * n=2^r, where r is the number of bits per word in the generator's 
 * output (a maximum of 32 for this version of Dieharder).  This test 
 * prefers a larger t-count (-t option) and p-value samples set to 1 
 * (-p 1, which is the default).
 *
 * Be careful when running this test against generators with reduced 
 * word sizes, as it may give false positives.  When it doubt, check 
 * against an assumed good generator that is set to produce the same 
 * size output.  As an example, for testing a generator with an output 
 * size of 20 bits, using "-n 50 -t 8000" produced a test that 
 * repeated passed an assumed good generator at "-p 100", but had 
 * trouble at "-p 500".  Alternately, raising the t-count also shows 
 * that m of 50 isn't low enough to give a good approximation.  For 
 * long tests of generators with an output size smaller than 30 bits, 
 * roducing the target by simulation instead of relying on the 
 * Poisson approximation will probably be necessary.
 *
*========================================================================
 */


#include "libdieharder.h"
#define NMS 4096 /* Maximum value */
#define DEFAULT_NMS 1700

static double lambda;
static unsigned int *intervals;
static unsigned int nms,nbits,kmax;

int dab_birthdays1(Test **test, int irun)
{

 uint i,k,t,m,mnext;
 uint *js;
 uint rand_uint[NMS];
 
 double binfreq;

 /*
  * for display only.
  */
 test[0]->ntuple = rmax_bits;

 nms = ntuple == 0 ? DEFAULT_NMS : ntuple;
 nbits = rmax_bits;

 /*
  * This is the one thing that matters.  We're going to make the
  * exact poisson distribution we expect below, and lambda has to
  * be right.  lambda = nms^3/4n where n = 2^nbits, which is:
  *   lambda = (2^9)^3/(2^2 * 2^24) = 2^27/2^26 = 2.0
  * for Marsaglia's defaults.  Small changes in nms make big changes
  * in lambda, but we can easily pick e.g. nbits = 30, nms = 2048
  *   lambda =  (2^11)^3/2^32 = 2.0
  * and get the same test, but with a lot more samples and perhaps a
  * slightly smoother result.
  */
 // lambda = .53; /* Target */
 // lambda = 4.1; /* TEMPORARY */
 // nms = (uint) pow(lambda * pow(2.0, (double) nbits + 2.0), 1.0 / 3.0);

 if (nms > NMS) nms = NMS; /* Can't be larger than allocated space. */
 lambda = (double) nms*nms*nms/pow(2.0,(double)nbits+2.0);
 /* printf("\tdab_birthdays: nms=%d, lambda = %f\n", nms, lambda); */

 /*
  * Allocate memory for intervals
  */
 intervals = (unsigned int *)malloc(nms*sizeof(unsigned int));

 /*
  * This should be more than twice as many slots as we really
  * need for the Poissonian tail.  We're going to sample tsamples
  * times, and we only want to keep the histogram out to where
  * it has a reasonable number of hits/degrees of freedom, just
  * like we do with all the chisq's built on histograms.
  */
 kmax = 1;
 while((binfreq = test[0]->tsamples*gsl_ran_poisson_pdf(kmax,lambda)) > 5) {
   /* if (irun == 0) printf("\tbin[%d] = %.1f\n", kmax, binfreq); */
   kmax++;
 }
 /* if (irun == 0) printf("\tbin[%d] = %.1f\n", kmax, binfreq); */
 /* Cruft: printf("binfreq[%u] = %f\n",kmax,binfreq); */
 kmax++;   /* and one to grow on...*/
 /* if (irun == 0) printf("\tbin[%d] = %.1f\n", kmax, test[0]->tsamples*gsl_ran_poisson_pdf(kmax, lambda)); */

 /*
  * js[kmax] is the histogram we increment using the
  * count of repeated intervals as an index.  Clear it.
  */
 js = (unsigned int *)malloc(kmax*sizeof(unsigned int));
 for(i=0;i<kmax;i++) js[i] = 0;

 /*
  * Each sample uses a unique set of tsample rand_uint[]'s, but evaluates
  * the Poissonian statistic for each cyclic rotation of the bits across
  * the 24 bit mask.
  */
 for(t=0;t<test[0]->tsamples;t++) {
	/* Fill the array with nms samples; each will be rmax_bits long */
   for(m = 0;m<nms;m++){
	 rand_uint[m] = gsl_rng_get(rng);
   }

   /*
    * The actual test logic starts right here.  We have nms random ints
    * with rmax_bits bits each.  We sort them.
    */
   MYDEBUG(D_DIEHARD_BDAY){
     for(m=0;m<nms;m++){
       printf("Before sort %u:  %u\n",m,rand_uint[m]);
     }
   }
   gsl_sort_uint(rand_uint,1,nms);
   MYDEBUG(D_DIEHARD_BDAY){
     for(m=0;m<nms;m++){
       printf("After sort %u:  %u\n",m,rand_uint[m]);
     }
   }

   /*
    * We create the intervals between entries in the sorted
    * list and sort THEM.
	* The first interval is the interval from 0 to the smallest number.
    */
   intervals[0] = rand_uint[0];
   for(m=1;m<nms;m++){
     intervals[m] = rand_uint[m] - rand_uint[m-1];
   }
   gsl_sort_uint(intervals,1,nms);
   MYDEBUG(D_DIEHARD_BDAY){
     for(m=0;m<nms;m++){
       printf("Sorted Intervals %u:  %u\n",m,intervals[m]);
     }
   }

   /*
    * We count the number of interval values that occur more than
    * once in the list.  Presumably that means that even if an interval
    * occurs 3 or 4 times, it counts only once!
    *
    * k is the interval count (Marsaglia calls it j).
    */
   k = 0;
   for(m=0;m<nms-1;m++){
     mnext = m+1;
     while(intervals[m] == intervals[mnext]){
       /* There is at least one repeat of this interval */
       if(mnext == m+1){
         /* increment the count of repeated intervals */
        k++;
       }
       MYDEBUG(D_DIEHARD_BDAY){
         printf("repeated intervals[%u] = %u == intervals[%u] = %u\n",
            m,intervals[m],mnext,intervals[mnext]);
       }
       mnext++;
     }
     /*
      * Skip all the rest that were identical.
      */
     if(mnext != m+1) m = mnext;
   }

   /*
    * k now is the total number of intervals that occur more than once in
    * this sample of nms numbers.  We increment the sample counter in
    * this slot.  If k is bigger than kmax, we simply ignore it -- it is a
    * BAD IDEA to bundle all the points from the tail into the last bin,
    * as a Poisson distribution can have a lot of points out in that tail!
    */
   if(k<kmax) {
     js[k]++;
     MYDEBUG(D_DIEHARD_BDAY){
       printf("incremented js[%u] = %u\n",k,js[k]);
     }
   } else {
     MYDEBUG(D_DIEHARD_BDAY){
       printf("%u >= %u: skipping increment of js[%u]\n",k,kmax,k);
     }
   }
       
 }


 /*
  * Let's sort the result (for fun) and print it out for each bit
  * position.
  */
 MYDEBUG(D_DIEHARD_BDAY){
   printf("#==================================================================\n");
   printf("# This is the repeated interval histogram:\n");
   for(k=0;k<kmax;k++){
     printf("js[%u] = %u\n",k,js[k]);
   }
 }


 /*
  * Fine fine fine.  We FINALLY have a distribution of the binned repeat
  * interval counts of many samples of nms numbers drawn from 2^rmax_bits.  We
  * should now be able to pass this vector of results off to a Pearson
  * chisq computation for the expected Poissonian distribution.
  */
 test[0]->pvalues[irun] = chisq_poisson(js,lambda,kmax,test[0]->tsamples);
 MYDEBUG(D_DIEHARD_BDAY){
   printf("# diehard_birthdays(): test[0]->pvalues[%u] = %10.5f\n",irun,test[0]->pvalues[irun]);
 }

 nullfree(intervals);
 nullfree(js);

 return(0);

}

