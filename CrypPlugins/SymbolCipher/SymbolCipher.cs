/*
   Copyright 2022 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Controls;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using CrypTool.Plugins.SymbolCipher.CipherImplementations;

namespace CrypTool.Plugins.SymbolCipher
{
    [Author("Nils Kopal", "kopal@CrypTool.org", "CrypTool 2 Team", "https://www.CrypTool.org")]
    [PluginInfo("CrypTool.Plugins.SymbolCipher.Properties.Resources", "PluginCaption", "PluginTooltip", "SymbolCipher/userdoc.xml", new[] { "SymbolCipher/icon.png" })]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class SymbolCipher : ICrypComponent
    {
        private readonly SymbolCipherSettings _settings = new SymbolCipherSettings();
        public ISettings Settings => _settings;

        public UserControl Presentation => null;

        [PropertyInfo(Direction.InputData, "PlaintextCaption", "PlaintextTooltip", true)]
        public string Plaintext
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "CiphertextImageCaption", "CiphertextImageTooltip")]
        public byte[] CiphertextImage
        {
            get;
            set;
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PropertyChangedEventHandler PropertyChanged;


        public void Dispose()
        {

        }

        public void Execute()
        {
            ASymbolCipher symbolCipher;
            switch (_settings.SymbolCipherType)
            {
                case SymbolCipherType.Pigpen:
                    symbolCipher = new PigpenCipher();
                    break;
                case SymbolCipherType.Musical:
                    if (_settings.MusicalCipherType == MusicalCipherType.DanielSchwenter)
                    {
                        symbolCipher = new MusicalCipher(MusicalCipherType.DanielSchwenter);
                        break;
                    }
                    if (_settings.MusicalCipherType == MusicalCipherType.JohnWilkins)
                    {
                        symbolCipher = new MusicalCipher(MusicalCipherType.JohnWilkins);
                        break;
                    }                   
                    throw new NotImplementedException(string.Format("MusicalCipherType {0} not implemented", _settings.MusicalCipherType));
                default:
                    throw new NotImplementedException(string.Format("SymbolCipherType {0} not implemented", _settings.SymbolCipherType));
            }
                        
            Image image = symbolCipher.Encrypt(Plaintext);
            if(image == null) 
            {
                return;
            }

            using (MemoryStream stream = new MemoryStream())
            {
                image.ToBitmap().Save(stream, ImageFormat.Png);
                CiphertextImage = stream.ToArray();
            }
            OnPropertyChanged("CiphertextImage");
        }

        public void Initialize()
        {

        }

        public void PostExecution()
        {

        }

        public void PreExecution()
        {

        }

        public void Stop()
        {

        }

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
    }
}
