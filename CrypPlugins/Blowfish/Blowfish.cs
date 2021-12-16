/*
   Copyright 2020 Nils Kopal <Nils.Kopal<at>CrypTool.org

   The three ciphers are invented by Bruce Schneier (and others)
   Source codes are based on/from:

   a) Blowfish:
   Implementation of Bruce Schneier's Blowfish cipher. 
   The code is based on pseudo code from the German and English Wikipedia articles, see:
   https://de.wikipedia.org/wiki/Blowfish
   https://en.wikipedia.org/wiki/Blowfish_(cipher)

   b) Twofish:
   Original Twofish C# algorithm implemention is from Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License   
   Found on Bruce Schneier's homepage: https://www.schneier.com/academic/twofish/download/
   Test vectors: https://www.schneier.com/wp-content/uploads/2015/12/ecb_ival.txt
   
   c) Threefish:
   Original Threefish code is taken from: https://code.google.com/archive/p/skeinfish/ 
   Threefish code was written by Alberto Fajardo, 2010   
   
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
using CrypTool.Plugins.Blowfish.Properties;
using CrypTool.Plugins.Blowfish.Threefish;
using System;
using System.ComponentModel;
using System.Windows.Controls;
using static CrypTool.PluginBase.Miscellaneous.BlockCipherHelper;

namespace CrypTool.Plugins.Blowfish
{
    [Author("Nils Kopal", "Nils.Kopal@CrypTool.org", "CrypTool 2 Team", "https://www.CrypTool.org")]
    [PluginInfo("CrypTool.Plugins.Blowfish.Properties.Resources", "PluginCaption", "PluginTooltip", "Blowfish/userdoc.xml", "Blowfish/Images/icon.png")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class Blowfish : ICrypComponent
    {
        #region Private Variables

        private BlowfishSettings _BlowfishSettings = new BlowfishSettings();

        private byte[] _InputKey = null;
        private byte[] _InputIV = null;
        private byte[] _InputTweak = null;
        private ICrypToolStream _OutputStreamWriter;
        private ICrypToolStream _InputStream;

        private bool _stop = false;

        private byte[] _lastInputBlock = null; //needed for the visualization of the cipher; we only show the last encrypted/decrypted block

        #endregion

        #region Data Properties

        public ISettings Settings
        {
            get => _BlowfishSettings;
            set => _BlowfishSettings = (BlowfishSettings)value;
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
            get => _InputKey;
            set => _InputKey = value;
        }

        [PropertyInfo(Direction.InputData, "InputIVCaption", "InputIVTooltip", false)]
        public byte[] InputIV
        {
            get => _InputIV;
            set => _InputIV = value;
        }

        [PropertyInfo(Direction.InputData, "TweakCaption", "TweakTooltip", false)]
        public byte[] Tweak
        {
            get => _InputTweak;
            set => _InputTweak = value;
        }

        [PropertyInfo(Direction.OutputData, "OutputStreamCaption", "OutputStreamTooltip", true)]
        public ICrypToolStream OutputStream
        {
            get => _OutputStreamWriter;
            set
            {
                // empty
            }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public Blowfish()
        {
        }

        #region IPlugin Members

        public UserControl Presentation => null;

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);

            _lastInputBlock = null;
            _OutputStreamWriter = null;
            _stop = false;

            CheckKeyLength();

            //Select crypto function based on algorithm, blockmode, and action
            BlockCipher blockCipher = null;
            //Blowfish:
            if (_BlowfishSettings.BlowfishAlgorithmType == BlowfishAlgorithmType.Blowfish && _BlowfishSettings.BlockMode == BlockMode.CFB)
            {
                BlowfishAlgorithm algorithm = new BlowfishAlgorithm();
                algorithm.KeySchedule(_InputKey);
                blockCipher = new BlockCipher(algorithm.Encrypt);
            }
            else if (_BlowfishSettings.BlowfishAlgorithmType == BlowfishAlgorithmType.Blowfish && _BlowfishSettings.BlockMode == BlockMode.OFB)
            {
                BlowfishAlgorithm algorithm = new BlowfishAlgorithm();
                algorithm.KeySchedule(_InputKey);
                blockCipher = new BlockCipher(algorithm.Encrypt);
            }
            else if (_BlowfishSettings.BlowfishAlgorithmType == BlowfishAlgorithmType.Blowfish && _BlowfishSettings.Action == CipherAction.Encrypt)
            {
                BlowfishAlgorithm algorithm = new BlowfishAlgorithm();
                algorithm.KeySchedule(_InputKey);
                blockCipher = new BlockCipher(algorithm.Encrypt);
            }
            else if (_BlowfishSettings.BlowfishAlgorithmType == BlowfishAlgorithmType.Blowfish && _BlowfishSettings.Action == CipherAction.Decrypt)
            {
                BlowfishAlgorithm algorithm = new BlowfishAlgorithm();
                algorithm.KeySchedule(_InputKey);
                blockCipher = new BlockCipher(algorithm.Decrypt);
            }
            //Twofish:
            else if (_BlowfishSettings.BlowfishAlgorithmType == BlowfishAlgorithmType.Twofish && _BlowfishSettings.BlockMode == BlockMode.CFB)
            {
                TwofishAlgorithm algorithm = new TwofishAlgorithm(_InputKey);
                blockCipher = new BlockCipher(algorithm.Encrypt);
            }
            else if (_BlowfishSettings.BlowfishAlgorithmType == BlowfishAlgorithmType.Twofish && _BlowfishSettings.BlockMode == BlockMode.OFB)
            {
                TwofishAlgorithm algorithm = new TwofishAlgorithm(_InputKey);
                blockCipher = new BlockCipher(algorithm.Encrypt);
            }
            else if (_BlowfishSettings.BlowfishAlgorithmType == BlowfishAlgorithmType.Twofish && _BlowfishSettings.Action == CipherAction.Encrypt)
            {
                TwofishAlgorithm algorithm = new TwofishAlgorithm(_InputKey);
                blockCipher = new BlockCipher(algorithm.Encrypt);
            }
            else if (_BlowfishSettings.BlowfishAlgorithmType == BlowfishAlgorithmType.Twofish && _BlowfishSettings.Action == CipherAction.Decrypt)
            {
                TwofishAlgorithm algorithm = new TwofishAlgorithm(_InputKey);
                blockCipher = new BlockCipher(algorithm.Decrypt);
            }
            //Threefish:
            else if (_BlowfishSettings.BlowfishAlgorithmType == BlowfishAlgorithmType.Threefish && _BlowfishSettings.BlockMode == BlockMode.CFB)
            {
                CheckTweak();
                ThreefishAlgorithm algorithm = GetThreefishAlgorithm();
                algorithm.SetKey(_InputKey);
                if (_InputTweak != null && _InputTweak.Length != 0)
                {
                    algorithm.SetTweak(_InputTweak);
                }
                blockCipher = new BlockCipher(algorithm.Encrypt);
            }
            else if (_BlowfishSettings.BlowfishAlgorithmType == BlowfishAlgorithmType.Threefish && _BlowfishSettings.BlockMode == BlockMode.OFB)
            {
                CheckTweak();
                ThreefishAlgorithm algorithm = GetThreefishAlgorithm();
                algorithm.SetKey(_InputKey);
                if (_InputTweak != null && _InputTweak.Length != 0)
                {
                    algorithm.SetTweak(_InputTweak);
                }
                blockCipher = new BlockCipher(algorithm.Encrypt);
            }
            else if (_BlowfishSettings.BlowfishAlgorithmType == BlowfishAlgorithmType.Threefish && _BlowfishSettings.Action == CipherAction.Encrypt)
            {
                CheckTweak();
                ThreefishAlgorithm algorithm = GetThreefishAlgorithm();
                algorithm.SetKey(_InputKey);
                if (_InputTweak != null && _InputTweak.Length != 0)
                {
                    algorithm.SetTweak(_InputTweak);
                }
                blockCipher = new BlockCipher(algorithm.Encrypt);
            }
            else if (_BlowfishSettings.BlowfishAlgorithmType == BlowfishAlgorithmType.Threefish && _BlowfishSettings.Action == CipherAction.Decrypt)
            {
                CheckTweak();
                ThreefishAlgorithm algorithm = GetThreefishAlgorithm();
                algorithm.SetKey(_InputKey);
                if (_InputTweak != null && _InputTweak.Length != 0)
                {
                    algorithm.SetTweak(_InputTweak);
                }
                blockCipher = new BlockCipher(algorithm.Decrypt);
            }

            //Check, if we found a crypto function that we can use
            //this error should NEVER occur. Only in case someone adds functionality and misses
            //to create a valid configuration
            if (blockCipher == null)
            {
                GuiLogMessage("No crypto function could be selected based on your settings", NotificationLevel.Error);
                return;
            }

            int blocksize = 8;
            if (_BlowfishSettings.BlowfishAlgorithmType == BlowfishAlgorithmType.Twofish)
            {
                blocksize = 16;
            }
            if (_BlowfishSettings.BlowfishAlgorithmType == BlowfishAlgorithmType.Threefish)
            {
                //Threefish blocksize is equal to the key length
                blocksize = _InputKey.Length;
            }

            //Append padding
            if (_BlowfishSettings.Action == CipherAction.Encrypt && _BlowfishSettings.Padding != PaddingType.None)
            {
                //in case of encryption, we have to add padding
                _InputStream = AppendPadding(InputStream, _BlowfishSettings.Padding, blocksize);
            }
            else
            {
                //with decryption, we have to do nothing
                _InputStream = InputStream;
            }

            switch (_BlowfishSettings.BlockMode)
            {
                case BlockMode.ECB:
                    ExecuteECB(blockCipher,
                        _BlowfishSettings.Action,
                        ref _InputStream,
                        ref _OutputStreamWriter,
                        _InputKey,
                        _BlowfishSettings.Padding,
                        ref _stop,
                        ProgressChanged,
                        ref _lastInputBlock,
                        blocksize);
                    break;
                case BlockMode.CBC:
                    CheckIV();
                    ExecuteCBC(blockCipher,
                       _BlowfishSettings.Action,
                       ref _InputStream,
                       ref _OutputStreamWriter,
                       _InputKey,
                       _InputIV,
                       _BlowfishSettings.Padding,
                       ref _stop,
                       ProgressChanged,
                       ref _lastInputBlock,
                       blocksize);
                    break;
                case BlockMode.CFB:
                    CheckIV();
                    ExecuteCFB(blockCipher,
                      _BlowfishSettings.Action,
                      ref _InputStream,
                      ref _OutputStreamWriter,
                      _InputKey,
                      _InputIV,
                      _BlowfishSettings.Padding,
                      ref _stop,
                      ProgressChanged,
                      ref _lastInputBlock,
                      blocksize);
                    break;
                case BlockMode.OFB:
                    CheckIV();
                    ExecuteOFB(blockCipher,
                      _BlowfishSettings.Action,
                      ref _InputStream,
                      ref _OutputStreamWriter,
                      _InputKey,
                      _InputIV,
                      _BlowfishSettings.Padding,
                      ref _stop,
                      ProgressChanged,
                      ref _lastInputBlock,
                      blocksize);
                    break;
                default:
                    throw new NotImplementedException(string.Format("The mode {0} has not been implemented.", _BlowfishSettings.BlockMode));
            }

            OnPropertyChanged("OutputStream");

            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Returns instance of Threefish based on provided key length
        /// hint: key length is automatically "fixed" by adding zeros if CT2 user
        /// provided a key that was too short or too long
        /// </summary>
        /// <returns></returns>
        private ThreefishAlgorithm GetThreefishAlgorithm()
        {
            if (_InputKey.Length == 32)
            {
                return new Threefish256();
            }
            else if (_InputKey.Length == 64)
            {
                return new Threefish512();
            }
            else if (_InputKey.Length == 128)
            {
                return new Threefish1024();
            }
            throw new ArgumentException("Provided key for Threefish has not length 256 bit, 512 bit, or 1024 bit");
        }

        /// <summary>
        /// Checks the given key and extends/cuts it, if needed
        /// </summary>
        private void CheckKeyLength()
        {
            if (_BlowfishSettings.BlowfishAlgorithmType == BlowfishAlgorithmType.Blowfish)
            {
                //Blowfish specifies key lengths between 32 bit (4 bytes) and 448 bit (56 bytes)
                //usual case is 16 byte (= 128 bit key)
                if (_InputKey.Length < 4)
                {
                    byte[] key = new byte[4];
                    Array.Copy(_InputKey, 0, key, 0, _InputKey.Length);
                    GuiLogMessage(string.Format(Resources.Blowfish_Execute_Key_too_short, _InputKey.Length, 4), NotificationLevel.Warning);
                    _InputKey = key;
                }
                if (_InputKey.Length > 56)
                {
                    byte[] key = new byte[56];
                    Array.Copy(_InputKey, 0, key, 0, 56);
                    GuiLogMessage(string.Format(Resources.Blowfish_Execute_Key_too_long, _InputKey.Length, 56), NotificationLevel.Warning);
                    _InputKey = key;
                }
            }
            else if (_BlowfishSettings.BlowfishAlgorithmType == BlowfishAlgorithmType.Twofish)
            {
                //Twofish specifies keylengths of 128 bit (16 byte), 192 bit (24 byte), and 256 bit (32 byte)
                if (_InputKey.Length < 16)
                {
                    byte[] key = new byte[16];
                    Array.Copy(_InputKey, 0, key, 0, _InputKey.Length);
                    GuiLogMessage(string.Format(Resources.Blowfish_Execute_Key_too_short, _InputKey.Length, 16), NotificationLevel.Warning);
                    _InputKey = key;
                }
                else if (_InputKey.Length != 16 && _InputKey.Length < 24)
                {
                    byte[] key = new byte[24];
                    Array.Copy(_InputKey, 0, key, 0, _InputKey.Length);
                    GuiLogMessage(string.Format(Resources.Blowfish_Execute_Key_too_short, _InputKey.Length, 24), NotificationLevel.Warning);
                    _InputKey = key;
                }
                else if (_InputKey.Length != 16 && _InputKey.Length != 24 && _InputKey.Length < 32)
                {
                    byte[] key = new byte[32];
                    Array.Copy(_InputKey, 0, key, 0, _InputKey.Length);
                    GuiLogMessage(string.Format(Resources.Blowfish_Execute_Key_too_short, _InputKey.Length, 32), NotificationLevel.Warning);
                    _InputKey = key;
                }
                else if (_InputKey.Length > 32)
                {
                    byte[] key = new byte[32];
                    Array.Copy(_InputKey, 0, key, 0, 32);
                    GuiLogMessage(string.Format(Resources.Blowfish_Execute_Key_too_long, _InputKey.Length, 32), NotificationLevel.Warning);
                    _InputKey = key;
                }
            }
            else if (_BlowfishSettings.BlowfishAlgorithmType == BlowfishAlgorithmType.Threefish)
            {
                //Threefish specifies keylengths of 256 bit (32 byte), 512 bit (64 byte), and 1024 bit (128 byte)
                if (_InputKey.Length < 32)
                {
                    byte[] key = new byte[32];
                    Array.Copy(_InputKey, 0, key, 0, _InputKey.Length);
                    GuiLogMessage(string.Format(Resources.Blowfish_Execute_Key_too_short, _InputKey.Length, 32), NotificationLevel.Warning);
                    _InputKey = key;
                }
                else if (_InputKey.Length != 32 && _InputKey.Length < 64)
                {
                    byte[] key = new byte[64];
                    Array.Copy(_InputKey, 0, key, 0, _InputKey.Length);
                    GuiLogMessage(string.Format(Resources.Blowfish_Execute_Key_too_short, _InputKey.Length, 64), NotificationLevel.Warning);
                    _InputKey = key;
                }
                else if (_InputKey.Length != 32 && _InputKey.Length != 64 && _InputKey.Length < 128)
                {
                    byte[] key = new byte[128];
                    Array.Copy(_InputKey, 0, key, 0, _InputKey.Length);
                    GuiLogMessage(string.Format(Resources.Blowfish_Execute_Key_too_short, _InputKey.Length, 128), NotificationLevel.Warning);
                    _InputKey = key;
                }
                else if (_InputKey.Length > 128)
                {
                    byte[] key = new byte[128];
                    Array.Copy(_InputKey, 0, key, 0, 128);
                    GuiLogMessage(string.Format(Resources.Blowfish_Execute_Key_too_long, _InputKey.Length, 128), NotificationLevel.Warning);
                    _InputKey = key;
                }
            }
        }

        /// <summary>
        /// Checks the given initialization vector and extends/cuts it, if needed
        /// </summary>
        private void CheckIV()
        {
            int blocksize = 8;
            if (_BlowfishSettings.BlowfishAlgorithmType == BlowfishAlgorithmType.Twofish)
            {
                blocksize = 16;
            }
            if (_BlowfishSettings.BlowfishAlgorithmType == BlowfishAlgorithmType.Threefish)
            {
                //blocksize of Threefish is the same as the keylength
                blocksize = _InputKey.Length;
            }
            //if no IV is given, we set it to an array with length 0
            if (_InputIV == null)
            {
                _InputIV = new byte[0];
            }
            //Extend or cut IV to length 8
            if (_InputIV.Length < blocksize)
            {
                byte[] iv = new byte[blocksize];
                Array.Copy(_InputIV, 0, iv, 0, _InputIV.Length);
                GuiLogMessage(string.Format(Resources.Blowfish_CheckIV_IV_too_short, _InputIV.Length, blocksize), NotificationLevel.Warning);
                _InputIV = iv;
            }
            if (_InputIV.Length > blocksize)
            {
                byte[] iv = new byte[blocksize];
                Array.Copy(_InputIV, 0, iv, 0, blocksize);
                GuiLogMessage(string.Format(Resources.Blowfish_CheckIV_IV_too_long, _InputIV.Length, blocksize), NotificationLevel.Warning);
                _InputIV = iv;
            }
        }

        /// <summary>
        /// Checks if Threefish's tweak is too long or too short
        /// Default tweak is all set to zero
        /// </summary>
        private void CheckTweak()
        {
            //if no tweak is given, we set it to an array with length 24
            if (_InputTweak == null)
            {
                //default tweak is all set to zero
                _InputTweak = new byte[16];
            }
            //Extend or cut tweak to length 24
            if (_InputTweak.Length < 16)
            {
                byte[] tweak = new byte[16];
                Array.Copy(_InputTweak, 0, tweak, 0, _InputIV.Length);
                GuiLogMessage(string.Format(Resources.Blowfish_CheckTweak_Tweak_too_short, _InputTweak.Length, 16), NotificationLevel.Warning);
                _InputTweak = tweak;
            }
            if (_InputTweak.Length > 16)
            {
                byte[] tweak = new byte[24];
                Array.Copy(_InputTweak, 0, tweak, 0, 16);
                GuiLogMessage(string.Format(Resources.Blowfish_CheckTweak_Tweak_too_long, _InputTweak.Length, 16), NotificationLevel.Warning);
                _InputTweak = tweak;
            }
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
            _stop = true;
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

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        /// <summary>
        /// Helper method to invoke property change event
        /// </summary>
        /// <param name="name"></param>
        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Helper method to invoke progress changed event
        /// </summary>
        /// <param name="value"></param>
        /// <param name="max"></param>
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}
