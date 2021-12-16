/*
 * Copyright (c) 2009 Olav Kalgraf(olav.kalgraf@gmail.com)
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;

namespace OpenCLNet
{
    [Obsolete("SimpleOCLHelper is superseded by OpenCLManager")]
    public class SimpleOCLHelper
    {
        public Platform Platform;
        /// <summary>
        /// The devices bound to the Helper
        /// </summary>
        public Device[] Devices;
        /// <summary>
        /// The Context associated with this Helper
        /// </summary>
        public Context Context;
        /// <summary>
        /// CommandQueue for device with same index
        /// </summary>
        public CommandQueue[] CQs;
        /// <summary>
        /// Alias for CQs[0]
        /// </summary>
        public CommandQueue CQ;
        public Program Program;
        protected Dictionary<string, Kernel> Kernels;

        public SimpleOCLHelper(Platform platform, DeviceType deviceType, string source)
        {
            Platform = platform;
            Initialize(deviceType, source);
        }

        protected virtual void Initialize(DeviceType deviceType, string source)
        {
            Devices = Platform.QueryDevices(deviceType);
            if (Devices.Length == 0)
            {
                throw new OpenCLException("No devices of type " + deviceType + " present");
            }

            Context = Platform.CreateContext(null, Devices, null, IntPtr.Zero);
            CQs = new CommandQueue[Devices.Length];
            for (int i = 0; i < CQs.Length; i++)
            {
                CQs[i] = Context.CreateCommandQueue(Devices[i], CommandQueueProperties.PROFILING_ENABLE);
            }

            CQ = CQs[0];
            Program = Context.CreateProgramWithSource(source);
            Program.Build();
            Kernels = Program.CreateKernelDictionary();
        }

        public Kernel GetKernel(string kernelName)
        {
            return Kernels[kernelName];
        }
    }
}