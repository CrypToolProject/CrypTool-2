using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace ImageSteganographyVisualization
{
    [CrypTool.PluginBase.Attributes.Localization("ImageSteganographyVisualization.Properties.Resources")]

    public partial class ImageSteganographyPresentation : UserControl
    {
        public ModeType mode;
        public LSBPresentation lsb;
        public BPCSPresentation bpcs;
        public ImageSteganographyVisualization imageStegVis;

        public ImageSteganographyPresentation(ImageSteganographyVisualization imageStegVis)
        {
            InitializeComponent();
            this.imageStegVis = imageStegVis;
            mode = imageStegVis.GetSettings().GetMode();
            Prompt.Text = Properties.Resources.ShowPresentationPrompt;
        }

        /// <summary>
        /// When the presentation is activated in the settings, this method creates a new instace of lsb or bpcs presentation (depends on the parameter) 
        /// and adds it to the main frame
        /// </summary>
        public void DisplayHidingProcessPresentation(ModeType mode)
        {
            this.mode = mode;
            MinHeight = 350;
            MinWidth = 550;
            MainFrame.Children.Remove(MainPanel);

            if (mode == ModeType.BPCS)
            {
                MainFrame.Children.Add(bpcs);
            }
            else
            {
                MainFrame.Children.Add(lsb);
            }
        }

        /// <summary>
        /// When is presentation is not activated in the settings, this method removes the content of the main frame and gives a prompt to activate the presentation
        /// in the settings if required
        /// </summary>
        public void HidePresentation()
        {
            if (mode == ModeType.LSB)
            {
                MainFrame.Children.Remove(lsb);
            }
            else if (mode == ModeType.BPCS)
            {
                MainFrame.Children.Remove(bpcs);
            }
            MinHeight = 100;
            MinWidth = 150;
            MainFrame.Children.Clear();
            MainFrame.Children.Add(MainPanel);
            Prompt.Text = Properties.Resources.ShowPresentationPrompt;
        }

        /// <summary>
        /// This method displays the header information in the presentation when extracting a secret message from a stego image using lsb technique
        /// </summary>
        public void DisplayExtractionPresentation(int messageLength, BitArray redBitMask, BitArray greenBitMask, BitArray blueBitMask)
        {
            MinHeight = 100;
            MinWidth = 150;
            MainPanel.Visibility = Visibility.Hidden;
            ExtractedHeader.Visibility = Visibility.Visible;
            MessageLengthTB.Text = string.Format(Properties.Resources.MessageLengthExtractLabel + "{0} bits = {1} characters.", messageLength * 8, messageLength);
            RedBitMask.Text = Properties.Resources.RedBitMaskLabel + ": " + BitArrayToString(redBitMask);
            GreenBitMask.Text = Properties.Resources.GreenBitMaskLabel + ": " + BitArrayToString(greenBitMask);
            BlueBitMask.Text = Properties.Resources.BlueBitMaskLabel + ": " + BitArrayToString(blueBitMask);
        }

        /// <summary>
        /// This method displays a prompt that no presentation is available when extracting a secret message from a stego image using bpcs technique
        /// </summary>
        public void DisplayNoPresentation()
        {
            MinHeight = 100;
            MinWidth = 150;
            MainPanel.Visibility = Visibility.Visible;
            ExtractedHeader.Visibility = Visibility.Hidden;
            Prompt.Text = Properties.Resources.NoPresentationPrompt;
        }

        /// <summary>
        /// Creates and returns a string that represents the bitarray provided
        /// </summary>
        private string BitArrayToString(BitArray bits)
        {
            string bitString = "";
            for (int i = 7; i >= 0; i--)
            {
                if (bits[i])
                {
                    bitString += "1";
                }
                else
                {
                    bitString += "0";
                }
            }
            return bitString;
        }
    }

}