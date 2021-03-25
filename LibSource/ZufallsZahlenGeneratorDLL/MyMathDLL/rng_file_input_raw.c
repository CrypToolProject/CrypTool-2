/*
 * file_input_raw
 *
 * See copyright in copyright.h and the accompanying file COPYING
 *
 */

#include "libdieharder.h"

/*
 * This is a wrapper for getting random numbers in RAW (binary integer)
 * format from a file.  A raw file has no headers -- it is presumed to be
 * a pure bit stream.  We therefore have to read it in a page at a time,
 * realloc its required storage as needed, and count as we go.  In this
 * way we can figure out if e.g. a compressed file is sufficiently
 * "random" to make it likely that the compression is good and so on.
 */

static unsigned long int file_input_raw_get(void *vstate);
static double file_input_raw_get_double(void *vstate);
static void file_input_raw_set(void *vstate, unsigned long int s);

/*
 * This typedef struct file_input_state_t struct contains the data
 * maintained on the operation of the file_input rng, and can be accessed
 * via rng->state->whatever
 *
 *   fp is the file pointer
 *   flen is the number of rands in the file (filecount)
 *   rptr is a count of rands returned since last rewind
 *   rtot is a count of rands returned since the file was opened or it
 *      was deliberately reset.
 *   rewind_cnt is a count of how many times the file was rewound since
 *      its last open.
 *
 * file_input_state_t is defined in libdieharder.h currently and shared with
 * file_input_raw.c
 */

static unsigned long int file_input_raw_get(void *vstate)
{

	file_input_state_t *state = (file_input_state_t *)vstate;
	unsigned int iret;

	/*
	 * Check that the file is open (via file_input_raw_set()).
	 */
	if (state->fp != NULL) {

		/*
		 * Read in the next random number from the file
		 */
		if (fread(&iret, sizeof(uint), 1, state->fp) != 1){
			fprintf(stderr, "# file_input_raw(): Error.  This cannot happen.\n");
			exit(0);
		}

		/*
		 * Success. iret is presumably valid and ready to return.  Increment the
		 * counter of rands read so far.
		 */
		state->rptr++;
		state->rtot++;
		if (verbose){
			fprintf(stdout, "# file_input() %u: %u/%u -> %u\n", (uint)state->rtot, (uint)state->rptr, (uint)state->flen, (uint)iret);
		}

		/*
		 * This (with seed s == 0) basically rewinds the file and resets
		 * state->rptr to 0, but rtot keeps running,
		 */
		if (state->flen && state->rptr == state->flen){
			/*
			 * Reset/rewind the file
			 */
			file_input_raw_set(vstate, 0);
		}
		return(iret);
	}
	else {
		fprintf(stderr, "Error: %s not open.  Exiting.\n", filename);
		exit(0);
	}

}

static double file_input_raw_get_double(void *vstate)
{
	return file_input_raw_get(vstate) / (double)UINT_MAX;
}


/*
 * file_input_raw_set() is very simple.  If the file hasn't been opened
 * yet, it opens it and sets flen and rptr to zero.  Otherwise it
 * rewinds it and sets rptr to zero.  Typically it is only called one
 * time per file by the user, although it will be called once per read
 * page by file_input_raw_get().
 */

static void file_input_raw_set(void *vstate, unsigned long int s)
{

	static uint first = 1;
	struct stat sbuf;
	

	file_input_state_t *state = (file_input_state_t *)vstate;

	if (verbose == D_FILE_INPUT_RAW || verbose == D_ALL){
		fprintf(stdout, "# file_input_raw(): entering file_input_raw_set\n");
		fprintf(stdout, "# file_input_raw(): state->fp = %p, seed = %lu\n", (void*)state->fp, s);
	}

	/*
	 * Get and set the file length, check to make sure the file exists,
	 * whatever...
	 */
	if (first){
		if (verbose){
			fprintf(stdout, "# file_input_raw(): entering file_input_raw_set 1st call.\n");
		}

		/*
		 * This clears an obscure bug in FreeBSD reported by Lucius Windschuh,
		 * lwindschuh@googlemail.com, I think.  Otherwise it should be
		 * harmless.  It just initializes state->fp to 0 so that the file
		 * correctly opens later.
		 */
		state->fp = NULL;

		if (stat(filename, &sbuf)){
			if (errno == EBADF){
				fprintf(stderr, "# file_input_raw(): Error -- file descriptor %s bad.\n", filename);
				exit(0);
			}
		}
		/*
		 * Is this a regular file?  If so, turn its byte length into a 32 bit uint
		 * length.
		 */
		struct stat s;
		if (stat(sbuf.st_mode, &s) == 0)
		{
			if (s.st_mode & S_IFDIR)
			{
				fprintf(stderr, "# file_input_raw(): Error -- path %s is a directory.\n", filename);
				exit(0);
			}
			else if (s.st_mode & S_IFREG)
			{
				/*
				* sbuf.st_size should be type off_t, which is automatically u_int64_t
				* if FILE_OFFSET_BITS is set to 64, which it is.  So this should be
				* able to manage large file sizes.   Similarly, in the struct
				* file_input_state_t flen should be type off_t.  This means that
				* filecount has to be off_t as well.
				*/
				state->flen = sbuf.st_size / sizeof(uint);
				filecount = state->flen;
				if (filecount < 16) {
					fprintf(stderr, "# file_input_raw(): Error -- file %s is too small.\n", filename);
					exit(0);
				}
			}
			else
			{
				//something else
			}
		}
		else
		{
			/*
			* This is neither a file nor a directory, so we will not
			* even try to seek.
			*/
			state->flen = 0;
		}
		
		/*
		if (S_ISREG(sbuf.st_mode)){
			//*
			 * sbuf.st_size should be type off_t, which is automatically u_int64_t
			 * if FILE_OFFSET_BITS is set to 64, which it is.  So this should be
			 * able to manage large file sizes.   Similarly, in the struct
			 * file_input_state_t flen should be type off_t.  This means that
			 * filecount has to be off_t as well.
			 
			state->flen = sbuf.st_size / sizeof(uint);
			filecount = state->flen;
			if (filecount < 16) {
				fprintf(stderr, "# file_input_raw(): Error -- file %s is too small.\n", filename);
				exit(0);
			}
		}
		else if (S_ISDIR(sbuf.st_mode)){
			fprintf(stderr, "# file_input_raw(): Error -- path %s is a directory.\n", filename);
			exit(0);
		}
		else {
			/*
			 * This is neither a file nor a directory, so we will not
			 * even try to seek.
			 
			state->flen = 0;
		}

		/*
		 * This segment is executed only one time when the file is FIRST opened.
		 */
		first = 0;
	}

	/*
	 * We use the "seed" to determine whether or not to reopen or
	 * rewind.  A seed s == 0 for an open file means rewind; a seed
	 * of anything else forces a close (resetting rewind_cnt) followed
	 * by a reopen.
	 */
	if (state->fp && s) {
		if (verbose == D_FILE_INPUT || verbose == D_ALL){
			fprintf(stdout, "# file_input(): Closing/reopening/resetting %s\n", filename);
		}
		fclose(state->fp);
		state->fp = NULL;
	}

	if (state->fp == NULL){
		if (verbose == D_FILE_INPUT_RAW || verbose == D_ALL){
			fprintf(stdout, "# file_input_raw(): Opening %s\n", filename);
		}

		/*
		 * If we get here, the file exists, is a regular file, and we know its
		 * length.  We can now open it.  The test catches all other conditions
		 * that might keep the file from reading, e.g. permissions.
		 */
		if ((state->fp = fopen(filename, "r")) == NULL) {
			fprintf(stderr, "# file_input_raw(): Error: Cannot open %s, exiting.\n", filename);
			exit(0);
		}

		/*
		 * OK, so if we get here, the file is open.
		 */
		if (verbose == D_FILE_INPUT_RAW || verbose == D_ALL){
			fprintf(stdout, "# file_input_raw(): Opened %s for the first time.\n", filename);
			fprintf(stdout, "# file_input_raw(): state->fp is %8p, file contains %u unsigned integers.\n", (void*)state->fp, (uint)state->flen);
		}
		state->rptr = 0;  /* No rands read yet */
		/*
		 * We only reset the entire file if there is a nonzero seed passed in.
		 * This clears both rtot and rewind_cnt in addition to rptr.
		 */
		if (s) {
			state->rtot = 0;
			state->rewind_cnt = 0;
		}

	}
	else {
		/*
		 * Rewinding seriously reduces the size of the space being explored.
		 * On the other hand, bombing a test also sucks, especially in a long
		 * -a(ll) run.  Therefore we rewind every time our file pointer reaches
		 * the end of the file or call gsl_rng_set(rng,0).
		 */
		if (state->flen && state->rptr >= state->flen){
			rewind(state->fp);
			state->rptr = 0;
			state->rewind_cnt++;
			if (verbose == D_FILE_INPUT_RAW || verbose == D_ALL){
				fprintf(stderr, "# file_input_raw(): Rewinding %s at rtot = %u\n", filename, (uint)state->rtot);
				fprintf(stderr, "# file_input_raw(): Rewind count = %u, resetting rptr = %u\n", state->rewind_cnt, (uint)state->rptr);
			}
		}
		else {
			return;
		}

	}

}

static const gsl_rng_type file_input_raw_type =
{ "file_input_raw",                        /* name */
UINT_MAX,                    /* RAND_MAX */
0,                           /* RAND_MIN */
sizeof (file_input_state_t),
&file_input_raw_set,
&file_input_raw_get,
&file_input_raw_get_double };

const gsl_rng_type *gsl_rng_file_input_raw = &file_input_raw_type;
