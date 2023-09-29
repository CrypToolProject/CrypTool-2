/*
   Copyright 2008 Sebastian Przybylski, University of Siegen

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
using CrypTool.PluginBase.Control;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace CrypTool.Plugins.Cryptography.Encryption
{
    [Author("Sebastian Przybylski", "sebastian@przybylski.org", "Uni Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("CrypTool.Plugins.Cryptography.Encryption.Properties.Resources", "PluginCaption", "PluginTooltip", "RC2/DetailedDescription/doc.xml", "RC2/icon.png", "RC2/Images/encrypt.png", "RC2/Images/decrypt.png")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class RC2 : ICrypComponent
    {
        #region Private variables
        private RC2Settings settings;
        private ICrypToolStream inputStream;
        private CStreamWriter outputStreamWriter;
        private byte[] inputKey;
        private byte[] inputIV;
        private CryptoStream p_crypto_stream;
        private bool stop = false;
        private IControlEncryption controlSlave;
        #endregion

        public RC2()
        {
            settings = new RC2Settings();
            settings.OnPluginStatusChanged += settings_OnPluginStatusChanged;
        }

        private void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (OnPluginStatusChanged != null)
            {
                OnPluginStatusChanged(this, args);
            }
        }

        public ISettings Settings
        {
            get => settings;
            set => settings = (RC2Settings)value;
        }

        [PropertyInfo(Direction.InputData, "InputStreamCaption", "InputStreamTooltip", true)]
        public ICrypToolStream InputStream
        {
            get => inputStream;
            set => inputStream = value;//wander 20100427: unnecessary, event should've been propagated by editor//OnPropertyChanged("InputStream");
        }

        [PropertyInfo(Direction.InputData, "InputKeyCaption", "InputKeyTooltip", true)]
        public byte[] InputKey
        {
            get => inputKey;
            set
            {
                inputKey = value;
                OnPropertyChanged("InputKey");
            }
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
            get => outputStreamWriter;
            set
            {
                //wander 20100427: unnecessary, propagated by Execute()
                //OnPropertyChanged("OutputStream");
            }
        }

        [PropertyInfo(Direction.ControlSlave, "ControlSlaveCaption", "ControlSlaveTooltip")]
        public IControlEncryption ControlSlave
        {
            get
            {
                if (controlSlave == null)
                {
                    controlSlave = new RC2Control(this);
                }

                return controlSlave;
            }
        }

        private void ConfigureAlg(SymmetricAlgorithm alg)
        {
            //check for a valid key
            if (inputKey == null)
            {
                //create a trivial key 
                inputKey = new byte[16];
                // write a warning to the ouside word
                GuiLogMessage("WARNING - No key provided. Using 0x000..00!", NotificationLevel.Warning);
            }

            try
            {
                alg.Key = inputKey;
            }
            catch (Exception)
            {
                //if alg.Key is set to an "unsecure" key, crappy ms class throws an exception :/
                //so we have to hack in that key value
                FieldInfo keyValue = alg.GetType().GetField("KeyValue", BindingFlags.NonPublic | BindingFlags.Instance);
                keyValue.SetValue(alg, inputKey);
            }
            //check for a valid IV
            if (inputIV == null)
            {
                //create a trivial key 
                inputIV = new byte[alg.BlockSize / 8];
                if (settings.Mode != 0)  // no IV needed for ECB
                {
                    GuiLogMessage("WARNING - No IV provided. Using 0x000..00!", NotificationLevel.Warning);
                }
            }
            alg.IV = inputIV;

            switch (settings.Mode)
            { //0="ECB"=default, 1="CBC", 2="CFB", 3="OFB"
                case 1: alg.Mode = CipherMode.CBC; break;
                case 2: alg.Mode = CipherMode.CFB; break;
                case 3: alg.Mode = CipherMode.ECB; break;
                default: alg.Mode = CipherMode.ECB; break;
            }

            switch (settings.Padding)
            { // 0="None", 1="Zeros"=default, 2="PKCS7", 3="ANSIX923", 4="ISO10126", 5=", 5="1-0-padding"
                case 0: alg.Padding = PaddingMode.None; break;
                case 1: alg.Padding = PaddingMode.Zeros; break;
                case 2: alg.Padding = PaddingMode.PKCS7; break;
                case 3: alg.Padding = PaddingMode.ANSIX923; break;
                case 4: alg.Padding = PaddingMode.ISO10126; break;
                case 5: alg.Padding = PaddingMode.None; break;  // 1-0 padding, use PaddingMode.None, as it's handeled separately
                default: alg.Padding = PaddingMode.Zeros; break;
            }
            alg.Padding = PaddingMode.None;
        }

        public void Execute()
        {
            process(settings.Action);
        }

        private void process(int action)
        {
            //Encrypt/Decrypt Stream
            try
            {
                if (inputStream == null || inputStream.Length == 0)
                {
                    GuiLogMessage("No input data, aborting now", NotificationLevel.Error);
                    return;
                }

                ICryptoTransform p_encryptor;
                SymmetricAlgorithm p_alg = new RC2CryptoServiceProvider();

                ConfigureAlg(p_alg);

                outputStreamWriter = new CStreamWriter();
                ICrypToolStream inputdata = InputStream;

                if (action == 0)
                {
                    inputdata = BlockCipherHelper.AppendPadding(InputStream, settings.padmap[settings.Padding], p_alg.BlockSize / 8);
                }

                CStreamReader reader = inputdata.CreateReader();

                GuiLogMessage("Starting " + ((settings.Action == 0) ? "encryption" : "decryption") + " [Keysize=" + p_alg.KeySize.ToString() + " Bits, Blocksize=" + p_alg.BlockSize.ToString() + " Bits]", NotificationLevel.Info);
                DateTime startTime = DateTime.Now;

                // special handling of OFB mode, as it's not available for RC2 in .Net
                if (settings.Mode == 3)    // OFB - bei OFB ist encrypt = decrypt, daher keine Fallunterscheidung
                {
                    p_encryptor = p_alg.CreateEncryptor(p_alg.Key, p_alg.IV);

                    byte[] IV = new byte[p_alg.IV.Length];
                    Array.Copy(p_alg.IV, IV, p_alg.IV.Length);
                    byte[] tmpInput = BlockCipherHelper.StreamToByteArray(inputdata);
                    byte[] outputData = new byte[tmpInput.Length];

                    for (int pos = 0; pos <= tmpInput.Length - p_encryptor.InputBlockSize;)
                    {
                        int l = p_encryptor.TransformBlock(IV, 0, p_encryptor.InputBlockSize, outputData, pos);
                        for (int i = 0; i < l; i++)
                        {
                            IV[i] = outputData[pos + i];
                            outputData[pos + i] ^= tmpInput[pos + i];
                        }
                        pos += l;
                    }

                    outputStreamWriter.Write(outputData);
                }
                else
                {
                    p_encryptor = (action == 0) ? p_alg.CreateEncryptor() : p_alg.CreateDecryptor();
                    p_crypto_stream = new CryptoStream(reader, p_encryptor, CryptoStreamMode.Read);

                    byte[] buffer = new byte[p_alg.BlockSize / 8];
                    int bytesRead;

                    while ((bytesRead = p_crypto_stream.Read(buffer, 0, buffer.Length)) > 0 && !stop)
                    {
                        outputStreamWriter.Write(buffer, 0, bytesRead);
                    }

                    p_crypto_stream.Flush();
                }

                outputStreamWriter.Close();

                DateTime stopTime = DateTime.Now;
                TimeSpan duration = stopTime - startTime;

                if (action == 1)
                {
                    outputStreamWriter = BlockCipherHelper.StripPadding(outputStreamWriter, settings.padmap[settings.Padding], p_alg.BlockSize / 8) as CStreamWriter;
                }

                if (!stop)
                {
                    GuiLogMessage(((settings.Action == 0) ? "Encryption" : "Decryption") + " complete! (in: " + InputStream.Length + " bytes, out: " + outputStreamWriter.Length + " bytes)", NotificationLevel.Info);
                    GuiLogMessage("Time used: " + duration, NotificationLevel.Debug);
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
                p_crypto_stream = null;
                GuiLogMessage(cryptographicException.Message, NotificationLevel.Error);
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

        public void Encrypt()
        {
            //Encrypt stream
            process(0);
        }

        public void Decrypt()
        {
            //Decrypt Stream
            process(1);
        }

        #region IPlugin Member

        public System.Windows.Controls.UserControl Presentation => null;

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
                outputStreamWriter = null;

                if (p_crypto_stream != null)
                {
                    p_crypto_stream.Flush();
                    p_crypto_stream.Clear();
                    p_crypto_stream = null;
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
            stop = false;
        }

        public void Stop()
        {
            stop = true;
        }

        public void PostExecution()
        {
            Dispose();
        }

        public void PreExecution()
        {
            Dispose();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, logLevel));
            }
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            if (OnPluginProgressChanged != null)
            {
                OnPluginProgressChanged(this, new PluginProgressEventArgs(value, max));
            }
        }

        #endregion
    }


    public class RC2Control : IControlEncryption
    {
        private readonly RC2 _plugin;

        public RC2Control(RC2 rc2)
        {
            _plugin = rc2;
        }

        #region IControlEncryption Members

        public byte[] Encrypt(byte[] plaintext, byte[] key)
        {
            throw new NotImplementedException();
        }

        public byte[] Encrypt(byte[] plaintext, byte[] key, byte[] IV)
        {
            throw new NotImplementedException();
        }

        public byte[] Encrypt(byte[] plaintext, byte[] key, byte[] IV, int bytesToUse)
        {
            throw new NotImplementedException();
        }

        public byte[] Decrypt(byte[] plaintext, byte[] key)
        {
            throw new NotImplementedException();
        }

        public byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] IV)
        {
            return Decrypt(ciphertext, key, IV, ciphertext.Length);
        }

        public byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] IV, int bytesToUse)
        {
            if (IV == null || IV.Length != 8)
            {
                IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            }
            return NativeCryptography.Crypto.decryptRC2(ciphertext, key, IV, bytesToUse, ((RC2Settings)_plugin.Settings).Mode);
        }

        public string GetCipherShortName()
        {
            throw new NotImplementedException();
        }

        public int GetBlockSizeAsBytes()
        {
            return 64;
        }

        public int GetKeySizeAsBytes()
        {
            throw new NotImplementedException();
        }

        public void SetToDefaultSettings()
        {
            throw new NotImplementedException();
        }

        public string GetKeyPattern()
        {
            return string.Join("-", Enumerable.Repeat("[0-9A-F][0-9A-F]", 16));
        }

        public IKeyTranslator GetKeyTranslator()
        {
            return new KeySearcher.KeyTranslators.ByteArrayKeyTranslator();
        }      

        public void ChangeSettings(string setting, object value)
        {
        }

        public IControlEncryption Clone()
        {
            RC2Control control = new RC2Control(_plugin);
            return control;
        }

        public event KeyPatternChanged KeyPatternChanged;

        #endregion

        #region IControl Members

        public event IControlStatusChangedEventHandler OnStatusChanged;

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }
}
