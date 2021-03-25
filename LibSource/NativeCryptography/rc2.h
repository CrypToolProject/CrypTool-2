//Source: http://www.koders.com/c/fidE0FF4557F4BCEADBB18DAB8178BF7959466D3870.aspx?s=rsa#L2
#ifndef _RC2_H_
#define _RC2_H_

/**********************************************************************\
* Expand a variable-length user key (between 1 and 128 bytes) to a     *
* 64-short working rc2 key, of at most "bits" effective key bits.      *
* The effective key bits parameter looks like an export control hack.  *
* For normal use, it should always be set to 1024.  For convenience,   *
* zero is accepted as an alias for 1024.                               *
\***********************************************************************/
void rc2_keyschedule( unsigned short xkey[64],
                      const unsigned char *key,
                      unsigned len,
                      unsigned bits );

/**********************************************************************\
* Encrypt an 8-byte block of plaintext using the given key.            *
\**********************************************************************/
void rc2_encrypt( const unsigned short xkey[64],
                  const unsigned char *plain,
                  unsigned char *cipher );

/**********************************************************************\
* Decrypt an 8-byte block of ciphertext using the given key.           *
\**********************************************************************/
void rc2_decrypt( const unsigned short xkey[64],
                  unsigned char *plain,
                  const unsigned char *cipher );

#endif