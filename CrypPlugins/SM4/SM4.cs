/*                              
   Copyright 2024 Nils Kopal, CrypTool Team

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
using static CrypTool.PluginBase.Miscellaneous.BlockCipherHelper;

namespace CrypTool.Plugins.SM4
{
    [Author("Nils Kopal", "nils.kopal@cryptool.org", "CrypTool 2 Team", "https://www.cryptool.org")]
    [PluginInfo("CrypTool.Plugins.SM4.Properties.Resources", "PluginCaption", "PluginTooltip", "SM4/doc.xml", "SM4/icon.png")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class SM4 : ICrypComponent
    {
        #region Private Variables

        private SM4Settings _SM4Settings = new SM4Settings();
        private byte[] _InputKey;
        private byte[] _InputIV;
        private ICrypToolStream _OutputStreamWriter;
        private ICrypToolStream _InputStream;
        private bool _stop = false;

        #endregion

        #region Data Properties

        public ISettings Settings
        {
            get => _SM4Settings;
            set => _SM4Settings = (SM4Settings)value;
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

        #region IPlugin Members        

        public UserControl Presentation
        {
            get { return null; }
        }

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

            _OutputStreamWriter = null;
            _stop = false;

            //Extend or cut key to length 16
            if (_InputKey.Length < 16)
            {
                byte[] key = new byte[16];
                Array.Copy(_InputKey, 0, key, 0, _InputKey.Length);
                GuiLogMessage(string.Format(Properties.Resources.SM4_Execute_Key_too_short, _InputKey.Length), NotificationLevel.Warning);
                _InputKey = key;
            }
            if (_InputKey.Length > 16)
            {
                byte[] key = new byte[16];
                Array.Copy(_InputKey, 0, key, 0, 16);
                GuiLogMessage(string.Format(Properties.Resources.SM4_Execute_Key_too_long, _InputKey.Length), NotificationLevel.Warning);
                _InputKey = key;
            }

            //Select crypto function based on blockmode and action
            BlockCipher blockCipher = null;
            if (_SM4Settings.BlockMode == BlockMode.CFB)
            {
                blockCipher = new BlockCipher(SM4Cipher.EncryptBlock); //uses always encryption since it only XORs the result of cipher
            }
            if (_SM4Settings.BlockMode == BlockMode.OFB)
            {
                blockCipher = new BlockCipher(SM4Cipher.EncryptBlock); //uses always encryption since it only XORs the result of cipher
            }
            else if (_SM4Settings.Action == CipherAction.Encrypt)
            {
                blockCipher = new BlockCipher(SM4Cipher.EncryptBlock);
            }
            else if (_SM4Settings.Action == CipherAction.Decrypt)
            {
                blockCipher = new BlockCipher(SM4Cipher.DecryptBlock);
            }
            else if (_SM4Settings.Action == CipherAction.Encrypt)
            {
                blockCipher = new BlockCipher(SM4Cipher.EncryptBlock);
            }
            else if (_SM4Settings.Action == CipherAction.Decrypt)
            {
                blockCipher = new BlockCipher(SM4Cipher.DecryptBlock);
            }

            //Check, if we found a crypto function that we can use
            //This error should NEVER occur. Only in case someone adds functionality and misses
            //to create a valid configuration
            if (blockCipher == null)
            {
                GuiLogMessage("No crypto function could be selected based on your settings", NotificationLevel.Error);
                return;
            }

            if (_SM4Settings.Action == CipherAction.Encrypt)
            {
                //in case of encryption, we have to add padding
                _InputStream = AppendPadding(InputStream, _SM4Settings.Padding, 16);
            }
            else
            {
                //with decryption, we have to do nothing
                _InputStream = InputStream;
            }

            byte[] lastInputBlock = null; //only needed, if we want to build a presentation. Todo: Build presentation

            //Perform the actual encryption/decryption
            switch (_SM4Settings.BlockMode)
            {
                case BlockMode.ECB:
                    ExecuteECB(blockCipher,
                        _SM4Settings.Action,
                        ref _InputStream,
                        ref _OutputStreamWriter,
                        _InputKey,
                        _SM4Settings.Padding,
                        ref _stop,
                        ProgressChanged,
                        ref lastInputBlock,
                        16);
                    break;
                case BlockMode.CBC:
                    CheckIV();
                    ExecuteCBC(blockCipher,
                       _SM4Settings.Action,
                       ref _InputStream,
                       ref _OutputStreamWriter,
                       _InputKey,
                       _InputIV,
                       _SM4Settings.Padding,
                       ref _stop,
                       ProgressChanged,
                       ref lastInputBlock,
                       16);
                    break;
                case BlockMode.CFB:
                    CheckIV();
                    ExecuteCFB(blockCipher,
                      _SM4Settings.Action,
                      ref _InputStream,
                      ref _OutputStreamWriter,
                      _InputKey,
                      _InputIV,
                      _SM4Settings.Padding,
                      ref _stop,
                      ProgressChanged,
                      ref lastInputBlock,
                      16);
                    break;
                case BlockMode.OFB:
                    CheckIV();
                    ExecuteOFB(blockCipher,
                      _SM4Settings.Action,
                      ref _InputStream,
                      ref _OutputStreamWriter,
                      _InputKey,
                      _InputIV,
                      _SM4Settings.Padding,
                      ref _stop,
                      ProgressChanged,
                      ref lastInputBlock,
                      16);
                    break;
                default:
                    throw new NotImplementedException(string.Format("The mode {0} has not been implemented.", _SM4Settings.BlockMode));
            }

            //output the result by notifying that the output stream has changed
            OnPropertyChanged(nameof(OutputStream));

            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Checks the given initialization vector and extends/cuts it, if needed
        /// </summary>
        private void CheckIV()
        {
            //if no IV is given, we set it to an array with length 0
            if (_InputIV == null)
            {
                _InputIV = new byte[0];
            }
            //Extend or cut IV to length 16
            if (_InputIV.Length < 16)
            {
                byte[] iv = new byte[16];
                Array.Copy(_InputIV, 0, iv, 0, _InputIV.Length);
                GuiLogMessage(string.Format(Properties.Resources.SM4_CheckIV_IV_too_short, _InputIV.Length), NotificationLevel.Warning);
                _InputIV = iv;
            }
            if (_InputIV.Length > 16)
            {
                byte[] iv = new byte[16];
                Array.Copy(_InputIV, 0, iv, 0, 16);
                GuiLogMessage(string.Format(Properties.Resources.SM4_CheckIV_IV_too_long, _InputIV.Length), NotificationLevel.Warning);
                _InputIV = iv;
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