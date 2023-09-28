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

namespace CrypTool.MiniTreyfer
{
    [Author("Aditya Deshpande", "adeshpan@mail.uni-mannheim.de", "Universität Mannheim", "https://www.uni-mannheim.de/1/")]
    [PluginInfo("MiniTreyfer.Properties.Resources", "PluginCaption", "PluginTooltip", "MiniTreyfer/userdoc.xml", "MiniTreyfer/Images/cipher.jpg")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class MiniTreyfer : ICrypComponent
    {
        #region Private elements

        private readonly MiniTreyferSettings settings;
        private bool isPlayMode = false;

        #endregion

        #region Public interface

        /// <summary>
        /// Constructor
        /// </summary>
        public MiniTreyfer()
        {
            settings = new MiniTreyferSettings();
            settings.LogMessage += GuiLogMessage;
        }

        /// <summary>
        /// Get or set all settings for this algorithm.
        /// </summary>
        public ISettings Settings => settings;

        private readonly string _inputString;
        private readonly string _key;
        private readonly int _numberOfRounds;

        /* [PropertyInfo(Direction.InputData, "InputStringCaption", "InputStringTooltip", true)]
         public string InputString
         {
             get { return _inputString; }
             set { _inputString = value; }
         }

         [PropertyInfo(Direction.InputData, "InputKeyCaption", "InputKeyTooltip", true)]
         public string Key
         {
             get { return _key; }
             set { _key = value; }
         }

         [PropertyInfo(Direction.InputData, "InputRoundsCaption", "InputRoundsTooltip", true)]
         public int numberOfRounds
         {
             get { return _numberOfRounds; }
             set { _numberOfRounds = value; }
         }*/

        [PropertyInfo(Direction.OutputData, "OutputStringCaption1", "OutputStringTooltip1", false)]
        public string OutputString1
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputStringCaption2", "OutputStringTooltip2", false)]
        public string OutputString2
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "keyCaption", "keyTooltip", false)]
        public string Key
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
        private readonly Random randomKey = new Random();
        private readonly Random randomText = new Random();



        public void Execute()
        {
            isPlayMode = true;

            char[] inputChars = new char[9];
            char[] keyChars = new char[8];
            List<string> outputlist = new List<string>();


            //generate random key
            int k1 = randomText.Next(0, 255);
            int k2 = randomText.Next(0, 255);
            int text1 = -1;
            int text2 = -1;
            int text11 = -1;
            int text22 = -1;

            /* cipherText = InputString;


             for (int i = 0; i < InputString.Length; i++)
             {
                 inputChars[i] = InputString[i];
                 keyChars[i] = Key[i];
             }*/


            /////////////////////////the code to generate a slide pair/////////


            for (int p11 = 0; p11 < 255; p11++)
            {
                for (int p2 = 0; p2 < 255; p2++)
                {
                    if (p11 == ((p2 + substitution_box(p2 + k1))) % 256)
                    {

                        text2 = (char)p2;
                        text11 = (char)p11;
                        break;

                    }
                }
            }


            for (int p1 = 0; p1 < 255 & p1 != text2; p1++)
            {
                for (int p22 = 0; p22 < 255; p22++)
                {
                    if (p22 == ((p1 + substitution_box(p1 + k2))) % 256)
                    {

                        text1 = (char)p1;
                        text22 = (char)p22;
                        break;

                    }
                }
            }

            char P1 = (char)text1;
            char P2 = (char)text2;
            char P11 = (char)text11;
            char P22 = (char)text22;
            char key1 = (char)k1;
            char key2 = (char)k2;

            OutputString1 = string.Format("{0}{1}", P1, P2);
            OnPropertyChanged("OutputString1");
            OutputString2 = string.Format("{0}{1}", P11, P22);
            OnPropertyChanged("OutputString2");
            Key = string.Format("{0}{1}", key1, key2);
            OnPropertyChanged("Key");



            /////////////////////////////////////////the code ends here//////

            /*if (!string.IsNullOrEmpty(InputString) && InputString.Length <= 8)
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
                                case MiniTreyferSettings.MiniTreyferMode.Encrypt:

                                    break;
                                    //case MiniTreyferSettings.MiniTreyferMode.Decrypt:

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

            }*/
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
