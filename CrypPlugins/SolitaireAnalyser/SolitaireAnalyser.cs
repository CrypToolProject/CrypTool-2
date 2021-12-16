/*
   Copyright 2010 CrypTool 2 Team

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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SolitaireAnalyser
{
    [Author("Coen Ramaekers", "cfwramaekers@gmail.com", "Technische Universiteit Eindhoven", "http://www.win.tue.nl")]
    [PluginInfo("SolitaireAnalyser.Properties.Resources", "PluginCaption", "PluginTooltip", "SolitaireAnalyser/DetailedDescription/doc.xml", "SolitaireAnalyser/solitaireanalyser.jpg")]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class SolitaireAnalyser : ICrypComponent

    {
        #region Private Variables
        private readonly SolitaireAnalyserSettings settings = new SolitaireAnalyserSettings();

        private readonly SolitaireAnalyserQuickWatchPresentation myPresentation;

        private string inputString, outputString, password;

        private string[] wordDictionary, passDictionary, scoreList, passList, textList;

        public int[] indexList;

        private bool stop = false;

        #endregion

        #region Data Properties

        public SolitaireAnalyser()
        {
            myPresentation = new SolitaireAnalyserQuickWatchPresentation();
            Presentation = myPresentation;
        }

        /// <summary>
        /// The ciphertext to be analyzed
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputStringCaption", "InputStringTooltip", true)]
        public string InputString
        {
            get => inputString;
            set
            {
                if (value != InputString)
                {
                    inputString = value;
                    OnPropertyChanged("InputString");
                }
            }
        }

        /// <summary>
        /// The dictionary to be used to recognize words in the plaintext
        /// </summary>
        [PropertyInfo(Direction.InputData, "WordDictionaryCaption", "WordDictionaryTooltip", true)]
        public string[] WordDictionary
        {
            get => wordDictionary;
            set
            {
                wordDictionary = value;
                OnPropertyChanged("Dictionary");
            }
        }

        /// <summary>
        /// The dictionary holding all passwords to be tried
        /// </summary>
        [PropertyInfo(Direction.InputData, "PassDictionaryCaption", "PassDictionaryTooltip", true)]
        public string[] PassDictionary
        {
            get => passDictionary;
            set
            {
                passDictionary = value;
                OnPropertyChanged("PassDictionary");
            }
        }

        /// <summary>
        /// The plaintext to be output
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", false)]
        public string OutputString
        {
            get => outputString;
            set
            {
                outputString = value;
                OnPropertyChanged("OutputString");
            }
        }

        /// <summary>
        /// The password which had the best result
        /// </summary>
        [PropertyInfo(Direction.OutputData, "PasswordCaption", "PasswordTooltip", false)]
        public string Password
        {
            get => password;
            set
            {
                password = value;
                OnPropertyChanged("Password");
            }
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings => settings;

        public UserControl Presentation
        {
            get;
            private set;
        }

        public void PreExecution()
        {
        }

        public void Execute()
        {
            clearScreen();
            if (passDictionary != null)
            {
                if (wordDictionary != null)
                {
                    if (inputString != null)
                    {
                        dictionaryAttack();
                    }
                    else
                    {
                        GuiLogMessage("You have to insert a ciphertext!", NotificationLevel.Warning);
                    }
                }
                else
                {
                    GuiLogMessage("You have to connect a dictionary!", NotificationLevel.Warning);
                }
            }
            else
            {
                GuiLogMessage("You have to connect a password dictionary!", NotificationLevel.Warning);
            }

            Stop();
        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
            stop = true;
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        public void dictionaryAttack()
        {
            stop = false;

            System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();

            int i, maxLength = 0, start, length, score, j, k;
            int[] bestScore = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            string decryption = "";

            updateDicSize(passDictionary.Length);
            for (i = 0; i < wordDictionary.Length; i++)
            {
                maxLength = Math.Max(wordDictionary[i].Length, maxLength);
            }

            System.Collections.Generic.Dictionary<string, string> dictionary = wordDictionary
                .GroupBy(item => item, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(item => item.Key, item => item.First(), StringComparer.OrdinalIgnoreCase);

            indexList = new int[10];
            scoreList = new string[10];
            passList = new string[10];
            textList = new string[10];
            Stopwatch sw = new Stopwatch();
            TimeSpan time;
            double ts;
            string remaining = "";
            bool test;
            sw.Start();

            for (i = 0; i < passDictionary.Length & !stop; i++)
            {
                if (i != 0 & i % 1000 == 0)
                {
                    ts = sw.ElapsedMilliseconds * ((double)(passDictionary.Length - i) / i);
                    time = TimeSpan.FromMilliseconds(ts);
                    remaining = string.Format("{0:00}:{1:00}:{2:00}", time.Hours, time.Minutes, time.Seconds);
                    remaining = remaining + " (hh:mm:ss)";
                    ts = 1000 * (i / (double)sw.ElapsedMilliseconds);
                    updateRemTime(remaining);
                    updateDicPos(i);
                    updatePassSec(Convert.ToInt32(ts));
                }

                decryption = decrypt(passDictionary[i].ToUpper(), inputString.Substring(0, Math.Min(inputString.Length, 23)));

                start = 0; length = Math.Min(maxLength, decryption.Length - start); score = 0;
                test = true;
                while (test)
                {
                    if (dictionary.ContainsKey(decryption.Substring(start, length).ToLower()))
                    {
                        score = score + length * length;
                        decryption = decryption.Substring(0, start + length) + " " + decryption.Substring(start + length);
                        start = start + length + 1;
                        length = Math.Min(maxLength, decryption.Length - start);
                    }
                    else
                    {
                        length--;
                        if (length < 3)
                        {
                            start++;
                            decryption = decryption.Substring(0, start) + " " + decryption.Substring(start);
                            start++;
                            length = Math.Min(maxLength, decryption.Length - start);
                        }
                    }
                    if (start >= decryption.Length)
                    {
                        test = false;
                    }
                }
                if (score > bestScore[bestScore.Length - 1])
                {
                    j = bestScore.Length - 2;
                    for (; j >= 0; j--)
                    {
                        if (bestScore[j] > score)
                        {
                            break;
                        }
                    }
                    if (!(j == 0 & score > bestScore[0]))
                    {
                        j++;
                    }

                    for (k = bestScore.Length - 1; k > j; k--)
                    {
                        bestScore[k] = bestScore[k - 1];
                    }

                    bestScore[j] = score;
                    updateList(i, j, score, passDictionary[i], decryption);
                }
            }

            updateDicPos(i);
            Password = passDictionary[indexList[0]];
            OutputString = decrypt(passDictionary[indexList[0]].ToUpper(), inputString);
        }

        public void updateList(int idx, int pos, int score, string pass, string dec)
        {
            for (int j = scoreList.Length - 1; j > pos; j--)
            {
                indexList[j] = indexList[j - 1];
                scoreList[j] = scoreList[j - 1];
                passList[j] = passList[j - 1];
                textList[j] = textList[j - 1];
            }
            indexList[pos] = idx;
            scoreList[pos] = Convert.ToString(score);
            passList[pos] = pass;
            textList[pos] = dec;
            ((SolitaireAnalyserQuickWatchPresentation)Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).score1.Content = "" + scoreList[0];
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).score2.Content = "" + scoreList[1];
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).score3.Content = "" + scoreList[2];
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).score4.Content = "" + scoreList[3];
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).score5.Content = "" + scoreList[4];
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).score6.Content = "" + scoreList[5];
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).score7.Content = "" + scoreList[6];
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).score8.Content = "" + scoreList[7];
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).score9.Content = "" + scoreList[8];
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).score10.Content = "" + scoreList[9];
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).pass1.Content = "" + passList[0];
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).pass2.Content = "" + passList[1];
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).pass3.Content = "" + passList[2];
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).pass4.Content = "" + passList[3];
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).pass5.Content = "" + passList[4];
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).pass6.Content = "" + passList[5];
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).pass7.Content = "" + passList[6];
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).pass8.Content = "" + passList[7];
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).pass9.Content = "" + passList[8];
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).pass10.Content = "" + passList[9];
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).text1.Content = "" + textList[0];
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).text2.Content = "" + textList[1];
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).text3.Content = "" + textList[2];
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).text4.Content = "" + textList[3];
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).text5.Content = "" + textList[4];
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).text6.Content = "" + textList[5];
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).text7.Content = "" + textList[6];
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).text8.Content = "" + textList[7];
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).text9.Content = "" + textList[8];
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).text10.Content = "" + textList[9];

            }
            , null);
        }

        public void updateDicSize(int size)
        {
            ((SolitaireAnalyserQuickWatchPresentation)Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).dicSize.Content = "" + Convert.ToString(size);
            }
            , null);
        }

        public void updateRemTime(string time)
        {
            ((SolitaireAnalyserQuickWatchPresentation)Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).remTime.Content = "" + time;
            }
            , null);
        }

        public void updateDicPos(int pos)
        {
            ((SolitaireAnalyserQuickWatchPresentation)Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).dicPos.Content = "" + Convert.ToString(pos);
            }
            , null);
        }

        public void updatePassSec(int passSec)
        {
            ((SolitaireAnalyserQuickWatchPresentation)Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).passSec.Content = "" + Convert.ToString(passSec);
            }
            , null);
        }



        public void clearScreen()
        {
            ((SolitaireAnalyserQuickWatchPresentation)Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).score1.Content = "";
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).score2.Content = "";
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).score3.Content = "";
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).score4.Content = "";
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).score5.Content = "";
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).score6.Content = "";
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).score7.Content = "";
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).score8.Content = "";
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).score9.Content = "";
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).score10.Content = "";
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).pass1.Content = "";
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).pass2.Content = "";
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).pass3.Content = "";
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).pass4.Content = "";
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).pass5.Content = "";
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).pass6.Content = "";
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).pass7.Content = "";
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).pass8.Content = "";
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).pass9.Content = "";
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).pass10.Content = "";
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).text1.Content = "";
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).text2.Content = "";
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).text3.Content = "";
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).text4.Content = "";
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).text5.Content = "";
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).text6.Content = "";
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).text7.Content = "";
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).text8.Content = "";
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).text9.Content = "";
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).text10.Content = "";
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).dicSize.Content = "";
                ((SolitaireAnalyserQuickWatchPresentation)Presentation).remTime.Content = "";
            }
            , null);
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

        #region test

        private void FormatPass(ref string msg)
        {
            msg = msg.ToUpper();
            Regex regex = new Regex("[^A-Z0-9]", RegexOptions.None);
            if (regex.IsMatch(msg))
            {
                msg = regex.Replace(msg, "");
            }
        }

        public string decrypt(string pass, string cipher)
        {
            int numberOfCards = 54;
            int[] deck = new int[numberOfCards];
            StringBuilder output = new StringBuilder("");
            for (int i = 0; i < numberOfCards; i++)
            {
                deck[i] = i + 1;
            }

            int curChar;
            FormatPass(ref pass);
            for (int i = 0; i < pass.Length; i++)
            {
                PushAndCut(ref deck, numberOfCards);
                if (Regex.IsMatch(pass.Substring(i, 1), "[A-Z]{1}"))
                {
                    curChar = pass[i] - 65;
                }
                else
                {
                    curChar = Convert.ToInt16(pass.Substring(i, 1));
                }

                CountCut(ref deck, curChar + 1, numberOfCards);
            }
            int curKey, j = 1;
            for (int i = 0; i < cipher.Length; i++)
            {
                PushAndCut(ref deck, numberOfCards);
                curKey = deck[0];
                curChar = (cipher[i] - 64);
                while (curChar == -32 & i < cipher.Length - 1)
                {
                    i++;
                    curChar = (cipher[i] - 64);
                }


                if (curKey == numberOfCards)
                {
                    curKey = deck[numberOfCards - 1];
                }
                else
                {
                    curKey = deck[curKey];
                }

                if (curChar < curKey)
                {
                    curChar += 26;
                }

                curChar = (curChar - curKey);

                if (curKey < numberOfCards - 1)
                {
                    if (curChar > 26)
                    {
                        curChar %= 26;
                    }

                    if (curChar < 1)
                    {
                        curChar += 26;
                    }

                    output.Append((char)(curChar + 64));
                    j++;
                }
                else
                {
                    i--;
                }
            }
            return output.ToString();
        }

        internal void PushAndCut(ref int[] deck, int numberOfCards)
        {
            MoveCardDown(ref deck, numberOfCards - 1, numberOfCards);
            MoveCardDown(ref deck, numberOfCards, numberOfCards);
            MoveCardDown(ref deck, numberOfCards, numberOfCards);
            TripleCut(ref deck, numberOfCards);
            CountCut(ref deck, numberOfCards);
        }

        internal void MoveCardDown(ref int[] deck, int card, int numberOfCards)
        {
            if (deck != null)
            {
                int pos = Array.IndexOf(deck, card);
                if (pos == numberOfCards - 1)
                {
                    BottomToTop(ref deck, numberOfCards);
                    MoveCardDown(ref deck, card, numberOfCards);
                }
                else
                {
                    deck[pos] = deck[pos + 1];
                    deck[pos + 1] = card;
                }
            }
        }

        internal void BottomToTop(ref int[] deck, int numberOfCards)
        {
            int card = deck[numberOfCards - 1];
            for (int i = numberOfCards - 1; i > 0; i--)
            {
                deck[i] = deck[i - 1];
            }

            deck[0] = card;
        }

        internal void TripleCut(ref int[] deck, int numberOfCards)
        {
            int jokerTop = Math.Min(Array.IndexOf(deck, numberOfCards - 1), Array.IndexOf(deck, numberOfCards));
            int jokerBottom = Math.Max(Array.IndexOf(deck, numberOfCards - 1), Array.IndexOf(deck, numberOfCards));

            int[] newDeck = new int[numberOfCards];
            int lengthBottom = numberOfCards - 1 - jokerBottom;
            int lengthMiddle = jokerBottom - jokerTop - 1;

            Array.Copy(deck, jokerBottom + 1, newDeck, 0, lengthBottom);
            Array.Copy(deck, jokerTop, newDeck, lengthBottom, lengthMiddle + 2);
            Array.Copy(deck, 0, newDeck, lengthBottom + lengthMiddle + 2, jokerTop);

            newDeck.CopyTo(deck, 0);
        }

        internal void CountCut(ref int[] deck, int cutPos, int numberOfCards)
        {
            int[] newDeck = new int[numberOfCards];
            if (cutPos < numberOfCards - 1)
            {
                Array.Copy(deck, cutPos, newDeck, 0, numberOfCards - 1 - (cutPos));
                Array.Copy(deck, 0, newDeck, numberOfCards - 1 - (cutPos), cutPos);
                newDeck[numberOfCards - 1] = deck[numberOfCards - 1];
                newDeck.CopyTo(deck, 0);
            }
        }

        internal void CountCut(ref int[] deck, int numberOfCards)
        {
            CountCut(ref deck, deck[numberOfCards - 1], numberOfCards);
        }

        #endregion

    }
}
