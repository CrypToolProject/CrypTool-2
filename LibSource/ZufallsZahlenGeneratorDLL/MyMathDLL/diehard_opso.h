/*
 * diehard_opso test header.
 */

/*
 * function prototype
 */
int diehard_opso(Test **test, int irun);

_Check_return_
static 
Dtest diehard_opso_dtest = {
  "Diehard OPSO",
  "diehard_opso",
  "\
#==================================================================\n\
#        Diehard Overlapping Pairs Sparse Occupance (OPSO)\n\
# The OPSO test considers 2-letter words from an alphabet of    \n\
# 1024 letters.  Each letter is determined by a specified ten   \n\
# bits from a 32-bit integer in the sequence to be tested. OPSO \n\
# generates  2^21 (overlapping) 2-letter words  (from 2^21+1    \n\
# \"keystrokes\")  and counts the number of missing words---that  \n\
# is 2-letter words which do not appear in the entire sequence. \n\
# That count should be very close to normally distributed with  \n\
# mean 141,909, sigma 290. Thus (missingwrds-141909)/290 should \n\
# be a standard normal variable. The OPSO test takes 32 bits at \n\
# a time from the test file and uses a designated set of ten    \n\
# consecutive bits. It then restarts the file for the next de-  \n\
# signated 10 bits, and so on.                                  \n\
# \n\
#  Note 2^21 = 2097152, tsamples cannot be varied.\n\
#==================================================================\n",
  100,
  2097152,
  1,
  diehard_opso,
  0
};

