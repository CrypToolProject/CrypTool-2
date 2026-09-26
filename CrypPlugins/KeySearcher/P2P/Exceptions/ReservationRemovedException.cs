/*
   Copyright (C) CrypTool 2 Team

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
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