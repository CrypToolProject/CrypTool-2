/*
    Copyright 2013 Christopher Konze, University of Kassel
 
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
using CrypTool.Plugins.VisualDecoder.Decoders;
using CrypTool.Plugins.VisualDecoder.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using ZXing;

namespace CrypTool.Plugins.VisualDecoder
{
    [Author("Christopher Konze", "Christopher.Konze@CrypTool.org", "University of Kassel", "http://www.uni-kassel.de/eecs/")]
    [PluginInfo("VisualDecoder.Properties.Resources", "PluginCaption", "PluginTooltip", "VisualDecoder/userdoc.xml", new[] { "VisualDecoder/Images/icon.png" })]
    [ComponentCategory(ComponentCategory.ToolsCodes)]
    public class VisualDecoder : ICrypComponent
    {

        #region Private Variables

        private readonly VisualDecoderPresentation presentation = new VisualDecoderPresentation();
        private readonly VisualDecoderSettings settings = new VisualDecoderSettings();
        private Thread decodingThread;
        private readonly ParameterizedThreadStart threadStart;
        private bool codeFound;

        // decoder chain
        private readonly Dictionary<VisualDecoderSettings.DimCodeType, DimCodeDecoder> codeTypeHandler =
                                                        new Dictionary<VisualDecoderSettings.DimCodeType, DimCodeDecoder>();


        #endregion

        public VisualDecoder()
        {
            threadStart = ProcessInput;

            //init chain
            codeTypeHandler.Add(VisualDecoderSettings.DimCodeType.EAN8, new ZXingDecoder(this, BarcodeFormat.EAN_8));
            codeTypeHandler.Add(VisualDecoderSettings.DimCodeType.EAN13, new ZXingDecoder(this, BarcodeFormat.EAN_13));
            codeTypeHandler.Add(VisualDecoderSettings.DimCodeType.Code39, new ZXingDecoder(this, BarcodeFormat.CODE_39));
            codeTypeHandler.Add(VisualDecoderSettings.DimCodeType.Code128, new ZXingDecoder(this, BarcodeFormat.CODE_128));
            codeTypeHandler.Add(VisualDecoderSettings.DimCodeType.QRCode, new ZXingDecoder(this, BarcodeFormat.QR_CODE));
            codeTypeHandler.Add(VisualDecoderSettings.DimCodeType.PDF417, new ZXingDecoder(this, BarcodeFormat.PDF_417));
            codeTypeHandler.Add(VisualDecoderSettings.DimCodeType.DataMatrix, new ZXingDecoder(this, BarcodeFormat.DATA_MATRIX));
            codeTypeHandler.Add(VisualDecoderSettings.DimCodeType.MaxiCode, new ZXingDecoder(this, BarcodeFormat.MAXICODE));
            codeTypeHandler.Add(VisualDecoderSettings.DimCodeType.Aztec, new ZXingDecoder(this, BarcodeFormat.AZTEC));
            codeTypeHandler.Add(VisualDecoderSettings.DimCodeType.CodaBar, new ZXingDecoder(this, BarcodeFormat.CODABAR));
            codeTypeHandler.Add(VisualDecoderSettings.DimCodeType.RSS, new ZXingDecoder(this, BarcodeFormat.RSS_EXPANDED));
            codeTypeHandler.Add(VisualDecoderSettings.DimCodeType.UPC_A, new ZXingDecoder(this, BarcodeFormat.UPC_A));
            codeTypeHandler.Add(VisualDecoderSettings.DimCodeType.UPC_E, new ZXingDecoder(this, BarcodeFormat.UPC_E));
        }


        #region Data Properties

        [PropertyInfo(Direction.InputData, "PictureInputCaption", "PictureInputTooltip")]
        public byte[] PictureInput
        {
            get;
            set;
        }


        [PropertyInfo(Direction.OutputData, "OutputDataCaption", "OutputDataTooltip")]
        public string OutputData
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
        public UserControl Presentation => presentation;

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
            {
                try
                {
                    presentation.ClearPresentation();
                }
                catch (Exception e)
                {
                    GuiLogMessage(e.Message, NotificationLevel.Error);
                }
            }), null);

            codeFound = false;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0.1, 1);
            if (decodingThread == null || !decodingThread.IsAlive   // decoding thread is idle
                && (!codeFound || !settings.StopOnSuccess)) // stop if setting is selected and we decoded  something  
            {
                ProgressChanged(0.5, 1);
                decodingThread = new Thread(threadStart); // unfortunately we cant resart a thread and a threadpool with just one thread
                                                          // would produce more overhead, hence we have to create a new thread
                decodingThread.Start(PictureInput);
            }
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

        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        /// <summary>
        /// This Methode decodes the given Image and updates the outputs and presentation. 
        /// </summary>
        /// <param name="image"></param>
        public void ProcessImage(byte[] image)
        {
            DimCodeDecoderItem dimCode = null;
            if (settings.DecodingType != VisualDecoderSettings.DimCodeType.AUTO)
            {
                dimCode = codeTypeHandler[settings.DecodingType].Decode(image);
            }
            else // 'automatic' mode (try all decoder)
            {
                foreach (KeyValuePair<VisualDecoderSettings.DimCodeType, DimCodeDecoder> decoder in codeTypeHandler)
                {
                    dimCode = decoder.Value.Decode(image);
                    if (dimCode != null)
                    {
                        break;
                    }
                }
            }

            if (dimCode != null) //input is valid and has been decoded
            {
                //update Presentation
                presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
                {
                    try
                    {
                        presentation.SetImages(dimCode.BitmapWithMarkedCode);
                        presentation.SetData(dimCode.CodePayload, dimCode.CodeType);
                    }
                    catch (Exception e)
                    {
                        GuiLogMessage(e.Message, NotificationLevel.Error);
                    }
                }), null);

                //update output
                OutputData = dimCode.CodePayload;
                OnPropertyChanged("OutputData");

                //update Progress
                ProgressChanged(1, 1);

                codeFound = true;
            }
            else
            {
                //reset metadata and set image
                presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
                {
                    try
                    {
                        presentation.ClearPresentation();
                        presentation.SetImages(PictureInput);
                    }
                    catch (Exception e)
                    {
                        GuiLogMessage(e.Message, NotificationLevel.Error);
                    }
                }), null);
            }
        }

        /// <summary>
        ///  Execution Methode of the DecodingThread
        ///  catches errors
        /// </summary>
        /// <param name="image">has to be the bytearray representation of an image, otherwise it will be ignored</param>
        public void ProcessInput(object image)
        {
            try
            {
                byte[] curImage = image as byte[];
                if (curImage != null && curImage.Length != 0)
                {
                    //we did the null and lengthcheck but the only way to know for sure that curImage is a byte[] image is to convert it.
                    codeTypeHandler[VisualDecoderSettings.DimCodeType.QRCode].ByteArrayToImage(curImage);
                    ProcessImage(curImage);
                }

            }
            catch (Exception e)
            {
                GuiLogMessage("Error in DecodingThread: " + e.Message, NotificationLevel.Error);
            }
        }


        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public void GuiLogMessage(string message, NotificationLevel logLevel)
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
