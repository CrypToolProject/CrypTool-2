#include "StereotypedAttack_Wrapper.h"
#include "StereotypedAttack.h"
using namespace std;

namespace NTL
{
	public ref class StereotypedAttack_Wrapper
	{		

	private:
		StereotypedAttack *_stereotypedAttack;

	public:
		StereotypedAttack_Wrapper(String^ N, String^ e, String^ leftText, String^ rightText, int unknownLength, String^ cipherText, String^ h)
		{
			_stereotypedAttack = new StereotypedAttack();		
			ZZ zzN;
			zzN = conv<ZZ>((char *) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(N).ToPointer());
			_stereotypedAttack->setN(zzN);

			ZZ zzE;
			zzE = conv<ZZ>((char *) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(e).ToPointer());
			_stereotypedAttack->setE(zzE);

			ZZ zzLeftText;
			zzLeftText = conv<ZZ>((char *) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(leftText).ToPointer());
			_stereotypedAttack->setLeftText(zzLeftText);

			ZZ zzRightText;
			zzRightText = conv<ZZ>((char *) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(rightText).ToPointer());
			_stereotypedAttack->setRightText(zzRightText);

			_stereotypedAttack->setUnknownLength(unknownLength);

			ZZ zzCipherText;
			zzCipherText = conv<ZZ>((char *) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(cipherText).ToPointer());
			_stereotypedAttack->setCiphertext(zzCipherText);

			ZZ zzH;
			zzH = conv<ZZ>((char *) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(h).ToPointer());
			_stereotypedAttack->setH(zzH);
		}
		~StereotypedAttack_Wrapper()
		{    
			delete _stereotypedAttack;
		}

		void Attack ()
		{
			_stereotypedAttack->attack();				
		}	

		String^ GetSolution()
		{
			ZZ solution = _stereotypedAttack->getSolution();	

			String^ result ("");	
			Char tmp;
			while(solution>0){
				tmp = HEXenc(to_int(solution % to_ZZ(10)));
				if(tmp!=0)
					result = System::String::Concat(tmp, result);
				
				solution /= 10;
			}
			
			return result;
		}

		char HEXenc(int v){
			char* tab = "0123456789ABCDEF";
			if (v>=0 && v<16)
				return tab[v];
			else
				return 0;
		}
	};
}