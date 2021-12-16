/*
   Copyright 2019 George Lasry, Nils Kopal, CrypTool 2 Team

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
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace CrypTool.PlayfairAnalyzer
{

    public enum Language
    {
        English
    }
    public class PlayfairAnalyzerSettings : ISettings
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Language _language;

        private int _threads;
        private int _cycles;
        private bool _discardSamePlaintexts = true;
        private bool _advancedBestListView = false;

        public PlayfairAnalyzerSettings()
        {
            //fill the list for the dropdown menu with numbers from 1 to ProcessorCount
            CoresAvailable.Clear();
            for (int i = 1; i <= Environment.ProcessorCount; i++)
            {
                CoresAvailable.Add(i.ToString());
            }
        }

        public void Initialize()
        {

        }

        [TaskPane("ThreadsCaption", "ThreadsTooltip", null, 1, false, ControlType.DynamicComboBox, new string[] { "CoresAvailable" })]
        public int Threads
        {
            get => _threads;
            set
            {
                if (value != _threads)
                {
                    _threads = value;
                    OnPropertyChanged("Threads");
                }
            }
        }

        [TaskPane("CyclesCaption", "CyclesTooltip", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 1000)]
        public int Cycles
        {
            get => _cycles;
            set
            {
                if (value != _cycles)
                {
                    _cycles = value;
                    OnPropertyChanged("Cycles");
                }
            }
        }

        [TaskPane("LanguageCaption", "LanguageTooltip", null, 4, false, ControlType.ComboBox, new string[] { "English" })]
        public Language Language
        {
            get => _language;
            set
            {
                if (value != _language)
                {
                    _language = value;
                }
            }
        }

        [TaskPane("DiscardSamePlaintextsCaption", "DiscardSamePlaintextsTooltip", null, 5, false, ControlType.CheckBox)]
        public bool DiscardSamePlaintexts
        {
            get => _discardSamePlaintexts;
            set
            {
                if (value != _discardSamePlaintexts)
                {
                    _discardSamePlaintexts = value;
                    OnPropertyChanged(nameof(DiscardSamePlaintexts));
                }
            }
        }

        [TaskPane("AdvancedBestListViewCaption", "AdvancedBestListViewTooltip", null, 6, true, ControlType.CheckBox)]
        public bool AdvancedBestListView
        {
            get => _advancedBestListView;
            set
            {
                if (value != _advancedBestListView)
                {
                    _advancedBestListView = value;
                    OnPropertyChanged(nameof(AdvancedBestListView));
                }
            }
        }

        private ObservableCollection<string> coresAvailable = new ObservableCollection<string>();
        [DontSave]
        public ObservableCollection<string> CoresAvailable
        {
            get => coresAvailable;
            set
            {
                if (value != coresAvailable)
                {
                    coresAvailable = value;
                    OnPropertyChanged("CoresAvailable");
                }
            }
        }

        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

    }
}
