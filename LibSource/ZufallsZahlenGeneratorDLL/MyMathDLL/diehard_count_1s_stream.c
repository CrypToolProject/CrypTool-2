/*
 * See copyright in copyright.h and the accompanying file COPYING
 */

/*
 *========================================================================
 * This is the Diehard Count a Stream of 1's test, rewritten from the
 * description in tests.txt on George Marsaglia's diehard site.
 *
 *     This is the COUNT-THE-1's TEST on a stream of bytes.      ::
 * Consider the file under test as a stream of bytes (four per   ::
 * 32 bit integer).  Each byte can contain from 0 to 8 1's,      ::
 * with probabilities 1,8,28,56,70,56,28,8,1 over 256.  Now let  ::
 * the stream of bytes provide a string of overlapping  5-letter ::
 * words, each "letter" taking values A,B,C,D,E. The letters are ::
 * determined by the number of 1's in a byte::  0,1,or 2 yield A,::
 * 3 yields B, 4 yields C, 5 yields D and 6,7 or 8 yield E. Thus ::
 * we have a monkey at a typewriter hitting five keys with vari- ::
 * ous probabilities (37,56,70,56,37 over 256).  There are 5^5   ::
 * possible 5-letter words, and from a string of 256,000 (over-  ::
 * lapping) 5-letter words, counts are made on the frequencies   ::
 * for each word.   The quadratic form in the weak inverse of    ::
 * the covariance matrix of the cell counts provides a chisquare ::
 * test::  Q5-Q4, the difference of the naive Pearson sums of    ::
 * (OBS-EXP)^2/EXP on counts for 5- and 4-letter cell counts.    ::
 *
 *                       Comment
 *
 * For the less statistically trained amongst us, the translation
 * of the above is:
 *  Generate a string of base-5 digits derived as described from
 * random bytes (where we by default generate NON-overlapping bytes
 * but overlapping ones can be selected by setting the overlap flag
 * on the command line).
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
 * This test is written so overlap can be flag-controlled and so that
 * tsamples can be freely varied compared to diehard's presumed 256,000.
 * As usual, it also runs at 100 times and not 10 to generate the final
 * KS pvalue for the test.
 *
 * One can easily prove that this test will fail whenever rgb_bitdist
 * fails at 40 bits and pass when rgb_bitdist passes, as rgb_bitdist
 * tests the much more stringent requirement that the actual underlying
 * 40 bit strings are correctly distributed not just with respect to
 * the morphed number of 1 bits but with respect to the actual underlying
 * bit patterns.  It is, however, vastly less sensitive -- rgb_bitdist
 * already FAILS at six bit strings for every generator thus far tested,
 * meaning that two OCTAL digits WITHOUT any transformations or bit counts
 * are already incorrectly distributed.  It is then perfectly obvious that
 * all bitstrings with higher numbers of bits will be incorrectly
 * distributed as well (including 40 bit strings), but count_the_1s is
 * distressingly insensitive to this embedded but invisible failure.
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

char b5s[] = {
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
 * The following are needed to generate the test statistic.
 * Note that sqrt(5000) = 70.710678118654752440084436210485
 */
const double mu=2500, std=70.7106781;

/*
 * Vector of probabilities for each integer.  (All exact, btw.)
 * 37.0/256.0,56.0/256.0,70.0/256.0,56.0/256.0,37.0/256.0
 */
const double ps[]={
0.144531250,
0.218750000,
0.273437500,
0.218750000,
0.144531250};

#define LSHIFT5(old,new) (old*5 + new)

int diehard_count_1s_stream(Test **test, int irun)
{

 uint i,j,k,index5=0,index4,letter,t;
 uint boffset;
 Vtest vtest4,vtest5;
 Xtest ptest;
 uint overlap = 1; /* leftovers/cruft */

 /*
  * Count a Stream of 1's is a very complex way of generating a statistic.
  * We take a random stream, and turn it bytewise (overlapping or not)
  * into a 4 and/or 5 digit base 5 integer (in the ranges 0-624 and 0-3124
  * respectively) via the bytewise mapping in b5[] above, derived from the
  * prescription of Marsaglia in diehard.  Increment a vector for 4 and 5
  * digit numbers separately that counts the number of times that 4, 5
  * digit integer has occurred in the random stream.  Compare these
  * vectors to their expected values, generated from the probabilities of
  * the occurrence of each base 5 integer in the byte map.  Compute chisq
  * for each of these vectors -- the "degrees of freedom" of the stream
  * thus mapped..  The difference between chisq(5) and chisq(4)should be
  * 5^4 - 5^3 = 2500 with stddev sqrt(5000), use this in an Xtest to
  * generate the final trial statistic.
  */

 /*
  * I'm leaving this in so the chronically bored can validate that
  * the table exists and is correctly loaded and addressable.
  */
 if(verbose == -1){
   for(i=0;i<256;i++){
     printf("%u, ",b5s[i]);
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
     vtest4.y[i] *= ps[letter];
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
     vtest5.y[i] *= ps[letter];
     /*
      * Right shift j to get next digit.
      */
     j /= 5;
   }
 }

 /*
  * Preload index with the four bytes of the first rand if overlapping
  * only.
  */
 if(overlap){
   i = get_rand_bits_uint(32, 0xFFFFFFFF, rng);
   MYDEBUG(D_DIEHARD_COUNT_1S_STREAM){
     dumpbits(&i,32);
   }
   /* 1st byte */
   /*
    * Bauer fix.  I don't think he likes my getting ntuples sequentially
    * from the stream in diehard tests, which is fair enough...;-)
    * In the raw bit distribution tests, however, it is essential.
    * j = get_bit_ntuple_from_uint(i,8,0x000000FF,0);
    */
   j = get_bit_ntuple_from_whole_uint(i,8,0x000000FF,0);
   index5 = b5s[j];
   if(verbose == D_DIEHARD_COUNT_1S_STREAM || verbose == D_ALL){
     printf("b5s[%u] = %u, index5 = %u\n",j,b5s[j],index5);
     dumpbits(&j,8);
   }
   /* 2nd byte */
   /* Cruft:  See above.  j = get_bit_ntuple_from_uint(i,8,0x000000FF,8); */
   j = get_bit_ntuple_from_whole_uint(i,8,0x000000FF,8);
   index5 = LSHIFT5(index5,b5s[j]);
   if(verbose == D_DIEHARD_COUNT_1S_STREAM || verbose == D_ALL){
     printf("b5s[%u] = %u, index5 = %u\n",j,b5s[j],index5);
     dumpbits(&j,8);
   }
   /* 3rd byte */
   /* Cruft:  See above. j = get_bit_ntuple_from_uint(i,8,0x000000FF,16); */
   j = get_bit_ntuple_from_whole_uint(i,8,0x000000FF,16);
   index5 = LSHIFT5(index5,b5s[j]);
   if(verbose == D_DIEHARD_COUNT_1S_STREAM || verbose == D_ALL){
     printf("b5s[%u] = %u, index5 = %u\n",j,b5s[j],index5);
     dumpbits(&j,8);
   }
   /* 4th byte */
   /* Cruft:  See above. j = get_bit_ntuple_from_uint(i,8,0x000000FF,24); */
   j = get_bit_ntuple_from_whole_uint(i,8,0x000000FF,24);
   index5 = LSHIFT5(index5,b5s[j]);
   if(verbose == D_DIEHARD_COUNT_1S_STREAM || verbose == D_ALL){
     printf("b5s[%u] = %u, index5 = %u\n",j,b5s[j],index5);
     dumpbits(&j,8);
   }
 }

 boffset = 0;
 for(t=0;t<test[0]->tsamples;t++){

   /*
    * OK, we now have to do a simple modulus decision as to whether or not
    * a new random uint is required AND to track the byte offset.  We also
    * have to determine whether or not to use overlapping bytes.  I
    * actually think that we may need to turn index5 into a subroutine
    * where each successive call returns the next morphed index5,
    * overlapping or not.
    */
   if(overlap){
     /*
      * Use overlapping bytes to generate the next index5 according to
      * the diehard prescription (designed to work with a very small
      * input file of rands).
      */
     if(boffset%32 == 0){
       /*
        * We need a new rand to get our next byte.
        */
       boffset = 0;
       i = get_rand_bits_uint(32, 0xFFFFFFFF, rng);
       if(verbose == D_DIEHARD_COUNT_1S_STREAM || verbose == D_ALL){
         dumpbits(&i,32);
       }
     }
     /*
      * get next byte from the last rand we generated.
      */
     /* Cruft:  See above.  j = get_bit_ntuple_from_uint(i,8,0x000000FF,boffset); */
     j = get_bit_ntuple_from_whole_uint(i,8,0x000000FF,boffset);
     index5 = LSHIFT5(index5,b5s[j]);
     /*
      * I THINK that this basically throws away the sixth digit in the
      * left-shifted base 5 value, keeping the value of the 5-digit base 5
      * number in the range 0 to 5^5-1 or 0 to 3124 decimal.
      */
     index5 = index5%3125;
     if(verbose == D_DIEHARD_COUNT_1S_STREAM || verbose == D_ALL){
       printf("b5s[%u] = %u, index5 = %u\n",j,b5s[j],index5);
       dumpbits(&j,8);
     }
     boffset+=8;
   } else {
     /*
      * Get the next five bytes and make an index5 out of them, no
      * overlap.
      */
     for(k=0;k<5;k++){
       if(boffset%32 == 0){
         /*
	  * We need a new rand to get our next byte.
	  */
         boffset = 0;
         i = get_rand_bits_uint(32, 0xFFFFFFFF, rng);
         if(verbose == D_DIEHARD_COUNT_1S_STREAM || verbose == D_ALL){
           dumpbits(&i,32);
         }
       }
       /*
        * get next byte from the last rand we generated.
        */
       /* Cruft:  See above. j = get_bit_ntuple_from_uint(i,8,0x000000FF,boffset); */
       j = get_bit_ntuple_from_whole_uint(i,8,0x000000FF,boffset);
       index5 = LSHIFT5(index5,b5s[j]);
       if(verbose == D_DIEHARD_COUNT_1S_STREAM || verbose == D_ALL){
         printf("b5s[%u] = %u, index5 = %u\n",j,b5s[j],index5);
         dumpbits(&j,8);
       }
       boffset+=8;
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
 if(verbose == D_DIEHARD_COUNT_1S_STREAM || verbose == D_ALL){
   for(i = 0;i<625;i++){
     printf("%u:  %f    %f\n",i,vtest4.y[i],vtest4.x[i]);
   }
   for(i = 0;i<3125;i++){
     printf("%u:  %f    %f\n",i,vtest5.y[i],vtest5.x[i]);
   }
 }
 Vtest_eval(&vtest4);
 Vtest_eval(&vtest5);
 MYDEBUG(D_DIEHARD_COUNT_1S_STREAM) {
   printf("vtest4.chisq = %f   vtest5.chisq = %f\n",vtest4.chisq,vtest5.chisq);
 }
 ptest.x = vtest5.chisq - vtest4.chisq;

 Xtest_eval(&ptest);
 test[0]->pvalues[irun] = ptest.pvalue;

 MYDEBUG(D_DIEHARD_COUNT_1S_STREAM) {
   printf("# diehard_count_1s_stream(): test[0]->pvalues[%u] = %10.5f\n",irun,test[0]->pvalues[irun]);
 }

 Vtest_destroy(&vtest4);
 Vtest_destroy(&vtest5);

 return(0);

}

