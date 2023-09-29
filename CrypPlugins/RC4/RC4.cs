/*
   Copyright 2011 Florian Marchal

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
using System;
using System.ComponentModel;
using System.Linq;
using System.Resources;
using System.Security.Cryptography;

namespace CrypTool.Plugins.Cryptography.Encryption
{
    [Author("Florian Marchal", "florian@marchal.de", "", "")]
    [PluginInfo("CrypTool.RC4.Properties.Resources", "PluginCaption", "PluginTooltip", "RC4/DetailedDescription/doc.xml", "RC4/icon.png", "RC4/Images/encrypt.png", "RC4/Images/decrypt.png")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class RC4 : ICrypComponent
    {
        #region Private variables
        // RC4 settings
        // the input data provided by the user
        private ICrypToolStream inputData;
        // the input key provided by the user
        private ICrypToolStream inputKey;
        // the output stream
        private CStreamWriter outputStreamWriter;
        // indicates if we need to stop the algorithm
        private bool stop = false;
        private IControlEncryption controlSlave;
        private readonly RC4Settings settings;
        #endregion

        public RC4()
        {
            settings = new RC4Settings();
        }

        private void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (OnPluginStatusChanged != null)
            {
                OnPluginStatusChanged(this, args);
            }
        }

        public ISettings Settings => settings;

        [PropertyInfo(Direction.InputData, "InputDataCaption", "InputDataTooltip", true)]
        public ICrypToolStream InputData
        {
            get => inputData;
            set => inputData = value;
        }

        [PropertyInfo(Direction.InputData, "InputKeyCaption", "InputKeyTooltip", true)]
        public ICrypToolStream InputKey
        {
            get => inputKey;
            set => inputKey = value;
        }

        [PropertyInfo(Direction.OutputData, "OutputStreamCaption", "OutputStreamTooltip", true)]
        public ICrypToolStream OutputStream
        {
            get => outputStreamWriter;
            set
            {

            }
        }

        private byte[] ToByteArray(ICrypToolStream icstr)
        {
            CStreamReader stream = icstr.CreateReader();
            stream.WaitEof();
            byte[] buffer = new byte[stream.Length];
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            stream.ReadFully(buffer);
            return buffer;
        }

        public void Execute()
        {
            try
            {
                // this is for localization
                ResourceManager resourceManager = new ResourceManager("CrypTool.RC4.Properties.Resources", GetType().Assembly);

                // make sure we have a valid data input
                if (inputData == null)
                {
                    GuiLogMessage(resourceManager.GetString("ErrorInputDataNotProvided"), NotificationLevel.Error);
                    return;
                }

                // make sure we have a valid key input
                if (inputKey == null)
                {
                    GuiLogMessage(resourceManager.GetString("ErrorInputKeyNotProvided"), NotificationLevel.Error);
                    return;
                }

                // make sure we have a valid key input
                if (inputKey.Length < settings.Keylength)
                {
                    GuiLogMessage(string.Format(resourceManager.GetString("ErrorInputKeyTooShort"), inputKey.Length, settings.Keylength), NotificationLevel.Error);
                    return;
                }

                byte[] key = ToByteArray(inputKey);

                // make sure the input key is within the desired range
                if ((key.Length < 5 || key.Length > 256))
                {
                    GuiLogMessage(resourceManager.GetString("ErrorInputKeyInvalidLength"), NotificationLevel.Error);
                    return;
                }

                // now execute the actual encryption
                using (CStreamReader reader = inputData.CreateReader())
                {
                    // create the output stream
                    outputStreamWriter = new CStreamWriter();

                    // some variables
                    int i = 0;
                    int j = 0;
                    // create the sbox
                    byte[] sbox = new byte[256];
                    // initialize the sbox sequentially
                    for (i = 0; i < 256; i++)
                    {
                        sbox[i] = (byte)(i);
                    }
                    // re-align the sbox (and incorporate the key)
                    j = 0;
                    for (i = 0; i < 256; i++)
                    {
                        j = (j + sbox[i] + key[i % settings.Keylength]) % 256;
                        byte sboxOld = sbox[i];
                        sbox[i] = sbox[j];
                        sbox[j] = sboxOld;
                    }

                    // process the input data using the modified sbox
                    int position = 0;
                    DateTime startTime = DateTime.Now;

                    // some inits
                    i = 0;
                    j = 0;

                    long bytesRead = 0;
                    const long blockSize = 256;
                    byte[] buffer = new byte[blockSize];

                    while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0 && !stop)
                    {
                        for (long n = 0; n < bytesRead; n++)
                        {
                            i = (i + 1) % 256;
                            j = (j + sbox[i]) % 256;
                            byte sboxOld = sbox[i];
                            sbox[i] = sbox[j];
                            sbox[j] = sboxOld;

                            byte sboxRandom = sbox[(sbox[i] + sbox[j]) % 256];
                            byte cipherByte = (byte)(sboxRandom ^ buffer[n]);
                            outputStreamWriter.WriteByte(cipherByte);
                        }

                        if ((int)(reader.Position * 100 / reader.Length) > position)
                        {
                            position = (int)(reader.Position * 100 / reader.Length);
                            ProgressChanged(reader.Position, reader.Length);
                        }
                    }
                    outputStreamWriter.Close();

                    // dump status information
                    DateTime stopTime = DateTime.Now;
                    TimeSpan duration = stopTime - startTime;
                    if (!stop)
                    {
                        GuiLogMessage("Encryption complete! (in: " + reader.Length.ToString() + " bytes, out: " + outputStreamWriter.Length.ToString() + " bytes)", NotificationLevel.Info);
                        GuiLogMessage("Time used: " + duration.ToString(), NotificationLevel.Debug);
                        OnPropertyChanged("OutputStream");
                    }
                    if (stop)
                    {
                        GuiLogMessage("Aborted!", NotificationLevel.Info);
                    }
                }
            }
            catch (CryptographicException cryptographicException)
            {
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

        #region IPlugin Member

        public System.Windows.Controls.UserControl Presentation => null;

        public void Initialize()
        {
        }

        public void Dispose()
        {
            stop = false;
            inputKey = null;
            inputData = null;
            outputStreamWriter = null;
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
        private void GuiLogMessage(string message, NotificationLevel logLevel)
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

        [PropertyInfo(Direction.ControlSlave, "ControlSlaveCaption", "ControlSlaveTooltip")]
        public IControlEncryption ControlSlave
        {
            get
            {
                if (controlSlave == null)
                {
                    controlSlave = new RC4Control(this);
                }

                return controlSlave;
            }
        }
    }

    public class RC4Control : IControlEncryption
    {
        private readonly RC4 _plugin;

        public RC4Control(RC4 rc4)
        {
            _plugin = rc4;
            _plugin.Settings.PropertyChanged += _plugin_settings_PropertyChanged;
        }

        private void _plugin_settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Keylength"))
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
            int length = Math.Min(bytesToUse, ciphertext.Length);
            return NativeCryptography.Crypto.decryptRC4(ciphertext, key, length, ((RC4Settings)_plugin.Settings).Keylength);
        }

        public string GetCipherShortName()
        {
            throw new NotImplementedException();
        }

        public int GetBlockSizeAsBytes()
        {
            return 0;
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
            return string.Join("-", Enumerable.Repeat("[0-9A-F][0-9A-F]", ((RC4Settings)_plugin.Settings).Keylength));
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
            RC4Control control = new RC4Control(_plugin);
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