/*
 * diehard_count_1s_stream test header.
 */

/*
 * function prototype
 */
int diehard_count_1s_stream(Test **test,int irun);

_Check_return_
static Dtest diehard_count_1s_stream_dtest = {
  "Diehard Count the 1s (stream) Test",
  "diehard_count_1s_stream",
  "\
#==================================================================\n\
#          Diehard Count the 1s (stream) (modified) Test.\n\
# Consider the file under test as a stream of bytes (four per   \n\
# 32 bit integer).  Each byte can contain from 0 to 8 1's,      \n\
# with probabilities 1,8,28,56,70,56,28,8,1 over 256.  Now let  \n\
# the stream of bytes provide a string of overlapping  5-letter \n\
# words, each \"letter\" taking values A,B,C,D,E. The letters are \n\
# determined by the number of 1's in a byte::  0,1,or 2 yield A,\n\
# 3 yields B, 4 yields C, 5 yields D and 6,7 or 8 yield E. Thus \n\
# we have a monkey at a typewriter hitting five keys with vari- \n\
# ous probabilities (37,56,70,56,37 over 256).  There are 5^5   \n\
# possible 5-letter words, and from a string of 256,000 (over-  \n\
# lapping) 5-letter words, counts are made on the frequencies   \n\
# for each word.   The quadratic form in the weak inverse of    \n\
# the covariance matrix of the cell counts provides a chisquare \n\
# test::  Q5-Q4, the difference of the naive Pearson sums of    \n\
# (OBS-EXP)^2/EXP on counts for 5- and 4-letter cell counts.    \n\
#==================================================================\n",
  100,
  256000,
  1,
  diehard_count_1s_stream,
  0
};

