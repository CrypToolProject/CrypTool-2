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

#pragma once
#include <string.h>
#include <stdlib.h>
#include <string.h>
#include "aes_core.h"
#include "DES/des.h"
#include "rc2.h"
#include "iaes_asm_interface.h"
#include "MD5/md5.h"

#include "rc4.h"

using namespace System::Threading;
using namespace System;

namespace NativeCryptography {

	public ref class Crypto
	{
	private:
		static array<unsigned char>^ zeroIV8 = gcnew array<unsigned char>(8);
		static array<unsigned char>^ zeroIV16 = gcnew array<unsigned char>(16);
		static Mutex^ prepareMutex = gcnew Mutex();				

	public:

		//Data Encryption Standard
		static array<unsigned char>^ decryptDES_ECB(array<unsigned char>^ Input, array<unsigned char>^ Key, const int length);
		static array<unsigned char>^ decryptDES_CBC(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length);
		static array<unsigned char>^ decryptDES_CFB(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length);

		//3DES
		static array<unsigned char>^ decrypt3DES_ECB(array<unsigned char>^ Input, array<unsigned char>^ Key, const int length);
		static array<unsigned char>^ decrypt3DES_CBC(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length);
		static array<unsigned char>^ decrypt3DES_CFB(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length);

		//Advanced Encryption Standard
		static array<unsigned char>^ decryptAES128_ECB(array<unsigned char>^ Input, array<unsigned char>^ Key, const int length);
		static array<unsigned char>^ decryptAES128_CBC(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length);
		static array<unsigned char>^ decryptAES128_ECB_NI(array<unsigned char>^ Input, array<unsigned char>^ Key, const int length);
		static array<unsigned char>^ decryptAES128_CBC_NI(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length);
		static array<unsigned char>^ decryptAES128_CFB(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length);
		static array<unsigned char>^ decryptAES192_ECB(array<unsigned char>^ Input, array<unsigned char>^ Key, const int length);
		static array<unsigned char>^ decryptAES192_CBC(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length);
		static array<unsigned char>^ decryptAES192_ECB_NI(array<unsigned char>^ Input, array<unsigned char>^ Key, const int length);
		static array<unsigned char>^ decryptAES192_CBC_NI(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length);
		static array<unsigned char>^ decryptAES192_CFB(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length);
		static array<unsigned char>^ decryptAES256_ECB(array<unsigned char>^ Input, array<unsigned char>^ Key, const int length);
		static array<unsigned char>^ decryptAES256_CBC(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length);
		static array<unsigned char>^ decryptAES256_ECB_NI(array<unsigned char>^ Input, array<unsigned char>^ Key, const int length);
		static array<unsigned char>^ decryptAES256_CBC_NI(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length);
		static array<unsigned char>^ decryptAES256_CFB(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length);
		//Check method for AES new instructions
		static bool supportsAESNI();

		//RC2
		static array<unsigned char>^ decryptRC2(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length, const int mode);

		//RC4
		static array<unsigned char>^ decryptRC4(array<unsigned char>^ Input, array<unsigned char>^ Key, const int length, const int keylength);

		//Cost functions
		static double calculateEntropy(array<unsigned char>^ text, int bytesToUse);
		
		//Message-Digest Algorithm 5
		static array<unsigned char>^ md5(array<unsigned char>^ Input);
	};
}