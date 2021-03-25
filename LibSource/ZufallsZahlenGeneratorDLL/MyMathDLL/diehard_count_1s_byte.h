/*
 * diehard_count_1s_byte test header.
 */

/*
 * function prototype
 */
int diehard_count_1s_byte(Test **test,int irun);

_Check_return_
static 
Dtest diehard_count_1s_byte_dtest = {
  "Diehard Count the 1s Test (byte)",
  "diehard_count_1s_byte",
  "\
#==================================================================\n\
#         Diehard Count the 1s Test (byte) (modified).\n\
#     This is the COUNT-THE-1's TEST for specific bytes.        \n\
# Consider the file under test as a stream of 32-bit integers.  \n\
# From each integer, a specific byte is chosen , say the left-  \n\
# most::  bits 1 to 8. Each byte can contain from 0 to 8 1's,   \n\
# with probabilitie 1,8,28,56,70,56,28,8,1 over 256.  Now let   \n\
# the specified bytes from successive integers provide a string \n\
# of (overlapping) 5-letter words, each \"letter\" taking values  \n\
# A,B,C,D,E. The letters are determined  by the number of 1's,  \n\
# in that byte::  0,1,or 2 ---> A, 3 ---> B, 4 ---> C, 5 ---> D,\n\
# and  6,7 or 8 ---> E.  Thus we have a monkey at a typewriter  \n\
# hitting five keys with with various probabilities::  37,56,70,\n\
# 56,37 over 256. There are 5^5 possible 5-letter words, and    \n\
# from a string of 256,000 (overlapping) 5-letter words, counts \n\
# are made on the frequencies for each word. The quadratic form \n\
# in the weak inverse of the covariance matrix of the cell      \n\
# counts provides a chisquare test::  Q5-Q4, the difference of  \n\
# the naive Pearson  sums of (OBS-EXP)^2/EXP on counts for 5-   \n\
# and 4-letter cell counts.                                     \n\
# \n\
# Note: We actually cycle samples over all 0-31 bit offsets, so \n\
# that if there is a problem with any particular offset it has \n\
# a chance of being observed.  One can imagine problems with odd \n\
# offsets but not even, for example, or only with the offset 7.\n\
# tsamples and psamples can be freely varied, but you'll likely \n\
# need tsamples >> 100,000 to have enough to get a reliable kstest \n\
# result. \n\
#==================================================================\n",
  100,
  256000,
  1,
  diehard_count_1s_byte,
  0
};

