#include <cstdio>
#include <cstdlib>
#include <iostream>
#include <cfloat>
#include <SDKFile.hpp>
#include <SDKCommon.hpp>
#include <SDKApplication.hpp>

#ifdef _OPENMP
#include <omp.h>
#else
#warning No OpenMP support. Expect performance impacts.
#endif

#define __NO_STD_STRING

#include "CrypTool.h"

unsigned long DiffMicSec(timeval & start, timeval & end)
{
    return (end.tv_sec - start.tv_sec)*1000000 + (end.tv_usec - start.tv_usec);
}

CrypTool::CrypTool()
{
    cl_int err;

    kernel = 0;
    platformChoice = -1;
    deviceChoice = -1;

    // Get platform information
    std::cout<<"Getting platform"<<std::endl;
    err = cl::Platform::get(&platforms);

    if(err != CL_SUCCESS)
    {
        std::cerr << "Platform::get() failed (" << err << ")" << std::endl;
        throw std::exception();
    }
    if (platforms.size() <= 0)
    {
        std::cerr << "No platforms available!" << std::endl;
        throw std::exception();
    }

    //Iterate over all platforms to generate output for platform choosing by user
    std::vector<cl::Platform>::iterator i;
	std::cout << "Number of platforms: " << platforms.size() << std::endl << std::endl;
	for(i = platforms.begin(); i != platforms.end(); ++i)
	{
		std::cout << "Platform Profile: " << (*i).getInfo<CL_PLATFORM_PROFILE>(&err).c_str() << std::endl;
		std::cout << "Platform Version: " << (*i).getInfo<CL_PLATFORM_VERSION>(&err).c_str() << std::endl;
		std::cout << "Platform Name: " << (*i).getInfo<CL_PLATFORM_NAME>(&err).c_str() << std::endl;
		std::cout << "Platform Vendor: " << (*i).getInfo<CL_PLATFORM_VENDOR>(&err).c_str() << std::endl;
		std::cout << std::endl;

	    if(err != CL_SUCCESS)
	    {
	        std::cerr << "Platform::getInfo() failed (" << err << ")" << std::endl;
	        throw std::exception();
	    }

	    //Get all devices for the current platform to show device information for each device
		(*i).getDevices((cl_device_type)CL_DEVICE_TYPE_ALL, &devices);

		std::cout << "There are " << devices.size() << " devices for this platform." <<std::endl;

		for (std::vector<cl::Device>::iterator z = devices.begin(); z != devices.end(); ++z) {
			cl_device_type dtype = (*z).getInfo<CL_DEVICE_TYPE>();
			std::cout << "  Device Type: ";
			switch (dtype) {
				case CL_DEVICE_TYPE_ACCELERATOR:
					std::cout << "CL_DEVICE_TYPE_ACCELERATOR" << std::endl;
					break;
				case CL_DEVICE_TYPE_CPU:
					std::cout << "CL_DEVICE_TYPE_CPU" << std::endl;
					break;
				case CL_DEVICE_TYPE_DEFAULT:
					std::cout << "CL_DEVICE_TYPE_DEFAULT" << std::endl;
					break;
				case CL_DEVICE_TYPE_GPU:
					std::cout << "CL_DEVICE_TYPE_GPU" << std::endl;
					break;
				}

            std::cout << "  Platform ID: " << (*z).getInfo<CL_DEVICE_PLATFORM>() << std::endl;
            std::cout << "  Name: " << (*z).getInfo<CL_DEVICE_NAME>().c_str() << std::endl;
            std::cout << "  Vendor: " << (*z).getInfo<CL_DEVICE_VENDOR>().c_str() << std::endl;
            std::cout << "  Driver version: " << (*z).getInfo<CL_DRIVER_VERSION>().c_str() << std::endl;
            std::cout << "  Is device available?  " << ((*z).getInfo<CL_DEVICE_AVAILABLE>() ? "Yes" : "No") << std::endl;
            std::cout << "  Device ID: " << (*z).getInfo<CL_DEVICE_VENDOR_ID>() << std::endl;
            std::cout << "  Max clock frequency: " << (*z).getInfo<CL_DEVICE_MAX_CLOCK_FREQUENCY>() << "Mhz" << std::endl;
            std::cout << "  Local memory size: " << (*z).getInfo<CL_DEVICE_LOCAL_MEM_SIZE>() << std::endl;
            std::cout << "  Global memory size: " << (*z).getInfo<CL_DEVICE_GLOBAL_MEM_SIZE>() << std::endl;
            std::cout << "  Max memory allocation: " << (*z).getInfo<CL_DEVICE_MAX_MEM_ALLOC_SIZE>() << std::endl;
            std::cout << "  Cache size: " << (*z).getInfo<CL_DEVICE_GLOBAL_MEM_CACHE_SIZE>() << std::endl;
            std::cout << "  Extensions: " << (*z).getInfo<CL_DEVICE_EXTENSIONS>().c_str() << std::endl;
            std::cout << "  Execution capabilities: " << std::endl;
            std::cout << "    Execute OpenCL kernels: " << ((*z).getInfo<CL_DEVICE_EXECUTION_CAPABILITIES>() & CL_EXEC_KERNEL ? "Yes" : "No") << std::endl;
            std::cout << "    Execute native function: " << ((*z).getInfo<CL_DEVICE_EXECUTION_CAPABILITIES>() & CL_EXEC_NATIVE_KERNEL ? "Yes" : "No") << std::endl;

			std::cout << std::endl;
		}
	}

	if(platforms.size() > 1) {
		while(platformChoice<0 || platformChoice>=(int)platforms.size()) {
			std::cout << "Choose your platform!" << std::endl;
			std::cin >> platformChoice;
		}
	} else {
		std::cout << "Choosing only available platform." << std::endl;
		platformChoice=0;
	}

	if(devices.size() > 1) {
		while(deviceChoice<0 || deviceChoice>=(int)devices.size()) {
			std::cout << "Choose the device to calculate on!" << std::endl;
			std::cin >> deviceChoice;
		}
	} else {
		std::cout << "Choosing only available device." << std::endl;
		deviceChoice=0;
	}


    /* 
     * If we could find our platform, use it. Otherwise pass a NULL and get whatever the
     * implementation thinks we should be using.
     */

    cl_context_properties cps[3] = { CL_CONTEXT_PLATFORM, (cl_context_properties)platforms.at(platformChoice)(), 0 };

    std::cout<<"Creating CL context" << std::endl;
    context = new cl::Context(CL_DEVICE_TYPE_ALL, cps, NULL, NULL, &err);

    if (err != CL_SUCCESS) {
        std::cerr << "Context::Context() failed (" << err << ")\n";
        throw std::exception();
    }
    
    // results
    costs = cl::Buffer(*context, CL_MEM_WRITE_ONLY, sizeof(float)*subbatch, NULL, &err);

    if(err != CL_SUCCESS)
    {
        std::cerr << "Failed allocate to costsbuffer(" << err << ")\n";
        throw std::exception();
    }
    
    localCosts = new float[subbatch];

    gettimeofday(&lastSubbatchCompleted, NULL);

    // required for thousand/million separator in printf
    setlocale(LC_ALL,"");
}

void CrypTool::buildKernel(const Job& j)
{
    if (j.Src == "")
    {
        if (kernel != 0)
            return;
        else
        {
            std::cout << "Source transmission failure!" << std::endl;
            throw std::exception();
        }
    }

    cl_int err;

    std::cout<<"Compiling CL source\n";
    cl::Program::Sources sources(1, std::make_pair(j.Src.c_str(), j.Src.length()));

    cl::Program program = cl::Program(*context, sources, &err);
    if (err != CL_SUCCESS) {
        std::cerr << "Program::Program() failed (" << err << ")\n";
        throw std::exception();
    }

    // nvidia GPUs deliver incorrect results without disabled optimizations
    err = program.build(devices, "-cl-opt-disable");
    if (err != CL_SUCCESS) {

		if(err == CL_BUILD_PROGRAM_FAILURE)
			{
				cl::string str = program.getBuildInfo<CL_PROGRAM_BUILD_LOG>(devices[0]);

				std::cout << " \n\t\t\tBUILD LOG\n";
				std::cout << " ************************************************\n";
				std::cout << str.c_str() << std::endl;
				std::cout << " ************************************************\n";
			}

		//If there is an -33 error thrown here, you are most commonly trying to run an unsupported OpenCL version
        std::cerr << "Program::build() failed (" << err << ")\n";
        throw std::exception();
    }

	delete kernel;

    kernel = new cl::Kernel(program, "bruteforceKernel", &err);
    if (err != CL_SUCCESS) {
        std::cerr << "Kernel::Kernel() failed (" << err << ")\n";
        throw std::exception();
    }
}

JobResult CrypTool::doOpenCLJob(const Job& j)
{
    res.Guid = j.Guid;
    cl_int err;

    buildKernel(j);

    std::cout << "Using device named: " << getDeviceName() << " to calculate." << std::endl;
    //Use the chosen device to calculate on!
    cl::CommandQueue queue(*context, devices[deviceChoice], 0, &err);
    if (err != CL_SUCCESS) {
        std::cerr << "CommandQueue::CommandQueue() failed (" << err << ")\n";
        throw std::exception();
    }

    // key
    cl::Buffer keybuffer = cl::Buffer(*context, CL_MEM_READ_ONLY, j.KeySize*sizeof(float), NULL, &err);
    if(err != CL_SUCCESS)
    {
        std::cerr << "Failed to allocate keybuffer(" << err << ")\n";
        throw std::exception();
    }

    err = queue.enqueueWriteBuffer(keybuffer, 1, 0, j.KeySize*sizeof(float), j.Key);
    if(err != CL_SUCCESS)
    {
        std::cerr << "Failed write to keybuffer(" << err << ")\n";
        throw std::exception();
    }

    this->compareLargerThan = j.LargerThen;
    this->resultSize = j.ResultSize;
    res.ResultList.resize(j.ResultSize);
    initTop(res.ResultList, j.LargerThen);

    //execute:
    std::cout<<"Running CL program with " << j.Size << " calculations!\n";
    enqueueKernel(queue, j.Size, keybuffer, costs, j);

    std::cout<<"Done!\n";

    return res;
}

void CrypTool::enqueueSubbatch(cl::CommandQueue& queue, cl::Buffer& keybuffer, cl::Buffer& costs, int add, int length, const Job& j)
{
    timeval openCLStart;
    gettimeofday(&openCLStart, NULL);
	cl_int err;

	err = kernel->setArg(0, keybuffer);
	if (err != CL_SUCCESS) {
		std::cerr << "Kernel::setArg() failed (" << err << ")\n";
		throw std::exception();
	}

	err = kernel->setArg(1, costs);
	if (err != CL_SUCCESS) {
		std::cerr << "Kernel::setArg() failed (" << err << ")\n";
		throw std::exception();
	}

	err = kernel->setArg(2, add);
	if (err != CL_SUCCESS) {
		std::cerr << "Kernel::setArg() failed (" << err << ")\n";
		throw std::exception();
	}

	err = queue.enqueueNDRangeKernel(*kernel, cl::NullRange, cl::NDRange(256, 256, 256), cl::NullRange);

	if (err != CL_SUCCESS) {
		std::cerr << "CommandQueue::enqueueNDRangeKernel()" \
		    " failed (" << err << ")\n";
		throw std::exception();
	}

	err = queue.finish();
	if (err != CL_SUCCESS) {
		std::cerr << "Event::wait() failed (" << err << ")\n";
		throw std::exception();
	}

	queue.enqueueReadBuffer(costs, 1, 0, sizeof(float)*length, localCosts);
	err = queue.finish();
	if (err != CL_SUCCESS) {
		std::cerr << "Event::wait() failed (" << err << ")\n";
		throw std::exception();
	}

    timeval openCLEnd;
    gettimeofday(&openCLEnd, NULL);
#ifdef _OPENMP
#pragma omp parallel
    {
        std::list<std::pair<float, int> > localtop;
	localtop.resize(j.ResultSize);
	initTop(localtop, j.LargerThen);
        int eachChunk = length/omp_get_num_threads();
        int from = omp_get_thread_num()*eachChunk;
        int to = from + eachChunk;
        if(omp_get_thread_num() == omp_get_num_threads()-1)
        {
            to = length;
        }
        for(int i=from; i<to; ++i)
        {
            std::list<std::pair<float, int> >::iterator it = isInTop(localtop, localCosts[i], j.LargerThen);
            if (it != localtop.end() || it == localtop.begin())
                pushInTop(localtop, it, localCosts[i], i+add);
        }
        // merge it
#pragma omp critical
        {
            std::list<std::pair<float, int> >::iterator itr;
            for(itr = localtop.begin(); itr != localtop.end(); ++itr)
            {
                std::list<std::pair<float, int> >::iterator posInGlobalList = isInTop(res.ResultList, itr->first, j.LargerThen);
                if (posInGlobalList != res.ResultList.end())
                    pushInTop(res.ResultList, posInGlobalList, itr->first, itr->second);
            }
        }
    }
#else
	//check results:
	for(int i=0; i<length; ++i)
	{
		//std::cout << localCosts[i] << std::endl;
		std::list<std::pair<float, int> >::iterator it = isInTop(res.ResultList, localCosts[i], j.LargerThen);
		if (it != res.ResultList.end())
			pushInTop(res.ResultList, it, localCosts[i], i+add);
	}
#endif

    timeval finishedSubbatch;
    gettimeofday(&finishedSubbatch, NULL);

    unsigned long totalMic= DiffMicSec(openCLStart, finishedSubbatch);

    printf("Completed a subbatch in %.3f seconds. %.2f%% spent on OpenCL, %.2f%% on sorting.\n",
            (float)totalMic/1000000, DiffMicSec(openCLStart, openCLEnd)/(float)totalMic*100, DiffMicSec(openCLEnd, finishedSubbatch)/(float)totalMic*100);

}

void CrypTool::enqueueKernel(cl::CommandQueue& queue, int size, cl::Buffer& keybuffer, cl::Buffer& costs, const Job& j)
{
    for (int i = 0; i < (size/subbatch); i++)
    {
        enqueueSubbatch(queue, keybuffer, costs, i*subbatch, subbatch, j);

        timeval now;
        gettimeofday(&now, NULL);
        unsigned long timeDiffMicroSec = (now.tv_sec - lastSubbatchCompleted.tv_sec)*1000000 + (now.tv_usec - lastSubbatchCompleted.tv_usec);
        lastSubbatchCompleted = now;
        float keysPerSecond = subbatch/((float)timeDiffMicroSec/1000000);
        printf("% .2f%% done. %'u keys/sec %u seconds remaining\n", ((i+1)*subbatch)/(float)size*100, (unsigned int)keysPerSecond,
                (unsigned int)((float)(size-(subbatch*(i+1)))/keysPerSecond));
    }

    int remain = (size%subbatch);
    if (remain != 0)
    {
        enqueueSubbatch(queue, keybuffer, costs, size-remain, remain, j);
    }
}

void CrypTool::pushInTop(std::list<std::pair<float, int> >& top, std::list<std::pair<float, int> >::iterator it, float val, int k) {
	top.insert(it, std::pair<float, int>(val, k));
    if(top.size() > this->resultSize)
        top.pop_back();
}

std::list<std::pair<float, int> >::iterator CrypTool::isInTop(std::list<std::pair<float, int> >& top, float val, bool LargerThen) {
    if (top.size() == 0)
        return top.begin();

	if (LargerThen)
	{
		if(top.size() > 0 && val <= top.rbegin()->first)
			return top.end();
		for (std::list<std::pair<float, int> >::iterator k = top.begin(); k != top.end(); k++)
			if (val > k->first)
				return k;
	}
	else
	{
		if(top.size() > 0 && val >= top.rbegin()->first)
			return top.end();
		for (std::list<std::pair<float, int> >::iterator k = top.begin(); k != top.end(); k++)
			if (val < k->first)
				return k;
	}

	return top.end();
}

void CrypTool::initTop(std::list<std::pair<float, int> >& top, bool LargerThen) {
	for (std::list<std::pair<float, int> >::iterator k = top.begin(); k != top.end(); k++)
        {
            if (LargerThen)
		k->first = -FLT_MAX;
            else
                k->first = FLT_MAX;
        }
}

std::string CrypTool::getDeviceName()
{
    return std::string(devices[deviceChoice].getInfo<CL_DEVICE_NAME>().c_str());
}
