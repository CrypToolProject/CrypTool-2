/*
 * $Id: rank.c 223 2006-08-17 06:19:38Z rgb $
 *
 * See copyright in copyright.h and the accompanying file COPYING
 *
 */

/*
 *========================================================================
 * This code evaluates the binary rank of a bit matrix on the field[0,1],
 * for example:
 *
 *   0 1 1 0
 *   1 0 1 0
 *   1 0 1 1
 *   0 0 1 1
 *
 * would be a 4x4 bitwise matrix.  The rank of the matrix is determined
 * as is the rank of any matrix, by gauss elimination with pivoting, but
 * in this case the "pivots" are 1 bits (the largest element in any
 * column or row) and the required transpositions and subtractions are
 * all done by pointer or bitwise (xor) operations.
 * 
 *========================================================================
 */


#include "libdieharder.h"


int binary_rank(uint **mtx,int mrows,int ncols)
{

 int i,j,k,m,n,s_uint,rank;
 int col_ind,uint_col_max;
 uint mask,colchk;
 uint *rowp;

 /*
  * mtx is a vector of unsigned integers, e.g.:
  *
  * mtx[0]       = 0110...110
  * mtx[1]       = 1010...001
  * ...
  * mtx[ncols-1] = 0010...100
  *
  * We go through this vector a row at a time, searching each
  * column for a pivot (1).  When we find a pivot, we swap rows
  * and eliminate the column bitwise.
  */

 /*
  * size of an unsigned int
  */
 s_uint = 8*sizeof(uint);
 /*
  * row size in uint columns.  Note that we have to remember
  * which uint ** column we are in and therefore have to
  * convert bit column into uint column regularly.
  * Subtract 1, because it is zero-basd.
  */
 uint_col_max = (ncols-1)/s_uint;

 if(verbose == D_BRANK || verbose == D_ALL){
   printf("Starting bitmatrix:\n");
   for(m=0;m<mrows;m++){
     printf("# br: ");
     dumpbits(&mtx[m][0],32);
   }
 }

 rank = 0;
 i = 0;
 mask = 1;
 /*
  * j is the column BIT index, which can be
  * larger than s_uint (or rmax_bits).
  */
 for(j = 0;j < ncols && i < mrows;j++){
   /*
    * This is the mxt[i][j] index of the
    * current bit column.
    */
   col_ind = j/s_uint; 

   /*
    * This handles the transition to the next
    * uint when the bit index j passes the uint
    * boundary.  mask picks out the correct bit
    * column from right to left (why not from
    * left to right?).
    */
   j%s_uint ? (mask <<=1) : (mask = 1);

   /*
    * Find a pivot element (a 1) if there is one
    * in this column (column fixed by mask).
    */
   if(verbose == D_BRANK || verbose == D_ALL){
     printf("Checking column mask ");
     dumpbits(&mask,32);
   }
   for(k=i;k<mrows;k++){
     colchk = mtx[k][col_ind]&mask;
     if(verbose == D_BRANK || verbose == D_ALL){
       printf("row %d = ",k);
       dumpbits(&colchk,32);
     }
     if(colchk) break;
   }

   /*
    * OK, k either points to a row with a "1" in
    * the mask column or it equals mrows.  In the latter
    * case, the entire column from i on down is zero
    * and we move on without incrementing rank.
    *
    * Otherwise, we bring the pivot ROW to the ith position
    * (which may involve doing nothing).  k is set to
    * point to the first location that could still be
    * a 1, which is always k+1;
    */
   if(k < mrows){
     if(verbose == D_BRANK || verbose == D_ALL){
       printf("Swapping %d and %d rows. before bitmatrix:\n",i,k);
       for(m=0;m<mrows;m++){
         printf("# br: ");
         dumpbits(&mtx[m][col_ind],32);
       }
     }
     if(i != k){
       if(verbose == D_BRANK || verbose == D_ALL){
         printf("before: mtx[%d] = %p  mtx[%d = %p\n",i,(void*) mtx[i],k,(void*) mtx[k]);
       }
       rowp = mtx[i]; /* rowp is the ADDRESS of the ith row */
       mtx[i] = mtx[k];
       mtx[k] = rowp;
       if(verbose == D_BRANK || verbose == D_ALL){
         printf("after mtx[%d] = %p  mtx[%d = %p\n",i,(void*) mtx[i],k,(void*) mtx[k]);
       }
     }
     if(verbose == D_BRANK || verbose == D_ALL){
       printf("Swapped %d and %d rows. after bitmatrix:\n",i,k);
       for(m=0;m<mrows;m++){
         printf("# br: ");
         dumpbits(&mtx[m][col_ind],32);
       }
     }
     k++;  /* First row that could have a 1 in this column after i */

     /*
      * Now we eliminate the rest of the column, by rows, starting
      * at k.
      */
     while(k<mrows){
       /*
        * if the row also has a 1 in this column...
        */
       if(mtx[k][col_ind] & mask){
         /*
          * we use exclusive or to eliminate the
          * rest of the column.
          */
         n = uint_col_max - col_ind;
         if(verbose == D_BRANK || verbose == D_ALL){
           printf("eliminating against row %2d = ",i);
           dumpbits(&mtx[i][col_ind],32);
           printf("eliminating row %2d, before = ",k);
           dumpbits(&mtx[k][col_ind],32);
         }
         while (n >= 0){
           if(verbose == D_BRANK || verbose == D_ALL){
             printf("xoring column = %2d\n",n);
	   }
           mtx[k][n] ^= mtx[i][n];
	   n--;
         }
         if(verbose == D_BRANK || verbose == D_ALL){
           printf("eliminating row %2d, after  = ",k);
           dumpbits(&mtx[k][col_ind],32);
           printf("\n");
         }
       }
       k++;
     }
     if(verbose == D_BRANK || verbose == D_ALL){
       printf("Eliminated. New bitmatrix:\n");
       for(m=0;m<mrows;m++){
         printf("# br: ");
         dumpbits(&mtx[m][col_ind],32);
       }
     }
     i++;
     if(verbose == D_BRANK || verbose == D_ALL){
       printf("NEW RANK = %d\n",i);
     }
   }
 }

 return(i);

}

