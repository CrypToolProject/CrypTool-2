/*
 * By Bob Jenkins, public domain implementing Bob Blorp2's public domain test.
 *
 * Given a subsequence of n 32-bit random numbers, compute the number
 * of bits set in each term, reduce that to low, medium or high number
 * of bits, and concatenate a bunch of those 3-item buckets.
 * Do this for len overlapping n-value sequences.  And report the chi-square
 * measure of the results compared to the ideal distribution.
 */

#include <stdlib.h>
#include <string.h>
#include <stdio.h>
#include <math.h>
#include <float.h>
#include <time.h>

typedef  unsigned char      u1;
typedef  unsigned long      u4;
typedef  unsigned long long u8;

#define LOGBUCKETS 2
#define BUCKETS (1<<LOGBUCKETS)
#define TERMS 6
#define GRAY_CODE 1

typedef struct ranctx { u4 a; u4 b; u4 c; u4 d;} ranctx;

#define rot(x,k) ((x<<k)|(x>>(32-k)))

/* static u4 iii = 0; */

static u4 ranval( ranctx *x ) {
  u4 e;
  e = x->a;
  x->a = x->b;
  x->b = rot(x->c, 19) + x->d;
  x->c = x->d ^ x->a;
  x->d = e + x->b;
  return x->c; 
}

static void raninit( ranctx *x, u4 seed ) {
  u4 i;
  x->a = 0xf1ea5eed; 
  x->b = x->c = x->d = seed;
  for (i=0; i<20; ++i) {
    (void)ranval(x);
  }
}

/* count how many bits are set in a 32-bit integer, returns 0..32 */
static u4 count(u4 x)
{
  u4 c = x;

  if (GRAY_CODE) c = c^(c<<1);

  c = (c & 0x55555555) + ((c>>1 ) & 0x55555555);
  c = (c & 0x33333333) + ((c>>2 ) & 0x33333333);
  c = (c & 0x0f0f0f0f) + ((c>>4 ) & 0x0f0f0f0f);
  c = (c & 0x00ff00ff) + ((c>>8 ) & 0x00ff00ff);
  c = (c & 0x0000ffff) + ((c>>16) & 0x0000ffff);
  return c;
}

/* somehow covert 0..32 into 0..BUCKETS-1 */
static u4 ftab[] = {
  0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 1,
  1, 
  1, 2, 2, 2, 2, 2, 2, 2,
  2, 2, 2, 2, 2, 2, 2, 2
};

/* initialize the data collection array -- UNUSED (yet)
static void datainit2( u8 *data, u4 index, u4 depth, u4 terms)
{
  u4 i;
  index *= 3;
  if (depth == terms-1) {
    for (i=0; i<3; ++i)
      data[index+i] = 0;
  } else {
    for (i=0; i<3; ++i)
      datainit2(data, index+i, depth+1, terms);
  }
}
 */

static void datainit( u8 *data, u4 terms)
{
  u4 i;
  for (i=0; i<(1<<(LOGBUCKETS*terms)); ++i)
    data[i] = 0;
}

/* gather statistics on len overlapping subsequences of "terms" values each */
static void gather( ranctx *r, u8 *data, u8 len, u4 terms)
{
  u8 i;
  u4 val = 0;
  u4 mask = (1<<(LOGBUCKETS*terms))-1;
  for (i=0; i<BUCKETS; ++i)
    val = ((val<<LOGBUCKETS)&mask) + ftab[count(ranval(r))];
  for (i=0; i<len; ++i) {
    val = ((val<<LOGBUCKETS)&mask) + ftab[count(ranval(r))];
    ++data[val];
  }
}

/* figure out the probability of 0..BUCKETS-1=ftab[count(u4)] */
static void probinit( double *pc, u4 maxbits)
{
  u8 i,j,k;
  for (i=0; i<=maxbits; ++i) {
    pc[i] = 0.0;
  }
  for (i=0; i<=maxbits; ++i) {
    k = 1;
    for (j=1; j<=i; ++j) {
      k = (k * (maxbits+1-j)) / j;
    }
    pc[ftab[i]] += ldexp((double)k,-32);
  }
}

#define MAXBITS 32
static void chi( u8 *data, u8 len, u4 terms)
{
  u4 i,j,k;                 /* counters */
  double pc[MAXBITS+1];     /* pc[i] is probability of a bitcount of i */
  double expect = 0.0;      /* expected fullness of current bucket */
  double expectother = 0.0; /* expected fullness of "other" bucket */
  double var = 0.0;         /* total variance */
  double temp;              /* used to calculate variance of a bucket */
  u8 buckets = 0;           /* number of buckets used */
  u8 countother = 0;
  
  probinit(pc, MAXBITS);

  /* handle the nonnegligible buckets */
  for (i=0; i < (1<<(LOGBUCKETS*terms)); ++i) {
    
    /* determine the expected frequency of this bucket */
    expect = (double)len;
    k = i;
    for (j=0; j<terms; ++j) {
      expect *= pc[k&(BUCKETS-1)];
      k >>= LOGBUCKETS;
    }

    /* calculate the variance for this bucket */
    if (expect < 5.0) {
      expectother += expect;
      countother += data[i];
    } else {
      ++buckets;
      temp = (double)data[i] - expect;
      temp = temp*temp/expect;
      if (temp > 20.0) {
	k = i;
	for (j=0; j<terms; ++j) {
	  printf("%2d ", (int) k&(BUCKETS-1));
	  k >>= LOGBUCKETS;
	}
	printf("%14.4f %14.4f %14.4f\n",
	       (float)temp,(float)expect,(float)data[i]);
      }
      var += temp;
    }
  }

  /* lump all the others into one bucket */
  if (expectother > 5.0) {
    ++buckets;
    temp = (double)countother - expectother;
    temp = temp*temp/expectother;
    if (temp > 20.0) {
      printf("other %14.4f %14.4f %14.4f\n",
	     (float)temp,(float)expectother,(float)countother);
    }
    var += temp;
  }
  --buckets;

  /* calculate the total variance and chi-square measure */
  printf("expected variance: %11.4f   got: %11.4f   chi-square: %6.4f\n",
         (float)buckets, (float)var, 
	 (float)((var-buckets)/sqrt((float)buckets)));
}

int main_countx( int argc, char **argv)
{
  u8 len;
  u8 *data;
  u4 i, loglen, terms;
  ranctx r;
  time_t a,z;
  
  time(&a);
  if (argc == 3) {
    sscanf(argv[1], "%lu", &loglen);
    printf("sequence length: 2^^%lu\n", loglen);
    len = (((u8)1)<<loglen);

    sscanf(argv[2], "%lu", &terms);
    printf("terms in subsequences: %lu\n", terms);
  } else {
    fprintf(stderr, "usage: \"countn 24 6\" means use 2^^24 sequences of length 6\n");
    return 1;
  }

  data = (u8 *)malloc(sizeof(u8)*(1<<(LOGBUCKETS*terms)));
  if (!data) {
    fprintf(stderr, "could not malloc data\n");
    return 1;
  }

  for (i=0; i<=MAXBITS; ++i) {
    if (ftab[i] > BUCKETS) {
      fprintf(stderr, "ftab[%lu]=%lu needs a bigger LOGBUCKETS\n", i, ftab[i]);
      return 1;
    }
  }

  datainit(data, terms);
  raninit(&r, 0);
  gather(&r, data, len, terms);
  chi(data, len, terms);

  free(data);

  time(&z);
  printf("number of seconds: %6lu\n", (size_t)(z-a));

  return 0;

}
