/*
   Copyright 2014 Olga Groh

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
using CrypTool.PluginBase.Attributes;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Transcriptor;
using Transcriptor.Properties;

namespace CrypTool.Plugins.Transcriptor
{
    [Author("Olga Groh", "o_groh@student.uni-kassel.de", "Uni Kassel", "www.uni-kassel.de")]
    [PluginInfo("Transcriptor.Properties.Resources", "PluginCaption", "PluginTooltip", "Transcriptor/DetailedDescription/doc.xml", new[] { "Transcriptor/icon.png" })]

    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
    [ComponentVisualAppearance(ComponentVisualAppearance.VisualAppearanceEnum.Opened)]
    public class Transcriptor : ICrypComponent
    {
        #region Private Variables

        private readonly TranscriptorSettings settings;
        private readonly TranscriptorPresentation transcriptorPresentation;

        #endregion

        public Transcriptor()
        {
            transcriptorPresentation = new TranscriptorPresentation(this);
            settings = new TranscriptorSettings();
            Presentation = transcriptorPresentation;
        }

        #region Data Properties

        /// <summary>
        /// The Image File from the user
        /// </summary>
        [PropertyInfo(Direction.InputData, "ImageFileName", "ImageToolTip", true)]
        public ICrypToolStream Image
        {
            get;
            set;
        }

        /// <summary>
        /// It's possible to use a custom Alphabet
        /// </summary>
        [PropertyInfo(Direction.InputData, "AlphabetName", "AlphabetTooltip2", false)]
        public string Alphabet
        {
            get;
            set;
        }

        /// <summary>
        /// The Output Text
        /// </summary>
        [PropertyInfo(Direction.OutputData, "TextOutputName", "TextOutputTooltip")]
        public string Text
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings => settings;

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation { get; private set; }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            Alphabet = null;
            Image = null;

            // Transfer the choosen rectangle color setting to the GUI
            switch (settings.RectangleColor)
            {
                case 0: transcriptorPresentation.RectangleColor = "Blue"; break;
                case 1: transcriptorPresentation.RectangleColor = "Green"; break;
                case 2: transcriptorPresentation.RectangleColor = "Yellow"; break;
            }

            // Transfer the color which will be presented whenn the user marks a new symbol
            switch (settings.SelectedRectangleColor)
            {
                case 0: transcriptorPresentation.SelectedRectangleColor = "Red"; break;
                case 1: transcriptorPresentation.SelectedRectangleColor = "Black"; break;
                case 2: transcriptorPresentation.SelectedRectangleColor = "White"; break;
            }

            // Transfer the Mode to the GUI
            if (settings.Mode == 0)
            {
                transcriptorPresentation.MatchTemplateOn = false;
            }
            else
            {
                transcriptorPresentation.MatchTemplateOn = true;
                transcriptorPresentation.Threshold = settings.Threshold / 100f;
            }
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);
            // If the Alphabet plugin is not plug the standard Alphabet will be used
            if (Alphabet == null || Alphabet.Length == 0)
            {
                Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            }

            transcriptorPresentation.Dispatcher.Invoke(DispatcherPriority.Background, (SendOrPostCallback)delegate
            {
                try
                {
                    //deserialize data structure from settings
                    transcriptorPresentation.Deserialize();

                    // When the workflow runs the GUI is set to be enabled
                    transcriptorPresentation.grid.IsEnabled = true;

                    // When the mode is set to manually the firstLine Button is not necessary and therefor disabled
                    if (transcriptorPresentation.MatchTemplateOn == false)
                    {
                        transcriptorPresentation.TransformButton.IsEnabled = false;
                    }
                    else
                    {
                        transcriptorPresentation.TransformButton.IsEnabled = true;
                    }

                    //Gets the Image from the Input Plugin and chage the DPI to 96
                    transcriptorPresentation.picture.Source = ByteToImage(Image.CreateReader().ReadFully());

                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format(Resources.CouldnotDisplayPicture, ex.Message), NotificationLevel.Error);
                }
            }, null);

        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            //When the worklow stops the GUI is set to disable so its not possible to triger click events
            transcriptorPresentation.Dispatcher.Invoke(DispatcherPriority.Background, (SendOrPostCallback)delegate
            {
                try
                {
                    transcriptorPresentation.grid.IsEnabled = false;
                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format(Resources.Error, ex.Message), NotificationLevel.Error);
                }
            }, null);
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
            ((TranscriptorPresentation)Presentation).Settings = settings;
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {

        }

        #endregion

        #region Event Handling

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

        /// <summary>
        /// The outputText contains the Letters of the sorted
        /// symbolList which will be handed over to the Text vaiable
        /// </summary>
        /// <param name="outputText"></param>
        internal void GenerateText(string outputText)
        {
            Text = outputText;
            OnPropertyChanged("Text");
            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Creates a new ImageSource out of a given byte array
        /// Converts the image to 96 DPI
        /// </summary>
        /// <param name="imageData"></param>
        /// <returns></returns>
        private static ImageSource ByteToImage(byte[] imageData)
        {
            //load image
            BitmapImage sourceImage = new BitmapImage();
            MemoryStream ms = new MemoryStream(imageData);
            sourceImage.BeginInit();
            sourceImage.StreamSource = ms;
            sourceImage.EndInit();
            //create new with 96 dpi
            const int dpi = 96;
            int width = sourceImage.PixelWidth;
            int height = sourceImage.PixelHeight;
            PixelFormat pixelFormat = sourceImage.Format;

            //If the format of the Image has 8 Bits per Pixel the Image in the Transcriptor will be represented wrong therefor the Gray8 format is used
            if (pixelFormat.BitsPerPixel == 8)
            {
                pixelFormat = PixelFormats.Gray8;
            }

            int stride = (width * pixelFormat.BitsPerPixel + 7) / 8;
            byte[] pixelData = new byte[stride * height];
            sourceImage.CopyPixels(pixelData, stride, 0);
            BitmapSource dpi96Image = BitmapSource.Create(width, height, dpi, dpi, pixelFormat, null, pixelData, stride);
            //finally return the new image source
            return dpi96Image;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ex"></param>
        internal void GuiLogMessage(Exception ex)
        {
            GuiLogMessage(string.Format(Resources.Error, ex.Message), NotificationLevel.Error);
        }
    }
}
