#include "libdieharder.h"
#include "DieHarderDLL.h"

#if defined(RDIEHARDER)
# include <R_ext/Memory.h>
#endif

void rdh_get_pvalues(Dtest *dtest, Test **test);
void output_rng_info();
void output_table_line_header();
void output_table_line(Dtest *dtest, Test **test);
int output_histogram(double *input, char *pvlabel, int inum, double min, double max, int nbins, char *label);

void output(Dtest *dtest, Test **test)
{

	/*
	* If this is rdieharder, skip all stdout or stderr entirely, period.
	* Run only Dirk's code to grab the pvalues and load them into
	* rdieharder memory to go back to R.  Otherwise generate output in
	* whatever general form the user desires, either a table with user
	* control over fields via a binary flag variable or a "verbose
	* report" which shows the test description, the output pvalue histogram,
	* and sundry gingerbready stuff as well.  Note that I should probably
	* simplify this still further and just have the table output line but
	* use flag bits to turn on and off even these elements of the output.
	*/
#if defined(RDIEHARDER)

	rdh_get_pvalues(dtest, test);

#else

	/*
	* Show the table header at most one time.
	*/
	static unsigned int firstcall = 1;
	if (firstcall){

		/*
		* Output dieharder copyright/version information
		*/

		if (tflag & THEADER){
			/*
			* This is actually a libdieharder call now!
			*/
			dh_header();
		}

		/*
		* Next we output information about the random number generator
		* being tested, according to its flags.  This is just one time,
		* on the first call.
		*/

		if (tflag & TSHOW_RNG){
			output_rng_info();
		}

		/*
		* The last thing we output "just one time" is the line header,
		* which has to match the actual selected output fields in the
		* table.  Note that we move the header to follow the histogram
		* if histogram is on, as otherwise everything looks funny.
		*/
		if ((tflag & TLINE_HEADER) && !(tflag & THISTOGRAM)){
			output_table_line_header();
		}

		firstcall = 0;

	}

	/*
	* This almost certainly belongs in the show_test_results section,
	* possibly with additional conditionals rejecting test results involving
	* rewinds, period.
	*/
	if (strncmp("file_input", gsl_rng_name(rng), 10) == 0){
		/*
		* This needs its own output flag and field.  I'm losing it for now.
		if(!quiet){
		fprintf(stdout,"# %u rands were used in this test\n",file_input_get_rtot(rng));
		fflush(stdout);
		}
		*/
		if (file_input_get_rewind_cnt(rng) != 0){
			fprintf(stderr, "# The file %s was rewound %u times\n", gsl_rng_name(rng), file_input_get_rewind_cnt(rng));
			fflush(stderr);
		}
	}

	/*
	* Everything below is the output PER TEST.  It cannot be skipped,
	* although I suppose it can be empty if no non-header output flags are
	* turned on.
	*/
	output_table_line(dtest, test);

#endif

}

/*
* This is ALMOST the single point of contact between rdh and dh.
* dh/shared routines do most of the initialization and setup the
* same between the two, a test is called the same way by the same
* code, but rdh gets the FINAL pvalues and processes them further
* within R; dh has to "report" them to e.g. stdout.  On a normal,
* error-free run the shared dh code should generate no output
* whatsoever to keep rdh happy.
*/
#if defined(RDIEHARDER)

void rdh_get_pvalues(Dtest *dtest, Test **test)
{

	int i;

	if (rdh_dtestptr == NULL) {
		rdh_dtestptr = dtest;
		/* we use R_alloc as R will free this upon return; see R Extensions manual */
		rdh_testptr = (Test **)R_alloc((size_t)dtest->nkps, sizeof(Test *));
		for (i = 0; i<dtest->nkps; i++) {
			rdh_testptr[i] = (Test *)R_alloc(1, sizeof(Test));
			memcpy(rdh_testptr[i], test[i], sizeof(Test));
		}
	}

}

#endif

/*
* This is just dieharder version/copyright information, #-delimited,
* controlled by the THEADER bit in tflag.
*/
void output_rng_info()
{

	if (tflag & TLINE_HEADER){
		if (tflag & TPREFIX){
			fprintf(stdout, "0%c", table_separator);
		}
		if (tflag & TNO_WHITE){
			fprintf(stdout, "%s%c", "rng_name", table_separator);
		}
		else {
			fprintf(stdout, "%15s%c", "rng_name    ", table_separator);
		}
		if (tflag & TNUM){
			fprintf(stdout, "%3s%c", "num", table_separator);
		}
		if (fromfile){
			if (tflag & TNO_WHITE){
				fprintf(stdout, "%s%c", "filename", table_separator);
			}
			else {
				fprintf(stdout, "%32s%c", "filename             ", table_separator);
			}
		}
		if (tflag & TRATE){
			fprintf(stdout, "%12s%c", "rands/second", table_separator);
		}
		if (tflag & TSEED && strategy == 0 && !fromfile){
			if (tflag & TNO_WHITE){
				fprintf(stdout, "%s%c", "Seed", table_separator);
			}
			else {
				fprintf(stdout, "%10s%c", "Seed   ", table_separator);
			}
		}
		fprintf(stdout, "\n");
	}

	if (tflag & TPREFIX){
		fprintf(stdout, "1%c", table_separator);
	}
	if (tflag & TNO_WHITE){
		fprintf(stdout, "%s%c", gsl_rng_name(rng), table_separator);
	}
	else {
		fprintf(stdout, "%15s%c", gsl_rng_name(rng), table_separator);
	}
	if (tflag & TNUM){
		if (tflag & TNO_WHITE){
			fprintf(stdout, "%d%c", generator, table_separator);
		}
		else {
			fprintf(stdout, "%3d%c", generator, table_separator);
		}
	}
	if (fromfile){
		if (tflag & TNO_WHITE){
			fprintf(stdout, "%s%c", filename, table_separator);
		}
		else {
			fprintf(stdout, "%32s%c", filename, table_separator);
		}
	}
	if (tflag & TRATE){
		if (tflag & TNO_WHITE){
			fprintf(stdout, "%.2e%c", rng_rands_per_second, table_separator);
		}
		else {
			fprintf(stdout, "%10.2e  %c", rng_rands_per_second, table_separator);
		}
	}
	if (tflag & TSEED && strategy == 0 && !fromfile){
		if (tflag & TNO_WHITE){
			fprintf(stdout, "%lu%c", seed, table_separator);
		}
		else {
			fprintf(stdout, "%10lu%c", seed, table_separator);
		}
	}
	fprintf(stdout, "\n");
	fflush(stdout);

}

void output_table_line_header()
{

	unsigned int field = 0;

	/*
	* We assemble the table header according to what tflag's value is.
	* If header is turned on, we also insert some pretty-printing
	* line separators.
	*/
	if (tflag & THEADER){
		fprintf(stdout, "#=============================================================================#\n");
	}

	if (tflag & TPREFIX){
		fprintf(stdout, "0");
		field++;
	}

	if (tflag & TTEST_NAME){
		if (field){
			fprintf(stdout, "%c", table_separator);
		}
		if (tflag & TNO_WHITE){
			fprintf(stdout, "%s", "test_name");
		}
		else {
			fprintf(stdout, "%20s", "test_name   ");
		}
		field++;
	}

	if (tflag & TNUM){
		if (field){
			fprintf(stdout, "%c", table_separator);
		}
		if (tflag & TNO_WHITE){
			fprintf(stdout, "%s", "num");
		}
		else {
			fprintf(stdout, "%s", "num");
		}
		field++;
	}

	if (tflag & TNTUPLE){
		if (field){
			fprintf(stdout, "%c", table_separator);
		}
		fprintf(stdout, "%4s", "ntup");
		field++;
	}

	if (tflag & TTSAMPLES){
		if (field){
			fprintf(stdout, "%c", table_separator);
		}
		if (tflag & TNO_WHITE){
			fprintf(stdout, "%s", "tsamples");
		}
		else {
			fprintf(stdout, "%10s", " tsamples ");
		}
		field++;
	}
	if (tflag & TPSAMPLES){
		if (field){
			fprintf(stdout, "%c", table_separator);
		}
		fprintf(stdout, "%8s", "psamples");
		field++;
	}

	if (tflag & TPVALUES){
		if (field){
			fprintf(stdout, "%c", table_separator);
		}
		if (tflag & TNO_WHITE){
			fprintf(stdout, "%s", "p-value");
		}
		else {
			fprintf(stdout, "%10s", "p-value ");
		}
		field++;
	}

	if (tflag & TASSESSMENT){
		if (field){
			fprintf(stdout, "%c", table_separator);
		}
		if (tflag & TNO_WHITE){
			fprintf(stdout, "%s", "Assessment");
		}
		else {
			fprintf(stdout, "%10s", "Assessment");
		}
		field++;
	}

	if (tflag & TSEED && strategy){
		if (field){
			fprintf(stdout, "%c", table_separator);
		}
		if (tflag & TNO_WHITE){
			fprintf(stdout, "%s", "Seed");
		}
		else {
			fprintf(stdout, "%10s", "Seed  ");
		}
		field++;
	}

	fprintf(stdout, "\n");
	if (tflag & THEADER){
		fprintf(stdout, "#=============================================================================#\n");
	}
	fflush(stdout);

}


/*
* Print out all per-test results in a table format where users
* can select table columns (fields) and can also select whether or
* not to output one-per-test stuff or one-per-pvalue stuff like
* the test description or pvalue histogram.
*/
void output_table_line(Dtest *dtest, Test **test)
{

	unsigned int i;
	unsigned int field;

	/*
	* IF a user wants something like the old-style "report", they
	* can toggle on the per-test description and the per pvalue
	* pvalue histogram below.
	*/
	if (tflag & TDESCRIPTION){
		fprintf(stdout, "%s", dtest->description);
	}

	/*
	* There may be more than one statistic (final p-value) generated by
	* this test; we loop over all of them.
	*/
	for (i = 0; i < dtest->nkps; i++){
		/*
		* Don't put a separator in slot for the first field, period.
		*/
		field = 0;

		/*
		* If a user wants a per-test histogram, here it is.  Note that it
		* will probably be difficult to parse, but that won't matter as it
		* can trivially be turned off.
		*/
		if (tflag & THISTOGRAM){
			output_histogram(test[i]->pvalues, test[i]->pvlabel, test[i]->psamples, 0.0, 1.0, 10, "p-values");
			if (tflag & TLINE_HEADER){
				output_table_line_header();
			}
		}

		/*
		* This must be first if it is turned on.
		*/
		if (tflag & TPREFIX){
			fprintf(stdout, "2");
			field++;
		}

		if (tflag & TTEST_NAME){
			if (field != 0){
				fprintf(stdout, "%c", table_separator);
			}
			if (tflag & TNO_WHITE){
				fprintf(stdout, "%s", dtest->sname);
			}
			else {
				fprintf(stdout, "%20.20s", dtest->sname);
			}
			field++;
		}

		if (tflag & TNUM){
			if (field != 0){
				fprintf(stdout, "%c", table_separator);
			}
			if (tflag & TNO_WHITE){
				fprintf(stdout, "%d", dtest_num);
			}
			else {
				fprintf(stdout, "%3d", dtest_num);
			}
			field++;
		}

		if (tflag & TNTUPLE){
			if (field != 0){
				fprintf(stdout, "%c", table_separator);
			}
			if (tflag & TNO_WHITE){
				fprintf(stdout, "%d", test[i]->ntuple);
			}
			else {
				fprintf(stdout, "%4d", test[i]->ntuple);
			}
			field++;
		}

		if (tflag & TTSAMPLES){
			if (field != 0){
				fprintf(stdout, "%c", table_separator);
			}
			if (tflag & TNO_WHITE){
				fprintf(stdout, "%u", test[0]->tsamples);
			}
			else {
				fprintf(stdout, "%10u", test[0]->tsamples);
			}
			field++;
		}

		if (tflag & TPSAMPLES){
			if (field != 0){
				fprintf(stdout, "%c", table_separator);
			}
			if (tflag & TNO_WHITE){
				fprintf(stdout, "%u", test[0]->psamples);
			}
			else {
				fprintf(stdout, "%8u", test[0]->psamples);
			}
			field++;
		}

		if (tflag & TPVALUES){
			if (field != 0){
				fprintf(stdout, "%c", table_separator);
			}
			fprintf(stdout, "%10.8f", test[i]->ks_pvalue);
			field++;
		}

		/*
		* Here is where dieharder sets is assessment.  Note that the
		* assessment MUST be correctly interpreted.  Basically, we set things
		* so that a test is judged weak if its final outcome pvalue occurs
		* less than 1% of the time symmetrically split on BOTH ends -- less
		* than 0.005 or greater than 0.995.  Failure is a pvalue occurring
		* less than 0.1% of the time (<0.0005 or >0.9995).  Weak results
		* SHOULD occur (therefore) one time in 100 on average or once every
		* run or two of dieharder -a.  Failure should also be carefully
		* judged -- a rng SHOULD generate a pvalue that "fails" one time in
		* every 30 runs of dieharder -a.
		*
		* The point is that both of these WILL HAPPEN FROM TIME TO TIME and
		* NOT mean that the generator is "bad" or "weak".  If a generator
		* shows up as "weak" on three results in one -a(ll) run, though,
		* quite frequently (as one tests different seeds) that's a problem!
		* In fact, it is failure!  Just remember that p should be uniformly
		* distributed on [0,1), and so judging failure on any particular
		* range only makes sense if it occurs systematically, indicating that
		* p is NOT uniform on [0,1).
		*/
		if (tflag){
			if (field != 0){
				fprintf(stdout, "%c", table_separator);
			}
			/*
			* I'm doing two sided testing, but not really anymore.  At
			* least, if I'm going to do two sided testing I need to make
			* the Xtrategy code in run_test.c do matching two sided
			* testing as well.  But for the moment, I think I'll just
			* leave in the high side weak/fail without kicking into
			* TTD/RA mode because it should be rare compared to normal
			* low-side failure in second order (at the kstest level) but
			* I could be wrong so why not flag it?
			*
			* I may change the WEAK call on the high side, though.  That
			* will be triggered too often for comfort.
			*/
			printf(dtest->name);
			printf("\n Xfail = %f \n", Xfail);
			printf("Xweak = %f \n", Xweak);
			printf("ks_pvalue = %f \n", test[i]->ks_pvalue);

			if (test[i]->ks_pvalue < Xfail || test[i]->ks_pvalue > 1.0 - Xfail){
				if (tflag & TNO_WHITE){
					fprintf(stdout, "%s", "FAILED");
					passed = -1;
				}
				else {
					fprintf(stdout, "%10s", "FAILED  ");
					passed = -1;
				}
			}
			else if (test[i]->ks_pvalue < Xweak || test[i]->ks_pvalue > 1.0 - Xweak){
				if (tflag & TNO_WHITE){
					fprintf(stdout, "%s", "WEAK");
					passed = 0;
				}
				else {
					fprintf(stdout, "%10s", "WEAK   ");
					passed = 0;
				}
			}
			else {
				if (tflag & TNO_WHITE){
					fprintf(stdout, "%s", "PASSED");
					passed = 1;
				}
				else {
					fprintf(stdout, "%10s", "PASSED  ");
					passed = 1;
				}
			}
			field++;
		}

		if (tflag & TSEED && strategy){
			if (field != 0){
				fprintf(stdout, "%c", table_separator);
			}
			if (tflag & TNO_WHITE){
				fprintf(stdout, "%lu", seed);
			}
			else {
				fprintf(stdout, "%10lu", seed);
			}
			field++;
		}

		/*
		* No separator at the end, just EOL
		*/
		fprintf(stdout, "\n");
		fflush(stdout);

		printf("%i sequenzes of bytes needed so far \n ", byteCount);
		byteCount = 0;
	}

}

/*
*========================================================================
* This code displays an ascii "*" histogram of the input e.g. p-value
* vector.
*========================================================================
*/

int output_histogram(double *input, char *pvlabel, int inum, double min, double max, int nbins, char *label)
{

	int i, j, hindex;
	unsigned int *bin, binmax;
	double binscale;
	unsigned int vscale;

	/*
	* This is where we put the binned count(s).  Make and zero it
	*/
	bin = (unsigned int *)malloc(nbins*sizeof(unsigned int));
	for (i = 0; i < nbins; i++) bin[i] = 0.0;

	/*
	* Set up the double precision size of a bin in the data range.
	*/
	binscale = (max - min) / (double)nbins;

	/*
	* Now we loop the data, incrementing bins accordingly.  There
	* are LOTS of ways to do this; we pick a brute force one instead
	* of e.g. sorting first because we don't quibble about microseconds
	* of run time...
	*/
	binmax = 0;

	/*
	* The only reason anyone might use histogram is so they can see
	* the distribution of pvalues in a pretty-printed report-style
	* exploration.  We therefore leave the # header character ONLY
	* in at the beginning, independent of prefix.  There's nothing
	* to parse here...
	*/
	printf("#=============================================================================#\n");
	printf("#                         Histogram of test p-values                          #\n");
	printf("#=============================================================================#\n");
	printf("# Bin scale = %f\n", binscale);
	for (i = 0; i < inum; i++){
		hindex = (int)(input[i] / binscale);
		/* printf("ks_pvalue = %f: bin[%d] = ",input[i],hindex); */
		if (hindex < 0) hindex = 0;
		if (hindex >= nbins) hindex = nbins - 1;
		bin[hindex]++;
		if (bin[hindex] > binmax) binmax = bin[hindex];
		/* printf("%d\n",bin[hindex]); */
	}

	/*
	* OK, at this point bin[] contains a histogram of the data.  All that
	* remains is to make a scaling decision and display it.  We'll
	* arbitrarily assume that the peak * scale is at 20, with two lines per
	* 0.1 of the scale, but we'll then scale this assumption using vscale.
	* Basically, the default is for psamples of 100, but we really need
	* to check the actual bins to ensure that we're good.
	*/
	vscale = ceil(psamples / 100.0);
	/* printf("psamples = %u   vscale = %u\n",psamples,vscale); */
	while (binmax >= 20 * vscale) {
		vscale++;
		/* printf("binmax = %u   vscale = %u\n",binmax,vscale); */
	}

	/*
	* Now we just display the histogram, which should be in range to
	* be displayed.
	*/
	for (i = 20; i > 0; i--){
		if (i % 2 == 0){
			printf("#  %5d|", i*vscale);
		}
		else {
			printf("#       |");
		}
		for (j = 0; j < nbins; j++){
			if (bin[j] >= i*vscale){
				printf("****|");
			}
			else {
				printf("    |");
			}
		}
		printf("\n");
	}
	printf("#       |--------------------------------------------------\n");
	printf("#       |");
	for (i = 0; i < nbins; i++) printf("%4.1f|", (i + 1)*binscale);
	printf("\n");
	printf("#=============================================================================#\n");
	fflush(stdout);

	return(0);

}

