/*
 * dab_bytedistrib test header.
 */

/*
 * function prototype
 */
int dab_bytedistrib(Test **test, int irun);

_Check_return_
static 
Dtest dab_bytedistrib_dtest = {
  "Byte Distribution",
  "dab_bytedistrib",
  "\
#==================================================================\n\
        		DAB Byte Distribution Test\n\
#\n\
# Extract n independent bytes from each of k consecutive words. Increment\n\
# indexed counters in each of n tables.  (Total of 256 * n counters.)\n\
# Currently, n=3 and is fixed at compile time.\n\
# If n>=2, then the lowest and highest bytes will be used, along\n\
# with n-2 bytes from the middle.\n\
# If the generator's word size is too small, overlapped bytes will\n\
# be used.\n\
# Current, k=3 and is fixed at compile time.\n\
# Use a basic chisq fitting test (chisq_pearson) for the p-value.\n\
# Previous version also used a chisq independence test (chisq2d); it\n\
# was found to be slightly less sensitive.\n\
# I envisioned this test as using a small number of samples and large\n\
# number of separate tests. Experiments so far show that keeping -p 1\n\
# and increasing -t performs best.\n\
#==================================================================\n",
  1,
  51200000,
  1,
  dab_bytedistrib,
  0
};

