/*
   Copyright 2008 Timm Korte, University of Siegen

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
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ClipboardInput
{
    // Retrieves data from clipboard and passes it on as a stream
    [Author("Timm Korte", "CrypTool@easycrypt.de", "Uni Bochum", "http://www.ruhr-uni-bochum.de")]
    [PluginInfo("ClipboardInput.Properties.Resources", "PluginCaption", "PluginTooltip", "ClipboardInput/DetailedDescription/doc.xml", "ClipboardInput/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class ClipboardInput : ICrypComponent
    {

        #region Private Variables
        private ClipboardInputSettings settings;

        public ISettings Settings
    {
      get => settings;
      set => settings = (ClipboardInputSettings)value;
    }

        #endregion

        public ClipboardInput()
        {
            settings = new ClipboardInputSettings();
        }


        private string data;
        // [QuickWatch(QuickWatchFormat.Text, null)]
        public string Data
    {
      get => data;
      set => data = value;// OnPropertyChanged("Data");
    }

        private void GetClipboardData()
        {
            try
            {
                Data = Clipboard.GetText();
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
        }

        #region Interface
        [PropertyInfo(Direction.OutputData, "StreamOutputCaption", "StreamOutputTooltip", true)]
        public ICrypToolStream StreamOutput
        {
            get
            {
                Progress(0.5, 1.0);
                Thread t = new Thread(new ThreadStart(GetClipboardData));
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                t.Join();

                if (Data != null && Data != string.Empty)
                {
                    ICrypToolStream CrypToolStream = null;
                    switch (settings.Format)
                    { //0="Text", 1="Hex", 2="Base64"
                        case 1:
                            GuiLogMessage("converting " + Data.Length + " bytes of clipboard data from hex...", NotificationLevel.Debug);
                            CrypToolStream = Hex2Stream(Data);
                            break;
                        case 2:
                            GuiLogMessage("converting " + Data.Length + " bytes of clipboard data from base64...", NotificationLevel.Debug);
                            CrypToolStream = Base642Stream(Data);
                            break;
                        default:
                            GuiLogMessage("converting " + Data.Length + " bytes of clipboard data from text...", NotificationLevel.Debug);
                            CrypToolStream = Text2Stream(Data);
                            break;
                    }
                    Progress(1.0, 1.0);
                    return CrypToolStream;
                }
                else
                {
                    GuiLogMessage("clipboard did not contain any text data", NotificationLevel.Error);
                    return null;
                }
            }
            set
            {
            }
        }
        #endregion

        private ICrypToolStream Text2Stream(string data)
        {
            return new CStreamWriter(Encoding.UTF8.GetBytes(data));
        }

        private ICrypToolStream Base642Stream(string data)
        {
            byte[] temp = new byte[0];
            try
            {
                temp = Convert.FromBase64String(data);
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }

            return new CStreamWriter(temp);
        }

        private ICrypToolStream Hex2Stream(string data)
        {
            return new CStreamWriter(ToByteArray(data));
        }

        public byte[] ToByteArray(string HexString)
        {
            byte[] bytes = new byte[(HexString.Length + 1) / 2];

            for (int i = 0; i < bytes.Count(); i++)
            {
                try
                {
                    bytes[i] = Convert.ToByte(HexString.Substring(2 * i, 2), 16);
                }
                catch (Exception exception)
                {
                    GuiLogMessage(exception.Message, NotificationLevel.Error);
                    bytes[i] = 0;
                }
            }

            return bytes;
        }

        #region IPlugin Member
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public System.Windows.Controls.UserControl Presentation => null;

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        public void Stop()
        {

        }

        public void PreExecution()
        {
            Dispose();
        }

        public void PostExecution()
        {
            Dispose();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        private void Progress(double value, double max)
        {
            if (OnPluginProgressChanged != null)
            {
                OnPluginProgressChanged(this, new PluginProgressEventArgs(value, max));
            }
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, logLevel));
            }
        }

        #region IPlugin Members


        public void Execute()
        {
            OnPropertyChanged("StreamOutput");
        }

        #endregion
    }
}
