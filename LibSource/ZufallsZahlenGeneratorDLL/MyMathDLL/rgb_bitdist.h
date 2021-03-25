/*
 * rgb_bitdist test header.
 */

/*
 * function prototype
 */
int rgb_bitdist(Test **test,int irun);

_Check_return_
static Dtest rgb_bitdist_dtest = {
  "RGB Bit Distribution Test",
  "rgb_bitdist",
  "\n\
#========================================================================\n\
#                 RGB Bit Distribution Test\n\
# Accumulates the frequencies of all n-tuples of bits in a list\n\
# of random integers and compares the distribution thus generated\n\
# with the theoretical (binomial) histogram, forming chisq and the\n\
# associated p-value.  In this test n-tuples are selected without\n\
# WITHOUT overlap (e.g. 01|10|10|01|11|00|01|10) so the samples\n\
# are independent.  Every other sample is offset modulus of the\n\
# sample index and ntuple_max.\n\
#\n\
# This test must be run with -n ntuple for ntuple > 0.  Note that if\n\
# ntuple > 12, one should probably increase tsamples so that each of the\n\
# 2^ntuple bins should end up with an average of around 30 occurrences.\n\
# Note also that the memory requirements and CPU time requirements will\n\
# get quite large by e.g. ntuple = 20 -- use caution when sampling the\n\
# distribution of very large ntuples.\n\
#\n",
  100,     /* Default psamples */
  100000,  /* Default tsamples */
  1,       /* We magically make all the bit tests return a single histogram */
  rgb_bitdist,
  0
};

