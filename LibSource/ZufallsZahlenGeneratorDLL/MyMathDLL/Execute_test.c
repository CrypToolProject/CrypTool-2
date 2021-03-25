#include "DieHarderDLL.h"
#include "libdieharder.h"

int execute_test(int dtest_num)
{

	int i;
	unsigned int need_more_p;
	double smallest_p;
	/*
	* Declare the results struct.
	*/
	Test **dieharder_test;

	/*
	* Here we have to look at strategy FIRST.  If strategy is not zero,
	* we have to reseed either randomly or from the value of nonzero Seed.
	*/
	if (strategy){
		if (Seed == 0){
			seed = random_seed();
			MYDEBUG(D_SEED){
				fprintf(stdout, "# execute_test(): Generating random seed %lu\n", seed);
			}
		}
		else {
			seed = Seed;
			MYDEBUG(D_SEED){
				fprintf(stdout, "# execute_test(): Setting fixed seed %lu\n", seed);
			}
		}
		gsl_rng_set(rng, seed);

	}

	/* printf("Test number %d: execute_test(%s) being run.\n",dtest_num,dh_test_types[dtest_num]->sname);*/

	/*
	* First we create the test (to set some values displayed in test header
	* correctly).
	*/
	dieharder_test = create_test(dh_test_types[dtest_num], tsamples, psamples);

	/*
	* We now have to implement Xtrategy.  Since std_test is now smart enough
	* to be able to differentiate a first call after creation or clear from
	* subsequent calls (where the latter adds Xstep more psamples) all we
	* need is a simple case switch on -Y Xtrategy to decide what to do.
	* Note well that we can reset Xstep here (hard code) or add additional
	* cases below quite easily to e.g. exponentially grow Xstep as we proceed.
	* If you do this, please preserve Xstep and put it back when you are done.
	*/
	/* Xstep = whatever; */
	need_more_p = YES;
	while (need_more_p){
		std_test(dh_test_types[dtest_num], dieharder_test);
		output(dh_test_types[dtest_num], dieharder_test);
		smallest_p = 0.5;
		for (i = 0; i < dh_test_types[dtest_num]->nkps; i++){
			if (0.5 - fabs(dieharder_test[i]->ks_pvalue - 0.5) < smallest_p) {
				smallest_p = 0.5 - fabs(dieharder_test[i]->ks_pvalue - 0.5);
			}
		}
		switch (Xtrategy){
			/*
			* This just runs std_test a single time, period, for good or ill.
			*/
		default:
		case 0:
			need_more_p = NO;
			break;
			/*
			*             Resolve Ambiguity (RA) mode
			*
			* If any test has a p that is less than Xfail, we are done.
			* If the entire test has pvalues that are bigger than Xweak,
			* we are done (we really need this to happen e.g. 3x consecutively
			* or exceed a much larger threshold, but that is more work to code
			* and I want to be certain of the algorithm first).  If the test
			* has accumulated Xoff psamples, we are done.
			*/
		case 1:
			if (smallest_p < Xfail) need_more_p = NO;
			if (smallest_p >= Xweak) need_more_p = NO;
			if (dieharder_test[0]->psamples >= Xoff) need_more_p = NO;
			break;
			/*
			*             Test To Destruction (TTD) mode
			*
			* If any test has a p that is less than Xfail, we are done.
			* If the test has accumulated Xoff psamples, we are done.
			*/
		case 2:
			if (smallest_p < Xfail) need_more_p = NO;
			if (dieharder_test[0]->psamples >= Xoff) need_more_p = NO;
			break;
		}
	}

	destroy_test(dh_test_types[dtest_num], dieharder_test);

	fclose(dataIN);

	return(0);

}