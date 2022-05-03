/*
   Copyright 2021 Nils Kopal, CrypTool project

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
using System.Security.Cryptography;
using System.Windows.Controls;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

namespace CrypTool.PBKDF
{
    [Author("Nils Kopal", "kopal@CrypTool.org", "CrypTool project", "http://www.cryptool.org")]
    [PluginInfo("CrypTool.PBKDF.Properties.Resources", "PluginCaption", "PluginTooltip", "PBKDF/DetailedDescription/doc.xml", "PBKDF/Images/icon.png")]
    [ComponentCategory(ComponentCategory.HashFunctions)]
    public class PBKDF : ICrypComponent
    {
        private readonly PBKDFSettings _settings = new PBKDFSettings();
        private byte[] _password;
        private byte[] _salt;
        private byte[] _key;
        private HashAlgorithm _hashAlgorithm;

        public ISettings Settings => _settings;

        [PropertyInfo(Direction.InputData, "PasswordCaption", "PasswordTooltip", true)]
        public byte[] Password
        {
            get => _password;
            set => _password = value;
        }

        [PropertyInfo(Direction.InputData, "SaltCaption", "SaltTooltip", false)]
        public byte[] Salt
        {
            get => _salt;
            set => _salt = value;
        }

        [PropertyInfo(Direction.OutputData, "KeyCaption", "KeyTooltip", false)]
        public byte[] Key
        {
            get => _key;
            set => _key = value;
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void Progress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public UserControl Presentation => null;

        public void Stop()
        {
        }

        public void PostExecution()
        {
        }

        public void PreExecution()
        {
        }        

        public void Execute()
        {
            Progress(0, 1.0);
            
            if (_password == null)
            {
                _password = new byte[0];
            }
            if (_salt == null)
            {
                _salt = new byte[0];
            }

            //1. Create hash algo
            CreateHashAlgorithmInstanec();

            //2. Perform kdf
            switch (_settings.KeyDerivationFunction)
            {
                case KeyDerivationFunctions.PBKDF1:
                    PerformPBKDF1();
                    if (_settings.OutputLength > Key.Length)
                    {
                        GuiLogMessage(string.Format("Defined output length (={0} bytes) is longer than the hash function length. Output length is reduced to {0} bytes", _settings.OutputLength, Key.Length), NotificationLevel.Warning);
                    }
                    if (Key.Length > _settings.OutputLength)
                    {
                        Key = Key.SubArray(_settings.OutputLength);
                    }
                    break;
                case KeyDerivationFunctions.PBKDF2:
                    PerformPBKDF2();
                    break;
            }

            //3. Notify CT2 that we createad a key
            OnPropertyChanged("Key");

            Progress(1.0, 1.0);
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void GuiLogMessage(string msg, NotificationLevel logLevel)
        {
            OnGuiLogNotificationOccured?.Invoke(this, new GuiLogEventArgs(msg, this, logLevel));
        }

        /// <summary>
        /// Creates the hash algorithm instance used by the key derivation function based on the settings values
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void CreateHashAlgorithmInstanec()
        {
            if (_settings.KeyDerivationFunction == KeyDerivationFunctions.PBKDF1)
            {
                switch (_settings.HashAlgorithm)
                {
                    case HashAlgorithms.SHA1:
                        _hashAlgorithm = new SHA1Managed();
                        return;
                    case HashAlgorithms.SHA256:
                        _hashAlgorithm = new SHA256Managed();
                        return;
                    case HashAlgorithms.SHA384:
                        _hashAlgorithm = new SHA384Managed();
                        return;
                    case HashAlgorithms.SHA512:
                        _hashAlgorithm = new SHA512Managed();
                        return;
                }
            }
            else if(_settings.KeyDerivationFunction == KeyDerivationFunctions.PBKDF2)
            {
                switch (_settings.HashAlgorithm)
                {
                    case HashAlgorithms.SHA1:
                        _hashAlgorithm = new HMACSHA1();
                        return;
                    case HashAlgorithms.SHA256:
                        _hashAlgorithm = new HMACSHA256();
                        return;
                    case HashAlgorithms.SHA384:
                        _hashAlgorithm = new HMACSHA384();
                        return;
                    case HashAlgorithms.SHA512:
                        _hashAlgorithm = new HMACSHA512();
                        return;
                }
            }
            throw new NotImplementedException(string.Format("Selected hash function {0} is not implemented", _settings.HashAlgorithm.ToString()));
        }

        /// <summary>
        /// Implementation of PBKDF1 as described in the RFC2898: https://tools.ietf.org/html/rfc2898
        /// </summary>
        private void PerformPBKDF1()
        {
            byte[] key = _password;
            if (_salt != null)
            {
                key = key.Concat(_salt);
            }
            for (int iteration = 0; iteration < _settings.Iterations; iteration++)
            {
                key = _hashAlgorithm.ComputeHash(key);
            }
            Key = key;
        }

        /// <summary>
        /// Implementation of PBKDF2 as described in the RFC2898: https://tools.ietf.org/html/rfc2898
        /// </summary>
        private void PerformPBKDF2()
        {
            //int l = (int)Math.Ceiling(_settings.OutputLength / (_hashAlgorithm.HashSize / 8.0));
            int blocks = _settings.OutputLength / (_hashAlgorithm.HashSize / 8) + (_settings.OutputLength % (_hashAlgorithm.HashSize / 8) != 0 ? 1 : 0);
            int sizeOfLastBlock = _settings.OutputLength - (blocks - 1) * _hashAlgorithm.HashSize / 8;

            byte[] key = new byte[0];
            for (uint i = 1; i <= blocks; i++)
            {
                byte[] T = new byte[_hashAlgorithm.HashSize / 8];
                byte[] S = _salt.Concat(BitConverter.GetBytes(i).Reverse());
                ((HMAC)_hashAlgorithm).Key = Password;
                for (int iteration = 0; iteration < _settings.Iterations; iteration++)
                {                    
                    S = _hashAlgorithm.ComputeHash(S);
                    T = T.XOR(S);
                }
                if (i != blocks)
                {
                    key = key.Concat(T);
                }
                else
                {
                    key = key.Concat(T.SubArray(sizeOfLastBlock));
                }
            }
            Key = key;
        }
    }

    public static class ByteArrayExtensions
    {
        /// <summary>
        /// Concats the byte array and the given one
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        public static byte[] Concat(this byte[] array1, byte[] array2)
        {
            byte[] array3 = new byte[array1.Length + array2.Length];
            Array.Copy(array1, 0, array3, 0, array1.Length);
            Array.Copy(array2, 0, array3, array1.Length, array2.Length);
            return array3;
        }

        /// <summary>
        /// Returns a sub array of the byte array. If sub array is longer than the original one it returns the original one
        /// </summary>
        /// <param name="array"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static byte[] SubArray(this byte[] array, int length)
        {
            if (length > array.Length)
            {
                return array;
            }
            byte[] array2 = new byte[length];
            Array.Copy(array, 0, array2, 0, length);
            return array2;
        }

        /// <summary>
        /// Computes the XOR of array1 and array2
        /// </summary>
        /// <param name="array"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static byte[] XOR(this byte[] array1, byte[] array2)
        {
            byte[] array3 = new byte[array1.Length];
            for(int i = 0; i < array1.Length; i++)
            {
                array3[i] = (byte)(array1[i] ^ array2[i]);
            }
            return array3;
        }

        /// <summary>
        /// Reverses the given array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static byte[] Reverse(this byte[] array)
        {
            byte[] array2 = new byte[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array2[i] = array[array.Length - i - 1];
            }
            return array2;
        }
    }

}
