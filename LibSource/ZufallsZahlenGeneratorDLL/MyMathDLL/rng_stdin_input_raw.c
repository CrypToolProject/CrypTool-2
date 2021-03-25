#include <errno.h>
#include <stdlib.h>
#include <string.h>
#include <stdio.h>
#include "libdieharder.h"
#include "DieHarderDLL.h"
#include <setjmp.h>

/*
 * This is a wrapping of stdin.
 */

static unsigned long int stdin_input_raw_get (void *vstate);
static double stdin_input_raw_get_double (void *vstate);
static void stdin_input_raw_set (void *vstate, unsigned long int s);

static unsigned long int
stdin_input_raw_get (void *vstate)
{
  unsigned int j;
  int suc = fread(&j, sizeof(j), 1, dataIN);
  if (suc != 1) {
	  printf("%i sequenzes of size %i bytes needed so far \n ", byteCount, sizeof(j));
      if (feof(dataIN)) {
          fprintf(stderr,"# stdin_input_raw(): Error: EOF\n");
	  }
	  else {
		  fprintf(stderr, "# stdin_input_raw(): Error: %s\n", strerror(errno));
	  }
	  reachedEOF = 1;
	  longjmp(env, 1);
  }
  //printf("raw: %10u\n",j); 
  byteCount++;
  //printf("%i sequenzes of size %i bytes needed so far \n ", byteCount, sizeof(j));
  return j;
}

static double
stdin_input_raw_get_double (void *vstate)
{
  return stdin_input_raw_get(vstate) / (double)UINT_MAX;
}

static void
stdin_input_raw_set (void *vstate, unsigned long int s)
{
    /* empty */
}

static const gsl_rng_type stdin_input_raw_type =
{"stdin_input_raw",             /* name */
 UINT_MAX,                      /* RAND_MAX */
 0,                             /* RAND_MIN */
 0,
 stdin_input_raw_set,
 stdin_input_raw_get,
 stdin_input_raw_get_double};

const gsl_rng_type *gsl_rng_stdin_input_raw = &stdin_input_raw_type;
