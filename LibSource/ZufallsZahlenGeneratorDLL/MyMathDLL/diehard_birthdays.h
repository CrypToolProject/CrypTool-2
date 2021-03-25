/*
 * diehard_birthdays test header.
 */

/*
 * function prototype
 */
int diehard_birthdays(Test **test, int irun);

_Check_return_
static Dtest diehard_birthdays_dtest = {
  "Diehard Birthdays Test",
  "diehard_birthdays",
  "\n\
#==================================================================\n\
#                Diehard \"Birthdays\" test (modified).\n\
# Each test determines the number of matching intervals from 512\n\
# \"birthdays\" (by default) drawn on a 24-bit \"year\" (by\n\
# default).  This is repeated 100 times (by default) and the\n\
# results cumulated in a histogram.  Repeated intervals should be\n\
# distributed in a Poisson distribution if the underlying generator\n\
# is random enough, and a a chisq and p-value for the test are\n\
# evaluated relative to this null hypothesis.\n\
#\n\
# It is recommended that you run this at or near the original\n\
# 100 test samples per p-value with -t 100.\n\
#\n\
# Two additional parameters have been added. In diehard, nms=512\n\
# but this CAN be varied and all Marsaglia's formulae still work.  It\n\
# can be reset to different values with -x nmsvalue.\n\
# Similarly, nbits \"should\" 24, but we can really make it anything\n\
# we want that's less than or equal to rmax_bits = 32.  It can be\n\
# reset to a new value with -y nbits.  Both default to diehard's\n\
# values if no -x or -y options are used.\n\
#==================================================================\n",
  100,
  100,
  1,
  diehard_birthdays,
  0
};

/*
 * Global variables
uint diehard_birthdays_nms,diehard_birthdays_nbits;
uint *diehard_birthdays_rand_uint;
 */
