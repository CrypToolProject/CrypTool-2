/*
 *  rng_kiss.c
 * 
 *  This is a version of (j)kiss (in C) from:
 *
 * Good Practice in (Pseudo) Random Number Generation for
 * Bioinformatics Applications
 *
 * http://www.cs.ucl.ac.uk/staff/d.jones/GoodPracticeRNG.pdf
 *
 * David Jones, UCL Bioinformatics Group
 * (E-mail: d.jones@cs.ucl.ac.uk)
 * (Last revised May 7th 2010)
 *
 *  GSL packaged for dieharder by Robert G. Brown 01/06/11
 *
 * David has kindly agreed to make this code vailable under the GPL so 
 * it can be integrated with the dieharder package and/or the Gnu
 * Scentific Libraary (GSL).
 *
 */

#include "libdieharder.h"

static unsigned long int kiss_get (void *vstate);
static double kiss_get_double (void *vstate);
static void kiss_set (void *vstate, unsigned long int s);

typedef struct {
 /*
  * Seed variables.  Note that kiss requires a moderately complex
  * seeding using a "seed rng" that we will arbitrarily set to be
  * a MT from the GSL.  This makes this routine NOT PORTABLE, but
  * D. Jones' article (URL above) contains a portable, public doman
  * version of this code that includes a seeding routine (and commentary!).
  */
 unsigned int x;
 unsigned int y;
 unsigned int z;
 unsigned int c;
} kiss_state_t;


static unsigned long int kiss_get (void *vstate)
{

 kiss_state_t *state = vstate;
 unsigned long long t;
 state->x = 314527869 * state->x + 1234567;
 state->y ^= state->y << 5;
 state->y ^= state->y >> 7;
 state->y ^= state->y << 22;
 t = 4294584393ULL * state->z + state->c;
 state->c = t >> 32;
 state->z = t;
 return (unsigned int)(state->x + state->y + state->z);

}

static double kiss_get_double (void *vstate)
{
  return (double) kiss_get (vstate) / (double) UINT_MAX;
}

static void
kiss_set (void *vstate, unsigned long int s)
{

 /* Initialize automaton using specified seed. */
 kiss_state_t *state = (kiss_state_t *) vstate;
 
 uint seed_seed;
 gsl_rng *seed_rng;    /* random number generator used to seed uvag */

 /*
  * kiss needs four random number seeds.  They have to be reproducible
  * from a single seed in order to be consistent with the GSL.  We
  * therefore have to do a two step process where we use seed to
  * seed an existing GSL generator (say mt19937_1999) and take the
  * first three returns as the rest of our seed for this generator.
  */
 seed_rng = gsl_rng_alloc(dh_rng_types[14]);
 seed_seed = s;
 gsl_rng_set(seed_rng,seed_seed);
 /* printf("Seeding kiss\n"); */
 state->x = gsl_rng_get(seed_rng);
 while (!(state->y = gsl_rng_get(seed_rng))); /* y must not be zero! */
 state->z = gsl_rng_get(seed_rng);
 /* printf("Done!\n"); */

 /*
  * We don't really need to set c as well but let's anyway.
  * Notes: offset c by 1 to avoid z=c=0; should be less than 698769069.
  */
 state->c = gsl_rng_get(seed_rng) % 698769068 + 1;
 /* printf("x = %10u y = %10u z = %10u c = %10u\n",
         state->x,state->y,state->z,state->c); */
 return;

}

static const gsl_rng_type kiss_type =
{"kiss",			/* name */
 UINT_MAX,			/* RAND_MAX */
 0,				/* RAND_MIN */
 sizeof (kiss_state_t),
 &kiss_set,
 &kiss_get,
 &kiss_get_double};

const gsl_rng_type *gsl_rng_kiss = &kiss_type;
