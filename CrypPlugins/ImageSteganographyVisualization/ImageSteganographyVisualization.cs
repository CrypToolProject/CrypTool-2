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
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ImageSteganographyVisualization
{
    [Author("Sally Addad", "addadsally@gmail.com", "University of Mannheim", "https://www.uni-mannheim.de")]
    [PluginInfo("ImageSteganographyVisualization.Properties.Resources", "PluginCaption", "PluginTooltip", "ImageSteganographyVisualization/userdoc.xml", new[] { "ImageSteganographyVisualization/images/icon.png" })]
    [ComponentCategory(ComponentCategory.Steganography)]

    public class ImageSteganographyVisualization : ICrypComponent
    {
        #region Private Variables

        private readonly ImageSteganographyVisualizationSettings settings;
        private readonly ImageSteganographyPresentation presentation;
        public Bitmap inputBitmap;
        public Bitmap outputBitmap;
        public BitArray rBitMask = new BitArray(new byte[] { 1 });
        public BitArray gBitMask = new BitArray(new byte[] { 1 });
        public BitArray bBitMask = new BitArray(new byte[] { 1 });
        public static double complexityThreshold = 0.3;
        public ColorLayerOrder order;

        public ImageSteganographyVisualization()
        {
            settings = new ImageSteganographyVisualizationSettings();
            presentation = new ImageSteganographyPresentation(this);
            settings.PropertyChanged += settings_PropertyChanged;
            complexityThreshold = settings.ComplexityThreshold;
            order = settings.SelectedOrder;
        }

        public ImageSteganographyVisualizationSettings GetSettings()
        {
            return settings;
        }

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputMessageCaption", "InputMessageTooltip", false)]
        public string InputSecretMessage
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "InputImageCaption", "InputImageTooltip", true)]
        public ICrypToolStream InputImage
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputImageCaption", "OutputImageTooltip", false)]
        public ICrypToolStream OutputImage
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputMessageCaption", "OutputMessageTooltip", false)]
        public string OutputSecretMessage
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings => settings;

        public UserControl Presentation => presentation;

        public void PreExecution()
        {
            InputImage = null;
        }

        public void Execute()
        {
            ProgressChanged(0, 1);

            if (InputImage == null)
            {
                GuiLogMessage(Properties.Resources.NoImageLogMessage, NotificationLevel.Error);
                return;
            }

            if (settings.Action == ActionType.Hide && InputSecretMessage == null)
            {
                GuiLogMessage(Properties.Resources.NoSecretMessageGiven, NotificationLevel.Error);
                return;
            }

            CStreamReader imageReader = InputImage.CreateReader();
            inputBitmap = new Bitmap(imageReader);

            if (settings.SelectedMode == ModeType.LSB)
            {
                if (settings.Action == ActionType.Hide)
                {
                    if (settings.ShowPresentation)
                    {
                        Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            presentation.lsb = new LSBPresentation(this);
                            presentation.DisplayHidingProcessPresentation(settings.SelectedMode);
                            presentation.lsb.EnableButtons();
                        }, null);
                        ProgressChanged(0.5, 1);
                    }
                    else
                    {
                        Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            HideDataLSB();
                            presentation.HidePresentation();
                        }, null);
                    }
                }
                else
                {
                    ExtractDataLSB();
                    Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        presentation.DisplayExtractionPresentation(OutputSecretMessage.Length, rBitMask, gBitMask, bBitMask);
                    }, null);
                }
            }
            else
            {
                if (settings.Action == ActionType.Hide)
                {
                    Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        if (settings.ShowPresentation)
                        {
                            presentation.bpcs = new BPCSPresentation(this);
                            presentation.DisplayHidingProcessPresentation(settings.SelectedMode);
                            HideDataBPCS();
                            presentation.bpcs.EnableButtons();
                        }
                        else
                        {
                            presentation.HidePresentation();
                            HideDataBPCS();
                        }
                    }, null);
                }
                else
                {
                    Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        presentation.DisplayNoPresentation();
                    }, null);
                    ExtractDataBPCS();
                }
            }
        }

        private void settings_PropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "ComplexityThreshold")
            {
                complexityThreshold = settings.ComplexityThreshold;
            }
            else if (propertyChangedEventArgs.PropertyName == "SelectedOrder")
            {
                order = settings.SelectedOrder;
            }
            else if (propertyChangedEventArgs.PropertyName == "RedBitmask")
            {
                rBitMask = GetBitmask(settings.RedBitmask);
            }
            else if (propertyChangedEventArgs.PropertyName == "GreenBitmask")
            {
                gBitMask = GetBitmask(settings.GreenBitmask);
            }
            else if (propertyChangedEventArgs.PropertyName == "BlueBitmask")
            {
                bBitMask = GetBitmask(settings.BlueBitmask);
            }
            else if (propertyChangedEventArgs.PropertyName == "ComplexityThreshold")
            {
                if (settings.ComplexityThreshold < 0 || settings.ComplexityThreshold > 1)
                {
                    GuiLogMessage("InvalidComplexityThresholdInput", NotificationLevel.Warning);
                }
            }
        }

        public void PostExecution()
        {

        }

        public void Stop()
        {
            if (settings.Action == ActionType.Hide && settings.ShowPresentation && InputImage != null)
            {
                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    if (settings.SelectedMode == ModeType.BPCS)
                    {
                        presentation.bpcs.DisableButtons();
                        presentation.bpcs.ShowMainMenu();
                    }
                    else
                    {
                        presentation.lsb.DisableButtons();
                        presentation.lsb.ShowMainMenu();
                    }
                }, null);
            }
        }

        public void Initialize()
        {
            if (settings.ShowPresentation)
            {
                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                if (settings.SelectedMode == ModeType.BPCS)
                {
                    presentation.bpcs = new BPCSPresentation(this);
                }
                else
                {
                    presentation.lsb = new LSBPresentation(this);
                }
                presentation.DisplayHidingProcessPresentation(settings.SelectedMode);
            }, null);
            }
        }

        public void Dispose()
        {
        }

        #endregion

        #region Hiding and extracting methods

        /// <summary>
        /// Hides a secret message in a cover image using lsb technique
        /// </summary>
        public void HideDataLSB()
        {
            outputBitmap = new Bitmap(inputBitmap);
            if (!settings.ShowPresentation)
            {
                rBitMask = GetBitmask(settings.RedBitmask);
                gBitMask = GetBitmask(settings.GreenBitmask);
                bBitMask = GetBitmask(settings.BlueBitmask);
            }

            BitArray messageBits = new BitArray(Encoding.UTF8.GetBytes(InputSecretMessage));
            int totalBitsToHide = messageBits.Length;
            int counter = 0;

            for (int y = 0; y < inputBitmap.Height; y++)
            {
                for (int x = 0; x < inputBitmap.Width; x++)

                {
                    Color currentPixel = inputBitmap.GetPixel(x, y);
                    BitArray redValueBits = new BitArray(new byte[] { currentPixel.R });
                    BitArray greenValueBits = new BitArray(new byte[] { currentPixel.G });
                    BitArray blueValueBits = new BitArray(new byte[] { currentPixel.B });

                    for (int k = 0; k < 8; k++)
                    {
                        if (counter == totalBitsToHide)
                        {
                            goto MessageDone;
                        }
                        if (rBitMask[k])
                        {
                            redValueBits[k] = messageBits[counter++];
                        }
                    }

                    for (int k = 0; k < 8; k++)
                    {
                        if (counter == totalBitsToHide)
                        {
                            goto MessageDone;
                        }
                        if (gBitMask[k])
                        {
                            greenValueBits[k] = messageBits[counter++];
                        }
                    }

                    for (int k = 0; k < 8; k++)
                    {
                        if (counter == totalBitsToHide)
                        {
                            goto MessageDone;
                        }
                        if (bBitMask[k])
                        {
                            blueValueBits[k] = messageBits[counter++];
                        }
                    }

                MessageDone:
                    byte newRedValue = ConvertToByte(redValueBits);
                    byte newGreenValue = ConvertToByte(greenValueBits);
                    byte newBlueValue = ConvertToByte(blueValueBits);
                    Color newColor = Color.FromArgb(newRedValue, newGreenValue, newBlueValue);
                    outputBitmap.SetPixel(x, y, newColor);
                }
            }
            // display warning if the hiding capacity is not enough
            if (counter < totalBitsToHide)
            {
                GuiLogMessage(Properties.Resources.NotEnoughHidingCapacity, NotificationLevel.Warning);
            }
            // Create header to encode message length and used bitmasks
            byte[] messageLengthBytes = BitConverter.GetBytes(totalBitsToHide);
            int lastX = outputBitmap.Width - 1;
            int lastY = outputBitmap.Height - 1;
            outputBitmap.SetPixel(lastX - 2, lastY, Color.FromArgb(ConvertToByte(rBitMask), ConvertToByte(gBitMask), ConvertToByte(bBitMask)));
            Color headerPixel1 = outputBitmap.GetPixel(lastX - 1, lastY);
            outputBitmap.SetPixel(lastX - 1, lastY, Color.FromArgb(headerPixel1.R, headerPixel1.G, messageLengthBytes[0]));
            Color headerPixel2 = Color.FromArgb(messageLengthBytes[1], messageLengthBytes[2], messageLengthBytes[3]);
            outputBitmap.SetPixel(lastX, lastY, headerPixel2);

            // Update output image component with new stego image 
            ImageConverter converter = new ImageConverter();
            byte[] outputbytes = (byte[])converter.ConvertTo(outputBitmap, typeof(byte[]));
            OutputImage = new CStreamWriter(outputbytes);
            OnPropertyChanged("OutputImage");
            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Extracts a message from a stego image which was hidden using the lsb technique
        /// </summary>
        public void ExtractDataLSB()
        {
            byte[] lengthBytes = new byte[4];
            int lastX = inputBitmap.Width - 1;
            int lastY = inputBitmap.Height - 1;
            rBitMask = new BitArray(new byte[] { inputBitmap.GetPixel(lastX - 2, lastY).R });
            gBitMask = new BitArray(new byte[] { inputBitmap.GetPixel(lastX - 2, lastY).G });
            bBitMask = new BitArray(new byte[] { inputBitmap.GetPixel(lastX - 2, lastY).B });
            Color lastPixel2 = inputBitmap.GetPixel(lastX - 1, lastY);
            lengthBytes[0] = lastPixel2.B;
            Color lastPixel1 = inputBitmap.GetPixel(lastX, lastY);
            lengthBytes[1] = lastPixel1.R;
            lengthBytes[2] = lastPixel1.G;
            lengthBytes[3] = lastPixel1.B;

            int totalBitsToExtract = BitConverter.ToInt32(lengthBytes, 0);
            BitArray bits = new BitArray(totalBitsToExtract);
            int counter = 0;

            for (int y = 0; y < inputBitmap.Height; y++)
            {
                for (int x = 0; x < inputBitmap.Width; x++)
                {
                    Color currentPixel = inputBitmap.GetPixel(x, y);
                    BitArray redValueBits = new BitArray(new byte[] { currentPixel.R });
                    BitArray greenValueBits = new BitArray(new byte[] { currentPixel.G });
                    BitArray blueValueBits = new BitArray(new byte[] { currentPixel.B });

                    for (int k = 0; k < 8; k++)
                    {
                        if (counter == totalBitsToExtract)
                        {
                            goto ExtractingDone;
                        }
                        if (rBitMask[k])
                        {
                            bits[counter++] = redValueBits[k];
                        }
                    }

                    for (int k = 0; k < 8; k++)
                    {
                        if (counter == totalBitsToExtract)
                        {
                            goto ExtractingDone;
                        }
                        if (gBitMask[k])
                        {
                            bits[counter++] = greenValueBits[k];
                        }
                    }

                    for (int k = 0; k < 8; k++)
                    {
                        if (counter == totalBitsToExtract)
                        {
                            goto ExtractingDone;
                        }
                        if (bBitMask[k])
                        {
                            bits[counter++] = blueValueBits[k];
                        }
                    }
                }
            }
        ExtractingDone:
            byte[] messageBytes = ConvertToBytes(bits);
            OutputSecretMessage = Encoding.UTF8.GetString(messageBytes);
            OnPropertyChanged("OutputSecretMessage");
            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Hides a secret message in a cover image using bpcs technique
        /// </summary>
        public void HideDataBPCS()
        {
            int width = inputBitmap.Width;
            int height = inputBitmap.Height;

            // Create header message block and add it to arraylist messageblocks 
            BitArray messageBits = new BitArray(Encoding.UTF8.GetBytes(InputSecretMessage));
            long totalBitsToHide = messageBits.Length;
            BitArray lengthBits = new BitArray(BitConverter.GetBytes(totalBitsToHide));
            MessageBlock lengthMessageBlock = new MessageBlock(lengthBits);
            ArrayList messageBlocks = new ArrayList
            {
                lengthMessageBlock
            };

            // Create message blocks for the actual message and add them to arraylist messageblocks 
            int counter = 0;
            for (int j = 0; j < messageBits.Length; j += 63)
            {
                bool[] currentMessageBits = new bool[64];
                for (int k = 0; (j + k) < messageBits.Length && k < 63; k++)
                {
                    currentMessageBits[k] = messageBits[counter++];
                }
                messageBlocks.Add(new MessageBlock(currentMessageBits));
            }

            // Fetch CGC bits for all pixels by creating a Pixel instance for each pixel in the image
            Pixel[,] pixels = new Pixel[height, width];
            string orderString = order.ToString();
            int blueIndex = orderString.IndexOf('B');
            int greenIndex = orderString.IndexOf('G');
            int redIndex = orderString.IndexOf('R');

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    pixels[row, col] = new Pixel(inputBitmap.GetPixel(col, row), blueIndex, redIndex, greenIndex);
                }
            }

            // Generate 24 bit planes of the image from CGC bits of the pixels 
            Plane[] rgbPlanesPres = new Plane[24];
            Plane[] rgbPlanes = new Plane[24];
            for (int i = 0; i < 24; i++)
            {
                bool[,] rgbPlane = new bool[height, width];
                for (int j = 0; j < height; j++)
                {
                    for (int k = 0; k < width; k++)
                    {
                        rgbPlane[j, k] = pixels[j, k].getCGCBit(i);
                    }
                }
                rgbPlanesPres[i] = new Plane(rgbPlane, i);
                rgbPlanes[i] = new Plane(rgbPlane, i);
            }

            // Retrieve all complex blocks from the bit planes and add them to hiderblocks arraylist
            ArrayList hiderBlocks = new ArrayList();
            ArrayList hiderBlocksPres = new ArrayList();
            for (int i = 0; i < 24; i++)
            {
                hiderBlocks.AddRange(rgbPlanes[i].GetComplexBlocksOfPlane());
                hiderBlocksPres.AddRange(rgbPlanesPres[i].GetComplexBlocksOfPlane());
            }

            // Retrieve all image blocks to display in the presentation
            ArrayList allImageBlocksPres = new ArrayList();
            for (int i = 0; i < 24; i++)
            {
                allImageBlocksPres.AddRange(rgbPlanesPres[i].GetAllImageBlocks());
            }

            // Initialize bit planes and image blocks in presentation if it is activated
            if (settings.ShowPresentation)
            {
                presentation.bpcs.InitBitPlanes(rgbPlanes);
                presentation.bpcs.InitMessageAndImageBlocks(hiderBlocksPres, messageBlocks, allImageBlocksPres);
            }

            // Display warning if hiding capacity is not enough to hide the entire message
            if (hiderBlocks.Count < messageBlocks.Count)
            {
                GuiLogMessage(Properties.Resources.NotEnoughHidingCapacity, NotificationLevel.Warning);
            }

            // Replace complex blocks with message blocks
            for (int j = 0; j < messageBlocks.Count; j++)
            {
                ((ImageBlock)hiderBlocks[j]).ReplaceWith((MessageBlock)messageBlocks[j]);
            }

            // Build the stego image by retreiving the new PBC bits of the pixels 
            outputBitmap = new Bitmap(width, height);
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    bool[] rgb = new bool[24];
                    for (int i = 0; i < 24; i++)
                    {
                        rgb[i] = rgbPlanes[i].GetBit(row, col);
                    }
                    Pixel pixel = new Pixel(rgb, blueIndex, redIndex, greenIndex);
                    outputBitmap.SetPixel(col, row, Color.FromArgb(pixel.GetRedValue(), pixel.GetGreenValue(), pixel.GetBlueValue()));
                }
            }

            // Update output image component with new stego image 
            ImageConverter converter = new ImageConverter();
            byte[] outputbytes = (byte[])converter.ConvertTo(outputBitmap, typeof(byte[]));
            OutputImage = new CStreamWriter(outputbytes);
            OnPropertyChanged("OutputImage");
            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Extracts a message from a stego image which was hidden using the bpcs technique
        /// </summary>
        public void ExtractDataBPCS()
        {
            int height = inputBitmap.Height;
            int width = inputBitmap.Width;

            // Fetch CGC bits for all pixels by creating a Pixel instance for each pixel in the image
            Pixel[,] pixels = new Pixel[height, width];
            string orderString = order.ToString();
            int blueIndex = orderString.IndexOf('B');
            int greenIndex = orderString.IndexOf('G');
            int redIndex = orderString.IndexOf('R');

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    pixels[row, col] = new Pixel(inputBitmap.GetPixel(col, row), blueIndex, redIndex, greenIndex);
                }
            }

            // Generate 24 bit planes of the image from CGC bits of the pixels 
            Plane[] rgbPlanes = new Plane[24];
            for (int i = 0; i < 24; i++)
            {
                bool[,] rgb = new bool[height, width];
                for (int j = 0; j < height; j++)
                {
                    for (int k = 0; k < width; k++)
                    {
                        rgb[j, k] = pixels[j, k].getCGCBit(i);
                    }
                }
                rgbPlanes[i] = new Plane(rgb, i);
            }

            // Retrieve all complex blocks from the bit planes and add them to hiderblocks arraylist
            ArrayList hiderBlocks = new ArrayList();
            for (int i = 0; i < 24; i++)
            {
                hiderBlocks.AddRange(rgbPlanes[i].GetComplexBlocksOfPlane());
            }

            // Get header message block to determine length of the message
            ImageBlock lengthBlock = (ImageBlock)hiderBlocks[0];
            lengthBlock.GetOriginal();
            bool[] lengthBoolArray = lengthBlock.GetArray();
            byte[] lengthBytes = ConvertToBytes(new BitArray(lengthBoolArray));
            long totalBits = BitConverter.ToInt64(lengthBytes, 0);

            // Retrieve message bits from complex image blocks, exit loop when total bits of message length is achieved
            bool[] messageBits = new bool[totalBits];
            int counter = 0;
            for (int i = 1; i < hiderBlocks.Count; i++)
            {
                bool[,] imageBlock = ((ImageBlock)hiderBlocks[i]).GetBlockArray();

                if (imageBlock[0, 0] == true)
                {
                    ((ImageBlock)hiderBlocks[i]).GetOriginal();
                }
                imageBlock = ((ImageBlock)hiderBlocks[i]).GetBlockArray();
                for (int j = 0; j < 8; j++)
                {
                    for (int k = (j == 0) ? 1 : 0; k < 8; k++)
                    {
                        if (counter == totalBits)
                        {
                            goto MessageDone;
                        }
                        else
                        {
                            messageBits[counter++] = imageBlock[j, k];
                        }
                    }
                }
            }

        // Build message from bits and update output message component with the result 
        MessageDone:
            BitArray bits = new BitArray(messageBits);
            byte[] messageBytes = ConvertToBytes(bits);
            OutputSecretMessage = Encoding.UTF8.GetString(messageBytes);
            OnPropertyChanged("OutputSecretMessage");
            ProgressChanged(1, 1);
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Get bitmask from binary string selected from the combobox in the settings
        /// </summary>
        private BitArray GetBitmask(string binaryString)
        {
            int value = Convert.ToInt32(binaryString);
            BitArray bits = new BitArray(new int[] { value });
            BitArray bitMask8bits = new BitArray(8);
            for (int i = 0; i < 8; i++)
            {
                bitMask8bits[i] = bits[i];
            }
            return bitMask8bits;
        }

        /// <summary>
        /// Converts bitarray to a byte
        /// </summary>
        private byte ConvertToByte(BitArray bits)
        {
            byte[] bytes = new byte[1];
            bits.CopyTo(bytes, 0);
            return bytes[0];
        }

        /// <summary>
        /// Converts bitarray to a byte array
        /// </summary>
        private byte[] ConvertToBytes(BitArray bits)
        {
            byte[] bytes = new byte[bits.Length / 8 + 1];
            bits.CopyTo(bytes, 0);
            return bytes;
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
    }

    /// <summary>
    /// Class that represents a message block which later replaces an image block in the cover image
    /// </summary>
    public class MessageBlock
    {
        private readonly bool[,] block;
        private bool conjugated;

        /// <summary>
        /// Constructor 1 for actual message
        /// </summary>
        public MessageBlock(bool[] block)
        {
            int k = 0;
            this.block = new bool[8, 8];
            for (int i = 0; i < 8; i++)
            {
                for (int j = (i == 0) ? 1 : 0; j < 8; j++)
                {
                    this.block[i, j] = block[k++];
                }
            }
            if (!IsComplex())
            {
                Conjugate();
            }
        }

        /// <summary>
        /// Constructor 2 for the length of the secret message
        /// </summary>
        public MessageBlock(BitArray lengthBits)
        {
            int q = 0;
            block = new bool[8, 8];
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    block[i, j] = lengthBits[q++];
                }
            }
            Conjugate();
        }

        /// <summary>
        /// Computes complexity of message block
        /// </summary>
        public double GetComplexity()
        {
            return (GetBorder() / 112.0);
        }

        /// <summary>
        /// Computes border of the message block used to compute its complexity
        /// </summary>
        public int GetBorder()
        {
            int changes = 0;
            for (int row = 0; row < 8; row++)
            {
                for (int col = 1; col < 8; col++)
                {
                    if (block[row, col] != block[row, col - 1])
                    {
                        changes++;
                    }
                }
            }
            for (int col = 0; col < 8; col++)
            {
                for (int row = 1; row < 8; row++)
                {
                    if (block[row, col] != block[row - 1, col])
                    {
                        changes++;
                    }
                }
            }
            return changes;
        }

        public bool IsComplex()
        {
            return GetComplexity() > ImageSteganographyVisualization.complexityThreshold;
        }

        /// <summary>
        /// Conjugates a message block in case the complexity was under the selected complexity threshold
        /// </summary>
        public void Conjugate()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    block[row, col] ^= ((row % 2 == 0) ? ((col % 2 == 0) ? false : true) : ((col % 2 == 0) ? true : false));
                }

            }
            block[0, 0] = true;
            conjugated = true;
        }

        public bool IsConjugated()
        {
            return conjugated;
        }

        public bool[,] GetBlock()
        {
            return block;
        }
    }

    /// <summary>
    /// Class that represents an image block (local areas) in the cover image
    /// </summary>
    public class ImageBlock
    {
        private readonly int row, col, layer;
        private readonly bool[,] plane;

        /// <summary>
        /// Constructor
        /// </summary>
        public ImageBlock(int row, int col, bool[,] plane, int layer)
        {
            this.row = row;
            this.col = col;
            this.plane = plane;
            this.layer = layer;
        }

        /// <summary>
        /// Returns bits of the image block in form of a two-dimensional bool array
        /// </summary>
        public bool[,] GetBlockArray()
        {
            bool[,] blockArray = new bool[8, 8];
            for (int i = row; i < row + 8; i++)
            {
                for (int j = col; j < col + 8; j++)
                {
                    blockArray[i - row, j - col] = plane[i, j];
                }
            }
            return blockArray;
        }

        /// <summary>
        /// Returns bits of the image block in form of a bool array
        /// </summary>
        public bool[] GetArray()
        {
            bool[] arr = new bool[64];
            int counter = 0;
            for (int i = row; i < row + 8; i++)
            {
                for (int j = col; j < col + 8; j++)
                {
                    arr[counter++] = plane[i, j];
                }
            }
            return arr;
        }

        public int GetLayer()
        {
            return layer;
        }
        public int GetCol()
        {
            return col;
        }
        public int GetRow()
        {
            return row;
        }

        /// <summary>
        /// Retrieves the original image block if it was conjugated
        /// </summary>
        public void GetOriginal()
        {
            for (int i = row; i < row + 8; i++)
            {
                for (int j = col; j < col + 8; j++)
                {
                    plane[i, j] ^= ((i % 2 == 0) ? ((j % 2 == 0) ? false : true) : ((j % 2 == 0) ? true : false));
                }
            }
        }

        /// <summary>
        /// Computes border of an image block used to compute its complexity
        /// </summary>
        public int GetBorder()
        {
            int changes = 0;
            for (int i = row; i < row + 8; i++)
            {
                for (int j = col + 1; j < col + 8; j++)
                {
                    if (plane[i, j] != plane[i, j - 1])
                    {
                        changes++;
                    }
                }
            }
            for (int j = col; j < col + 8; j++)
            {
                for (int i = row + 1; i < row + 8; i++)
                {
                    if (plane[i, j] != plane[i - 1, j])
                    {
                        changes++;
                    }
                }
            }
            return changes;
        }

        /// <summary>
        /// Computes complexity of image block
        /// </summary>
        public double GetComplexity()
        {
            return (GetBorder() / 112.0);
        }

        public bool IsComplex()
        {
            return (GetBorder() / 112.0) > ImageSteganographyVisualization.complexityThreshold;
        }

        /// <summary>
        /// Replaces an image block with a message block provided as a paramater for the method
        /// </summary>
        public void ReplaceWith(MessageBlock data)
        {
            bool[,] temp = data.GetBlock();

            for (int i = row; i < row + 8; i++)
            {
                for (int j = col; j < col + 8; j++)
                {
                    plane[i, j] = temp[i - row, j - col];
                }
            }
        }

    }

    /// <summary>
    /// Class that represents a bit plane in an image 
    /// </summary>
    public class Plane
    {
        private readonly int layer;
        private readonly bool[,] plane;
        private readonly ArrayList imageBlocks;

        /// <summary>
        /// Constructor
        /// </summary>
        public Plane(bool[,] plane, int layer)
        {
            this.plane = (bool[,])plane.Clone();
            this.layer = layer;
            imageBlocks = new ArrayList();
            for (int row = 0; row < this.plane.GetUpperBound(0) - 8; row += 8)
            {
                for (int col = 0; col < this.plane.GetUpperBound(1) - 8; col += 8)
                {
                    imageBlocks.Add(new ImageBlock(row, col, this.plane, layer));
                }
            }
        }

        public bool GetBit(int row, int col)
        {
            return plane[row, col];
        }

        public bool[,] GetPlane()
        {
            return plane;
        }

        public int GetLayer()
        {
            return layer;
        }
        public int GetWidth()
        {
            return plane.GetUpperBound(0);
        }

        public int GetHeight()
        {
            return plane.GetUpperBound(1);
        }

        public ArrayList GetAllImageBlocks()
        {
            return imageBlocks;
        }

        /// <summary>
        /// Computes complexity of image blocks of the bit planes and adds the complex image blocks
        /// </summary>
        public ArrayList GetComplexBlocksOfPlane()
        {
            ArrayList complexBlocks = new ArrayList();
            for (int i = 0; i < imageBlocks.Count; i++)
            {
                if (((ImageBlock)imageBlocks[i]).IsComplex())
                {
                    complexBlocks.Add(imageBlocks[i]);
                }
            }
            return complexBlocks;
        }
    }

    /// <summary>
    /// Class that represents a pixel in an image and stores the pbc and cgc bits of the pixel
    /// </summary>
    public class Pixel
    {
        private readonly byte red, green, blue;
        private readonly bool[] cgcBits;

        /// <summary>
        /// Constructor 1: computes and stores the cgc bits of the pixel from its original pbc bits
        /// </summary>
        public Pixel(Color c, int blueIndex, int redIndex, int greenIndex)
        {
            green = c.G;
            red = c.R;
            blue = c.B;

            cgcBits = new bool[24];
            bool[] blueCGC = GetCGC(blue);
            bool[] greenCGC = GetCGC(green);
            bool[] redCGC = GetCGC(red);

            int blueCounter = 0;
            int greenCounter = 0;
            int redeCounter = 0;

            for (int i = 0; i < 24; i++)
            {
                if (i % 3 == blueIndex)
                {
                    cgcBits[i] = blueCGC[blueCounter++];
                }
                else if (i % 3 == greenIndex)
                {
                    cgcBits[i] = greenCGC[greenCounter++];
                }
                else if (i % 3 == redIndex)
                {
                    cgcBits[i] = redCGC[redeCounter++];
                }
            }
        }

        /// <summary>
        /// Constructor 2: computes the bpc bits of the pixel from the modified cgc bits after embedding the message blocks
        /// </summary>
        public Pixel(bool[] rgbPlanes, int blueIndex, int redIndex, int greenIndex)
        {
            bool[] cgcBlue = new bool[8];
            bool[] cgcGreen = new bool[8];
            bool[] cgcRed = new bool[8];

            int blueCounter = 0;
            int greenCounter = 0;
            int redeCounter = 0;

            for (int i = 0; i < 24; i++)
            {
                if (i % 3 == blueIndex)
                {
                    cgcBlue[blueCounter++] = rgbPlanes[i];
                }
                else if (i % 3 == greenIndex)
                {
                    cgcGreen[greenCounter++] = rgbPlanes[i];
                }
                else if (i % 3 == redIndex)
                {
                    cgcRed[redeCounter++] = rgbPlanes[i];
                }
            }
            red = ConvertToByte(GetPBC(cgcRed));
            blue = ConvertToByte(GetPBC(cgcBlue));
            green = ConvertToByte(GetPBC(cgcGreen));
        }

        /// <summary>
        /// Converts bool array to a byte
        /// </summary>
        private byte ConvertToByte(bool[] arr)
        {
            BitArray bits = new BitArray(arr);
            byte[] bytes = new byte[1];
            bits.CopyTo(bytes, 0);
            return bytes[0];
        }

        /// <summary>
        /// Computes cgc bits from original pbc bits
        /// </summary>
        public bool[] GetCGC(byte b)
        {
            bool[] cgc = new bool[8];
            BitArray PBCbits = new BitArray(new byte[] { b });
            cgc[7] = PBCbits[7];
            for (int i = 6; i >= 0; i--)
            {
                cgc[i] = PBCbits[i] ^ PBCbits[i + 1];
            }
            return cgc;
        }

        /// <summary>
        /// Computes pbc bits from modified cgc bits
        /// </summary>
        public bool[] GetPBC(bool[] b)
        {
            bool[] bpc = new bool[8];
            BitArray CGCbits = new BitArray(b);
            bpc[7] = CGCbits[7];
            for (int i = 6; i >= 0; i--)
            {
                bpc[i] = bpc[i + 1] ^ CGCbits[i];
            }
            return bpc;
        }

        public bool getCGCBit(int index)
        {
            return cgcBits[index];
        }

        public byte GetRedValue()
        {
            return red;
        }
        public byte GetGreenValue()
        {
            return green;
        }
        public byte GetBlueValue()
        {
            return blue;
        }
    }
}
