/*
 * By Bob Jenkins, public domain
 *
 * With a 4-term state, results are w, x+stuff, y+stuff, z+stuff, w+stuff.
 * Make sure we've mixed the state well enough that 1-bit differences at 
 * w are pretty different by the time we report w+stuff.
 */

#include <stdlib.h>
#include <string.h>
#include <stdio.h>
#include <math.h>
#include <float.h>
#include <time.h>

typedef  unsigned char      u1;
typedef  unsigned long      u4;

#define BUCKETS 128
#define LOGLEN  16
#define CUTOFF  13.0

typedef struct ranctx { u4 a; u4 b; u4 c; u4 d;} ranctx;

#define rot(x,k) ((x<<(k))|(x>>(32-(k))))

static u4 iii, jjj, kkk;

static u4 ranval( ranctx *x ) {
  /* xxx: the generator being tested */
  u4 e = x->a - rot(x->b, iii);
  x->a = x->b ^ rot(x->c, jjj);
  x->b = x->c + rot(x->d, kkk);
  x->c = x->d + e;
  x->d = e + x->a;
  return x->d; 
#ifdef NEVER
  /* yyy: the same generator in reverse */
  u4 e = x->d - x->a;
  x->d = x->c - e;
  x->c = x->b - rot(x->d, 32-kkk);
  x->b = x->a ^ rot(x->c, 32-jjj);
  x->a = e + rot(x->b, 32-iii);
  return x->d;
#endif
}

static u4 raninit( ranctx *x, u4 seed ) {
  u4 i, e;
  x->a = x->b = x->c = 0xf1ea5eed;
  x->d = seed - x->a;
  for (i=0; i<20; ++i) {
    e = ranval(x);
  }
  return e;
}

/* count how many bits are set in a 32-bit integer, returns 0..32 */
static u4 count(u4 x)
{
  u4 c = x;

  c = (c & 0x55555555) + ((c>>1 ) & 0x55555555);
  c = (c & 0x33333333) + ((c>>2 ) & 0x33333333);
  c = (c & 0x0f0f0f0f) + ((c>>4 ) & 0x0f0f0f0f);
  c = (c & 0x00ff00ff) + ((c>>8 ) & 0x00ff00ff);
  c = (c & 0x0000ffff) + ((c>>16) & 0x0000ffff);
  return c;
}

/* initialize the data collection array */
static void datainit( u4 *data, u4 *data2)
{
  u4 i;
  for (i=0; i<BUCKETS; ++i) {
    data[i] = 0;   /* look for poor XOR mixing */
    data2[i] = 0;  /* look for poor additive mixing */
  }
}

/* gather statistics on len overlapping subsequences of length 5 each */
static void gather( ranctx *x, u4 *data, u4 *data2, u4 length)
{
  u4 i, j, h;
  u4 k;
  ranctx y;

  for (i=0; i<BUCKETS; ++i) {
    for (k=0; k<length; ++k) {
      y = *x;
      if (i<32)
	y.a ^= (1<<i);
      else if (i<64)
	y.b ^= (1<<(i-32));
      else if (i<96)
	y.c ^= (1<<(i-64));
      else
	y.d ^= (1<<(i-96));
      for (j=0; j<4; ++j) {
	h = ranval(x) ^ ranval(&y);         /* look for poor mixing */
      }
      data[i] += count(h);
      h ^= (h<<1);     /* graycode to look for poor additive mixing */
      data2[i] += count(h);
    }
  }
}


static int report( u4 *data, u4 *data2, u4 length, int print)
{
  u4 i;
  double worst = data[0];
  for (i=1; i<BUCKETS; ++i) {
    if (worst > data[i]) {
      worst = data[i];
    }
    if (worst > 32-data[i]) {
      worst = 32-data[i];
    }
    if (worst > data2[i]) {
      worst = data2[i];
    }
    if (worst > 32-data2[i]) {
      worst = 32-data2[i];
    }
  }
  worst /= length;
  if (worst > CUTOFF) {
    if (print) {
      printf("iii=%2lu jjj=%2lu kkk=%2lu worst=%14.4f\n", 
	     iii, jjj, kkk, (float)worst);
    }
    return 1;
  } else {
    return 0;
  }
}

void driver()
{
  u4 i;
  u4 data[BUCKETS];
  u4 data2[BUCKETS];
  ranctx r;

  (void)raninit(&r, 0);
  datainit(data, data2);
  gather(&r, data, data2, (1<<6));
  for (i=6; i<LOGLEN; ++i) {
    gather(&r, data, data2, (1<<i));
    if (!report(data, data2, (1<<(i+1)), ((i+1)==LOGLEN))) {
      break;
    }
  }
}

int main_rngav( int argc, char **argv)
{
  u4 i, j, k;
  time_t a,z;
  
  time(&a);

  for (i=0; i<30; ++i) {
    for (j=0; j<30; ++j) {
      for (k=0; k<30; ++k) {
	kkk = k+1;
	jjj = j+1;
        iii = i+1;
	driver();
      }
    }
  }

  time(&z);

  printf("number of seconds: %6lu\n", (size_t)(z-a));

  return 0;

}
