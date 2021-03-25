/*
 * dab_dct test header.
 */

/*
 * function prototype
 */
int dab_dct(Test **test, int irun);

_Check_return_
static Dtest dab_dct_dtest = {
  "DAB DCT",
  "dab_dct",
  "\
#========================================================================\n\
#                    DCT (Frequency Analysis) Test\n\
#========================================================================\n\
#\n\
# This test performs a Discrete Cosine Transform (DCT) on the output of\n\
# the RNG. More specifically, it performs tsamples transforms, each over\n\
# an independent block of ntuple words. If tsamples is large enough, the\n\
# positions of the maximum (absolute) value in each transform are\n\
# recorded and subjected to a chisq test for uniformity/independence. [1]\n\
# (A standard type II DCT is used.)\n\
# \n\
# If tsamples is smaller than or equal to 5 times ntuple then a fallback\n\
# test will be used, whereby all DCT values are converted to p-values\n\
# and tested for uniformity via a KS test. This version is significantly\n\
# less sensitive, and is not recommended.\n\
#\n\
# Power: With the right parameters, this test catches more GSL\n\
# generators than any other; however, that count is biased by each of\n\
# the randomNNN generators having three copies.\n\
#\n\
# Limitations: ntuple is required to be a power of 2, because a radix 2\n\
# algorithm is used to calculate the DCT.\n\
#\n\
# False positives: targets are (mostly) calculated exactly, however it\n\
# will still return false positives when ntuple is small and tsamples is\n\
# very large. For the default ntuple value of 256, I get bad scores with\n\
# about 100 million or more tsamples (psamples set to 1).\n\
#\n\
# [1] The samples are taken as unsigned integers, and the DC coefficient\n\
# is adjusted to compensate for this.\n\
#/\n",
  1,
  50000,
  1,
  dab_dct,
  0
};

