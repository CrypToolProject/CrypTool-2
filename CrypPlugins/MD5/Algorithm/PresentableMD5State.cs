/* 
   Copyright 2009 Holger Pretzsch, University of Duisburg-Essen

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
using System.Reflection;

namespace CrypTool.MD5.Algorithm
{
    /// <summary>
    /// The state of a <see cref="PresentableMD5"/> algorithm
    /// </summary>
    /// <remarks>
    /// This class contains a simple enumeration describing the current state. It also contains any computed results which are needed to perform the next step of the algorithm.
    /// </remarks>
    /// <seealso cref="PresentableMD5"/>
    public class PresentableMD5State
    {
        /// <summary>
        /// Description of the current state
        /// </summary>
        /// <seealso cref="MD5StateDescription"/>
        public MD5StateDescription State { get; set; }

        /// <summary>
        /// The size of one MD5 data block
        /// </summary>
        public const int DATA_BLOCK_SIZE = 64;

        /// <summary>
        /// Temporary variable A, used inside the compression function
        /// </summary>
        public uint A { get; set; }

        /// <summary>
        /// Temporary variable B, used inside the compression function
        /// </summary>
        public uint B { get; set; }

        /// <summary>
        /// Temporary variable C, used inside the compression function
        /// </summary>
        public uint C { get; set; }

        /// <summary>
        /// Temporary variable D, used inside the compression function
        /// </summary>
        public uint D { get; set; }

        /// <summary>
        /// Determines in which round of the compression function the algorithm currently is, value range 0-3
        /// </summary>
        public uint RoundIndex;

        /// <summary>
        /// Determines in which round of the compression function the algorithm currently is, value range 1-4
        /// </summary>
        public uint Round => RoundIndex + 1;

        /// <summary>
        /// Returns the function name of the inner round function currently used
        /// </summary>
        public string RoundFunctionName
        {
            get
            {
                switch (Round)
                {
                    case 1:
                        return "F";
                    case 2:
                        return "G";
                    case 3:
                        return "H";
                    case 4:
                        return "I";
                    default:
                        return "";
                }
            }
        }

        /// <summary>
        /// Determines in which step of the current round of the compression function the algorithm currently is, value range 0-15
        /// </summary>
        public uint RoundStepIndex;

        /// <summary>
        /// Determines in which step of the current round of the compression function the algorithm currently is, value range 1-16
        /// </summary>
        public uint RoundStep => RoundStepIndex + 1;

        /// <summary>
        /// Absolute index of the current step, value range 0 - 63
        /// </summary>
        public uint AbsoluteStepIndex => RoundIndex * 16 + RoundStepIndex;

        /// <summary>
        /// Shift constant used for current step
        /// </summary>
        public uint ShiftConstant => PresentableMD5.ShiftConstantTable[AbsoluteStepIndex];

        /// <summary>
        /// Addition constant used for current step
        /// </summary>
        public uint AdditionConstant => PresentableMD5.AdditionConstantTable[AbsoluteStepIndex];

        /// <summary>
        /// The index of the message part used for current step
        /// </summary>
        public int MessagePartIndex => PresentableMD5.GetWordIndex(RoundIndex, AbsoluteStepIndex);

        /// <summary>
        /// The total number of bytes hashed so far
        /// </summary>
        /// <remarks>
        /// Updates after the compression function has run.
        /// </remarks>
        public uint BytesHashed { get; set; }

        /// <summary>
        /// Have all padding steps been completed?
        /// </summary>
        public bool IsPaddingDone { get; set; }

        /// <summary>
        /// Is this step the last step in the round?
        /// </summary>
        public bool IsLastStepInRound => RoundStep == 16;

        /// <summary>
        /// Is this the last round for this call of the compression function?
        /// </summary>
        public bool IsLastRound => Round == 4;

        /// <summary>
        /// Accumulated hash value H1
        /// </summary>
        public uint H1 { get; set; }

        /// <summary>
        /// Accumulated hash value H2
        /// </summary>
        public uint H2 { get; set; }

        /// <summary>
        /// Accumulated hash value H3
        /// </summary>
        public uint H3 { get; set; }

        /// <summary>
        /// Accumulated hash value H4
        /// </summary>
        public uint H4 { get; set; }

        /// <summary>
        /// The data used in the current run of the compression function, read into unsigned integer variables
        /// </summary>
        public uint[] DataAsIntegers { get; set; }

        /// <summary>
        /// The data in the buffer
        /// </summary>
        /// <remarks>
        /// Normally, this holds exactly 64 bytes of data. It may contain less immediately after reading the last data block, which is usually less than 64 bytes long. It may contain more (exactly 128 bytes in that case) when padding requires a new block to be appended to the buffer.
        /// </remarks>
        public byte[] Data { get; set; }

        /// <summary>
        /// Returns the 64 byte block for the current iteration (downsizes the 128 byte Data array to 64 bytes)
        /// </summary>
        public byte[] CurrentDataBlock
        {
            get
            {
                uint currentBlockLength = Math.Min(DataLength, 64);
                byte[] result = new byte[currentBlockLength];
                Array.Copy(Data, DataOffset, result, 0, currentBlockLength);
                return result;
            }
        }

        /// <summary>
        /// Determines the length of data in the buffer
        /// </summary>
        public uint DataLength { get; set; }

        /// <summary>
        /// Determines the offset to be used when running the compression function
        /// </summary>
        /// <remarks>
        /// This is always 0 except for the last step, where it may be 64 to process an extra block appended through padding.
        /// </remarks>
        public uint DataOffset { get; set; }

        /// <summary>
        /// Holds the length of data in bit as determined at the start of the padding process
        /// </summary>
        public ulong LengthInBit { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public PresentableMD5State()
        {
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="other">State to copy</param>
        public PresentableMD5State(PresentableMD5State other)
        {
            foreach (FieldInfo fi in GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                fi.SetValue(this, fi.GetValue(other));
            }

            foreach (PropertyInfo pi in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (pi.CanWrite && pi.CanRead)
                {
                    pi.SetValue(this, pi.GetValue(other, null), null);
                }
            }
        }
    }
}
