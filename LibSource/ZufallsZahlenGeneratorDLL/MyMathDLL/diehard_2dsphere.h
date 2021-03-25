/*
 * diehard_2dsphere test header.
 */

/*
 * function prototype
 */
int diehard_2dsphere(Test **test,int irun);

_Check_return_
static Dtest diehard_2dsphere_dtest = {
  "Diehard Minimum Distance (2d Circle) Test",
  "diehard_2dsphere",
  "\
#==================================================================\n\
#         Diehard Minimum Distance (2d Circle) Test \n\
# It does this 100 times::   choose n=8000 random points in a   \n\
# square of side 10000.  Find d, the minimum distance between   \n\
# the (n^2-n)/2 pairs of points.  If the points are truly inde- \n\
# pendent uniform, then d^2, the square of the minimum distance \n\
# should be (very close to) exponentially distributed with mean \n\
# .995 .  Thus 1-exp(-d^2/.995) should be uniform on [0,1) and  \n\
# a KSTEST on the resulting 100 values serves as a test of uni- \n\
# formity for random points in the square. Test numbers=0 mod 5 \n\
# are printed but the KSTEST is based on the full set of 100    \n\
# random choices of 8000 points in the 10000x10000 square.      \n\
#\n\
# This test uses a fixed number of samples -- tsamples is ignored.\n\
# It also uses the default value of 100 psamples in the final\n\
# KS test, for once agreeing precisely with Diehard.\n\
#==================================================================\n",
  100,
  8000,
  1,
  diehard_2dsphere,
  0
};

