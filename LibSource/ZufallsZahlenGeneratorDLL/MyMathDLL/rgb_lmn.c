/*
 * $Id: diehard_craps.c 191 2006-07-13 08:23:50Z rgb $
 *
 * See copyright in copyright.h and the accompanying file COPYING
 *
 */

/*
 *========================================================================
 *
 * This is a "supertest" of random number generators -- a test that should
 * systematically subsume pretty much all tests and provide very precise
 * information on how a RNG fails.
 *
 * The way it works is as follows:
 *
 *    Take l bits (starting at an arbitrary offset) from an RNG
 *    Skip m bits
 *    Take n bits
 *
 * to form a random integer l+n bits long.  These integers should be
 * uniformly distributed across the range 0 to (2^(l+n)-1).
 *
 * I'm not PRECISELY certain how to best form the test statistic here.
 * It should be the same as the future "sts_series" test or the current
 * "rgb_bitdist" test, and indeed either of these should be the same
 * as l=bits, m=0, n=0 of the lmn test.
 *
 * Let me explain how this test is a supertest of sorts.  First of all,
 * the l00 test (bitdist/series) is already a universal test, in principle.
 * Any generator that fails fails because integers cease to be uniformly
 * distributed at some bit level.  For example, suppose l = 2, m=n=0.
 * One can achieve non-random uniformity by counting:
 *
 *  00 01 10 11 00 01 10 11 00 01 10 11 00 01 10 11...
 *
 * but the sequence is then highly non-random, obviously.  This lack of
 * randomness is revealed most glaringly by looking at the distribution
 * of l=8,m=0,n=0 patterns, where only 00011011 appears out of the 256
 * possibilities!  Or, if one examines l = 2, m = 6, n = 2 one forms:
 * 0000 0000 0000 (an obvious failure).
 *
 * A bit of meditation on this will convince you that ANY possible failure
 * in randomness -- be it fourier related or high dimensional striping or
 * monkey tests or whatever -- is revealed by a lack of uniformity for some
 * sufficiently high l,m=0,n=0.  A "good" generator is one where that does
 * not occur for l << period of the generator, and a GREAT generator is
 * one where it does not occur for l << period and that period is very large
 * indeed!
 *
 * However, practical considerations prevent us from testing uniformity
 * for l > 32 or thereabouts.  There are 4 billion-odd bins at l = 32 (m = 0,
 * n = 0).  To test uniformity on this, one needs to generate roughly
 * 400 billion rands (to get an average of 100 hits per bin, with a
 * reasonable sigma).  Cumulating e.g. chisquare across this would be
 * difficult, but conceivably could be done on a cluster if nothing else.
 * At 64 bits, though, one cannot imagine holding the bins on any extant
 * system, and it would take far longer than the lifetime of the universe
 * to compute etc.  128 bits is just laughable.
 *
 * Hence the "lmn" part -- if we assume that (say) 24 bits is a practical
 * upper bound on what we can bin, we can extend our test into a PARTIAL
 * projection of randomness over longer sequences by testing:
 * l+n = 24, m=0-104.  This slides two windows across 128 bits and looks
 * at the relative frequency of the integers thus generated.  If the
 * RNG generates numbers that are uniform at 24 bits but correlated across
 * 128, this has some chance of revealing the correlation.  Indeed,
 * m can be quite large here -- one can potentially discover long range
 * fourier correlations in this way.
 *
 * This approach has one more weakness, though.  It is known that certain
 * generators fail because e.g. triples of integers, viewed as coordinates,
 * fall only on certain hyperplanes.  The lmn test should reveal this
 * profoundly for doublets of integers, but might have a hard time doing
 * third order correlations for triplets. That is, for large enough l it
 * can do so, but there is probably a projection of numbers that will pass
 * second order at l+n one can afford while missing a glaring third order
 * violation).  And so on -- there is likely a four-at-a-time correlated
 * sequency that passes three-at-a-time tests.  So the MOST general test
 * may need to be an lmnop... series.
 *
 * This isn't quite the only test one would ever need.  One may well want
 * to look at particular statistics for l=32 (for example), m = whatever,
 * n = 32, where one cannot afford to actually bin the 64 bits.  In that
 * case one has to e.g. figure out some sort of asymptotic statistic that
 * would deviate for at least a certain class of nonrandomness and pray.
 * this is what e.g. monkey tests do today, but they are not arranged
 * anywhere nearly this systematically and hence do not reveal specific
 * details about the correlations they uncover.
 *========================================================================
 */

#include "libdieharder.h"
#include "rgb_lmn.h"

int rgb_lmn(Dtest *dtest,Test **test)
{

 double pks;
 uint ps_save=0,ts_save=0;

 /*
  * Do a standard test if -a(ll) is selected.
  * ALSO use standard values if tsamples or psamples are 0
  */
 if(all == YES){
   ts_save = tsamples;
   tsamples = dtest->tsamples_std;
   ps_save = psamples;
   psamples = dtest->psamples_std;
 }
 if(tsamples == 0){
   tsamples = dtest->tsamples_std;
 }
 if(psamples == 0){
   psamples = dtest->psamples_std;
 }
 
 /*
  * Allocate memory for THIS test's ks_pvalues, etc.  Make sure that
  * any missed prior allocations are freed.
  */
 if(ks_pvalue) nullfree(ks_pvalue);
 ks_pvalue  = (double *)malloc((size_t) psamples*sizeof(double));

 /*
  * Reseed FILE random number generators once per individual test.
  * This correctly resets the rewind counter per test.
  */
 if(strncmp("file_input",gsl_rng_name(rng),10) == 0){
   gsl_rng_set(rng,1);
 }

 /* show_test_header(dtest); */

 /*
  * Any custom test header output lines go here.  They should be
  * used VERY sparingly.
  */

 /*
  * This is the standard test call.
  */
 kspi = 0;  /* Always zero first */
 /* pks = sample((void *)rgb_lmn_test); */

 /*
  * Test Results, standard form.
 show_test_results(dtest,pks,ks_pvalue,"Lagged Sum Test");
  */

 /*
  * Put back tsamples
  */
 if(all == YES){
   tsamples = ts_save;
   psamples = ps_save;
 }

 if(ks_pvalue) nullfree(ks_pvalue);

 return(0);

}

void rgb_lmn_test()
{

 uint t,i,lag;
 Xtest ptest;

 /*
  * ptest.x = actual sum of tsamples lagged samples from rng
  * ptest.y = tsamples*0.5 is the expected mean value of the sum
  * ptest.sigma = sqrt(tsamples/12.0) is the standard deviation
  */
 ptest.x = 0.0;  /* Initial value */
 ptest.y = (double) tsamples*0.5;
 ptest.sigma = sqrt(tsamples/12.0);

 /*
  * sample only every lag returns from the rng, discard the rest.
  * We have to get the (float) value from the user input and set
  * a uint 
  */
 if(x_user){
   lag = x_user;
 } else {
   lag = 2; /* Why not?  Anything but 0, really... */
 }

 if(verbose == D_USER_TEMPLATE || verbose == D_ALL){
   printf("# rgb_lmn(): Doing a test on lag %u\n",lag);
 }

 for(t=0;t<tsamples;t++){

   /*
    * A VERY SIMPLE test (probably not too sensitive)
    */

   /* Throw away lag-1 per sample */
   for(i=0;i<(lag-1);i++) gsl_rng_uniform(rng);

   /* sample only every lag numbers, reset counter */
   ptest.x += gsl_rng_uniform(rng);

 }

 Xtest_eval(&ptest);
 ks_pvalue[kspi] = ptest.pvalue;

 if(verbose == D_USER_TEMPLATE || verbose == D_ALL){
   printf("# rgb_lmn(): ks_pvalue[%u] = %10.5f\n",kspi,ks_pvalue[kspi]);
 }

 kspi++;

}

