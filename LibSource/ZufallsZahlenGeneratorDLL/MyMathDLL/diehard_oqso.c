/*
 * ========================================================================
 * See copyright in copyright.h and the accompanying file COPYING
 * ========================================================================
 */

/*
 * ========================================================================
 * This is the Diehard QPSO test, rewritten from the description
 * in tests.txt on George Marsaglia's diehard site.
 *
 *     OQSO means Overlapping-Quadruples-Sparse-Occupancy        ::
 * The test OQSO is similar, except that it considers 4-letter ::
 * words from an alphabet of 32 letters, each letter determined  ::
 * by a designated string of 5 consecutive bits from the test    ::
 * file, elements of which are assumed 32-bit random integers.   ::
 * The mean number of missing words in a sequence of 2^21 four-  ::
 * letter words,  (2^21+3 "keystrokes"), is again 141909, with   ::
 * sigma = 295.  The mean is based on theory; sigma comes from   ::
 * extensive simulation.                                         ::
 *
 * Note: 2^21 = 2097152
 * Note: One must use overlapping samples to get the right sigma.
 * The tests BITSTREAM, OPSO, OQSO and DNA are all closely related.
 *
 * This test is now CORRECTED on the basis of a private communication
 * from Paul Leopardi (MCQMC-2008 presentation) and Noonan and Zeilberger
 * (1999), Rivals and Rahmann (2003), and Rahmann and Rivals (2003).
 * The "exact" target statistic (to many places) is:
 * \mu = 141909.6005321316,  \sigma = 294.6558723658
 * ========================================================================
 */


#include "libdieharder.h"

int diehard_oqso(Test **test, int irun)
{

 uint i,j,k,l,i0=0,j0=0,k0=0,l0=0,t,boffset=0;
 Xtest ptest;
 char w[32][32][32][32];


 /*
  * for display only.  0 means "ignored".
  */
 test[0]->ntuple = 0;

 /*
  * p = 141909, with sigma 295, FOR tsamples 2^21 2 letter words.
  * These cannot be varied unless one figures out the actual
  * expected "missing works" count as a function of sample size.  SO:
  *
  * ptest.x = number of "missing words" given 2^21 trials
  * ptest.y = 141909.6005321316
  * ptest.sigma = 294.6558723658
  */
 ptest.x = 0.0;  /* Initial value */
 ptest.y = 141909.6005321316;
 ptest.sigma = 294.6558723658;

 /*
  * We now make tsamples measurements, as usual, to generate the
  * missing statistic.  We proceed exactly as we did in opso, but
  * with a 4d 32x32x32x32 matrix and 5 bit indices.  This should
  * basically be strongly related to a Knuth hyperplane test in
  * four dimensions.  Equally obviously there is a sequence of
  * tests, all basically identical, that can be done here much
  * as rgb_bitdist tries to do them.  I'll postpone thinking about
  * this in detail until I'm done with diehard and some more of STS
  * and maybe have implemented the REAL Knuth tests from the Art of
  * Programming.
  */

 memset(w,0,sizeof(char)*32*32*32*32);

 /*
  * To minimize the number of rng calls, we use each j and k mod 32
  * to determine the offset of the 10-bit long string (with
  * periodic wraparound) to be used for the next iteration.  We
  * therefore have to "seed" the process with a random l
  */
 for(t=0;t<test[0]->tsamples;t++){
   if(t%6 == 0) {
     i0 = 
		 (rng);
     j0 = gsl_rng_get(rng);
     k0 = gsl_rng_get(rng);
     l0 = gsl_rng_get(rng);
     boffset = 0;
   }
   /*
    * Get four "letters" (indices into w)
    */
   i = (i0 >> boffset) & 0x01f;
   j = (j0 >> boffset) & 0x01f;
   k = (k0 >> boffset) & 0x01f;
   l = (l0 >> boffset) & 0x01f;

   w[i][j][k][l]=1;
   boffset+=5;

 }
 
 /*
  * Now we count the holes, so to speak
  */
 t = 0;
 for(i=0;i<32;i++){
   for(j=0;j<32;j++){
     for(k=0;k<32;k++){
       for(l=0;l<32;l++){
         if(w[i][j][k][l] == 0){
           t++;
           /* printf("ptest.x = %f  Hole: w[%u][%u][%u][%u] = %u\n",t,i,j,k,l,w[i][j][k][l]); */
	 }
       }
     }
   }
 }
 ptest.x = t;

 MYDEBUG(D_DIEHARD_OQSO){
   printf("%f %f %f\n",ptest.y,ptest.x,ptest.x-ptest.y);
 }

 Xtest_eval(&ptest);
 test[0]->pvalues[irun] = ptest.pvalue;

 MYDEBUG(D_DIEHARD_OQSO) {
   printf("# diehard_oqso(): ks_pvalue[%u] = %10.5f\n",irun,test[0]->pvalues[irun]);
 }

 return(0);

}

