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
using ByteArrayOperations.Properties;
using CrypTool.PluginBase;

namespace ByteArrayOperations
{
    [Author("Nils Kopal", "nils.kopal@CrypTool.org", "CrypTool Team", "http://www.uni-duisburg-essen.de")]
    [PluginInfo("ByteArrayOperations.Properties.Resources", "PluginCaption", "PluginTooltip", "ByteArrayOperations/DetailedDescription/doc.xml", "ByteArrayOperations/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class ByteArrayOperations : ICrypComponent
    {
        private readonly ByteArrayOperationsSettings _settings = null;

        private byte[] _array1;
        private byte[] _array2;
        private byte[] _array3;
        private int _value1;
        private int _value2;
        private byte[] _outputArray;
        private int _outputValue;

        public ByteArrayOperations()
        {
            _settings = new ByteArrayOperationsSettings();
        }

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public ISettings Settings => _settings;

        public System.Windows.Controls.UserControl Presentation => null;

        public void PreExecution()
        {
            _array1 = null;
            _array2 = null;
            _array3 = null;
            _value1 = -1;
            _value2 = -1;
        }

        public void Execute()
        {

            if (_array1 == null)
            {
                _array1 = new byte[0];
            }
            if (_array2 == null)
            {
                _array2 = new byte[0];
            }
            if (_array3 == null)
            {
                _array3 = new byte[0];
            }

            if (_value1 == -1)
            {
                _value1 = _settings.Value1;
            }
            if (_value2 == -1)
            {
                _value2 = _settings.Value2;
            }

            try
            {
                switch (_settings.Operation)
                {
                    case ByteArrayOperation.Concatenate:
                        _outputArray = Concat(_array1, _array2);
                        OnPropertyChanged(nameof(OutputArray));
                        break;
                    case ByteArrayOperation.Subarray:
                        if (_array1.Length > 0 && _value1 < 0 && _value1 >= -_array1.Length)
                        {
                            _value1 = (_value1 + _array1.Length) % _array1.Length;
                        }
                        _outputArray = Subarray(_array1, _value1, _value2);
                        OnPropertyChanged(nameof(OutputArray));
                        break;
                    case ByteArrayOperation.Length:
                        _outputValue = _array1.Length;
                        OnPropertyChanged(nameof(OutputValue));
                        break;
                    case ByteArrayOperation.IndexOf:
                        _outputValue = IndexOf(_array1, _array2);
                        OnPropertyChanged(nameof(OutputValue));
                        break;
                    case ByteArrayOperation.Equals:
                        _outputValue = Equals(_array1, _array2) ? 1 : 0;
                        OnPropertyChanged(nameof(OutputValue));
                        break;
                    case ByteArrayOperation.Replace:
                        _outputArray = Replace(_array1, _array2, _array3);
                        OnPropertyChanged(nameof(OutputArray));
                        break;
                    case ByteArrayOperation.Reverse:
                        _outputArray = Reverse(_array1);
                        OnPropertyChanged(nameof(OutputArray));
                        break;
                    case ByteArrayOperation.And:
                        _outputArray = And(_array1, _array2);
                        OnPropertyChanged(nameof(OutputArray));
                        break;
                    case ByteArrayOperation.Or:
                        _outputArray = Or(_array1, _array2);
                        OnPropertyChanged(nameof(OutputArray));
                        break;
                    case ByteArrayOperation.ExclusiveOr:
                        _outputArray = ExclusiveOr(_array1, _array2);
                        OnPropertyChanged(nameof(OutputArray));
                        break;
                    case ByteArrayOperation.Not:
                        _outputArray = Not(_array1);
                        OnPropertyChanged(nameof(OutputArray));
                        break;
                    case ByteArrayOperation.LeftShift:
                        _outputArray = LeftShift(_array1, _value1);
                        OnPropertyChanged(nameof(OutputArray));
                        break;
                    case ByteArrayOperation.RightShift:
                        _outputArray = RightShift(_array1, _value1);
                        OnPropertyChanged(nameof(OutputArray));
                        break;
                    case ByteArrayOperation.LeftCircularShift:
                        _outputArray = LeftCircularShift(_array1, _value1);
                        OnPropertyChanged(nameof(OutputArray));
                        break;
                    case ByteArrayOperation.RightCircularShift:
                        _outputArray = RightCircularShift(_array1, _value1);
                        OnPropertyChanged(nameof(OutputArray));
                        break;
                    case ByteArrayOperation.Shuffle:
                        _outputArray = Shuffle(_array1);
                        OnPropertyChanged(nameof(OutputArray));
                        break;
                }
                ProgressChanged(1, 1);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format(Resources.ByteArrayOperations_Execute_Could_not_execute_operation___0______1_, (_settings).Operation, ex.Message), NotificationLevel.Error);
            }
        }


        /// <summary>
        /// Shuffles the given byte array using Fisher–Yates_shuffle
        /// See https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        private byte[] Shuffle(byte[] array)
        {
            if (array == null)
            {
                return new byte[0];
            }
            byte[] result = new byte[array.Length];
            Array.Copy(array, 0, result, 0, array.Length);

            Random random = new Random();
            for (int i = result.Length - 1; i >= 0; i--)
            {
                int j = random.Next(0, i);
                (result[i], result[j]) = (result[j], result[i]);
            }
            return result;
        }

        /// <summary>
        /// Returns a reversed copy of the byte array
        /// </summary>
        /// <param name="array1"></param>
        /// <returns></returns>
        private byte[] Reverse(byte[] array1)
        {
            byte[] result = new byte[array1.Length];
            int length = result.Length;
            int length_minus1 = length - 1;
            for (int i = 0; i < length; i++)
            {
                result[length_minus1 - i] = array1[i];
            }
            return result;
        }

        /// <summary>
        /// Returns a copy of the array1 where it replaced
        /// all occurences of array2 by array3
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <param name="array3"></param>
        /// <returns></returns>
        private byte[] Replace(byte[] array1, byte[] array2, byte[] array3)
        {
            List<byte> list = new List<byte>();
            int lengthToCheck = array1.Length - (array2.Length - 1);
            int array2length = array2.Length;

            for (int i = 0; i < lengthToCheck; i++)
            {
                if (CheckWord(i))
                {
                    foreach (byte b in array3)
                    {
                        list.Add(b);
                    }
                    i += (array2length - 1);
                }
                else
                {
                    list.Add(array1[i]);

                }
            }
            return list.ToArray();

            //check, if the given "word" is at given position
            bool CheckWord(int position)
            {
                for (int i = position; i < position + array2length; i++)
                {
                    if (array1[i] != array2[i - position])
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Returns the index of the first occurnce of array2 in array1
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private int IndexOf(byte[] array1, byte[] array2)
        {
            if (array1.Length == 0)
            {
                throw new ArgumentException("Search array must not be empty!");
            }
            if (array2.Length == 0)
            {
                throw new ArgumentException("Array to search for must not be empty!");
            }

            int lengthToCheck = array1.Length - (array2.Length - 1);
            int array2length = array2.Length;

            for (int i = 0; i < lengthToCheck; i++)
            {
                if (CheckWord(i))
                {
                    return i;
                }
            }

            return -1;

            //check, if the given "word" is at given position
            bool CheckWord(int position)
            {
                for (int i = position; i < position + array2length; i++)
                {
                    if (array1[i] != array2[i - position])
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Returns true if both given arrays are equal, otherwise returns false
        /// 
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        private bool Equals(byte[] array1, byte[] array2)
        {
            int array1length = array1.Length;
            int array2length = array2.Length;

            if (array1length != array2length)
            {
                return false;
            }
            for (int i = 0; i < array1length; i++)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns a subarray of the given array defined by offset and length
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private byte[] Subarray(byte[] array, int offset, int length)
        {
            if (offset < 0 || offset >= array.Length)
            {
                throw new ArgumentException(string.Format("Invalid offset. Array length is {0}, subarray offset is {1}, and subarray length is {2}", array.Length, offset, length));
            }
            if (offset + length > array.Length)
            {
                throw new ArgumentException(string.Format("Invalid length. Array length is {0}, subarray offset is {1}, and subarray length is {2}", array.Length, offset, length));
            }

            byte[] result = new byte[length];
            Array.Copy(array, offset, result, 0, length);

            return result;
        }

        /// <summary>
        /// Creates a new array that is the concatenation of array1 and array2
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        private byte[] Concat(byte[] array1, byte[] array2)
        {
            byte[] result = new byte[array1.Length + array2.Length];
            Array.Copy(array1, result, array1.Length);
            Array.Copy(array2, 0, result, array1.Length, array2.Length);
            return result;
        }

        /// <summary>
        /// Returns new array that contains "array1 logical AND array2"
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private byte[] And(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length)
            {
                throw new ArgumentException(string.Format("Array lengths are not equal. Array 1 lengt={0} and array 2 length={1}", array1.Length, array2.Length));
            }
            int length = array1.Length;
            byte[] result = new byte[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = (byte)(array1[i] & array2[i]);
            }
            return result;
        }

        /// <summary>
        /// Returns new array that contains "array1 logical OR array2"
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private byte[] Or(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length)
            {
                throw new ArgumentException(string.Format("Array lengths are not equal. Array 1 lengt={0} and array 2 length={1}", array1.Length, array2.Length));
            }
            int length = array1.Length;
            byte[] result = new byte[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = (byte)(array1[i] | array2[i]);
            }
            return result;
        }

        /// <summary>
        /// Returns new array that contains "array1 logical XOR array2"
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private byte[] ExclusiveOr(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length)
            {
                throw new ArgumentException(string.Format("Array lengths are not equal. Array 1 lengt={0} and array 2 length={1}", array1.Length, array2.Length));
            }
            int length = array1.Length;
            byte[] result = new byte[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = (byte)(array1[i] ^ array2[i]);
            }
            return result;
        }

        /// <summary>
        /// Returns new array that contains "logical NOT of array"
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private byte[] Not(byte[] array)
        {
            int length = array.Length;
            byte[] result = new byte[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = (byte)(~array[i]);
            }
            return result;
        }

        /// <summary>
        /// Returns new array that contains the by bits left-shifted given array
        /// Method found here: https://stackoverflow.com/questions/1275572/bit-shifting-a-byte-array-by-n-bits
        /// </summary>
        /// <param name="array"></param>
        /// <param name="bits"></param>
        /// <returns></returns>
        private byte[] LeftShift(byte[] array, int bits)
        {
            byte[] result = new byte[array.Length];
            if (bits >= 8)
            {
                Array.Copy(array, bits / 8, result, 0, result.Length - (bits / 8));
            }
            else
            {
                Array.Copy(array, result, result.Length);
            }
            int bitsmod8 = bits % 8;
            if (bitsmod8 != 0)
            {
                int length = result.Length;
                for (int i = 0; i < length; i++)
                {
                    result[i] <<= bitsmod8;
                    if (i < length - 1)
                    {
                        result[i] |= (byte)(result[i + 1] >> 8 - bitsmod8);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns new array that contains the by bits right-shifted given array
        /// Method found here: https://stackoverflow.com/questions/1275572/bit-shifting-a-byte-array-by-n-bits
        /// </summary>
        /// <param name="array"></param>
        /// <param name="bits"></param>
        /// <returns></returns>
        private byte[] RightShift(byte[] array, int bits)
        {
            byte[] result = new byte[array.Length];
            if (bits >= 8)
            {
                Array.Copy(array, 0, result, bits / 8, result.Length - (bits / 8));
            }
            else
            {
                Array.Copy(array, result, result.Length);
            }
            int bitsmod8 = bits % 8;
            if (bitsmod8 != 0)
            {
                int length = result.Length;
                for (int i = length - 1; i >= 0; i--)
                {
                    result[i] >>= bitsmod8;
                    if (i > 0)
                    {
                        result[i] |= (byte)(result[i - 1] << 8 - bitsmod8);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns new array that contains the by bits left-circular-shifted given array
        /// Method found here: https://stackoverflow.com/questions/30442915/ror-byte-array-with-c-sharp
        /// </summary>
        /// <param name="array"></param>
        /// <param name="bits"></param>
        /// <returns></returns>
        private byte[] LeftCircularShift(byte[] array, int bits)
        {
            byte[] result = new byte[array.Length];

            if (array.Length > 0)
            {
                int nByteShift = bits / (sizeof(byte) * 8);
                int nBitShift = bits % (sizeof(byte) * 8);

                if (nByteShift >= array.Length)
                {
                    nByteShift %= array.Length;
                }

                int length = array.Length;
                int s = length - 1;
                int d = s - nByteShift;

                for (int nCnt = 0; nCnt < array.Length; nCnt++, d--, s--)
                {
                    while (d < 0)
                    {
                        d += length;
                    }
                    while (s < 0)
                    {
                        s += length;
                    }

                    byte byteS = array[s];

                    result[d] |= (byte)(byteS << nBitShift);
                    result[d > 0 ? d - 1 : length - 1] |= (byte)(byteS >> (sizeof(byte) * 8 - nBitShift));
                }
            }

            return result;
        }

        /// <summary>
        /// Returns new array that contains the by bits right-circular-shifted given array
        /// Method found here: https://stackoverflow.com/questions/30442915/ror-byte-array-with-c-sharp
        /// </summary>
        /// <param name="array"></param>
        /// <param name="bits"></param>
        /// <returns></returns>
        private byte[] RightCircularShift(byte[] array, int bits)
        {
            byte[] result = new byte[array.Length];
            bits = array.Length * 8 - bits;
            if (array.Length > 0)
            {
                int nByteShift = bits / (sizeof(byte) * 8);
                int nBitShift = bits % (sizeof(byte) * 8);

                if (nByteShift >= array.Length)
                {
                    nByteShift %= array.Length;
                }

                int length = array.Length;
                int s = length - 1;
                int d = s - nByteShift;

                for (int nCnt = 0; nCnt < array.Length; nCnt++, d--, s--)
                {
                    while (d < 0)
                    {
                        d += length;
                    }
                    while (s < 0)
                    {
                        s += length;
                    }

                    byte byteS = array[s];

                    result[d] |= (byte)(byteS << nBitShift);
                    result[d > 0 ? d - 1 : length - 1] |= (byte)(byteS >> (sizeof(byte) * 8 - nBitShift));
                }
            }

            return result;
        }

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

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        [PropertyInfo(Direction.InputData, "Array1Caption", "Array1Tooltip", false)]
        public byte[] Array1
        {
            get => _array1;
            set => _array1 = value;
        }

        [PropertyInfo(Direction.InputData, "Array2Caption", "Array2Tooltip", false)]
        public byte[] Array2
        {
            get => _array2;
            set => _array2 = value;
        }

        [PropertyInfo(Direction.InputData, "Array3Caption", "Array3Tooltip", false)]
        public byte[] Array3
        {
            get => _array3;
            set => _array3 = value;
        }


        [PropertyInfo(Direction.InputData, "Value1Caption", "Value1Tooltip", false)]
        public int Value1
        {
            get => _value1;
            set => _value1 = value;
        }

        [PropertyInfo(Direction.InputData, "Value2Caption", "Value2Tooltip", false)]
        public int Value2
        {
            get => _value2;
            set => _value2 = value;
        }

        [PropertyInfo(Direction.OutputData, "OutputArrayCaption", "OutputArrayTooltip", false)]
        public byte[] OutputArray => _outputArray;

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        [PropertyInfo(Direction.OutputData, "OutputValueCaption", "OutputValueTooltip", false)]
        public int OutputValue => _outputValue;

        public void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            OnGuiLogNotificationOccured?.Invoke(this, new GuiLogEventArgs(message, this, logLevel));
        }

        public void ProgressChanged(double value, double max)
        {
            OnPluginProgressChanged?.Invoke(this, new PluginProgressEventArgs(value, max));
        }
    }
}
