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

#include "StereotypedAttack.h"


using namespace NTL;

void StereotypedAttack::attack(){
	status=SA_BUILDING;
	solution=0;
	reductions=0;
	buildLatticeTime =0.0;
	reduceLatticeTime=0.0;
	startTime=GetTime();

	//Log=timeStamp()+" Stereotyped Attack begonnen /  Stereotyped Attack started \r\n";
	//Log+=timeStamp()+" Erzeuge Gitter / Building lattice \r\n";
	if(status!=SA_CANCELED){
		buildLatticeTime=GetTime();
		buildPoly();
		buildLattice();
	}
	//Log+=timeStamp()+" Reduziere Gitter / Reducing lattice \r\n";
	buildLatticeTime=GetTime()-buildLatticeTime;
	if(status!=SA_CANCELED){
		reduceLatticeTime=GetTime();
		status=SA_REDUCING;
		reduceLattice();
	}
	
	reduceLatticeTime=GetTime()-reduceLatticeTime;
	if(status!=SA_CANCELED){
		findSolution();
		if(solution>0){
			//Log+=timeStamp()+" Lösung gefunden / Solution found:\r\n";
			//Log+=toString(solution,256,0);
			status=SA_SUCCESSFUL;
		}
		else{
			//Log+=timeStamp()+"Keine Lösung gefunden / No Solution found \r\n";
			status=SA_FAILED;
		}
	}//else
		//Log+=timeStamp()+" Angriff vom Benutzer abgebrochen / Attack canceled by user \r\n";

};

void StereotypedAttack::cancel(){
	status=SA_CANCELED;
}

long StereotypedAttack::StopLLL(const NTL::vec_ZZ& z){
	reductions++;
	if(status!=SA_CANCELED)
		return 0;
	else
		return 1;
}

long StereotypedAttack::status = 0;
long StereotypedAttack::reductions = 0;

// Builds a modular univariate Polynom.
// (left * offs + unknown * offs +right)^e - ciphertext = 0 mod N

void StereotypedAttack::buildPoly(){
	poly.kill();

	/*Log+="Text links des unbekannten Teils / Text left of unknown part:\r\n";
	Log+=toString(leftText,256,0)+"\r\n";
	Log+="Text rechts des unbekannten Teils / Text right of unknown part:\r\n";
	Log+=toString(rightText,256,0)+"\r\n";

	Log+="Länge des unbekannten Teils / Length of unknown part:\r\n";
	Log+=toString(to_ZZ(unknownLength),10,0)+"\r\n";*/

	int rightLength = 0;
	if (rightText!=0)  // calculate length of right
		rightLength = to_int(floor(log(rightText)/log(256.0)))+1;
	
	int leftOffset =  // calculate position of left
		unknownLength + rightLength;
	
	// calculate the constant term of the bracket
	// left*256^pos + right
	ZZ a0 = rightText + power(to_ZZ(256), leftOffset) * leftText;
	// calculate the coefficient of the unknown
	// x*256^|right|
	ZZ a1 = to_ZZ(power(to_ZZ(256), rightLength));
	
	// now we have to exponentiate the bracket term by e
	int i;
	for(i=0; i<=e; i++)
		SetCoeff(poly,i, 
		binom(to_int(e),i)*power(a0,to_int(e-i))*power(a1,i));

	poly=poly-ciphertext%N;

	// We need a monic polynomial, so we first look for 
	// the multiplicative inverse of the leading coefficient
	// using the extended GCD function.
	// Then multiply all coefficients with it and reduce mod N.
	ZZ inv,d,t;
	XGCD(d, inv,t, LeadCoeff(poly), N);
	for(i=0; i<=deg(poly); i++)
		SetCoeff(poly,i,coeff(poly,i)*inv%N);

	//Log+="Zu lösendes Polynom / Polynomial to solve:\r\n";
	//Log+=readable(poly)+"\r\n";

};

void StereotypedAttack::buildLattice(){
	long k = to_long(e);
	long lh = to_long(h);
	updateX();
	ZZX *polyPowers;
	polyPowers = new ZZX[lh];
	ZZX ptmp; //calculate p(x)^v
	SetCoeff(ptmp, 0, 1);
	for(int a=0; a < h && status!=SA_CANCELED; a++){
		polyPowers[a]=ptmp;
		ptmp*=poly;
	}
	//Log+="Aufgestelltes Gitter / Built lattice:\r\n";
	//Log+="[\r\n";
	if(!IsZero(X)){
		HGMatrix.SetDims(lh*k, lh*k);
		for(int i=1; i <= lh*k && status!=SA_CANCELED; i++){
			//Log+="[";
			long v = (i - 1) / k;
			long u = (i - 1) - k * v;
			ZZX qi;
			SetCoeff(qi, u, power(N,(lh-1-v)));
			qi*=polyPowers[v];//ptmp;
			for(int j=1; j <= lh*k && status!=SA_CANCELED; j++){
				HGMatrix[i-1][j-1]=coeff(qi, j-1)*power(X, j-1);
				//Log+=toString(HGMatrix[i-1][j-1],10,0)+" ";
			}
			//Log+="]\r\n";
		}
	}
	//Log+="]\r\n";
	delete [] polyPowers;
};
ZZ StereotypedAttack::binom(int i, int j)
{
	ZZ b;
	int k;
	b=1;
	for (k=2; k<=i; k++)   b=b*k;
	for (k=2; k<=j; k++)   b=b/k;
	for (k=2; k<=i-j; k++) b=b/k;
	return b;
}
void StereotypedAttack::reduceLattice(){
	long x = LLL_XD(HGMatrix, 0.75,0,StopLLL,0);
    
	for(int i=0; i < HGMatrix.NumCols();i++)
		for(int j=0; j< HGMatrix.NumRows(); j++)
			HGMatrix[i][j]=HGMatrix[i][j]/power(X, j);
};

void StereotypedAttack::findSolution(){
	ZZX result;
	vec_ZZ roots;
	int i;
	for(i=0; i< HGMatrix.NumCols(); i++)
		SetCoeff(result,i,HGMatrix[0][i]);
	if (FindRoots(result, roots))
		for(i=0; i < roots.length(); i++){
			solution=-1*roots[i];
		}
};

bool StereotypedAttack::FindRoots(ZZX f, vec_ZZ& r){
	vec_pair_ZZX_long factors;
	ZZ c;
	factor(c,factors,f);
	r.SetLength(0);
	for(int i=0; i < factors.length(); i++)
		if(deg(factors[i].a)==1
			&&LeadCoeff(factors[i].a)==1)
			append(r, ConstTerm(factors[i].a));
	return (r.length()>0);
}

// Getters and setters
void StereotypedAttack::setN(ZZ& N){
	this->N=N;
	updateX();
}

ZZ& StereotypedAttack::getN(){
	return N;
}

void StereotypedAttack::setE(ZZ& e){
	this->e=e;
	updateX();
}

ZZ& StereotypedAttack::getE(){
	return e;
}

void StereotypedAttack::setH(ZZ& h){
	this->h=h;
	updateX();
}

ZZ& StereotypedAttack::getH(){
	return h;
}

void StereotypedAttack::setLeftText(ZZ& leftText){
	this->leftText=leftText;
}

void StereotypedAttack::setRightText(ZZ& rightText){
	this->rightText=rightText;
}

void StereotypedAttack::setCiphertext(ZZ& ciphertext){
	this->ciphertext=ciphertext;
}

void StereotypedAttack::setUnknownLength(int unknownLength){
	this->unknownLength=unknownLength;
}

ZZ& StereotypedAttack::getSolution(){
	return this->solution;
}

ZZ& StereotypedAttack::getBound(){
	return X;
}

void StereotypedAttack::updateX(){
	X=to_ZZ(0);
	if(N>0){
		if(e>0){
			if(h>0){
				RR hk= to_RR(h*e);
				X=CeilToZZ(pow(to_RR(2),to_RR(-0.5f))
					*pow(hk,to_RR(-1)/(hk - 1))
					*pow(to_RR(N),to_RR(h-1)/(hk - 1))-1);
			}
		}
	}
}

int StereotypedAttack::getLatticeTime(){
	if(status==SA_BUILDING)
		return (int)(GetTime() - buildLatticeTime);
	else
		return (int)(buildLatticeTime);
}
int StereotypedAttack::getReductionTime(){
	if(status==SA_REDUCING)
		return (int)(GetTime()-reduceLatticeTime);
	else return (int)reduceLatticeTime;
}

int StereotypedAttack::getOverallTime(){
	return (int)(GetTime()-startTime);
}


//CString StereotypedAttack::timeStamp()
//{
//	int t=(int)(GetTime()-startTime);
//	CString timeFormat;
//	timeFormat.Format("***%3dh%3dm%3ds*** ",
//		t/3600,
//		(t/60)%60,
//		(t)%60);
//	return timeFormat;
//}
