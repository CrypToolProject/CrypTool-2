/*
   Copyright 2018 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using FormatPreservingEncryptionWeydstone;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Xml;

namespace CrypTool.Plugins.FormatPreservingEncryption
{
    [Author("Alexander Hirsch", "alexander.hirsch@CrypTool.org", "Universität Kassel", "https://www.CrypTool.org")]
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("FormatPreservingEncryption.Properties.Resources", "PluginCaption", "PluginTooltip", "FormatPreservingEncryption/userdoc.xml", new[] { "FormatPreservingEncryption/Images/FormatPreservingEncryptionIcon.png" })]

    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class FormatPreservingEncryption : ICrypComponent
    {
        #region Private Variables

        private readonly FormatPreservingEncryptionSettings settings = new FormatPreservingEncryptionSettings();
        private readonly StringBuilder logBuilder = new StringBuilder();
        private readonly char TASK_SEPERATOR = ';';
        private readonly char ALPHABET_SEPERATOR = '#';


        #endregion

        #region Data Properties
        [PropertyInfo(Direction.InputData, "AlphabetCaption", "AlphabetTooltip", true)]
        public string Alphabet
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "InputCaption", "InputTooltip", true)]
        public string Input
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "KeyCaption", "KeyTooltip", true)]
        public byte[] Key
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "TweakCaption", "TweakTooltip", false)]
        public byte[] Tweak
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "TweakRadixCaption", "TweakRadixTooltip", false)]
        public int TweakRadix
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip")]
        public string Output
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "Log", "Log")]
        public string Log
        {
            get;
            set;
        }
        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings => settings;

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation => null;

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            //Clear log from previous operations.
            logBuilder.Clear();
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);

            //check Key
            if (Key.Length != 16)
            {
                GuiLogMessage("Key must be 128bit.", NotificationLevel.Error);
                return;
            }


            //switch mode
            if (settings.Mode == (int)Modes.Normal)
            {
                if (!ValidateAlphabet(Alphabet))
                {
                    return;
                }

                char[] mapping = Alphabet.ToCharArray();
                int radix = mapping.Length;

                //store all positions of spaces and then remove these (only if space is not part of alphabet)
                string input = Input;
                List<int> whitespacePositions = new List<int>();
                if (!Alphabet.Contains(" "))
                {
                    whitespacePositions = StoreAndRemoveSpaces(ref input);
                }
                Input = input;

                if (!ValidateInput(Input))
                {
                    return;
                }

                //string -> int[]
                int[] intInput = StringToIntArray(Input, Alphabet);
                int[] intOutput;
                try
                {
                    intOutput = Crypt(intInput, radix, true);
                }
                catch (Exception e)
                {
                    GuiLogMessage(e.Message, NotificationLevel.Error);
                    return;
                }

                //int[] -> string
                string output = IntArrayToString(intOutput, mapping);

                //restore spaces
                foreach (int i in whitespacePositions)
                {
                    output = output.Insert(i, " ");
                }

                Output = output;
                OnPropertyChanged("Output");
                ProgressChanged(1, 1);

            }
            else if (settings.Mode == (int)Modes.XML)
            {

                StringWriter sws = new StringWriter();
                XmlDocument xmlDo = new XmlDocument();
                xmlDo.LoadXml(Input);
                xmlDo.Save(sws);
                if (settings.PassPlaintext)
                {
                    Output = sws.ToString();
                    OnPropertyChanged("Output");
                }

                string[] tasks = Alphabet.Split(new char[] { TASK_SEPERATOR }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string task in tasks)
                {
                    if (task.Length <= 1 || !task.Contains(ALPHABET_SEPERATOR.ToString()))
                    {
                        GuiLogMessage("Task is invalid. Too short or Alphabet missing", NotificationLevel.Error);
                        return;
                    }
                    string[] splits = task.Split(new char[] { ALPHABET_SEPERATOR });
                    string xpath = splits[0];
                    string taskAlphabet = splits[1];

                    //validate Alphabet and create mapping
                    if (!ValidateAlphabet(taskAlphabet))
                    {
                        continue;
                    }

                    char[] mapping = taskAlphabet.ToCharArray();
                    int radix = mapping.Length;

                    string xmlString = Input;
                    try
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(xmlString);
                        XmlNodeList nodes = xmlDoc.SelectNodes(xpath);
                        foreach (XmlNode node in nodes)
                        {
                            //store all positions of spaces and then remove these (only if space is not part of alphabet)
                            string text = node.InnerText;
                            List<int> whitespacePositions = new List<int>();
                            if (!task.Contains(" "))
                            {
                                whitespacePositions = StoreAndRemoveSpaces(ref text);
                            }

                            //validate Plaintext
                            if (!ValidateInput(text))
                            {
                                continue;
                            }

                            //string -> int[]
                            int[] intInput = StringToIntArray(text, taskAlphabet);
                            int[] intOutput;
                            try
                            {
                                intOutput = Crypt(intInput, radix, false);
                            }
                            catch (Exception e)
                            {
                                GuiLogMessage(e.Message, NotificationLevel.Error);
                                return;
                            }
                            //int[] -> string
                            string stringOutput = IntArrayToString(intOutput, mapping);

                            //restore spaces
                            foreach (int i in whitespacePositions)
                            {
                                stringOutput = stringOutput.Insert(i, " ");
                            }

                            node.InnerText = stringOutput;

                            xmlDoc.ImportNode(node, true);
                        }
                        StringWriter sw = new StringWriter();
                        xmlDoc.Save(sw);
                        Output = sw.ToString();
                        Input = sw.ToString();

                    }
                    catch (Exception e)
                    {
                        GuiLogMessage("Could not process XML document", NotificationLevel.Error);
                        GuiLogMessage("Source: " + e.Source + ", text: " + e.Message, NotificationLevel.Error);
                    }
                }

                OnPropertyChanged("Output");
                ProgressChanged(1, 1);
            }
            else
            {
                GuiLogMessage("Error: selected mode not implemented", NotificationLevel.Error);
            }

            Log = logBuilder.ToString();
            OnPropertyChanged("Log");

        }

        /// <summary>
        /// Searches for all spaces and removes these
        /// returns the positions in a list
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private List<int> StoreAndRemoveSpaces(ref string input)
        {
            List<int> whitespacePositions = new List<int>();
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == ' ')
                {
                    whitespacePositions.Add(i);
                }
            }

            input = input.Replace(" ", "");
            return whitespacePositions;
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
            Tweak = new byte[] { };
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
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

        private void UpdateProgressValue(object sender, FormatPreservingEncryptionWeydstone.ProgressChangedEventArgs e)
        {
            ProgressChanged(e.Progress, 1);
        }

        /// <summary>
        /// Eventhandler to log strings.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OutputChanged(object sender, OutputChangedEventArgs e)
        {
            logBuilder.Append(e.Text + Environment.NewLine);
        }

        /// <summary>
        /// Encrypts or decrypts the input. This method works only with integer values. 
        /// </summary>
        /// <param name="intInput"> The value to encrypt or decrypt.</param>
        /// <param name="radix">The radix.</param>
        /// <param name="updateProgressValue">Indicates if the progress property should be updated.</param>
        /// <returns>The result of the operation as integer[]</returns>
        private int[] Crypt(int[] intInput, int radix, bool updateProgressValue)
        {
            int[] intOutput = { };
            switch (settings.Algorithm)
            {
                case (int)Algorithms.FF1:
                    {
                        //validate Tweak
                        int maxTweakLength = Constants.MAXLEN;
                        if (Tweak.Length > maxTweakLength)
                        {
                            GuiLogMessage("Tweak is too long (" + Tweak.Length + " > " + maxTweakLength + ").", NotificationLevel.Warning);
                            byte[] newTweak = new byte[maxTweakLength];
                            Array.Copy(Tweak, 0, newTweak, 0, maxTweakLength);
                            Tweak = newTweak;
                            GuiLogMessage("New Tweak is: " + Common.byteArrayToHexString(Tweak), NotificationLevel.Warning);
                        }

                        FF1 ff1 = new FF1(radix, Constants.MAXLEN);
                        ff1.OutputChanged += OutputChanged;
                        if (updateProgressValue)
                        {
                            ff1.ProgressChanged += UpdateProgressValue;
                        }

                        if (settings.Action == (int)Actions.Encrypt)
                        {
                            intOutput = ff1.encrypt(Key, Tweak, intInput);
                        }
                        else if (settings.Action == (int)Actions.Decrypt)
                        {
                            intOutput = ff1.decrypt(Key, Tweak, intInput);
                        }
                        return intOutput;
                    }
                case (int)Algorithms.FF2:
                    {
                        if (TweakRadix < Constants.MINRADIX_FF2 || TweakRadix > Constants.MAXRADIX_FF2)
                        {
                            GuiLogMessage("Tweak radix must be in the range [" + Constants.MINRADIX_FF2 + ".." + Constants.MAXRADIX_FF2 + "]: " + TweakRadix, NotificationLevel.Error);
                            return intOutput;
                        }
                        int maxTlen = Common.floor(104 / (Math.Log(TweakRadix) / Math.Log(2))) - 1;
                        if (Tweak == null || Tweak.Length == 0)
                        {
                            GuiLogMessage("Tweak is empty.", NotificationLevel.Warning);
                            Tweak = new byte[] { 0x00 };
                            GuiLogMessage("New Tweak is: " + Common.byteArrayToHexString(Tweak), NotificationLevel.Warning);
                        }
                        if (Tweak.Length > maxTlen)
                        {
                            GuiLogMessage("Tweak is too long (" + Tweak.Length + " > " + maxTlen + ").", NotificationLevel.Warning);
                            byte[] newTweak = new byte[maxTlen];
                            Array.Copy(Tweak, 0, newTweak, 0, maxTlen);
                            Tweak = newTweak;
                            GuiLogMessage("New Tweak is: " + Common.byteArrayToHexString(Tweak), NotificationLevel.Warning);
                        }

                        FF2 ff2 = new FF2(radix, TweakRadix);
                        ff2.OutputChanged += OutputChanged;
                        if (updateProgressValue)
                        {
                            ff2.ProgressChanged += UpdateProgressValue;
                        }

                        if (settings.Action == (int)Actions.Encrypt)
                        {
                            intOutput = ff2.encrypt(Key, Tweak, intInput);
                        }
                        else if (settings.Action == (int)Actions.Decrypt)
                        {
                            intOutput = ff2.decrypt(Key, Tweak, intInput);
                        }
                        return intOutput;
                    }
                case (int)Algorithms.FF3:
                    {
                        int expectedTweakLength = 8;
                        if (Tweak == null || Tweak.Length < expectedTweakLength)
                        {
                            GuiLogMessage("Tweak is too small (" + Tweak.Length + " < " + 8 + ").", NotificationLevel.Warning);
                            byte[] newTweak = new byte[expectedTweakLength];
                            for (int i = 0; i < expectedTweakLength; i++)
                            {
                                newTweak[i] = i < Tweak.Length ? Tweak[i] : (byte)0x00;
                            }
                            Tweak = newTweak;
                            GuiLogMessage("New Tweak is: " + Common.byteArrayToHexString(Tweak), NotificationLevel.Warning);
                        }
                        if (Tweak.Length > expectedTweakLength)
                        {
                            GuiLogMessage("Tweak is too long (" + Tweak.Length + " > " + expectedTweakLength + ").", NotificationLevel.Warning);
                            byte[] newTweak = new byte[expectedTweakLength];
                            Array.Copy(Tweak, 0, newTweak, 0, expectedTweakLength);
                            Tweak = newTweak;
                            GuiLogMessage("New Tweak is: " + Common.byteArrayToHexString(Tweak), NotificationLevel.Warning);
                        }


                        FF3 ff3 = new FF3(radix);
                        ff3.OutputChanged += OutputChanged;
                        if (updateProgressValue)
                        {
                            ff3.ProgressChanged += UpdateProgressValue;
                        }

                        if (settings.Action == (int)Actions.Encrypt)
                        {
                            intOutput = ff3.encrypt(Key, Tweak, intInput);
                        }
                        else if (settings.Action == (int)Actions.Decrypt)
                        {
                            intOutput = ff3.decrypt(Key, Tweak, intInput);
                        }
                        return intOutput;
                    }
                case (int)Algorithms.DFF:
                    {
                        if (TweakRadix < Constants.MINRADIX_FF2 || TweakRadix > Constants.MAXRADIX_FF2)
                        {
                            GuiLogMessage("Tweak radix must be in the range [" + Constants.MINRADIX_FF2 + ".." + Constants.MAXRADIX_FF2 + "]: " + TweakRadix, NotificationLevel.Error);
                            return intOutput;
                        }
                        int maxTlen = Common.floor(104 / (Math.Log(TweakRadix) / Math.Log(2))) - 1;
                        if (Tweak == null || Tweak.Length == 0)
                        {
                            GuiLogMessage("Tweak is empty.", NotificationLevel.Warning);
                            Tweak = new byte[] { 0x00 };
                            GuiLogMessage("New Tweak is: " + Common.byteArrayToHexString(Tweak), NotificationLevel.Warning);
                        }
                        if (Tweak.Length > maxTlen)
                        {
                            GuiLogMessage("Tweak is too long (" + Tweak.Length + " > " + maxTlen + ").", NotificationLevel.Warning);
                            byte[] newTweak = new byte[maxTlen];
                            Array.Copy(Tweak, 0, newTweak, 0, maxTlen);
                            Tweak = newTweak;
                            GuiLogMessage("New Tweak is: " + Common.byteArrayToHexString(Tweak), NotificationLevel.Warning);
                        }

                        DFF dff = new DFF(radix, TweakRadix, new OFF2());
                        dff.OutputChanged += OutputChanged;
                        if (updateProgressValue)
                        {
                            dff.ProgressChanged += UpdateProgressValue;
                        }

                        if (settings.Action == (int)Actions.Encrypt)
                        {
                            intOutput = dff.encrypt(Key, Tweak, intInput);
                        }
                        else if (settings.Action == (int)Actions.Decrypt)
                        {
                            intOutput = dff.decrypt(Key, Tweak, intInput);
                        }
                        return intOutput;
                    }
                default:
                    {
                        GuiLogMessage("Something went wrong. Algorithm not supported", NotificationLevel.Error);
                        return Array.Empty<int>();
                    }
            }
        }
        /// <summary>
        /// Converts the given string into an array of integers determined by the alphabet.
        /// </summary>
        /// <param name="stringInput"></param>
        /// <param name="alphabet"></param>
        /// <returns></returns>
        private int[] StringToIntArray(string stringInput, string alphabet)
        {
            int[] intPlaintext = new int[stringInput.Length];
            for (int i = 0; i < stringInput.Length; i++)
            {
                int index = alphabet.IndexOf(stringInput[i]);

                if (index < 0)
                {
                    GuiLogMessage("Character '" + stringInput[i] + "' does not exist in alphabet", NotificationLevel.Error);
                    return Array.Empty<int>();
                }

                intPlaintext[i] = index;
            }
            return intPlaintext;
        }

        #endregion
        #region methods
        /// <summary>
        /// Validates the given Alphabet. The Alphabet must consist of distinct values.
        /// </summary>
        /// <param name="Alphabet"> The Alphabet as a String</param>
        /// <returns>True if valid, false otherwise.</returns>
        private bool ValidateAlphabet(string Alphabet)
        {
            if (Alphabet == null || Alphabet.Length < Constants.MINLEN)
            {
                GuiLogMessage("Alphabet is missing or too short (length < " + Constants.MINLEN + ")", NotificationLevel.Error);
                return false;
            }
            char[] mapping = Alphabet.ToCharArray();
            //validate each char is unique
            for (int i = 0; i < mapping.Length - 1; i++)
            {
                char c = mapping[i];
                for (int j = i + 1; j < mapping.Length; j++)
                {
                    if (c == mapping[j])
                    {
                        GuiLogMessage("Alphabet has to consist of distinct values. Found multiple occurrences of character '" + c + "' in alphabet " + mapping.ToString(), NotificationLevel.Error);
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Validates the Input.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>True if valid, false otherwise.</returns>
        private bool ValidateInput(string input)
        {
            if (input == null || input.Length < Constants.MINLEN)
            {
                GuiLogMessage("Input is missing or too short (length < 2)", NotificationLevel.Error);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Converts an integer Array into a String under the provided mapping.
        /// Each integer x is mapped to the corresponding character c at position x of the given mapping.
        /// </summary>
        /// <param name="intArray"></param>
        /// <param name="mapping"></param>
        /// <returns></returns>
        private string IntArrayToString(int[] intArray, char[] mapping)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < intArray.Length; i++)
            {
                int value = intArray[i];
                if (value > mapping.Length)
                {
                    //This should not happen because of the previous validation.
                    return string.Empty;
                }
                result.Append(mapping[value]);
            }
            return result.ToString();
        }
        #endregion
    }
}