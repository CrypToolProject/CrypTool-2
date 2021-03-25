/*
 * $Id: diehard_craps.c 231 2006-08-22 16:18:05Z rgb $
 *
 * See copyright in copyright.h and the accompanying file COPYING
 *
 */

/*
 *========================================================================
 * This is the Diehard Craps test, rewritten from the description
 * in tests.txt on George Marsaglia's diehard site.
 *
 *: This is the CRAPS TEST. It plays 200,000 games of craps, finds::
 *: the number of wins and the number of throws necessary to end  ::
 *: each game.  The number of wins should be (very close to) a    ::
 *: normal with mean 200000p and variance 200000p(1-p), with      ::
 *: p=244/495.  Throws necessary to complete the game can vary    ::
 *: from 1 to infinity, but counts for all>21 are lumped with 21. ::
 *: A chi-square test is made on the no.-of-throws cell counts.   ::
 *: Each 32-bit integer from the test file provides the value for ::
 *: the throw of a die, by floating to [0,1), multiplying by 6    ::
 *: and taking 1 plus the integer part of the result.             ::
 *
 *========================================================================
 */


#include "libdieharder.h"

uint roll(){
  uint d = 1 + gsl_rng_uniform_int(rng,6);
  return d;
}

int diehard_craps(Test **test, int irun)
{

 uint i;
 uint point,throw,tries,wins;
 double sum,p;
 Xtest ptest;
 Vtest vtest;

 /*
  * This is just for output display.
  */
 test[0]->ntuple = 0;
 test[1]->ntuple = 0;

 /*
  * ptest.x = number of wins
  *   p = 244.0/495.0 is the probability of winning, so the mean
  * should be normally distributed with a binary distribution
  * sigma (standard stuff).
  * ptest.y = test[0]->tsamples*p
  * ptest.sigma = sqrt(test[0]->tsamples*p*(1 - p))
  *
  * HOWEVER, it also counts the number of throws required to win
  * each game, binned according to 1,2,3... 21+ (the last bin is
  * holds all games that require more than 20 throws).  The
  * vector of bin values is subjected to a chi-sq test.  BOTH
  * tests must be passed, making this one a bit more complex to
  * report on, as it is really two tests in one.
  */
 p = 244.0/495.0;
 ptest.y = (double) test[0]->tsamples*p;
 ptest.sigma = sqrt(ptest.y*(1.0 - p));

 /*
  * Allocate memory for Vtest struct vector (length 21) and initialize
  * it with the expected values.
  */
 Vtest_create(&vtest,21);
 vtest.cutoff = 5.0;
 sum = 1.0/3.0;
 vtest.y[0] = sum;
 for(i=1;i<20;i++){
   vtest.y[i] = (27.0*pow(27.0/36.0,i-1) + 40*pow(13.0/18.0,i-1) +
                55.0*pow(25.0/36.0,i-1))/648.0;
   sum += vtest.y[i];
 }
 vtest.y[20] = 1.0 - sum;
 /*
  * Normalize the probabilities by the expected number of trials
  */
 for(i=0;i<21;i++){
   vtest.y[i] *= test[0]->tsamples;
 }



 /*
  * Initialize sundry things.  This is short enough I'll use
  * a loop instead of memset.
  */
 for(i=0;i<21;i++) vtest.x[i] = 0;
 wins = 0;

 /*
  * We now play test[0]->tsamples games of craps!
  */
 for(i=0;i<test[0]->tsamples;i++){

   /*
    * This is the point count we have to make, the sum of two rolled
    * dice.
    */
   point = roll() + roll();
   tries = 0;

   if(point == 7 || point == 11) {
     /*
      * If we rolled 7 or 11, we just win.
      */
     wins++;
     vtest.x[tries]++;
   } else if(point == 2 || point == 3 || point == 12){
     /*
      * If we rolled 2, 3, or 12, we just lose.
      */
     vtest.x[tries]++;
   } else {
     /*
      * We have to roll until we make the point (win) or roll
      * a seven (lose).  We have to keep going until we win
      * or lose, but have to compress the number of throws
      * to bin 21 for all throw>21.
      */
     while(1){
       /*
        * This little ditty increments tries if it is less than 20
	* then freezes it.
        */
       (tries<20)?tries++:tries;
       throw = roll() + roll();
       if(throw == 7){
         vtest.x[tries]++;
	 break;
       } else if(throw == point){
         vtest.x[tries]++;
	 wins++;
	 break;
       }
     }
   }
 }

 ptest.x = wins++;
 Xtest_eval(&ptest);
 Vtest_eval(&vtest);
 test[0]->pvalues[irun] = ptest.pvalue;
 test[1]->pvalues[irun] = vtest.pvalue;

 MYDEBUG(D_DIEHARD_CRAPS) {
   printf("# diehard_runs(): test[0]->pvalues[%u] = %10.5f\n",irun,test[0]->pvalues[irun]);
   printf("# diehard_runs(): test[1]->pvalues[%u] = %10.5f\n",irun,test[1]->pvalues[irun]);
 }

 Vtest_destroy(&vtest);

 return(0);

}

