/*
 * ========================================================================
 * See copyright in copyright.h and the accompanying file COPYING
 * ========================================================================
 */

/*
 *========================================================================
 * This is the Diehard Count a Stream of 1's test, rewritten from the
 * description in tests.txt on George Marsaglia's diehard site.
 *
 *     This is the COUNT-THE-1's TEST for specific bytes.        ::
 * Consider the file under test as a stream of 32-bit integers.  ::
 * From each integer, a specific byte is chosen , say the left-  ::
 * most::  bits 1 to 8. Each byte can contain from 0 to 8 1's,   ::
 * with probabilitie 1,8,28,56,70,56,28,8,1 over 256.  Now let   ::
 * the specified bytes from successive integers provide a string ::
 * of (overlapping) 5-letter words, each "letter" taking values  ::
 * A,B,C,D,E. The letters are determined  by the number of 1's,  ::
 * in that byte::  0,1,or 2 ---> A, 3 ---> B, 4 ---> C, 5 ---> D,::
 * and  6,7 or 8 ---> E.  Thus we have a monkey at a typewriter  ::
 * hitting five keys with with various probabilities::  37,56,70,::
 * 56,37 over 256. There are 5^5 possible 5-letter words, and    ::
 * from a string of 256,000 (overlapping) 5-letter words, counts ::
 * are made on the frequencies for each word. The quadratic form ::
 * in the weak inverse of the covariance matrix of the cell      ::
 * counts provides a chisquare test::  Q5-Q4, the difference of  ::
 * the naive Pearson  sums of (OBS-EXP)^2/EXP on counts for 5-   ::
 * and 4-letter cell counts.                                     ::
 *
 *                       Comment
 *
 * For the less statistically trained amongst us, the translation
 * of the above is:
 *  Generate a string of base-5 digits derived as described from
 * specific (randomly chosen) byte offsets in a random integer.
 *  Turn four and five digit base 5 numbers (created from these digits)
 * into indices and incrementally count the frequency of occurrence of
 * each index.
 *  Compute the expected value of these counts given tsamples samples,
 * and thereby (from the vector of actual vs expected counts) generate
 * a chisq for 4 and 5 digit numbers separately.  These chisq's are
 * expected, for a large number of trials, to be equal to the number of
 * degrees of freedom of the vectors, (5^5 - 1) or (5^4 - 1).  Generate
 * the mean DIFFERENCE = 2500 as the expected value of the difference
 * chisq_5 - chisq_4 and compute a pvalue based on this expected value and
 * the associated standard deviation sqrt(2*2500).
 *
 * Note:  The byte offset is systematically cycled PER tsample, so enough
 * tsamples gives one a reasonable chance of discovering "bad" offsets
 * in any exist.  How many is enough?  Difficult to say a priori -- it
 * depends on how bit the failure is and how many offsets create a
 * failure.  Play around with it.
 *
 * Note also that the code itself is a bit simpler than the stream
 * version (no need to worry about e.g. overlap or modulus 4/5 when
 * getting the next int/byte) but that it generates 4x as many rands
 * as non-overlapping stream: tsamples*psamples*5, basically.
 *
 * This test is actually LESS stringent than the stream version overall,
 * and is much closer to rgb_bitdist.  In fact, if rgb_bitdist for 8 bits
 * fails the diehard_count_1s_byte test MUST fail as it too samples all
 * the different offsets systematically.  However, as before, MOST failures
 * at 8 bits are derived from failures at smaller numbers of bits (this
 * is an assertion that can be made precise in terms of contributing
 * permutations) and again, the test is vastly less sensitive than
 * rgb_bitdist and is less senstive that diehard_count_1s_stream EXCEPT
 * that it samples more rands and might reveal problems with specific
 * offsets ignored by the stream test.  Honestly, I could fix the stream
 * test to cycle through the possible bitlevel offsets and make this
 * test completely obsolete.
 *========================================================================
 */


#include "libdieharder.h"

/*
 * Include _inline uint generator
 */
#include "static_get_bits.c"

/*
 * This table was generated using the following code fragment.
 {
   char table[256];
   table[i] = 0;
   for(j=0;j<8*sizeof(uint);j++){
     table[i] += get_int_bit(i,j);
   }
   switch(table[i]){
     case 0:
     case 1:
     case 2:
       table[i] = 0;
       break;
     case 3:
       table[i] = 1;
       break;
     case 4:
       table[i] = 2;
       break;
     case 5:
       table[i] = 3;
       break;
     case 6:
     case 7:
     case 8:
       table[i] = 4;
       break;
     default:
       fprintf(stderr,"Hahahahah\n");
       exit(0);
       break;
   }
 }
 */

char b5b[] = {
0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 1, 1, 2,
0, 0, 0, 1, 0, 1, 1, 2, 0, 1, 1, 2, 1, 2, 2, 3,
0, 0, 0, 1, 0, 1, 1, 2, 0, 1, 1, 2, 1, 2, 2, 3,
0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4,
0, 0, 0, 1, 0, 1, 1, 2, 0, 1, 1, 2, 1, 2, 2, 3,
0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4,
0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4,
1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 4,
0, 0, 0, 1, 0, 1, 1, 2, 0, 1, 1, 2, 1, 2, 2, 3,
0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4,
0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4,
1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 4,
0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4,
1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 4,
1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 4,
2, 3, 3, 4, 3, 4, 4, 4, 3, 4, 4, 4, 4, 4, 4, 4};

/*
 * The following is needed to generate the test statistic
 *
 * Vector of probabilities for each integer.
 * 37.0/256.0,56.0/256.0,70.0/256.0,56.0/256.0,37.0/256.0
 */
const double pb[]={
0.144531250,
0.218750000,
0.273437500,
0.218750000,
0.144531250};


 
/*
 * Useful macro for building 4 and 5 digit base 5 numbers from
 * base 5 digits
 */
#define LSHIFT5(old,new) (old*5 + new)

int diehard_count_1s_byte(Test **test, int irun)
{

 uint i,j,k,index5=0,index4,letter,t;
 uint boffset;
 Vtest vtest4,vtest5;
 Xtest ptest;

 /*
  * count_1s in specific bytes is straightforward after looking over
  * count_1s in a byte.  The statistic is identical; we just have to
  * cycle the offset of the bytes selected and generate 1 random uint
  * per digit.
  */

 /*
  * I'm leaving this in so the chronically bored can validate that
  * the table exists and is correctly loaded and addressable.
  */
 if(verbose == -1){
   for(i=0;i<256;i++){
     printf("%u, ",b5b[i]);
     /* dumpbits(&i,8); */
     if((i+1)%16 == 0){
       printf("\n");
     }
   }
   exit(0);
 }

 /*
  * for display only.  0 means "ignored".
  */
 test[0]->ntuple = 0;

 /*
  * This is basically a pair of parallel vtests, with a final test
  * statistic generated from their difference (somehow).  We therefore
  * create two vtests, one for four digit base 5 integers and one for
  * five digit base 5 integers, and generate their expected values for
  * test[0]->tsamples trials.
  */

 ptest.y = 2500.0;
 ptest.sigma = sqrt(5000.0);

 Vtest_create(&vtest4,625);
 vtest4.cutoff = 5.0;
 for(i=0;i<625;i++){
   j = i;
   vtest4.y[i] = test[0]->tsamples;
   vtest4.x[i] = 0.0;
   /*
    * Digitize base 5, compute expected value for THIS integer i.
    */
   /* printf("%u:  ",j); */
   for(k=0;k<4;k++){
     /*
      * Take the least significant "letter" of j in range 0-4
      */
     letter = j%5;
     /*
      * multiply by the probability of getting this letter
      */
     vtest4.y[i] *= pb[letter];
     /*
      * Right shift j to get next digit.
      */
     /* printf("%1u",letter); */
     j /= 5;
   }
   /* printf(" = %f\n",vtest4.y[i]); */
 }

 Vtest_create(&vtest5,3125);
 vtest5.cutoff = 5.0;
 for(i=0;i<3125;i++){
   j = i;
   vtest5.y[i] = test[0]->tsamples;
   vtest5.x[i] = 0.0;
   /*
    * Digitize base 5, compute expected value for THIS integer i.
    */
   for(k=0;k<5;k++){
     /*
      * Take the least significant "letter" of j in range 0-4
      */
     letter = j%5;
     /*
      * multiply by the probability of getting this letter
      */
     vtest5.y[i] *= pb[letter];
     /*
      * Right shift j to get next digit.
      */
     j /= 5;
   }
 }

 /*
  * Here is the test.  We cycle boffset through test[0]->tsamples
  */
 boffset = 0;
 for(t=0;t<test[0]->tsamples;t++){

   boffset = t%32;  /* Remember that get_bit_ntuple periodic wraps the uint */
   /*
    * Get the next five bytes and make an index5 out of them, no
    * overlap.
    */
   for(k=0;k<5;k++){
     i = get_rand_bits_uint(32, 0xFFFFFFFF, rng);
     if(verbose == D_DIEHARD_COUNT_1S_STREAM || verbose == D_ALL){
       dumpbits(&i,32);
     }
     /*
      * get next byte from the last rand we generated.
      * Bauer fix - 
      *   Cruft: j = get_bit_ntuple_from_uint(i,8,0x000000FF,boffset);
      */
     j = get_bit_ntuple_from_whole_uint(i,8,0x000000FF,boffset);
     index5 = LSHIFT5(index5,b5b[j]);
     if(verbose == D_DIEHARD_COUNT_1S_STREAM || verbose == D_ALL){
       printf("b5b[%u] = %u, index5 = %u\n",j,b5b[j],index5);
       dumpbits(&j,8);
     }
   }
   /*
    * We use modulus to throw away the sixth digit in the left-shifted
    * base 5 index value, keeping the value of the 5-digit base 5 number
    * in the range 0 to 5^5-1 or 0 to 3124 decimal.  We repeat for the
    * four digit index.  At this point we increment the counts for index4
    * and index5.  Tres simple, no?
    */
   index5 = index5%3125;
   index4 = index5%625;
   vtest4.x[index4]++;
   vtest5.x[index5]++;
 }
 /*
  * OK, all that is left now is to figure out the statistic.
  */
 if(verbose == D_DIEHARD_COUNT_1S_BYTE || verbose == D_ALL){
   for(i = 0;i<625;i++){
     printf("%u:  %f    %f\n",i,vtest4.y[i],vtest4.x[i]);
   }
   for(i = 0;i<3125;i++){
     printf("%u:  %f    %f\n",i,vtest5.y[i],vtest5.x[i]);
   }
 }
 Vtest_eval(&vtest4);
 Vtest_eval(&vtest5);
 if(verbose == D_DIEHARD_COUNT_1S_BYTE || verbose == D_ALL){
   printf("vtest4.chisq = %f   vtest5.chisq = %f\n",vtest4.chisq,vtest5.chisq);
 }
 ptest.x = vtest5.chisq - vtest4.chisq;

 Xtest_eval(&ptest);
 test[0]->pvalues[irun] = ptest.pvalue;

 MYDEBUG(D_DIEHARD_COUNT_1S_BYTE) {
   printf("# diehard_count_1s_byte(): test[0]->pvalues[%u] = %10.5f\n",irun,test[0]->pvalues[irun]);
 }


 Vtest_destroy(&vtest4);
 Vtest_destroy(&vtest5);

 return(0);
}

