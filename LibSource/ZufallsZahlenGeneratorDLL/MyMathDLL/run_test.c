/*
*========================================================================
* See copyright in copyright.h and the accompanying file COPYING
*========================================================================
*/
#include "libdieharder.h"
#include "DieHarderDLL.h"

void run_test()
{

	int i;

	/*
	* ========================================================================
	* This is where I'm installing the new dh_test_types[].
	* ========================================================================
	*/
	if (dtest_num < 0){
		/* printf("dtest_name = %s\n",dtest_name); */
		for (i = 0; i<MAXTESTS; i++){
			if (dh_test_types[i]){
				/* printf("Trying %s\n",dh_test_types[i]->sname); */
				if (strncmp(dh_test_types[i]->sname, dtest_name, 128) == 0){
					dtest_num = i;
					break;
				}
			}
		}
	}
	if (dtest_num >= 0){
		execute_test(dtest_num);
	}
	else {
		fprintf(stderr, "Error:  dtest_num = %d.  No test found.\n", dtest_num);
		exit(1);
	}

}