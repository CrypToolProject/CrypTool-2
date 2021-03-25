/*
 * See copyright in copyright.h and the accompanying file COPYING
 */

/*
 *========================================================================
 * This is the Diehard BITSTREAM test, rewritten from the description
 * in tests.txt on George Marsaglia's diehard site.
 *
 *                   THE BITSTREAM TEST                          ::
 * The file under test is viewed as a stream of bits. Call them  ::
 * b1,b2,... .  Consider an alphabet with two "letters", 0 and 1 ::
 * and think of the stream of bits as a succession of 20-letter  ::
 * "words", overlapping.  Thus the first word is b1b2...b20, the ::
 * second is b2b3...b21, and so on.  The bitstream test counts   ::
 * the number of missing 20-letter (20-bit) words in a string of ::
 * 2^21 overlapping 20-letter words.  There are 2^20 possible 20 ::
 * letter words.  For a truly random string of 2^21+19 bits, the ::
 * number of missing words j should be (very close to) normally  ::
 * distributed with mean 141,909 and sigma 428.  Thus            ::
 *  (j-141909)/428 should be a standard normal variate (z score) ::
 * that leads to a uniform [0,1) p value.  The test is repeated  ::
 * twenty times.                                                 ::
 *
 * Except that in dieharder, the test is not run 20 times, it is
 * run a user-selectable number of times, default 100.
 *========================================================================
 *                       NOTE WELL!
 * If you use non-overlapping samples, sigma is 290, not 428.  This
 * makes sense -- overlapping samples aren't independent and so you
 * have fewer "independent" samples than you think you do, and
 * the variance is consequently larger.
 *========================================================================
 */


#include "libdieharder.h"

/*
 * Include _inline uint generator
 */
#include "static_get_bits.c"

int diehard_bitstream(Test **test, int irun)
{

 uint i,j,t,boffset,coffset;
 Xtest ptest;
 char *w;
 uint *bitstream,w20,wscratch,newbyte;
 unsigned char *cbitstream = 0;
 uint overlap = 1;  /* Leftovers/Cruft */

 /*
  * for display only.  0 means "ignored".
  */
 test[0]->ntuple = 0;

 /*
  * p = 141909, with sigma 428, for test[0]->tsamples = 2^21 20 bit ntuples.
  * a.k.a. the number of 20 bit integers missing from 2^21 random
  * samples drawn from this field.  At some point, I should be able
  * to figure out the expected value for missing integers as a rigorous
  * function of the size of the field sampled and number of samples drawn
  * and hence make this test capable of being run with variable sample
  * sizes, but at the moment I cannot do this and so test[0]->tsamples cannot be
  * varied.  Hence we work with diehard's values and hope that they are
  * correct.
  *
  * ptest.x = number of "missing ntuples" given 2^21 trials
  * ptest.y = 141909
  *
  * for non-overlapping samples we need (2^21)*5/8 = 1310720 uints, but
  * for luck we add one as we'd hate to run out.  For overlapping samples,
  * we need 2^21 BITS or 2^18 = 262144 uints, again plus one to be sure
  * we don't run out.
  */
#define BS_OVERLAP 262146
#define BS_NO_OVERLAP 1310722
 ptest.y = 141909;
 if(overlap){
   ptest.sigma = 428.0;
   bitstream = (uint *)malloc(BS_OVERLAP*sizeof(uint));
   for(i = 0; i < BS_OVERLAP; i++){
     bitstream[i] = get_rand_bits_uint(32,0xffffffff,rng);
   }
   MYDEBUG(D_DIEHARD_BITSTREAM) {
     printf("# diehard_bitstream: Filled bitstream with %u rands for overlapping\n",BS_OVERLAP);
     printf("# diehard_bitstream: samples.  Target is mean 141909, sigma = 428.\n");
   }
 } else {
   ptest.sigma = 290.0;
   bitstream = (uint *)malloc(BS_NO_OVERLAP*sizeof(uint));
   for(i = 0; i < BS_NO_OVERLAP; i++){
     bitstream[i] = get_rand_bits_uint(32,0xffffffff,rng);
   }
   cbitstream = (unsigned char *)bitstream;   /* To allow us to access it by bytes */
   MYDEBUG(D_DIEHARD_BITSTREAM) {
     printf("# diehard_bitstream: Filled bitstream with %u rands for non-overlapping\n",BS_NO_OVERLAP);
     printf("# diehard_bitstream: samples.  Target is mean 141909, sigma = 290.\n");
   }
 }

 /*
  * We now make test[0]->tsamples measurements, as usual, to generate the
  * missing statistic.  The easiest way to proceed is to just increment a
  * vector of length 2^20 using the generated ntuples as the indices of
  * the slot being incremented.  Then we zip through the vector counting
  * the remaining zeros.  This is horribly nonlocal but then, these ARE
  * random numbers, right?
  */

 w = (char *)malloc(M*sizeof(char));
 memset(w,0,M*sizeof(char));

 MYDEBUG(D_DIEHARD_BITSTREAM) {
   printf("# diehard_bitstream: w[] (counter vector) is allocated and zeroed\n");
 }

 i = 0;
 wscratch = bitstream[i++];     /* Get initial uint into wscratch */
 for(t=0;t<test[0]->tsamples;t++){

   if(overlap){

     /*
      * We have to slide an overlapping 20-bit window along one bit at a
      * time to be able to use Marsaglia's sigma of 428.  We do this by
      * taking two scratch uints and pulling a left, right trick to get
      * the desired window for 8 returns, then shifting left a byte for
      * four bytes, advancing to the next full uint in bitstream[] on
      * the boundary.  Yuk, but what can one do?
      */
     coffset = (t%32)/8;     /* next byte to shift in from bitstream, 0,1,2 or 3 */
     boffset = t%8;          /* bit offset of w20 in wscratch */
     i = t/32 + 1;           /* bitstream index of uint from which we draw next byte */
     /* printf("t = %u, coffset = %u\n",t,coffset); */
     if(boffset == 0) {      /* Get a new byte */
       wscratch = wscratch << 8;  /* make room for next byte */
       /*
       printf("# diehard_bitstream: left shift 8 wscratch = ");
       dumpuintbits(&wscratch, 1);
       printf("\n");
       */
       newbyte = bitstream[i] << (8*coffset);
       /*
       printf("# diehard_bitstream: left shift %u newbyte = ",8*coffset);
       dumpuintbits(&newbyte, 1);
       printf("\n");
       */
       newbyte = newbyte >> 24;
       /*
       printf("# diehard_bitstream: newbyte = ");
       dumpuintbits(&newbyte, 1);
       printf("\n");
       */
       wscratch += newbyte;
     }
     /*
     MYDEBUG(D_DIEHARD_BITSTREAM) {
       printf("# diehard_bitstream: wscratch = ");
       dumpuintbits(&wscratch, 1);
       printf("\n");
     }
     */
     w20 = ((wscratch << boffset) >> 12);
     MYDEBUG(D_DIEHARD_BITSTREAM) {
       printf("# diehard_bitstream: w20 = ");
       dumpuintbits(&w20, 1);
       printf("\n");
     }
     w[w20]++;

   } else {

     /*
      * For non-overlapping samples, easiest way is to just get 2.5 bytes
      * at a time and keep track of a byte index into bitstream,
      * cbitstream.  Then things are actually pretty straightforward.
      */
     MYDEBUG(D_DIEHARD_BITSTREAM) {
       printf("# diehard_bitstream: Non-overlapping t = %u, i = %u\n",t,i);
     }
     if(t%2 == 0){
       w20 = 0;  /* Start with window clear, of course... */
       for(j=0;j<2;j++){          /* Get two bytes */
         w20 = w20 << 8;          /* Does nothing on first call */
         w20 += cbitstream[i];  /* Shift in each byte */
         MYDEBUG(D_DIEHARD_BITSTREAM) {
           printf("# diehard_bitstream: i = %u  cb = %u w20 = ",i,cbitstream[i]);
           dumpuintbits(&w20, 1);
           printf("\n");
         }
	 i++;
       }
       wscratch = (uint) (cbitstream[i] >> 4);    /* Get first 4 bits of next byte */
       MYDEBUG(D_DIEHARD_BITSTREAM) {
         printf("# diehard_bitstream: wscratch = ");
         dumpuintbits(&wscratch, 1);
         printf("\n");
       }
       w20 = (w20 << 4) + wscratch;               /* Gets evens */
       MYDEBUG(D_DIEHARD_BITSTREAM) {
         printf("# diehard_bitstream: w20 = ");
         dumpuintbits(&w20, 1);
         printf("\n");
       }
     } else {
       wscratch = (uint) cbitstream[i];
       MYDEBUG(D_DIEHARD_BITSTREAM) {
         printf("# diehard_bitstream: i = %u  wscratch = ",i);
         dumpuintbits(&wscratch, 1);
         printf("\n");
       }
       w20 = wscratch & 0x0000000F ; /* Get last 4 bits of next byte */
       MYDEBUG(D_DIEHARD_BITSTREAM) {
         printf("# diehard_bitstream: i = %u  w20 = ",i);
         dumpuintbits(&w20, 1);
         printf("\n");
       }
       i++;
       for(j=0;j<2;j++){          /* Get two more bytes */
         w20 = w20 << 8;
         w20 += cbitstream[i];  /* Shift in each byte */
         MYDEBUG(D_DIEHARD_BITSTREAM) {
           printf("# diehard_bitstream: i = %u  w20 = ",i);
           dumpuintbits(&w20, 1);
           printf("\n");
         }
	 i++;
       }
       MYDEBUG(D_DIEHARD_BITSTREAM) {
         printf("# diehard_bitstream: w20 = ");
         dumpuintbits(&w20, 1);
         printf("\n");
       }
     }
     w[w20]++;

   }
 }

 /*
  * Now we count the holes, so to speak
  */
 ptest.x = 0;
 for(i=0;i<M;i++){
   if(w[i] == 0){
     ptest.x++;
     /* printf("ptest.x = %f  Hole: w[%u] = %u\n",ptest.x,i,w[i]); */
   }
 }
 if(verbose == D_DIEHARD_BITSTREAM || verbose == D_ALL){
   printf("%f %f %f\n",ptest.y,ptest.x,ptest.x-ptest.y);
 }
 /*
  * I used this to prove that sigma = 288.6
  * So while it is cruft, let's leave it in case anybody else wants
  * to make a histogram and fit a normal and check.
 printf("%f\n",ptest.x);
  */

 Xtest_eval(&ptest);
 test[0]->pvalues[irun] = ptest.pvalue;

 MYDEBUG(D_DIEHARD_BITSTREAM) {
   printf("# diehard_bitstream(): test[0]->pvalues[%u] = %10.5f\n",irun,test[0]->pvalues[irun]);
 }

 /*
  * Don't forget to free or we'll leak.  Hate to have to wear
  * depends...
  */
 nullfree(w);
 nullfree(bitstream);

 return(0);

}

