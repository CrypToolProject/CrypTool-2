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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DCAPathVisualiser.UI.Controls
{
    /// <summary>
    /// Interaktionslogik für _4BitSBox.xaml
    /// </summary>
    public partial class _4BitSBox : UserControl, INotifyPropertyChanged
    {
        private readonly string _activeColor = "Red";
        private readonly string _inActiveColor = "Black";
        private bool _isActive;

        public _4BitSBox()
        {
            IsActive = false;
            DataContext = this;
            InitializeComponent();
        }

        /// <summary>
        /// Property for the SBoxColor
        /// </summary>
        public string SBoxColor
        {
            get
            {
                if (IsActive)
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for IsActive
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                OnPropertyChanged();
                OnPropertyChanged("SBoxColor");
            }
        }

        /// <summary>
        /// Handles the mouse click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleSBoxClick(object sender, MouseButtonEventArgs e)
        {
            Rectangle elem = (Rectangle)sender;
            if (elem.Stroke == Brushes.Black)
            {
                elem.Stroke = Brushes.Red;
            }
            else
            {
                elem.Stroke = Brushes.Black;
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
