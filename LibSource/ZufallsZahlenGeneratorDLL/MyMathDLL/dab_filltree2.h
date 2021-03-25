/*
 * dab_filltree2 test header.
 */

/*
 * function prototype
 */
int dab_filltree2(Test **test, int irun);

_Check_return_
static Dtest dab_filltree2_dtest = {
  "DAB Fill Tree 2 Test",
  "dab_filltree2",
  "\
#==========================================================\n\
#        DAB Fill Tree 2 Test\n\
# Bit version of Fill Tree test.\n\
# This test fills small binary trees of fixed depth with\n\
# \"visited\" markers.  When a marker cannot be placed, the\n\
# current count of markers in the tree and the position\n\
# that the marker would have been inserted, if it hadn't\n\
# already been marked.\n\
#\n\
# For each bit in the RNG input, the test takes a step\n\
# right (for a zero) or left (for a one) in the tree.\n\
# If the node hasn't been marked, it is marked, and the\n\
# path restarts.  Otherwise, the test continues with the\n\
# next bit.\n\
#\n\
# The test returns two p-values.  The first is a Pearson\n\
# chi-sq test against the expected values (which were\n\
# estimated empirically.  The second is a Pearson chi-sq\n\
# test for a uniform distribution of the positions at\n\
# which the insert failed.\n\
#\n\
# Because of the target data for the first p-value,\n\
# ntuple must be kept at the default (128).\n\
#==========================================================\n",
  1,
  5000000,
  2,
  dab_filltree2,
  0
};

