/*
 * diehard_marsaglia_tsang_gcd test header.
 */

/*
 * function prototype
 */
int marsaglia_tsang_gcd(Test **test,int irun);

_Check_return_
static 
Dtest marsaglia_tsang_gcd_dtest = {
  "Marsaglia and Tsang GCD Test",
  "marsaglia_tsang_gcd",
  "\
#==================================================================\n\
#                     Marsaglia and Tsang GCD Test\n\
#\n\
# 10^7 tsamples (default) of uint rands u, v are generated and two\n\
# statistics are generated: their greatest common divisor (GCD) (w)\n\
# and the number of steps of Euclid's Method required to find it\n\
# (k).  Two tables of frequencies are thus generated -- one for the\n\
# number of times each value for k in the range 0 to 41 (with counts\n\
# greater than this range lumped in with the endpoints).\n\
# The other table is the frequency of occurrence of each GCD w.\n\
# k is be distributed approximately binomially, but this is useless for\n\
# the purposes of performing a stringent test.  Instead four \"good\"\n\
# RNGs (gfsr4,mt19937_1999,rndlxs2,taus2) were used to construct a\n\
# simulated table of high precision probabilities for k (a process that\n\
# obviously begs the question as to whether or not THESE generators\n\
# are \"good\" wrt the test).  At any rate, they produce very similar tables\n\
# and pass the test with each other's tables (and are otherwise very\n\
# different RNGs).  The table of probabilities for the gcd distribution is\n\
# generated dynamically per test (it is easy to compute).  Chisq tests\n\
# on both of these binned distributions yield two p-values per test,\n\
# and 100 (default) p-values of each are accumulated and subjected to\n\
# final KS tests and displayed in a histogram.\n\
#==================================================================\n",
  100,
  10000000,
  2,       /* This test returns two statistics */
  marsaglia_tsang_gcd,
  0
};

