/*
   Copyright 2008-2012 Arno Wacker, University of Kassel

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
using NativeCryptography;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CrypTool.Plugins.Cryptography.Encryption
{
    [Author("Arno Wacker", "arno.wacker@CrypTool.org", "Universität Kassel", "http://www.ais.uni-kassel.de")]
    [PluginInfo("CrypTool.Plugins.Cryptography.Encryption.Properties.Resources", "PluginCaption", "PluginTooltip", "AES/DetailedDescription/doc.xml", "AES/Images/AES.png", "AES/Images/encrypt.png", "AES/Images/decrypt.png", "AES/Images/Rijndael.png")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class AES : ContextBoundObject, ICrypComponent
    {
        #region Private variables
        private AESSettings settings;
        private CStreamWriter outputStreamWriter;
        private byte[] inputKey;
        private byte[] inputIV;
        private CryptoStream p_crypto_stream;
        private bool stop = false;
        #endregion

        public AES()
        {
            settings = new AESSettings();
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
            set => settings = (AESSettings)value;
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
                // empty
            }
        }

        private void ConfigureAlg(SymmetricAlgorithm alg)
        {
            // first set all paramter, then assign input values!
            switch (settings.Mode)
            { //0="ECB"=default, 1="CBC", 2="CFB", 3="OFB"
                case 1: alg.Mode = CipherMode.CBC; break;
                case 2: alg.Mode = CipherMode.CFB; break;
                // case 3: alg.Mode = CipherMode.OFB; break;
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

            if (settings.CryptoAlgorithm == 1)
            {
                switch (settings.Blocksize)
                {
                    case 1: alg.BlockSize = 192; break;
                    case 2: alg.BlockSize = 256; break;
                    default: alg.BlockSize = 128; break;
                }
            }

            //check for a valid key
            if (inputKey == null)
            {
                //create a trivial key 
                inputKey = new byte[16];
                // write a warning to the ouside word
                GuiLogMessage("ERROR: No key provided. Using 0x000..00!", NotificationLevel.Error);
            }

            int keySizeInBytes = (16 + settings.Keysize * 8);

            //prepare a long enough key
            byte[] key = new byte[keySizeInBytes];

            // copy the input key into the temporary key array
            Array.Copy(inputKey, key, Math.Min(inputKey.Length, key.Length));

            // Note: the SymmetricAlgorithm.Key setter clones the passed byte[] array and keeps his own copy
            alg.Key = key;

            if (inputKey.Length > keySizeInBytes)
            {
                GuiLogMessage("Overlength (" + inputKey.Length * 8 + " Bits) key provided. Removing trailing bytes to fit the desired key length of " + (keySizeInBytes * 8) + " Bits: " + bytesToHexString(key), NotificationLevel.Warning);
            }

            if (inputKey.Length < keySizeInBytes)
            {
                GuiLogMessage("Short (" + inputKey.Length * 8 + " Bits) key provided. Adding zero bytes to fill up to the desired key length of " + (keySizeInBytes * 8) + " Bits: " + bytesToHexString(key), NotificationLevel.Warning);
            }

            //check for a valid IV
            if (inputIV == null)
            {
                //create a trivial IV
                inputIV = new byte[alg.BlockSize / 8];
                if (settings.Mode != 0)  // ECB needs no IV, thus no warning if IV misses
                {
                    GuiLogMessage("NOTE: No IV provided. Using 0x000..00!", NotificationLevel.Info);
                }
            }

            alg.IV = inputIV;
        }

        private string bytesToHexString(byte[] array)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in array)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }

        public void Execute()
        {
            process(settings.Action);
        }

        public bool isStopped()
        {
            return stop;
        }

        private void process(int action)
        {
            //Encrypt/Decrypt Stream
            try
            {
                if (InputStream == null || InputStream.Length == 0)
                {
                    GuiLogMessage("No input data, aborting now", NotificationLevel.Error);
                    return;
                }

                SymmetricAlgorithm p_alg = null;
                if (settings.CryptoAlgorithm == 1)
                {
                    p_alg = new RijndaelManaged();
                }
                else
                {
                    p_alg = new AesCryptoServiceProvider();
                }

                ConfigureAlg(p_alg);

                ICryptoTransform p_encryptor = null;
                switch (action)
                {
                    case 0:
                        p_encryptor = p_alg.CreateEncryptor();
                        break;
                    case 1:
                        p_encryptor = p_alg.CreateDecryptor();
                        break;
                }

                outputStreamWriter = new CStreamWriter();

                ICrypToolStream inputdata = InputStream;

                string mode = action == 0 ? "encryption" : "decryption";
                long inbytes, outbytes;

                DateTime startTime = DateTime.Now;

                // special handling of OFB mode, as it's not available for AES in .Net
                if (settings.Mode == 3)    // OFB - bei OFB ist encrypt = decrypt, daher keine Fallunterscheidung
                {
                    if (action == 0 && settings.Padding == 5)
                    {
                        inputdata = BlockCipherHelper.AppendPadding(InputStream, settings.padmap[settings.Padding], p_alg.BlockSize / 8);
                    }

                    ICryptoTransform encrypt = p_alg.CreateEncryptor(p_alg.Key, p_alg.IV);

                    byte[] IV = new byte[p_alg.IV.Length];
                    Array.Copy(p_alg.IV, IV, p_alg.IV.Length);
                    byte[] tmpInput = BlockCipherHelper.StreamToByteArray(inputdata);
                    byte[] outputData = new byte[tmpInput.Length];

                    for (int pos = 0; pos <= tmpInput.Length - encrypt.InputBlockSize;)
                    {
                        int l = encrypt.TransformBlock(IV, 0, encrypt.InputBlockSize, outputData, pos);
                        for (int i = 0; i < l; i++)
                        {
                            IV[i] = outputData[pos + i];
                            outputData[pos + i] ^= tmpInput[pos + i];
                        }
                        pos += l;
                    }

                    int validBytes = (int)inputdata.Length;

                    if (action == 1 && (settings.Padding == 5 || settings.Padding == 1))
                    {
                        validBytes = BlockCipherHelper.StripPadding(outputData, validBytes, settings.padmap[settings.Padding], p_alg.BlockSize / 8);
                    }

                    encrypt.Dispose();
                    outputStreamWriter.Write(outputData, 0, validBytes);
                    inbytes = inputdata.Length;
                }
                else if (settings.Mode == 4)
                {
                    if (action == 0 && settings.Padding == 5)
                    {
                        inputdata = BlockCipherHelper.AppendPadding(InputStream, settings.padmap[settings.Padding], p_alg.BlockSize / 8);
                    }

                    byte[] tmpInput = BlockCipherHelper.StreamToByteArray(inputdata);

                    AesEngine cipher = new AesEngine();
                    EaxBlockCipher eaxCipher = new EaxBlockCipher(cipher);
                    KeyParameter keyParameter = new KeyParameter(p_alg.Key);
                    ParametersWithIV parameter = new ParametersWithIV(keyParameter, p_alg.IV);
                    eaxCipher.Init((action == 0) ? true : false, parameter);

                    byte[] datOut = new byte[eaxCipher.GetOutputSize(tmpInput.Length)];
                    int outOff = eaxCipher.ProcessBytes(tmpInput, 0, tmpInput.Length, datOut, 0);
                    outOff += eaxCipher.DoFinal(datOut, outOff);

                    int validBytes = eaxCipher.GetOutputSize(tmpInput.Length);
                    if (action == 1 && (settings.Padding == 5 || settings.Padding == 1))
                    {
                        validBytes = BlockCipherHelper.StripPadding(datOut, validBytes, settings.padmap[settings.Padding], p_alg.BlockSize / 8);
                    }

                    outputStreamWriter.Write(datOut, 0, validBytes);
                    inbytes = inputdata.Length;
                }
                else
                {
                    // append 1-0 padding (special handling, as it's not present in System.Security.Cryptography.PaddingMode)
                    if (action == 0 && settings.Padding == 5)
                    {
                        inputdata = BlockCipherHelper.AppendPadding(InputStream, settings.padmap[settings.Padding], p_alg.BlockSize / 8);
                    }

                    CStreamReader reader = inputdata.CreateReader();

                    p_crypto_stream = new CryptoStream(reader, p_encryptor, CryptoStreamMode.Read);
                    byte[] buffer = new byte[p_alg.BlockSize / 8];
                    int bytesRead;
                    int position = 0;

                    while ((bytesRead = p_crypto_stream.Read(buffer, 0, buffer.Length)) > 0 && !stop)
                    {
                        // remove 1-0 padding (special handling, as it's not present in System.Security.Cryptography.PaddingMode)
                        if (action == 1 && (settings.Padding == 5 || settings.Padding == 1) && reader.Position == reader.Length)
                        {
                            bytesRead = BlockCipherHelper.StripPadding(buffer, bytesRead, settings.padmap[settings.Padding], buffer.Length);
                        }

                        outputStreamWriter.Write(buffer, 0, bytesRead);

                        if ((int)(reader.Position * 100 / reader.Length) > position)
                        {
                            position = (int)(reader.Position * 100 / reader.Length);
                            ProgressChanged(reader.Position, reader.Length);
                        }
                    }

                    p_crypto_stream.Flush();
                    inbytes = reader.Length;
                }

                outbytes = outputStreamWriter.Length;

                DateTime stopTime = DateTime.Now;
                TimeSpan duration = stopTime - startTime;
                // (outputStream as CrypToolStream).FinishWrite();

                if (!stop)
                {
                    mode = action == 0 ? "Encryption" : "Decryption";
                    outputStreamWriter.Close();
                    OnPropertyChanged("OutputStream");
                }

                if (stop)
                {
                    outputStreamWriter.Close();
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

                string msg = cryptographicException.Message;

                // Workaround for misleading german error message
                if (msg == "Die Zeichenabstände sind ungültig und können nicht entfernt werden.")
                {
                    msg = "Das Padding ist ungültig und kann nicht entfernt werden.";
                }

                GuiLogMessage(msg, NotificationLevel.Error);
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
            //Encrypt Stream
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
            settings.UpdateTaskPaneVisibility();
        }

        public void Dispose()
        {
            try
            {
                stop = false;
                inputKey = null;
                inputIV = null;

                if (outputStreamWriter != null)
                {
                    outputStreamWriter.Dispose();
                    outputStreamWriter = null;
                }

                if (p_crypto_stream != null)
                {
                    p_crypto_stream.Flush();
                    p_crypto_stream.Close();
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
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
            //if (PropertyChanged != null)
            //{
            //  PropertyChanged(this, new PropertyChangedEventArgs(name));
            //}
        }

        #endregion

        #region IPlugin Members

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

        #endregion

        private IControlEncryption controlSlave;
        [PropertyInfo(Direction.ControlSlave, "ControlSlaveCaption", "ControlSlaveTooltip")]
        public IControlEncryption ControlSlave
        {
            get
            {
                if (controlSlave == null)
                {
                    controlSlave = new AESControl(this);
                }

                return controlSlave;
            }
        }
    }

    public class AESControl : IControlEncryption
    {
        public event KeyPatternChanged KeyPatternChanged;
        public event IControlStatusChangedEventHandler OnStatusChanged;

        private readonly AES plugin;
        private readonly AESSettings settings;

        public AESControl(AES plugin)
        {
            this.plugin = plugin;
            settings = (AESSettings)plugin.Settings;
            ((AESSettings)plugin.Settings).PropertyChanged += new PropertyChangedEventHandler(AESControl_PropertyChanged);
        }

        private void AESControl_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Keysize"))
            {
                if (KeyPatternChanged != null)
                {
                    KeyPatternChanged();
                }
            }
        }

        #region IControlEncryption Members

        public byte[] Encrypt(byte[] plaintext, byte[] key)
        {
            // AES or Rijndael
            SymmetricAlgorithm alg;
            if (settings.CryptoAlgorithm == 1)
            {
                alg = new RijndaelManaged();
            }
            else
            {
                alg = new AesCryptoServiceProvider();
            }

            // ECB mode
            alg.Mode = CipherMode.ECB;

            // No padding
            alg.Padding = PaddingMode.None;

            // Blocksize for Rijndael
            if (settings.CryptoAlgorithm == 1)
            {
                switch (settings.Blocksize)
                {
                    case 1: alg.BlockSize = 192; break;
                    case 2: alg.BlockSize = 256; break;
                    default: alg.BlockSize = 128; break;
                }
            }

            // Provide key
            alg.Key = key;

            // Provide the trivial IV (0x0...0)
            alg.IV = new byte[plaintext.Length];

            // Ecnryption only
            ICryptoTransform encryptor = alg.CreateEncryptor();
            MemoryStream input = new MemoryStream(plaintext);
            CryptoStream p_crypto_stream = new CryptoStream(input, encryptor, CryptoStreamMode.Read);

            // Read encrypted bytes from crypto stream.
            byte[] result = new byte[plaintext.Length];
            int offset = 0;
            do
            {
                int bytesRead = p_crypto_stream.Read(result, offset, result.Length);
                offset += bytesRead;
            } while (offset < result.Length);

            return result;
        }

        public byte[] Encrypt(byte[] plaintext, byte[] key, byte[] iv)
        {
            throw new NotImplementedException("This encryption method is not yet implemented!");
        }

        public byte[] Encrypt(byte[] plaintext, byte[] key, byte[] iv, int length)
        {
            throw new NotImplementedException("This encryption method is not yet implemented!");
        }

        public byte[] Decrypt(byte[] ciphertext, byte[] key)
        {
            int bytesToUse = ciphertext.Length;

            switch (settings.KeysizeAsBits)
            {
                case 128:
                    if (Crypto.supportsAESNI())
                    {
                        return Crypto.decryptAES128_ECB_NI(ciphertext, key, bytesToUse);
                    }
                    else
                    {
                        return NativeCryptography.Crypto.decryptAES128_ECB(ciphertext, key, bytesToUse);
                    }

                case 192:
                    if (Crypto.supportsAESNI())
                    {
                        return Crypto.decryptAES192_ECB_NI(ciphertext, key, bytesToUse);
                    }
                    else
                    {
                        return NativeCryptography.Crypto.decryptAES192_ECB(ciphertext, key, bytesToUse);
                    }

                case 256:
                    if (Crypto.supportsAESNI())
                    {
                        return Crypto.decryptAES256_ECB_NI(ciphertext, key, bytesToUse);
                    }
                    else
                    {
                        return NativeCryptography.Crypto.decryptAES256_ECB(ciphertext, key, bytesToUse);
                    }

                default:
                    throw new NotSupportedException(string.Format("Non supported bit size of AES selected: {0}", settings.KeysizeAsBits));
            }
        }

        public byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] iv)
        {
            return Decrypt(ciphertext, key, iv, ciphertext.Length);
        }

        public byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] iv, int length)
        {
            // Don't allow sizes greater than the length of the input
            int bytesToUse = Math.Min(length, ciphertext.Length);
            // Note: new size is assumed to be multiple of 16

            // 0="ECB", 1="CBC", 2="CFB", 3="OFB"
            switch (settings.Mode)
            {
                case 0: // ECB:
                    switch (settings.KeysizeAsBits)
                    {
                        case 128:
                            if (NativeCryptography.Crypto.supportsAESNI())
                            {
                                return NativeCryptography.Crypto.decryptAES128_ECB_NI(ciphertext, key, bytesToUse);
                            }
                            else
                            {
                                return NativeCryptography.Crypto.decryptAES128_ECB(ciphertext, key, bytesToUse);
                            }
                        case 192:
                            if (NativeCryptography.Crypto.supportsAESNI())
                            {
                                return NativeCryptography.Crypto.decryptAES192_ECB_NI(ciphertext, key, bytesToUse);
                            }
                            else
                            {
                                return NativeCryptography.Crypto.decryptAES192_ECB(ciphertext, key, bytesToUse);
                            }
                        case 256:
                            if (NativeCryptography.Crypto.supportsAESNI())
                            {
                                return NativeCryptography.Crypto.decryptAES256_ECB_NI(ciphertext, key, bytesToUse);
                            }
                            else
                            {
                                return NativeCryptography.Crypto.decryptAES256_ECB(ciphertext, key, bytesToUse);
                            }
                        default:
                            throw new NotSupportedException(string.Format("Non supported bit size of AES selected: {0}", settings.KeysizeAsBits));
                    }
                case 1: // CBC:
                    switch (settings.KeysizeAsBits)
                    {
                        case 128:
                            if (NativeCryptography.Crypto.supportsAESNI())
                            {
                                return NativeCryptography.Crypto.decryptAES128_CBC_NI(ciphertext, key, iv, bytesToUse);
                            }
                            else
                            {
                                return NativeCryptography.Crypto.decryptAES128_CBC(ciphertext, key, iv, bytesToUse);
                            }
                        case 192:
                            if (NativeCryptography.Crypto.supportsAESNI())
                            {
                                return NativeCryptography.Crypto.decryptAES192_CBC_NI(ciphertext, key, iv, bytesToUse);
                            }
                            else
                            {
                                return NativeCryptography.Crypto.decryptAES192_CBC(ciphertext, key, iv, bytesToUse);
                            }
                        case 256:
                            if (NativeCryptography.Crypto.supportsAESNI())
                            {
                                return NativeCryptography.Crypto.decryptAES256_CBC_NI(ciphertext, key, iv, bytesToUse);
                            }
                            else
                            {
                                return NativeCryptography.Crypto.decryptAES256_CBC(ciphertext, key, iv, bytesToUse);
                            }
                        default:
                            throw new NotSupportedException(string.Format("Non supported bit size of AES selected: {0}", settings.KeysizeAsBits));
                    }
                case 2: // CFB:
                    switch (settings.KeysizeAsBits)
                    {
                        case 128:
                            return NativeCryptography.Crypto.decryptAES128_CFB(ciphertext, key, iv, bytesToUse);
                        case 192:
                            return NativeCryptography.Crypto.decryptAES192_CFB(ciphertext, key, iv, bytesToUse);
                        case 256:
                            return NativeCryptography.Crypto.decryptAES256_CFB(ciphertext, key, iv, bytesToUse);
                        default:
                            throw new NotSupportedException(string.Format("Non supported bit size of AES selected: {0}", settings.KeysizeAsBits));
                    }
                default:
                    throw new NotSupportedException(string.Format("Non supported mode selected: {0}", settings.Mode));
            }
        }

        public string GetCipherShortName()
        {
            // cryptoAlgorithm = 0; // 0=AES, 1=Rijndael
            return settings.CryptoAlgorithm == 0 ? "AES" : "Rijndael";
        }

        public int GetBlockSizeAsBytes()
        {
            return settings.BlocksizeAsBytes;
        }

        public int GetKeySizeAsBytes()
        {
            return settings.KeysizeAsBytes;
        }

        public string GetKeyPattern()
        {
            return string.Join("-", Enumerable.Repeat("[0-9A-F][0-9A-F]", settings.KeysizeAsBytes));
        }

        public IKeyTranslator GetKeyTranslator()
        {
            return new KeySearcher.KeyTranslators.ByteArrayKeyTranslator();
        }

        public void ChangeSettings(string setting, object value)
        {
            throw new NotImplementedException("Changing the settings is not yet implemented!");
        }

        public IControlEncryption Clone()
        {
            return new AESControl(plugin);
        }

        public void Dispose()
        {

        }

        #endregion
    }
}