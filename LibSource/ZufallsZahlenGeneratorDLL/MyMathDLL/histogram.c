/*
 * $Id: histogram.c 529 2011-04-01 17:49:31Z rgbatduke $
 *
 * See copyright in copyright.h and the accompanying file COPYING
 *
 */

/*
 *========================================================================
 * This code displays an ascii "*" histogram of the input e.g. p-value
 * vector.
 *========================================================================
 */


#include "libdieharder.h"


void histogram(double *input,char *pvlabel,int inum,double min,double max,int nbins,char *label)
{

 int i,j,hindex;
 uint *bin,binmax;
 double binscale;
 uint vscale;

 /*
  * This is where we put the binned count(s).  Make and zero it
  */
 bin = (uint *)malloc(nbins*sizeof(uint));
 for(i=0;i<nbins;i++) bin[i] = 0.0;

 /*
  * Set up the double precision size of a bin in the data range.
  */
 binscale = (max - min)/(double)nbins;

 /*
  * Now we loop the data, incrementing bins accordingly.  There
  * are LOTS of ways to do this; we pick a brute force one instead
  * of e.g. sorting first because we don't quibble about microseconds
  * of run time...
  */
 binmax = 0;
 printf("#==================================================================\n");
 printf("#                Histogram of %s\n",label);
 printf("%s",pvlabel);
 printf("# Counting histogram bins, binscale = %f\n",binscale);
 for(i=0;i<inum;i++){
   hindex = (int) (input[i]/binscale);
   /* printf("ks_pvalue = %f: bin[%d] = ",input[i],hindex); */
   if(hindex < 0) hindex = 0;
   if(hindex >= nbins) hindex = nbins-1;
   bin[hindex]++;
   if(bin[hindex] > binmax) binmax = bin[hindex];
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
 vscale = ceil(psamples/100.0);
 /* printf("psamples = %u   vscale = %u\n",psamples,vscale); */
 while(binmax >= 20*vscale) {
   vscale++;
   /* printf("binmax = %u   vscale = %u\n",binmax,vscale); */
 }

 /*
  * Now we just display the histogram, which should be in range to
  * be displayed.
  */
 for(i=20;i>0;i--){
   if(i%2 == 0){
     printf("#  %5d|",i*vscale);
   } else {
     printf("#       |");
   }
   for(j=0;j<nbins;j++){
     if(bin[j] >= i*vscale ){
       printf("****|");
     } else {
       printf("    |");
     }
   }
   printf("\n");
 }
 printf("#       |--------------------------------------------------\n");
 printf("#       |");
 for(i=0;i<nbins;i++) printf("%4.1f|",(i+1)*binscale);
 printf("\n");
 printf("#==================================================================\n");

}

