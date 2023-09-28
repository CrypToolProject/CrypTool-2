/*                              
   Copyright 2009 Team CrypTool (Sven Rech,Dennis Nolte,Raoul Falk,Nils Kopal), Uni Duisburg-Essen

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
using KeySearcher.KeyPattern;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.Plugins.Cryptography.Encryption
{
    /// <summary>
    /// This plugin encrypts / decrypts texts with the simplified DES alogrithm (SDES)
    /// It can be used as plugin in a normal encryption/decryption chanin or be 
    /// used by the KeySearcher to do bruteforcing
    /// </summary>
    [Author("Nils Kopal", "nils.kopal@CrypTool.de", "Uni Duisburg", "http://www.uni-duisburg-essen.de")]
    [PluginInfo("CrypTool.Plugins.Cryptography.Encryption.Properties.Resources", "PluginCaption", "PluginTooltip", "SDES/DetailedDescription/doc.xml", "SDES/icon.png", "SDES/Images/encrypt.png", "SDES/Images/decrypt.png")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class SDES : ICrypComponent
    {
        #region Private variables

        private SDESSettings settings;
        private ICrypToolStream inputStream;
        private CStreamWriter outputStream;
        private byte[] inputKey;
        //default inputIV: (0,0,0,0,0,0,0,0)
        private byte[] inputIV = {   (byte)'0',
                                     (byte)'0',
                                     (byte)'0',
                                     (byte)'0',
                                     (byte)'0',
                                     (byte)'0',
                                     (byte)'0',
                                     (byte)'0'   };
        private bool stop = false;
        private readonly UserControl presentation = new SDESPresentation();
        private SDESControl controlSlave;

        #endregion

        #region events

        public event PropertyChangedEventHandler PropertyChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        #endregion

        #region public

        /// <summary>
        /// Tells you wether input changed or not
        /// </summary>
        public bool InputChanged
        { get; set; }

        /// <summary>
        /// Constructs a new SDES
        /// </summary>
        public SDES()
        {
            InputChanged = false;
            settings = new SDESSettings();
            settings.OnPluginStatusChanged += settings_OnPluginStatusChanged;
        }

        /// <summary>        
        /// The status of the plugin changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (OnPluginStatusChanged != null)
            {
                OnPluginStatusChanged(this, args);
            }
        }

        /// <summary>
        /// Sets/Gets the settings of this plugin
        /// </summary>
        public ISettings Settings
        {
            get => settings;
            set => settings = (SDESSettings)value;
        }

        /// <summary>
        /// Is this Plugin in Status stop?
        /// </summary>
        /// <returns></returns>
        public bool getStop()
        {
            return stop;
        }

        /// <summary>
        /// Gets/Sets the input of the SDES plugin (the text which should be encrypted/decrypted)
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputStreamCaption", "InputStreamTooltip", true)]
        public ICrypToolStream InputStream
        {
            get => inputStream;
            set
            {
                inputStream = value;
                OnPropertyChanged("InputStream");
            }
        }

        /// <summary>
        /// Gets/Sets the output of the SDES plugin (the text which is encrypted/decrypted)
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputStreamCaption", "OutputStreamTooltip", true)]
        public ICrypToolStream OutputStream
        {
            get => outputStream;
            set
            {
            }
        }

        /// <summary>
        /// Gets/Sets the key which should be used.Must be 10 bytes  (only 1 or 0 allowed).
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputKeyCaption", "InputKeyTooltip", true)]
        public byte[] InputKey
        {
            get => inputKey;
            set
            {
                InputChanged = true;
                inputKey = value;
                OnPropertyChanged("InputKey");
            }
        }

        /// <summary>
        /// Gets/Sets the Initialization Vector which should be used.Must be 8 bytes  (only 1 or 0 allowed).
        /// </summary>
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

        /// <summary>
        /// Start encrypting
        /// </summary>
        public void Encrypt()
        {
            //Encrypt Stream
            process(0);
        }

        /// <summary>
        /// Start decrypting
        /// </summary>
        public void Decrypt()
        {
            //Decrypt Stream
            process(1);
        }

        /// <summary>
        /// Called by the environment to start this plugin
        /// </summary>
        public void Execute()
        {
            process(settings.Action);
        }


        /// <summary>
        /// Get the Presentation of this plugin
        /// </summary>
        public UserControl Presentation => presentation;

        /// <summary>
        /// Called by the environment to do initialization
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called by the envorinment if this plugin is unloaded
        /// closes all streams
        /// </summary>
        public void Dispose()
        {
            try
            {
                stop = false;
                inputKey = null;
                //default inputIV: (0,0,0,0,0,0,0,0)
                inputIV = new byte[]{   (byte)'0',
                                             (byte)'0',
                                             (byte)'0',
                                             (byte)'0',
                                             (byte)'0',
                                             (byte)'0',
                                             (byte)'0',
                                             (byte)'0'   };
                if (outputStream != null)
                {
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

        /// <summary>
        /// Called by the environment of this plugin to stop it
        /// </summary>
        public void Stop()
        {
            stop = true;
        }

        /// <summary>
        /// Called by the environment of this plugin after execution
        /// </summary>
        public void PostExecution()
        {
            Dispose();
        }

        /// <summary>
        /// Called by the environment of this plugin before execution
        /// </summary>
        public void PreExecution()
        {
            Dispose();
        }

        /// <summary>
        /// A property of this plugin changed
        /// </summary>
        /// <param name="name">propertyname</param>
        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        /// <summary>
        /// Logs a message into the messages of crypttool
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="logLevel">logLevel</param>
        public void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, logLevel));
            }
        }

        /// <summary>
        /// Sets the current progess of this plugin
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="max">max</param>
        public void ProgressChanged(double value, double max)
        {
            if (OnPluginProgressChanged != null)
            {
                OnPluginProgressChanged(this, new PluginProgressEventArgs(value, max));
            }
        }

        /// <summary>
        /// Sets/Gets the ControlSlave of this plugin
        /// </summary>
        [PropertyInfo(Direction.ControlSlave, "ControlSlaveCaption", "ControlSlaveTooltip")]
        public IControlEncryption ControlSlave
        {
            get
            {
                if (controlSlave == null)
                {
                    controlSlave = new SDESControl(this);
                }

                return controlSlave;
            }
        }

        #endregion public

        #region private

        /// <summary>
        /// Checks if the Input key is not null and has length 10 and only contains 1s and 0s
        /// and if Input IV is not null and has length 8 and only contains 1s and 0s
        /// </summary>
        /// <returns>true if ok</returns>
        private bool areKeyAndIVValid()
        {

            if (inputKey == null || inputKey.Length != 10)
            {
                GuiLogMessage("The Key has to have the length of 10 bytes (containing only '1' and '0')", NotificationLevel.Error);
                return false;
            }
            if (inputIV == null || inputIV.Length != 8)
            {
                GuiLogMessage("The IV has to have the length of 8 bytes (containing only '1' and '0')", NotificationLevel.Error);
                return false;
            }

            foreach (char character in inputKey)
            {
                if (character != '0' && character != '1')
                {
                    GuiLogMessage("Invalid character in Key: '" + character + "' - may only contain '1' and '0'", NotificationLevel.Error);
                    return false;
                }
            }

            foreach (char character in inputIV)
            {
                if (character != '0' && character != '1')
                {
                    GuiLogMessage("Invalid character in IV: '" + character + "' - may only contain '1' and '0'", NotificationLevel.Error);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Starts the encryption/decryption process with SDES
        /// </summary>
        /// <param name="action">0 = encrypt, 1 = decrypt</param>
        private void process(int action)
        {
            if (controlSlave is object && InputStream is object && InputIV is object)
            {
                controlSlave.onStatusChanged();
            }

            try
            {
                if (!areKeyAndIVValid())
                {
                    return;
                }

                if (inputStream == null || inputStream.Length == 0)
                {
                    GuiLogMessage("No input data, aborting now", NotificationLevel.Error);
                    return;
                }

                using (CStreamReader reader = inputStream.CreateReader())
                {
                    outputStream = new CStreamWriter();

                    System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                    DateTime startTime = DateTime.Now;

                    //Encrypt
                    if (action == 0)
                    {

                        if (settings.Mode == 0)
                        {
                            GuiLogMessage("Starting encryption with ECB", NotificationLevel.Info);
                            ElectronicCodeBook ecb = new ElectronicCodeBook(this);
                            ecb.encrypt(reader, outputStream, Tools.stringToBinaryByteArray(enc.GetString(inputKey)));
                        }
                        else if (settings.Mode == 1)
                        {
                            GuiLogMessage("Starting encryption with CBC", NotificationLevel.Info);
                            CipherBlockChaining cbc = new CipherBlockChaining(this);
                            cbc.encrypt(reader, outputStream, Tools.stringToBinaryByteArray(enc.GetString(inputKey)), Tools.stringToBinaryByteArray(enc.GetString(inputIV)));
                        }
                        else if (settings.Mode == 2)
                        {
                            GuiLogMessage("Starting encryption with CFB", NotificationLevel.Info);
                            CipherFeedBack cfb = new CipherFeedBack(this);
                            cfb.encrypt(reader, outputStream, Tools.stringToBinaryByteArray(enc.GetString(inputKey)), Tools.stringToBinaryByteArray(enc.GetString(inputIV)));
                        }
                        else if (settings.Mode == 3)
                        {
                            GuiLogMessage("Starting encryption with OFB", NotificationLevel.Info);
                            OutputFeedBack ofb = new OutputFeedBack(this);
                            ofb.encrypt(reader, outputStream, Tools.stringToBinaryByteArray(enc.GetString(inputKey)), Tools.stringToBinaryByteArray(enc.GetString(inputIV)));
                        }
                    }
                    //Decrypt
                    else if (action == 1)
                    {

                        if (settings.Mode == 0)
                        {
                            GuiLogMessage("Starting decryption with ECB", NotificationLevel.Info);
                            ElectronicCodeBook ecb = new ElectronicCodeBook(this);
                            ecb.decrypt(reader, outputStream, Tools.stringToBinaryByteArray(enc.GetString(inputKey)));
                        }
                        else if (settings.Mode == 1)
                        {
                            GuiLogMessage("Starting decryption with CBC", NotificationLevel.Info);
                            CipherBlockChaining cbc = new CipherBlockChaining(this);
                            cbc.decrypt(reader, outputStream, Tools.stringToBinaryByteArray(enc.GetString(inputKey)), Tools.stringToBinaryByteArray(enc.GetString(inputIV)));
                        }
                        else if (settings.Mode == 2)
                        {
                            GuiLogMessage("Starting decryption with CFB", NotificationLevel.Info);
                            CipherFeedBack cfb = new CipherFeedBack(this);
                            cfb.decrypt(reader, outputStream, Tools.stringToBinaryByteArray(enc.GetString(inputKey)), Tools.stringToBinaryByteArray(enc.GetString(inputIV)));
                        }
                        else if (settings.Mode == 3)
                        {
                            GuiLogMessage("Starting decryption with OFB", NotificationLevel.Info);
                            OutputFeedBack ofb = new OutputFeedBack(this);
                            ofb.decrypt(reader, outputStream, Tools.stringToBinaryByteArray(enc.GetString(inputKey)), Tools.stringToBinaryByteArray(enc.GetString(inputIV)));
                        }
                    }

                    outputStream.Close();
                    reader.Close();

                    DateTime stopTime = DateTime.Now;
                    TimeSpan duration = stopTime - startTime;
                    if (!stop)
                    {
                        GuiLogMessage(((settings.Action == 0) ? "Encryption" : "Decryption") + " complete! ", NotificationLevel.Info);
                        GuiLogMessage("Time used: " + duration.ToString(), NotificationLevel.Debug);
                        OnPropertyChanged("OutputStream");

                    }
                    else
                    {
                        GuiLogMessage("Aborted!", NotificationLevel.Info);
                    }
                }
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
                GuiLogMessage(exception.StackTrace, NotificationLevel.Error);
            }
            finally
            {
                ProgressChanged(1, 1);
            }
        }

        #endregion

    }//end SDES

    /// <summary>
    /// This Class is for controlling the SDES with a "brute forcer" like KeySearcher
    /// </summary>
    public class SDESControl : IControlEncryption
    {
        #region private
        private readonly SDES plugin;
        private byte[] input;
        private readonly ElectronicCodeBook ecb;
        private readonly CipherBlockChaining cbc;
        private readonly CipherFeedBack cfb;
        private readonly OutputFeedBack ofb;
        #endregion

        #region events
        public event KeyPatternChanged KeyPatternChanged; //not used, because we only have one key length
        public event IControlStatusChangedEventHandler OnStatusChanged;
        #endregion

        #region public

        /// <summary>
        /// Constructs a new SDESControl
        /// </summary>
        /// <param name="Plugin"></param>
        public SDESControl(SDES Plugin)
        {
            plugin = Plugin;
            ecb = new ElectronicCodeBook(plugin);
            cbc = new CipherBlockChaining(plugin);
            cfb = new CipherFeedBack(plugin);
            ofb = new OutputFeedBack(plugin);
        }

        /// <summary>
        /// Called by SDES if its status changes
        /// </summary>
        public void onStatusChanged()
        {
            if (OnStatusChanged != null)
            {
                OnStatusChanged(this, true);
            }
        }

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

        public byte[] Decrypt(byte[] ciphertext, byte[] key)
        {
            byte[] defaultIv = new byte[ciphertext.Length];
            return Decrypt(ciphertext, key, defaultIv);
        }

        /// <summary>
        /// Called by a Master to start decryption with ciphertext
        /// </summary>
        /// <param name="ciphertext">encrypted text</param>
        /// <param name="key">key</param>
        /// <returns>decrypted text</returns>
        public byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] IV)
        {
            return execute(ciphertext, key, ciphertext.Length, 1);
        }

        /// <summary>
        /// Called by a Master to start decryption with ciphertext
        /// </summary>
        /// <param name="ciphertext">encrypted text</param>
        /// <param name="key">key</param>
        /// <param name="bytesToUse">bytesToUse</param>
        /// <returns>decrypted text</returns>
        public byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] IV, int bytesToUse)
        {
            return execute(ciphertext, key, bytesToUse, 1);
        }

        public string GetCipherShortName()
        {
            return "SDES";
        }

        public int GetBlockSizeAsBytes()
        {
            throw new NotImplementedException();
        }

        public int GetKeySizeAsBytes()
        {
            throw new NotImplementedException();
        }

        public void SetToDefaultSettings()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Called by a Master to start decryption
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="bytesToUse">bytesToUse</param>
        /// <returns>decrypted text</returns>
        public byte[] Decrypt(byte[] key, byte[] IV, int bytesToUse)
        {
            return execute(key, bytesToUse, 1);
        }

        /// <summary>
        /// Get the key pattern of the SDES algorithm
        /// </summary>
        /// <returns>[01][01][01][01][01][01][01][01][01][01]</returns>
        public string GetKeyPattern()
        {
            return "[01][01][01][01][01][01][01][01][01][01]";
        }

        public IControlEncryption Clone()
        {
            return new SDESControl(plugin);
        }

        public void Dispose()
        {
            //closeStreams();
        }

        #endregion

        #region private

        /// <summary>
        /// Called by itself to start encryption/decryption
        /// </summary>
        /// /// <param name="data">The data for encryption/decryption</param>
        /// <param name="key">key</param>
        /// <param name="bytesToUse">bytesToUse</param>
        /// <returns>encrypted/decrypted text</returns>
        private byte[] execute(byte[] data, byte[] key, int bytesToUse, int action)
        {
            byte[] output;
            if (bytesToUse > 0)
            {
                output = new byte[bytesToUse];
            }
            else
            {
                output = new byte[data.Length];
            }

            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

            string IVString = "00000000";
            if (plugin.InputIV != null)
            {
                IVString = enc.GetString(plugin.InputIV);
            }

            if (action == 0)
            {
                switch (((SDESSettings)plugin.Settings).Mode)
                {
                    case 0: output = ecb.encrypt(data, key, bytesToUse); break;
                    case 1: output = cbc.encrypt(data, key, Tools.stringToBinaryByteArray(IVString), bytesToUse); break;
                    case 2: output = cfb.encrypt(data, key, Tools.stringToBinaryByteArray(IVString), bytesToUse); break;
                    case 3: output = ofb.encrypt(data, key, Tools.stringToBinaryByteArray(IVString), bytesToUse); break;
                }
            }
            else
            {
                switch (((SDESSettings)plugin.Settings).Mode)
                {
                    case 0: output = ecb.decrypt(data, key, bytesToUse); break;
                    case 1: output = cbc.decrypt(data, key, Tools.stringToBinaryByteArray(IVString), bytesToUse); break;
                    case 2: output = cfb.decrypt(data, key, Tools.stringToBinaryByteArray(IVString), bytesToUse); break;
                    case 3: output = ofb.decrypt(data, key, Tools.stringToBinaryByteArray(IVString), bytesToUse); break;
                }
            }

            return output;

        }

        /// <summary>
        /// Called by itself to start encryption/decryption
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="bytesToUse">bytesToUse</param>
        /// <returns>encrypted/decrypted text</returns>
        private byte[] execute(byte[] key, int bytesToUse, int action)
        {

            if (input == null || plugin.InputChanged)
            {
                plugin.InputChanged = false;
                input = new byte[bytesToUse];

                byte[] buffer = new byte[1];

                int i = 0;
                using (CStreamReader reader = plugin.InputStream.CreateReader())
                {
                    while ((reader.Read(buffer, 0, 1)) > 0 && i < bytesToUse)
                    {
                        input[i] = buffer[0];
                        i++;
                    }
                }
            }

            return execute(input, key, bytesToUse, action);
        }     

        public void ChangeSettings(string setting, object value)
        {

        }

        public IKeyTranslator GetKeyTranslator()
        {
            return new SDESKeyTranslator();
        }

        #endregion
    }

    internal class SDESKeyTranslator : IKeyTranslator
    {
        private KeyPattern pattern;
        private int progress = 0;

        #region KeyTranslator Members

        public byte[] GetKeyFromRepresentation(string representation)
        {
            byte[] bkey = new byte[10];
            int count = 0;
            foreach (char c in representation)
            {
                if (c == '0')
                {
                    bkey[count++] = 0;
                }
                else
                {
                    bkey[count++] = 1;
                }
            }

            return bkey;
        }

        public void SetKeys(object keys)
        {
            if (!(keys is KeyPattern))
            {
                throw new Exception("Something went horribly wrong!");
            }

            pattern = (KeyPattern)keys;
        }

        public byte[] GetKey()
        {
            string key = pattern.getKey();
            return GetKeyFromRepresentation(key);
        }

        public bool NextKey()
        {
            progress++;
            return pattern.nextKey();
        }

        public string GetKeyRepresentation()
        {
            return pattern.getKey();
        }

        public string GetKeyRepresentation(int add)
        {
            return pattern.getKey(add);
        }

        public int GetProgress()
        {
            int result = progress;
            progress = 0;
            return result;
        }      
     
        #endregion
    }

    /// <summary>
    /// Encapsulates the SDES algorithm
    /// </summary>
    public class SDES_algorithm
    {
        private readonly SDES mSdes;         //to call some methods on the plugin
        private int fkstep = 0;     //for presentation to check the number of fk we are in
        private int mode = 0;       //for presentation to check the mode we use (0 = en/1 = decrypt)

        public SDES_algorithm(SDES sdes)
        {
            mSdes = sdes;
        }

        /// <summary>
        /// Encrypt-function        
        /// Encrypts the input plaintext with the given key 
        /// </summary>
        /// <param name="plaintext">plaintext as byte array of size 8</param>
        /// <param name="key">key as byte array of size 10</param>
        /// <returns>ciphertext as byte array of size 8</returns>
        public byte[] encrypt(byte[] plaintext, byte[] key)
        {
            mode = 0; // to tell presentation what we are doing

            if (mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).key_txt.Text =
                    Tools.byteArrayToStringWithSpaces(key);
                }
                , null);
            }
            //calculate sub key 1
            byte[] vp10 = p10(key);
            if (mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).key_txt_p10_input.Text =
                    Tools.byteArrayToStringWithSpaces(key);
                    ((SDESPresentation)mSdes.Presentation).key_txt_ls1_input_1.Text =
                    Tools.byteArrayToStringWithSpaces(vp10);
                }
                , null);
            }

            byte[] vls1 = ls_1(vp10);
            if (mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).key_txt_p8_1_input.Text =
                    Tools.byteArrayToStringWithSpaces(vls1);
                }
                , null);
            }

            byte[] key1 = p8(vls1);
            if (mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).key_txt_k1.Text =
                    Tools.byteArrayToStringWithSpaces(key1);
                }
                , null);
            }

            //calculate sub key 2
            byte[] vls1_old = vls1;
            vls1 = ls_1(vls1);
            if (mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).key_txt_p8_copy.Text =
                    Tools.byteArrayToStringWithSpaces(vls1_old);
                    ((SDESPresentation)mSdes.Presentation).key_txt_ls1_2.Text =
                    Tools.byteArrayToStringWithSpaces(vls1_old);
                    ((SDESPresentation)mSdes.Presentation).key_txt_ls1_3.Text =
                   Tools.byteArrayToStringWithSpaces(vls1);
                }
                , null);
            }

            vls1 = ls_1(vls1);
            if (mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).key_txt_p8_2_input.Text =
                    Tools.byteArrayToStringWithSpaces(vls1);
                }
               , null);
            }

            byte[] key2 = p8(vls1);
            if (mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).key_txt_k2.Text =
                    Tools.byteArrayToStringWithSpaces(key2);
                }
               , null);
            }

            // ip_inverse(fk_2(sw(fk_1(ip(plaintext))))) :

            byte[] ip = this.ip(plaintext);
            if (mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).encrypt_txt_plaintext.Text =
                    Tools.byteArrayToStringWithSpaces(plaintext);
                    ((SDESPresentation)mSdes.Presentation).encrypt_txt_ip_input.Text =
                    Tools.byteArrayToStringWithSpaces(plaintext);
                    ((SDESPresentation)mSdes.Presentation).encrypt_txt_ip_output.Text =
                    Tools.byteArrayToStringWithSpaces(ip);
                }
               , null);
            }

            byte[] fk1 = fk(ip, key1);
            if (mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).encrypt_txt_sw_input.Text =
                    Tools.byteArrayToStringWithSpaces(fk1);
                }
               , null);
            }

            byte[] swtch = sw(fk1);
            if (mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).encrypt_txt_sw_output.Text =
                    Tools.byteArrayToStringWithSpaces(swtch);
                }
               , null);
            }

            byte[] fk2 = fk(swtch, key2);
            if (mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).encrypt_txt_ip_invers_input.Text =
                    Tools.byteArrayToStringWithSpaces(fk2);
                }
               , null);
            }

            byte[] ciphertext = ip_inverse(fk2);
            if (mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).encrypt_txt_ip_invers_output.Text =
                    Tools.byteArrayToStringWithSpaces(ciphertext);
                }
               , null);
            }

            return ciphertext;

        }//end encrypt

        /// <summary>
        /// Decrypt-function
        /// Decrypts the input ciphertext with the given key
        /// </summary>
        /// <param name="ciphertext">ciphertext as byte array of size 8</param>
        /// <param name="key"> key as byte array of size 10</param>
        /// <returns>plaintext as byte array of size 8</returns>
        public byte[] decrypt(byte[] ciphertext, byte[] key)
        {
            mode = 1; // to tell presentation what we are doing

            if (mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).key_txt.Text =
                    Tools.byteArrayToStringWithSpaces(key);
                }
                , null);
            }
            //calculate sub key 1
            byte[] vp10 = p10(key);
            if (mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).key_txt_p10_input.Text =
                    Tools.byteArrayToStringWithSpaces(key);
                    ((SDESPresentation)mSdes.Presentation).key_txt_ls1_input_1.Text =
                    Tools.byteArrayToStringWithSpaces(vp10);
                }
                , null);
            }

            byte[] vls1 = ls_1(vp10);
            if (mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).key_txt_p8_1_input.Text =
                    Tools.byteArrayToStringWithSpaces(vls1);
                }
                , null);
            }

            byte[] key1 = p8(vls1);
            if (mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).key_txt_k1.Text =
                    Tools.byteArrayToStringWithSpaces(key1);
                }
                , null);
            }

            //calculate sub key 2
            byte[] vls1_old = vls1;
            vls1 = ls_1(vls1);
            if (mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).key_txt_p8_copy.Text =
                    Tools.byteArrayToStringWithSpaces(vls1_old);
                    ((SDESPresentation)mSdes.Presentation).key_txt_ls1_2.Text =
                    Tools.byteArrayToStringWithSpaces(vls1_old);
                    ((SDESPresentation)mSdes.Presentation).key_txt_ls1_3.Text =
                   Tools.byteArrayToStringWithSpaces(vls1);
                }
                , null);
            }

            vls1 = ls_1(vls1);
            if (mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).key_txt_p8_2_input.Text =
                    Tools.byteArrayToStringWithSpaces(vls1);
                }
               , null);
            }

            byte[] key2 = p8(vls1);
            if (mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).key_txt_k2.Text =
                    Tools.byteArrayToStringWithSpaces(key2);
                }
               , null);
            }

            // ip_inverse(fk_1(sw(fk_2(ip(ciphertext))))) :

            byte[] ip = this.ip(ciphertext);
            if (mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).decrypt_txt_plaintext.Text =
                    Tools.byteArrayToStringWithSpaces(ciphertext);
                    ((SDESPresentation)mSdes.Presentation).decrypt_txt_ip_input.Text =
                    Tools.byteArrayToStringWithSpaces(ciphertext);
                    ((SDESPresentation)mSdes.Presentation).decrypt_txt_ip_output.Text =
                    Tools.byteArrayToStringWithSpaces(ip);
                }
               , null);
            }

            byte[] fk2 = fk(ip, key2);
            if (mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).decrypt_txt_sw_input.Text =
                    Tools.byteArrayToStringWithSpaces(fk2);
                }
               , null);
            }

            byte[] swtch = sw(fk2);
            if (mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).decrypt_txt_sw_output.Text =
                    Tools.byteArrayToStringWithSpaces(swtch);
                }
               , null);
            }

            byte[] fk1 = fk(swtch, key1);
            if (mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).decrypt_txt_ip_invers_input.Text =
                    Tools.byteArrayToStringWithSpaces(fk1);
                }
               , null);
            }

            byte[] plaintext = ip_inverse(fk1);
            if (mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).decrypt_txt_ip_invers_output.Text =
                    Tools.byteArrayToStringWithSpaces(plaintext);
                }
               , null);
            }

            return plaintext;

        }//end decrypt

        ///<summary>
        ///p10-function
        ///Permutates the input bytes array of "10 bits" to another by 
        ///the following rule:
        ///
        ///src    dest
        ///1   -> 3
        ///2   -> 5
        ///3   -> 2
        ///4   -> 7
        ///5   -> 4
        ///6   -> 10
        ///7   -> 1
        ///8   -> 9
        ///9   -> 8
        ///10  -> 6
        ///</summary>
        ///<param name="bits">byte array of size 10</param>
        ///<returns>byte array of size 10</returns>
        ///
        private byte[] p10(byte[] bits)
        {

            byte[] p10 = new byte[10];

            p10[1 - 1] = bits[3 - 1];
            p10[2 - 1] = bits[5 - 1];
            p10[3 - 1] = bits[2 - 1];
            p10[4 - 1] = bits[7 - 1];
            p10[5 - 1] = bits[4 - 1];
            p10[6 - 1] = bits[10 - 1];
            p10[7 - 1] = bits[1 - 1];
            p10[8 - 1] = bits[9 - 1];
            p10[9 - 1] = bits[8 - 1];
            p10[10 - 1] = bits[6 - 1];

            //mSdes.GuiLogMessage("P10 with " + Tools.intArray2String(bits) + " is " + Tools.intArray2String(p10), NotificationLevel.Debug);
            return p10;

        }//end p10

        ///<summary>
        ///p8-function
        ///Permutates the input bytes array of "8 bits" to another by 
        ///the following rule:
        ///
        ///src    dest
        ///1   -> 6
        ///2   -> 3
        ///3   -> 7
        ///4   -> 4
        ///5   -> 8
        ///6   -> 5
        ///7   -> 10
        ///8   -> 9
        ///</summary>
        ///<param name="bits">byte array of size 10</param>
        ///<returns>byte array of size 8</returns>
        private byte[] p8(byte[] bits)
        {

            byte[] p8 = new byte[8];

            p8[1 - 1] = bits[6 - 1];
            p8[2 - 1] = bits[3 - 1];
            p8[3 - 1] = bits[7 - 1];
            p8[4 - 1] = bits[4 - 1];
            p8[5 - 1] = bits[8 - 1];
            p8[6 - 1] = bits[5 - 1];
            p8[7 - 1] = bits[10 - 1];
            p8[8 - 1] = bits[9 - 1];

            //mSdes.GuiLogMessage("P8 with " + Tools.intArray2String(bits) + " is " + Tools.intArray2String(p8), NotificationLevel.Debug);
            return p8;

        }//end p8

        ///<summary>
        ///ip-function (initial permutation)
        ///Permutates the input array of "8 bits" to another by 
        ///the following rule:
        ///
        ///src    dest
        ///1   -> 2
        ///2   -> 6
        ///3   -> 3
        ///4   -> 1
        ///5   -> 4
        ///6   -> 8
        ///7   -> 5
        ///8   -> 7
        ///</summary>
        ///<param name="bits">byte array of size 8</param>
        ///<returns>byte array of size 8</returns>
        private byte[] ip(byte[] bits)
        {

            byte[] ip = new byte[8];

            ip[1 - 1] = bits[2 - 1];
            ip[2 - 1] = bits[6 - 1];
            ip[3 - 1] = bits[3 - 1];
            ip[4 - 1] = bits[1 - 1];
            ip[5 - 1] = bits[4 - 1];
            ip[6 - 1] = bits[8 - 1];
            ip[7 - 1] = bits[5 - 1];
            ip[8 - 1] = bits[7 - 1];

            //mSdes.GuiLogMessage("ip with " + Tools.intArray2String(bits) + " is " + Tools.intArray2String(ip), NotificationLevel.Debug);
            return ip;

        }//end ip

        ///<summary>
        ///ip^-1-function (initial permutation inverse)
        ///Permutates the input array of "8 bits" to another by 
        ///the following rule:
        ///
        ///src    dest
        ///1   -> 4
        ///2   -> 1
        ///3   -> 3
        ///4   -> 5
        ///5   -> 7
        ///6   -> 2
        ///7   -> 8
        ///8   -> 6
        ///</summary>
        ///<param name="bits">byte array of size 8</param>
        ///<returns>byte array of size 8</returns>
        private byte[] ip_inverse(byte[] bits)
        {

            byte[] ip_inverse = new byte[8];

            ip_inverse[1 - 1] = bits[4 - 1];
            ip_inverse[2 - 1] = bits[1 - 1];
            ip_inverse[3 - 1] = bits[3 - 1];
            ip_inverse[4 - 1] = bits[5 - 1];
            ip_inverse[5 - 1] = bits[7 - 1];
            ip_inverse[6 - 1] = bits[2 - 1];
            ip_inverse[7 - 1] = bits[8 - 1];
            ip_inverse[8 - 1] = bits[6 - 1];

            //mSdes.GuiLogMessage("ip_inverse with " + Tools.intArray2String(bits) + " is " + Tools.intArray2String(ip_inverse), NotificationLevel.Debug);		
            return ip_inverse;

        }//end ip_inverse

        ///<summary>
        ///fk-function
        ///
        ///combines the following functions:
        ///
        ///right is the right part of the input array
        ///left is the left part of the input array
        ///
        ///(right | left) := (inputarray))
        ///ret := exclusive_or(left,F(right,key)) + right)
        ///</summary>
        ///<param name="bits">byte array of size 8</param>
        ///<param name="key">byte array of size 8</param>
        ///<returns>byte array of size 8</returns>
        private byte[] fk(byte[] bits, byte[] key)
        {
            byte[] left = { bits[1 - 1], bits[2 - 1], bits[3 - 1], bits[4 - 1] };
            byte[] right = { bits[5 - 1], bits[6 - 1], bits[7 - 1], bits[8 - 1] };

            byte[] exclusive_oder = Tools.exclusive_or(left, F(right, key));

            byte[] ret = {exclusive_oder[1-1],exclusive_oder[2-1],exclusive_oder[3-1],exclusive_oder[4-1],
                     right[1-1],right[2-1],right[3-1],right[4-1]};

            fkstep++;
            if (fkstep == 2)
            {
                fkstep = 0;
            }

            //mSdes.GuiLogMessage("fk with " + Tools.intArray2String(bits) + " is " + Tools.intArray2String(ret), NotificationLevel.Debug);
            return ret;

        }//end fk

        ///<summary>
        ///ls-1 function
        ///</summary>
        ///<param name="bits">byte array of size 10</param>
        ///<returns>byte array of size 10</returns>
        private byte[] ls_1(byte[] bits)
        {

            byte[] ls_1 = new byte[10];

            ls_1[1 - 1] = bits[2 - 1];
            ls_1[2 - 1] = bits[3 - 1];
            ls_1[3 - 1] = bits[4 - 1];
            ls_1[4 - 1] = bits[5 - 1];
            ls_1[5 - 1] = bits[1 - 1];
            ls_1[6 - 1] = bits[7 - 1];
            ls_1[7 - 1] = bits[8 - 1];
            ls_1[8 - 1] = bits[9 - 1];
            ls_1[9 - 1] = bits[10 - 1];
            ls_1[10 - 1] = bits[6 - 1];

            //mSdes.GuiLogMessage("ls-1 with " + Tools.intArray2String(bits) + " is " + Tools.intArray2String(ls_1), NotificationLevel.Debug);
            return ls_1;

        }//end ls_1

        ///<summary>
        ///switch-function
        ///
        ///switches the left side and the right side of the 8 bit array
        ///(left|right) -> (right|left)
        ///</summary>
        ///<param name="bits">byte array of size 8</param>
        ///<returns>byte array of size 8</returns>
        private byte[] sw(byte[] bits)
        {

            byte[] ret = {bits[5-1],bits[6-1],bits[7-1],bits[8-1],
                         bits[1-1],bits[2-1],bits[3-1],bits[4-1]};

            //mSdes.GuiLogMessage("sw with " + Tools.intArray2String(bits) + " is " + Tools.intArray2String(ret), NotificationLevel.Debug);
            return ret;

        }//end sw

        ///<summary>
        ///F-function
        ///
        ///combines both s-boxes and permutates the return value with p4
        ///p4( s0(exclusive_or(ep(number),key) | s1(exclusive_or(ep(number),key) )
        ///</summary>
        ///<param name="bits">byte array of size 8</param>
        ///<param name="bits">key of size 8</param>
        ///<returns>byte array of size 8</returns>
        private byte[] F(byte[] bits, byte[] key)
        {

            byte[] ep = this.ep(bits);

            byte[] exclusive = Tools.exclusive_or(ep, key);

            byte[] s0_input = { exclusive[1 - 1], exclusive[2 - 1], exclusive[3 - 1], exclusive[4 - 1] };
            byte[] s0 = sbox_0(s0_input);

            byte[] s1_input = { exclusive[5 - 1], exclusive[6 - 1], exclusive[7 - 1], exclusive[8 - 1] };
            byte[] s1 = sbox_1(s1_input);

            byte[] s0_s1 = { s0[1 - 1], s0[2 - 1], s1[1 - 1], s1[2 - 1] };
            byte[] ret = p4(s0_s1);

            if (mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    if (mode == 0 && fkstep == 0)
                    {
                        ((SDESPresentation)mSdes.Presentation).encrypt_txt_sbox1_output.Text =
                        Tools.byteArrayToStringWithSpaces(ret);
                    }
                    if (mode == 0 && fkstep == 1)
                    {
                        ((SDESPresentation)mSdes.Presentation).encrypt_txt_sbox2_output.Text =
                        Tools.byteArrayToStringWithSpaces(ret);
                    }
                    if (mode == 1 && fkstep == 0)
                    {
                        ((SDESPresentation)mSdes.Presentation).decrypt_txt_sbox1_output.Text =
                        Tools.byteArrayToStringWithSpaces(ret);
                    }
                    if (mode == 1 && fkstep == 1)
                    {
                        ((SDESPresentation)mSdes.Presentation).decrypt_txt_sbox2_output.Text =
                        Tools.byteArrayToStringWithSpaces(ret);
                    }
                }
               , null);
            }

            //mSdes.GuiLogMessage("F with " + Tools.intArray2String(bits) + " is " + Tools.intArray2String(key) + " ist " + Tools.intArray2String(ret), NotificationLevel.Debug);
            return ret;

        }//end F

        ///<summary>
        ///p4-function
        ///Permutates the input array of "4 bits" to another by 
        ///the following rule:
        ///
        ///src    dest
        ///1   -> 2
        ///2   -> 4
        ///3   -> 3
        ///4   -> 1
        ///</summary>
        ///<param name="bits">byte array of size 4</param>
        ///<returns>byte array of size 4</returns>
        private byte[] p4(byte[] bits)
        {

            byte[] ret = new byte[4];
            ret[1 - 1] = bits[2 - 1];
            ret[2 - 1] = bits[4 - 1];
            ret[3 - 1] = bits[3 - 1];
            ret[4 - 1] = bits[1 - 1];

            return ret;

        }//end p4

        ///<summary>
        ///ep-function
        ///Permutates the input array of "4 bits" to another array of "8 bits" by 
        ///the following rule:
        ///
        ///src    dest
        ///1   -> 4
        ///2   -> 1
        ///3   -> 2
        ///4   -> 3
        ///5   -> 2
        ///6   -> 3
        ///7   -> 4
        ///8   -> 1
        ///</summary>
        ///<param name="bits">byte array of size 4</param>
        ///<returns>byte array of size 8</returns>
        private byte[] ep(byte[] bits)
        {

            byte[] ep = new byte[8];
            ep[1 - 1] = bits[4 - 1];
            ep[2 - 1] = bits[1 - 1];
            ep[3 - 1] = bits[2 - 1];
            ep[4 - 1] = bits[3 - 1];
            ep[5 - 1] = bits[2 - 1];
            ep[6 - 1] = bits[3 - 1];
            ep[7 - 1] = bits[4 - 1];
            ep[8 - 1] = bits[1 - 1];

            if (mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    if (mode == 0 && fkstep == 0)
                    {
                        ((SDESPresentation)mSdes.Presentation).encrypt_txt_ep_output.Text =
                        Tools.byteArrayToStringWithSpaces(ep);
                    }
                    if (mode == 0 && fkstep == 1)
                    {
                        ((SDESPresentation)mSdes.Presentation).encrypt_txt_ep_output1.Text =
                        Tools.byteArrayToStringWithSpaces(ep);
                    }
                    if (mode == 1 && fkstep == 0)
                    {
                        ((SDESPresentation)mSdes.Presentation).decrypt_txt_ep_output.Text =
                        Tools.byteArrayToStringWithSpaces(ep);
                    }
                    if (mode == 1 && fkstep == 1)
                    {
                        ((SDESPresentation)mSdes.Presentation).decrypt_txt_ep_output1.Text =
                        Tools.byteArrayToStringWithSpaces(ep);
                    }
                }
               , null);
            }

            return ep;
        }

        ///<summary>
        ///SBox-0
        ///
        ///S0 =  1 0 3 2
        ///      3 2 1 0
        ///      0 2 1 3    
        ///      3 1 3 2           
        ///</summary>
        ///<param name="bits">byte array of size 4</param>
        ///<returns>byte array of size 2</returns>
        private byte[] sbox_0(byte[] bits)
        {

            int row = 2 * bits[1 - 1] + 1 * bits[4 - 1];
            int column = 2 * bits[2 - 1] + 1 * bits[3 - 1];

            byte[,][] sbox_0 = new byte[4, 4][]
                            {
                            {new byte[] {0,1}, new byte[] {0,0}, new byte[] {1,1}, new byte[] {1,0}},
                              {new byte[] {1,1}, new byte[] {1,0}, new byte[] {0,1}, new byte[] {0,0}},
                              {new byte[] {0,0}, new byte[] {1,0}, new byte[] {0,1}, new byte[] {1,1}},
                              {new byte[] {1,1}, new byte[] {0,1}, new byte[] {1,1}, new byte[] {1,0}}
                            };

            byte[] ret = sbox_0[row, column];

            if (mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    if (mode == 0 && fkstep == 0)
                    {
                        ((SDESPresentation)mSdes.Presentation).encrypt_txt_s0_1_output.Text =
                        Tools.byteArrayToStringWithSpaces(ret);
                    }
                    if (mode == 0 && fkstep == 1)
                    {
                        ((SDESPresentation)mSdes.Presentation).encrypt_txt_s0_2_output.Text =
                        Tools.byteArrayToStringWithSpaces(ret);
                    }
                    if (mode == 1 && fkstep == 0)
                    {
                        ((SDESPresentation)mSdes.Presentation).decrypt_txt_s0_1_output.Text =
                        Tools.byteArrayToStringWithSpaces(ret);
                    }
                    if (mode == 1 && fkstep == 1)
                    {
                        ((SDESPresentation)mSdes.Presentation).decrypt_txt_s0_2_output.Text =
                        Tools.byteArrayToStringWithSpaces(ret);
                    }
                }
               , null);
            }

            //mSdes.GuiLogMessage("S0 with " + Tools.intArray2String(bits) + " is " + Tools.intArray2String(ret), NotificationLevel.Debug);
            return ret;

        }//end sbox-0


        ///<summary>
        ///SBox-1
        ///
        ///S1 =  0 1 2 3
        ///      2 0 1 3
        ///      3 0 1 0
        ///      2 1 0 3
        ///</summary>
        ///<param name="bits">byte array of size 4</param>
        ///<returns>byte array of size 2</returns>
        private byte[] sbox_1(byte[] bits)
        {

            int row = 2 * bits[1 - 1] + 1 * bits[4 - 1];
            int column = 2 * bits[2 - 1] + 1 * bits[3 - 1];

            byte[,][] sbox_1 = new byte[4, 4][]
                            {
                            {new byte[] {0,0}, new byte[] {0,1}, new byte[] {1,0}, new byte[] {1,1}},
                             {new byte[] {1,0}, new byte[] {0,0}, new byte[] {0,1}, new byte[] {1,1}},
                             {new byte[] {1,1}, new byte[] {0,0}, new byte[] {0,1}, new byte[] {0,0}},
                             {new byte[] {1,0}, new byte[] {0,1}, new byte[] {0,0}, new byte[] {1,1}}
                            };

            byte[] ret = sbox_1[row, column];

            if (mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    if (mode == 0 && fkstep == 0)
                    {
                        ((SDESPresentation)mSdes.Presentation).encrypt_txt_s1_1_output.Text =
                        Tools.byteArrayToStringWithSpaces(ret);
                    }
                    if (mode == 0 && fkstep == 1)
                    {
                        ((SDESPresentation)mSdes.Presentation).encrypt_txt_s1_2_output.Text =
                        Tools.byteArrayToStringWithSpaces(ret);
                    }
                    if (mode == 1 && fkstep == 0)
                    {
                        ((SDESPresentation)mSdes.Presentation).decrypt_txt_s1_1_output.Text =
                        Tools.byteArrayToStringWithSpaces(ret);
                    }
                    if (mode == 1 && fkstep == 1)
                    {
                        ((SDESPresentation)mSdes.Presentation).decrypt_txt_s1_2_output.Text =
                        Tools.byteArrayToStringWithSpaces(ret);
                    }
                }
               , null);
            }

            //mSdes.GuiLogMessage("S1 with " + Tools.intArray2String(bits) + " is " + Tools.intArray2String(ret), NotificationLevel.Debug);		
            return ret;

        }//end sbox-1

    }

    ///<summary>
    ///Encapsulates some necessary functions
    ///</summary>
    public class Tools
    {

        /// <summary>
        /// transforms a byte array into a String with spaces after each byte
        /// example:
        ///     1,0 => "1 0"
        /// </summary>
        /// <param name="byt">byt</param>
        /// <returns>s</returns>
        public static string byteArrayToStringWithSpaces(byte[] byt)
        {
            string s = "";

            foreach (byte b in byt)
            {
                s = s + b + " ";
            }
            return s;
        }
        ///<summary>
        ///Converts an byte array to a String
        ///</summary>
        ///<param name="bits">byte array of size n</param>
        ///<returns>String</returns>
        public static string byteArray2String(byte[] bits)
        {

            string ret = "";
            for (int i = 0; i < bits.Length; i++)
            {
                ret += ("" + bits[i]);
            }
            return ret;

        }//end byteArray2String

        ///<summary>
        ///Converts the given byte array to a printable String
        ///
        ///example {72, 101, 108, 108, 111} -> "Hello"
        ///</summary>
        ///<param name="bits">byte array of size n</param>
        ///<returns>String</returns>
        public static string byteArray2PrintableString(byte[] bits)
        {

            string ret = "";
            for (int i = 0; i < bits.Length; i++)
            {
                ret += ("" + (char)bits[i]);
            }
            return ret;

        }// byteArray2PrintableString

        ///<summary>
        ///equals-function
        ///
        ///returns true if both integer arrays are equal
        ///</summary>
        ///<param name="a">byte array of size n</param>
        ///<param name="b">byte array of size n</param>
        ///<returns>bool</returns>
        public static bool byteArrays_Equals(byte[] a, byte[] b)
        {

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;

        }//end byteArrays_Equals	

        ///<summary>
        ///converts an Byte to an byte array of (0,1)
        ///
        ///100 -> {1,1,0,0,1,0,0}
        ///</summary>
        ///<param name="byt">byte array of size n</param>
        ///<returns>byte array</returns>
        public static byte[] byteToByteArray(byte byt)
        {

            byte[] bytearray = new byte[8];

            for (int i = 7; i >= 0; i--)
            {

                bytearray[i] = (byte)(byt % 2);
                byt = (byte)Math.Floor((double)(byt / 2));

            }

            return bytearray;

        }//end byteTointArray

        ///<summary>
        ///converts an byte array of (0,1) to an byte
        ///
        ///{1,1,0,0,1,0,0} -> 100 
        ///</summary>
        ///<param name="bytearray">byte array of size n</param>
        ///<returns>byte</returns>
        public static byte byteArrayToByte(byte[] bytearray)
        {

            int byt = 0;

            byt = (bytearray[0] * 128)
                        + (bytearray[1] * 64)
                        + (bytearray[2] * 32)
                        + (bytearray[3] * 16)
                        + (bytearray[4] * 8)
                        + (bytearray[5] * 4)
                        + (bytearray[6] * 2)
                        + (bytearray[7] * 1);

            return (byte)byt;

        }//end byteArrayToInteger

        ///<summary>
        ///Exclusiv-OR function
        ///
        ///Does a exlusiv-or on two byte arrays 
        ///
        ///example {1,0,1} XOR {1,0,0} -> {0,0,1}
        ///</summary>
        ///<param name="bitsA">byte array of size n</param>
        ///<param name="bitsB">byte array of size n</param>
        ///<returns>byte array of size n</returns>
        public static byte[] exclusive_or(byte[] bitsA, byte[] bitsB)
        {

            byte[] exclusive_or_AB = new byte[bitsA.Length];

            for (int i = 0; i < bitsA.Length; i++)
            {

                if ((bitsA[i] == 0 && bitsB[i] == 1) ||
                   (bitsA[i] == 1 && bitsB[i] == 0)
                )
                {
                    exclusive_or_AB[i] = 1;
                }
                else
                {
                    exclusive_or_AB[i] = 0;
                }//end if

            }//end for

            return exclusive_or_AB;

        }//end exclusive_or

        ///<summary>
        ///converts string to an byte array
        ///
        ///example "Hello" -> {72, 101, 108, 108, 111}
        ///</summary>
        ///<param name="s">String</param>
        ///<returns>byte array</returns>
        public static byte[] stringToByteArray(string s)
        {
            byte[] bytearray = new byte[s.Length];

            for (int i = 0; i < s.Length; i++)
            {
                bytearray[i] = (byte)s[i];
            }

            return bytearray;

        }// end stringToByteArray

        ///<summary>
        ///converts a binary string to an byte array
        ///
        ///example "10010" -> {1, 0, 0, 1, 0}
        ///</summary>
        ///<param name="s">String</param>
        ///<returns>byte array</returns>
        public static byte[] stringToBinaryByteArray(string s)
        {
            byte[] bytearray = new byte[s.Length];

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '1')
                {
                    bytearray[i] = 1;
                }
                else if (s[i] == '0')
                {
                    bytearray[i] = 0;
                }
                else
                {
                    throw new Exception("Invalid Character '" + s[i] + "' at position " + i + " in String which represents binary values: " + s);
                }
            }

            return bytearray;

        }// end stringToByteArray
    }

    ///<summary>
    ///Encapsulates the CipherBlockChaining algorithm
    ///</summary>
    public class CipherBlockChaining
    {

        private readonly SDES mSdes;
        private readonly SDES_algorithm mAlgorithm;

        /// <summary>
        /// Constructs a CipherBlockChaining for SDES
        /// </summary>
        /// <param name="sdes">plugin</param>
        public CipherBlockChaining(SDES sdes)
        {
            mSdes = sdes;
            mAlgorithm = new SDES_algorithm(sdes);
        }

        ///<summary>
        ///Encrypts the given plaintext with the given key
        ///using CipherBlockChaining 
        ///</summary>
        public void encrypt(CStreamReader reader, CStreamWriter writer, byte[] key, byte[] vector)
        {

            byte[] buffer = new byte[1];
            int position = 0;

            while (!mSdes.getStop() && (reader.Read(buffer, 0, 1)) > 0)
            {
                //Step 1 get plaintext symbol
                byte symbol = buffer[0];
                //Step 2 exclusiv OR with vector
                vector = Tools.exclusive_or(vector, Tools.byteToByteArray(symbol));
                //Step 3 decrypt vector with key
                vector = mAlgorithm.encrypt(vector, key);
                //Step 4 store symbol in ciphertext
                writer.WriteByte(Tools.byteArrayToByte(vector));

                if ((int)(reader.Position * 100 / reader.Length) > position)
                {
                    position = (int)(reader.Position * 100 / reader.Length);
                    mSdes.ProgressChanged(reader.Position, reader.Length);
                }
                writer.Flush();
            }

        }//end encrypt

        ///
        ///Encrypts the given plaintext with the given key
        ///using CipherBlockChaining 
        ///
        /// bytesToUse tells the algorithm how many bytes it has to encrypt
        /// bytesToUse = 0 => encrypt all
        public byte[] encrypt(byte[] input, byte[] key, byte[] vector, [Optional, DefaultParameterValue(0)] int bytesToUse)
        {
            int until = input.Length;

            if (bytesToUse < until && bytesToUse > 0)
            {
                until = bytesToUse;
            }

            byte[] output = new byte[until];

            for (int i = 0; i < until; i++)
            {
                vector = Tools.exclusive_or(vector, Tools.byteToByteArray(input[i]));
                vector = mAlgorithm.encrypt(vector, key);
                output[i] = Tools.byteArrayToByte(vector);

            }//end while

            return output;

        }//end encrypt

        ///<summary>
        ///Decrypts the given plaintext with the given Key
        ///using CipherBlockChaining 
        ///</summary>
        public void decrypt(CStreamReader reader, CStreamWriter writer, byte[] key, byte[] vector)
        {

            byte[] buffer = new byte[1];
            int position = 0;

            while (!mSdes.getStop() && (reader.Read(buffer, 0, 1)) > 0)
            {
                //Step 1 get Symbol of Ciphertext
                byte symbol = buffer[0];
                //Step 2 Decrypt symbol with key and exclusiv-or with vector
                writer.WriteByte((Tools.byteArrayToByte(Tools.exclusive_or(mAlgorithm.decrypt(Tools.byteToByteArray(symbol), key), vector))));
                //Step 3 let vector be the decrypted Symbol
                vector = Tools.byteToByteArray(buffer[0]);

                if ((int)(reader.Position * 100 / reader.Length) > position)
                {
                    position = (int)(reader.Position * 100 / reader.Length);
                    mSdes.ProgressChanged(reader.Position, reader.Length);
                }
                writer.Flush();
            }

        }//end decrypt

        ///
        ///Decrypt the given plaintext with the given key
        ///using CipherBlockChaining 
        ///
        /// bytesToUse tells the algorithm how many bytes it has to encrypt
        /// bytesToUse = 0 => encrypt all
        public byte[] decrypt(byte[] input, byte[] key, byte[] vector, [Optional, DefaultParameterValue(0)] int bytesToUse)
        {

            int until = input.Length;

            if (bytesToUse < until && bytesToUse > 0)
            {
                until = bytesToUse;
            }

            byte[] output = new byte[until];

            for (int i = 0; i < until; i++)
            {
                output[i] = (Tools.byteArrayToByte(Tools.exclusive_or(mAlgorithm.decrypt(Tools.byteToByteArray(input[i]), key), vector)));
                vector = Tools.byteToByteArray(input[i]);

            }//end while

            return output;

        }//end encrypt

    }//end class CipherBlockChaining

    ///<summary>
    ///Encapsulates the ElectronicCodeBook algorithm
    ///</summary>
    public class ElectronicCodeBook
    {

        private readonly SDES mSdes;
        private readonly SDES_algorithm mAlgorithm;

        /// <summary>
        /// Constructs a ElectronicCodeBook for SDES
        /// </summary>
        /// <param name="sdes">plugin</param>
        public ElectronicCodeBook(SDES sdes)
        {
            mSdes = sdes;
            mAlgorithm = new SDES_algorithm(sdes);
        }

        ///
        ///Encrypts the given plaintext with the given key
        ///using ElectronicCodeBookMode 
        ///
        public void encrypt(CStreamReader reader, CStreamWriter writer, byte[] key)
        {

            byte[] buffer = new byte[1];
            int position = 0;

            while (!mSdes.getStop() && (reader.Read(buffer, 0, 1)) > 0)
            {
                //Step 1 get plaintext symbol
                byte symbol = buffer[0]; ;
                //Step 2 encrypt symbol
                writer.WriteByte(Tools.byteArrayToByte(mAlgorithm.encrypt(Tools.byteToByteArray(symbol), key)));

                if ((int)(reader.Position * 100 / reader.Length) > position)
                {
                    position = (int)(reader.Position * 100 / reader.Length);
                    mSdes.ProgressChanged(reader.Position, reader.Length);
                }
                writer.Flush();

            }//end while

        }//end encrypt

        ///
        ///Encrypts the given plaintext with the given key
        ///using ElectronicCodeBookMode 
        ///
        /// bytesToUse tells the algorithm how many bytes it has to encrypt
        /// bytesToUse = 0 => encrypt all
        public byte[] encrypt(byte[] input, byte[] key, [Optional, DefaultParameterValue(0)] int bytesToUse)
        {

            int until = input.Length;

            if (bytesToUse < until && bytesToUse > 0)
            {
                until = bytesToUse;
            }

            byte[] output = new byte[until];

            for (int i = 0; i < until; i++)
            {
                //Step 2 encrypt symbol
                output[i] = Tools.byteArrayToByte(mAlgorithm.encrypt(Tools.byteToByteArray(input[i]), key));

            }//end while

            return output;

        }//end encrypt

        ///<summary>
        ///Decrypts the given plaintext with the given Key
        ///using ElectronicCodeBook mode 
        ///</summary>
        public void decrypt(CStreamReader reader, CStreamWriter writer, byte[] key)
        {

            byte[] buffer = new byte[1];
            int position = 0;

            while (!mSdes.getStop() && (reader.Read(buffer, 0, 1)) > 0)
            {
                //Step 1 get plaintext symbol
                byte symbol = buffer[0];
                //Step 2 encrypt symbol
                writer.WriteByte(Tools.byteArrayToByte(mAlgorithm.decrypt(Tools.byteToByteArray(symbol), key)));

                if ((int)(reader.Position * 100 / reader.Length) > position)
                {
                    position = (int)(reader.Position * 100 / reader.Length);
                    mSdes.ProgressChanged(reader.Position, reader.Length);
                }
                writer.Flush();

            }//end while

        }//end decrypt

        ///
        ///Decrypt the given plaintext with the given key
        ///using ElectronicCodeBookMode 
        ///
        /// bytesToUse tells the algorithm how many bytes it has to decrypt
        /// bytesToUse = 0 => encrypt all
        public byte[] decrypt(byte[] input, byte[] key, [Optional, DefaultParameterValue(0)] int bytesToUse)
        {
            int until = input.Length;

            if (bytesToUse < until && bytesToUse > 0)
            {
                until = bytesToUse;
            }

            byte[] output = new byte[until];

            for (int i = 0; i < until; i++)
            {
                //Step 2 encrypt symbol
                output[i] = Tools.byteArrayToByte(mAlgorithm.decrypt(Tools.byteToByteArray(input[i]), key));

            }//end while

            return output;

        }//end encrypt

    }//end class ElectronicCodeBook

    ///<summary>
    ///Encapsulates the CipherFeedBack algorithm
    ///</summary>
    public class CipherFeedBack
    {

        private readonly SDES mSdes;
        private readonly SDES_algorithm mAlgorithm;

        /// <summary>
        /// Constructs a CipherFeedBack for SDES
        /// </summary>
        /// <param name="sdes">plugin</param>
        public CipherFeedBack(SDES sdes)
        {
            mSdes = sdes;
            mAlgorithm = new SDES_algorithm(sdes);
        }

        ///<summary>
        ///Encrypts the given plaintext with the given key
        ///using CipherFeedBack 
        ///</summary>
        public void encrypt(CStreamReader reader, CStreamWriter writer, byte[] key, byte[] vector)
        {

            byte[] buffer = new byte[1];
            int position = 0;

            while (!mSdes.getStop() && (reader.Read(buffer, 0, 1)) > 0)
            {
                //Step 1 get plaintext symbol
                byte symbol = buffer[0];
                //Step 2 encrypt vector with key
                vector = mAlgorithm.encrypt(vector, key);
                //Step 3 exclusiv OR with input
                vector = Tools.exclusive_or(vector, Tools.byteToByteArray(symbol));
                //Step 4 store symbol in ciphertext
                writer.WriteByte(Tools.byteArrayToByte(vector));

                if ((int)(reader.Position * 100 / reader.Length) > position)
                {
                    position = (int)(reader.Position * 100 / reader.Length);
                    mSdes.ProgressChanged(reader.Position, reader.Length);
                }
                writer.Flush();
            }

        }//end encrypt

        ///
        ///Encrypts the given plaintext with the given key
        ///using CipherFeedBack 
        ///
        /// bytesToUse tells the algorithm how many bytes it has to encrypt
        /// bytesToUse = 0 => encrypt all
        public byte[] encrypt(byte[] input, byte[] key, byte[] vector, [Optional, DefaultParameterValue(0)] int bytesToUse)
        {
            int until = input.Length;

            if (bytesToUse < until && bytesToUse > 0)
            {
                until = bytesToUse;
            }

            byte[] output = new byte[until];

            for (int i = 0; i < until; i++)
            {
                vector = Tools.exclusive_or(mAlgorithm.encrypt(vector, key), Tools.byteToByteArray(input[i]));
                output[i] = Tools.byteArrayToByte(vector);

            }//end while

            return output;

        }//end encrypt

        ///<summary>
        ///Decrypts the given plaintext with the given Key
        ///using CipherFeedBack 
        ///</summary>
        public void decrypt(CStreamReader reader, CStreamWriter writer, byte[] key, byte[] vector)
        {

            byte[] buffer = new byte[1];
            int position = 0;

            while (!mSdes.getStop() && (reader.Read(buffer, 0, 1)) > 0)
            {
                byte symbol = buffer[0];
                vector = mAlgorithm.encrypt(vector, key);
                writer.WriteByte(Tools.byteArrayToByte(Tools.exclusive_or(Tools.byteToByteArray(symbol), vector)));
                vector = Tools.byteToByteArray(symbol);

                if ((int)(reader.Position * 100 / reader.Length) > position)
                {
                    position = (int)(reader.Position * 100 / reader.Length);
                    mSdes.ProgressChanged(reader.Position, reader.Length);
                }
                writer.Flush();
            }

        }//end decrypt

        ///
        ///Decrypt the given plaintext with the given key
        ///using CipherFeedBack 
        ///
        /// bytesToUse tells the algorithm how many bytes it has to encrypt
        /// bytesToUse = 0 => encrypt all
        public byte[] decrypt(byte[] input, byte[] key, byte[] vector, [Optional, DefaultParameterValue(0)] int bytesToUse)
        {

            int until = input.Length;

            if (bytesToUse < until && bytesToUse > 0)
            {
                until = bytesToUse;
            }

            byte[] output = new byte[until];

            for (int i = 0; i < until; i++)
            {
                vector = mAlgorithm.encrypt(vector, key);
                output[i] = (Tools.byteArrayToByte(Tools.exclusive_or(Tools.byteToByteArray(input[i]), vector)));
                vector = Tools.byteToByteArray(input[i]);

            }//end while

            return output;

        }//end encrypt

    }//end class CipherFeedBack

    ///<summary>
    ///Encapsulates the OutputFeedBack algorithm
    ///</summary>
    public class OutputFeedBack
    {

        private readonly SDES mSdes;
        private readonly SDES_algorithm mAlgorithm;

        /// <summary>
        /// Constructs a OutputFeedBack for SDES
        /// </summary>
        /// <param name="sdes">plugin</param>
        public OutputFeedBack(SDES sdes)
        {
            mSdes = sdes;
            mAlgorithm = new SDES_algorithm(sdes);
        }

        ///<summary>
        ///Encrypts the given plaintext with the given key
        ///using OutputFeedBack 
        ///</summary>
        public void encrypt(CStreamReader reader, CStreamWriter writer, byte[] key, byte[] vector)
        {

            byte[] buffer = new byte[1];
            int position = 0;

            while (!mSdes.getStop() && (reader.Read(buffer, 0, 1)) > 0)
            {
                byte symbol = buffer[0];
                vector = mAlgorithm.encrypt(vector, key);
                writer.WriteByte(Tools.byteArrayToByte(Tools.exclusive_or(vector, Tools.byteToByteArray(symbol))));

                if ((int)(reader.Position * 100 / reader.Length) > position)
                {
                    position = (int)(reader.Position * 100 / reader.Length);
                    mSdes.ProgressChanged(reader.Position, reader.Length);
                }
                writer.Flush();
            }

        }//end encrypt

        ///
        ///Encrypts the given plaintext with the given key
        ///using OutputFeedBack 
        ///
        /// bytesToUse tells the algorithm how many bytes it has to encrypt
        /// bytesToUse = 0 => encrypt all
        public byte[] encrypt(byte[] input, byte[] key, byte[] vector, [Optional, DefaultParameterValue(0)] int bytesToUse)
        {
            int until = input.Length;

            if (bytesToUse < until && bytesToUse > 0)
            {
                until = bytesToUse;
            }

            byte[] output = new byte[until];

            for (int i = 0; i < until; i++)
            {
                vector = mAlgorithm.encrypt(vector, key);
                output[i] = Tools.byteArrayToByte(Tools.exclusive_or(vector, Tools.byteToByteArray(input[i])));

            }//end while

            return output;

        }//end encrypt

        ///<summary>
        ///Decrypts the given plaintext with the given Key
        ///using OutputFeedBack 
        ///</summary>
        public void decrypt(CStreamReader reader, CStreamWriter writer, byte[] key, byte[] vector)
        {

            byte[] buffer = new byte[1];
            int position = 0;

            while (!mSdes.getStop() && (reader.Read(buffer, 0, 1)) > 0)
            {
                byte symbol = buffer[0];
                vector = mAlgorithm.encrypt(vector, key);
                writer.WriteByte((Tools.byteArrayToByte(Tools.exclusive_or(Tools.byteToByteArray(symbol), vector))));

                if ((int)(reader.Position * 100 / reader.Length) > position)
                {
                    position = (int)(reader.Position * 100 / reader.Length);
                    mSdes.ProgressChanged(reader.Position, reader.Length);
                }
                writer.Flush();
            }

        }//end decrypt

        ///
        ///Decrypt the given plaintext with the given key
        ///using OutputFeedBack 
        ///
        /// bytesToUse tells the algorithm how many bytes it has to encrypt
        /// bytesToUse = 0 => encrypt all
        public byte[] decrypt(byte[] input, byte[] key, byte[] vector, [Optional, DefaultParameterValue(0)] int bytesToUse)
        {

            int until = input.Length;

            if (bytesToUse < until && bytesToUse > 0)
            {
                until = bytesToUse;
            }

            byte[] output = new byte[until];

            for (int i = 0; i < until; i++)
            {
                vector = mAlgorithm.encrypt(vector, key);
                output[i] = (Tools.byteArrayToByte(Tools.exclusive_or(Tools.byteToByteArray(input[i]), vector)));

            }//end while

            return output;

        }//end encrypt

    }//end class OutputFeedBack

}//end namespace CrypTool.Plugins.Cryptography.Encryption
