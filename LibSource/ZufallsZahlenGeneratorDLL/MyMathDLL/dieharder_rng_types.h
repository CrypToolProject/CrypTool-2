/*
 *========================================================================
 * See copyright in copyright.h and the accompanying file COPYING
 *========================================================================
 */

/*
 *========================================================================
 * This is being directly adapted from and modified for use in dieharder
 * so it can maintain its own independent RNG space of types not limited
 * by the GSL's current internal limit of 100.  To avoid collisions,
 * we'll start our number space above 100, and extend it to 1000.
 *
 * To be concrete, I'm going to define the following ranges:
 *
 *   0-99    GSL generators, in GSL order.  If it changes, tough.
 *   100-199 reserved for spillover if the GSL ends up with >100 generators
 *   200-399 libdieharder generators (fixed order from now on)
 *   400-499 R-based generators (fixed order from now on)
 *   500-599 hardware generators (starting with /dev/random and friends)
 *   600-699 user-defined generators (starting with dieharder example)
 *   700-999 reserved, e.g. for future integration with R-like environments
 *
 * I may have to experiment some to determine if there is any problem
 * with defining my own gsl_rng_type table of types and then filling it
 * first with the GSL routines, then with dieharder's.
 *========================================================================
 */

#include <gsl/gsl_rng.h>

 /* #define GSL_VAR */
 /* List new rng types to be added. */
 GSL_VAR const gsl_rng_type *gsl_rng_stdin_input_raw;   /* rgb Aug 2008 */
 GSL_VAR const gsl_rng_type *gsl_rng_file_input_raw;
 GSL_VAR const gsl_rng_type *gsl_rng_file_input;

 GSL_VAR const gsl_rng_type *gsl_rng_dev_random;
 GSL_VAR const gsl_rng_type *gsl_rng_dev_arandom;
 GSL_VAR const gsl_rng_type *gsl_rng_dev_urandom;

 GSL_VAR const gsl_rng_type *gsl_rng_r_wichmann_hill;	/* edd May 2007 */
 GSL_VAR const gsl_rng_type *gsl_rng_r_marsaglia_mc;	/* edd May 2007 */
 GSL_VAR const gsl_rng_type *gsl_rng_r_super_duper;	/* edd May 2007 */
 GSL_VAR const gsl_rng_type *gsl_rng_r_mersenne_twister;/* edd May 2007 */
 GSL_VAR const gsl_rng_type *gsl_rng_r_knuth_taocp;	/* edd May 2007 */
 GSL_VAR const gsl_rng_type *gsl_rng_r_knuth_taocp2;	/* edd May 2007 */

 GSL_VAR const gsl_rng_type *gsl_rng_ca;
 GSL_VAR const gsl_rng_type *gsl_rng_uvag;	        /* rgb Jun 2007 */
 GSL_VAR const gsl_rng_type *gsl_rng_aes;	        /* bauer Oct 2009 */
 GSL_VAR const gsl_rng_type *gsl_rng_threefish;	        /* bauer Oct 2009 */
 GSL_VAR const gsl_rng_type *gsl_rng_kiss;	        /* rgb Jan 2011 */
 GSL_VAR const gsl_rng_type *gsl_rng_superkiss;	        /* rgb Jan 2011 */
 GSL_VAR const gsl_rng_type *gsl_rng_XOR;	        /* rgb Jan 2011 */

 /*
  * rng global vectors and variables for setup and tests.
  */
#define MAXRNGS 1000

 void dieharder_rng_types();

 const gsl_rng_type *dh_rng_types[MAXRNGS];
 const gsl_rng_type **gsl_types;    /* where all the rng types go */

#define ADD(t) {if (i==MAXRNGS) abort(); dh_rng_types[i] = (t); i++; };

 /*
  * Global shared counters for the new types of rngs in the organization
  * defined above.
  */
 unsigned int dh_num_rngs;           /* dh rngs available in dieharder */
 unsigned int dh_num_gsl_rngs;       /* GSL rngs available in dieharder */
 unsigned int dh_num_dieharder_rngs; /* dh rngs available in libdieharder */
 unsigned int dh_num_R_rngs;         /* R-derived rngs available in libdieharder */
 unsigned int dh_num_hardware_rngs;  /* hardware rngs supported in libdieharder */
 unsigned int dh_num_user_rngs;      /* user-added rngs */
 unsigned int dh_num_reserved_rngs;  /* ngs added in reserved space by new UI */

 gsl_rng *rng;                  /* global gsl random number generator */

