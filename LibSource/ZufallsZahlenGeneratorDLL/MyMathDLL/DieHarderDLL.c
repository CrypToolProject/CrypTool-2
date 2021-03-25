#include "DieHarderDLL.h"
#include "libdieharder.h"
#include <stdio.h>
#include <setjmp.h>

boolean run_tests()
{
	if (setjmp(env) == 0)
	{
		if (all)
		{
			run_all_tests();
			return 1;
		}
		else
		{
			run_test();
			return 1;
		}
	}
}

void list_rngs()
{

	int i, j;

	/*
	* This is run right away or not at all, so just announce that we're
	* listing rngs if verbose is set at all.
	*/
	if (verbose){
		printf("list_rngs():\n");
	}

	/*
	* These have to be called to set up the types[] vector with all
	* the available rngs so we can list them.  This SHOULD be the first
	* time this is called (and the only time) but it shouldn't leak memory
	* even if it is called more than once -- it uses a static global vector
	* in libdieharder's data space and is therefore persistent.
	*/
	dieharder_rng_types();
	//add_ui_rngs();


	/* Version string seems like a good idea */
	dh_header();
	printf("#   %3s %-20s|%3s %-20s|%3s %-20s#\n", " Id", "Test Name",
		" Id", "Test Name", " Id", "Test Name");
	printf("#=============================================================================#\n");
	i = 0;
	j = 0;
	while (dh_rng_types[i] != NULL){
		if (j % 3 == 0) printf("|   ");
		printf("%3.3d %-20s|", i, dh_rng_types[i]->name);
		if (((j + 1) % 3) == 0 && i > 0) printf("\n");
		i++;
		j++;
	}
	/*
	* Finish off each block neatly.  If j == 0, we finished the row
	* and do nothing.  Otherwise, pad to the end of the row.
	*/
	j = j % 3;
	if (j == 1) printf("                        |                        |\n");
	if (j == 2) printf("                        |\n");
	printf("#=============================================================================#\n");
	i = 200;
	j = 0;
	while (dh_rng_types[i] != NULL){
		if (j % 3 == 0) printf("|   ");
		printf("%3.3d %-20s|", i, dh_rng_types[i]->name);
		if (((j + 1) % 3) == 0 && i > 200) printf("\n");
		i++;
		j++;
	}
	j = j % 3;
	if (j == 1) printf("                        |                        |\n");
	if (j == 2) printf("                        |\n");
	printf("#=============================================================================#\n");
	i = 400;
	j = 0;
	while (dh_rng_types[i] != NULL){
		if (j % 3 == 0) printf("|   ");
		printf("%3d %-20s|", i, dh_rng_types[i]->name);
		if (((j + 1) % 3) == 0 && i > 400) printf("\n");
		i++;
		j++;
	}
	j = j % 3;
	if (j == 1) printf("                        |                        |\n");
	if (j == 2) printf("                        |\n");
	printf("#=============================================================================#\n");
	i = 500;
	j = 0;
	while (dh_rng_types[i] != NULL){
		if (j % 3 == 0) printf("|   ");
		printf("%3d %-20s|", i, dh_rng_types[i]->name);
		if (((j + 1) % 3) == 0 && i > 500) printf("\n");
		i++;
		j++;
	}
	j = j % 3;
	if (j == 1) printf("                        |                        |\n");
	if (j == 2) printf("                        |\n");
	printf("#=============================================================================#\n");
	if (dh_num_user_rngs){

		i = 600;
		j = 0;
		while (dh_rng_types[i] != NULL){
			if (j % 3 == 0) printf("|   ");
			printf("%3d %-20s|", i, dh_rng_types[i]->name);
			if (((j + 1) % 3) == 0 && i > 600) printf("\n");
			i++;
			j++;
		}
		j = j % 3;
		if (j == 1) printf("                        |                        |\n");
		if (j == 2) printf("                        |\n");
		printf("#=============================================================================#\n");

	}

}

void Exit(int exitcode) {

#if !defined(RDIEHARDER)
	exit(exitcode);
#endif  /* !defined(RDIEHARDER) */

	/* Add any e.g. free() statements below */

}

void output_rnds()
{
	unsigned int i, j;
	double d;
	FILE *fp;

	if (verbose) {
		fprintf(stderr, "# output_rnds: Dumping %lu rands\n", tsamples);
	}

	/*
	* If Seed is set, use it.  Otherwise reseed from /dev/random
	*/
	if (Seed){
		seed = Seed;
		if (verbose) {
			fprintf(stderr, "# output_rnds: seeding rng %s with %lu\n", gsl_rng_name(rng), seed);
		}
		gsl_rng_set(rng, seed);
	}
	else {
		seed = random_seed();
		if (verbose) {
			fprintf(stderr, "# output_rnds: seeding rng %s with %lu\n", gsl_rng_name(rng), seed);
		}
		gsl_rng_set(rng, seed);
	}

	/*
	* Open the output file.  If no filename is specified, or if
	* filename is "-", use stdout.
	*/
	if (verbose) {
		fprintf(stderr, "# output_rnds: Opening file %s\n", filename);
	}
	if ((filename[0] == 0) || (strncmp("-", filename, 1) == 0)){
		fp = stdout;
	}
	else {
		printf("Filename: %c \n", filename);
		if ((fp = fopen(filename, "w+")) == NULL) {
			fprintf(stderr, "Error: Cannot open %s, exiting.\n", filename);
			exit(0);
		}
	}

	if (verbose) {
		fprintf(stderr, "# output_rnds: Opened %s\n", filename);
	}
	/*
	* We completely change the way we control output.
	*
	*   -O output_format
	*      output_format = 0 (binary), 1 (uint), 2 (decimal)
	*
	* We just do a case switch, since each of them has its own
	* peculiarities.
	*/
	switch (output_format){
	case 0:
		if (verbose) {
			fprintf(stderr, "Ascii values of binary data being written into file %s:\n", filename);
		}
		/*
		* make the samples and output them.  If we run binary with tsamples
		* = 0, we just loop forever or until the program is interrupted by
		* hand.
		*/
		if (tsamples > 0){
			for (i = 0; i < tsamples; i++){
				j = gsl_rng_get(rng);
				fwrite(&j, sizeof(unsigned int), 1, fp);
				/*
				* Printing to stderr lets me read it and pass the binaries on through
				* to stdout and a pipe.
				*/
				if (verbose) {
					fprintf(stderr, "%10u\n", j);
				}
			}
		}
		else {
			/*
			* If tsamples = 0, just pump them into stdout.  One HOPES that this
			* blocks when out goes into a pipe -- but that's one of the
			* questions I need to resolve.  This will make an infinite number of
			* binary rands (until the pipe is broken and this instance of
			* dieharder dies).
			*/
			while (1){
				j = gsl_rng_get(rng);
				fwrite(&j, sizeof(unsigned int), 1, fp);
				/*
				* Printing to stderr lets me read it and pass the binaries on through
				* to stdout and a pipe.
				*/
				if (verbose) {
					fprintf(stderr, "%10u\n", j);
				}
			}
		}
		break;
	case 1:
		fprintf(fp, "#==================================================================\n");
		fprintf(fp, "# generator %s  seed = %lu\n", gsl_rng_name(rng), seed);
		fprintf(fp, "#==================================================================\n");
		fprintf(fp, "type: d\ncount: %lu\nnumbit: 32\n", tsamples);
		for (i = 0; i < tsamples; i++){
			j = gsl_rng_get(rng);
			fprintf(fp, "%10u\n", j);
		}
		break;
	case 2:
		fprintf(fp, "#==================================================================\n");
		fprintf(fp, "# generator %s  seed = %lu\n", gsl_rng_name(rng), seed);
		fprintf(fp, "#==================================================================\n");
		fprintf(fp, "type: f\ncount: %lu\nnumbit: 32\n", tsamples);
		for (i = 0; i < tsamples; i++){
			d = gsl_rng_uniform(rng);
			fprintf(fp, "%0.10f\n", d);
		}
		break;

	}

	fclose(fp);

}

void time_rng()
{

	/*
	* Declare the results struct.
	*/
	Rgb_Timing timing;
	Test **rgb_timing_test;

	/*
	* First we create the test (to set some values displayed in test header
	* correctly).
	*/
	rgb_timing_test = create_test(&rgb_timing_dtest, tsamples, psamples);

	/*
	* Call the actual test that fills in the results struct.
	*/
	rgb_timing(rgb_timing_test, &timing);

	/*
	* Save this for display in
	*/
	rng_avg_time_nsec = timing.avg_time_nsec;
	rng_rands_per_second = timing.rands_per_sec;

	destroy_test(&rgb_timing_dtest, rgb_timing_test);

}
