/*
   Copyright 2019 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using System.ComponentModel;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using Webcam;

namespace CrypTool.Plugins.Webcam
{
    [Author("Mirko Sartorius", "mirkosartorius@web.de", "CrypTool 2 Team", "university of kassel")]
    [PluginInfo("WebCam.Properties.Resources", "PluginCaption", "PluginTooltip", "WebCam/userdoc.xml", new[] { "Webcam/images/webcam.png" })]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class Webcam : ICrypComponent
    {
        #region Private Variables

        private readonly WebcamSettings _settings = new WebcamSettings();
        private readonly WebcamPresentation _presentation = new WebcamPresentation();
        private bool takeImage;
        private bool running = false;

        public Webcam()
        {

        }

        #endregion

        #region Data Properties

        /// <summary>
        /// </summary>
        [PropertyInfo(Direction.OutputData, "ImageOutPutCaption", "ImageOutPutTooltip")]
        public byte[] ImageOutput
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "SingleOutPutCaption", "SingleOutPutTooltip")]
        public byte[] SingleOutPut
        {
            get;
            set;
        }


        [PropertyInfo(Direction.InputData, "TakeImageCaption", "TakeImageTooltip", false)]
        public bool TakeImage
        {
            get => takeImage;
            set
            {
                takeImage = value;
                CaptureImage();
            }
        }

        /// <summary>
        /// Takes a single picture and outputs it to the "single output"
        /// triggered by the TakeImage (boolean) input
        /// </summary>
        private void CaptureImage()
        {
            if (running)
            {
                bool takeit = false;
                switch (_settings.TakeImageChoice)
                {
                    case 0://if takeImage == true OR takeImage == false
                    default:
                        takeit = true;
                        break;
                    case 1://if takeImage == true
                        takeit = takeImage;
                        break;
                    case 2://if takeImage == false
                        takeit = !takeImage;
                        break;
                }

                if (takeit)
                {
                    byte[] data = null;
                    _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        data = _presentation.CaptureImage();
                    }, null);

                    if (data != null)
                    {
                        SingleOutPut = data;
                        OnPropertyChanged("SingleOutPut");
                    }
                }
            }
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings => _settings;

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation => _presentation;

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);
            if (!running)
            {
                _presentation.WebcamSettings = _settings;
                running = true;
                Thread thread = new Thread(CaptureThread)
                {
                    IsBackground = true
                };
                thread.Start();
            }
            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Thread for capturing images
        /// </summary>
        private void CaptureThread()
        {
            // 1) start camera
            _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _presentation.Start(_settings.DeviceChoice);
            }, null);

            // 2) loop capturing images
            while (running)
            {
                byte[] data = null;
                _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    data = _presentation.CaptureImage();
                }, null);

                if (data != null)
                {
                    ImageOutput = data;
                    OnPropertyChanged("ImageOutput");
                }
                Thread.Sleep((int)(1000.0 / _settings.CaptureFrequency));
            }

            // 3) stop camera
            _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _presentation.Stop();
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
            running = false;
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
