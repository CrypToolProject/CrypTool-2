/*
 * $Id: rgb_persist.c 250 2006-10-10 05:02:26Z rgb $
 *
 * See copyright in copyright.h and the accompanying file COPYING
 */

/*
 *========================================================================
 * This is a test that checks to see if all the bits returned by the
 * rng change.  Surprisingly, several generators have bits that do NOT
 * change, hence the test.  It also reveals tests that for whatever reason
 * return less than the expected uint number of bits (32) as unchanging
 * high bits
 *
 * This (and all further rgb_ tests) are "my own".  Some of them may turn
 * out to be formally equivalent to diehard or sts or knuth tests in the
 * specific sense that failure in one always matches or precedes failure
 * in the other.
 *========================================================================
 */

#include "libdieharder.h"

int rgb_persist(Test **test, Rgb_Persist *persist)
{

 uint last_rand;
 int i,j;

 /*
  * Now go through the list and dump the numbers several ways.
  */
 if(bits > 32) {
   persist->nbits = 32;
 } else {
   persist->nbits = bits;
 }

 persist->cumulative_mask = 0;
 for(j=0;j<psamples;j++){
   /*
    * Do not reset the total count of rands or rewind file input
    * files -- let them auto-rewind as needed.  Otherwise try
    * different seeds for different samples.
    */
   if(strncmp("file_input",gsl_rng_name(rng),10)){
     seed = random_seed();
     gsl_rng_set(rng,seed);
   }
   /*
    * Fill rgb_persist_rand_uint with a string of random numbers
    */
   for(i=0;i<256;i++) rgb_persist_rand_uint[i] = gsl_rng_get(rng);
   last_rand = rgb_persist_rand_uint[0];  /* to start it */
   persist->and_mask = ~(last_rand ^ rgb_persist_rand_uint[0]);
   for(i=0;i<256;i++){
     if(verbose){
       printf("rgb_persist_rand_uint[%d] = %u = ",i,rgb_persist_rand_uint[i]);
       dumpbits(&rgb_persist_rand_uint[i],persist->nbits);
       printf("\n");
     }

     /*
      * Now we make a mask of bits that coincide. Logic 41, where are you?
      */
     persist->and_mask = persist->and_mask & (~(last_rand ^ rgb_persist_rand_uint[i]));
     if(verbose){
       printf("and_mask = %u = ",persist->and_mask);
       dumpbits(&persist->and_mask,persist->nbits);
       printf("\n");
     }

   }
   persist->and_mask = persist->and_mask & rmax_mask;
   persist->cumulative_mask = persist->cumulative_mask | persist->and_mask;
 }

 return(0);

}

