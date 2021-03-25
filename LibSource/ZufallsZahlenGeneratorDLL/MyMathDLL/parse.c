/*
 *========================================================================
 * See copyright in copyright.h and the accompanying file COPYING
 *========================================================================
 * Utility routines to support basic needs of the program like managing
 * models, creating and deleting directories with contents.  Probable
 * cruft or yet-unused routines belong at the bottom...
 *========================================================================
 */

#include <stdio.h>
#include <string.h>
#include "parse.h"

 int verbose;

/*
 * split() is a reusable routine to break up a string into char[PBUF]
 * fields.  Anything past PBUF characters is truncated.  The output
 * is put into parsebuf[i] and the number of fields found is returned.
 * It is somewhat like the perl split function except that I give it
 * a 128K buffer of its very own to work with and you don't get to
 * choose your delimiters.
 */
int split(char *inbuffer)
{ 

 char delim[7],*nextval;

 if(verbose){
   printf("split(%s)\n",inbuffer);
 }

  
 /* 
  * Permit blank, tab, or comma separators anywhere we need to parse
  * a line.
  */
 delim[0] = ' ';                /* blanks */
 delim[1] = (char) 9;           /* tab */
 delim[2] = ',';                /* comma */
 delim[3] = (char) 10;		/* LF */
 delim[4] = (char) 13;		/* CR */
 delim[5] = ':';		/* : needed to parse /proc/net/dev or passwd */
 delim[6] = (char) 0;           /* terminator */

 
 nextval = strtok(inbuffer,delim);
 if(nextval == (char *)NULL)
 {
	return 0;
 }
 else
 {
 int i = 0;
 strncpy(splitbuf[i],nextval,PBUF);
 if(verbose){
   printf("split(): split field[%d] = %s.\n",i,splitbuf[i]);
 }
 i++;

 while(i < PK-1){
   nextval = strtok((char *) NULL, delim);
   if(nextval == (char *)NULL) break;
   strncpy(splitbuf[i], nextval,PBUF);
   if(verbose){
     printf("split(): split field[%d] = %s.\n",i,splitbuf[i]);
   }
   i++;
 }

 /* Null the last field */
 memset(splitbuf[i],0,PBUF);
 if(verbose){
   printf("split(): Terminated split field[%d] = %s.\n",i,splitbuf[i]);
   printf("split(): Returning %d as the field count\n",i);
 }

 return i;
 }
}

/*
 * parse() is a reusable routine to break up a string into char[32]
 * fields.  Anything past 32 characters is truncated.
 */

int parse(char *inbuffer,char **outfields,int maxfields,int maxfieldlength)
{

 char delim[7],*nextval;
 int i = 0;

 if(verbose){
   printf("parse():\n");
 }



/* 
 * Permit blank, tab, or comma separators anywhere we need to parse
 * a line.
 */
 delim[0] = ' ';                /* blanks */
 delim[1] = (char) 9;           /* tab */
 delim[2] = ',';                /* comma */
 delim[3] = (char) 10;		/* LF */
 delim[4] = (char) 13;		/* CR */
 delim[5] = ':';		/* : needed to parse /proc/net/dev or passwd */
 delim[6] = (char) 0;           /* terminator */

 
 nextval = strtok(inbuffer,delim);
 if(nextval == (char *)NULL) return 0;
 strncpy(outfields[i++],nextval,maxfieldlength);
 if(verbose){
   printf("parse(): Parsed field[%d] = %s.\n",i-1,outfields[i-1]);
 }

 while(i < maxfields-1){
   nextval = strtok((char *) NULL, delim);
   if(nextval == (char *)NULL) break;
   strncpy(outfields[i++], nextval,maxfieldlength);
   if(verbose){
     printf("parse(): Parsed field[%d] = %s.\n",i-1,outfields[i-1]);
   }
 }

 /* Null the last field */
 memset(outfields[i],0,maxfieldlength);
 if(verbose){
   printf("parse(): Terminated field[%d] = %s.\n",i,outfields[i]);
 }

 return i;

}

void chop(char *buf)
{

 /* Advance to end of string */
 while(*buf != 0) buf++;
 buf--;
 /* Reterminate one character earlier */
 *buf = 0;
}
 
