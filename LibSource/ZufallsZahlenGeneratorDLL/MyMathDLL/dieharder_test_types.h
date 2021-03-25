/*
 * ========================================================================
 * $Id: sts_runs.c 237 2006-08-23 01:33:46Z rgb $
 *
 * See copyright in copyright.h and the accompanying file COPYING
 * ========================================================================
 */

/* 
 * ========================================================================
 * This program initializes a permanent internal vector of pointers to all
 * the tests known to dieharder that generate a pvalue or vector of
 * pvalues.  With it we abandon our former addressing of tests by source
 * (the -d, -r, -s testnumber invocation) in favor of a segmented single
 * number.  There is initial room for up to 1000 tests, but this can easily
 * be increased.
 *
 * We define the ranges:
 *
 *   0-99    diehard (or Marsaglia & Tsang) based tests.
 *   100-199 the NIST STS
 *   200-499 everything else.
 *   500-999 reserved for future sets of "named" tests if it seems
 *           reasonable to use it that way, or straight expansion
 *           space otherwise.  500 tests will hold us for a while...
 *
 * ========================================================================
 */

 /*
  * test global vectors and variables for tests.
  */
#define MAXTESTS 1000



 void dieharder_test_types();

 Dtest *dh_test_types[MAXTESTS];

#define ADD_TEST(t) {if (i==MAXTESTS) abort(); dh_test_types[i] = (t); i++; };

 /*
  * Global shared counters for the new types of rngs in the organization
  * defined above.
  */
 unsigned int dh_num_diehard_tests;  /* diehard tests available in dieharder */
 unsigned int dh_num_sts_tests;      /* STS tests available in dieharder */
 unsigned int dh_num_other_tests;    /* other tests available in dieharder */
 unsigned int dh_num_user_tests;     /* user tests added in ui segment */
 unsigned int dh_num_tests;          /* total tests available in dieharder */


 Dtest *dh_test;             /* global pointer to the current test */

