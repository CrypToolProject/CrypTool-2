/*
   Copyright 2011-2023 CrypTool 2 Team <ct2contact@CrypTool.org>

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

using AvalancheVisualization;
using AvalancheVisualization.Properties;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Attributes;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.Plugins.AvalancheVisualization
{
    [Author("Camilo Echeverri", "caeche20@hotmail.com", "Universität Mannheim", "http://CrypTool2.vs.uni-due.de")]
    [PluginInfo("AvalancheVisualization.Properties.Resources", "PluginCaption", "AvalancheTooltip", "AvalancheVisualization/userdoc.xml", new[] { "AvalancheVisualization/Images/Avalanche.png" })]
    [AutoAssumeFullEndProgress(false)]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class AvalancheVisualization : ICrypComponent
    {
        #region Private Variables

        private readonly AvalancheVisualizationSettings settings = new AvalancheVisualizationSettings();
        private ICrypToolStream text;
        private ICrypToolStream key;
        private string outputStream;
        private byte[] originalText;
        private byte[] originalKey;
        private byte[] textInput;
        private byte[] keyInput;
        private string msgA;
        private string msgB;
        private readonly AES aes = new AES();
        private readonly DES des = new DES();
        private bool textChanged = false;
        private readonly AvalanchePresentation presentation;
        private byte[] initialDES;
        private byte[] current;

        #endregion

        public AvalancheVisualization()
        {
            presentation = new AvalanchePresentation(this);
        }

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputKey", "InputKeyTooltip", false)]
        public ICrypToolStream Key
        {
            get => key;
            set
            {
                key = value;
                OnPropertyChanged("Key");
            }
        }

        [PropertyInfo(Direction.InputData, "InputMessage", "InputMessageTooltip", true)]
        public ICrypToolStream Text
        {
            get => text;
            set
            {
                text = value;
                OnPropertyChanged("Text");
            }
        }

        [PropertyInfo(Direction.OutputData, "Output", "OutputTooltip", false)]
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

        public ISettings Settings => settings;

        public UserControl Presentation => presentation;

        public void PreExecution()
        {
        }      
        
        public void Execute()
        {
            ProgressChanged(0, 1);

            textInput = new byte[Text.Length];
            presentation.contrast = settings.Contrast;

            try
            {
                switch (settings.SelectedCategory)
                {
                    case AvalancheVisualizationSettings.Category.Prepared:
                        keyInput = new byte[Key.Length];

                        using (CStreamReader reader = Text.CreateReader())
                        {
                            reader.Read(textInput);
                        }
                        using (CStreamReader reader = Key.CreateReader())
                        {
                            reader.Read(keyInput);
                        }
                        if (settings.PrepSelection == 0)
                        {
                            presentation.mode = 0;
                            bool valid = ValidSize();
                            string inputMessage = Encoding.Default.GetString(textInput);

                            if (valid)
                            {
                                if (textChanged && presentation.canModify)
                                {
                                    presentation.newText = null;
                                    presentation.newKey = null;
                                    presentation.newChanges = false;

                                    presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                    {
                                        presentation.ChangesMadeButton.IsEnabled = true;
                                        presentation.ChangesMadeButton.Opacity = 1;

                                    }, null);

                                    aes.text = textInput;
                                    aes.key = keyInput;

                                    byte[] temporary = aes.checkTextLength();
                                    byte[] tmpKey = aes.checkKeysize();
                                    presentation.key = tmpKey;
                                    presentation.textB = temporary;
                                    presentation.canStop = true;
                                    aes.executeAES(false);
                                    presentation.EcryptionProgress(-1);

                                    presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                    {
                                        presentation.SetAndLoadButtons();
                                        presentation.LoadChangedMsg(temporary, true);
                                        presentation.LoadChangedKey(tmpKey);
                                        presentation.ColoringText();
                                        presentation.ColoringKey();
                                        presentation.UpdateDataColor();
                                    }, null);

                                    presentation.statesB = aes.statesB;
                                    OutputStream = string.Format("{0}{1}{2}", GeneratedData(0), GeneratedData(1), GeneratedData(2));
                                }
                                else if (!textChanged && !presentation.canModify)
                                {
                                    textChanged = true;
                                    originalText = textInput;
                                    aes.text = textInput;
                                    aes.key = keyInput;
                                    presentation.keysize = settings.KeyLength;

                                    presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                    {
                                        if (presentation.skip.IsChecked == true)
                                        {
                                            presentation.ComparisonPane();
                                        }
                                        else
                                        {
                                            presentation.Instructions();
                                        }
                                    }, null);

                                    AES.keysize = settings.KeyLength;
                                    byte[] tmpKey = aes.checkKeysize();
                                    originalKey = tmpKey;
                                    presentation.key = tmpKey;
                                    presentation.keyA = tmpKey;
                                    byte[] temporary = aes.checkTextLength();
                                    presentation.textA = temporary;
                                    aes.executeAES(true);
                                    presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                    {
                                       presentation.LoadInitialState(temporary, tmpKey);
                                    }, null);

                                    presentation.states = aes.states;
                                    presentation.keyList = aes.keyList;

                                };
                            }
                        }
                        else
                        {
                            presentation.mode = 1;

                            bool valid = ValidSize();

                            if (valid)
                            {

                                if (textChanged && presentation.canModifyDES)
                                {
                                    presentation.newText = null;
                                    presentation.newKey = null;
                                    presentation.newChanges = false;

                                    presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                    {
                                        presentation.ChangesMadeButton.IsEnabled = true;
                                        presentation.ChangesMadeButton.Opacity = 1;

                                    }, null);

                                    des.inputKey = keyInput;
                                    des.inputMessage = textInput;
                                    des.textChanged = true;
                                    des.DESProcess();
                                    presentation.key = keyInput;
                                    presentation.textB = textInput;
                                    presentation.canStop = true;
                                    presentation.EcryptionProgress(-1);

                                    presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                    {

                                        presentation.SetAndLoadButtons();
                                        presentation.LoadChangedMsg(textInput, true);
                                        presentation.LoadChangedKey(keyInput);
                                        presentation.ColoringText();
                                        presentation.ColoringKey();
                                        presentation.UpdateDataColor();
                                    }, null);

                                    presentation.lrDataB = des.lrDataB;
                                    current = des.outputCiphertext;                                   
                                    OutputStream = string.Format("{0}{1}{2}", GeneratedData(0), GeneratedData(1), GeneratedData(2));
                                }
                                else if (!textChanged && !presentation.canModifyDES)
                                {
                                    des.inputKey = keyInput;
                                    des.inputMessage = textInput;
                                    byte[] tmpKey = keyInput;
                                    byte[] tmpText = textInput;
                                    originalText = tmpText;
                                    originalKey = tmpKey;
                                    des.textChanged = false;
                                    des.DESProcess();
                                    presentation.keyA = tmpKey;
                                    textChanged = true;

                                    presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                    {

                                       if (presentation.skip.IsChecked == true)
                                       {
                                           presentation.ComparisonPane();
                                       }
                                       else
                                       {
                                           presentation.Instructions();
                                       }
                                       presentation.LoadInitialState(textInput, keyInput);

                                    }, null);

                                    presentation.textA = textInput;
                                    presentation.lrData = des.lrData;
                                    initialDES = des.outputCiphertext;
                                }
                                return;
                            }
                        }
                        break;

                    case AvalancheVisualizationSettings.Category.Unprepared:

                        using (CStreamReader reader = Text.CreateReader())
                        {
                            reader.Read(textInput);

                        }

                        switch (settings.UnprepSelection)
                        {
                            //Hash functions
                            case 0:
                                presentation.mode = 2;

                                presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                {
                                    if (textChanged && presentation.canModifyOthers)
                                    {
                                        bool validEntry = CheckByteArray(presentation.unchangedCipher, textInput);
                                        if (validEntry)
                                        {
                                            string cipherB = presentation.BinaryAsString(textInput);
                                            if (presentation.radioDecOthers.IsChecked == true)
                                            {
                                                presentation.modifiedMsg.Text = presentation.DecimalAsString(textInput);
                                            }

                                            if (presentation.radioHexOthers.IsChecked == true)
                                            {
                                                presentation.modifiedMsg.Text = presentation.HexaAsString(textInput);
                                            }

                                            presentation.TB2.Text = cipherB;
                                            presentation.changedCipher = textInput;
                                            presentation.Comparison();
                                            OutputStream = string.Format("{0}{1}", GeneratedData(0), GeneratedData(1));
                                        }
                                        else
                                        {
                                            GuiLogMessage(string.Format(Resources.Warning, textInput.Length, presentation.unchangedCipher.Length), NotificationLevel.Warning);
                                        }
                                    }
                                    else if (!textChanged && !presentation.canModifyOthers)
                                    {
                                        if (presentation.skip.IsChecked == true)
                                        {
                                            presentation.ComparisonPane();
                                        }
                                        else
                                        {
                                            presentation.Instructions();
                                        }

                                        originalText = textInput;
                                        string cipherA = presentation.BinaryAsString(textInput);
                                        presentation.originalMsg.Text = presentation.HexaAsString(textInput);
                                        presentation.TB1.Text = cipherA;
                                        presentation.unchangedCipher = textInput;
                                        presentation.radioHexOthers.IsChecked = true;
                                    }
                                }, null);

                                textChanged = true;
                                break;

                            //classic
                            case 1:

                                presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                {
                                    presentation.mode = 3;
                                    string otherText = Encoding.Default.GetString(textInput);

                                    if (textChanged && presentation.canModifyOthers)
                                    {
                                        msgB = otherText;
                                        bool validEntry = CheckSize(msgA, msgB);

                                        if (validEntry)
                                        {
                                            string cipherB = presentation.BinaryAsString(textInput);
                                            if (presentation.radioText.IsChecked == true)
                                            {
                                                presentation.modifiedMsg.Text = otherText;
                                            }
                                            if (presentation.radioDecOthers.IsChecked == true)
                                            {
                                                presentation.modifiedMsg.Text = presentation.DecimalAsString(textInput);
                                            }
                                            if (presentation.radioHexOthers.IsChecked == true)
                                            {
                                                presentation.modifiedMsg.Text = presentation.HexaAsString(textInput);
                                            }

                                            presentation.TB2.Text = cipherB;
                                            presentation.changedCipher = textInput;

                                            presentation.Comparison();

                                            OutputStream = string.Format("{0}{1}", GeneratedData(0), GeneratedData(1));
                                        }
                                        else
                                        {
                                            GuiLogMessage(string.Format(Resources.Warning, textInput.Length, presentation.unchangedCipher.Length), NotificationLevel.Warning);
                                        }
                                    }
                                    else if (!textChanged && !presentation.canModifyOthers)
                                    {
                                        if (presentation.skip.IsChecked == true)
                                        {
                                            presentation.ComparisonPane();
                                        }
                                        else
                                        {
                                            presentation.Instructions();
                                        }

                                        string cipherA = presentation.BinaryAsString(textInput);
                                        presentation.originalMsg.Text = otherText;
                                        msgA = otherText;
                                        presentation.TB1.Text = cipherA;
                                        presentation.unchangedCipher = textInput;
                                        presentation.radioText.IsChecked = true;
                                    }
                                }, null);

                                textChanged = true;
                                break;

                            //modern
                            case 2:
                                presentation.mode = 4;
                                presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                {
                                   if (textChanged && presentation.canModifyOthers)
                                   {
                                       bool validEntry = CheckByteArray(presentation.unchangedCipher, textInput);
                                       if (validEntry)
                                       {
                                           string cipherB = presentation.BinaryAsString(textInput);
                                           if (presentation.radioDecOthers.IsChecked == true)
                                           {
                                               presentation.modifiedMsg.Text = presentation.DecimalAsString(textInput);
                                           }
                                           if (presentation.radioHexOthers.IsChecked == true)
                                           {
                                               presentation.modifiedMsg.Text = presentation.HexaAsString(textInput);
                                           }
                                           presentation.TB2.Text = cipherB;
                                           presentation.changedCipher = textInput;
                                           presentation.Comparison();
                                           OutputStream = string.Format("{0}{1}", GeneratedData(0), GeneratedData(1));
                                       }
                                       else
                                       {
                                           GuiLogMessage(string.Format(Resources.Warning, textInput.Length, presentation.unchangedCipher.Length), NotificationLevel.Warning);
                                       }
                                   }
                                   else if (!textChanged && !presentation.canModifyOthers)
                                   {
                                       if (presentation.skip.IsChecked == true)
                                       {
                                           presentation.ComparisonPane();
                                       }
                                       else
                                       {
                                           presentation.Instructions();
                                       }

                                       originalText = textInput;
                                       string cipherA = presentation.BinaryAsString(textInput);
                                       presentation.originalMsg.Text = presentation.HexaAsString(textInput);
                                       presentation.TB1.Text = cipherA;
                                       presentation.unchangedCipher = textInput;
                                       presentation.radioHexOthers.IsChecked = true;

                                   }
                                }, null);
                                textChanged = true;
                                break;
                            default:
                                break;

                        }
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
        }

        public void PostExecution()
        {
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            textChanged = false;
            presentation.progress = 0;
            ProgressChanged(0, 1);
            presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {

                presentation.RemoveElements();

            }, null);

        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {

        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region Methods

        public string[] Sequence(Tuple<string, string> strTuple)
        {
            string[] diffBits = new string[strTuple.Item1.Length];

            for (int i = 0; i < strTuple.Item1.Length; i++)
            {
                if (strTuple.Item1[i] != strTuple.Item2[i])
                {
                    diffBits[i] = "X";
                }
                else
                {
                    diffBits[i] = " ";
                }
            }

            string[] differentBits = diffBits;

            return differentBits;
        }

        public Tuple<string, string> TupleDES(int roundDES)
        {
            string encryptionStateA = presentation.lrData[roundDES, 0] + presentation.lrData[roundDES, 1];
            string encryptionStateB = presentation.lrDataB[roundDES, 0] + presentation.lrDataB[roundDES, 1];

            Tuple<string, string> tuple = new Tuple<string, string>(encryptionStateA, encryptionStateB);

            return tuple;
        }

        public string GeneratedData(int position)
        {
            List<byte[]> bl = new List<byte[]>();
            List<string> sl = new List<string>();

            switch (settings.SelectedCategory)
            {
                case AvalancheVisualizationSettings.Category.Prepared:
                    if (settings.PrepSelection == 0)
                    {
                        string initialtxt = string.Format(Resources.OutputInitialAESMsg, Environment.NewLine);
                        string modifiedtxt = string.Format(Resources.OutputModifiedAESMsg, Environment.NewLine);
                        string initialkey = string.Format(Resources.OutputInitialAESKey, Environment.NewLine);
                        string modifiedkey = string.Format(Resources.OutputModifiedAESKey, Environment.NewLine);


                        string inputMessage = Encoding.ASCII.GetString(originalText);



                        string initial = string.Format("{0}{1}", inputMessage, Environment.NewLine);
                        string initialk = string.Format("{0}{1}{2}", presentation.HexaAsString(originalKey), Environment.NewLine, Environment.NewLine);
                        string modified = "";
                        string modifiedk = "";

                        if (presentation.newText != null && presentation.newKey != null)
                        {
                            string inputMessageB = Encoding.ASCII.GetString(presentation.newText);
                            modified = string.Format("{0}{1}", inputMessageB, Environment.NewLine);
                            modifiedk = string.Format("{0}{1}{2}", presentation.HexaAsString(presentation.newKey), Environment.NewLine, Environment.NewLine);


                        }
                        else
                        {
                            string inputMessageB = Encoding.ASCII.GetString(textInput);
                            modified = string.Format("{0}{1}", inputMessageB, Environment.NewLine);
                            modifiedk = string.Format("{0}{1}{2}", presentation.HexaAsString(keyInput), Environment.NewLine, Environment.NewLine);

                        }

                        string inputstr = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}", initialtxt, initial, initialkey, initialk, modifiedtxt, modified, modifiedkey, modifiedk);

                        if (initial.Equals(modified))
                        {
                            modifiedtxt = string.Format(Resources.OutputNotModAESMsg, Environment.NewLine);
                            inputstr = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}", initialtxt, initial, initialkey,
                                initialk, modifiedtxt, Environment.NewLine, modifiedkey, modifiedk);
                        }

                        if (initialk.Equals(modifiedk))
                        {
                            modifiedkey = string.Format(Resources.OutputNotModAESKey, Environment.NewLine);

                            inputstr = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", initialtxt, initial, initialkey,
                                initialk, modifiedtxt, modified, modifiedkey, Environment.NewLine, Environment.NewLine);
                        }

                        if (initial.Equals(modified) && initialk.Equals(modifiedk))
                        {
                            modifiedtxt = string.Format("{0}{1}", Resources.OutputNotModAESMsg, Environment.NewLine);
                            modifiedkey = string.Format("{0}{1}", Resources.OutputNotModAESKey, Environment.NewLine);

                            inputstr = string.Format("{0}{1}{2}{3}{4}{5}{6}", initialtxt, initial, initialkey,
                               initialk, modifiedtxt, modifiedkey, Environment.NewLine);
                        }

                        sl.Add(inputstr);

                        List<object> information = new List<object>();
                        List<byte> byteList = new List<byte>();
                        byte[] statsArray = new byte[100];
                        Tuple<string, string> strings;
                        int number = 0;
                        int rounds = 0;

                        switch (settings.KeyLength)
                        {
                            case 0:
                                number = 36;
                                rounds = 11;
                                break;
                            case 1:
                                number = 44;
                                rounds = 13;
                                break;
                            case 2:
                                number = 52;
                                rounds = 15;
                                break;
                            default:
                                break;
                        }

                        for (int aesRound = 0; aesRound <= number; aesRound += 4)
                        {

                            strings = presentation.BinaryStrings(presentation.states[aesRound], presentation.statesB[aesRound]);
                            int nrDiffBits = presentation.NoOfBitsFlipped(presentation.states[aesRound], presentation.statesB[aesRound]);
                            double avalanche = presentation.CalcAvalancheEffect(nrDiffBits, strings);
                            string[] differentBits = Sequence(strings);
                            int lengthIdentSequence = presentation.LongestIdenticalSequence(differentBits);
                            int lengthFlippedSequence = presentation.LongestFlippedSequence(differentBits);

                            information.Add(nrDiffBits);
                            information.Add(avalanche);
                            information.Add(lengthIdentSequence);
                            information.Add(presentation.sequencePosition);
                            information.Add(lengthFlippedSequence);
                            information.Add(presentation.flippedSeqPosition);
                        }

                        strings = presentation.BinaryStrings(presentation.states[number + 3], presentation.statesB[number + 3]);
                        int nrDiffBits2 = presentation.NoOfBitsFlipped(presentation.states[number + 3], presentation.statesB[number + 3]);
                        double avalanche2 = presentation.CalcAvalancheEffect(nrDiffBits2, strings);
                        string[] differentBits2 = Sequence(strings);
                        int lengthIdentSequence2 = presentation.LongestIdenticalSequence(differentBits2);
                        int lengthFlippedSequence2 = presentation.LongestFlippedSequence(differentBits2);

                        information.Add(nrDiffBits2);
                        information.Add(avalanche2);
                        information.Add(lengthIdentSequence2);
                        information.Add(presentation.sequencePosition);
                        information.Add(lengthFlippedSequence2);
                        information.Add(presentation.flippedSeqPosition);

                        object[] data = information.ToArray();

                        StringBuilder sb = new StringBuilder();

                        int i = 0;

                        for (int round = 0; round < rounds; round++)
                        {
                            sb.AppendFormat(Resources.AfterRound, round, Environment.NewLine);
                            sb.AppendFormat(Resources.OutputStats1, data[i].ToString(), data[i+ 1].ToString(), Environment.NewLine);
                            sb.AppendFormat(Resources.OutputStats2, data[i + 2].ToString(), data[i + 3].ToString(), Environment.NewLine);
                            sb.AppendFormat(Resources.OutputStats3, data[i + 4].ToString(), data[i + 5].ToString(), Environment.NewLine);
                            sb.AppendFormat("{0}", Environment.NewLine);
                            i += 6;
                        }

                        string newString = sb.ToString();

                        sl.Add(newString);

                        string finalModified = "";
                        string finalInitial = "";
                        string finalAESInitial = string.Format(Resources.AESDerivedFromInit, Environment.NewLine, Environment.NewLine);
                        string finalAESModified = string.Format(Resources.AESDerivedFromMod, Environment.NewLine);

                        finalInitial = string.Format("{0}{1}", presentation.HexaAsString(aes.states[number + 3]), Environment.NewLine);

                        if (presentation.newKey != null)
                        {
                            finalModified = string.Format("{0}{1}", presentation.HexaAsString(presentation.aesDiffusion.statesB[number + 3]), Environment.NewLine);
                        }
                        else
                        {
                            finalModified = string.Format("{0}{1}", presentation.HexaAsString(presentation.statesB[number + 3]), Environment.NewLine);
                        }

                        string finalstrAES = string.Format("{0}{1}{2}{3}", finalAESInitial, finalInitial, finalAESModified, finalModified);

                        sl.Add(finalstrAES);
                    }
                    else
                    {
                        string initialtxt = string.Format(Resources.OutputInitialDESMsg, Environment.NewLine);
                        string modifiedtxt = string.Format(Resources.OutputModifiedDESMsg, Environment.NewLine);
                        string initialkey = string.Format(Resources.OutputInitialDESKey, Environment.NewLine);
                        string modifiedkey = string.Format(Resources.OutputModifiedDESKey, Environment.NewLine);
                        string inputMessage = Encoding.ASCII.GetString(originalText);
                        string initial = string.Format("{0}{1}", inputMessage, Environment.NewLine);
                        string initialk = string.Format("{0}{1}{2}", presentation.HexaAsString(originalKey), Environment.NewLine, Environment.NewLine);
                        string modified = "";
                        string modifiedk = "";

                        if (presentation.newText != null && presentation.newKey != null)
                        {
                            string inputMessageB = Encoding.ASCII.GetString(presentation.newText);
                            modified = string.Format("{0}{1}", inputMessageB, Environment.NewLine);
                            modifiedk = string.Format("{0}{1}{2}", presentation.HexaAsString(presentation.newKey), Environment.NewLine, Environment.NewLine);
                        }
                        else
                        {
                            string inputMessageB = Encoding.ASCII.GetString(textInput);
                            modified = string.Format("{0}{1}", inputMessageB, Environment.NewLine);
                            modifiedk = string.Format("{0}{1}{2}", presentation.HexaAsString(keyInput), Environment.NewLine, Environment.NewLine);
                        }

                        string inputstr = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}", initialtxt, initial, initialkey, initialk, modifiedtxt, modified, modifiedkey, modifiedk);

                        if (initial.Equals(modified))
                        {
                            modifiedtxt = string.Format(Resources.OutputNotModDESMsg, Environment.NewLine);
                            inputstr = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}", initialtxt, initial, initialkey,
                                initialk, modifiedtxt, Environment.NewLine, modifiedkey, modifiedk);
                        }

                        if (initialk.Equals(modifiedk))
                        {
                            modifiedkey = string.Format(Resources.OutputNotModDESKey, Environment.NewLine);
                            inputstr = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", initialtxt, initial, initialkey,
                                initialk, modifiedtxt, modified, modifiedkey, Environment.NewLine, Environment.NewLine);
                        }

                        if (initial.Equals(modified) && initialk.Equals(modifiedk))
                        {
                            modifiedtxt = string.Format("{0}{1}", Resources.OutputNotModDESMsg, Environment.NewLine);
                            modifiedkey = string.Format("{0}{1}", Resources.OutputNotModDESKey, Environment.NewLine);
                            inputstr = string.Format("{0}{1}{2}{3}{4}{5}{6}", initialtxt, initial, initialkey,
                               initialk, modifiedtxt, modifiedkey, Environment.NewLine);
                        }

                        sl.Add(inputstr);

                        List<object> information = new List<object>();
                        List<byte> byteList = new List<byte>();
                        byte[] dataArray = new byte[120];
                        Tuple<string, string> strings;

                        for (int desRound = 0; desRound < 17; desRound++)
                        {
                            strings = TupleDES(desRound);
                            presentation.ToStringArray(desRound);
                            int nrDiffBits = presentation.NoOfBitsFlipped(presentation.seqA, presentation.seqB);
                            double avalanche = presentation.CalcAvalancheEffect(nrDiffBits, strings);
                            string[] differentBits = Sequence(strings);
                            int lengthIdentSequence = presentation.LongestIdenticalSequence(differentBits);
                            int lengthFlippedSequence = presentation.LongestFlippedSequence(differentBits);
                            information.Add(nrDiffBits);
                            information.Add(avalanche);
                            information.Add(lengthIdentSequence);
                            information.Add(presentation.sequencePosition);
                            information.Add(lengthFlippedSequence);
                            information.Add(presentation.flippedSeqPosition);
                        }

                        object[] dat = information.ToArray();
                        StringBuilder sbuilder = new StringBuilder();
                        int j = 0;

                        for (int round = 0; round < 17; round++)
                        {
                            sbuilder.AppendFormat(Resources.AfterRound, round, Environment.NewLine);
                            sbuilder.AppendFormat(Resources.OutputStats1, dat[j].ToString(), dat[j + 1].ToString(), Environment.NewLine);
                            sbuilder.AppendFormat(Resources.OutputStats2, dat[j + 2].ToString(), dat[j + 3].ToString(), Environment.NewLine);
                            sbuilder.AppendFormat(Resources.OutputStats3, dat[j + 4].ToString(), dat[j + 5].ToString(), Environment.NewLine);
                            sbuilder.AppendFormat("{0}", Environment.NewLine);
                            j += 6;
                        }

                        string newStr = sbuilder.ToString();
                        sl.Add(newStr);
                        string finalModified = "";
                        string finalDESInitial = string.Format(Resources.DESderivedFromInit, Environment.NewLine, Environment.NewLine);
                        string finalDESModified = string.Format(Resources.DESderivedFromMod, Environment.NewLine);
                        string finalInitial = string.Format("{0}{1}", presentation.HexaAsString(initialDES), Environment.NewLine);

                        if (presentation.currentDES != null)
                        {
                            finalModified = string.Format("{0}{1}", presentation.HexaAsString(presentation.currentDES), Environment.NewLine);
                        }
                        else
                        {
                            finalModified = string.Format("{0}{1}", presentation.HexaAsString(current), Environment.NewLine);
                        }

                        string finalstr = string.Format("{0}{1}{2}{3}", finalDESInitial, finalInitial, finalDESModified, finalModified);

                        sl.Add(finalstr);
                    }
                    break;

                case AvalancheVisualizationSettings.Category.Unprepared:

                    if (settings.UnprepSelection == 0 || settings.UnprepSelection == 2)
                    {
                        string initialCaption = "";
                        string modifiedCaption = "";

                        if (presentation.mode == 2)
                        {
                            initialCaption = string.Format(Resources.InitialHashOutput, Environment.NewLine);
                            modifiedCaption = string.Format(Resources.ModHashOutput, Environment.NewLine);
                        }
                        if (presentation.mode == 4)
                        {
                            initialCaption = string.Format(Resources.EncryptionInitialMsgOut, Environment.NewLine);
                            modifiedCaption = string.Format(Resources.EncryptionModifiedlMsgOut, Environment.NewLine);

                        }

                        string init = string.Format("{0}{1}", presentation.HexaAsString(originalText), Environment.NewLine);
                        string mod = string.Format("{0}{1}", presentation.HexaAsString(textInput), Environment.NewLine);
                        string inputStr = string.Format("{0}{1}{2}{3}", initialCaption, init, modifiedCaption, mod);

                        if (init.Equals(mod))
                        {
                            modifiedCaption = string.Format("{0}{1}", Resources.OutputNotMod, Environment.NewLine);
                            inputStr = string.Format("{0}{1}{2}{3}", initialCaption, init, modifiedCaption, Environment.NewLine);
                        }

                        sl.Add(inputStr);

                        Tuple<string, string> strings = presentation.BinaryStrings(presentation.unchangedCipher, presentation.changedCipher);
                        int bitsFlipped = presentation.NoOfBitsFlipped(presentation.unchangedCipher, presentation.changedCipher);
                        double avalanche = presentation.CalcAvalancheEffect(bitsFlipped, strings);
                        presentation.ShowBitSequence(strings);
                        int lengthIdentSequence = presentation.LongestIdenticalSequence(presentation.differentBits);
                        int lengthFlippedSequence = presentation.LongestFlippedSequence(presentation.differentBits);

                        string flippedBits = string.Format(Resources.OutputStats1, bitsFlipped, avalanche, Environment.NewLine);
                        string identSeq = string.Format(Resources.OutputStats2, lengthIdentSequence, presentation.sequencePosition, Environment.NewLine);
                        string flippedSeq = string.Format(Resources.OutputStats3, lengthFlippedSequence, presentation.flippedSeqPosition, Environment.NewLine);

                        string statsStr = string.Format("{0}{1}{2}{3}", Environment.NewLine, flippedBits, identSeq, flippedSeq);
                        sl.Add(statsStr);

                    }
                    else
                    {
                        string initialCaption = string.Format(Resources.EncryptionInitialMsgOut, Environment.NewLine);
                        string modifiedCaption = string.Format(Resources.EncryptionModifiedlMsgOut, Environment.NewLine);
                        string init = string.Format("{0}{1}", msgA, Environment.NewLine);
                        string mod = string.Format("{0}{1}{2}", msgB, Environment.NewLine, Environment.NewLine);
                        string inputStr = string.Format("{0}{1}{2}{3}", initialCaption, init, modifiedCaption, mod);

                        if (init.Equals(mod))
                        {
                            modifiedCaption = string.Format("{0}{1}", Resources.OutputNotMod, Environment.NewLine);
                            inputStr = string.Format("{0}{1}{2}{3}", initialCaption, init, modifiedCaption, Environment.NewLine);
                        }

                        sl.Add(inputStr);

                        //classic
                        Tuple<string, string> strings = presentation.BinaryStrings(presentation.unchangedCipher, presentation.changedCipher);
                        int nrBytesFlipped = presentation.BytesFlipped();
                        double avalanche = presentation.AvalancheEffectBytes(nrBytesFlipped);
                        presentation.ShowBitSequence(strings);
                        int lengthIdentSequence = presentation.LongestIdentSequenceBytes();
                        int lengthFlippedSequence = presentation.LongestFlippedSequenceBytes();
                        string flippedBits = string.Format(Resources.OutputStatsClassic1, nrBytesFlipped, avalanche, Environment.NewLine);
                        string identSeq = string.Format(Resources.OutputStatsClassic2, lengthIdentSequence, presentation.sequencePosition, Environment.NewLine);
                        string flippedSeq = string.Format(Resources.OutputStatsClassic3, lengthFlippedSequence, presentation.flippedSeqPosition, Environment.NewLine);
                        string statsStr = string.Format("{0}{1}{2}", flippedBits, identSeq, flippedSeq);
                        sl.Add(statsStr);
                    }
                    break;

                default:
                    break;
            }
            return sl[position];

        }


        public bool CheckSize(string A, string B)
        {
            if (A.Length != B.Length)
            {
                return false;
            }
            return true;
        }

        public bool CheckByteArray(byte[] A, byte[] B)
        {
            if (A.Length != B.Length)
            {
                return false;
            }
            return true;
        }

        public bool ValidSize()
        {
            if (presentation.mode == 0)
            {
                switch (settings.KeyLength)
                {
                    case 0:
                        if (key.Length != 16)
                        {
                            GuiLogMessage(Resources.KeyLength128, NotificationLevel.Warning);
                        }
                        else
                        {
                            return true;
                        }

                        break;
                    case 1:
                        if (key.Length != 24)
                        {
                            GuiLogMessage(Resources.KeyLength192, NotificationLevel.Warning);
                        }
                        else
                        {
                            return true;
                        }

                        break;
                    case 2:
                        if (key.Length != 32)
                        {
                            GuiLogMessage(Resources.KeyLength256, NotificationLevel.Warning);
                        }
                        else
                        {
                            return true;
                        }

                        break;
                }
            }

            if (presentation.mode == 1)
            {
                if (key.Length != 8)
                {
                    GuiLogMessage(Resources.KeyLengthDES, NotificationLevel.Warning);
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        public byte[] FllArray(int arrayLength)
        {
            byte[] byteArr = new byte[arrayLength];
            int i = 0;

            foreach (byte b in byteArr)
            {
                byteArr[i] = 0;
                i++;
            }
            return byteArr;
        }

        public void instantiation()
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

        public void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}
