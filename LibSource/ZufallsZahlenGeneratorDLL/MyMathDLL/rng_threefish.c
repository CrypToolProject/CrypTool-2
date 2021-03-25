/* rng_threefish
 * 
 * Copyright (C) 1996, 1997, 1998, 1999, 2000 James Theiler, Brian Gough
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
#include "skein.h"

// State blocks = 1 for weakest form
#define STATE_BLOCKS (1)
#define BLOCKS_SIZE (64 * STATE_BLOCKS)

#define Threefish_512_Process_Blocks(a,b,c,d) Threefish_512_Process_Blocks64(a,b,c,d)

/*
 * This is a wrapping of the Threefish algorithm as a generator
 */

typedef struct {
	Threefish_512_Ctxt_t ctx;
	unsigned char block[BLOCKS_SIZE];
	short int pos;
} Threefish_state_t;

unsigned long int threefish_get (void *vstate);
double threefish_get_double (void *vstate);
void threefish_set (void *vstate, unsigned long int s);

#ifndef BADRNG
unsigned long int threefish_get_long (void *vstate) {
	Threefish_state_t *state = vstate;
	unsigned long int ret;

	if (state->pos + sizeof(unsigned long int) > BLOCKS_SIZE) {
		Threefish_512_Process_Blocks(&(state->ctx), state->block, state->block, STATE_BLOCKS);
		state->pos = 0;
	}

	ret = *((unsigned long int *) (state->block + state->pos));
	state->pos += sizeof(unsigned long int);

	return(ret);
}

unsigned long int threefish_get (void *vstate) {
	Threefish_state_t *state = vstate;
	unsigned int ret;

	if (state->pos + sizeof(ret) > BLOCKS_SIZE) {
		Threefish_512_Process_Blocks(&(state->ctx), state->block, state->block, STATE_BLOCKS);
		state->pos = 0;
	}

	ret = *((unsigned int *) (state->block + state->pos));
	state->pos += sizeof(ret);

//	ret &= 0x7fffffff;
	return(ret);
}


double threefish_get_double (void *vstate) {
//	return threefish_get_long(vstate) / (double) ULONG_MAX;
	return (double) threefish_get(vstate) / (double) (UINT_MAX >> 0);
}

void threefish_set (void *vstate, unsigned long int s) {
	Threefish_state_t *state = vstate;
	int i;
	unsigned long int *blockptr;

	memset(state, 0, sizeof(*state));
	for (i = 0; i < 16; i++) ((unsigned char *) state->ctx.T)[i] = 112 + i;
	for (i = 0; i < 64; i++) ((unsigned char *) state->ctx.Key)[i] = 64 + i;

        /*
	 * This causes the dread "dereferencing type-punned pointer" warning
	 * and breaks strict aliasing rules.  It seems to say
	 * "set the contents of the unsigned long int pointer state->block
	 * to be s".  OTOH, we see unsigned char block[BLOCKS_SIZE],
	 * suggesting that block is a character string.  BLOCKS_SIZE
	 * seems to be de facto 64, so block is basically a chunk of
	 * memory that is 64 bytes long.  The unsigned long int s,
	 * OTOH, is only 32 bits long.  This, therefore, sets only the
	 * first four bytes of it, and makes the compiler complain.
	 * I'm going to try to make exactly the same thing happen a
	 * different way.
	*((unsigned long int *) state->block) = s;
	 */
	blockptr = (unsigned long int*) &state->block;
	*blockptr = s;
	Threefish_512_Process_Blocks(&(state->ctx), state->block, state->block, 1);

	for (i = 1; i < STATE_BLOCKS; i++) {
		state->ctx.T[0] = i;
		Threefish_512_Process_Blocks(&(state->ctx), state->block, state->block+(i*64), 1);
	}
	state->ctx.T[0] = 112;

	return;
}

static const gsl_rng_type threefish_type = {
	"Threefish_OFB",	/* name */
	UINT_MAX>>0,	// UINT_MAX,			/* RAND_MAX */
	0,					/* RAND_MIN */
	sizeof (Threefish_state_t),
	&threefish_set,
	&threefish_get,
	&threefish_get_double};

const gsl_rng_type *gsl_rng_threefish = &threefish_type;
#endif

