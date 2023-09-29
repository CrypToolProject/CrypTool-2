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

namespace CrypTool.SlideAttackOnTreyfer
{
    [Author("Aditya Deshpande", "adeshpan@mail.uni-mannheim.de", "Universität Mannheim", "https://www.uni-mannheim.de/1/")]
    [PluginInfo("SlideAttackOnTreyfer.Properties.Resources", "PluginCaption", "PluginTooltip", "SlideAttackOnTreyfer/userdoc.xml",
        new[] { "SlideAttackOnTreyfer/Images/attack.jpg", "SlideAttackOnTreyfer/Images/attack.jpg", "SlideAttackOnTreyfer/Images/attack.jpg" })]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class SlideAttackOnTreyfer : ICrypComponent
    {
        #region Private elements

        private readonly SlideAttackOnTreyferSettings settings;
        private bool isPlayMode = false;

        #endregion

        #region Public interface

        /// <summary>
        /// Constructor
        /// </summary>
        public SlideAttackOnTreyfer()
        {
            settings = new SlideAttackOnTreyferSettings();
            settings.LogMessage += GuiLogMessage;
        }

        /// <summary>
        /// Get or set all settings for this algorithm.
        /// </summary>
        public ISettings Settings => settings;

        private string _inputPlainText;
        private string _inputCipherText;

        [PropertyInfo(Direction.InputData, "InputStringCaption", "InputStringTooltip", true)]
        public string InputPlainText
        {
            get => _inputPlainText;
            set => _inputPlainText = value;
        }

        [PropertyInfo(Direction.InputData, "InputCipherCaption", "InputCipherTooltip", true)]
        public string InputCipherText
        {
            get => _inputCipherText;
            set => _inputCipherText = value;
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

            char[] plainText = new char[9];
            char[] cipherText = new char[8];
            char[] key = new char[8];
            List<int> values = new List<int>();
            int currentChar = 0;

            int[,] s_box = new int[16, 16];
            int temp = 0;
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {

                    s_box[i, j] = temp;
                    temp = temp + 1;
                }

            }




            for (int i = 0; i < InputPlainText.Length; i++)
            {
                plainText[i] = InputPlainText[i];
            }
            for (int i = 0; i < InputCipherText.Length; i++)
            {
                cipherText[i] = InputCipherText[i];
            }

            if (!string.IsNullOrEmpty(InputPlainText))
            {
                for (int g = 0; g < cipherText.Length; g++)
                {


                    for (int i = 0; i < 256; i++)
                    {
                        if (g == cipherText.Length - 1)
                        {
                            currentChar = plainText[0];
                            if ((currentChar + i) % 256 == cipherText[0])
                            {
                                values.Add(i);
                            }
                        }
                        else
                        {
                            currentChar = plainText[g + 1];
                            if ((currentChar + i) % 256 == cipherText[g + 1])
                            {
                                values.Add(i);
                            }
                        }


                    }

                    int binary = values[g];

                    int half1 = 0;
                    int half2 = 0;
                    for (int i = 0; i < 16; i++)
                    {
                        for (int j = 0; j < 16; j++)
                        {
                            if (s_box[i, j] == binary)
                            {
                                half1 = i;
                                half2 = j;
                                break;
                            }
                        }
                    }


                    string required = Convert.ToString(half1, 2).PadLeft(4, '0') + Convert.ToString(half2, 2).PadLeft(4, '0');
                    int final = Convert.ToInt32(required, 2);
                    int last = final - currentChar;
                    key[g] = (char)last;


                    if (true)
                    {

                        switch (settings.Action)
                        {
                            case SlideAttackOnTreyferSettings.SlideAttackOnTreyferMode.Encrypt:
                                break;

                        }

                    }

                    ProgressChanged(g, InputPlainText.Length - 1);



                }
                string output = new string(key);

                OutputString = output;
                OnPropertyChanged("OutputString");
            }
        }

        //public int substitution_box(int a)
        //{
        //    // define a 8x8 s-box
        //    for (int i = 0; i < 16; i++)
        //    {
        //        for (int j = 0; j < 16; j++)
        //        {
        //            int temp = i * j;
        //            if (temp == 0)
        //            {
        //                temp = i + j;
        //            }
        //            s_box[i, j] = temp;
        //        }

        //    }

        //    string temp1 = Convert.ToString(a, 2).PadLeft(8, '0');

        //    // split the 8-bit binary number into two parts to be used in s-box
        //    string p = temp1.Substring(0, 4);
        //    string q = temp1.Substring(4, 4);

        //    int index1 = Convert.ToInt32(p, 2);
        //    int index2 = Convert.ToInt32(q, 2);


        //    int returnValue = s_box[index1, index2];
        //    return returnValue;
        //}

        #endregion

    }
}
