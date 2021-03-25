/*
 * diehard_sums test header.
 */

/*
 * function prototype
 */
int diehard_sums(Test **test,int irun);

_Check_return_
static Dtest diehard_sums_dtest = {
  "Diehard Sums Test",
  "diehard_sums",
  "\
#==================================================================\n\
#                  Diehard Sums Test\n\
# Integers are floated to get a sequence U(1),U(2),... of uni-  \n\
# form [0,1) variables.  Then overlapping sums,                 \n\
#   S(1)=U(1)+...+U(100), S2=U(2)+...+U(101),... are formed.    \n\
# The S's are virtually normal with a certain covariance mat-   \n\
# rix.  A linear transformation of the S's converts them to a   \n\
# sequence of independent standard normals, which are converted \n\
# to uniform variables for a KSTEST. The  p-values from ten     \n\
# KSTESTs are given still another KSTEST.                       \n\
#\n\
#                       Comments\n\
#\n\
# At this point I think there is rock solid evidence that this test\n\
# is completely useless in every sense of the word.  It is broken,\n\
# and it is so broken that there is no point in trying to fix it.\n\
# The problem is that the transformation above is not linear, and\n\
# doesn't work.  Don't use it.\n\
#\n\
# For what it is worth, rgb_lagged_sums with ntuple 0 tests for\n\
# exactly the same thing, but scalably and reliably without the\n\
# complication of overlapping samples and covariance.  Use it\n\
# instead.\n\
#==================================================================\n",
  100,
  100,
  1,
  diehard_sums,
  0
};

