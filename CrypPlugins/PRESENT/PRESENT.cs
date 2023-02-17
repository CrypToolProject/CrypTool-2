/*
   Copyright 2008 Timm Korte, University of Siegen

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

// CrypTool PRESENT Plugin
// Author: Timm Korte, CrypTool@easycrypt.de
// PRESENT information: http://www.crypto.rub.de/imperia/md/content/texte/publications/conferences/present_ches2007.pdf

using CrypTool.PluginBase;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;

namespace CrypTool.PRESENT
{
    [Author("Timm Korte", "CrypTool@easycrypt.de", "Uni Bochum", "http://www.ruhr-uni-bochum.de")]
    [PluginInfo("CrypTool.PRESENT.Properties.Resources", "PluginCaption", "PluginTooltip", "PRESENT/DetailedDescription/doc.xml", "PRESENT/icon.png", "PRESENT/Images/encrypt.png", "PRESENT/Images/decrypt.png")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]    
    public class PRESENT : ICrypComponent
    {
        #region Private variables
        private PRESENTSettings settings;
        private readonly PRESENTAnimation presentation;
        private ICrypToolStream inputStream;
        private CStreamWriter outputStream;
        private byte[] inputKey;
        private byte[] inputIV;
        private CryptoStream p_crypto_stream_enc;
        private CryptoStream p_crypto_stream_dec;
        private bool stop;
        #endregion


        public PRESENT()
        {
            presentation = new PRESENTAnimation();
            settings = new PRESENTSettings();
        }

        public ISettings Settings
        {

            get => settings;

            set => settings = (PRESENTSettings)value;
        }

        [PropertyInfo(Direction.InputData, "InputStreamCaption", "InputStreamTooltip", true)]
        public ICrypToolStream InputStream
        {

            get => inputStream;

            set
            {
                if (value != inputStream)
                {
                    inputStream = value;
                    OnPropertyChanged("InputStream");
                }
            }
        }

        [PropertyInfo(Direction.InputData, "InputKeyCaption", "InputKeyTooltip", true)]
        public byte[] InputKey
        {

            get => inputKey;

            set
            {
                if (value != inputKey)
                {
                    inputKey = value;
                    // OnPropertyChanged("InputKey");
                }
            }
        }

        [PropertyInfo(Direction.InputData, "InputIVCaption", "InputIVTooltip", false)]
        public byte[] InputIV
        {

            get => inputIV;

            set
            {
                if (value != inputIV)
                {
                    inputIV = value;
                    OnPropertyChanged("InputIV");
                }
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

        private void ConfigureAlg(SymmetricAlgorithm alg, bool encrypt)
        {
            switch (settings.Mode)
            { // 0="ECB"=default, 1="CBC", 2="CFB", 3="OFB"
                case 1: alg.Mode = CipherMode.CBC; break;
                case 2: alg.Mode = CipherMode.CFB; break;
                case 3: alg.Mode = CipherMode.OFB; break;
                default: alg.Mode = CipherMode.ECB; break;
            }
            switch (settings.Padding)
            { //0="Zeros"=default, 1="None", 2="PKCS7" , 3="ANSIX923", 4="ISO10126", 5="1-0-padding"
                case 0: alg.Padding = PaddingMode.None; break;
                case 1: alg.Padding = PaddingMode.Zeros; break;
                case 2: alg.Padding = PaddingMode.PKCS7; break;
                case 3: alg.Padding = PaddingMode.ANSIX923; break;
                case 4: alg.Padding = PaddingMode.ISO10126; break;
                case 5: alg.Padding = PaddingMode.None; break;  // 1-0 padding, use PaddingMode.None, as it's handeled separately
                default: alg.Padding = PaddingMode.Zeros; break;
            }
            alg.Padding = PaddingMode.None;

            // Check input data
            if (inputStream == null)
            { // no input connected
                GuiLogMessage("ERROR - No input data provided", NotificationLevel.Error);
            }
            else if ((inputStream.Length % (alg.BlockSize >> 3)) != 0)
            {
                if (!encrypt)
                { // when decrypting, input size must be multiple of blocksize
                    GuiLogMessage("ERROR - When decrypting, the input length (" + inputStream.Length + " bytes) has to be a multiple of the blocksize (n*" + (alg.BlockSize >> 3) + " bytes).", NotificationLevel.Error);
                }
            }

            // Check Key
            if (inputKey == null)
            { //key is required, "null" is an Error
                GuiLogMessage("ERROR - No key provided", NotificationLevel.Error);
                GuiLogMessage("WARNING - No key provided. Using 0x000..00!", NotificationLevel.Warning);
                inputKey = new byte[10];
            }
            else if ((inputKey.Length != 10) && (inputKey.Length != 16))
            { // invalid key length
                GuiLogMessage("ERROR - Invalid key length (" + inputKey.Length + " bytes), must be 10 or 16 bytes", NotificationLevel.Error);
                inputKey = new byte[10];
            }
            alg.Key = inputKey;

            // Check IV
            if (inputIV == null)
            { // IV might be optional, "null" = none given
                if (alg.Mode != CipherMode.ECB)
                { // if not using ECB, we need an IV -> generate default 0x00...0
                    GuiLogMessage("WARNING - No IV for chaining mode (" + alg.Mode.ToString() + ") provided. Using 0x000..00!", NotificationLevel.Warning);
                }
                inputIV = new byte[8];
            }
            else if (inputIV.Length != 8)
            { // invalid IV length
                GuiLogMessage("ERROR - Invalid IV length (" + inputIV.Length + " bytes), must be 8 bytes", NotificationLevel.Error);
                inputIV = new byte[8];
            }
            alg.IV = inputIV;
        }


        private string getHex(byte[] p)
        {
            StringBuilder strHex = new StringBuilder();
            for (int i = 0; i < p.Length; i++)
            {
                strHex.Append(p[i].ToString("X2"));
            }
            return strHex.ToString();
        }

        public void Encrypt()
        {
            if (inputStream != null)
            {
                // Encrypt Stream
                try
                {
                    SymmetricAlgorithm p_alg = new PresentManaged();
                    ConfigureAlg(p_alg, true);

                    ICrypToolStream inputdata = InputStream;

                    inputdata = BlockCipherHelper.AppendPadding(InputStream, settings.padmap[settings.Padding], p_alg.BlockSize / 8);

                    CStreamReader reader = inputdata.CreateReader();

                    if ((presentation != null) & (p_alg.KeySize == 80))
                    {
                        byte[] block = new byte[8];
                        byte[] key = (byte[])p_alg.Key.Clone();
                        int r = reader.Read(block, 0, 8);
                        if (reader.CanSeek)
                        {
                            reader.Position = 0;
                        }

                        if (r < 8)
                        {
                            for (int i = 0; i < r; i++)
                            {
                                block[7 - i] = block[r - i - 1];
                            }
                            byte p;
                            if (p_alg.Padding == PaddingMode.PKCS7) { p = (byte)(8 - r); } else { p = 0; }
                            for (int i = 0; i < 8 - r; i++)
                            {
                                block[i] = p;
                            }
                        }
                        presentation.Assign_Values(key, block);
                    }

                    ICryptoTransform p_encryptor = p_alg.CreateEncryptor();

                    outputStream = new CStreamWriter();
                    p_crypto_stream_enc = new CryptoStream(reader, p_encryptor, CryptoStreamMode.Read);
                    byte[] buffer = new byte[p_alg.BlockSize / 8];
                    int bytesRead;
                    int position = 0;
                    DateTime startTime = DateTime.Now;
                    while ((bytesRead = p_crypto_stream_enc.Read(buffer, 0, buffer.Length)) > 0 && !stop)
                    {
                        outputStream.Write(buffer, 0, bytesRead);
                        if ((int)(reader.Position * 100 / inputStream.Length) > position)
                        {
                            position = (int)(reader.Position * 100 / inputStream.Length);
                            Progress(reader.Position, inputStream.Length);
                        }
                    }
                    p_crypto_stream_enc.Flush();
                    // p_crypto_stream_enc.Close();        

                    DateTime stopTime = DateTime.Now;
                    TimeSpan duration = stopTime - startTime;

                    outputStream.Close();

                    if (!stop)
                    {
                        GuiLogMessage("Encryption complete! (in: " + inputStream.Length.ToString() + " bytes, out: " + outputStream.Length.ToString() + " bytes)", NotificationLevel.Info);
                        GuiLogMessage("Time used: " + duration.ToString(), NotificationLevel.Debug);
                        OnPropertyChanged("OutputStream");
                    }
                    else
                    {
                        GuiLogMessage("Aborted!", NotificationLevel.Info);
                    }
                }
                catch (CryptographicException cryptographicException)
                {
                    // TODO: For an unknown reason p_crypto_stream cannot be closed after exception.
                    // Trying so makes p_crypto_stream throw the same exception again. So in Dispose 
                    // the error messages will be doubled. 
                    // As a workaround we set p_crypto_stream to null here.
                    p_crypto_stream_enc = null;
                    GuiLogMessage(cryptographicException.Message, NotificationLevel.Error);
                }
                catch (Exception exception)
                {
                    GuiLogMessage(exception.Message, NotificationLevel.Error);
                }
                finally
                {
                    Progress(1, 1);
                }
            }
        }

        public void Decrypt()
        {
            if (inputStream != null)
            {
                // Decrypt Stream
                try
                {
                    SymmetricAlgorithm p_alg = new PresentManaged();
                    ConfigureAlg(p_alg, true);

                    ICrypToolStream inputdata = InputStream;

                    CStreamReader reader = inputdata.CreateReader();

                    ICryptoTransform p_decryptor = p_alg.CreateDecryptor();
                    outputStream = new CStreamWriter();
                    p_crypto_stream_dec = new CryptoStream(reader, p_decryptor, CryptoStreamMode.Read);
                    byte[] buffer = new byte[p_alg.BlockSize / 8];
                    int bytesRead;
                    int position = 0;
                    DateTime startTime = DateTime.Now;

                    while ((bytesRead = p_crypto_stream_dec.Read(buffer, 0, buffer.Length)) > 0 && !stop)
                    {
                        outputStream.Write(buffer, 0, bytesRead);
                        if ((int)(reader.Position * 100 / inputStream.Length) > position)
                        {
                            position = (int)(reader.Position * 100 / inputStream.Length);
                            Progress(reader.Position, inputStream.Length);
                        }
                    }

                    p_crypto_stream_dec.Flush();
                    p_crypto_stream_dec.Close();

                    DateTime stopTime = DateTime.Now;
                    TimeSpan duration = stopTime - startTime;

                    outputStream.Close();

                    if (settings.Action == 1)
                    {
                        outputStream = BlockCipherHelper.StripPadding(outputStream, settings.padmap[settings.Padding], p_alg.BlockSize / 8) as CStreamWriter;
                    }

                    if (!stop)
                    {
                        GuiLogMessage("Decryption complete! (in: " + inputStream.Length.ToString() + " bytes, out: " + outputStream.Length.ToString() + " bytes)", NotificationLevel.Info);
                        GuiLogMessage("Time used: " + duration.ToString(), NotificationLevel.Debug);
                        OnPropertyChanged("OutputStream");
                    }
                    else
                    {
                        GuiLogMessage("Aborted!", NotificationLevel.Info);
                    }
                }
                catch (CryptographicException cryptographicException)
                {
                    // TODO: For an unknown reason p_crypto_stream cannot be closed after exception.
                    // Trying so makes p_crypto_stream throw the same exception again. So in Dispose 
                    // the error messages will be doubled. 
                    // As a workaround we set p_crypto_stream to null here.
                    p_crypto_stream_dec = null;
                    GuiLogMessage(cryptographicException.Message, NotificationLevel.Error);
                }
                catch (Exception exception)
                {
                    GuiLogMessage(exception.Message, NotificationLevel.Error);
                }
                finally
                {
                    Progress(1, 1);
                }
            }
        }

        #region IPlugin Member

#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
#pragma warning restore


        public System.Windows.Controls.UserControl Presentation => presentation;

        public void Initialize()
        {

        }

        public void Dispose()
        {
            try
            {
                stop = false;
                inputKey = null;
                inputIV = null;
                inputStream = null;
                outputStream = null;

                if (p_crypto_stream_dec != null)
                {
                    p_crypto_stream_dec.Flush();
                    p_crypto_stream_dec.Close();
                    p_crypto_stream_dec = null;
                }

                if (p_crypto_stream_enc != null)
                {
                    p_crypto_stream_enc.Flush();
                    p_crypto_stream_enc.Close();
                    p_crypto_stream_enc = null;
                }
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }

            stop = false;
        }

        public void Stop()
        {
            stop = true;
        }

        public void PreExecution()
        {
            Dispose();
            //stop = false;
            //inputIV = null;
        }

        public void PostExecution()
        {
            Dispose();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
            //if (PropertyChanged != null)
            //{
            //  PropertyChanged(this, new PropertyChangedEventArgs(name));
            //}
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
            //if (OnGuiLogNotificationOccured != null)
            //{
            //  OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, logLevel));
            //}
        }

        private void Progress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion

        #region IPlugin Members


        public void Execute()
        {
            switch (settings.Action)
            {
                case 0:
                    Encrypt();
                    break;
                case 1:
                    Decrypt();
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}
