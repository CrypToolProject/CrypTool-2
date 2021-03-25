/*
 * diehard_rgb_timing test header.  This is a nonstandard test and
 * returns no pvalues, and should probably be converted so that it
 * doesn't LOOK like a test.
 */

/*
 * function prototype
 */
typedef struct {
 double avg_time_nsec;
 double rands_per_sec;
} Rgb_Timing;
int rgb_timing(Test **test, Rgb_Timing *timing);

_Check_return_
static Dtest rgb_timing_dtest = {
  "RGB Timing Test",
  "rgb_timing",
  "\
#========================================================================\n\
#                      RGB Timing Test\n\
#\n\
# This test times the selected random number generator only.  It is\n\
# generally run at the beginning of a run of -a(ll) the tests to provide\n\
# some measure of the relative time taken up generating random numbers\n\
# for the various generators and tests.\n\
",
  10,        /* Number of psamples (passes) */
  1000000,   /* Number of samples in inner loop */
  1,         /* Number of tests */
  rgb_timing,
  0
};

