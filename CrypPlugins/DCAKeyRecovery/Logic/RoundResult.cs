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

namespace DCAKeyRecovery.Logic
{
    public class RoundResult : INotifyPropertyChanged
    {
        private int _roundNumber;
        private int _remainingKeys;

        /// <summary>
        /// Property for remaining keys count
        /// </summary>
        public int RemainingKeys
        {
            get => _remainingKeys;
            set
            {
                _remainingKeys = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for round number
        /// </summary>
        public int RoundNumber
        {
            get => _roundNumber;
            set
            {
                _roundNumber = value;
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
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
