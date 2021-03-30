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
using System.ComponentModel;
using System.Windows.Controls;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System.Text;
using Vernam.Properties; 

namespace CrypTool.Plugins.Vernam
{
   
    [Author("Benedict Beuscher", "benedict.beuscher@hotmail.com", "Uni Duisburg-Essen", "http://www.uni-due.de/")]
    [PluginInfo("Vernam.Properties.Resources", "PluginCaption", "PluginTooltip", "Vernam/userdoc.xml", new[] { "Vernam/Images/Vernam.png" })]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class Vernam : ICrypComponent
    {
        #region Private Variables

        private readonly VernamSettings settings = new VernamSettings();
        private string inputString;
        private string outputString;
        private string keyString;
        private string _alphabet;

        #endregion

        #region Data Properties 
        [PropertyInfo(Direction.InputData, "TextInput", "TextInputTooltip", true)]
        public string InputString
        {
            get { return this.inputString; }
            set
            {
                if (value != InputString)
                {
                    this.inputString = value;
                    OnPropertyChanged("newInputString");
                }
            }
        }

        [PropertyInfo(Direction.InputData, "KeyInput", "KeyTooltip", true)]
        public string KeyString
        {
            get { return this.keyString; }
            set
            {
                if (value != keyString)
                {
                    this.keyString = value;
                    OnPropertyChanged("newKeyString");
                }
            }
        }


        [PropertyInfo(Direction.InputData, "AlphabetInput", "AlphabetInputTooltip", false)]
        public string AlphabetSymbols
        {
            get
            {
                return _alphabet;
            }
            set
            {
                if (value != _alphabet)
                {
                    _alphabet = value;
                    OnPropertyChanged("AlphabetSymbols");
                }
            }
        }



        [PropertyInfo(Direction.OutputData, "TextOutput", "TextOutputTooltip", false)]
        public string OutputString
        {
            get { return this.outputString; }
            set { this.outputString = value; }
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation
        {
            get { return null; }
        }

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
            if (KeyString.Length == 0 || InputString.Length == 0)
            {
                OutputString = "";
                OnPropertyChanged("OutputString");
                return;
            }

            ProgressChanged(0, 1);

            var alphabet = !string.IsNullOrEmpty(_alphabet) ? _alphabet : settings.alphabet;

            if (string.IsNullOrEmpty(alphabet))
            {
                GuiLogMessage(Resources.NoAlphabetProvided, NotificationLevel.Error);
                return;
            }

            var result = new StringBuilder();
            for (var i = 0; i < InputString.Length; i++)
            {
                var inputChar = InputString[i];
                var keyChar = KeyString[i % KeyString.Length];

                var positionOfInputChar = alphabet.IndexOf(inputChar);
                var positionOfKeyChar = alphabet.IndexOf(keyChar);
                //unknown char replacement
                if (positionOfInputChar == -1 || positionOfKeyChar == -1)
                {
                    var visibileChar = HandleUnknownSymbol(inputChar);
                    result.Append(visibileChar);
                    continue;
                }

                //encrypt
                var positionOfcipherChar = positionOfInputChar + positionOfKeyChar;
                if (settings.Action == VernamSettings.CipherMode.Decrypt)
                {
                    positionOfcipherChar = positionOfInputChar - positionOfKeyChar + alphabet.Length;
                }
                positionOfcipherChar %= alphabet.Length;

                result.Append(alphabet[positionOfcipherChar]);
            }

            OutputString = result.ToString();
            OnPropertyChanged("OutputString");

            ProgressChanged(1, 1);
        }

        private string HandleUnknownSymbol(char currentKeyChar)
        {  
            //remove
            var visibleChar = "";

            if (settings.UnknownSymbolHandling == VernamSettings.UnknownSymbolHandlingMode.Replace)
            {
                visibleChar = "#";
            }

            if (settings.UnknownSymbolHandling == VernamSettings.UnknownSymbolHandlingMode.Ignore)
            {
                visibleChar = "" + currentKeyChar;
            }

            return visibleChar;
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
