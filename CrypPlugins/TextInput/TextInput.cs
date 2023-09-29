/*
   Copyright 2008 Thomas Schmid, University of Siegen

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
using CrypTool.PluginBase.Miscellaneous;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace CrypTool.TextInput
{
    [Author("Thomas Schmid", "thomas.schmid@CrypTool.org", "Uni Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("CrypTool.TextInput.Properties.Resources", "PluginCaption", "PluginTooltip", "TextInput/DetailedDescription/doc.xml", "TextInput/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    [ComponentVisualAppearance(ComponentVisualAppearance.VisualAppearanceEnum.Opened)]
    public class TextInput : DependencyObject, ICrypComponent
    {
        private readonly TextInputPresentation textInputPresentation;

        public TextInput()
        {
            settings = new TextInputSettings();
            settings.OnLogMessage += settings_OnLogMessage;
            settings.PropertyChanged += settings_OnPropertyChanged;
            textInputPresentation = new TextInputPresentation(PluginBase.Properties.Settings.Default.FontFamily, PluginBase.Properties.Settings.Default.FontSize);
            Presentation = textInputPresentation;
            textInputPresentation.UserKeyDown += textInputPresentation_UserKeyDown;
        }

        private void textInputPresentation_UserKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
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
            if (e.PropertyName == "Text")
            {
                textInputPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    textInputPresentation.textBoxInputText.Text = settings.Text;
                }, null);
            }
            else if (e.PropertyName == "Font")
            {
                textInputPresentation.MyFontFamily = new System.Windows.Media.FontFamily(settings.Fonts[settings.Font]);
            }
            else if (e.PropertyName == "FontSize")
            {
                textInputPresentation.MyFontSize = settings.FontSize;
            }
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                SetStatusBar();
            }, null);

        }

        private void textBoxInputText_TextChanged(object sender, TextChangedEventArgs e)
        {
            NotifyUpdate();

            // No dispatcher necessary, handler is being called from GUI component
            settings.Text = textInputPresentation.textBoxInputText.Text;
            SetStatusBar();
        }

        private void SetStatusBar()
        {
            // create status line string

            string s = textInputPresentation.textBoxInputText.Text;
            string label = "";

            if (settings.ShowChars)
            {
                int chars = (s == null) ? 0 : s.Length;
                string entity = (chars == 1) ? Properties.Resources.Char : Properties.Resources.Chars;
                label += string.Format(" {0:#,0} " + entity, chars);
            }

            if (settings.ShowLines)
            {
                int lines = 0;
                if (s != null && s.Length > 0)
                {
                    lines = new Regex("\n", RegexOptions.Multiline).Matches(s).Count;
                    if (s[s.Length - 1] != '\n')
                    {
                        lines++;
                    }
                }
                string entity = (lines == 1) ? Properties.Resources.Line : Properties.Resources.Lines;
                if (label != "")
                {
                    label += ", ";
                }

                label += string.Format(" {0:#,0} " + entity, lines);
            }

            textInputPresentation.labelBytesCount.Content = label;
        }

        public void NotifyUpdate()
        {
            OnPropertyChanged("TextOutput");
        }

        private void settings_OnLogMessage(string message, NotificationLevel loglevel)
        {
            GuiLogMessage(message, loglevel);
        }

        private string GetInputString()
        {
            if (textInputPresentation.textBoxInputText.Dispatcher.CheckAccess())
            {
                return textInputPresentation.textBoxInputText.Text;
            }
            else
            {
                return (string)textInputPresentation.textBoxInputText.Dispatcher.Invoke(DispatcherPriority.Normal, (DispatcherOperationCallback)delegate
              {
                  return textInputPresentation.textBoxInputText.Text;
              }, textInputPresentation);
            }
        }


        #region Properties

        [PropertyInfo(Direction.OutputData, "TextOutputCaption", "TextOutputTooltip", true)]
        public string TextOutput
        {
            get => GetInputString();
            set { }
        }

        #endregion

        #region IPlugin Members

        public UserControl Presentation { get; private set; }

        public void Initialize()
        {
            if (textInputPresentation.textBoxInputText != null)
            {
                textInputPresentation.textBoxInputText.TextChanged -= textBoxInputText_TextChanged;
                textInputPresentation.textBoxInputText.TextChanged += textBoxInputText_TextChanged;

                textInputPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    textInputPresentation.textBoxInputText.Text = settings.Text;
                }, null);
            }
        }

        public void Dispose()
        {
            textInputPresentation.textBoxInputText.TextChanged -= textBoxInputText_TextChanged;
        }

        public void Execute()
        {
            NotifyUpdate();
            ShowProgress(100, 100);
            string value = (string)textInputPresentation.textBoxInputText.Dispatcher.Invoke(DispatcherPriority.Normal, (DispatcherOperationCallback)delegate
            {
                return textInputPresentation.textBoxInputText.Text;
            }, textInputPresentation);

            if (string.IsNullOrEmpty(value))
            {
                GuiLogMessage("No input value returning null.", NotificationLevel.Debug);
            }
        }

        public void Stop()
        {
        }

        public void PreExecution()
        {
        }

        public void PostExecution()
        {
        }

        #endregion

        private void ShowProgress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #region IPlugin Members

#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, logLevel));
            }
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private readonly TextInputSettings settings;
        public ISettings Settings => settings;

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}