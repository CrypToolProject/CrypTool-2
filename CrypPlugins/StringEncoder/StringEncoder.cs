/*
   Copyright 2008-2012 Dr. Arno Wacker, University of Kassel

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
using System.IO;
using System.Text;
using System.Windows.Controls;

namespace CrypTool.Plugins.Convertor
{
    [Author("Arno Wacker", "arno.wacker@CrypTool.org", "Universität Kassel", "http://www.uc.uni-kassel.de")]
    [PluginInfo("CrypTool.Plugins.Convertor.Properties.Resources", "PluginCaption", "PluginTooltip", "StringEncoder/DetailedDescription/doc.xml", "StringEncoder/s2t-icon.png")]
    [ComponentCategory(ComponentCategory.ToolsCodes)]
    public class StringEncoder : ICrypComponent
    {
        #region Public interface

        /// <summary>
        /// Returns the settings object, or sets it
        /// </summary>
        public ISettings Settings
        {
            get => settings;
            set => settings = (StringEncoderSettings)value;
        }

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", true)]
        public string OutputString
        {
            get => outputString;
            set
            {
                outputString = value;
                OnPropertyChanged("OutputString");
            }
        }

        [PropertyInfo(Direction.InputData, "InputStreamCaption", "InputStreamTooltip", false)]
        public ICrypToolStream InputStream
        {
            get => inputStream;
            set
            {
                if (inputStream != value)
                {
                    inputStream = value;
                }
            }
        }

        [PropertyInfo(Direction.InputData, "InputBytesCaption", "InputBytesTooltip", false)]
        public byte[] InputBytes
        {
            get => inputBytes;
            set
            {
                if (inputBytes != value)
                {
                    inputBytes = value;
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

        public UserControl Presentation => null;

        public void Initialize()
        {
            settings.SetVisibilityOfEncoding();
        }

        public void Dispose()
        {
            inputStream = null;
            inputBytes = null;
        }


        public void Stop() { }

        public void PostExecution()
        {
            Dispose();
        }

        public void PreExecution()
        {
            outputString = null;
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        #region Private variables
        private StringEncoderSettings settings = new StringEncoderSettings();
        private ICrypToolStream inputStream = null;
        private byte[] inputBytes = null;
        private string outputString;
        #endregion

        #region Private methods

        private string GetStringForEncoding(byte[] buffer, StringEncoderSettings.EncodingTypes encoding)
        {
            if (buffer == null)
            {
                return null;
            }

            switch (encoding)
            {
                case StringEncoderSettings.EncodingTypes.UTF16:
                    return Encoding.Unicode.GetString(buffer);

                case StringEncoderSettings.EncodingTypes.UTF7:
                    return Encoding.UTF7.GetString(buffer);

                case StringEncoderSettings.EncodingTypes.UTF8:
                    return Encoding.UTF8.GetString(buffer);

                case StringEncoderSettings.EncodingTypes.UTF32:
                    return Encoding.UTF32.GetString(buffer);

                case StringEncoderSettings.EncodingTypes.ASCII:
                    return Encoding.ASCII.GetString(buffer);

                case StringEncoderSettings.EncodingTypes.ISO8859_15:
                    return Encoding.GetEncoding("iso-8859-15").GetString(buffer);

                case StringEncoderSettings.EncodingTypes.Windows1252:
                    return Encoding.GetEncoding(1252).GetString(buffer);

                default:
                    return Encoding.Default.GetString(buffer);
            }
        }

        private string GetPresentation(byte[] buffer, StringEncoderSettings.PresentationFormat presentation)
        {
            if (buffer == null)
            {
                return null;
            }

            switch (presentation)
            {
                case StringEncoderSettings.PresentationFormat.Base64:
                    return Convert.ToBase64String(buffer);

                case StringEncoderSettings.PresentationFormat.Binary:
                    return string.Join(" ", Array.ConvertAll(buffer, x => Convert.ToString(x, 2).PadLeft(8, '0')));

                case StringEncoderSettings.PresentationFormat.Hex:
                    return string.Join(" ", Array.ConvertAll(buffer, x => x.ToString("X2")));

                case StringEncoderSettings.PresentationFormat.Octal:
                    return string.Join(" ", Array.ConvertAll(buffer, x => Convert.ToString(x, 8).PadLeft(3, '0')));

                case StringEncoderSettings.PresentationFormat.Decimal:
                    return string.Join(" ", Array.ConvertAll(buffer, x => x.ToString()));

                case StringEncoderSettings.PresentationFormat.Text:
                default:
                    return GetStringForEncoding(buffer, settings.Encoding);
            }
        }

        private void processInput(byte[] buffer)
        {
            ShowProgress(50, 100);
            OutputString = GetPresentation(buffer, settings.PresentationFormatSetting);
            ShowProgress(100, 100);
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
            if (InputStream != null)
            {
                using (CStreamReader reader = InputStream.CreateReader())
                {
                    reader.WaitEof();
                    if (reader.Length > settings.MaxLength)
                    {
                        ShowStatusBarMessage("WARNING - Input stream is too large (" + (reader.Length / 1024).ToString("0.00") + " kB), output will be truncated to " + (settings.MaxLength / 1024).ToString("0.00") + "kB", NotificationLevel.Warning);
                    }

                    byte[] buffer = new byte[Math.Min(reader.Length, settings.MaxLength)];
                    reader.Seek(0, SeekOrigin.Begin);
                    reader.Read(buffer, 0, buffer.Length);

                    processInput(buffer);
                }
            }
            else if (InputBytes != null)
            {
                processInput(InputBytes);
            }
            else
            {
                ShowStatusBarMessage("Inputs are null. Nothing to convert.", NotificationLevel.Warning);
            }
        }

        #endregion
    }
}
