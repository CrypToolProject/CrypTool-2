/*
   Copyright 2009 Sören Rinne, Ruhr-Universität Bochum, Germany

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
using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace CrypTool.TEA
{
    [Author("Soeren Rinne", "soeren.rinne@CrypTool.de", "Ruhr-Universitaet Bochum, Chair for Embedded Security (EmSec)", "http://www.crypto.ruhr-uni-bochum.de/")]
    [PluginInfo("TEA.Properties.Resources", "PluginCaption", "PluginTooltip", "TEA/DetailedDescription/doc.xml", "TEA/Images/tea.png", "TEA/Images/encrypt.png", "TEA/Images/decrypt.png", "TEA/Images/encryptX.png", "TEA/Images/decryptX.png")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class TEA : ICrypComponent
    {
        #region IPlugin Members

        private TEASettings settings;
        private CStreamWriter outputStream;
        private byte[] inputKey;
        private byte[] inputIV;
        private bool stop = false;

        private delegate void CryptDelegate(int rounds, uint[] v, uint[] k);

        private readonly CryptDelegate[] delegates;
        private readonly TEAImage[] teaimages;

        #endregion

        public TEA()
        {
            settings = new TEASettings();

            delegates = new CryptDelegate[] { encode_tea, encode_xtea, encode_btea, decode_tea, decode_xtea, decode_btea };
            teaimages = new TEAImage[] { TEAImage.Encode, TEAImage.EncodeX, TEAImage.EncodeX, TEAImage.Decode, TEAImage.DecodeX, TEAImage.DecodeX };
        }

        public ISettings Settings
        {
            get => settings;
            set => settings = (TEASettings)value;
        }

        [PropertyInfo(Direction.InputData, "InputStreamCaption", "InputStreamTooltip", true)]
        public ICrypToolStream InputStream
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "InputKeyCaption", "InputKeyTooltip", true)]
        public byte[] InputKey
        {
            get => inputKey;
            set => inputKey = value;
        }

        [PropertyInfo(Direction.InputData, "InputIVCaption", "InputIVTooltip", false)]
        public byte[] InputIV
        {
            get => inputIV;
            set
            {
                inputIV = value;
                OnPropertyChanged("InputIV");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputStreamCaption", "OutputStreamTooltip", true)]
        public ICrypToolStream OutputStream
        {
            get => outputStream;
            set
            {
            }
        }

        public void Dispose()
        {
            Reset();
        }

        private void Reset()
        {
            try
            {
                stop = false;
                inputKey = null;
                outputStream = null;

                if (outputStream != null)
                {
                    outputStream.Flush();
                    outputStream.Close();
                    outputStream.Dispose();
                    outputStream = null;
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
        }

        public void Execute()
        {
            process(settings.Action, settings.Padding);
        }

        private void process(int action, int padding)
        {
            //Encrypt/Decrypt Stream
            try
            {
                if (InputStream == null || InputStream.Length == 0)
                {
                    GuiLogMessage("No input data, aborting now", NotificationLevel.Error);
                    return;
                }

                byte[] inputbuffer = new byte[8];
                byte[] outputbuffer = new byte[8];

                uint[] key = new uint[4];
                long[] longKey = new long[4];
                long keybytes = inputKey.Length;
                GuiLogMessage("inputKey length [byte]: " + keybytes.ToString(), NotificationLevel.Debug);

                if (keybytes != 16)
                {
                    GuiLogMessage("Given key has false length. Please provide a key with 128 Bits length. Aborting now.", NotificationLevel.Error);
                    return;
                }

                if (settings.Version != 2)
                {
                    key[0] = BitConverter.ToUInt32(inputKey, 0);
                    key[1] = BitConverter.ToUInt32(inputKey, 4);
                    key[2] = BitConverter.ToUInt32(inputKey, 8);
                    key[3] = BitConverter.ToUInt32(inputKey, 12);
                }
                else
                {
                    longKey[0] = BitConverter.ToUInt32(inputKey, 0);
                    longKey[1] = BitConverter.ToUInt32(inputKey, 4);
                    longKey[2] = BitConverter.ToUInt32(inputKey, 8);
                    longKey[3] = BitConverter.ToUInt32(inputKey, 12);
                }

                //check for a valid IV
                if (inputIV == null)
                {
                    //create a trivial IV 
                    inputIV = new byte[8];
                    if (settings.Mode != 0)
                    {
                        GuiLogMessage("WARNING - No IV provided. Using 0x000..00!", NotificationLevel.Warning);
                    }
                }
                byte[] IV = new byte[8];
                Array.Copy(inputIV, IV, Math.Min(inputIV.Length, IV.Length));

                DateTime startTime = DateTime.Now;

                uint[] vector = new uint[2];
                long[] longVector = new long[2];

                CryptDelegate crypfunc = delegates[settings.Action * 3 + settings.Version];
                StatusChanged((int)teaimages[settings.Action * 3 + settings.Version]);

                outputStream = new CStreamWriter();
                ICrypToolStream inputdata = InputStream;

                if (action == 0)
                {
                    inputdata = BlockCipherHelper.AppendPadding(InputStream, settings.padmap[settings.Padding], 8);
                }

                CStreamReader reader = inputdata.CreateReader();

                GuiLogMessage("Starting " + ((action == 0) ? "encryption" : "decryption") + " [Keysize=128 Bits, Blocksize=64 Bits]", NotificationLevel.Info);

                if (settings.Mode == 0)    // ECB
                {
                    while (reader.Read(inputbuffer, 0, inputbuffer.Length) > 0 && !stop)
                    {
                        Bytes2Vector(vector, inputbuffer);
                        crypfunc(settings.Rounds, vector, key);
                        Vector2Bytes(vector, outputbuffer);
                        outputStream.Write(outputbuffer);
                    }
                }
                else if (settings.Mode == 1)    // CBC
                {
                    if (settings.Action == 0)
                    {
                        while (reader.Read(inputbuffer, 0, inputbuffer.Length) > 0 && !stop)
                        {
                            for (int i = 0; i < 8; i++)
                            {
                                inputbuffer[i] ^= IV[i];
                            }

                            Bytes2Vector(vector, inputbuffer);
                            crypfunc(settings.Rounds, vector, key);
                            Vector2Bytes(vector, outputbuffer);
                            for (int i = 0; i < 8; i++)
                            {
                                IV[i] = outputbuffer[i];
                            }

                            outputStream.Write(outputbuffer);
                        }
                    }
                    else
                    {
                        while (reader.Read(inputbuffer, 0, inputbuffer.Length) > 0 && !stop)
                        {
                            Bytes2Vector(vector, inputbuffer);
                            crypfunc(settings.Rounds, vector, key);
                            Vector2Bytes(vector, outputbuffer);
                            for (int i = 0; i < 8; i++)
                            {
                                outputbuffer[i] ^= IV[i];
                            }

                            for (int i = 0; i < 8; i++)
                            {
                                IV[i] = inputbuffer[i];
                            }

                            outputStream.Write(outputbuffer);
                        }
                    }
                }
                else if (settings.Mode == 2)    // CFB
                {
                    if (settings.Action == 0)
                    {
                        while (reader.Read(inputbuffer, 0, inputbuffer.Length) > 0 && !stop)
                        {
                            Bytes2Vector(vector, IV);
                            crypfunc(settings.Rounds, vector, key);
                            Vector2Bytes(vector, outputbuffer);
                            for (int i = 0; i < 8; i++)
                            {
                                outputbuffer[i] ^= inputbuffer[i];
                            }

                            for (int i = 0; i < 8; i++)
                            {
                                IV[i] = outputbuffer[i];
                            }

                            outputStream.Write(outputbuffer);
                        }
                    }
                    else
                    {
                        crypfunc = delegates[settings.Version]; // choose encrypt function
                        while (reader.Read(inputbuffer, 0, inputbuffer.Length) > 0 && !stop)
                        {
                            Bytes2Vector(vector, IV);
                            crypfunc(settings.Rounds, vector, key);
                            Vector2Bytes(vector, outputbuffer);
                            for (int i = 0; i < 8; i++)
                            {
                                outputbuffer[i] ^= inputbuffer[i];
                            }

                            for (int i = 0; i < 8; i++)
                            {
                                IV[i] = inputbuffer[i];
                            }

                            outputStream.Write(outputbuffer);
                        }
                    }
                }
                else if (settings.Mode == 3)    // OFB - encrypt = decrypt
                {
                    crypfunc = delegates[settings.Version]; // choose encrypt function
                    while (reader.Read(inputbuffer, 0, inputbuffer.Length) > 0 && !stop)
                    {
                        Bytes2Vector(vector, IV);
                        crypfunc(settings.Rounds, vector, key);
                        Vector2Bytes(vector, outputbuffer);
                        for (int i = 0; i < 8; i++)
                        {
                            IV[i] = outputbuffer[i];
                        }

                        for (int i = 0; i < 8; i++)
                        {
                            outputbuffer[i] ^= inputbuffer[i];
                        }

                        outputStream.Write(outputbuffer);
                    }
                }

                long outbytes = outputStream.Length;
                DateTime stopTime = DateTime.Now;
                TimeSpan duration = stopTime - startTime;

                outputStream.Close();

                if (action == 1)
                {
                    outputStream = BlockCipherHelper.StripPadding(outputStream, settings.padmap[settings.Padding], 8) as CStreamWriter;
                }

                if (!stop)
                {
                    GuiLogMessage("Time used: " + duration.ToString(), NotificationLevel.Debug);
                    OnPropertyChanged("OutputStream");
                }
                else
                {
                    GuiLogMessage("Aborted!", NotificationLevel.Info);
                }
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
            finally
            {
                ProgressChanged(1, 1);
            }
        }

        private void Vector2Bytes(uint[] vector, byte[] buffer)
        {
            Array.Copy(BitConverter.GetBytes(vector[0]), 0, buffer, 0, 4);
            Array.Copy(BitConverter.GetBytes(vector[1]), 0, buffer, 4, 4);
        }

        private void Bytes2Vector(uint[] vector, byte[] buffer)
        {
            vector[0] = BitConverter.ToUInt32(buffer, 0);
            vector[1] = BitConverter.ToUInt32(buffer, 4);
        }

        private void encode_tea(int rounds, uint[] v, uint[] k)
        {
            uint y = v[0];
            uint z = v[1];

            uint k0 = k[0], k1 = k[1], k2 = k[2], k3 = k[3];

            uint sum = 0;
            uint delta = 0x9e3779b9;
            uint n = 64;

            while (n-- > 0)
            {
                /*
                 sum += delta;
                 v0 += ((v1<<4) + k0) ^ (v1 + sum) ^ ((v1>>5) + k1);
                 v1 += ((v0<<4) + k2) ^ (v0 + sum) ^ ((v0>>5) + k3);
                */
                sum += delta;
                y += ((z << 4) + k0) ^ (z + sum) ^ ((z >> 5) + k1);
                z += ((y << 4) + k2) ^ (y + sum) ^ ((y >> 5) + k3);
            }

            v[0] = y;
            v[1] = z;
        }

        private void decode_tea(int rounds, uint[] v, uint[] k)
        {
            uint n = 64;
            uint sum = 0x8DDE6E40; // for 64 rounds, for 32 rounds it would be 0xC6EF3720

            uint k0 = k[0], k1 = k[1], k2 = k[2], k3 = k[3];
            uint y = v[0];
            uint z = v[1];
            uint delta = 0x9e3779b9;

            while (n-- > 0)
            {
                /*
                 v1 -= ((v0<<4) + k2) ^ (v0 + sum) ^ ((v0>>5) + k3);
                 v0 -= ((v1<<4) + k0) ^ (v1 + sum) ^ ((v1>>5) + k1);
                 sum -= delta;
                */
                z -= ((y << 4) + k2) ^ (y + sum) ^ ((y >> 5) + k3);
                y -= ((z << 4) + k0) ^ (z + sum) ^ ((z >> 5) + k1);
                sum -= delta;
            }

            v[0] = y;
            v[1] = z;
        }

        private void encode_xtea(int rounds, uint[] v, uint[] k)
        {
            uint y = v[0];
            uint z = v[1];

            uint sum = 0;
            uint delta = 0x9e3779b9;
            int n = rounds;

            while (n-- > 0)
            {
                y += (z << 4 ^ z >> 5) + z ^ sum + k[sum & 3];
                sum += delta;
                z += (y << 4 ^ y >> 5) + y ^ sum + k[sum >> 11 & 3];
            }

            v[0] = y;
            v[1] = z;
        }

        private void decode_xtea(int rounds, uint[] v, uint[] k)
        {
            uint n = (uint)rounds;
            uint sum;
            uint y = v[0];
            uint z = v[1];
            uint delta = 0x9e3779b9;

            sum = delta * n;

            while (n-- > 0)
            {
                z -= (y << 4 ^ y >> 5) + y ^ sum + k[sum >> 11 & 3];
                sum -= delta;
                y -= (z << 4 ^ z >> 5) + z ^ sum + k[sum & 3];
            }

            v[0] = y;
            v[1] = z;
        }

        private void encode_btea(int rounds, uint[] v, uint[] k)
        {
            btea(v, 2, k);
        }

        private void decode_btea(int rounds, uint[] v, uint[] k)
        {
            btea(v, -2, k);
        }

        private uint btea(uint[] v, int n, uint[] k)
        {
            int m = n;
            if (n < -1)
            {
                m = -n;
            }

            uint z = v[m - 1], y = v[0], sum = 0, e, DELTA = 0x9e3779b9;

            int p, q;

            uint MX;

            if (n > 1)
            {          /* Coding Part */
                q = 6 + 52 / n;
                while (q-- > 0)
                {
                    sum += DELTA;
                    e = (sum >> 2) & 3;
                    for (p = 0; p < n - 1; p++)
                    {
                        y = v[p + 1];
                        MX = (z >> 5 ^ y << 2) + (y >> 3 ^ z << 4) ^ (sum ^ y) + (k[p & 3 ^ e] ^ z);
                        z = v[p] += MX;
                    }
                    y = v[0];
                    GuiLogMessage("y: " + y.ToString("X"), NotificationLevel.Debug);
                    MX = (z >> 5 ^ y << 2) + (y >> 3 ^ z << 4) ^ (sum ^ y) + (k[p & 3 ^ e] ^ z);
                    z = v[n - 1] += MX;
                    GuiLogMessage("z: " + z.ToString("X"), NotificationLevel.Debug);
                }

                GuiLogMessage("v[n-1]: " + v[n - 1].ToString("X"), NotificationLevel.Debug);
                GuiLogMessage("v[0]: " + v[0].ToString("X"), NotificationLevel.Debug);

                return 0;
            }
            else if (n < -1)
            {  /* Decoding Part */
                n = -n;
                q = 6 + 52 / n;
                sum = (uint)q * DELTA;
                while (sum != 0)
                {
                    e = (sum >> 2) & 3;
                    for (p = n - 1; p > 0; p--)
                    {
                        z = v[p - 1];
                        MX = (z >> 5 ^ y << 2) + (y >> 3 ^ z << 4) ^ (sum ^ y) + (k[p & 3 ^ e] ^ z);
                        y = v[p] -= MX;
                    }
                    z = v[n - 1];
                    MX = (z >> 5 ^ y << 2) + (y >> 3 ^ z << 4) ^ (sum ^ y) + (k[p & 3 ^ e] ^ z);
                    y = v[0] -= MX;
                    sum -= DELTA;
                }
                return 0;
            }
            return 1;
        }


        public void Encrypt()
        {
            //Encrypt Stream
            process(0, settings.Padding);
        }

        public void Decrypt()
        {
            //Decrypt Stream
            process(1, settings.Padding);
        }

        public void Initialize()
        {
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public void PostExecution()
        {
            Reset();
        }

        public void PreExecution()
        {
            Reset();
        }

        public UserControl Presentation => null;

        public void Stop()
        {
            stop = true;
        }

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
            /*if (PropertyChanged != null)
            {
              PropertyChanged(this, new PropertyChangedEventArgs(name));
            }*/
        }

        private void StatusChanged(int imageIndex)
        {
            EventsHelper.StatusChanged(OnPluginStatusChanged, this, new StatusEventArgs(StatusChangedMode.ImageUpdate, imageIndex));
        }

        #endregion
    }

    internal enum TEAImage
    {
        Default,
        Encode,
        Decode,
        EncodeX,
        DecodeX
    }
}
