/*
 *  rng_ca.c
 * 
 *  Copyright (C) Tony Pasqualoni / Sept. 20, 2006
 *
 *  From:
 *
 *      http://home.southernct.edu/~pasqualonia1/ca/report.html
 *
 *  GSL-style packaging for dieharder by Robert G. Brown 2/27/07
 *
 *  Cellular automaton random number generator
 *  Uses a 256-state automaton to generate random sequences of
 *  32-bit unsigned integers.
 *
 *========================================================================
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or (at
 * your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 */

#include "libdieharder.h"

/*
 * This is a wrapping of the /dev/random hardware rng
 */

static unsigned long int ca_get (void *vstate);
static double ca_get_double (void *vstate);
static void ca_set (void *vstate, unsigned long int s);

typedef struct
  {
    FILE *fp;
  }
ca_state_t;

#define CA_WIDTH 2056   // width in cells of cellular automaton
#define RULESIZE 511    // amount of elements in rule table

static unsigned char init_config[CA_WIDTH];  // initial configuration of CA
static unsigned char * first_cell;           // leftmost cell
static unsigned char * last_cell;            // rightmost cell
static unsigned char * cell_d;               // current cell

unsigned int rule[RULESIZE] = {
   100,75,16,3,229,51,197,118,24,62,198,11,141,152,241,188,2,17,71,47,179,177,126,231,202,243,59,25,77,196,30,134,
   199,163,34,216,21,84,37,182,224,186,64,79,225,45,143,20,48,147,209,221,125,29,99,12,46,190,102,220,80,215,242,
   105,15,53,0,67,68,69,70,89,109,195,170,78,210,131,42,110,181,145,40,114,254,85,107,87,72,192,90,201,162,122,86,
   252,94,129,98,132,193,249,156,172,219,230,153,54,180,151,83,214,123,88,164,167,116,117,7,27,23,213,235,5,65,124,
   60,127,236,149,44,28,58,121,191,13,250,10,232,112,101,217,183,239,8,32,228,174,49,113,247,158,106,218,154,66,
   226,157,50,26,253,93,205,41,133,165,61,161,187,169,6,171,81,248,56,175,246,36,178,52,57,212,39,176,184,185,245,
   63,35,189,206,76,104,233,194,19,43,159,108,55,200,155,14,74,244,255,222,207,208,137,128,135,96,144,18,95,234,
   139,173,92,1,203,115,223,130,97,91,227,146,4,31,120,211,38,22,138,140,237,238,251,240,160,142,119,73,103,166,33,
   148,9,111,136,168,150,82,204,100,75,16,3,229,51,197,118,24,62,198,11,141,152,241,188,2,17,71,47,179,177,126,231,
   202,243,59,25,77,196,30,134,199,163,34,216,21,84,37,182,224,186,64,79,225,45,143,20,48,147,209,221,125,29,99,12,
   46,190,102,220,80,215,242,105,15,53,0,67,68,69,70,89,109,195,170,78,210,131,42,110,181,145,40,114,254,85,107,87,
   72,192,90,201,162,122,86,252,94,129,98,132,193,249,156,172,219,230,153,54,180,151,83,214,123,88,164,167,116,117,
   7,27,23,213,235,5,65,124,60,127,236,149,44,28,58,121,191,13,250,10,232,112,101,217,183,239,8,32,228,174,49,113,
   247,158,106,218,154,66,226,157,50,26,253,93,205,41,133,165,61,161,187,169,6,171,81,248,56,175,246,36,178,52,57,
   212,39,176,184,185,245,63,35,189,206,76,104,233,194,19,43,159,108,55,200,155,14,74,244,255,222,207,208,137,128,
   135,96,144,18,95,234,139,173,92,1,203,115,223,130,97,91,227,146,4,31,120,211,38,22,138,140,237,238,251,240,160,
   142,119,73,103,166,33,148,9,111,136,168,150,82
};

static _inline unsigned long int
ca_get (void *vstate)
{
  /* Returns a 32-bit unsigned integer produced by the automaton */

  /*
   * cell addresses (these cells and cell_d serve as pointers to the 4
   * bytes of the next integer)
   */
  unsigned char * cell_a;
  unsigned char * cell_b;
  unsigned char * cell_c;

  /* set cell addresses using address of current cell (cell_d) */
  cell_c = cell_d - 1; 
  cell_b = cell_c - 1; 
  cell_a = cell_b - 1; 

  /* update cell states using rule table */
  *cell_d = rule[*cell_c + *cell_d]; 
  *cell_c = rule[*cell_b + *cell_c]; 
  *cell_b = rule[*cell_a + *cell_b]; 

  /*
   * update the state of cell_a and shift current cell (cell_d) to the
   * left by 4 bytes (first_cell - 1 = last_cell)
   */
  if (cell_a == first_cell) {
    *cell_a = rule[*cell_a]; 
    cell_d = last_cell; 
    return( *((unsigned int *)cell_a) ); 
  } else { 
    *cell_a = rule[*(cell_a - 1) + *cell_a];
    cell_d -= 4; 
    return( *((unsigned int *)cell_a) ); 
  }

}

static double
ca_get_double (void *vstate)
{
  return ca_get (vstate) / (double) UINT_MAX;
}

static void
ca_set (void *vstate, unsigned long int s) {

 /* Initialize automaton using specified seed. */
_Check_return_
 ca_state_t *state = (ca_state_t *) vstate;


 int i;

 /* clear cells */
 for (i = 0; i < CA_WIDTH - 1; i++) 

   init_config[i] = 0;

   /* set initial cell states using seed */
   init_config[CA_WIDTH - 1] = (unsigned char)(seed);
   init_config[CA_WIDTH - 2] = (unsigned char)(seed << 8);
   init_config[CA_WIDTH - 3] = (unsigned char)(seed << 16);
   init_config[CA_WIDTH - 4] = (unsigned char)(seed << 24);
   if (seed != 0xFFFFFFFF)
      seed++;
   for (i = 0; i < CA_WIDTH - 4; i++) 
     init_config[i] = (unsigned char) ( seed >> (i % 32) );

   /* define addresses of first_cell and last_cell */
   first_cell = init_config;
   last_cell = init_config + CA_WIDTH - 1;

   /*
    * set address of current cell to last_cell (automaton is updated right
    * to left)
    */
   cell_d = last_cell;

   /* evolve automaton before returning integers */
   for (i = 0 ; i < ( (CA_WIDTH * CA_WIDTH) / 4.0); i++) 
      ca_get(vstate);

}

static const gsl_rng_type ca_type =
{"ca",                          /* name */
 UINT_MAX,			/* RAND_MAX */
 0,				/* RAND_MIN */
 sizeof (ca_state_t),
 &ca_set,
 &ca_get,
 &ca_get_double};

const gsl_rng_type *gsl_rng_ca = &ca_type;
