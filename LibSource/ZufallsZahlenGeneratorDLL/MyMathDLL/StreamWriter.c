#include "MyMathDll.h"

FILE* writeFILE;
void createStream()
{
	//writeFILE = fopen("data.txt", "wb");

}

void writeToStream(char input)
{
	char buffWrite[1];
	memset(buffWrite, 0, sizeof(buffWrite));
	buffWrite[0] = input;
	int suc = fwrite(buffWrite, 1, 1, writeFILE);
}

void endStream()
{
	fclose(writeFILE);
}
