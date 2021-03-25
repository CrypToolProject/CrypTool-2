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

using System;
using System.Linq;
using System.Text;
using CrypTool.PluginBase;
using System.ComponentModel;
using CrypTool.PluginBase.IO;
using System.Windows.Controls;
using CrypTool.PluginBase.Miscellaneous;
using System.Text.RegularExpressions;

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
            get { return this.settings; }
            set { this.settings = (StringDecoderSettings)value; }
        }

        [PropertyInfo(Direction.OutputData, "OutputStreamCaption", "OutputStreamTooltip", true)]
        public ICrypToolStream OutputStream
        {
            get 
            {
                return outputStream;
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputBytesCaption", "OutputBytesTooltip", true)]
        public byte[] OutputBytes
        {
            get
            {
                return outputBytes;
            }
        }

        [PropertyInfo(Direction.InputData, "InputTextCaption", "InputTextTooltip", true)]
        public string InputText
        {
            get { return this.inputString;  }
            set 
            {
                if (inputString != value)
                {
                    inputString = value;
                    OnPropertyChanged("InputText");
                }
            }
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Feuern, wenn ein neuer Text im Statusbar angezeigt werden soll.
        /// </summary>
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        /// <summary>
        /// Feuern, wenn sich sich eine Änderung des Fortschrittsbalkens ergibt 
        /// </summary>
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public UserControl Presentation
        {
            get { return null; }
        }

        public void Initialize()
        {
            this.settings.SetVisibilityOfEncoding();
        }

        public void Dispose()
        {
            if (outputStream != null)
            {
                outputStream.Flush();
                outputStream.Close();
                outputStream.Dispose();
                outputStream = null;
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
        private StringDecoderSettings settings = new StringDecoderSettings();
        private CStreamWriter outputStream = null;
        private byte[] outputBytes = null;
        private string inputString;
        #endregion

        #region Private methods

        private byte[] GetBytesForEncoding(string s, StringDecoderSettings.EncodingTypes encoding)
        {
            if (s == null) s = "";
            
            switch (encoding)
            {
                case StringDecoderSettings.EncodingTypes.UTF16:
                    return Encoding.Unicode.GetBytes(s);

                case StringDecoderSettings.EncodingTypes.UTF7:
                    return Encoding.UTF7.GetBytes(s);

                case StringDecoderSettings.EncodingTypes.UTF8:
                    return Encoding.UTF8.GetBytes(s);

                case StringDecoderSettings.EncodingTypes.UTF32:
                    return Encoding.UTF32.GetBytes(s);

                case StringDecoderSettings.EncodingTypes.ASCII:
                    return Encoding.ASCII.GetBytes(s);

                case StringDecoderSettings.EncodingTypes.ISO8859_15:
                    return Encoding.GetEncoding("iso-8859-15").GetBytes(s);

                case StringDecoderSettings.EncodingTypes.Windows1252:
                    return Encoding.GetEncoding(1252).GetBytes(s);

                default:
                    return Encoding.Default.GetBytes(s);
            }
        }

        private void processInput(string value)
        {
            ShowProgress(50, 100);

            //ShowStatusBarMessage("Converting input ...", NotificationLevel.Debug);
                
            switch (settings.PresentationFormatSetting)
            {
                case StringDecoderSettings.PresentationFormat.Base64:
                    outputBytes = Convert.FromBase64String(value);
                    break;

                case StringDecoderSettings.PresentationFormat.Octal:
                    outputBytes = ToByteArray(value, 3, "[^0-7]", 8);
                    break;

                case StringDecoderSettings.PresentationFormat.Decimal:
                    outputBytes = ToByteArray(value, 3, "[^0-9]", 10);
                    break;

                case StringDecoderSettings.PresentationFormat.Binary:

                    outputBytes = ToByteArray(value, 8, "[^01]", 2);
                    break;

                case StringDecoderSettings.PresentationFormat.Hex:
                    outputBytes = ToByteArray(value, 2, "[^a-fA-F0-9]", 16);
                    break;

                case StringDecoderSettings.PresentationFormat.Text:
                default:
                    outputBytes = GetBytesForEncoding(value, settings.Encoding);
                    break;
            }
           
            outputStream = new CStreamWriter(outputBytes);

            //ShowStatusBarMessage("Input converted.", NotificationLevel.Debug);

            ShowProgress(100, 100);

            OnPropertyChanged("OutputBytes");
            OnPropertyChanged("OutputStream");
        }

        private byte[] ToByteArray(string input, int blocksize, string removepattern, int b)
        {
            string[] matches;

            if (settings.UseSeparators)
            {
                matches = input.Split((settings.Separators + "\n\r").ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                input = new Regex(removepattern).Replace(input, "");
                if (input.Length % blocksize != 0) {
                    string prefix = new String('0', (blocksize - input.Length % blocksize) % blocksize);
                    input = prefix + input;
                }
                string pattern = String.Format(".{{{0}}}", blocksize);
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
                processInput(InputText);
            }
            catch (Exception ex)
            {
                ShowStatusBarMessage("Error converting input: " + ex.Message, NotificationLevel.Error);
            }
        }

        #endregion
    }
}
