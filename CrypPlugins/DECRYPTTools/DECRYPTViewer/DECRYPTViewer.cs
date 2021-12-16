/*
   Copyright 2018 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using CrypTool.Plugins.DECRYPTTools.Util;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.Plugins.DECRYPTTools
{
    [Author("Nils Kopal", "nils.kopal@CrypTool.org", "CrypTool 2 Team", "https://www.CrypTool.org")]
    [PluginInfo("CrypTool.Plugins.DECRYPTTools.Properties.Resources", "DecodeViewerPluginCaption", "DecodeViewerPluginTooltip", "DECRYPTTools/userdoc.xml", "DECRYPTTools/icon.png")]
    [ComponentCategory(ComponentCategory.DECRYPTProjectComponent)]
    public class DECRYPTViewer : ICrypComponent
    {
        #region Private Variables
        private readonly DECRYPTViewerSettings _settings;
        private readonly DECRYPTViewerPresentation _presentation;
        private bool _running;
        private Thread _workerThread;
        private readonly DownloadProgress _downloadProgress = new DownloadProgress();

        #endregion

        /// <summary>
        /// Creats a new DECRYPT Viewer
        /// </summary>
        public DECRYPTViewer()
        {
            _settings = new DECRYPTViewerSettings();
            _presentation = new DECRYPTViewerPresentation(this);
            _downloadProgress.NewDownloadProgress += _downloadProgress_NewDownloadProgress;
        }

        #region Data Properties

        /// <summary>
        /// Input of a json record of the DECRYPT database
        /// </summary>
        [PropertyInfo(Direction.InputData, "DecodeRecordCaption", "DecodeRecordTooltip")]
        public string DECRYPTRecord
        {
            get;
            set;
        }

        /// <summary>
        /// Outputs a selected Image in a CrypToolStream
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputImageCaption", "OutputImageTooltip")]
        public ICrypToolStream OutputImage
        {
            get;
            set;
        }

        /// <summary>
        /// Outputs a selected document as byte array
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputDocumentCaption", "OutputDocumentTooltip")]
        public byte[] OutputDocument
        {
            get;
            set;
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
            _presentation.Record = new Record();
            _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    _presentation.ImageList.Items.Clear();
                    _presentation.DocumentList.Items.Clear();
                }
                catch (Exception)
                {
                    //wtf?
                }
            }, null);
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            if (!JsonDownloaderAndConverter.IsLoggedIn())
            {
                string username = null;
                string password = null;
                try
                {
                    username = DECRYPTSettingsTab.GetUsername();
                    password = DECRYPTSettingsTab.GetPassword();
                }
                catch (Exception)
                {
                    //do nothing
                }

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    username = "anonymous";
                    password = "anonymous";
                }
                try
                {
                    bool loginSuccess = JsonDownloaderAndConverter.Login(username, password);
                    if (!loginSuccess)
                    {
                        GuiLogMessage(Properties.Resources.LoginFailed, NotificationLevel.Error);
                    }
                }
                catch (Exception ex)
                {
                    GuiLogMessage(ex.Message, NotificationLevel.Warning);
                }
            }
            _running = false;
            if (_workerThread == null)
            {
                //create a new thread if we have none
                _workerThread = new Thread(new ParameterizedThreadStart(ExecuteThread))
                {
                    IsBackground = true
                };
                _workerThread.Start(DECRYPTRecord);
            }
            else
            {
                //wait for current thread to stop
                while (_workerThread.IsAlive)
                {
                    Thread.Sleep(10);
                }
                //start a new one
                _workerThread = new Thread(new ParameterizedThreadStart(ExecuteThread))
                {
                    IsBackground = true
                };
                _workerThread.Start(DECRYPTRecord);
            }
        }

        /// <summary>
        /// Thread for executing viewer
        /// We use this to allow restart during execution
        /// </summary>
        private void ExecuteThread(object decodeRecord)
        {
            _running = true;
            ProgressChanged(0, 1);
            Record record;
            try
            {
                record = JsonDownloaderAndConverter.ConvertStringToRecord((string)decodeRecord);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Could not convert data from DECRYPT database: {0}", ex.Message), NotificationLevel.Error);
                return;
            }
            try
            {
                _presentation.Record = record;
                //remove all images from the list view
                _presentation.Dispatcher.Invoke(DispatcherPriority.Background, (SendOrPostCallback)delegate
                {
                    _presentation.IsEnabled = false;
                    _presentation.ImageList.Items.Clear();
                }, null);

                Document newestTranscription = null;
                foreach (Document document in record.documents.transcription)
                {
                    if (newestTranscription == null || newestTranscription.document_id < document.document_id)
                    {
                        newestTranscription = document;
                    }
                }

                if (newestTranscription != null)
                {
                    DoDownloadDocument(newestTranscription);
                }
                else
                {
                    OutputDocument = Encoding.UTF8.GetBytes("n/a");
                    OnPropertyChanged("OutputDocument");
                }

                ProgressChanged(0, 0);

                //add all documents to the ListView of documents
                _presentation.Dispatcher.Invoke(DispatcherPriority.Background, (SendOrPostCallback)delegate
                {
                    _presentation.DocumentList.Items.Clear();
                    foreach (Document document in record.documents.AllDocuments)
                    {
                        _presentation.DocumentList.Items.Add(document);
                        if (_running == false)
                        {
                            return;
                        }
                    }
                    //set checkboxes
                    //transcription
                    _presentation.HasTranscriptionCheckbox.IsChecked = record.documents.transcription.Count > 0;
                    //cryptanalysis_statistics
                    _presentation.HasStatisticsCheckbox.IsChecked = record.documents.cryptanalysis_statistics.Count > 0;
                    //deciphered_text
                    _presentation.HasDeciphermentCheckbox.IsChecked = record.documents.deciphered_text.Count > 0;
                    //translation
                    _presentation.HasTranslationCheckbox.IsChecked = record.documents.translation.Count > 0;

                    _presentation.IsEnabled = true;
                }, null);

                if (_settings.DownloadImages)
                {
                    //add all documents to the ListView of images
                    for (int i = 0; i < record.images.Count; i++)
                    {
                        if (!record.images[i].DownloadThumbnail())
                        {
                            break;
                        }
                        _presentation.Dispatcher.Invoke(DispatcherPriority.Background, (SendOrPostCallback)delegate
                        {
                            try
                            {
                                _presentation.ImageList.Items.Add(record.images[i]);
                            }
                            catch (Exception ex)
                            {
                                GuiLogMessage(string.Format("Exception while adding thumbnail to list: {0}", ex.Message), NotificationLevel.Error);
                            }
                        }, null);
                        ProgressChanged(i, record.images.Count);
                        if (_running == false)
                        {
                            return;
                        }
                    }
                }
                ProgressChanged(1, 1);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Error while adding data:{0}", ex.Message), NotificationLevel.Error);
                return;
            }
            ProgressChanged(1, 1);
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
            _running = false;
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
        /// Download and output an image
        /// </summary>
        /// <param name="image"></param>
        internal void DownloadImage(Util.Image image)
        {
            if (!_running)
            {
                return;
            }
            Task.Run(() =>
            {
                byte[] imageBytes = null;
                try
                {
                    imageBytes = image.GetFullImage(_downloadProgress);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format("Exception occured during downloading of image: {0}", ex.Message), NotificationLevel.Error);
                }
                if (imageBytes != null)
                {
                    OutputDownloadedImage(imageBytes);
                }
            });
        }

        /// <summary>
        /// Checks, if the image bytes are an html document (=> display not allowed) or an actual image and returns
        /// this
        /// </summary>
        /// <param name="imageBytes"></param>
        private void OutputDownloadedImage(byte[] imageBytes)
        {
            try
            {
                _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    _presentation.IsEnabled = false;
                }, null);
                string text = Encoding.UTF8.GetString(imageBytes);
                //this is a hacky check... if the database returns text instead of an image
                //the user is not allowed to download the image
                //we know, it is text, if it contains html and body texts
                if (text.ToLower().Contains("html") && text.ToLower().Contains("body"))
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        Bitmap bitmap = (Bitmap)Properties.Resources.DECRYPT_message.Clone();

                        RectangleF rectf = new RectangleF(50, 50, 450, 450);
                        Graphics graphics = Graphics.FromImage(bitmap);
                        graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        graphics.DrawString(Properties.Resources.image_not_available_for_download, new Font("Tahoma", 12), Brushes.Black, rectf);

                        bitmap.Save(stream, ImageFormat.Png);
                        bitmap.Dispose();
                        OutputImage = new CStreamWriter(stream.ToArray());
                        OnPropertyChanged("OutputImage");
                    }
                }
                else
                {
                    OutputImage = new CStreamWriter(imageBytes);
                    OnPropertyChanged("OutputImage");
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception occured during creation of image: {0}", ex.Message), NotificationLevel.Error);
            }
            finally
            {
                _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    _presentation.IsEnabled = true;
                }, null);
            }
        }

        /// <summary>
        /// Download and output a document
        /// </summary>
        /// <param name="document"></param>
        internal void DownloadDocument(Document document)
        {
            Task.Run(() => DoDownloadDocument(document));
        }

        /// <summary>
        /// Do download of a document
        /// </summary>
        /// <param name="document"></param>
        internal void DoDownloadDocument(Document document)
        {
            try
            {
                _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    _presentation.IsEnabled = false;
                }, null);

                OutputDocument = document.DownloadDocument(_downloadProgress);
                OnPropertyChanged("OutputDocument");
                //GuiLogMessage(String.Format("Downloaded {0} with id {1}", document.title, document.document_id), NotificationLevel.Info);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception during downloading of document: {0}", ex.Message), NotificationLevel.Error);
            }
            finally
            {
                _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    _presentation.IsEnabled = true;
                }, null);
            }
        }

        /// <summary>
        /// Fired, when download progress changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="downloadProgressEventArgs"></param>
        private void _downloadProgress_NewDownloadProgress(object sender, DownloadProgressEventArgs downloadProgressEventArgs)
        {
            ProgressChanged(downloadProgressEventArgs.BytesDownloaded, downloadProgressEventArgs.TotalBytes);
        }

    }
}
