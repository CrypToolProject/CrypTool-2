/*
 * ========================================================================
 * See copyright in copyright.h and the accompanying file COPYING
 * ========================================================================
 */

/*
 *========================================================================
 * This is the Diehard OPERM5 test, rewritten from the description
 * in tests.txt on George Marsaglia's diehard site.
 *
 *          THE OVERLAPPING 5-PERMUTATION TEST                   ::
 * This is the OPERM5 test.  It looks at a sequence of one mill- ::
 * ion 32-bit random integers.  Each set of five consecutive     ::
 * integers can be in one of 120 states, for the 5! possible or- ::
 * derings of five numbers.  Thus the 5th, 6th, 7th,...numbers   ::
 * each provide a state. As many thousands of state transitions  ::
 * are observed,  cumulative counts are made of the number of    ::
 * occurences of each state.  Then the quadratic form in the     ::
 * weak inverse of the 120x120 covariance matrix yields a test   ::
 * equivalent to the likelihood ratio test that the 120 cell     ::
 * counts came from the specified (asymptotically) normal dis-   ::
 * tribution with the specified 120x120 covariance matrix (with  ::
 * rank 99).  This version uses 1,000,000 integers, twice.       ::
 *
 * Note -- the original diehard test almost certainly had errors,
 * as did the documentation.  For example, the actual rank is
 * 5!-4!=96, not 99.  The original dieharder version validated
 * against the c port of dieharder to give the same answers from
 * the same data, but failed gold-standard generators such as AES
 * or the XOR supergenerator with AES and several other top rank
 * generators.  Frustration with trying to fix the test with very
 * little useful documentation caused me to eventually write
 * the rgb permutations test, which uses non-overlapping samples
 * (and hence avoids the covariance problem altogether) and can
 * be used for permutations of other than 5 integers.  I was able
 * to compute the covariance matrix for the problem, but was unable
 * break it down into the combination of R, S and map that Marsaglia
 * used, and I wanted to (if possible) use the GSL permutations
 * routines to count/index the permutations, which yield a different
 * permutation index from Marsaglia's (adding to the problem).
 *
 * Fortunately, Stephen Moenkehues (moenkehues@googlemail.com) was
 * bored and listless and annoyed all at the same time while using
 * dieharder to test his SWIFFTX rng, a SHA3-candidate and fixed
 * diehard_operm5.  His fix avoids the R, S and map -- he too went
 * the route of directly computing the correlation matrix but he
 * figured out how to transform the correlation matrix plus the
 * counts from a run directly into the desired statistic (a thing
 * that frustrated me in my own previous attempts) and now it works!
 * He even made it work (correctly) in overlapping and non-overlapping
 * versions, so one can invoke dieharder with the -L 1 option and run
 * what should be the moral equivalent of the rgb permutation test at
 * -n 5!
 *
 * So >>thank you<< Stephen!  Thank you Open Source development
 * process!  Thank you Ifni, Goddess of Luck and Numbers!  And anybody
 * who wants to tackle the remaining diehard "problem" tests, (sums in
 * particular) should feel free to play through...
 *========================================================================
 */


#include "libdieharder.h"

static int tflag=0;
static double tcount[120];

/*
* kperm computes the permutation number of a vector of five integers
* passed to it.
*/
int kperm(uint v[],uint voffset)
{

 int i,j,k,max;
 int w[5];
 int pindex,uret,tmp;

 /*
  * work on a copy of v, not v itself in case we are using
  * overlapping 5-patterns.
  */
 for(i=0;i<5;i++){
   j = (i+voffset)%5;
   w[i] = v[j];
 }

 if(verbose == -1){
   printf("==================================================================\n");
   printf("%10u %10u %10u %10u %10u\n",w[0],w[1],w[2],w[3],w[4]);
   printf(" Permutations = \n");
 }

 pindex = 0;
 for(i=4;i>0;i--){
   max = w[0];
   k = 0;
   for(j=1;j<=i;j++){
     if(max <= w[j]){
       max = w[j];
       k = j;
     }
   }
   pindex = (i+1)*pindex + k;
   tmp = w[i];
   w[i] = w[k];
   w[k] = tmp;
   if(verbose == -1){
     printf("%10u %10u %10u %10u %10u\n",w[0],w[1],w[2],w[3],w[4]);
   }
 }

 uret = pindex;

 if(verbose == -1){
   printf(" => %u\n",pindex);
 }

 return uret;

}

int diehard_operm5(Test **test, int irun)
{

 int i,j,kp,t,vind;
 uint v[5];
 double count[120];
 double av,norm,x[120],chisq,ndof;

 /*
  * Zero count vector, was t(120) in diehard.f90.
  */
 for(i=0;i<120;i++) {
   count[i] = 0.0;
   if(tflag == 0){
     tcount[i] = 0.0;
     tflag = 1;
   }
 }

 if(overlap){
   for(i=0;i<5;i++){
     v[i] = gsl_rng_get(rng);
   }
   vind = 0;
 } else {
   for(i=0;i<5;i++){
     v[i] = gsl_rng_get(rng);
   }
 }

 for(t=0;t<test[0]->tsamples;t++){

   /*
    * OK, now we are ready to generate a list of permutation indices.
    * Basically, we take a vector of 5 integers and transform it into a
    * number with the kperm function.  We will use the overlap flag to
    * determine whether or not to refill the entire v vector or just
    * rotate bytes.
    */
  if(overlap){
    kp = kperm(v,vind);
    count[kp] += 1;
    v[vind] = gsl_rng_get(rng);
    vind = (vind+1)%5;
  } else {
    for(i=0;i<5;i++){
      v[i] = gsl_rng_get(rng);
    }
    kp = kperm(v,0);
    count[kp] += 1;
  }
 }

 for(i=0;i<120;i++){
   tcount[i] += count[i];
   /* printf("%u: %f\n",i,tcount[i]); */
 }

 chisq = 0.0;
 av = test[0]->tsamples/120.0;
 norm = test[0]->tsamples; // this belongs to the pseudoinverse
 /*
  * The pseudoinverse P of the covariancematrix C is computed for n = 1.
  * If n = 100 the new covariancematrix is C_100 = 100*C. Therefore the
  * new pseudoinverse is P_100 = (1/100)*P.  You can see this from the
  * equation C*P*C = C
  */
	
 if(overlap==0){
   norm = av;
 }
 for(i=0;i<120;i++){
   x[i] = count[i] - av;
 }

 if(overlap){
   for(i=0;i<120;i++){
     for(j=0;j<120;j++){
       chisq = chisq + x[i]*pseudoInv[i][j]*x[j];
     }
   }
 }

 if(overlap==0){
   for(i=0;i<120;i++){
     chisq = chisq + x[i]*x[i];
   }
 }

 if(verbose == -2){
   printf("norm = %10.2f, av = %10.2f",norm,av);
   for(i=0;i<120;i++){
     printf("count[%u] = %4.0f; x[%u] = %3.2f ",i,count[i],i,x[i]);
     if((i%2)==0){printf("\n");}
   }
   if((chisq/norm) >= 0){
     printf("\n\nchisq/norm: %10.5f :-) and chisq: %10.5f\n",(chisq/norm), chisq);
   }
 }
	
 if((chisq/norm) < 0){
   printf("\n\nCHISQ NEG.! chisq/norm: %10.5f and chisq: %10.5f",(chisq/norm), chisq);
 }
	
 chisq = fabs(chisq / norm);

 ndof = 96; /* the rank of the covariancematrix and the pseudoinverse */
 if(overlap == 0){
   ndof = 120-1;
 }

 MYDEBUG(D_DIEHARD_OPERM5){
   printf("# diehard_operm5(): chisq[%u] = %10.5f\n",kspi,chisq);
 }

 test[0]->pvalues[irun] = gsl_sf_gamma_inc_Q((double)(ndof)/2.0,chisq/2.0);

 MYDEBUG(D_DIEHARD_OPERM5){
   printf("# diehard_operm5(): test[0]->pvalues[%u] = %10.5f\n",irun,test[0]->pvalues[irun]);
 }

 kspi++;

 return(0);

}

