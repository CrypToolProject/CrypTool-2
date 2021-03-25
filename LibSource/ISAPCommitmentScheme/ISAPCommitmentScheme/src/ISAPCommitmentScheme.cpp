// ---------------------------------------------------------------------------
// Author: Martin Schmidt (mschmidt@ifam.uni-hannover.de)
// ---------------------------------------------------------------------------

// ---------------------------------------------------------------------------
// Local includes
// ---------------------------------------------------------------------------

#include "ISAPCommitmentScheme.hh"

// ---------------------------------------------------------------------------
// Macro definitions
// ---------------------------------------------------------------------------

#define SETUP_PHASE_LOG                 1
#define MAP_MESSAGE_LOG                 1
#define GENERATE_RATIONALS_LOG          0
#define GENERATE_APPROXIMATIONS_LOG     0
#define CHECK_ADDITIONAL_CONDITIONS_LOG 0
#define GENERATE_P_AND_ETA_LOG          1
#define OPENING_PHASE_LOG               1
#define TIME_MEASUREMENT                1
#define RESTARTS_LOG                    0


ISAPCommitmentScheme::ISAPCommitmentScheme()
{
    // Initialize standard random seed
    srand( (unsigned)time( NULL ) );

    // Initialize random state of the GNU MP library
    _randomizer = new gmp_randclass(gmp_randinit_default);

    // Initialize commitment phase restart counters
    _restartCounter1 = 0;
    _restartCounter2 = 0;

    // Initializing the cpu times
    _commitmentCpuTime = 0.0;
    _openingCpuTime = 0.0;
} // ISAPCommitmentScheme


void
ISAPCommitmentScheme::setup(long dimension,
                            long s,
                            double delta,
                            double deltaPrime)
{
    _dimension = dimension;

    _a = new mpz_class[_dimension];
    _b = new mpz_class[_dimension];
    _c = new mpz_class[_dimension];
    _d = new mpz_class[_dimension];
    _p = new mpz_class[_dimension];
    _eta = new mpq_class[_dimension];

    _delta = delta;
    _deltaPrime = deltaPrime;

    assert(_deltaPrime <= _delta);
    assert(_delta - _deltaPrime == 0.5);

    _s = s;

    _aLowerBound = mpz_class(0);
    _aUpperBound = mpz_class(std::pow(2, (double)_s));

    _bLowerBound = mpz_class(0);
    _bUpperBound = mpz_class(std::pow(2, _deltaPrime * _s));

    _pLowerBound = mpz_class(0);
    _pUpperBound = mpz_class(std::pow(2, (double)_s));

    _qUpperBound = mpz_class(std::pow(2, (double)_s)); // also named N in the paper

    // Computation of accuracy _eps
    mpfr_t eps_tmp;
    mpfr_t qUpperBound_tmp;
    mpfr_t delta_tmp;

    unsigned int mpfr_precision = 200; // a heuristically chosen number
    mpfr_init2(eps_tmp, mpfr_precision);
    mpfr_init2(qUpperBound_tmp, mpfr_precision);
    mpfr_init2(delta_tmp, mpfr_precision);

    mpfr_set_z(qUpperBound_tmp, _qUpperBound.get_mpz_t(), GMP_RNDN);
    mpfr_set_d(delta_tmp, -_delta, GMP_RNDN);
    mpfr_pow(eps_tmp, qUpperBound_tmp, delta_tmp, GMP_RNDN);

    mpfr_get_f(_eps.get_mpf_t(), eps_tmp, GMP_RNDN);

#if SETUP_PHASE_LOG
    std::cout << "Setup phase log\n";
    std::cout << "---------------\n";
    std::cout << "Dimension n\t\t= " << _dimension << "\n";
    std::cout << "Security parameter s\t= " << _s << "\n";
    std::cout << "Delta\t\t\t= " << _delta << "\n";
    std::cout << "Delta'\t\t\t= " << _deltaPrime << "\n";
    std::cout << "N (upper bound on q)\t= " << _qUpperBound << "\n";
    std::cout << "Precision eps\t\t= " << _eps << "\n";
    std::cout << "Lower bound on a_i\t= " << _aLowerBound << "\n";
    std::cout << "Upper bound on a_i\t= " << _aUpperBound << "\n";
    std::cout << "Lower bound on b_i\t= " << _bLowerBound << "\n";
    std::cout << "Upper bound on b_i\t= " << _bUpperBound << "\n";
    std::cout << "Lower bound on p_i\t= " << _pLowerBound << "\n";
    std::cout << "Upper bound on p_i\t= " << _pUpperBound << "\n";
#endif
} // setup


void
ISAPCommitmentScheme::commit(bool m)
{
#if TIME_MEASUREMENT
    // Time stamp before the computations
    clock_t startCommit = clock();
#endif

    _m = m;

    // Map the message (m -> q)
    mapMessage(_m);

    // Generate rational numbers a_i / b_i
    generateRationals();

    // Generate approximations c_i / d_i of q * a_i / b_i
    if (!generateApproximations())
    {
#if RESTARTS_LOG
        std::cerr << "********************************************************\n"
                  << "* Approximations don't satisfy denominator conditions. *\n"
                  << "* See equation (19). Restarting commitment phase.      *\n"
                  << "********************************************************\n";
#endif
        _restartCounter1++;
        commit(m); // Restarting the commitment phase
        return;
    }

    // Check additional conditions
    if (!checkAdditionalConditions())
    {
#if RESTARTS_LOG
        std::cerr << "*******************************************************\n"
                  << "* Approximations don't satisfy additional conditions. *\n"
                  << "* See equation (20). Restarting commitment phase.     *\n"
                  << "*******************************************************\n";
#endif
        _restartCounter2++;
        commit(m); // Restarting the commitment phase
        return;
    }

    // Generate vector p and vector eta
    generatePAndEta();

#if TIME_MEASUREMENT
    // Time stamp after the computations
    _commitmentCpuTime += 
      static_cast<long double>(clock() - startCommit) / CLOCKS_PER_SEC;
#endif

} // commit


bool
ISAPCommitmentScheme::open()
{
#if TIME_MEASUREMENT
    // Time stamp before the computations
    clock_t startOpen = clock();
#endif

    bool valid = true;

#if OPENING_PHASE_LOG
    std::cout << "\nOpening phase log\n";
    std::cout << "---------------\n";
    std::cout << "eps (right hand side)\t= " << _eps << "\n";
    std::cout << "q (mapped message)\t= " << _q << "\n";
#endif

    for (long i = 0; i != _dimension; ++i)
    {
        mpf_class lhs;
        lhs.set_prec(log((double)_s) * _eps.get_prec()); // @todo cast _s to double?
        lhs = mpf_class(_q) * mpf_class(_a[i]) / mpf_class(_b[i])
              - mpf_class(_p[i]) - _eta[i];

        valid = abs(lhs) < _eps;

#if OPENING_PHASE_LOG
        std::cout << "i = " << i << ",\t"
                  << "Achieved approximation quality (left hand side) = "
                  << abs(lhs) << "\n";
#endif
    }

#if OPENING_PHASE_LOG
    if (valid) std::cout << "Verifier: accept" << std::endl;
    else std::cout << "Verifier: reject" << std::endl;
#endif

#if TIME_MEASUREMENT
    // Time stamp after the computations
    _openingCpuTime = 
      static_cast<long double>(clock() - startOpen) / CLOCKS_PER_SEC;
#endif

    return valid;
} // open


long double
ISAPCommitmentScheme::getCommitmentCpuTime() const
{
    return _commitmentCpuTime;
} // getCommitmentCpuTime


long double
ISAPCommitmentScheme::getOpeningCpuTime() const
{
    return _openingCpuTime;
} // getOpeningCpuTime


long
ISAPCommitmentScheme::getRestartCounter1() const
{
    return _restartCounter1;
} // getRestartCounter1


long
ISAPCommitmentScheme::getRestartCounter2() const
{
    return _restartCounter2;
} // getRestartCounter2


//----------------------------------------------------------------------------
// Private methods
//----------------------------------------------------------------------------

void
ISAPCommitmentScheme::mapMessage(bool m)
{
    char* bitstring = new char[_s + 1];
	
    // setting the least significant bit to the message
    bitstring[_s - 1] = (char)((int)m + 48);
    // randomly fill the s-bit string
    for (long i = 0; i != _s - 1; ++i)
    {
        bitstring[i] = (char)((rand() % 2) + 48);
    }

    bitstring[_s] = '\0'; // null-termination (required by mpz_set_str)
    _q = mpz_class(bitstring, 2);

#if MAP_MESSAGE_LOG
    std::cout << "\nMap message log\n";
    std::cout << "---------------\n";
    std::cout << "Message m\t\t= " << m << "\n";
    std::cout << "Mapped message, [q]_2\t= ("
              << bitstring << ")\n";
    std::cout << "Mapped message, [q]_10\t= " << _q << "\n";
#endif

    delete [] bitstring;
} // mapMessage


void
ISAPCommitmentScheme::generateRationals()
{
    long trialsForA = 0;
    long trialsForB = 0;

    // Generating the a_i and b_i
    for (long i = 0; i != _dimension; ++i)
    {
        mpz_class aTmp;
        mpz_class bTmp;
        bool coprime = false;

        while (!coprime)
        {
            // Choose an arbitrary aTmp
            aTmp = _randomizer->get_z_range(_aUpperBound);
            ++trialsForA;

            // Choose an arbitrary but odd b that is coprime with q
            bool bCoprimeWithQ = false;

            while (!bCoprimeWithQ)
            {
                // Randomly choose bTmp
                bTmp = _randomizer->get_z_range(_bUpperBound);
                ++trialsForB;

                // Check if b is even (mpz_odd_p has has no C++ class interface)
                bool bOdd = (bool)mpz_odd_p(bTmp.get_mpz_t());
                if (!bOdd) bTmp -= 1;

                // Check if b is coprime with q (mpz_gcd has no C++ class interface)
                mpz_class gcd;
                mpz_gcd(gcd.get_mpz_t(), bTmp.get_mpz_t(), _q.get_mpz_t());

                if (gcd == 1) bCoprimeWithQ = true;
            }

            // Check if gcd(a,b) = 1 (mpz_gcd has no C++ class interface)
            mpz_class gcd;
            mpz_gcd(gcd.get_mpz_t(), aTmp.get_mpz_t(), bTmp.get_mpz_t());

            if (gcd == 1) coprime = true;
        }

        _a[i] = mpz_class(aTmp);
        _b[i] = mpz_class(bTmp);
    }

#if GENERATE_RATIONALS_LOG
    std::cout << "\nGenerate rationals log\n";
    std::cout << "----------------------\n";
    for (long i = 0; i != _dimension; ++i)
    {
        std::cout << "a_" << i << " / b_" << i << "\t\t= "
                  << _a[i] << " / " << _b[i] << "\n";
    }
    std::cout << "Trials for choosing a\t= " << trialsForA << "\n";
    std::cout << "Trials for choosing b\t= " << trialsForB << "\n";
#endif
} // generateRationals


bool
ISAPCommitmentScheme::generateApproximations()
{
    // Lower bound on the approximation denominators d_i
    mpf_class dLowerBound;
    dLowerBound = 1.0 / sqrt(_eps);

#if GENERATE_APPROXIMATIONS_LOG
    std::cout << "\nGenerate approximations log\n";
    std::cout << "---------------------------\n";
    std::cout << "Lower bound on d_i \t = " << dLowerBound << "\n";
#endif

    // Compute the convergents of q * a_i / b_i
    for (long i = 0; i != _dimension; ++i)
    {
        mpz_class numer(_q * _a[i]);
        mpz_class denom(_b[i]);

#if GENERATE_APPROXIMATIONS_LOG
        std::cout << "q * a_" << i << "\t= " << numer << "\n";
        std::cout << "b_" << i << "\t= " << denom << "\n";
#endif

        // Convergent
        mpz_class oldConvergentNumer(0); // numerator of the convergent for k = -1
        mpz_class oldConvergentDenom(1); // denominator of the convergent for k = -1

        _c[i] = 1; // Numerator of the convergent for k = 0
        _d[i] = 0; // Denominator of the convergent k = 0

        // Quantities for division with remainder
        mpz_class remainder = -1;
        mpz_class quotient; // = partial quotient of the continued fraction

        long k = 1; // Counter
        long boundCondition = false;

        while (remainder != 0 && _d[i] < _b[i] && !boundCondition)
        {
            // Call to GNU MP's division with remainder
            mpz_tdiv_qr(quotient.get_mpz_t(), remainder.get_mpz_t(),
                        numer.get_mpz_t(), denom.get_mpz_t());

#if GENERATE_APPROXIMATIONS_LOG
            std::cout << "Division with remainder:\t"
                      << numer << " = " << quotient << " * " << denom
                      << " + " << remainder << "\n";
#endif

            // Compute convergent numerator by the recurrence formula (4)
            mpz_class convergentNumerTmp = _c[i];
            _c[i] = _c[i] * quotient + oldConvergentNumer;
            oldConvergentNumer = convergentNumerTmp;

            // Compute convergent denominator by the recurrence formula (5)
            mpz_class convergentDenomTmp = _d[i];
            _d[i] = _d[i] * quotient + oldConvergentDenom;
            oldConvergentDenom = convergentDenomTmp;

#if GENERATE_APPROXIMATIONS_LOG
            std::cout << k << "-th convergent:\t\t"
                      << "c_" << i << "^" << k << " / d_" << i << "^" << k
                      << " = " << _c[i] << " / " << _d[i] << "\n";
#endif

            // Check bound condition (see equation (22) in algorithm 1)
            boundCondition = (dLowerBound < mpf_class(_d[i]) && _d[i] < _b[i]);

            // Reset numerator and denominator for the next division with remainder
            numer = denom;
            denom = remainder;

            ++k;
        }

        if (!boundCondition) return false; // This will restart the commitment phase
    }

    return true;
} // generateApproximations


bool
ISAPCommitmentScheme::checkAdditionalConditions()
{
#if CHECK_ADDITIONAL_CONDITIONS_LOG
    std::cout << "\nCheck additional conditions log\n";
    std::cout << "-------------------------------\n";
#endif

    for (int i = 0; i != _dimension; ++i)
    {
#if CHECK_ADDITIONAL_CONDITIONS_LOG
        std::cout << "N = " << _qUpperBound << " < "
                  << "b_" << i << " = " << _b[i] << " ?\n"
                  << "sqrt(2*b_" << i << ") = " << sqrt(2 * _b[i]) << " < "
                  << "d_" << i << " = " << _d[i] << " ?\n";
#endif

        // Check additional conditions (see equation (23) in algorithm 1)
        if (sqrt(2 * _b[i]) < _d[i] && _qUpperBound < _b[i]) return true;
    }

#if CHECK_ADDITIONAL_CONDITIONS_LOG
    std::cout << "No index found fulfilling the additional conditions "
	      << "(see equation (20)) of algorithm 1." << std::endl;
#endif

    return false;
} // checkAdditionalConditions


void
ISAPCommitmentScheme::generatePAndEta()
{
    for (long i = 0; i != _dimension; ++i)
    {
        // Choose p_i randomly
        _p[i] = _randomizer->get_z_range(_pUpperBound - _pLowerBound)
	  + _pLowerBound;

        // Compute eta[i]
        mpq_class approximation(_c[i], _d[i]);
        mpq_class pFrac(_p[i], 1);
        _eta[i] = approximation - pFrac;
    }

#if GENERATE_P_AND_ETA_LOG
    std::cout << "\nGenerate p and eta log\n";
    std::cout << "----------------------\n";
    for (long i = 0; i != _dimension; ++i)
    {
        std::cout << "p_" << i << "\t= " << _p[i] << "\n";
        std::cout << "eta_" << i << "\t= " << _eta[i] << "\n";
    }
#endif

} // generatePAndEta
