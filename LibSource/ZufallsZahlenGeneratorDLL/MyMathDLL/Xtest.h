/*
 *========================================================================
 * $Id: Btest.c 212 2006-07-21 18:07:33Z rgb $
 *
 * See copyright in copyright.h and the accompanying file COPYING
 *========================================================================
 */

 /*
  * This is the struct used in Xtest.c, a erfc based test that generates
  * a p-value for a single normally distributed statistic.
  */
 typedef struct {
   unsigned int npts;
   double p;
   double x;
   double y;
   double sigma;
   double pvalue;
 } Xtest;

 void Xtest_eval(Xtest *xtest);
