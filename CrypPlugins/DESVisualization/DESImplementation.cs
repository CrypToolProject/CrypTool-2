using System;
using System.Text;

namespace CrypTool.DESVisualization
{

    /// <summary>
    /// Declaration of the DESImplementation class which executes the Data Encryption Standard
    /// algorithm for encryption and saves all the binary strings needed for DESPresentation.xaml.
    /// </summary>
    public class DESImplementation
    {

        // Constructor
        public DESImplementation(byte[] keyInput, byte[] messageInput)
        {
            inputMessage = messageInput;
            inputKey = keyInput;
        }

        #region Attributes

        public byte[] inputMessage;
        public byte[] inputKey;
        public byte[] outputCiphertext;

        // Row, Column and Output of each S-Box for all 16 rounds saved as bytes
        public byte[,] sBoxNumberDetails = new byte[16, 24];

        // Binary Strings for DESPresentation

        // Cn, Dn for all 16 rounds n (C0D0 = Result of PC1)
        public string[,] keySchedule = new string[17, 2];
        // Kn for all 16 rounds n (Result of PC2)
        public string[] roundKeys = new string[16];

        // Ln, Rn for all 16 rounds n (L0R0 =  Result of IP)
        public string[,] lrData = new string[17, 2];
        // E(Rn-1), KeyAddition, SBoxOutput, FunctionOutput for all 16 rounds n
        public string[,] roundDetails = new string[16, 4];
        // Input, Row, Column and Output of each S-Box for all 16 rounds saved as binary string
        public string[,] sBoxStringDetails = new string[16, 32];

        // Result of FP(R16L16)
        public string ciphertext;
        public string message;
        public string key;

        // Constants

        public const int KeyByteLength = 8;
        public const int BitsPerByte = 8;

        #endregion Attributes

        #region Nested classes

        /// <summary>
        /// Declaration of ByteBlock class which represents a 64 bit block.
        /// </summary>
        internal class ByteBlock
        {

            // Constants
            public const int ByteLength = 8;

            // Attributes
            internal byte[] data = new byte[ByteLength];

            // Operations

            /// <summary>
            /// Reset Data bytes
            /// </summary>
            public void Reset()
            {
                Array.Clear(data, 0, ByteLength);
            }

            /// <summary>
            /// Copy source Data to this Data
            /// </summary>
            public void Set(ByteBlock source)
            {
                Set(source.data, 0);
            }

            /// <summary>
            /// Copy bytes from buffer starting at offset into Data
            /// </summary>
            public void Set(byte[] buffer, int offset)
            {
                Array.Copy(buffer, offset, data, 0, ByteLength);
            }

            /// <summary>
            /// Set result of a xor b to Data
            /// </summary>
            public void Xor(ByteBlock a, ByteBlock b)
            {
                for (int offset = 0; offset < ByteLength; offset++)
                {
                    data[offset] = Convert.ToByte(a.data[offset] ^ b.data[offset]);
                }
            }

            /// <summary>
            /// Set one bit in Data to true (1) or false (0)
            /// </summary>
            public void SetBit(int byteOffset, int bitOffset, bool flag)
            {
                byte mask = Convert.ToByte(1 << bitOffset);
                if (((data[byteOffset] & mask) == mask) != flag)
                {
                    data[byteOffset] ^= mask;
                }
            }

            /// <summary>
            /// Return one bit in Data, true (1) or false (0)
            /// </summary>
            public bool GetBit(int byteOffset, int bitOffset)
            {
                return ((data[byteOffset] >> bitOffset) & 0x01) == 0x01;
            }

            /// <summary>
            ///  Cyclic left shift by 1 or 2 is only applied to the first 32 bits and parity bit is ignored
            /// </summary>
            public void ShiftLeftWrapped(ByteBlock s, int bitShift)
            {
                int byteOffset;
                bool bit;

                // Copy byte and shift regardless
                for (byteOffset = 0; byteOffset < 4; byteOffset++)
                {
                    data[byteOffset] = Convert.ToByte((s.data[byteOffset] << bitShift) & 0xFF);
                }

                if (bitShift == 1)
                {

                    // repair bits on right of BYTE
                    for (byteOffset = 0; byteOffset < 3; byteOffset++)
                    {

                        bit = s.GetBit(byteOffset + 1, 7);
                        SetBit(byteOffset, 1, bit);

                    }

                    // wrap around the final bit
                    SetBit(3, 1, s.GetBit(0, 7));

                }
                else if (bitShift == 2)
                {

                    // repair bits on right of BYTE
                    for (byteOffset = 0; byteOffset < 3; byteOffset++)
                    {

                        bit = s.GetBit(byteOffset + 1, 7);
                        SetBit(byteOffset, 2, bit);
                        bit = s.GetBit(byteOffset + 1, 6);
                        SetBit(byteOffset, 1, bit);

                    }

                    // wrap around the final bit
                    SetBit(3, 2, s.GetBit(0, 7));
                    SetBit(3, 1, s.GetBit(0, 6));

                }

            }

            /// <summary>
            /// This method creates a modified binary string equivalent to the data byte array.
            /// </summary>
            /// <param name="length">Sets the length of the returned string.</param>
            /// <param name="lastPos">The bit at this position and all bits after that position in each byte will be skipped. They are not included in the returned binary string. It starts by 1 and ends with 8. If no bit should be skipped this parameter has to be 0.</param>
            /// <returns>Binary string with specified length</returns>
            public string ToBinaryString(int length, int lastPos)
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < ByteLength; i++)
                {
                    for (int j = 0; j < ByteLength; j++)
                    {
                        if (lastPos == 0 || lastPos - 1 - j > 0)
                        {
                            if (GetBit(i, 7 - j))
                            {
                                builder.Append('1');
                            }
                            else
                            {
                                builder.Append('0');
                            }
                        }
                    }
                }
                string tmp = builder.ToString();
                tmp = tmp.Remove(length, tmp.Length - length);
                return tmp;
            }
        }

        /// <summary>
        /// Declaration of KeySet class which contains all RoundKeys.
        /// </summary>
        internal class KeySet
        {

            // Constants
            public const int KeyCount = 17;

            // Attributes
            internal ByteBlock[] keys;

            // Constructor
            internal KeySet()
            {

                // Create array
                keys = new ByteBlock[KeyCount];
                for (int i = 0; i < KeyCount; i++)
                {
                    keys[i] = new ByteBlock();
                }

            }

            // Operation

            public ByteBlock GetAt(int arrayOffset)
            {
                return keys[arrayOffset];
            }

        }

        /// <summary>
        /// Declaration of WorkingSet class which contains all data blocks calculated in the 16 DES-Rounds.
        /// </summary>
        internal class WorkingSet
        {

            // Attributes

            internal ByteBlock ip = new ByteBlock();
            internal ByteBlock[] ln = new ByteBlock[17];
            internal ByteBlock[] rn = new ByteBlock[17];
            internal ByteBlock rnExpand = new ByteBlock();
            internal ByteBlock xorBlock = new ByteBlock();
            internal ByteBlock sBoxValues = new ByteBlock();
            internal ByteBlock f = new ByteBlock();
            internal ByteBlock x = new ByteBlock();

            internal ByteBlock inputBlock = new ByteBlock();
            internal ByteBlock outputBlock = new ByteBlock();

            // Constructor
            internal WorkingSet()
            {

                // Build the arrays
                for (int i1 = 0; i1 < 17; i1++)
                {
                    ln[i1] = new ByteBlock();
                    rn[i1] = new ByteBlock();
                }
            }

        }

        #endregion Nested classes

        #region DES Tables

        // Permuted Choice 1 (PCl)
        private static readonly byte[] bytePC1 = {
            57, 49, 41, 33, 25, 17,  9,
            1,  58, 50, 42, 34, 26, 18,
            10,  2, 59, 51, 43, 35, 27,
            19, 11,  3, 60, 52, 44, 36,
            63, 55, 47, 39, 31, 23, 15,
            7,  62, 54, 46, 38, 30, 22,
            14,  6, 61, 53, 45, 37, 29,
            21, 13,  5, 28, 20, 12,  4,
        };

        // Permuted Choice 2 (PC2)
        private static readonly byte[] bytePC2 = {
            14, 17, 11, 24,  1,  5,
            3,  28, 15,  6, 21, 10,
            23, 19, 12,  4, 26,  8,
            16,  7, 27, 20, 13,  2,
            41, 52, 31, 37, 47, 55,
            30, 40, 51, 45, 33, 48,
            44, 49, 39, 56, 34, 53,
            46, 42, 50, 36, 29, 32,
        };

        // Initial Permutation (IP)
        private static readonly byte[] byteIP =  {
            58, 50, 42, 34, 26, 18, 10,  2,
            60, 52, 44, 36, 28, 20, 12,  4,
            62, 54, 46, 38, 30, 22, 14,  6,
            64, 56, 48, 40, 32, 24, 16,  8,
            57, 49, 41, 33, 25, 17,  9,  1,
            59, 51, 43, 35, 27, 19, 11,  3,
            61, 53, 45, 37, 29, 21, 13,  5,
            63, 55, 47, 39, 31, 23, 15,  7
        };

        // Final Permutation (FP)
        private static readonly byte[] byteFP = {
            40,  8,   48,    16,    56,   24,    64,   32,
            39,  7,   47,    15,    55,   23,    63,   31,
            38,  6,   46,    14,    54,   22,    62,   30,
            37,  5,   45,    13,    53,   21,    61,   29,
            36,  4,   44,    12,    52,   20,    60,   28,
            35,  3,   43,    11,    51,   19,    59,   27,
            34,  2,   42,    10,    50,   18,    58,   26,
            33,  1,   41,     9,    49,   17,    57,   25,
        };

        // Expansion Function (E)
        private static readonly byte[] byteE = {
            32,  1,  2,  3,  4,  5,
            4,   5,  6,  7,  8,  9,
            8,   9, 10, 11, 12, 13,
            12, 13, 14, 15, 16, 17,
            16, 17, 18, 19, 20, 21,
            20, 21, 22, 23, 24, 25,
            24, 25, 26, 27, 28, 29,
            28, 29, 30, 31, 32,  1
        };

        // Permutation Function (P)
        private static readonly byte[] byteP = {
            16,  7, 20, 21,
            29, 12, 28, 17,
            1,  15, 23, 26,
            5,  18, 31, 10,
            2,   8, 24, 14,
            32, 27,  3,  9,
            19, 13, 30,  6,
            22, 11,  4, 25
        };

        // Schedule of cyclic left shifts for C and D blocks
        public static byte[] byteShifts = { 1, 1, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 1 };

        // S-Boxes
        private static readonly byte[,] byteSBox = new byte[,] {
            {14,  4, 13,  1,     2, 15, 11,  8, 3, 10,  6, 12,   5,  9,  0,  7},
            { 0, 15,  7,  4,    14,  2, 13,  1, 10,  6, 12, 11,  9,  5,  3,  8},
            { 4,  1, 14,  8,    13,  6,  2, 11, 15, 12,  9,  7,  3, 10,  5,  0},
            {15, 12,  8,  2, 4,  9,  1,  7,   5, 11,  3, 14,    10,  0,  6, 13},

            {15, 1,  8, 14,  6, 11,  3,  4,  9, 7,   2, 13, 12,  0,  5, 10},
            {3, 13,  4,  7, 15,  2,  8, 14, 12, 0,   1, 10,  6,  9, 11,  5},
            {0, 14,  7, 11, 10,  4, 13,  1, 5, 8,   12,  6,  9,  3,  2, 15},
            {13, 8, 10,  1,  3, 15,  4,  2, 11, 6,   7, 12,  0,  5, 14,  9},

            {10,     0,  9, 14,  6,  3, 15,  5,  1, 13, 12,  7, 11,  4,  2,  8},
            {13,     7,  0,  9,  3,  4,  6, 10,  2,  8,  5, 14, 12, 11, 15,  1},
            {13,     6,  4,  9,  8, 15,  3,  0, 11,  1,  2, 12,  5, 10, 14,  7},
            {1, 10, 13,  0,  6,  9,  8,  7,  4, 15, 14,  3, 11,  5,  2, 12},

            {7, 13, 14,  3,  0,  6,  9, 10,  1,  2,  8,  5, 11, 12,  4, 15},
            {13,     8, 11,  5,  6, 15,  0,  3,  4,  7,  2, 12,  1, 10, 14,  9},
            {10,     6,  9,  0, 12, 11,  7, 13, 15,  1,  3, 14,  5,  2,  8,  4},
            {3, 15,  0,  6, 10,  1, 13,  8, 9,   4,  5, 11, 12,  7,  2, 14},

            {2, 12,  4,  1,  7, 10, 11, 6,  8,   5,  3, 15, 13,  0, 14,  9},
            {14,    11,  2, 12,  4,  7, 13,  1, 5,   0, 15, 10,  3,  9,  8,  6},
            {4,  2,  1, 11, 10, 13,  7,  8,15,   9, 12,  5,  6,  3,  0, 14},
            {11,     8, 12,  7,  1, 14,  2, 13, 6,  15,  0,  9, 10,  4,  5,  3},

            {12,     1, 10, 15,  9,  2,  6,  8,0,   13,  3,  4, 14,  7,  5, 11},
            {10,    15,  4,  2,  7, 12,  9,  5,6,    1, 13, 14,  0, 11,  3,  8},
            {9, 14, 15,  5,  2,  8, 12,  3,7,    0,  4, 10,  1, 13, 11,  6},
            {4,  3,  2, 12,  9,  5, 15, 10,11,  14,  1,  7,  6,  0,  8, 13},

            {4, 11,  2, 14, 15,  0,  8, 13, 3,  12,  9,  7,  5, 10,  6,  1},
            {13,     0, 11,  7,  4,  9,  1, 10,14,   3,  5, 12,  2, 15,  8,  6},
            {1,  4, 11, 13, 12,  3,  7, 14,10,  15,  6,  8,  0,  5,  9,  2},
            {6, 11, 13,  8,  1,  4, 10,  7,9,    5,  0, 15, 14,  2,  3, 12},

            {13,     2,  8,  4,  6, 15, 11,  1,10,   9,  3, 14,  5,  0, 12,  7},
            {1, 15, 13,  8, 10,  3,  7,  4, 12,  5,  6, 11,  0, 14,  9,  2},
            {7, 11,  4,  1,  9, 12, 14,  2,0,    6, 10, 13, 15,  3,  5,  8},
            {2,  1, 14,  7,  4, 10,  8, 13,15,  12,  9,  0,  3,  5,  6, 11}
        };

        #endregion DES Tables

        #region  DES Operations

        /// <summary>
        /// Check the correctness of the DES input. It must contain 8 bytes.
        /// </summary>
        private bool IsValidDESInput(byte[] input)
        {
            if (input == null)
            {
                return false;
            }

            if (input.Length != KeyByteLength)
            {
                return false;
            }

            // Return success
            return true;
        }

        /// <summary>
        /// High level method to start the DES encryption process.
        /// </summary>
        public void DES()
        {

            // Check correctness of Input
            if (!IsValidDESInput(inputKey))
            {
                throw new Exception("ERROR: Invalid DES key.");
            }
            if (!IsValidDESInput(inputMessage))
            {
                throw new Exception("ERROR: Invalid DES message.");
            }

            // Expand the keys into Kn
            KeySet kn = ExpandKey(inputKey);

            // Apply DES keys
            DesAlgorithm(inputMessage, kn);

        }

        #endregion DES-Operations

        #region Low-level DES Operations

        /// <summary>
        /// Expand an 8 byte DES key into a set of permuted round keys.
        /// </summary>
        /// <param name="key">8 byte DES key</param>
        /// <returns>KeySet instance containing the round keys</returns>
        private KeySet ExpandKey(byte[] key)
        {

            // Declare return variable
            KeySet tmp = new KeySet();

            // Declaration of local variables
            int tableOffset, arrayOffset, permOffset, byteOffset, bitOffset;
            bool bit;

            // Put key into an 8-bit block
            ByteBlock k = new ByteBlock();
            k.Set(key, 0);

            //Fill String Attribute
            this.key = k.ToBinaryString(64, 0);

            // Permutate Kp with PC1
            ByteBlock kp = new ByteBlock();
            for (arrayOffset = 0; arrayOffset < bytePC1.Length; arrayOffset++)
            {

                // Get permute offset
                permOffset = bytePC1[arrayOffset];
                permOffset--;

                // Get and set bit
                kp.SetBit(
                    BitAddressToByteOffset(arrayOffset, 7),
                    BitAddressToBitOffset(arrayOffset, 7),
                    k.GetBit(
                        BitAddressToByteOffset(permOffset, 8),
                        BitAddressToBitOffset(permOffset, 8)
                    )
                );

            }


            // Create 17 blocks of C and D from Kp
            ByteBlock[] kpCn = new ByteBlock[17];
            ByteBlock[] kpDn = new ByteBlock[17];
            for (arrayOffset = 0; arrayOffset < 17; arrayOffset++)
            {
                kpCn[arrayOffset] = new ByteBlock();
                kpDn[arrayOffset] = new ByteBlock();
            }
            for (arrayOffset = 0; arrayOffset < 32; arrayOffset++)
            {

                // Set bit in KpCn
                byteOffset = BitAddressToByteOffset(arrayOffset, 8);
                bitOffset = BitAddressToBitOffset(arrayOffset, 8);
                bit = kp.GetBit(byteOffset, bitOffset);
                kpCn[0].SetBit(byteOffset, bitOffset, bit);

                // Set bit in KpDn
                bit = kp.GetBit(byteOffset + 4, bitOffset);
                kpDn[0].SetBit(byteOffset, bitOffset, bit);

            }

            for (arrayOffset = 1; arrayOffset < 17; arrayOffset++)
            {

                // Shift left wrapped
                kpCn[arrayOffset].ShiftLeftWrapped(kpCn[arrayOffset - 1], byteShifts[arrayOffset - 1]);
                kpDn[arrayOffset].ShiftLeftWrapped(kpDn[arrayOffset - 1], byteShifts[arrayOffset - 1]);

            }

            // Fill Cn und Dn  binary strings into KeySchedule
            for (arrayOffset = 0; arrayOffset < 17; arrayOffset++)
            {
                keySchedule[arrayOffset, 0] = kpCn[arrayOffset].ToBinaryString(29, 8).Remove(28, 1);
                keySchedule[arrayOffset, 1] = kpDn[arrayOffset].ToBinaryString(29, 8).Remove(28, 1);
            }


            // Create 17 keys Kn
            for (arrayOffset = 0; arrayOffset < 17; arrayOffset++)
            {

                // Loop through the bits
                for (tableOffset = 0; tableOffset < 48; tableOffset++)
                {

                    // Get address of bit
                    permOffset = bytePC2[tableOffset];
                    permOffset--;

                    // Convert to byte and bit offsets
                    byteOffset = BitAddressToByteOffset(permOffset, 7);
                    bitOffset = BitAddressToBitOffset(permOffset, 7);

                    // Get bit
                    if (byteOffset < 4)
                    {
                        bit = kpCn[arrayOffset].GetBit(byteOffset, bitOffset);
                    }
                    else
                    {
                        bit = kpDn[arrayOffset].GetBit(byteOffset - 4, bitOffset);
                    }

                    // Set bit
                    byteOffset = BitAddressToByteOffset(tableOffset, 6);
                    bitOffset = BitAddressToBitOffset(tableOffset, 6);
                    tmp.GetAt(arrayOffset).SetBit(byteOffset, bitOffset, bit);
                }

            }

            // Fill in binary strings into RoundKeys
            for (arrayOffset = 0; arrayOffset < 16; arrayOffset++)
            {
                roundKeys[arrayOffset] = tmp.GetAt(arrayOffset + 1).ToBinaryString(48, 7);
            }

            // Return filled KeySet variable tmp
            return tmp;

        }

        /// <summary>
        /// Apply the DES encryption algorithm to the message with the calculated RoundKeys.
        /// </summary>
        /// <param name="message">8 byte DES message to encrypt</param>
        /// <param name="keySets">Set of the 16 RoundKeys needed for encryption</param>
        private void DesAlgorithm(byte[] message, KeySet keySet)
        {

            // Declare a WorkingSet
            WorkingSet workingSet = new WorkingSet();
            ByteBlock msg = new ByteBlock();
            msg.Set(message, 0);
            workingSet.inputBlock.Set(msg);

            // Set binary string of message
            this.message = msg.ToBinaryString(64, 0);

            // Apply the algorithm
            LowLevelDesAlgorithm(workingSet, keySet);

        }

        /// <summary>
        /// Apply the low level DES encryption algorithm to a WorkingSet with the calculated RoundKeys.
        /// </summary>
        /// <param name="workingSet">Contains the information needed in the encryption process</param>
        /// <param name="keySet">Set of the 16 RoundKeys needed for encryption</param>
        private void LowLevelDesAlgorithm(WorkingSet workingSet, KeySet keySet)
        {

            // Declaration of local variables
            int tableOffset;
            int arrayOffset;
            int permOffset;
            int byteOffset;
            int bitOffset;

            // Permute with byteIP
            for (tableOffset = 0; tableOffset < byteIP.Length; tableOffset++)
            {

                // Get perm offset
                permOffset = byteIP[tableOffset];
                permOffset--;

                // Get and set bit
                workingSet.ip.SetBit(
                    BitAddressToByteOffset(tableOffset, 8),
                    BitAddressToBitOffset(tableOffset, 8),
                    workingSet.inputBlock.GetBit(
                        BitAddressToByteOffset(permOffset, 8),
                        BitAddressToBitOffset(permOffset, 8)
                    )
                );

            }

            // Create Ln[0] and Rn[0]
            for (arrayOffset = 0; arrayOffset < 32; arrayOffset++)
            {
                byteOffset = BitAddressToByteOffset(arrayOffset, 8);
                bitOffset = BitAddressToBitOffset(arrayOffset, 8);
                workingSet.ln[0].SetBit(byteOffset, bitOffset, workingSet.ip.GetBit(byteOffset, bitOffset));
                workingSet.rn[0].SetBit(byteOffset, bitOffset, workingSet.ip.GetBit(byteOffset + 4, bitOffset));
            }

            // Loop through 17 interations
            for (int blockOffset = 1; blockOffset < 17; blockOffset++)
            {
                // Get the array offset
                int iKeyOffset = blockOffset;

                // Set Ln[N] = Rn[N-1]
                workingSet.ln[blockOffset].Set(workingSet.rn[blockOffset - 1]);

                // Set Rn[N] = Ln[0] + f(R[N-1],K[N])
                for (tableOffset = 0; tableOffset < byteE.Length; tableOffset++)
                {

                    // Get perm offset
                    permOffset = byteE[tableOffset];
                    permOffset--;

                    // Get and set bit
                    workingSet.rnExpand.SetBit(
                        BitAddressToByteOffset(tableOffset, 6),
                        BitAddressToBitOffset(tableOffset, 6),
                        workingSet.rn[blockOffset - 1].GetBit(
                            BitAddressToByteOffset(permOffset, 8),
                            BitAddressToBitOffset(permOffset, 8)
                        )
                    );

                }

                //Fill String Attribute
                roundDetails[blockOffset - 1, 0] = workingSet.rnExpand.ToBinaryString(48, 7);

                // XOR expanded block with K-block
                workingSet.xorBlock.Xor(workingSet.rnExpand, keySet.GetAt(iKeyOffset));

                //Fill String Attribute
                roundDetails[blockOffset - 1, 1] = workingSet.xorBlock.ToBinaryString(48, 7);

                // Set S-Box values
                workingSet.sBoxValues.Reset();
                for (tableOffset = 0; tableOffset < 8; tableOffset++)
                {

                    //Fill String Attribute
                    sBoxStringDetails[blockOffset - 1, tableOffset * 4] = Convert.ToString(workingSet.xorBlock.data[tableOffset], 2).PadLeft(8, '0').Remove(6, 2);

                    // Calculate m and n
                    int m = ((workingSet.xorBlock.GetBit(tableOffset, 7) ? 1 : 0) << 1) | (workingSet.xorBlock.GetBit(tableOffset, 2) ? 1 : 0);
                    int n = (workingSet.xorBlock.data[tableOffset] >> 3) & 0x0F;

                    // Get s-box value
                    permOffset = byteSBox[(tableOffset * 4) + m, n];
                    workingSet.sBoxValues.data[tableOffset] = (byte)(permOffset << 4);

                    //Fill String Attributes
                    sBoxNumberDetails[blockOffset - 1, tableOffset * 3] = (byte)(m);
                    sBoxStringDetails[blockOffset - 1, (tableOffset * 4) + 1] = Convert.ToString(m, 2).PadLeft(2, '0');
                    sBoxNumberDetails[blockOffset - 1, (tableOffset * 3) + 1] = (byte)(n);
                    sBoxStringDetails[blockOffset - 1, (tableOffset * 4) + 2] = Convert.ToString(n, 2).PadLeft(4, '0');
                    sBoxNumberDetails[blockOffset - 1, (tableOffset * 3) + 2] = (byte)(workingSet.sBoxValues.data[tableOffset] >> 4);
                    sBoxStringDetails[blockOffset - 1, (tableOffset * 4) + 3] = Convert.ToString((byte)(workingSet.sBoxValues.data[tableOffset] >> 4), 2).PadLeft(4, '0');

                }

                //Fill String Attributes
                roundDetails[blockOffset - 1, 2] = workingSet.sBoxValues.ToBinaryString(32, 5);

                // Permute with P -> f
                workingSet.f.Reset();
                for (tableOffset = 0; tableOffset < byteP.Length; tableOffset++)
                {

                    // Get perm offset
                    permOffset = byteP[tableOffset];
                    permOffset--;

                    // Get and set bit
                    workingSet.f.SetBit(
                        BitAddressToByteOffset(tableOffset, 4),
                        BitAddressToBitOffset(tableOffset, 4),
                        workingSet.sBoxValues.GetBit(
                            BitAddressToByteOffset(permOffset, 4),
                            BitAddressToBitOffset(permOffset, 4)
                        )
                    );

                }

                //Fill String Attributes
                roundDetails[blockOffset - 1, 3] = workingSet.f.ToBinaryString(32, 5);

                // Rn[N] = Ln[N-1] ^ f
                workingSet.rn[blockOffset].Reset();
                for (tableOffset = 0; tableOffset < 8; tableOffset++)
                {

                    // Get Ln[N-1] -> A
                    byte a = workingSet.ln[blockOffset - 1].data[(tableOffset >> 1)];
                    if ((tableOffset % 2) == 0)
                    {
                        a >>= 4;
                    }
                    else
                    {
                        a &= 0x0F;
                    }

                    // Get f -> B
                    byte b = Convert.ToByte(workingSet.f.data[tableOffset] >> 4);

                    // Update Rn[N]
                    if ((tableOffset % 2) == 0)
                    {
                        workingSet.rn[blockOffset].data[tableOffset >> 1] |= Convert.ToByte((a ^ b) << 4);
                    }
                    else
                    {
                        workingSet.rn[blockOffset].data[tableOffset >> 1] |= Convert.ToByte(a ^ b);
                    }
                }

            }

            // X = R16 L16
            workingSet.x.Reset();
            for (tableOffset = 0; tableOffset < 4; tableOffset++)
            {
                workingSet.x.data[tableOffset] = workingSet.rn[16].data[tableOffset];
                workingSet.x.data[tableOffset + 4] = workingSet.ln[16].data[tableOffset];
            }

            // C = X perm IP
            workingSet.outputBlock.Reset();
            for (tableOffset = 0; tableOffset < byteFP.Length; tableOffset++)
            {

                // Get perm offset
                permOffset = byteFP[tableOffset];
                permOffset--;

                // Get and set bit
                workingSet.outputBlock.SetBit(
                    BitAddressToByteOffset(tableOffset, 8),
                    BitAddressToBitOffset(tableOffset, 8),
                    workingSet.x.GetBit(
                        BitAddressToByteOffset(permOffset, 8),
                        BitAddressToBitOffset(permOffset, 8)
                    )
                );

            }
            outputCiphertext = workingSet.outputBlock.data;

            //Fill String Attribute
            ciphertext = workingSet.outputBlock.ToBinaryString(64, 0);


            //Fill String Attributes
            for (int i = 0; i < 17; i++)
            {
                lrData[i, 0] = workingSet.ln[i].ToBinaryString(32, 0);
                lrData[i, 1] = workingSet.rn[i].ToBinaryString(32, 0);
            }
        }

        #endregion Low-level Operations

        #region Helper Operations

        private int BitAddressToByteOffset(int tableAddress, int tableWidth)
        {
            int tmp = tableAddress / tableWidth;
            return tmp;
        }

        private int BitAddressToBitOffset(int tableAddress, int tableWidth)
        {
            int tmp = BitsPerByte - 1 - (tableAddress % tableWidth);
            return tmp;
        }

        #endregion Helper Operations

    }
}

