/*
 *========================================================================
 * See copyright in copyright.h and the accompanying file COPYING
 *========================================================================
 */

/*
 *========================================================================
 * This should be a nice, big case switch where we add EACH test
 * we might want to do and either just configure and do it or
 * prompt for input (if absolutely necessary) and then do it.
 *========================================================================
 */

/*
 * This is a not-quite-drop-in replacement for my old get_rand_bits()
 * routine contributed by John E. Davis.
 *
 * It should give us the "next N bits" from the random number stream,
 * without dropping any.  The return can even be of arbitrary size --
 * we make the return a void pointer whose size is specified by the
 * caller (and guaranteed to be big enough to hold the result).
 */

#include "libdieharder.h"

typedef unsigned int uint;
__inline static uint get_rand_bits_uint (uint nbits, uint mask, gsl_rng *rng)
{

 static uint bit_buffer;
 static uint bits_left_in_bit_buffer = 0;
 uint bits,breturn;

 /*
  * If there are enough bits left in the bit buffer, shift them out into
  * bits and return.  I like to watch this happen, so I'm instrumenting
  * this with some I/O from bits.c.  I'm also adding the following
  * conditionals so it works even if the mask isn't set by the caller
  * and does the right thing if the mask is supposed to be all 1's
  * (in which case this is a dumb routine to call, in some sense).
  */
 if(mask == 0){
   mask = ((1u << nbits) - 1);
 }
 if(nbits == 32){
   mask = 0xFFFFFFFF;
 }
 if(nbits > 32){
   fprintf(stderr,"Warning!  dieharder cannot yet work with\b");
   fprintf(stderr,"           %u > 32 bit chunks.  Exiting!\n\n",nbits);
   exit(0);
 }

/*
******************************************************************
 * OK, the way it works is:
First entry, nbits = 12
Mask = |00000000000000000000111111111111|
Buff = |00000000000000000000000000000000|
Not enough:
Bits = |00000000000000000000000000000000|
So we refill the bit_buffer (which now has 32 bits left):
Buff = |11110101010110110101010001110000|
We RIGHT shift this (32-nbits), aligning it for return,
& with mask, and return.
Bits = |00000000000000000000111101010101|
Need the next one.  There are 20 bits left.  Buff is
not changed.  We right shift buffer by 20-12 = 8,
then & with mask to return:
Buff = |11110101010110110101010001110000|
                    ^          ^ 8 bits->
Bits = |00000000000000000000101101010100|
Ready for the next one. There are only 8 bits left
and we need 12.  We LEFT shift Buff onto Bits
by needbits = 12-8 = 4
Buff = |11110101010110110101010001110000|
Bits = |01010101101101010100011100000000|
We refill the bit buffer Buff:
Buff = |01011110001111000000001101010010|
        ^  ^
We right shift 32 - needbits and OR the result with
Bits, & mask, and return.  The mask dumps the high part
from the old buffer.:
Bits = |00000000000000000000011100000101|
We're back around the horn with 28 bits left.  This is
enough, so we just right shift until the window is aligned,
mask out what we want, decrement the counter of number
of bits left, return:
Buff = |01011110001111000000001101010010|'
            ^          ^ 
Bits = |00000000000000000000111000111100|
and so on.  Very nice.
******************************************************************
* Therefore, this routine delivers bits in left to right bits
* order, which is fine.
*/

 /*
  * FIRST of all, if nbits == 32 and rmax_bits == 32 (or for that matter,
  * if we ever seek nbits == rmax_bits) we might as well just return the
  * gsl rng right away and skip all the logic below.  In the particular
  * case of nbits == 32 == rmax_bits, this also avoids a nasty problem
  * with bitshift operators on x86 architectures, see below.  I left a
  * local patch in below as well just to make double-dog sure that one
  * never does (uintvar << 32) for some uint variable; probably should
  * do the same for (uintvar >> 32) calls below.
  */
 if(nbits == rmax_bits){
   return gsl_rng_get(rng);
 }
  
 MYDEBUG(D_BITS) {
   printf("Entering get_rand_bits_uint. nbits = %d\n",nbits);
   printf(" Mask = ");
   dumpuintbits(&mask,1);
   printf("\n");
   printf("%u bits left\n",bits_left_in_bit_buffer);
   printf(" Buff = ");
   dumpuintbits(&bit_buffer,1);
   printf("\n");
 }

 if (bits_left_in_bit_buffer >= nbits) {
   bits_left_in_bit_buffer -= nbits;
   bits = (bit_buffer >> bits_left_in_bit_buffer);
   MYDEBUG(D_BITS) {
     printf("Enough:\n");
     printf(" Bits = ");
     breturn = bits & mask;
     dumpuintbits(&breturn,1);
     printf("\n");
   }
   return bits & mask;
 }

 nbits = nbits - bits_left_in_bit_buffer;
 /*
  * This fixes an annoying quirk of the x86.  It only uses the bottom five
  * bits of the shift value.  That means that if you shift right by 32 --
  * required in this routine to return 32 bit integers from a 32 bit
  * generator -- nothing happens as 32 is 0100000 and only the 00000 is used
  * to shift!  What a bitch!
  *
  * I'm going to FIRST try this -- which should work to clear the
  * bits register if nbits for the shift is 32 -- and then very
  * likely alter this to just check for rmax_bits == nbits == 32
  * and if so just shovel gsl_rng_get(rng) straight through...
  */
 if(nbits == 32){
   bits = 0;
 } else {
   bits = (bit_buffer << nbits);
 }
 MYDEBUG(D_BITS) {
   printf("Not enough, need %u:\n",nbits);
   printf(" Bits = ");
   dumpuintbits(&bits,1);
   printf("\n");
 }
 while (1) {
   bit_buffer = gsl_rng_get (rng);
   bits_left_in_bit_buffer = rmax_bits;

   MYDEBUG(D_BITS) {
     printf("Refilled bit_buffer\n");
     printf("%u bits left\n",bits_left_in_bit_buffer);
     printf(" Buff = ");
     dumpuintbits(&bit_buffer,1);
     printf("\n");
   }

   if (bits_left_in_bit_buffer >= nbits) {
     bits_left_in_bit_buffer -= nbits;
     bits |= (bit_buffer >> bits_left_in_bit_buffer);

     MYDEBUG(D_BITS) {
       printf("Returning:\n");
       printf(" Bits = ");
       breturn = bits & mask;
       dumpuintbits(&breturn,1);
       printf("\n");
     }

     return bits & mask;
   }
   nbits -= bits_left_in_bit_buffer;
   bits |= (bit_buffer << nbits);

   MYDEBUG(D_BITS) {
     printf("This should never execute:\n");
     printf("  Bits = ");
     dumpuintbits(&bits,1);
     printf("\n");
   }

 }

}

/*
 * This is a drop-in-replacement for get_bit_ntuple() contributed by
 * John E. Davis.  It speeds up this code substantially but may
 * require work if/when rngs that generate 64-bit rands come along.
 * But then, so will other programs.
 */
__inline static uint get_bit_ntuple_from_uint (uint bitstr, uint nbits, 
                                      uint mask, uint boffset)
{
   uint result;
   uint len;
   
   /* Only rmax_bits in bitstr are meaningful */
   boffset = boffset % rmax_bits;
   result = bitstr >> boffset;
   
   if (boffset + nbits <= rmax_bits)
     return result & mask;
   
   /* Need to wrap */
   len = rmax_bits - boffset;
   while (len < nbits)
     {
	result |= (bitstr << len);
	len += rmax_bits;
     }
   return result & mask;
}

/*
 * David Bauer doesn't like using the routine above to "fix" the
 * problem that some generators don't return 32 bit random uints.  This
 * version of the routine just ignore rmax_bits.  If a routine returns
 * 31 or 24 bit uints, tough.  This is harmless enough since nobody cares
 * about obsolete generators that return signed uints or worse anyway, I
 * imagine.  It MIGHT affect people writing HW generators that return only
 * 16 bits at a time or the like -- they need to be advised to wrap their
 * call routines up to return uints.  It's faster, too -- less checking
 * of the stream, fewer conditionals.
 */
__inline static uint get_bit_ntuple_from_whole_uint (uint bitstr, uint nbits, 
		uint mask, uint boffset)
{
 uint result;
 uint len;

 result = bitstr >> boffset;

 if (boffset + nbits <= 32) return result & mask;

 /* Need to wrap */
 len = 32 - boffset;
 while (len < nbits) {
   result |= (bitstr << len);
   len += 32;
 }

 return result & mask;

}
 
