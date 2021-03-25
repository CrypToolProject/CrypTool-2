#pragma once

#include <CL/cl.hpp>
#include <sys/time.h>

#include "Job.h"

class CrypTool
{
    private:
	std::vector<cl::Platform> platforms;
	std::vector<cl::Device> devices;
	cl::Context* context;
	cl::Kernel* kernel;
	JobResult res;
	cl::Buffer costs;
	float* localCosts;
    bool compareLargerThan;
    int resultSize;
    int platformChoice;
    int deviceChoice;
    timeval lastSubbatchCompleted;

	static const int subbatch = 256*256*256;

	void buildKernel(const Job& j);
	void enqueueKernel(cl::CommandQueue& queue, int size, cl::Buffer& keybuffer, cl::Buffer& costs, const Job& j);
	void enqueueSubbatch(cl::CommandQueue& queue, cl::Buffer& keybuffer, cl::Buffer& costs, int add, int length, const Job& j);
	void pushInTop(std::list<std::pair<float, int> >& top, std::list<std::pair<float, int> >::iterator it, float val, int k);
	std::list<std::pair<float, int> >::iterator isInTop(std::list<std::pair<float, int> >& top, float val, bool LargerThen);
	void initTop(std::list<std::pair<float, int> >& top, bool LargerThen);
    public:
	CrypTool();
	JobResult doOpenCLJob(const Job& j);
    std::string getDeviceName();
};
