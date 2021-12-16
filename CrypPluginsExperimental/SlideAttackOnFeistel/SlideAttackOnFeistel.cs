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

namespace CrypTool.SlideAttackOnFeistel
{
    [Author("Aditya Deshpande", "adeshpan@mail.uni-mannheim.de", "Universität Mannheim", "https://www.uni-mannheim.de/1/")]
    [PluginInfo("SlideAttackOnFeistel.Properties.Resources", "PluginCaption", "PluginTooltip", "SlideAttackOnFeistel/userdoc.xml", "SlideAttackOnFeistel/Images/SlideAttackOnFeistel.jpg")]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class SlideAttackOnFeistel : ICrypComponent
    {
        #region Private elements

        private readonly SlideAttackOnFeistelSettings settings;
        private bool isPlayMode = false;

        #endregion

        #region Public interface

        /// <summary>
        /// Constructor
        /// </summary>
        public SlideAttackOnFeistel()
        {
            settings = new SlideAttackOnFeistelSettings();
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
        private void SlideAttackOnFeistel_ReExecute()
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
            //checks if the cipher text is equal in lenght as that of entered plaintext
            if (InputCipherText.Length != InputPlainText.Length)
            {
                GuiLogMessage("The cipher text must be euqal in lenght as that of plaintext!", NotificationLevel.Error);
            }

            char[] plainTxt = new char[InputPlainText.Length];
            char[] cipherTxt = new char[InputCipherText.Length];
            //int[] tempChars = new int[10];
            List<int> leftPlainText = new List<int>();
            List<int> rightPlainText = new List<int>();
            List<int> leftCipherText = new List<int>();
            List<int> rightCipherText = new List<int>();
            char[] keyChars = new char[InputPlainText.Length / 2];
            char[] outputChars = new char[InputPlainText.Length];


            for (int i = 0; i < InputPlainText.Length / 2; i++)
            {
                leftPlainText.Add(InputPlainText[i]);
            }

            for (int i = InputPlainText.Length / 2; i < InputPlainText.Length; i++)
            {
                rightPlainText.Add(InputPlainText[i]);
            }

            for (int i = 0; i < InputCipherText.Length / 2; i++)
            {
                leftCipherText.Add(InputCipherText[i]);
            }

            for (int i = InputCipherText.Length / 2; i < InputCipherText.Length; i++)
            {
                rightCipherText.Add(InputCipherText[i]);
            }

            if (!string.IsNullOrEmpty(InputPlainText))
            {
                for (int i = 0; i < leftPlainText.Count; i++)
                {
                    //char temp = cipherText[0];
                    //for (int i = 0; i < leftPlainText.Count; i++)
                    //{
                    //    leftPlainText[i] = leftPlainText[i] ^ (( rightPlainText[i] + Key[i]) % 256);
                    //}

                    //for (int i = 0; i < leftPlainText.Count; i++)
                    //{
                    //    tempChars[i] = leftPlainText[i];
                    //}

                    //for (int i = 0; i < rightPlainText.Count; i++)
                    //{
                    //    leftPlainText[i] = rightPlainText[i];
                    //}

                    //for (int i = 0; i < tempChars.Length; i++)
                    //{
                    //    rightPlainText[i] = tempChars[i];
                    //}

                    //the attack on Feistel cipher to get the key characters
                    for (int j = 0; j < 256; j++)
                    {
                        if (rightCipherText[i] == (leftPlainText[i] ^ ((rightPlainText[i] + j) % 256)))
                        {
                            keyChars[i] = (char)j;
                        }
                    }




                    if (true)
                    {
                        switch (settings.Action)
                        {
                            case SlideAttackOnFeistelSettings.SlideAttackOnFeistelMode.Encrypt:

                                break;
                                //case SlideAttackOnFeistelSettings.SlideAttackOnFeistelMode.Decrypt:

                        }

                    }

                    // Show the progress.
                    ProgressChanged(i, leftPlainText.Count - 1);

                }

                //for (int k = 0; k < rightPlainText.Count; k++)
                //{
                //    outputChars[k] = (char)rightPlainText[k];
                //}

                //for (int k = 0; k < leftPlainText.Count; k++)
                //{
                //    int p = rightPlainText.Count;
                //    outputChars[p] = (char)leftPlainText[k];
                //    p = p + 1;
                //}


            }
            string output = new string(keyChars);
            OutputString = output;
            OnPropertyChanged("OutputString");

        }
    }



    #endregion

}

