/*
 * $Id: rgb_bitdist.c 225 2006-08-17 13:15:00Z rgb $
 *
 * See copyright in copyright.h and the accompanying file COPYING
 *
 */

#include "libdieharder.h"
#include "DieHarderDLL.h"

/*
 * A standard test returns a single-pass p-value as an end result.
 * The "size" of the single pass test is controlled by the tsamples
 * parameter, where possible (some tests use statistics evaluated only
 * for very particular values of tsamples.
 *
 * The test is, in general, executed psamples times and the p-values
 * for each run are in turn accumulated in a vector.  This can all be
 * done in a very standard loop.
 *
 * These values SHOULD be uniformly distributed from [0-1].  A
 * Kuiper Kolmogorov-Smirnov test is performed on the final distribution
 * of p to generate a p-value for the entire test series, containing
 * (typically) tsamples*psamples actual samples.
 *
 * Some tests generate more than one p-value per single pass.  Others
 * are designed to be iterated over yet another integer-indexed control
 * parameter.  To facilitate the consistent pass-back of test results to
 * a UI and allow test reuse without leaks, it is important to be able
 * to create "a standard test" and destroy/free it when done.  For this
 * reason the API for the library standard test function is quite
 * object-oriented in its implementation.
 *
 * It is strongly suggested that this object oriented design be reused
 * whenever possible when designing new tests.  This also permits the
 * maximal reuse of code in the UI or elsewhere.
 */

/* static uint save_psamples; */

/*
 * Create a new test that will return nkps p-values per single pass,
 * for psamples passes.  dtest is a pointer to a struct containing
 * the test description and default values for tsamples and psamples.
 * This should be called before a test is started in the UI.
 */
Test **create_test(Dtest *dtest, uint tsamples, uint psamples)
{

	uint i, j;
	uint pcutoff;
	Test **newtest;

	MYDEBUG(D_STD_TEST){
		fprintf(stdout, "# create_test(): About to create test %s\n", dtest->sname);
	}

	/*
	 * Here we have to create a vector of tests of length nkps
	 */
	/* printf("Allocating vector of pointers to Test structs of length %d\n",dtest->nkps); */
	newtest = (Test **)malloc((size_t)dtest->nkps*sizeof(Test *));
	for (i = 0; i < dtest->nkps; i++){
		/* printf("Allocating the actual test struct for the %d th test\n",i); */
		newtest[i] = (Test *)malloc(sizeof(Test));
	}

	/*
	 * Initialize the newtests.  The implementation of TTD (test to
	 * destruction) modes makes this inevitably a bit complex.  In
	 * particular, we have to malloc the GREATER of Xoff and psamples in
	 * newtest[i]->pvalues to make room for more pvalues right up to the
	 * Xoff cutoff.
	 */
	for (i = 0; i < dtest->nkps; i++){

		/*
		 * Do a standard test if -a(ll) is selected no matter what people enter
		 * for tsamples or psamples.  ALSO use standard values if tsamples or
		 * psamples are 0 (not initialized).  HOWEVER, note well the new control
		 * for psamples that permits one to scale the standard number of psamples
		 * in an -a(ll) run by multiply_p.
		 */
		if (all == YES || tsamples == 0){
			newtest[i]->tsamples = dtest->tsamples_std;
		}
		else {
			newtest[i]->tsamples = tsamples;
		}
		if (all == YES || psamples == 0){
			newtest[i]->psamples = dtest->psamples_std*multiply_p;
			if (newtest[i]->psamples < 1) newtest[i]->psamples = 1;
		}
		else {
			newtest[i]->psamples = psamples;
		}

		/* Give ntuple an initial value of zero; most tests will set it. */
		newtest[i]->ntuple = 0;

		/*
		 * Now we can malloc space for the pvalues vector, and a
		 * single (80-column) LINE for labels for the pvalues.  We default
		 * the label to a line of #'s.
		 */
		if (Xtrategy != 0 && Xoff > newtest[i]->psamples){
			pcutoff = Xoff;
		}
		else {
			pcutoff = newtest[i]->psamples;
		}
		newtest[i]->pvalues = (double *)malloc((size_t)pcutoff*sizeof(double));
		newtest[i]->pvlabel = (char *)malloc((size_t)LINE*sizeof(char));
		_snprintf(newtest[i]->pvlabel, LINE, "##################################################################\n");
		for (j = 0; j < pcutoff; j++){
			newtest[i]->pvalues[j] = 0.0;
		}

		/*
		 * Finally, we initialize ks_pvalue so that std_test() knows the next
		 * call is the first call.  It will be nonzero after the first call.
		 */
		newtest[i]->ks_pvalue = 0.0;

		MYDEBUG(D_STD_TEST){
			printf("Allocated and set newtest->tsamples = %d\n", newtest[i]->tsamples);
			printf("Xtrategy = %u -> pcutoff = %u\n", Xtrategy, pcutoff);
			printf("Allocated and set newtest->psamples = %d\n", newtest[i]->psamples);
		}

	}

	/* printf("Allocated complete test struct at %0x\n",newtest); */
	return(newtest);

}

/*
 * Destroy (free) a test created with create_test without leaking.
 * This should be called as soon as a test is finished in the UI.
 */
void destroy_test(Dtest *dtest, Test **test)
{

	int i;

	/*
	 * To destroy a test one has to first free its allocated contents
	 * or leak.
	 */
	/*
	printf("Destroying test %s\n",dtest->name);
	printf("Looping over %d test pvalue vectors\n",dtest->nkps);
	*/
	for (i = 0; i < dtest->nkps; i++){
		free(test[i]->pvalues);
		free(test[i]->pvlabel);
	}
	/* printf("Freeing all the test structs\n"); */
	for (i = 0; i < dtest->nkps; i++){
		free(test[i]);
	}
	/* printf("Freeing the toplevel test struct at %0x\n",test); */
	free(test);

}

/*
 * Clear a test.  This must be called if one wants to call std_test()
 * twice after creating it and have the second call just add samples to
 * the previous call.  std_test needs the vector of ks_psamples
 * accumulated so far to be clear, and for test[i]->psamples to be
 * reset to its original/default value.  I'm not sure that one cannot
 * screw this up by creating multiple tests and running interleaved
 * std_tests and clear_tests, but at least it makes it challenging to
 * do so.
 */
void clear_test(Dtest *dtest, Test **test)
{

	int i;

	/*
	 * reset psamples and clear the ks_pvalues
	 */
	for (i = 0; i < dtest->nkps; i++){
		if (all == YES || psamples == 0){
			test[i]->psamples = dtest->psamples_std*multiply_p;
		}
		else {
			test[i]->psamples = psamples;
		}
		test[i]->ks_pvalue = 0.0;
	}

	/*
	 * At this point you can call std_test() and it will start over, instead
	 * of just adding more samples to a run for this particular test.
	 */

}

/*
 * Test To Destruction (TTD) or Resolve Ambiguity (RA) modes require one
 * to iterate, adding psamples until:
 *
 *    TTD -- a test either fails or completes Xoff psamples without
 *           completely failing.
 *     RA -- a test that is initially "weak" (compared to Xweak) either
 *           fails or gets back up over 0.05 or completes Xoff psamples
 *           without completely failing.
 *
 * This routine just adds count (usually Xstep) psamples to a test.  It
 * is called by std_test() in two modes -- first call and TTD/RA (add more
 * samples) mode.  This is completely automagic, though.
 *
 * Note that Xoff MUST remain global, if nothing else.  Otherwise we
 * can run out of allocated headroom in the pvalues vector.
 */
void add_2_test(Dtest *dtest, Test **test, int count)
{

	uint i, j, imax;


	/*
	 * Will count carry us over Xoff?  If it will, stop at Xoff and
	 * adjust count to match.  test[0]->psamples is the running total
	 * of how many samples we have at the end of it all.
	 */
	imax = test[0]->psamples + count;
	if (imax > Xoff) imax = Xoff;
	count = imax - test[0]->psamples;
	for (i = test[0]->psamples; i < imax; i++){
		if (dtest->test == 0)
		{
			printf("nullTest gefunden");
		}
		printf("i = %i \n",i);
		dtest->test(test, i);	// test variable innerhalb dtest ist 0x000000 auf diesen Speicher darf nicht zugegriffen werden
		printf("%i sequenzes of bytes needed so far \n ", byteCount);
		if (reachedEOF)
		{
			return;
		}
	}
	
	for (j = 0; j < dtest->nkps; j++){
		/*
		 * Don't forget to count the new number of samples and use it in the
		 * new KS test.
		 */
		test[j]->psamples += count;

		if (ks_test >= 3){
			/*
			 * This (Kuiper KS) can be selected with -k 3 from the command line.
			 * Generally it is ignored.  All smaller values of ks_test are passed
			 * through to kstest() and control its precision (and speed!).
			 */
			test[j]->ks_pvalue = kstest_kuiper(test[j]->pvalues, test[j]->psamples);
		}
		else {
			/* This is (symmetrized Kolmogorov-Smirnov) is the default */
			test[j]->ks_pvalue = kstest(test[j]->pvalues, test[j]->psamples);
		}

	}
	/* printf("test[0]->ks_pvalue = %f\n",test[0]->ks_pvalue); */

}

/*
 * std_test() checks to see if this is the first call by looking at
 * all the ks_pvalues.  If they are zero, it assumes first call (in
 * general they will be clear only right after create_test or clear_test
 * have been called).  If it is first call, it calls add_2_test() in just
 * the right way to create test[0]->psamples, starting with 0.  If it is
 * the second or beyond call, it just adds Xstep more psamples to the
 * vector, up to the cutoff Xoff.
 */
void std_test(Dtest *dtest, Test **test)
{

	int j, count;
	double pmax = 0.0;

	/*
	 * First we see if this is the first call.  If it is, we save
	 * test[0]->psamples as count, then call add_2_test().  We determine
	 * first call by checking the vector of pvalues and seeing if they
	 * are all still zero (something that should pretty much never happen
	 * except on first call).
	 */
	for (j = 0; j < dtest->nkps; j++){
		if (test[j]->ks_pvalue > pmax) pmax = test[j]->ks_pvalue;
	}
	if (pmax == 0.0){
		/* First call */
		count = test[0]->psamples;
		for (j = 0; j < dtest->nkps; j++){
			test[j]->psamples = 0;
		}
	}
	else {
		/* Add Xstep more samples */
		count = Xstep;
	}

	add_2_test(dtest, test, count);

}

