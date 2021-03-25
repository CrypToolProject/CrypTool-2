/*
 * diehard_runs test header.
 */

/*
 * function prototype
 */
int diehard_runs(Test **test,int irun);

_Check_return_
static 
Dtest diehard_runs_dtest = {
  "Diehard Runs Test",
  "diehard_runs",
  "\
#==================================================================\n\
#                    Diehard Runs Test\n\
#  This is the RUNS test.  It counts runs up, and runs down, \n\
# in a sequence of uniform [0,1) variables, obtained by float-  \n\
# ing the 32-bit integers in the specified file. This example   \n\
# shows how runs are counted:  .123,.357,.789,.425,.224,.416,.95\n\
# contains an up-run of length 3, a down-run of length 2 and an \n\
# up-run of (at least) 2, depending on the next values.  The    \n\
# covariance matrices for the runs-up and runs-down are well    \n\
# known, leading to chisquare tests for quadratic forms in the  \n\
# weak inverses of the covariance matrices.  Runs are counted   \n\
# for sequences of length 10,000.  This is done ten times. Then \n\
# repeated.                                                     \n\
#\n\
# In Dieharder sequences of length tsamples = 100000 are used by\n\
# default, and 100 p-values thus generated are used in a final\n\
# KS test.\n\
#==================================================================\n",
  100,     /* Default psamples */
  100000,  /* Default tsamples */
  2,       /* runs returns two pvalues, not just one */
  diehard_runs,
  0
};

