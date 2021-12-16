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

namespace DCAPathFinder.UI.Tutorial1
{
    public class XorTableMapping : INotifyPropertyChanged
    {
        private string _col1;
        private string _col2;
        private string _colResult;

        /// <summary>
        /// Property for column1
        /// </summary>
        public string Col1
        {
            get => _col1;
            set
            {
                _col1 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for column2
        /// </summary>
        public string Col2
        {
            get => _col2;
            set
            {
                _col2 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for column3
        /// </summary>
        public string ColResult
        {
            get => _colResult;
            set
            {
                _colResult = value;
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