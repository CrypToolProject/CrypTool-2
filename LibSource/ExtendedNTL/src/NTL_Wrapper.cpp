#include "NTL/LLL.h"
#include <iostream>
#include <vector>
using namespace System::Numerics; 
using namespace std;
using namespace System;
using namespace System::Collections::Generic; 

namespace NTL
{
	public ref class NTL_Wrapper
	{			

	public:
		NTL_Wrapper()
		{			
		}
		~NTL_Wrapper()
		{    
		}

		//Obsolet
		cli::array<BigInteger,2>^ LLLReduce (cli::array<BigInteger,2>^ matrix, long dim, double delta)
		{
			mat_ZZ B, U;	
			vector<mat_ZZ> S;

			B.SetDims(dim, dim);

			for (int i = 1; i <= dim; i++)
			{
				for (int j = 1; j <= dim; j++) 
				{		
					B(i,j) = conv<ZZ>((char *) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(matrix[i - 1,j - 1].ToString()).ToPointer());
				}
			}

			LLL_FP(B, U, S, delta);

			for (int i = 1; i <= dim; i++)
			{
				for (int j = 1; j <= dim; j++) 
				{	
					matrix[i-1,j-1] = ConvertFromZZToBigInt(B(i,j));
				}
			}

			auto transMatrix  = gcnew cli::array<BigInteger,2>(dim, dim);
			for (int i = 1; i <= dim; i++)
			{
				for (int j = 1; j <= dim; j++) 
				{	
					transMatrix[i-1,j-1] = ConvertFromZZToBigInt(U(i,j));
				}
			}
			return transMatrix;
		}

		void LLLReduce (cli::array<BigInteger,2>^ matrix,  cli::array<BigInteger,2>^ transMatrix, List<cli::array<BigInteger,2>^>^ steps, long n, long m, double delta)
		{
			mat_ZZ B, U;
			vector<mat_ZZ> S;

			B.SetDims(n, m);

			for (int i = 1; i <= n; i++)
			{
				for (int j = 1; j <= m; j++) 
				{		
					B(i, j) = conv<ZZ>((char *) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(matrix[i - 1, j - 1].ToString()).ToPointer());
				}
			}

			LLL_FP(B, U, S, delta);

			for (int i = 1; i <= n; i++)
			{
				for (int j = 1; j <= m; j++) 
				{	
					matrix[i-1, j-1] = ConvertFromZZToBigInt(B(i, j));
				}
			}

			for (int i = 1; i <= n; i++)
			{
				for (int j = 1; j <= n; j++) 
				{	
					transMatrix[i-1, j-1] = ConvertFromZZToBigInt(U(i, j));
				}
			}

			for (int k = 0; k < S.size(); k++)
			{
				mat_ZZ step = S[k];
				auto stepMatrix = gcnew cli::array<BigInteger,2>(n, m);
				
				for (int i = 1; i <= n; i++)
				{
					for (int j = 1; j <= m; j++) 
					{	
						stepMatrix[i-1, j-1] = ConvertFromZZToBigInt(step(i, j));
					}
				}
				steps->Add(stepMatrix);
			}
		}

		BigInteger ConvertFromZZToBigInt(ZZ zz)
		{
			String^ result ("");	
			Char tmp;
			bool setMinus = false;

			if (zz < 0)
			{
				setMinus = true;
				zz *= -1;
			}

			while(zz > 0)
			{
				tmp = INTenc(to_int(zz % to_ZZ(10)));
				if(tmp!=0)
					result = System::String::Concat(tmp, result);

				zz /= 10;
			}

			if (setMinus)
				result = System::String::Concat("-", result);
			if (result == "")
				result = "0";

			return BigInteger::Parse(result);
		}


		char INTenc(int v)
		{
			char* tab = "0123456789";
			if (v>=0 && v<10)
				return tab[v];
			else
				return 0;
		}

		BigInteger Determinant (cli::array<BigInteger,2>^ matrix, int dim)
		{
			mat_ZZ B;

			B.SetDims(dim, dim);

			for (int i = 1; i <= dim; i++)
			{
				for (int j = 1; j <= dim; j++) 
				{		
					B(i,j) = conv<ZZ>((char *) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(matrix[i - 1,j - 1].ToString()).ToPointer());
				}
			}

			ZZ det = determinant(B, 0);
			
			return ConvertFromZZToBigInt(det);	
		}

		BigInteger ModInverse (BigInteger a, BigInteger mod)
		{
			ZZ aZZ, modZZ;

			aZZ = conv<ZZ>((char *) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(a.ToString()).ToPointer());
			modZZ = conv<ZZ>((char *) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(mod.ToString()).ToPointer());

			ZZ invModZZ = InvMod(aZZ, modZZ);

			return ConvertFromZZToBigInt(invModZZ);	
		}
	};
}