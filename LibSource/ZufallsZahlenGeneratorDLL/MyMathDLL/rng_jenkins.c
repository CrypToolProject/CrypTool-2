/**
 * This program uses the Jenkins One-at-a-Time hash function as a PRNG
 * by simulating the process of feeding it successively longer blocks
 * of all zeros.  Each 32-bit hash value doubles as the pseudo-random
 * number which is written as binary on stdout in the byte order of
 * the machine.
 *
 * @file
 * @brief Jenkins One-at-a-Time hash function as PRNG
 */

#include <errno.h>
#include <stdio.h>
#include <stdint.h>
#include <string.h>

int
main(int argc,
     char* argv[])
{
    int rv = 0;
    uint32_t a = 0xbb48e941;
    uint32_t b = 0;

    for (;;) {
        /* Inner loop of Jenkins One-at-a-Time hash function where the
         * next character is '\0'. */
        a += '\0';
        a += (a << 10);
        a ^= (a >> 6);

        /* Post-processing. */
        b = a + (a << 3);
        b ^= (b >> 11);
        b += (b << 15);

        /* Write the hash value to stdout as binary in the byte order
         * of the machine. */
        if (fwrite(&b, sizeof(b), 1, stdout) != 1) {
            if (feof(stdout)) {
                fprintf(stderr, "*** Error: fwrite: Unexpected EOF.\n");
            } else {
                fprintf(stderr, "*** Error: fwrite: %s\n", strerror(errno));
            }
            rv = 1;
            break;
        }
    }

    return rv;
}
