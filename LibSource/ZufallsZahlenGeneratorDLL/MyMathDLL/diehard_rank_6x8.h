/*
 * diehard_rank_6x8 test header.
 */

/*
 * function prototype
 */
int diehard_rank_6x8(Test **test,int irun);

_Check_return_
static 
Dtest diehard_rank_6x8_dtest = {
  "Diehard 6x8 Binary Rank Test",
  "diehard_rank_6x8",
  "\n\
#==================================================================\n\
#              Diehard 6x8 Binary Rank Test\n\
# This is the BINARY RANK TEST for 6x8 matrices.  From each of\n\
# six random 32-bit integers from the generator under test, a\n\
# specified byte is chosen, and the resulting six bytes form a\n\
# 6x8 binary matrix whose rank is determined.  That rank can be\n\
# from 0 to 6, but ranks 0,1,2,3 are rare; their counts are\n\
# pooled with those for rank 4. Ranks are found for 100,000\n\
# random matrices, and a chi-square test is performed on\n\
# counts for ranks 6,5 and <=4.\n\
#\n\
# As always, the test is repeated and a KS test applied to the\n\
# resulting p-values to verify that they are approximately uniform.\n\
#==================================================================\n",
  100,
  100000,
  1,
  diehard_rank_6x8,
  0
};

