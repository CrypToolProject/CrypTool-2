/*                              
   Copyright 2009-2010 Fabian Enkler, Matthäus Wander

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


using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;
using CrypTool.PluginBase;
using System.Diagnostics;
using CrypTool.PluginBase.Miscellaneous;
using System.Resources;

namespace Nihilist
{
    [Author("Fabian Enkler", "enkler@CrypTool.org", "", "")]
    [PluginInfo("Nihilist.Properties.Resources", "PluginCaption", "PluginTooltip", "Nihilist/DetailedDescription/doc.xml", "Nihilist/Images/icon3.png")]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class Nihilist : ICrypComponent
    {
        public const string ALPHABET = "abcdefghiklmnopqrstuvwxyz";

        private readonly NihilistSettings settings = new NihilistSettings();

        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public ISettings Settings
        {
            get { return settings; }
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        private byte[] input = new byte[] { };
        [PropertyInfo(Direction.InputData, "InputCaption", "InputTooltip", true)]
        public byte[] Input
        {
            get { return input; }
            set
            {
                input = value;
                OnPropertyChanged("Input");
            }
        }

        private byte[] output = new byte[] { };
        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip", true)]
        public byte[] Output
        {
            get { return output; }
            set
            {
                output = value;
                OnPropertyChanged("Output");
            }
        }

        public void PreExecution()
        {

        }

        // Convert byte[] indexes (0..4, 0..4) to two-digit number (11..55)
        // 0, 0 -> 11
        // 4, 1 -> 52
        private byte ConvertIndexesToTwoDigit(byte[] arr)
        {
            Debug.Assert(arr.Length == 2);
            return ConvertToDimensionOne(arr[0], arr[1]);
        }

        private byte ConvertToDimensionOne(byte x, byte y)
        {
            Debug.Assert(0 <= x && x <= 4);
            Debug.Assert(0 <= y && y <= 4);

            x++;
            y++;
            return (byte) (x * 10 + y);
        }

        // Convert two-digit number (11..55) to byte[] indexes (0..4, 0..4)
        // 11 -> 0, 0
        // 52 -> 4, 1
        private byte[] ConvertTwoDigitToIndexes(byte z)
        {
            Debug.Assert(11 <= z && z <= 55, "Expected 11..55, got: " + z);

            byte x = (byte)(z / 10);
            byte y = (byte)(z % 10);
            x--;
            y--;

            return new byte[] { x, y };
        }

        public void Execute()
        {
            // this is for localization
            ResourceManager resourceManager = new ResourceManager("Nihilist.Properties.Resources", GetType().Assembly);
            
            // flomar, 09/23/2011: make sure we have two valid keywords (plugin would crash without two valid keywords)
            if (settings.KeyWord.Length == 0 || settings.SecondKeyWord.Length == 0)
            {
                GuiLogMessage(resourceManager.GetString("ErrorInputKeywordsNotProvided"), NotificationLevel.Error);
                return;
            }

            // flomar, 09/23/2011: proceed with the old code as usual...
            char[,] KeyArray;
            Dictionary<char, byte[]> CryptMatrix = CreateCryptMatrix(out KeyArray);

            string secondKeyWord = settings.SecondKeyWord.ToLower();
            byte[] secondKeyNumbers = new byte[secondKeyWord.Length];
            for(int i = 0; i < secondKeyWord.Length; i++)
            {
                char c = secondKeyWord[i];
                secondKeyNumbers[i] = ConvertIndexesToTwoDigit(CryptMatrix[c]);
            }
            if (settings.Action == 0)
            {
                string rawInputString = ByteArrayToString(input).ToLower();
                string inputString = StringUtil.StripUnknownSymbols(ALPHABET, rawInputString);
                byte[] outputBytes = new byte[inputString.Length];
                for (int i = 0; i < inputString.Length; i++)
                {
                    // convert character -> two-digit number
                    char plainChar = inputString[i];
                    byte plainNumber = ConvertIndexesToTwoDigit(CryptMatrix[plainChar]);

                    // calculate cipher number
                    outputBytes[i] = (byte)(plainNumber + secondKeyNumbers[i % secondKeyNumbers.Length]);

                    OnProgressChanged(i+1, inputString.Length);
                }
                this.output = outputBytes;
            }
            else
            {
                char[] outputChars = new char[input.Length];
                for (int i = 0; i < input.Length; i++)
                {
                    // calculate plain number
                    byte plainNumber = (byte)(input[i] - secondKeyNumbers[i % secondKeyNumbers.Length]);
                    if (plainNumber < 11 || plainNumber > 55)
                    {
                        GuiLogMessage("Plaintext two-digit-number out of range, expected 11 <= x <= 55, got: " + plainNumber + ". Wrong key?", NotificationLevel.Error);
                        break;
                    }
                    byte[] indexes = ConvertTwoDigitToIndexes(plainNumber);

                    // convert two-digit number -> character
                    outputChars[i] = KeyArray[indexes[0], indexes[1]];

                    OnProgressChanged(i+1, input.Length);
                }
                this.output = CharArrayToByteArray(outputChars);
            }
            OnPropertyChanged("Output");
        }

        private static string ByteArrayToString(byte[] arr)
        {
            return Encoding.UTF8.GetString(arr);
        }

        private static byte[] CharArrayToByteArray(char[] arr)
        {
            return Encoding.UTF8.GetBytes(arr);
        }

        private Dictionary<char, byte[]> CreateCryptMatrix(out char[,] KeyArr)
        {
            var KeyArray = new char[5, 5];
            var CharDic = new HashSet<char>();
            int Row = 0;
            int Col = 0;
            foreach (var c in settings.KeyWord.ToLower() + ALPHABET)
            {
                if (!CharDic.Contains(c))
                {
                    if (Row < KeyArray.GetLength(1))
                        KeyArray[Row, Col] = c;
                    CharDic.Add(c);
                    Col++;
                    if (Col >= KeyArray.GetLength(0))
                    {
                        Col = 0;
                        Row++;
                    }
                }
            }
            KeyArr = KeyArray;
            var CharPosDic = new Dictionary<char, byte[]>();
            for (byte i = 0; i < KeyArray.GetLength(0); i++)
            {
                for (byte j = 0; j < KeyArray.GetLength(1); j++)
                {
                    CharPosDic.Add(KeyArray[i, j], new byte[] { i, j });
                }
            }
            return CharPosDic;
        }

        public void PostExecution()
        {

        }

        public void Stop()
        {

        }

        public void Initialize()
        {

        }

        public void Dispose()
        {

        }

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        private void OnProgressChanged(int value, int max)
        {
            if (OnPluginProgressChanged != null)
                OnPluginProgressChanged(this, new PluginProgressEventArgs(value, max));
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, logLevel));
        }
    }
}
