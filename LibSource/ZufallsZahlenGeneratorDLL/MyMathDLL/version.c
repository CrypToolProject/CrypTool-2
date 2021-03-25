/*
 * $Id: list_rngs.c 420 2008-08-18 18:29:17Z rgb $
 *
 * See copyright in copyright.h and the accompanying file COPYING
 */

/*
 * ========================================================================
 * These two functions JUST output a "standard header" with version and
 * copyright information for use in various output routines or the version
 * information alone.  Note that version might be e.g. 3.28.0beta, that
 * is, it may not be strictly numerical.
 * ========================================================================
 */

#include "libdieharder.h"

/*
 * dh_header isn't QUITE trivial, because the version string can vary in
 * length.  If it is longer than around 20 characters, this is going to
 * make ugly output but nothing should "break".  Note that we assume 80
 * character lines, sorry.
 */

#define LINE_LENGTH 80
void dh_header()
{

 int i,half,version_length;

 version_length = strlen(QUOTEME(VERSION));

 fprintf(stdout,"#=============================================================================#\n");
 fprintf(stdout,"#");
 /* Pad the front */
 half = (LINE_LENGTH - 48 - version_length - 2)/2;
 for(i=0;i<half;i++){
   fprintf(stdout," ");
 }
 fprintf(stdout,"dieharder version %s Copyright 2003 Robert G. Brown",QUOTEME(VERSION));
 /* Pad the rear */
 half = LINE_LENGTH - 52 - version_length - half;
 for(i=0;i<half;i++){
   fprintf(stdout," ");
 }
 fprintf(stdout,"#\n");
 fprintf(stdout,"#=============================================================================#\n");

}

void dh_version()
{
 fprintf(stdout,"%s\n",QUOTEME(VERSION));
}
