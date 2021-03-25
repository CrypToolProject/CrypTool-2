#!/bin/bash
export LD_LIBRARY_PATH=~/src/ati-stream-sdk-v2.2-lnx32/lib/x86/:.
# some vendors install an incompatbile own libGL
export LD_PRELOAD=/usr/lib/libGL.so
make && nice ./bin/CrypTool 192.168.178.27 6666 wasdwasd
