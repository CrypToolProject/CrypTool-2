/*
 * dab_monobit2 test header.
 */

/*
 * function prototype
 */
int dab_monobit2(Test **test,int irun);

_Check_return_
static Dtest dab_monobit2_dtest = {
  "DAB Monobit 2 Test",
  "dab_monobit2",
  "\
#==================================================================\n\
#                     DAB Monobit 2 Test\n\
# Block-monobit test.\n\
# Since we don't know what block size to use, try multiple block\n\
# sizes.  In particular, try all block sizes of 2^k words, where\n\
# k={0..n}.  The value of n is calculated from the word size of the\n\
# generator and the sample size used, and is shown as ntuple.\n\
#==================================================================\n",
  1,
  65000000,
  1,
  dab_monobit2,
  0
};

