/*
 * dab_opso2 test header.
 */

/*
 * function prototype
 */
int dab_opso2(Test **test, int irun);

static Dtest dab_opso2_dtest = {
  "DAB OPSO2",
  "dab_opso2",
  "\
#==================================================================\n\
#           DAB OPSO2 Test\n\
#  This test is misnamed.  It is an evolution of the OPSO test from\n\
#  the original Diehard program.  However, it does not use\n\
#           DAB OPSO2 Test\n\
#  This test is misnamed.  It is an evolution of the OPSO test from\n\
#  the original Diehard program.  However, it does not use\n\
#  overlapping samples.  Additionally, it returns two p-values,\n\
#  the second of which follows the Pairs-Sparse-Occupancy part of\n\
#  the name.  The first p-value effectively takes both letters from\n\
#  the same input word.  However, that isn't any different from\n\
#  having 1-letter words, where each letter is twice as long.\n\
# \n\
#  This verion uses 2^24 slots.  The first p-value thus takes 24\n\
#  bits directly from each input word.  The second p-value is based\n\
#  on two 12-bit letters from each of two words; each pair of input\n\
#  words will produce two output \"words\".\n\
# \n\
#  This test will give a false positive for all generators with an\n\
#  output word of less than 24 bits.\n\
# \n\
#  Note tsamples is set to 2^26 = 67108864, and cannot be varied.\n\
#\n\
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
#==================================================================\n",
  1,
  67108864,
  2,
  dab_opso2,
  0
};

