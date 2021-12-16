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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DCAPathFinder.UI.Controls
{
    /// <summary>
    /// Interaktionslogik für _4BitSBox.xaml
    /// </summary>
    public partial class _4BitSBox : UserControl, INotifyPropertyChanged
    {
        private string _outputColor;
        private Brush _labelTextColor;
        private bool _isClickable;
        private bool _alreadyAttacked;
        private bool _isSelected = false;
        public event EventHandler<EventArgs> SelectionChanged;

        public _4BitSBox()
        {
            _isSelected = false;
            _outputColor = "Black";
            _labelTextColor = Brushes.Black;
            _isClickable = true;
            _alreadyAttacked = false;

            DataContext = this;
            InitializeComponent();
        }

        /// <summary>
        /// Property for label text color
        /// </summary>
        public Brush LabelTextColor
        {
            get => _labelTextColor;
            set
            {
                _labelTextColor = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to indicate that SBox was already attacked
        /// </summary>
        public bool AlreadyAttacked
        {
            get => _alreadyAttacked;
            set
            {
                _alreadyAttacked = value;
                if (!_alreadyAttacked)
                {
                    OutputColor = "Black";
                    LabelTextColor = Brushes.Black;
                }
                else
                {
                    //OutputColor = "Gray";
                    //LabelTextColor = Brushes.Gray;

                    OutputColor = "LimeGreen";
                    LabelTextColor = Brushes.LimeGreen;
                }

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to configure if SBox is clickable
        /// </summary>
        public bool IsClickable
        {
            get => _isClickable;
            set
            {
                _isClickable = value;

                if (_isClickable)
                {
                    OutputColor = "Gray";
                    LabelTextColor = Brushes.Gray;
                }

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for output color
        /// </summary>
        public string OutputColor
        {
            get => _outputColor;
            set
            {
                _outputColor = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property of _isSelected
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                if (SelectionChanged != null)
                {
                    SelectionChanged.Invoke(this, null);
                }

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Handles the mouse click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleSBoxClick(object sender, MouseButtonEventArgs e)
        {
            if (!IsClickable)
            {
                return;
            }

            Rectangle elem = (Rectangle)sender;
            /*
            if (elem.Stroke == Brushes.Black)
            {
                IsSelected = true;
                elem.Stroke = Brushes.Red;
                LabelTextColor = Brushes.Red;
            }
            else
            {
                IsSelected = false;
                elem.Stroke = Brushes.Black;
                LabelTextColor = Brushes.Black;
            }
            */

            if (elem.Stroke == Brushes.Gray)
            {
                IsSelected = true;
                elem.Stroke = Brushes.Red;
                LabelTextColor = Brushes.Red;
            }
            else
            {
                IsSelected = false;
                elem.Stroke = Brushes.Gray;
                LabelTextColor = Brushes.Gray;
            }
        }

        public void SetOfflineSelected()
        {
            OutputColor = "Red";
            LabelTextColor = Brushes.Red;
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