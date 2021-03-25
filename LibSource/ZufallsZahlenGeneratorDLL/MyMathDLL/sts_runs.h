/*
 * sts_runs test header.
 */

/*
 * function prototype
 */
int sts_runs(Test **test,int irun);

_Check_return_
static Dtest sts_runs_dtest = {
  "STS Runs Test",
  "sts_runs",
  "\
#==================================================================\n\
#                       STS Runs Test\n\
# Counts the total number of 0 runs + total number of 1 runs across\n\
# a sample of bits.  Note that a 0 run must begin with 10 and end\n\
# with 01.  Note that a 1 run must begin with 01 and end with a 10.\n\
# This test, run on a bitstring with cyclic boundary conditions, is\n\
# absolutely equivalent to just counting the 01 + 10 bit pairs.\n\
# It is therefore totally redundant with but not as good as the\n\
# rgb_bitdist() test for 2-tuples, which looks beyond the means to the\n\
# moments, testing an entire histogram  of 00, 01, 10, and 11 counts\n\
# to see if it is binomially distributed with p = 0.25.\n\
#==================================================================\n",
  100,
  100000,
  1,
  sts_runs,
  0
};


