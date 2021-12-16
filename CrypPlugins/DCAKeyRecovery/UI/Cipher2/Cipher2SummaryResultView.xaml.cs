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

namespace DCAKeyRecovery.UI.Cipher2
{
    /// <summary>
    /// Interaktionslogik für Cipher2SummaryResultView.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("DCAKeyRecovery.Properties.Resources")]
    public partial class Cipher2SummaryResultView : UserControl, INotifyPropertyChanged
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

        //round2
        private DateTime _startTimeRound2;
        private DateTime _endTimeRound2;
        private int _messageCountRound2;
        private int _decryptionCountRound2;
        private int _testedKeysRound2;
        private string _recoveredSubKey2;

        //round3
        private DateTime _startTimeRound3;
        private DateTime _endTimeRound3;
        private int _messageCountRound3;
        private int _decryptionCountRound3;
        private int _testedKeysRound3;
        private string _recoveredSubKey3;

        /// <summary>
        /// Constructor
        /// </summary>
        public Cipher2SummaryResultView()
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

        /// <summary>
        /// Property for _startTimeRound2
        /// </summary>
        public DateTime StartTimeRound2
        {
            get => _startTimeRound2;
            set
            {
                _startTimeRound2 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _endTimeRound2
        /// </summary>
        public DateTime EndTimeRound2
        {
            get => _endTimeRound2;
            set
            {
                _endTimeRound2 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _messageCountRound2
        /// </summary>
        public int MessageCountRound2
        {
            get => _messageCountRound2;
            set
            {
                _messageCountRound2 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _decryptionCountRound2
        /// </summary>
        public int DecryptionCountRound2
        {
            get => _decryptionCountRound2;
            set
            {
                _decryptionCountRound2 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _testedKeysRound2
        /// </summary>
        public int TestedKeysRound2
        {
            get => _testedKeysRound2;
            set
            {
                _testedKeysRound2 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _recoveredSubKey2
        /// </summary>
        public string RecoveredSubKey2
        {
            get => _recoveredSubKey2;
            set
            {
                _recoveredSubKey2 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _startTimeRound3
        /// </summary>
        public DateTime StartTimeRound3
        {
            get => _startTimeRound3;
            set
            {
                _startTimeRound3 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _endTimeRound3
        /// </summary>
        public DateTime EndTimeRound3
        {
            get => _endTimeRound3;
            set
            {
                _endTimeRound3 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _messageCountRound3
        /// </summary>
        public int MessageCountRound3
        {
            get => _messageCountRound3;
            set
            {
                _messageCountRound3 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _decryptionCountRound3
        /// </summary>
        public int DecryptionCountRound3
        {
            get => _decryptionCountRound3;
            set
            {
                _decryptionCountRound3 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _testedKeysRound3
        /// </summary>
        public int TestedKeysRound3
        {
            get => _testedKeysRound3;
            set
            {
                _testedKeysRound3 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _recoveredSubKey3
        /// </summary>
        public string RecoveredSubKey3
        {
            get => _recoveredSubKey3;
            set
            {
                _recoveredSubKey3 = value;
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
