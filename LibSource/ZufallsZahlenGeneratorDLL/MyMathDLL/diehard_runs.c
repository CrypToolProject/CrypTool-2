/*
 * See copyright in copyright.h and the accompanying file COPYING
 */

/*
 *========================================================================
 * This is the Diehard Runs test, rewritten from the description
 * in tests.txt on George Marsaglia's diehard site.
 *
 * Here is the test description from diehard_tests.txt:
 *
 * This is the RUNS test. It counts runs up, and runs down,in a sequence
 * of uniform [0,1) variables, obtained by floating the 32-bit integers
 * in the specified file. This example shows how runs are counted:
 *  .123, .357, .789, .425,. 224, .416, .95
 * contains an up-run of length 3, a down-run of length 2 and an up-run
 * of (at least) 2, depending on the next values.  The covariance matrices
 * for the runs-up and runs-down are well-known, leading to chisquare tests
 * for quadratic forms in the weak inverses of the covariance matrices.
 * Runs are counted for sequences of length 10,000.  This is done ten times,
 * then repeated.
 *
 *                            Comment
 *
 * I modify this the following ways. First, I let the sequence length be
 * the variable -t (vector length) instead of fixing it at 10,000.  This
 * lets one test sequences that are much longer (entirely possible with
 * a modern CPU even for a fairly slow RNG).  Second, I repeat this for
 * the variable -s (samples) times, default 100 and not just 10.  Third,
 * because RNG's often have "bad seeds" for which they misbehave, the
 * individual sequences can be optionally -i reseeded for each sample.
 * Because this CAN let bad behavior be averaged out to where
 * it isn't apparent for many samples with few bad seeds, we may need to
 * plot the actual distribution of p-values for this and other tests where
 * this option is used.  Fourth, it is silly to convert integers into floats
 * in order to do this test.  Up sequences in integers are down sequences in
 * floats once one divides by the largest integer available to the generator,
 * period. Integer arithmetic is much faster than float AND one skips the
 * very costly division associated with conversion.
 * *========================================================================
 */


#include "libdieharder.h"
/*
 * The following are the definitions and parameters for runs, based on
 * Journal of Applied Statistics v30, Algorithm AS 157, 1981:
 *    The Runs-Up and Runs-Down Tests, by R. G. T. Grafton.
 * (and before that Knuth's The Art of Programming v. 2).
 */

#define RUN_MAX 6

/*
 * a_ij
 */
static double a[6][6] = {
	{ 4529.4, 9044.9, 13568.0, 18091.0, 22615.0, 27892.0 },
	{ 9044.9, 18097.0, 27139.0, 36187.0, 45234.0, 55789.0 },
	{ 13568.0, 27139.0, 40721.0, 54281.0, 67852.0, 83685.0 },
	{ 18091.0, 36187.0, 54281.0, 72414.0, 90470.0, 111580.0 },
	{ 22615.0, 45234.0, 67852.0, 90470.0, 113262.0, 139476.0 },
	{ 27892.0, 55789.0, 83685.0, 111580.0, 139476.0, 172860.0 }
};

/*
 * b_i
 */
static double b[6] = {
	1.0 / 6.0,
	5.0 / 24.0,
	11.0 / 120.0,
	19.0 / 720.0,
	29.0 / 5040.0,
	1.0 / 840.0, };

int diehard_runs(Test **test, int irun)
{

	int i, j, k, t;
	unsigned int ucount, dcount;
	int upruns[RUN_MAX], downruns[RUN_MAX];
	double uv, dv, up_pks, dn_pks;
	uint first, last, next = 0;

	/*
	 * This is just for display.
	 */
	test[0]->ntuple = 0;
	test[1]->ntuple = 0;

	/*
	 * Clear up and down run bins
	 */
	for (k = 0; k < RUN_MAX; k++){
		upruns[k] = 0;
		downruns[k] = 0;
	}

	/*
	 * Now count up and down runs and increment the bins.  Note
	 * that each successive up counts as a run of one down, and
	 * each successive down counts as a run of one up.
	 */
	ucount = dcount = 1;
	if (verbose){
		printf("j    rand    ucount  dcount\n");
	}
	first = last = gsl_rng_get(rng);
	for (t = 1; t<test[0]->tsamples; t++) {
		next = gsl_rng_get(rng);
		if (verbose){
			printf("%d:  %10u   %u    %u\n", t, next, ucount, dcount);
		}

		/*
		 * Did we increase?
		 */
		if (next > last){
			ucount++;
			if (ucount > RUN_MAX) ucount = RUN_MAX;
			downruns[dcount - 1]++;
			dcount = 1;
		}
		else {
			dcount++;
			if (dcount > RUN_MAX) dcount = RUN_MAX;
			upruns[ucount - 1]++;
			ucount = 1;
		}
		last = next;
	}
	if (next > first){
		ucount++;
		if (ucount > RUN_MAX) ucount = RUN_MAX;
		downruns[dcount - 1]++;
		dcount = 1;
	}
	else {
		dcount++;
		if (dcount > RUN_MAX) dcount = RUN_MAX;
		upruns[ucount - 1]++;
		ucount = 1;
	}

	/*
	 * This ends a single sample.
	 * Compute the test statistic for up and down runs.
	 */
	uv = 0.0;
	dv = 0.0;
	if (verbose){
		printf(" i      upruns    downruns\n");
	}
	for (i = 0; i < RUN_MAX; i++) {
		if (verbose){
			printf("%d:   %7d   %7d\n", i, upruns[i], downruns[i]);
		}
		for (j = 0; j < RUN_MAX; j++) {
			uv += ((double)upruns[i] - test[0]->tsamples*b[i])*(upruns[j] - test[0]->tsamples*b[j])*a[i][j];
			dv += ((double)downruns[i] - test[0]->tsamples*b[i])*(downruns[j] - test[0]->tsamples*b[j])*a[i][j];
		}
	}
	uv /= (double)test[0]->tsamples;
	dv /= (double)test[0]->tsamples;

	/*
	 * This NEEDS WORK!  It isn't right, somehow...
	 */
	up_pks = 1.0 - exp(-0.5 * uv) * (1.0 + 0.5 * uv + 0.125 * uv*uv);
	dn_pks = 1.0 - exp(-0.5 * dv) * (1.0 + 0.5 * dv + 0.125 * dv*dv);

	MYDEBUG(D_DIEHARD_RUNS) {
		printf("uv = %f   dv = %f\n", uv, dv);
	}
	test[0]->pvalues[irun] = gsl_sf_gamma_inc_Q(3.0, uv / 2.0);
	test[1]->pvalues[irun] = gsl_sf_gamma_inc_Q(3.0, dv / 2.0);

	MYDEBUG(D_DIEHARD_RUNS) {
		printf("# diehard_runs(): test[0]->pvalues[%u] = %10.5f\n", irun, test[0]->pvalues[irun]);
		printf("# diehard_runs(): test[1]->pvalues[%u] = %10.5f\n", irun, test[1]->pvalues[irun]);
	}

	return(0);

}

