/*
 *========================================================================
 * See copyright in copyright.h and the accompanying file COPYING
 *========================================================================
 */

#include "libdieharder.h"

/*
 * This should be the only tool we use to access bit substrings
 * from now on.  Note that we write our own bitstring access tools
 * instead of using ldap's LBER (Basic Encoding Rules) library call
 * to both retain control and because it would introduce a slightly
 * exotic dependency in the resulting application.
 *
 * bstring is a pointer to the uint string to be parsed.  It is a uint
 * pointer to make it easy to pass arbitrary strings which will generally
 * be e.g. unsigned ints in dieharder but might be other data types
 * in other applications (might as well make this semi-portable while I'm
 * writing it at all).  bslen is the length of bitstring in uints.  blen is
 * the length of the bitstring to be returned (as an unsigned int) and has
 * to be less than the length, in bits, of bitstring.  Finally, boffset
 * is the bit index of the point in the bitstring from which the result
 * will be returned.
 *
 * The only other thing that has to be defined is the direction from which
 * the bit offset and bit length are counted.  We will make the
 * LEAST significant bit offset 0, and take the string from the direction
 * of increasing signicance.  Examples:
 *
 *   bitstring:  10010110, length 1 (byte, char).
 * for offset 0, length 4 this should return: 0110
 *     offset 1, length 4: 1011
 *     offset 5, length 4: 0100 (note periodic wraparound)
 *     offset 6, length 4: 1010 (ditto)
 *
 * where of course the strings are actually returned as e.g.
 *     00000000000000000000000000000110  (unsigned int).
 */
unsigned int get_bit_ntuple(unsigned int *bitstring,unsigned int bslen,unsigned int blen,unsigned int boffset)
{

 unsigned int b,rlen;
 int ioffset;
 unsigned int result,carry,nmask;

 /*
  * Some tests for failure or out of bounds.  8*blen has to be less than
  * sizeof(uint).
  */
 if(blen > 8*sizeof(unsigned int)) blen = 8*sizeof(unsigned int);
 /*
  * Now that blen is sane, we can make a mask for the eventual return
  * value of length blen.
  */
 nmask = 1;
 /* dumpbits(&nmask,32); */
 for(b=0;b<blen-1;b++) {
   nmask = nmask <<1;
   nmask++;
   /* dumpbits(&nmask,32); */
 }
 /* dumpbits(&nmask,32); */
 
 if(verbose == D_BITS || verbose == D_ALL){
   printf("# get_bit_ntuple(): bslen = %u, blen = %u, boffset = %u\n",bslen,blen,boffset);
   printf("# get_bit_ntuple(): bitstring (uint) = %u\n",*bitstring);
   printf("# get_bit_ntuple(): bitstring = ");
   dumpbits(&bitstring[0],32);
   printf("# get_bit_ntuple(): nmask     = ");
   dumpbits(&nmask,32);
 }

 /*
  * This is the index of bitstring[] containing the first bit to
  * be returned, for example if boffset is 30, rmax_bits is 24,
  * and bslen (the length of the uint bitstring) is 4 (indices
  * run from 0-3) then ioffset is 4 - 1 -1 = 2 and the 30th bit
  * from the RIGHT is to be found in bitstring[2]. Put this uint
  * into result to work on further.
  */
 ioffset = bslen - (unsigned int) boffset/rmax_bits - 1;
 result = bitstring[ioffset];
 if(verbose == D_BITS || verbose == D_ALL){
   printf("bitstring[%d] = %u\n",ioffset,result);
   printf("Initial result =          ");
   dumpbits(&result,32);
 }
 /*
  * Now, WHICH bit in result is the boffset relative to ITS
  * rightmost bit?  We have to find the modulus boffset%rmax_bits.
  * For example 30%24 = 6, so in the continuing example it would
  * be bit 6 in result.
  */
 boffset = boffset%rmax_bits;
 if(verbose == D_BITS || verbose == D_ALL){
   printf("Shifting to bit offset %u\n",boffset);
 }

 /*
  * Now for the easy part.  We shift right boffset bits.
  */
 for(b=0;b<boffset;b++) result = result >> 1;
 if(verbose == D_BITS || verbose == D_ALL){
   printf("Shifted result =          ");
   dumpbits(&result,32);
 }

 /*
  * rlen is the cumulated length of result.  Initially, it is set to
  * rmax_bits - boffset, the number of bits result can now contribute from
  * boffset.  We now have to loop, adding bits from uints up the list
  * (cyclic) until we get enough to return blen.  Note that we have to
  * loop because (for example) blen = 32, rmax_bits = 16, boffset = 30
  * would start in bitstring[2] and get 2 bits (30 and 31), get all 16 bits
  * from bitstring[1], and still need 12 bits of bitstring[0] to return.
  */
 rlen = rmax_bits - boffset;
 if(verbose == D_BITS || verbose == D_ALL){
   printf("Cumulated %u signifcant bits\n",rlen);
 }

 while(blen > rlen){
   /*
    * If we get here, we have to use either bitstring[ioffset-1] or
    * bitstring[bslen-1], shifted and filled into result starting
    * at the correct slot.  Put this into carry to work on.
    */
   ioffset--;
   if(ioffset < 0) ioffset = bslen-1;
   carry = bitstring[ioffset];
   if(verbose == D_BITS || verbose == D_ALL){
     printf("bitstring[%d] = %u\n",ioffset,carry);
     printf("Next carry =              ");
     dumpbits(&carry,32);
   }

   /*
    * This is tricky!  Shift carry left until the first bit lines
    * up with the first empty bit in result, which we'll hope is
    * the current rlen bit.
    */
   for(b=0;b<rlen;b++){
     carry = carry << 1;
   }
   if(verbose == D_BITS || verbose == D_ALL){
     printf("Shifted carry =           ");
     dumpbits(&carry,32);
   }

   /*
    * Now we simply add result and carry AND increment rlen by
    * rmax_bit (since this is the exact number of bits it adds
    * to rlen).
    */
   result += carry;
   rlen += rmax_bits;
   if(verbose == D_BITS || verbose == D_ALL){
     printf("Cumulated %u signifcant bits\n",rlen);
     printf("result+carry =            ");
     dumpbits(&result,32);
   }
 }

 result = result & nmask;
   if(verbose == D_BITS || verbose == D_ALL){
     printf("Returning Result =        ");
     dumpbits(&result,32);
     printf("==========================================================\n");
   }
 return(result);

}

/*
 * dumpbits only can dump <= 8*sizeof(unsigned int) bits at a time.
 */
void dumpbits(unsigned int *data, unsigned int nbits)
{

 unsigned int i,j;
 unsigned int mask;

 if(nbits > 8*sizeof(unsigned int)) {
   nbits = 8*sizeof(unsigned int);
 }
 
 mask = (unsigned int)pow(2,nbits-1);
 for(i=0;i<nbits;i++){
   if(verbose == -1){
     printf("\nmask = %u = %04x :",mask,mask);
   }
   j = (mask & *data)?1:0;
   printf("%1u",j);
   mask = mask >> 1;
 }

}

/*
 * dumpbitwin is being rewritten to dump the rightmost nbits
 * from a uint, only.
 */
void dumpbitwin(unsigned int data, unsigned int nbits)
{

 while (nbits > 0){
   printf("%d",(data >> (nbits-1)) & 1);
   nbits--;
 }

}

void dumpuintbits(unsigned int *data, unsigned int nuints)
{

 unsigned int i;
 printf("|");
 for(i=0;i<nuints;i++) {
  dumpbits(&data[i],sizeof(unsigned int)*CHAR_BIT);
  printf("|");
 }

}

void cycle(unsigned int *data, unsigned int nbits)
{
 unsigned int i;
 unsigned int result,rbit,lmask,rmask;
 /*
  * We need two masks.  One is a mask of all the significant bits
  * in the rng, which may be as few as 24 and can easily be only
  * 31.
  *
  * The other is lmask, with a single bit in the position of the
  * leftmost significant bit.  We can make them both at once.
  */
 rmask = 1;
 lmask = 1;
 for(i=0;i<nbits-1;i++) {
  rmask = rmask << 1;
  rmask++;
  lmask = lmask << 1;
 }
 if(verbose){
   printf("%u bit rmask = ",nbits);
   dumpbits(&rmask,32);
   printf("%u bit lmask = ",nbits);
   dumpbits(&lmask,32);
 }
 /*
  * Next, we create mask the int being bit cycled into an internal
  * register.
  */
 result = *data & rmask;
 if(verbose){
   printf("Original int: ");
   dumpbits(data,32);
   printf("  Masked int: ");
   dumpbits(&result,32);
 }
 rbit = 1 & result;
 result = result >> 1;
 result += rbit*lmask;
 *data = result;
 if(verbose){
   printf(" Rotated int: ");
   dumpbits(data,32);
 }

}

/*
 * This is still a good idea, but we have to modify it so that it ONLY
 * gets VALID bits by their absolute index.
 */
int get_bit(unsigned int *rand_uint, unsigned int n)
{

 unsigned int index,offset,mask;

 /*
  * This routine is designed to get the nth VALID bit of an input uint
  * *rand_int.  The indexing is a bit tricky.  index tells us which vector
  * element contains the bit being sought:
  */
 index = (int) (n/rmax_bits);
 
 /*
  * Then we have to compute the offset of the bit desired, starting from
  * the first significant/valid bit in the unsigned int.
  */
 offset = (8*sizeof(unsigned int) - rmax_bits) + n%rmax_bits;
 mask = (int)pow(2,8*sizeof(unsigned int) - 1);
 mask = mask>>offset;
 if(mask & rand_uint[index]){
   return(1);
 } else {
   return(0);
 }
 
}

int get_int_bit(unsigned int i, unsigned int n)
{

 unsigned int mask;

 /*
  * This routine gets the nth bit of the unsigned int i.  It does very
  * limited checking to ensure that n is in the range 0-sizeof(uint)
  * Note
  */
 if(n < 0 || n > 8*sizeof(unsigned int)){
   fprintf(stderr,"Error: bit offset %u exceeds length %lu of uint.\n",n,8*sizeof(unsigned int));
   exit(0);
 }

 
 /*
  * Then we have make a mask and shift it over from the first (least
  * significant) bit in the unsigned int.  AND the result with i and
  * we're done.
  */
 mask = 1;
 mask = mask<<n;
 /* dumpbits(&mask,32); */
 /* dumpbits(&i,32); */
 if(mask & i){
   return(1);
 } else {
   return(0);
 }
 
}

/*
 * dumpbits only can dump 8*sizeof(unsigned int) bits at a time.
 */
void dumpbits_left(unsigned int *data, unsigned int nbits)
{

 int i;
 unsigned int mask;

 if(nbits > 8*sizeof(unsigned int)) {
   nbits = 8*sizeof(unsigned int);
 }
 
 mask = 1;
 for(i=0;i<nbits;i++){
   if(mask & *data){
     printf("1");
   } else {
     printf("0");
   }
   mask = mask << 1;
 }
 printf("\n");
}


unsigned int bit2uint(char *abit,unsigned int blen)
{

 int i,bit;
 unsigned int result;

 /* Debugging
 if(verbose == D_BITS || verbose == D_ALL){
   printf("# bit2uint(): converting %s\n",abit);
 }
 */

 result = 0;
 for(i = 0; i < blen; i++){
   result = result << 1;
   bit = abit[i] - '0';
   result += bit;
   /* Debugging
   if(verbose == D_BITS || verbose == D_ALL){
     printf("# bit2uint(): bit[%d] = %d, result = %u\n",i,bit,result);
   }
   */
 }

 /* Debugging
 if(verbose == D_BITS || verbose == D_ALL){
   printf("# bit2uint(): returning %0X\n",result);
 }
 */
 return(result);

}

void fill_uint_buffer(unsigned int *data,unsigned int buflength)
{

 /*
  * This routine fills *data with random bits from the current
  * random number generator.  Note that MANY generators return
  * fewer than 32 bits, making this a bit of a pain in the ass.
  * We need buffers like this for several tests, though, so it
  * is worth it to create a routine to do this once and for all.
  */

 unsigned int bufbits,bdelta;
 unsigned int i,tmp1,tmp2,mask;

 /*
  * Number of bits we must generate.
  */
 bufbits = buflength*sizeof(unsigned int)*CHAR_BIT;
 bdelta = sizeof(unsigned int)*CHAR_BIT - rmax_bits;
 mask = 0;
 for(i=0;i<bdelta;i++) {
  mask = mask<<1;
  mask++;
 }
 if(verbose == D_BITS || verbose == D_ALL){
   printf("rmax_bits = %d  bdelta = %d\n",rmax_bits,bdelta);
 }

 for(i=0;i<buflength;i++){

   /* put rmax_bits into tmp1 */
   tmp1 = gsl_rng_get(rng);
   /* Cruft
   printf("tmp1 = %10u = ",tmp1);
   dumpbits(&tmp1,32);
   */

   /* Shift it over to the left */
   tmp1 = tmp1 << bdelta;
   /*
   printf("tmp1 = %10u = ",tmp1);
   dumpbits(&tmp1,32);
   */

   /* put rmax_bits into tmp2 */
   tmp2 = gsl_rng_get(rng);
   /*
   printf("tmp2 = %10u = ",tmp2);
   printf("mask = %10u = ",mask);
   dumpbits(&mask,32);
   */

   /* mask the second rand */
   tmp2 = tmp2&mask;
   /*
   printf("tmp2 = %10u = ",tmp2);
   dumpbits(&tmp2,32);
   */

   /* Fill in the rest of the uint */
   tmp1 = tmp1 + tmp2;
   /* Cruft
   printf("tmp1 = %10u = ",tmp1);
   dumpbits(&tmp1,32);
   */

   data[i] = tmp1;
 }

}

/*
 * OK, the routines above work but they suck.  We need a set of SMALL
 * bitlevel routines for manipulating, aggregating, windowing and so
 * on, especially if we want to write extended versions of the bit
 * distribution test.
 *
 * On that note, let's generate SMALL routines for:
 *
 *  a) creating a uint mask() to select bits from uints
 *
 *  b) grabbing a masked window of bits and left/right shifting it
 *     to an arbitrary offset within the uint (no wraparound).
 *
 *  c) conjoining two such masked objects to produce a new object.
 *
 *  d) filling a buffer of arbitrary length with sequential bits from
 *     the selected rng.
 *
 *  e) Selecting a given window from that buffer with a given bitwise offset
 *     and with periodic wraparound.
 *
 *  f).... (we'll see what we need)
 */


/*
 * This generates a uint-sized mask of 1's starting at bit position
 * bstart FROM THE LEFT (with 0 being the most significant/sign bit)
 * and ending at bit position bstop.
 *
 * That is:
 *
 * umask(3,9) generates a uint containing (first line indicates bit
 * positions only):
 *  01234567890123456789012345678901
 *  00011111110000000000000000000000
 */
unsigned int b_umask(unsigned int bstart,unsigned int bstop)
{

 unsigned int b,mask,blen;

 if(bstart < 0 || bstop > 31 || bstop < bstart){
   printf("b_umask() error: bstart <= bstop must be in range 0-31.\n");
   exit(0);
 }
 blen = bstop-bstart+1;

 /*
  * Create blen 1's on right
  */
 mask = 1;
 for(b=1;b<blen;b++) {
   mask = mask <<1;
   mask++;
   /* dumpbits(&mask,sizeof(uint)*CHAR_BIT); */
 }

 /*
  * Now shift them over to the correct starting point.
  */
 mask = mask << (32-blen-bstart);
 /* dumpbits(&mask,sizeof(uint)*CHAR_BIT); */

 return mask;

}

/*
 * This uses b_umask to grab a particular window's worth of bits
 * from an arbitrary uint and THEN shift it to the new desired offset.
 * bstart FROM THE LEFT (with 0 being the most significant/sign bit)
 * and ending at bit position bstop.
 *
 * That is:
 *
 * b_window(input,2,5,0) generates a uint containing (first line indicates bit
 * positions only):
 *  input 01101011010000111010101001110110
 *   mask 00111100000000000000000000000000
 *      & 00101000000000000000000000000000
 *  shift 10100000000000000000000000000000
 */
unsigned int b_window(unsigned int input,unsigned int bstart,unsigned int bstop,unsigned int boffset)
{

 unsigned int mask,output;
 int shift;

 if(bstart < 0 || bstop > 31 || bstop < bstart){
   printf("b_umask() error: bstart <= bstop must be in range 0-31.\n");
   exit(0);
 }
 if(boffset < 0 || boffset > 31){
   printf("b_window() error: boffset must be in range 0-31.\n");
   exit(0);
 }
 shift = bstart - boffset;

 /* dumpbits(&input,sizeof(uint)*CHAR_BIT); */
 mask = b_umask(bstart,bstop);
 /* dumpbits(&mask,sizeof(uint)*CHAR_BIT); */
 output = input & mask;
 /* dumpbits(&output,sizeof(uint)*CHAR_BIT); */
 if(shift>0){
   output = output << shift;
 } else {
   output = output >> (-shift);
 }
 /* dumpbits(&output,sizeof(uint)*CHAR_BIT); */

 return output;

}

/*
 * Rotate the uint left (with periodic BCs)
 */
unsigned int b_rotate_left(unsigned int input,unsigned int shift)
{

 unsigned int tmp;

 dumpbits(&input,sizeof(unsigned int)*CHAR_BIT);
 tmp = b_window(input,0,shift-1,32-shift);
 dumpbits(&tmp,sizeof(unsigned int)*CHAR_BIT);
 input = input << shift;
 dumpbits(&input,sizeof(unsigned int)*CHAR_BIT);
 input += tmp;
 dumpbits(&input,sizeof(unsigned int)*CHAR_BIT);

 return input;

}

/*
 * Rotate the uint right (with periodic BCs)
 */
unsigned int b_rotate_right(unsigned int input, unsigned int shift)
{

 unsigned int tmp;

 if(shift == 0) return(input);
 MYDEBUG(D_BITS) {
   printf("Rotate right %d\n",shift);
   dumpbits(&input,sizeof(unsigned int)*CHAR_BIT);printf("|");
 }
 tmp = b_window(input,32-shift,31,0);
 MYDEBUG(D_BITS) {
   dumpbits(&tmp,sizeof(unsigned int)*CHAR_BIT);printf("\n");
 }
 input = input >> shift;
 MYDEBUG(D_BITS) {
   dumpbits(&input,sizeof(unsigned int)*CHAR_BIT);printf("|");
 }
 input += tmp;
 MYDEBUG(D_BITS) {
   dumpbits(&input,sizeof(unsigned int)*CHAR_BIT);printf("\n\n");
 }

 return(input);

}

/*
 * OK, with this all in hand, we can NOW write routines to return
 * pretty much any sort of string of bits from the prevailing rng
 * without too much effort. Let's get an ntuple from a uint vector
 * of arbitrary length and offset, with cyclic boundary conditions.
 *
 * We have to pack the answer into the LEAST significant bits in the
 * output vector, BTW, not the MOST.  That is, we have to fill the
 * output window all the way to the rightmost bit.  Tricky.
 *
 * I think that I can make this 2-3 times faster than it is by using
 * John E. Davis's double buffering trick.  In context, it would be:
 *   1) Limit return size to e.g. 32 bits.  I think that this is fair;
 * for the moment I can't see needing more than a uint return, but it
 * would be easy enough to generate an unsigned long long (64 bit)
 * uint return if we ever get to where it would help.  For example,
 * by calling this routine twice if nothing else.
 *   2) Pad the input buffer by cloning the first uint onto the end
 * to easily manage the wraparound.
 *   3) Use a dynamic buffer to rightshift directly into alignment
 * with rightmost part of output.
 *   4) IF NECESSARY Use a dynamic buffer to leftshift from the previous
 * word into alignment with the rightmost in the output.
 *   5) Mask the two parts and add (or logical and) them.
 *   6) Return.  Note that the contents of the starting buffer do not
 * change, and the two dynamic buffers are transient.
 *
 * BUT, we're not going to mess with that NOW unless we must.  The routine
 * below is AFAIK tested and reliable, if not optimal.
 */
void get_ntuple_cyclic(unsigned int *input,unsigned int ilen,
    unsigned int *output,unsigned int jlen,unsigned int ntuple,unsigned int offset)
{

 /* important bitlevel indices */
 int i,j,bs,be,bu,br1,br2;
 /* counter of number of bits remaining to be parsed */
 int bleft;


 /*
  * Now we set all the bit indices.
  */
 bu = sizeof(unsigned int)*CHAR_BIT;   /* index/size of uint in bits */
 bs = offset%bu;               /* starting bit */
 be = (offset + ntuple)%bu;    /* ending bit */
 if(be == 0) be = bu;          /* point PAST end of last bit */
 br1 = be - bs;                /* For Rule 1 */
 br2 = bu - bs;                /* For Rule 2 */
 MYDEBUG(D_BITS) {
   printf("bu = %d, bs = %d, be = %d, br1 = %d, br2 = %d\n",
             bu,bs,be,br1,br2);
 }

 /*
  * Set starting i (index of uint containing last bit) and j (the last
  * index in output).  We will impose periodic wraparound on i inside the
  * main loop.
  */
 i = (offset+ntuple)/bu;
 j = jlen - 1;
 if(be == bu) i--;   /* Oops.  be is whole line exactly */
 i = i%ilen;         /* Periodic wraparound on start */
 MYDEBUG(D_BITS) {
   printf("i = %d, j = %d\n",i,j);
 }

 /*
  * Always zero output
  */
 memset(output,0,jlen*sizeof(unsigned int));

 /*
  * Number of bits left to parse out
  */
 bleft = ntuple;

 /*
  * First we handle the trivial short cases -- one line of input
  * mapping to one line of output.  These are cases that are very
  * difficult for the rules below to catch correctly, as they presume
  * at least one cycle of the Right-Left rules.
  */

 /*
  * Start with all cases where the input lives on a single line and runs
  * precisely to the end (be = bu).  Apply Rule 2 to terminate.  That way
  * Rule 2 below can be CERTAIN that it is being invoked after a right
  * fill (only).
  */
 if(bleft == br2) {
   MYDEBUG(D_BITS) {
     printf("Rule 2a: From input[%d] to output[%d] = ",i,j);
   }
   output[j] += b_window(input[i],bs,bu-1,bu-br2);
   bleft -= br2;
   MYDEBUG(D_BITS) {
     dumpuintbits(&output[j],1);printf("\n");
     printf("bleft = %d\n",bleft);
     printf("Rule 2a: terminate.\n");
   }
 }

 /*
  * Similarly, resolve all cases where the input lives on a single line
  * and runs from start to finish within the line (so e.g. be <= bu-1,
  * bs >= 0).
  */
 if(bleft == br1) {
   MYDEBUG(D_BITS) {
     printf("Rule 1a: From input[%d] to output[%d] = ",i,j);
   }
   output[j] = b_window(input[i],bs,be-1,bu-bleft);
   bleft -= br1;
   MYDEBUG(D_BITS) {
     dumpuintbits(&output[j],1);printf("\n");
     printf("bleft = %d\n",bleft);
     printf("Rule 1a: terminate.\n");
   }
 }


 
 while(bleft > 0){

   /*
    * Rule 1
    */
   if(bleft == br1) {
     MYDEBUG(D_BITS) {
       printf("Rule  1: From input[%d] to output[%d] = ",i,j);
     }
     output[j] = b_window(input[i],bs,be-1,bu-bleft);
     bleft -= br1;
     MYDEBUG(D_BITS) {
       dumpuintbits(&output[j],1);printf("\n");
       printf("bleft = %d\n",bleft);
       printf("Rule  1: terminate.\n");
     }
     break;  /* Terminate while loop */
   }

   /*
    * Rule Right -- with termination check
    */
   if(bleft != 0) {
     MYDEBUG(D_BITS) {
       printf("Rule  R: From input[%d] to output[%d] = ",i,j);
     }
     output[j] += b_window(input[i],0,be-1,bu-be);
     bleft -= be;
     MYDEBUG(D_BITS) {
       dumpuintbits(&output[j],1);printf("\n");
       printf("bleft = %d\n",bleft);
     }
     i--;
     if(i<0) i = ilen-1;  /* wrap i around */
   } else {
     MYDEBUG(D_BITS) {
       printf("Rule  R: terminate.\n");
     }
     break;  /* Terminate while loop */
   }

   /*
    * This rule terminates if Rule Right is getting whole lines and
    * we're down to the last whole or partial line.  In this case we
    * have to decrement j on our own, as we haven't yet reached
    * Rule Left.
    * Rule 2b
    */
   if(bleft == br2 && be == bu ) {
     j--;
     MYDEBUG(D_BITS) {
       printf("Rule 2b: From input[%d] to output[%d] = ",i,j);
     }
     output[j] += b_window(input[i],bs,bu-1,bu - br2);
     bleft -= br2;
     MYDEBUG(D_BITS) {
       dumpuintbits(&output[j],1);printf("\n");
       printf("bleft = %d\n",bleft);
       printf("Rule 2b: terminate.\n");
     }
     break;  /* Terminate while loop */
   }

 
   /*
    * This rule terminates when Rule Right is getting partial lines.
    * In this case we KNOW that we must terminate with a Rule 2
    * partial line.
    * Rule 2c
    */
   if(bleft == br2 && br2 < bu) {
     MYDEBUG(D_BITS) {
       printf("Rule 2c: From input[%d] to output[%d] = ",i,j);
     }
     output[j] += b_window(input[i],bs,bu-1,bs - be);
     bleft -= br2;
     MYDEBUG(D_BITS) {
       dumpuintbits(&output[j],1);printf("\n");
       printf("bleft = %d\n",bleft);
       printf("Rule 2c: terminate.\n");
     }
     break;  /* Terminate while loop */
   }

 
   /*
    * Rule Left -- with termination check
    */
   if(bleft != 0) {
     if(be != bu) {
       /*
        * We skip Rule Left if Rule Right is getting full lines
	*/
       MYDEBUG(D_BITS) {
         printf("Rule  L: From input[%d] to output[%d] = ",i,j);
       }
       output[j] += b_window(input[i],be,bu-1,0);
       bleft -= bu-be;
       MYDEBUG(D_BITS) {
         dumpuintbits(&output[j],1);printf("\n");
         printf("bleft = %d\n",bleft);
       }
     }
   } else {
     MYDEBUG(D_BITS) {
       printf("Rule  L: terminate.\n");
     }
     break;  /* Terminate while loop */
   }
   /*
    * With this arrangment we can always decrement the second loop counter
    * here.
    */
   j--;

 }

}

/*
 * The last thing I need to make (well, it may not be the last thing,
 * we'll see) is a routine that
 *
 *   a) fills an internal static circulating buffer with random bits pulled
 * from the current rng.
 *
 *   b) returns a rand of any requested size (a void * routine with a
 * size parameter in bits or bytes) using the previous routine, keep
 * track of the current position in the periodic buffer with a static
 * pointer.
 *
 *   c) refills the circulating buffer from the current rng.
 *
 * Note well that this should be the ONLY point of access to even the
 * gsl rngs, as they do not all return the same number of bits.  We need
 * to be able to deal with e.g. 24 bit rands, 31 bit rands (quite a few
 * of them) and 32 bit uint rands.  This routine will completely hide this
 * level of detail from the caller and permit any number of bitlevel tests
 * to be conducted on the One True Bitstream produced by the generator
 * without artificial gaps or compression.
 *
 * Note that this routine is NOT portable (although it could be made to
 * be portable) and requires that global rng exist and be set up ready to go
 * by the calling routine.
 */

static unsigned int bits_rand[2];   /* A buffer that can handle partial returns */
static int bleft = -1; /* Number of bits we still need in rand[1] */

unsigned int get_uint_rand(gsl_rng *gsl_rng)
{

 static unsigned int bl,bu,tmp;

 /*
  * First call -- initialize/fill bits_rand from current rng.  bl and bu
  * should be static so they are preserved for later calls.
  */
 if(bleft == -1){
   /* e.g. 32 */
   bu = sizeof(unsigned int)*CHAR_BIT;
   /* e.g. 32 - 31 = 1 for a generator that returns 31 bits */
   bl = bu - rmax_bits;
   /* For the first call, we start with bits_rand[1] all or partially filled */
   bits_rand[0] = 0;
   bits_rand[1] = gsl_rng_get(gsl_rng);
   /* This is how many bits we still need. */
   bleft = bu - rmax_bits;
   /*
    * The state of the generator is now what it would be on a
    * typical running call.  bits_rand[1] contains the leftover bits from the
    * last call (if any).  We now have to interatively fill bits_rand[0],
    * grab (from the RIGHT) just the number of bits we need to fill the
    * rest of bits_rand[1] (which might be zero bits).  Then we save bits_rand[1]
    * for return and move the LEFTOVER (unused) bits from bits_rand[0] into
    * bits_rand[1], adjust bleft accordingly, and return the uint bits_rand.
    */
   MYDEBUG(D_BITS) {
     printf("bu = %d bl = %d\n",bu,bl);
     printf("  init: |");
     dumpbits(&bits_rand[0],bu);
     printf("|");
     dumpbits(&bits_rand[1],bu);
     printf("|\n");
   }
 }

 /*
  * We have to iterate into range because it is quite possible that
  * rmax_bits won't be enough to fill bits_rand[1].
  */
 while(bleft > rmax_bits){
   /* Get a bits_rand's worth (rmax_bits) into bits_rand[0] */
   bits_rand[0] = gsl_rng_get(gsl_rng);
   MYDEBUG(D_BITS) {
     printf("before %2d: |",bleft);
     dumpbits(&bits_rand[0],bu);
     printf("|");
     dumpbits(&bits_rand[1],bu);
     printf("|\n");
   }
   /* get the good bits only and fill in bits_rand[1] */
   bits_rand[1] += b_window(bits_rand[0],bu-rmax_bits,bu-1,bleft-rmax_bits);
   MYDEBUG(D_BITS) {
     printf(" after %2d: |",bleft);
     dumpbits(&bits_rand[0],bu);
     printf("|");
     dumpbits(&bits_rand[1],bu);
     printf("|\n");
   }
   bleft -= rmax_bits;  /* Number of bits we still need to fill bits_rand[1] */
 }

 /*
  * We are now in range.  We get just the number of bits we need, from
  * the right of course, and add them to bits_rand[1].
  */
 bits_rand[0] = gsl_rng_get(gsl_rng);
 MYDEBUG(D_BITS) {
   printf("before %2d: |",bleft);
   dumpbits(&bits_rand[0],bu);
   printf("|");
   dumpbits(&bits_rand[1],bu);
   printf("|\n");
 }
 if(bleft != 0) {
   bits_rand[1] += b_window(bits_rand[0],bu-bleft,bu-1,0);
 }
 MYDEBUG(D_BITS) {
   printf(" after %2d: |",bleft);
   dumpbits(&bits_rand[0],bu);
   printf("|");
   dumpbits(&bits_rand[1],bu);
   printf("|\n");
 }
 /* Save for return */
 tmp = bits_rand[1];
 /*
  * Move the leftover bits from bits_rand[0] into bits_rand[1] (right
  * justified), adjust bleft accordingly, and return.  Note that if we
  * exactly filled the return with ALL the bits in rand[0] then we
  * need to start over on the next one.
  */
 if(bleft == rmax_bits){
   bleft = bu;
 } else {
   bits_rand[1] = b_window(bits_rand[0],bu-rmax_bits,bu-bleft-1,bu-rmax_bits+bleft);
   bleft = bu - rmax_bits + bleft;
   MYDEBUG(D_BITS) {
     printf("  done %2d: |",bleft);
     dumpbits(&bits_rand[0],bu);
     printf("|");
     dumpbits(&bits_rand[1],bu);
     printf("|\n");
   }
 }
 return(tmp);

}

/*
 * With get_uint(rand() in hand, we can FINALLY create a routine that
 * can give us neither more nor less than the "next N bits" from the
 * random number stream, without dropping any.  The return can even be
 * of arbitrary size -- we make the return a void pointer whose size is
 * specified by the caller (and guaranteed to be big enough to hold
 * the result).
 */

/*
 * We'll use a BIG static circulating buffer so we can handle BIG
 * lags without worrying too much about it.  Space is cheap in the
 * one-page range.
 */
#define BRBUF 6
static unsigned int bits_randbuf[BRBUF];
static unsigned int bits_output[BRBUF];
/* pointer to line containing LAST return */
static int brindex = -1;
/* pointer to region being backfilled */
static int iclear = -1;
/* pointer to the last (most significant) returned bit */
static int bitindex = -1;

void get_rand_bits(void *result,unsigned int rsize,unsigned int nbits,gsl_rng *gsl_rng)
{

 int i,offset;
 unsigned int bu;
 char *output,*resultp;

 /*
  * Zero the return.  Note rsize is in characters/bytes.
  */
 memset(result,0,rsize);
 MYDEBUG(D_BITS) {
   printf("Entering get_rand_bits.  rsize = %d, nbits = %d\n",rsize,nbits);
 }

 /*
  * We have to do a bit of testing on call parameters.  We cannot return
  * more bits than the result buffer will hold.  We return 0 if nbits = 0.
  * We cannot return more bits than bits_randbuf[] will hold.
  */
 bu = sizeof(unsigned int)*CHAR_BIT;
 if(nbits == 0) return;  /* Handle a "dumb call" */
 if(nbits > (BRBUF-2)*bu){
   fprintf(stderr,"Warning:  get_rand_bits capacity exceeded!\n");
   fprintf(stderr," nbits = %d > %d (nbits max)\n",nbits,(BRBUF-2)*bu);
   return;
 }
 if(nbits > rsize*CHAR_BIT){
   fprintf(stderr,"Warning:  Cannot get more bits than result vector will hold!\n");
   fprintf(stderr," nbits = %d > %d (rsize max bits)\n",nbits,rsize*CHAR_BIT);
   return;   /* Unlikely, but possible */
 }

 if(brindex == -1){
   /*
    * First call, fill the buffer BACKWARDS.  I know this looks odd,
    * but we have to think of bits coming off the generator from least
    * significant on the right to most significant on the left as
    * filled by get_uint_rand(), so we have to do it this way to avoid
    * a de-facto shuffle for generators with rmax_bits < 32.
    */
   for(i=BRBUF-1;i>=0;i--) {
     bits_randbuf[i] = get_uint_rand(gsl_rng);
     /* printf("bits_randbuf[%d] = %u\n",i,bits_randbuf[i]); */
   }
   /*
    * Set the pointers to point to the last line, and the bit AFTER the
    * last bit.  Note that iclear should always start equal to brindex
    * as one enters the next code segment.
    */
   brindex = BRBUF;
   iclear = brindex-1;
   bitindex = 0;
   MYDEBUG(D_BITS) {
     printf("Initialization: iclear = %d  brindex = %d   bitindex = %d\n",iclear,brindex,bitindex);
   }
 }
 MYDEBUG(D_BITS) {
   for(i=0;i<BRBUF;i++){
     printf("%2d: ",i);
     dumpuintbits(&bits_randbuf[i],1);
     printf("\n");
   }
 }
 /*
  * OK, the logic here is: grab a window that fills the bit request
  * precisely (determining the starting buffer index and offset
  * beforehand) and put it into bits_output;  backfill WHOLE uints from
  * the END of the window to the last uint before the BEGINNING of the
  * window, (in reverse order!)
  */

 /*
  * Get starting indices.  Shift the buffer index back by the number of
  * whole uints in nbits, then shift back the bit index back by the
  * modulus/remainder, then handle a negative result (borrow), then finally
  * deal with wraparound of the main index as well.
  */
 brindex -= nbits/bu;
 bitindex = bitindex - nbits%bu;
 if(bitindex < 0) {
   /* Have to borrow from previous uint */
   brindex--;                /* So we push back one more */
   bitindex += bu;           /* and find the new bitindex */
 }
 if(brindex < 0) brindex += BRBUF;  /* Oops, need to wrap around */
 MYDEBUG(D_BITS) {
   printf("  Current Call: iclear = %d  brindex = %d   bitindex = %d\n",iclear,brindex,bitindex);
 }

 /*
  * OK, so we want a window nbits long, starting in the uint indexed
  * by brindex, displaced by bitindex.
  */
 offset = brindex*bu + bitindex;
 MYDEBUG(D_BITS) {
   printf("   Window Call: tuple = %d  offset = %d\n",nbits,offset);
 }
 get_ntuple_cyclic(bits_randbuf,BRBUF,bits_output,BRBUF,nbits,offset);
 /* Handle case where we returned whole uint at brindex location */
 MYDEBUG(D_BITS) {
   printf("   Cleaning up:  iclear = %d  brindex = %d  bitindex = %d\n",iclear,brindex,bitindex);
 }

 /*
  * Time to backfill.  We walk backwards, filling until we reach
  * the current index.
  */
 while(iclear != brindex){
   bits_randbuf[iclear--] = get_uint_rand(gsl_rng);
   if(iclear < 0) iclear += BRBUF;  /* wrap on around */
 }
 /*
  * Dump the refilled buffer
  */
 MYDEBUG(D_BITS) {
   for(i=0;i<BRBUF;i++){
     printf("%2d: ",i);
     dumpuintbits(&bits_randbuf[i],1);
     printf("\n");
   }
 }

 /*
  * At this point iclear SHOULD equal brindex, guaranteed, and bits_output
  * contains the answer desired.  However, NOW we have to copy this answer
  * back into result, a byte at a time, in reverse order.
  */
 MYDEBUG(D_BITS) {
   printf("bits_output[%d] = ",BRBUF-1);
   dumpuintbits(&bits_output[BRBUF-1],1);
   printf("\n");
 }

 /*
  * Get and align addresses of char *pointers into bits_output and result
  */
 output = (char *)&bits_output[BRBUF]-rsize;
 resultp = (char *)result;
 MYDEBUG(D_BITS) {
   printf("rsize = %d  output address = %p result address = %p\n",rsize,output,resultp);
 }

 /* copy them over characterwise */
 for(i=0;i<rsize;i++){
   resultp[i] = output[i];
   MYDEBUG(D_BITS) {
     printf(" Returning: result[%d} = ",i);
     dumpbits((unsigned int *)&resultp[i],8);
     printf(" output[%d} = ",i);
     dumpbits((unsigned int *)&output[i],8);
     printf("\n");
   }
 }

}

/*
 * OK, I CLEARLY need this.  What it will do is take a source and
 * destination address and BIT level offsets therein and copy
 * src into dest all aligned and everything.
 *
 * Example:
 *  dst = 01100000 00000000, doffset = 3
 *  src = 10100111 11100100, soffset = 7, slen = 9
 *
 *  dst = 01111110 01000000
 */
void mybitadd(char *dst, int doffset, char *src, int soffset, int slen)
{

 int sindex,dindex;
 int sblen;
 unsigned int tmp;
 char *btmp;

 btmp = (char *)(&tmp + 2);  /* we only need the last two bytes of tmp */

 sindex = soffset/CHAR_BIT;  /* index of first source byte */
 soffset = soffset%CHAR_BIT; /* index WITHIN first source byte */
 dindex = doffset/CHAR_BIT;  /* index of first destination byte */
 doffset = doffset%CHAR_BIT; /* index WITHIN first destination byte */
 sblen = CHAR_BIT - soffset; /* assume we go to byte boundary (std form) */

 printf("sindex = %d soffset = %d  dindex = %d doffset = %d sblen = %d\n",
   sindex,soffset,dindex,doffset,sblen);
 while(slen > 0){
   tmp = 0;
   tmp = (unsigned int) src[sindex++];   /* Put current source byte into workspace. */
   tmp = 255;
   printf("Source byte %2d= ",sindex-1);
   /* dumpbitwin((char *)&tmp,4,0,32); */
   printf("\n");
   /*
    * This signals the final byte to process
    */
   if(sblen >= slen){
     sblen = slen;                            /* number of bits we get */
   }
   tmp = tmp >> (CHAR_BIT - soffset - sblen); /* right shift to byte edge */
   soffset = CHAR_BIT - sblen;                /* fix offset */

   /*
    * tmp is now in "standard form" -- right aligned, with
    * sblen = CHAR_BIT - soffset.  We don't care how we got there -- now
    * we just put it away.
    */
   tmp = tmp << (CHAR_BIT+soffset-doffset);   /* align with target bytes */
   dst[dindex] += btmp[0];                    /* always add in left byte */

   /*
    * This is the final piece of trickiness.  If the left byte is too small
    * to reach the right margin of dst[dindex], we do NOT increment dindex,
    * instead we increment doffset by sblen and are done.  Otherwise we
    * go ahead and increment dindex and add in the second byte.  We have to
    * be careful with the boundary case where sblen PRECISELY fills the
    * first byte, as then we want to increment dindex but set doffset to 0.
    */
   if(soffset >= doffset){
     doffset += sblen;
     if(doffset == CHAR_BIT){
       dindex++;
       doffset = 0;
     }
   } else {
     dindex++;
     dst[dindex] = btmp[1];
     doffset = sblen - CHAR_BIT + doffset;
   }

   slen -= sblen;         /* This accounts for the chunk we've just gotten */

 }

}

/* static unsigned int pattern_output[BRBUF]; */

void get_rand_pattern(void *result,unsigned int rsize,int *pattern,gsl_rng *gsl_rng)
{

 int i,j,pindex,poffset;
 unsigned int bu,nbits,tmpuint;
 char *resultp;

 MYDEBUG(D_BITS) {
   printf("# get_rand_pattern: Initializing with rsize = %d\n",rsize);
 }

 /*
  * Count the number of bits in the actual returned object.
  */
 i = 0;
 nbits = 0;
 while(pattern[i]){
   if(pattern[i]>0) nbits += pattern[i];
   /*
    * Sorry, I want to use a uint to hold snippets from get_rand_bits().
    * So we must bitch if we try to use more and quit.
    */
   if(pattern[i]>32) {
     fprintf(stderr,"Error: pattern[%d] = %d chunks must not exceed 32 in length.\n",i,pattern[i]);
     fprintf(stderr,"         Use contiguous 32 bit pieces to create a longer chunk.\n");
     exit(0);
   }
   MYDEBUG(D_BITS) {
     printf("# get_rand_pattern: pattern[%d] = %d nbits = %u\n",i,pattern[i],nbits);
   }
   i++;
 }

 /*
  * Zero the return.  Note rsize is in characters/bytes.
  */
 memset(result,0,rsize);

 /*
  * We have to do a bit of testing on call parameters.  We cannot return
  * more bits than the result buffer will hold.  We return 0 if nbits = 0.
  * We cannot return more bits than bits_randbuf[] or result[] will hold.
  */
 bu = sizeof(unsigned int)*CHAR_BIT;
 if(nbits == 0) return;  /* Handle a "dumb call" */
 if(nbits > (BRBUF-2)*bu){
   fprintf(stderr,"Warning:  get_rand_bits capacity exceeded!\n");
   fprintf(stderr," nbits = %d > %d (nbits max)\n",nbits,(BRBUF-2)*bu);
   return;
 }
 if(nbits > rsize*CHAR_BIT){
   fprintf(stderr,"Warning:  Cannot get more bits than result vector will hold!\n");
   fprintf(stderr," nbits = %d > %d (rsize max bits)\n",nbits,rsize*CHAR_BIT);
   return;   /* Unlikely, but possible */
 }

 /*
  * This should really be pretty simple.  nbits holds the displacement
  * BACKWARDS from the end of the return.  We therefore:
  *
  *   a) Get the ith chunk into tmpuint OR iterate calls to skip ith bits.
  *   b) if we got a chunk, fill it into resultp[] bytewise by e.g.
  *      rotating left or right to align the piece and masking it into
  *      place.  decrement nbits as we go by the number of bits we've filled
  *      in (adjusting resultp index and bit pointer as we go).
  *   c) iterate until pattern[i] == 0 AND nbits = 0 (done) and return.
  *
  * Get and align addresses of char *pointers for (void *)result and
  * tmpuint to make it relatively easy to line up and generate result bytes.
  */

 /*
  * Set up index and pointer into resultp as usual.  Remember this is
  * BYTEWISE, not UINTWISE.
  *
  * For example, suppose rsize = 4 bytes and nbits = 28 bits.  Then
  * we want pindex = 0 (it starts to fill in the first byte of resultp[])
  * and poffset = 4.  28/8 = 3 (remainder 4) so:
  */
 pindex = rsize - nbits/CHAR_BIT - 1;
 poffset = nbits%CHAR_BIT;

 while(nbits != 0){

   if(pattern[i] > 0){

     /*
      * Get pattern[i] bits (in uint chunks)
      */
     j = pattern[i];
     while(j>bu) {

       get_rand_bits(&tmpuint,sizeof(unsigned int),bu,rng);
       /*
        * Pack this whole uint into result at the offset.
	*/
       mybitadd((char *)(&resultp + pindex),poffset,(char *)&tmpuint,0,bu);

       /*
        * Decrement j, increment pindex, poffset remains unchanged.
        */
       j -= bu;
       pindex += sizeof(unsigned int);

     }

     get_rand_bits(&tmpuint,sizeof(unsigned int),j,rng);
     /*
      * Pack this partial uint into resultp
      */
     mybitadd((char *)(&resultp + pindex),poffset,(char *)&tmpuint,bu-j,j);

     /*
      * Done with pattern, decrement nbits
      */
     nbits -= pattern[i];
      
   } else if(pattern[i] < 0){

     /* Skip -pattern[i] bits */
     j = -pattern[i];
     while(j>bu) {
       /* skip whole uint's worth */
       get_rand_bits(&tmpuint,sizeof(unsigned int),bu,rng);
       j -= bu;

     }
     /* skip final remaining <bu chunk */
     get_rand_bits(&tmpuint,sizeof(unsigned int),j,rng);

   } else {

     /* We SHOULD terminate by running out of nbits exactly... */
     fprintf(stdout,"# get_rand_pattern():  Sorry, this cannot happen.\n\
    If it did, then you're in deep trouble bugwise.  Refer to rgb.\n");
     exit(0);

   }

 }
 

}

/*
 * The bits.c module doesn't malloc anything, but it does maintain some
 * static buffers that must be cleared in order to achieve consistent
 * results from a rng reseed on, per run.
 */
void reset_bit_buffers()
{

  int i;
  
  bits_rand[0] = bits_rand[1] = 0;
  bleft = -1;
  for(i = 0;i<BRBUF;i++){
    bits_randbuf[i] = 0;
    bits_output[i] = 0;
  }
  brindex = -1;
  iclear = -1;
  bitindex = -1;

}
