/*
   Copyright 2023 Nils Kopal, CrypTool project

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

namespace CrypTool.Plugins.SAES
{
    [Author("Nils Kopal", "kopal@CrypTool.org", "CrypTool 2 Team", "https://www.CrypTool.org")]
    [PluginInfo("CrypTool.Plugins.SAES.Properties.Resources", "PluginCaption", "PluginTooltip", "SAES/DetailedDescription/doc.xml", "SAES/images/icon.png")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class SAES : ICrypComponent
    {
        #region Private Variables

        private readonly SAESSettings _settings = new SAESSettings();
        private byte[] _inputKey;
        private byte[] _inputIV;

        private ICrypToolStream _outputStreamWriter;
        private ICrypToolStream _inputStream;
        private bool _stop = false;

        #endregion

        #region Data Properties

        public ISettings Settings => _settings;
        public UserControl Presentation => null;

        [PropertyInfo(Direction.InputData, "InputStreamCaption", "InputStreamTooltip", true)]
        public ICrypToolStream InputStream
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "InputKeyCaption", "InputKeyTooltip", true)]
        public byte[] InputKey
        {
            get => _inputKey;
            set => _inputKey = value;
        }

        [PropertyInfo(Direction.InputData, "InputIVCaption", "InputIVTooltip", false)]
        public byte[] InputIV
        {
            get => _inputIV;
            set => _inputIV = value;
        }
    
        [PropertyInfo(Direction.OutputData, "OutputStreamCaption", "OutputStreamTooltip", true)]
        public ICrypToolStream OutputStream
        {
            get => _outputStreamWriter;
            set
            {

            }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public SAES()
        {

        }

        #region IPlugin Members        

        /// <summary>
        /// Called once when workflow execution starts
        /// </summary>
        public void PreExecution()
        {
            _inputIV = null;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);

            _outputStreamWriter = null;
            _stop = false;

            //Extend or cut key to length 16 (= 128 bit)
            if (_inputKey.Length < 2)
            {
                byte[] key = new byte[2];
                Array.Copy(_inputKey, 0, key, 0, _inputKey.Length);
                GuiLogMessage(string.Format(Properties.Resources.KeyTooShort, _inputKey.Length), NotificationLevel.Warning);
                _inputKey = key;
            }
            else if (_inputKey.Length > 2)
            {
                byte[] key = new byte[2];
                Array.Copy(_inputKey, 0, key, 0, 2);
                GuiLogMessage(string.Format(Properties.Resources.KeyTooLong, _inputKey.Length), NotificationLevel.Warning);
                _inputKey = key;
            }

            //apply padding
            if (_settings.Action == CipherAction.Encrypt)
            {
                //in case of encryption, we have to add padding
                _inputStream = AppendPadding(InputStream, _settings.Padding, 2);
            }
            else
            {
                //with decryption, we have to do nothing
                _inputStream = InputStream;
            }                       

            SAESAlgorithm saes = new SAESAlgorithm();
          
            //Create reference to encrypt or decrypt method of SAES
            BlockCipher blockCipher = null;
            if (_settings.Action == CipherAction.Encrypt ||
                _settings.BlockMode == BlockMode.CFB ||
                _settings.BlockMode == BlockMode.OFB)
            {
                blockCipher = new BlockCipher(saes.EncryptBlock);
            }
            else
            {
                blockCipher = new BlockCipher(saes.DecryptBlock);
            }

            //Perform actual encryption or decryption using the helper methods of
            //BlockCipherHelper class located in CrypPluginBase
            byte[] lastInputBlock = null;
            switch (_settings.BlockMode)
            {
                case BlockMode.ECB:
                    ExecuteECB(blockCipher,
                        _settings.Action,
                        ref _inputStream,
                        ref _outputStreamWriter,
                        _inputKey,
                        _settings.Padding,
                        ref _stop,
                        ProgressChanged,
                        ref lastInputBlock,
                        2);
                    break;
                case BlockMode.CBC:
                    CheckIV();
                    ExecuteCBC(blockCipher,
                       _settings.Action,
                       ref _inputStream,
                       ref _outputStreamWriter,
                       _inputKey,
                       _inputIV,
                       _settings.Padding,
                       ref _stop,
                       ProgressChanged,
                       ref lastInputBlock,
                       2);
                    break;
                case BlockMode.CFB:
                    CheckIV();
                    ExecuteCFB(blockCipher,
                      _settings.Action,
                      ref _inputStream,
                      ref _outputStreamWriter,
                      _inputKey,
                      _inputIV,
                      _settings.Padding,
                      ref _stop,
                      ProgressChanged,
                      ref lastInputBlock,
                      2);
                    break;
                case BlockMode.OFB:
                    CheckIV();
                    ExecuteOFB(blockCipher,
                      _settings.Action,
                      ref _inputStream,
                      ref _outputStreamWriter,
                      _inputKey,
                      _inputIV,
                      _settings.Padding,
                      ref _stop,
                      ProgressChanged,
                      ref lastInputBlock,
                      2);
                    break;
                default:
                    throw new NotImplementedException(string.Format("The mode {0} has not been implemented.", _settings.BlockMode));
            }           

            OnPropertyChanged(nameof(OutputStream));
            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Checks the given initialization vector and extends/cuts it, if needed
        /// </summary>
        private void CheckIV()
        {
            //if no IV is given, we set it to an array with length 0
            if (_inputIV == null)
            {
                _inputIV = new byte[0];
            }
            //Extend or cut IV to length 2
            if (_inputIV.Length < 2)
            {
                byte[] iv = new byte[2];
                Array.Copy(_inputIV, 0, iv, 0, _inputIV.Length);
                GuiLogMessage(string.Format(Properties.Resources.IVTooShort, _inputIV.Length), NotificationLevel.Warning);
                _inputIV = iv;
            }
            if (_inputIV.Length > 2)
            {
                byte[] iv = new byte[2];
                Array.Copy(_inputIV, 0, iv, 0, 8);
                GuiLogMessage(string.Format(Properties.Resources.IVTooLong, _inputIV.Length), NotificationLevel.Warning);
                _inputIV = iv;
            }
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

        public void PostExecution()
        {

        }

        public void Stop()
        {
            _stop = true;
        }

        public void Initialize()
        {

        }

        public void Dispose()
        {

        }

        #endregion
    }
}
