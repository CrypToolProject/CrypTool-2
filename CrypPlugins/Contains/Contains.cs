/*
   Copyright 2008 Thomas Schmid, University of Siegen

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

using Contains.Aho_Corasick;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Contains
{
    [Author("Thomas Schmid", "thomas.schmid@CrypTool.org", "Uni Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("Contains.Properties.Resources", "PluginCaption", "PluginTooltip", "Contains/DetailedDescription/doc.xml", "Contains/icon.png", "Contains/subset.png", "Contains/no_subset.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
    public class Contains : ICrypComponent
    {
        private readonly ContainsSettings settings;
        private StringSearch stringSearch;
        private string inputString = "";
        private string dictionaryInputString;
        private readonly ContainsPresentation presentation = new ContainsPresentation();
        private Hashtable hashTable = new Hashtable();
        private readonly Dictionary<string, NotificationLevel> dicWarningsAndErros = new Dictionary<string, NotificationLevel>();

        public Contains()
        {
            settings = new ContainsSettings();
        }

        [PropertyInfo(Direction.InputData, "InputStringCaption", "InputStringTooltip", true)]
        public string InputString
        {
            get => inputString;
            set
            {
                // It's not enough to check inputString only.
                // Value must also be evaluated if delimiter character is changed or 'ignore case' is toggled.
                //if (value != inputString)
                {
                    inputString = value;

                    if (inputString != null)
                    {
                        if (settings.ToLower)
                        {
                            inputString = inputString.ToLower();
                        }

                        if (settings.IgnoreDiacritics)
                        {
                            inputString = RemoveDiacritics(inputString);
                        }
                    }

                    OnPropertyChanged("InputString");
                }
            }
        }

        [PropertyInfo(Direction.InputData, "DictionaryInputStringCaption", "DictionaryInputStringTooltip", true)]
        public string DictionaryInputString
        {
            get => dictionaryInputString;
            set
            {
                // It's not enough to check dictionaryInputString only.
                // Value must also be evaluated if delimiter character is changed or 'ignore case' is toggled.
                //if (value != dictionaryInputString)
                {
                    dictionaryInputString = value;

                    if (dictionaryInputString != null)
                    {
                        if (settings.ToLower)
                        {
                            dictionaryInputString = dictionaryInputString.ToLower();
                        }

                        if (settings.IgnoreDiacritics)
                        {
                            dictionaryInputString = RemoveDiacritics(dictionaryInputString);
                        }
                    }

                    Stopwatch stopWatch = new Stopwatch();
                    EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(Properties.Resources.building_search_structure, this, NotificationLevel.Info));
                    stopWatch.Start();
                    SetSearchStructure();
                    stopWatch.Stop();
                    EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(string.Format(Properties.Resources.finished_building_search_structure, new object[] { stopWatch.Elapsed.Seconds.ToString() }), this, NotificationLevel.Info));

                    OnPropertyChanged("DictionaryInputString");
                }
            }
        }

        [PropertyInfo(Direction.InputData, "HitsCaption", "HitsTooltip", false)]
        public int Hits
        {
            get => settings.Hits;
            set
            {
                if (value != settings.Hits && value >= 1)
                {
                    settings.Hits = value;
                    OnPropertyChanged("Hits");
                }
                string msg = "Error: got hit value < 1";
                if (value < 1 && !dicWarningsAndErros.ContainsKey(msg))
                {
                    dicWarningsAndErros.Add(msg, NotificationLevel.Error);
                }
            }
        }

        private static readonly Regex nonSpacingMarkRegex = new Regex(@"\p{Mn}", RegexOptions.Compiled);

        public static string RemoveDiacritics(string text)
        {
            if (text == null)
            {
                return string.Empty;
            }

            string normalizedText = text.Normalize(NormalizationForm.FormD);

            return nonSpacingMarkRegex.Replace(normalizedText, string.Empty);
        }

        /// <summary>
        /// Builds the search structure, e.g. Hashtable or AhoCorasick. This takes about 6sec for a 
        /// 4MB Dictionary file.
        /// 
        /// Add sync Attribute, because on 
        /// foreach (string item in theWords)
        ///   if (!hashTable.ContainsKey(item))
        ///     hashTable.Add(item, null);
        /// appeared erros after firering a lot events in loop
        /// </summary>    
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void SetSearchStructure()
        {
            try
            {
                stringSearch = null;
                hashTable = null;

                if (dictionaryInputString != null && inputString != null)
                {
                    if (settings.Search == ContainsSettings.SearchType.AhoCorasick)
                    {
                        // if DicDelimiter is set we have to split the input string
                        if (settings.DelimiterDictionary.Length == 1)
                        {
                            stringSearch = new StringSearch(dictionaryInputString.Split(settings.DelimiterDictionary[0]));
                        }
                        else
                        {
                            string[] arr = new string[1];
                            arr[0] = dictionaryInputString;
                            stringSearch = new StringSearch(arr);
                        }
                    }
                    else if (settings.Search == ContainsSettings.SearchType.Hashtable)
                    {
                        hashTable = new Hashtable();
                        string[] theWords = null;
                        if (settings.DelimiterDictionary.Length == 1)
                        {
                            theWords = dictionaryInputString.Split(settings.DelimiterDictionary[0]);
                        }
                        else
                        {
                            theWords = new string[1];
                            theWords[0] = dictionaryInputString;
                        }

                        foreach (string item in theWords)
                        {
                            string tmp = item;
                            //if( settings.IgnoreDiacritics ) tmp = RemoveDiacritics(item);
                            if (!hashTable.ContainsKey(tmp))
                            {
                                hashTable.Add(tmp, null);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(exception.Message, this, NotificationLevel.Error));
                EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(exception.StackTrace, this, NotificationLevel.Error));
            }
        }

        private bool result;
        [PropertyInfo(Direction.OutputData, "ResultCaption", "ResultTooltip", false)]
        public bool Result
        {
            get => result;
            set
            {
                result = value;
                OnPropertyChanged("Result");
                if (result)
                {
                    EventsHelper.StatusChanged(OnPluginStatusChanged, this, new StatusEventArgs(StatusChangedMode.ImageUpdate, 1));
                }
                else
                {
                    EventsHelper.StatusChanged(OnPluginStatusChanged, this, new StatusEventArgs(StatusChangedMode.ImageUpdate, 2));
                }
            }
        }


        //Angelov:
        private int hitCount = 0;

        [PropertyInfo(Direction.OutputData, "HitCountCaption", "HitCountTooltip", false)]
        public int HitCount
        {
            get => hitCount;
            set
            {
                hitCount = value;
                OnPropertyChanged("HitCount");
            }
        }
        //End Angelov


        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public ISettings Settings => settings;

        public UserControl Presentation => presentation;

        public void PreExecution()
        {
            EventsHelper.StatusChanged(OnPluginStatusChanged, this, new StatusEventArgs(StatusChangedMode.ImageUpdate, 0));
            // set hits to zero
            List<StringSearchResult> list = new List<StringSearchResult>();
            presentation.SetData(list.ToArray());
        }

        // If this attribute is not used large loops, containing this plugin, will
        // produce threads that aren't finished before next execution takes place.
        // So after pressing stop button there may be a lot threads in queue that have still
        // to be executed
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Execute()
        {
            try
            {
                List<StringSearchResult> listReturn = new List<StringSearchResult>();
                string[] arrSearch = null;
                List<string> wordsFound = new List<string>();

                if (InputString != null && DictionaryInputString != null)
                {
                    if (settings.Search == ContainsSettings.SearchType.AhoCorasick && stringSearch != null)
                    {
                        listReturn.AddRange(stringSearch.FindAll(InputString));
                    }
                    else if (settings.Search == ContainsSettings.SearchType.Hashtable && hashTable != null)
                    {
                        if (settings.DelimiterInputString != null && settings.DelimiterInputString.Length == 1)
                        {
                            arrSearch = InputString.Split(settings.DelimiterInputString[0]);
                        }

                        if (arrSearch != null)
                        {
                            for (int i = 0; i < arrSearch.Length; i++)
                            {
                                string s = arrSearch[i];
                                if (settings.IgnoreDiacritics)
                                {
                                    s = RemoveDiacritics(s);
                                }

                                if (hashTable.ContainsKey(arrSearch[i]))
                                {
                                    if (settings.CountWordsOnlyOnce)
                                    {
                                        if (!wordsFound.Contains(arrSearch[i]))
                                        {
                                            wordsFound.Add(arrSearch[i]);
                                            listReturn.Add(new StringSearchResult(i, arrSearch[i]));
                                        }
                                    }
                                    else
                                    {
                                        listReturn.Add(new StringSearchResult(i, arrSearch[i]));
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (hashTable.ContainsKey(InputString))
                            {
                                listReturn.Add(new StringSearchResult(0, InputString));
                            }
                        }
                    }


                    HitCount = listReturn.Count;
                    Result = (HitCount >= settings.Hits);

                    double percentage = HitCount * 100.0 / settings.Hits;

                    // set target-hits bases on current setting
                    presentation.TargetHits = (settings.HitPercentFromInputString && arrSearch != null) ? (int)percentage : settings.Hits;
                    presentation.SetData(listReturn.ToArray());

                    //ProgressChanged(Math.Min(percentage, 100.0), 100);
                    ProgressChanged(100, 100);
                }
                else
                {
                    foreach (KeyValuePair<string, NotificationLevel> kvp in dicWarningsAndErros)
                    {
                        EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(kvp.Key, this, kvp.Value));
                    }
                    dicWarningsAndErros.Clear();
                    if (InputString == null)
                    {
                        EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(Properties.Resources.no_input_string, this, NotificationLevel.Error));
                    }

                    if (DictionaryInputString == null)
                    {
                        EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(Properties.Resources.no_dictionary, this, NotificationLevel.Error));
                    }
                }
            }
            catch (Exception exception)
            {
                EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(exception.Message, this, NotificationLevel.Error));
            }



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

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
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
