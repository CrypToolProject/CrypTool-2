/*
   Copyright 2011 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace CrypTool.Plugins.SpanishStripCipher
{
    public class SpanishStripCipherSettings : ISettings
    {
        #region Public Variables

        public enum CipherMode { Encrypt = 0, Decrypt = 1 };
        /// <summary>
        /// We use this delegate to send log messages from the settings class to the SpanishStripCipher plugin
        /// </summary>
        public delegate void SpanishStripCipherLogMessage(string msg, NotificationLevel loglevel);
        public event SpanishStripCipherLogMessage LogMessage;

        #endregion 

        #region Private Variables

        private CipherMode selectedAction = CipherMode.Encrypt;
        private int selectedHomophonesTables = 0;
        private int selectedAlphabet = 0;
        private int selectedLetter1 = 0;
        private int selectedLetter2 = 0;
        private int selectedNumberLetter = 0;
        private string selectedLetter1String = "A";
        private string selectedLetter2String = "A";
        private static readonly string[] alpha = new string[] { "A", "B" };
        private string orderedAlphabet = "ABCDEFGHIJKLMNÑOPQRSTUVWXYZ";
        private readonly string orderedAlphabet27Letters = "ABCDEFGHIJKLMNÑOPQRSTUVWXYZ";
        private readonly string orderedAlphabet29Letters = "ABCßDEFGHIJKLÄMNÑOPQRSTUVWXYZ"; // ß<-CH and Ä<-LL encoded 
        private readonly string orderedAlphabet27LettersPanel = "A B C D E F G H I J K L M N Ñ O P Q R S T U V W X Y Z";
        private readonly string orderedAlphabet29LettersPanel = "A B C CH D E F G H I J K L LL M N Ñ O P Q R S T U V W X Y Z"; // ß<-CH and Ä<-LL encoded  
        private string orderedAlphabetPanel = "A B C D E F G H I J K L M N Ñ O P Q R S T U V W X Y Z";
        public string unorderedAlphabet = " ";
        private string unorderedAlphabetPanel = " ";
        private string keyword = "";
        private readonly List<List<string>> homophones = new List<List<string>>();
        private readonly List<string> numbersRandomTable = "01 02 03 04 05 06 07 08 09 11 22 33 44 55 66 77 88 99 10 12 23 34 93 94 45 56 67 78 89 20 21 13 24 25 26 27 28 29 30 31 32 14 35 36 37 38 39 40 70 71 72 41 42 43 15 46 47 48 49 50 51 52 53 54 16 57 58 59 60 61 62 63 64 65 17 68 69 95 96 97 98 73 74 75 76 18 79 80 81 82 83 84 85 86 87 19 90 91 92".Split(' ').ToList();
        private readonly List<string> numbersTableOne = "10 37 61 81 12 56 99 20 44 55 82 32 54 77 36 45 60 95 30 59 68 86 11 38 78 21 53 62 18 46 75 88 31 74 80 17 39 57 96 23 63 83 13 47 76 97 33 64 94 19 40 87 22 65 58 28 48 73 15 51 93 26 49 85 16 41 89 24 66 72 29 50 90 34 42 84 25 67 71 92 35 70 98 27 52 79 14 43 69 91".Split(' ').ToList();
        private readonly List<string> numbersTableTwo = "10 42 57 97 23 51 75 99 07 41 65 87 14 45 68 93 01 40 69 78 28 55 74 06 30 63 92 02 32 66 17 34 62 11 29 60 98 18 26 54 84 09 35 53 90 08 38 64 91 13 44 72 19 43 86 22 48 82 15 36 71 96 05 33 61 81 27 59 73 94 16 39 88 24 52 70 89 21 49 67 85 04 31 50 79 20 37 56 80 12 46 77 95 00 25 58 76 03 47 83".Split(' ').ToList();
        private readonly List<int> numbersPerColRandomTableAlphabet27 = new List<int>() { 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 };
        private readonly List<int> numbersPerColTableOneAlphabet27 = new List<int>() { 4, 3, 4, 3, 4, 4, 3, 3, 4, 3, 4, 3, 4, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 3, 3, 4 };
        private readonly List<int> numbersPerColTableTwoAlphabet27 = new List<int>() { 4, 4, 4, 4, 4, 3, 4, 3, 3, 4, 4, 4, 4, 3, 3, 3, 4, 4, 4, 3, 4, 4, 4, 4, 4, 4, 3 };
        private readonly List<int> numbersPerColRandomTableAlphabet29 = new List<int>() { 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 };
        private readonly List<int> numbersPerColTableOneAlphabet29 = new List<int>() { 3, 4, 3, 3, 4, 3, 4, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3 };
        private readonly List<int> numbersPerColTableTwoAlphabet29 = new List<int>() { 3, 4, 3, 4, 4, 3, 4, 3, 4, 3, 3, 3, 3, 4, 4, 3, 4, 3, 3, 3, 4, 3, 4, 3, 3, 3, 3, 4, 4 };
        private string homophoneString = "";
        private int homophoneSelection = 0; // 0=random, 1=round robin
        private bool flag = true;
        private readonly Random rand = new Random();

        #endregion

        #region functions

        public List<List<string>> getHomophones()
        {
            return homophones;
        }

        public Dictionary<string, string> Number2Char
        {
            get
            {
                Dictionary<string, string> number2char = new Dictionary<string, string>();

                for (int i = 0; i < homophones.Count; i++)
                {
                    foreach (string n in homophones[i])
                    {
                        number2char[n] = unmapDigraphs(unorderedAlphabet[i]);
                    }
                }

                return number2char;
            }
        }

        public void GenerateHomophoneTable()
        {
            List<string> numbers;
            List<int> numbersPerCol;

            switch (selectedHomophonesTables)
            {
                case 1: // 90 homophones (10-99)
                    numbers = numbersTableOne;
                    numbersPerCol = (selectedAlphabet == 0) ? numbersPerColTableOneAlphabet27 : numbersPerColTableOneAlphabet29;
                    break;
                case 2: // 99 homophones (01-99)
                    numbers = numbersTableTwo;
                    numbersPerCol = (selectedAlphabet == 0) ? numbersPerColTableTwoAlphabet27 : numbersPerColTableTwoAlphabet29;
                    break;
                default: // Random homophones table
                    numbers = ShuffleArrayList(numbersRandomTable);
                    numbersPerCol = (selectedAlphabet == 0) ? ShuffleArrayList(numbersPerColRandomTableAlphabet27) : ShuffleArrayList(numbersPerColRandomTableAlphabet29);
                    break;
            }

            homophones.Clear();
            for (int i = 0, j = 0; i < orderedAlphabet.Length; j += numbersPerCol[i++])
            {
                homophones.Add(numbers.Skip(j).Take(numbersPerCol[i]).ToList());
            }
        }

        public void GenerateRandomAlphabet()
        {
            unorderedAlphabet = "";

            //remove repeated letters from keyword
            foreach (char c in mapDigraphs(keyword))
            {
                if (unorderedAlphabet.IndexOf(c) < 0 && orderedAlphabet.IndexOf(c) >= 0)
                {
                    unorderedAlphabet += c;
                }
            }

            int keywordWithoutRepeatedLettersLength = unorderedAlphabet.Length;
            if (keywordWithoutRepeatedLettersLength > 0)
            {
                //remove repeated letters from ordered alphabet
                foreach (char c in orderedAlphabet)
                {
                    if (unorderedAlphabet.IndexOf(c) < 0)
                    {
                        unorderedAlphabet += c;
                    }
                }

                string unorderedAlphabetFinal = "";
                int col = 0;
                for (int i = 0, j = 0; i < unorderedAlphabet.Length; i++)
                {
                    unorderedAlphabetFinal += unorderedAlphabet[j];
                    j += keywordWithoutRepeatedLettersLength;
                    if (j >= unorderedAlphabet.Length)
                    {
                        j = ++col;
                    }
                }

                int unorderedIndex = unorderedAlphabetFinal.IndexOf(orderedAlphabet[selectedLetter1]);
                int orderedIndex = orderedAlphabet.IndexOf(orderedAlphabet[selectedLetter2]);
                int shift = (unorderedIndex - orderedIndex + orderedAlphabet.Length) % orderedAlphabet.Length;
                unorderedAlphabet = unorderedAlphabetFinal.Substring(shift) + unorderedAlphabetFinal.Substring(0, shift);

                if (flag)
                {
                    GenerateHomophoneTable();
                    flag = false;
                }

                unorderedAlphabetPanel = string.Join(" ", unorderedAlphabet.Select(c => unmapDigraphs(c)));
                homophoneString = string.Join(", ", unorderedAlphabet.Select((c, i) => unmapDigraphs(c) + "=(" + string.Join(" ", homophones[i]) + ")"));
            }
            else
            {
                unorderedAlphabetPanel = "";
                homophoneString = "";
            }

            OnPropertyChanged("Homophones");
            OnPropertyChanged("UnorderedAlphabetPanel");
            OnPropertyChanged("Keyword");
        }

        public string unmapDigraphs(char c)
        {
            if (Alphabets == 1)
            {
                if (c == 'Ä')
                {
                    return "LL";
                }

                if (c == 'ß')
                {
                    return "CH";
                }
            }
            return c + "";
        }

        public string mapDigraphs(string s)
        {
            if (Alphabets == 1)
            {
                s = Regex.Replace(s, "CH", "ß");
                s = Regex.Replace(s, "LL", "Ä");
            }
            return s;
        }

        public List<T> ShuffleArrayList<T>(List<T> source)
        {
            return source.OrderBy(c => rand.Next()).ToList();
        }

        public bool checkInitialPositionLetter(string letter)
        {
            int index = orderedAlphabet.IndexOf(mapDigraphs(letter));
            if (index == -1)
            {
                return false;
            }

            selectedNumberLetter = index;
            return true;

            //if (letter == "CH" && orderedAlphabet.Length == 29)
            //{
            //    letter = "ß";
            //    result = true;
            //}
            //else if (letter == "LL" && orderedAlphabet.Length == 29)
            //{
            //    letter = "Ä";
            //    result = true;
            //}
            //else if (this.orderedAlphabet.IndexOf(letter) != -1) 
            //{
            //    result = true;
            //}
            //if (true)
            //{
            //    selectedNumberLetter = orderedAlphabet.IndexOf(letter); 
            //}
            //return result;
        }

        #endregion

        #region TaskPane Settings

        [TaskPane("ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
        public CipherMode Action
        {
            get => selectedAction;
            set
            {
                if (value != selectedAction)
                {
                    selectedAction = value;
                    OnPropertyChanged("Action");
                }
            }
        }

        [TaskPane("AlphabetsCaption", "AlphabetsTooltip", null, 2, false, ControlType.ComboBox, new string[] { "AlphabetsList1", "AlphabetsList2" })]
        public int Alphabets
        {
            get => selectedAlphabet;
            set
            {
                if (value != selectedAlphabet)
                {
                    selectedAlphabet = value;
                    if (selectedAlphabet == 0)
                    {
                        orderedAlphabetPanel = orderedAlphabet27LettersPanel;
                        orderedAlphabet = orderedAlphabet27Letters;
                    }
                    else
                    {
                        orderedAlphabetPanel = orderedAlphabet29LettersPanel;
                        orderedAlphabet = orderedAlphabet29Letters;
                    }
                    if (!string.IsNullOrEmpty(Keyword))
                    {
                        flag = true;
                        GenerateRandomAlphabet();
                    }
                    Position1 = "A";
                    Position2 = "A";
                    OnPropertyChanged("OrderedAlphabet");
                    // OnPropertyChanged("UnorderedAlphabetPanel");
                    OnPropertyChanged("Alphabets");
                }
            }
        }

        [TaskPane("HomophonesTablesCaption", "HomophonesTablesTooltip", null, 2, false, ControlType.ComboBox, new string[] { "HomophonesTablesList1", "HomophonesTablesList2", "HomophonesTablesList3" })]
        public int HomophonesTables
        {
            get => selectedHomophonesTables;
            set
            {
                if (value != selectedHomophonesTables)
                {
                    selectedHomophonesTables = value;
                    OnPropertyChanged("HomophonesTables");
                    if (!string.IsNullOrEmpty(Keyword))
                    {
                        flag = true;
                        GenerateRandomAlphabet();
                    }
                }
            }
        }

        [TaskPane("KeywordCaption", "KeywordTooltip", null, 3, false, ControlType.TextBox, "")]
        public string Keyword
        {
            get => keyword;
            set
            {
                if (keyword != value.ToUpper())
                {
                    keyword = value.ToUpper();
                    GenerateRandomAlphabet();
                }
            }
        }

        [TaskPane("Position1Caption", "Position1Tooltip", null, 4, false, ControlType.TextBox)]
        public string Position1
        {
            get => selectedLetter1String;
            set
            {
                value = value.ToUpper();
                if (value != selectedLetter1String)
                {
                    if (checkInitialPositionLetter(value))
                    {
                        selectedLetter1String = value;
                        selectedLetter1 = selectedNumberLetter;
                    }
                    else
                    {
                        OnLogMessage(value + " is not a valid entry. Please enter only letters of the selected alphabet. " + value + " was replaced by the letter A.", NotificationLevel.Error);
                        selectedLetter1String = "A";
                        selectedLetter1 = 0;
                    }
                    OnPropertyChanged("Position1");
                    GenerateRandomAlphabet();
                }
            }
        }

        [TaskPane("Position2Caption", "Position2Tooltip", null, 5, false, ControlType.TextBox)]
        public string Position2
        {
            get => selectedLetter2String;
            set
            {
                value = value.ToUpper();
                if (value != selectedLetter2String)
                {
                    selectedLetter2String = "";
                    if (checkInitialPositionLetter(value))
                    {
                        selectedLetter2String = value;
                        selectedLetter2 = selectedNumberLetter;
                    }
                    else
                    {
                        OnLogMessage(value + " is not a valid entry. Please enter only letters of the selected alphabet. " + value + " was replaced by the letter A.", NotificationLevel.Error);
                        selectedLetter2String = "A";
                        selectedLetter2 = 0;
                    }
                    OnPropertyChanged("Position2");
                    GenerateRandomAlphabet();
                }
            }
        }

        [TaskPane("HomophoneSelectionCaption", "HomophoneSelectionTooltip", null, 6, false, ControlType.ComboBox, new string[] { "HomophoneSelectionList1", "HomophoneSelectionList2" })]
        public int HomophoneSelection
        {
            get => homophoneSelection;
            set
            {
                if (value != homophoneSelection)
                {
                    homophoneSelection = value;
                    OnPropertyChanged("HomophoneSelection");
                }
            }
        }

        [TaskPane("OrderedAlphabetCaption", "OrderedAlphabetTooltip", null, 7, false, ControlType.TextBoxReadOnly, "")]
        public string OrderedAlphabet
        {
            get => orderedAlphabetPanel;
            set
            {
                if (orderedAlphabetPanel != value)
                {
                    orderedAlphabetPanel = value;
                    OnPropertyChanged("OrderedAlphabet");
                }
            }
        }

        [TaskPane("UnorderedAlphabetPanelCaption", "UnorderedAlphabetPanelTooltip", null, 8, false, ControlType.TextBoxReadOnly)]
        public string UnorderedAlphabetPanel
        {
            get => unorderedAlphabetPanel;
            set
            {
                if (unorderedAlphabetPanel != value)
                {
                    //for (int i = 0; i < keyword.Length; i++)
                    //{
                    //  string charKey = keyword[i].ToString();
                    //if (unorderedAlphabet.Contains(charKey) == false)
                    //{
                    //unorderedAlphabet = unorderedAlphabet + charKey;
                    //}

                    //}                   
                    OnPropertyChanged("UnorderedAlphabetPanel");
                }
            }
        }

        [TaskPane("HomophonesCaption", "HomophonesTooltip", null, 9, false, ControlType.TextBoxReadOnly)]
        public string Homophones
        {
            get => homophoneString;
            set
            {
                if (homophoneString != value)
                {
                    OnPropertyChanged("Homophones");
                }
            }
        }

        #endregion

        #region

        private void OnLogMessage(string msg, NotificationLevel level)
        {
            if (LogMessage != null)
            {
                LogMessage(msg, level);
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }
        #endregion
    }
}