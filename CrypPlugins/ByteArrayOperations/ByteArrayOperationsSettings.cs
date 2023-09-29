/*                              
   Copyright 2022 Nils Kopal, CrypTool Team

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
using System.ComponentModel;
using System.Linq;
using System.Windows;
using CrypTool.PluginBase;

namespace ByteArrayOperations
{
    internal enum ByteArrayOperation
    {
        Concatenate,
        Subarray,
        Length,
        IndexOf,
        Equals,
        Replace,
        Reverse,
        And,
        Or,
        ExclusiveOr,
        Not,
        LeftShift,
        RightShift,
        LeftCircularShift,
        RightCircularShift,
        Shuffle
    }

    internal class ByteArrayOperationsSettings : ISettings
    {
        private ByteArrayOperation _stringOperationType;
        private readonly Dictionary<ByteArrayOperation, List<string>> _operationVisibility = new Dictionary<ByteArrayOperation, List<string>>();
        private readonly List<string> _operationList = new List<string>();
        private int _value1 = 0;
        private int _value2 = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        public ByteArrayOperationsSettings()
        {
            foreach (ByteArrayOperation name in Enum.GetValues(typeof(ByteArrayOperation)))
            {
                _operationVisibility[name] = new List<string>();
            }
        }

        public void Initialize()
        {
            _operationList.Add("Array1");
            _operationList.Add("Array2");
            _operationList.Add("Array3");
            _operationList.Add("Value1");
            _operationList.Add("Value2");
            _operationVisibility[ByteArrayOperation.Concatenate].Add("Array2");
            _operationVisibility[ByteArrayOperation.Equals].Add("Array2");
            _operationVisibility[ByteArrayOperation.IndexOf].Add("Array2");
            _operationVisibility[ByteArrayOperation.Replace].Add("Array2");
            _operationVisibility[ByteArrayOperation.Replace].Add("Array3");
            _operationVisibility[ByteArrayOperation.Subarray].Add("Value1");
            _operationVisibility[ByteArrayOperation.Subarray].Add("Value2");
            UpdateTaskPaneVisibility();
        }

        [TaskPane("OperationCaption", "OperationTooltip", null, 1, false, ControlType.ComboBox, new[] { "Concatenate", "Subarray", "Length", "IndexOf", "Equals", "Replace", "Reverse", "And", "Or", "ExclusiveOR", "Not", "LeftShift", "RightShift", "LeftCircularShift", "RightCircularShift", "Shuffle" })]
        public ByteArrayOperation Operation
        {
            get => _stringOperationType;
            set
            {
                if (_stringOperationType != value)
                {
                    _stringOperationType = value;
                    UpdateTaskPaneVisibility();
                    OnPropertyChanged("Operation");
                }
            }
        }

        [TaskPane("Value1Caption", "Value1Tooltip", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int Value1
        {
            get => _value1;
            set
            {
                _value1 = value;
                OnPropertyChanged("Value1");
            }
        }

        [TaskPane("Value2Caption", "Value2Tooltip", null, 3, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int Value2
        {
            get => _value2;
            set
            {
                _value2 = value;
                OnPropertyChanged("Value2");
            }
        }

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        internal void UpdateTaskPaneVisibility()
        {
            if (TaskPaneAttributeChanged == null)
            {
                return;
            }

            foreach (TaskPaneAttribteContainer tpac in _operationList.Select(operation => new TaskPaneAttribteContainer(operation, (_operationVisibility[Operation].Contains(operation)) ? Visibility.Visible : Visibility.Collapsed)))
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(tpac));
            }
        }

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;
    }
}
