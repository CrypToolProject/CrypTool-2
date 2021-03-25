/* rng_es
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
#include "rijndael-alg-fst.h"

// State blocks = 1 for weakest form
#define STATE_BLOCKS (1)	/* MUST be 1 for AES */
#define BLOCKS_SIZE (16 * STATE_BLOCKS)
#define NR 10

/*
 * This is a wrapping of the AES algorithm as a generator
 */

typedef struct {
	u32 rk[4*(NR + 1)];
	unsigned char block[BLOCKS_SIZE];
	short int pos;
} AES_state_t;

unsigned long int aes_get (void *vstate);
double aes_get_double (void *vstate);
void aes_set (void *vstate, unsigned long int s);

unsigned long int aes_get (void *vstate) {
	AES_state_t *state = vstate;
	unsigned int ret;

	if (state->pos + sizeof(ret) > BLOCKS_SIZE) {
		rijndaelEncrypt(state->rk, NR, state->block, state->block);
		state->pos = 0;
	}

	ret = *((unsigned int *) (state->block + state->pos));
	state->pos += sizeof(ret);

//	ret &= 0x7fffffff;
	return(ret);
}


double aes_get_double (void *vstate) {
//	return aes_get_long(vstate) / (double) ULONG_MAX;
	return (double) aes_get(vstate) / (double) (UINT_MAX >> 0);
}

void aes_set (void *vstate, unsigned long int s) {
	AES_state_t *state = vstate;
	int i;
	u8 key[16];

	memset(state, 0, sizeof(*state));	// Zero pos and block

	/* Make sure to use all bits of s in the key:
	 * (5 * i) % 26 => {0,5,10,15,20,25,4,9,14,19,24,3,8,13,18,23}
	 * */
	for (i = 0; i < 16; i++) {
		key[i] = (u8) (112 + i + (s >> ((5 * i) % 26)));
	}
	rijndaelKeySetupEnc(state->rk, key, 128);
	rijndaelEncrypt(state->rk, NR, state->block, state->block);

	return;
}

static const gsl_rng_type aes_type = {
	"AES_OFB",	/* name */
	UINT_MAX>>0,	// UINT_MAX,			/* RAND_MAX */
	0,					/* RAND_MIN */
	sizeof (AES_state_t),
	&aes_set,
	&aes_get,
	&aes_get_double};

const gsl_rng_type *gsl_rng_aes = &aes_type;

