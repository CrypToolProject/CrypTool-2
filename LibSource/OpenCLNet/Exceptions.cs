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
    public class OpenCLException : Exception
    {
        public ErrorCode ErrorCode = ErrorCode.SUCCESS;

        public OpenCLException()
        {
        }

        public OpenCLException(ErrorCode errorCode)
        {
            ErrorCode = errorCode;
        }

        public OpenCLException(string errorMessage)
            : base(errorMessage)
        {
        }

        public OpenCLException(string errorMessage, ErrorCode errorCode)
            : base(errorMessage)
        {
            ErrorCode = errorCode;
        }

        public OpenCLException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public OpenCLException(string message, ErrorCode errorCode, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }

    public class OpenCLBuildException : OpenCLException
    {
        public List<string> BuildLogs = new List<string>();

        public OpenCLBuildException(Program program, ErrorCode result)
            : base("Build failed with error code " + result, result)
        {
            foreach (Device d in program.Devices)
            {
                BuildLogs.Add(program.GetBuildLog(d));
            }
        }
    }

    public class OpenCLNotAvailableException : OpenCLException
    {
        public OpenCLNotAvailableException()
            : base("OpenCL not available")
        {
        }
    }
}
