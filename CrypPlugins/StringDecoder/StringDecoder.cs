/*
   Copyright 2008-2012 Arno Wacker, University of Kassel

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

using CrypTool.PluginBase;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace CrypTool.Plugins.Convertor
{
    // Converts a given string into a stream by using different encodings.
    [Author("Arno Wacker", "arno.wacker@CrypTool.org", "Universität Kassel", "http://www.uc.uni-kassel.de")]
    [PluginInfo("CrypTool.Plugins.Convertor.Properties.Resources", "PluginCaption", "PluginTooltip", "StringDecoder/DetailedDescription/doc.xml", "StringDecoder/t2s-icon.png")]
    [ComponentCategory(ComponentCategory.ToolsCodes)]
    public class StringDecoder : ICrypComponent
    {
        #region Public interface

        /// <summary>
        /// Returns the settings object, or sets it
        /// </summary>
        public ISettings Settings
        {
            get => _settings;
            set => _settings = (StringDecoderSettings)value;
        }

        [PropertyInfo(Direction.OutputData, "OutputStreamCaption", "OutputStreamTooltip", false)]
        public ICrypToolStream OutputStream => _outputStream;

        [PropertyInfo(Direction.OutputData, "OutputBytesCaption", "OutputBytesTooltip", false)]
        public byte[] OutputBytes => _outputBytes;

        [PropertyInfo(Direction.InputData, "InputTextCaption", "InputTextTooltip", true)]
        public string InputText
        {
            get => _inputString;
            set
            {
                if (_inputString != value)
                {
                    _inputString = value;
                }
            }
        }

        #endregion

        #region IPlugin Members

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public UserControl Presentation => null;

        public void Initialize()
        {
            _settings.SetVisibilityOfEncoding();
        }

        public void Dispose()
        {
            if (_outputStream != null)
            {
                _outputStream.Flush();
                _outputStream.Close();
                _outputStream.Dispose();
                _outputStream = null;
            }
        }


        public void Stop() { }

        public void PostExecution()
        {
        }

        public void PreExecution()
        {
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region Private variables
        private StringDecoderSettings _settings = new StringDecoderSettings();
        private CStreamWriter _outputStream = null;
        private byte[] _outputBytes = null;
        private string _inputString;
        #endregion

        #region Private methods

        private byte[] GetBytesForEncoding(string str, StringDecoderSettings.EncodingTypes encoding)
        {
            if (str == null)
            {
                str = string.Empty;
            }

            switch (encoding)
            {
                case StringDecoderSettings.EncodingTypes.UTF16:
                    return Encoding.Unicode.GetBytes(str);

                case StringDecoderSettings.EncodingTypes.UTF7:
                    return Encoding.UTF7.GetBytes(str);

                case StringDecoderSettings.EncodingTypes.UTF8:
                    return Encoding.UTF8.GetBytes(str);

                case StringDecoderSettings.EncodingTypes.UTF32:
                    return Encoding.UTF32.GetBytes(str);

                case StringDecoderSettings.EncodingTypes.ASCII:
                    return Encoding.ASCII.GetBytes(str);

                case StringDecoderSettings.EncodingTypes.ISO8859_15:
                    return Encoding.GetEncoding("iso-8859-15").GetBytes(str);

                case StringDecoderSettings.EncodingTypes.Windows1252:
                    return Encoding.GetEncoding(1252).GetBytes(str);

                default:
                    return Encoding.Default.GetBytes(str);
            }
        }

        private void ProcessInput(string str)
        {
            ShowProgress(50, 100);

            switch (_settings.PresentationFormatSetting)
            {
                case StringDecoderSettings.PresentationFormat.Base64:
                    _outputBytes = Convert.FromBase64String(str);
                    break;

                case StringDecoderSettings.PresentationFormat.Octal:
                    _outputBytes = ToByteArray(str, 3, "[^0-7]", 8);
                    break;

                case StringDecoderSettings.PresentationFormat.Decimal:
                    _outputBytes = ToByteArray(str, 3, "[^0-9]", 10);
                    break;

                case StringDecoderSettings.PresentationFormat.Binary:

                    _outputBytes = ToByteArray(str, 8, "[^01]", 2);
                    break;

                case StringDecoderSettings.PresentationFormat.Hex:
                    _outputBytes = ToByteArray(str, 2, "[^a-fA-F0-9]", 16);
                    break;

                case StringDecoderSettings.PresentationFormat.Text:
                default:
                    _outputBytes = GetBytesForEncoding(str, _settings.Encoding);
                    break;
            }

            _outputStream = new CStreamWriter(_outputBytes);

            ShowProgress(100, 100);

            OnPropertyChanged("OutputBytes");
            OnPropertyChanged("OutputStream");
        }

        private byte[] ToByteArray(string input, int blocksize, string removepattern, int b)
        {
            string[] matches;

            if (_settings.UseSeparators)
            {
                matches = input.Split((_settings.Separators + "\n\r").ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                input = new Regex(removepattern).Replace(input, "");
                if (input.Length % blocksize != 0)
                {
                    string prefix = new string('0', (blocksize - input.Length % blocksize) % blocksize);
                    input = prefix + input;
                }
                string pattern = string.Format(".{{{0}}}", blocksize);
                matches = new Regex(pattern).Matches(input).Cast<Match>().Select(m => m.Value).ToArray();
            }

            return matches.Select(m => Convert.ToByte(m, b)).ToArray();
        }

        private void ShowStatusBarMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void ShowProgress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion


        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public void Execute()
        {
            try
            {
                ProcessInput(InputText);
            }
            catch (Exception ex)
            {
                ShowStatusBarMessage("Error converting input: " + ex.Message, NotificationLevel.Error);
            }
        }

        #endregion
    }
}
