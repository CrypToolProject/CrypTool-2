/*
 * rgb_permutations test header.
 */

/*
 * function prototype
 */
int rgb_permutations(Test **test,int irun);

_Check_return_
static 
Dtest rgb_permutations_dtest = {
  "RGB Permutations Test",
  "rgb_permutations",
  "\n\
#========================================================================\n\
#                       RGB Permutations Test\n\
# This is a non-overlapping test that simply counts order permutations of\n\
# random numbers, pulled out n at a time.  There are n! permutations\n\
# and all are equally likely.  The samples are independent, so one can\n\
# do a simple chisq test on the count vector with n! - 1 degrees of\n\
# freedom.  This is a poor-man's version of the overlapping permutations\n\
# tests, which are much more difficult because of the covariance of the\n\
# overlapping samples.\n\
#\n",
  100,     /* Default psamples */
  100000,  /* Default tsamples */
  1,       /* We magically make all the bit tests return a single histogram */
  rgb_permutations,
  0
};

