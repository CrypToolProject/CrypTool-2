/*
 * See copyright in copyright.h and the accompanying file COPYING
 */

/*
 *========================================================================
 *                    Marsaglia and Tsang GCD Test
 * 
 * 10^7 tsamples (default) of uint rands u, v are generated and two
 * statistics are generated: their greatest common divisor (GCD) (w)
 * and the number of steps of Euclid's Method required to find it
 * (k).  Two tables of frequencies are thus generated -- one for the
 * number of times each value for k in the range 0 to 41 (with counts
 * greater than this range lumped in with the endpoints).
 * The other table is the frequency of occurrence of each GCD w.
 * k is be distributed approximately binomially, but this is useless for
 * the purposes of performing a stringent test.  Instead four "good"
 * RNGs (gfsr4,mt19937_1999,rndlxs2,taus2) were used to construct a
 * simulated table of high precision probabilities for k (a process that
 * obviously begs the question as to whether or not THESE generators
 * are "good" wrt the test.  At any rate, they produce very similar tables
 * and pass the test with each other's tables (and are otherwise very
 * different RNGs).  The table of probabilities for the gcd distribution is
 * generated dynamically per test (it is easy to compute).  Chisq tests
 * on both of these binned distributions yield two p-values per test,
 * and 100 (default) p-values of each are accumulated and subjected to
 * final KS tests and displayed in a histogram.
 *========================================================================
 */

#include "libdieharder.h"

/*
 * Include _inline uint generator
 */
#include "static_get_bits.c"

/*
 * This determines the number of samples that go into building the kprob[]
 * table.
 */
#define KCNT 1000000000000L
#define KTBLSIZE 41

/*
 * This table is the result of "extensive simulation" with RNGs believed
 * to be "the best of show" -- good ones according to the GSL and the
 * other dieharder tests.  Eventually it will be based on roughly
 * 10^12 samples each from mt19937_1999, ranlxd2, gfsr4, and taus2
 * (presuming that all of them produce mean probabilities that
 * are both normally distributed and overlapping for each number).
 *
 * This SHOULD result in all four of these generators passing
 * easily for the more common 10^7 steps per run and 100 runs
 * (10^9 total samples) which begs the question, maybe, dunno.
 */

/*
 * 1000000000 passes through mt19937_1999 produce this table
double kprob[41] = {
 0.00000000e+00, 9.00000000e-09, 6.10000000e-08, 4.64000000e-07, 2.97000000e-06, 1.44620000e-05,
 5.89260000e-05, 2.06560000e-04, 6.29030000e-04, 1.67947700e-03, 3.99509000e-03, 8.51673000e-03,
 1.63520170e-02, 2.84309990e-02, 4.49361310e-02, 6.47695460e-02, 8.53523910e-02, 1.03000804e-01,
 1.14058597e-01, 1.16057281e-01, 1.08529716e-01, 9.33552020e-02, 7.38878410e-02, 5.38052490e-02,
 3.60252240e-02, 2.21585000e-02, 1.25080210e-02, 6.47410800e-03, 3.07120300e-03, 1.32867000e-03,
 5.22293000e-04, 1.88175000e-04, 6.06610000e-05, 1.75190000e-05, 4.72500000e-06, 1.09300000e-06,
 2.21000000e-07, 2.90000000e-08, 3.00000000e-09, 2.00000000e-09, 0.00000000e+00 };
*/


/*
 * 1000000000 passes through gfsr4 produce this table
double kprob[41] = {
 0.00000000e+00, 2.00000000e-09, 5.60000000e-08, 4.88000000e-07, 2.93600000e-06, 1.41980000e-05,
 5.87710000e-05, 2.06463000e-04, 6.28227000e-04, 1.67945800e-03, 3.99414700e-03, 8.51530300e-03,
 1.63522620e-02, 2.84365590e-02, 4.49312500e-02, 6.47630060e-02, 8.53330790e-02, 1.02999632e-01,
 1.14062682e-01, 1.16051639e-01, 1.08537040e-01, 9.33631650e-02, 7.38971760e-02, 5.37974130e-02,
 3.60227920e-02, 2.21610450e-02, 1.25156100e-02, 6.47749100e-03, 3.07318400e-03, 1.32887900e-03,
 5.23531000e-04, 1.87855000e-04, 6.08240000e-05, 1.77430000e-05, 4.71300000e-06, 1.10000000e-06,
 2.40000000e-07, 3.60000000e-08, 5.00000000e-09, 0.00000000e+00, 0.00000000e+00 };
*/
       
/*
 * 1000000000 passes through ranlxd2 produce this table
double kprob[41] = {
 0.00000000e+00, 1.00000000e-09, 5.70000000e-08, 4.87000000e-07, 2.96000000e-06, 1.44010000e-05,
 5.93860000e-05, 2.06081000e-04, 6.27411000e-04, 1.68188600e-03, 3.99561800e-03, 8.51950500e-03,
 1.63582570e-02, 2.84329560e-02, 4.49402540e-02, 6.47584160e-02, 8.53356540e-02, 1.02996361e-01,
 1.14069062e-01, 1.16032478e-01, 1.08535610e-01, 9.33725140e-02, 7.38923460e-02, 5.37952690e-02,
 3.60179830e-02, 2.21606850e-02, 1.25166100e-02, 6.47897800e-03, 3.07285400e-03, 1.32957500e-03,
 5.24012000e-04, 1.88015000e-04, 6.05240000e-05, 1.78120000e-05, 4.61000000e-06, 1.09400000e-06,
 2.26000000e-07, 4.60000000e-08, 6.00000000e-09, 0.00000000e+00, 0.00000000e+00 };
 */
       
/*
 * 1000000000 passes through taus2 produce this table
double kprob[41] = {
 0.00000000e+00, 1.00000000e-08, 5.90000000e-08, 4.69000000e-07, 2.89800000e-06, 1.43030000e-05,
 5.90690000e-05, 2.06573000e-04, 6.27223000e-04, 1.67981400e-03, 3.99532600e-03, 8.51784800e-03,
 1.63539980e-02, 2.84268470e-02, 4.49374280e-02, 6.47643620e-02, 8.53356470e-02, 1.03006610e-01,
 1.14052510e-01, 1.16046884e-01, 1.08547672e-01, 9.33837210e-02, 7.38873400e-02, 5.37986890e-02,
 3.60130290e-02, 2.21577570e-02, 1.25135350e-02, 6.47871400e-03, 3.06750200e-03, 1.32797700e-03,
 5.24629000e-04, 1.86837000e-04, 6.07010000e-05, 1.79740000e-05, 4.64900000e-06, 1.14000000e-06,
 2.17000000e-07, 2.90000000e-08, 9.00000000e-09, 0.00000000e+00, 1.00000000e-09 };
 */

/*
 * This is the full scale table -- something like 10^11 samples went into
 * building it, using all four "good" rng's above.
 */
double kprob_orig[41] = {
0.00000000e+00, 5.04750000e-09, 6.02750000e-08, 4.85600000e-07, 2.95277750e-06, 1.44486175e-05, 
5.90652150e-05, 2.06498620e-04, 6.27678690e-04, 1.67988137e-03, 3.99620540e-03, 8.51586863e-03, 
1.63523276e-02, 2.84322640e-02, 4.49370192e-02, 6.47662520e-02, 8.53358330e-02, 1.03002036e-01, 
1.14069579e-01, 1.16043292e-01, 1.08530910e-01, 9.33655294e-02, 7.38958017e-02, 5.38017783e-02, 
3.60191431e-02, 2.21585513e-02, 1.25137621e-02, 6.47848384e-03, 3.06968758e-03, 1.32847777e-03, 
5.23845965e-04, 1.87623133e-04, 6.08442950e-05, 1.77866925e-05, 4.65595750e-06, 1.09139000e-06, 
2.26025000e-07, 4.06075000e-08, 6.64500000e-09, 9.07500000e-10, 9.00000000e-11 };

double kprob1[KTBLSIZE] = {
0.0, 6.05359673641e-09, 5.89061528581e-08, 4.8032961797e-07, 2.95718200574e-06, 1.44296791438e-05,
5.90975396473e-05, 0.000206532422501, 0.000627402914834, 0.00167920626739, 0.00399624579679, 0.00851591071312,
0.0163514101916, 0.0284297291721, 0.0449405030918, 0.0647657727973, 0.0853352192988, 0.103003382707,
0.114073939881, 0.116038943668, 0.108533219692, 0.0933620445648, 0.0739014379387, 0.053795814061,
0.0360227392605, 0.022158237645, 0.0125126319501, 0.00647749635542, 0.00306979287487, 0.00132933422023,
0.000524417963001, 0.000187318073629, 6.04786910257e-05, 1.77870970261e-05, 4.671746866e-06, 1.06613151754e-06,
2.33296304995e-07, 4.00468707178e-08, 9.08039510462e-09, 6.98491931124e-10, 0};

double kprob[KTBLSIZE] = {
	0.0,   5.39e-09,  6.077e-08, 4.8421e-07, 2.94869e-06, 1.443266e-05,
	5.908569e-05, 0.00020658047, 0.00062764766, 0.00167993762, 0.00399620143, 0.00851629626,
	0.01635214339, 0.02843154488, 0.04493723812, 0.06476525706, 0.08533638862, 0.1030000214,
	0.11407058851, 0.11604146948, 0.10853040184, 0.09336837411, 0.07389607162, 0.05380182248,
	0.03601960159, 0.02215902902, 0.01251328472, 0.00647884418, 0.00306981507, 0.00132828179,
	0.00052381841, 0.00018764452, 6.084138e-05, 1.779885e-05, 4.66795e-06, 1.09504e-06,
	2.2668e-07,  4.104e-08,   6.42e-09,    8.4e-10,    1.4e-10 };

double kprob2[KTBLSIZE] = {
          0.0,  5.213e-09, 6.0704e-08, 4.8521e-07, 2.95083e-06, 1.4447958e-05,
 5.9070059e-05, 0.000206521906, 0.000627679842, 0.001679797186, 0.003996414492, 0.008515785524,
 0.016352439788, 0.028432147703, 0.044937745833, 0.064765999943, 0.085335932168, 0.103001938773,
 0.114069284452, 0.116042509045, 0.108530663851, 0.093367044789, 0.073896153625, 0.053801064832,
 0.036018738358, 0.022158331272, 0.012513639927, 0.006478573777, 0.003069820497, 0.001328600857,
 0.000523884717, 0.000187620922, 6.0831732e-05, 1.7787961e-05, 4.66037e-06, 1.090656e-06,
 2.26719e-07, 4.1078e-08,  6.431e-09,    8.8e-10,    1.2e-10,};


int marsaglia_tsang_gcd(Test **test, int irun)
{

 unsigned long long int t,ktbl[KTBLSIZE];
 uint i,j,k,u,v,w;
 static uint *gcd = 0;
 static double gnorm = 6.0/(PI*PI);
 static uint gtblsize = 0;
 Vtest vtest_k,vtest_u;

 /*
  * For output only
  */
 test[0]->ntuple = 0;
 test[1]->ntuple = 0;

 /* Make data tables for one-time entry -- do not delete.
 uint nbin = 50;
 double pbin = 0.376;
 printf("double kprob[%u] = {\n",KTBLSIZE);
 printf(" %10.8f",gsl_ran_binomial_pdf(0,pbin,nbin));
 for(i=1;i<KTBLSIZE;i++){
   if(i%6 == 0) {
     if((i)%6 == 0) printf(", \n");
     printf(" %10.8f",gsl_ran_binomial_pdf(i,pbin,nbin));
   } else {
     printf(", %10.8f",gsl_ran_binomial_pdf(i,pbin,nbin));
   }
 }
 printf("};\n");
 exit(0);
 */

 /*
  * Zero both tables, set gtblsize so that the expectation of gcd[] > 10
  * (arbitrary cutoff).  We don't free this on exit, but then, we only
  * allocate it once so it should be OK.
  */
 if(gtblsize == 0) {
   gtblsize = sqrt((double)test[0]->tsamples*gnorm/100.0);
   /* printf("gtblsize = %u\n",gtblsize); */
 }
 if(gcd == 0) gcd = (uint *)malloc(gtblsize*sizeof(uint));
 memset(gcd,0,gtblsize*sizeof(uint));
 memset(ktbl,0,KTBLSIZE*sizeof(unsigned long long int));


 Vtest_create(&vtest_k,KTBLSIZE);
 Vtest_create(&vtest_u,gtblsize);

 /* exit(0); */

 MYDEBUG(D_MARSAGLIA_TSANG_GCD) {
   printf("# user_marsaglia_tsang_gcd(): Beginning gcd test\n");
 }

 /* for(t=0;t<KCNT;t++){ */
 for(t=0;t<test[0]->tsamples;t++){
   /* Initialize counter for this sample */
   k = 0;
   /* Get nonzero u,v */
   do{
    u = get_rand_bits_uint(32,0xffffffff,rng);
   } while(u == 0);
   do{
    v = get_rand_bits_uint(32,0xffffffff,rng);
   } while(v == 0);

   do{
     w = u%v;
     u = v;
     v = w;
     k++;
   } while(v>0);

   /*
    * We just need test[0]->tsamples*c/u^2 to be greater than about 10, the
    * cutoff built into Vtest_eval()  For test[0]->tsamples = 10^7, turns out that
    * gtblsize < sqrt((double)test[0]->tsamples*gnorm/10.0) (about 780) should be just
    * about right.  We lump all counts larger than that into "the tail",
    * which MUST be included in the chisq targets down below.
    */
   if(u>=gtblsize) u = gtblsize-1;
   if(u<gtblsize) {
     gcd[u]++;
   }

   /*
    * lump the k's > KTBLSIZE only because that's what we did generating
    * the table...
    */
   k = (k>KTBLSIZE-1)?KTBLSIZE-1:k;
   ktbl[k]++;

 }

 /*
  * This is where I formulate my own probability table, using
  * a mix of the best RNGs I have available.  Of course this ultimately
  * begs many questions...
  *
 printf("double kprob[KTBLSIZE] = {\n");
 for(i=0;i<KTBLSIZE;i++){
   printf(" %10.12g,",(double)ktbl[i]/KCNT);
   if((i+1)%6 == 0) printf("\n");
 }
 printf("};\n");

 return;
  *
  */

 /*
  * Put tabular results into vtest_k, normalizing by the number
  * of samples as usual.  Note kprob is preloaded table of
  * target probabilities generated in commented fragement above.
  */
 MYDEBUG(D_MARSAGLIA_TSANG_GCD) {
   printf(" Binomial probability table for k distribution.\n");
   printf("  i\t  mean\n");
 }
 vtest_k.cutoff = 5.0;
 for(i=0;i<KTBLSIZE;i++){
   vtest_k.x[i] = (double)ktbl[i];
   vtest_k.y[i] = test[0]->tsamples*kprob[i];
   MYDEBUG(D_MARSAGLIA_TSANG_GCD) {
     printf(" %2u\t%f\t%f\t%f\n",i,vtest_k.x[i],vtest_k.y[i],vtest_k.x[i]-vtest_k.y[i]);
   }
 }
 /*
  * We will probably turn this into a table, but it isn't that expensive in the
  * short run as is.
  */
 for(i=0;i<gtblsize;i++){
   /*
    * No cutoff for this test?
    */
   vtest_u.cutoff = 5.0;
   if(i>1){
     vtest_u.x[i] = (double)gcd[i];
     if(i == gtblsize-1){
       /* This should be close enough to convergence */
       for(j=i;j<100000;j++){
         vtest_u.y[i] += test[0]->tsamples*gnorm/(1.0*j*j);
       }
       /* printf(" %2u\t%f\t%f\t%f\n",i,vtest_u.x[i],vtest_u.y[i],vtest_u.x[i]-vtest_u.y[i]); */
     } else {
       vtest_u.y[i] = test[0]->tsamples*gnorm/(i*i);
     }
   } else {
     vtest_u.x[i] = 0.0;
     vtest_u.y[i] = 0.0;
   }
   MYDEBUG(D_MARSAGLIA_TSANG_GCD) {
     printf(" %2u\t%f\t%f\t%f\n",i,vtest_u.x[i],vtest_u.y[i],vtest_u.x[i]-vtest_u.y[i]);
   }
 }

 /*
  * Evaluate test statistics for this run
  */

 Vtest_eval(&vtest_k);
 Vtest_eval(&vtest_u);

 test[0]->pvalues[irun] = vtest_k.pvalue;
 test[1]->pvalues[irun] = vtest_u.pvalue;
 

 MYDEBUG(D_MARSAGLIA_TSANG_GCD) {
   printf("# diehard_runs(): test[0]->pvalues[%u] = %10.5f\n",irun,test[0]->pvalues[irun]);
   printf("# diehard_runs(): test[1]->pvalues[%u] = %10.5f\n",irun,test[1]->pvalues[irun]);
 }



 Vtest_destroy(&vtest_k);
 Vtest_destroy(&vtest_u);

 MYDEBUG(D_MARSAGLIA_TSANG_GCD){
   printf("# marsaglia_tsang_gcd(): ks_pvalue_k[%u] = %10.5f  ks_pvalue_w[%u] = %10.5f\n",kspi,ks_pvalue[kspi],kspi,ks_pvalue2[kspi]);
 }

 kspi++;

 return(0);

}
