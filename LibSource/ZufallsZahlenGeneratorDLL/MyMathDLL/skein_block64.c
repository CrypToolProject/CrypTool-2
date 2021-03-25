/***********************************************************************
**
** Implementation of the Skein block functions.
**
** Source code author: Doug Whiting, 2008.
**
** This algorithm and source code is released to the public domain.
**
** Compile-time switches:
**
**  SKEIN_USE_ASM  -- set bits (256/512/1024) to select which
**                    versions use ASM code for block processing
**                    [default: use C for all block sizes]
**
************************************************************************/

#include <string.h>
#include "skein.h"

#ifndef SKEIN_USE_ASM
#define SKEIN_USE_ASM   (0)                     /* default is all C code (no ASM) */
#endif

#ifndef SKEIN_LOOP
#define SKEIN_LOOP 001                          /* default: unroll 256 and 512, but not 1024 */
#endif

#define BLK_BITS        (WCNT*64)               /* some useful definitions for code here */
#define KW_TWK_BASE     (0)
#define KW_KEY_BASE     (3)
#define ks              (kw + KW_KEY_BASE)                
#define ts              (kw + KW_TWK_BASE)

#ifdef SKEIN_DEBUG
#define DebugSaveTweak(ctx) { ctx->h.T[0] = ts[0]; ctx->h.T[1] = ts[1]; }
#else
#define DebugSaveTweak(ctx)
#endif


/*****************************  Skein_512 ******************************/
#if !(SKEIN_USE_ASM & 512)
void Threefish_512_Process_Blocks64(Threefish_512_Ctxt_t *ctx, const u08b_t *input,
		        void *output, size_t blkCnt) {
    enum { WCNT = SKEIN_512_STATE_WORDS };
#undef  RCNT
#define RCNT  (SKEIN_512_ROUNDS_TOTAL/8)

#ifdef  SKEIN_LOOP                              /* configure how much to unroll the loop */
#define SKEIN_UNROLL_512 (((SKEIN_LOOP)/10)%10)
#else
#define SKEIN_UNROLL_512 (0)
#endif

#if SKEIN_UNROLL_512
#if (RCNT % SKEIN_UNROLL_512)
#error "Invalid SKEIN_UNROLL_512"               /* sanity check on unroll count */
#endif
    size_t  r;
    u64b_t  kw[WCNT+4+RCNT*2];                  /* key schedule words : chaining vars + tweak + "rotation"*/
#else
    u64b_t  kw[WCNT+4];                         /* key schedule words : chaining vars + tweak */
#endif
    u64b_t  X0,X1,X2,X3,X4,X5,X6,X7;            /* local copy of vars, for speed */
    u64b_t  w [WCNT];                           /* local copy of input block */
#ifdef SKEIN_DEBUG
    const u64b_t *Xptr[8];                      /* use for debugging (help compiler put Xn in registers) */
    Xptr[0] = &X0;  Xptr[1] = &X1;  Xptr[2] = &X2;  Xptr[3] = &X3;
    Xptr[4] = &X4;  Xptr[5] = &X5;  Xptr[6] = &X6;  Xptr[7] = &X7;
#endif

    Skein_assert(blkCnt != 0);                  /* never call with blkCnt == 0! */
	ts[0] = ctx->T[0];
	ts[1] = ctx->T[1];

	/* precompute the key schedule for this block */
	ks[0] = ctx->Key[0];
	ks[1] = ctx->Key[1];
	ks[2] = ctx->Key[2];
	ks[3] = ctx->Key[3];
	ks[4] = ctx->Key[4];
	ks[5] = ctx->Key[5];
	ks[6] = ctx->Key[6];
	ks[7] = ctx->Key[7];
	ks[8] = ks[0] ^ ks[1] ^ ks[2] ^ ks[3] ^ 
			ks[4] ^ ks[5] ^ ks[6] ^ ks[7] ^ SKEIN_KS_PARITY;

	ts[2] = ts[0] ^ ts[1];

	do  {
		Skein_Get64_LSB_First(w,input,WCNT); /* get input block in little-endian format */

        X0   = w[0] + ks[0];                    /* do the first full key injection */
        X1   = w[1] + ks[1];
        X2   = w[2] + ks[2];
        X3   = w[3] + ks[3];
        X4   = w[4] + ks[4];
        X5   = w[5] + ks[5] + ts[0];
        X6   = w[6] + ks[6] + ts[1];
        X7   = w[7] + ks[7];

        input += SKEIN_512_BLOCK_BYTES;

        Skein_Show_R_Ptr(BLK_BITS,&ctx->h,SKEIN_RND_KEY_INITIAL,Xptr);
        /* run the rounds */
#define Round512(p0,p1,p2,p3,p4,p5,p6,p7,ROT,rNum)                  \
    X##p0 += X##p1; X##p1 = RotL_64(X##p1,ROT##_0); X##p1 ^= X##p0; \
    X##p2 += X##p3; X##p3 = RotL_64(X##p3,ROT##_1); X##p3 ^= X##p2; \
    X##p4 += X##p5; X##p5 = RotL_64(X##p5,ROT##_2); X##p5 ^= X##p4; \
    X##p6 += X##p7; X##p7 = RotL_64(X##p7,ROT##_3); X##p7 ^= X##p6; \

#if SKEIN_UNROLL_512 == 0                       
#define R512(p0,p1,p2,p3,p4,p5,p6,p7,ROT,rNum)      /* unrolled */  \
    Round512(p0,p1,p2,p3,p4,p5,p6,p7,ROT,rNum)                      \
    Skein_Show_R_Ptr(BLK_BITS,&ctx->h,rNum,Xptr);

#define I512(R)                                                     \
    X0   += ks[((R)+1) % 9];   /* inject the key schedule value */  \
    X1   += ks[((R)+2) % 9];                                        \
    X2   += ks[((R)+3) % 9];                                        \
    X3   += ks[((R)+4) % 9];                                        \
    X4   += ks[((R)+5) % 9];                                        \
    X5   += ks[((R)+6) % 9] + ts[((R)+1) % 3];                      \
    X6   += ks[((R)+7) % 9] + ts[((R)+2) % 3];                      \
    X7   += ks[((R)+8) % 9] +     (R)+1;                            \
    Skein_Show_R_Ptr(BLK_BITS,&ctx->h,SKEIN_RND_KEY_INJECT,Xptr);
#else                                       /* looping version */
#define R512(p0,p1,p2,p3,p4,p5,p6,p7,ROT,rNum)                      \
    Round512(p0,p1,p2,p3,p4,p5,p6,p7,ROT,rNum)                      \
    Skein_Show_R_Ptr(BLK_BITS,&ctx->h,4*(r-1)+rNum,Xptr);

#define I512(R)                                                     \
    X0   += ks[r+(R)+0];        /* inject the key schedule value */ \
    X1   += ks[r+(R)+1];                                            \
    X2   += ks[r+(R)+2];                                            \
    X3   += ks[r+(R)+3];                                            \
    X4   += ks[r+(R)+4];                                            \
    X5   += ks[r+(R)+5] + ts[r+(R)+0];                              \
    X6   += ks[r+(R)+6] + ts[r+(R)+1];                              \
    X7   += ks[r+(R)+7] +    r+(R)   ;                              \
    ks[r +       (R)+8] = ks[r+(R)-1];  /* rotate key schedule */   \
    ts[r +       (R)+2] = ts[r+(R)-1];                              \
    Skein_Show_R_Ptr(BLK_BITS,&ctx->h,SKEIN_RND_KEY_INJECT,Xptr);

    for (r=1;r < 2*RCNT;r+=2*SKEIN_UNROLL_512)   /* loop thru it */
#endif                         /* end of looped code definitions */
        {
#define R512_8_rounds(R)  /* do 8 full rounds */  \
        R512(0,1,2,3,4,5,6,7,R_512_0,8*(R)+ 1);   \
        R512(2,1,4,7,6,5,0,3,R_512_1,8*(R)+ 2);   \
        R512(4,1,6,3,0,5,2,7,R_512_2,8*(R)+ 3);   \
        R512(6,1,0,7,2,5,4,3,R_512_3,8*(R)+ 4);   \
        I512(2*(R));                              \
        R512(0,1,2,3,4,5,6,7,R_512_4,8*(R)+ 5);   \
        R512(2,1,4,7,6,5,0,3,R_512_5,8*(R)+ 6);   \
        R512(4,1,6,3,0,5,2,7,R_512_6,8*(R)+ 7);   \
        R512(6,1,0,7,2,5,4,3,R_512_7,8*(R)+ 8);   \
        I512(2*(R)+1);        /* and key injection */

        R512_8_rounds( 0);

#define R512_Unroll_R(NN) ((SKEIN_UNROLL_512 == 0 && SKEIN_512_ROUNDS_TOTAL/8 > (NN)) || (SKEIN_UNROLL_512 > (NN)))

  #if   R512_Unroll_R( 1)
        R512_8_rounds( 1);
  #endif
  #if   R512_Unroll_R( 2)
        R512_8_rounds( 2);
  #endif
  #if   R512_Unroll_R( 3)
        R512_8_rounds( 3);
  #endif
  #if   R512_Unroll_R( 4)
        R512_8_rounds( 4);
  #endif
  #if   R512_Unroll_R( 5)
        R512_8_rounds( 5);
  #endif
  #if   R512_Unroll_R( 6)
        R512_8_rounds( 6);
  #endif
  #if   R512_Unroll_R( 7)
        R512_8_rounds( 7);
  #endif
  #if   R512_Unroll_R( 8)
        R512_8_rounds( 8);
  #endif
  #if   R512_Unroll_R( 9)
        R512_8_rounds( 9);
  #endif
  #if   R512_Unroll_R(10)
        R512_8_rounds(10);
  #endif
  #if   R512_Unroll_R(11)
        R512_8_rounds(11);
  #endif
  #if   R512_Unroll_R(12)
        R512_8_rounds(12);
  #endif
  #if   R512_Unroll_R(13)
        R512_8_rounds(13);
  #endif
  #if   R512_Unroll_R(14)
        R512_8_rounds(14);
  #endif
  #if  (SKEIN_UNROLL_512 > 14)
#error  "need more unrolling in Skein_512_Process_Block"
  #endif
        }
		((u64b_t *) output)[0] = X0;
		((u64b_t *) output)[1] = X1;
		((u64b_t *) output)[2] = X2;
		((u64b_t *) output)[3] = X3;
		((u64b_t *) output)[4] = X4;
		((u64b_t *) output)[5] = X5;
		((u64b_t *) output)[6] = X6;
		((u64b_t *) output)[7] = X7;

                /*
		 * This is a silly fix, perhaps, BUT it shuts up the
		 * compiler warning about doing arithmetic with a void
		 * pointer.  I think it will do the same thing the commented
		 * line did, without the warning.
		 */
		/* output += SKEIN_512_BLOCK_BYTES; */
		unsigned long long int output_tmp = (unsigned long long int) output;
		output_tmp += SKEIN_512_BLOCK_BYTES;
		output = (void *) output_tmp;
        } while (--blkCnt);
    }

#if defined(SKEIN_CODE_SIZE) || defined(SKEIN_PERF)
size_t Skein_512_Process_Block_CodeSize(void)
    {
    return ((u08b_t *) Skein_512_Process_Block_CodeSize) -
           ((u08b_t *) Skein_512_Process_Block);
    }
uint_t Skein_512_Unroll_Cnt(void)
    {
    return SKEIN_UNROLL_512;
    }
#endif
#endif
