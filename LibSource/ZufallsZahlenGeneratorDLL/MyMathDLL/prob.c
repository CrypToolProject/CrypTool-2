/*
* $Id: prob.c 223 2006-08-17 06:19:38Z rgb $
*
* See copyright in copyright.h and the accompanying file COPYING
*
*/

/*
 *========================================================================
 * timing and utility sources.  tv_start and tv_stop are globals.
 *========================================================================
 */

#include "libdieharder.h"

double binomial(unsigned int n, unsigned int k, double p)
{

 double pnk;

 if(verbose > 10){
   printf("binomial(): Making binomial p(%d,%d,%f)\n",n,k,p);
 }

 pnk = gsl_sf_fact(n)*pow(p,(double)k)*pow((1.0-p),(double)(n-k))/
             (gsl_sf_fact(k)*gsl_sf_fact(n-k));

 if(verbose > 10){
   printf("binomial(): Made binomial p(%d,%d,%f) = %f\n",n,k,p,pnk);
 }

 return(pnk);

}

