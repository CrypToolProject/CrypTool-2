/*
 * ========================================================================
 * $Id: diehard_opso.c 231 2006-08-22 16:18:05Z rgb $
 *
 * See copyright in copyright.h and the accompanying file COPYING
 * ========================================================================
 */

/*
 * ========================================================================
 * This is the Diehard OPSO test, rewritten from the description
 * in tests.txt on George Marsaglia's diehard site.
 *
 *         OPSO means Overlapping-Pairs-Sparse-Occupancy         ::
 * The OPSO test considers 2-letter words from an alphabet of    ::
 * 1024 letters.  Each letter is determined by a specified ten   ::
 * bits from a 32-bit integer in the sequence to be tested. OPSO ::
 * generates  2^21 (overlapping) 2-letter words  (from 2^21+1    ::
 * "keystrokes")  and counts the number of missing words---that  ::
 * is 2-letter words which do not appear in the entire sequence. ::
 * That count should be very close to normally distributed with  ::
 * mean 141,909, sigma 290. Thus (missingwrds-141909)/290 should ::
 * be a standard normal variable. The OPSO test takes 32 bits at ::
 * a time from the test file and uses a designated set of ten    ::
 * consecutive bits. It then restarts the file for the next de-  ::
 * signated 10 bits, and so on.                                  ::
 *
 * Note: Overlapping samples must be used to get the right sigma.
 * The tests BITSTREAM, OPSO, OQSO and DNA are all closely related.
 *
 * This test is now CORRECTED on the basis of a private communication
 * from Paul Leopardi (MCQMC-2008 presentation) and Noonan and Zeilberger
 * (1999), Rivals and Rahmann (2003), and Rahmann and Rivals (2003).
 * The "exact" target statistic (to many places) is:
 * \mu  = 141909.3299550069,  \sigma = 290.4622634038
 *========================================================================
 */


#include "libdieharder.h"

int diehard_opso(Test **test, int irun)
{

 uint j0=0,k0=0,j,k,t;
 Xtest ptest;
 /*
  * Fixed test size for speed and as per diehard.
  */
 char w[1024][1024];

 /*
  * for display only.  0 means "ignored".
  */
 test[0]->ntuple = 0;

 /*
  * p = 141909, with sigma 290, FOR test[0]->tsamples 2^21+1 2 letter words.
  * These cannot be varied unless one figures out the actual
  * expected "missing works" count as a function of sample size.  SO:
  *
  * ptest.x = number of "missing words" given 2^21+1 trials
  * Recalculation by David Bauer, from the original Monkey Tests paper:
  *    ptest.y = 141909.600361375512162724864.
  * This shouldn't matter, I don't think, at least at any reasonable scale
  * dieharder can yet reach (but we'll see!).  If we start getting
  * unreasonable failures we may have to try switching this number around,
  * but given sigma and y, we'd need a LOT of rands to result the 0.3 diff.
  * 
  * ptest.y = 141909.3299550069;
  * ptest.sigma = 290.4622634038;
  * 
  */
 ptest.y = 141909.3299550069;
 ptest.sigma = 290.4622634038;
 
 /*
  * We now make test[0]->tsamples measurements, as usual, to generate the
  * missing statistic.  The easiest way to proceed, I think, will
  * be to generate a simple char matrix 1024x1024 in size and empty.
  * Each pair of "letters" generated become indices, and a (char) 1
  * is inserted there.  At the end we just loop the matrix and count
  * the zeros.
  *
  * Of course doing it THIS way it is pretty obvious that we could,
  * say, display the 2-color 1024x1024 bitmap this represented graphically.
  * Missing words are just pixels that are still in the background color.
  * Hmmm, sounds a whole lot like Knuth's test looking for hyperplanes
  * in 2 dimensions, hmmm.  At the very least, any generator that produces
  * hyperplanar banding at 2 dimensions should fail this test, but it is
  * possible for it to find distributions that do NOT have banding but
  * STILL fail the test, I suppose.  Projectively speaking, though,
  * I have some fairly serious doubts about this, though.
  */

 memset(w,0,sizeof(char)*1024*1024);

 k = 0;
 for(t=0;t<test[0]->tsamples;t++){
   /*
    * Let's do this the cheap/easy way first, sliding a 20 bit
    * window along each int for the 32 possible starting
    * positions a la birthdays, before trying to slide it all
    * the way down the whole random bitstring implicit in a
    * long sequence of random ints.  That way we can exit
    * the test[0]->tsamples loop at test[0]->tsamples = 2^15...
    */
   if(t%2 == 0) {
     j0 = gsl_rng_get(rng);
     k0 = gsl_rng_get(rng);
     j = j0 & 0x03ff;
     k = k0 & 0x03ff;
   } else {
      j = (j0 >> 10) & 0x03ff;
      k = (k0 >> 10) & 0x03ff;
   }
   /*
    * Get two "letters" (indices into w)
    */
   /* printf("%u:   %u  %u  %u\n",t,j,k,boffset); */
   w[j][k] = 1;
 }
 
 /*
  * Now we count the holes, so to speak
  */
 ptest.x = 0;
 for(j=0;j<1024;j++){
   for(k=0;k<1024;k++){
     if(w[j][k] == 0){
       ptest.x += 1.0;
       /* printf("ptest.x = %f  Hole: w[%u][%u] = %u\n",ptest.x,j,k,w[j][k]); */
     }
   }
 }
 MYDEBUG(D_DIEHARD_OPSO) {
   printf("%f %f %f\n",ptest.y,ptest.x,ptest.x-ptest.y);
 }

 Xtest_eval(&ptest);
 test[0]->pvalues[irun] = ptest.pvalue;

 MYDEBUG(D_DIEHARD_OPSO) {
   printf("# diehard_opso(): ks_pvalue[%u] = %10.5f\n",irun,test[0]->pvalues[irun]);
 }

 return(0);

}

