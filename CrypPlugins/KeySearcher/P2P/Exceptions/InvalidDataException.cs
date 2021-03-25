using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeySearcher.P2P.Exceptions
{
    public class InvalidDataException : Exception
    {
        public InvalidDataException(string message) : base(message)
        {
            
        }
    }
}
