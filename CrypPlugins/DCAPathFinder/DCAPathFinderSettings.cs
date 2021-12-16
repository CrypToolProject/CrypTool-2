/*
   Copyright 2019 Christian Bender christian1.bender@student.uni-siegen.de

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
using DCAPathFinder;
using DCAPathFinder.Logic;
using DCAPathFinder.Properties;
using System;
using System.ComponentModel;
using System.Windows;

namespace CrypTool.Plugins.DCAPathFinder
{
    public class DCAPathFinderSettings : ISettings
    {
        #region Private Variables

        private int _chosenMessagePairsCount = 5000;
        private Algorithms _currentAlgorithm = Algorithms.Cipher1;
        private SearchPolicy _currentSearchPolicy = SearchPolicy.FirstAllCharacteristicsDepthSearch;
        private AbortingPolicy _currentAbortingPolicy = AbortingPolicy.Threshold;
        private bool _presentationMode = true;
        private bool _automaticMode;
        private readonly int _maxThreads = Environment.ProcessorCount;
        private int _threadCount = 1;
        private bool _useOfflinePaths;
        private double _thresholdDifferentialSearch = 0.0001;
        private double _thresholdCharacteristicSearch = 0.001;

        #endregion

        #region TaskPane Settings

        /// <summary>
        /// Selection of the toy cipher algorithm
        /// </summary>
        [TaskPane("ChoiceOfAlgorithm", "ChoiceOfAlgorithmToolTop", "ChoiceOfAlgorithmGroup", 1, false,
            ControlType.ComboBox, new string[] { "Cipher1", "Cipher2", "Cipher3" })]
        public Algorithms CurrentAlgorithm
        {
            get => _currentAlgorithm;
            set
            {
                if (_currentAlgorithm != value)
                {
                    _currentAlgorithm = value;
                    switch (_currentAlgorithm)
                    {
                        case Algorithms.Cipher1:
                            HideSettingsElement("ChosenMessagePairsCount");
                            HideSettingsElement("CurrentSearchPolicy");
                            HideSettingsElement("CurrentAbortingPolicy");
                            HideSettingsElement("ThreadCount");
                            HideSettingsElement("UseOfflinePaths");
                            HideSettingsElement("AbortingThresholdCharacteristicSearch");
                            HideSettingsElement("AbortingThresholdDifferentialSearch");
                            break;
                        case Algorithms.Cipher2:
                            ShowSettingsElement("ChosenMessagePairsCount");
                            ShowSettingsElement("CurrentSearchPolicy");
                            ShowSettingsElement("CurrentAbortingPolicy");
                            ShowSettingsElement("ThreadCount");
                            ShowSettingsElement("UseOfflinePaths");

                            if (_currentSearchPolicy == SearchPolicy.FirstBestCharacteristicDepthSearch)
                            {
                                ShowSettingsElement("CurrentAbortingPolicy");
                                if (_currentAbortingPolicy == AbortingPolicy.GlobalMaximum)
                                {
                                    ShowSettingsElement("AbortingThresholdDifferentialSearch");
                                    HideSettingsElement("AbortingThresholdCharacteristicSearch");
                                }
                                else
                                {
                                    ShowSettingsElement("AbortingThresholdCharacteristicSearch");
                                    ShowSettingsElement("AbortingThresholdDifferentialSearch");
                                }
                            }
                            else
                            {
                                HideSettingsElement("CurrentAbortingPolicy");
                                HideSettingsElement("AbortingThresholdCharacteristicSearch");
                                ShowSettingsElement("AbortingThresholdDifferentialSearch");
                            }

                            break;
                        case Algorithms.Cipher3:
                            ShowSettingsElement("ChosenMessagePairsCount");
                            ShowSettingsElement("CurrentSearchPolicy");
                            ShowSettingsElement("CurrentAbortingPolicy");
                            ShowSettingsElement("ThreadCount");
                            ShowSettingsElement("UseOfflinePaths");

                            if (_currentSearchPolicy == SearchPolicy.FirstBestCharacteristicDepthSearch)
                            {
                                ShowSettingsElement("CurrentAbortingPolicy");
                                if (_currentAbortingPolicy == AbortingPolicy.GlobalMaximum)
                                {
                                    ShowSettingsElement("AbortingThresholdDifferentialSearch");
                                    HideSettingsElement("AbortingThresholdCharacteristicSearch");
                                }
                                else
                                {
                                    ShowSettingsElement("AbortingThresholdCharacteristicSearch");
                                    ShowSettingsElement("AbortingThresholdDifferentialSearch");
                                }
                            }
                            else
                            {
                                HideSettingsElement("CurrentAbortingPolicy");
                                HideSettingsElement("AbortingThresholdCharacteristicSearch");
                                ShowSettingsElement("AbortingThresholdDifferentialSearch");
                            }

                            break;
                        case Algorithms.Cipher4:
                            ShowSettingsElement("ChosenMessagePairsCount");
                            ShowSettingsElement("CurrentSearchPolicy");
                            ShowSettingsElement("CurrentAbortingPolicy");
                            ShowSettingsElement("ThreadCount");
                            ShowSettingsElement("UseOfflinePaths");
                            break;
                    }

                    OnPropertyChanged("CurrentAlgorithm");
                }
            }
        }

        /// <summary>
        /// setting to specify the number of threads to use in key recovery
        /// </summary>
        [TaskPane("ThreadCount", "ThreadCountToolTip", "PerformanceSettingsGroup", 1, false, ControlType.NumericUpDown,
            ValidationType.RangeInteger, 1, 64)]
        public int ThreadCount
        {
            get => _threadCount;
            set
            {
                if (value <= _maxThreads)
                {
                    _threadCount = value;
                    OnPropertyChanged("ThreadCount");
                }
                else
                {
                    SettingsErrorMessagsEventArgs e = new SettingsErrorMessagsEventArgs()
                    {
                        message = Resources.ThreadSettingError.Replace("{0}", _maxThreads.ToString())
                    };

                    if (SettingsErrorOccured != null)
                    {
                        SettingsErrorOccured.Invoke(this, e);
                    }

                    ThreadCount = _maxThreads;
                }
            }
        }

        /// <summary>
        /// textbox to specify the count of chosen message pairs
        /// </summary>
        [TaskPane("ChosenMessagePairsCount", "ChosenMessagePairsCountToolTip", "DCAOptions", 1, false,
            ControlType.TextBox)]
        public int ChosenMessagePairsCount
        {
            get => _chosenMessagePairsCount;
            set
            {
                _chosenMessagePairsCount = value;
                OnPropertyChanged("ChosenMessagePairsCount");
            }
        }

        /// <summary>
        /// setting to specify that paths should be loaded from a file to prevent long search times
        /// </summary>
        [TaskPane("UseOfflinePaths", "UseOfflinePathsToolTip", "PerformanceSettingsGroup", 2, false,
            ControlType.CheckBox)]
        public bool UseOfflinePaths
        {
            get => _useOfflinePaths;
            set
            {
                _useOfflinePaths = value;
                OnPropertyChanged("UseOfflinePaths");
            }
        }

        /// <summary>
        /// checkbox to activate the automatic mode (no user interaction needed)
        /// </summary>
        [TaskPane("AutomaticMode", "AutomaticModeToolTip", "ChoiceOfAlgorithmGroup", 3, false, ControlType.CheckBox)]
        public bool AutomaticMode
        {
            get => _automaticMode;
            set
            {
                _automaticMode = value;

                if (_automaticMode)
                {
                    HideSettingsElement("PresentationMode");
                    _presentationMode = false;
                }
                else
                {
                    ShowSettingsElement("PresentationMode");
                }

                OnPropertyChanged("AutomaticMode");
            }
        }

        /// <summary>
        /// checkbox to activate the presentation mode
        /// </summary>
        [TaskPane("PresentationMode", "PresentationModeToolTip", "ChoiceOfAlgorithmGroup", 2, false,
            ControlType.CheckBox)]
        public bool PresentationMode
        {
            get => _presentationMode;
            set
            {
                _presentationMode = value;

                if (_presentationMode)
                {
                    HideSettingsElement("AutomaticMode");
                    _automaticMode = false;
                }
                else
                {
                    ShowSettingsElement("AutomaticMode");
                }

                OnPropertyChanged("PresentationMode");
            }
        }

        [TaskPane("AbortingThresholdDifferentialSearch", "AbortingThresholdDifferentialSearchToolTip", "DCAOptions", 5,
            false, ControlType.NumericUpDown, ValidationType.RangeDouble, 0, 1, 0.0001)]
        public double AbortingThresholdDifferentialSearch
        {
            get => _thresholdDifferentialSearch;
            set
            {
                _thresholdDifferentialSearch = value;
                OnPropertyChanged("AbortingThresholdDifferentialSearch");
            }
        }

        [TaskPane("AbortingThresholdBestCharacteristicSearch", "AbortingThresholdBestCharacteristicSearchToolTip",
            "DCAOptions", 4, false, ControlType.NumericUpDown, ValidationType.RangeDouble, 0, 1, 0.0001)]
        public double AbortingThresholdCharacteristicSearch
        {
            get => _thresholdCharacteristicSearch;
            set
            {
                _thresholdCharacteristicSearch = value;
                OnPropertyChanged("AbortingThresholdCharacteristicSearch");
            }
        }

        /// <summary>
        /// Selection of the search policy
        /// </summary>
        [TaskPane("ChoiceOfSearchPolicy", "ChoiceOfSearchPolicyToolTop", "DCAOptions", 2, false, ControlType.ComboBox,
            new string[] { "SearchPolicy1", "SearchPolicy2", "SearchPolicy3" })]
        public SearchPolicy CurrentSearchPolicy
        {
            get => _currentSearchPolicy;
            set
            {
                if (_currentSearchPolicy != value)
                {
                    _currentSearchPolicy = value;
                    switch (_currentSearchPolicy)
                    {
                        case SearchPolicy.FirstBestCharacteristicHeuristic:
                            {
                                HideSettingsElement("CurrentAbortingPolicy");
                                HideSettingsElement("AbortingThresholdCharacteristicSearch");
                                ShowSettingsElement("AbortingThresholdDifferentialSearch");
                            }
                            break;
                        case SearchPolicy.FirstBestCharacteristicDepthSearch:
                            {
                                ShowSettingsElement("CurrentAbortingPolicy");

                                if (_currentAbortingPolicy == AbortingPolicy.GlobalMaximum)
                                {
                                    HideSettingsElement("AbortingThresholdCharacteristicSearch");
                                    ShowSettingsElement("AbortingThresholdDifferentialSearch");
                                }
                                else if (_currentAbortingPolicy == AbortingPolicy.Threshold)
                                {
                                    ShowSettingsElement("AbortingThresholdCharacteristicSearch");
                                    ShowSettingsElement("AbortingThresholdDifferentialSearch");
                                }
                            }
                            break;
                        case SearchPolicy.FirstAllCharacteristicsDepthSearch:
                            {
                                HideSettingsElement("CurrentAbortingPolicy");
                                HideSettingsElement("AbortingThresholdCharacteristicSearch");
                                ShowSettingsElement("AbortingThresholdDifferentialSearch");
                            }
                            break;
                    }

                    OnPropertyChanged("CurrentSearchPolicy");
                }
            }
        }

        /// <summary>
        /// Selection of the aborting policy
        /// </summary>
        [TaskPane("ChoiceAbortingPolicyPolicy", "ChoiceOfAbortingPolicyToolTop", "DCAOptions", 3, false,
            ControlType.ComboBox, new string[] { "AbortingPolicy1", "AbortingPolicy2" })]
        public AbortingPolicy CurrentAbortingPolicy
        {
            get => _currentAbortingPolicy;
            set
            {
                if (_currentAbortingPolicy != value)
                {
                    _currentAbortingPolicy = value;
                    switch (_currentAbortingPolicy)
                    {
                        case AbortingPolicy.GlobalMaximum:
                            {
                                HideSettingsElement("AbortingThresholdCharacteristicSearch");
                            }
                            break;
                        case AbortingPolicy.Threshold:
                            {
                                ShowSettingsElement("AbortingThresholdCharacteristicSearch");
                            }
                            break;
                    }

                    OnPropertyChanged("CurrentAbortingPolicy");
                }
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public event EventHandler<SettingsErrorMessagsEventArgs> SettingsErrorOccured;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion

        /// <summary>
        /// shows a hidden settings element
        /// </summary>
        /// <param name="element"></param>
        private void ShowSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this,
                    new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Visible)));
            }
        }

        /// <summary>
        /// hides a settings element
        /// </summary>
        /// <param name="element"></param>
        private void HideSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this,
                    new TaskPaneAttributeChangedEventArgs(
                        new TaskPaneAttribteContainer(element, Visibility.Collapsed)));
            }
        }

        public void Initialize()
        {
            //ChosenMessagePairsCount
            //ChoiceOfSearchPolicy
            //ChoiceOfAbortingPolicy


            //check what mode is activated to hide impossible settings
            if (_presentationMode)
            {
                ShowSettingsElement("PresentationMode");
                HideSettingsElement("AutomaticMode");
            }
            else if (_automaticMode)
            {
                ShowSettingsElement("AutomaticMode");
                HideSettingsElement("PresentationMode");
            }

            //check which algorithm is chosen
            switch (CurrentAlgorithm)
            {
                case Algorithms.Cipher1:
                    HideSettingsElement("ChosenMessagePairsCount");
                    HideSettingsElement("CurrentSearchPolicy");
                    HideSettingsElement("CurrentAbortingPolicy");
                    HideSettingsElement("ThreadCount");
                    HideSettingsElement("UseOfflinePaths");
                    HideSettingsElement("AbortingThresholdDifferentialSearch");
                    HideSettingsElement("AbortingThresholdCharacteristicSearch");
                    break;
                default:
                    ShowSettingsElement("ChosenMessagePairsCount");
                    ShowSettingsElement("CurrentSearchPolicy");
                    ShowSettingsElement("ThreadCount");
                    ShowSettingsElement("UseOfflinePaths");
                    ShowSettingsElement("AbortingThresholdDifferentialSearch");
                    ShowSettingsElement("AbortingThresholdCharacteristicSearch");

                    if (_currentSearchPolicy == SearchPolicy.FirstBestCharacteristicDepthSearch)
                    {
                        ShowSettingsElement("CurrentAbortingPolicy");
                        if (CurrentAbortingPolicy == AbortingPolicy.GlobalMaximum)
                        {
                            ShowSettingsElement("AbortingThresholdDifferentialSearch");
                            HideSettingsElement("AbortingThresholdCharacteristicSearch");
                        }
                        else
                        {
                            ShowSettingsElement("AbortingThresholdCharacteristicSearch");
                            ShowSettingsElement("AbortingThresholdDifferentialSearch");
                        }
                    }
                    else
                    {
                        HideSettingsElement("CurrentAbortingPolicy");
                        HideSettingsElement("AbortingThresholdCharacteristicSearch");
                        ShowSettingsElement("AbortingThresholdDifferentialSearch");
                    }


                    break;
            }
        }
    }
}