// ISAPCommitmentScheme.h

#pragma once

#include <iostream>
#include <sstream>
#include "src/ISAPCommitmentScheme.hh"

using namespace System;
using namespace System::Numerics;

namespace ISAPCommitmentSchemeWrapper {

	public ref class ISAPResult
	{
	public:
		array<BigInteger>^ p;
		BigInteger q;
		array<double>^ alpha;
		array<BigInteger>^ a;
		array<BigInteger>^ b;
		array<double>^ eta;
		String^ log;
	};

	public ref class Wrapper
	{
	public:
		ISAPResult^ Run(bool m, long dimension, long s)
		{
			//redirect cout output:
			std::stringstream redirectedOutput;
			std::cout.rdbuf(redirectedOutput.rdbuf());

			long double epsilonPrime = 1.0 / (3 * dimension * s);
			long double delta = 1.5 + epsilonPrime;
			long double deltaPrime = 1.0 + epsilonPrime;

			// Instantiating the bit commitment scheme
			ISAPCommitmentScheme cs;

			// Setup phase
			cs.setup(dimension, s, delta, deltaPrime);

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

			std::string output = redirectedOutput.str();
			ISAPResult^ result = gcnew ISAPResult();
			result->log = gcnew String(output.c_str());			

			result->q = BigInteger::Parse(gcnew String(cs.getQ().get_str().c_str()));

			result->p = gcnew array<BigInteger>(dimension);
			for (int i = 0; i < dimension; i++)
			{
				result->p[i] = BigInteger::Parse(gcnew String(cs.getP()[i].get_str().c_str()));
			}

			result->alpha = gcnew array<double>(dimension);
			result->a = gcnew array<BigInteger>(dimension);
			result->b = gcnew array<BigInteger>(dimension);
			for (int i = 0; i < dimension; i++)
			{				
				result->alpha[i] = cs.getA()[i].get_d() / cs.getB()[i].get_d();
				result->a[i] = BigInteger::Parse(gcnew String(cs.getA()[i].get_str().c_str()));
				result->b[i] = BigInteger::Parse(gcnew String(cs.getB()[i].get_str().c_str()));
			}

			result->eta = gcnew array<double>(dimension);
			for (int i = 0; i < dimension; i++)
			{
				result->eta[i] = cs.getEta()[i].get_d();
			}

			return result;
		}
	};
}
