/*                              
   Copyright 2012 Nils Kopal, Uni Duisburg-Essen

   the underlying password strength algorithm was obtained from 
   http://people.mozilla.org/~jfinette/public_html/topics/passwords/test/
 
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
using System;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace CrypTool.Plugins.Tools
{

    [Author("Nils Kopal", "nils.kopal@CrypTool.de", "Uni Duisburg", "http://www.uni-duisburg-essen.de")]
    [PluginInfo("CrypTool.Plugins.Tools.Properties.Resources", "PluginCaption", "PluginTooltip",
        "PasswordStrength/DetailedDescription/doc.xml", "PasswordStrength/images/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class PasswordStrength : ICrypComponent
    {
        private readonly PasswordStrengthPresentation _presentation = new PasswordStrengthPresentation();
        private byte[] _password;
        private int _strength;
        private double _entropy;
        private int _keePass;

        public event PropertyChangedEventHandler PropertyChanged;

        [PropertyInfo(Direction.InputData, "PasswordCaption", "PasswordTooltip", false)]
        public byte[] Password
        {
            get => _password;
            set => _password = value;
        }

        [PropertyInfo(Direction.OutputData, "KeePassCaption", "KeePassTooltip", false)]
        public int KeePass
        {
            get => _keePass;
            set
            {
                _keePass = value;
                OnPropertyChanged("KeePass");
            }
        }

        [PropertyInfo(Direction.OutputData, "StrengthCaption", "StrengthTooltip", false)]
        public int Strength
        {
            get => _strength;
            set
            {
                _strength = value;
                OnPropertyChanged("Strength");
            }
        }

        [PropertyInfo(Direction.OutputData, "EntropyCaption", "EntropyTooltip", false)]
        public double Entropy
        {
            get => _entropy;
            set
            {
                _entropy = value;
                OnPropertyChanged("Entropy");
            }
        }

        public void Dispose()
        {

        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public ISettings Settings => null;

        public UserControl Presentation => _presentation;

        public void Execute()
        {
            if (_password == null)
            {
                return;
            }

            string password = Encoding.UTF8.GetString(_password);
            int numberOfCharacters = password.Length;
            int numberOfLowercaseLetters = 0;
            int numberOfUppercaseLetters = 0;
            int numberOfNumbers = 0;
            int numberOfSymbols = 0;
            int middleNumberOfSymbols = 0;
            int requirements = 0;
            int lettersOnly = 0;
            int numbersOnly = 0;
            int consecutiveUppercaseLetters = 0;
            int consecutiveLowercaseLetters = 0;
            int consecutiveNumbers = 0;
            int sequentialLetters = 0;
            int sequentialNumbers = 0;
            int sequentialSymbols = 0;
            int repeatCharacters = 0;
            double repeatedCharsBonus = 0;

            int position = 0;
            char lastChar = (char)0;

            foreach (char c in password)
            {
                //Number of lowercase letters
                if (c >= 'a' && c <= 'z')
                {
                    numberOfLowercaseLetters++;
                }
                //Number of uppercase letters
                if (c >= 'A' && c <= 'Z')
                {
                    numberOfUppercaseLetters++;
                }
                //Number of numbers
                if (c >= '0' && c <= '9')
                {
                    numberOfNumbers++;
                }
                //Number or symbols
                if (!(c >= 'a' && c <= 'z') &&
                   !(c >= 'A' && c <= 'Z') &&
                   !(c >= '0' && c <= '9') &&
                   !(c >= '0' && c <= '9') &&
                   c != ' ')
                {
                    numberOfSymbols++;
                }
                //middle number of symbols
                if (position > 0 && position < numberOfCharacters - 1 &&
                   !(c >= 'a' && c <= 'z') &&
                   !(c >= 'A' && c <= 'Z') &&
                   c != ' ')
                {
                    middleNumberOfSymbols++;
                }

                //Consecutive Uppercase Letters
                if ((c >= 'A' && c <= 'Z') &&
                   (lastChar >= 'A' && lastChar <= 'Z'))
                {
                    consecutiveUppercaseLetters++;
                }

                //Consecutive Lowercase Letters
                if ((c >= 'a' && c <= 'z') &&
                   (lastChar >= 'a' && lastChar <= 'z'))
                {
                    consecutiveLowercaseLetters++;
                }

                //Consecutive Numbers
                if ((c >= '0' && c <= '9') &&
                   (lastChar >= '0' && lastChar <= '9'))
                {
                    consecutiveNumbers++;
                }

                //Repeated Characters
                bool charExists = false;
                for (int j = 0; j < password.Length; j++)
                {
                    if (password[position] == password[j] && position != j)
                    {
                        charExists = true;
                        double add = password.Length / ((double)j - position);
                        if (add < 0)
                        {
                            add *= -1;
                        }
                        repeatedCharsBonus += add;
                    }
                }
                if (charExists)
                {
                    repeatCharacters++;
                    int nUnqChar = password.Length - repeatCharacters;
                    repeatedCharsBonus = (nUnqChar != 0) ? Math.Ceiling(repeatedCharsBonus / nUnqChar) : Math.Ceiling(repeatedCharsBonus);
                }

                position++;
                lastChar = c;
            }//foreach (byte b in _password)

            const string alphas = "abcdefghijklmnopqrstuvwxyz";
            for (int s = 0; s < 23; s++)
            {
                string sFwd = alphas.Substring(s, 3);
                string sRev = reverseString(sFwd);
                if (password.ToLower().IndexOf(sFwd, StringComparison.Ordinal) != -1 ||
                    password.ToLower().IndexOf(sRev, StringComparison.Ordinal) != -1) { sequentialLetters++; }
            }

            const string numerics = "01234567890";
            for (int s = 0; s < 8; s++)
            {
                string sFwd = numerics.Substring(s, 3);
                string sRev = reverseString(sFwd);
                if (password.ToLower().IndexOf(sFwd, StringComparison.Ordinal) != -1 ||
                    password.ToLower().IndexOf(sRev, StringComparison.Ordinal) != -1) { sequentialNumbers++; }
            }

            //Sequential Symbols
            const string usSymbols = ")!@#$%^&*()";
            const string gerSymbols = "=!\"§$%&/()=";
            for (int s = 0; s < 8; s++)
            {
                //US Keyboard
                string sFwd = usSymbols.Substring(s, 3);
                string sRev = reverseString(sFwd);
                if (password.ToLower().IndexOf(sFwd, StringComparison.OrdinalIgnoreCase) != -1 ||
                    password.ToLower().IndexOf(sRev, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    sequentialSymbols++;
                }

                //German Keyboard
                sFwd = gerSymbols.Substring(s, 3);
                sRev = reverseString(sFwd);
                if (password.ToLower().IndexOf(sFwd, StringComparison.OrdinalIgnoreCase) != -1 ||
                    password.ToLower().IndexOf(sRev, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    sequentialSymbols++;
                }
            }


            //Requirements

            if (numberOfLowercaseLetters > 0)
            {
                requirements++;
            }
            if (numberOfUppercaseLetters > 0)
            {
                requirements++;
            }
            if (numberOfNumbers > 0)
            {
                requirements++;
            }
            if (numberOfSymbols > 0)
            {
                requirements++;
            }

            //Letters only
            if ((numberOfLowercaseLetters != 0 ||
                numberOfUppercaseLetters != 0) &&
                numberOfSymbols == 0 &&
                numberOfNumbers == 0)
            {
                lettersOnly = numberOfCharacters;
            }

            //Numbers Only
            if (numberOfLowercaseLetters == 0 &&
                numberOfUppercaseLetters == 0 &&
                numberOfSymbols == 0 &&
                numberOfNumbers > 0)
            {
                numbersOnly = numberOfCharacters;
            }

            _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    // Additions:

                    //Number of characters
                    _presentation.NumberOfCharacters_CountTextBlock.Text = string.Format("{0}", numberOfCharacters);
                    _presentation.NumberOfCharacters_BonusTextBlock.Text = string.Format("{0}", numberOfCharacters * 4);
                    _presentation.NumberOfCharacters_BonusTextBlock.Foreground = numberOfCharacters < 8 ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Green);
                    if (numberOfCharacters < 8)
                    {
                        _presentation.NumberOfCharactersImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/failure.png", UriKind.Relative));
                    }
                    else if (numberOfCharacters == 8)
                    {
                        _presentation.NumberOfCharactersImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/sufficient.png", UriKind.Relative));
                    }
                    else
                    {
                        _presentation.NumberOfCharactersImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/exceptional.png", UriKind.Relative));
                    }

                    //Number of lowercase letters
                    _presentation.LowercaseLetters_CountTextBlock.Text = string.Format("{0}", numberOfLowercaseLetters);
                    _presentation.LowercaseLetters_BonusTextBlock.Text = string.Format("{0}", numberOfLowercaseLetters > 0 && (numberOfCharacters - numberOfLowercaseLetters) > 0 ? (numberOfCharacters - numberOfLowercaseLetters) * 2 : 0);
                    _presentation.LowercaseLetters_BonusTextBlock.Foreground = numberOfLowercaseLetters == 0 ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Green);
                    if (numberOfLowercaseLetters < 1)
                    {
                        _presentation.LowercaseLettersImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/failure.png", UriKind.Relative));
                    }
                    else if (numberOfLowercaseLetters == 1)
                    {
                        _presentation.LowercaseLettersImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/sufficient.png", UriKind.Relative));
                    }
                    else
                    {
                        _presentation.LowercaseLettersImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/exceptional.png", UriKind.Relative));
                    }

                    //Number of uppercase letters
                    _presentation.UppercaseLetters_CountTextBlock.Text = string.Format("{0}", numberOfUppercaseLetters);
                    _presentation.UppercaseLetters_BonusTextBlock.Text = string.Format("{0}", numberOfUppercaseLetters > 0 && (numberOfCharacters - numberOfUppercaseLetters) > 0 ? (numberOfCharacters - numberOfUppercaseLetters) * 2 : 0);
                    _presentation.UppercaseLetters_BonusTextBlock.Foreground = numberOfUppercaseLetters == 0 ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Green);
                    if (numberOfUppercaseLetters < 1)
                    {
                        _presentation.UppercaseLettersImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/failure.png", UriKind.Relative));
                    }
                    else if (numberOfUppercaseLetters == 1)
                    {
                        _presentation.UppercaseLettersImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/sufficient.png", UriKind.Relative));
                    }
                    else
                    {
                        _presentation.UppercaseLettersImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/exceptional.png", UriKind.Relative));
                    }

                    //Number of numbers
                    _presentation.Numbers_CountTextBlock.Text = string.Format("{0}", numberOfNumbers);
                    _presentation.Numbers_BonusTextBlock.Text = string.Format("{0}", numberOfNumbers < numberOfCharacters ? numberOfNumbers * 4 : 0);
                    _presentation.Numbers_BonusTextBlock.Foreground = numberOfNumbers == 0 ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Green);
                    if (numberOfNumbers < 1)
                    {
                        _presentation.NumbersImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/failure.png", UriKind.Relative));
                    }
                    else if (numberOfNumbers == 1)
                    {
                        _presentation.NumbersImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/sufficient.png", UriKind.Relative));
                    }
                    else
                    {
                        _presentation.NumbersImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/exceptional.png", UriKind.Relative));
                    }

                    //Number of symbols
                    _presentation.Symbols_CountTextBlock.Text = string.Format("{0}", numberOfSymbols);
                    _presentation.Symbols_BonusTextBlock.Text = string.Format("{0}", numberOfSymbols * 6);
                    _presentation.Symbols_BonusTextBlock.Foreground = numberOfSymbols == 0 ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Green);
                    if (numberOfSymbols < 1)
                    {
                        _presentation.SymbolsImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/failure.png", UriKind.Relative));
                    }
                    else if (numberOfSymbols == 1)
                    {
                        _presentation.SymbolsImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/sufficient.png", UriKind.Relative));
                    }
                    else
                    {
                        _presentation.SymbolsImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/exceptional.png", UriKind.Relative));
                    }

                    //Middle number of symbols
                    _presentation.MiddleNumberOrSymbols_CountTextBlock.Text = string.Format("{0}", middleNumberOfSymbols);
                    _presentation.MiddleNumberOrSymbols_BonusTextBlock.Text = string.Format("{0}", middleNumberOfSymbols * 2);
                    _presentation.MiddleNumberOrSymbols_BonusTextBlock.Foreground = middleNumberOfSymbols == 0 ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Green);
                    if (middleNumberOfSymbols < 1)
                    {
                        _presentation.MiddleNumbersOrSymbolsImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/failure.png", UriKind.Relative));
                    }
                    else if (middleNumberOfSymbols == 1)
                    {
                        _presentation.MiddleNumbersOrSymbolsImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/sufficient.png", UriKind.Relative));
                    }
                    else
                    {
                        _presentation.MiddleNumbersOrSymbolsImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/exceptional.png", UriKind.Relative));
                    }

                    //Requirements
                    _presentation.Requirements_CountTextBlock.Text = string.Format("{0}", requirements + (numberOfCharacters > 8 ? 1 : 0));
                    _presentation.Requirements_BonusTextBlock.Text = string.Format("{0}", (requirements >= 3 && numberOfCharacters >= 8) ? ((requirements + (numberOfCharacters > 8 ? 1 : 0)) * 2) : 0);
                    _presentation.Requirements_BonusTextBlock.Foreground = requirements < 3 || numberOfCharacters < 8 ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Green);
                    if (requirements < 3 || numberOfCharacters < 8)
                    {
                        _presentation.RequirementsImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/failure.png", UriKind.Relative));
                    }
                    else if (requirements + (numberOfCharacters > 8 ? 1 : 0) == 4)
                    {
                        _presentation.RequirementsImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/sufficient.png", UriKind.Relative));
                    }
                    else
                    {
                        _presentation.RequirementsImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/exceptional.png", UriKind.Relative));
                    }

                    //Deductions

                    //Letters only
                    _presentation.LettersOnly_CountTextBlock.Text = string.Format("{0}", lettersOnly);
                    _presentation.LettersOnly_BonusTextBlock.Text = string.Format("{0}", lettersOnly);
                    _presentation.LettersOnly_BonusTextBlock.Foreground = lettersOnly == 0 ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
                    if (lettersOnly == 0)
                    {
                        _presentation.LettersOnlyImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/sufficient.png", UriKind.Relative));
                    }
                    else
                    {
                        _presentation.LettersOnlyImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/warning.png", UriKind.Relative));
                    }

                    //Numbers only
                    _presentation.NumbersOnly_CountTextBlock.Text = string.Format("{0}", numbersOnly);
                    _presentation.NumbersOnly_BonusTextBlock.Text = string.Format("{0}", numbersOnly);
                    _presentation.NumbersOnly_BonusTextBlock.Foreground = numbersOnly == 0 ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
                    if (numbersOnly == 0)
                    {
                        _presentation.NumbersOnlyImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/sufficient.png", UriKind.Relative));
                    }
                    else
                    {
                        _presentation.NumbersOnlyImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/warning.png", UriKind.Relative));
                    }

                    //Consecutive Uppercase Letters
                    _presentation.ConsecutiveUppercaseLetters_CountTextBlock.Text = string.Format("{0}", consecutiveUppercaseLetters);
                    _presentation.ConsecutiveUppercaseLetters_BonusTextBlock.Text = string.Format("{0}", consecutiveUppercaseLetters * 2);
                    _presentation.ConsecutiveUppercaseLetters_BonusTextBlock.Foreground = consecutiveUppercaseLetters == 0 ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
                    if (consecutiveUppercaseLetters == 0)
                    {
                        _presentation.ConsecutiveUppercaseLettersImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/sufficient.png", UriKind.Relative));
                    }
                    else
                    {
                        _presentation.ConsecutiveUppercaseLettersImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/warning.png", UriKind.Relative));
                    }

                    //Consecutive Lowercase Letters
                    _presentation.ConsecutiveLowercaseLetters_CountTextBlock.Text = string.Format("{0}", consecutiveLowercaseLetters);
                    _presentation.ConsecutiveLowercaseLetters_BonusTextBlock.Text = string.Format("{0}", consecutiveLowercaseLetters * 2);
                    _presentation.ConsecutiveLowercaseLetters_BonusTextBlock.Foreground = consecutiveLowercaseLetters == 0 ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
                    if (consecutiveLowercaseLetters == 0)
                    {
                        _presentation.ConsecutiveLowercaseLettersImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/sufficient.png", UriKind.Relative));
                    }
                    else
                    {
                        _presentation.ConsecutiveLowercaseLettersImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/warning.png", UriKind.Relative));
                    }

                    //Consecutive numbers
                    _presentation.ConsecutiveNumbers_CountTextBlock.Text = string.Format("{0}", consecutiveNumbers);
                    _presentation.ConsecutiveNumbers_BonusTextBlock.Text = string.Format("{0}", consecutiveNumbers * 2);
                    _presentation.ConsecutiveNumbers_BonusTextBlock.Foreground = consecutiveNumbers == 0 ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
                    if (consecutiveNumbers == 0)
                    {
                        _presentation.ConsecutiveNumbersImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/sufficient.png", UriKind.Relative));
                    }
                    else
                    {
                        _presentation.ConsecutiveNumbersImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/warning.png", UriKind.Relative));
                    }

                    //Sequential Letters
                    _presentation.SequentialLetters_CountTextBlock.Text = string.Format("{0}", sequentialLetters);
                    _presentation.SequentialLetters_BonusTextBlock.Text = string.Format("{0}", sequentialLetters * 3);
                    _presentation.SequentialLetters_BonusTextBlock.Foreground = sequentialLetters == 0 ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
                    if (sequentialLetters == 0)
                    {
                        _presentation.SequentialLettersImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/sufficient.png", UriKind.Relative));
                    }
                    else
                    {
                        _presentation.SequentialLettersImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/warning.png", UriKind.Relative));
                    }

                    //Sequential Numbers
                    _presentation.SequentialNumbers_CountTextBlock.Text = string.Format("{0}", sequentialNumbers);
                    _presentation.SequentialNumbers_BonusTextBlock.Text = string.Format("{0}", sequentialNumbers * 3);
                    _presentation.SequentialNumbers_BonusTextBlock.Foreground = sequentialNumbers == 0 ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
                    if (sequentialNumbers == 0)
                    {
                        _presentation.SequentialNumbers.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/sufficient.png", UriKind.Relative));
                    }
                    else
                    {
                        _presentation.SequentialNumbers.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/warning.png", UriKind.Relative));
                    }

                    //Sequential Symbols
                    _presentation.SequentialSymbols_CountTextBlock.Text = string.Format("{0}", sequentialSymbols);
                    _presentation.SequentialSymbols_BonusTextBlock.Text = string.Format("{0}", sequentialSymbols * 3);
                    _presentation.SequentialSymbols_BonusTextBlock.Foreground = sequentialSymbols == 0 ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
                    if (sequentialSymbols == 0)
                    {
                        _presentation.SequentialSymbols.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/sufficient.png", UriKind.Relative));
                    }
                    else
                    {
                        _presentation.SequentialSymbols.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/warning.png", UriKind.Relative));
                    }

                    //Repeat Symbols
                    _presentation.RepeatCharactersCaseInsensitive_CountTextBlock.Text = string.Format("{0}", repeatCharacters);
                    _presentation.RepeatCharactersCaseInsensitive_BonusTextBlock.Text = string.Format("{0}", repeatedCharsBonus);
                    _presentation.RepeatCharactersCaseInsensitive_BonusTextBlock.Foreground = repeatCharacters == 0 ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
                    if (repeatCharacters == 0)
                    {
                        _presentation.RepeatCharactersImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/sufficient.png", UriKind.Relative));
                    }
                    else
                    {
                        _presentation.RepeatCharactersImage.Source = new BitmapImage(new Uri(@"/PasswordStrength;component/Images/warning.png", UriKind.Relative));
                    }

                    //Calculate and set progress bar
                    int totalValue = int.Parse(_presentation.NumberOfCharacters_BonusTextBlock.Text) +
                                     int.Parse(_presentation.LowercaseLetters_BonusTextBlock.Text) +
                                     int.Parse(_presentation.UppercaseLetters_BonusTextBlock.Text) +
                                     int.Parse(_presentation.Numbers_BonusTextBlock.Text) +
                                     int.Parse(_presentation.Symbols_BonusTextBlock.Text) +
                                     int.Parse(_presentation.Requirements_BonusTextBlock.Text) +
                                     int.Parse(_presentation.MiddleNumberOrSymbols_BonusTextBlock.Text) -
                                     int.Parse(_presentation.LettersOnly_BonusTextBlock.Text) -
                                     int.Parse(_presentation.NumbersOnly_BonusTextBlock.Text) -
                                     int.Parse(_presentation.ConsecutiveUppercaseLetters_BonusTextBlock.Text) -
                                     int.Parse(_presentation.ConsecutiveLowercaseLetters_BonusTextBlock.Text) -
                                     int.Parse(_presentation.ConsecutiveNumbers_BonusTextBlock.Text) -
                                     int.Parse(_presentation.SequentialLetters_BonusTextBlock.Text) -
                                     int.Parse(_presentation.SequentialNumbers_BonusTextBlock.Text) -
                                     int.Parse(_presentation.SequentialSymbols_BonusTextBlock.Text) -
                                     int.Parse(_presentation.RepeatCharactersCaseInsensitive_BonusTextBlock.Text);
                    if (totalValue > 100)
                    {
                        totalValue = 100;
                    }
                    if (totalValue < 0)
                    {
                        totalValue = 0;
                    }
                    _presentation.ScoreBar.Value = totalValue;
                    _presentation.ScoreValue.Text = string.Format("{0}", totalValue);
                    byte r = (byte)((1.0 - totalValue / 100.0) * 255.0);
                    byte g = (byte)((totalValue / 100.0) * 255.0);
                    Brush brush = new SolidColorBrush(Color.FromRgb(r, g, 0));
                    _presentation.ScoreBar.Foreground = brush;

                    //Calculate and set Complexity
                    string complexity = null;
                    if (totalValue >= 0 && totalValue < 20) { complexity = Properties.Resources._VeryWeak; }
                    else if (totalValue >= 20 && totalValue < 40) { complexity = Properties.Resources._Weak; }
                    else if (totalValue >= 40 && totalValue < 60) { complexity = Properties.Resources._Good; }
                    else if (totalValue >= 60 && totalValue < 80) { complexity = Properties.Resources._Strong; }
                    else if (totalValue >= 80 && totalValue <= 100) { complexity = Properties.Resources._VeryStrong; }
                    _presentation.ComplexityTextBlock.Text = complexity;
                    Strength = totalValue;
                    double entropyValue = calculateEntropy(_password);
                    _presentation.EntropyTextblock.Text = string.Format("{0}", Math.Round(entropyValue, 3));
                    Entropy = entropyValue;
                    int keePassValue = KeePassQualityEstimation.EstimatePasswordBits(_password);
                    _presentation.BitStrengthTextBlock.Text = string.Format("{0}", keePassValue, 3);
                    KeePass = keePassValue;
                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format("Exception during update of PasswordStrenghPresentation: {0}", ex.Message), NotificationLevel.Error);
                }
            }, null);

        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, logLevel));
            }
        }

        private string reverseString(string str)
        {
            string ret = "";
            for (int i = str.Length; i > 0; i--)
            {
                ret += str.Substring(i - 1, 1);
            }
            return ret;
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
        }

        public void PreExecution()
        {
        }

        public void PostExecution()
        {
            _password = new byte[0];
            Execute();
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void OnPropertyChanged(string p)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(p));
        }

        /// <summary>
        /// Calculates the Entropy of a given byte array 
        /// for example a German text has about 4.0629
        /// </summary>
        /// <param name="text">text to use</param>
        /// <returns>Entropy</returns>
        public double calculateEntropy(byte[] text)
        {

            float[] xlogx = new float[text.Length + 1];
            //precomputations for fast entropy calculation	
            xlogx[0] = 0.0f;
            for (int i = 1; i <= text.Length; i++)
            {
                xlogx[i] = (float)(-1.0f * i * Math.Log(i / (double)text.Length) / Math.Log(2.0));
            }

            int[] n = new int[256];
            //count all ASCII symbols
            for (int counter = 0; counter < text.Length; counter++)
            {
                n[text[counter]]++;
            }

            float entropy = 0;
            //calculate probabilities and sum entropy
            for (int i = 0; i < 256; i++)
            {
                entropy += xlogx[n[i]];
            }
            return entropy / (double)text.Length;
        }
    }
}
