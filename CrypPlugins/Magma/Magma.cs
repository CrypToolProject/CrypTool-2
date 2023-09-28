/*
   Copyright 2022 Nils Kopal, CrypTool project

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
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using static CrypTool.PluginBase.Miscellaneous.BlockCipherHelper;

namespace CrypTool.Plugins.Magma
{
    /// <summary>
    /// Implementation of the GOST block cipher "Magma", defined in the standard: GOST R 34.12-2015: Block Cipher "Magma"
    /// Standard:  https://datatracker.ietf.org/doc/html/rfc8891
    /// Wikipedia: https://en.wikipedia.org/wiki/GOST_(block_cipher)
    /// Magma is a Soviet and Russian government standard symmetric key block cipher with a block size of 64 bits and a keysize of 256 bits
    /// </summary>
    [Author("Nils Kopal", "kopal@CrypTool.org", "CrypTool 2 Team", "https://www.CrypTool.org")]
    [PluginInfo("CrypTool.Plugins.Magma.Properties.Resources", "PluginCaption", "PluginTooltip", "Magma/DetailedDescription/doc.xml", new[] { "Magma/Images/icon.png" })]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class Magma : ICrypComponent
    {
        #region Private Variables

        private readonly MagmaSettings _settings = new MagmaSettings();
        private readonly MagmaPresentation _presentation = new MagmaPresentation();
        private byte[] _inputKey;
        private byte[] _inputIV;
        private byte[] _sboxes;
        private ICrypToolStream _outputStreamWriter;
        private ICrypToolStream _inputStream;
        private bool _stop = false;

        #endregion

        #region Data Properties

        public ISettings Settings => _settings;
        public UserControl Presentation => _presentation;

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

        [PropertyInfo(Direction.InputData, "SBoxesCaption", "InputSBoxesTooltip", false)]
        public byte[] SBoxes
        {
            get => _sboxes;
            set => _sboxes = value;
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
        public Magma()
        {
            
        }        

        #region IPlugin Members        

        /// <summary>
        /// Called once when workflow execution starts
        /// </summary>
        public void PreExecution()
        {
            _sboxes = null;
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

            //Extend or cut key to length 32 (= 256 bit)
            if (_inputKey.Length < 32)
            {
                byte[] key = new byte[32];
                Array.Copy(_inputKey, 0, key, 0, _inputKey.Length);
                GuiLogMessage(string.Format(Properties.Resources.KeyTooShort, _inputKey.Length), NotificationLevel.Warning);
                _inputKey = key;
            }
            else  if (_inputKey.Length > 32)
            {
                byte[] key = new byte[32];
                Array.Copy(_inputKey, 0, key, 0, 32);
                GuiLogMessage(string.Format(Properties.Resources.KeyTooLong, _inputKey.Length), NotificationLevel.Warning);
                _inputKey = key;
            }

            if(_sboxes != null && _sboxes.Length != 128) // we need 8 x 16 bytes
            {
                GuiLogMessage(String.Format(Properties.Resources.WrongSBoxesLength, _sboxes.Length), NotificationLevel.Error);
                return;
            }

            //apply padding
            if (_settings.Action == CipherAction.Encrypt)
            {
                //in case of encryption, we have to add padding
                _inputStream = AppendPadding(InputStream, _settings.Padding, 8);
            }
            else
            {
                //with decryption, we have to do nothing
                _inputStream = InputStream;
            }

            //convert key to UInt32 array
            UInt32[] keyArray = new UInt32[]
            {
                (UInt32)(_inputKey[0]  << 24 | _inputKey[1]  << 16 | _inputKey[2]  << 8 | _inputKey[3]),
                (UInt32)(_inputKey[4]  << 24 | _inputKey[5]  << 16 | _inputKey[6]  << 8 | _inputKey[7]),
                (UInt32)(_inputKey[8]  << 24 | _inputKey[9]  << 16 | _inputKey[10] << 8 | _inputKey[11]),
                (UInt32)(_inputKey[12] << 24 | _inputKey[13] << 16 | _inputKey[14] << 8 | _inputKey[15]),
                (UInt32)(_inputKey[16] << 24 | _inputKey[17] << 16 | _inputKey[18] << 8 | _inputKey[19]),
                (UInt32)(_inputKey[20] << 24 | _inputKey[21] << 16 | _inputKey[22] << 8 | _inputKey[23]),
                (UInt32)(_inputKey[24] << 24 | _inputKey[25] << 16 | _inputKey[26] << 8 | _inputKey[27]),
                (UInt32)(_inputKey[28] << 24 | _inputKey[29] << 16 | _inputKey[30] << 8 | _inputKey[31])
            };

            //creates the Magma algorithm with the selected or provided x-boxes
            MagmaAlgorithm magma;
            if (_sboxes == null) //no S-boxes given by the user
            {
                switch (_settings.SBoxes)
                {
                    case Plugins.Magma.SBoxes.GOST_R_34_12_2015N:
                        magma = new MagmaAlgorithm(MagmaAlgorithm.SBOX_GOST_R_34_12_2015N, keyArray);
                        break;
                    case Plugins.Magma.SBoxes.CENTRAL_BANK_OF_RUSSIAN_FEDERATION:
                        magma = new MagmaAlgorithm(MagmaAlgorithm.SBOX_CENTRAL_BANK_OF_RUSSIAN_FEDERATION, keyArray);
                        break;
                    default:
                        magma = new MagmaAlgorithm(MagmaAlgorithm.SBOX_GOST_R_34_12_2015N, keyArray);
                        break;
                }
            }
            else // S-boxes provided by the user
            {                   
                magma = new MagmaAlgorithm(_sboxes, keyArray);
            }

            //Create reference to encrypt or decrypt method of Magma
            BlockCipher blockCipher = null;
            if (_settings.Action == CipherAction.Encrypt || 
                _settings.BlockMode == BlockMode.CFB || 
                _settings.BlockMode == BlockMode.OFB)
            {
                blockCipher = new BlockCipher(magma.EncryptBlock);
            }
            else
            {
                blockCipher = new BlockCipher(magma.DecryptBlock);
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
                        ref lastInputBlock);
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
                       ref lastInputBlock);
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
                      ref lastInputBlock);
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
                      ref lastInputBlock);
                    break;
                default:
                    throw new NotImplementedException(string.Format("The mode {0} has not been implemented.", _settings.BlockMode));
            }

            //visualize data of last block (stored during execution of Magma):
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    //here, we put the data from the last encryption or decryption to the ui for visualization
                    if (_settings.Action == CipherAction.Encrypt)
                    {
                        _presentation.VisualizeCompleteDataEncryption(magma.MagmaCompleteData);
                    }
                    else
                    {
                        _presentation.VisualizeCompleteDataDecryption(magma.MagmaCompleteData);
                    }
                }
                catch(Exception)
                {
                    //do nothing
                }
            }, null);

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
            //Extend or cut IV to length 8
            if (_inputIV.Length < 8)
            {
                byte[] iv = new byte[8];
                Array.Copy(_inputIV, 0, iv, 0, _inputIV.Length);
                GuiLogMessage(string.Format(Properties.Resources.IVTooShort, _inputIV.Length), NotificationLevel.Warning);
                _inputIV = iv;
            }
            if (_inputIV.Length > 8)
            {
                byte[] iv = new byte[8];
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
