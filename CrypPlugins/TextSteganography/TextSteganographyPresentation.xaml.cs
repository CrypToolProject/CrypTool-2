using System.Collections;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace TextSteganography
{
    [CrypTool.PluginBase.Attributes.Localization("TextSteganography.Properties.Resources")]
    public partial class TextSteganographyPresentation : UserControl
    {
        private readonly TextSteganography textSteg;

        public TextSteganographyPresentation(TextSteganography textSteg)
        {
            InitializeComponent();
            this.textSteg = textSteg;
        }

        /// <summary>
        /// displays stego text in the presentation with highlighted blocks that represent the zero width spaces added 
        /// </summary>
        public void ShowZeroWidthSpaceEncoding()
        {
            StegoTextBlock.Text = "";
            if (textSteg.GetAction() == ActionType.Hide)
            {
                BitArray messagebits = new BitArray(Encoding.UTF8.GetBytes(textSteg.InputSecretMessage));
                StegoTextBlock.Inlines.Add(textSteg.CoverText.Substring(0, textSteg.offset));
                for (int i = 0; i < messagebits.Length; i++)
                {
                    if (messagebits[i])
                    {
                        StegoTextBlock.Inlines.Add(new Run(" ") { Background = Brushes.Aquamarine });
                    }
                    else
                    {
                        StegoTextBlock.Inlines.Add(new Run(" ") { Background = Brushes.LightYellow });
                    }
                }
                StegoTextBlock.Inlines.Add(textSteg.CoverText.Substring(textSteg.offset));
            }
            else
            {
                for (int i = 0; i < textSteg.CoverText.Length; i++)
                {
                    if (textSteg.CoverText[i] == '\u200b')
                    {
                        StegoTextBlock.Inlines.Add(new Run(" ") { Background = Brushes.Aquamarine });
                    }
                    else if (textSteg.CoverText[i] == '\u200c')
                    {
                        StegoTextBlock.Inlines.Add(new Run(" ") { Background = Brushes.LightYellow });
                    }
                    else
                    {
                        StegoTextBlock.Inlines.Add((textSteg.CoverText[i]).ToString());
                    }
                }
            }
            CBShowBits.IsChecked = false;
        }

        public void ShowBitsCheckbox()
        {
            CBPanel.Visibility = Visibility.Visible;
        }

        public void HideBitsCheckBox()
        {
            CBPanel.Visibility = Visibility.Hidden;
        }

        public void ClearPres()
        {
            StegoTextBlock.Text = "";
        }

        /// <summary>
        /// hides bits of the secret message and display only the stego text
        /// </summary>
        public void HideMessageBits(object sender, RoutedEventArgs e)
        {
            ShowZeroWidthSpaceEncoding();
        }

        /// <summary>
        /// display bits of the secret message in the highlighted blocks if the check box is checked
        /// </summary>
        public void ShowMessageBits(object sender, RoutedEventArgs e)
        {
            StegoTextBlock.Text = "";
            if (textSteg.GetAction() == ActionType.Hide)
            {
                BitArray messagebits = new BitArray(Encoding.UTF8.GetBytes(textSteg.InputSecretMessage));
                StegoTextBlock.Inlines.Add(textSteg.CoverText.Substring(0, textSteg.offset));
                for (int i = 0; i < messagebits.Length; i++)
                {
                    if (messagebits[i])
                    {
                        StegoTextBlock.Inlines.Add(new Run("1") { Background = Brushes.Aquamarine });
                    }
                    else
                    {
                        StegoTextBlock.Inlines.Add(new Run("0") { Background = Brushes.LightYellow });
                    }
                }
                StegoTextBlock.Inlines.Add(textSteg.CoverText.Substring(textSteg.offset));
            }
            else
            {
                for (int i = 0; i < textSteg.CoverText.Length; i++)
                {
                    if (textSteg.CoverText[i] == '\u200b')
                    {
                        StegoTextBlock.Inlines.Add(new Run("1") { Background = Brushes.Aquamarine });
                    }
                    else if (textSteg.CoverText[i] == '\u200c')
                    {
                        StegoTextBlock.Inlines.Add(new Run("0") { Background = Brushes.LightYellow });
                    }
                    else
                    {
                        StegoTextBlock.Inlines.Add((textSteg.CoverText[i]).ToString());
                    }
                }
            }
        }

        /// <summary>
        /// displays stego text in the presentation with the capital letters highlighted
        /// </summary>
        public void ShowCapitalLetterEncoding(string stegoText)
        {
            StegoTextBlock.Text = "";
            for (int i = 0; i < stegoText.Length; i++)
            {
                char c = stegoText[i];
                // highlight the letter if it is capital letter
                if (char.IsUpper(stegoText[i]))
                {
                    StegoTextBlock.Inlines.Add(new Run(char.ToString(stegoText[i])) { FontWeight = FontWeights.Bold, Background = Brushes.Aquamarine });
                }
                else if (stegoText[i] == '\n')
                {
                    StegoTextBlock.Inlines.Add(new Run(char.ToString(stegoText[i])) { Background = Brushes.Aquamarine });
                }
                else
                {
                    StegoTextBlock.Inlines.Add(new Run(char.ToString(stegoText[i])) { });
                }
            }
        }

        /// <summary>
        /// displays stego text in the presentation with the marked letters highlighted
        /// </summary>
        public void ShowLettersMarkingEncoding(string stegoText)
        {
            StegoTextBlock.Text = "";
            for (int i = 0; i < stegoText.Length - 1; i++)
            {
                char c = stegoText[i];
                char d = stegoText[i + 1];
                // highlight the letter if the next character is a mark
                if (d == '\u0323')
                {
                    string s = "";
                    s += c;
                    s += d;

                    StegoTextBlock.Inlines.Add(new Run(s) { FontWeight = FontWeights.Bold, Background = Brushes.Aquamarine });
                    i++;
                }
                // add the character without highlight
                else
                {
                    StegoTextBlock.Inlines.Add(new Run(char.ToString(c)));
                }
            }
        }
    }
}
