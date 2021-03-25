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


#ifndef HEADER_RC4_H
#define HEADER_RC4_H

#define SWAP(a, b) ((a) ^= (b), (b) ^= (a), (a) ^= (b))
public class RC4
{
public:

	static unsigned char* crypt(unsigned char* textin, unsigned char* textout, const unsigned char* keyin, const int textlength, const int keylength);

};

#endif