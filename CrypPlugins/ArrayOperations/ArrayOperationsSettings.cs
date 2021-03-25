/*                              
   Copyright 2011 Nils Kopal, Uni Duisburg-Essen

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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using CrypTool.PluginBase;
using System;

namespace ArrayOperations
{
    class ArrayOperationsSettings : ISettings
    {
        private ArrayOperationType _arrayOperationType;
        private readonly Dictionary<ArrayOperationType, List<string>> _operationVisibility = new Dictionary<ArrayOperationType, List<string>>();
        private readonly List<string> _operationList = new List<string>();

        private object _object1 = null;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            UpdateTaskPaneVisibility();
        }

        #endregion

        public ArrayOperationsSettings()
        {
            foreach (ArrayOperationType name in Enum.GetValues(typeof(ArrayOperationType)))
            {
                _operationVisibility[name] = new List<string>();
            }
        }


        [TaskPane("OperationCaption", "OperationTooltip", null, 1, false, ControlType.ComboBox, new string[] { "OperationList1", "OperationList2", "OperationList3", "OperationList4", "OperationList5", "OperationList6", "OperationList7", "OperationList8", "OperationList9", "OperationList10", "OperationList11", "OperationList12", "OperationList13", "OperationList14" })]
        public ArrayOperationType Operation
        {
            get
            {
                return _arrayOperationType;
            }
            set
            {
                if (_arrayOperationType != value)
                {
                    _arrayOperationType = value;
                    UpdateTaskPaneVisibility();
                    OnPropertyChanged("Operation");
                }
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
                return;

            foreach (var tpac in _operationList.Select(operation => new TaskPaneAttribteContainer(operation, (_operationVisibility[Operation].Contains(operation)) ? Visibility.Visible : Visibility.Collapsed)))
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(tpac));
            }
        }

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;
    }

    enum ArrayOperationType
    {
        Union,
        Complement,
        Intersection,
        Concatenation,
        Equals,
        Unique,
        Length,
        Replace,
        SortAscending,
        SortDescending,
        Reverse,
        Subarray,
        IndexOf,
        Contains
    }
}
