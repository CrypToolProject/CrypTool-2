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

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace DCAPathVisualiser.UI.Cipher1
{
    /// <summary>
    /// Interaktionslogik für Cipher1Table.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("DCAPathVisualiser.Properties.Resources")]
    public partial class Cipher1Table : UserControl, INotifyPropertyChanged
    {
        private int _currentRound;
        private string _currentProbability;
        private string _currentInputDiff;
        private string _currentExpectedDiff;
        private int _currentCountOfCharacteristics;
        private string _currentActiveSBoxes;

        public Cipher1Table()
        {
            DataContext = this;
            InitializeComponent();
        }

        /// <summary>
        /// Property for _currentActiveSBoxes
        /// </summary>
        public string CurrentActiveSBoxes
        {
            get => _currentActiveSBoxes;
            set
            {
                _currentActiveSBoxes = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _currentCountOfCharacteristics
        /// </summary>
        public int CurrentCountOfCharacteristics
        {
            get => _currentCountOfCharacteristics;
            set
            {
                _currentCountOfCharacteristics = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _currentExpectedDiff
        /// </summary>
        public string CurrentExpectedDiff
        {
            get => _currentExpectedDiff;
            set
            {
                _currentExpectedDiff = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _currentInputDiff
        /// </summary>
        public string CurrentInputDiff
        {
            get => _currentInputDiff;
            set
            {
                _currentInputDiff = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _currentProbability
        /// </summary>
        public string CurrentProbability
        {
            get => _currentProbability;
            set
            {
                _currentProbability = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _currentRound
        /// </summary>
        public int CurrentRound
        {
            get => _currentRound;
            set
            {
                _currentRound = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// OnPropertyChanged-method for INotifyPropertyChanged
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
