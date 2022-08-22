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
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;

namespace CrypTool.MexicanArmyCipherDisk
{
    [Author("Nils Kopal", "kopal@CrypTool.org", "CrypTool 2 Team", "https://www.CrypTool.org")]
    [PluginInfo("CrypTool.MexicanArmyCipherDisk.Properties.Resources", "PluginCaption", "PluginTooltip", "MexicanArmyCipherDisk/DetailedDescription/doc.xml", new[] { "MexicanArmyCipherDisk/Images/icon.png" })]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class MexicanArmyCipherDisk : ICrypComponent
    {
        private readonly MexicanArmyCipherDiskSettings _settings;

        private const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string DIGITS = "0123456789";
        private readonly string[,] DISKS = new string[,]
        {
            { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26" },
            { "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52" },
            { "53", "54", "55", "56", "57", "58", "59", "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71", "72", "73", "74", "75", "76", "77", "78" },
            { "79", "80", "81", "82", "83", "84", "85", "86", "87", "88", "89", "90", "91", "92", "93", "94", "95", "96", "97", "98", "99", "00", null, null, null, null }
        };

        public UserControl Presentation => null;

        public MexicanArmyCipherDisk()
        {
            _settings = new MexicanArmyCipherDiskSettings();
        }

        public ISettings Settings => _settings;

        [PropertyInfo(Direction.InputData, "InputTextCaption", "InputStringTooltip", true)]
        public string InputText
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "KeyCaption", "KeyTooltip", true)]
        public string Key
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputTextCaption", "OutputStringTooltip", false)]
        public string OutputText
        {
            get;
            set;
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        public void Stop()
        {
        }

        public void PostExecution()
        {
        }

        public void PreExecution()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public void Execute()
        {
            int[] key;
            try
            {
                key = CheckAndExtractKey();
            }
            catch (ArgumentException ex)
            {
                //no valid key given, thus, we abort here with error
                GuiLogMessage(ex.Message, NotificationLevel.Error);
                return;
            }

            switch (_settings.Action)
            {
                case Action.Encrypt:
                    OutputText = EncryptText(key, InputText.ToUpper());
                    break;
                case Action.Decrypt:
                    OutputText = DecryptText(key, InputText.ToUpper());
                    break;
            }
            OnPropertyChanged("OutputText");
        }

        /// <summary>
        /// Checks, if the key is valid, meaning
        /// - if key consist of 4 comma-separated numbers AND
        /// - if each number is a valid number with respect to its disk
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private int[] CheckAndExtractKey()
        {
            if (string.IsNullOrEmpty(Key))
            {
                throw new ArgumentException(Properties.Resources.NoKeyGiven);
            }

            string[] tokens = Key.ToUpper().Split(',', '.', ' ', '/', ';', ':', '-');
            if (tokens.Length != 4)
            {
                throw new ArgumentException(string.Format(Properties.Resources.InvalidKeyGiven, tokens.Length));
            }

            int[] keySettings = new int[4];
            GetDiskSetting(0);
            GetDiskSetting(1);
            GetDiskSetting(2);
            GetDiskSetting(3);

            return keySettings;

            void GetDiskSetting(int diskId)
            {
                try
                {
                    if (tokens[diskId].Length == 1)
                    {                        
                        int symbol;
                        if ((symbol = IndexOfChar(ALPHABET, tokens[diskId][0])) != -1)
                        {
                            //key element is a single alphabet letter
                            keySettings[diskId] = (((diskId + 1) * 26) - ((symbol - 1) + 26) % 26);
                        }
                        else if ((symbol = IndexOfChar(DIGITS, tokens[diskId][0])) != -1)
                        {
                            //key element is a single digit
                            keySettings[diskId] = symbol;
                        }
                        else
                        {
                            //key element is something different
                            throw new ArgumentException(string.Format(Properties.Resources.DiskKeySettingInvalid, diskId + 1, tokens[diskId]));
                        }
                    }
                    else if (tokens[diskId].Length > 2)
                    {
                        throw new ArgumentException(string.Format(Properties.Resources.DiskKeySettingInvalid, diskId + 1, tokens[diskId]));
                    }
                    else
                    {                        
                        keySettings[diskId] = int.Parse(tokens[diskId]);
                        //key element is number
                        if (keySettings[diskId] == 0) // 00 is 100
                        {
                            keySettings[diskId] = 100;
                        }
                    }
                }
                catch (Exception)
                {
                    throw new ArgumentException(string.Format(Properties.Resources.DiskKeySettingInvalid, diskId + 1, tokens[diskId]));
                }

                if (keySettings[diskId] < diskId * 26 + 1 || keySettings[diskId] > (diskId + 1) * 26)
                {
                    throw new ArgumentException(string.Format(Properties.Resources.DiskKeysSettingInvalidHasToBe, diskId + 1, tokens[diskId], diskId * 26 + 1, (diskId + 1) * 26));
                }
                //third disk has only 22 numbers
                else if ((diskId == 3 && keySettings[diskId] > diskId * 26 + 22))
                {                    
                    throw new ArgumentException(string.Format(Properties.Resources.DiskKeysSettingInvalidHasToBe, diskId + 1, tokens[diskId], diskId * 26 + 1, diskId * 26 + 22));
                }
            }
        }

        /// <summary>
        /// Encrypts a given plaintext with the Mexican Army Cipher Disk using the given key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ciphertext"></param>
        /// <returns></returns>
        private string EncryptText(int[] key, string plaintext)
        {
            int[] offsets = new int[key.Length];
            StringBuilder ciphertextBuilder = new StringBuilder();
            Random random = new Random();

            //step 1: create lookup array for encryption

            for (int i = 0; i < offsets.Length; i++)
            {
                offsets[i] = (key[i] - 1) % 26;
            }

            //step 2: encrypt plaintext

            for (int i = 0; i < plaintext.Length; i++)
            {
                int letter = ALPHABET.IndexOf(plaintext[i]);

                if (letter == -1)
                {
                    //handle unknown symbols
                    switch (_settings.UnknownSymbolHandling)
                    {
                        case UnknownSymbolHandlingMode.Ignore:
                            ciphertextBuilder.Append(plaintext[i]);
                            break;
                        case UnknownSymbolHandlingMode.Remove:
                            //do nothing;
                            break;
                        case UnknownSymbolHandlingMode.Replace:
                            ciphertextBuilder.Append("?");
                            break;
                    }
                    continue;
                }

                //disk 3 has 4 null values, so homophone count can be either 3 or 4                
                int numberOfHomophones = DISKS[3, (letter + offsets[3]) % 26] == null ? 3 : 4;
                int diskid = random.Next(numberOfHomophones);
                ciphertextBuilder.Append(DISKS[diskid, (letter + offsets[diskid]) % 26]);

                ProgressChanged(i, plaintext.Length);
            }

            return ciphertextBuilder.ToString();
        }

        /// <summary>
        /// Decrypts a given ciphertext with the Mexican Army Cipher Disk using the given key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ciphertext"></param>
        /// <returns></returns>
        private string DecryptText(int[] key, string ciphertext)
        {
            int digits = 0;
            int homophone = 0;

            //step 1: create lookup array for decryption

            int[] offsets = new int[key.Length];
            int[] homophone_to_plaintextletter = new int[100];
            StringBuilder plaintextBuilder = new StringBuilder();

            for (int i = 0; i < offsets.Length; i++)
            {
                offsets[i] = (key[i] - 1) % 26;
            }

            for (int letter = 0; letter < ALPHABET.Length; letter++)
            {
                int numberOfHomophones = DISKS[3, (letter + offsets[3]) % 26] == null ? 3 : 4;
                for (int diskid = 0; diskid < numberOfHomophones; diskid++)
                {
                    homophone = int.Parse((DISKS[diskid, (letter + offsets[diskid]) % 26]));
                    homophone_to_plaintextletter[homophone] = letter;
                }
            }

            //step 2: decrypt ciphertext

            homophone = 0;

            for (int i = 0; i < ciphertext.Length; i++)
            {
                char letter = ciphertext[i];
                int index;
                if ((index = IndexOfChar(DIGITS, letter)) > -1)
                {
                    //collect two digit homophone
                    homophone *= 10;
                    homophone += index;
                    digits++;
                }
                else
                {
                    //handle unknown symbols
                    switch (_settings.UnknownSymbolHandling)
                    {
                        case UnknownSymbolHandlingMode.Ignore:
                            plaintextBuilder.Append(letter);
                            break;
                        case UnknownSymbolHandlingMode.Remove:
                            //do nothign;
                            break;
                        case UnknownSymbolHandlingMode.Replace:
                            plaintextBuilder.Append("?");
                            break;
                    }
                }

                if (digits == 2)
                {
                    //we have 2 digits (= complete homophone)
                    //add corresponding plaintext letter to plaintext
                    plaintextBuilder.Append(ALPHABET[homophone_to_plaintextletter[homophone]]);
                    digits = 0;
                    homophone = 0;
                }

                ProgressChanged(i, ciphertext.Length);
            }
            return plaintextBuilder.ToString();
        }

        /// <summary>
        /// Returns the first index of the given character in given string
        /// returns -1 if character is not in the string
        /// </summary>
        /// <param name="str"></param>
        /// <param name="chr"></param>
        /// <returns></returns>
        private int IndexOfChar(string str, char chr)
        {
            int index = 0;
            foreach (char chr2 in str)
            {
                if (chr == chr2)
                {
                    return index;
                }
                index++;
            }
            return -1;
        }
    }
}
