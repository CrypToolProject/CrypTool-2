using CrypTool.PluginBase;
using CrypTool.PluginBase.Control;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows.Controls;

namespace CrypTool.Plugins.Cryptography.Encryption
{
    [Author("Arno Wacker", "arno.wacker@CrypTool.org", "Universität Kassel", "http://www.uc.uni-kassel.de")]
    [PluginInfo("CrypTool.Plugins.Cryptography.Encryption.Properties.Resources", "PluginCaption", "PluginTooltip", "DES/DetailedDescription/doc.xml", "DES/icon.png", "DES/Images/encrypt.png", "DES/Images/decrypt.png")]
    [FunctionList("3DES")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class DES : ICrypComponent
    {
        #region Private variables

        private DESSettings settings;
        private byte[] inputKey;
        private byte[] inputIV;
        private CStreamWriter outputStreamWriter;
        private CryptoStream p_crypto_stream;
        private bool stop = false;
        private IControlEncryption controlSlave;

        #endregion

        #region Initialisation

        public DES()
        {
            settings = new DESSettings();
        }

        #endregion

        #region External connection properties

        [PropertyInfo(Direction.ControlSlave, "ControlSlaveCaption", "ControlSlaveTooltip")]
        public IControlEncryption ControlSlave
        {
            get
            {
                if (controlSlave == null)
                {
                    controlSlave = new DESControl(this);
                }

                return controlSlave;
            }
        }

        private IControlCubeAttack desSlave;
        [PropertyInfo(Direction.ControlSlave, "DESSlaveCaption", "DESSlaveTooltip")]
        public IControlCubeAttack DESSlave
        {
            get
            {
                if (desSlave == null)
                {
                    desSlave = new CubeAttackControl(this);
                }

                return desSlave;
            }
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
        public ICrypToolStream OutputStream => outputStreamWriter;

        #endregion       

        #region Public IPlugin Member

        #region Events
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        #endregion

        public ISettings Settings
        {
            get => settings;
            set => settings = (DESSettings)value;
        }

        public UserControl Presentation => null;

        public void Initialize()
        {
        }

        public void Execute()
        {
            process(settings.Action);
        }


        public void Stop()
        {
            stop = true;
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



        #region Public DES specific members

        public bool isStopped()
        {

            return stop;
        }

        #endregion


        #region Private methods

        private void ConfigureAlg(SymmetricAlgorithm alg)
        {
            //check for a valid key

            if (inputKey == null)
            {
                //create a default key 
                inputKey = new byte[settings.TripleDES ? 16 : 8];
                // write a warning to the ouside world
                GuiLogMessage("ERROR: No key provided. Using 0x000..00!", NotificationLevel.Error);
            }

            if (settings.TripleDES)
            {
                if (inputKey.Length != 16 && inputKey.Length != 24)
                {
                    throw new Exception(string.Format("The specified key has a length of {0} bytes. The selected variant TripleDES of the DES algorithm needs a keylength of 16 or 24 bytes.", inputKey.Length));
                }
            }
            else
            {
                if (inputKey.Length != 8)
                {
                    throw new Exception(string.Format("The specified key has a length of {0} bytes. The DES algorithm needs a keylength of 8 bytes.", inputKey.Length));
                }
            }

            try
            {
                alg.Key = inputKey;
            }
            catch
            {
                //dirty hack to allow weak keys:
                FieldInfo field = alg.GetType().GetField("KeyValue", BindingFlags.NonPublic | BindingFlags.Instance);
                field.SetValue(alg, inputKey);
            }

            //check for a valid IV

            int bsize = alg.BlockSize / 8;
            if (inputIV == null)
            {
                //create a default IV
                inputIV = new byte[bsize];
                if (settings.Mode != 0)  // ECB needs no IV, thus no warning if IV misses
                {
                    GuiLogMessage("NOTE: No IV provided. Using 0x000..00!", NotificationLevel.Info);
                }
            }

            if (inputIV.Length != bsize)
            {
                if (settings.TripleDES)
                {
                    throw new Exception(string.Format("The specified IV has a length of {0} bytes. The selected variant TripleDES of the DES algorithm needs an IV of {1} bytes.", inputIV.Length, bsize));
                }
                else
                {
                    throw new Exception(string.Format("The specified IV has a length of {0} bytes. The DES algorithm needs an IV of {1} bytes.", inputIV.Length, bsize));
                }
            }

            alg.IV = inputIV;

            switch (settings.Mode)
            { // 0="ECB"=default, 1="CBC", 2="CFB", 3="OFB"
                case 1: alg.Mode = CipherMode.CBC; break;
                case 2: alg.Mode = CipherMode.CFB; break;
                case 3: alg.Mode = CipherMode.ECB; break;
                default: alg.Mode = CipherMode.ECB; break;
            }

            switch (settings.Padding)
            { // 0="None", 1="Zeros"=default, 2="PKCS7", 3="ANSIX923", 4="ISO10126", 5=", 5="1-0-padding"
                case 0: alg.Padding = PaddingMode.None; break;
                case 2: alg.Padding = PaddingMode.PKCS7; break;
                case 3: alg.Padding = PaddingMode.ANSIX923; break;
                case 4: alg.Padding = PaddingMode.ISO10126; break;
                case 5: alg.Padding = PaddingMode.None; break;  // 1-0 padding, use PaddingMode.None, as it's handeled separately
                default: alg.Padding = PaddingMode.Zeros; break;
            }
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

                ICryptoTransform p_encryptor;
                SymmetricAlgorithm p_alg = null;

                if (settings.TripleDES)
                {
                    p_alg = new TripleDESCryptoServiceProvider();
                }
                else
                {
                    p_alg = new DESCryptoServiceProvider();
                }

                ConfigureAlg(p_alg);

                outputStreamWriter = new CStreamWriter();
                ICrypToolStream inputdata = InputStream;

                // append 1-0 padding (special handling, as it's not present in System.Security.Cryptography.PaddingMode)
                if (action == 0)
                {
                    inputdata = BlockCipherHelper.AppendPadding(InputStream, settings.padmap[settings.Padding], p_alg.BlockSize / 8);
                }

                CStreamReader reader = inputdata.CreateReader();

                //GuiLogMessage("Starting encryption [Keysize=" + p_alg.KeySize.ToString() + " Bits, Blocksize=" + p_alg.BlockSize.ToString() + " Bits]", NotificationLevel.Info);
                DateTime startTime = DateTime.Now;

                // special handling of OFB mode, as it's not available for DES in .Net
                if (settings.Mode == 3)    // OFB - bei OFB ist encrypt = decrypt, daher keine Fallunterscheidung
                {
                    try
                    {
                        p_encryptor = p_alg.CreateEncryptor(p_alg.Key, p_alg.IV);
                    }
                    catch
                    {
                        //dirty hack to allow weak keys:
                        MethodInfo mi = p_alg.GetType().GetMethod("_NewEncryptor", BindingFlags.NonPublic | BindingFlags.Instance);
                        object[] Par = { p_alg.Key, p_alg.Mode, p_alg.IV, p_alg.FeedbackSize, 0 };
                        p_encryptor = mi.Invoke(p_alg, Par) as ICryptoTransform;
                    }

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
                    try
                    {
                        p_encryptor = (action == 0) ? p_alg.CreateEncryptor() : p_alg.CreateDecryptor();
                    }
                    catch
                    {
                        //dirty hack to allow weak keys:
                        MethodInfo mi = (action == 0) ? p_alg.GetType().GetMethod("_NewEncryptor", BindingFlags.NonPublic | BindingFlags.Instance)
                            : p_alg.GetType().GetMethod("_NewDecryptor", BindingFlags.NonPublic | BindingFlags.Instance); ;
                        object[] par = { p_alg.Key, p_alg.Mode, p_alg.IV, p_alg.FeedbackSize, 0 };
                        p_encryptor = mi.Invoke(p_alg, par) as ICryptoTransform;
                    }
                    p_crypto_stream = new CryptoStream(reader, p_encryptor, CryptoStreamMode.Read);

                    byte[] buffer = new byte[p_alg.BlockSize / 8];
                    int bytesRead;

                    while ((bytesRead = p_crypto_stream.Read(buffer, 0, buffer.Length)) > 0 && !stop)
                    {
                        //// remove 1-0 padding (special handling, as it's not present in System.Security.Cryptography.PaddingMode)
                        //if (action == 1 && settings.Padding == 5 && reader.Position == reader.Length)
                        //    bytesRead = BlockCipherHelper.StripPadding(buffer, bytesRead, BlockCipherHelper.PaddingType.OneZeros, buffer.Length);

                        outputStreamWriter.Write(buffer, 0, bytesRead);
                    }

                    p_crypto_stream.Flush();
                }

                outputStreamWriter.Close();

                //DateTime stopTime = DateTime.Now;
                //TimeSpan duration = stopTime - startTime;

                if (action == 1)
                {
                    outputStreamWriter = BlockCipherHelper.StripPadding(outputStreamWriter, settings.padmap[settings.Padding], p_alg.BlockSize / 8) as CStreamWriter;
                }

                if (!stop)
                {
                    //GuiLogMessage("Encryption complete! (in: " + reader.Length.ToString() + " bytes, out: " + outputStreamWriter.Length.ToString() + " bytes)", NotificationLevel.Info);
                    //GuiLogMessage("Time used: " + duration.ToString(), NotificationLevel.Debug);
                    OnPropertyChanged("OutputStream");
                }
                else
                {
                    GuiLogMessage("Aborted!", NotificationLevel.Info);
                }
            }
            catch (CryptographicException cryptographicException)
            {
                p_crypto_stream = null;
                string msg = cryptographicException.Message;

                // Workaround for misleading padding error message
                if (msg.StartsWith("Ungültige Daten"))
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

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, logLevel));
            }
        }

        private void ProgressChanged(double value, double max)
        {
            if (OnPluginProgressChanged != null)
            {
                OnPluginProgressChanged(this, new PluginProgressEventArgs(value, max));
            }
        }

        #endregion
    }

    #region DESControl : IControlCubeAttack

    public class CubeAttackControl : IControlCubeAttack
    {
        public event IControlStatusChangedEventHandler OnStatusChanged;
        private readonly DES plugin;

        public CubeAttackControl(DES Plugin)
        {
            plugin = Plugin;
        }

        #region IControlEncryption Members

        public int GenerateBlackboxOutputBit(int[] IV, int[] key, int length)
        {
            // public bits := plaintext
            // secret bits := key 
            SymmetricAlgorithm p_alg = new DESCryptoServiceProvider();
            string secretBits = string.Empty;
            string publicBits = string.Empty;

            // save public and secret bits as string
            for (int i = 0; i < key.Length; i++)
            {
                secretBits += key[i];
            }

            for (int i = 0; i < IV.Length; i++)
            {
                publicBits += IV[i];
            }

            // convert secret bits to byte array
            int[] arrInt = new int[8];
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (secretBits[(8 * i) + j] == '1')
                    {
                        arrInt[i] += (int)Math.Pow(2, 7 - j);
                    }
                }
            }
            byte[] keyByte = new byte[8];
            for (int i = 0; i < arrInt.Length; i++)
            {
                keyByte[i] = (byte)arrInt[i];
            }

            // convert public bits to byte array
            arrInt = new int[8];
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (publicBits[(8 * i) + j] == '1')
                    {
                        arrInt[i] += (int)Math.Pow(2, 7 - j);
                    }
                }
            }
            byte[] publicByte = new byte[8];
            for (int i = 0; i < arrInt.Length; i++)
            {
                publicByte[i] = (byte)arrInt[i];
            }

            ICryptoTransform p_encryptor;
            p_alg.IV = new byte[8];
            p_alg.Padding = PaddingMode.Zeros;
            try
            {
                p_alg.Key = keyByte;
            }
            catch
            {
                //dirty hack to allow weak keys:
                FieldInfo field = p_alg.GetType().GetField("KeyValue", BindingFlags.NonPublic | BindingFlags.Instance);
                field.SetValue(p_alg, keyByte);
            }
            try
            {
                p_encryptor = p_alg.CreateEncryptor();
            }
            catch
            {
                //dirty hack to allow weak keys:
                MethodInfo mi = p_alg.GetType().GetMethod("_NewEncryptor", BindingFlags.NonPublic | BindingFlags.Instance);
                object[] Par = { p_alg.Key, p_alg.Mode, p_alg.IV, p_alg.FeedbackSize, 0 };
                p_encryptor = mi.Invoke(p_alg, Par) as ICryptoTransform;
            }

            Stream inputPublic = new MemoryStream(publicByte);
            // starting encryption
            CryptoStream p_crypto_stream = new CryptoStream(inputPublic, p_encryptor, CryptoStreamMode.Read);
            byte[] buffer = new byte[p_alg.BlockSize / 8];
            p_crypto_stream.Read(buffer, 0, buffer.Length);

            // convert encrypted block to binary string
            string strBytes = string.Empty;
            for (int i = 0; i < buffer.Length; i++)
            {
                for (int j = 7; j >= 0; j--)
                {
                    strBytes += (buffer[i] & 1 << j) > 0 ? 1 : 0;
                }
            }
            p_crypto_stream.Flush();

            // return single output bit
            return int.Parse(strBytes.Substring(length - 1, 1));
        }
        #endregion
    }

    #endregion


    #region DESControl : IControlEncryption
    public class DESControl : IControlEncryption
    {
        public event KeyPatternChanged KeyPatternChanged;
        public event IControlStatusChangedEventHandler OnStatusChanged;

        private readonly DES plugin;

        public DESControl(DES Plugin)
        {
            plugin = Plugin;

            // Change the padding mode to zeroes, since we want to do bruteforcing..
            ((DESSettings)plugin.Settings).Padding = 1;

            ((DESSettings)plugin.Settings).PropertyChanged += new PropertyChangedEventHandler(DESControl_PropertyChanged);
        }

        private void DESControl_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("TripleDES"))
            {
                if (KeyPatternChanged != null)
                {
                    KeyPatternChanged();
                }
            }
        }

        public byte[] Encrypt(byte[] plaintext, byte[] key)
        {
            throw new NotImplementedException();
        }

        public byte[] Encrypt(byte[] plaintext, byte[] key, byte[] iv)
        {
            return Encrypt(plaintext, key, iv, plaintext.Length);
        }

        public byte[] Encrypt(byte[] plaintext, byte[] key, byte[] iv, int bytesToUse)
        {
            throw new NotImplementedException();
        }

        public byte[] Decrypt(byte[] plaintext, byte[] key)
        {
            throw new NotImplementedException();
        }

        public byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] iv)
        {
            return Decrypt(ciphertext, key, iv, ciphertext.Length);
        }

        // TODO: add override with iv, mode, blocksize
        public byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] iv, int bytesToUse)
        {
            int size = bytesToUse > ciphertext.Length ? ciphertext.Length : bytesToUse;

            if (((DESSettings)plugin.Settings).TripleDES)
            {
                //0="ECB", 1="CBC", 2="CFB", 3="OFB"
                switch (((DESSettings)plugin.Settings).Mode)
                {
                    case 0: //ECB
                        return NativeCryptography.Crypto.decrypt3DES_ECB(ciphertext, key, size);
                    case 1: //CBC
                        return NativeCryptography.Crypto.decrypt3DES_CBC(ciphertext, key, iv, size);
                    case 2: //CFB
                        return NativeCryptography.Crypto.decrypt3DES_CFB(ciphertext, key, iv, size);
                    default:
                        throw new NotSupportedException(string.Format("Non supported mode selected: {0}", ((DESSettings)plugin.Settings).Mode));
                }
            }
            else
            {
                //0="ECB", 1="CBC", 2="CFB", 3="OFB"
                switch (((DESSettings)plugin.Settings).Mode)
                {
                    case 0: //ECB
                        return NativeCryptography.Crypto.decryptDES_ECB(ciphertext, key, size);
                    case 1: //CBC
                        return NativeCryptography.Crypto.decryptDES_CBC(ciphertext, key, iv, size);
                    case 2: //CFB
                        return NativeCryptography.Crypto.decryptDES_CFB(ciphertext, key, iv, size);
                    default:
                        throw new NotSupportedException(string.Format("Non supported mode selected: {0}", ((DESSettings)plugin.Settings).Mode));
                }
            }
        }

        public string GetCipherShortName()
        {
            throw new NotImplementedException();
        }

        public int GetBlockSizeAsBytes()
        {
            return 8;
        }

        public int GetKeySizeAsBytes()
        {
            throw new NotImplementedException();
        }

        public string GetKeyPattern()
        {
            int bytes = ((DESSettings)plugin.Settings).TripleDES ? 16 : 8;
            return string.Join("-", Enumerable.Repeat("[0-9A-F][0-9A-F]", bytes));
        }

        public IControlEncryption Clone()
        {
            DESControl des = new DESControl(plugin);
            return des;
        }

        public void Dispose()
        {
        }

        public void ChangeSettings(string setting, object value)
        {
            throw new NotImplementedException();
        }

        public IKeyTranslator GetKeyTranslator()
        {
            return new KeySearcher.KeyTranslators.ByteArrayKeyTranslator();
        }

        public void SetToDefaultSettings()
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}
