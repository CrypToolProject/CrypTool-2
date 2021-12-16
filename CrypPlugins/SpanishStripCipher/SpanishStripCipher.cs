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
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace CrypTool.Plugins.SpanishStripCipher
{
    [Author("Prof. Christof Paar, Prof. Gregor Leander, Luis Alberto Benthin Sanguino", "Luis.BenthinSanguino@rub.de", "Ruhr-Universität Bochum - Chair for Embedded Security", "http://www.emsec.rub.de/chair/home/")]
    [PluginInfo("SpanishStripCipher.Properties.Resources", "PluginCaption", "PluginTooltip", "SpanishStripCipher/DetailedDescription/doc.xml", new[] { "SpanishStripCipher/Images/SpanishStripCipher.png" })]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class SpanishStripCipher : ICrypComponent
    {
        #region Private Variables

        private readonly SpanishStripCipherSettings settings = new SpanishStripCipherSettings();
        private readonly Random rand = new Random();

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public SpanishStripCipher()
        {
            settings.LogMessage += GuiLogMessage;
        }

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputCaption", "InputTooltip", true)]
        public string Input
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

        [PropertyInfo(Direction.InputData, "KeywordCaption", "KeywordTooltip")]
        public string Keyword
        {
            get => settings.Keyword;
            set
            {
                if (!string.IsNullOrEmpty(value) && settings.Keyword != value.ToUpper())
                {
                    settings.Keyword = value.ToUpper();
                    OnPropertyChanged("Keyword");
                }
            }
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
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            StringBuilder output = new StringBuilder();
            int index = 0;

            ProgressChanged(0, 1);

            if (string.IsNullOrEmpty(Input))
            {
                if (settings.Action == SpanishStripCipherSettings.CipherMode.Encrypt)
                {
                    GuiLogMessage("Please enter plaintext to be encrypted.", NotificationLevel.Info);
                }
                else
                {
                    GuiLogMessage("Please enter ciphertext to be decrypted.", NotificationLevel.Info);
                }

                return;
            }

            if (string.IsNullOrEmpty(settings.Keyword))
            {
                GuiLogMessage("The parameter \"keyword\" cannot be left empty.", NotificationLevel.Error);
                return;
            }

            if (!checkKeyword(settings.Keyword.ToUpper()))
            {
                GuiLogMessage("The parameter \"keyword\" must only contain letters of the fixed alphabet.", NotificationLevel.Error);
                return;
            }

            Output = "";

            if (settings.Action == SpanishStripCipherSettings.CipherMode.Decrypt)
            {
                Dictionary<string, string> number2char = settings.Number2Char;

                string ciphertext = Regex.Replace(Input, "[^0-9]", "");
                if (ciphertext.Length % 2 == 1)
                {
                    GuiLogMessage("The length of the ciphertext must be an even number. Ignoring last digit...", NotificationLevel.Warning);
                }

                for (int i = 0; i < ciphertext.Length / 2; i++)
                {
                    string n = ciphertext.Substring(2 * i, 2);
                    if (!number2char.ContainsKey(n))
                    {
                        GuiLogMessage("The ciphertext contains the illegal number '" + n + "'. Giving up...", NotificationLevel.Error);
                        break;
                    }
                    Output += number2char[n];
                }
            }
            else
            {
                List<List<string>> homophones = settings.getHomophones();
                string plaintext = settings.mapDigraphs(Input.ToUpper());

                if (settings.HomophoneSelection == 0)   // random
                {
                    foreach (char c in plaintext)
                    {
                        index = settings.unorderedAlphabet.IndexOf(c);
                        if (index != -1)
                        {
                            int ofs = rand.Next(homophones[index].Count);
                            Output += homophones[index][ofs];
                        }
                    }
                }
                else   // round robin
                {
                    int[] offsets = new int[homophones.Count];
                    foreach (char c in plaintext)
                    {
                        index = settings.unorderedAlphabet.IndexOf(c);
                        if (index != -1)
                        {
                            int ofs = offsets[index];
                            offsets[index] = (offsets[index] + 1) % homophones[index].Count;
                            Output += homophones[index][ofs];
                        }
                    }
                }
            }

            OnPropertyChanged("Output");
            return;
        }

        public bool checkKeyword(string keyword)
        {
            foreach (char c in keyword)
            {
                if (settings.OrderedAlphabet.IndexOf(c) == -1)
                {
                    return false;
                }
            }

            return true;
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

        #endregion
    }
}