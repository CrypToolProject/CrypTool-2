/*
   Copyright 2021 by Nils Kopal

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
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace CrypTool.Plugins.SIGABA
{
    [Author("George Lasry, Nils Kopal", "kopal@cryptool.org", "CrypTool project", "http://www.cryptool.org")]
    [PluginInfo("CrypTool.Plugins.SIGABA.Properties.Resources", "PluginCaption", "PluginTooltip", "SIGABA/doc.xml", "SIGABA/icon.png")]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class SIGABA : ICrypComponent
    {
        private readonly SIGABASettings _settings = new SIGABASettings();
        private string _input;
        private string _output;
        private string _key;

        public ISettings Settings => _settings;

        public UserControl Presentation => null;

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PropertyChangedEventHandler PropertyChanged;

        [PropertyInfo(Direction.InputData, "InputCaption", "InputTooltip", true)]
        public string Input
        {
            get => _input;
            set
            {
                _input = value;
                OnPropertyChanged("Input");
            }
        }

        [PropertyInfo(Direction.InputData, "KeyCaption", "KeyTooltip", false)]
        public string Key
        {
            get => _key;
            set
            {
                _key = value;
                OnPropertyChanged("Key");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip", true)]
        public string Output
        {
            get => _output;
            set
            {
                _output = value;
                OnPropertyChanged("Output");
            }
        }

        public void Dispose()
        {

        }

        public void Execute()
        {
            string cipherRotors;
            string controlRotors;
            string indexRotors;
            string cipherRotorPositions;
            string controlRotorPositions;
            string indexRotorPositions;

            if (string.IsNullOrEmpty(Key)) //we have no keystring, thus, we have to use the plugin's settings
            {
                cipherRotors = GetCipherRotorsFromSettings();
                controlRotors = GetControlRotorsFromSettings();
                indexRotors = GetIndexRotorsFromSettings();
                cipherRotorPositions = _settings.CipherRotorPositions;
                if (!Regex.IsMatch(cipherRotorPositions, "[A-Za-z]{5}$"))
                {
                    GuiLogMessage(string.Format("Invalid cipher rotor positions: ", cipherRotorPositions), NotificationLevel.Error);
                    return;
                }
                controlRotorPositions = _settings.ControlRotorPositions;
                if (!Regex.IsMatch(controlRotorPositions, "[A-Za-z]{5}$"))
                {
                    GuiLogMessage(string.Format("Invalid control rotor positions: ", controlRotorPositions), NotificationLevel.Error);
                    return;
                }
                indexRotorPositions = _settings.IndexRotorPositions;
                if (!Regex.IsMatch(indexRotorPositions, @"\d{5}$"))
                {
                    GuiLogMessage(string.Format("Invalid index rotor positions: ", indexRotorPositions), NotificationLevel.Error);
                    return;
                }
            }
            else //we have a keystring which we can use for the configuration of the machine
            {
                string key = Regex.Replace(Key, @"\.|,|;|\s", "");//we allow the usge of delimeters and spaces to separate the individual parts of the key to ease reading
                cipherRotors = GetCipherRotorsFromKey(key);
                if (cipherRotors == null)
                {
                    GuiLogMessage(string.Format("Invalid cipher rotors definition in key: {0}", Key), NotificationLevel.Error);
                    return;
                }
                controlRotors = GetControlRotorsFromKey(key);
                if (controlRotors == null)
                {
                    GuiLogMessage(string.Format("Invalid control rotors definition in key: {0}", Key), NotificationLevel.Error);
                    return;
                }
                indexRotors = GetIndexRotorsFromKey(key);
                if (indexRotors == null)
                {
                    GuiLogMessage(string.Format("Invalid index rotors definition in key: {0}", Key), NotificationLevel.Error);
                    return;
                }
                cipherRotorPositions = GetCipherRotorPositionsFromKey(key);
                if (cipherRotorPositions == null)
                {
                    GuiLogMessage(string.Format("Invalid cipher rotor positions definition in key: {0}", Key), NotificationLevel.Error);
                    return;
                }
                controlRotorPositions = GetControlRotorPositionsFromKey(key);
                if (controlRotorPositions == null)
                {
                    GuiLogMessage(string.Format("Invalid control rotor positions definition in key:  {0}", Key), NotificationLevel.Error);
                    return;
                }
                indexRotorPositions = GetIndexRotorPositionsFromKey(key);
                if (indexRotorPositions == null)
                {
                    GuiLogMessage(string.Format("Invalid index rotor positions definition in key: {0}", Key), NotificationLevel.Error);
                    return;
                }
            }

            try
            {
                string input = Input.ToUpper();
                if (_settings.UseZAsSpace && _settings.Action == SIGABAAction.Encrypt)
                {
                    input = input.Replace(" ", "Z");
                }

                List<UnknownSymbol> unknownSymbolList = new List<UnknownSymbol>();
                input = RemoveUnknownSymbols(input, SIGABAConstants.ALPHABET, out unknownSymbolList);

                SIGABAImplementation sigabaImplementation = new SIGABAImplementation(
                    _settings.Model,
                    cipherRotors,
                    controlRotors,
                    indexRotors,
                    cipherRotorPositions,
                    controlRotorPositions,
                    indexRotorPositions);

                string output = sigabaImplementation.EncryptDecrypt(_settings.Action == SIGABAAction.Decrypt, input);
                switch (_settings.UnknownSymbolHandling)
                {
                    case UnknownSymbolHandling.Ignore:
                        output = AddUnknownSymbols(output, unknownSymbolList, null);
                        break;
                    case UnknownSymbolHandling.Replace:
                        output = AddUnknownSymbols(output, unknownSymbolList, "?");
                        break;
                }

                if (_settings.UseZAsSpace && _settings.Action == SIGABAAction.Decrypt)
                {
                    output = output.Replace("Z", " ");
                }
                Output = output;
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception occured during excecution of the SIGABAImplementation: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        #region Extract configuration

        private string GetCipherRotorsFromKey(string key)
        {
            try
            {
                string cipherRotors = key.Substring(0, 10);
                if (Regex.IsMatch(cipherRotors, @"[0-9,NR]{5}$"))
                {
                    return cipherRotors;
                }
            }
            catch (Exception)
            {
                //do nothing
            }
            return null;
        }

        private string GetCipherRotorsFromSettings()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append((int)_settings.CipherRotor0);
            builder.Append(_settings.CipherRotor0Reversed == true ? "R" : "N");
            builder.Append((int)_settings.CipherRotor1);
            builder.Append(_settings.CipherRotor1Reversed == true ? "R" : "N");
            builder.Append((int)_settings.CipherRotor2);
            builder.Append(_settings.CipherRotor2Reversed == true ? "R" : "N");
            builder.Append((int)_settings.CipherRotor3);
            builder.Append(_settings.CipherRotor3Reversed == true ? "R" : "N");
            builder.Append((int)_settings.CipherRotor4);
            builder.Append(_settings.CipherRotor4Reversed == true ? "R" : "N");
            return builder.ToString();
        }

        private string GetControlRotorsFromKey(string key)
        {
            try
            {
                string controlRotors = key.Substring(10, 10);
                if (Regex.IsMatch(controlRotors, @"[0-9,NR]{5}$"))
                {
                    return controlRotors;
                }
            }
            catch (Exception)
            {
                //do nothing
            }
            return null;
        }

        private string GetControlRotorsFromSettings()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append((int)_settings.ControlRotor0);
            builder.Append(_settings.ControlRotor0Reversed == true ? "R" : "N");
            builder.Append((int)_settings.ControlRotor1);
            builder.Append(_settings.ControlRotor1Reversed == true ? "R" : "N");
            builder.Append((int)_settings.ControlRotor2);
            builder.Append(_settings.ControlRotor2Reversed == true ? "R" : "N");
            builder.Append((int)_settings.ControlRotor3);
            builder.Append(_settings.ControlRotor3Reversed == true ? "R" : "N");
            builder.Append((int)_settings.ControlRotor4);
            builder.Append(_settings.ControlRotor4Reversed == true ? "R" : "N");
            return builder.ToString();
        }

        private string GetIndexRotorsFromKey(string key)
        {
            try
            {
                string indexRotors = key.Substring(20, 5);
                if (Regex.IsMatch(indexRotors, "[0-4]{5}$"))
                {
                    return indexRotors;
                }
            }
            catch (Exception)
            {
                //do nothing
            }
            return null;
        }

        private string GetIndexRotorsFromSettings()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append((int)_settings.IndexRotor0);
            builder.Append((int)_settings.IndexRotor1);
            builder.Append((int)_settings.IndexRotor2);
            builder.Append((int)_settings.IndexRotor3);
            builder.Append((int)_settings.IndexRotor4);
            return builder.ToString();
        }

        private string GetCipherRotorPositionsFromKey(string key)
        {
            string cipherRotorPositions = key.Substring(25, 5);
            try
            {
                if (Regex.IsMatch(cipherRotorPositions, "[A-Z]{5}$"))
                {
                    return cipherRotorPositions;
                }
            }
            catch (Exception)
            {
                //do nothing
            }
            return null;
        }

        private string GetControlRotorPositionsFromKey(string key)
        {
            try
            {
                string controlRotorPositions = key.Substring(30, 5);
                if (Regex.IsMatch(controlRotorPositions, "[A-Z]{5}$"))
                {
                    return controlRotorPositions;
                }
                return controlRotorPositions;
            }
            catch (Exception)
            {
                //do nothing
            }
            return null;
        }

        private string GetIndexRotorPositionsFromKey(string key)
        {
            try
            {
                string indexRotorPositions = key.Substring(35, 5);
                if (Regex.IsMatch(indexRotorPositions, "[0-9]{5}$"))
                {
                    return indexRotorPositions;
                }
            }
            catch (Exception)
            {
                //do nothing
            }
            return null;
        }

        #endregion

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

        /// <summary>
        /// A property of this plugin changed
        /// </summary>
        /// <param name="name">propertyname</param>
        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        /// <summary>
        /// Removes all symbols that are not part of the alphabet; puts all symbols and their positions into the unkownSymbolList
        /// </summary>
        /// <param name="text"></param>
        /// <param name="alphabet"></param>
        /// <param name="unknownSymbolList"></param>
        /// <returns></returns>
        private string RemoveUnknownSymbols(string text,
                                            string alphabet,
                                            out List<UnknownSymbol> unknownSymbolList)
        {
            unknownSymbolList = new List<UnknownSymbol>();
            for (int position = 0; position < text.Length; position++)
            {
                string symbol = text.Substring(position, 1);
                if (!alphabet.Contains(symbol))
                {
                    unknownSymbolList.Add(new UnknownSymbol() { Symbol = symbol, Position = position });
                }
            }
            foreach (UnknownSymbol unkownSymbol in unknownSymbolList)
            {
                text = text.Replace(unkownSymbol.Symbol, "");
            }
            return text;
        }

        /// <summary>
        /// Adds all unknown symbols from the list back to the text. If a replacement string is given, it replaces these by this string
        /// </summary>
        /// <param name="text"></param>
        /// <param name="unknownSymbolList"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        private string AddUnknownSymbols(string text,
                                         List<UnknownSymbol> unknownSymbolList,
                                         string replacement = null)
        {
            foreach (UnknownSymbol unkownSymbol in unknownSymbolList)
            {
                if (replacement == null)
                {
                    text = text.Insert(unkownSymbol.Position, unkownSymbol.Symbol);
                }
                else
                {
                    text = text.Insert(unkownSymbol.Position, replacement);
                }
            }
            return text;
        }

    }

    public class UnknownSymbol
    {
        public string Symbol { get; set; }
        public int Position { get; set; }
    }
}
