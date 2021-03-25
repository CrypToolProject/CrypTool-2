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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DCAKeyRecovery.UI.Models;

namespace DCAKeyRecovery.UI.Cipher2
{
    /// <summary>
    /// Interaktionslogik für Cipher2AnyRoundResultView.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("DCAKeyRecovery.Properties.Resources")]
    public partial class Cipher2AnyRoundResultView : UserControl, INotifyPropertyChanged
    {
        private DateTime _startTime;
        private DateTime _endTime;
        private int _round;
        private double _currentExpectedProbability;
        private string _expectedDifference;
        private int _expectedHitCount;
        private string _currentKeyCandidate;
        private string _messagePairCountToExamine;
        private string _currentRecoveredRoundKey;
        private int _currentKeysToTestThisRound;
        private ObservableCollection<KeyResult> _keyResults;

        /// <summary>
        /// Constructor
        /// </summary>
        public Cipher2AnyRoundResultView()
        {
            _keyResults = new ObservableCollection<KeyResult>();
            DataContext = this;
            InitializeComponent();
        }

        /// <summary>
        /// Property for _startTime
        /// </summary>
        public DateTime StartTime
        {
            get { return _startTime; }
            set
            {
                _startTime = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _endTime
        /// </summary>
        public DateTime EndTime
        {
            get { return _endTime; }
            set
            {
                _endTime = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _round
        /// </summary>
        public int Round
        {
            get { return _round; }
            set
            {
                _round = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _currentExpectedProbability
        /// </summary>
        public double CurrentExpectedProbability
        {
            get { return _currentExpectedProbability; }
            set
            {
                _currentExpectedProbability = value;
                OnPropertyChanged();
                OnPropertyChanged("CurrentExpectedProbabilityStr");
            }
        }

        /// <summary>
        /// Property for formatted _currentExpectedProbability
        /// </summary>
        public string CurrentExpectedProbabilityStr
        {
            get { return String.Format("{0:0.0000}", CurrentExpectedProbability); }
        }

        /// <summary>
        /// Property for _expectedDifference
        /// </summary>
        public string ExpectedDifference
        {
            get { return _expectedDifference; }
            set
            {
                _expectedDifference = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _expectedHitCount
        /// </summary>
        public int ExpectedHitCount
        {
            get { return _expectedHitCount; }
            set
            {
                _expectedHitCount = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _currentKeyCandidate
        /// </summary>
        public string CurrentKeyCandidate
        {
            get { return _currentKeyCandidate; }
            set
            {
                _currentKeyCandidate = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _messagePairCountToExamine
        /// </summary>
        public string MessagePairCountToExamine
        {
            get { return _messagePairCountToExamine; }
            set
            {
                _messagePairCountToExamine = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _currentRecoveredRoundKey
        /// </summary>
        public string CurrentRecoveredRoundKey
        {
            get { return _currentRecoveredRoundKey; }
            set
            {
                _currentRecoveredRoundKey = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _currentKeysToTestThisRound
        /// </summary>
        public int CurrentKeysToTestThisRound
        {
            get { return _currentKeysToTestThisRound; }
            set
            {
                _currentKeysToTestThisRound = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _keyResults
        /// </summary>
        public ObservableCollection<KeyResult> KeyResults
        {
            get { return _keyResults; }
            set
            {
                _keyResults = value;
                OnPropertyChanged();
            }
        }

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
            if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
