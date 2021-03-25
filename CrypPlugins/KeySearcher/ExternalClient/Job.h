#pragma once
#include <list>
#include <string>

class Job
{
    public:
        Job();
        ~Job();
        std::string Guid;
        std::string Src;
        int KeySize;
        char *Key;
        bool LargerThen;
        int Size;
        int ResultSize;
};

class JobResult
{
    public:
        std::string Guid;
        std::list<std::pair<float, int> > ResultList;
};
