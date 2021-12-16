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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Solitaire
{
    public delegate void DelegateFunction();

    [Author("Coen Ramaekers", "cfwramaekers@gmail.com", "Technische Universiteit Eindhoven", "http://www.win.tue.nl")]
    [PluginInfo("Solitaire.Properties.Resources", "PluginCaption", "PluginTooltip", "Solitaire/DetailedDescription/doc.xml", "Solitaire/solitaire.jpg")]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class Solitaire : ICrypComponent
    {
        #region Private Variables

        private SolitaireSettings settings;

        private readonly SolitaireQuickWatchPresentation myPresentation;

        private string inputString, outputString, outputStream, password, deckstate, initialDeck, finalDeck;

        private readonly StringBuilder output, stream, sb;

        private bool isPlayMode = false;

        private int numberOfCards;

        private int[] deck, newDeck;

        internal enum CipherMode { encrypt, decrypt };

        #endregion

        #region Data Properties

        public Solitaire()
        {
            output = new StringBuilder("");
            stream = new StringBuilder("");
            sb = new StringBuilder(152);
            settings = new SolitaireSettings();
            myPresentation = new SolitaireQuickWatchPresentation(this);
            Presentation = myPresentation;
        }

        /// <summary>
        /// Read the text which is to be encrypted or decrypted.
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputStringCaption", "InputStringTooltip", false)]
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
        /// Read the password with which the deckstate is generated.
        /// </summary>
        [PropertyInfo(Direction.InputData, "PasswordCaption", "PasswordTooltip", false)]
        public string Password
        {
            get => password;
            set
            {
                if (value != Password)
                {
                    password = value;
                    OnPropertyChanged("Password");
                }
            }
        }

        /// <summary>
        /// Read a given deckstate.
        /// </summary>
        [PropertyInfo(Direction.InputData, "DeckstateCaption", "DeckstateTooltip", false)]
        public string Deckstate
        {
            get => deckstate;
            set
            {
                if (value != Deckstate)
                {
                    deckstate = value;
                    OnPropertyChanged("Deckstate");
                }
            }
        }


        /// <summary>
        /// Outputs the encrypted or decrypted text.
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
        /// Displays the initial deck.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "InitialDeckCaption", "InitialDeckTooltip", false)]
        public string InitialDeck
        {
            get => initialDeck;
            set
            {
                initialDeck = value;
                OnPropertyChanged("InitialDeck");
            }
        }


        /// <summary>
        /// Displays the final deck.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "FinalDeckCaption", "FinalDeckTooltip", false)]
        public string FinalDeck
        {
            get => finalDeck;
            set
            {
                finalDeck = value;
                OnPropertyChanged("FinalDeck");
            }
        }

        /// <summary>
        /// Outputs the stream used to encrypt or decrypt.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputStreamCaption", "OutputStreamTooltip", false)]
        public string OutputStream
        {
            get => outputStream;
            set
            {
                outputStream = value;
                OnPropertyChanged("OutputStream");
            }
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings
        {
            get => settings;
            set => settings = (SolitaireSettings)value;
        }

        public UserControl Presentation
        {
            get;
            private set;
        }

        public void PreExecution()
        {
            myPresentation.Dispatcher.Invoke(new DelegateFunction(myPresentation.clear), null);
        }

        public void Execute()
        {
            if (inputString == null)
            {
                return;
            }

            isPlayMode = true;
            if (settings.ActionType == 0)
            {
                GuiLogMessage("Encrypting", NotificationLevel.Debug);
                if (settings.StreamType == 0)
                {
                    SolitaireCipher(CipherMode.encrypt, true);
                }

                if (settings.StreamType == 1)
                {
                    SolitaireManual(0);
                }
            }
            else
            {
                GuiLogMessage("Decrypting", NotificationLevel.Debug);
                if (settings.StreamType == 0)
                {
                    SolitaireCipher(CipherMode.decrypt, true);
                }

                if (settings.StreamType == 1)
                {
                    SolitaireManual(1);
                }
            }
        }

        public void PostExecution()
        {
            isPlayMode = false;
        }

        public void Stop()
        {
            myPresentation.Dispatcher.Invoke(new DelegateFunction(myPresentation.stop), null);
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

        #region Private Methods

        private void generateDeck(int type)
        {
            switch (type)
            {
                case 0: //Ascending
                    KeyTheDeckAsc(settings.NumberOfCards);
                    break;

                case 1: //Descending
                    KeyTheDeckDesc(settings.NumberOfCards);
                    break;

                case 2: //Given state
                    if (deckstate != null)
                    {
                        KeyTheDeckSequence(deckstate);
                    }
                    else
                    {
                        GuiLogMessage("Given deckstate missing!", NotificationLevel.Error);
                    }

                    break;

                case 3: //Keyword
                    if (password != null)
                    {
                        KeyTheDeckPassword(password, settings.NumberOfCards);
                    }
                    else
                    {
                        GuiLogMessage("Keyword missing!", NotificationLevel.Error);
                    }

                    break;

                case 4: //Random
                    KeyTheDeckRandom(settings.NumberOfCards);
                    break;
            }

            numberOfCards = deck.Length;
        }

        private void SolitaireManual(int mode)
        {
            GuiLogMessage("Input: " + inputString, NotificationLevel.Debug);
            generateDeck(settings.GenerationType);

            if (deck != null)
            {
                myPresentation.enable(numberOfCards, mode);
            }
        }

        public void SolitaireCipher(int mode, bool separator)
        {
            if (mode == 0)
            {
                SolitaireCipher(CipherMode.encrypt, separator);
            }
            else
            {
                SolitaireCipher(CipherMode.decrypt, separator);
            }
        }

        private void SolitaireCipher(CipherMode mode, bool separator)
        {
            output.Clear();
            stream.Clear();

            FormatText(ref inputString);
            generateDeck(settings.GenerationType);

            if (deck != null)
            {
                for (int i = 0; i < inputString.Length; i++)
                {
                    PushAndCut(numberOfCards);
                    int curKey = GetKey(numberOfCards);
                    char curChar = EncryptChar(mode, inputString[i], curKey, numberOfCards);

                    stream.Append(Convert.ToString(curKey));
                    output.Append(curChar);

                    if (i != inputString.Length - 1)
                    {
                        stream.Append(",");
                    }

                    if (i % 5 == 4 & separator)
                    {
                        output.Append(" ");
                    }

                    if (separator)
                    {
                        ProgressChanged(i, inputString.Length - 1);
                    }
                }

                outputString = output.ToString();
                outputStream = stream.ToString();
                finalDeck = GetDeck(numberOfCards);
                OnPropertyChanged("FinalDeck");
                OnPropertyChanged("OutputString");
                OnPropertyChanged("OutputStream");
                OnPropertyChanged("OutputData");
            }

        }

        private int GetNextKey(int numberOfCards)
        {
            int key = deck[0];
            if (key == numberOfCards)
            {
                key--;
            }

            return deck[key];
        }

        internal int GetKey(int numberOfCards)
        {
            while (true)
            {
                int key = GetNextKey(numberOfCards);
                if (key < numberOfCards - 1)
                {
                    return key;
                }

                PushAndCut(numberOfCards);
            }
        }

        internal char EncryptChar(CipherMode mode, char c, int key, int numberOfCards)
        {
            key = (key - 1) % 26 + 1;
            key = (mode == CipherMode.encrypt) ? key : (26 - key);

            return (char)((c - 'A' + key) % 26 + 'A');
        }

        public string GetDeck(int numberOfCards)
        {
            sb.Clear();
            for (int i = 0; i < numberOfCards; i++)
            {
                sb.Append((deck[i] == numberOfCards - 1) ? "A" : ((deck[i] == numberOfCards) ? "B" : deck[i].ToString()));
                if (i != numberOfCards - 1)
                {
                    sb.Append(",");
                }
            }
            return sb.ToString();
        }

        internal int[] GetDeck()
        {
            return deck;
        }

        public void FormatText(ref string msg)
        {
            msg = msg.ToUpper();
            Regex regex = new Regex("[^A-Z]", RegexOptions.None);
            if (regex.IsMatch(msg))
            {
                msg = regex.Replace(msg, "");
            }

            while (msg.Length % 5 != 0)
            {
                msg = msg + "X";
            }
        }

        private void FormatPass(ref string msg)
        {
            msg = msg.ToUpper();
            Regex regex = new Regex("[^A-Z0-9]", RegexOptions.None);
            if (regex.IsMatch(msg))
            {
                msg = regex.Replace(msg, "");
            }
        }

        private void KeyTheDeckPassword(string pass, int numberOfCards)
        {
            deck = new int[numberOfCards];
            for (int i = 0; i < numberOfCards; i++)
            {
                deck[i] = i + 1;
            }

            FormatPass(ref pass);
            int curChar;
            for (int i = 0; i < pass.Length; i++)
            {
                PushAndCut(numberOfCards);
                if (Regex.IsMatch(pass.Substring(i, 1), "[A-Z]{1}"))
                {
                    curChar = pass[i] - 65;
                }
                else
                {
                    curChar = Convert.ToInt16(pass.Substring(i, 1));
                }

                CountCut(curChar + 1, numberOfCards);
            }
            initialDeck = GetDeck(numberOfCards);
            OnPropertyChanged("InitialDeck");
        }

        private void KeyTheDeckSequence(string seq)
        {
            try
            {
                //  sanity check of input
                seq = seq.ToUpper();
                seq = new Regex("\\s+").Replace(seq, "");

                string[] sequence = seq.Split(',');
                int numberOfCards = sequence.Length;

                HashSet<string> set = new HashSet<string>(sequence);
                if (numberOfCards > 54)
                {
                    throw new Exception("Too many cards (>54)");
                }

                if (numberOfCards < 3)
                {
                    throw new Exception("Too few cards (<3)");
                }

                if (set.Contains(""))
                {
                    throw new Exception("Sequence contains empty values");
                }

                if (set.Contains("A") ^ set.Contains("B"))
                {
                    throw new Exception("Sequence contains only one of A and B");
                }

                if (set.Contains("A"))  // replace A and B by their numerical values
                {
                    for (int i = 0; i < numberOfCards; i++)
                    {
                        if (sequence[i].Equals("A"))
                        {
                            sequence[i] = (numberOfCards - 1).ToString();
                        }
                        else if (sequence[i].Equals("B"))
                        {
                            sequence[i] = numberOfCards.ToString();
                        }
                    }
                    set = new HashSet<string>(sequence);
                }
                if (set.Count < numberOfCards)
                {
                    throw new Exception("Sequence contains duplicates");
                }

                deck = new int[numberOfCards];
                for (int i = 0; i < numberOfCards; i++)
                {
                    deck[i] = int.Parse(sequence[i]);
                    if (deck[i] > numberOfCards)
                    {
                        throw new Exception("Sequence value too big: " + deck[i]);
                    }

                    if (deck[i] < 1)
                    {
                        throw new Exception("Sequence value too small: " + deck[i]);
                    }
                }

                // check if all values from 1 to numberOfCards are present
                HashSet<int> set2 = new HashSet<int>(deck);
                for (int i = 1; i <= numberOfCards; i++)
                {
                    if (!set2.Contains(i))
                    {
                        throw new Exception("Missing value in sequence: " + i);
                    }
                }

                initialDeck = GetDeck(deck.Length);
                OnPropertyChanged("InitialDeck");
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
        }

        private void KeyTheDeckAsc(int numberOfCards)
        {
            deck = new int[numberOfCards];
            for (int i = 0; i < numberOfCards; i++)
            {
                deck[i] = i + 1;
            }

            initialDeck = GetDeck(numberOfCards);
            OnPropertyChanged("InitialDeck");
        }

        private void KeyTheDeckDesc(int numberOfCards)
        {
            deck = new int[numberOfCards];
            for (int i = 0; i < numberOfCards; i++)
            {
                deck[i] = numberOfCards - i;
            }

            initialDeck = GetDeck(numberOfCards);
            OnPropertyChanged("InitialDeck");
        }

        private void KeyTheDeckRandom(int numberOfCards)
        {
            deck = new int[numberOfCards];
            ArrayList choices = new ArrayList();
            for (int i = 0; i < numberOfCards; i++)
            {
                choices.Add(i + 1);
            }

            Random r = new Random();
            int randomIndex = 0;
            while (choices.Count > 0)
            {
                randomIndex = r.Next(0, choices.Count);
                deck[choices.Count - 1] = (int)choices[randomIndex];
                choices.RemoveAt(randomIndex);
            }
            initialDeck = GetDeck(numberOfCards);
            OnPropertyChanged("InitialDeck");
        }

        internal void PushAndCut(int numberOfCards)
        {
            MoveCardDown(numberOfCards - 1, numberOfCards);
            MoveCardDown(numberOfCards, numberOfCards);
            MoveCardDown(numberOfCards, numberOfCards);
            TripleCut(numberOfCards);
            CountCut(numberOfCards);
        }

        internal void InversePushAndCut(int numberOfCards)
        {
            InverseCountCut(numberOfCards);
            InverseTripleCut(numberOfCards);
            MoveCardUp(numberOfCards, numberOfCards);
            MoveCardUp(numberOfCards, numberOfCards);
            MoveCardUp(numberOfCards - 1, numberOfCards);
        }

        internal void MoveCardDown(int card, int numberOfCards)
        {
            if (deck != null)
            {
                int pos = Array.IndexOf(deck, card);
                if (pos == numberOfCards - 1)
                {
                    BottomToTop(numberOfCards);
                    MoveCardDown(card, numberOfCards);
                }
                else
                {
                    deck[pos] = deck[pos + 1];
                    deck[pos + 1] = card;
                }
            }
        }

        internal void MoveCardUp(int card, int numberOfCards)
        {
            int pos = Array.IndexOf(deck, card);
            if (pos == 0)
            {
                TopToBottom(numberOfCards);
                MoveCardUp(card, numberOfCards);
            }
            else
            {
                deck[pos] = deck[pos - 1];
                deck[pos - 1] = card;
            }
        }

        internal void BottomToTop(int numberOfCards)
        {
            int card = deck[numberOfCards - 1];
            for (int i = numberOfCards - 1; i > 0; i--)
            {
                deck[i] = deck[i - 1];
            }

            deck[0] = card;
        }

        internal void TopToBottom(int numberOfCards)
        {
            int card = deck[0];
            for (int i = 0; i < numberOfCards; i++)
            {
                deck[i] = deck[i + 1];
            }

            deck[numberOfCards] = card;
        }

        internal void TripleCut(int numberOfCards)
        {
            int jokerTop = Math.Min(Array.IndexOf(deck, numberOfCards - 1), Array.IndexOf(deck, numberOfCards));
            int jokerBottom = Math.Max(Array.IndexOf(deck, numberOfCards - 1), Array.IndexOf(deck, numberOfCards));

            newDeck = new int[numberOfCards];
            int lengthBottom = numberOfCards - 1 - jokerBottom;
            int lengthMiddle = jokerBottom - jokerTop - 1;

            Array.Copy(deck, jokerBottom + 1, newDeck, 0, lengthBottom);
            Array.Copy(deck, jokerTop, newDeck, lengthBottom, lengthMiddle + 2);
            Array.Copy(deck, 0, newDeck, lengthBottom + lengthMiddle + 2, jokerTop);

            newDeck.CopyTo(deck, 0);
        }

        internal void InverseTripleCut(int numberOfCards)
        {
            TripleCut(numberOfCards);
        }

        internal void CountCut(int cutPos, int numberOfCards)
        {
            newDeck = new int[numberOfCards];
            if (cutPos < numberOfCards - 1)
            {
                Array.Copy(deck, cutPos, newDeck, 0, numberOfCards - 1 - (cutPos));
                Array.Copy(deck, 0, newDeck, numberOfCards - 1 - (cutPos), cutPos);
                newDeck[numberOfCards - 1] = deck[numberOfCards - 1];
                newDeck.CopyTo(deck, 0);
            }
        }

        internal void InverseCountCut(int cutPos, int numberOfCards)
        {
            newDeck = new int[numberOfCards];
            if (cutPos < numberOfCards - 1)
            {
                Array.Copy(deck, 0, newDeck, cutPos, numberOfCards - 1 - cutPos);
                Array.Copy(deck, numberOfCards - 1 - cutPos, newDeck, 0, cutPos);
                newDeck[numberOfCards - 1] = deck[numberOfCards - 1];
                newDeck.CopyTo(deck, 0);
            }
        }

        internal void CountCut(int numberOfCards)
        {
            CountCut(deck[numberOfCards - 1], numberOfCards);
        }

        internal void InverseCountCut(int numberOfCards)
        {
            InverseCountCut(deck[numberOfCards - 1], numberOfCards);
        }

        public void changeSettings(string setting, object value)
        {
            if (setting.Equals("Action Type"))
            {
                settings.ActionType = (int)value;
            }
            else if (setting.Equals("Cards"))
            {
                settings.NumberOfCards = (int)value;
            }
            else if (setting.Equals("Deck Generation"))
            {
                settings.GenerationType = (int)value;
            }
            else if (setting.Equals("Stream Generation"))
            {
                settings.StreamType = (int)value;
            }
        }

        #endregion
    }
}

