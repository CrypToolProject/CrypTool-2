/*                              
   Copyright 2011 Nils Kopal, Uni Duisburg-Essen

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
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PictureOutput
{
    [Author("Nils Kopal", "nils.kopal@CrypTool.org", "Uni Duisburg", "http://www.uni-duisburg-essen.de")]
    [PluginInfo("PictureOutput.Properties.Resources", "PluginCaption", "PluginTooltip", "PictureOutput/DetailedDescription/doc.xml", "PictureOutput/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class PictureOutput : ICrypComponent
    {
        private readonly PictureOutputPresentation _presentation = null;
        private byte[] _data = null;
        private ICrypToolStream _stream = null;

        public PictureOutput()
        {
            _presentation = new PictureOutputPresentation();
        }

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public ISettings Settings => null;

        public System.Windows.Controls.UserControl Presentation => _presentation;

        public void PreExecution()
        {
            _data = null;
            _stream = null;
            _presentation.Dispatcher.Invoke(DispatcherPriority.Background, (SendOrPostCallback)delegate
            {
                try
                {
                    _presentation.Picture.Source = null;
                }
                catch (Exception ex)
                {
                    GuiLogMessage("Could not clear picture in PreExecute: " + ex.Message, NotificationLevel.Error);
                }
            }, null);
        }

        public void Execute()
        {
            try
            {
                ProgressChanged(0, 0);
                if (_data == null && _stream == null)
                {
                    return;
                }

                if (_stream != null)
                {
                    CStreamReader reader = _stream.CreateReader();
                    _data = reader.ReadFully();
                }

                _presentation.Dispatcher.Invoke(DispatcherPriority.Background, (SendOrPostCallback)delegate
                {
                    try
                    {

                        BitmapDecoder decoder = BitmapDecoder.Create(new MemoryStream(_data),
                                BitmapCreateOptions.PreservePixelFormat,
                                BitmapCacheOption.None);

                        if (decoder.Frames.Count > 0)
                        {
                            _presentation.Picture.Source = decoder.Frames[0];
                        }
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage("Could not display picture: " + ex.Message, NotificationLevel.Error);
                        return;
                    }
                }, null);
                ProgressChanged(1, 1);
            }
            catch (Exception ex)
            {
                GuiLogMessage("Could not display picture: " + ex.Message, NotificationLevel.Error);
            }
        }

        public void PostExecution()
        {

        }

        public void Stop()
        {

        }

        public void Initialize()
        {

        }

        public void Dispose()
        {

        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        [PropertyInfo(Direction.InputData, "pictureInputCaption", "pictureInputTooltip", false)]
        public byte[] PictureInput
        {
            set => _data = value;
        }

        [PropertyInfo(Direction.InputData, "pictureInputCaption", "pictureInputTooltip", false)]
        public ICrypToolStream PictureStream
        {
            set => _stream = value;
        }

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, logLevel));
            }
        }

        public void ProgressChanged(double value, double max)
        {
            if (OnPluginProgressChanged != null)
            {
                OnPluginProgressChanged(this, new PluginProgressEventArgs(value, max));
            }
        }
    }
}
