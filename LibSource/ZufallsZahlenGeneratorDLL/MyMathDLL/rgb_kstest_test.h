/*
 * rgb_kstest_test test header
 */

/*
 * function prototype
 */
int rgb_kstest_test(Test **test,int irun);

_Check_return_
static Dtest rgb_kstest_test_dtest = {
  "RGB Kolmogorov-Smirnov Test Test",
  "rgb_kstest_test",
  "#\n\
#            The Kolmogorov-Smirnov Test Test\n\
#\n\
#\n\
# This test generates a vector of tsamples uniform deviates from the\n\
# selected rng, then applies an Anderson-Darling or Kuiper KS test to\n\
# it to directly test for uniformity.  The AD version has been symmetrized\n\
# to correct for weak left bias for small sample sets; Kuiper is already\n\
# ring-symmetric on the interval.  The AD code corresponds roughly to\n\
# what is in R (thanks to a correction sent in by David Bauer).\n\
# As always, the test is run pvalues times and the (same) KS test is then\n\
# used to generate a final test pvalue, but the real purpose of this test\n\
# is to test ADKS and KKS, not to test rngs.  This test clearly reveals\n\
# that kstests run on only 100 test values (tsamples, herein) are only\n\
# approximately accurate; their pvalues are distinctly high-biased (but\n\
# less so than Kuiper or KS before the fix).  This bias is hardly visible\n\
# for less than 1000 trivals (psamples, herein) but will constently cause\n\
# failure for -t 100, -p 10000 or higher.  For -t 1000, it is much more\n\
# difficult to detect, and the final kstest is approximately valid for the\n\
# test in question.\n\
#\n",
  1000,
  10000,
  1,
  rgb_kstest_test,
  0
};

