/*
 * rgb_operm test header.
 */

/*
 * function prototype
 */
int rgb_operm(Test **test,int irun);

_Check_return_
static Dtest rgb_operm_dtest = {
  "RGB Overlapping Permuations Test",
  "rgb_operm",
  "\n\
#========================================================================\n\
#                 RGB Overlapping Permutations Test\n\
# Forms both the exact (expected) covariance matrix for overlapping\n\
# permutations of random integer and an empirical covariance matrix\n\
# formed from a long string of samples.  The difference is expected\n\
# to have a chisq distribution and hence can be transformed into a\n\
# sample p-value.  Note that this is one possible functional replacement\n\
# for the broken/defunct diehard operm5 test, but one that permits k (the\n\
# number of numbers in the overlapping permutation window) to be varied\n\
# from 2 to perhaps 8.\n\
#\n",
  100,     /* Default psamples */
  100000,  /* Default tsamples */
  1,       /* We magically make all the bit tests return a single histogram */
  rgb_operm,
  0
};

/*
 * Global variables.
 *
 * rgb_operm_k is the size of the overlapping window that is slid along
 * a data stream of rands from x_i to x_{i+k} to compute c[][].
 */
unsigned int rgb_operm_k;
