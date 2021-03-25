using System;

namespace KeySearcher.P2P.Exceptions
{
    /// <summary>
    /// Represents a reservation error that can occur, 
    /// if the instance of this CrypTool reserved a leaf, but 
    /// needs more time than the timeout to calculate the result 
    /// of the leaf. Another node claims the reserved leaf, finishes it 
    /// and removes it from the distributted storage before this 
    /// instance is finished.
    /// </summary>
    public class ReservationRemovedException : Exception
    {
        public ReservationRemovedException(string msg) : base(msg)
        {
        }
    }
}