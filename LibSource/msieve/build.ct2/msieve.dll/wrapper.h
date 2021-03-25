#pragma once

void showProgress(void* conf, int num_relations, int max_relations);
int prepare_sieving(void* conf, int update, void* core_sieve_fcn, int max_relations);
void throwException(char* message);

struct relation
{
	uint32 sieve_offset;
	uint32 *fb_offsets;
	uint32 num_factors;
	uint32 poly_index;
	uint32 large_prime1;
	uint32 large_prime2;
};

struct package_element
{
	int type;	// 0 = relation; 1 = poly
	struct relation rel;
	char polybuf[256];
};

typedef struct
{
	int package_count;
	int package_capacity;
	struct package_element *package_array;
	int num_relations;
} RelationPackage;