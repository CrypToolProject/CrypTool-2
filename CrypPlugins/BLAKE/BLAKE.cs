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

namespace CrypTool.Plugins.BLAKE
{
    [Author("Diego Alejandro Gómez", "diego.gomezy@udea.edu.co", "Universidad de Antioquia", "http://www.udea.edu.co")]
    [PluginInfo("BLAKE.Properties.Resources", "PluginCaption", "PluginTooltip", "BLAKE/DetailedDescription/doc.xml",
        new[] { "BLAKE/icon.png" })]
    [ComponentCategory(ComponentCategory.HashFunctions)]
    public class BLAKE : ICrypComponent
    {
        #region Private Variables

        private readonly BLAKESettings settings = new BLAKESettings();

        private uint[] IV32BitWords;
        private ulong[] IV64BitWords;

        private readonly uint[] IV224 = new uint[] {
            0xc1059ed8, 0x367cd507,
            0x3070dd17, 0xf70e5939,
            0xffc00b31, 0x68581511,
            0x64f98fa7, 0xbefa4fa4
        };

        private readonly uint[] IV256 = new uint[] {
            0x6a09e667, 0xbb67ae85,
            0x3c6ef372, 0xa54ff53a,
            0x510e527f, 0x9b05688c,
            0x1f83d9ab, 0x5be0cd19
        };

        private readonly ulong[] IV384 = new ulong[] {
            0xcbbb9d5dc1059ed8, 0x629a292a367cd507,
            0x9159015a3070dd17, 0x152fecd8f70e5939,
            0x67332667ffc00b31, 0x8eb44a8768581511,
            0xdb0c2e0d64f98fa7, 0x47b5481dbefa4fa4
        };

        private readonly ulong[] IV512 = new ulong[] {
            0x6a09e667f3bcc908, 0xbb67ae8584caa73b,
            0x3c6ef372fe94f82b, 0xa54ff53a5f1d36f1,
            0x510e527fade682d1, 0x9b05688c2b3e6c1f,
            0x1f83d9abfb41bd6b, 0x5be0cd19137e2179
        };

        private readonly uint[] c256 = new uint[] {
            0x243f6a88, 0x85a308d3,
            0x13198a2e, 0x03707344,
            0xa4093822, 0x299f31d0,
            0x082efa98, 0xec4e6c89,
            0x452821e6, 0x38d01377,
            0xbe5466cf, 0x34e90c6c,
            0xc0ac29b7, 0xc97c50dd,
            0x3f84d5b5, 0xb5470917
        };

        private readonly ulong[] c512 = new ulong[] {
            0x243f6a8885a308d3, 0x13198a2e03707344,
            0xa4093822299f31d0, 0x082efa98ec4e6c89,
            0x452821e638d01377, 0xbe5466cf34e90c6c,
            0xc0ac29b7c97c50dd, 0x3f84d5b5b5470917,
            0x9216d5d98979fb1b, 0xd1310ba698dfb5ac,
            0x2ffd72dbd01adfb7, 0xb8e1afed6a267e96,
            0xba7c9045f12c7f99, 0x24a19947b3916cf7,
            0x0801f2e2858efc16, 0x636920d871574e69
        };

        private readonly uint[][] sigma = new uint[][] {
            new uint[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15},
            new uint[] {14, 10, 4, 8, 9, 15, 13, 6, 1, 12, 0, 2, 11, 7, 5, 3},
            new uint[] {11, 8, 12, 0, 5, 2, 15, 13, 10, 14, 3, 6, 7, 1, 9, 4},
            new uint[] {7, 9, 3, 1, 13, 12, 11, 14, 2, 6, 5, 10, 4, 0, 15, 8},
            new uint[] {9, 0, 5, 7, 2, 4, 10, 15, 14, 1, 11, 12, 6, 8, 3, 13},
            new uint[] {2, 12, 6, 10, 0, 11, 8, 3, 4, 13, 7, 5, 15, 14, 1, 9},
            new uint[] {12, 5, 1, 15, 14, 13, 4, 10, 0, 7, 6, 3, 9, 2, 8, 11},
            new uint[] {13, 11, 7, 14, 12, 1, 3, 9, 5, 0, 15, 4, 8, 6, 2, 10},
            new uint[] {6, 15, 14, 9, 11, 3, 0, 8, 12, 2, 13, 7, 1, 4, 10, 5},
            new uint[] {10, 2, 8, 4, 7, 6, 1, 5, 15, 11, 9, 14, 3, 12, 13, 0},
        };

        private int bufferSize = 0;
        private byte paddingBit = 0;
        private int finalBytes = 0;

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

        [PropertyInfo(Direction.InputData, "SaltDataCaption", "SaltDataTooltip", false)]
        public byte[] SaltData
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
            if (settings.SelectedFunction == (int)BLAKEFunction.BLAKE224)
            {
                bufferSize = 64;
                paddingBit = 0x00;
                finalBytes = 8;
                IV32BitWords = IV224;
                IV64BitWords = IV384;
            }
            else if (settings.SelectedFunction == (int)BLAKEFunction.BLAKE256)
            {
                bufferSize = 64;
                paddingBit = 0x01;
                finalBytes = 8;
                IV32BitWords = IV256;
                IV64BitWords = IV512;
            }
            else if (settings.SelectedFunction == (int)BLAKEFunction.BLAKE384)
            {
                bufferSize = 128;
                paddingBit = 0x00;
                finalBytes = 16;
                IV32BitWords = IV224;
                IV64BitWords = IV384;
            }
            else
            {
                bufferSize = 128;
                paddingBit = 0x01;
                finalBytes = 16;
                IV32BitWords = IV256;
                IV64BitWords = IV512;
            }
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ulong dataBitLength = 0;
            int blockBitLength = bufferSize * 8;
            ulong n = 0;
            byte[] hash;

            ProgressChanged(0, 1);

            // Used when the BLAKE function is either BLAKE-224 or BLAKE-256.
            uint[] h32 = new uint[8];
            System.Array.Copy(IV32BitWords, h32, 8);
            uint[] m32 = new uint[16];
            uint[] s32 = new uint[4] { 0, 0, 0, 0 };
            uint[] t32;

            // Used when the BLAKE function is either BLAKE-384 or BLAKE-512.
            ulong[] h64 = new ulong[8];
            System.Array.Copy(IV64BitWords, h64, 8);
            ulong[] m64 = new ulong[16];
            ulong[] s64 = new ulong[4] { 0, 0, 0, 0 };
            ulong[] t64;

            switch (CheckSalt(SaltData))
            {
                case 0:
                    // Nothing connected to InputSalt connector. Use default value for salt.
                    break;
                case 1:
                    // Salt size is different from the one required for BLAKE-224 and BLAKE-256.
                    GuiLogMessage(string.Format("ERROR: Wrong salt size. Salt size must be {0} bytes", 16), NotificationLevel.Error);
                    return;
                case 2:
                    // Salt size is different from the one required for BLAKE-384 and BLAKE-512.
                    GuiLogMessage(string.Format("ERROR: Wrong salt size. Salt size must be {0} bytes", 32), NotificationLevel.Error);
                    return;
                default:
                    // Salt size is correct. Fill the right salt variable depending on the BLAKE function selected.
                    if (settings.SelectedFunction == (int)BLAKEFunction.BLAKE224 ||
                        settings.SelectedFunction == (int)BLAKEFunction.BLAKE256)
                    {
                        s32 = BytesToUint32Block(SaltData);
                    }
                    else
                    {
                        s64 = BytesToUint64Block(SaltData);
                    }
                    break;
            }

            using (CStreamReader reader = InputStream.CreateReader())
            {
                int bytesRead;
                byte[] buffer = new byte[bufferSize];
                while ((bytesRead = reader.ReadFully(buffer)) == bufferSize)
                {
                    dataBitLength = dataBitLength + (ulong)bytesRead * 8;
                    if (settings.SelectedFunction == (int)BLAKEFunction.BLAKE224 ||
                        settings.SelectedFunction == (int)BLAKEFunction.BLAKE256)
                    {
                        m32 = BytesToUint32Block(buffer);
                        t32 = LengthToUint32Words(dataBitLength);
                        Compress32(h32, m32, s32, t32);
                    }
                    else
                    {
                        m64 = BytesToUint64Block(buffer);
                        t64 = LengthToUint64Words(dataBitLength);
                        Compress64(h64, m64, s64, t64);
                    }
                    n = n + 1;
                }
                ProgressChanged(0.8, 1);
                dataBitLength = dataBitLength + (ulong)bytesRead * 8;
                ulong numberOfZeroBits = (ulong)((((-(long)dataBitLength - (finalBytes * 8 + 1)) % blockBitLength) + blockBitLength) % blockBitLength);
                ulong N = (dataBitLength + numberOfZeroBits + ((ulong)finalBytes * 8 + 1)) / (ulong)blockBitLength;
                int blocksLeft = (int)(N - n);
                buffer[bytesRead] = 0x80;
                int numberOfZeroBytes = bufferSize - bytesRead - 1;
                byte[] zeroes = new byte[numberOfZeroBytes];
                System.Array.Copy(zeroes, 0, buffer, bytesRead + 1, numberOfZeroBytes);
                for (int i = 0; i < blocksLeft; i++)
                {
                    if (i == blocksLeft - 1)
                    {
                        byte[] length = System.BitConverter.GetBytes(dataBitLength);
                        if (System.BitConverter.IsLittleEndian)
                        {
                            System.Array.Reverse(length);
                        }
                        System.Array.Copy(length, 0, buffer, bufferSize - 8, 8);
                        buffer[bufferSize - finalBytes - 1] ^= paddingBit;
                        if (blocksLeft == 2 || (blocksLeft == 1 && bytesRead == 0))
                        {
                            dataBitLength = 0;
                        }
                    }
                    if (settings.SelectedFunction == (int)BLAKEFunction.BLAKE224 ||
                        settings.SelectedFunction == (int)BLAKEFunction.BLAKE256)
                    {
                        m32 = BytesToUint32Block(buffer);
                        t32 = LengthToUint32Words(dataBitLength);
                        Compress32(h32, m32, s32, t32);
                    }
                    else
                    {
                        m64 = BytesToUint64Block(buffer);
                        t64 = LengthToUint64Words(dataBitLength);
                        Compress64(h64, m64, s64, t64);
                    }
                    buffer = new byte[bufferSize];
                }
                if (settings.SelectedFunction == (int)BLAKEFunction.BLAKE224 ||
                    settings.SelectedFunction == (int)BLAKEFunction.BLAKE256)
                {
                    hash = Uint32BlockToBytes(h32);
                }
                else
                {
                    hash = Uint64BlockToBytes(h64);
                }
                if (settings.SelectedFunction == (int)BLAKEFunction.BLAKE224)
                {
                    System.Array.Resize(ref hash, 28);
                }

                if (settings.SelectedFunction == (int)BLAKEFunction.BLAKE384)
                {
                    System.Array.Resize(ref hash, 48);
                }

                OutputData = hash;
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

        private void Compress32(uint[] h, uint[] m, uint[] s, uint[] t)
        {
            int R = 14;
            uint[] v = new uint[] {
                h[0], h[1], h[2], h[3],
                h[4], h[5], h[6], h[7],
                s[0] ^ c256[0], s[1] ^ c256[1], s[2] ^ c256[2], s[3] ^ c256[3],
                t[0] ^ c256[4], t[0] ^ c256[5], t[1] ^ c256[6], t[1] ^ c256[7],
            };
            for (int r = 0; r < R; r++)
            {
                G32(0, ref v[0], ref v[4], ref v[8], ref v[12], m, r);
                G32(1, ref v[1], ref v[5], ref v[9], ref v[13], m, r);
                G32(2, ref v[2], ref v[6], ref v[10], ref v[14], m, r);
                G32(3, ref v[3], ref v[7], ref v[11], ref v[15], m, r);

                G32(4, ref v[0], ref v[5], ref v[10], ref v[15], m, r);
                G32(5, ref v[1], ref v[6], ref v[11], ref v[12], m, r);
                G32(6, ref v[2], ref v[7], ref v[8], ref v[13], m, r);
                G32(7, ref v[3], ref v[4], ref v[9], ref v[14], m, r);
            }
            h[0] = h[0] ^ s[0] ^ v[0] ^ v[8];
            h[1] = h[1] ^ s[1] ^ v[1] ^ v[9];
            h[2] = h[2] ^ s[2] ^ v[2] ^ v[10];
            h[3] = h[3] ^ s[3] ^ v[3] ^ v[11];
            h[4] = h[4] ^ s[0] ^ v[4] ^ v[12];
            h[5] = h[5] ^ s[1] ^ v[5] ^ v[13];
            h[6] = h[6] ^ s[2] ^ v[6] ^ v[14];
            h[7] = h[7] ^ s[3] ^ v[7] ^ v[15];
        }

        private void Compress64(ulong[] h, ulong[] m, ulong[] s, ulong[] t)
        {
            int R = 16;
            ulong[] v = new ulong[] {
                h[0], h[1], h[2], h[3],
                h[4], h[5], h[6], h[7],
                s[0] ^ c512[0], s[1] ^ c512[1], s[2] ^ c512[2], s[3] ^ c512[3],
                t[0] ^ c512[4], t[0] ^ c512[5], t[1] ^ c512[6], t[1] ^ c512[7],
            };
            for (int r = 0; r < R; r++)
            {
                G64(0, ref v[0], ref v[4], ref v[8], ref v[12], m, r);
                G64(1, ref v[1], ref v[5], ref v[9], ref v[13], m, r);
                G64(2, ref v[2], ref v[6], ref v[10], ref v[14], m, r);
                G64(3, ref v[3], ref v[7], ref v[11], ref v[15], m, r);

                G64(4, ref v[0], ref v[5], ref v[10], ref v[15], m, r);
                G64(5, ref v[1], ref v[6], ref v[11], ref v[12], m, r);
                G64(6, ref v[2], ref v[7], ref v[8], ref v[13], m, r);
                G64(7, ref v[3], ref v[4], ref v[9], ref v[14], m, r);
            }
            h[0] = h[0] ^ s[0] ^ v[0] ^ v[8];
            h[1] = h[1] ^ s[1] ^ v[1] ^ v[9];
            h[2] = h[2] ^ s[2] ^ v[2] ^ v[10];
            h[3] = h[3] ^ s[3] ^ v[3] ^ v[11];
            h[4] = h[4] ^ s[0] ^ v[4] ^ v[12];
            h[5] = h[5] ^ s[1] ^ v[5] ^ v[13];
            h[6] = h[6] ^ s[2] ^ v[6] ^ v[14];
            h[7] = h[7] ^ s[3] ^ v[7] ^ v[15];
        }

        private void G32(int i, ref uint a, ref uint b, ref uint c, ref uint d, uint[] m, int r)
        {
            r = r % 10;
            a = a + b + (m[sigma[r][2 * i]] ^ c256[sigma[r][2 * i + 1]]);
            d = (d ^ a) >> 16 | (d ^ a) << (32 - 16);
            c = c + d;
            b = (b ^ c) >> 12 | (b ^ c) << (32 - 12);
            a = a + b + (m[sigma[r][2 * i + 1]] ^ c256[sigma[r][2 * i]]);
            d = (d ^ a) >> 8 | (d ^ a) << (32 - 8);
            c = c + d;
            b = (b ^ c) >> 7 | (b ^ c) << (32 - 7);
        }

        private void G64(int i, ref ulong a, ref ulong b, ref ulong c, ref ulong d, ulong[] m, int r)
        {
            r = r % 10;
            a = a + b + (m[sigma[r][2 * i]] ^ c512[sigma[r][2 * i + 1]]);
            d = (d ^ a) >> 32 | (d ^ a) << (64 - 32);
            c = c + d;
            b = (b ^ c) >> 25 | (b ^ c) << (64 - 25);
            a = a + b + (m[sigma[r][2 * i + 1]] ^ c512[sigma[r][2 * i]]);
            d = (d ^ a) >> 16 | (d ^ a) << (64 - 16);
            c = c + d;
            b = (b ^ c) >> 11 | (b ^ c) << (64 - 11);
        }

        private int CheckSalt(byte[] salt)
        {
            if (salt == null)
            {
                return 0;
            }

            if ((settings.SelectedFunction == (int)BLAKEFunction.BLAKE224 ||
                settings.SelectedFunction == (int)BLAKEFunction.BLAKE256) &&
                salt != null && salt.Length != 16)
            {
                return 1;
            }

            if ((settings.SelectedFunction == (int)BLAKEFunction.BLAKE384 ||
                settings.SelectedFunction == (int)BLAKEFunction.BLAKE512) &&
                salt != null && salt.Length != 32)
            {
                return 2;
            }

            return 3;
        }

        private uint[] BytesToUint32Block(byte[] x)
        {
            uint[] result = new uint[x.Length / 4];
            byte[] temp = new byte[4];
            for (int i = 0; i < x.Length / 4; i++)
            {
                System.Array.Copy(x, i * 4, temp, 0, 4);
                if (System.BitConverter.IsLittleEndian)
                {
                    System.Array.Reverse(temp);
                }
                result[i] = System.BitConverter.ToUInt32(temp, 0);
            }
            return result;
        }

        private ulong[] BytesToUint64Block(byte[] x)
        {
            ulong[] result = new ulong[x.Length / 8];
            byte[] temp = new byte[8];
            for (int i = 0; i < x.Length / 8; i++)
            {
                System.Array.Copy(x, i * 8, temp, 0, 8);
                if (System.BitConverter.IsLittleEndian)
                {
                    System.Array.Reverse(temp);
                }
                result[i] = System.BitConverter.ToUInt64(temp, 0);
            }
            return result;
        }

        private byte[] Uint32BlockToBytes(uint[] x)
        {
            byte[] result = new byte[32];
            byte[] temp = new byte[4];
            for (int i = 0; i < x.Length; i++)
            {
                temp = System.BitConverter.GetBytes(x[i]);
                if (System.BitConverter.IsLittleEndian)
                {
                    System.Array.Reverse(temp);
                }
                result[(4 * i)] = temp[0];
                result[(4 * i) + 1] = temp[1];
                result[(4 * i) + 2] = temp[2];
                result[(4 * i) + 3] = temp[3];
            }
            return result;
        }

        private byte[] Uint64BlockToBytes(ulong[] x)
        {
            byte[] result = new byte[64];
            byte[] temp = new byte[8];
            for (int i = 0; i < x.Length; i++)
            {
                temp = System.BitConverter.GetBytes(x[i]);
                if (System.BitConverter.IsLittleEndian)
                {
                    System.Array.Reverse(temp);
                }
                result[(8 * i)] = temp[0];
                result[(8 * i) + 1] = temp[1];
                result[(8 * i) + 2] = temp[2];
                result[(8 * i) + 3] = temp[3];
                result[(8 * i) + 4] = temp[4];
                result[(8 * i) + 5] = temp[5];
                result[(8 * i) + 6] = temp[6];
                result[(8 * i) + 7] = temp[7];
            }
            return result;
        }

        private uint[] LengthToUint32Words(ulong N)
        {
            uint[] result = new uint[2];
            byte[] byteRepresentation = System.BitConverter.GetBytes(N);
            if (System.BitConverter.IsLittleEndian)
            {
                System.Array.Reverse(byteRepresentation);
            }
            byte[] temp = new byte[4];
            System.Array.Copy(byteRepresentation, byteRepresentation.Length - 4, temp, 0, 4);
            if (System.BitConverter.IsLittleEndian)
            {
                System.Array.Reverse(temp);
            }
            result[0] = System.BitConverter.ToUInt32(temp, 0);
            System.Array.Copy(byteRepresentation, byteRepresentation.Length - 8, temp, 0, 4);
            if (System.BitConverter.IsLittleEndian)
            {
                System.Array.Reverse(temp);
            }
            result[1] = System.BitConverter.ToUInt32(temp, 0);
            return result;
        }

        private ulong[] LengthToUint64Words(ulong N)
        {
            ulong[] result = new ulong[2];
            byte[] byteRepresentation = System.BitConverter.GetBytes(N);
            if (System.BitConverter.IsLittleEndian)
            {
                System.Array.Reverse(byteRepresentation);
            }
            byte[] temp = new byte[8];
            System.Array.Copy(byteRepresentation, byteRepresentation.Length - 8, temp, 0, 8);
            if (System.BitConverter.IsLittleEndian)
            {
                System.Array.Reverse(temp);
            }
            result[0] = System.BitConverter.ToUInt64(temp, 0);
            result[1] = 0;
            return result;
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
