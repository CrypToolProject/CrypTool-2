/*
 * sts_serial test header.
 */

/*
 * function prototype
 */
int sts_serial(Test **test,int irun);

_Check_return_
static 
Dtest sts_serial_dtest = {
  "STS Serial Test (Generalized)",
  "sts_serial",
  "\
#========================================================================\n\
#                         STS Serial Test\n\
# Accumulates the frequencies of overlapping n-tuples of bits drawn\n\
# from a source of random integers.  The expected distribution of n-bit\n\
# patterns is multinomial with p = 2^(-n) e.g. the four 2-bit patterns\n\
#    00 01 10 11\n\
# should occur with equal probability.  The target distribution is thus\n\
# a simple chisq with 2^n - 1 degrees of freedom, one lost due to the\n\
# constraint that:\n\
#\n\
#         p_00 + p_01 + p_01 + p_11 = 1\n\
#\n\
# With overlap, though the test statistic is more complex.  For example,\n\
# given a bit string such as 0110100111000110 without overlap, it becomes\n\
# 01|10|10|01|11|00|01|10 and we count 1 00, 3 01s, 3 10s, and 1 11.\n\
# WITH overlap we get all of these patterns as well as (with cyclic wrap):\n\
# 0|11|01|00|11|10|00|11|0 and we count 4 00s, 4 01s, 4 10s, and 3 11s.\n\
# There is considerable covariance in the bit frequencies and a simple\n\
# chisq test no longer suffices.  The STS test uses target statistics that\n\
# are valid for overlapping samples but which require multiple orders\n\
# to generate.\n\
#\n\
# It is much easier to write a test that doesn't use overlapping samples\n\
# and directly checks to ensure that the distribution of bit ntuples\n\
# is consistent with a multinomial distribution with uniform probability\n\
# p = 1/2^n, e.g. 1/8 for n = 3 bit, 1/16 for n = 4 bit NON-overlapping\n\
# samples, and the rgb_bitdist is just such a test.  This test doesn't\n\
# require comparing different orders.  An open research question is\n\
# whether or not test sensitivity significantly depends on managing\n\
# overlap testing software RNGs where it is presumed that generation\n\
# is cheap and unlimited.  This question pertains to related tests, such\n\
# as overlapping permutations tests (where non-overlapping permutation\n\
# tests are isomorphic to non-overlapping frequency tests, fairly\n\
# obviously).\n\
#\n\
# This test does all the possible bitlevel tests from n=1 to n=24 bits\n\
# (where n=1 is basically sts_monobit, and n=2 IMO is redundant with\n\
# sts_runs).  However, if I understand things correctly it is not\n\
# possible to fail a 2 bit test and pass a 24 bit test, as if 2 bits are\n\
# biased so that (say) 00 occurs a bit too often, then 24 bit strings\n\
# containing 00's MUST be imbalanced as well relative to ones that do\n\
# not, so we really only need to check n=24 bit results to get all\n\
# the rest for free, so to speak.\n\
#\n",
  100,     /* Default psamples */
  100000,  /* Default tsamples */
  /* 44,    * We need to be ABLE to make 1 pvalue from m=1,2, 2 from m=[3,24] */
  30,      /* We need to be ABLE to make 1 pvalue from m=1,2, 2 from m=[3,16] */
  sts_serial,
  0
};

