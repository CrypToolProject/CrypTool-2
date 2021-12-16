/*                              
   Copyright 2009-2012 Fabian Enkler, Arno Wacker (maintenance, updates), University of Kassel

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
using System.ComponentModel;
using System.Windows.Controls;

namespace CrypTool.Scytale
{
    [Author("F. Enkler, A. Wacker", "enkler@CrypTool.org, wacker@CrypTool.org", "Universität Kassel", "http://www.uc.uni-kassel.de")]
    [PluginInfo("CrypTool.Scytale.Properties.Resources", "PluginCaption", "PluginTooltip", "Scytale/DetailedDescription/doc.xml", "Scytale/icon.png")]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class Scytale : ICrypComponent
    {
        private readonly ScytaleSettings settings;
        private bool isRunning = false;
        public event PropertyChangedEventHandler PropertyChanged;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore

        private void Progress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public Scytale()
        {
            settings = new ScytaleSettings();
        }

        private string inputString = string.Empty;
        [PropertyInfo(Direction.InputData, "InputStringCaption", "InputStringTooltip", true)]
        public string InputString
        {
            get => inputString;
            set
            {
                if (value != inputString)
                {
                    inputString = value;
                    OnPropertyChanged("InputString");
                }
            }
        }

        private string outputString = string.Empty;
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



        [PropertyInfo(Direction.InputData, "StickSizeCaption", "StickSizeTooltip", false)]
        public int StickSize
        {
            get => settings.StickSize;
            set
            {
                if (!isRunning)
                {
                    return;
                }
                if (value != settings.StickSize)
                {
                    settings.StickSize = value;
                    OnPropertyChanged("StickSize");
                }
            }
        }

        public void PreExecution()
        {
            isRunning = true;
        }

        public void Execute()
        {
            if (!string.IsNullOrEmpty(inputString))
            {
                if (settings.StickSize < 1)
                {
                    EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs("Got an invalid stick size of " + settings.StickSize + "! Reverting to 1.", this, NotificationLevel.Warning));
                    settings.StickSize = 1;
                }

                //remove line breaks since they do not make any sense in the scytale
                inputString = inputString.Replace("\r", "").Replace("\n", "");

                Progress(0, 1);

                switch (settings.Action)
                {
                    case 0:
                        OutputString = Encrypt(inputString, settings.StickSize);
                        break;
                    case 1:
                        OutputString = Decrypt(inputString, settings.StickSize);
                        break;
                    default:
                        break;
                }

                Progress(1, 1);
            }
        }

        private string Decrypt(string s, int size)
        {
            string result = "";

            for (int ofs = 0; ofs < size; ofs++)
            {
                for (int i = ofs; i < s.Length; i += size)
                {
                    result += s[i];
                }
            }

            return result.Replace('_', ' ').Trim();
        }

        private string Encrypt(string s, int size)
        {
            string result = "";

            s = s.Replace('_', ' ');

            int CharsPerRow = (s.Length + size - 1) / size;
            for (int ofs = 0; ofs < CharsPerRow; ofs++)
            {
                for (int i = 0, j = ofs; i < size; i++, j += CharsPerRow)
                {
                    result += (j < s.Length) ? s[j] : '_';
                }
            }

            return result;
        }

        public void PostExecution()
        {
            isRunning = false;
        }

        public void Stop()
        {
            isRunning = false;
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        public ISettings Settings => settings;

        public UserControl Presentation => null;

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
