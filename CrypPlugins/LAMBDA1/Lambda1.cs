/*
   Copyright 2011 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using System.ComponentModel;
using System.Windows.Controls;
using System;
using System.IO;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;


namespace CrypTool.Plugins.LAMBDA1
{
    [Author("Michael Altenhuber", "michael@altenhuber.net", "CrypTool 2 Team", "http://CrypTool2.vs.uni-due.de")]
    [PluginInfo("CrypTool.Plugins.LAMBDA1.Properties.Resources", "PluginCaption", "PluginTooltip", "LAMBDA1/userdoc.xml", "LAMBDA1/images/t316_icon.png")]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class LAMBDA1 : ICrypComponent
    {
        private readonly LAMBDA1Settings settings = new LAMBDA1Settings();
        private bool stopPressed = false;
        private byte[] iV;


        #region Data Properties


        [PropertyInfo(Direction.InputData, "InputData", "InputDataTooltip")]
        public byte[] InputData
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "InputKey", "InputKeyTooltip")]
        public byte[] InputKey
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "InputIV", "InputIVTooltip", false)]
        public byte[] InputIV
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputData", "OutputDataTooltip")]
        public byte[] OutputData
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members
        public ISettings Settings
        {
            get { return settings; }
        }

        public UserControl Presentation
        {
            get { return null; }
        }


        public void PreExecution()
        {
            // empty
        }

        public void Execute()
        {
            ProgressChanged(0, 1);
            stopPressed = false;

            if (!CheckInput())
                return;

            if (settings.Mode == OperationMode.Encrypt)
                PrepareAndEncrypt();
            else
                PrepareAndDecrypt();

        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            Dispose();
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// </summary>
        public void Stop()
        {
            stopPressed = true;
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
            iV = new byte[LAMBDA1Algorithm.BlockSize];
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
            InputData = null;
            InputKey = null;
            OutputData = null;
        }

        #endregion

        #region Private Functions

        /// <summary>
        /// Takes the InputData and encrypts it with the LAMBDA1-Algorithm
        /// </summary>
        private void PrepareAndEncrypt()
        {
            MemoryStream outputStream = new MemoryStream();
          
            if (InputIV != null)
            {
                if (InputIV.Length != LAMBDA1Algorithm.BlockSize)
                {
                    GuiLogMessage(
                         String.Format(
                             Properties.Resources.ErrorIVLength,
                             InputIV.Length,
                             LAMBDA1Algorithm.BlockSize),
                         NotificationLevel.Error);
                    return;
                }
                iV = InputIV;
            } else
            {
                // iV gets initialized to 0 by default
                GuiLogMessage(Properties.Resources.InfoIVMissing, NotificationLevel.Info);
            }
            
            LAMBDA1Algorithm lambda1 = new LAMBDA1Algorithm(InputKey, settings.Mode);
            byte[] tmpBlock;
            byte[] plainBlock = new byte[LAMBDA1Algorithm.BlockSize];
            byte[] currentBlock = new byte[LAMBDA1Algorithm.BlockSize];

            byte[] inputData = BlockCipherHelper.AppendPadding(InputData,
                BlockCipherHelper.PaddingType.Zeros, LAMBDA1Algorithm.BlockSize);

            Array.Copy(iV, currentBlock, LAMBDA1Algorithm.BlockSize);

            for (int i = 0; i < inputData.Length && !stopPressed; i += LAMBDA1Algorithm.BlockSize)
            {
                Array.Copy(inputData, i, plainBlock, 0, LAMBDA1Algorithm.BlockSize);
                lambda1.ProcessBlock(currentBlock, out tmpBlock);

                currentBlock = XorBlock(plainBlock, tmpBlock);
                outputStream.Write(currentBlock, 0, LAMBDA1Algorithm.BlockSize);
                ProgressChanged(i, inputData.Length - 1);
            }

            ProgressChanged(1, 1);
            OutputData = outputStream.ToArray();
            OnPropertyChanged("OutputData");
        }


        /// <summary>
        /// Takes the InputData and decrypts it
        /// </summary>
        private void PrepareAndDecrypt()
        {
            if (InputData.Length % LAMBDA1Algorithm.BlockSize != 0)
            {
                GuiLogMessage(String.Format(Properties.Resources.ErrorInputBlockLength,
                    InputData.Length, LAMBDA1Algorithm.BlockSize), NotificationLevel.Error);
                return;
            }

            if (InputIV != null)
            {
                if (InputIV.Length != LAMBDA1Algorithm.BlockSize)
                {
                    GuiLogMessage(
                        String.Format(
                            Properties.Resources.ErrorIVLength,
                            InputIV.Length,
                            LAMBDA1Algorithm.BlockSize),
                        NotificationLevel.Error);
                    return;
                }
                iV = InputIV;
            }
            else
            {
                // iV gets initialized to 0 by default
                GuiLogMessage(Properties.Resources.InfoIVMissing, NotificationLevel.Info);
            }

            byte[] decryptionBuffer = new byte[InputData.Length];

            LAMBDA1Algorithm lambda1 = new LAMBDA1Algorithm(InputKey, OperationMode.Encrypt);
            byte[] cipherBlock = new byte[LAMBDA1Algorithm.BlockSize];
            byte[] currentBlock = new byte[LAMBDA1Algorithm.BlockSize];
            byte[] tmpBlock;

            Array.Copy(iV, currentBlock, LAMBDA1Algorithm.BlockSize);
            Array.Copy(InputData, 0, cipherBlock, 0, LAMBDA1Algorithm.BlockSize);

            // Decryption Loop with CBC mode
            for (int i = 0; (i + 8) < InputData.Length && !stopPressed; i += LAMBDA1Algorithm.BlockSize)
            {            
                lambda1.ProcessBlock(currentBlock, out tmpBlock);
                Array.Copy(XorBlock(tmpBlock, cipherBlock), 0, decryptionBuffer, i, LAMBDA1Algorithm.BlockSize);
                Array.Copy(InputData, i + 8, cipherBlock, 0, LAMBDA1Algorithm.BlockSize);
                Array.Copy(InputData, i, currentBlock, 0, LAMBDA1Algorithm.BlockSize);
                ProgressChanged(i, InputData.Length - 1);
            }
            lambda1.ProcessBlock(currentBlock, out tmpBlock);
            Array.Copy(XorBlock(tmpBlock, cipherBlock), 0, decryptionBuffer,
                decryptionBuffer.Length - LAMBDA1Algorithm.BlockSize, LAMBDA1Algorithm.BlockSize);
            
            //Strip padding and convert back to ASCII
            decryptionBuffer = BlockCipherHelper.StripPadding(decryptionBuffer,
                BlockCipherHelper.PaddingType.Zeros, LAMBDA1Algorithm.BlockSize);

            // Set progress to finished and exit
            ProgressChanged(1, 1);
            OutputData = decryptionBuffer;
            OnPropertyChanged("OutputData");
        }

        /// <summary>
        /// Checks if keys and input connectors are correct
        /// </summary>
        /// <returns></returns>
        private bool CheckInput()
        {
            if (InputData == null)
            {
                GuiLogMessage(Properties.Resources.ErrorInputDataNull, NotificationLevel.Error);
                return false;
            }

            if (InputKey == null)
            {
                GuiLogMessage(Properties.Resources.ErrorKeyNull, NotificationLevel.Error);
                return false;
            }

            if (InputData.Length == 0)
            {
                GuiLogMessage(string.Format(Properties.Resources.ErrorInputDataEmpty, InputData.Length), NotificationLevel.Error);
                return false;
            }

            if (InputKey.Length < LAMBDA1Algorithm.KeySize)
            {
                GuiLogMessage(string.Format(InputKey.Length == 0 ?
                    Properties.Resources.ErrorKeyLengthEmpty :
                    Properties.Resources.ErrorKeyLengthShort,
                    InputKey.Length, LAMBDA1Algorithm.KeySize), NotificationLevel.Error);
                return false;
            }

            if (InputKey.Length > LAMBDA1Algorithm.KeySize)
            {
                GuiLogMessage(string.Format(Properties.Resources.ErrorKeyOverlength,
                    InputKey.Length, LAMBDA1Algorithm.KeySize), NotificationLevel.Warning);
                byte[] tmp = new byte[LAMBDA1Algorithm.KeySize];
                Array.Copy(InputKey, tmp, LAMBDA1Algorithm.KeySize);
                InputKey = tmp;
            }
            return true;
        }

        /// <summary>
        ///  XORs a block for CBC mode
        /// </summary>
        /// <param name="a">a 4 byte block or initialisation vector</param>
        /// <param name="b">a 4 byte block </param>
        /// <returns> the XORed 4 byte block</returns>
        private byte[] XorBlock(byte[] a, byte[] b)
        {
            int length = Math.Min(a.Length, b.Length);
            byte[] tmp = new byte[length];
            for (int i = 0; i < length; ++i)
                tmp[i] = (byte)(a[i] ^ b[i]);
            return tmp;
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
