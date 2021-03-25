/*
 ---------------------------------------------------------------------------
 Copyright (c) 2003, Dr Brian Gladman, Worcester, UK.   All rights reserved.

 LICENSE TERMS

 The free distribution and use of this software in both source and binary
 form is allowed (with or without changes) provided that:

   1. distributions of this source code include the above copyright
      notice, this list of conditions and the following disclaimer;

   2. distributions in binary form include the above copyright
      notice, this list of conditions and the following disclaimer
      in the documentation and/or other associated materials;

   3. the copyright holder's name is not used to endorse products
      built using this software without specific written permission.

 ALTERNATIVELY, provided that this notice is retained in full, this product
 may be distributed under the terms of the GNU General Public License (GPL),
 in which case the provisions of the GPL apply INSTEAD OF those given above.

 DISCLAIMER

 This software is provided 'as is' with no explicit or implied warranties
 in respect of its properties, including, but not limited to, correctness
 and/or fitness for purpose.
 ---------------------------------------------------------------------------
 Issue 09/09/2006

 The unsigned integer types defined here are of the form uint_<nn>t where
 <nn> is the length of the type; for example, the unsigned 32-bit type is
 'uint_32t'.  These are NOT the same as the 'C99 integer types' that are
 defined in the inttypes.h and stdint.h headers since attempts to use these
 types have shown that support for them is still highly variable.  However,
 since the latter are of the form uint<nn>_t, a regular expression search
 and replace (in VC++ search on 'uint_{:z}t' and replace with 'uint\1_t')
 can be used to convert the types used here to the C99 standard types.
*/

#if defined(__cplusplus)
extern "C" {
#endif

#include <limits.h>

#ifndef BRG_UI8
#  define BRG_UI8
#  if UCHAR_MAX == 255
     typedef unsigned char uint_8t;
#  else
#    error Please define uint_8t as an 8-bit unsigned integer type in brg_types.h
#  endif
#endif

#ifndef BRG_UI16
#  define BRG_UI16
#  if USHRT_MAX == 65535
     typedef unsigned short uint_16t;
#  else
#    error Please define uint_16t as a 16-bit unsigned short type in brg_types.h
#  endif
#endif

#ifndef BRG_UI32
#  define BRG_UI32
#  if UINT_MAX == 4294967295
     typedef unsigned int uint_32t;
#  elif ULONG_MAX == 4294967295
     typedef unsigned long uint_32t;
#  elif defined( _CRAY )
  /* USE_AES is undefined on Crays, however we define a dummy data type
     to get the code to compile - pcg */
/*#error Crays don't support 32-bit data types, this code won't compile on a Cray*/
	typedef   unsigned int     uint_32t;
#  else
#    error Please define uint_32t as a 32-bit unsigned integer type in brg_types.h
#  endif
#endif

#ifndef BRG_UI64
#  if defined( _MSC_VER )
#    define BRG_UI64
#    if _MSC_VER >= 1310
       typedef unsigned long long uint_64t;
#      define li_64(h) 0x##h##ull
#    else
       typedef unsigned __int64 uint_64t;
#      define li_64(h) 0x##h##ui64
#    endif
#  elif defined( __BORLANDC__ ) && !defined( __MSDOS__ )
#    define BRG_UI64
#    define li_64(h) 0x##h##ull
     typedef __int64 uint_64t;
#  elif defined( __sun )
#    if defined( ULONG_MAX ) && ULONG_MAX == 0xfffffffful
#      define BRG_UI64
#      define li_64(h) 0x##h##ull
       typedef unsigned long long uint_64t;
#    elif defined( ULONG_LONG_MAX ) && ULONG_LONG_MAX == 0xfffffffffffffffful
#      define BRG_UI64
#      define li_64(h) 0x##h##ul
       typedef unsigned long uint_64t;
#    endif
#  elif defined( __MVS__)
#    define li_64(h)    0x##h##ull
     typedef unsigned int long long uint_64t;
#  elif defined( ULLONG_MAX ) && ULLONG_MAX > 4294967295
#    if ULLONG_MAX == 18446744073709551615ull
#      define BRG_UI64
#      define li_64(h) 0x##h##ull
       typedef unsigned long long uint_64t;
#    endif
#  elif defined( ULONG_LONG_MAX ) && ULONG_LONG_MAX > 4294967295
#    if ULONG_LONG_MAX == 18446744073709551615
#      define BRG_UI64
#      define li_64(h) 0x##h##ull
       typedef unsigned long long uint_64t;
#    endif
#  elif defined( ULONG_MAX ) && ULONG_MAX > 4294967295
#    if ULONG_MAX == 18446744073709551615
#      define BRG_UI64
#      define li_64(h) 0x##h##ul
       typedef unsigned long uint_64t;
#    endif
#  elif defined( UINT_MAX ) && UINT_MAX > 4294967295
#    if UINT_MAX == 18446744073709551615
#      define BRG_UI64
#      define li_64(h) 0x##h##u
       typedef unsigned int uint_64t;
#    endif
#  endif
#endif

#if !defined( BRG_UI64 )
#  if defined( NEED_UINT_64T ) 
#    error Please define uint_64t as an unsigned 64 bit type in brg_types.h
#  endif
#endif

#ifndef RETURN_VALUES
#  define RETURN_VALUES
#  if defined( DLL_EXPORT )
#    if defined( _MSC_VER ) || defined ( __INTEL_COMPILER )
#      define VOID_RETURN    __declspec( dllexport ) void __stdcall
#      define INT_RETURN     __declspec( dllexport ) int  __stdcall
#    elif defined( __GNUC__ )
#      define VOID_RETURN    __declspec( __dllexport__ ) void
#      define INT_RETURN     __declspec( __dllexport__ ) int
#    else
#      error Use of the DLL is only available on the Microsoft, Intel and GCC compilers
#    endif
#  elif defined( DLL_IMPORT )
#    if defined( _MSC_VER ) || defined ( __INTEL_COMPILER )
#      define VOID_RETURN    __declspec( dllimport ) void __stdcall
#      define INT_RETURN     __declspec( dllimport ) int  __stdcall
#    elif defined( __GNUC__ )
#      define VOID_RETURN    __declspec( __dllimport__ ) void
#      define INT_RETURN     __declspec( __dllimport__ ) int
#    else
#      error Use of the DLL is only available on the Microsoft, Intel and GCC compilers
#    endif
#  elif defined( __WATCOMC__ )
#    define VOID_RETURN  void __cdecl
#    define INT_RETURN   int  __cdecl
#  else
#    define VOID_RETURN  void
#    define INT_RETURN   int
#  endif
#endif

#if defined(__cplusplus)
}
#endif
