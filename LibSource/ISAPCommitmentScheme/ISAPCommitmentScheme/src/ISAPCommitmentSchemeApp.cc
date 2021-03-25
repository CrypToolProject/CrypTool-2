/**
 * @mainpage A bit commitment scheme based on the ISAP.
 *
 * This software package contains an (object oriented C++) 
 * implementation of the bit commitment scheme based
 * on the inhomogeneous simultaneous diophantine approximation
 * problem (ISAP) by Frederik Armknecht (Universität Mannheim),
 * Carsten Elsner (FHDW Hannover) and Martin Schmidt
 * (Leibniz Universität Hannover).
 *
 * As reference see the paper "Using the Inhomogeneous
 * Simultaneous Approximation Problem for Cryptographic
 * Design" by F. Armknecht, C. Elsner and M. Schmidt.
 * (to appear in LNCS, Proceedings of Africa Crypt 2011)
 *
 * If you detect any bugs or if you think some things
 * may be done in a smarter way, please contact me 
 * (mschmidt@ifam.uni-hannover.de)! 
 *
 * The GNU Multiple Precision Library (GMP) and the 
 * GNU MPFR Library are required as additional libraries.
 *
 * To compile and generate the binary, type
 *
 * > make
 *
 * in the base directory.
 *
 * To execute the binary type
 *
 * > ISAPCommitmentSchemeApp <bit-to-commit-to>
 *
 * @author   Martin Schmidt (mschmidt@ifam.uni-hannover.de)
 * @date     April 2011
 * @version  1.0
 */

// ---------------------------------------------------------------------------
// Global includes
// ---------------------------------------------------------------------------

#include <iostream>

// ---------------------------------------------------------------------------
// Local includes
// ---------------------------------------------------------------------------

#include "ISAPCommitmentScheme.hh"

// ---------------------------------------------------------------------------
// Function definitions
// ---------------------------------------------------------------------------

int
main(int argc, char** argv)
{
    std::cout << "\nA bit commitment scheme based on the Inhomogeneous\n"
              << "Simultaneous Diophantine Approximation Problem (ISAP).\n\n"
              << "See 'Using the Inhomogeneous Simultaneous\n"
              << "Approximation Problem for Cryptographic\n"
              << "Design' by F. Armknecht, C. Elsner and M. Schmidt\n"
              << "Implementation by M. Schmidt, Leibniz Universitaet Hannover\n"
              << "http://www.ifam.uni-hannover.de/~mschmidt\n\n";

    if (argc != 2)
    {
        std::cerr << "Wrong number of arguments. The right command format is\n"
                  << "> ISAPCommitmentSchemeApp <bit-to-commit-to>\n\n";
        exit(1);
    }
    else if (*argv[1] != '0' && *argv[1] != '1')
    {
        std::cerr << "Invalid commitment: " << argv[1] << ".\n"
                  << "The commitment has to be a single bit (0 or 1)\n\n";
        exit(1);
    }
    else
    {
        // Instantiating the bit commitment scheme
        ISAPCommitmentScheme cs;

	// Initializing data for the setup phase
        long dimension = 7;
        long s = 128;
        long double epsilonPrime = 1.0 / (3 * dimension * s);
        long double delta = 1.5 + epsilonPrime;
        long double deltaPrime = 1.0 + epsilonPrime;

	// Setup phase
        cs.setup(dimension, s, delta, deltaPrime);

	// Message m to commit to
        bool m = (bool)((int)*argv[1] - 48);

        // Time stamp before the computations
        clock_t startCommit = clock();

	// Commitment phase
        cs.commit(m);

        // Time stamp after the computations
        long double commitTime =
            static_cast<long double>(clock() - startCommit) / CLOCKS_PER_SEC;

	// Opening phase
        cs.open();

        std::cout << "\nRestarts log" << std::endl;
        std::cout << "------------" << std::endl;
        std::cout << "Number of restart due to equation (19)\t\t\t= "
                  << cs.getRestartCounter1() << std::endl;
        std::cout << "Number of restart due to equation (20)\t\t\t= "
                  << cs.getRestartCounter2() << std::endl;

        std::cout << "Time to compute the commitment (inner measurement)\t= "
                  << cs.getCommitmentCpuTime() << "s\n";
        std::cout << "Time to compute the commitment (outer measurement)\t= "
                  << commitTime << "s\n";
        std::cout << "Time to compute the verification\t\t\t= "
                  << cs.getOpeningCpuTime() << "s\n";
    }

} // main
