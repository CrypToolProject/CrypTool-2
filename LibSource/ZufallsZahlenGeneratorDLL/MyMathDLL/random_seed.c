/*
 *========================================================================
 * $Id: random_seed.c 223 2006-08-17 06:19:38Z rgb $
 *
 * See copyright in copyright.h and the accompanying file COPYING
 *========================================================================
 */

/*
 *========================================================================
 * This routine does all the required initialization and startup,
 * including memory allocation and prefilling of vectors.  It is
 * COMPLETELY outside the timing loops.
 *========================================================================
 */

#include "libdieharder.h"

unsigned long int random_seed()
{

 unsigned int seed;
 struct timeval tv;
 FILE *devurandom;

 /*
  * We will routinely use /dev/urandom here, which is entropic noise
  * supplemented by an RNG of high quality.  This is much faster than
  * /dev/random (doesn't block when the entropy pool is exhausted) and
  * should serve our purpose of providing a sufficiently random and
  * uniformly distributed seed.
  *
  * It falls back on the clock, which is a POOR choice.  I really should
  * use the clock to seed a good rng (e.g. mt19937), generate a
  * random integer in the range 1-100, discard this number of rand and use
  * the next one as the seed (for example).  The point isn't to get a
  * perfectly random seed -- it is to get a uniform distribution of possible
  * seeds.
  */
 if ((devurandom = fopen("/dev/urandom","r")) == NULL) {
   gettimeofday(&tv,0);
   seed = tv.tv_sec + tv.tv_usec;
   if(verbose == D_SEED) printf("Got seed %u from gettimeofday()\n",seed);
 } else {
   fread(&seed,sizeof(seed),1,devurandom);
   if(verbose == D_SEED) printf("Got seed %u from /dev/urandom\n",seed);
   fclose(devurandom);
 }

 return(seed);

}
