// ---------------------------------------------------------------------------
// Author: Martin Schmidt (mschmidt@ifam.uni-hannover.de)
// ---------------------------------------------------------------------------

#ifndef ISAPCOMMITMENTSCHEME_H
#define ISAPCOMMITMENTSCHEME_H 1

// ---------------------------------------------------------------------------
// Global includes
// ---------------------------------------------------------------------------

#include <cassert>
#include <cmath>
#include <iostream>
#include <cstdlib>
#include <ctime>
#include <gmpxx.h>
#include <gmp.h>
#include <mpfr.h>

/**
 * @class    ISAPCommitmentScheme
 * @author   Martin Schmidt (mschmidt@ifam.uni-hannover.de)
 * @brief    A bit commitment scheme based on ISAP
 * @version  1.0
 * @date     April 2011
 *
 * This class encapsulates the bit commitment scheme based on
 * ISAP. See "Using the Inhomogeneous Simultaneous Approximation
 * Problem for Cryptographic Design" by F. Armknecht, C. Elsner
 * and M. Schmidt.
 *
 * Like other commitment schemes it is split up into
 * a setup, a commitment phase and an opening phase.
 * These algorithms are implemented in their own methods.
 * Additionally, there are some auxiliary methods for the
 * commitment phase.
 */
class ISAPCommitmentScheme
{

public:

    /**
     * @brief Empty constructor
     *
     * Constructs an empty commitment scheme object and
     * initializes some random seeds and logging counters.
     */
    ISAPCommitmentScheme();

    /**
     * @brief Setup phase
     *
     * @param  dimension   Dimension of the ISAP problem
     * @param  s           Security parameter
     * @param  delta       Part of the approximation quality
     * @param  deltaPrime  Part of the approximation quality
     *
     * For delta and deltaPrime, see section 6 in the paper.
     */
    void setup(long dimension,
               long s,
               double delta,
               double deltaPrime);

    /**
     * @brief Commitment phase
     *
     * See "Algorithm 1: The commitment algorithm Commit_P"
     * in the paper.
     *
     * @param  m  The message m (a single bit)
     */
    void commit(bool m);

    /**
     * @brief Opening phase of the commitment scheme
     *
     * @return  bool  Returns true, if the opening phase was successul,
     *                else false
     */
    bool open();

    /**
     * @brief Getter for the CPU time used by the commitment algorithm
     *
     * @return  long double  Returns the CPU time used by the commitment algorithm
     */
    long double getCommitmentCpuTime() const;

    /**
     * @brief Getter for the CPU time used by the opening algorithm
     *
     * @return  long double  Returns the CPU time used by the opening algorithm
     */
    long double getOpeningCpuTime() const;


    /**
     * @brief Getter for the counter of restarts due to equation (22)
     *
     * @return  long  Returns number of restarts due to equation (22)
     */
    long getRestartCounter1() const;

    /**
     * @brief Getter for the counter of restarts due to equation (23)
     *
     * @return  long  Returns number of restarts due to equation (23)
     */
    long getRestartCounter2() const;

	mpz_class getQ()
	{
		return _q;
	}

	mpz_class* getP()
	{
		return _p;
	}

	mpz_class* getA()
	{
		return _a;
	}

	mpz_class* getB()
	{
		return _b;
	}

	mpq_class* getEta()
	{
		return _eta;
	}

private:

    /**
     * @brief Maps the message to a s-bit value
     *
     * Complexity: - randomly create and set s-1 bits
     *             - initialize an integer out of the s-bit-string
     *
     * @param  m  The message (a single bit)
     */
    void mapMessage(bool m);

    /**
     * @brief  Generates the rational numbers a_i and b_i
     *
     * This method computes rational numbers a_i / b_i such that
     * the a_i and b_i are co-prime and b_i is odd and co-prime
     * with q holds.
     */
    void generateRationals();

    /**
     * @brief  Generates the rational approximations c_i / d_i of q * a_i / b_i
     *
     * This method computes the rational approximations c_i / d_i of
     * q * a_i / b_i * and checks whether the condition 1/sqrt(eps) < d_i < b_i
     * is fulfilled.
     *
     * @return  bool  Returns true, if the condition 1/sqrt(eps) < d_i < b_i is
     *                fulfilled for all i, else false
     */
    bool generateApproximations();

    /**
     * @brief  Checks the additional conditions on b_i and d_i
     *
     * The conditions are that there exist at least one index i with
     * N < b_i and sqrt(2*b_i) < d_i
     *
     * @return  bool  Return true, if there exists an index i fulfilling the
     *                conditions N < b_i and sqrt(2*b_i) < d_i, else false
     */
    bool checkAdditionalConditions();

    /**
     * @brief Generates the vector p and eta
     */
    void generatePAndEta();

    // -------------------------------------------------------------------------
    // private data member
    // -------------------------------------------------------------------------

    /// Dimension of the diophantine inequality system
    long _dimension;

    /// Precision of the ISAP problem
    mpf_class _eps;

    /// Message m (a single bit) to commit to
    bool _m;

    /// Security parameter s
    long _s;

    /// Setup parameter delta
    double _delta;

    /// Setup parameter delta'
    double _deltaPrime;

    /// s-bit value q generated out of the message m
    mpz_class _q;

    /// Upper bound on the simultaneous denominator _q (N = 2^s)
    mpz_class _qUpperBound;

    /// Vector of numerators a_i of the rationals a_i / b_i
    mpz_class* _a;

    /// Vector of numerators b_i of the rationals a_i / b_i
    mpz_class* _b;

    /// Lower bound of the numerators a_i of the rationals a_i / b_i
    mpz_class _aLowerBound;

    /// Upper bound of the numerators a_i of the rationals a_i / b_i
    mpz_class _aUpperBound;

    /// Lower bound of the numerators b_i of the rationals a_i / b_i
    mpz_class _bLowerBound;

    /// Upper bound of the numerators b_i of the rationals a_i / b_i
    mpz_class _bUpperBound;

    /// Vector of the numerators c_i of the rational approximations c_i / d_i
    mpz_class* _c;

    /// Vector of the numerators d_i of the rational approximations c_i / d_i
    mpz_class* _d;

    /// Vector p (p_1,...,p_n)
    mpz_class* _p;

    /// Lower bound on the p_i (i=1,...,n)
    mpz_class _pLowerBound;

    /// Upper bound on the p_i (i=1,...,n)
    mpz_class _pUpperBound;

    /// Vector eta (eta_1,...,eta_n)
    mpq_class* _eta;

    /// CPU time for the commitment phase
    long double _commitmentCpuTime;

    /// CPU time for the opening phase
    long double _openingCpuTime;

    /// Counter for the number of restarts due to equation (19)
    long _restartCounter1;

    /// Counter for the number of restarts due to equation (20)
    long _restartCounter2;

    /// Random state for the GNU MP
    gmp_randclass* _randomizer;

}; // class ISAPCommitmentScheme

#endif // ISAPCOMMITMENTSCHEME_H
