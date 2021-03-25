/*
 * ========================================================================
 * $Id: diehard_opso.c 231 2006-08-22 16:18:05Z rgb $
 *
 * See copyright in copyright.h and the accompanying file COPYING
 * ========================================================================
 */

/*
 * ========================================================================
 *          DAB OPSO2 Test
 * This test is misnamed.  It is an evolution of the OPSO test from
 * the original Diehard program.  However, it does not use
 * overlapping samples.  Additionally, it returns two p-values,
 * the second of which follows the Pairs-Sparse-Occupancy part of
 * the name.  The first p-value effectively takes both letters from
 * the same input word.  However, that isn't any different from
 * having 1-letter words, where each letter is twice as long.
 *
 * This verion uses 2^24 slots.  The first p-value thus takes 24
 * bits directly from each input word.  The second p-value is based
 * on two 12-bit letters from each of two words; each pair of input
 * words will produce two output "words".
 *
 * This test will give a false positive for all generators with an
 * output word of less than 24 bits.
 *
 *         OPSO means Overlapping-Pairs-Sparse-Occupancy         ::
 * The OPSO test considers 2-letter words from an alphabet of    ::
 * 1024 letters.  Each letter is determined by a specified ten   ::
 * bits from a 32-bit integer in the sequence to be tested. OPSO ::
 * generates  2^21 (overlapping) 2-letter words  (from 2^21+1    ::
 * "keystrokes")  and counts the number of missing words---that  ::
 * is 2-letter words which do not appear in the entire sequence. ::
 * That count should be very close to normally distributed with  ::
 * mean 141,909, sigma 290. Thus (missingwrds-141909)/290 should ::
 * be a standard normal variable. The OPSO test takes 32 bits at ::
 * a time from the test file and uses a designated set of ten    ::
 * consecutive bits. It then restarts the file for the next de-  ::
 * signated 10 bits, and so on.                                  ::
 *
 *========================================================================
 */


#include "libdieharder.h"

int dab_opso2(Test **test, int irun) {
	uint i, j, k, t;
	unsigned int j0 = 0;
	unsigned int k0 = 0;
	Xtest ptest1, ptest2;
	unsigned int w1[524288], w2[524288];  /* 2^24 positions = 2^5 * 2^19; 2^19 = 524288 */
	unsigned int mask[32];      /* Masks to take the place of a bitset operation */

	for (i = 0; i < 32; i++) mask[i] = 1 << i;

	test[0]->ntuple = 0;
	test[1]->ntuple = 1;

	/* If the generator word size is too small, abort early. */
	if (rmax_bits < 24) {
		test[0]->pvalues[irun] = 0.5;
		test[1]->pvalues[irun] = 0.5;
		if (irun == 0) {
			printf("OPSO2: Requires rmax_bits to be >= 24\n");
		}
		return 0;
	}

	/* slots = 2^24, words = 2^26 */
	// New calculations from ossotarg.c
	// y = 307285.393182468
	// sigma = 528.341514924662
	ptest1.y = 307285.393182468;
	ptest1.sigma = 528.341514924662;
	ptest2.y = ptest1.y;
	ptest2.sigma = ptest1.sigma;
	test[0]->tsamples = 1 << 26;

	/* Zero the column */
	memset(w1, 0, sizeof(unsigned int)* 524288);
	memset(w2, 0, sizeof(unsigned int)* 524288);

	/* The main loop looks more complicated, because it is
	 * two tests in one loop.  w1 tests for patterns within
	 * each word.  w2 tests for pattens in pairs of words.
	 *
	 * Yes, that means that w2 is the OPSO, while w1 is
	 * something else.
	 *
	 * Generally, w2 is the stronger test.  However,
	 * generator 18 fails w1 much easier than w2.
	 */
	for (t = 0; t < test[0]->tsamples; t++) {  /* Start main loop */
		if (t % 2 == 0) {  // Get two inputs every other round
			j0 = gsl_rng_get(rng);
			k0 = gsl_rng_get(rng);

			w1[(j0 >> 5) % 524288] |= mask[j0 % 32];
			j = j0 & 0x0fff;
			k = (j << 12) | (k0 & 0x0fff);
		}
		else {
			w1[(k0 >> 5) % 524288] |= mask[k0 % 32];
			j = (j0 >> 12) & 0x0fff;
			k = (j << 12) | ((k0 >> 12) & 0x0fff);
		}

		w2[k >> 5] |= mask[k % 32];
	}

	/*
	 * Now we count the holes, so to speak
	 */
	j0 = 0;
	k0 = 0;
	ptest1.x = 0;
	ptest2.x = 0;
	for (i = 0; i < 32; i++) {
		for (j = 0; j < 524288; j++) {
			if ((w1[j] & mask[i]) == 0) j0++;
			if ((w2[j] & mask[i]) == 0) k0++;
		}
	}

	ptest1.x = j0;
	ptest2.x = k0;
	Xtest_eval(&ptest1);
	Xtest_eval(&ptest2);
	test[0]->pvalues[irun] = ptest1.pvalue;
	test[1]->pvalues[irun] = ptest2.pvalue;

	return(0);
}

