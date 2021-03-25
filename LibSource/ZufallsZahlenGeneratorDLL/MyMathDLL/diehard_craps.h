/*
 * diehard_craps test header.
 */

/*
 * function prototype
 */
int diehard_craps(Test **test,int irun);

_Check_return_
static 
Dtest diehard_craps_dtest = {
  "Diehard Craps Test",
  "diehard_craps",
  "\
#==================================================================\n\
#                   Diehard Craps Test\n\
#  This is the CRAPS TEST. It plays 200,000 games of craps, finds  \n\
#  the number of wins and the number of throws necessary to end    \n\
#  each game.  The number of wins should be (very close to) a      \n\
#  normal with mean 200000p and variance 200000p(1-p), with        \n\
#  p=244/495.  Throws necessary to complete the game can vary      \n\
#  from 1 to infinity, but counts for all>21 are lumped with 21.   \n\
#  A chi-square test is made on the no.-of-throws cell counts.     \n\
#  Each 32-bit integer from the test file provides the value for   \n\
#  the throw of a die, by floating to [0,1), multiplying by 6      \n\
#  and taking 1 plus the integer part of the result.               \n\
#==================================================================\n",
  100,     /* Default psamples */
  200000,  /* Default tsamples */
  2,       /* This test returns two statistics */
  diehard_craps,
  0
};

