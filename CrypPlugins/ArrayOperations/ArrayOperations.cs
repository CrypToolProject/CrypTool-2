/*                              
   Copyright 2019 Simon Leischnig

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
using System.Text;
using System.Text.RegularExpressions;
using CrypTool.PluginBase;
using ArrayOperations.Properties;
using System.Collections.Generic;

namespace ArrayOperations
{
    [Author("Simon Leischnig", "simon-jena@gmx.net", "", "https://simlei.github.io/hi/")]
    [PluginInfo("ArrayOperations.Properties.Resources", "PluginCaption", "PluginTooltip", "ArrayOperations/DetailedDescription/doc.xml", "ArrayOperations/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class ArrayOperations : ICrypComponent
    {
        private ArrayOperationsSettings _settings = null;

        private Array _array1;
        private Array _array2;
        private Array _array3;
        private Object _object1;
        private Object _object2;
        private int _value1;
        private int _value2;
        private Array _outputArray;
        private object _outputValue;

        public ArrayOperations()
        {
            _settings = new ArrayOperationsSettings();            
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public ISettings Settings
        {
            get { return _settings; }
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
            // MinValue for integer-valued inputs is a marker for 
            // "not set through an input" in this plugin
            _array1 = null;
            _array2 = null;
            _array3 = null;
            _value1 = Int32.MinValue;
            _value2 = Int32.MinValue;
            _object1 = null;
            _object2 = null;

        }

        #region basic methods
        // looks for the first occurrence of object v2 in object v1
        // returns the index of that occurrence
        private int idxOf(object[] v1, object[] v2)
        {
            for (int i = 0; i < v1.Length; i++)
            {
                if (i + v2.Length > v1.Length)
                {
                    break;
                }
                bool matched = true;
                for (int j = 0; j < v2.Length; j++)
                {
                    if (! v1[i + j].Equals(v2[j]))
                    {
                        matched = false;
                        break;
                    }
                }
                if (matched)
                {
                    return i;
                }
            }
            return -1;
        }

        // reads an Array input into a primitive array type and performs null checking
        // throws ArgumentException if anything is wrong with an input
        private object[] unboxArray(Array boxed, string target)
        {

            if (boxed == null)
            {
                throw new ArgumentException(String.Format(Resources.ArrayNotSetError, target));
            }
            object[] result = new object[boxed.Length];
            boxed.CopyTo(result, 0);
            return result;
        }

        // reads an array or, if that is null, a one-elemnent array (the latter from a provided value)
        // throws ArgumentException if anything is wrong with those values (e.g. either are null valued)
        private object[] unboxArrayOrOneElementList(Array array1, object value1, string target)
        {
            if (array1 == null)
            {
                return unboxArrayFromObject(value1, target);
            }
            else
            {
                return unboxArray(array1, target);
            }
        }

        private object[] unboxArrayFromObject(object value1, string target)
        {
            if (value1 == null)
            {
                throw new ArgumentException(String.Format(Resources.ArrayNotSetError, target));
            }
            else if (value1 is Array result)
            {
                return unboxArray(result, target);
            }
            else
            {
                return new object[] { value1 };
            }
        }

        // performs range checking on an integer input and also checks for Int32.MinValue
        // throws ArgumentException if anything goes wrong in those checks
        private int unboxInt(int numericVal, int min, int max, string target, int valIfUndefined=Int32.MinValue)
        {
            if (numericVal == Int32.MinValue)
            {
                if (valIfUndefined == Int32.MinValue)
                {
                    throw new ArgumentException(String.Format(Resources.InputNotSetError, target));
                }
                return valIfUndefined;
            }
            if (numericVal < min || numericVal > max)
            {
                throw new ArgumentException(String.Format(Resources.InputNotValidError, target, String.Format(Resources.IndexError, numericVal, min, max)));
            }
            return numericVal;
        }

        #endregion

        // Executes the module. For this, the _settings.operation is decoded
        // and subsequently, the correct checks are performed for the relevant inputs.
        // Each Operation is then performed in it's designated method.
        // throws ArgumentException if anything goes wrong in the input checks
        public void Execute()
        {

            try
            {
                switch (_settings.Operation)
                {
                    case ArrayOperationType.Union:
                        assignOutputArray(OpUnion(
                            unboxArray(_array1, Resources.Arrayarg1), 
                            unboxArrayOrOneElementList(_array2, _object1, Resources.Arrayarg2)
                            ));
                        break;
                    case ArrayOperationType.Complement:
                        assignOutputArray(OpComplement(
                            unboxArray(_array1, Resources.Arrayarg1), 
                            unboxArrayOrOneElementList(_array2, _object1, Resources.Arrayarg2)
                            ));
                        break;
                    case ArrayOperationType.Intersection:
                        assignOutputArray(OpIntersection(
                            unboxArray(_array1, Resources.Arrayarg1), 
                            unboxArrayOrOneElementList(_array2, _object1, Resources.Arrayarg2)
                            ));
                        break;
                    case ArrayOperationType.Concatenation:
                        assignOutputArray(OpConcatenation(
                            unboxArray(_array1, Resources.Arrayarg1), 
                            unboxArrayOrOneElementList(_array2, _object1, Resources.Arrayarg2)
                            ));
                        break;
                    case ArrayOperationType.Equals:
                        assignOutputValue(OpEquals(
                            unboxArray(_array1, Resources.Arrayarg1), 
                            unboxArrayOrOneElementList(_array2, _object1, Resources.Arrayarg2)
                            ));
                        break;
                    case ArrayOperationType.Unique:
                        assignOutputArray(OpUnique(
                            unboxArray(_array1, Resources.Arrayarg1)
                            ));
                        break;
                    case ArrayOperationType.Length:
                        assignOutputValue(OpLength(
                            unboxArray(_array1, Resources.Arrayarg1)
                            ));
                        break;
                    case ArrayOperationType.Replace:
                        assignOutputArray(OpReplace(
                            unboxArray(_array1, Resources.Arrayarg1), 
                            unboxArrayOrOneElementList(_array2, _object1, Resources.Arrayarg2), 
                            unboxArrayOrOneElementList(_array3, _object2, Resources.Arrayarg3)
                            ));
                        break;
                    case ArrayOperationType.SortAscending:
                        assignOutputArray(OpSort(
                            unboxArray(_array1, Resources.Arrayarg1),
                            1
                            ));
                        break;
                    case ArrayOperationType.SortDescending:
                        assignOutputArray(OpSort(
                            unboxArray(_array1, Resources.Arrayarg1),
                            -1
                            ));
                        break;
                    case ArrayOperationType.Reverse:
                        assignOutputArray(OpReverse(
                            unboxArray(_array1, Resources.Arrayarg1)
                            ));
                        break;
                    case ArrayOperationType.Subarray:
                        assignOutputArray(OpSubarray(
                            unboxArray(_array1, Resources.Arrayarg1),
                            unboxInt(_value1, 0, _array1.Length-1, Resources.NumericArg1), 
                            unboxInt(_value2, 0, _array1.Length-_value1, Resources.NumericArg2, _array1.Length-_value1)
                            ));
                        break;
                    case ArrayOperationType.IndexOf:
                        assignOutputValue(OpIndexOf(
                            unboxArray(_array1, Resources.Arrayarg1),
                            unboxArrayOrOneElementList(_array2, _object1, Resources.Arrayarg2)
                            ));
                        break;
                    case ArrayOperationType.Contains:
                        assignOutputValue(OpContains(
                            unboxArray(_array1, Resources.Arrayarg1),
                            unboxArrayOrOneElementList(_array2, _object1, Resources.Arrayarg2)
                            ));
                        break;
                }
                ProgressChanged(1, 1);
            }
            catch (Exception ex)
            {
                if (ex is ArgumentException argEx)
                {
                    GuiLogMessage("Input Error: " + argEx.Message, NotificationLevel.Error);
                } else
                {
                    GuiLogMessage("Error", NotificationLevel.Error);
                }
            }
        }

        #region Operation implementations

        // individual implementations for array operations. Each operation can safely assume that 
        // the provided values make sense (are valid).

        private bool OpContains(object[] v1, object[] v2)
        {
            int idx = idxOf(v1, v2);
            if (idx == -1)
            {
                return false;
            }
            return true;
        }

        private int OpIndexOf(object[] v1, object[] v2)
        {
            return idxOf(v1, v2);
        }

        private object[] OpReverse(object[] v)
        {
            object[] result = new object[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                result[v.Length - i - 1] = v[i];
            }
            return result;
        }

        private object[] OpSort(object[] v1, int v2)
        {
            Array.Sort(v1);
            if (v2 == -1)
            {
                Array.Reverse(v1);
            }
            return v1;
        }

        private object[] OpReplace(object[] v1, object[] v2, object[] v3)
        {
            int idx = idxOf(v1, v2);
            if (idx > -1)
            {
                object[] result = new object[v1.Length - v2.Length + v3.Length];
                Array.Copy(v1, 0, result, 0, idx);
                Array.Copy(v3, 0, result, idx, v3.Length);
                Array.Copy(v1, idx+v3.Length, result, idx+v3.Length, v1.Length-idx-v3.Length);
                return result;
            }
            else
            {
                return v1;
            }
        }

        private int OpLength(object[] v)
        {
            return v.Length;
        }

        private object[] OpUnique(object[] v)
        {
            object[] result = new object[v.Length];
            int uniques = 0;
            for (int i = 0; i < v.Length; i++)
            {
                bool isUniq = true;
                for (int j = 0; j < uniques; j++)
                {
                    if (v[i].Equals(result[j]))
                    {
                        isUniq = false;
                        break;
                    }
                }

                if (isUniq)
                {
                    result[uniques] = v[i];
                    uniques = uniques + 1;
                }
            }
            object[] endresult = new object[uniques];
            for (int i = 0; i < uniques; i++)
            {
                endresult[i] = result[i];
            }
            return endresult;
        }

        private bool OpEquals(object[] v1, object[] v2)
        {
            if (v1.Length != v2.Length)
            {
                return false;
            }
            for (int i = 0; i < v1.Length; i++)
            {
                if (! v1[i].Equals(v2[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private object[] OpConcatenation(object[] v, object[] v1)
        {
            object[] result = new object[v.Length + v1.Length];
            v.CopyTo(result, 0);
            v1.CopyTo(result, v.Length);
            return result;
        }

        private object[] OpComplement(object[] v1, object[] v2)
        {
            object[] all = new object[v1.Length + v2.Length];
            object[] result = new object[v1.Length + v2.Length];
            v1.CopyTo(all, 0);
            v2.CopyTo(all, v1.Length);
            int uniques = 0;
            for (int i = 0; i < all.Length; i++)
            {
                bool isUniq = true;
                bool isInArr2 = false;
                for (int j = 0; j < uniques; j++)
                {
                    if (all[i].Equals(result[j]))
                    {
                        isUniq = false;
                        break;
                    }
                }
                for (int j = 0; j < v2.Length; j++)
                {
                    if (all[i].Equals(v2[j]))
                    {
                        isInArr2 = true;
                        break;
                    }
                }

                if (isUniq && ! isInArr2)
                {
                    result[uniques] = all[i];
                    uniques = uniques + 1;
                }
            }
            object[] endresult = new object[uniques];
            for (int i = 0; i < uniques; i++)
            {
                endresult[i] = result[i];
            }
            return endresult;
        }

        private object[] OpIntersection(object[] v1, object[] v2)
        {
            object[] all = new object[v1.Length + v2.Length];
            object[] result = new object[v1.Length + v2.Length];
            v1.CopyTo(all, 0);
            v2.CopyTo(all, v1.Length);
            int uniques = 0;
            for (int i = 0; i < all.Length; i++)
            {
                bool isUniq = true;
                bool isInArr1 = false;
                bool isInArr2 = false;
                for (int j = 0; j < uniques; j++)
                {
                    if (all[i].Equals(result[j]))
                    {
                        isUniq = false;
                        break;
                    }
                }
                for (int j = 0; j < v1.Length; j++)
                {
                    if (all[i].Equals(v1[j]))
                    {
                        isInArr1 = true;
                        break;
                    }
                }
                for (int j = 0; j < v2.Length; j++)
                {
                    if (all[i].Equals(v2[j]))
                    {
                        isInArr2 = true;
                        break;
                    }
                }

                if (isUniq && isInArr1 && isInArr2)
                {
                    result[uniques] = all[i];
                    uniques = uniques + 1;
                }
            }
            object[] endresult = new object[uniques];
            for (int i = 0; i < uniques; i++)
            {
                endresult[i] = result[i];
            }
            return endresult;
        }

        private object[] OpSubarray(object[] arr, int idx, int length)
        {
            object[] result = new object[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = arr[idx + i];
            }
            return result;
        }


        private object[] OpUnion(object[] arr1, object[] arr2)
        {
            object[] all = new object[arr1.Length + arr2.Length];
            object[] result = new object[arr1.Length + arr2.Length];
            arr1.CopyTo(all, 0);
            arr2.CopyTo(all, arr1.Length);

            int uniques = 0;
            for (int i = 0; i < all.Length; i++)
            {
                bool isUniq = true;
                for (int j = 0; j < uniques; j++)
                {
                    if (all[i].Equals(result[j]))
                    {
                        isUniq = false;
                        break;
                    }
                }

                if (isUniq)
                {
                    result[uniques] = all[i];
                    uniques = uniques + 1;
                }
            }
            object[] endresult = new object[uniques];
            for (int i = 0; i < uniques; i++)
            {
                endresult[i] = result[i];
            }
            return endresult;
        }

        #endregion

        public void PostExecution()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
            _settings.UpdateTaskPaneVisibility();
        }

        public void Dispose()
        {
        }

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        [PropertyInfo(Direction.InputData, "Array1Caption", "Array1Tooltip", false)]
        public Array Array1
        {
            get { return _array1; }
            set { _array1 = value; }
        }

        [PropertyInfo(Direction.InputData, "Array2Caption", "Array2Tooltip", false)]
        public Array Array2
        {
            get { return _array2; }
            set { _array2 = value; }
        }

        [PropertyInfo(Direction.InputData, "Array3Caption", "Array3Tooltip", false)]
        public Array Array3
        {
            get { return _array3; }
            set { _array3 = value; }
        }

        [PropertyInfo(Direction.InputData, "Value1Caption", "Value1Tooltip", false)]
        public int Value1
        {
            get { return _value1; }
            set { _value1 = value; }
        }

        [PropertyInfo(Direction.InputData, "Value2Caption", "Value2Tooltip", false)]
        public int Value2
        {
            get { return _value2; }
            set { _value2 = value; }
        }

        [PropertyInfo(Direction.InputData, "Object1Caption", "Object1Tooltip", false)]
        public Object Object1
        {
            get { return _object1; }
            set { _object1 = value; }
        }

        [PropertyInfo(Direction.InputData, "Object2Caption", "Object2Tooltip", false)]
        public Object Object2
        {
            get { return _object2; }
            set { _object2 = value; }
        }

        [PropertyInfo(Direction.OutputData, "OutputArrayCaption", "OutputArrayTooltip", false)]
        public Array OutputArray
        {
            get { return _outputArray; }
        }


        [PropertyInfo(Direction.OutputData, "OutputValueCaption", "OutputValueTooltip", false)]
        public Object OutputValue
        {
            get { return _outputValue; }
        }

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        private void assignOutputArray(object[] subject)
        {
            _outputArray = subject;
            OnPropertyChanged("OutputArray");
        }

        private void assignOutputValue(object subject)
        {
            _outputValue = subject;
            OnPropertyChanged("OutputValue");
        }

        public void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, logLevel));
            }
        }

        public void ProgressChanged(double value, double max)
        {
            if (OnPluginProgressChanged != null)
            {
                OnPluginProgressChanged(this, new PluginProgressEventArgs(value, max));
            }
        }
    }
}
