/**************************************************************************

  Copyright [2009] [CrypTool Team]

  This file is part of CrypTool.

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

      http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.

**************************************************************************/

#include "NTL/ZZ.h"
#include <NTL/ZZX.h>
#include <NTL/LLL.h>
#include <NTL/ZZXFactoring.h>
#include <NTL/mat_ZZ.h>
//#include <afx.h>
//#include <sstream>
//#include <iostream>

#define SA_CANCELED 0
#define SA_BUILDING	1
#define SA_REDUCING 2
#define SA_SUCCESSFUL 3
#define SA_FAILED 4

using namespace NTL;

class StereotypedAttack{
public:
	// Constructors
	// StereotypedAttack();

	// get/set-methods
	void setN(ZZ&);
	void setE(ZZ&);
	void setLeftText(ZZ&);
	void setRightText(ZZ&);
	void setUnknownLength(int);
	void setCiphertext(ZZ&);
	void setH(ZZ&);

	ZZ& getBound();
	ZZ& getSolution();
	ZZ& getE();
	ZZ& getN();
	ZZ& getH();
	int getLatticeTime();
	int getReductionTime();
	int getOverallTime();
	// main Function
	void attack();
	void cancel();

	bool allProperlySet();
	static long status;
	static long reductions;
	//CString Log;

private:
	ZZ N; // RSA - modulus
	ZZ e; // Public exponent
	ZZ leftText; // Text left of unknown
	ZZ rightText; // Text right of unknown
	ZZ ciphertext; // (leftText + solution +rightText)^e mod N
	ZZ X; // Bound on the unknown
	ZZ h; // Parameter h 

	int unknownLength; // length of unknown in chars
	NTL::ZZX poly; //Polynom to find root for

	mat_ZZ HGMatrix; // Howgrave-Graham matrix to be reduced

	ZZ solution; // solution
	ZZ binom(int i, int j);
	void buildPoly();
	void buildLattice();
	void reduceLattice();
	void findSolution();
	void updateX();
	//CString timeStamp();
	bool FindRoots(ZZX f, vec_ZZ& r);
	static long StopLLL(const vec_ZZ& z);
	double startTime;
	double buildLatticeTime;
	double reduceLatticeTime;
};
