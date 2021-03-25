/*
 * rgb_minimum_distance test header
 */

/*
 * function prototype
 */
int rgb_minimum_distance(Test **test,int irun);

_Check_return_
static Dtest rgb_minimum_distance_dtest = {
  "RGB Generalized Minimum Distance Test",
  "rgb_minimum_distance",
  "#\n\
#            THE GENERALIZED MINIMUM DISTANCE TEST\n\
#\n\
# This is the generalized minimum distance test, based on the paper of M.\n\
# Fischler in the doc directory and private communications.  This test\n\
# utilizes correction terms that are essential in order for the test not\n\
# to fail for large numbers of trials.  It replaces both\n\
# diehard_2dsphere.c and diehard_3dsphere.c, and generalizes the test\n\
# itself so that it can be run for any d = 2,3,4,5.  There is no\n\
# fundamental obstacle to running it for d = 1 or d>5, but one would need\n\
# to compute the expected overlap integrals (q) for the overlapping\n\
# d-spheres in the higher dimensions.  Note that in this test there is no\n\
# real need to stick to the parameters of Marsaglia.  The test by its\n\
# nature has three controls: n (the number of points used to sample the\n\
# minimum distance) which determines the granularity of the test -- the\n\
# approximate length scale probed for an excess of density; p, the usual\n\
# number of trials; and d the dimension.  As Fischler points out, to\n\
# actually resolve problems with a generator that had areas 20% off the\n\
# expected density (consistently) in d = 2, n = 8000 (Marsaglia's\n\
# parameters) would require around 2500 trials, where p = 100 (the old\n\
# test default) would resolve only consistent deviations of around 1.5\n\
# times the expected density.  By making both of these user selectable\n\
# parameters, dieharder should be able to test a generator pretty much\n\
# as thoroughly as one likes subject to the generous constraints\n\
# associated with the eventual need for still higher order corrections\n\
# as n and p are made large enough.\n\
#\n",
  1000,
  10000,
  1,
  rgb_minimum_distance,
  0
};

