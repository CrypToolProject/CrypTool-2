/*
 * Hopefully this is a valid default initialization of the template test.
 */

#include "libdieharder.h"

int rgb_lmn(Dtest *dtest,Test **test);

static Dtest lmn_test = {
  "RGB lmn Test",
  "rgb_lmn_test",
  "\n\
#==================================================================\n\
#                      RGB lmn Test\n\
#\n\
# This is a template for a Universal Test (of sorts) that replaces\n\
# or subsumes sts_series and rgb_bitdist.  It should prove to be\n\
# a very powerful way of revealing bitlevel correlations and indeed\n\
# should systematically subsume many other tests as well.\n\
#==================================================================\n",
  100,
  100000,
};
