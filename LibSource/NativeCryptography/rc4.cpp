/*
Copyright 2016 Nils Kopal, University of Kassel

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

#include "rc4.h"
#include <stdio.h>
#include <string.h>

unsigned char* RC4::crypt(unsigned char* textin, unsigned char* textout, const unsigned char* keyin, const int textlength, const int keylength)
{
	unsigned char sbox[256];
	unsigned char key[256], k;
	int  m, n, i, j;

	memset(sbox, 0, 256);
	memset(key, 0, 256);

	i = 0, j = 0, n = 0;

	for (int m = 0; m < 256; m++)
	{
		*(key + m) = *(keyin + (m % keylength));
		*(sbox + m) = m;
	}
	for (m = 0; m < 256; m++)
	{
		n = (n + *(sbox + m) + *(key + m)) & 0xff;
		SWAP(*(sbox + m), *(sbox + n));
	}
	
	for (m = 0; m < textlength; m++)
	{
		i = (i + 1) & 0xff;
		j = (j + *(sbox + i)) & 0xff;
		SWAP(*(sbox + i), *(sbox + j));
		k = *(sbox + ((*(sbox + i) + *(sbox + j)) & 0xff));
		*(textout + m) = *(textin + m) ^ k;
	}

	return textout;
}
