/*
 * diehard_bitstream test header.
 */

/*
 * function prototype
 */
int diehard_bitstream(Test **test,int irun);

_Check_return_
static Dtest diehard_bitstream_dtest = {
  "Diehard Bitstream Test",
  "diehard_bitstream",
  "\n\
#==================================================================\n\
#                  Diehard Bitstream Test.\n\
# The file under test is viewed as a stream of bits. Call them  \n\
# b1,b2,... .  Consider an alphabet with two \"letters\", 0 and 1 \n\
# and think of the stream of bits as a succession of 20-letter  \n\
# \"words\", overlapping.  Thus the first word is b1b2...b20, the \n\
# second is b2b3...b21, and so on.  The bitstream test counts   \n\
# the number of missing 20-letter (20-bit) words in a string of \n\
# 2^21 overlapping 20-letter words.  There are 2^20 possible 20 \n\
# letter words.  For a truly random string of 2^21+19 bits, the \n\
# number of missing words j should be (very close to) normally  \n\
# distributed with mean 141,909 and sigma 428.  Thus            \n\
# (j-141909)/428 should be a standard normal variate (z score) \n\
# that leads to a uniform [0,1) p value.  The test is repeated  \n\
# twenty times.                                                 \n\
#\n\
#                         NOTE WELL!\n\
#\n\
# The test is repeated 100 times by default in dieharder, but the\n\
# size of the sample is fixed (tsamples cannot/should not be\n\
# varied from the default).  The sigma of this test REQUIRES the\n\
# use of overlapping samples, and overlapping samples are not\n\
# independent.  If one uses the non-overlapping version of this\n\
# test, sigma = 290 is used instead, smaller because now there\n\
# are 2^21 INDEPENDENT samples.\n\
#==================================================================\n",
  100,
  2097152,
  1,
  diehard_bitstream,
  0
};

