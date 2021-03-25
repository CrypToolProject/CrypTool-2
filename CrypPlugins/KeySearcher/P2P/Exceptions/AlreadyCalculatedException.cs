using System;

namespace KeySearcher.P2P.Exceptions
{
    /// <summary>
    /// Represents errors that occur, when node states of the KeyPoolTree 
    /// change during the process of finding the next leaf. 
    /// If that happens, this exception is thrown in order to 
    /// reinitialize the tree.
    /// </summary>
    public class AlreadyCalculatedException : Exception
    {
    }
}