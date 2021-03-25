/*
 * diehard_squeeze test header.
 */

/*
 * function prototype
 */
int diehard_squeeze(Test **test,int irun);

_Check_return_
static Dtest diehard_squeeze_dtest = {
  "Diehard Squeeze Test",
  "diehard_squeeze",
  "\
#==================================================================\n\
#                  Diehard Squeeze Test.\n\
#  Random integers are floated to get uniforms on [0,1). Start- \n\
#  ing with k=2^31=2147483647, the test finds j, the number of  \n\
#  iterations necessary to reduce k to 1, using the reduction   \n\
#  k=ceiling(k*U), with U provided by floating integers from    \n\
#  the file being tested.  Such j's are found 100,000 times,    \n\
#  then counts for the number of times j was <=6,7,...,47,>=48  \n\
#  are used to provide a chi-square test for cell frequencies.  \n\
#==================================================================\n",
  100,
  100000,
  1,
  diehard_squeeze,
  0
};

