/*
   Copyright 2017 Nils Kopal, Applied Information Security, Uni Kassel
   https://www.uni-kassel.de/eecs/fachgebiete/ais/mitarbeiter/nils-kopal-m-sc.html


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
using System.Text;
using CrypTool.PluginBase;
using System.ComponentModel;
using CrypTool.PluginBase.Miscellaneous;
using System.Collections.Generic;
using System.IO;
using CrypTool.PluginBase.IO;
using System.Linq;

namespace CrypTool.CylinderCipher
{
    [Author("Nils Kopal", "nils.kopal@uni-kassel.de", "Applied Information Security - University of Kassel", "http://www.ais.uni-kassel.de")]
    [PluginInfo("CrypTool.CylinderCipher.Properties.Resources", "PluginCaption", "PluginTooltip", "CylinderCipher/DetailedDescription/doc.xml", "CylinderCipher/Images/icon.png")]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class CylinderCipher : ICrypComponent
    {
        private CylinderCipherSettings _settings = new CylinderCipherSettings();
        public int[][] _cylinders;          // the current loaded cylinders
        public int[][] _cylindersIndexOf;   // helper data structure to speed up encryption/decryption
        private const string _alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private List<string> _ignoredCharacters = new List<string>();
        private int[] _input = null;
        private int[] _key = null;
        private int[] _offsets = null;

        private string _textInput;
        private string _keyInput;

        private bool _newText;
        private bool _newKey;

        [PropertyInfo(Direction.InputData, "TextInputCaption", "TextInputTooltip")]
        public string TextInput
        {
            get { return this._textInput; }
            set
            {
                if (!String.IsNullOrEmpty(value) && value != this._textInput)
                {
                    this._textInput = value;
                    this._newText = true;
                    OnPropertyChanged("TextInput");
                }
            }
        }

        [PropertyInfo(Direction.InputData, "KeyCaption", "KeyTooltip")]
        public string Key
        {
            get { return this._keyInput; }
            set
            {
                if (!String.IsNullOrEmpty(value) && value != this._keyInput)
                {
                    this._keyInput = value;
                    this._newKey = true;
                    OnPropertyChanged("Key");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "TextOutputCaption", "TextOutputTooltip")]
        public string TextOutput
        {
            get;
            set;
        }

        public void PreExecution()
        {
            
        }

        public void PostExecution()
        {
            _keyInput = String.Empty;
            _textInput = String.Empty;
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public ISettings Settings
        {
            get { return _settings; }
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public void Execute()
        {
            if (!_newKey || !_newText)
                return;

            //Consume values
            _newKey = false;
            _newText = false;

            //Load cylinders and prepare texts
            if (_settings.DeviceType == 0)
            {
                LoadCylinders(DirectoryHelper.DirectoryCrypPlugins + Path.DirectorySeparatorChar + "Cylinders_M94.txt");
            }
            else if (_settings.DeviceType == 1)
            {                
                LoadCylinders(DirectoryHelper.DirectoryCrypPlugins + Path.DirectorySeparatorChar + "Cylinders_Bazeries.txt");
                _textInput = _textInput.Replace("W","VV"); // Bazeries has no W - instead he uses VV
            }

            //Invalid character handling
            if (_settings.InvalidCharacterHandling == 0)
                _textInput = RemoveInvalidChars(_textInput, _alphabet);
            else
                _ignoredCharacters = new List<string>();

            //Map input to integer array
            _input = MapTextIntoNumberSpace(_textInput.ToUpper(), _alphabet, _settings.InvalidCharacterHandling);           

            //Prepare IndexOf datastructure of cylinders
            PrepareCylindersIndexOf(_cylinders);

            //Convert given string key to integer array and offset array
            if (!ConvertStringKeyToIntegerArrays())
            {
                return;
            }

            int[] result = null;
            //Encrypt or Decrypt
            if (_settings.Action == 0)
            {
                result = EncryptCylinderCipher(_input, _key, _offsets);
            }
            else
            {
                result = DecryptCylinderCipher(_input, _key, _offsets);
            }
          
            TextOutput = MapNumbersIntoTextSpace(result, _alphabet, _settings.InvalidCharacterHandling);            
            if (_settings.CaseSensitivity)
            {
                String tmp = MapNumbersIntoTextSpace(result, _alphabet, _settings.InvalidCharacterHandling);
                TextOutput = new string(tmp.Select((c, k) => Char.IsLower(_textInput[k]) ? Char.ToLower(c) : c).ToArray());
            }
            OnPropertyChanged("TextOutput");

            //System.Console.WriteLine("### Execute end of: CC");

            //set component to 100% progress
            OnProgressChanged(1, 1);
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Dispose()
        {
            
        }

        private int[] EncryptCylinderCipher(int[] cleartext, int[] key, int[] offsets)
        {
            var length = cleartext.Length;
            var ciphertext = new int[length];
            var offsetid = -1;
            var j = 0;
            for (int i = 0; i < length; i++)
            {
                if (cleartext[i] != -1)
                {                    
                    //Change the offset every "key.Length" times
                    if (j % key.Length == 0)
                    {
                        offsetid++;
                        if (offsetid == offsets.Length)
                        {
                            offsetid = 0;
                        }
                    }
                    var index = _cylindersIndexOf[key[Mod(j, _key.Length)]][cleartext[i]];
                    ciphertext[i] = _cylinders[key[Mod(j, _key.Length)]][Mod(index + offsets[offsetid], _cylinders[0].Length)];
                    j++;
                }
                else
                {
                    ciphertext[i] = -1;
                }
            }
            return ciphertext;
        }

        private int[] DecryptCylinderCipher(int[] ciphertext, int[] key, int[] offsets)
        {
            var length = ciphertext.Length;
            var cleartext = new int[length];
            var offsetid = -1;
            var j = 0;
            for (int i = 0; i < length; i++)
            {
                if (ciphertext[i] != -1)
                {
                    //Change the offset every "key.Length" times
                    if (j % key.Length == 0)
                    {
                        offsetid++;
                        if (offsetid == offsets.Length)
                        {
                            offsetid = 0;
                        }
                    }
                    var index = _cylindersIndexOf[key[Mod(j, _key.Length)]][ciphertext[i]];
                    cleartext[i] = _cylinders[key[Mod(j, _key.Length)]][Mod(index - offsets[offsetid], _cylinders[0].Length)];
                    j++;
                }
                else
                {
                    cleartext[i] = -1;
                }
            }
            return cleartext;
        }

        private bool ConvertStringKeyToIntegerArrays()
        {
            try
            {
                char sep = "/,."[_settings.SeparatorOffChar];

                if (_keyInput.IndexOf(sep) < 0)
                    throw new Exception("The key contains no offset separator '" + sep + "'.");

                string[] splitted;
                splitted = _keyInput.Split(sep);                
                sep = ",./"[_settings.SeparatorStripChar];

                //Create key list
                List<int> list = new List<int>();
                foreach (var cylinder in splitted[0].Split(sep))
                {
                    int n = Convert.ToInt32(cylinder);
                    if (n >= _cylinders.Length)
                    {
                        GuiLogMessage("Selected cylinder number " + n + " is larger or equal to the number of available cylinders " + _cylinders.Length + ". Using cylinder " + (n % _cylinders.Length) + " instead.", NotificationLevel.Warning);
                        n = (n % _cylinders.Length);
                    }
                    list.Add(n);
                }
                _key = list.ToArray();

                //Create offset list
                list = new List<int>();
                foreach (var offset in splitted[1].Split(sep))
                {
                    int n = Convert.ToInt32(offset);
                    if (n > _cylinders[0].Length)
                    {
                        GuiLogMessage("Selected offset " + n + " is larger than the length of a cylinder " + _cylinders[0].Length + ". Using offset " + (n % _cylinders[0].Length) + " instead.", NotificationLevel.Warning);
                        n = (n % _cylinders[0].Length);
                    }
                    list.Add(n);
                }
                _offsets = list.ToArray();
            }
            catch (Exception ex)
            {
                GuiLogMessage("Error while parsing key: " + ex.Message, NotificationLevel.Error);
                return false;
            }

            return true;
        }


        private void PrepareCylindersIndexOf(int[][] cylinders)
        {
            _cylindersIndexOf = new int[cylinders.Length][];
            for (var cylinder = 0; cylinder < cylinders.Length; cylinder++)
            {
                _cylindersIndexOf[cylinder] = new int[26];
                for (var i = 0; i < 26; i++)
                {
                    _cylindersIndexOf[cylinder][i] = IndexOf(cylinders[cylinder], i);
                }
            }
        }

        private int IndexOf(int[] array, int value)
        {
            for (var i = 0; i < array.Length; i++)
            {
                if (array[i] == value)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Calculates the mathemtical modulo operation: a mod n
        /// </summary>
        /// <param name="a"></param>
        /// <param name="n"></param>
        /// <returns>a mod n</returns>
        private int Mod(int a, int n)
        {
            var result = a % n;
            if (a < 0)
            {
                result += n;
            }
            return result;
        }

        private string RemoveInvalidChars(string text, string alphabet)
        {
            var builder = new StringBuilder();
            foreach (char c in text)
            {
                if (alphabet.Contains(c.ToString()) | alphabet.Contains(c.ToString().ToUpper()))
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }


        private int[] MapTextIntoNumberSpace(string text, string alphabet, int invalidCharacterHandling)
        {
            var numbers = new int[text.Length];
            var position = 0;

            if (invalidCharacterHandling == 0)
            {
                foreach (char c in text)
                {
                    numbers[position] = alphabet.IndexOf(c);
                    position++;
                }
            }
            else
            {
                foreach (char c in text)
                {
                    if (alphabet.Contains(c.ToString()))
                    {
                        numbers[position] = alphabet.IndexOf(c);
                    }
                    else
                    {
                        numbers[position] = -1;
                        if (invalidCharacterHandling == 1)
                        {
                            _ignoredCharacters.Add(c.ToString());
                        }
                    }
                    position++;
                }
            }

            return numbers;
        }

        private string MapNumbersIntoTextSpace(int[] numbers, string alphabet, int invalidCharacterHandling)
        {
            var builder = new StringBuilder();
            int counter = 0;

            if (invalidCharacterHandling == 0)
            {
                foreach (char c in numbers)
                {
                    builder.Append(alphabet[c]);
                }
            }
            else
            {
                foreach (char c in numbers)
                {
                    if (c == 65535)
                    {
                        if (invalidCharacterHandling == 1)
                        {
                            builder.Append(_ignoredCharacters[counter]);
                            counter++;
                        }
                        else
                        {
                            builder.Append('?');
                        }
                    }
                    else
                    {
                        builder.Append(alphabet[c]);
                    }
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Loads the cylinders from the given file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="alphabet"></param>
        private void LoadCylinders(string path)
        {
            var cylinders = new List<string>();
            using (var instream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(instream))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        cylinders.Add(line);
                    }
                }
            }
            _cylinders = new int[cylinders.Count][];
            var i = 0;
            foreach (var stripe in cylinders)
            {
                _cylinders[i] = MapTextIntoNumberSpace(stripe, _alphabet, _settings.InvalidCharacterHandling);
                i++;
            }
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }
        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }
    }
}
