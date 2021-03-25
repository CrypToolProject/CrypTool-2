/*
 * ========================================================================
 * $Id: diehard_birthdays.c 250 2006-10-10 05:02:26Z rgb $
 *
 * See copyright in copyright.h and the accompanying file COPYING
 * ========================================================================
 */

/*
 * ========================================================================
 * This is a test that checks to see if the rng generates bit patterns
 * (n-tuples) that are distributed correctly (binomially).  For example,
 * for 2-tuples (bit pairs) there are four possibilities: 00, 01, 10, 11.
 * Each should occur with a probability of 1/4, hence the count of (say) K
 * 00 bitpairs out of N trials should be distributed (over M samples)
 * according to the binomial probability p = binomial(N,K,0.25) -- that
 * is, the expected count for k 00's is M*p(N,K,0.25).
 *
 * This test should be more sensitive than just doing a large number of
 * trials and aggregating all the 00's over all the samples and comparing
 * the result to the expected mean, as there can be distributions that
 * have the expected mean of 0.25 that are >>not<< binomially distributed.
 *
 * By making a single program capable of doing any n-tuple, we gain
 * immediate benefit.  Both STS and Diehard, for example, contain
 * tests that validate bit distribution frequencies; however, they typically
 * do so for specific n-tuples, e.g. 5 in several diehard tests.  There
 * is nothing terribly special about 5, and no point in looking too
 * hard at e.g. intervals between patterns at 5, since testing pure
 * frequency at 6 bits simultaneously ensures that all 6 bit patterns have
 * the correct frequency (since they are sub-patterns of 6 bit patterns
 * that have the correct frequency, and if they had the INcorrect frequency
 * the six-bit patterns would as well by inheritance) and (less obviously,
 * but I'm pretty sure I can prove it) that intervals between the five
 * bit patterns must also be correct IF the test is satisfied robustly
 * for various arbitrary test string lengths and sampling counts.
 *
 * Anyway, I'm giving this one a shot as it may end up being a one-size
 * fits all tool for validating bitlevel randomness for any generator,
 * up to some n-tuple.  I expect that 2004/2005 computers will easily
 * be able to validate through n=8 without really warming up or taking
 * terribly long (validating the frequency and distribution for e.g.
 * the entire ascii alphabet at the byte level) and MAY be able to do
 * actual frequency validation on 16-bit integers.  Note that it only takes
 * a few seconds to generate millions of 32 bit integers with most of
 * the GSL routines.  To get statistically valid results, one needs to
 * accumulate order of 100 trials per outcome, or test order of millions
 * of integers and accumulate the results in an outcome vector 64K long.
 * both of these are well within reach using at most hours of CPU time,
 * and partitioned on a beowulf might take only minutes or even seconds.
 *
 * The latest version of the actual test (below) tests only ONE ntuple,
 * the value set in the global variable rgb_bitdist_ntuple which must
 * be a positive integer.  The calling program is responsible for e.g.
 * testing a range of ntuples.
 * ========================================================================
 */

#include "libdieharder.h"

#include "static_get_bits.c"

int rgb_bitdist(Test **test,int irun)
{

 uint nb;          /* number of bits in a tested ntuple */
 uint value_max;   /* 2^{nb}, basically (max size of nb bit word + 1) */
 uint bsamples;    /* The number of non-overlapping samples in buffer */
 uint value;       /* value of sampled ntuple (as a uint) */
 uint mask;

 /* Look for cruft below */

 uint b,t,i;   /* loop indices? */
 uint ri;
 uint *count,ctotal; /* count of any ntuple per bitstring */

 double ntuple_prob,pbin;  /* probabilities */
 Vtest *vtest;               /* A reusable vector of binomial test bins */

 /*
  * Sample a bitstring ntuple in length (exactly).
  */
 if(ntuple>0){
   /*
    * Set test[0]->ntuple to pass back to output()
    */
   test[0]->ntuple = ntuple;
   nb = ntuple;
   MYDEBUG(D_RGB_BITDIST){
     printf("# rgb_bitdist: Testing ntuple = %u\n",nb);
   }
 } else {
   fprintf(stderr,"Error:  Can only test distribution of positive ntuples.\n");
   fprintf(stderr,"        Use -n ntuple for 0 < ntuple.\n");
   fprintf(stderr,"        Read test description with dieharder -d 200 -h.\n");
   exit(0);
 }

 /*
  * The largest integer for this ntuple is 2^nb-1 (they range from 0 to
  * 2^nb - 1).  However, this is used to size count and limit loops, so
  * we use 2^nb and start indices from 0 as usual.
  */
 value_max = (uint) pow(2,nb);
 MYDEBUG(D_RGB_BITDIST){
   printf("# rgb_bitdist(): value_max = %u\n",value_max);
 }

 /*
  * This is the number of bit samples we wish to accumulate per tsample.
  * It basically determines the length of the bit string we chop up into
  * ntuples.   There is no compelling reason for it to be a power of two,
  * but why not?  Note that in the Old Days (where bsamples came from a
  * fixed string size) its value was typically e.g. 128/8 = 16 or even less.
  * We will assume the usual rule of thumb -- there need to be at least
  * 30 objects in a sample for it to "behave".  We'll double this to 64
  * as this still leaves us with "reasonable" run times.  With nb = 8 (one
  * byte) this samples 64 byte chunks of the bitstream.
  */
 bsamples = 64;

 /*
  * Allocate memory for value_max vector of Vtest structs and counts,
  * PER TEST.  Note that we must free both of these when we are done
  * or leak.
  */
 vtest = (Vtest *)malloc(value_max*sizeof(Vtest));
 count = (uint *)malloc(value_max*sizeof(uint));

 /*
  * This is the probability of getting any given ntuple.  For example,
  * for bit triples, value_max = 2^3 = 8 and each value should occur
  * with probability 1/8.
  */
 ntuple_prob = 1.0/(double)value_max;
 MYDEBUG(D_RGB_BITDIST){
   printf("# rgb_bitdist(): ntuple_prob = %f\n",ntuple_prob);
   printf("# rgb_bitdist(): Testing %u samples of %u bit strings\n",test[0]->tsamples,bits);
   printf("# rgb_bitdist():=====================================================\n");
   printf("# rgb_bitdist():            vtest table\n");
   printf("# rgb_bitdist(): Outcome   bit          x           y       sigma\n");
 }
   
 tsamples = test[0]->tsamples;

 /*
  * Set the mask for bits to be returned.  I think that I want to
  * change routines here over to the sliding window routine.
  *
  * The following mask will work OK, but John says that it won't
  * work unless nb != CHAR_BIT*sizeof(uint), so the loop is a bit
  * more robust.
  *
  *  mask = ((1u << nb) - 1);
  */
 mask = 0;
 for(i = 0; i < nb; i++){
   mask |= (1u << nb);
 }
 mask = ((1u << nb) - 1);

 /*
  * OK, I'm getting to where I think I can manage to fix this test once
  * and for all.  I want to make this test evaluate a counter:
  *   freq[ntup][irun]
  * that counts how many times ntup occurs in the irunth sample.  The
  * test is then performed on the DISTRIBUTION of this count over all
  * irun samples.  For each irun, we expect to get a binomial
  * distribution around the expected value of tsamples*ntuple_probl.
  *
  * We cannot quite do a straight chisq on this, however.  For one
  * thing, we do not know a priori what the number of degrees of
  * freedom is for any given ntuple or number of samples.  For another,
  * the asymptotic tails of the distribution have too few member for
  * Pearson's chisq to be very accurate, although we have just learned
  * of the "G-test" that is supposedly valid here if we can compute
  * or estimate the number of degrees of freedom.  Or we can do what
  * Marsaglia frequently does and bundle all counts outside of a
  * suitable cutoff to be a single bin with the sum probability of the
  * tail -- which we should be able to compute with the binomial CDF,
  * I think -- and then either compute chisq using it as a single bin
  * or (in the case of many of the monkey tests) make this tail the
  * FOCUS of the test and check to see if the tail is correctly occupied
  * as a simple normal with the binomial probability.
  *
  * In any of the chisq cases, though, I have to have some way of estimating
  * or computing the number of degrees of freedom.  That's the one thing
  * that is missing, so far.  If I get that, the rest should be pretty
  * easy to do several ways.
  */

 for(i=0;i<value_max;i++){
   Vtest_create(&vtest[i],bsamples+1);
   /*
    * We will experiment a bit with a cutoff that cleans up our degree of
    * freedom problem.
    */
   vtest[i].cutoff = 20.0;
   for(b=0;b<=bsamples;b++){
     if(i==0){
       pbin = gsl_ran_binomial_pdf(b,ntuple_prob,bsamples);
       vtest[i].x[b] = 0.0;
       vtest[i].y[b] = tsamples*pbin;
     } else {
       vtest[i].x[b] = 0.0;
       vtest[i].y[b] = vtest[0].y[b];
     }
     MYDEBUG(D_RGB_BITDIST){
       printf("# rgb_bitdist():  %3u     %3u   %10.5f  %10.5f\n",
         i,b,vtest[i].x[b],vtest[i].y[b]);
     }
     vtest[i].x[0] = tsamples;
   }
   MYDEBUG(D_RGB_BITDIST){
     printf("# rgb_bitdist():=====================================================\n");
   }
 }

 /*
  * Now (per ntuple) we check tsamples bitstrings of bits in length,
  * counting the 1's.  At the end we increment the result histogram
  * with the bitcount as an index as a trial that generated that
  * bitcount.
  */
 memset(count,0,value_max*sizeof(uint));
 for(t=0;t<tsamples;t++){

   /*
    * Clear the count vector for this sample.
    */
    
   for(b=0;b<bsamples;b++){

     /*
      * This gets the integer value of the ntuple of length nb that is the
      * next available in the bitstream provided by the generator, without
      * skipping bits.  Then increment the count of this ntuple value's
      * occurrence out of bsamples tries.
      */
     value = get_rand_bits_uint (nb, mask, rng);
     count[value]++;

     MYDEBUG(D_RGB_BITDIST) {
       printf("# rgb_bitdist():b=%u count[%u] = %u\n",b,value,count[value]);
     }

   }

   /*
    * We now increment the CUMULATIVE counter -- vtest -- so we can
    * compare the result to the expected value when we're done.
    */
   ctotal = 0;
   for(i=0;i<value_max;i++){
      uint count_i = count[i];
      if (count_i)
	{
	   count[i] = 0;		       /* performs memset */
	   ctotal += count_i;
	   vtest[i].x[count_i]++;
	   vtest[i].x[0]--;
	}
      MYDEBUG(D_RGB_BITDIST){
	 printf("# rgb_bitdist(): vtest[%u].x[%u] = %u\n",i,count[i],(uint)vtest[i].x[count[i]]);
     }
   }
   MYDEBUG(D_RGB_BITDIST){
     printf("# rgb_bitdist(): Sample %u: total count = %u (should be %u, count of bits)\n",t,ctotal,bits);
   }
 }

 /*
  * Now, finally, we need to score the vtest for this value of nb
  * (all tsamples of it) and turn it into a p-value.  This is one
  * of the two places this test may be screwing up big time.
  */

 MYDEBUG(D_RGB_BITDIST){
   printf("# rgb_bitdist(): ntuple_prob = %f\n",ntuple_prob);
   printf("# rgb_bitdist(): Testing %u samples of %u bit strings\n",test[0]->tsamples,bits);
   printf("# rgb_bitdist():=====================================================\n");
   printf("# rgb_bitdist():            vtest table\n");
   printf("# rgb_bitdist(): Outcome   bit          x           y       sigma\n");
 }
 ri = gsl_rng_uniform_int(rng,value_max);
 for(i=0;i<value_max;i++){
   for(b=0;b<=bsamples;b++){
     MYDEBUG(D_RGB_BITDIST){
       printf("# rgb_bitdist():  %3u     %3u   %10.5f  %10.5f\n",
         i,b,vtest[i].x[b],vtest[i].y[b]);
     }
   }
   MYDEBUG(D_RGB_BITDIST){
     printf("# rgb_bitdist():=====================================================\n");
   }
   Vtest_eval(&vtest[i]);

   /*
    * NOTE NOTE NOTE
    *
    * This is a bit nasty.  We can only save ONE pvalue per call.  The
    * only way to do so without bias is to randomly select which one to
    * save from large set of possibilities.
    *
    * However, this sucks.  Eventually I need to figure out how to
    * turn the whole list of pvalues into a pvalue.  They are NOT
    * independent though, so this is too difficult to deal with just
    * now.  Randomly sampling might miss one particular byte pattern
    * with a consistently bad pvalue unless/until the number of psamples
    * is high enough to resolve these deviations, but this seems
    * relatively "unlikely" -- deviations in the expected binomial bit
    * pattern distribution will usually be systematic.
    */
   if(i == ri ) {
     test[0]->pvalues[irun] = vtest[i].pvalue;
     MYDEBUG(D_RGB_BITDIST) {
       printf("# rgb_bitdist(): test[%u]->pvalues[%u] = %10.5f\n",
          0,irun,test[0]->pvalues[irun]);
     }
   }
   Vtest_destroy(&vtest[i]);
 }

 free(count);
 free(vtest);
 
 return(0);

}

