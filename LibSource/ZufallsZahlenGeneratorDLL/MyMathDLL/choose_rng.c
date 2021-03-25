#include "DieHarderDLL.h"
#include "libdieharder.h"

#pragma once

int select_rng(int gennum, char *genname, unsigned int initial_seed);

void choose_rng()
{

	/*
	* The way CLI dieharder seeding works is:  a) If Seed is set (e.g.
	* dieharder -S 1... ) then its value will be used to reseed the
	* selected rng at the BEGINNING of each test, or test series for tests
	* such as rgb_bitdist that work their way through a set of ntuples.  If
	* Seed is set for file-based random number generators, it forces a
	* rewind at the beginning of the tests (which should have no effect on
	* stdin-piped streams).
	*
	* Note that using the same seed for all the tests isn't that great from
	* a testing point of view; it is primarily useful for validation runs or
	* to conserve a limited supply of rands from a file by running different
	* tests on the one set of rands.
	*
	* b) If Seed is NOT set (is equal to 0) then the selected rng is seeded
	* one time using the reseed() function, which in turn uses e.g.
	* /dev/random if available or the clock if it is not to generate a
	* unique, moderately unpredictable seed.  The generator will NOT be
	* reseeded (or rewound) if more than one test is run on it, and distinct
	* test results or assessments can be thought of as being drawn as iid
	* samples.
	*
	* Note that results from different tests are likely to be CORRELATED if
	* the same seed is used for each test -- a test with a surplus of some
	* bit pattern -- say 000 -- will also have skewed distributions of all
	* the larger patterns that contain 000.
	*/

	/*
	* select_rng() returns -1 if it cannot find the desired generator
	* or encounters any other problem.  In the CLI, this means that
	* we must complain, list the available rngs, and exit.  In an
	* interactive UI, I imagine that one would get an error message
	* and a chance to try again.
	*
	* MUST FIX THIS for the new combo multigenerator.  All broken.  At least
	* this should force the output of generator names as usual, though.
	*/
	if (select_rng(generator, generator_name, Seed) < 0){
		list_rngs();
		Exit(0);
	}

	/*
	* This may or may not belong here.  We may move it somewhere else
	* if it needs to go.  But it should WORK here, at least for the moment.
	*/
	if (output_file){
		output_rnds();
		Exit(0);
	}


}


/*
* ========================================================================
* select_rng()
*
* This code can be executed by either the CLI
* dieharder or rdieharder to select a generator based on EITHER generator
* name (via a brute force search) OR generator number.  Number is more
* convenient for automation and obviously faster, name is perhaps easier
* to remember.  Note that it produces NO OUTPUT under normal operation
* and returns -1 if it cannot find the generator however it was entered;
* it is the responsibility of the UI to handle the error and warn the
* user.
*
* The way it works is it checks name FIRST and if not null, it uses the
* name it finds.  Otherwise it tries to use the number, where gennum=0
* actually corresponds to a generator.  It does very minimal bounds
* checking and will just return -1 if it falls through without finding
* a generator corresponding to its arguments.
* ========================================================================
*/

int select_rng(int gennum, char *genname, unsigned int initial_seed)
{

	int i;

	/*
	* REALLY out of bounds we can just test for and return an error.
	*/
	if ((int)gnumbs[0] < 0 || (int)gnumbs[0] >= MAXRNGS){
		return(-1);
	}

	/*
	* We are FINALLY ready, I think, to implement the super/vector generator
	* as soon as we get the talk all together and ready to go...
	*     (start here)
	* See if a gennum name has been set (genname not null).  If
	* so, loop through all the gennums in dh_rng_types looking for a
	* match and return a hit if there is one.  Note that this
	* routine just sets gennum and passes a (presumed valid)
	* gennum on for further processing, hence it has to be first.
	*/
	if (gnames[0][0] != 0){
		gennum = -1;
		for (i = 0; i < 1000; i++){
			if (dh_rng_types[i]){
				if (strncmp(dh_rng_types[i]->name, gnames[0], 20) == 0){
					gennum = i;
					break;
				}
			}
		}
		if (gennum == -1) return(-1);
	}
	else if (dh_rng_types[gnumbs[0]] != 0){
		/*
		* If we get here, then we are entering a gennum type by number.
		* We check to be sure there is a gennum with the given
		* number that CAN be used and return an error if there isn't.
		*/
		gennum = gnumbs[0];
		if (dh_rng_types[gennum]->name[0] == 0){
			/*
			* No generator with this name.
			*/
			return(-1);
		}
	}
	else {
		/*
		* Couldn't find a generator at all.  Should really never get here.
		*/
		return(-1);
	}

	/*
	* We need a sanity check for file input.  File input is permitted
	* iff we have a file name, if the output flag is not set, AND if
	* gennum is either file_input or file_input_raw.  Otherwise
	* IF gennum is a file input type (primary condition) we punt.
	*
	* For the moment we actually return an error message, but we may
	* want to pass the message back in a shared error buffer that
	* rdh can pick up or ignore, ditto dh, on a -1 return.
	*/
	if (strncmp("file_input", dh_rng_types[gennum]->name, 10) == 0){
		if (fromfile != 1){
			fprintf(stderr, "Error: gennum %s uses file input but no filename has been specified", dh_rng_types[gennum]->name);
			return(-1);
		}
	}

	/*
	* If we get here, in principle the gennum is valid and the right
	* inputs are defined to run it (in the case of file_input). We therefore
	* allocate the selected rng.  However, we FIRST check to see if rng is
	* not 0, and if it isn't 0 we assume that we're in a UI, that the user
	* has just changed rngs, and that we need to free the previous rng and
	* reset the bits buffers so they are empty.
	*/
	if (rng){
		MYDEBUG(D_SEED){
			fprintf(stdout, "# choose_rng(): freeing old gennum %s\n", gsl_rng_name(rng));
		}
		gsl_rng_free(rng);
		reset_bit_buffers();
	}

	/*
	* We should now be "certain" that it is safe to allocate a new gennum
	* without leaking memory.
	*/
	MYDEBUG(D_SEED){
		fprintf(stdout, "# choose_rng(): Creating and seeding gennum %s\n", dh_rng_types[gennum]->name);
	}
	rng = gsl_rng_alloc(dh_rng_types[gennum]);

	/*
	* Here we evaluate the speed of the generator if the rate flag is set.
	*/
	if (tflag & TRATE){
		time_rng();
	}

	/*
	* OK, here's the deal on seeds.  If strategy = 0, we set the seed
	* ONE TIME right HERE to either a randomly selected seed or whatever
	* has been entered for Seed, if nonzero.
	*
	* If strategy is not 0 (1 is fine) then no matter what we do below,
	* we will RESET the seed to either a NEW random seed or the value of
	* Seed at the beginning of each test.
	*
	* The default behavior for actual testing should obviously be
	* -s(trategy)=0 or 1 and -S(eed)=0, a random seed from /dev/random
	* used for the whole test or reseeded per test (with a new seed each
	* time).  -S(eed)=anything else can be used to fix a seed/strategy to
	* match a previous run to reproduce it, or it can be used to set a seed
	* to be used for each test individually, probably for a validation (of
	* the test, not the rng) run.  DO NOT USE THE LATTER FOR TESTING!  The
	* pvalues generated by test SERIES that e.g test ntuples over a range
	* are obviously not independent if they all are started with the same
	* seed (and hence test largely the same series).
	*
	* Regardless, the CURRENT seed is stored in the global seed variable,
	* which we may need to move from libdieharder to the UI as I don't think
	* we'll need to share a seed variable with any test.
	*
	* Note that for file input (not stdin) "reseed per test" is interpreted
	* as "rewind per test".  Otherwise dieharder will rewind the file (and
	* complain) only if one hits EOF before all testing is done, and usually
	* that means that at least the test where the rewind occurs is useless.
	*/

	/*
	* We really don't need to worry about the value of strategy here, just
	* Seed.  If it is is 0 we reseed randomly, otherwise we PROceed.  We
	* actually seed from the variable seed, not Seed (which then remembers
	* the value as long as it remains valid).
	*/
	if (Seed == 0){
		seed = random_seed();
		MYDEBUG(D_SEED){
			fprintf(stdout, "# choose_rng(): Generating random seed %lu\n", seed);
		}
	}
	else {
		seed = Seed;
		MYDEBUG(D_SEED){
			fprintf(stdout, "# choose_rng(): Setting fixed seed %lu\n", seed);
		}
	}

	/*
	* Set the seed.  It may or may not ever be reset.
	*/
	gsl_rng_set(rng, seed);

	/*
	* Before we quit, we must count the number of significant bits in the
	* selected rng AND create a mask.  Note that several routines in bits
	* WILL NOT WORK unless this is all set correctly, which actually
	* complicates things because I stupidly didn't make this data into
	* components of the rng object (difficult to do given that the latter is
	* actually already defined in the GSL, admittedly).
	*/
	random_max = gsl_rng_max(rng);
	rmax = random_max;
	rmax_bits = 0;
	rmax_mask = 0;
	while (rmax){
		rmax >>= 1;
		rmax_mask = rmax_mask << 1;
		rmax_mask++;
		rmax_bits++;
	}

	/*
	* If we get here, we are all happy, and return false (no error).
	*/
	return(0);

}


/*
* ========================================================================
* select_XOR()
*
* It is our profound wish to leave the code above mostly unmolested as we
* add the "special" XOR supergenerator.  We therefore make this a
* separate routine altogether and add a call to this on a simple
* conditional up above.
* ========================================================================
*/

int select_XOR()
{

	int i, j;
	int one_file;

	/*
	* See if a gennum name has been set (genname not null).  If
	* so, loop through all the gennums in dh_rng_types looking for a
	* match and return a hit if there is one.  Note that this
	* routine just sets gennum and passes a (presumed valid)
	* gennum on for further processing, hence it has to be first.
	*/
	for (j = 0; j < gvcount; j++){
		if (gnames[j][0] != 0){
			gnumbs[j] = -1;
			for (i = 0; i < 1000; i++){
				if (dh_rng_types[i]){
					if (strncmp(dh_rng_types[i]->name, gnames[j], 20) == 0){
						gnumbs[j] = i;
						break;
					}
				}
			}
			if (gnumbs[j] == -1) return(-1);
		}

	}

	/*
	* If we get here, then gnumbs[j] contains only numbers.  We check to be
	* sure all the values are valid and the number CAN be used and
	* return an error if any of them can't.
	*/
	one_file = 0;
	for (j = 0; j < gvcount; j++){

		if (dh_rng_types[gnumbs[j]] == 0){
			return(-1);
		}

		/*
		* We need a sanity check for file input.  File input is permitted
		* iff we have a file name AND if gnumbs[j] is either file_input or
		* file_input_raw.
		*/
		if (strncmp("file_input", dh_rng_types[gnumbs[j]]->name, 10) == 0){
			one_file++;
			if (fromfile != 1 || one_file > 1){
				fprintf(stderr, "Error: generator %s uses file input but no filename has been specified", dh_rng_types[gnumbs[j]]->name);
				return(-1);
			}
		}

	}

	/*
	* If we get here, in principle the gnumbs vector is filled with valid
	* rngs and the right inputs are defined to run them (in the case of a
	* SINGLE file_input). We therefore allocate the selected rng.  However,
	* we FIRST check to see if rng is not 0, and if it isn't 0 we assume
	* that we're in a UI, that the user has just changed rngs, and that we
	* need to free the previous rng and reset the bits buffers so they are
	* empty.
	*/
	if (rng){
		MYDEBUG(D_SEED){
			fprintf(stdout, "# choose_rng(): freeing old gennum %s\n", gsl_rng_name(rng));
		}
		gsl_rng_free(rng);
		reset_bit_buffers();
	}

	/*
	* We should now be "certain" that it is safe to allocate the XOR rng
	* without leaking memory and with a list of legitimate rngs.
	*/
	MYDEBUG(D_SEED){
	}
	for (j = 0; j < gvcount; j++){
		fprintf(stdout, "# choose_XOR(): generator[%i] = %s\n", j, dh_rng_types[gnumbs[j]]->name);
	}
	/*
	* Change 14 to the actual number
	*/
	rng = gsl_rng_alloc(dh_rng_types[14]);

	/*
	* Here we evaluate the speed of the generator if the rate flag is set.
	*/
	if (tflag & TRATE){
		time_rng();
	}

	/*
	* OK, here's the deal on seeds.  If strategy = 0, we set the seed
	* ONE TIME right HERE to either a randomly selected seed or whatever
	* has been entered for Seed, if nonzero.
	*
	* If strategy is not 0 (1 is fine) then no matter what we do below,
	* we will RESET the seed to either a NEW random seed or the value of
	* Seed at the beginning of each test.
	*
	* The default behavior for actual testing should obviously be
	* -s(trategy)=0 or 1 and -S(eed)=0, a random seed from /dev/random
	* used for the whole test or reseeded per test (with a new seed each
	* time).  -S(eed)=anything else can be used to fix a seed/strategy to
	* match a previous run to reproduce it, or it can be used to set a seed
	* to be used for each test individually, probably for a validation (of
	* the test, not the rng) run.  DO NOT USE THE LATTER FOR TESTING!  The
	* pvalues generated by test SERIES that e.g test ntuples over a range
	* are obviously not independent if they all are started with the same
	* seed (and hence test largely the same series).
	*
	* Regardless, the CURRENT seed is stored in the global seed variable,
	* which we may need to move from libdieharder to the UI as I don't think
	* we'll need to share a seed variable with any test.
	*
	* Note that for file input (not stdin) "reseed per test" is interpreted
	* as "rewind per test".  Otherwise dieharder will rewind the file (and
	* complain) only if one hits EOF before all testing is done, and usually
	* that means that at least the test where the rewind occurs is useless.
	*/

	/*
	* We really don't need to worry about the value of strategy here, just
	* Seed.  If it is is 0 we reseed randomly, otherwise we PROceed.  We
	* actually seed from the variable seed, not Seed (which then remembers
	* the value as long as it remains valid).
	*/
	if (Seed == 0){
		seed = random_seed();
		MYDEBUG(D_SEED){
			fprintf(stdout, "# choose_rng(): Generating random seed %lu\n", seed);
		}
	}
	else {
		seed = Seed;
		MYDEBUG(D_SEED){
			fprintf(stdout, "# choose_rng(): Setting fixed seed %lu\n", seed);
		}
	}

	/*
	* Set the seed.  It may or may not ever be reset.
	*/
	gsl_rng_set(rng, seed);

	/*
	* We don't really need this anymore, I don't think.  But we'll leave it
	* for now.
	*/
	random_max = gsl_rng_max(rng);
	rmax = random_max;
	rmax_bits = 0;
	rmax_mask = 0;
	while (rmax){
		rmax >>= 1;
		rmax_mask = rmax_mask << 1;
		rmax_mask++;
		rmax_bits++;
	}

	/*
	* If we get here, we are all happy, and return false (no error).
	*/
	return(0);

}