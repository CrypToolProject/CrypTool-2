/*                              
   Aditya Deshpande, University of Mannheim

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
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;

namespace CrypTool.Feistel
{
    [Author("Aditya Deshpande", "adeshpan@mail.uni-mannheim.de", "Universität Mannheim", "https://www.uni-mannheim.de/1/")]
    [PluginInfo("Feistel.Properties.Resources", "PluginCaption", "PluginTooltip", "Feistel/userdoc.xml", "Feistel/Images/Feistel.jpg")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class Feistel : ICrypComponent
    {
        #region Private elements

        private readonly FeistelSettings settings;
        private bool isPlayMode = false;

        #endregion

        #region Public interface

        /// <summary>
        /// Constructor
        /// </summary>
        public Feistel()
        {
            settings = new FeistelSettings();
            settings.LogMessage += GuiLogMessage;
        }

        /// <summary>
        /// Get or set all settings for this algorithm.
        /// </summary>
        public ISettings Settings => settings;

        private string _inputString;
        private string _key;
        private int _numberOfRounds;

        [PropertyInfo(Direction.InputData, "InputStringCaption", "InputStringTooltip", true)]
        public string InputString
        {
            get => _inputString;
            set => _inputString = value;
        }

        [PropertyInfo(Direction.InputData, "InputKeyCaption", "InputKeyTooltip", true)]
        public string Key
        {
            get => _key;
            set => _key = value;
        }

        [PropertyInfo(Direction.InputData, "InputRoundsCaption", "InputRoundsTooltip", true)]
        public int numberOfRounds
        {
            get => _numberOfRounds;
            set => _numberOfRounds = value;
        }

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", false)]
        public string OutputString
        {
            get;
            set;
        }



        #endregion

        #region IPlugin members
        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Fires events to indicate progress bar changes.
        /// </summary>
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        /// <summary>
        /// Fires events to indicate log messages.
        /// </summary>
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        /// <summary>
        /// No algorithm visualization
        /// </summary>
        public UserControl Presentation => null;

        public void Stop()
        {
        }

        public void PostExecution()
        {
            isPlayMode = false;
        }

        public void PreExecution()
        {
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Handles re-execution events from settings class
        /// </summary>
        private void Feistel_ReExecute()
        {
            if (isPlayMode)
            {
                Execute();
            }
        }

        #endregion

        #region IPlugin Members

#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore



        public void Execute()
        {
            // reports an error if key lenght is not half as that of input text
            if (Key.Length != (InputString.Length / 2))
            {
                GuiLogMessage("Key length should be exactly half of Input in length!!", NotificationLevel.Error);
                //System.Environment.Exit(0);
            }

            isPlayMode = true;

            List<char> inputChars = new List<char>();
            List<int> left = new List<int>();
            List<int> right = new List<int>();




            for (int i = 0; i < InputString.Length; i++)
            {
                inputChars.Add(InputString[i]);

            }
            // reports error if input plain text is not of even lenght
            if (InputString.Length % 2 != 0)
            {
                GuiLogMessage("Input plainttext must be of even lenght!", NotificationLevel.Error);
                //inputChars.Add('#');

            }

            // defining length variables
            int fullLength = inputChars.Count;
            int halfLength = fullLength / 2;

            char[] outputChars = new char[fullLength];
            //int[] tempChars = new int[halfLength];

            for (int i = 0; i < halfLength; i++)
            {
                left.Add(inputChars[i]);
            }

            for (int i = halfLength; i < fullLength; i++)
            {
                right.Add(inputChars[i]);
            }

            if (!string.IsNullOrEmpty(InputString) && Key.Length == (halfLength))
            {
                for (int j = 0; j < numberOfRounds; j++)
                {
                    // encrypting left half of the plain text
                    for (int i = 0; i < halfLength; i++)
                    {
                        left[i] = left[i] ^ ((right[i] + Key[i]) % 256);
                    }

                    //swapping left and right halves
                    for (int i = 0; i < halfLength; i++)
                    {
                        int tempChars = left[i];

                        left[i] = right[i];

                        right[i] = tempChars;
                    }


                    if (true)
                    {
                        switch (settings.Action)
                        {
                            case FeistelSettings.FeistelMode.Encrypt:

                                break;
                                //case FeistelSettings.FeistelMode.Decrypt:

                        }

                    }

                    // Show the progress.
                    ProgressChanged(j, numberOfRounds - 1);

                }
                // concatenating the two halves after swapping to get the cipher text    
                for (int k = 0; k < halfLength; k++)
                {
                    outputChars[k] = (char)left[k];
                }

                int p = halfLength;
                for (int k = 0; k < halfLength; k++)
                {
                    outputChars[p] = (char)right[k];
                    p = p + 1;
                }


            }
            string output = new string(outputChars);
            OutputString = output;
            OnPropertyChanged("OutputString");

        }
    }



    #endregion

}

