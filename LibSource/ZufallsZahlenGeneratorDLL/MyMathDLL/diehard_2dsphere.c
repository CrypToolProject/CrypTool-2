/*
 * ========================================================================
 * $Id: diehard_2dsphere.c 231 2006-08-22 16:18:05Z rgb $
 *
 * See copyright in copyright.h and the accompanying file COPYING
 * ========================================================================
 */

/*
 * ========================================================================
 * This is the Diehard minimum distance (2d sphere) test, rewritten from 
 * the description in tests.txt on George Marsaglia's diehard site.
 *
 *               THE MINIMUM DISTANCE TEST                       ::
 * It does this 100 times::   choose n=8000 random points in a   ::
 * square of side 10000.  Find d, the minimum distance between   ::
 * the (n^2-n)/2 pairs of points.  If the points are truly inde- ::
 * pendent uniform, then d^2, the square of the minimum distance ::
 * should be (very close to) exponentially distributed with mean ::
 * .995 .  Thus 1-exp(-d^2/.995) should be uniform on [0,1) and  ::
 * a KSTEST on the resulting 100 values serves as a test of uni- ::
 * formity for random points in the square. Test numbers=0 mod 5 ::
 * are printed but the KSTEST is based on the full set of 100    ::
 * random choices of 8000 points in the 10000x10000 square.      ::
 *
 *                      Comment
 * Obviously the same test as 3d Spheres but in 2d, hence the
 * name.  This test has a BUG in it -- the expression it uses to
 * evaluate p is not accurate enough to withstand the demands of
 * dieharder.  M. Fischler pointed this out and derived the
 * required corrections (giving explicit forms useful out to d=5)
 * and they are incorporated in rgb_minimum_distance().  This test
 * is hence OBSOLETE and is left in so people can play with it and
 * convince themselves that this is so.
 *
 * I did make one set of changes to this test to make it considerably more
 * efficient at extracting the minimum distance.
 * ========================================================================
 */


#include "libdieharder.h"

#define POINTS_2D 8000
#define DIM_2D 2

typedef struct {
  double x[DIM_2D];
} C3_2D;
 
int compare_points(const dTuple *a,const dTuple *b);
double distance(const dTuple a,const dTuple b,uint dim);

int diehard_2dsphere(Test **test, int irun)
{

 int i,j,d,t;

 /*
  * These are the vector of points and the current point being
  * considered.  We may or may not need to restructure the vectors
  * to be able to do the sort.  I'm going to TRY to implement
  * Fischler's suggested algorithm here that is NlogN instead of doing
  * the straightforward N^2 algorithm, but we'll see.
  */
 dTuple *points;
 double dist,mindist;

 /*
  * for display only.
  */
 test[0]->ntuple = ntuple;

 /*
  * Generate d-tuples of tsamples random coordinates in the range 0-10000
  * (which we may have to scale with dimension). Determine the shortest
  * separation of any pair of points.  From this generate p from the
  * Marsaglia form, and apply the usual KS test over psamples of
  * independent tests, per dimension.
  */
 test[0]->ntuple = 2;      /* 2 dimensional test, of course */
 points = (dTuple *)malloc(test[0]->tsamples*sizeof(dTuple));


 if(verbose == D_DIEHARD_2DSPHERE || verbose == D_ALL){
     printf("Generating a list of %u points in %d dimensions\n",test[0]->tsamples,test[0]->ntuple);
 }
 for(t=0;t<test[0]->tsamples;t++){
   /*
    * Generate a new d-dimensional point in the unit d-cube (with
    * periodic boundary conditions).
    */
   if(verbose == D_DIEHARD_2DSPHERE || verbose == D_ALL){
       printf("points[%u]: (",t);
   }
   for(d=0;d<2;d++) {
     points[t].c[d] = gsl_rng_uniform_pos(rng)*10000;
     if(verbose == D_DIEHARD_2DSPHERE || verbose == D_ALL){
       printf("%6.4f",points[t].c[d]);
       if(d == 1){
         printf(")\n");
       } else {
         printf(",");
       }
     }
   }
 }

 /*
  * Now we sort the points using gsl_heapsort and a comparator
  * on the first coordinate only.  Don't know how to get rid
  * of the gcc prototype warning.  Probably need a cast of some
  * sort.
  */
 gsl_heapsort(points,test[0]->tsamples,sizeof(dTuple),
                    (gsl_comparison_fn_t) compare_points);

 if(verbose == D_DIEHARD_2DSPHERE || verbose == D_ALL){
   printf("List of points sorted by first coordinate:\n");
   for(t=0;t<test[0]->tsamples;t++){
     printf("points[%u]: (",t);
     for(d=0;d<2;d++) {
       printf("%6.4f",points[t].c[d]);
       if(d == 1){
         printf(")\n");
       } else {
         printf(",");
       }
     }
   }
 }

 /*
  * Now we do the SINGLE PASS through to determine mindist
  */
 mindist = 10000.0;
 for(i=0;i<test[0]->tsamples;i++){
   /*
    * One thing to experiment with here (very much) is
    * whether or not we need periodic wraparound.  For
    * the moment we omit it, although distributing
    * the points on a euclidean d-torus seems more symmetric
    * than not and checks to be sure that points are correct
    * on or very near a boundary.
    */
   for(j=i+1;j<test[0]->tsamples;j++){
     if(points[j].c[0] - points[i].c[0] > mindist) break;
     dist = distance(points[j],points[i],2);
     MYDEBUG(D_DIEHARD_2DSPHERE) {
       printf("d(%d,%d) = %16.10e\n",i,j,dist);
     }
     if( dist < mindist) mindist = dist;
   }
 }
 MYDEBUG(D_DIEHARD_2DSPHERE) {
   printf("Found minimum distance = %16.10e\n",mindist);
 }

 /*
  * This form should be "bad", but I'm finding the badness
  * difficult to demonstrate for presumed good rngs.  One
  * does wonder how large an effect Fischler's corrections
  * are for n = 8000.  I'm guessing that they should be
  * more important for smaller n, but that will be difficult
  * to try without converting this further into a scale-free
  * form (that is, just like rgb_minimum_distance() but without
  * the qarg correction piece.  Hmmm, I could do that by hacking
  * its value to 1.0 in rgb_minimum_distance now, couldn't I?
  */
 test[0]->pvalues[irun] = 1.0 - exp(-mindist*mindist/0.995);

 free(points);

 MYDEBUG(D_DIEHARD_2DSPHERE) {
   printf("# diehard_2dsphere(): test[0]->pvalues[%u] = %10.5f\n",irun,test[0]->pvalues[irun]);
 }

 return(0);

}

