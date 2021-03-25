/*                              
   Copyright 2013 Nils Kopal, Universität Kassel

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

using System;
using System.Collections.Generic;
using System.Text;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace AlphabetPermutator
{
    [Author("Nils Kopal", "Nils.Kopal@Uni-Kassel.de", "Universität Kassel", "http://www.uni-kassel.de")]
    [PluginInfo("AlphabetPermutator.Properties.Resources", "PluginCaption", "PluginTooltip", "AlphabetPermutator/DetailedDescription/doc.xml", "AlphabetPermutator/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataflow)]
    public class AlphabetPermutator : ICrypComponent
    {
        private AlphabetPermutatorSettings _alphabetPermutatorSettings = new AlphabetPermutatorSettings();


        [PropertyInfo(Direction.InputData, "SourceAlphabetCaption", "SourceAlphabetTooltip", true)]
        public string SourceAlphabet
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "PasswordCaption", "PasswordTooltip", false)]
        public string Password
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "OffsetCaption", "OffsetTooltip", false)]
        public int Offset
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "DestinationAlphabetCaption", "DestinationAlphabetTooltip", true)]
        public string DestinationAlphabet
        {
            get;
            set;
        }

        public void PreExecution()
        {
            Offset = int.MaxValue;
            Password = null;
        }

        public void PostExecution()
        {
            
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public ISettings Settings
        {
            get { return _alphabetPermutatorSettings; }
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public void Execute()
        {

            if (string.IsNullOrEmpty(Password) && string.IsNullOrEmpty(_alphabetPermutatorSettings.Password))
            {
                Password = "";
                return;
            }

            StringBuilder builder = new StringBuilder();

            string distinctSourceAlphabet = Distinct(SourceAlphabet);
            string distinctPassword = Distinct(!string.IsNullOrEmpty(Password) ? Password : _alphabetPermutatorSettings.Password);
                        
            if (_alphabetPermutatorSettings.Order < 2 )
            {
                distinctSourceAlphabet = Sort(distinctSourceAlphabet, _alphabetPermutatorSettings.Order);
            }

            distinctSourceAlphabet = Regex.Replace(distinctSourceAlphabet, "[" + distinctPassword + "]", "");

            int offset = (Offset != int.MaxValue ? Offset % (distinctSourceAlphabet.Length + 1) : _alphabetPermutatorSettings.Offset % (distinctSourceAlphabet.Length+1));

            string left = distinctSourceAlphabet.Substring(0, offset);
            string right = distinctSourceAlphabet.Substring(offset, distinctSourceAlphabet.Length - offset);

            builder.Append(left);
            builder.Append(distinctPassword);
            builder.Append(right);

            DestinationAlphabet = builder.ToString();
            OnPropertyChanged("DestinationAlphabet");
        }

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public void Stop()
        {
           
        }

        public void Initialize()
        {
            
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void Dispose()
        {
            
        }

        private string Reverse(String str)
        {
            char[] arr = str.ToCharArray();
	        Array.Reverse(arr);
            return new string(arr);
        }
                        
        private string Sort(String str, int direction)
        {
            char[] sortarr = str.ToCharArray();
            Array.Sort(sortarr);
            if (direction == 1)
            {
                Array.Reverse(sortarr);
            }
            return new string(sortarr);
        }

        private String Distinct(string str)
        {
            StringBuilder builder = new StringBuilder();
            HashSet<char> chars = new HashSet<char>();

            foreach (char c in str)
            {
                if (!chars.Contains(c))
                {
                    chars.Add(c);
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, logLevel));
            }
        }

    }
}
