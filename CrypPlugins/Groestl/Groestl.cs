/*
   Copyright 2014 Diego Alejandro Gómez <diego.gomezy@udea.edu.co>

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

using CrypTool.PluginBase;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using System.ComponentModel;
using System.Windows.Controls;

namespace CrypTool.Plugins.Groestl
{
    [Author("Diego Alejandro Gómez", "diego.gomezy@udea.edu.co", "Universidad de Antioquia", "http://www.udea.edu.co")]
    [PluginInfo("Groestl.Properties.Resources", "PluginCaption", "PluginTooltip", "Groestl/DetailedDescription/doc.xml",
        new[] { "Groestl/icon.png" })]
    [ComponentCategory(ComponentCategory.HashFunctions)]
    public class Groestl : ICrypComponent
    {
        #region Private Variables

        private readonly GroestlSettings settings = new GroestlSettings();

        private readonly byte[] IV256 = {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        };


        private readonly byte[] IV512 = {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        };

        private readonly byte[] Sbox = {
            0x63, 0x7c, 0x77, 0x7b, 0xf2, 0x6b, 0x6f, 0xc5,
            0x30, 0x01, 0x67, 0x2b, 0xfe, 0xd7, 0xab, 0x76,
            0xca, 0x82, 0xc9, 0x7d, 0xfa, 0x59, 0x47, 0xf0,
            0xad, 0xd4, 0xa2, 0xaf, 0x9c, 0xa4, 0x72, 0xc0,
            0xb7, 0xfd, 0x93, 0x26, 0x36, 0x3f, 0xf7, 0xcc,
            0x34, 0xa5, 0xe5, 0xf1, 0x71, 0xd8, 0x31, 0x15,
            0x04, 0xc7, 0x23, 0xc3, 0x18, 0x96, 0x05, 0x9a,
            0x07, 0x12, 0x80, 0xe2, 0xeb, 0x27, 0xb2, 0x75,
            0x09, 0x83, 0x2c, 0x1a, 0x1b, 0x6e, 0x5a, 0xa0,
            0x52, 0x3b, 0xd6, 0xb3, 0x29, 0xe3, 0x2f, 0x84,
            0x53, 0xd1, 0x00, 0xed, 0x20, 0xfc, 0xb1, 0x5b,
            0x6a, 0xcb, 0xbe, 0x39, 0x4a, 0x4c, 0x58, 0xcf,
            0xd0, 0xef, 0xaa, 0xfb, 0x43, 0x4d, 0x33, 0x85,
            0x45, 0xf9, 0x02, 0x7f, 0x50, 0x3c, 0x9f, 0xa8,
            0x51, 0xa3, 0x40, 0x8f, 0x92, 0x9d, 0x38, 0xf5,
            0xbc, 0xb6, 0xda, 0x21, 0x10, 0xff, 0xf3, 0xd2,
            0xcd, 0x0c, 0x13, 0xec, 0x5f, 0x97, 0x44, 0x17,
            0xc4, 0xa7, 0x7e, 0x3d, 0x64, 0x5d, 0x19, 0x73,
            0x60, 0x81, 0x4f, 0xdc, 0x22, 0x2a, 0x90, 0x88,
            0x46, 0xee, 0xb8, 0x14, 0xde, 0x5e, 0x0b, 0xdb,
            0xe0, 0x32, 0x3a, 0x0a, 0x49, 0x06, 0x24, 0x5c,
            0xc2, 0xd3, 0xac, 0x62, 0x91, 0x95, 0xe4, 0x79,
            0xe7, 0xc8, 0x37, 0x6d, 0x8d, 0xd5, 0x4e, 0xa9,
            0x6c, 0x56, 0xf4, 0xea, 0x65, 0x7a, 0xae, 0x08,
            0xba, 0x78, 0x25, 0x2e, 0x1c, 0xa6, 0xb4, 0xc6,
            0xe8, 0xdd, 0x74, 0x1f, 0x4b, 0xbd, 0x8b, 0x8a,
            0x70, 0x3e, 0xb5, 0x66, 0x48, 0x03, 0xf6, 0x0e,
            0x61, 0x35, 0x57, 0xb9, 0x86, 0xc1, 0x1d, 0x9e,
            0xe1, 0xf8, 0x98, 0x11, 0x69, 0xd9, 0x8e, 0x94,
            0x9b, 0x1e, 0x87, 0xe9, 0xce, 0x55, 0x28, 0xdf,
            0x8c, 0xa1, 0x89, 0x0d, 0xbf, 0xe6, 0x42, 0x68,
            0x41, 0x99, 0x2d, 0x0f, 0xb0, 0x54, 0xbb, 0x16
    };
        private byte[] IV;
        private int bufferSize = 0;

        private byte[] _outputData;

        #endregion

        #region Data Properties

        /// <summary>
        /// Input data to be hashed.
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputDataStreamCaption", "InputDataStreamTooltip", true)]
        public ICrypToolStream InputStream
        {
            get;
            set;
        }

        /// <summary>
        /// Hash value.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputDataCaption", "OutputDataTooltip", true)]
        public byte[] OutputData
        {
            get => _outputData;
            private set
            {
                _outputData = value;
                OnPropertyChanged("OutputData");
            }
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings => settings;

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation => null;

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            if (settings.SelectedVariant == (int)GroestlVariant.Groestl224)
            {
                bufferSize = 64;
                IV = IV256;
                IV[62] = 0x00;
                IV[63] = 0xe0;
            }
            else if (settings.SelectedVariant == (int)GroestlVariant.Groestl256)
            {
                bufferSize = 64;
                IV = IV256;
                IV[62] = 0x01;
                IV[63] = 0x00;
            }
            else if (settings.SelectedVariant == (int)GroestlVariant.Groestl384)
            {
                bufferSize = 128;
                IV = IV512;
                IV[126] = 0x01;
                IV[127] = 0x80;
            }
            else
            {
                bufferSize = 128;
                IV = IV512;
                IV[126] = 0x02;
                IV[127] = 0x00;
            }
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            // The following variables follow the same naming convention used in the
            // specification:
            //     l: length of each message block, in bits.
            //     t: number of blocks the whole message uses.
            //     T: total number of blocks.
            //     N: number of bits in the input message.
            //     w: number of zeroes for padding.
            int l = bufferSize * 8;
            ulong t = 0;
            ulong T = 0;
            ulong N = 0;
            ulong w = 0;

            byte[] h = new byte[bufferSize];
            System.Array.Copy(IV, h, bufferSize);
            ProgressChanged(0, 1);
            using (CStreamReader reader = InputStream.CreateReader())
            {
                int bytesRead;
                byte[] buffer = new byte[bufferSize];

                while ((bytesRead = reader.ReadFully(buffer)) == bufferSize)
                {
                    N = N + (ulong)bytesRead * 8;
                    h = Compression(h, buffer);
                    t = t + 1;
                }
                ProgressChanged(0.8, 1);
                N = N + (ulong)bytesRead * 8;
                w = (ulong)((((-(long)N - 65) % l) + l) % l);
                T = (N + w + 65) / (ulong)l;
                int blocksLeft = (int)(T - t);
                buffer[bytesRead] = 0x80;
                int numberOfZeroBytes = bufferSize - bytesRead - 1;
                byte[] zeroes = new byte[numberOfZeroBytes];
                System.Array.Copy(zeroes, 0, buffer, bytesRead + 1, numberOfZeroBytes);
                for (uint i = 0; i < blocksLeft; i++)
                {
                    if (i == blocksLeft - 1)
                    {
                        byte[] bytes = System.BitConverter.GetBytes(T);
                        if (System.BitConverter.IsLittleEndian)
                        {
                            System.Array.Reverse(bytes);
                        }
                        System.Array.Copy(bytes, 0, buffer, bufferSize - 8, 8);
                    }
                    h = Compression(h, buffer);
                    buffer = new byte[bufferSize];
                }
                h = Truncation(XOR(P(h), h));
                OutputData = h;
            }
            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region Private Methods

        private byte[] Compression(byte[] h, byte[] m)
        {
            return XOR(XOR(P(XOR(h, m)), Q(m)), h);
        }

        private byte[] P(byte[] input)
        {
            int R = 0;
            if (settings.SelectedVariant == (int)GroestlVariant.Groestl224 ||
                settings.SelectedVariant == (int)GroestlVariant.Groestl256)
            {
                R = 10;
            }
            else
            {
                R = 14;
            }

            byte[][] state = BytesToMatrixMap(input);
            for (byte r = 0; r < R; r++)
            {
                AddRoundConstant(state, r, 0);
                SubBytes(state);
                ShiftBytes(state, 0);
                MixBytes(state);
            }
            return MatrixToBytesMap(state);
        }

        private byte[] Q(byte[] input)
        {
            int R = 0;
            if (settings.SelectedVariant == (int)GroestlVariant.Groestl224 ||
                settings.SelectedVariant == (int)GroestlVariant.Groestl256)
            {
                R = 10;
            }
            else
            {
                R = 14;
            }

            byte[][] state = BytesToMatrixMap(input);
            for (byte r = 0; r < R; r++)
            {
                AddRoundConstant(state, r, 1);
                SubBytes(state);
                ShiftBytes(state, 1);
                MixBytes(state);
            }
            return MatrixToBytesMap(state);
        }

        #region RoundTransformations

        private void AddRoundConstant(byte[][] state, byte r, byte permutation)
        {
            int nColumns = 0;
            if (settings.SelectedVariant == (int)GroestlVariant.Groestl224 ||
                settings.SelectedVariant == (int)GroestlVariant.Groestl256)
            {
                nColumns = 8;
            }
            else
            {
                nColumns = 16;
            }
            //permutation == 0 -> P ^ permutation == 1 -> Q
            if (permutation == 0)
            {
                byte[] C = {(byte)(0x00 ^ r), (byte)(0x10 ^ r), (byte)(0x20 ^ r),
                           (byte)(0x30 ^ r), (byte)(0x40 ^ r), (byte)(0x50 ^ r),
                           (byte)(0x60 ^ r), (byte)(0x70 ^ r), (byte)(0x80 ^ r),
                           (byte)(0x90 ^ r), (byte)(0xa0 ^ r), (byte)(0xb0 ^ r),
                           (byte)(0xc0 ^ r), (byte)(0xd0 ^ r), (byte)(0xe0 ^ r),
                           (byte)(0xf0 ^ r)};
                for (int j = 0; j < nColumns; j++)
                {
                    state[0][j] = (byte)(state[0][j] ^ C[j]);
                }
            }
            else
            {
                byte[] C = {(byte)(0xff ^ r), (byte)(0xef ^ r), (byte)(0xdf ^ r),
                           (byte)(0xcf ^ r), (byte)(0xbf ^ r), (byte)(0xaf ^ r),
                           (byte)(0x9f ^ r), (byte)(0x8f ^ r), (byte)(0x7f ^ r),
                           (byte)(0x6f ^ r), (byte)(0x5f ^ r), (byte)(0x4f ^ r),
                           (byte)(0x3f ^ r), (byte)(0x2f ^ r), (byte)(0x1f ^ r),
                           (byte)(0x0f ^ r)};
                for (int i = 0; i < 7; i++)
                {
                    for (int j = 0; j < nColumns; j++)
                    {
                        state[i][j] = (byte)(state[i][j] ^ 0xff);
                    }
                }
                for (int j = 0; j < nColumns; j++)
                {
                    state[7][j] = (byte)(state[7][j] ^ C[j]);
                }
            }
        }

        private void SubBytes(byte[][] state)
        {
            int nColumns = 0;
            if (settings.SelectedVariant == (int)GroestlVariant.Groestl224 ||
                settings.SelectedVariant == (int)GroestlVariant.Groestl256)
            {
                nColumns = 8;
            }
            else
            {
                nColumns = 16;
            }

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < nColumns; j++)
                {
                    state[i][j] = Sbox[state[i][j]];
                }
            }
        }

        private void ShiftBytes(byte[][] state, byte permutation)
        {
            //permutation == 0 -> P ^ permutation == 1 -> Q
            byte[] sigma;
            if (settings.SelectedVariant == (int)GroestlVariant.Groestl224 ||
                settings.SelectedVariant == (int)GroestlVariant.Groestl256)
            {
                if (permutation == 0)
                {
                    sigma = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
                }
                else
                {
                    sigma = new byte[] { 1, 3, 5, 7, 0, 2, 4, 6 };
                }
            }
            else
            {
                if (permutation == 0)
                {
                    sigma = new byte[] { 0, 1, 2, 3, 4, 5, 6, 11 };
                }
                else
                {
                    sigma = new byte[] { 1, 3, 5, 11, 0, 2, 4, 6 };
                }
            }
            for (int i = 0; i < 8; i++)
            {
                state[i] = ShiftRow(state[i], sigma[i]);
            }
        }

        private void MixBytes(byte[][] state)
        {
            int nColumns = 0;
            if (settings.SelectedVariant == (int)GroestlVariant.Groestl224 ||
                settings.SelectedVariant == (int)GroestlVariant.Groestl256)
            {
                nColumns = 8;
            }
            else
            {
                nColumns = 16;
            }

            byte[] x = new byte[8];
            byte[] y = new byte[8];
            byte[] z = new byte[8];
            for (int j = 0; j < nColumns; j++)
            {
                x[0] = (byte)(state[0][j] ^ state[(0 + 1) % 8][j]);
                x[1] = (byte)(state[1][j] ^ state[(1 + 1) % 8][j]);
                x[2] = (byte)(state[2][j] ^ state[(2 + 1) % 8][j]);
                x[3] = (byte)(state[3][j] ^ state[(3 + 1) % 8][j]);
                x[4] = (byte)(state[4][j] ^ state[(4 + 1) % 8][j]);
                x[5] = (byte)(state[5][j] ^ state[(5 + 1) % 8][j]);
                x[6] = (byte)(state[6][j] ^ state[(6 + 1) % 8][j]);
                x[7] = (byte)(state[7][j] ^ state[(7 + 1) % 8][j]);
                y[0] = (byte)(x[0] ^ x[(0 + 3) % 8]);
                y[1] = (byte)(x[1] ^ x[(1 + 3) % 8]);
                y[2] = (byte)(x[2] ^ x[(2 + 3) % 8]);
                y[3] = (byte)(x[3] ^ x[(3 + 3) % 8]);
                y[4] = (byte)(x[4] ^ x[(4 + 3) % 8]);
                y[5] = (byte)(x[5] ^ x[(5 + 3) % 8]);
                y[6] = (byte)(x[6] ^ x[(6 + 3) % 8]);
                y[7] = (byte)(x[7] ^ x[(7 + 3) % 8]);
                z[0] = (byte)(x[0] ^ x[(0 + 2) % 8] ^ state[(0 + 6) % 8][j]);
                z[1] = (byte)(x[1] ^ x[(1 + 2) % 8] ^ state[(1 + 6) % 8][j]);
                z[2] = (byte)(x[2] ^ x[(2 + 2) % 8] ^ state[(2 + 6) % 8][j]);
                z[3] = (byte)(x[3] ^ x[(3 + 2) % 8] ^ state[(3 + 6) % 8][j]);
                z[4] = (byte)(x[4] ^ x[(4 + 2) % 8] ^ state[(4 + 6) % 8][j]);
                z[5] = (byte)(x[5] ^ x[(5 + 2) % 8] ^ state[(5 + 6) % 8][j]);
                z[6] = (byte)(x[6] ^ x[(6 + 2) % 8] ^ state[(6 + 6) % 8][j]);
                z[7] = (byte)(x[7] ^ x[(7 + 2) % 8] ^ state[(7 + 6) % 8][j]);
                state[0][j] = (byte)(Doubling((byte)(Doubling(y[(0 + 3) % 8]) ^ z[(0 + 7) % 8])) ^ z[(0 + 4) % 8]);
                state[1][j] = (byte)(Doubling((byte)(Doubling(y[(1 + 3) % 8]) ^ z[(1 + 7) % 8])) ^ z[(1 + 4) % 8]);
                state[2][j] = (byte)(Doubling((byte)(Doubling(y[(2 + 3) % 8]) ^ z[(2 + 7) % 8])) ^ z[(2 + 4) % 8]);
                state[3][j] = (byte)(Doubling((byte)(Doubling(y[(3 + 3) % 8]) ^ z[(3 + 7) % 8])) ^ z[(3 + 4) % 8]);
                state[4][j] = (byte)(Doubling((byte)(Doubling(y[(4 + 3) % 8]) ^ z[(4 + 7) % 8])) ^ z[(4 + 4) % 8]);
                state[5][j] = (byte)(Doubling((byte)(Doubling(y[(5 + 3) % 8]) ^ z[(5 + 7) % 8])) ^ z[(5 + 4) % 8]);
                state[6][j] = (byte)(Doubling((byte)(Doubling(y[(6 + 3) % 8]) ^ z[(6 + 7) % 8])) ^ z[(6 + 4) % 8]);
                state[7][j] = (byte)(Doubling((byte)(Doubling(y[(7 + 3) % 8]) ^ z[(7 + 7) % 8])) ^ z[(7 + 4) % 8]);
            }
        }

        #endregion

        #region Mapping methods

        private byte[][] BytesToMatrixMap(byte[] input)
        {
            int nColumns = 0;
            if (settings.SelectedVariant == (int)GroestlVariant.Groestl224 ||
                settings.SelectedVariant == (int)GroestlVariant.Groestl256)
            {
                nColumns = 8;
            }
            else
            {
                nColumns = 16;
            }
            int k = 0;
            byte[][] result = new byte[][]
        {
            new byte[nColumns],
            new byte[nColumns],
            new byte[nColumns],
            new byte[nColumns],
            new byte[nColumns],
            new byte[nColumns],
            new byte[nColumns],
            new byte[nColumns]
        };
            for (int j = 0; j < nColumns; j++)
            {
                for (int i = 0; i < 8; i++)
                {
                    result[i][j] = input[k];
                    k++;
                }
            }
            return result;
        }

        private byte[] MatrixToBytesMap(byte[][] input)
        {
            int nBytes = 0;
            if (settings.SelectedVariant == (int)GroestlVariant.Groestl224 ||
                settings.SelectedVariant == (int)GroestlVariant.Groestl256)
            {
                nBytes = 64;
            }
            else
            {
                nBytes = 128;
            }
            int k = 0;
            byte[] result = new byte[nBytes];
            for (int j = 0; j < nBytes / 8; j++)
            {
                for (int i = 0; i < 8; i++)
                {
                    result[k] = input[i][j];
                    k++;
                }
            }
            return result;
        }

        #endregion

        private byte[] Truncation(byte[] x)
        {
            int nBytes = 0;
            if (settings.SelectedVariant == (int)GroestlVariant.Groestl224)
            {
                nBytes = 28;
            }
            else if (settings.SelectedVariant == (int)GroestlVariant.Groestl256)
            {
                nBytes = 32;
            }
            else if (settings.SelectedVariant == (int)GroestlVariant.Groestl384)
            {
                nBytes = 48;
            }
            else
            {
                nBytes = 64;
            }
            byte[] result = new byte[nBytes];
            System.Buffer.BlockCopy(x, x.Length - nBytes, result, 0, nBytes);
            return result;
        }

        private byte[] XOR(byte[] op1, byte[] op2)
        {
            byte[] result = new byte[op1.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (byte)(op1[i] ^ op2[i]);
            }
            return result;
        }

        private byte Doubling(byte x)
        {
            return (byte)((x & 0x80) == 0x80 ? (x << 1) ^ 0x1b : x << 1);
        }

        private byte[] ShiftRow(byte[] row, int shift)
        {
            byte[] newRow = new byte[row.Length];
            System.Buffer.BlockCopy(row, shift, newRow, 0, row.Length - shift);
            System.Buffer.BlockCopy(row, 0, newRow, row.Length - shift, shift);
            return newRow;
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}
