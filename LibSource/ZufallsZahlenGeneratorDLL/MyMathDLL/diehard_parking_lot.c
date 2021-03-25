/*
 * $Id: diehard_parking_lot.c 231 2006-08-22 16:18:05Z rgb $
 *
 * See copyright in copyright.h and the accompanying file COPYING
 *
 */

/*
 *========================================================================
 * This is the Diehard Parking Lot test, rewritten from the description
 * in tests.txt on  George Marsaglia's diehard site.
 *
 * ::               THIS IS A PARKING LOT TEST                      ::
 * :: In a square of side 100, randomly "park" a car---a circle of  ::
 * :: radius 1.   Then try to park a 2nd, a 3rd, and so on, each    ::
 * :: time parking "by ear".  That is, if an attempt to park a car  ::
 * :: causes a crash with one already parked, try again at a new    ::
 * :: random location. (To avoid path problems, consider parking    ::
 * :: helicopters rather than cars.)   Each attempt leads to either ::
 * :: a crash or a success, the latter followed by an increment to  ::
 * :: the list of cars already parked. If we plot n:  the number of ::
 * :: attempts, versus k::  the number successfully parked, we get a::
 * :: curve that should be similar to those provided by a perfect   ::
 * :: random number generator.  Theory for the behavior of such a   ::
 * :: random curve seems beyond reach, and as graphics displays are ::
 * :: not available for this battery of tests, a simple characteriz ::
 * :: ation of the random experiment is used: k, the number of cars ::
 * :: successfully parked after n=12,000 attempts. Simulation shows ::
 * :: that k should average 3523 with sigma 21.9 and is very close  ::
 * :: to normally distributed.  Thus (k-3523)/21.9 should be a st-  ::
 * :: andard normal variable, which, converted to a uniform varia-  ::
 * :: ble, provides input to a KSTEST based on a sample of 10.      ::
 *
 *                         Comments
 *
 *    First, the description above is incorrect in two regards.
 *    As seen in the original code, the test measures
 *    overlap of SQUARES of radius one, a thing I only observed after
 *    actually programming circles as described (which are
 *    also easy, although a bit more expensive to evaluate crashes
 *    for).  Circles produce an altogether different mean and are
 *    probably a bit more sensitive to 2d striping at arbitrary angles.
 *
 *    Note that I strongly suspect that this test is basically
 *    equivalent to Knuth's better conceived hyperplane test, which
 *    measures aggregation of N dimensional sets of "coordinates" in
 *    hyperplanes.  To put it another way, if something fails a
 *    hyperplane test in 2d, it will certainly fail this test as well.
 *    If something fails this test, I'd bet serious money that it
 *    is because of aggregation of points on hyperplanes although
 *    there MAY be other failure patterns as well.
 *
 *    Finally, note that the probability that any given k is
 *    obtained for a normal random distribution is just determined
 *    from the erf() -- this is just an Xtest().
 *
 * As always, we will increase the number of tsamples and hopefully improve
 * the resolution of the test.  However, it should be carefully noted
 * that modern random number generators can almost certainly add many
 * decimal places to the simulation value used in this test.  In other
 * words, test failure at higher resolution can be INVERTED -- it can
 * indicate the relative failure of the generators used to produce the
 * earlier result!  This is really a subject for future research...
 *========================================================================
 */


#include "libdieharder.h"

typedef struct {
  double x;
  double y;
} Cars;

int diehard_parking_lot(Test **test, int irun)
{

 /*
  * This is the most that could under any circumstances be parked.
  */
 Cars parked[12000];
 uint k,n,i,crashed;
 double xtry,ytry;
 Xtest ptest;

 /*
  * for display only.  0 means "ignored".
  */
 test[0]->ntuple = 0;
 test[0]->tsamples = 12000;

 /*
  * ptest.x = (double) k
  * ptest.y = 3523.0
  * ptest.sigma = 21.9
  * This will generate ptest->pvalue when Xtest(ptest) is called
  */
 ptest.y = 3523.0;
 ptest.sigma = 21.9;

 /*
  * Clear the parking lot the fast way.
  */
 memset(parked,0,12000*sizeof(Cars));

 /*
  * Park a single car to have something to avoid and count it.
  */
 parked[0].x = 100.0*gsl_rng_uniform(rng);
 parked[0].y = 100.0*gsl_rng_uniform(rng);
 k = 1;
 

 /*
  * This is now a really simple test.  Park them cars!  We try to park
  * 12000 times, and increment k (the number successfully parked) on
  * successes.  We brute force the crash test.
  */
 for(n=1;n<12000;n++){
   xtry = 100.0*gsl_rng_uniform(rng);
   ytry = 100.0*gsl_rng_uniform(rng);
   crashed = 0;
   for(i=0;i<k;i++){
     /*
      * We do this REASONABLY efficiently.  As soon as we know we didn't
      * crash we move on until we learn that we crashed, trying to skip
      * arithmetic.  Once we've crashed, we break out of the loop.
      * Uncrashed survivors join the parked list.
      */
     if( (fabs(parked[i].x - xtry) <= 1.0) && (fabs(parked[i].y - ytry) <= 1.0)){
       crashed = 1;  /* We crashed! */
       break;        /* So quit the loop here */
     }
   }
   /*
    * Save uncrashed helicopter coordinates.
    */
   if(crashed == 0){
     parked[k].x = xtry;
     parked[k].y = ytry;
     crashed = 0;
     k++;
   }
 }

 ptest.x = (double)k;
 Xtest_eval(&ptest);
 test[0]->pvalues[irun] = ptest.pvalue;

 MYDEBUG(D_DIEHARD_PARKING_LOT) {
   printf("# diehard_parking_lot(): test[0]->pvalues[%u] = %10.5f\n",irun,test[0]->pvalues[irun]);
 }

 return(0);

}

