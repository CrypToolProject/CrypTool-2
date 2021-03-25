/**
* This .Net msieve wrapper was written by Sven Rech (rech@cryptool.org)
**/

#include <msieve.h>
#include "wrapper.h"
#include "../../mpqs/mpqs.h"
extern "C" {

}

using namespace System;
using namespace System::Collections;

//From demo.c:
extern "C" msieve_factor* factor(char *number, char *savefile_name);
extern "C" char* getNextFactor(msieve_factor** factor);
extern "C" void stop_msieve(msieve_obj *obj);
extern "C" void get_random_seeds(uint32 *seed1, uint32 *seed2);
//From sieve.c:
extern "C" void collect_relations(sieve_conf_t *conf, uint32 target_relations, qs_core_sieve_fcn core_sieve_fcn);
//From relation.c:
extern "C" void save_relation(sieve_conf_t *conf, uint32 sieve_offset,
		uint32 *fb_offsets, uint32 num_factors, 
		uint32 poly_index, uint32 large_prime1, uint32 large_prime2);
//From driver.c:
extern "C" uint32 msieve_run_core(msieve_obj *obj, mp_t *n, 
				factor_list_t *factor_list);

//Copy a sieve configuration that can be used in a different thread:
sieve_conf_t *copy_sieve_conf(sieve_conf_t *conf) {
	sieve_conf_t *copy = (sieve_conf_t*)malloc(sizeof(sieve_conf_t));
	msieve_obj *objcopy = (msieve_obj*)malloc(sizeof(msieve_obj));

	*copy = *conf;
	*objcopy = *(conf->obj);
	copy->obj = objcopy;
	copy->slave = 1;	//we are a slave

	//threads shouldn't be allowed to access files or the factor list:
	objcopy->logfile_name = 0;
	objcopy->savefile.fp = 0;
	objcopy->factors = 0;
	
	//threads shouldn't be allowed to access these fields:
	copy->poly_a_list = 0;
	copy->poly_list = 0;	
	copy->relation_list = 0;
	copy->num_relations = 0;
	copy->cycle_list = 0;
	copy->num_cycles = 0;
	copy->cycle_table = 0;
	copy->cycle_hashtable = 0;
	copy->cycle_table_size = 0;
	copy->cycle_table_alloc = 0;
	copy->components = 0;
	copy->vertices = 0;

	//deep copies:
	copy->sieve_array = (uint8 *)aligned_malloc(
				(size_t)copy->sieve_block_size, 64);
	for (uint32 i = 0; i < copy->sieve_block_size; i++)
		copy->sieve_array[i] = conf->sieve_array[i];

	copy->factor_base = (fb_t *)xmalloc(objcopy->fb_size * sizeof(fb_t));
	for (uint32 i = 0; i < objcopy->fb_size; i++)
		copy->factor_base[i] = conf->factor_base[i];

	copy->packed_fb = (packed_fb_t *)xmalloc(conf->tf_large_cutoff * sizeof(packed_fb_t));
	for (uint32 i = 0; i < conf->tf_large_cutoff; i++)
		copy->packed_fb[i] = conf->packed_fb[i];

	copy->buckets = (bucket_t *)xcalloc((size_t)(copy->poly_block *
						copy->num_sieve_blocks), 
						sizeof(bucket_t));
	for (uint32 i = 0; i < copy->poly_block * copy->num_sieve_blocks; i++) {
		copy->buckets[i].num_alloc = 1000;
		copy->buckets[i].list = (bucket_entry_t *)
				xmalloc(1000 * sizeof(bucket_entry_t));
	}

	copy->modsqrt_array = (uint32 *)xmalloc(objcopy->fb_size * sizeof(uint32));
	for (uint32 i = 0; i < objcopy->fb_size; i++)
		copy->modsqrt_array[i] = conf->modsqrt_array[i];

	//we need new seeds:
	uint32 seed1, seed2;
	get_random_seeds(&seed1, &seed2);
	copy->obj->seed1 = seed1;
	copy->obj->seed2 = seed2;

	poly_init(copy, copy->num_sieve_blocks * copy->sieve_block_size / 2);

	return copy;
}

namespace Msieve
{
	public delegate bool prepareSievingDelegate(IntPtr conf, int update, IntPtr core_sieve_fcn, int max_relations);
	public delegate void putTrivialFactorlistDelegate(IntPtr list, IntPtr obj);

	public ref struct callback_struct
	{
	public:
		prepareSievingDelegate^ prepareSieving;
		putTrivialFactorlistDelegate^ putTrivialFactorlist;
	};

	public ref class msieve 
	{
	private:
		static char* stringToCharA(String^ str)
		{
			if (!str)
				return 0;
			char* ch = (char*)malloc(str->Length + 1);
			for (int i = 0; i < str->Length; i++)
				ch[i] = (char)str[i];
			ch[str->Length] = 0;
			return ch;
		}

		static void copyIntToArray(array<unsigned char>^ arr, int pos, int theInt)
		{
			//We always use 4 bytes
			arr[pos] = theInt & 255;
			arr[pos+1] = (theInt >> 8) & 255;
			arr[pos+2] = (theInt >> 16) & 255;
			arr[pos+3] = (theInt >> 24) & 255;
		}

		static int getIntFromArray(array<unsigned char>^ arr, int pos)
		{
			//We always use 4 bytes
			int res = arr[pos];
			res |= arr[pos+1]<<8;
			res |= arr[pos+2]<<16;
			res |= arr[pos+3]<<24;
			
			return res;
		}

	public:
		static callback_struct^ callbacks;

		//initialize msieve with callback functions:
		static void initMsieve(callback_struct^ cb)
		{
			callbacks = cb;
		}

		//start the factorization process:
		static void start(String^ number, String^ savefile)
		{
			char* num = stringToCharA(number);	
			char* save = stringToCharA(savefile);
			factor(num, save);
		}

		//stop msieve:
		static void stop(IntPtr obj)
		{
			stop_msieve((msieve_obj*)obj.ToPointer());
		}

		//clone this conf (the clone can be used to run the sieving in a different thread):
		static IntPtr cloneSieveConf(IntPtr conf)
		{
			return IntPtr(copy_sieve_conf((sieve_conf_t*)conf.ToPointer()));
		}

		//free this conf (shouldn't be the conf file that belongs to the main thread):
		static void freeSieveConf(IntPtr conf)
		{
			sieve_conf_t* c = (sieve_conf_t*)conf.ToPointer();
			if (!c->slave)
				return;
			free(c->obj);
			free(c->next_poly_action);
			free(c->curr_b);
			free(c->poly_b_small[0]);
			free(c->poly_b_array);
			aligned_free(c->sieve_array);
			free(c->factor_base);
			free(c->packed_fb);
			for (uint32 i = 0; i < c->poly_block * c->num_sieve_blocks; i++)
				free(c->buckets[i].list);
			free(c->buckets);
			free(c->modsqrt_array);
			if (c->relationPackage != 0)
			{
				for (int j = 0; j < c->relationPackage->package_count; j++)			
					if (c->relationPackage->package_array[j].type == 0)
						free(c->relationPackage->package_array->rel.fb_offsets);
				free(c->relationPackage->package_array);
				free(c->relationPackage);
			}
			free(c);
		}

		static void collectRelations(IntPtr conf, int target_relations, IntPtr core_sieve_fcn)
		{
			collect_relations((sieve_conf_t*)conf.ToPointer(), target_relations, (qs_core_sieve_fcn)core_sieve_fcn.ToPointer());
		}
		
		//get the relation package in the thread of "conf" (shoudn't be the main thread):
		static IntPtr getRelationPackage(IntPtr conf)
		{
			sieve_conf_t* c = (sieve_conf_t*)conf.ToPointer();
			if (!c->slave)
				return IntPtr::Zero;
			RelationPackage* relationPackage = c->relationPackage;
			c->relationPackage = 0;
			return IntPtr((void*)relationPackage);
		}

		//stores the relation package in the thread of "conf" (should be the main thread), and destroys the relation package:
		static void saveRelationPackage(IntPtr conf, IntPtr relationPackage)
		{
			sieve_conf_t* c = (sieve_conf_t*)conf.ToPointer();
			if (c->slave)
				return;			
			RelationPackage* y = (RelationPackage*)relationPackage.ToPointer();

			for (int j = 0; j < y->package_count; j++)
			{
				if (y->package_array[j].type == 1)
					savefile_write_line(&c->obj->savefile, y->package_array[j].polybuf);
				else
				{
					relation* rel = &y->package_array[j].rel;
					save_relation(c, rel->sieve_offset, rel->fb_offsets, rel->num_factors, rel->poly_index, rel->large_prime1, rel->large_prime2);
					free(rel->fb_offsets);
				}
			}

			free(y->package_array);
			free(y);
		}

		static IntPtr getObjFromConf(IntPtr conf)
		{
			sieve_conf_t* c = (sieve_conf_t*)conf.ToPointer();
			return IntPtr(c->obj);
		}

		static ArrayList^ getPrimeFactors(IntPtr factorList)
		{
            char* buf = (char *)xmalloc(32 * MAX_MP_WORDS + 1);
			ArrayList^ factors = gcnew ArrayList;
			factor_list_t * factor_list = (factor_list_t *)factorList.ToPointer();
			for (int c = 0; c < factor_list->num_factors; c++)
			{
				if (factor_list->final_factors[c]->type != MSIEVE_COMPOSITE)
				{
					char* factor = mp_sprintf(&factor_list->final_factors[c]->factor, 10, buf);
					factors->Add(gcnew String(factor));
				}
			}

            free(buf);
			return factors;
		}

		static ArrayList^ getCompositeFactors(IntPtr factorList)
		{
            char* buf = (char *)xmalloc(32 * MAX_MP_WORDS + 1);
			ArrayList^ factors = gcnew ArrayList;
			factor_list_t * factor_list = (factor_list_t *)factorList.ToPointer();
			for (int c = 0; c < factor_list->num_factors; c++)
			{
				if (factor_list->final_factors[c]->type == MSIEVE_COMPOSITE)
				{
					char* factor = mp_sprintf(&factor_list->final_factors[c]->factor, 10, buf);
					factors->Add(gcnew String(factor));
				}
			}

            free(buf);
			return factors;
		}

		//serialize the relation package, so that you can send it over the net:
		static array<unsigned char>^ serializeRelationPackage(IntPtr relationPackage)
		{
			RelationPackage* y = (RelationPackage*)relationPackage.ToPointer();

			//calculate needed size:
			int size = 0;
			for (int c = 0; c < y->package_count; c++)
			{
				size++;	//type information
				if (y->package_array[c].type == 1)	//poly	(256 bytes)
					size += 256;
				else								//relation	((5+num_factors)*4 bytes)
					size += (5 + y->package_array[c].rel.num_factors)*4;
			}

			//serialize:			
			array<unsigned char>^ out = gcnew array<unsigned char>(size + 4);
			copyIntToArray(out, 0, y->package_count);
			int pos = 4;

			for (int c = 0; c < y->package_count; c++)
			{
				out[pos++] = (char)(y->package_array[c].type);
				if (y->package_array[c].type == 1)	//poly	(256 bytes)
				{
					for (int i = 0; i < 256; i++)
						out[pos++] = y->package_array[c].polybuf[i];
				}
				else								//relation	((5+num_factors)*4 bytes)
				{
					copyIntToArray(out, pos, y->package_array[c].rel.sieve_offset);
					copyIntToArray(out, pos + 4, y->package_array[c].rel.num_factors);
					copyIntToArray(out, pos + 8, y->package_array[c].rel.poly_index);
					copyIntToArray(out, pos + 12, y->package_array[c].rel.large_prime1);
					copyIntToArray(out, pos + 16, y->package_array[c].rel.large_prime2);
					pos += 20;
					for (int i = 0; i < y->package_array[c].rel.num_factors; i++)
					{
						copyIntToArray(out, pos, y->package_array[c].rel.fb_offsets[i]);
						pos += 4;
					}
				}
			}

			return out;
		}

		static IntPtr deserializeRelationPackage(array<unsigned char>^ relationPackage)
		{
			RelationPackage* y = (RelationPackage*)malloc(sizeof(RelationPackage));
			y->package_count = getIntFromArray(relationPackage, 0);
			y->package_array = (package_element*)malloc(sizeof(package_element)*y->package_count);
			int pos = 4;
			
			for (int c = 0; c < y->package_count; c++)
			{
				y->package_array[c].type = relationPackage[pos++];
				if (y->package_array[c].type == 1)	//poly	(256 bytes)
				{
					for (int i = 0; i < 256; i++)
						y->package_array[c].polybuf[i] = relationPackage[pos++];
				}
				else								//relation	((5+num_factors)*4 bytes)
				{
					y->package_array[c].rel.sieve_offset = getIntFromArray(relationPackage, pos);
					y->package_array[c].rel.num_factors = getIntFromArray(relationPackage, pos + 4);
					y->package_array[c].rel.poly_index = getIntFromArray(relationPackage, pos + 8);
					y->package_array[c].rel.large_prime1 = getIntFromArray(relationPackage, pos + 12);
					y->package_array[c].rel.large_prime2 = getIntFromArray(relationPackage, pos + 16);
					pos += 20;
					y->package_array[c].rel.fb_offsets = (uint32*)malloc(sizeof(uint32) * y->package_array[c].rel.num_factors);
					for (int i = 0; i < y->package_array[c].rel.num_factors; i++)
					{
						y->package_array[c].rel.fb_offsets[i] = getIntFromArray(relationPackage, pos);
						pos += 4;
					}
				}
			}
			
			return IntPtr((void*)y);
		}

		static int getAmountOfRelationsInRelationPackage(IntPtr relationPackage)
		{
			RelationPackage* y = (RelationPackage*)relationPackage.ToPointer();
			return y->num_relations;
		}

		static int getNumRelations(IntPtr conf)
		{
			sieve_conf_t* c = (sieve_conf_t*)conf.ToPointer();
			return c->num_relations + 
				c->num_cycles +
				c->components - c->vertices;
		}

		static IntPtr msieve_run_core(IntPtr obj, String^ n)
		{
			try
			{
				mp_t N;
				msieve_obj* o = (msieve_obj*)obj.ToPointer();
				evaluate_expression(stringToCharA(n), &N);
				factor_list_t* factor_list = new factor_list_t;
				factor_list_init(factor_list);
				factor_list_add(o, factor_list, &N);
				::msieve_run_core(o, &N, factor_list);
				return IntPtr(factor_list);
			}
			catch (...)
			{
				return IntPtr::Zero;
			}
		}
	};

}

extern "C" int prepare_sieving(void* conf, int update, void* core_sieve_fcn, int max_relations)
{
	return Msieve::msieve::callbacks->prepareSieving(IntPtr(conf), update, IntPtr(core_sieve_fcn), max_relations) ? 1 : 0;
}

extern "C" void throwException(char* message)
{
	throw gcnew Exception(gcnew String(message));
}

extern "C" void put_trivial_factorlist(factor_list_t * factor_list, msieve_obj *obj)
{
	Msieve::msieve::callbacks->putTrivialFactorlist(IntPtr(factor_list), IntPtr(obj));
}