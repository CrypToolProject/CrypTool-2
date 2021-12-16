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
using CrypTool.Plugins.VisualEncoder.Encoders;
using CrypTool.Plugins.VisualEncoder.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.Plugins.VisualEncoder
{
    [Author("Christopher Konze", "Christopher.Konze@CrypTool.org", "University of Kassel", "http://www.uni-kassel.de/eecs/")]
    [PluginInfo("VisualEncoder.Properties.Resources", "PluginCaption", "PluginTooltip", "VisualEncoder/userdoc.xml", new[] { "VisualEncoder/Images/icon.png" })]
    [ComponentCategory(ComponentCategory.ToolsCodes)]
    public class VisualEncoder : ICrypComponent
    {
        #region Const / Variables

        private readonly Dictionary<VisualEncoderSettings.DimCodeType, DimCodeEncoder> codeTypeHandler = new Dictionary<VisualEncoderSettings.DimCodeType, DimCodeEncoder>();
        private readonly VisualEncoderSettings settings;
        private readonly VisualEncoderPresentation presentation = new VisualEncoderPresentation();

        #endregion

        public VisualEncoder()
        {
            settings = new VisualEncoderSettings(this);

            codeTypeHandler.Add(VisualEncoderSettings.DimCodeType.EAN8, new EAN8(this));
            codeTypeHandler.Add(VisualEncoderSettings.DimCodeType.EAN13, new EAN13(this));
            codeTypeHandler.Add(VisualEncoderSettings.DimCodeType.Code39, new Code39(this));
            codeTypeHandler.Add(VisualEncoderSettings.DimCodeType.Code128, new Code128(this));
            codeTypeHandler.Add(VisualEncoderSettings.DimCodeType.QRCode, new QRCode(this));
            codeTypeHandler.Add(VisualEncoderSettings.DimCodeType.PDF417, new PDF417(this));
        }

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputStreamCaption", "InputStreamTooltip")]
        public string InputStream
        {
            get;
            set;
        }


        [PropertyInfo(Direction.OutputData, "PictureBytesCaption", "PictureBytesTooltip")]
        public byte[] PictureBytes
        {
            get;
            private set;
        }

        #endregion

        #region IPlugin Members
        #region std functions 
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
        }
        #endregion
        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);
            if (InputStream != null && InputStream.Length >= 1)
            {
                DimCodeEncoderItem dimCode = null;
                try
                {
                    dimCode = codeTypeHandler[settings.EncodingType].Encode(InputStream, settings);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(ex.Message, NotificationLevel.Error);
                    return;
                }
                if (dimCode != null) //input is valid
                {
                    //update Presentation
                    presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
                    {
                        try
                        {
                            presentation.SetImages(dimCode.PresentationBitmap, dimCode.PureBitmap);
                            presentation.SetList(dimCode.Legend);
                        }
                        catch (Exception e)
                        {
                            GuiLogMessage(e.Message, NotificationLevel.Error);
                        }
                    }), null);

                    //update output
                    PictureBytes = dimCode.PureBitmap;
                    OnPropertyChanged("PictureBytes");
                }
            }
            ProgressChanged(1, 1);
        }

        #region std functions
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
        #endregion

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
