/*
 * See copyright in copyright.h and the accompanying file COPYING
 */

/*
 *========================================================================
 * This just counts the permutations of n samples.  They should
 * occur n! times each.  We count them and do a straight chisq.
 *========================================================================
 */

#include "libdieharder.h"

#define RGB_PERM_KMAX 10
uint nperms;
double fpipi(int pi1,int pi2,int nkp);
uint rgb_permutations_k;

int rgb_permutations(Test **test,int irun)
{

 uint i,k,permindex=0,t;
 Vtest vtest;
 double *testv;
 size_t ps[4096];
 gsl_permutation** lookup;


 MYDEBUG(D_RGB_PERMUTATIONS){
   printf("#==================================================================\n");
   printf("# rgb_permutations: Debug with %u\n",D_RGB_PERMUTATIONS);
 }

 /*
  * Number of permutations.  Note that the minimum ntuple value for a
  * valid test is 2.  If ntuple is less than 2, we choose the default
  * test size as 5 (like operm5).
  */
 if(ntuple<2){
   test[0]->ntuple = 5;
 } else {
   test[0]->ntuple = ntuple;
 }
 k = test[0]->ntuple;
 nperms = gsl_sf_fact(k);

 /*
  * A vector to accumulate rands in some sort order
  */
 testv = (double *)malloc(k*sizeof(double));

 MYDEBUG(D_RGB_PERMUTATIONS){
   printf("# rgb_permutations: There are %u permutations of length k = %u\n",nperms,k);
 }

 /*
  * Create a test, initialize it.
  */
 Vtest_create(&vtest,nperms);
 vtest.cutoff = 5.0;
 for(i=0;i<nperms;i++){
   vtest.x[i] = 0.0;
   vtest.y[i] = (double) test[0]->tsamples/nperms;
 }

 MYDEBUG(D_RGB_PERMUTATIONS){
   printf("# rgb_permutations: Allocating permutation lookup table.\n");
 }
 lookup = (gsl_permutation**) malloc(nperms*sizeof(gsl_permutation*));
 for(i=0;i<nperms;i++){
   lookup[i] = gsl_permutation_alloc(k);
 }
 for(i=0;i<nperms;i++){
   if(i == 0){
     gsl_permutation_init(lookup[i]);
   } else {
     gsl_permutation_memcpy(lookup[i],lookup[i-1]);
     gsl_permutation_next(lookup[i]);
   }
 }

 MYDEBUG(D_RGB_PERMUTATIONS){
   for(i=0;i<nperms;i++){
     printf("# rgb_permutations: %u => ",i);
     gsl_permutation_fprintf(stdout,lookup[i]," %u");
     printf("\n");
   }
 }

 /*
  * We count the order permutations in a long string of samples of
  * rgb_permutation_k non-overlapping rands.  This is done by:
  *   a) Filling testv[] with rgb_permutation_k rands.
  *   b) Using gsl_sort_index to generate the permutation index.
  *   c) Incrementing a counter for that index (a-c done tsamples times)
  *   d) Doing a straight chisq on the counter vector with nperms-1 DOF
  *
  * This test should be done with tsamples > 30*nperms, easily met for
  * reasonable rgb_permutation_k
  */
 for(t=0;t<test[0]->tsamples;t++){
   /*
    * To sort into a perm, test vector needs to be double.
    */
   for(i=0;i<k;i++) {
     testv[i] = (double) gsl_rng_get(rng);
     MYDEBUG(D_RGB_PERMUTATIONS){
       printf("# rgb_permutations: testv[%u] = %u\n",i,(uint) testv[i]);
     }
   }

   gsl_sort_index(ps,testv,1,k);

   MYDEBUG(D_RGB_PERMUTATIONS){
     for(i=0;i<k;i++) {
       printf("# rgb_permutations: ps[%u] = %lu\n",i,ps[i]);
     }
   }

   for(i=0;i<nperms;i++){
     if(memcmp(ps,lookup[i]->data,k*sizeof(size_t))==0){
       permindex = i;
       MYDEBUG(D_RGB_PERMUTATIONS){
         printf("# Found permutation: ");
         gsl_permutation_fprintf(stdout,lookup[i]," %u");
         printf(" = %u\n",i);
       }
       break;
     }
   }

   vtest.x[permindex]++;
   MYDEBUG(D_RGB_PERMUTATIONS){
     printf("# rgb_permutations: Augmenting vtest.x[%u] = %f\n",permindex,vtest.x[permindex]);
   }

 }

 MYDEBUG(D_RGB_PERMUTATIONS){
   printf("# rgb_permutations:==============================\n");
   printf("# rgb_permutations: permutation count = \n");
   for(i=0;i<nperms;i++){
     printf("# count[%u] = %u\n",i,(uint) vtest.x[i]);
   }
 }

 Vtest_eval(&vtest);
 test[0]->pvalues[irun] = vtest.pvalue;
 MYDEBUG(D_RGB_PERMUTATIONS) {
   printf("# rgb_permutations(): test[0]->pvalues[%u] = %10.5f\n",irun,test[0]->pvalues[irun]);
 }

 for(i=0;i<nperms;i++){
   gsl_permutation_free(lookup[i]);
 }
 free(lookup);
 free(testv);
 Vtest_destroy(&vtest);

 return(0);

}

