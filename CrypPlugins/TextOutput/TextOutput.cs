/*
   Copyright 2008-2022 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using CrypTool.PluginBase.Attributes;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using DiffMatchPatch;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static TextOutput.TextOutputSettings;

namespace TextOutput
{
    [Author("Thomas Schmid", "thomas.schmid@CrypTool.org", "Uni Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("TextOutput.Properties.Resources", "PluginCaption", "PluginTooltip", "TextOutput/DetailedDescription/doc.xml", "TextOutput/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    [ComponentVisualAppearance(ComponentVisualAppearance.VisualAppearanceEnum.Opened)]
    public class TextOutput : DependencyObject, ICrypComponent
    {
        #region Fields and properties

        /// <summary>
        /// This dic is used to store error messages while properties are set in PlayMode. The messages
        /// will be sent in the execute method.
        /// The editor flushes plugin color markers before calling the execute method.
        /// So these messages would still appear in LogWindow, but the color marker of the
        /// plugin (red/yellow) would be lost if sending the messages right on property set.
        /// </summary>
        private readonly Dictionary<string, NotificationLevel> dicWarningsAndErros = new Dictionary<string, NotificationLevel>();
        private readonly TextOutputPresentation textOutputPresentation;

        private TextOutputSettings settings;
        public ISettings Settings
        {
            get => settings;
            set => settings = (TextOutputSettings)value;
        }

        private object input;

        [PropertyInfo(Direction.InputData, "InputCaption", "InputTooltip", true)]
        public object Input
        {
            get => input;
            set
            {
                if(value == null)
                {
                    return;
                }
                try
                {
                    Progress(0, 1);
                    input = value;

                    string fillValue = ObjectToString(value);
                    switch (settings.LineBreaks)
                    {
                        case LineBreaksEnum.UNIX:                        
                            fillValue = fillValue.Replace("\r\n", "\n");
                            break;
                        case LineBreaksEnum.Windows:
                            fillValue = fillValue.Replace("\r\n", "\n");
                            fillValue = fillValue.Replace("\n", "\r\n");
                            break;
                        default:
                        case LineBreaksEnum.DontChange:
                            //Do nothing here
                            break;
                    }
                                       
                    ShowInPresentation(fillValue);                

                    CurrentValue = getNewValue(fillValue);

                    Progress(1, 1);

                    //needed by CrypConsole; so it can catch the values which the TextOutput obtains
                    OnPropertyChanged(nameof(Input));
                }
                catch (Exception ex)
                {
                    AddMessage(ex.Message, NotificationLevel.Error);
                }
            }
        }

        private string GetSetLineBreak()
        {
            switch (settings.LineBreaks)
            {
                case LineBreaksEnum.UNIX:
                    return "\n";
                case LineBreaksEnum.Windows:
                    return "\r\n";
                default:
                case LineBreaksEnum.DontChange:
                    return Environment.NewLine;
            }
        }

        private string GenerateLinebreaks()
        {
            StringBuilder builder = new StringBuilder();
            string lineBreak = GetSetLineBreak();
            for(int i = 0; i < settings.AppendBreaks; i++)
            {
                builder.Append(lineBreak);
            }
            return builder.ToString();
        }

        private string _currentValue = string.Empty;
        public string CurrentValue
        {
            get => _currentValue;
            private set
            {
                _currentValue = value;
            }
        }

        #endregion

        #region Event handling

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PropertyChangedEventHandler PropertyChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void settings_OnGuiLogNotificationOccured(IPlugin sender, GuiLogEventArgs args)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(args.Message, this, args.NotificationLevel));
        }

        private void Progress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion events

        #region Constructor and implementation

        public TextOutput()
        {
            settings = new TextOutputSettings(this);
            settings.PropertyChanged += settings_OnPropertyChanged;

            textOutputPresentation = new TextOutputPresentation(CrypTool.PluginBase.Properties.Settings.Default.FontFamily, CrypTool.PluginBase.Properties.Settings.Default.FontSize)
            {
                _textOutput = this
            };
            textOutputPresentation.UserKeyDown += textOutputPresentation_UserKeyDown;
            setStatusBar();
        }

        private void textOutputPresentation_UserKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (settings.ManualFontSettings == false)
            {
                return;
            }
            if ((e.Key == Key.Add) && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                if (settings.FontSize < 72)
                {
                    settings.FontSize++;
                }
            }
            else if ((e.Key == Key.Subtract) && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                if (settings.FontSize > 8)
                {
                    settings.FontSize--;
                }
            }
        }

        private void settings_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ShowChars" || e.PropertyName == "ShowLines" || e.PropertyName == "ShowDigits")
            {
                setStatusBar();
            }
            if (e.PropertyName == "Font")
            {
                textOutputPresentation.MyFontFamily = new System.Windows.Media.FontFamily(settings.Fonts[settings.Font]);
            }
            if (e.PropertyName == "FontSize")
            {
                textOutputPresentation.MyFontSize = settings.FontSize;
            }
        }

        private Thread statusBarThread = null;
        private void setStatusBar()
        {
            try
            {
                if (statusBarThread != null && statusBarThread.IsAlive)
                {
                    statusBarThread.Abort();
                }
                statusBarThread = new Thread(() => setStatusBar_invoke())
                {
                    IsBackground = true,
                    CurrentCulture = Thread.CurrentThread.CurrentCulture,
                    CurrentUICulture = Thread.CurrentThread.CurrentUICulture
                };
                statusBarThread.Start();
            }
            catch (Exception)
            {
            }

        }

        private void setStatusBar_invoke()
        {
            textOutputPresentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    setStatusBar_orig();
                }
                catch (Exception)
                {
                }
            }, null);
        }

        private byte[] ConvertStreamToByteArray(ICrypToolStream stream)
        {
            CStreamReader reader = stream.CreateReader();
            reader.WaitEof(); // does not support chunked streaming

            if (reader.Length > settings.MaxLength)
            {
                AddMessage("WARNING - Stream is too large (" + (reader.Length / 1024).ToString("0.00") + " kB), output will be truncated to " + (settings.MaxLength / 1024).ToString("0.00") + "kB", NotificationLevel.Warning);
            }

            byte[] byteArray = new byte[Math.Min(settings.MaxLength, reader.Length)];
            reader.Seek(0, SeekOrigin.Begin);
            reader.ReadFully(byteArray, 0, byteArray.Length);
            reader.Close();

            return byteArray;
        }

        private byte[] GetByteArray(byte[] byteArray)
        {
            if (byteArray.Length <= settings.MaxLength)
            {
                return byteArray;
            }

            AddMessage("WARNING - Byte array is too large (" + (byteArray.Length / 1024).ToString("0.00") + " kB), output will be truncated to " + (settings.MaxLength / 1024).ToString("0.00") + "kB", NotificationLevel.Warning);

            byte[] truncatedByteArray = new byte[settings.MaxLength];
            Buffer.BlockCopy(byteArray, 0, truncatedByteArray, 0, settings.MaxLength);

            return truncatedByteArray;
        }

        internal string ObjectToString(object value)
        {
            string result = string.Empty;

            if (value == null)
            {
                result = string.Empty;
            }
            else if (value is string)
            {
                result = (string)value;
            }
            else if (value is byte[])
            {
                byte[] byteArray = GetByteArray((byte[])value);
                result = BitConverter.ToString(byteArray).Replace("-", " ");
            }
            else if (value is ICrypToolStream)
            {
                byte[] byteArray = ConvertStreamToByteArray((ICrypToolStream)value);
                result = BitConverter.ToString(byteArray).Replace("-", " ");
            }
            else if (value is System.Collections.IEnumerable)
            {
                System.Collections.IEnumerable enumerable = value as System.Collections.IEnumerable;

                List<string> s = new List<string>();
                foreach (object obj in enumerable)
                {
                    s.Add((obj == null ? "null" : obj.ToString()));
                }

                result = string.Join(GetSetLineBreak(), s);
            }
            else if (value is BigInteger)
            {
                result = value.ToString();   // ~ 2x faster than ToBaseString
            }
            else
            {
                result = value.ToString();
            }

            if (result == null)
            {
                result = string.Empty;
            }

            return result;
        }

        internal string getNewValue(string fillValue)
        {
            string newValue = string.Empty;

            if (settings.Append)
            {
                newValue = (CurrentValue == null ? string.Empty : CurrentValue);
                if (!string.IsNullOrEmpty(newValue))
                {
                    newValue += GenerateLinebreaks();
                }
            }

            newValue += fillValue;

            if (newValue.Length > settings.MaxLength)
            {
                AddMessage("WARNING - String is too large (" + (newValue.Length / 1024).ToString("0.00") + " kB), output will be truncated to " + (settings.MaxLength / 1024).ToString("0.00") + "kB", NotificationLevel.Warning);
                newValue = newValue.Substring(0, settings.MaxLength);
            }

            return newValue;
        }

        internal void ShowInPresentation(string fillValue, bool maximized = false)
        {
            if (!Presentation.IsVisible)
            {
                return;
            }

            //Check if we are in the UI thread
            if (Thread.CurrentThread == Application.Current.Dispatcher.Thread)
            {
                //we are in the UI thread, thus we can directly call
                UpdateTextControls(fillValue, maximized);
            }
            else
            {
                //we are not in the UI thread, thus we have to get into
                Presentation.Dispatcher.Invoke(DispatcherPriority.Background, (SendOrPostCallback)delegate
                {
                    UpdateTextControls(fillValue, maximized);
                }, fillValue);
                try
                {
                    //we give the others component some time to work with the UI thread
                    Thread.Sleep(20);
                }
                catch (Exception)
                {
                    //wtf?
                }
            }
        }

        private void UpdateTextControls(string fillValue, bool maximized = false)
        {
            if (!Presentation.IsVisible)
            {
                return;
            }

            string oldtext = (CurrentValue == null ? string.Empty : CurrentValue);
            string newtext = string.Empty;

            if (settings.Append)
            {
                newtext = oldtext;
                // append line breaks only if not first line
                if (!string.IsNullOrEmpty(oldtext))
                {
                    newtext += GenerateLinebreaks();
                }
            }

            newtext += fillValue;

            if (settings.ShowChanges == 0)
            {
                textOutputPresentation.richTextBox.Visibility = Visibility.Hidden;
                textOutputPresentation.textBox.Visibility = Visibility.Visible;

                textOutputPresentation.textBox.Text = newtext;
                if (settings.Append)
                {
                    textOutputPresentation.textBox.ScrollToEnd();
                }
            }
            else if (settings.ShowChanges == 1 || settings.ShowChanges == 2)
            {
                textOutputPresentation.richTextBox.Visibility = Visibility.Visible;
                textOutputPresentation.textBox.Visibility = Visibility.Hidden;

                if (settings.Append)
                {
                    // append line breaks only if not first line
                    if (!string.IsNullOrEmpty(oldtext))
                    {
                        textOutputPresentation.richTextBox.AppendText(GenerateLinebreaks());
                    }

                    if (maximized)
                    {
                        textOutputPresentation.richTextBox.Document.Blocks.Clear();
                    }
                    textOutputPresentation.richTextBox.AppendText(fillValue);
                    textOutputPresentation.richTextBox.ScrollToEnd();
                }
                else
                {
                    textOutputPresentation.richTextBox.Document.Blocks.Clear();
                    textOutputPresentation.richTextBox.AppendText(fillValue);
                }

                diff_match_patch diff = new diff_match_patch();
                List<Diff> diffs = diff.diff_main(oldtext, newtext, true);
                diff.diff_cleanupSemanticLossless(diffs);

                if (textOutputPresentation.richTextBox.Document == null)
                {
                    textOutputPresentation.richTextBox.Document = new FlowDocument();
                }
                else
                {
                    textOutputPresentation.richTextBox.Document.Blocks.Clear();
                }

                Paragraph para = new Paragraph();
                foreach (Diff d in diffs)
                {
                    switch (d.operation)
                    {
                        case Operation.EQUAL:
                            para.Inlines.Add(new Run(d.text));
                            break;
                        case Operation.INSERT:
                            if (settings.ShowChanges == 1)
                            {
                                Run run = new Run(d.text)
                                {
                                    Background = new SolidColorBrush(Colors.LightBlue)
                                };
                                para.Inlines.Add(run);
                            }
                            else if (settings.ShowChanges == 2)
                            {
                                Run run = new Run(d.text)
                                {
                                    Background = new SolidColorBrush(Colors.LightGreen)
                                };
                                para.Inlines.Add(run);
                            }
                            break;
                        case Operation.DELETE:
                            if (settings.ShowChanges == 2 && d.text.Trim().Length > 0)
                            {
                                Run run = new Run(d.text)
                                {
                                    Background = new SolidColorBrush(Color.FromRgb(0xF3, 0x6D, 0x74))
                                };
                                para.Inlines.Add(run);
                            }
                            break;
                    }
                }
                textOutputPresentation.richTextBox.Document.Blocks.Add(para);
            }
            else if (settings.ShowChanges == 3)
            {
                textOutputPresentation.richTextBox.Visibility = Visibility.Visible;
                textOutputPresentation.textBox.Visibility = Visibility.Hidden;

                if (settings.Append)
                {
                    // append line breaks only if not first line
                    if (!string.IsNullOrEmpty(oldtext))
                    {
                        textOutputPresentation.richTextBox.AppendText(GenerateLinebreaks());
                    }

                    if (maximized)
                    {
                        textOutputPresentation.richTextBox.Document.Blocks.Clear();
                    }
                    textOutputPresentation.richTextBox.AppendText(fillValue);
                    textOutputPresentation.richTextBox.ScrollToEnd();
                }
                else
                {
                    textOutputPresentation.richTextBox.Document.Blocks.Clear();
                    textOutputPresentation.richTextBox.AppendText(fillValue);
                }

                if (textOutputPresentation.richTextBox.Document == null)
                {
                    textOutputPresentation.richTextBox.Document = new FlowDocument();
                }
                else
                {
                    textOutputPresentation.richTextBox.Document.Blocks.Clear();
                }
                Paragraph para = new Paragraph();
                int position = 0;
                while (position < newtext.Length)
                {
                    Run run = new Run(string.Empty + newtext[position]);
                    if (oldtext.Length == 0 || position > oldtext.Length || (position < oldtext.Length && oldtext[position] != newtext[position]))
                    {
                        run.Background = new SolidColorBrush(Colors.LightBlue);
                    }
                    para.Inlines.Add(run);
                    position++;
                }
                textOutputPresentation.richTextBox.Document.Blocks.Add(para);
            }

            setStatusBar();
        }

        private void clearStatusBar()
        {
            textOutputPresentation.labelBytes.Content = string.Empty;
        }

        private void setStatusBar_orig()
        {
            // create status line string
            textOutputPresentation.labelBytes.Content = "...";

            string currentText;

            if (settings.ShowChanges == 0)
            {
                currentText = textOutputPresentation.textBox.Text;
            }
            else 
            {
                currentText = new TextRange(textOutputPresentation.richTextBox.Document.ContentStart, textOutputPresentation.richTextBox.Document.ContentEnd).Text;
            }
            string label = string.Empty;

            if (settings.ShowDigits)
            {
                if (Input is string)
                {
                    string value = Input as string;
                    if (Regex.IsMatch(value, @"^-?\d+$"))
                    {
                        try
                        {
                            input = BigIntegerHelper.Parse(value, 10);
                        }
                        catch (Exception)
                        {
                            //wtf ?
                        }
                    }
                }

                if (Input is short)
                {
                    input = (BigInteger)(int)(short)Input;
                }
                else if (Input is int)
                {
                    input = (BigInteger)(int)Input;
                }
                else if (Input is byte)
                {
                    input = (BigInteger)(byte)Input;
                }

                if (Input is BigInteger)
                {
                    int digits = 0;
                    int bits;
                    try
                    {
                        BigInteger number = (BigInteger)input;
                        double log2 = BigInteger.Log(BigInteger.Abs(number), 2.0);
                        if (log2 < 10000000)
                        {
                            bits = number.BitCount();
                        }
                        else
                        {
                            bits = (int)System.Math.Ceiling(log2);
                        }
                        digits = currentText.Length;
                        if (number < 0)
                        {
                            digits--;
                        }
                    }
                    catch (Exception)
                    {
                        digits = 0;
                        bits = 0;
                    }
                    string digitText = (digits == 1) ? Properties.Resources.Digit : Properties.Resources.Digits;
                    string bitText = (bits == 1) ? Properties.Resources.Bit : Properties.Resources.Bits;
                    label = string.Format(" {0:#,0} {1}, {2:#,0} {3}", digits, digitText, bits, bitText);
                    textOutputPresentation.labelBytes.Content = label;
                    return;
                }
            }

            if (settings.ShowChars)
            {
                int chars = 0;
                if (currentText != null)
                {
                    currentText = Regex.Replace(currentText, @"\r(?!\n)", "\r\n");  // replace single \r with \r\n
                    currentText = Regex.Replace(currentText, @"\r\n$", string.Empty);         // ignore trailing \r\n
                    chars = currentText.Length;
                }
                string entity = (chars == 1) ? Properties.Resources.Char : Properties.Resources.Chars;
                label += string.Format(" {0:#,0} " + entity, chars);
            }

            if (settings.ShowLines)
            {
                int lines = 0;
                if (currentText != null && currentText.Length > 0)
                {
                    lines = new Regex(System.Environment.NewLine, RegexOptions.Multiline).Matches(currentText).Count;
                    if (currentText[currentText.Length - 1] != '\n')
                    {
                        lines++;
                    }
                }
                string entity = (lines == 1) ? Properties.Resources.Line : Properties.Resources.Lines;
                if (label != string.Empty)
                {
                    label += ", ";
                }

                label += string.Format(" {0:#,0} " + entity, lines);
            }

            textOutputPresentation.labelBytes.Content = label;
        }

        private void AddMessage(string message, NotificationLevel level)
        {
            if (!dicWarningsAndErros.ContainsKey(message))
            {
                dicWarningsAndErros.Add(message, level);
            }
        }

        #endregion

        #region IPlugin Members

        public UserControl Presentation => textOutputPresentation;

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        public void Stop()
        {
            if (statusBarThread != null && statusBarThread.IsAlive)
            {
                statusBarThread.Abort();
            }
        }

        public void PreExecution()
        {
            textOutputPresentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                textOutputPresentation.richTextBox.Document = new FlowDocument();
                textOutputPresentation.textBox.Text = string.Empty;
                _currentValue = string.Empty;
                Input = null;
                clearStatusBar();
            }, null);
        }

        public void PostExecution()
        {
        }

        public void Execute()
        {
            Progress(100, 100);
            foreach (KeyValuePair<string, NotificationLevel> kvp in dicWarningsAndErros)
            {
                GuiLogMessage(kvp.Key, kvp.Value);
            }
            dicWarningsAndErros.Clear();
        }

        #endregion
    }
}