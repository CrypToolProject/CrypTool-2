/*
 * $id: rgb_timing.c 142 2005-03-11 02:56:31Z rgb $
 *
 * See copyright in copyright.h and the accompanying file COPYING
 */

/*
 *========================================================================
 * This is not a standard test -- this just times the rng.  It therefore
 * has a very nonstandard initialization and return.  One can still create
 * the test, but the **test struct is used only to determine the number
 * of samples used in the timing test.
 *========================================================================
 */

#include "libdieharder.h"

int rgb_timing(Test **test, Rgb_Timing *timing)
{

 double total_time,avg_time;
 int i,j;
 unsigned int *rand_uint;

 MYDEBUG(D_RGB_TIMING){
   printf("# Entering rgb_timing(): ps = %u  ts = %u\n",test[0]->psamples,test[0]->tsamples);
 }

 seed = random_seed();
 gsl_rng_set(rng,seed);

 rand_uint = (uint *)malloc((size_t)test[0]->tsamples*sizeof(uint));

 total_time = 0.0;
 for(i=0;i<test[0]->psamples;i++){
   start_timing();
   for(j=0;j<test[0]->tsamples;j++){
     rand_uint[j] = gsl_rng_get(rng);
   }
   stop_timing();
   total_time += delta_timing();
 }
 avg_time = total_time/(test[0]->psamples*test[0]->tsamples);

 timing->avg_time_nsec = avg_time*1.0e+9;
 timing->rands_per_sec = 1.0/avg_time;

 free(rand_uint);

 return(0);
 
}

