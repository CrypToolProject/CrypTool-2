/*
 * ========================================================================
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

#include "libdieharder.h"

void dieharder_test_types()
{

 int i;

 /*
  * Null the whole thing for starters
  */
 for(i=0;i<MAXTESTS;i++) dh_test_types[i] = 0;

 /*
  * Copy its contents over into dieharder_rng_generator_types.
  */
 i = 0;
 dh_num_diehard_tests = 0;

 ADD_TEST(&diehard_birthdays_dtest);
 dh_num_diehard_tests++;

 ADD_TEST(&diehard_operm5_dtest);
 dh_num_diehard_tests++;

 ADD_TEST(&diehard_rank_32x32_dtest);
 dh_num_diehard_tests++;

 ADD_TEST(&diehard_rank_6x8_dtest);
 dh_num_diehard_tests++;

 ADD_TEST(&diehard_bitstream_dtest);
 dh_num_diehard_tests++;

 //ADD_TEST(&diehard_opso_dtest);
 //dh_num_diehard_tests++;

 //ADD_TEST(&diehard_oqso_dtest);
 //dh_num_diehard_tests++;

 ADD_TEST(&diehard_dna_dtest);
 dh_num_diehard_tests++;

 ADD_TEST(&diehard_count_1s_stream_dtest);
 dh_num_diehard_tests++;

 ADD_TEST(&diehard_count_1s_byte_dtest);
 dh_num_diehard_tests++;

 ADD_TEST(&diehard_parking_lot_dtest);
 dh_num_diehard_tests++;

 ADD_TEST(&diehard_2dsphere_dtest);
 dh_num_diehard_tests++;

 ADD_TEST(&diehard_3dsphere_dtest);
 dh_num_diehard_tests++;

 ADD_TEST(&diehard_squeeze_dtest);
 dh_num_diehard_tests++;

 ADD_TEST(&diehard_sums_dtest);
 dh_num_diehard_tests++;

 ADD_TEST(&diehard_runs_dtest);
 dh_num_diehard_tests++;

 ADD_TEST(&diehard_craps_dtest);
 dh_num_diehard_tests++;

 ADD_TEST(&marsaglia_tsang_gcd_dtest);
 dh_num_diehard_tests++;

 MYDEBUG(D_TYPES){
   printf("# dieharder_test_types():  Found %u diehard tests.\n",dh_num_diehard_tests);
 }

 /*
  * Next, it is about time to add the first sts tests.
  */
 i = 100;
 ADD_TEST(&sts_monobit_dtest);
 dh_num_sts_tests++;

 ADD_TEST(&sts_runs_dtest);
 dh_num_sts_tests++;

 ADD_TEST(&sts_serial_dtest);
 dh_num_sts_tests++;

 /*
  * Finally, from here on we add the "rgb" tests, only they aren't,
  * really -- this is where all new non-diehard, non-sts tests will
  * go.  So we call them "other" tests.
  */
 i = 200;
 ADD_TEST(&rgb_bitdist_dtest);
 dh_num_other_tests++;

 ADD_TEST(&rgb_minimum_distance_dtest);
 dh_num_other_tests++;

 ADD_TEST(&rgb_permutations_dtest);
 dh_num_other_tests++;

 ADD_TEST(&rgb_lagged_sums_dtest);
 dh_num_other_tests++;

 ADD_TEST(&rgb_kstest_test_dtest);
 dh_num_other_tests++;

 ADD_TEST(&dab_bytedistrib_dtest);
 dh_num_other_tests++;

 ADD_TEST(&dab_dct_dtest);
 dh_num_other_tests++;

 ADD_TEST(&dab_filltree_dtest);
 dh_num_other_tests++;

 ADD_TEST(&dab_filltree2_dtest);
 dh_num_other_tests++;

 ADD_TEST(&dab_monobit2_dtest);
 dh_num_other_tests++;

 ADD_TEST(&dab_birthdays1_dtest);
 dh_num_other_tests++;

 ADD_TEST(&dab_opso2_dtest);
 dh_num_other_tests++;


 /*
  * This is the total number of DOCUMENTED tests reported back to the
  * UIs.  Note that dh_num_user_tests is counted up by add_ui_tests(),
  * which also sets this variable (so they can be called in either
  * order).
  */
 dh_num_tests = dh_num_diehard_tests + dh_num_sts_tests + dh_num_other_tests
                + dh_num_user_tests;

 /*
  * Except that clever old me will put an undocumented test range out here
  * at 900 reserved for development!  We move them back down to 200+
  * if/when we are ready to release them.  They are not looped over by
  * run_all_tests() but can of course be directly invoked by hand.
  */
 i = 900;

 /* ADD_TEST(&rgb_operm_dtest); */
 /* dh_num_other_tests++; */

 /* ADD_TEST(&rgb_lmn_dtest); */
 /* dh_num_other_tests++; */

 

}

