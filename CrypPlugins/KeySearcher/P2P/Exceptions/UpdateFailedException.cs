using System;

namespace KeySearcher.P2P.Exceptions
{
    /// <summary>
    /// Represents errors, that occur when this instance is 
    /// unable to update the stored data of a key.
    /// </summary>
    internal class UpdateFailedException : Exception
    {
        public UpdateFailedException(string msg) : base(msg)
        {
        }
    }
}