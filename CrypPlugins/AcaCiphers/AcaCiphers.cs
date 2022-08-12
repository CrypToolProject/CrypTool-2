/*
   Copyright CrypTool 2 Team <ct2contact@cryptool.org>

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
using CrypTool.ACACiphersLib;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

namespace CrypTool.Plugins.AcaCiphers
{
    [Author("Eduard Scherf", "scherfeduard@gmail.com", "CrypTool 2 Team", "https://www.cryptool.org")]
    [PluginInfo("ACA Ciphers", "A collection of implementations of all ACA ciphers", "AcaCiphers/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class AcaCiphers : ICrypComponent
    {
        #region Private Variables

        private readonly AcaCiphersSettings settings = new AcaCiphersSettings();
        private const string LATIN_ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string LATIN_ALPHABET_WITH_BLANKSPACE = "ABCDEFGHIJKLMNOPQRSTUVWXYZ ";
        public const string LATIN_ALPHABET_WITH_EQUAL_I_AND_J = "ABCDEFGHIKLMNOPQRSTUVWXYZ";

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "Plaintext/Ciphertext", "Here you can add the plaintext, if you want to encrypt or a ciphertext if you want to decrypt")]
        public string Input
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "Key", "Here you can add the key for multiple aca ciphers")]
        public string Key
        {
            get;
            set;
        }


        [PropertyInfo(Direction.InputData, "Dictionary", "Here you can add a dictionary")]
        public string[] Dictionary
        {
            get;
            set;
        }


        [PropertyInfo(Direction.OutputData, "Ciphertext/Plaintext", "The ciphertext or plaintext, based on the mode you selected in the settings, is shown")]
        public string Output
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
        }


        public void Execute()
        {

            ProgressChanged(0, 1);

            string[] additional_parameters = Key.Split(',').Select(str => str.Trim().ToUpper()).ToArray();
            Key = additional_parameters[0];

            Cipher cipher = Cipher.CreateCipher(settings.AcaCipherType,Input.ToUpper(),Key.ToUpper(),additional_parameters,Dictionary,settings.parameter);

            Input = String.Concat(Input.Where(c => !Char.IsWhiteSpace(c)));

            if (!(Input == Input.ToUpper()))
            {
                GuiLogMessage("Text converted to uppercase", NotificationLevel.Info);
            }

            if (settings.AcaMode == Action.Encrypt)
            {
                if (settings.MapIntoTextSpace == true) {
                    Output = Tools.MapNumbersIntoTextSpace(cipher.Encrypt(Tools.MapTextIntoNumberSpace(Input.ToUpper(), LATIN_ALPHABET), Tools.MapTextIntoNumberSpace(Key.ToUpper(), LATIN_ALPHABET)), LATIN_ALPHABET);
                }
                else
                {
                    //TODO
                    Output = string.Join(string.Empty,cipher.Encrypt(Tools.MapTextIntoNumberSpace(Input.ToUpper(), LATIN_ALPHABET), Tools.MapTextIntoNumberSpace(Key.ToUpper(), LATIN_ALPHABET)));
                }
            }
            else if (settings.AcaMode == Action.Decrypt)
            {
                if (settings.MapIntoTextSpace == true)
                {
                    Output = Tools.MapNumbersIntoTextSpace(cipher.Decrypt(Tools.MapTextIntoNumberSpace(Input.ToUpper(), LATIN_ALPHABET), Tools.MapTextIntoNumberSpace(Key.ToUpper(), LATIN_ALPHABET)), LATIN_ALPHABET);
                }
                else
                {
                    Output = string.Join(string.Empty,cipher.Decrypt(Tools.MapTextIntoNumberSpace(Input.ToUpper(), LATIN_ALPHABET), Tools.MapTextIntoNumberSpace(Key.ToUpper(), LATIN_ALPHABET)));
                }
            }

            OnPropertyChanged("Output");
            ProgressChanged(1, 1);
        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
        }

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
