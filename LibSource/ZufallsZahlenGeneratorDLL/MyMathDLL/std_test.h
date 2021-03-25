/*
 * This is is the std_test struct.  It is the sole output interface between
 * the user and the test program -- all standard test inputs and returns
 * are passed through this struct.  A test can have additional user-settable
 * arguments, of course, but they must be passed in GLOBAL SHARED VARIABLES
 * defined just for the particular test in question.  This is a bit ugly,
 * but so are void *args argument lists and va_start/va_end for multiple
 * layers of passing arguments.
 *
 * This is silly.  We clearly need for each test to have access to all
 * variables set on the command line.  That way the ONE Test object
 * can be passed to e.g report() or table() and they'll know exactly
 * what to do with it.
 */
typedef struct {
  unsigned int nkps;           /* Number of test statistics created per run */
  unsigned int tsamples;       /* Number of samples per test (if applicable) */
  unsigned int psamples;       /* Number of test runs per final KS p-value */
  unsigned int ntuple;         /* Number of bits in ntuples being tested */
  double *pvalues;     /* Vector of length psamples to hold test p-values */
  char *pvlabel;       /* Vector of length LINE to hold labels per p-value */
  double ks_pvalue;    /* Final KS p-value from run of many tests */
  double x;            /* Extra variable passed on command line */
  double y;            /* Extra variable passed on command line */
  double z;            /* Extra variable passed on command line */
} Test;


Test **create_test(Dtest *dtest, unsigned int tsamples, unsigned int psamples);
void destroy_test(Dtest *dtest, Test **test);
void std_test(Dtest *dtest, Test **test);

