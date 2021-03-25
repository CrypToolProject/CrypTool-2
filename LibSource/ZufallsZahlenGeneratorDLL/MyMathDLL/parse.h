/*
 *========================================================================
 * See copyright in copyright.h and the accompanying file COPYING
 *========================================================================
 * Utility routines to support basic needs of the program like managing
 * models, creating and deleting directories with contents.  Probable
 * cruft or yet-unused routines belong at the bottom...
 *========================================================================
 */


/*
 * This is a somewhat sloppy way to do this, I suppose, but it will work
 */
#define PBUF 128
#define PK   1024

 /*
  * Shared space
  */
 char splitbuf[PK][PBUF];

 /*
  * parse.c prototypes
  */
 int split(char *inbuffer);
 int parse(char *inbuffer,char **outfields,int maxfields,int maxfieldlength);
 void chop(char *buf);
