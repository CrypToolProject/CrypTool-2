/*
 * $Id: chisq.c 229 2006-08-21 20:33:07Z rgb $
 *
 * See copyright in copyright.h and the accompanying file COPYING
 *
 */

/*
 *========================================================================
 * This code evaluates a vector of chisq's and p-values from a vector
 * of sampled results.  I believe that it assumes a Poissonian
 * distribution in the vector, not a normal one.  If so, we'll rename
 * this appropriately.
 *========================================================================
 */


#include "libdieharder.h"


/*
 * This routine computes chisq on a vector of nvec values drawn
 * presumably from a known discrete distribution (Poisson).
 * lambda is the expected mean, and pvec is a vector
 * where a list of associated p-values is returned.
 *
 * Note that a Poisson distribution with mean lambda is:
 *
 *  p(k) = (lambda^k/k!) exp(-lambda)
 *
 * and that it can be computed in a single pass by:
 *
 *  p(k) = gsl_ran_poisson_pdf(uint k,double lambda);
 *
 * All we need to do, then, is compute p(k) and use it to
 * determine a vector of estimates for the interval frequency,
 * then compute Pearson's chisq in a straightforward way.
 * The only "tricky" part of all of this is going to be a)
 * scaling the data by the number of independent samples; and
 * b) converting into p-values, which requires a knowledge of
 * the number of degrees of freedom of the fit.
 */

double chisq_poisson(unsigned int *observed,double lambda,int kmax,unsigned int nsamp)
{

 unsigned int k;
 double *expected;
 double delchisq,chisq,pvalue;

 /*
  * Allocate a vector for the expected value of the bin frequencies up
  * to kmax-1.
  */
 expected = (double *)malloc(kmax*sizeof(double));
 for(k = 0;k<kmax;k++){
   expected[k] = nsamp*gsl_ran_poisson_pdf(k,lambda);
 }

 /*
  * Compute Pearson's chisq for this vector of the data with poisson
  * expected values.
  */
 chisq = 0.0;
 for(k = 0;k < kmax;k++){
   delchisq = ((double) observed[k] - expected[k])*
      ((double) observed[k] - expected[k])/expected[k];
   chisq += delchisq;
   if(verbose == D_CHISQ || verbose == D_ALL){
     printf("%u:  observed = %f,  expected = %f, delchisq = %f, chisq = %f\n",
        k,(double)observed[k],expected[k],delchisq,chisq);
   }
 }

 if(verbose == D_CHISQ || verbose == D_ALL){
   printf("Evaluated chisq = %f for %u k values\n",chisq,kmax);
 }

 /*
  * Now evaluate the corresponding pvalue.  The only real question
  * is what is the correct number of degrees of freedom.  We have
  * kmax bins, so it should be kmax-1.
  */
 pvalue = gsl_sf_gamma_inc_Q((double)(kmax-1)/2.0,chisq/2.0);
 if(verbose == D_CHISQ || verbose == D_ALL){
   printf("pvalue = %f in chisq_poisson.\n",pvalue);
 }

 free(expected);

 return(pvalue);

}

/*
 * Pearson is the test for a straight up binned histogram, where bin
 * membership is with "independent" probabilities (that sum to 1).
 * Observed is the vector of observed histogram values.  Expected is
 * the vector of expected histogram values (where the two should
 * agree in total count).  kmax is the dimension of the data vectors.
 * It returns a pvalue PRESUMING kmax-1 degrees of freedom (independent
 * bin probabilities, but with a constraint that they sum to 1).
 */
double chisq_pearson(double *observed,double *expected,int kmax)
{

 unsigned int k;
 double delchisq,chisq,pvalue;

 /*
  * Compute Pearson's chisq for this vector of the data.
  */
 chisq = 0.0;
 for(k = 0;k < kmax;k++){
   delchisq = (observed[k] - expected[k])*
      (observed[k] - expected[k])/expected[k];
   chisq += delchisq;
   if(verbose){
     printf("%u:  observed = %f,  expected = %f, delchisq = %f, chisq = %f\n",
        k,observed[k],expected[k],delchisq,chisq);
   }
 }

 if(verbose){
   printf("Evaluated chisq = %f for %u k values\n",chisq,kmax);
 }

 /*
  * Now evaluate the corresponding pvalue.  The only real question
  * is what is the correct number of degrees of freedom.  We have
  * kmax bins, so it should be kmax-1.
  */
 pvalue = gsl_sf_gamma_inc_Q((double)(kmax-1)/2.0,chisq/2.0);
 if(verbose){
   printf("pvalue = %f in chisq_pearson.\n",pvalue);
 }

 return(pvalue);

}

/*
 * This does the pearson above, but computes histogram occupation using a
 * binomial distribution.  This is useful to compute e.g. the pvalue for a
 * vector of tally results from flipping 100 coins, performed 100 times.
 * It automatically cuts off the tails where bin membership isn't large
 * enough to give a good result.
 */
double chisq_binomial(double *observed,double prob,unsigned int kmax,unsigned int nsamp)
{

 unsigned int n,nmax,ndof;
 double expected,delchisq,chisq,pvalue,obstotal,exptotal;

 chisq = 0.0;
 obstotal = 0.0;
 exptotal = 0.0;
 ndof = 0;
 nmax = kmax;
 if(verbose){
   printf("# %7s   %3s      %3s %10s      %10s %9s\n",
           "bit/bin","DoF","X","Y","del-chisq","chisq");
   printf("#==================================================================\n");
 }
 for(n = 0;n <= nmax;n++){
   if(observed[n] > 10.0){
     expected = nsamp*gsl_ran_binomial_pdf(n,prob,nmax);
     obstotal += observed[n];
     exptotal += expected;
     delchisq = (observed[n] - expected)*(observed[n] - expected)/expected;
     chisq += delchisq;
     if(verbose){
       printf("# %5u     %3u   %10.4f %10.4f %10.4f %10.4f\n",
                  n,ndof,observed[n],expected,delchisq,chisq);
     }
     ndof++;
   }
 }
 if(verbose){
   printf("Total:  %10.4f  %10.4f\n",obstotal,exptotal);
   printf("#==================================================================\n");
   printf("Evaluated chisq = %f for %u degrees of freedom\n",chisq,ndof);
 }

 /*
  * Now evaluate the corresponding pvalue.  The only real question
  * is what is the correct number of degrees of freedom.  I'd argue we
  * did use a constraint when we set expected = binomial*nsamp, so we'll
  * go for ndof (count of bins tallied) - 1.
  */
 ndof--;
 pvalue = gsl_sf_gamma_inc_Q((double)(ndof)/2.0,chisq/2.0);
 if(verbose){
   printf("Evaluted pvalue = %6.4f in chisq_binomial.\n",pvalue);
 }

 return(pvalue);

}

/*
 * Contributed by David Bauer to do a Pearson chisq on a 2D
 * histogram.
 */
double chisq2d(unsigned int *obs, unsigned int rows, unsigned int columns, unsigned int N) {
	double chisq = 0.0;
	unsigned int i, j, k;
	unsigned int ndof = (rows - 1) * (columns - 1);

	for (i = 0; i < rows; i++) {
		for (j = 0; j < columns; j++) {
			unsigned int sum1 = 0, sum2 = 0;
			double expected, top;
			for (k = 0; k < columns; k++) sum1 += obs[i * columns + k];
			for (k = 0; k < rows; k++) sum2 += obs[k * columns + j];
			expected = (double) sum1 * sum2 / N;
			top = (double) obs[i * columns + j] - expected;
			chisq += (top * top) / expected;
		}
	}

	return( gsl_sf_gamma_inc_Q((double)(ndof)/2.0,chisq/2.0) );
}

/*
 * Contributed by David Bauer, copied from chisq_poisson, with trivial
 * modifications to change it to use the geometric distribution.
 */
double chisq_geometric(unsigned int *observed,double prob,int kmax,unsigned int nsamp)
{

 unsigned int k;
 double *expected;
 double delchisq,chisq,pvalue;

 /*
  * Allocate a vector for the expected value of the bin frequencies up
  * to kmax-1.
  */
 expected = (double *)malloc(kmax*sizeof(double));
 for(k = 0;k<kmax;k++){
   expected[k] = nsamp*gsl_ran_geometric_pdf(k+1,prob);
 }

 /*
  * Compute Pearson's chisq for this vector of the data with poisson
  * expected values.
  */
 chisq = 0.0;
 for(k = 0;k < kmax;k++){
   delchisq = ((double) observed[k] - expected[k])*
      ((double) observed[k] - expected[k])/expected[k];
   chisq += delchisq;
   if(verbose == D_CHISQ || verbose == D_ALL){
     printf("%u:  observed = %f,  expected = %f, delchisq = %f, chisq = %f\n",
        k,(double)observed[k],expected[k],delchisq,chisq);
   }
 }

 if(verbose == D_CHISQ || verbose == D_ALL){
   printf("Evaluated chisq = %f for %u k values\n",chisq,kmax);
 }

 /*
  * Now evaluate the corresponding pvalue.  The only real question
  * is what is the correct number of degrees of freedom.  We have
  * kmax bins, so it should be kmax-1.
  */
 pvalue = gsl_sf_gamma_inc_Q((double)(kmax-1)/2.0,chisq/2.0);
 if(verbose == D_CHISQ || verbose == D_ALL){
   printf("pvalue = %f in chisq_geometric.\n",pvalue);
 }

 free(expected);

 return(pvalue);
}

