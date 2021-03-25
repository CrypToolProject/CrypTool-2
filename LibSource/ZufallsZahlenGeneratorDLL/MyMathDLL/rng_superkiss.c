/*
 * rng_superkiss.c
 *
 * Super KISS based on Usenet posting by G. Marsaglia:
 *       Period 54767 * 2^1337279
 * Adopted in C from David Jones'
 *
 *   Good Practice in (Pseudo) Random Number Generation for
 *   Bioinformatics Applications
 *
 * http://www.cs.ucl.ac.uk/staff/d.jones/GoodPracticeRNG.pdf
 *
 * David Jones, UCL Bioinformatics Group
 * E-mail: d.jones@cs.ucl.ac.uk
 * Last revised May 7th 2010
 *
 *
 *  GSL/GPL packaged for dieharder by Robert G. Brown 01/07/11
 *
 * David has kindly agreed to make the GPRNG code vailable under the
 * GPL so it can be integrated with the dieharder package and/or the
 * Gnu Scentific Libraary (GSL).
 *
 */

#include "libdieharder.h"
#include "DieHarderDLL.h"
#define SUPERKISS_QMAX 41790
#define GSL_MT19937_1999 14

static unsigned long int superkiss_get(void *vstate);
static double superkiss_get_double(void *vstate);
static void superkiss_set(void *vstate, unsigned long int s);

typedef struct {
	/*
	 * Seed/state variables.  Note that superkiss has to be carefully
	 * seeded, that its first and subsequent refill() calls are expensive,
	 * so not a good choice if all you're doing is generating five or ten
	 * random numbers.  Should be GREAT for simulations, with effectively
	 * infinite period and HOPEFULLY good randomness.
	 */

	unsigned int Q[SUPERKISS_QMAX];
	unsigned int indx;
	unsigned int carry;
	unsigned int xcng;
	unsigned int xs;

} superkiss_state_t;

static unsigned long int superkiss_refill(void *vstate){

	superkiss_state_t *state = vstate;
	int i;
	unsigned long long t;
	for (i = 0; i < SUPERKISS_QMAX; i++) {
		t = 7010176ULL * state->Q[i] + state->carry;
		state->carry = (t >> 32);
		state->Q[i] = ~t;
	}
	state->indx = 1;
	return (state->Q[0]);


}

static unsigned long int
superkiss_get(void *vstate)
{

	superkiss_state_t *state = vstate;

	state->xcng = 69069 * state->xcng + 123;
	state->xs ^= state->xs << 13;
	state->xs ^= state->xs >> 17;
	state->xs ^= state->xs >> 5;
	byteCount++;
	return (state->indx < SUPERKISS_QMAX ? state->Q[state->indx++] : superkiss_refill(vstate)) + state->xcng + state->xs;

}

static double superkiss_get_double(void *vstate)
{

	return (unsigned int)superkiss_get(vstate) / (double)UINT_MAX;

}

static void superkiss_set(void *vstate, unsigned long int s)
{

	/* Initialize automaton using specified seed. */
	superkiss_state_t *state = (superkiss_state_t *)vstate;

	uint seed_seed;
	gsl_rng *seed_rng;    /* random number generator used to seed uvag */
	int i;

	/*
	 * superkiss needs MANY initial seeds.  They have to be reproducible
	 * from a single seed in order to be consistent with the GSL.  We
	 * therefore have to do a two step process where we use seed to
	 * seed an existing GSL generator (say mt19937_1999) and take the
	 * first 41790 returns as the rest of our seed for this generator.
	 */
	seed_rng = gsl_rng_alloc(dh_rng_types[GSL_MT19937_1999]);
	seed_seed = s;
	gsl_rng_set(seed_rng, seed_seed);
	for (i = 0; i < SUPERKISS_QMAX; i++){
		state->Q[i] = gsl_rng_get(seed_rng);
	}
	state->indx = SUPERKISS_QMAX;
	state->carry = 362436;
	state->xcng = 1236789;
	state->xs = 521288629;

}

static const gsl_rng_type superkiss_type =
{ "superkiss",			/* name */
UINT_MAX,			/* RAND_MAX */
0,				/* RAND_MIN */
sizeof (superkiss_state_t),
&superkiss_set,
&superkiss_get,
&superkiss_get_double };

const gsl_rng_type *gsl_rng_superkiss = &superkiss_type;
