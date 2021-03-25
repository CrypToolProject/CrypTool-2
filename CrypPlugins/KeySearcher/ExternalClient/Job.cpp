#include "Job.h"

Job::Job():
    KeySize(0), Key(NULL), LargerThen(false), Size(0),
    ResultSize(0)
{
}

Job::~Job()
{
    delete[] Key;
}

