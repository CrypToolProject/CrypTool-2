/*
 * diehard_3dsphere test header.
 */

/*
 * function prototype
 */
int diehard_3dsphere(Test **test,int irun);

_Check_return_
static Dtest diehard_3dsphere_dtest = {
  "Diehard 3d Sphere (Minimum Distance) Test",
  "diehard_3dsphere",
  "\
#==================================================================\n\
#          Diehard 3d Sphere (Minimum Distance) Test\n\
# Choose  4000 random points in a cube of edge 1000.  At each   \n\
# point, center a sphere large enough to reach the next closest \n\
# point. Then the volume of the smallest such sphere is (very   \n\
# close to) exponentially distributed with mean 120pi/3.  Thus  \n\
# the radius cubed is exponential with mean 30. (The mean is    \n\
# obtained by extensive simulation).  The 3DSPHERES test gener- \n\
# ates 4000 such spheres 20 times.  Each min radius cubed leads \n\
# to a uniform variable by means of 1-exp(-r^3/30.), then a     \n\
#  KSTEST is done on the 20 p-values.                           \n\
#\n\
# This test ignores tsamples, and runs the usual default 100\n\
# psamples to use in the final KS test.\n\
#==================================================================\n",
  100,
  4000,
  1,
  diehard_3dsphere,
  0
};

