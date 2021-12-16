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

using DCAKeyRecovery.Logic;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace DCAKeyRecovery.UI.Cipher1
{
    /// <summary>
    /// Interaktionslogik für LastRoundAttack.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("DCAKeyRecovery.Properties.Resources")]
    public partial class Cipher1ResultView : UserControl, INotifyPropertyChanged
    {
        private string _attackLastRoundHeader;
        private DateTime _startTime;
        private DateTime _endTime;
        private string _currentPlainText;
        private string _currentCipherText;
        private string _currentKeyCandidate;
        private string _expectedDifference;
        private int _round;
        private int _currentKeysToTestThisRound;
        private int _remainingKeyCandidates;
        private int _examinedPairCount;
        private ObservableCollection<RoundResult> _roundResults;

        /// <summary>
        /// Constructor
        /// </summary>
        public Cipher1ResultView()
        {
            _roundResults = new ObservableCollection<RoundResult>();
            DataContext = this;
            InitializeComponent();
        }

        #region properties

        /// <summary>
        /// Property for round results
        /// </summary>
        public ObservableCollection<RoundResult> RoundResults
        {
            get => _roundResults;
            set
            {
                _roundResults = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for header
        /// </summary>
        public string AttackLastRoundHeader
        {
            get => _attackLastRoundHeader;
            set
            {
                _attackLastRoundHeader = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for start time
        /// </summary>
        public DateTime StartTime
        {
            get => _startTime;
            set
            {
                _startTime = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for end time
        /// </summary>
        public DateTime EndTime
        {
            get => _endTime;
            set
            {
                _endTime = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for current plaintext
        /// </summary>
        public string CurrentPlainText
        {
            get => _currentPlainText;
            set
            {
                _currentPlainText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for current ciphertext
        /// </summary>
        public string CurrentCipherText
        {
            get => _currentCipherText;
            set
            {
                _currentCipherText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for current key candidate
        /// </summary>
        public string CurrentKeyCandidate
        {
            get => _currentKeyCandidate;
            set
            {
                _currentKeyCandidate = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for expected difference
        /// </summary>
        public string ExpectedDifference
        {
            get => _expectedDifference;
            set
            {
                _expectedDifference = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for current round
        /// </summary>
        public int Round
        {
            get => _round;
            set
            {
                _round = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for count of keys to test
        /// </summary>
        public int CurrentKeysToTestThisRound
        {
            get => _currentKeysToTestThisRound;
            set
            {
                _currentKeysToTestThisRound = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for remaining key candidate count
        /// </summary>
        public int RemainingKeyCandidates
        {
            get => _remainingKeyCandidates;
            set
            {
                _remainingKeyCandidates = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for examined pair count
        /// </summary>
        public int ExaminedPairCount
        {
            get => _examinedPairCount;
            set
            {
                _examinedPairCount = value;
                OnPropertyChanged();
            }
        }

        #endregion

        /// <summary>
        /// Event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Method to call if data changes
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
