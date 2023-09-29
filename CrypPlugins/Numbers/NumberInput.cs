/*                              
   Copyright 2009 Team CrypTool (Sven Rech,Dennis Nolte,Raoul Falk,Nils Kopal), Uni Duisburg-Essen

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
using System;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace CrypTool.Plugins.Numbers
{
    [Author("Sven Rech, Nils Kopal", "sven.rech@CrypTool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("CrypTool.Plugins.Numbers.Properties.Resources", "PluginInputCaption", "PluginInputTooltip", "Numbers/DetailedDescription/doc.xml", "Numbers/icons/inputIcon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    [ComponentVisualAppearance(ComponentVisualAppearance.VisualAppearanceEnum.Opened)]
    public class NumberInput : ICrypComponent
    {
        private readonly NumberInputPresentation _presentation;
        private bool _running = false;

        public NumberInput()
        {
            settings = new NumberInputSettings();
            _presentation = new NumberInputPresentation(CrypTool.PluginBase.Properties.Settings.Default.FontFamily, CrypTool.PluginBase.Properties.Settings.Default.FontSize);
            _presentation.TextBox.TextChanged += new TextChangedEventHandler(TextBox_TextChanged);
            DataObject.AddPastingHandler(_presentation.TextBox, OnCancelCommand);
            settings.PropertyChanged += settings_OnPropertyChanged;
            _presentation.UserKeyDown += _presentation_UserKeyDown;
        }

        private void _presentation_UserKeyDown(object sender, KeyEventArgs e)
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
            if (e.PropertyName == "Number")
            {
                _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    _presentation.TextBox.Text = settings.Number;
                }, null);
            }
            else if (e.PropertyName == "Font")
            {
                _presentation.MyFontFamily = new System.Windows.Media.FontFamily(settings.Fonts[settings.Font]);
            }
            else if (e.PropertyName == "FontSize")
            {
                _presentation.MyFontSize = settings.FontSize;
            }
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                SetStatusBar();
            }, null);
        }

        private void OnCancelCommand(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetData(DataFormats.Text) is string)
            {
                string s = (string)e.DataObject.GetData(DataFormats.Text);
                if (s.Any(c => !"01234567890+-*/^ ()AaBbCcDdEeFf#HhXx\r\n\t".Contains(c)))
                {
                    e.CancelCommand();
                }
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs args)
        {
            settings.Number = _presentation.TextBox.Text;
            SetStatusBar();
            if (_running)
            {
                Execute();
            }
        }

        private void SetStatusBar()
        {
            if (settings.ShowDigits)
            {
                _presentation.StatusBar.Visibility = Visibility.Visible;
                int digits, bits;
                try
                {
                    GetDigitsAndBitsWithTimeOut(out digits, out bits);
                }
                catch (Exception ex)
                {
                    _presentation.StatusBar.Content = (ex is OutOfMemoryException || ex is OverflowException) ? "Overflow" : (ex is TimeoutException) ? "Timeout" : "Not a number";
                    return;
                }

                string digitText = (digits == 1) ? Properties.Resources.Digit : Properties.Resources.Digits;
                string bitText = (bits == 1) ? Properties.Resources.Bit : Properties.Resources.Bits;
                _presentation.StatusBar.Content = string.Format(" {0:#,0} {1}, {2:#,0} {3}", digits, digitText, bits, bitText);
            }
            else
            {
                _presentation.StatusBar.Visibility = Visibility.Collapsed;
            }
        }

        // status=0: no problems occured
        // status=1: overflow
        // status=2: not a number
        private int[] GetDigitsAndBits(out int status)
        {
            try
            {
                BigInteger number = GetNumber();
                int bits = number.BitCount();
                int digits = BigInteger.Abs(number).ToString().Length;
                status = 0;
                return new int[] { digits, bits };
            }
            catch (Exception ex)
            {
                status = (ex is OutOfMemoryException || ex is OverflowException) ? 1 : 2;
                return null;
            }
        }

        private Thread workerThread;
        private void GetDigitsAndBitsWithTimeOut(out int digits, out int bits)
        {
            int[] result = null;
            int status = 0;

            if (workerThread != null && workerThread.IsAlive)
            {
                workerThread.Abort();
            }

            workerThread = new Thread(() => result = GetDigitsAndBits(out status));

            workerThread.Start();

            bool finished = workerThread.Join(TimeSpan.FromMilliseconds(500));
            if (!finished)
            {
                workerThread.Abort();
                throw new TimeoutException();
            }

            if (status == 1)
            {
                throw new OverflowException();
            }

            if (status == 2)
            {
                throw new Exception();
            }

            digits = result[0];
            bits = result[1];
        }

        private BigInteger GetNumber()
        {
            //The input from the taskpane is converted to a BigNumber and is sent to the output.
            if (settings.Number == null || settings.Number.Equals(""))
            {
                return BigInteger.Zero;
            }
            string strNumber = Regex.Replace(settings.Number, @"\s+", "");

            //if we have only a single minus, we throw a special exception
            //to avoid showing the user a parsing exception
            if (strNumber.Equals("-"))
            {
                throw new NumberIsMinusSymbolException();
            }
            return BigIntegerHelper.ParseExpression(strNumber);
        }

        #region Properties

        private BigInteger numberOutput = 0;
        /// <summary>
        /// The output is defined
        /// </summary>
        [PropertyInfo(Direction.OutputData, "NumberOutputCaption", "NumberOutputTooltip")]
        public BigInteger NumberOutput
        {
            get => numberOutput;
            set
            {
                numberOutput = value;
                OnPropertyChanged("NumberOutput");
            }
        }

        #endregion

        #region IPlugin Members

        public event CrypTool.PluginBase.StatusChangedEventHandler OnPluginStatusChanged;

        public event CrypTool.PluginBase.GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        public event CrypTool.PluginBase.PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        private NumberInputSettings settings;
        public ISettings Settings
        {
            get => settings;
            set => settings = (NumberInputSettings)value;
        }

        public UserControl Presentation => _presentation;

        public void PreExecution()
        {
            _running = true;
        }

        private Thread thread = null;
        public void Execute()
        {
            try
            {
                if (thread != null && thread.IsAlive)
                {
                    thread.Abort();
                }
            }
            catch (Exception)
            {
            }
            thread = new Thread(() => Execute2())
            {
                IsBackground = true,
                CurrentCulture = Thread.CurrentThread.CurrentCulture,
                CurrentUICulture = Thread.CurrentThread.CurrentUICulture
            };
            thread.Start();

            //thread.Join();
        }

        public void Execute2()
        {
            ProgressChanged(0.0, 1.0);

            try
            {
                NumberOutput = GetNumber();
            }
            catch (ThreadAbortException)
            {
                return;
            }
            catch (NumberIsMinusSymbolException)
            {
                //we have a single minus symbol, thus, ignore exception
            }
            catch (Exception ex)
            {
                GuiLogMessage("Invalid Big Number input: " + ex.Message, NotificationLevel.Error);
                return;
            }
            ProgressChanged(1.0, 1.0);
        }

        public void PostExecution()
        {
            Dispose();
        }

        public void Stop()
        {
            _running = false;
            if (thread != null && thread.IsAlive)
            {
                thread.Abort();
            }
        }

        public void Initialize()
        {
            _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _presentation.TextBox.Text = settings.Number;
            }
            , null);
        }

        public void Dispose()
        {
            _running = false;
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string p)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(p));
        }

        #endregion
    }

    public class NumberIsMinusSymbolException : Exception
    {

    }
}
