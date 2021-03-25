#include "DieHarderDLL.h"
#include "libdieharder.h"

void run_all_tests()
{
	/*
	* The nt variables control ntuple loops for the -a(ll) display only.
	*/
	int ntmin, ntmax, ntsave;
	FILE *datei;
	datei = fopen("testNums.txt", "w");

	if (datei == NULL)
	{
		printf("Fehler beim oeffnen der Datei.");
		return 1;
	}

	/*
	* This isn't QUITE a simple loop because -a is a dieharder-only function,
	* so that all running of ntuples etc has to be mediated here, per very
	* specific test.  Only certain tests are run over an ntuple range.
	*/

	/*
	* No special ntuple tests in diehard
	*/
	for (dtest_num = 0; dtest_num < dh_num_diehard_tests; dtest_num++){
		if (dh_test_types[dtest_num]){
			fprintf(datei, "\n dtest_num: %i \n %s \n ", dtest_num, dh_test_types[dtest_num]->name);
			execute_test(dtest_num);
		}
	}

	/*
	* No special ntuple tests in sts (yet)
	*/
	for (dtest_num = 100; dtest_num < 100 + dh_num_sts_tests; dtest_num++){
		if (dh_test_types[dtest_num]){
			fprintf(datei, "\n dtest_num: %i \n %s \n ", dtest_num, dh_test_types[dtest_num]->name);
			execute_test(dtest_num);
		}
	}

	/*
	* Here we have to mess with ntuples for various rgb -- I mean "other"
	* tests. Sorry!  We do this in a case switch.  Note that we could just
	* loop over all tests from 0-899 with the same case switch, but since
	* we take the trouble to count the three categories of test, might as
	* well use them.
	*/
	for (dtest_num = 200; dtest_num < 200 + dh_num_other_tests; dtest_num++){

		switch (dtest_num){

			/*
			* Test 200 is rgb_bitdist, and needs an ntuple set/loop.
			*/
		case 200:

			if (dh_test_types[dtest_num]){
				fprintf(datei, "\n dtest_num: %i \n %s \n ", dtest_num, dh_test_types[dtest_num]->name);
				if (ntuple){
					/*
					* If ntuple is set to be nonzero, just use that value in "all".
					* We might need to check to be sure it is "doable", but probably
					* not...
					*/
					execute_test(dtest_num);
				}
				else {
					/*
					* Default is to test 1 through 8 bits, which takes a while on my
					* (quite fast) laptop but is a VERY thorough test of randomness
					* out to byte level.
					*/
					ntmin = 1;
					ntmax = 12;
					/* ntmax = 8; */
					/* printf("Setting ntmin = %d ntmax = %d\n",ntmin,ntmax); */
					for (ntuple = ntmin; ntuple <= ntmax; ntuple++){
						execute_test(dtest_num);
					}
					/*
					* This RESTORES ntuple = 0, which is the only way we could have
					* gotten here in the first place!
					*/
					ntuple = 0;

				}
			}
			break;

			/*
			* Test 201 is rgb_minimum_distance, and needs an ntuple set/loop.
			*/
		case 201:

			if (dh_test_types[dtest_num]){
				fprintf(datei, "\n dtest_num: %i \n %s \n ", dtest_num, dh_test_types[dtest_num]->name);
				if (ntuple){
					/*
					* If ntuple is set to be nonzero, just use that value in "all",
					* but only if it is in bounds.
					*/
					if (ntuple < 2 || ntuple > 5){
						ntsave = ntuple;
						ntuple = 5;  /* This is the hardest test anyway */
						execute_test(dtest_num);
						ntuple = ntsave;
					}
					else {
						execute_test(dtest_num);
					}
				}
				else {
					fprintf(datei, "\n dtest_num: %i \n %s \n ", dtest_num, dh_test_types[dtest_num]->name);
					/*
					* Default is to 2 through 5 dimensions, all that are supported by
					* the test.
					*/
					ntmin = 2;
					ntmax = 5;
					/* printf("Setting ntmin = %d ntmax = %d\n",ntmin,ntmax); */
					for (ntuple = ntmin; ntuple <= ntmax; ntuple++){
						execute_test(dtest_num);
					}
					/*
					* This RESTORES ntuple = 0, which is the only way we could have
					* gotten here in the first place!
					*/

					ntuple = 0;

				}
			}
			break;

			/*
			* Test 202 is rgb_permutations, and needs an ntuple set/loop.
			*/
		case 202:

			if (dh_test_types[dtest_num]){
				fprintf(datei, "\n dtest_num: %i \n %s \n ", dtest_num, dh_test_types[dtest_num]->name);
				if (ntuple){
					/*
					* If ntuple is set to be nonzero, just use that value in "all",
					* but only if it is in bounds.
					*/
					if (ntuple < 2){
						ntsave = ntuple;
						ntuple = 5;  /* This is the default operm5 value */
						execute_test(dtest_num);
						ntuple = ntsave;
					}
					else {
						execute_test(dtest_num);
					}
				}
				else {
					fprintf(datei, "\n dtest_num: %i \n %s \n ", dtest_num, dh_test_types[dtest_num]->name);
					/*
					* Default is to 2 through 5 permutations.  Longer than 5 takes
					* a LONG TIME and must be done by hand.
					*/
					ntmin = 2;
					ntmax = 5;
					/* printf("Setting ntmin = %d ntmax = %d\n",ntmin,ntmax); */
					for (ntuple = ntmin; ntuple <= ntmax; ntuple++){
						execute_test(dtest_num);
					}
					/*
					* This RESTORES ntuple = 0, which is the only way we could have
					* gotten here in the first place!
					*/

					ntuple = 0;

				}
			}
			break;

			/*
			* Test 203 is rgb_lagged_sums, and needs an ntuple set/loop.
			*/
		case 203:

			if (dh_test_types[dtest_num]){
				fprintf(datei, "\n dtest_num: %i \n %s \n ", dtest_num, dh_test_types[dtest_num]->name);
				if (ntuple){
					/*
					* If ntuple is set to be nonzero, just use that value in "all".
					*/
					execute_test(dtest_num);
				}
				else {
					/*
					* Do all lags from 0 to 32.
					*/
					ntmin = 0;
					ntmax = 32;
					/* printf("Setting ntmin = %d ntmax = %d\n",ntmin,ntmax); */
					for (ntuple = ntmin; ntuple <= ntmax; ntuple++){
						execute_test(dtest_num);
					}
					/*
					* This RESTORES ntuple = 0, which is the only way we could have
					* gotten here in the first place!
					*/

					ntuple = 0;

				}
			}
			break;

			/*
			* Test 204 is rgb_kstest_test.
			*/
		case 204:

			if (dh_test_types[dtest_num]){
				fprintf(datei, "\n dtest_num: %i \n %s \n ", dtest_num, dh_test_types[dtest_num]->name);
				execute_test(dtest_num);
			}
			break;

			/*
			* Test 205 is dab_bytedistrib.
			*/
		case 205:

			if (dh_test_types[dtest_num]){
				fprintf(datei, "\n dtest_num: %i \n %s \n ", dtest_num, dh_test_types[dtest_num]->name);
				execute_test(dtest_num);
			}
			break;

			/*
			* Test 206 is dab_dct.
			*/
		case 206:

			if (dh_test_types[dtest_num]){
				fprintf(datei, "\n dtest_num: %i \n %s \n ", dtest_num, dh_test_types[dtest_num]->name);
				execute_test(dtest_num);
			}
			break;

		default:
			printf("Preparing to run test %d.  ntuple = %d\n", dtest_num, ntuple);
			if (dh_test_types[dtest_num]){   /* This is the fallback to normal tests */
				fprintf(datei, "\n dtest_num: %i \n %s \n ", dtest_num, dh_test_types[dtest_num]->name);
				execute_test(dtest_num);
			}
			break;

		}
		

	}
	fclose(datei);
	/*
	* Future expansion in -a tests...
	for(dtest_num=600;dtest_num<600+dh_num_user_tests;dtest_num++){
	if(dh_test_types[dtest_num]){
	execute_test(dtest_num);
	}
	}
	*/

}
