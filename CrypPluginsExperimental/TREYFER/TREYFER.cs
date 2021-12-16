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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;

namespace CrypTool.TREYFER
{
    [Author("Aditya Deshpande", "adeshpan@mail.uni-mannheim.de", "Universität Mannheim", "https://www.uni-mannheim.de/1/")]
    [PluginInfo("TREYFER.Properties.Resources", "PluginCaption", "PluginTooltip", "TREYFER/userdoc.xml", "TREYFER/Images/Treyfer.jpg")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class TREYFER : ICrypComponent
    {
        #region Private elements

        private readonly TREYFERSettings settings;
        private bool isPlayMode = false;

        #endregion

        #region Public interface

        /// <summary>
        /// Constructor
        /// </summary>
        public TREYFER()
        {
            settings = new TREYFERSettings();
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
        private void Caesar_ReExecute()
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
        private static readonly int[,] s_box = new int[16, 16];


        public void Execute()
        {
            isPlayMode = true;
            string cipherText;
            char[] inputChars = new char[9];
            char[] keyChars = new char[8];
            List<string> outputlist = new List<string>();

            cipherText = InputString;


            for (int i = 0; i < InputString.Length; i++)
            {
                inputChars[i] = InputString[i];
                keyChars[i] = Key[i];
            }

            if (!string.IsNullOrEmpty(InputString) && InputString.Length <= 8)
            {
                for (int j = 0; j < numberOfRounds; j++)
                {
                    //char temp = cipherText[0];
                    inputChars[8] = inputChars[0];
                    for (int i = 0; i < InputString.Length; i++)
                    {

                        int temp = (inputChars[i + 1] + substitution_box((keyChars[i] + inputChars[i + 1]))) % 256;
                        char tempChar = (char)temp;
                        inputChars[i + 1] = tempChar;

                        if (true)
                        {
                            switch (settings.Action)
                            {
                                case TREYFERSettings.TREYFERMode.Encrypt:

                                    break;
                                    //case TREYFERSettings.TREYFERMode.Decrypt:

                            }



                        }


                        // Show the progress.
                        ProgressChanged(i, InputString.Length - 1);

                    }
                    inputChars[0] = inputChars[8];

                }
                string output = new string(inputChars);
                output = output.Remove(output.Length - 1);
                OutputString = output;
                OnPropertyChanged("OutputString");

            }
        }

        public int substitution_box(int a)
        {
            // define a 8x8 s-box
            //for (int i = 0; i < 16; i++)
            //{
            //    for (int j = 0; j < 16; j++)
            //    {
            //        int temp = i * j;
            //        if (temp == 0)
            //        {
            //            temp = i + j;
            //        }
            //        s_box[i, j] = temp;
            //    }

            //}
            int temp = 0;
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {

                    s_box[i, j] = temp;
                    temp = temp + 1;
                }

            }

            string temp1 = Convert.ToString(a, 2).PadLeft(8, '0');

            // split the 8-bit binary number into two parts to be used in s-box
            string p = temp1.Substring(0, 4);
            string q = temp1.Substring(4, 4);

            int index1 = Convert.ToInt32(p, 2);
            int index2 = Convert.ToInt32(q, 2);


            int returnValue = s_box[index1, index2];
            return returnValue;
        }

        #endregion

    }
}
