/*
   Copyright CrypTool 2 Team <ct2contact@CrypTool.org>

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
using System.Collections;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace TextSteganography
{
    [Author("Sally Addad", "addadsally@gmail.com", "University of Mannheim", "https://www.uni-mannheim.de")]
    [PluginInfo("TextSteganography.Properties.Resources", "PluginCaption", "PluginTooltip", "TextSteganography/userdoc.xml", new[] { "TextSteganography/icon.png" })]
    [ComponentCategory(ComponentCategory.Steganography)]
    public class TextSteganography : ICrypComponent
    {
        #region Private Variables

        private string coverText = "";
        private string secretMessage = "";
        private string stegoText = "";
        private readonly TextSteganographySettings settings;
        private readonly TextSteganographyPresentation presentation;
        public int offset = 0;

        public TextSteganography()
        {
            settings = new TextSteganographySettings();
            presentation = new TextSteganographyPresentation(this);
            settings.PropertyChanged += settings_PropertyChanged;
        }

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "CoverTextCaption", "CoverTextTooltip")]
        public string CoverText
        {
            get => coverText;
            set
            {
                if (coverText != value)
                {
                    coverText = value;
                    OnPropertyChanged("CoverText");
                }
            }
        }

        [PropertyInfo(Direction.InputData, "SecretMessageInputCaption", "SecretMessageInputTooltip")]
        public string InputSecretMessage
        {
            get => secretMessage;
            set
            {
                if (secretMessage != value)
                {
                    secretMessage = value;
                    //OnPropertyChanged("InputSecretMessage");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "StegoTextCaption", "StegoTextTooltip")]
        public string StegoText
        {
            get => stegoText;
            set
            {
                if (stegoText != value)
                {
                    stegoText = value;
                    //OnPropertyChanged("StegoText");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "SecretMessageOutputCaption", "SecretMessageOutputTooltip")]
        public string OutputSecretMessage
        {
            get => secretMessage;
            set
            {
                if (secretMessage != value)
                {
                    secretMessage = value;
                    //OnPropertyChanged("OutputSecretMessage");
                }
            }
        }

        public ISettings Settings => settings;

        public UserControl Presentation => presentation;

        public void PreExecution()
        {
        }

        public ActionType GetAction()
        {
            return settings.Action;
        }

        public void Execute()
        {
            ProgressChanged(0, 1);
            if (settings.Mode == ModeType.ZeroWidthSpace)
            {
                offset = settings.Offset;
                if (settings.Action == ActionType.Hide)
                {
                    ZeroWidthSpaceHide();
                }
                else
                {
                    ZeroWidthSpaceExtract();
                }

                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    presentation.ShowZeroWidthSpaceEncoding();

                }, null);
            }
            else if (settings.Mode == ModeType.CapitalLettersText)
            {
                if (settings.Action == ActionType.Hide)
                {
                    CapitalLettersTextHide();
                }
                else
                {
                    CapitalLettersTextExtract();
                }
            }
            else if (settings.Mode == ModeType.MarkingLettersBinary)
            {
                if (settings.Action == ActionType.Hide)
                {
                    MarkingLettersBinaryHide();
                }
                else
                {
                    MarkingLettersBinaryExtract();
                }
            }
            else if (settings.Mode == ModeType.CapitalLettersBinary)
            {
                if (settings.Action == ActionType.Hide)
                {
                    CapitalLettersBinaryHide();
                }
                else
                {
                    CapitalLettersBinaryExtract();
                }
            }
            else if (settings.Mode == ModeType.MarkingLettersText)
            {
                if (settings.Action == ActionType.Hide)
                {
                    MarkingLettersTextHide();
                }
                else
                {
                    MarkingLettersTextExtract();
                }
            }
            ProgressChanged(1, 1);
        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
            // set the presentation if the mode is already set to zero width spaces in settings
            if (settings.Mode == ModeType.ZeroWidthSpace)
            {
                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    if (settings.Mode == ModeType.ZeroWidthSpace)
                    {
                        presentation.ShowBitsCheckbox();
                    }
                    else
                    {
                        presentation.HideBitsCheckBox();
                    }
                }, null);
            }
        }

        public void Dispose()
        {
        }

        #endregion

        #region Implementations 

        /// <summary>
        /// hides a secret message using zero width spaces
        /// </summary>
        private void ZeroWidthSpaceHide()
        {
            StringBuilder stegoTextBuilder = new StringBuilder();
            BitArray messageBits = new BitArray(Encoding.UTF8.GetBytes(secretMessage));
            for (int i = 0; i < offset; i++)
            {
                stegoTextBuilder.Append(coverText[i]);
            }

            for (int i = 0; i < messageBits.Length; i++)
            {
                if (messageBits[i])
                {
                    stegoTextBuilder.Append('\u200b');
                }
                else
                {
                    stegoTextBuilder.Append('\u200c');
                }
            }
            stegoTextBuilder.Append(coverText.Substring(offset));
            StegoText = stegoTextBuilder.ToString();
            OnPropertyChanged("StegoText");
        }

        /// <summary>
        /// extracts a secret message that was hidden using zero width spaces
        /// </summary>
        private void ZeroWidthSpaceExtract()
        {
            string messageBitsString = "";
            for (int i = offset; i < coverText.Length; i++)
            {
                if (coverText[i] == '\u200B')
                {
                    messageBitsString += '1';
                }
                else if (coverText[i] == '\u200C')
                {
                    messageBitsString += '0';
                }
                else
                {
                    break;
                }
            }
            BitArray messageBits = new BitArray(messageBitsString.Length);
            for (int i = 0; i < messageBitsString.Length; i++)
            {
                if (messageBitsString[i] == '1')
                {
                    messageBits[i] = true;
                }
                else
                {
                    messageBits[i] = false;
                }
            }

            OutputSecretMessage = Encoding.UTF8.GetString(ConvertToBytes(messageBits));
            OnPropertyChanged("OutputSecretMessage");
        }

        /// <summary>
        /// hides a message in text by converting the letters of the secret message to capital letters in the cover text
        /// </summary>
        private void CapitalLettersTextHide()
        {
            string stegoTextTmp = coverText.ToLower();
            StringBuilder stegoTextBuilder = new StringBuilder();
            int messageIndex = 0;
            int coverTextIndex;
            for (coverTextIndex = 0; coverTextIndex < stegoTextTmp.Length; coverTextIndex++)
            {
                if (stegoTextTmp[coverTextIndex] == secretMessage[messageIndex])
                {
                    stegoTextBuilder.Append(char.ToUpper(stegoTextTmp[coverTextIndex]));
                    messageIndex++;
                    if (messageIndex == secretMessage.Length)
                    {
                        break;
                    }
                }
                else
                {
                    stegoTextBuilder.Append(stegoTextTmp[coverTextIndex]);
                }
                if (secretMessage[messageIndex] == ' ')
                {
                    stegoTextBuilder.Append('\n');
                    messageIndex++;
                }
            }
            // display warning if not the entire message could be hidden
            if (messageIndex < secretMessage.Length - 1)
            {
                GuiLogMessage(Properties.Resources.NotAllMessageHidden1, NotificationLevel.Warning);
            }
            // append the rest of the original cover text after the message is encoded
            if (coverTextIndex < stegoTextTmp.Length - 1)
            {
                stegoTextBuilder.Append(stegoTextTmp.Substring(coverTextIndex + 1));
            }
            StegoText = stegoTextBuilder.ToString();
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                presentation.ShowCapitalLetterEncoding(StegoText);

            }, null);
            OnPropertyChanged("StegoText");
        }

        /// <summary>
        ///  extracts a secret message that was hidden using capital letters (text mode)
        /// </summary>
        private void CapitalLettersTextExtract()
        {
            StringBuilder secretMessageTmp = new StringBuilder();
            for (int i = 0; i < coverText.Length; i++)
            {
                if (char.IsUpper(coverText[i]))
                {
                    secretMessageTmp.Append(char.ToLower(coverText[i]));
                }
                else if (coverText[i] == '\n')
                {
                    secretMessageTmp.Append(" ");
                }
            }

            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                presentation.ShowCapitalLetterEncoding(CoverText);

            }, null);

            OutputSecretMessage = secretMessageTmp.ToString();
            OnPropertyChanged("OutputSecretMessage");
        }

        /// <summary>
        /// hides a message in text using capital letters (binary mode)
        /// </summary> 
        private void CapitalLettersBinaryHide()
        {
            StringBuilder stegoTextBuilder = new StringBuilder();
            BitArray messageBits = new BitArray(Encoding.UTF8.GetBytes(secretMessage));

            // length of cover text should be equal to or greater than the number of bits of the secret message to encode it fully, if this is not the case display a warning
            if (messageBits.Length > coverText.Length)
            {
                GuiLogMessage(Properties.Resources.NotAllMessageHidden2, NotificationLevel.Warning);
            }
            int messageIndex = 0;
            int coverTextIndex;
            for (coverTextIndex = 0; (coverTextIndex < coverText.Length) && (messageIndex < messageBits.Length); coverTextIndex++)
            {
                if (char.IsLetter(coverText[coverTextIndex]))
                {
                    if (messageBits[messageIndex])
                    {
                        stegoTextBuilder.Append(char.ToUpper(coverText[coverTextIndex]));
                    }
                    else
                    {
                        stegoTextBuilder.Append(char.ToLower(coverText[coverTextIndex]));
                    }
                    messageIndex++;
                }
                else
                {
                    stegoTextBuilder.Append(coverText[coverTextIndex]);
                }
            }
            // append the rest of the original cover text after the message is encoded fully
            if (coverTextIndex < coverText.Length - 1)
            {
                stegoTextBuilder.Append(coverText.Substring(coverTextIndex).ToLower());
            }
            StegoText = stegoTextBuilder.ToString();
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                presentation.ShowCapitalLetterEncoding(StegoText);
            }, null);
            OnPropertyChanged("StegoText");
        }

        /// <summary>
        /// extracts a secret message that was hidden using capital letters (binary mode)
        /// </summary>
        private void CapitalLettersBinaryExtract()
        {
            string messageBitsString = "";
            for (int i = 0; i < coverText.Length; i++)
            {
                if (char.IsUpper(coverText[i]))
                {
                    messageBitsString += '1';
                }
                else if (char.IsLower(coverText[i]))
                {
                    messageBitsString += '0';
                }
            }

            BitArray messageBits = new BitArray(messageBitsString.Length);
            for (int i = 0; i < messageBits.Length; i++)
            {
                if (messageBitsString[i] == '1')
                {
                    messageBits[i] = true;
                }
                else
                {
                    messageBits[i] = false;
                }
            }

            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                presentation.ShowCapitalLetterEncoding(CoverText);

            }, null);

            OutputSecretMessage = Encoding.UTF8.GetString(ConvertToBytes(messageBits));
            OutputSecretMessage = OutputSecretMessage.Trim('\0');
            OnPropertyChanged("OutputSecretMessage");
        }

        /// <summary>
        /// hides a message in text by marking letters in the cover text (binary mode)
        /// </summary>
        private void MarkingLettersBinaryHide()
        {
            StringBuilder stegoTextBuilder = new StringBuilder();
            BitArray messageBits = new BitArray(Encoding.UTF8.GetBytes(secretMessage));

            // length of cover text should be equal to or greater than the number of bits of the secret message to encode it fully, if this is not the case display a warning
            if (messageBits.Length > coverText.Length)
            {
                GuiLogMessage(Properties.Resources.NotAllMessageHidden2, NotificationLevel.Warning);
            }

            int messageIndex = 0;
            int coverTextIndex;
            char mark;
            // set the marking character (dot above or under) depending on the chosen settings
            if (settings.Marking == MarkingType.DotUnder)
            {
                mark = '\u0323';
            }
            else
            {
                mark = '\u0307';
            }
            for (coverTextIndex = 0; (coverTextIndex < coverText.Length) && (messageIndex < messageBits.Length); coverTextIndex++)
            {
                stegoTextBuilder.Append(coverText[coverTextIndex]);

                if (messageBits[messageIndex])
                {
                    stegoTextBuilder.Append(mark);
                }
                messageIndex++;

            }
            if (coverTextIndex < coverText.Length - 1)
            {
                stegoTextBuilder.Append(coverText.Substring(coverTextIndex));
            }

            StegoText = stegoTextBuilder.ToString();
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                presentation.ShowLettersMarkingEncoding(StegoText);
            }, null);
            OnPropertyChanged("StegoText");
        }

        /// <summary>
        /// extracts a secret message that was hidden by marking letters (binary mode)
        /// </summary>
        private void MarkingLettersBinaryExtract()
        {
            string messageBitsString = "";
            char mark;
            if (settings.Marking == MarkingType.DotUnder)
            {
                mark = '\u0323';
            }
            else
            {
                mark = '\u0307';
            }
            for (int i = 0; i < coverText.Length - 1; i++)
            {
                if (coverText[i + 1] == mark)
                {
                    messageBitsString += '1';
                    i++;
                }
                else
                {
                    messageBitsString += '0';
                }
            }
            BitArray messageBits = new BitArray(messageBitsString.Length);
            for (int i = 0; i < messageBitsString.Length; i++)
            {
                if (messageBitsString[i] == '1')
                {
                    messageBits[i] = true;
                }
                else
                {
                    messageBits[i] = false;
                }
            }

            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                presentation.ShowLettersMarkingEncoding(CoverText);
            }, null);

            byte[] bytes = ConvertToBytes(messageBits);
            secretMessage = Encoding.UTF8.GetString(bytes);
            secretMessage = secretMessage.Trim('\0');
            OnPropertyChanged("OutputSecretMessage");
        }

        /// <summary>
        /// hides a message in text by marking letters in the cover text (text mode)
        /// </summary>
        private void MarkingLettersTextHide()
        {
            char mark;
            if (settings.Marking == MarkingType.DotUnder)
            {
                mark = '\u0323';
            }
            else
            {
                mark = '\u0307';
            }
            StringBuilder stegoTextBuilder = new StringBuilder();
            int messageIndex = 0;
            int coverTextIndex;
            for (coverTextIndex = 0; coverTextIndex < coverText.Length; coverTextIndex++)
            {
                stegoTextBuilder.Append(coverText[coverTextIndex]);
                if (coverText[coverTextIndex] == secretMessage[messageIndex])
                {
                    stegoTextBuilder.Append(mark);
                    messageIndex++;
                    if (messageIndex == secretMessage.Length)
                    {
                        break;
                    }
                }
            }

            if (messageIndex < secretMessage.Length - 1)
            {
                GuiLogMessage(Properties.Resources.NotAllMessageHidden1, NotificationLevel.Warning);
            }
            if (coverTextIndex < coverText.Length - 1)
            {
                stegoTextBuilder.Append(coverText.Substring(coverTextIndex + 1));
            }
            StegoText = stegoTextBuilder.ToString();
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                presentation.ShowLettersMarkingEncoding(StegoText);
            }, null);
            OnPropertyChanged("StegoText");
        }

        /// <summary>
        /// extracts a secret message that was hidden by marking letters (text mode)
        /// </summary>
        private void MarkingLettersTextExtract()
        {
            StringBuilder messageBuilder = new StringBuilder();
            char mark;
            if (settings.Marking == MarkingType.DotUnder)
            {
                mark = '\u0323';
            }
            else
            {
                mark = '\u0307';
            }
            for (int i = 0; i < coverText.Length - 1; i++)
            {
                if (coverText[i + 1] == mark)
                {
                    messageBuilder.Append(coverText[i]);
                }
            }

            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                presentation.ShowLettersMarkingEncoding(CoverText);
            }, null);
            OutputSecretMessage = messageBuilder.ToString();
            OnPropertyChanged("OutputSecretMessage");
        }

        /// <summary>
        /// converts a bit array to a byte array
        /// </summary>
        private byte[] ConvertToBytes(BitArray bits)
        {
            byte[] bytes = new byte[bits.Length / 8 + 1];
            bits.CopyTo(bytes, 0);
            return bytes;
        }

        #endregion

        #region Event Handling

        private void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Mode")
            {
                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    presentation.ClearPres();
                    if (settings.Mode == ModeType.ZeroWidthSpace)
                    {
                        presentation.ShowBitsCheckbox();
                    }
                    else
                    {
                        presentation.HideBitsCheckBox();
                    }
                }, null);
            }
        }

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
