/*
 * ========================================================================
 * See copyright in copyright.h and the accompanying file COPYING
 * ========================================================================
 */

/*
 * ========================================================================
 * This is the Diehard BINARY RANK 31x31 test, rewritten from the 
 * description in tests.txt on George Marsaglia's diehard site.
 *
 * This is the BINARY RANK TEST for 31x31 matrices. The leftmost ::
 * 31 bits of 31 random integers from the test sequence are used ::
 * to form a 31x31 binary matrix over the field {0,1}. The rank  ::
 * is determined. That rank can be from 0 to 31, but ranks< 28   ::
 * are rare, and their counts are pooled with those for rank 28. ::
 * Ranks are found for 40,000 such random matrices and a chisqua-::
 * re test is performed on counts for ranks 31,30,29 and <=28.   ::
 *
 *                          Comments
 * ========================================================================
 */


#include "libdieharder.h"

/*
 * Include _inline uint generator
 */
#include "static_get_bits.c"

int diehard_rank_32x32(Test **test, int irun)
{

 int i,t,rank;
 uint bitstring;
 /* uint mtx[32][1]; */
 uint **mtx;
 Vtest vtest;

 /*
  * for display only.  0 means "ignored".
  */
 test[0]->ntuple = 0;

 mtx=(uint **)malloc(32*sizeof(uint*));
 for(i=0;i<32;i++){
   mtx[i] = (uint*)malloc(sizeof(uint));
 }

 MYDEBUG(D_DIEHARD_RANK_32x32){
   fprintf(stdout,"# diehard_rank_32x32(): Starting test\n");
 }

 Vtest_create(&vtest,33);
 vtest.cutoff = 5.0;
 for(i=0;i<29;i++){
   vtest.x[0] = 0.0;
   vtest.y[0] = 0.0;
 }

 /*
  * David Bauer contributes -- 
  * From "On the Rank of Random Matrices":
  * 0.2887880951, 0.5775761902, 0.1283502645,
  * 0.0052387863, 0.0000465670, 0.0000000969
  *
  * rgb continues -- 
  * An interesting question is -- should we not bother to pool
  * and include all six of these terms?  That would in principle
  * permit the number of tsamples to be cranked up without distorting
  * the final p distribution from the chisq...
  */
 vtest.x[29] = 0.0;
 vtest.y[29] = test[0]->tsamples*0.0052854502e+00;
 vtest.x[30] = 0.0;
 vtest.y[30] = test[0]->tsamples*0.1283502644e+00;
 vtest.x[31] = 0.0;
 vtest.y[31] = test[0]->tsamples*0.5775761902e+00;
 vtest.x[32] = 0.0;
 vtest.y[32] = test[0]->tsamples*0.2887880952e+00;

 for(t=0;t<test[0]->tsamples;t++) {

   MYDEBUG(D_DIEHARD_RANK_32x32){
     fprintf(stdout,"# diehard_rank_32x32(): Input random matrix = \n");
   }

   for(i=0;i<32;i++){
     MYDEBUG(D_DIEHARD_RANK_32x32){
       fprintf(stdout,"# ");
     }

     bitstring = get_rand_bits_uint(32,0xffffffff,rng);
     mtx[i][0] = bitstring;

     MYDEBUG(D_DIEHARD_RANK_32x32){
       dumpbits(mtx[i],32);
       fprintf(stdout,"\n");
     }
   }

   /*
    * This is a silly thing to quiet gcc complaints about twin puns.
    */
   rank = binary_rank(mtx,32,32);
   MYDEBUG(D_DIEHARD_RANK_32x32){
     fprintf(stdout,"# binary rank = %d\n",rank);
   }

   if(rank <= 29){
     vtest.x[29]++;
   } else {
     vtest.x[rank]++;
   }
 }

 /* for(i=0;i<33;i++) printf("vtest.x[%d] =  %f\n",i,vtest.x[i]); */

 Vtest_eval(&vtest);
 test[0]->pvalues[irun] = vtest.pvalue;
 MYDEBUG(D_DIEHARD_RANK_32x32) {
   printf("# diehard_rank_32x32(): test[0]->pvalues[%u] = %10.5f\n",irun,test[0]->pvalues[irun]);
 }

 Vtest_destroy(&vtest);

 for(i=0;i<32;i++){
   free(mtx[i]);
 }
 free(mtx);

 return(0);

}

