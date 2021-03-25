/*
* See copyright in copyright.h and the accompanying file COPYING
*/

/*
*========================================================================
* This is the Kolmogorov-Smirnov test for uniformity on the interval
* [0,1).  p-values from a (large) number of independent trials should
* be uniformly distributed on the interval [0,1) if the underlying
* result is consistent with the hypothesis that the rng in use is
* not correlated.  Deviation from uniformity is a sign of the failure
* of the hypothesis that the generator is "good".  Here we simply
* input a vector of p-values and a count, and output an overall
* p-value for the set of tests.
*========================================================================
*/

#include "libdieharder.h"
#define KCOUNTMAX 4999

double p_ks_new(int n,double d);

double kstest(double *pvalue,int count)
{

	int i;
	double y,d,d1,d2,dmax,csqrt;
	double p,x;

	/* First, handle degenerate cases. */
	if (count < 1) return -1.0;
	if (count == 1) return *pvalue;

	/*
	* We start by sorting the list of pvalues.
	*/
	gsl_sort(pvalue,1,count);

	/*
	* Here's the test.  For each (sorted) pvalue, its index is the
	* number of values cumulated to the left of it.  d is the distance
	* between that number and the straight line representing a uniform
	* accumulation.  We save the maximum d across all cumulated samples
	* and transform it into a p-value at the end.
	*/
	dmax = 0.0;
	if(verbose == D_KSTEST || verbose == D_ALL){
		printf("       p             y              d             d1           d2         dmax\n");
	}
	for(i=1;i<=count;i++){
		y = (double) i/(count+1.0);
		/*
		* d = fabs(pvalue[i] - y);
		*
		* Correction by David Bauer, pulled from R code for KS.
		* Apparently the above line is right/left biased and this
		* handles the position more symmetrically.   This fix is
		* CRUCIAL for small sample sizes, and can be validated with:
		*   dieharder -d 204 -t 100 -p 10000 -D default -D histogram
		*
		* Note:  Without the fabs, pvalue could be LESS than y
		* and be ignored by fmax.  Also, I don't really like the end
		* points -- y[0] shouldn't be zero, y[count] shouldn't be one.  This
		* sort of thing seems as thought it might matter at very high
		* precision.  Let's try running from 1 to count and dividing by count
		* plus 1.
		*/
		d1 = pvalue[i-1] - y;
		d2 = fabs(1.0/(count+1.0) - d1);
		d1 = fabs(d1);
		d = fmax(d1,d2);

		if(d1 > dmax) dmax = d1;
		if(verbose == D_KSTEST || verbose == D_ALL){
			printf("%11.6f   %11.6f    %11.6f   %11.6f  %11.6f  %11.6f\n",pvalue[i-1],y,d,d1,d2,dmax);
		}

	}

	/*
	* Here's where we have to make a few choices:
	*
	* ks_test = 0
	* Use the new algorithm when count is less than KCOUNTMAX, but
	* for large values of the count use the old q_ks(), valid for
	* asymptotically large counts and MUCH faster.  This will
	* will introduce a SMALL error in the distribution of pvalues
	* for all of the tests together, but it will be negligible for
	* any given single test.
	*
	* ks_test = 1
	* Use the new (mostly exact) algorithm exactly as is.  This is
	* QUITE SLOW although it is "sped up" at the expense of some
	* precision.  By slow I mean 220 seconds at -k 1, 2.5 seconds at
	* (default) -k 0.
	*
	* ks_test = 2
	* Use the exact (7 digit accurate) version of the new code, but
	* be prepared for "long" runtimes.  Empirically I only got 230
	* seconds -- not enough to worry about, really.
	* 
	*/

	/*
	* We only need this test here (and can adjust KCOUNTMAX to play
	* with accuracy vs speed, although I've verified that 5000 isn't
	* terrible).  The ks_test = 1 or 2 option are fallthrough, with
	* 1 being "normal", 2 omitting the speedup step below to get FULL
	* precision.
	*/
	if(ks_test == 0 && count > KCOUNTMAX){
		csqrt = sqrt(count);
		x = (csqrt + 0.12 + 0.11/csqrt)*dmax;
		p = q_ks(x);
		if(verbose == D_KSTEST || verbose == D_ALL){
			printf("# kstest: returning p = %f\n",p);
		}
		return(p);
	}

	/*
	* This uses the new "exact" kolmogorov distribution, which appears to
	* work!  It's moderately expensive computationally, but I think it will
	* be in bounds for typical dieharder test ranges, and I also expect that
	* it can be sped up pretty substantially.
	*/

	if(verbose == D_KSTEST || verbose == D_ALL){
		printf("# kstest: calling p_ks_new(count = %d,dmax = %f)\n",count,dmax);
	}
	p = p_ks_new(count,dmax);
	if(verbose == D_KSTEST || verbose == D_ALL){
		printf("# kstest: returning p = %f\n",p);
	}

	return(p);

}


double q_ks(double x)
{

	int i,sign;
	double qsum;
	double kappa;

	kappa = -2.0*x*x;
	sign = -1;
	qsum = 0.0;
	for(i=1;i<100;i++){
		sign *= -1;
		qsum += (double)sign*exp(kappa*(double)i*(double)i);
		if(verbose == D_KSTEST || verbose == D_ALL){
			printf("Q_ks %d: %f\n",i,2.0*qsum);
		}
	}

	if(verbose == D_KSTEST || verbose == D_ALL){
		printf("Q_ks returning %f\n",2.0*qsum);
	}
	return(2.0*qsum);

}


/*
*========================================================================
* The following routines are from the paper "Evaluating Kolmogorov's
* Distribution" by Marsaglia, Tsang and Wang.  They should permit kstest
* to return precise pvalues for more or less arbitrary numbers of samples
* ranging from n = 10 out to n approaching the asymptotic form where
* Kolmogorov's original result is valid.  It contains some cutoffs
* intended to prevent excessive runtimes in the rare cases where p being
* returned is close to 1 and n is large (at which point the asymptotic
* form is generally adequate anyway).
*
* This is so far a first pass -- I'm guessing that we can improve the
* linear algebra using the GSL or otherwise.  The code is therefore
* more or less straight from the paper.
*========================================================================
*/
void mMultiply(double *A,double *B,double *C,int m)
{
	int i,j,k;
	double s;
	for(i=0; i<m; i++){
		for(j=0; j<m; j++){
			s=0.0;
			for(k=0; k<m; k++){
				s+=A[i*m+k]*B[k*m+j];
				C[i*m+j]=s;
			}
		}
	}
}

void mPower(double *A,int eA,double *V,int *eV,int m,int n)
{
	double *B;
	int eB,i,j;

	/*
	* n == 1: first power just returns A.
	*/
	if(n == 1){
		for(i=0;i<m*m;i++){
			V[i]=A[i];*eV=eA;
		}
		return;
	}

	/*
	* This is a recursive call.  Either n/2 will equal 1 (and the line
	* above will return and the recursion will terminate) or it won't
	* and we will cumulate the product.
	*/
	mPower(A,eA,V,eV,m,n/2);
	/* printf("n = %d  mP eV = %d\n",n/2,*eV); */
	B=(double*)malloc((m*m)*sizeof(double));
	mMultiply(V,V,B,m);
	eB=2*(*eV);
	if(n%2==0){
		for(i=0;i<m*m;i++){
			V[i]=B[i];
		}
		*eV=eB;
		/* printf("n = %d (even) eV = %d\n",n,*eV); */
	} else {
		mMultiply(A,B,V,m);
		*eV=eA+eB;
		/* printf("n = %d (odd) eV = %d\n",n,*eV); */
	}

	/*
	* Rescale as needed to avoid overflow.  Note that we check
	* EVERY element of V to make sure NONE of them exceed the
	* threshold (and if any do, rescale the whole thing).
	*/
	for(i=0;i<m*m;i++) {
		if( V[i] > 1.0e140 ) {
			for(j=0;j<m*m;j++) {
				V[j]=V[j]*1.0e-140;
			}
			*eV+=140;
			/* printf("rescale eV = %d\n",*eV); */
		}
	}

	free(B);

}

/*
* Marsaglia's definition is K = 1 - p.  I convert it to p, as p is
* what we want in dieharder.
*/
double p_ks_new(int n,double d)
{

	int k,m,i,j,g,eH,eQ;
	double h,s,*H,*Q;

	/*
	* The next fragment is used if ks_test is not 2.  This is faster
	* than going to convergence, but is still really slow compared to
	* switching to the asymptotic form.
	*
	* If you require >7 digit accuracy in the right tail use ks_test = 2
	* but be prepared for occasional long runtimes.
	*/
	s=d*d*n;
	if(ks_test != 2 && ( s>7.24 || ( s>3.76 && n>99 ))) {
		if(n == 10400) printf("Returning the easy way\n");
		return 2.0*exp(-(2.000071+.331/sqrt(n)+1.409/n)*s);
	}

	/*
	* If ks_test = 2, we always execute the following code and work to
	* convergence.
	*/
	k=(int)(n*d)+1;
	m=2*k-1;
	h=k-n*d;
	/* printf("p_ks_new:  n = %d  k = %d  m = %d  h = %f\n",n,k,m,h); */
	H=(double*)malloc((m*m)*sizeof(double));
	Q=(double*)malloc((m*m)*sizeof(double));
	for(i=0;i<m;i++){
		for(j=0;j<m;j++){
			if(i-j+1<0){
				H[i*m+j]=0;
			} else {
				H[i*m+j]=1;
			}
		}
	}

	for(i=0;i<m;i++){
		H[i*m]-=pow(h,i+1);
		H[(m-1)*m+i]-=pow(h,(m-i));
	}

	H[(m-1)*m]+=(2*h-1>0?pow(2*h-1,m):0);
	for(i=0;i<m;i++){
		for(j=0;j<m;j++){
			if(i-j+1>0){
				for(g=1;g<=i-j+1;g++){
					H[i*m+j]/=g;
				}
			}
		}
	}

	eH=0;
	mPower(H,eH,Q,&eQ,m,n);
	/* printf("p_ks_new eQ = %d\n",eQ); */
	s=Q[(k-1)*m+k-1];
	/* printf("s = %16.8e\n",s); */
	for(i=1;i<=n;i++){
		s=s*i/n;
		/* printf("i = %d: s = %16.8e\n",i,s); */
		if(s<1e-140){
			/* printf("Oops, starting to have underflow problems: s = %16.8e\n",s); */
			s*=1e140;
			eQ-=140;
		}
	}

	/* printf("I'll bet this is it: s = %16.8e  eQ = %d\n",s,eQ); */
	s*=pow(10.,eQ);
	s = 1.0 - s;
	free(H);
	free(Q);
	return s;

}

/*
*========================================================================
* This is the Kuiper variant of KS.  It is symmetric, that is,
* it isn't biased as to where the region tested is started or stopped
* on a ring of values.  However, we simply cannot evaluate the
* CDF below to the same precision that we can for the KS test above.
* For that reason this code is basically obsolete.  We'll leave it
* in for now in case somebody figures out how to evaluate the
* q_ks_kuiper() to high precision for arbitrary count but we really
* don't need it anymore unless it turns out to be faster AND precise.
*========================================================================
*/
double kstest_kuiper(double *pvalue,int count)
{

	int i;
	double y,v,vmax,vmin,csqrt;
	double p,x;

	/*
	* We start by sorting the list of pvalues.
	*/
	if(verbose == D_KSTEST || verbose == D_ALL){
		printf("# kstest_kuiper(): Computing Kuiper KS pvalue for:\n");
		for(i=0;i<count;i++){
			printf("# kstest_kuiper(): %3d    %10.5f\n",i,pvalue[i]);
		}
	}

	/*
	* This test is useless if there is only one pvalue.  In fact, it appears
	* to return a wrong answer in this case, as it cannot set BOTH vmin
	* AND vmax correctly, or so it appears.  So one solution is to just
	* return the one pvalue and skip the rest of the test.
	*/
	if(count == 1) return pvalue[0];
	gsl_sort(pvalue,1,count);

	/*
	* Here's the test.  For each (sorted) pvalue, its index is the number of
	* values cumulated to the left of it.  v is the distance between that
	* number and the straight line representing a uniform accumulation.  We
	* save the maximum AND minimum v across all cumulated samples and
	* transform it into a p-value at the end.
	*/
	if(verbose == D_KSTEST || verbose == D_ALL){
		printf("    obs       exp           v        vmin         vmax\n");
	}
	vmin = 0.0;
	vmax = 0.0;
	for(i=0;i<count;i++){
		y = (double) i/count;
		v = pvalue[i] - y;
		/* can only do one OR the other here, not AND the other. */
		if(v > vmax) {
			vmax = v;
		} else if(v < vmin) {
			vmin = v;
		}
		if(verbose == D_KSTEST || verbose == D_ALL){
			printf("%8.3f   %8.3f    %16.6e   %16.6e    %16.6e\n",pvalue[i],y,v,vmin,vmax);
		}
	}
	v = fabs(vmax) + fabs(vmin);
	csqrt = sqrt(count);
	x = (csqrt + 0.155 + 0.24/csqrt)*v;
	if(verbose == D_KSTEST || verbose == D_ALL){
		printf("Kuiper's V = %8.3f, evaluating q_ks_kuiper(%6.2f)\n",v,x);
	}
	p = q_ks_kuiper(x,count);

	if(verbose == D_KSTEST || verbose == D_ALL){
		if(p < 0.0001){
			printf("# kstest_kuiper(): Test Fails!  Visually inspect p-values:\n");
			for(i=0;i<count;i++){
				printf("# kstest_kuiper(): %3d    %10.5f\n",i,pvalue[i]);
			}
		}
	}

	return(p);

}

double q_ks_kuiper(double x,int count)
{

	uint m,msq;
	double xsq,preturn,q,q_last,p,p_last;

	/*
	* OK, Numerical Recipes screwed us even in terms of the algorithm.
	* This one includes BOTH terms.
	*   Q = 2\sum_{m=1}^\infty (4m^2x^2 - 1)exp(-2m^2x^2)
	*   P = 8x/3\sqrt{N}\sum_{m=i}^\infty m^2(4m^2x^2 - 3)
	*   Q = Q - P (and leaving off P has consistently biased us HIGH!
	* To get the infinite sum, we simply sum to double precision convergence.
	*/
	m = 0;
	q = 0.0;
	q_last = -1.0;
	while(q != q_last){
		m++;
		msq = m*m;
		xsq = x*x;
		q_last = q;
		q += (4.0*msq*xsq - 1.0)*exp(-2.0*msq*xsq);
	}
	q = 2.0*q;

	m = 0;
	p = 0.0;
	p_last = -1.0;
	while(p != p_last){
		m++;
		msq = m*m;
		xsq = x*x;
		p_last = p;
		p += msq*(4.0*msq*xsq - 3.0)*exp(-2.0*msq*xsq);
	}
	p = (8.0*x*p)/(3.0*sqrt(count));

	preturn = q - p;

	if(verbose == D_KSTEST || verbose == D_ALL){
		printf("Q_ks yields preturn = %f:  q = %f  p = %f\n",preturn,q,p);
	}
	return(preturn);

}

