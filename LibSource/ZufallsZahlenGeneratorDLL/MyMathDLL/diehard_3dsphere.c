/*
 * $Id: diehard_3dsphere.c 231 2006-08-22 16:18:05Z rgb $
 *
 * See copyright in copyright.h and the accompanying file COPYING
 */

/*
 *========================================================================
 * This is the Diehard 3d spheres test, rewritten from the description
 * in tests.txt on George Marsaglia's diehard site.
 *
 * Basically, we choose 4000 points in a cube of side 1000.  Compute the
 * smallest nearest neighbor distance (radius R of the smallest sphere
 * that doesn't overlap any neighboring point). R^3 is exponentially
 * distributed with an empirical exponential distribution with mean 30.
 * Thus p = 1.0 - exp(-R^3/30.0) should be a uniform distribution.  Run
 * a KS test on a vector of independent samples of this entire test to
 * find out.
 *========================================================================
 */


#include "libdieharder.h"

#define POINTS_3D 4000
#define DIM_3D 3

typedef struct {
  double x[DIM_3D];
} C3_3D;
 
int diehard_3dsphere(Test **test, int irun)
{

 int j,k;
 C3_3D *c3;
 double r1,r2,r3,rmin,r3min;
 double xdelta,ydelta,zdelta;

 /*
  * for display only.  Test dimension is 3, of course.
  */
 test[0]->ntuple = 3;

 r3min = 0;

 /*
  * This one should be pretty straightforward.  Generate a vector
  * of three random coordinates in the range 0-1000 (check the
  * diehard code to see what "in" a 1000^3 cube means, but I'm assuming
  * real number coordinates greater than 0 and less than 1000).  Do
  * a simple double loop through to float the smallest separation out.
  * Generate p, save in a sample vector.  Apply KS test.
  */
 c3 = (C3_3D *)malloc(POINTS_3D*sizeof(C3_3D));

 rmin = 2000.0;
 for(j=0;j<POINTS_3D;j++){
   /*
    * Generate a new point in the cube.
    */
   for(k=0;k<DIM_3D;k++) c3[j].x[k] = 1000.0*gsl_rng_uniform_pos(rng);
   if(verbose == D_DIEHARD_3DSPHERE || verbose == D_ALL){
     printf("%d: (%8.2f,%8.2f,%8.2f)\n",j,c3[j].x[0],c3[j].x[1],c3[j].x[2]);
   }

   /*
    * Now compute the distance between the new point and all previously
    * picked points.
    */
   for(k=j-1;k>=0;k--){
     xdelta = c3[j].x[0]-c3[k].x[0];
     ydelta = c3[j].x[1]-c3[k].x[1];
     zdelta = c3[j].x[2]-c3[k].x[2];
     r2 = xdelta*xdelta + ydelta*ydelta + zdelta*zdelta;
     r1 = sqrt(r2);
     r3 = r2*r1;
     if(verbose == D_DIEHARD_3DSPHERE || verbose == D_ALL){
       printf("%d-%d: |(%6.2f,%6.2f,%6.2f)| = r1 = %f rmin = %f, \n",
          j,k,xdelta,ydelta,zdelta,r1,rmin);
     }
     if(r1<rmin) {
       rmin = r1;
       r3min = r3;
     }
   }
 }

 MYDEBUG(D_DIEHARD_3DSPHERE) {
   printf("Found rmin = %f  (r^3 = %f)\n",rmin,r3min);
 }
 test[0]->pvalues[irun] = 1.0 - exp(-r3min/30.0);

 MYDEBUG(D_DIEHARD_3DSPHERE) {
   printf("# diehard_3dsphere(): test[0]->pvalues[%u] = %10.5f\n",irun,test[0]->pvalues[irun]);
 }

 nullfree(c3);

 return(0);

}

