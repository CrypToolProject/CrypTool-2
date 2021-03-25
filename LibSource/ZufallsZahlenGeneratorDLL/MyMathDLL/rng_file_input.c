/*
 *========================================================================
 * $Id: libdieharder.h 176 2006-07-11 21:18:27Z rgb $
 *
 * file_input (GSL compatible).
 * 
 * By Daniel Summerhays
 * Mar. 10, 2005
 *
 * Heavily modifed by rgb, June-July 2006 (and beyond)
 * 
 * See copyright in copyright.h and the accompanying file COPYING
 *========================================================================
 */

#include "libdieharder.h"

/*
 * This is a wrapper for getting random numbers from a file.  Note
 * CAREFULLY how we must proceed to access the state variables inside of a
 * given rng.
 */

static unsigned long int file_input_get (void *vstate);
static double file_input_get_double (void *vstate);
static void file_input_set (void *vstate, unsigned long int s);

/*
 * This typedef struct file_input_state_t struct contains the data
 * maintained on the operation of the file_input rng, and can be accessed
 * via rng->state->whatever
 *
 *   fp is the file pointer
 *   flen is the number of rands in the file (state->flen)
 *   rptr is a count of rands returned since last rewind
 *   rtot is a count of rands returned since the file was opened or it
 *      was deliberately reset.
 *   rewind_cnt is a count of how many times the file was rewound since
 *      its last open.
 *
 * file_input_state_t is defined in libdieharder.h currently and shared with
 * file_input_raw.c
 *
 * In this way the routines below should work for BOTH file_input AND
 * file_input_raw (as would rng->state->rtot, e.g., from the calling
 * routine :-).
 */

uint file_input_get_rewind_cnt(gsl_rng *rng)
{
  file_input_state_t *state = (file_input_state_t *) rng->state;
  return state->rewind_cnt;
}

off_t
file_input_get_rtot(gsl_rng *rng)
{
  file_input_state_t *state = (file_input_state_t *) rng->state;
  return state->rtot;
}

void
file_input_set_rtot(gsl_rng *rng,uint value)
{
  file_input_state_t *state = (file_input_state_t *) rng;
  state->rtot = 0;
}

static unsigned long int file_input_get (void *vstate)
{

 file_input_state_t *state = (file_input_state_t *) vstate;
 unsigned int iret;
 double f;
 char inbuf[K]; /* input buffer */

 /*
  * Check that the file is open (via file_input_set()).
  */
 if(state->fp != NULL) {

   /*
    * Read in the next random number from the file
    */
   if(fgets(inbuf,K,state->fp) == 0){
     fprintf(stderr,"# file_input(): Error: EOF on %s\n",filename);
     exit(0);
   }
   /*
    * Got one (as we pretty much have to unless the file is badly
    * broken).  Convert the STRING input above into a uint according to
    * the "type" (basically matching scanf type).
    */
   switch(filetype){
     /*
      * 32 bit unsigned int by assumption
      */
     case 'd':
     case 'i':
     case 'u':
       if(0 == sscanf(inbuf,"%u",&iret)){
         fprintf(stderr,"Error: converting %s failed.  Exiting.\n", inbuf);
         exit(0);
       }
       break;
     /*
      * double precision floats get converted to 32 bit uint
      */
     case 'e':
     case 'E':
     case 'f':
     case 'F':
     case 'g':
       if(0 == sscanf(inbuf,"%lg",&f)){
         fprintf(stderr,"Error: converting %s failed.  Exiting.\n", inbuf);
         exit(0);
       }
       iret = (uint) f*UINT_MAX;
       break;
     /*
      * OK, so octal is really pretty silly, but we got it.  Still uint.
      */
     case 'o':
       if(0 == sscanf(inbuf,"%o",&iret)){
         fprintf(stderr,"Error: converting %s failed.  Exiting.\n", inbuf);
         exit(0);
       }
       break;
     /*
      * hexadecimal is silly too, but we got it.  uint, of course.
      */
     case 'x':
       if(0 == sscanf(inbuf,"%x",&iret)){
         fprintf(stderr,"Error: converting %s failed.  Exiting.\n", inbuf);
         exit(0);
       }
       break;
     case 'X':
       if(0 == sscanf(inbuf,"%X",&iret)){
         fprintf(stderr,"Error: converting %s failed.  Exiting.\n", inbuf);
         exit(0);
       }
       break;
     /*
      * binary is NOT so silly.  Let's do it.  The hard way.  A typical
      * entry should look like:
      *    01110101001010100100111101101110
      */
     case 'b':
       iret = bit2uint(inbuf,filenumbits);
       break;
     default:
       fprintf(stderr,"# file_input(): Error. File type %c is not recognized.\n",filetype);
       exit(0);
       break;
   }

   /*
    * Success. iret is presumably valid and ready to return.  Increment the
    * counter of rands read so far.
    */
   state->rptr++;
   state->rtot++;
   if(verbose){
     fprintf(stdout,"# file_input() %lu: %lu/%lu -> %u\n", state->rtot, state->rptr,state->flen,(uint)iret);
   }

   /*
    * This (with seed s == 0) basically rewinds the file and resets
    * state->rptr to 0, but rtot keeps running,
    */
   if(state->rptr == state->flen) {
     /*
      * Reset/rewind the file
      */
     file_input_set(vstate, 0);
   }
   return iret;
 } else {
   fprintf(stderr,"Error: %s not open.  Exiting.\n", filename);
   exit(0);
 }

}

static double file_input_get_double (void *vstate)
{
  return file_input_get (vstate) / (double) UINT_MAX;
}


/*
 * file_input_set() is not yet terriby robust.  For example, it
 * cannot cope with missing header info, duplicate header info,
 * impossible header info.  It should work, though, for a well-formed
 * header
 */

static void file_input_set (void *vstate, unsigned long int s)
{

 int cnt,numfields;
 char inbuf[K]; /* input buffer */

 file_input_state_t *state = (file_input_state_t *) vstate;

 if(verbose == D_FILE_INPUT || verbose == D_ALL){
   fprintf(stdout,"# file_input(): entering file_input_set\n");
   fprintf(stdout,"# file_input(): state->fp = %p, seed = %lu\n",(void*) state->fp,s);
 }

 /*
  * We use the "seed" to determine whether or not to reopen or
  * rewind.  A seed s == 0 for an open file means rewind; a seed
  * of anything else forces a close (resetting rewind_cnt) followed
  * by a reopen.
  */
 if(state->fp && s ) {
   if(verbose == D_FILE_INPUT || verbose == D_ALL){
     fprintf(stdout,"# file_input(): Closing/reopening/resetting %s\n",filename);
   }
   /* fclose(state->fp); */
   state->fp = NULL;
 }

 if (state->fp == NULL){
   if(verbose == D_FILE_INPUT || verbose == D_ALL){
     fprintf(stdout,"# file_input(): Opening %s\n", filename);
   }

   /*
    * If we get here, the file exists, is a regular file, and we know its
    * length.  We can now open it.  The test catches all other conditions
    * that might keep the file from reading, e.g. permissions.
    */
   if ((state->fp = fopen(filename,"r")) == NULL) {
     fprintf(stderr,"# file_input(): Error: Cannot open %s, exiting.\n", filename);
     exit(0);
   }


   /*
    * OK, so if we get here, the file is open.
    */
   if(verbose == D_FILE_INPUT || verbose == D_ALL){
     fprintf(stdout,"# file_input(): Opened %s for the first time at %p\n", filename,(void*) state->fp);
     fprintf(stdout,"# file_input(): state->fp is %8p\n",(void*) state->fp);
     fprintf(stdout,"# file_input(): Parsing header:\n");
   }
   state->rptr = 0;  /* No rands read yet */
   /*
    * We only reset the entire file if there is a nonzero seed passed in.
    * This clears both rtot and rewind_cnt in addition to rptr.
    */
   if(s) {
     state->rtot = 0;
     state->rewind_cnt = 0;
   }

 } else {
   /*
    * Rewinding seriously reduces the size of the space being explored.
    * On the other hand, bombing a test also sucks, especially in a long
    * -a(ll) run.  Therefore we rewind every time our file pointer reaches
    * the end of the file or call gsl_rng_set(rng,0).
    */
   if(state->rptr >= state->flen){
     rewind(state->fp);
     state->rptr = 0;
     state->rewind_cnt++;
     if(verbose == D_FILE_INPUT || verbose == D_ALL){
       fprintf(stderr,"# file_input(): Rewinding %s at rtot = %u\n", filename,(uint) state->rtot);
       fprintf(stderr,"# file_input(): Rewind count = %u, resetting rptr = %lu\n",state->rewind_cnt,state->rptr);
     }
   } else {
     return;
   }
 }

 /*
  * We MUST have precisely three data lines at the beginning after
  * any comments.
  */
 cnt = 0;
 while(cnt < 3){
   if(state->fp != NULL) {
     if(fgets(inbuf,K,state->fp) == 0){
       fprintf(stderr,"# file_input(): Error: EOF on %s\n",filename);
       exit(0);
     }
   }
   if(verbose){
     fprintf(stdout,"%d: %s",cnt,inbuf);
   }

   /*
    * Skip comments altogether, whereever they might be.  Also adopt code
    * to use new, improved, more portable "split()" command.
    */
   if(inbuf[0] != '#'){
     /*
      * Just like perl, sorta.  In fact, I'm really liking using
      * perl-derived utility functions for parsing where I can.
      */
     chop(inbuf);
     numfields = split(inbuf);
     if(numfields != 2){
       fprintf(stderr,"# file_input(): Error: Wrong number of fields: format is 'fieldname: value'\n");
       exit(0);
     }
     if(strncmp(splitbuf[0],"type",4) == 0){
       filetype = splitbuf[1][0];
       cnt++;
       if(verbose){
         fprintf(stdout,"# file_input(): cnt = %d\n",cnt);
         fprintf(stdout,"# file_input(): filenumtype set to %c\n",filetype);
       }
     }
     if(strncmp(splitbuf[0],"count",5) == 0){
       state->flen = atoi(splitbuf[1]);
       filecount = state->flen;
       cnt++;
       if(verbose){ 
         fprintf(stdout,"# file_input(): cnt = %d\n",cnt);
         fprintf(stdout,"# file_input(): state->flen set to %lu\n",state->flen);
       }
     }
     if(strncmp(splitbuf[0],"numbit",6) == 0){
       filenumbits = atoi(splitbuf[1]);
       cnt++;
       if(verbose){ 
         fprintf(stdout,"# file_input(): cnt = %d\n",cnt);
         fprintf(stdout,"# file_input(): filenumbits set to %i\n",filenumbits);
       }
     }
   }
 }

 return;

}

static const gsl_rng_type file_input_type =
{"file_input",                        /* name */
 UINT_MAX,                    /* RAND_MAX */
 0,                           /* RAND_MIN */
 sizeof (file_input_state_t),
 &file_input_set,
 &file_input_get,
 &file_input_get_double };

const gsl_rng_type *gsl_rng_file_input = &file_input_type;
