/*                              
   Copyright 2009 Fabian Enkler

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
using System.Linq;
using System.Windows.Controls;

namespace CrypTool.CaesarAnalysisHelper
{
    [Author("Fabian Enkler", "enkler@CrypTool.org", "", "")]
    [PluginInfo("CaesarAnalysisHelper.Properties.Resources", "PluginCaption", "PluginTooltip", "CaesarAnalysisHelper/DetailedDescription/doc.xml", "CaesarAnalysisHelper/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class CaesarAnalysisHelper : ICrypComponent
    {
        private readonly CaesarAnalysisHelperSettings settings;

        public event PropertyChangedEventHandler PropertyChanged;

#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
#pragma warning restore

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void Progress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public CaesarAnalysisHelper()
        {
            settings = new CaesarAnalysisHelperSettings();
        }

        public ISettings Settings => settings;

        private string encryptedText;
        [PropertyInfo(Direction.InputData, "EncryptedTextCaption", "EncryptedTextTooltip", false)]
        public string EncryptedText
        {
            get => encryptedText;
            set
            {
                encryptedText = value;
                OnPropertyChanged("EncryptedText");
            }
        }

        private string frequencyList = string.Empty;
        [PropertyInfo(Direction.InputData, "FrequencyListCaption", "FrequencyListTooltip", true)]
        public string FrequencyList
        {
            get => frequencyList;
            set
            {
                frequencyList = value;
                OnPropertyChanged("FrequencyList");
            }
        }

        private int key;
        [PropertyInfo(Direction.OutputData, "KeyCaption", "KeyTooltip", true)]
        public int Key => key;

        private void CryptoAnalysis()
        {
            Dictionary<int, int> KeyList = new Dictionary<int, int>();
            int Counter = 0;
            foreach (int i in CountChars(frequencyList.ToLower()))
            {
                if (Counter < 5)
                {
                    if (!KeyList.ContainsKey(i))
                    {
                        KeyList.Add(i, 5 - Counter);
                    }
                    else
                    {
                        KeyList[i] += 5 - Counter;
                    }
                    Counter++;
                }
            }

            Counter = 0;
            foreach (int i in CountBigrams(encryptedText.ToLower()))
            {
                if (Counter < 5)
                {
                    if (!KeyList.ContainsKey(i))
                    {
                        KeyList.Add(i, 5 - Counter);
                    }
                    else
                    {
                        KeyList[i] += 5 - Counter;
                    }
                    Counter++;
                }
            }

            IOrderedEnumerable<int> items = (from k in KeyList.Keys
                                             orderby KeyList[k] descending
                                             select k);
            List<int> ResultList = new List<int>();
            foreach (int i in items)
            {
                ResultList.Add(i);
            }
            if (ResultList.Count > 0)
            {
                key = ResultList[0];
                OnPropertyChanged("Key");
            }
        }

        private List<int> CountChars(string text)
        {
            Dictionary<char, int> Dic = new Dictionary<char, int>();

            if (!string.IsNullOrEmpty(text))
            {
                foreach (string s in text.Split(new[] { "\r\n" }, StringSplitOptions.None))
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        string[] tmpArr = s.Split(new[] { ':' });
                        if (tmpArr.Length > 1)
                        {

                            char c = tmpArr[0][0];
                            int.TryParse(tmpArr[1], out int Count);
                            if (!Dic.ContainsKey(c))
                            {
                                Dic.Add(c, 0);
                            }

                            Dic[c] += Count;
                        }
                    }
                }

                IOrderedEnumerable<char> items = (from k in Dic.Keys
                                                  orderby Dic[k] descending
                                                  select k);

                List<int> Result = new List<int>();
                foreach (char c in items)
                {
                    int tmp = c - settings.FrequentChar;
                    int temp = 26 + tmp;
                    if (tmp < 0)
                    {
                        Result.Add(temp);
                    }

                    if (tmp > 0)
                    {
                        Result.Add(tmp);
                    }

                    if (tmp == 0)
                    {
                        Result.Add(tmp);
                    }
                }
                return Result;
            }
            return new List<int>();
        }

        private static List<int> CountBigrams(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {

                Dictionary<string, int> BigramDic = new Dictionary<string, int>();
                for (int i = 0; i < text.Length - 1; i++)
                {
                    string tmp = text.Substring(i, 2);
                    if (!tmp.Contains(" "))
                    {
                        if (!BigramDic.ContainsKey(tmp))
                        {
                            BigramDic.Add(tmp, 0);
                        }

                        BigramDic[tmp]++;
                    }
                }

                IOrderedEnumerable<string> items = (from k in BigramDic.Keys
                                                    orderby BigramDic[k] descending
                                                    select k);

                string[] Bigrams = new[] { "er", "en", "ch", "de" };
                Dictionary<int, int> KeyList = new Dictionary<int, int>();
                foreach (string s in items)
                {
                    int Counter = 0;
                    string CurrentBigramm;
                    do
                    {
                        if (Counter < Bigrams.Length)
                        {
                            CurrentBigramm = Bigrams[Counter];
                        }
                        else
                        {
                            CurrentBigramm = string.Empty;
                            break;
                        }
                        Counter++;
                    } while (!(CurrentBigramm[1] - CurrentBigramm[0] == s[1] - s[0]));

                    if (!string.IsNullOrEmpty(CurrentBigramm))
                    {
                        int tmpkey = s[0] - CurrentBigramm[0];
                        if (!KeyList.ContainsKey(tmpkey))
                        {
                            KeyList.Add(tmpkey, 0);
                        }

                        KeyList[tmpkey]++;
                    }
                }
                IOrderedEnumerable<int> items2 = (from k in KeyList.Keys
                                                  orderby KeyList[k] descending
                                                  select k);
                List<int> Result = new List<int>();
                foreach (int s in items2)
                {
                    Result.Add(s);
                }

                return Result;
            }
            return new List<int>();
        }

        public UserControl Presentation => null;

        public void PreExecution()
        {
            encryptedText = string.Empty;
            key = 0;
            frequencyList = string.Empty;
        }

        public void Execute()
        {
            Progress(0, 1);
            CryptoAnalysis();
            Progress(1, 1);
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

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        /*
                private void GuiNotification(string text)
                {
                    GuiNotification(text, NotificationLevel.Debug);
                }
        */

        /*
                private void GuiNotification(string text, NotificationLevel Level)
                {
                    if (OnGuiLogNotificationOccured != null)
                        OnGuiLogNotificationOccured(this, new GuiLogEventArgs(text, this, Level));
                }
        */
    }
}
