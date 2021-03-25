/*
 * See copyright in copyright.h and the accompanying file COPYING
 */

/*
 *========================================================================
 * This is the Diehard sums test, rewritten from the description
 * in tests.txt on George Marsaglia's diehard site.
 *
 *             The  OVERLAPPING SUMS test                        ::
 * Integers are floated to get a sequence U(1),U(2),... of uni-  ::
 * form [0,1) variables.  Then overlapping sums,                 ::
 *   S(1)=U(1)+...+U(100), S(2)=U(2)+...+U(101),... are formed.  ::
 * The S's are virtually normal with a certain covariance mat-   ::
 * rix.  A linear transformation of the S's converts them to a   ::
 * sequence of independent standard normals, which are converted ::
 * to uniform variables for a KSTEST. The  p-values from ten     ::
 * KSTESTs are given still another KSTEST.                       ::
 *
 *                       Comments
 * 
 * From the papers I've collected, the reason for the overlap is
 * simply to use all of the data when observing whether or not
 * the quantities are uniformly distributed.  The use of a
 * covariance matrix doesn't test anything different, it just
 * uses fewer rands to achieve a given sensitivity.  I would therefore
 * strongly suggest that users use the far more sensitive and reliable
 * rgb_lagged_sum test which tests more or less the same thing in an
 * unbroken and scalable way.
 *
 * At this point I think there is rock solid evidence that this test
 * is completely useless in every sense of the word.  It is broken,
 * and it is so broken that there is no point in trying to fix it.
 * If I crank up tsamples to where one can actually see the asymptotic
 * distribution (which should be uniform) it is visibly non-uniform and
 * indeed the final kstest CONVERGES to a non-zero pvalue of 0.09702690
 * for ALL rngs tested, which hardly seems useful.  If one runs the test
 * on only 100 samples (as Marsaglia's code did in both the original
 * fortran or the newer C version) but then increases the number of
 * runs of the test from the default 100, it is easy to make it fail for
 * gold standard generators.  The test just doesn't work.  It cannot be
 * used to identify a generator that fails the null hypothesis.
 *
 * Don't use it.
 *========================================================================
 */


#include "libdieharder.h"

int diehard_sums(Test **test, int irun)
{

 int m,t;
 double mean,std;
 double *x,*y,*rand_list;
 double newrand;
 double a,b;

 /*
  * for display only.  0 means "ignored".
  */
 test[0]->ntuple = 0;

 /*
  * SUMS requires that we evaluate y[i] = \sum_i^{100+i} u[i].  Naturally,
  * the sum (or mean, if you prefer) is supposedly normally distributed
  * according to the central limit theorem.  So each number has an
  * associated p-value -- the probability of that particular number
  * occurring from the random sum.  The vector of p-values can be turned
  * into an overall p-value via a KS test.
  *
  * I have a set of really good papers on this.  One is on nilpotent
  * Markov chain tests communicated by  Janusz Kawczak.  A whole
  * series of better ones have come from Stefan Wegankittl.  In fact,
  * Stefan give more or less an exact prescription for writing this
  * sort of test in terms of the weak inverse of its covariance matrix.
  *
  * Unfortunately, Marsaglia's description is pretty opaque, at least
  * to a poor ignorant physicist like myself.  I quote:
  *
  *   The y's covariance matrix T is Toeplitz with diagonals 100,99,...2,1
  *   A Cholesky factorization of T: V'V = T provides M=V^(-1).  The
  *   covariance of x = yM is M'TM=I.  The columns of M have at most
  *   3 non-zero elements.
  *
  * Ouch!  Toeplitz used as an adjective!  Cholesky factorization!
  * What means these strange terms?  OK, here's what Wikipedia has
  * to say:  A Toeplitz matrix is a "diagonal constant matrix".  This
  * means that all elements on the parallel diagonals are the same, so
  * the prescription above actually tells one exactly what T is.
  *
  * Cholesky decomposition takes a symmetric positive definite matrix
  * and decomposes it into a lower triangular matrix and the transpose
  * of the lower triangular matrix, where the lower triangular matrix
  * is called the Cholesky triangle.  It is primarily used to compute
  * the solution to Ax = b via A = LL^T (really LL^* but L is real),
  * solving Ly = b (which is easy, just "backsubstitution"), then
  * solving L^T x = y for x.
  *
  * Translated, this means (I think) that we form T = VV^T, invert
  * M = V^(-1), and then form x = My (where y is a set of overlapping
  * sums).  This converts y back to an ordinarly multivariate normal
  * with a chisq distribution with 100-1 degrees of freedom (where the
  * latter I'm not quite certain of).  But again Wegankittl will help
  * me rewrite the test from scratch, I think.
  *
  * Generators that fail when the number of test samples is only 100 PASS
  * when I crank up the number of samples to around 200 or 300, then fail
  * consistently with a distribution of p that is now biased LOW (where
  * before it was biased HIGH) for all the "best" rngs in the GSL.
  * Again, this suggests that there is a very subtle error somewhere but
  * is far from definitive.  If you are a real stats person and a coder
  * and are reading this, PLEASE consider helping out by figuring this
  * out and contributing a fix -- otherwise I'll fix it (or at least
  * address it) when I have time to get to it.
  */

 if(verbose == D_DIEHARD_SUMS || verbose == D_ALL){
   printf("# diehard_sums:  focus with -v %d.\n",D_DIEHARD_SUMS);
 }

 /*
  * John E. Davis noticed that I failed to initialize m.
  * This produces one set of annoying errors.  However,
  * it is better to say that I failed to eliminate m
  * altogether in favor of tsamples.  I'm going to go
  * BACKWARDS and restore m EVERYWHERE.  I'm going to
  * just freeze the diehard version as close as I can
  * make it to the way it was in the fortran and
  * rewrite the whole test from Wegankittl for general
  * tsamples later (volunteers welcome).
  */
 m = test[0]->tsamples;

 /*
  * Needed to transform raw sums into a N(0,1) distro.
  *
  * Note well that the "user" sample test in the dieharder
  * source proper >>is<< sums, only without overlap
  * and hellaciously more accurate and easier to understand.
  * A run of diehard_sums with tsamples = psamples = 100
  * consumes 20,000 rands -- 200 used to make 100 overlapping
  * sums, transformed into iid multivariate normal N(0,1),
  * turned into p-values on [0,1), turned into a p-value,
  * 100 times for 100 cumulate p-values.  No matter how
  * you slice it, no matter how hard you work to deal with
  * the overlapping sample covariance, you have 20,000
  * rands in the whole computation.  Compare to running
  * the "sample" sums test with tsamples = 100 and psamples
  * = 10,000, one also gets 100 sums of 100 each 100 times,
  * this time to just make 10,000 pvalues (which one might
  * as well do in this sums as well, frankly).  However,
  * one samples 1,000,000 rands along the way!  For most
  * systematic failures, the resolution in the latter case will
  * be AT LEAST \sqrt(50) times better, and there is
  * virtually no possibility of programmer error as the
  * code is trivial -- no Toeplitz or Cholesky need apply!
  */
 mean = 0.5*m;
 std = sqrt(12.0);

 /*
  * Allocate x, y and rand_list.  No need to zero x and rand_list,
  * y is an accumulator and we zero on general principles.
  */
 x = (double *)malloc(m*sizeof(double));
 rand_list = (double *)malloc(m*sizeof(double));
 y = (double *)malloc(m*sizeof(double));
 memset(y,0,m*sizeof(double));

 /*
  * Fill a rand_list, summing into y[0] at the same time.
  */
 if(verbose == D_DIEHARD_SUMS || verbose == D_ALL){
   printf("#==================================================================\n");
   printf("# Initializing initial y[0] and rand_list\n");
 }
 for(t=0;t<m;t++){
	 rand_list[t] = gsl_rng_uniform(rng);
   y[0] += rand_list[t];
   if(verbose == D_DIEHARD_SUMS || verbose == D_ALL){
     printf("y[0] =  y[0] + %f = %f\n",rand_list[t],y[0]);
   }
 }
   
 if(verbose == D_DIEHARD_SUMS || verbose == D_ALL){
   printf("#==================================================================\n");
   printf("# Now we generate the rest of the %u overlapping y's\n",m);
   printf("y[%u] =  %f (raw)\n",0,y[0]);
 }
 for(t=1;t<m;t++){
   /*
    * Each successive sum is the previous one, with its first
    * entry in rand_list[] removed.
    */
   newrand = gsl_rng_uniform(rng);
   y[t] = y[t-1] - rand_list[t-1] + newrand;
   if(verbose == D_DIEHARD_SUMS || verbose == D_ALL){
     printf("y[%u] =  %f - %f + %f = %f (raw)\n",t,y[t-1],rand_list[t-1],newrand,y[t]);
   }

   /* We're done with y[t-1] as a raw sum.  Convert it to be N(0,1) */
   y[t-1] = (y[t-1] - mean)*std;

   if(verbose == D_DIEHARD_SUMS || verbose == D_ALL){
     printf("y[%u] =  %f (converted)\n",t-1,y[t-1]);
   }
 }
 /* We're done with y[m-1] as a raw sum.  Convert it to be N(0,1) */
 y[m-1] = (y[m-1] - mean)*std;
 if(verbose == D_DIEHARD_SUMS || verbose == D_ALL){
   printf("y[%u] =  %f (converted)\n",m-1,y[m-1]);
 }

 /*
  * We convert these (evaluated either way) to a unit normal and thence
  * to a uniform distribution.
  */
 if(verbose == D_DIEHARD_SUMS || verbose == D_ALL){
   printf("#==================================================================\n");
   printf("# We convert it to a normal distribution of width 1.0\n");
 }
 x[0] = y[0]/sqrt(1.0*m);
 x[1] = -x[0]*(m-1)/sqrt(2.0*m - 1.0) + y[1]*sqrt(m/(2.0*m - 1.0));
 x[0] =  gsl_cdf_gaussian_P(x[0],1.0);
 x[1] =  gsl_cdf_gaussian_P(x[1],1.0);
 if(verbose == D_DIEHARD_SUMS || verbose == D_ALL){
   printf("x[0] = %f\n",x[0]);
   printf("x[1] = %f\n",x[1]);
 }

 for(t=2;t<m;t++){
   /*
    * t is offset by 1 relative to diehard's i.
    *
    * diehard: do i=3,m:  a = 2m + 2 - i
    *
    *  and
    *
    * diehard: b = 4m + 2 - 2i
    *
    * We note that diehard: 2a = 4m + 4 - 2i so b = 2a - 2 independent
    * of t or i.
    */

   a = 2.0*m + 1.0 - t;
   b = 2.0*a - 2.0;
   /* x[t] = y[0]/sqrt(a*b) - y[t-1]*sqrt((a-1.0)/(b+2.0)) + y[t]*sqrt(a/b);*/
   x[t] = y[t-2]/sqrt(a*b) - y[t-1]*sqrt((a-1.0)/(b+2.0)) + y[t]*sqrt(a/b);
   x[t] =  gsl_cdf_gaussian_P(x[t],1.0);
   if(verbose == D_DIEHARD_SUMS || verbose == D_ALL){
     printf("x[%u] = %f\n",t,x[t]);
   }
 }


 /*
  * If everything above went well, the x[i]'s are a somewhat noisy UNIFORM
  * distribution from 0-1.  We do the same old ks test on them that we'll
  * eventually do on ks_pvalue[] to make the "one" p-value for this test.
  * However, it is very important to be able to SEE this distribution for
  * large numbers of tsamples.
 histogram(x,"pvalues",m,0.0,1.0,10,"x-values");
  */
 if(verbose == D_DIEHARD_SUMS || verbose == D_ALL){
   histogram(x,"pvalues",m,0.0,1.0,10,"x-values");
 }
 test[0]->pvalues[irun] = kstest(x,m);
 MYDEBUG(D_DIEHARD_SUMS) {
   printf("# diehard_sums(): test[0]->pvalues[%u] = %10.5f\n",irun,test[0]->pvalues[irun]);
 }

 free(x);
 free(y);
 free(rand_list);

 return(0);

}

