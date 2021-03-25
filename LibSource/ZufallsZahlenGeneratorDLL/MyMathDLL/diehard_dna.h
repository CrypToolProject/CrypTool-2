/*
 * diehard_dna test header.
 */

/*
 * function prototype
 */
int diehard_dna(Test **test,int irun);

_Check_return_
static 
Dtest diehard_dna_dtest = {
  "Diehard DNA Test",
  "diehard_dna",
  "\
#==================================================================\n\
#                    Diehard DNA Test.\n\
# \n\
#   The DNA test considers an alphabet of 4 letters::  C,G,A,T,\n\
# determined by two designated bits in the sequence of random   \n\
# integers being tested.  It considers 10-letter words, so that \n\
# as in OPSO and OQSO, there are 2^20 possible words, and the   \n\
# mean number of missing words from a string of 2^21  (over-    \n\
# lapping)  10-letter  words (2^21+9 \"keystrokes\") is 141909.   \n\
# The standard deviation sigma=339 was determined as for OQSO   \n\
# by simulation.  (Sigma for OPSO, 290, is the true value (to   \n\
# three places), not determined by simulation.                  \n\
# \n\
# Note 2^21 = 2097152\n\
# Note also that we don't bother with overlapping keystrokes \n\
# (and sample more rands -- rands are now cheap). \n\
#==================================================================\n",
  100,
  2097152,
  1,
  diehard_dna,
  0
};

