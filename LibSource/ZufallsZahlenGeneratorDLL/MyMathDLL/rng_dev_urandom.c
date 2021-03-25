/*
 * dev_urandom
 */

#include "libdieharder.h"

/*
 * This is a wrapping of the /dev/random hardware rng
 */
static unsigned long int dev_urandom_get (void *vstate);
static double dev_urandom_get_double (void *vstate);
static void dev_urandom_set (void *vstate, unsigned long int s);

typedef struct
  {
    FILE *fp;
  }
dev_urandom_state_t;

static unsigned long int
dev_urandom_get (void *vstate)
{
  dev_urandom_state_t *state = (dev_urandom_state_t *) vstate;
  unsigned long int j;

  if(state->fp != NULL) {
    fread(&j,sizeof(j),1,state->fp);
    return j;
  } else {
    fprintf(stderr,"Error: /dev/urandom not open.  Exiting.\n");
    exit(0);
  }

}

static double
dev_urandom_get_double (void *vstate)
{
  return dev_urandom_get (vstate) / (double) UINT_MAX;
}

static void
dev_urandom_set (void *vstate, unsigned long int s)
{
  dev_urandom_state_t *state = (dev_urandom_state_t *) vstate;

 if ((state->fp = fopen("/dev/urandom","r")) == NULL) {
   fprintf(stderr,"Error: Cannot open /dev/urandom, exiting.\n");
   fprintf(stderr,"/dev/urandom may only be available on Linux systems.\n");
   exit(0);
 }

 return;

}

static const gsl_rng_type dev_urandom_type =
{"/dev/urandom",		/* name */
 UINT_MAX,			/* RAND_MAX */
 0,				/* RAND_MIN */
 sizeof (dev_urandom_state_t),
 &dev_urandom_set,
 &dev_urandom_get,
 &dev_urandom_get_double};

const gsl_rng_type *gsl_rng_dev_urandom = &dev_urandom_type;
