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

// CrypTool ClipboardOutput Plugin
// Author: Timm Korte, CrypTool@easycrypt.de

using CrypTool.PluginBase;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.ComponentModel;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ClipboardOutput
{
    // Converts a stream to given format and writes the results to the clipboard
    [Author("Timm Korte", "CrypTool@easycrypt.de", "Uni Bochum", "http://www.ruhr-uni-bochum.de")]
    [PluginInfo("ClipboardOutput.Properties.Resources", "PluginCaption", "PluginTooltip", "ClipboardOutput /DetailedDescription/doc.xml", "ClipboardOutput/icon.png")]
    [Synchronization(SynchronizationAttribute.REQUIRES_NEW)]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class ClipboardOutput : ICrypComponent
    {

        #region Private Variables
        private ClipboardOutputSettings settings;

        public ISettings Settings {
            get => settings;
            set => settings = (ClipboardOutputSettings)value;
        }

        #endregion

        public ClipboardOutput()
        {
            settings = new ClipboardOutputSettings();
        }

        private string data;
        public string Data
    {
      get => data;
      set => data = value;
    }

        private void SetClipboardData()
        {
            try
            {
                Clipboard.SetText(Data);
                OnPropertyChanged("StreamInput");
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
        }

        #region Interface
        private ICrypToolStream streamInput;
        [PropertyInfo(Direction.InputData, "StreamInputCaption", "StreamInputTooltip", true)]
        public ICrypToolStream StreamInput
        {
            get
            {
                if (Data != null && Data != string.Empty)
                {
                    return new CStreamWriter(Encoding.UTF8.GetBytes(Data));
                }
                return null;
            }
            set
            {
                Progress(0.5, 1.0);
                if (value != null)
                {
                    streamInput = value;
                    switch (settings.Format)
                    { //0="Text", 1="Hex", 2="Base64"
                        case 1:
                            Data = Stream2Hex(value);
                            GuiLogMessage("converting input data to hex...", NotificationLevel.Debug);
                            break;
                        case 2:
                            Data = Stream2Base64(value);
                            GuiLogMessage("converting input data to base64...", NotificationLevel.Debug);
                            break;
                        default:
                            Data = Stream2Text(value);
                            GuiLogMessage("converting input data to text...", NotificationLevel.Debug);
                            break;
                    }

                    Thread t = new Thread(new ThreadStart(SetClipboardData));
                    t.SetApartmentState(ApartmentState.STA);
                    t.Start();
                    t.Join();

                    GuiLogMessage("Wrote " + data.Length + " characters to clipboard", NotificationLevel.Info);
                    Progress(1.0, 1.0);
                }
                else
                {
                    GuiLogMessage("ERROR - no input data recieved", NotificationLevel.Error);
                    Progress(1.0, 1.0);
                }
            }
        }

        private string Stream2Text(ICrypToolStream value)
        {
            using (CStreamReader reader = value.CreateReader())
            {
                int byteValue;
                StringBuilder sb = new StringBuilder();
                while ((byteValue = reader.ReadByte()) != -1)
                {
                    // FIXME: UTF-8 characters may consist of more than a single byte
                    sb.Append(System.Convert.ToChar(byteValue));
                }
                return sb.ToString();
            }
        }

        private string Stream2Base64(ICrypToolStream value)
        {
            using (CStreamReader reader = value.CreateReader())
            {
                reader.WaitEof();
                byte[] byteValues = new byte[reader.Length];
                reader.Read(byteValues, 0, (int)reader.Length);
                return Convert.ToBase64String(byteValues);
            }
        }

        private string Stream2Hex(ICrypToolStream value)
        {
            using (CStreamReader reader = value.CreateReader())
            {
                int byteValue;
                StringBuilder sb = new StringBuilder();
                while ((byteValue = reader.ReadByte()) != -1)
                {
                    sb.Append(byteValue.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        #endregion

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

        }

        public void PostExecution()
        {
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion

        private void Progress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        #region IPlugin Members

        public void Execute()
        {

        }

        #endregion
    }
}
