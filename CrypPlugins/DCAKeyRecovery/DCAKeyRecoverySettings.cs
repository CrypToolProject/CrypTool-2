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
using DCAKeyRecovery;
using DCAKeyRecovery.Properties;
using System;
using System.ComponentModel;
using System.Windows;

namespace CrypTool.Plugins.DCAKeyRecovery
{
    public class DCAKeyRecoverySettings : ISettings
    {
        #region Private Variables

        private Algorithms _currentAlgorithm = Algorithms.Cipher1;
        private bool _automaticMode;
        private bool _uiUpdateWhileExecution = true;
        private readonly int _maxThreads = Environment.ProcessorCount;
        private int _threadCount = 1;

        #endregion

        #region TaskPane Settings

        /// <summary>
        /// setting to specify the number of threads to use in key recovery
        /// </summary>
        [TaskPane("ThreadCount", "ThreadCountToolTip", "PerformanceSettingsGroup", 3, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 64)]
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
        /// Checkbox to disable ui refresh during execution
        /// </summary>
        [TaskPane("UIUpdateWhileExecution", "UIUpdateWhileExecutionToolTip", "PerformanceSettingsGroup", 2, false, ControlType.CheckBox)]
        public bool UIUpdateWhileExecution
        {
            get => _uiUpdateWhileExecution;
            set
            {
                _uiUpdateWhileExecution = value;
                OnPropertyChanged("UIUpdateWhileExecution");
            }
        }

        /// <summary>
        /// checkbox to activate the automatic mode (no user interaction needed)
        /// </summary>
        [TaskPane("AutomaticMode", "AutomaticModeToolTip", "PerformanceSettingsGroup", 1, false, ControlType.CheckBox)]
        public bool AutomaticMode
        {
            get => _automaticMode;
            set
            {
                _automaticMode = value;
                OnPropertyChanged("AutomaticMode");
            }
        }

        /// <summary>
        /// Selection of the toy cipher algorithm
        /// </summary>
        [TaskPane("ChoiceOfAlgorithm", "ChoiceOfAlgorithmToolTop", "TutorialSettingsGroup", 1, false, ControlType.ComboBox, new string[] { "Cipher1", "Cipher2", "Cipher3" })]
        public Algorithms CurrentAlgorithm
        {
            get => _currentAlgorithm;
            set
            {
                if (_currentAlgorithm != value)
                {
                    _currentAlgorithm = value;

                    if (_currentAlgorithm == Algorithms.Cipher1)
                    {
                        HideSettingsElement("ThreadCount");
                    }
                    else
                    {
                        ShowSettingsElement("ThreadCount");
                    }

                    OnPropertyChanged("CurrentAlgorithm");
                }
            }
        }

        #endregion

        #region Events

        public event EventHandler<SettingsErrorMessagsEventArgs> SettingsErrorOccured;

        public event PropertyChangedEventHandler PropertyChanged;

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

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
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Visible)));
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
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Collapsed)));
            }
        }

        public void Initialize()
        {
            //check what cipher is activated to hide impossible settings
            if (_currentAlgorithm == Algorithms.Cipher1)
            {
                HideSettingsElement("ThreadCount");
            }
            else
            {
                ShowSettingsElement("ThreadCount");
            }
        }
    }
}
