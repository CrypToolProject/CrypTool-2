/*
 * ========================================================================
 * $Id: diehard_2dsphere.c 231 2006-08-22 16:18:05Z rgb $
 *
 * See copyright in copyright.h and the accompanying file COPYING
 * ========================================================================
 */

/*
 * ========================================================================
 *
 *            THE GENERALIZED MINIMUM DISTANCE TEST
 *
 * This is the generalized minimum distance test, based on the paper of
 * Mark Fischler in the doc directory and private communications.  This
 * test utilizes correction terms that are essential in order for the
 * test not to fail for large numbers of trials.  It replaces both
 * diehard_2dsphere.c and diehard_3dsphere.c, and generalizes the test
 * itself so that it can be run for any d = 2,3,4,5.  There is no
 * fundamental obstacle to running it for d = 1 or d>5, but one would need
 * to compute the expected overlap integrals (q) for the overlapping
 * d-spheres in the higher dimensions.  Note that in this test there is no
 * real need to stick to the parameters of Marsaglia.  The test by its
 * nature has three controls: n (the number of points used to sample the
 * minimum distance) which determines the granularity of the test -- the
 * approximate length scale probed for an excess of density; p, the usual
 * number of trials; and d the dimension.  As Fischler points out, to
 * actually resolve problems with a generator that had areas 20% off the
 * expected density (consistently) in d = 2, n = 8000 (Marsaglia's
 * parameters) would require around 2500 trials, where p = 100 (the old
 * test default) would resolve only consistent deviations of around 1.5
 * times the expected density.  By making both of these user selectable
 * parameters, dieharder should be able to test a generator pretty much
 * as thoroughly as one likes subject to the generous constraints
 * associated with the eventual need for still higher order corrections
 * as n and p are made large enough.
 * ========================================================================
 */


#include "libdieharder.h"

double rgb_mindist_avg;
static double rgb_md_Q[] = { 0.0, 0.0, 0.4135, 0.5312, 0.6202, 1.3789 };

int compare_points(const dTuple *a, const dTuple *b)
{
	double diff = a->c[0] - b->c[0];
	if (diff > 0) return 1;
	if (diff < 0) return -1;
	return 0;
}

double distance(const dTuple a, const dTuple b, uint dim)
{
	int i;
	double delta, distance = 0.0;
	for (i = 0; i < dim; i++){
		delta = a.c[i] - b.c[i];
		distance += delta*delta;
	}
	distance = sqrt(distance);
	return(distance);
}

int rgb_minimum_distance(Test **test, int irun)
{

	int i, j, d, t;
	uint rgb_md_dim;

	/*
	 * These are the vector of points and the current point being
	 * considered.  We may or may not need to restructure the vectors
	 * to be able to do the sort.  I'm going to TRY to implement
	 * Fischler's suggested algorithm here that is NlogN instead of doing
	 * the straightforward N^2 algorithm, but we'll see.
	 */
	dTuple *points;
	double earg, qarg, dist, mindist, dvolume;

	rgb_mindist_avg = 0.0;

	/*
	 * Generate d-tuples of tsamples random coordinates in the range
	 * 0-10000 (which we may have to scale with dimension). Determine
	 * the shortest separation of two points by any means available:
	 * a double loop is simplest and slowest, or we can sort the list
	 * of coordinates on the value of the first coordinate and evaluate
	 * distances for each point only until the distance to the next
	 * coordinate exceeds the minimum found so far, which scales like
	 * the sort (n log n) instead of the double loop (n^2).  From this
	 * we generate p from the Fischler form including corrections,
	 * actually computed in place so that e.g. n can be a variable, and
	 * apply the usual KS test over psamples of independent tests, per
	 * dimension.
	 *
	 * Actually, I don't see any particular reason that the d-cube should
	 * be of "length 10000".  In fact, it seems pretty obvious that it
	 * should have cube side = 1.0 since we're going to use double precision
	 * floating point variables.  Is there some aspect of this test that cares
	 * what the "scale" is?  I don't think so.
	 */
	points = (dTuple *)malloc(test[0]->tsamples*sizeof(dTuple));
	/*
	 * Set this for output.  ntuple should be set from the CLI or from
	 * -a(ll) (run_all_tests()).
	 */
	test[0]->ntuple = ntuple;
	rgb_md_dim = test[0]->ntuple;

	if (verbose == D_RGB_MINIMUM_DISTANCE || verbose == D_ALL){
		printf("Generating a list of %u points in %d dimensions\n", test[0]->tsamples, rgb_md_dim);
	}
	for (t = 0; t < test[0]->tsamples; t++){

		/*
		 * Generate a new d-dimensional point in the unit d-cube (with
		 * periodic boundary conditions).
		 */
		if (verbose == D_RGB_MINIMUM_DISTANCE || verbose == D_ALL){
			printf("points[%u]: (", t);
		}
		for (d = 0; d < rgb_md_dim; d++) {
			points[t].c[d] = gsl_rng_uniform_pos(rng);
			if (verbose == D_RGB_MINIMUM_DISTANCE || verbose == D_ALL){
				printf("%6.4f", points[t].c[d]);
				if (d == rgb_md_dim - 1){
					printf(")\n");
				}
				else {
					printf(",");
				}
			}
		}
	}

	/*
	 * Now we sort the points using gsl_heapsort and a comparator
	 * on the first coordinate only.  Don't know how to get rid
	 * of the gcc prototype warning.  Probably need a cast of some
	 * sort.
	 */
	gsl_heapsort(points, test[0]->tsamples, sizeof(dTuple),
		(gsl_comparison_fn_t)compare_points);

	if (verbose == D_RGB_MINIMUM_DISTANCE || verbose == D_ALL){
		printf("List of points sorted by first coordinate:\n");
		for (t = 0; t < test[0]->tsamples; t++){
			printf("points[%u]: (", t);
			for (d = 0; d < rgb_md_dim; d++) {
				printf("%6.4f", points[t].c[d]);
				if (d == rgb_md_dim - 1){
					printf(")\n");
				}
				else {
					printf(",");
				}
			}
		}
	}

	/*
	 * Now we do the SINGLE PASS through to determine mindist
	 */
	mindist = 1.0;
	for (i = 0; i < test[0]->tsamples; i++){
		/*
		 * One thing to experiment with here (very much) is
		 * whether or not we need periodic wraparound.  For
		 * the moment we omit it, although distributing
		 * the points on a euclidean d-torus seems more symmetric
		 * than not and checks to be sure that points are correct
		 * on or very near a boundary.
		 */
		for (j = i + 1; j<test[0]->tsamples; j++){
			if (points[j].c[0] - points[i].c[0] > mindist) break;
			dist = distance(points[j], points[i], rgb_md_dim);
			MYDEBUG(D_RGB_MINIMUM_DISTANCE) {
				printf("d(%d,%d) = %16.10e\n", i, j, dist);
			}
			if (dist < mindist) mindist = dist;
		}
	}
	MYDEBUG(D_RGB_MINIMUM_DISTANCE) {
		printf("Found rmin = %16.10e\n", mindist);
	}
	rgb_mindist_avg += mindist;

	/*
	 * OK, now we are cooking.  We have to DYNAMICALLY CALCULATE
	 * the pvalue using Fischler's formula, not Marsaglia's, with
	 * variables from the calculation.  We may put some warnings in
	 * to encourage people to use a minimum number of tsamples.  Then
	 * again, we may not -- seems like a research issue, right?
	 * People might well want to experiment without being pestered.
	 */
	if ((rgb_md_dim % 2) == 0){
		dvolume = pow(PI, rgb_md_dim / 2)*pow(mindist, rgb_md_dim) / gsl_sf_fact(rgb_md_dim / 2);
	}
	else {
		dvolume = 2.0*pow(2.0*PI, (rgb_md_dim - 1) / 2)*pow(mindist, rgb_md_dim) / gsl_sf_doublefact(rgb_md_dim);
	}
	earg = -1.0*test[0]->tsamples*(test[0]->tsamples - 1)*dvolume / 2.0;
	qarg = 1.0 + ((2.0 + rgb_md_Q[rgb_md_dim]) / 6.0)*pow(1.0*test[0]->tsamples, 3)*dvolume*dvolume;
	MYDEBUG(D_RGB_MINIMUM_DISTANCE) {
		printf("V_min = %16.10e, earg = %16.10e, qarg = %16.10e\n", dvolume, earg, qarg);
	}
	/* qarg = 1.0; */
	test[0]->pvalues[irun] = 1.0 - exp(earg)*qarg;

	free(points);

	MYDEBUG(D_RGB_MINIMUM_DISTANCE) {
		printf("# diehard_2dsphere(): test[0]->pvalues[%u] = %10.5f\n", irun, test[0]->pvalues[irun]);
	}

	/*
	 * I guess we return 0 on normal healthy return
	 */
	return(0);
}

