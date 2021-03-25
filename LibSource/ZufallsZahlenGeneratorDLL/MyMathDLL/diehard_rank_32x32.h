/*
 * diehard_rank_32x32 test header.
 */

/*
 * function prototype
 */
int diehard_rank_32x32(Test **test,int irun);

_Check_return_
static 
Dtest diehard_rank_32x32_dtest = {
  "Diehard 32x32 Binary Rank Test",
  "diehard_rank_32x32",
  "\n\
#==================================================================\n\
#                Diehard 32x32 Binary Rank Test\n\
# This is the BINARY RANK TEST for 32x32 matrices. A random 32x\n\
# 32 binary matrix is formed, each row a 32-bit random integer.\n\
# The rank is determined. That rank can be from 0 to 32, ranks\n\
# less than 29 are rare, and their counts are pooled with those\n\
# for rank 29.  Ranks are found for 40,000 such random matrices\n\
# and a chisquare test is performed on counts for ranks  32,31,\n\
# 30 and <=29.\n\
#\n\
# As always, the test is repeated and a KS test applied to the\n\
# resulting p-values to verify that they are approximately uniform.\n\
#==================================================================\n",
  100,
  40000,
  1,
  diehard_rank_32x32,
  0
};

