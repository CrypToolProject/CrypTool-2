/*                              
   Copyright 2023 Nils Kopal, CrypTool Project

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
using CrypTool.PluginBase.Miscellaneous;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;

namespace CrypTool.Plugins.AffineCipher
{
    [Author("Nils Kopal", "kopal@cryptool.org", "CrypTool 2 Team", "https://www.cryptool.org")]
    [PluginInfo("CrypTool.Plugins.AffineCipher.Properties.Resources", "PluginCaption", "PluginTooltip", "AffineCipher/userdoc.xml", new[] { "AffineCipher/icon.png" })]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class AffineCipher : ICrypComponent
    {
        #region Private Variables

        private const string LATIN_ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private readonly AffineCipherSettings _settings = new AffineCipherSettings();

        private int _a;
        private int _b;
        private bool _AHasData = false;
        private bool _BHasData = false;
        private bool _stop = false;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputTextCaption", "InputTextTooltip", true)]
        public string InputText
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "ACaption", "ATooltip", false)]
        public int A
        {
            get
            {
                return _a;
            }
            set
            {
                _a = value;
                _AHasData = true;
            }
        }

        [PropertyInfo(Direction.InputData, "BCaption", "BTooltip", false)]
        public int B
        {
            get
            {
                return _b;
            }
            set
            {
                _b = value;
                _BHasData = true;
            }
        }


        [PropertyInfo(Direction.InputData, "AlphabetCaption", "AlphabetTooltip", false)]
        public string Alphabet
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputTextCaption", "OutputTextTooltip", false)]
        public string OutputText
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings
        {
            get { return _settings; }
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
            Alphabet = LATIN_ALPHABET;
            _stop = false;
            _AHasData = false;
            _BHasData = false;
        }

        public void Execute()
        {
            ProgressChanged(0, 1);

            int a = _settings.A;
            if (_AHasData)
            {
                a = A;
            }
            int b = _settings.B;
            if (_BHasData)
            {
                b = B;
            }

            if (_settings.Action == Action.Encrypt)
            {
                Encrypt(a, b);
            }
            else
            {
                Decrypt(a, b);
            }

            _stop = false;

            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Ecrypts the given text using a and b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        private void Encrypt(int a, int b)
        {            
            int result = GCDExtendedIterative(Alphabet.Length, a, out int inverse);
            if (result != 1) //no inverse exists
            {
                GuiLogMessage(string.Format(Properties.Resources.NumberHasNoInverseElement, a, Alphabet.Length), NotificationLevel.Error);
                return;
            }

            StringBuilder stringBuilder = new StringBuilder();

            int percent = InputText.Length / 100;
            if(percent == 0)
            {
                percent = 1;
            }

            foreach (char c in InputText)
            {
                string str = c.ToString();
                if (!_settings.CaseSensitive)
                {
                    str = str.ToUpper();
                }

                if (Alphabet.Contains(str))
                {
                    int plaintext_symbol = Alphabet.IndexOf(str);
                    int ciphertext_symbol = Mod(a * plaintext_symbol + b, Alphabet.Length);
                    stringBuilder.Append(Alphabet[ciphertext_symbol]);
                }
                else
                {
                    //handle unknown symbol
                    switch (_settings.UnknownSymbolHandling)
                    {
                        case UnknownSymbolHandlingMode.Ignore:
                            stringBuilder.Append(c);
                            break;
                        case UnknownSymbolHandlingMode.Remove:
                            //do nothing;
                            break;
                        case UnknownSymbolHandlingMode.Replace:
                            stringBuilder.Append("?");
                            break;
                    }
                }

                //plugin progress changed every "percent" iterations to update the progress of the plugin
                //also check, if we should stop the plugin
                if (stringBuilder.Length % percent == 0)
                {
                    if (_stop)
                    {
                        return;
                    }
                    ProgressChanged(stringBuilder.Length, InputText.Length);
                }                
            }
            OutputText = stringBuilder.ToString();
            OnPropertyChanged(nameof(OutputText));
        }

        /// <summary>
        /// Decrypts the given text using a and b 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        private void Decrypt(int a, int b)
        {                       
            int result = GCDExtendedIterative(Alphabet.Length, a, out int inverse);
            if (result != 1) //no inverse exists
            {
                GuiLogMessage(string.Format(Properties.Resources.NumberHasNoInverseElement, a, Alphabet.Length), NotificationLevel.Error);
                return;
            }

            StringBuilder builder = new StringBuilder();

            int percent = InputText.Length / 100;
            if(percent == 0)
            {
                percent = 1;
            }

            foreach (char c in InputText)
            {
                string str = c.ToString();
                if (!_settings.CaseSensitive)
                {
                    str = str.ToUpper();
                }

                if (Alphabet.Contains(str))
                {
                    int ciphertext_symbol = Alphabet.IndexOf(str);
                    int plaintext_symbol = Mod((ciphertext_symbol - b) * inverse, Alphabet.Length);
                    builder.Append(Alphabet[plaintext_symbol]);
                }
                else
                {
                    //handle unknown symbol
                    switch (_settings.UnknownSymbolHandling)
                    {
                        case UnknownSymbolHandlingMode.Ignore:
                            builder.Append(c);
                            break;
                        case UnknownSymbolHandlingMode.Remove:
                            //do nothing;
                            break;
                        case UnknownSymbolHandlingMode.Replace:
                            builder.Append("?");
                            break;
                    }
                }

                //plugin progress changed every "percent" iterations to update the progress of the plugin
                //also check, if we should stop the plugin
                if (builder.Length % percent == 0)
                {
                    ProgressChanged(builder.Length, InputText.Length);
                    if (_stop)
                    {
                        return;
                    }
                }
            }
            OutputText = builder.ToString();
            OnPropertyChanged(nameof(OutputText));
        }
        
        public void PostExecution()
        {
        }

        public void Stop()
        {
            //set stop variable to true to stop the plugin
            _stop = true;
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion

        /// <summary>
        /// Mathematical modulo operator
        /// </summary>
        /// <param name="number"></param>
        /// <param name="mod"></param>
        /// <returns></returns>
        private int Mod(int number, int mod)
        {
            return (number % mod + mod) % mod;
        }

        /// <summary>
        /// Extended Euclidian algorithm to compute inverse of a given number when calculating with modulo
        /// See https://en.wikipedia.org/wiki/Extended_Euclidean_algorithm
        /// </summary>
        /// <param name="modulo"></param>
        /// <param name="number"></param>
        /// <param name="inverse"></param>
        /// <returns>1 if there exists an inverse or another number if not</returns>
        private int GCDExtendedIterative(int modulo, int number, out int inverse)
        {
            int s = 1;
            inverse = 0;
            int u = 0;
            int v = 1;
            while (number != 0)
            {
                int q = modulo / number;
                int b1 = number;
                number = modulo - q * number;
                modulo = b1;
                int u1 = u;
                u = s - q * u;
                s = u1;
                int v1 = v;
                v = inverse - q * v;
                inverse = v1;
            }
            return modulo;
        }
    }
}
