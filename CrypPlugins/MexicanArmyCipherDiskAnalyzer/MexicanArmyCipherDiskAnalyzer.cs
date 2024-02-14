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
using CrypTool.CrypAnalysisViewControl;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Linq;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using LanguageStatisticsLib;
using CrypTool.PluginBase.IO;

namespace CrypTool.MexicanArmyCipherDiskAnalyzer
{
    public delegate void UpdateOutput(string keyString, string plaintextString);

    [Author("Nils Kopal", "kopal@CrypTool.org", "CrypTool 2 Team", "https://www.CrypTool.org")]
    [PluginInfo("CrypTool.MexicanArmyCipherDiskAnalyzer.Properties.Resources", "PluginCaption", "PluginTooltip", "MexicanArmyCipherDiskAnalyzer/DetailedDescription/doc.xml", new[] { "MexicanArmyCipherDiskAnalyzer/Images/icon.png" })]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class MexicanArmyCipherDiskAnalyzer : ICrypComponent
    {
        private readonly MexicanArmyCipherDiskSettings _settings;
        private readonly AssignmentPresentation _presentation = new AssignmentPresentation();        
        private bool _running = false;
        private DateTime _startTime;
        private DateTime _endTime;

        private const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string DIGITS = "0123456789";
        private const int MAXBESTLISTENTRIES = 100;

        private readonly string[,] DISKS = new string[,]
        {
            { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26" },
            { "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52" },
            { "53", "54", "55", "56", "57", "58", "59", "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71", "72", "73", "74", "75", "76", "77", "78" },
            { "79", "80", "81", "82", "83", "84", "85", "86", "87", "88", "89", "90", "91", "92", "93", "94", "95", "96", "97", "98", "99", "00", null, null, null, null }
        };

        public UserControl Presentation => _presentation;

        public MexicanArmyCipherDiskAnalyzer()
        {
            _settings = new MexicanArmyCipherDiskSettings();
            _presentation.UpdateOutputFromUserChoice += UpdateOutputFromUserChoice;
        }

        public ISettings Settings => _settings;

        [PropertyInfo(Direction.InputData, "CiphertextCaption", "CiphertextTooltip", true)]
        public string Ciphertext
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "PlaintextCaption", "PlaintextTooltip", false)]
        public string Plaintext
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "KeyCaption", "KeyTooltip", false)]
        public string Key
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

        public void Stop()
        {
            _running = false;
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
            _startTime = DateTime.Now;

            UpdateDisplayStart();

            // Clear presentation
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ((AssignmentPresentation)Presentation).BestList.Clear();
            }, null);

            if (string.IsNullOrWhiteSpace(Ciphertext) || string.IsNullOrWhiteSpace(Ciphertext))
            {
                throw new ArgumentException(Properties.Resources.NoCiphertextGiven);
            }

            //the settings gramsType is between 0 and 4. Thus, we have to add 1 to cast it to a "GramsType", which starts at 1
            Grams grams = LanguageStatistics.CreateGrams(_settings.Language, DirectoryHelper.DirectoryLanguageStatistics, (GramsType)(_settings.GramsType + 1), false);
            grams.Normalize(10_000_000);

            _running = true;

            DateTime lastUpdateTime = DateTime.Now;

            int[] key = new int[4];

            int count = 0;
            //brute-force test all keys
            for (int disk_offset0 = 1; disk_offset0 < 27; disk_offset0++)
                for (int disk_offset1 = 27; disk_offset1 < 53; disk_offset1++)
                    for (int disk_offset2 = 53; disk_offset2 < 79; disk_offset2++)
                    {
                        
                        for (int disk_offset3 = 79; disk_offset3 <= 100; disk_offset3++)
                        {
                            //create key
                            key = new int[] { disk_offset0, disk_offset1, disk_offset2, disk_offset3 };

                            //decrypt ciphertext
                            string plaintext = DecryptText(key, Ciphertext);

                            //compute cost
                            double costvalue = grams.CalculateCost(plaintext);

                            //100 is represented as 0 on the disk
                            if (disk_offset3 == 100)
                            {
                                key[3] = 0;
                            }

                            //add to bestlist
                            AddNewBestListEntry(key, costvalue, plaintext);

                            if (!_running)
                            {
                                return;
                            }                            
                        }
                        count += 22;
                        if (DateTime.Now > lastUpdateTime.AddMilliseconds(250))
                        {
                            ProgressChanged(count, 26 * 26 * 26 * 22);
                            UpdateDisplay(key);
                            lastUpdateTime = DateTime.Now;
                        }
                    }
            UpdateDisplay(key);
            ProgressChanged(1, 1);
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
                
                //we automatically ignore all non-digit symbols here

                if (digits == 2)
                {
                    //we have 2 digits (= complete homophone)
                    //add corresponding plaintext letter to plaintext
                    plaintextBuilder.Append(ALPHABET[homophone_to_plaintextletter[homophone]]);
                    digits = 0;
                    homophone = 0;
                }
            }
            return plaintextBuilder.ToString();
        }

        /// <summary>
        /// Adds an entry to the BestList
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="text"></param>
        private void AddNewBestListEntry(int[] key, double value, string text)
        {
            string strkey;
            switch (_settings.KeyFormat)
            {
                default:
                case KeyFormat.Digits:
                    strkey = string.Format("{0},{1},{2},{3}", key[0].ToString("D2"), key[1].ToString("D2"), key[2].ToString("D2"), key[3].ToString("D2"));
                    break;
                case KeyFormat.LatinLetters:
                    strkey = GenerateTextKey(key);
                    break;
            }

            //if we have a worse value than the last one, skip
            if (_presentation.BestList.Count > 0 && value <= _presentation.BestList.Last().Value)
            {
                return;
            }

            ResultEntry entry = new ResultEntry
            {
                Key = strkey,
                Text = text,
                Value = value
            };          

            //if we have a better value than the first one, also output it
            if(_presentation.BestList.Count == 0 || value > _presentation.BestList.First().Value)
            {
                Plaintext = entry.Text;
                Key = entry.Key;
                OnPropertyChanged("Plaintext");
                OnPropertyChanged("Key");
            }

            int insertIndex = _presentation.BestList.TakeWhile(e => e.Value > entry.Value).Count();
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {                   
                    //insert new entry at correct place to sustain order of list:                    
                    _presentation.BestList.Insert(insertIndex, entry);
                    if (_presentation.BestList.Count > MAXBESTLISTENTRIES)
                    {
                        _presentation.BestList.RemoveAt(MAXBESTLISTENTRIES);
                    }
                    int ranking = 1;
                    foreach (ResultEntry e in _presentation.BestList)
                    {
                        e.Ranking = ranking;
                        ranking++;
                    }
                    _presentation.CrypAnalysisResultListView.ScrollIntoView(_presentation.CrypAnalysisResultListView.Items[0]);
                }
                catch (Exception)
                {
                    //do nothing
                }
            }, null);
        }
       
        /// <summary>
        /// Resets presentation at startup
        /// </summary>
        private void UpdateDisplayStart()
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _startTime = DateTime.Now;
                _presentation.StartTime.Value = _startTime.ToString();
                _presentation.EndTime.Value = string.Empty;
                _presentation.ElapsedTime.Value = string.Empty;
                _presentation.CurrentlyAnalyzedKey.Value = string.Empty;
            }, null);
        }
        
        /// <summary>
        /// Updates presentation during cryptanalysis
        /// </summary>
        /// <param name="key"></param>
        private void UpdateDisplay(int[] key)
        {
            string strkey;
            switch (_settings.KeyFormat)
            {
                default:
                case KeyFormat.Digits:
                    strkey = string.Format("{0},{1},{2},{3}", key[0].ToString("D2"), key[1].ToString("D2"), key[2].ToString("D2"), key[3].ToString("D2"));
                    break;
                case KeyFormat.LatinLetters:
                    strkey = GenerateTextKey(key);
                    break;
            }

            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _endTime = DateTime.Now;
                TimeSpan elapsedtime = _endTime.Subtract(_startTime);
                TimeSpan elapsedspan = new TimeSpan(elapsedtime.Days, elapsedtime.Hours, elapsedtime.Minutes, elapsedtime.Seconds, 0);
                _presentation.EndTime.Value = _endTime.ToString();
                _presentation.ElapsedTime.Value = elapsedspan.ToString();
                _presentation.CurrentlyAnalyzedKey.Value = strkey;
            }                
            , null);
        }

        /// <summary>
        /// Creates a text key out of a numeric key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string GenerateTextKey(int[] key)
        {
            StringBuilder keybuilder = new StringBuilder();
            for(int diskId = 0; diskId < 4; diskId++)
            {
                keybuilder.Append(ALPHABET[(((diskId + 1) * 26) - (key[diskId] == 0 ? 100 : (key[diskId]) - 1)) % 26]);
                if(diskId != 3)
                {
                    keybuilder.Append(",");
                }
            }

            return keybuilder.ToString();
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

        /// <summary>
        /// User wants to output a selected key and text
        /// </summary>
        /// <param name="keyString"></param>
        /// <param name="plaintextString"></param>
        private void UpdateOutputFromUserChoice(string keyString, string plaintextString)
        {           
            Key = keyString;
            Plaintext = plaintextString;            
            OnPropertyChanged("Key");
            OnPropertyChanged("Plaintext");
        }
    }

    /// <summary>
    /// A single entry of the best list presentation
    /// </summary>
    public class ResultEntry : ICrypAnalysisResultListEntry, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int ranking;
        public int Ranking
        {
            get => ranking;
            set
            {
                ranking = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Ranking)));
            }
        }

        public double Value { get; set; }

        public string DisplayValue
        {
            get
            {
                return $"{Value:N0}";
            }
        }

        public string Key { get; set; }
        public string Text { get; set; }

        public string ClipboardValue => Value.ToString();
        public string ClipboardKey => Key;
        public string ClipboardText => Text;
        public string ClipboardEntry =>
            "Rank: " + Ranking + Environment.NewLine +
            "Value: " + Value + Environment.NewLine +
            "Key: " + Key + Environment.NewLine +
            "Text: " + Text;
    }
}
