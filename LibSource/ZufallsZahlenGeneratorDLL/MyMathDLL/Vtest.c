/*
 *========================================================================
 * $Id: Vtest.c 223 2006-08-17 06:19:38Z rgb $
 *
 * See copyright in copyright.h and the accompanying file COPYING
 *========================================================================
 */

/*
 *========================================================================
 * Vtest.c contains a set of routines for evaluating a p-value, the
 * probability that a given test result was obtained IF the underlying
 * random number generator was a "good" one (the null hypothesis), given a
 * vector x of observed results and a vector y of expected results.  It
 * determines the p-value using Pearson's chisq, which does not require
 * the independent input of the expected sigma for each "bin" (vector
 * position).
 *========================================================================
 */
#include "libdieharder.h"

void Vtest_create(Vtest *vtest, unsigned int nvec)
{

 int i;
 MYDEBUG(D_VTEST){
   printf("#==================================================================\n");
   printf("# Vtest_create(): Creating test struct for %u nvec.\n",nvec);
 }
 vtest->x = (double *) malloc(sizeof(double)*nvec);       /* sample results */
 vtest->y = (double *) malloc(sizeof(double)*nvec);       /* expected sample results */
 /* zero or set everything */
 for(i=0;i<nvec;i++){
   vtest->x[i] = 0.0;
   vtest->y[i] = 0.0;
 }
 vtest->nvec = nvec;
 vtest->ndof = 0;  /* The user must enter this, or it will try to compute it */
 vtest->chisq = 0.0;
 vtest->pvalue = 0.0;
 vtest->cutoff = 5;
 MYDEBUG(D_VTEST){
   printf("# Vtest_create(): Done.\n");
 }


}

void Vtest_destroy(Vtest *vtest)
{

 free(vtest->x);
 free(vtest->y);

}

void Vtest_eval(Vtest *vtest)
{

 uint i,ndof,itail;
 double delchisq,chisq;
 double x_tot,y_tot;


 /*
  * This routine evaluates chi-squared, where:
  * vtest->x is the trial vector
  * vtest->y is the exact vector
  * vtest->sigma is the vector of expected error for y
  *              (for the exact/true distribution)
  * vtest->nvec is the vector length(s).
  * vtest->ndof is the number of degrees of freedom (default nvec-1)
  * vtest->cutoff is the minimum expected count for a cell to be
  * included in the chisq sum (it should be at least 5, in general,
  * probably higher in some cases).
  *
  * x, y, sigma, nvec all must be filled in my the calling routine.
  * Be sure to override the default value of ndof if it is known to
  * the caller.
  *
  * Note well that chisq is KNOWN to do poorly -- sometimes very
  * poorly -- if ndof=1 (two mutually exclusive and exhaustive parameters,
  * e.g. a normal approximation to the binomial) or if y (the expected
  * value) for any given cell is less than a cutoff usually set to around
  * 5.  This test will therefore routinely bundle all cells with expected
  * returns less than the user-defined cutoff AUTOMATICALLY into a single
  * cell (itail) and use the total number of cells EXCLUSIVE of this
  * "garbage" cell as the number of degrees of freedom unless ndof is
  * overridden.
  */
 /* verbose=1; */
 MYDEBUG(D_VTEST){
   printf("Evaluating chisq and pvalue for %d points\n",vtest->nvec);
   printf("Using a cutoff of %f\n",vtest->cutoff);
 }

 chisq = 0.0;
 x_tot = 0.0;
 y_tot = 0.0;
 ndof = 0;
 itail = -1;
 MYDEBUG(D_VTEST){
   printf("# %7s   %3s      %3s %10s      %10s %10s %9s\n",
           "bit/bin","DoF","X","Y","sigma","del-chisq","chisq");
   printf("#==================================================================\n");
 }
 /*
  * If vtest->ndof is nonzero, we use it to compute chisq.  If not, we try
  * to estimate it based on a vtest->cutoff that can be set by the caller.
  * If vtest->ndof is set, the cutoff should probably not be.
  */
 for (i=0;i<vtest->nvec;i++) {
   if(vtest->y[i] >= vtest->cutoff) {
     x_tot += vtest->x[i];
     y_tot += vtest->y[i];
     delchisq = (vtest->x[i] - vtest->y[i])*(vtest->x[i] - vtest->y[i])/vtest->y[i];
     /*  Alternative way of evaluating chisq for binomial only.
     delchisq = (vtest->x[i] - vtest->y[i])/vtest->sigma[i];
     delchisq *= delchisq;
     */
     chisq += delchisq;
     MYDEBUG(D_VTEST){
       printf("# %5u\t%3u\t%12.4f\t%12.4f\t%8.4f\t%10.4f\n",
                  i,vtest->ndof,vtest->x[i],vtest->y[i],delchisq,chisq);
     }
     /* increment only if the data is substantial */
     if(vtest->ndof == 0) ndof++;
   } else {
     if(itail == -1){
       itail = i;  /* Do nothing; just remember the index */
       MYDEBUG(D_VTEST){
         printf("  Saving itail = %u because vtest->x[i] = %f <= %f\n",itail,vtest->x[i],vtest->cutoff);
       }
     } else {
       /*
        * Accumulate all the tail expectation here.
	*/
       vtest->y[itail] += vtest->y[i];
       vtest->x[itail] += vtest->x[i];
     }
   }
 }
 /*
  * At the end, ALL the counts that are statistically weak should sum into
  * a statistically significant tail count, but the tail count still has
  * to make the cutoff!  Sometimes it won't!  Note that the toplevel
  * conditional guards against itail still being -1 because Vtest did nothing
  * in its last pass through the code above.
  */
 if(itail != -1){
   if(vtest->y[itail] >= vtest->cutoff){
     delchisq = (vtest->x[itail] - vtest->y[itail])*
              (vtest->x[itail] - vtest->y[itail])/vtest->y[itail];
     chisq += delchisq;
     /* increment only if the data is substantial */
     if(vtest->ndof == 0) ndof++;
     MYDEBUG(D_VTEST){
       printf("# %5u\t%3u\t%12.4f\t%12.4f\t%8.4f\t%10.4f\n",
              itail,vtest->ndof,vtest->x[itail],vtest->y[itail],delchisq,chisq);
     }
   }
 }
















 /*
  * Interestingly, one simply cannot make the tail "work" as a
  * contribution to the ndof.  The number of degrees of freedom is one
  * less than the number that make the cutoff, although it does seem
  * useful to add the last chunk from the tail before doing the
  * computation of the chisq p-value.
  */
 if(vtest->ndof == 0){
   vtest->ndof = ndof-1;
   /*
    * David Bauer:  TODO BUG??  The returned ndof is correct, but the
    * wrong value is used.
    *
    * RGB comment: Really, if ndof = 0, the test fails, does it not?
    * How can you fit a curve with no degrees of freedom.  Perhaps
    * an error and exit, or some other signal of failure?  (This should
    * almost never happen...)
    */
 }

 MYDEBUG(D_VTEST){
   printf("Total:  %10.4f  %10.4f\n",x_tot,y_tot);
   printf("#==================================================================\n");
   printf("Evaluated chisq = %f for %u degrees of freedom\n",chisq,vtest->ndof);
 }
 vtest->chisq = chisq;

 /*
  * Now evaluate the corresponding pvalue.  The only real question
  * is what is the correct number of degrees of freedom.  I'd argue we
  * did use a constraint when we set expected = binomial*nsamp, so we'll
  * go for ndof (count of nvec tallied) - 1.
  */
 vtest->pvalue = gsl_sf_gamma_inc_Q((double)(vtest->ndof)/2.0,chisq/2.0);
 /* printf("Evaluted pvalue = %6.4f in Vtest_eval() with %u ndof.\n",vtest->pvalue,vtest->ndof); */
 MYDEBUG(D_VTEST){
   printf("Evaluted pvalue = %6.4f in Vtest_eval().\n",vtest->pvalue);
 }
 /* verbose=0; */

}

