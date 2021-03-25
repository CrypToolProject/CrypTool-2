/*
 *========================================================================
 *  rng_XOR.c
 *
 * This generator takes a list of generators on the dieharder
 * command line and XOR's their output together.
 *========================================================================
 */

#include "libdieharder.h"

/*
 * This is a special XOR generator that takes a list of GSL
 * wrapped rngs and XOR's their uint output together to produce
 * each new random number.  Note that it SKIPS THE FIRST ONE which
 * MUST be the XOR rng itself.  So there have to be at least two -g X
 * stanzas on the command line to use XOR, and if there aren't three
 * or more it doesn't "do" anything but use the second one.
 */
static unsigned long int XOR_get (void *vstate);
static double XOR_get_double (void *vstate);
static void XOR_set (void *vstate, unsigned long int s);

typedef struct {
  /*
   * internal gsl random number generator vector
   */
  gsl_rng *grngs[GVECMAX];
  unsigned int XOR_rnd;
} XOR_state_t;

static _inline unsigned long int
XOR_get (void *vstate)
{
 XOR_state_t *state = (XOR_state_t *) vstate;
 int i;

 /*
  * There is always this one, or we are in deep trouble.  I am going
  * to have to decorate this code with error checks...
  */
 state->XOR_rnd = gsl_rng_get(state->grngs[1]);
 for(i=1;i<gvcount;i++){
   state->XOR_rnd ^= gsl_rng_get(state->grngs[i]);
 }
 return state->XOR_rnd;
 
}

static double
XOR_get_double (void *vstate)
{
  return XOR_get (vstate) / (double) UINT_MAX;
}

static void XOR_set (void *vstate, unsigned long int s) {

 XOR_state_t *state = (XOR_state_t *) vstate;
 int i;
 uint seed_seed;

 /*
  * OK, here's how it works.  grngs[0] is set to mt19937_1999, seeded
  * as per usual, and used (ONLY) to see the remaining generators.
  * The remaining generators.
  */
 state->grngs[0] = gsl_rng_alloc(dh_rng_types[14]);
 seed_seed = s;
 gsl_rng_set(state->grngs[0],seed_seed);
 for(i=1;i<gvcount;i++){

   /*
    * I may need to (and probably should) add a sanity check
    * here or in choose_rng() to be sure that all of the rngs
    * exist.
    */
   state->grngs[i] = gsl_rng_alloc(dh_rng_types[gnumbs[i]]);
   gsl_rng_set(state->grngs[i],gsl_rng_get(state->grngs[0]));

 }

}

static const gsl_rng_type XOR_type =
{"XOR (supergenerator)",        /* name */
 UINT_MAX,			/* RAND_MAX */
 0,				/* RAND_MIN */
 sizeof (XOR_state_t),
 &XOR_set,
 &XOR_get,
 &XOR_get_double};

const gsl_rng_type *gsl_rng_XOR = &XOR_type;
