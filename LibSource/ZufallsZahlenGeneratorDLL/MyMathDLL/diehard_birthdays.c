/*
 * ========================================================================
 * $Id: diehard_birthdays.c 250 2006-10-10 05:02:26Z rgb $
 *
 * See copyright in copyright.h and the accompanying file COPYING
 * ========================================================================
 */

/*
 * ========================================================================
 * This is the Diehard Birthdays test, rewritten from the description
 * in tests.txt on George Marsaglia's diehard site.
 *
 *               This is the BIRTHDAY SPACINGS TEST
 * Choose m birthdays in a year of n days.  List the spacings
 * between the birthdays.  If j is the number of values that
 * occur more than once in that list, then j is asymptotically
 * Poisson distributed with mean m^3/(4n).  Experience shows n
 * must be quite large, say n>=2^18, for comparing the results
 * to the Poisson distribution with that mean.  This test uses
 * n=2^24 and m=2^9,  so that the underlying distribution for j
 * is taken to be Poisson with lambda=2^27/(2^26)=2.  A sample
 * of 500 j's is taken, and a chi-square goodness of fit test
 * provides a p value.  The first test uses bits 1-24 (counting
 * from the left) from integers in the specified file.
 *   Then the file is closed and reopened. Next, bits 2-25 are
 * used to provide birthdays, then 3-26 and so on to bits 9-32.
 * Each set of bits provides a p-value, and the nine p-values
 * provide a sample for a KSTEST.
 *
 *                            Comment
 *
 * I'm modifying this test several ways.  First, I'm running it on all 32
 * 24 bit strings implicit in the variables.  We'll do this by rotating
 * each variable by one bit position in between a simple run of the test.
 * A full run will therefore be 32 simple (rotated) runs on bits 1-24, and
 * we can do -p psamples runs to get a final set of p-values to evaluate.
 *========================================================================
 */


#include "libdieharder.h"
#define NMS   512
#define NBITS 24

static double lambda;
static unsigned int *intervals;
static unsigned int nms, nbits, kmax;

int diehard_birthdays(Test **test, int irun)
{

	uint i, k, t, m, mnext;
	uint *js;
	uint rand_uint[NMS];

	double binfreq;

	/*
	 * for display only.  0 means "ignored".
	 */
	test[0]->ntuple = 0;

	/*
	 * We are taking a small step backwards and fixing these
	 * so that they are the old diehard values until we figure
	 * out how to deal with variable argument lists, which may be
	 * never in the CLI version.
	 */
	/* Cruft nms = diehard_birthdays_nms; */
	nms = NMS;
	/* Cruft nbits = diehard_birthdays_nbits; */
	nbits = NBITS;
	if (nbits > rmax_bits) nbits = rmax_bits;

	/*
	 * This is the one thing that matters.  We're going to make the
	 * exact poisson distribution we expect below, and lambda has to
	 * be right.  lambda = nms^3/4n where n = 2^nbits, which is:
	 *   lambda = (2^9)^3/(2^2 * 2^24) = 2^27/2^26 = 2.0
	 * for Marsaglia's defaults.  Small changes in nms make big changes
	 * in lambda, but we can easily pick e.g. nbits = 30, nms = 2048
	 *   lambda =  (2^11)^3/2^32 = 2.0
	 * and get the same test, but with a lot more samples and perhaps a
	 * slightly smoother result.
	 */
	lambda = (double)nms*nms*nms / pow(2.0, (double)nbits + 2.0);

	/*
	 * Allocate memory for intervals
	 */
	intervals = (unsigned int *)malloc(nms*sizeof(unsigned int));

	/*
	 * This should be more than twice as many slots as we really
	 * need for the Poissonian tail.  We're going to sample tsamples
	 * times, and we only want to keep the histogram out to where
	 * it has a reasonable number of hits/degrees of freedom, just
	 * like we do with all the chisq's built on histograms.
	 */
	kmax = 1;
	while ((binfreq = test[0]->tsamples*gsl_ran_poisson_pdf(kmax, lambda)) > 5) {
		kmax++;
	}
	/* Cruft: printf("binfreq[%u] = %f\n",kmax,binfreq); */
	kmax++;   /* and one to grow on...*/

	/*
	 * js[kmax] is the histogram we increment using the
	 * count of repeated intervals as an index.  Clear it.
	 */
	js = (unsigned int *)malloc(kmax*sizeof(unsigned int));
	for (i = 0; i < kmax; i++) js[i] = 0;

	/*
	 * Each sample uses a unique set of tsample rand_uint[]'s, but evaluates
	 * the Poissonian statistic for each cyclic rotation of the bits across
	 * the 24 bit mask.
	 */
	for (t = 0; t < test[0]->tsamples; t++) {

		/*
		 * Create a list of 24-bit masked rands.  This is easy now that
		 * we have get_bit_ntuple().  We use a more or less random offset
		 * of the bitstring, and use one and only one random number
		 * per bitstring, so that our samples are >>independent<<, and average
		 * over any particular bit position used as a starting point with
		 * cyclic/periodic bit wrap.
		 */
		memset(rand_uint, 0, nms*sizeof(uint));
		for (m = 0; m < nms; m++){
			/*
			 * This tests PRECISELY nbits guaranteed sequential bits from the
			 * generator, with no gaps.  We could actually test 32.
			 *
			 * Note -- removed all reference to overlap.
			 */
			get_rand_bits(&rand_uint[m], sizeof(uint), nbits, rng);
			MYDEBUG(D_DIEHARD_BDAY){
				printf("  %d-bit int = ", nbits);
				/* Should count dump from the right, sorry */
				dumpbits(&rand_uint[m], 32);
				printf("\n");
			}
			//printf("current m: %i \n", m);
		}

		/*
		 * The actual test logic goes right here.  We have nms random ints
		 * with 24 bits each.  We sort them.
		 */
		MYDEBUG(D_DIEHARD_BDAY){
			for (m = 0; m < nms; m++){
				printf("Before sort %u:  %u\n", m, rand_uint[m]);
			}
		}
		gsl_sort_uint(rand_uint, 1, nms);
		MYDEBUG(D_DIEHARD_BDAY){
			for (m = 0; m < nms; m++){
				printf("After sort %u:  %u\n", m, rand_uint[m]);
			}
		}

		/*
		 * We create the intervals between entries in the sorted
		 * list and sort THEM.
		 */
		intervals[0] = rand_uint[0];
		for (m = 1; m < nms; m++){
			intervals[m] = rand_uint[m] - rand_uint[m - 1];
		}
		gsl_sort_uint(intervals, 1, nms);
		MYDEBUG(D_DIEHARD_BDAY){
			for (m = 0; m < nms; m++){
				printf("Sorted Intervals %u:  %u\n", m, intervals[m]);
			}
		}

		/*
		 * We count the number of interval values that occur more than
		 * once in the list.  Presumably that means that even if an interval
		 * occurs 3 or 4 times, it counts only once!
		 *
		 * k is the interval count (Marsaglia calls it j).
		 */
		k = 0;
		for (m = 0; m < nms - 1; m++){
			mnext = m + 1;
			while (intervals[m] == intervals[mnext]){
				/* There is at least one repeat of this interval */
				if (mnext == m + 1){
					/* increment the count of repeated intervals */
					k++;
				}
				MYDEBUG(D_DIEHARD_BDAY){
					printf("repeated intervals[%u] = %u == intervals[%u] = %u\n",
						m, intervals[m], mnext, intervals[mnext]);
				}
				mnext++;
			}
			/*
			 * Skip all the rest that were identical.
			 */
			if (mnext != m + 1) m = mnext;
		}

		/*
		 * k now is the total number of intervals that occur more than once in
		 * this sample of nms=512 numbers.  We increment the sample counter in
		 * this slot.  If k is bigger than kmax, we simply ignore it -- it is a
		 * BAD IDEA to bundle all the points from the tail into the last bin,
		 * as a Poisson distribution can have a lot of points out in that tail!
		 */
		if (k < kmax) {
			js[k]++;
			MYDEBUG(D_DIEHARD_BDAY){
				printf("incremented js[%u] = %u\n", k, js[k]);
			}
		}
		else {
			MYDEBUG(D_DIEHARD_BDAY){
				printf("%u >= %u: skipping increment of js[%u]\n", k, kmax, k);
			}
		}

	}


	/*
	 * Let's sort the result (for fun) and print it out for each bit
	 * position.
	 */
	MYDEBUG(D_DIEHARD_BDAY){
		printf("#==================================================================\n");
		printf("# This is the repeated interval histogram:\n");
		for (k = 0; k < kmax; k++){
			printf("js[%u] = %u\n", k, js[k]);
		}
	}


	/*
	 * Fine fine fine.  We FINALLY have a distribution of the binned repeat
	 * interval counts of many samples of 512 numbers drawn from 2^24.  We
	 * should now be able to pass this vector of results off to a Pearson
	 * chisq computation for the expected Poissonian distribution and
	 * generate a p-value for each cyclic permutation of the bits through the
	 * 24 bit mask.
	 */
	test[0]->pvalues[irun] = chisq_poisson(js, lambda, kmax, test[0]->tsamples);
	MYDEBUG(D_DIEHARD_BDAY){
		printf("# diehard_birthdays(): test[0]->pvalues[%u] = %10.5f\n", irun, test[0]->pvalues[irun]);
	}

	nullfree(intervals);
	nullfree(js);

	return(0);

}

