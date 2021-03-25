/*
 *========================================================================
 * $Id: libdieharder.h 220 2006-08-16 19:26:56Z rgb $
 *
 * See copyright in copyright.h and the accompanying file COPYING
 *========================================================================
 */


#define	MYDEBUG(b)  if ( (verbose == b) || (verbose == D_ALL) )
 typedef enum {
   D_QUIET,
   D_ALL,
   D_DIEHARD_BDAY,
   D_DIEHARD_OPERM5,
   D_DIEHARD_RANK_32x32,
   D_DIEHARD_RANK_6x8,
   D_DIEHARD_BITSTREAM,
   D_DIEHARD_OPSO,
   D_DIEHARD_OQSO,
   D_DIEHARD_DNA,
   D_DIEHARD_COUNT_1S_STREAM,
   D_DIEHARD_COUNT_1S_BYTE,
   D_DIEHARD_PARKING_LOT,
   D_DIEHARD_2DSPHERE,
   D_DIEHARD_3DSPHERE,
   D_DIEHARD_SQUEEZE,
   D_DIEHARD_SUMS,
   D_DIEHARD_RUNS,
   D_DIEHARD_CRAPS,
   D_MARSAGLIA_TSANG_GCD,
   D_MARSAGLIA_TSANG_GORILLA,
   D_RGB_TIMING,
   D_RGB_PERSIST,
   D_RGB_BITDIST,
   D_RGB_KSTEST_TEST,
   D_RGB_MINIMUM_DISTANCE,
   D_RGB_PERMUTATIONS,
   D_RGB_LAGGED_SUMS,
   D_RGB_LMN,
   D_RGB_OPERM,
   D_STS_MONOBIT,
   D_STS_RUNS,
   D_STS_SERIAL,
   D_STS_BLOCK,
   D_TYPES,
   D_USER_TEMPLATE,
   D_MEASURE,
   D_SEED,
   D_STARTUP,
   D_BITS,
   D_SAMPLE,
   D_CHISQ,
   D_KSTEST,
   D_VTEST,
   D_XTEST,
   D_BRANK,
   D_FILE_INPUT,
   D_FILE_INPUT_RAW,
   D_ADD_MY_TYPES,
   D_STD_TEST,
   D_RNGS,
   N_DEBUG
 } Debug;

