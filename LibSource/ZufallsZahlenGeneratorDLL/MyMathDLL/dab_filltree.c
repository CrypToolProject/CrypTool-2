/*
 *=========================================================
 *				DAB Fill Tree Test
 * This test fills small binary trees of fixed depth with
 * words from the the RNG.  When a word cannot be inserted
 * into the tree, the current count of words in the tree is
 * recorded, along with the position at which the word
 * would have been inserted.
 *
 * The words from the RNG are rotated (in long cycles) to
 * better detect RNGs that may bias only the high, middle,
 * or low bytes.
 *
 * The test returns two p-values.  The first is a Pearson
 * chi-sq test against the expected values (which were
 * estimated empirically).  The second is a Pearson chi-sq
 * test for a uniform distribution of the positions at
 * which the insert failed.
 *
 * Because of the target data for the first p-value,
 * ntuple must be kept at the default (32).
 */
#include "libdieharder.h"

#define RotL(x,N)    (rmax_mask & (((x) << (N)) | ((x) >> (rmax_bits-(N)))))
#define CYCLES 4

_Check_return_
static double targetData1[] = {
 0,0,0,0,0.00000000,0.04446648,0.08890238,0.11821510,0.13166032,0.13135398,0.12074333,0.10339043,0.08300095,0.06272901,0.04470878,0.02987510,0.01872015,0.01095902,0.00597167,0.00298869,0.00138878,0.00059125,0.00022524,0.00007782,0.00002346,0.00000634,0.00000133,0.00000035,0.00000003,0.00000001,0.00000000,0.00000000
};
// n = 64, 0, 0.04446648, 0.08890238, ....

static double targetData[] = {
0.0, 0.0, 0.0, 0.0, 0.13333333, 0.20000000, 0.20634921, 0.17857143, 0.13007085, 0.08183633, 0.04338395, 0.01851828, 0.00617270, 0.00151193, 0.00023520, 0.00001680, 0.00000000, 0.00000000, 0.00000000, 0.00000000
};

_inline int insert(double x, double *array, unsigned int startVal);

int dab_filltree(Test **test,int irun) {
 int size = (ntuple == 0) ? 32 : ntuple;
 unsigned int target = sizeof(targetData)/sizeof(double);
 int startVal = (size / 2) - 1;
 double *array = (double *) malloc(sizeof(double) * size);
 double *counts, *expected;
 int i, j;
 double x;
 unsigned int start = 0;
 unsigned int end = 0;
 unsigned int rotAmount = 0;
 double *positionCounts;

 counts = (double *) malloc(sizeof(double) * target);
 expected = (double *) malloc(sizeof(double) * target);
 memset(counts, 0, sizeof(double) * target);

 positionCounts = (double *) malloc(sizeof(double) * size/2);
 memset(positionCounts, 0, sizeof(double) * size/2);

 test[0]->ntuple = size;
 test[1]->ntuple = size;

 /* Calculate expected counts. */
 for (i = 0; i < target; i++) {
   expected[i] = targetData[i] * test[0]->tsamples;
   if (expected[i] < 4) {
     if (end == 0) start = i;
   } else if (expected[i] > 4) end = i;
 }
 start++;


 for (j = 0; j < test[0]->tsamples; j++) {
   int ret;
   memset(array, 0, sizeof(double) * size);
   i = 0;
   do {
     unsigned int v = gsl_rng_get(rng);

     x = ((double) RotL(v, rotAmount)) / rmax_mask;
     i++;
     if (i > size * 2) {
       test[0]->pvalues[irun] = 0;
       return(0);
     }
     ret = insert(x, array, startVal);
   } while (ret == -1);
   positionCounts[ret/2]++;

   counts[i-1]++;
   if (j % (test[0]->tsamples/CYCLES) == 0) rotAmount++;
 }

 test[0]->pvalues[irun] = chisq_pearson(counts + start, expected + start, end - start);

 for (i = 0; i < size/2; i++) expected[i] = test[0]->tsamples/(size/2);
 test[1]->pvalues[irun] = chisq_pearson(positionCounts, expected, size/2);


 nullfree(positionCounts);
 nullfree(expected);
 nullfree(counts);
 nullfree(array);

 return(0);
}


_inline int insert(double x, double *array, unsigned int startVal) {
 uint d = (startVal + 1) / 2;
 uint i = startVal;
 while (d > 0) {
   if (array[i] == 0) {
     array[i] = x;
     return -1;
   }
   if (array[i] < x) {
     i += d;
   } else {
     i -= d;
   }
   d /= 2;
 }
 return i;
}

#include<time.h>

int main_filltree(int argc, char **argv) {
 int size = 64;
 int startVal = (size / 2) - 1;
 double *array = (double *) malloc(sizeof(double) * size);
 int i, j;
 double x;

 i = time(NULL);
 if (argc > 1) srand((i ^ (atoi(argv[1])<<7)) + (i<<4));
 else srand(i);

 for (j = 0; j < 10000000; j++) {
   memset(array, 0, sizeof(double) * size);
   i = 0;
   do {
     x = (double) rand() / RAND_MAX;
     i++;
   } while (insert(x, array, startVal) == 0);

   printf("%d\n", i);
 }

 //  for (i = 0; i < size; i++) printf("%f\n", array[i]);

 return(0);
}

