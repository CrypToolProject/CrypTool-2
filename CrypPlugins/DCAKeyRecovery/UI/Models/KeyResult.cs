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

namespace DCAKeyRecovery.UI.Models
{
    public class KeyResult : INotifyPropertyChanged
    {
        private int _key;
        private string _binaryKey;
        private double _probability;
        private int _hitCount;

        /// <summary>
        /// Property for _key
        /// </summary>
        public int Key
        {
            get => _key;
            set
            {
                _key = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _binaryKey
        /// </summary>
        public string BinaryKey
        {
            get => _binaryKey;
            set
            {
                _binaryKey = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _probability
        /// </summary>
        public double Probability
        {
            get => _probability;
            set
            {
                _probability = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _hitCount
        /// </summary>
        public int HitCount
        {
            get => _hitCount;
            set
            {
                _hitCount = value;
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
