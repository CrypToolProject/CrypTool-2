/*
   Copyright 2020 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using CrypTool.Plugins.FEAL.Properties;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using static CrypTool.PluginBase.Miscellaneous.BlockCipherHelper;

namespace CrypTool.Plugins.FEAL
{
    [Author("Nils Kopal", "Nils.Kopal@CrypTool.org", "CrypTool 2 Team", "https://www.CrypTool.org")]
    [PluginInfo("CrypTool.Plugins.Feal.Properties.Resources", "PluginCaption", "PluginTooltip", "", "FEAL/Images/icon.png")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class FEAL : ICrypComponent
    {
        #region Private Variables

        private FEALSettings _FEALSettings = new FEALSettings();
        private readonly UserControl _presentation = new FEALPresentation();

        private byte[] _InputKey;
        private byte[] _InputIV;
        private ICrypToolStream _OutputStreamWriter;
        private ICrypToolStream _InputStream;

        private bool _stop = false;

        private byte[] _lastInputBlock = null; //needed for the visualization of the cipher; we only show the last encrypted/decrypted block

        #endregion

        #region Data Properties

        public ISettings Settings
        {
            get => _FEALSettings;
            set => _FEALSettings = (FEALSettings)value;
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

        /// <summary>
        /// Constructor
        /// </summary>
        public FEAL()
        {
            _FEALSettings.PropertyChanged += _FEALSettings_PropertyChanged;
            UpdatePresentation();
        }

        /// <summary>
        /// Called, when a property of the settings changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _FEALSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //shows the visualization of the selected FEAL algorithm type
            if (e.PropertyName.Equals("FealAlgorithmType"))
            {
                UpdatePresentation();
            }
        }

        /// <summary>
        /// Updates the presentation by only showing the selected presentation
        /// </summary>
        private void UpdatePresentation()
        {
            FEALPresentation fealPresentation = (FEALPresentation)_presentation;

            fealPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    if (_FEALSettings.FealAlgorithmType == FealAlgorithmType.FEAL4)
                    {
                        fealPresentation.ShowFEAL4Presentation();
                    }
                    else if (_FEALSettings.FealAlgorithmType == FealAlgorithmType.FEAL8)
                    {
                        fealPresentation.ShowFEAL8Presentation();
                    }
                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format("Exception occured during updating of visualization: {0}", ex.Message), NotificationLevel.Error);
                }
            }, null);
        }

        #region IPlugin Members

        public UserControl Presentation => _presentation;

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

            //Extend or cut key to length 8
            if (_InputKey.Length < 8)
            {
                byte[] key = new byte[8];
                Array.Copy(_InputKey, 0, key, 0, _InputKey.Length);
                GuiLogMessage(string.Format(Resources.FEAL_Execute_Key_too_short, _InputKey.Length), NotificationLevel.Warning);
                _InputKey = key;
            }
            if (_InputKey.Length > 8)
            {
                byte[] key = new byte[8];
                Array.Copy(_InputKey, 0, key, 0, 8);
                GuiLogMessage(string.Format(Resources.FEAL_Execute_Key_too_long, _InputKey.Length), NotificationLevel.Warning);
                _InputKey = key;
            }

            //Select crypto function based on algorithm, blockmode, and action
            BlockCipher blockCipher = null;
            if (_FEALSettings.FealAlgorithmType == FealAlgorithmType.FEAL4 && _FEALSettings.BlockMode == BlockMode.CFB)
            {
                blockCipher = new BlockCipher(FEAL_Algorithms.FEAL4_EncryptBlock);
            }
            else if (_FEALSettings.FealAlgorithmType == FealAlgorithmType.FEAL8 && _FEALSettings.BlockMode == BlockMode.CFB)
            {
                blockCipher = new BlockCipher(FEAL_Algorithms.FEAL8_EncryptBlock); //uses encryption since it only XORs result of cipher
            }
            else if (_FEALSettings.FealAlgorithmType == FealAlgorithmType.FEAL4 && _FEALSettings.BlockMode == BlockMode.OFB)
            {
                blockCipher = new BlockCipher(FEAL_Algorithms.FEAL4_EncryptBlock);
            }
            else if (_FEALSettings.FealAlgorithmType == FealAlgorithmType.FEAL8 && _FEALSettings.BlockMode == BlockMode.OFB)
            {
                blockCipher = new BlockCipher(FEAL_Algorithms.FEAL8_EncryptBlock); //uses encryption since it only XORs result of cipher
            }
            else if (_FEALSettings.FealAlgorithmType == FealAlgorithmType.FEAL4 && _FEALSettings.Action == CipherAction.Encrypt)
            {
                blockCipher = new BlockCipher(FEAL_Algorithms.FEAL4_EncryptBlock);
            }
            else if (_FEALSettings.FealAlgorithmType == FealAlgorithmType.FEAL4 && _FEALSettings.Action == CipherAction.Decrypt)
            {
                blockCipher = new BlockCipher(FEAL_Algorithms.FEAL4_DecryptBlock);
            }
            else if (_FEALSettings.FealAlgorithmType == FealAlgorithmType.FEAL8 && _FEALSettings.Action == CipherAction.Encrypt)
            {
                blockCipher = new BlockCipher(FEAL_Algorithms.FEAL8_EncryptBlock);
            }
            else if (_FEALSettings.FealAlgorithmType == FealAlgorithmType.FEAL8 && _FEALSettings.Action == CipherAction.Decrypt)
            {
                blockCipher = new BlockCipher(FEAL_Algorithms.FEAL8_DecryptBlock);
            }

            //Check, if we found a crypto function that we can use
            //this error should NEVER occur. Only in case someone adds functionality and misses
            //to create a valid configuration
            if (blockCipher == null)
            {
                GuiLogMessage("No crypto function could be selected based on your settings", NotificationLevel.Error);
                return;
            }

            if (_FEALSettings.Action == CipherAction.Encrypt)
            {
                //in case of encryption, we have to add padding
                _InputStream = AppendPadding(InputStream, _FEALSettings.Padding, 8);
            }
            else
            {
                //with decryption, we have to do nothing
                _InputStream = InputStream;
            }

            //parity rule is: if parity is used, zero the least significant bit of each byte
            if (_FEALSettings.EnableKeyParityBits == true)
            {
                for (int i = 0; i < _InputKey.Length; i++)
                {
                    _InputKey[i] = (byte)(_InputKey[i] & 254);
                }
            }

            switch (_FEALSettings.BlockMode)
            {
                case BlockMode.ECB:
                    ExecuteECB(blockCipher,
                        _FEALSettings.Action,
                        ref _InputStream,
                        ref _OutputStreamWriter,
                        _InputKey,
                        _FEALSettings.Padding,
                        ref _stop,
                        ProgressChanged,
                        ref _lastInputBlock);
                    break;
                case BlockMode.CBC:
                    CheckIV();
                    ExecuteCBC(blockCipher,
                       _FEALSettings.Action,
                       ref _InputStream,
                       ref _OutputStreamWriter,
                       _InputKey,
                       _InputIV,
                       _FEALSettings.Padding,
                       ref _stop,
                       ProgressChanged,
                       ref _lastInputBlock);
                    break;
                case BlockMode.CFB:
                    CheckIV();
                    ExecuteCFB(blockCipher,
                      _FEALSettings.Action,
                      ref _InputStream,
                      ref _OutputStreamWriter,
                      _InputKey,
                      _InputIV,
                      _FEALSettings.Padding,
                      ref _stop,
                      ProgressChanged,
                      ref _lastInputBlock);
                    break;
                case BlockMode.OFB:
                    CheckIV();
                    ExecuteOFB(blockCipher,
                      _FEALSettings.Action,
                      ref _InputStream,
                      ref _OutputStreamWriter,
                      _InputKey,
                      _InputIV,
                      _FEALSettings.Padding,
                      ref _stop,
                      ProgressChanged,
                      ref _lastInputBlock);
                    break;
                default:
                    throw new NotImplementedException(string.Format("The mode {0} has not been implemented.", _FEALSettings.BlockMode));
            }

            if (_lastInputBlock != null)
            {
                FEALPresentation fealPresentation = (FEALPresentation)_presentation;
                fealPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        if (_FEALSettings.Action == BlockCipherHelper.CipherAction.Encrypt)
                        {
                            fealPresentation.VisualizeEncryptBlock(_lastInputBlock, _InputKey);
                        }
                        else
                        {
                            fealPresentation.VisualizeDecryptBlock(_lastInputBlock, _InputKey);
                        }
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage(string.Format("Exception occured during building of visualization: {0}", ex.Message), NotificationLevel.Error);
                    }
                }, null);
            }
            OnPropertyChanged("OutputStream");

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
            //Extend or cut IV to length 8
            if (_InputIV.Length < 8)
            {
                byte[] iv = new byte[8];
                Array.Copy(_InputIV, 0, iv, 0, _InputIV.Length);
                GuiLogMessage(string.Format(Resources.FEAL_CheckIV_IV_too_short, _InputIV.Length), NotificationLevel.Warning);
                _InputIV = iv;
            }
            if (_InputIV.Length > 8)
            {
                byte[] iv = new byte[8];
                Array.Copy(_InputIV, 0, iv, 0, 8);
                GuiLogMessage(string.Format(Resources.FEAL_CheckIV_IV_too_long, _InputIV.Length), NotificationLevel.Warning);
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
