/*
 * dab_birthdays1 test header.
 */

/*
 * function prototype
 */
int dab_birthdays1(Test **test, int irun);

_Check_return_
static Dtest dab_birthdays1_dtest = {
  "Diehard Birthdays Test",
  "dab_birthdays1",
  "\n\
#==================================================================\n\
#                Diehard \"Birthdays\" test (modified).\n\
# This is a version of the Diehard Birthdays test, modified from the\n\
# original Dieharder implementation of it.\n\
# \n\
#                 This is a BIRTHDAY SPACINGS TEST\n\
# Choose m birthdays in a year of n days.  List the spacings between \n\
# the birthdays.  If j is the number of values that occur more than \n\
# once in that list, then j is asympotically Poisson distributed with \n\
# mean lambda = m^3/(4n).  A Chi-Sq test is performed comparing the \n\
# seen distribution of repeated spacings to the Poisson distribution. \n\
#  Simulations show that the approximation is better for larger n and \n\
# smaller lambda.  However, since for any given run j must be an \n\
# integer, a small lambda value requires more runs to build up a good \n\
# statistic.  This test uses m=1700 as the default, but it may \n\
# changed (via the -n (ntuple) option), up to a maximum value of \n\
# 4096.  The value of n is fixed by the choice of generator, with \n\
# n=2^r, where r is the number of bits per word in the generator's \n\
# output (a maximum of 32 for this version of Dieharder).  This test \n\
# prefers a larger t-count (-t option) and p-value samples set to 1 \n\
# (-p 1, which is the default).\n\
# \n\
# Be careful when running this test against generators with reduced \n\
# word sizes, as it may give false positives.  When it doubt, check \n\
# against an assumed good generator that is set to produce the same \n\
# size output.  As an example, for testing a generator with an output \n\
# size of 20 bits, using \"-n 50 -t 8000\" produced a test that \n\
# repeated passed an assumed good generator at \"-p 100\", but had \n\
# trouble at \"-p 500\".  Alternately, raising the t-count also shows \n\
# that m of 50 isn't low enough to give a good approximation.  For \n\
# long tests of generators with an output size smaller than 30 bits, \n\
# producing the target by simulation instead of relying on the \n\
# Poisson approximation will probably be necessary.\n\
# \n\
#==================================================================\n",
  1,
  2000,
  1,
  dab_birthdays1,
  0
};

/*
 * Global variables
uint dab_birthdays1_nms,dab_birthdays1_nbits;
uint *dab_birthdays1_rand_uint;
 */
