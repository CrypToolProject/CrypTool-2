/*
 * dab_filltree test header.
 */

/*
 * function prototype
 */
int dab_filltree(Test **test, int irun);

_Check_return_
static Dtest dab_filltree_dtest = {
  "DAB Fill Tree Test",
  "dab_filltree",
  "\
#==================================================================\n\
#                DAB Fill Tree Test\n\
# This test fills small binary trees of fixed depth with\n\
# words from the the RNG.  When a word cannot be inserted\n\
# into the tree, the current count of words in the tree is\n\
# recorded, along with the position at which the word\n\
# would have been inserted.\n\
#\n\
# The words from the RNG are rotated (in long cycles) to\n\
# better detect RNGs that may bias only the high, middle,\n\
# or low bytes.\n\
#\n\
# The test returns two p-values.  The first is a Pearson\n\
# chi-sq test against the expected values (which were\n\
# estimated empirically).  The second is a Pearson chi-sq\n\
# test for a uniform distribution of the positions at\n\
# which the insert failed.\n\
#\n\
# Because of the target data for the first p-value,\n\
# ntuple must be kept at the default (32).\n\
#==================================================================\n",
  1,
  15000000,
  2,
  dab_filltree,
  0
};

