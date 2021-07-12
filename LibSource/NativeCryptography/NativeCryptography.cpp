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

#include "NativeCryptography.h"

namespace NativeCryptography {
	
	/*
	 * XOR of two 64bit blocks
	 * x = x XOR y 
	 */
	inline void xor64(void* x, void* y){
		((long*)x)[0] ^= ((long*)y)[0];
	}

	/*
	* XOR of two 128bit blocks
	* x = x XOR y
	*/
	inline void xor128(void* x, void* y){
		((long*)x)[0] ^= ((long*)y)[0];
		((long*)x)[1] ^= ((long*)y)[1];
	}

	/*
	 * Calculate the amount of blocks of size 8 bytes
	 */
	inline int blockAmount8(int length){
		if ((length & 7) == 0){
			return length >> 3;
		}
		return (length >> 3) + 1;
		
	}

	/*
	* Calculate the amount of blocks of size 16 bytes
	*/
	inline int blockAmount16(int length){
		if ((length & 15) == 0){
			return length >> 4;
		}
		return (length >> 4) + 1;
	}

	//Data Encryption Standard

	/*
	 * Decrypt DES with ECB mode
	 */
	array<unsigned char>^ Crypto::decryptDES_ECB(array<unsigned char>^ Input, array<unsigned char>^ Key, const int length){
		
		unsigned int numBlocks = blockAmount8(length);

		array<unsigned char>^ output = gcnew array<unsigned char>(length);

		pin_ptr<unsigned char> input = &Input[0];
		pin_ptr<unsigned char> key = &Key[0];
		pin_ptr<unsigned char> outp = &output[0];

		DES_key_schedule deskey;
		DES_set_key_unchecked((const_DES_cblock*)key, &deskey);
		
		for (unsigned int c = 0; c < numBlocks; c++)
		{		
			DES_ecb_encrypt((const_DES_cblock*)(input + c * 8), (const_DES_cblock*)(outp + c * 8), &deskey, DES_DECRYPT);
		}
		return output;
	}
	
	/*
	 * Decrypt DES with CBC mode
	 */
	array<unsigned char>^ Crypto::decryptDES_CBC(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length){

		unsigned int numBlocks = blockAmount8(length);
		
		if (IV == nullptr)
		{
			IV = zeroIV8;			
		}

		array<unsigned char>^ output = gcnew array<unsigned char>(length);

		pin_ptr<unsigned char> input = &Input[0];
		pin_ptr<unsigned char> key = &Key[0];
		pin_ptr<unsigned char> iv = &IV[0];
		pin_ptr<unsigned char> outp = &output[0];

		DES_key_schedule deskey;
		DES_set_key_unchecked((const_DES_cblock*)key, &deskey);

		//1. block
		DES_ecb_encrypt((const_DES_cblock*)input, (const_DES_cblock*)outp, &deskey, DES_DECRYPT);
		xor64(outp, iv);

		//rest of blocks
		for (unsigned int c = 1; c < numBlocks; c++)
		{
			DES_ecb_encrypt((const_DES_cblock*)(input + c * 8), (const_DES_cblock*)(outp + c * 8), &deskey, DES_DECRYPT);
			xor64(outp + c * 8, input + (c - 1) * 8);
		}
		return output;
	}

	/*
	* Decrypt DES with CFB mode
	*/
	array<unsigned char>^ Crypto::decryptDES_CFB(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length){
			
		if (IV == nullptr)
		{
			IV = zeroIV8;
		}

		array<unsigned char>^ output = gcnew array<unsigned char>(length);

		pin_ptr<unsigned char> input = &Input[0];
		pin_ptr<unsigned char> key = &Key[0];
		pin_ptr<unsigned char> iv = &IV[0];
		pin_ptr<unsigned char> outp = &output[0];

		DES_key_schedule deskey;
		DES_set_key_unchecked((const_DES_cblock*)key, &deskey);

		unsigned char block[8];
		unsigned char shiftregister[8];
		//works only for little endian architectures:
		
		*((unsigned int*)shiftregister) = *((unsigned int*)&iv[1]);
		*((unsigned int*)&shiftregister[4]) = (*((unsigned int*)&iv[4]) >> 8) | ((unsigned int)(input[0]) << 24);

		DES_ecb_encrypt((const_DES_cblock*)iv, (const_DES_cblock*)block, &deskey, DES_ENCRYPT);

		unsigned char leftmost = block[0];
		outp[0] = leftmost ^ input[0];

		for (int i = 1; i < length; i++)
		{					
			DES_ecb_encrypt((const_DES_cblock*)shiftregister, (const_DES_cblock*)block, &deskey, DES_ENCRYPT);
		
			leftmost = block[0];
			outp[i] = leftmost ^ input[i];
			
			*((unsigned int*)shiftregister) = *((unsigned int*)&shiftregister[1]);
			*((unsigned int*)&shiftregister[4]) = (*((unsigned int*)&shiftregister[4]) >> 8) | ((unsigned int)input[i] << 24);			
		}
	
		return output;	
	}

	//3DES

	/*
	* Decrypt 3DES with ECB mode
	*/
	array<unsigned char>^ Crypto::decrypt3DES_ECB(array<unsigned char>^ Input, array<unsigned char>^ Key, const int length){
		
		unsigned int numBlocks = blockAmount8(length);

		array<unsigned char>^ output = gcnew array<unsigned char>(length);

		pin_ptr<unsigned char> input = &Input[0];
		pin_ptr<unsigned char> key1 = &Key[0];
		pin_ptr<unsigned char> key2 = &Key[8];
		pin_ptr<unsigned char> outp = &output[0];

		DES_key_schedule deskey1;
		DES_key_schedule deskey2;
		
		DES_set_key_unchecked((const_DES_cblock*)key1, &deskey1);
		DES_set_key_unchecked((const_DES_cblock*)key2, &deskey2);

		for (unsigned int c = 0; c < numBlocks; c++)
		{
			DES_ecb_encrypt((const_DES_cblock*)(input + c * 8), (const_DES_cblock*)(outp + c * 8), &deskey1, DES_DECRYPT);
			DES_ecb_encrypt((const_DES_cblock*)(outp + c * 8), (const_DES_cblock*)(outp + c * 8), &deskey2, DES_ENCRYPT);
			DES_ecb_encrypt((const_DES_cblock*)(outp + c * 8), (const_DES_cblock*)(outp + c * 8), &deskey1, DES_DECRYPT);
		}
		return output;
	}

	/*
	* Decrypt 3DES with CBC mode
	*/
	array<unsigned char>^ Crypto::decrypt3DES_CBC(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length){
		
		unsigned int numBlocks = blockAmount8(length);

		array<unsigned char>^ output = gcnew array<unsigned char>(length);

		pin_ptr<unsigned char> input = &Input[0];
		pin_ptr<unsigned char> iv = &IV[0];
		pin_ptr<unsigned char> key1 = &Key[0];
		pin_ptr<unsigned char> key2 = &Key[8];		
		pin_ptr<unsigned char> outp = &output[0];

		DES_key_schedule deskey1;
		DES_key_schedule deskey2;

		DES_set_key_unchecked((const_DES_cblock*)key1, &deskey1);
		DES_set_key_unchecked((const_DES_cblock*)key2, &deskey2);

		//1. block
		DES_ecb_encrypt((const_DES_cblock*)input, (const_DES_cblock*)outp, &deskey1, DES_DECRYPT);
		DES_ecb_encrypt((const_DES_cblock*)outp, (const_DES_cblock*)outp, &deskey2, DES_ENCRYPT);
		DES_ecb_encrypt((const_DES_cblock*)outp, (const_DES_cblock*)outp, &deskey1, DES_DECRYPT);
		xor64(outp, iv);

		//rest of blocks
		for (unsigned int c = 1; c < numBlocks; c++)
		{
			DES_ecb_encrypt((const_DES_cblock*)(input + c * 8), (const_DES_cblock*)(outp + c * 8), &deskey1, DES_DECRYPT);
			DES_ecb_encrypt((const_DES_cblock*)(outp + c * 8), (const_DES_cblock*)(outp + c * 8), &deskey2, DES_ENCRYPT);
			DES_ecb_encrypt((const_DES_cblock*)(outp + c * 8), (const_DES_cblock*)(outp + c * 8), &deskey1, DES_DECRYPT);
			xor64(outp + c * 8, input + (c - 1) * 8);
		}
		return output;
	}

	/*
	* Decrypt 3DES with CFB mode
	*/
	array<unsigned char>^ Crypto::decrypt3DES_CFB(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length){
		
		if (IV == nullptr)
		{
			IV = zeroIV8;
		}

		array<unsigned char>^ output = gcnew array<unsigned char>(length);

		pin_ptr<unsigned char> input = &Input[0];
		pin_ptr<unsigned char> key1 = &Key[0];
		pin_ptr<unsigned char> key2 = &Key[8];
		pin_ptr<unsigned char> iv = &IV[0];
		pin_ptr<unsigned char> outp = &output[0];

		DES_key_schedule deskey1;
		DES_key_schedule deskey2;
		DES_set_key_unchecked((const_DES_cblock*)key1, &deskey1);
		DES_set_key_unchecked((const_DES_cblock*)key2, &deskey2);

		unsigned char block[8];
		unsigned char shiftregister[8];
		//works only for little endian architectures:

		*((unsigned int*)shiftregister) = *((unsigned int*)&iv[1]);
		*((unsigned int*)&shiftregister[4]) = (*((unsigned int*)&iv[4]) >> 8) | ((unsigned int)(input[0]) << 24);

		DES_ecb_encrypt((const_DES_cblock*)iv, (const_DES_cblock*)block, &deskey1, DES_ENCRYPT);
		DES_ecb_encrypt((const_DES_cblock*)block, (const_DES_cblock*)block, &deskey2, DES_DECRYPT);
		DES_ecb_encrypt((const_DES_cblock*)block, (const_DES_cblock*)block, &deskey1, DES_ENCRYPT);

		unsigned char leftmost = block[0];
		outp[0] = leftmost ^ input[0];

		for (int i = 1; i < length; i++)
		{
			DES_ecb_encrypt((const_DES_cblock*)shiftregister, (const_DES_cblock*)block, &deskey1, DES_ENCRYPT);
			DES_ecb_encrypt((const_DES_cblock*)block, (const_DES_cblock*)block, &deskey2, DES_DECRYPT);
			DES_ecb_encrypt((const_DES_cblock*)block, (const_DES_cblock*)block, &deskey1, DES_ENCRYPT);

			leftmost = block[0];
			outp[i] = leftmost ^ input[i];

			*((unsigned int*)shiftregister) = *((unsigned int*)&shiftregister[1]);
			*((unsigned int*)&shiftregister[4]) = (*((unsigned int*)&shiftregister[4]) >> 8) | ((unsigned int)input[i] << 24);
		}

		return output;
	}

	//Advanced Encryption Standard

	/*
	* Decrypt AES with ECB mode
	*/
	array<unsigned char>^ Crypto::decryptAES128_ECB(array<unsigned char>^ Input, array<unsigned char>^ Key, const int length){
		
		unsigned int numBlocks = blockAmount16(length);

		array<unsigned char>^ output = gcnew array<unsigned char>(length);

		pin_ptr<unsigned char> input = &Input[0];
		pin_ptr<unsigned char> key = &Key[0];
		pin_ptr<unsigned char> outp = &output[0];

		AES_KEY aeskey;
		AES_set_decrypt_key(key, 128, &aeskey);

		for (unsigned int c = 0; c < numBlocks; c++)
		{
			AES_decrypt(input + c * 16, outp + c * 16, &aeskey);
		}
		return output;
	}
	
	/*
	* Decrypt AES with CBC mode
	*/
	array<unsigned char>^ Crypto::decryptAES128_CBC(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length){
	
		unsigned int numBlocks = blockAmount16(length);

		if (IV == nullptr)
		{
			IV = zeroIV8;
		}

		array<unsigned char>^ output = gcnew array<unsigned char>(length);

		pin_ptr<unsigned char> input = &Input[0];
		pin_ptr<unsigned char> key = &Key[0];
		pin_ptr<unsigned char> iv = &IV[0];
		pin_ptr<unsigned char> outp = &output[0];

		AES_KEY aeskey;
		AES_set_decrypt_key(key, 128, &aeskey);

		//1. block		
		AES_decrypt(input, outp, &aeskey);
		xor128(outp, iv);

		//rest of blocks
		for (unsigned int c = 1; c < numBlocks; c++)
		{
			AES_decrypt(input + c * 16, outp + c * 16, &aeskey);
			xor128(outp + c * 16, input + (c - 1) * 16);
		}
		return output;	
	}

	/*
	* Decrypt AES with ECB mode using AES NI 
	*/
	array<unsigned char>^ Crypto::decryptAES128_ECB_NI(array<unsigned char>^ Input, array<unsigned char>^ Key, const int length){

		unsigned int numBlocks = blockAmount16(length);

		array<unsigned char>^ output = gcnew array<unsigned char>(length);

		pin_ptr<unsigned char> input = &Input[0];
		pin_ptr<unsigned char> key = &Key[0];		
		pin_ptr<unsigned char> outp = &output[0];

		intel_AES_dec128(input, outp, key, numBlocks);
		return output;
	}

	/*
	* Decrypt AES with CBC mode using AES NI
	*/
	array<unsigned char>^ Crypto::decryptAES128_CBC_NI(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length){
	
		unsigned int numBlocks = blockAmount16(length);

		array<unsigned char>^ output = gcnew array<unsigned char>(length);

		pin_ptr<unsigned char> input = &Input[0];
		pin_ptr<unsigned char> key = &Key[0];
		pin_ptr<unsigned char> iv = &IV[0];
		pin_ptr<unsigned char> outp = &output[0];

		intel_AES_dec128_CBC(input, outp, key, numBlocks, iv);
		return output;

	}

	array<unsigned char>^ Crypto::decryptAES128_CFB(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length){ 

		if (IV == nullptr)
		{
			IV = zeroIV16;
		}

		array<unsigned char>^ output = gcnew array<unsigned char>(length);

		pin_ptr<unsigned char> input = &Input[0];
		pin_ptr<unsigned char> key = &Key[0];
		pin_ptr<unsigned char> iv = &IV[0];
		pin_ptr<unsigned char> outp = &output[0];
		
		AES_KEY aeskey;
		AES_set_encrypt_key(key, 128, &aeskey);

		unsigned char block[16];
		unsigned char shiftregister[16];
		//works only for little endian architectures:

		*((unsigned int*)shiftregister) = *((unsigned int*)&iv[1]);
		*((unsigned int*)&shiftregister[4]) = (*((unsigned int*)&iv[4]) >> 8) | ((unsigned int)iv[8] << 24);
		*((unsigned int*)&shiftregister[8]) = (*((unsigned int*)&iv[8]) >> 8) | ((unsigned int)iv[12] << 24);
		*((unsigned int*)&shiftregister[12]) = (*((unsigned int*)&iv[12]) >> 8) | ((unsigned int)input[0] << 24);

		AES_encrypt(iv, block, &aeskey);

		unsigned char leftmost = block[0];
		outp[0] = leftmost ^ input[0];

		for (int i = 1; i < length; i++)
		{
			AES_encrypt(shiftregister, block, &aeskey);

			leftmost = block[0];
			outp[i] = leftmost ^ input[i];

			*((unsigned int*)shiftregister) = *((unsigned int*)&shiftregister[1]);
			*((unsigned int*)&shiftregister[4]) = (*((unsigned int*)&shiftregister[4]) >> 8) | ((unsigned int)shiftregister[8] << 24);
			*((unsigned int*)&shiftregister[8]) = (*((unsigned int*)&shiftregister[8]) >> 8) | ((unsigned int)shiftregister[12] << 24);
			*((unsigned int*)&shiftregister[12]) = (*((unsigned int*)&shiftregister[12]) >> 8) | ((unsigned int)input[i] << 24);
		}

		return output;	
	}
	
	/*
	* Decrypt AES with ECB mode
	*/
	array<unsigned char>^ Crypto::decryptAES192_ECB(array<unsigned char>^ Input, array<unsigned char>^ Key, const int length){

		unsigned int numBlocks = blockAmount16(length);

		array<unsigned char>^ output = gcnew array<unsigned char>(length);

		pin_ptr<unsigned char> input = &Input[0];
		pin_ptr<unsigned char> key = &Key[0];
		pin_ptr<unsigned char> outp = &output[0];

		AES_KEY aeskey;
		AES_set_decrypt_key(key, 192, &aeskey);

		for (unsigned int c = 0; c < numBlocks; c++)
		{
			AES_decrypt(input + c * 16, outp + c * 16, &aeskey);
		}
		return output;
	}

	/*
	* Decrypt AES with CBC mode
	*/
	array<unsigned char>^ Crypto::decryptAES192_CBC(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length){

		unsigned int numBlocks = blockAmount16(length);

		if (IV == nullptr)
		{
			IV = zeroIV8;
		}

		array<unsigned char>^ output = gcnew array<unsigned char>(length);

		pin_ptr<unsigned char> input = &Input[0];
		pin_ptr<unsigned char> key = &Key[0];
		pin_ptr<unsigned char> iv = &IV[0];
		pin_ptr<unsigned char> outp = &output[0];

		AES_KEY aeskey;
		AES_set_decrypt_key(key, 192, &aeskey);

		//1. block		
		AES_decrypt(input, outp, &aeskey);
		xor128(outp, iv);

		//rest of blocks
		for (unsigned int c = 1; c < numBlocks; c++)
		{
			AES_decrypt(input + c * 16, outp + c * 16, &aeskey);
			xor128(outp + c * 16, input + (c - 1) * 16);
		}
		return output;
	}

	/*
	* Decrypt AES with ECB mode using AES NI
	*/
	array<unsigned char>^ Crypto::decryptAES192_ECB_NI(array<unsigned char>^ Input, array<unsigned char>^ Key, const int length){

		unsigned int numBlocks = blockAmount16(length);

		array<unsigned char>^ output = gcnew array<unsigned char>(length);

		pin_ptr<unsigned char> input = &Input[0];
		pin_ptr<unsigned char> key = &Key[0];
		pin_ptr<unsigned char> outp = &output[0];

		intel_AES_dec192(input, outp, key, numBlocks);
		return output;
	}

	/*
	* Decrypt AES with CBC mode using AES NI
	*/
	array<unsigned char>^ Crypto::decryptAES192_CBC_NI(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length){

		unsigned int numBlocks = blockAmount16(length);

		array<unsigned char>^ output = gcnew array<unsigned char>(length);

		pin_ptr<unsigned char> input = &Input[0];
		pin_ptr<unsigned char> key = &Key[0];
		pin_ptr<unsigned char> iv = &IV[0];
		pin_ptr<unsigned char> outp = &output[0];

		intel_AES_dec192_CBC(input, outp, key, numBlocks, iv);
		return output;

	}

		
	array<unsigned char>^ Crypto::decryptAES192_CFB(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length){ 
		
		if (IV == nullptr)
		{
			IV = zeroIV16;
		}

		array<unsigned char>^ output = gcnew array<unsigned char>(length);

		pin_ptr<unsigned char> input = &Input[0];
		pin_ptr<unsigned char> key = &Key[0];
		pin_ptr<unsigned char> iv = &IV[0];
		pin_ptr<unsigned char> outp = &output[0];

		AES_KEY aeskey;
		AES_set_encrypt_key(key, 192, &aeskey);

		unsigned char block[16];
		unsigned char shiftregister[16];
		//works only for little endian architectures:

		*((unsigned int*)shiftregister) = *((unsigned int*)&iv[1]);
		*((unsigned int*)&shiftregister[4]) = (*((unsigned int*)&iv[4]) >> 8) | ((unsigned int)iv[8] << 24);
		*((unsigned int*)&shiftregister[8]) = (*((unsigned int*)&iv[8]) >> 8) | ((unsigned int)iv[12] << 24);
		*((unsigned int*)&shiftregister[12]) = (*((unsigned int*)&iv[12]) >> 8) | ((unsigned int)input[0] << 24);

		AES_encrypt(iv, block, &aeskey);

		unsigned char leftmost = block[0];
		outp[0] = leftmost ^ input[0];

		for (int i = 1; i < length; i++)
		{
			AES_encrypt(shiftregister, block, &aeskey);

			leftmost = block[0];
			outp[i] = leftmost ^ input[i];

			*((unsigned int*)shiftregister) = *((unsigned int*)&shiftregister[1]);
			*((unsigned int*)&shiftregister[4]) = (*((unsigned int*)&shiftregister[4]) >> 8) | ((unsigned int)shiftregister[8] << 24);
			*((unsigned int*)&shiftregister[8]) = (*((unsigned int*)&shiftregister[8]) >> 8) | ((unsigned int)shiftregister[12] << 24);
			*((unsigned int*)&shiftregister[12]) = (*((unsigned int*)&shiftregister[12]) >> 8) | ((unsigned int)input[i] << 24);
		}

		return output;
	}
	
	/*
	* Decrypt AES with ECB mode
	*/
	array<unsigned char>^ Crypto::decryptAES256_ECB(array<unsigned char>^ Input, array<unsigned char>^ Key, const int length){

		unsigned int numBlocks = blockAmount16(length);

		array<unsigned char>^ output = gcnew array<unsigned char>(length);

		pin_ptr<unsigned char> input = &Input[0];
		pin_ptr<unsigned char> key = &Key[0];
		pin_ptr<unsigned char> outp = &output[0];

		AES_KEY aeskey;
		AES_set_decrypt_key(key, 256, &aeskey);

		for (unsigned int c = 0; c < numBlocks; c++)
		{
			AES_decrypt(input + c * 16, outp + c * 16, &aeskey);
		}
		return output;
	}

	/*
	* Decrypt AES with CBC mode
	*/
	array<unsigned char>^ Crypto::decryptAES256_CBC(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length){

		unsigned int numBlocks = blockAmount16(length);

		if (IV == nullptr)
		{
			IV = zeroIV8;
		}

		array<unsigned char>^ output = gcnew array<unsigned char>(length);

		pin_ptr<unsigned char> input = &Input[0];
		pin_ptr<unsigned char> key = &Key[0];
		pin_ptr<unsigned char> iv = &IV[0];
		pin_ptr<unsigned char> outp = &output[0];

		AES_KEY aeskey;
		AES_set_decrypt_key(key, 256, &aeskey);

		//1. block		
		AES_decrypt(input, outp, &aeskey);
		xor128(outp, iv);

		//rest of blocks
		for (unsigned int c = 1; c < numBlocks; c++)
		{
			AES_decrypt(input + c * 16, outp + c * 16, &aeskey);
			xor128(outp + c * 16, input + (c - 1) * 16);
		}
		return output;
	}

	/*
	* Decrypt AES with ECB mode using AES NI
	*/
	array<unsigned char>^ Crypto::decryptAES256_ECB_NI(array<unsigned char>^ Input, array<unsigned char>^ Key, const int length){

		unsigned int numBlocks = blockAmount16(length);

		array<unsigned char>^ output = gcnew array<unsigned char>(length);

		pin_ptr<unsigned char> input = &Input[0];
		pin_ptr<unsigned char> key = &Key[0];
		pin_ptr<unsigned char> outp = &output[0];

		intel_AES_dec256(input, outp, key, numBlocks);
		return output;
	}

	/*
	* Decrypt AES with CBC mode using AES NI
	*/
	array<unsigned char>^ Crypto::decryptAES256_CBC_NI(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length){

		unsigned int numBlocks = blockAmount16(length);

		array<unsigned char>^ output = gcnew array<unsigned char>(length);

		pin_ptr<unsigned char> input = &Input[0];
		pin_ptr<unsigned char> key = &Key[0];
		pin_ptr<unsigned char> iv = &IV[0];
		pin_ptr<unsigned char> outp = &output[0];

		intel_AES_dec256_CBC(input, outp, key, numBlocks, iv);
		return output;

	}

	array<unsigned char>^ Crypto::decryptAES256_CFB(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length){
	
		if (IV == nullptr)
		{
			IV = zeroIV16;
		}

		array<unsigned char>^ output = gcnew array<unsigned char>(length);

		pin_ptr<unsigned char> input = &Input[0];
		pin_ptr<unsigned char> key = &Key[0];
		pin_ptr<unsigned char> iv = &IV[0];
		pin_ptr<unsigned char> outp = &output[0];

		AES_KEY aeskey;
		AES_set_encrypt_key(key, 256, &aeskey);

		unsigned char block[16];
		unsigned char shiftregister[16];
		//works only for little endian architectures:

		*((unsigned int*)shiftregister) = *((unsigned int*)&iv[1]);
		*((unsigned int*)&shiftregister[4]) = (*((unsigned int*)&iv[4]) >> 8) | ((unsigned int)iv[8] << 24);
		*((unsigned int*)&shiftregister[8]) = (*((unsigned int*)&iv[8]) >> 8) | ((unsigned int)iv[12] << 24);
		*((unsigned int*)&shiftregister[12]) = (*((unsigned int*)&iv[12]) >> 8) | ((unsigned int)input[0] << 24);

		AES_encrypt(iv, block, &aeskey);

		unsigned char leftmost = block[0];
		outp[0] = leftmost ^ input[0];

		for (int i = 1; i < length; i++)
		{
			AES_encrypt(shiftregister, block, &aeskey);

			leftmost = block[0];
			outp[i] = leftmost ^ input[i];

			*((unsigned int*)shiftregister) = *((unsigned int*)&shiftregister[1]);
			*((unsigned int*)&shiftregister[4]) = (*((unsigned int*)&shiftregister[4]) >> 8) | ((unsigned int)shiftregister[8] << 24);
			*((unsigned int*)&shiftregister[8]) = (*((unsigned int*)&shiftregister[8]) >> 8) | ((unsigned int)shiftregister[12] << 24);
			*((unsigned int*)&shiftregister[12]) = (*((unsigned int*)&shiftregister[12]) >> 8) | ((unsigned int)input[i] << 24);
		}

		return output;	
	}

	//Check method for AES new instructions
	bool Crypto::supportsAESNI(){
		return check_for_aes_instructions();
	}

	//RC2
	
	array<unsigned char>^ Crypto::decryptRC2(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length, const int mode)
	{
		if (mode == 2)	//CFB
		{
			throw gcnew System::Exception("Encrypting CFB not supported (yet?)");
		}

		array<unsigned char>^ output = gcnew array<unsigned char>(length);
		unsigned short xkey[64];

		cli::pin_ptr<unsigned char> p_key = &Key[0];
		cli::pin_ptr<unsigned char> p_iv = &IV[0];

		rc2_keyschedule(xkey, p_key, Key->Length, Key->Length * 8);

		//put IV into saving-block
		unsigned char block[8] = { 0, 0, 0, 0, 0, 0, 0, 0 };
		xor64(block, p_iv);

		for (int i = 0; i<length; i += 8)
		{
			if (mode == 0) //ECB
			{
				cli::pin_ptr<unsigned char> p_input = &Input[i];
				cli::pin_ptr<unsigned char> p_output = &output[i];
				rc2_decrypt(xkey, p_output, p_input);
			}
			if (mode == 1) //CBC
			{
				cli::pin_ptr<unsigned char> p_input = &Input[i];
				cli::pin_ptr<unsigned char> p_output = &output[i];

				rc2_decrypt(xkey, p_output, p_input);
				xor64(p_output, block);

				xor64(block, block);
				xor64(block, p_input);
			}
		}
		return output;
	}

	//RC4
	array<unsigned char>^ Crypto::decryptRC4(array<unsigned char>^ Input, array<unsigned char>^ Key, const int textlength, const int keylength){

		array<unsigned char>^ output = gcnew array<unsigned char>(textlength);
		pin_ptr<unsigned char> key = &Key[0];
		pin_ptr<unsigned char> outp = &output[0];
		pin_ptr<unsigned char> input = &Input[0];
		
		RC4::crypt(input, outp, key, textlength, keylength);
		return output;
	}

	float *xlogx = 0;

	void prepareEntropy(int size)
	{
		if (xlogx != 0)
		{
			free(xlogx);
		}
		xlogx = (float*)malloc((size + 1)*sizeof(float));
		//precomputations for fast entropy calculation	
		xlogx[0] = 0.0;
		for (int i = 1; i <= size; i++)
		{
			xlogx[i] = -1.0 * i * Math::Log(i / (float)size) / Math::Log(2.0);
		}
	}

	double Crypto::calculateEntropy(array<unsigned char>^ text, int bytesToUse){
		if (bytesToUse > text->Length)
		{
			bytesToUse = text->Length;
		}
		static int lastUsedSize = -1;

		if (lastUsedSize != bytesToUse)
		{
			try
			{
				prepareMutex->WaitOne();
				if (lastUsedSize != bytesToUse)
				{
					prepareEntropy(bytesToUse);
					lastUsedSize = bytesToUse;
				}
			}
			finally
			{
				prepareMutex->ReleaseMutex();
			}
		}

		pin_ptr<unsigned char> t = &text[0];

		int n[256];
		memset(n, 0, sizeof(n));
		//count all ASCII symbols
		for (int counter = 0; counter < bytesToUse; counter++)
		{
			n[t[counter]]++;
		}

		float entropy = 0;
		//calculate probabilities and sum entropy
		for (short i = 0; i < 256; i++)
		{
			entropy += xlogx[n[i]];
		}
		return entropy / (double)bytesToUse;
	}

	array<unsigned char>^ Crypto::md5(array<unsigned char>^ Input)
	{
		pin_ptr<unsigned char> input = &Input[0];
		int inputLength = Input->Length;
		md5_state_t state;
		array<unsigned char>^ digest = gcnew array<unsigned char>(16);
		pin_ptr<unsigned char> digestPtr = &digest[0];
		//md5_byte_t digest[16];

		md5_init(&state);
		md5_append(&state, (const md5_byte_t *)input, inputLength);
		md5_finish(&state, digestPtr);
		
		return digest;
	}
}