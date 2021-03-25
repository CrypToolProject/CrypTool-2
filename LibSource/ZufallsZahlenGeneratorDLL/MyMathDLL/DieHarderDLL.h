#include "libdieharder.h"
#include <setjmp.h>

#pragma once

#ifdef MYMATHDLL_EXPORTS
#define MYMATHDLL_API __declspec(dllexport)
#else
#define MYMATHDLL_API __declspec(dllimport)
#endif

char generator_name[128];
unsigned int tflag, tflag_default;
double rng_avg_time_nsec, rng_rands_per_second;
int dtest_num;
char dtest_name[128];
char table_separator;
double strategy;

void Exit(int exitCode);
void output_rnds();
void time_rng();

void run_all_tests();
void run_test();
int execute_test(int);
int passed;
int reachedEOF;
jmp_buf env;
int fasterFactor;

FILE *dataIN;
int byteCount;

MYMATHDLL_API Dtest * getDTest(int i);
MYMATHDLL_API unsigned int initializeTest_types();
MYMATHDLL_API void set_globals();
MYMATHDLL_API void set_ALL(int b);
MYMATHDLL_API void choose_rng();
MYMATHDLL_API void list_rngs();
MYMATHDLL_API boolean run_tests();
MYMATHDLL_API void set_DtestNum();
MYMATHDLL_API int get_passed();
MYMATHDLL_API int get_EOF();
MYMATHDLL_API void set_generator(int g);
MYMATHDLL_API unsigned int getNextUInt();
MYMATHDLL_API void setFasterFactor(int f);
MYMATHDLL_API void setNTuple(int n);
MYMATHDLL_API void setSeed(int s);
MYMATHDLL_API void closeFile();

int TDEFAULT;
int THEADER;
int TSHOW_RNG;
int TLINE_HEADER;
int TTEST_NAME;
int TNTUPLE ;
int TTSAMPLES;
int TPSAMPLES;
int TPVALUES;
int TASSESSMENT ;
int TPREFIX ;
int TDESCRIPTION ;
int THISTOGRAM ;
int TSEED;
int TRATE ;
int TNUM;
int TNO_WHITE;
