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

#include "BloemerMayAttack.h"

// static status variables
int BloemerMayAttack::status = BM_CANCELED;
long BloemerMayAttack::reductions=0;
void BloemerMayAttack::attack(){
	status=1;// building Lattice
	reductions=0;
	resultantsCheck=0;
	buildLatticeTime =0.0;
	reduceLatticeTime=0.0;
	findResultantTime=0.0;
	p=to_ZZ(0);
	q=to_ZZ(0);
	startTime=GetTime();
	
	log=timeStamp()+" Bloemer-May-Angriff gestartet / Bloemer-May-Attack started\r\n";

	buildPolyPowers();
	if(status!=BM_CANCELED){
		log+=timeStamp()+" Erzeuge Gitter / Building lattice\r\n";
		status=BM_BUILDING;
		buildLattice();
	}

	buildLatticeTime=GetTime()-buildLatticeTime;
	if(status!=BM_CANCELED){
		status=BM_DELETING;// deleting Vectors
		deleteVectors();
	}

	if(status!=BM_CANCELED){
		log+=timeStamp()+" Reduziere Gitter / Reducing lattice\r\n";
		status=BM_REDUCING;// reducing Lattice
		reduceLattice();
	}
	reduceLatticeTime=GetTime()-reduceLatticeTime;

	if(status!=BM_CANCELED){
		status=BM_RECONSTRUCTING;// reconstructing delteted Vectors
		reconstructLattice();
	}

	
	if(status!=BM_CANCELED){
		status=BM_BUILDINGRESULTANT;// calculation resultants
		getSolution();
	}
	if(status==BM_CANCELED)
		log+=timeStamp()+" Abbruch durch Benutzer / Canceled by user:\r\n";
	else
	if(!IsZero(p)){
		status=BM_SUCCESSFUL; // successful
		log+=timeStamp()+" Lösung gefunden / Found solution:\r\n";
		log+="p="+toString(p,10,0)+"\r\n";
		log+="q="+toString(q,10,0);
	}
	else{
		log+=timeStamp()+" Ohne Erfolg beendet / Finished without solution:\r\n";
		status=BM_FAILED; // failed
	}
}

ZZ BloemerMayAttack::binom(int i, int j)
{
	ZZ b;
	int k;
	b=1;
	for (k=2; k<=i; k++)   b=b*k;
	for (k=2; k<=j; k++)   b=b/k;
	for (k=2; k<=i-j; k++) b=b/k;
	return b;
}

void BloemerMayAttack::buildPolyPowers()
{
	buildLatticeTime=GetTime();
	A=(N+1);
	X=to_ZZ(pow(to_RR(e), to_RR(delta)));
	Y=3*SqrRoot(e);
	CString formatParam;
	formatParam.Format("delta=%f\r\nm=%d\r\nt=%d\r\n", delta,m,t);
	log+=timeStamp()+" Parameters:\r\n";
	log=log+formatParam;
	log=log+"e="+toString(e,10,0)+"\r\n";
	log=log+"N="+toString(N,10,0)+"\r\n";
	log=log+"X="+toString(X,10,0)+"\r\n";
	log=log+"Y="+toString(Y,10,0)+"\r\n";
	ZZ tmpKoeff;
	ZZ sign;
	ZZ bino;
	delete [] polyPowers;
	polyPowers = new ZZXY[m+1];
	int i,j,k;
	int s,bo,ko;
	for (k=0; k<=m; k++) {
		for (i=0; i<=k; i++) {
			for (j=0; j<=i && status!=BM_CANCELED; j++) {
				sign=power(to_ZZ(-1),i-j);
				s=to_int(sign);
				bino=binom(k,i)*binom(i,j);
				bo=to_int(bino);
				tmpKoeff=power(A,k-i)*power(X,k-i+j)*power(Y,j);
				ko=to_int(tmpKoeff);
				polyPowers[k].setCoeff(k-i+j,j,sign*bino*tmpKoeff);
			}
		}
	}


}

void BloemerMayAttack::buildLattice(){
	int i, j, k;
	dim=(m+1)*(m+2)/2+t*(m+1);
	Lattice.kill();
	Lattice.SetDims(dim,dim);
	delete [] columnXPowers;
	columnXPowers = new int[dim];
	delete [] columnYPowers;
	columnYPowers = new int[dim];

	int column=0;
	int row=0;

	for(i=0; i <= m; i++)
		for(j=0; j<=i; j++){
			columnXPowers[column]=i;
			columnYPowers[column]=j;
			column++;
		}
	
	for(i=0; i < t; i++)
		for(j=0; j<=m; j++){
			columnXPowers[column]=j;
			columnYPowers[column]=j+i+1;
			column++;
		}

	ZZXY g;
	ZZXY tmp;
	int c=0;
	for(k=0;k<=m;k++)
		for(i=0;i<=k && status!=BM_CANCELED;i++){
			tmp.kill();
			g=polyPowers[i];
			tmp.setCoeff(k-i,0,power(X,k-i));
			g=g*tmp;
			g=g*power(e,m-i);
			for(j=0;j<dim;j++)
				Lattice[c][j]=g.getCoeff(columnXPowers[j],columnYPowers[j]);
			c++;
		}

	ZZXY tmp2;
	for(i=1;i<=t;i++)
		for(k=0;k<=m && status!=BM_CANCELED;k++){
			tmp.kill();
			g=polyPowers[k];
			tmp2.setCoeff(0,i,power(Y,i));
			g=g*tmp2;
			g=g*power(e,m-k);
			for(j=0;j<dim;j++)
				Lattice[c][j]=g.getCoeff(columnXPowers[j],columnYPowers[j]);
			c++;
		}
	
}


void BloemerMayAttack::deleteVectors(){
	mat_ZZ merk=Lattice;
	int i,j,k,b,z;
	ZZ tmp;
	int firstYBlock=(m+2)*(m+1)/2;
	
	delete [] deletedVectors;
	deletedVectors = new bool[Lattice.NumCols()];
	z=0;

	for(i=0;i<Lattice.NumCols();i++)
		deletedVectors[i]=false;

	z=0;
	for(i=0; i<m-t ; i++)
		for(j=0; j<=i; j++){
			deletedVectors[z]=true;
			z++;
		}

		
	for(i=0; i<t; i++)//in the Yi Block...
		for(j=0; j<=m+i-t;j++){ // ...delete the first m+i-t vectors
			deletedVectors[firstYBlock+i*(m+1)+j]=true;
			z++;
		}
		
	Lattice.kill();
	Lattice.SetDims(dim-z,dim-z);
	k=0;
	b=0;
	CString dimForm;
	dimForm.Format("%d",dim-z);
	log+=timeStamp()+" Lattice dimension ="+ dimForm +" \r\n";
	log+=timeStamp()+" Lattice = \r\n";
	log+="[\r\n";
	for(i=0; i<merk.NumCols(); i++){
		if(!deletedVectors[i]){
			log+="[";
			for(j=0; j<merk.NumCols(); j++){
				if(!deletedVectors[j]){
					log+=toString(merk[i][j],10,0)+" ";
					Lattice[k][b++]=merk[i][j];
				}
			}
			log+="]\r\n";
			b=0;
			k++;
		}		
	}
	log+="]\r\n";

}

void BloemerMayAttack::reduceLattice(){
	// Here the LLL algorithm is called.
	// It gets the StopLLL function as parameter.
	//verbose+= "Reducing lattice (LLL)\r\n";
	ZZ tu;
	//long x = LLL(tu,Lattice);
	
	reduceLatticeTime=GetTime();
	long x = LLL_XD(Lattice, 0.99,0,StopLLL,0);


}
void BloemerMayAttack::getSolution(){
	ZZX result;
	dim=Lattice.NumCols();
	int newDim=(m+1)*(t+1);
	ZZ XYPow,tmp,bino;
	vec_ZZ roots;
	int i,j,k,l,b,x;
	l=0;
	k=0;
	b=0;
	int *treffer = new int[dim];
	int anzTreffer=0;
	ZZXY *polynomials= new ZZXY[dim];
	ZZ *norms = new ZZ[dim];

	ZZ norm;
	for(i=0; i< newDim; i++){
		for(j=0; j< dim; j++){
			if(!IsZero(Lattice[i][j])){
				int x=columnXPowers[j];
				int y=columnYPowers[j];
				XYPow=power(X,x);
				XYPow*=power(Y,y);
				polynomials[i].setCoeff(y,x,Lattice[i][j]/XYPow);

			}
		}
	}

	for(i=0; i<newDim; i++){
		double sq=sqrt(to_double(polynomials[i].monomCount()));
		ZZ norm;
		if(sq>0)
			norm=polynomials[i].norm(Y,X);
		else 
			norm =to_ZZ(0);
		norm=to_ZZ(to_RR(norm)*to_RR(sq));

		if(sq>0&&norm>to_ZZ(0)&&norm<power(e,m))
			norm=to_ZZ(1); // make sure this one gets evaluated first
		

		int a;
		// sorting the polynomials by norm
		if(IsZero(norm))
			for(a=0; !IsZero(norms[a]); a++);
		else
			for(a=0; !IsZero(norms[a])&&norms[a]<=norm; a++);

		for(int b=dim-1; b > a; b--){
			treffer[b]=treffer[b-1];
			norms[b]=norms[b-1];
		}
		treffer[a]=i;
		norms[a]=norm;
	}
	log+=timeStamp()+" Gefundene Polynome / Polynomials found:\r\n";
	CString polyName;
	for(j=1; j<newDim&&status!=BM_CANCELED; j++){
		polyName.Format("f%d=",j);
		log+=polyName+polynomials[treffer[j]].print()+"\r\n";
	}
	bool resultFound=false;
	for(j=1; j<newDim&&!resultFound;j++){
		for(i=0; i<j&&status!=BM_CANCELED&&!resultFound;i++){
			resultantsCheck++;
			findResultantTime=GetTime();
			polyName.Format("RES(f%d,f%d)\r\n",treffer[i],treffer[j]);
			log+=timeStamp()+" Calculating resultant "+polyName;
			result=polynomials[treffer[i]].resultant(polynomials[treffer[j]],&status);
			if(FindRoots(result, roots)){
				for(x=0; x < roots.length()&&!resultFound; x++){
					ZZ s=roots[x];
					if(s*s>4*N){
						p=(s+SqrRoot(s*s-4*N))/2;
						q=(s-SqrRoot(s*s-4*N))/2;
						if(p*q==N)
							resultFound=true;
					}
				}
			}
		}
	}
	if(!resultFound){
		p=0;
		q=0;
	}
	delete [] treffer;
	delete [] polynomials;
	delete [] norms;
}

bool BloemerMayAttack::FindRoots(ZZX f, vec_ZZ& r){
	vec_pair_ZZX_long factors;
	ZZ c;
	factor(c,factors,f);// factor polynomial
	r.SetLength(0);
	// look for  factors os the form (x - a)
	for(int i=0; i < factors.length(); i++)
		if(deg(factors[i].a)==1
			&&LeadCoeff(factors[i].a)==1)
			append(r, ConstTerm(factors[i].a));
	return (r.length()>0);
}

void BloemerMayAttack::reconstructLattice(){
	mat_ZZ reconstructed;
	ZZ tmp;
	int i,j,k,l,b,w;
	int bino;
	l=0;
	k=0;
	b=0;

	reconstructed.SetDims(dim,dim);

	for(i=0; i<dim; i++)
		if(!deletedVectors[i]){

			w=0;
			for(j=0; j<dim; j++){
				if(deletedVectors[j])
					reconstructed[i-b][j]=to_ZZ(0);
				else{
					reconstructed[i-b][j]=Lattice[k][l++];

					if(!IsZero(Lattice[k][l-1]))
					w++;
				}

			}

			l=0;
			k++;
		}else b++;

	Lattice=reconstructed;
	int YBlockstart=(m+2)*(m+1)/2;

	for(j=0; j<Lattice.NumCols(); j++){
		for(i=Lattice.NumCols()-1; i>=0; i--){
			tmp=to_ZZ(0);
			if(deletedVectors[i]){
				for(k=i+1; k<Lattice.NumCols(); k++){
					b=columnXPowers[k]-columnXPowers[i];
					if(b==(columnYPowers[k]-columnYPowers[i])&&b>0){
						if(k<=YBlockstart)
							bino=to_int(binom(columnYPowers[i]+b,columnYPowers[i]));
						else
							bino=to_int(binom(columnXPowers[i]+b,columnXPowers[i]));
						tmp+=to_ZZ(-1*bino)*Lattice[j][k]/power(X*Y,b);
					}
				}
				Lattice[j][i]=tmp;
			}
		}
	}

}


// Method to stop the attack.
void BloemerMayAttack::cancel(){
	status=BM_CANCELED;
}
// A pointer to this function is given to the LLL.
// It is called after each reduction step and stops
// it if anything but 0 is returned
long BloemerMayAttack::StopLLL(const NTL::vec_ZZ& z){
	reductions++;
	if(status!=BM_CANCELED) return 0;
	else return 1;

}



BloemerMayAttack::BloemerMayAttack()
{
	polyPowers = NULL;
	columnXPowers = NULL;
	columnYPowers = NULL;
	deletedVectors= NULL;
}

BloemerMayAttack::~BloemerMayAttack()
{

	delete [] polyPowers;
	delete [] columnXPowers;
	delete [] columnYPowers;
	delete [] deletedVectors;

}

void BloemerMayAttack::setN(ZZ N){
	this->N=N;
}
void BloemerMayAttack::setE(ZZ e){
	this->e=e;
}
void BloemerMayAttack::setM(int m){
	this->m=m;
}
void BloemerMayAttack::setT(int t){
	this->t=t;
}
void BloemerMayAttack::setDelta(double delta){
	this->delta=delta;
}
ZZ BloemerMayAttack::getN(){
	return N;
}
ZZ BloemerMayAttack::getE(){
	return e;
}
ZZ BloemerMayAttack::getP(){
	return p;
}
ZZ BloemerMayAttack::getQ(){
	return q;
}
int BloemerMayAttack::getM(){
	return m;
}

int BloemerMayAttack::optimalT(int m)
{
	RR retval;
	RR tmpM;
	RR d;
	RR t;
	tmpM=to_RR(m);
	tmpM=to_RR(m);
	d=3*tmpM*(2*tmpM+1)*(tmpM+1)*(tmpM+1);
	
	d=6*sqrt(d);
	d=d-6*tmpM*tmpM-12*tmpM-2;
	d=d/(to_RR(30)*tmpM*tmpM+to_RR(36)*tmpM-to_RR(2));
	delta=to_double(d);
	t=sqrt(-36*d*d*tmpM*tmpM-36*d*tmpM+3+12*d*d-36*d*d*tmpM-18*d*tmpM*tmpM-6*d+9*tmpM*tmpM+9*tmpM);
	t=12*d*tmpM-6+2*t;
	t=t/(12*d+6);
	double delta=to_double(t);
	if(t>2)
		return to_int(t);
	else
		return 2;
}

double BloemerMayAttack::maxDelta(int m, int t)
{
	RR retval;
	retval=to_RR(t*t+2*t-3*m*m-3*m);
	retval=-1*retval/to_RR(2*t*t-6*m*t-2*t+12*m+12*m*m);
	return to_double(retval);
}
int BloemerMayAttack::getLatticeTime(){
	if(status==BM_BUILDING)
		return (int)(GetTime() - buildLatticeTime);
	else
		return (int)(buildLatticeTime);
}
int BloemerMayAttack::getReductionTime(){
	if(status==BM_REDUCING)
		return (int)(GetTime()-reduceLatticeTime);
	else return (int)reduceLatticeTime;
}

int BloemerMayAttack::getResultantTime(){
	if(findResultantTime<0.1)
		return 0;
	else
		return (int)(GetTime()-findResultantTime);
}

int BloemerMayAttack::getOverallTime(){
	return (int)(GetTime()-startTime);
}
int BloemerMayAttack::getResultants(){
	return resultantsCheck;
}

CString BloemerMayAttack::timeStamp()
{
	int t=(int)(GetTime()-startTime);
	CString timeFormat;
	timeFormat.Format("***%3dh%3dm%3ds*** ",
		t/3600,
		(t/60)%60,
		(t)%60);
	return timeFormat;
}
