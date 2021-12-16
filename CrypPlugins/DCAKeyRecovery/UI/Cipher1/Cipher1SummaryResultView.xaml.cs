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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace DCAKeyRecovery.UI.Cipher1
{
    /// <summary>
    /// Interaktionslogik für Cipher1SummaryResultView.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("DCAKeyRecovery.Properties.Resources")]
    public partial class Cipher1SummaryResultView : UserControl, INotifyPropertyChanged
    {
        //summary view
        private DateTime _startTime;
        private DateTime _endTime;
        private int _testedKeys;
        private int _currentRound;
        private int _messageCount;
        private int _decryptionCount;

        //round1
        private DateTime _startTimeRound1;
        private DateTime _endTimeRound1;
        private int _messageCountRound1;
        private int _decryptionCountRound1;
        private int _testedKeysRound1;
        private string _recoveredSubKey0;
        private string _recoveredSubKey1;

        /// <summary>
        /// Constructor
        /// </summary>
        public Cipher1SummaryResultView()
        {
            DataContext = this;
            InitializeComponent();
        }

        #region properties

        /// <summary>
        /// Property for _startTime
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
        /// Property for _endTime
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
        /// Property for _testedKeys
        /// </summary>
        public int TestedKeys
        {
            get => _testedKeys;
            set
            {
                _testedKeys = value;
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
        /// Property for _messageCount
        /// </summary>
        public int MessageCount
        {
            get => _messageCount;
            set
            {
                _messageCount = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _decryptionCount
        /// </summary>
        public int DecryptionCount
        {
            get => _decryptionCount;
            set
            {
                _decryptionCount = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _startTimeRound1
        /// </summary>
        public DateTime StartTimeRound1
        {
            get => _startTimeRound1;
            set
            {
                _startTimeRound1 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _endTimeRound1
        /// </summary>
        public DateTime EndTimeRound1
        {
            get => _endTimeRound1;
            set
            {
                _endTimeRound1 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _messageCountRound1
        /// </summary>
        public int MessageCountRound1
        {
            get => _messageCountRound1;
            set
            {
                _messageCountRound1 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _decryptionCountRound1
        /// </summary>
        public int DecryptionCountRound1
        {
            get => _decryptionCountRound1;
            set
            {
                _decryptionCountRound1 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _testedKeysRound1
        /// </summary>
        public int TestedKeysRound1
        {
            get => _testedKeysRound1;
            set
            {
                _testedKeysRound1 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _recoveredSubKey0
        /// </summary>
        public string RecoveredSubKey0
        {
            get => _recoveredSubKey0;
            set
            {
                _recoveredSubKey0 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _recoveredSubKey1
        /// </summary>
        public string RecoveredSubKey1
        {
            get => _recoveredSubKey1;
            set
            {
                _recoveredSubKey1 = value;
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
