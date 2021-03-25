/*
 *=========================================================
 *        DAB Fill Tree 2 Test
 * Bit version of Fill Tree test.
 * This test fills small binary trees of fixed depth with
 * "visited" markers.  When a marker cannot be placed, the
 * current count of markers in the tree and the position
 * that the marker would have been inserted, if it hadn't
 * already been marked.
 *
 * For each bit in the RNG input, the test takes a step
 * right (for a zero) or left (for a one) in the tree.
 * If the node hasn't been marked, it is marked, and the
 * path restarts.  Otherwise, the test continues with the
 * next bit.
 *
 * The test returns two p-values.  The first is a Pearson
 * chi-sq test against the expected values (which were
 * estimated empirically.  The second is a Pearson chi-sq
 * test for a uniform distribution of the positions at
 * which the insert failed.
 *
 * Because of the target data for the first p-value,
 * ntuple must be kept at the default (128).
 */
#include "libdieharder.h"

typedef unsigned char uchar;

_Check_return_
static double targetData1[32] = {  // size=32, generated from 3e9 samples
0.00000000000e+00, 0.00000000000e+00, 0.00000000000e+00, 0.00000000000e+00,
9.76265666667e-04, 3.90648133333e-03, 9.42791500000e-03, 1.77898240000e-02,
2.88606903333e-02, 4.21206876667e-02, 5.67006123333e-02, 7.13000270000e-02,
8.43831060000e-02, 9.43500813333e-02, 9.97647353333e-02, 9.97074473333e-02,
9.39825660000e-02, 8.32745740000e-02, 6.90631193333e-02, 5.33001223333e-02,
3.80133193333e-02, 2.48386790000e-02, 1.47058170000e-02, 7.79203100000e-03,
3.63280466667e-03, 1.45833733333e-03, 4.88868000000e-04, 1.31773666667e-04,
2.63546666667e-05, 3.51800000000e-06, 2.42333333333e-07, 0.00000000000e+00
};

_Check_return_
static double targetData2[64] = {  // size=64, generated from 3e9 samples
0.00000000000e+00, 0.00000000000e+00, 0.00000000000e+00, 0.00000000000e+00,
0.00000000000e+00, 3.03990000000e-05, 1.52768666667e-04, 4.47074666667e-04,
1.00459133333e-03, 1.91267566667e-03, 3.25090066667e-03, 5.08490633333e-03,
7.45162400000e-03, 1.03865720000e-02, 1.38770320000e-02, 1.78957393333e-02,
2.23788223333e-02, 2.72281453333e-02, 3.23125090000e-02, 3.74760433333e-02,
4.25407143333e-02, 4.72809176667e-02, 5.15021953333e-02, 5.49909926667e-02,
5.75592136667e-02, 5.90709556667e-02, 5.94040870000e-02, 5.85191160000e-02,
5.64374923333e-02, 5.32542393333e-02, 4.91296690000e-02, 4.42833420000e-02,
3.89462263333e-02, 3.33966756667e-02, 2.78853806667e-02, 2.26466276667e-02,
1.78641310000e-02, 1.36666050000e-02, 1.01212106667e-02, 7.24765200000e-03,
5.00267066667e-03, 3.32686800000e-03, 2.12376733333e-03, 1.29971400000e-03,
7.59987666667e-04, 4.23040000000e-04, 2.23585333333e-04, 1.11777000000e-04,
5.27836666667e-05, 2.33956666667e-05, 9.67033333333e-06, 3.63066666667e-06,
1.29500000000e-06, 4.04666666667e-07, 1.24666666667e-07, 2.80000000000e-08,
8.33333333333e-09, 1.33333333333e-09, 0.00000000000e+00, 0.00000000000e+00,
0.00000000000e+00, 0.00000000000e+00, 0.00000000000e+00, 0.00000000000e+00
};

static double targetData[128] = {  // size=128, generated from 6e9 samples
0.00000000000e+00,0.00000000000e+00,0.00000000000e+00,0.00000000000e+00,
0.00000000000e+00,0.00000000000e+00,4.77166666667e-07,2.85516666667e-06,
9.85200000000e-06,2.55708333333e-05,5.54055000000e-05,1.06338000000e-04,
1.86151333333e-04,3.02864333333e-04,4.66803833333e-04,6.86296166667e-04,
9.71489833333e-04,1.33154316667e-03,1.77454416667e-03,2.31186450000e-03,
2.94724100000e-03,3.68750350000e-03,4.53733233333e-03,5.50155300000e-03,
6.58318550000e-03,7.77896366667e-03,9.08643266667e-03,1.05029766667e-02,
1.20238296667e-02,1.36316733333e-02,1.53215870000e-02,1.70715931667e-02,
1.88714935000e-02,2.06986750000e-02,2.25274171667e-02,2.43387205000e-02,
2.61018481667e-02,2.77880516667e-02,2.93792388333e-02,3.08369918333e-02,
3.21362530000e-02,3.32571148333e-02,3.41773491667e-02,3.48649186667e-02,
3.53142736667e-02,3.55033806667e-02,3.54345836667e-02,3.50962926667e-02,
3.44943555000e-02,3.36465076667e-02,3.25496588333e-02,3.12430565000e-02,
2.97450508333e-02,2.80761470000e-02,2.62786038333e-02,2.43732936667e-02,
2.24096238333e-02,2.04087746667e-02,1.84149658333e-02,1.64520138333e-02,
1.45553360000e-02,1.27460330000e-02,1.10448845000e-02,9.47002933333e-03,
8.03035333333e-03,6.73145016667e-03,5.58088483333e-03,4.56875066667e-03,
3.69710383333e-03,2.95267200000e-03,2.32891533333e-03,1.81311533333e-03,
1.39093383333e-03,1.05322383333e-03,7.86386833333e-04,5.78384666667e-04,
4.18726333333e-04,2.98206500000e-04,2.09200000000e-04,1.44533833333e-04,
9.79670000000e-05,6.52683333333e-05,4.27531666667e-05,2.73350000000e-05,
1.72115000000e-05,1.06390000000e-05,6.38166666667e-06,3.79950000000e-06,
2.21116666667e-06,1.25500000000e-06,6.95833333333e-07,3.83500000000e-07,
1.98000000000e-07,9.91666666667e-08,4.73333333333e-08,2.66666666667e-08,
1.13333333333e-08,5.50000000000e-09,2.66666666667e-09,1.00000000000e-09,
6.66666666667e-10,3.33333333333e-10,0.00000000000e+00,0.00000000000e+00,
0.00000000000e+00,0.00000000000e+00,0.00000000000e+00,0.00000000000e+00,
0.00000000000e+00,0.00000000000e+00,0.00000000000e+00,0.00000000000e+00,
0.00000000000e+00,0.00000000000e+00,0.00000000000e+00,0.00000000000e+00,
0.00000000000e+00,0.00000000000e+00,0.00000000000e+00,0.00000000000e+00,
0.00000000000e+00,0.00000000000e+00,0.00000000000e+00,0.00000000000e+00,
0.00000000000e+00,0.00000000000e+00,0.00000000000e+00,0.00000000000e+00,
};

__inline int insertBit(uint x, uchar *array, uint *i, uint *d);

int dab_filltree2(Test **test, int irun) {
 int size = (ntuple == 0) ? 128 : ntuple;
 uint target = sizeof(targetData)/sizeof(double);
 int startVal = (size / 2) - 1;
 uchar *array = (uchar *) malloc(sizeof(*array) * size);
 double *counts, *expected;
 int i, j;
 uint x;
 uint start = 0;
 uint end = 0;
 double *positionCounts;
 uint bitCount;

 test[0]->ntuple = 0;
 test[1]->ntuple = 1;

 counts = (double *) malloc(sizeof(double) * target);
 expected = (double *) malloc(sizeof(double) * target);

 memset(counts, 0, sizeof(double) * target);

 positionCounts = (double *) malloc(sizeof(double) * size/2);
 memset(positionCounts, 0, sizeof(double) * size/2);

 /* Calculate expected counts. */
 for (i = 0; i < target; i++) {
   expected[i] = targetData[i] * test[0]->tsamples;
   if (expected[i] < 4) {
     if (end == 0) start = i;
   } else if (expected[i] > 4) end = i;
 }
 start++;


 x = gsl_rng_get(rng);
 bitCount = rmax_bits;
 for (j = 0; j < test[0]->tsamples; j++) {
   int ret;
   memset(array, 0, sizeof(*array) * size);
   i = 0;
   do {  /* While new markers can be aded to this tree.... */
     uint index = startVal;
     uint d = (startVal + 1) / 2;
     if (i > size * 2) {
       test[0]->pvalues[irun] = 0;
       return(0);
     }
     do {  /* While this path has not yet found a blank node to mark.... */
       ret = insertBit(x & 0x01, array, &index, &d);  /* Keep going */
       x >>= 1;
       if (--bitCount == 0) {
         x = gsl_rng_get(rng);
         bitCount = rmax_bits;
       }
     } while (ret == -2);  /* End of path. */

     i++;
   } while (ret == -1);  /* Couldn't insert marker; end of this tree. */
   positionCounts[ret/2]++;

   counts[i-1]++;
 }

 /* First p-value is calculated based on the targetData array. */
 test[0]->pvalues[irun] = chisq_pearson(counts + start, expected + start, end - start);

 /* Second p-value is calculated against a uniform distribution. */
 for (i = 0; i < size/2; i++) expected[i] = test[0]->tsamples/(size/2);
 test[1]->pvalues[irun] = chisq_pearson(positionCounts, expected, size/2);


 nullfree(positionCounts);
 nullfree(expected);
 nullfree(counts);
 nullfree(array);

 return(0);
}

/* 
 * Insert a bit into the tree, represented by an array.
 * A value of one is marked; zero is unmarked.
 * The function returns -2 is still on the path.
 * The function returns -1 if the path ends by marking a node.
 * The function returns >= 0 if the path went too deep; the
 * returned value is the last position of the path.
 */
_inline int insertBit(uint x, uchar *array, uint *i, uint *d) {
 if (x != 0) {
   *i += *d;
 } else {
   *i -= *d;
 }
 *d /= 2;

 if (array[*i] == 0) {
   array[*i] = 1;
   return -1;
 }
 if (*d == 0) return *i;
 else return -2;
}

