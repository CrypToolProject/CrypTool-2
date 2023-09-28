/*
   Copyright 2011 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using System.ComponentModel;
using System.Windows.Controls;


namespace CrypTool.Plugins.StreamSplitter
{
    // HOWTO: Change author name, email address, organization and URL.
    [Author("Mostafa Dahshan", "mdahshan@ksu.edu.sa", "King Saud University", "http://fac.ksu.edu.sa/mdahshan")]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("StreamSplitter.Properties.Resources", "StreamSplitter", "Split a binary stream into two parts", "StreamSplitter/userdoc.xml", new[] { "StreamSplitter/icons/streamsplitter.png" })]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class StreamSplitter : ICrypComponent
    {
        #region Private Variables

        private CStreamWriter outputPart1, outputPart2;
        private const int BUFFSIZE = 1024;
        private int _offset;
        private bool _offsetChanged;


        // HOWTO: You need to adapt the settings class as well, see the corresponding file.
        private readonly StreamSplitterSettings settings = new StreamSplitterSettings();

        #endregion

        #region Data Properties

        /// <summary>
        /// HOWTO: Input interface to read the input data. 
        /// You can add more input properties of other type if needed.
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputStreamCaption", "InputStreamTooltip")]
        public ICrypToolStream InputStream
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "OffsetCaption", "OffsetTooltip", false)]
        public int Offset
        {
            get => _offset;
            set
            {
                if (_offset != value)
                {
                    _offset = value;
                    // HOWTO: MUST be called every time a property value changes with correct parameter name
                    OnPropertyChanged("Offset");
                    _offsetChanged = true;
                }
            }
        }

        /// <summary>
        /// HOWTO: Output interface to write the output data.
        /// You can add more output properties ot other type if needed.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputStream1Caption", "OutputStream1Tooltip")]
        public ICrypToolStream OutputStream1 => outputPart1;

        [PropertyInfo(Direction.OutputData, "OutputStream2Caption", "OutputStream2Tooltip")]
        public ICrypToolStream OutputStream2 => outputPart2;

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings => settings;

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation => null;

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            _offset = 0;
            _offsetChanged = false;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            // HOWTO: Use this to show the progress of a plugin algorithm execution in the editor.
            ProgressChanged(0, 1);

            // HOWTO: After you have changed an output property, make sure you announce the name of the changed property to the CT2 core.

            // HOWTO: You can pass error, warning, info or debug messages to the CT2 main window.
            //if (settings.Offset < 0)
            //GuiLogMessage("Offset is negative, seeking from the end", NotificationLevel.Debug);

            if (InputStream == null)
            {
                return;
            }

            using (CStreamReader reader = InputStream.CreateReader())
            {
                outputPart1 = new CStreamWriter();
                outputPart2 = new CStreamWriter();

                int bytesRead;
                long part1Remaining;
                byte[] buffer = new byte[BUFFSIZE];

                if (!_offsetChanged)
                {
                    _offset = settings.Offset;
                }

                if (Offset < 0) //Seek from the end
                {
                    //Find the input stream size
                    reader.WaitEof();
                    part1Remaining = reader.Length + Offset;
                }

                else
                {
                    part1Remaining = Offset;
                }

                //Remaining bytes are larger than the buffer size, write full buffer-sized chunks
                while (part1Remaining > buffer.Length)
                {
                    if ((bytesRead = reader.Read(buffer)) > 0)
                    {
                        outputPart1.Write(buffer, 0, bytesRead);
                        part1Remaining -= bytesRead;
                    }
                }

                //Write the remaining bytes in the last chunk
                if ((bytesRead = reader.Read(buffer, 0, (int)part1Remaining)) > 0)
                {
                    outputPart1.Write(buffer, 0, bytesRead);
                    //GuiLogMessage("Written "+ bytesRead.ToString()+ " byte(s) to OutputStream1", NotificationLevel.Debug);
                }

                outputPart1.Close();

                // HOWTO: After you have changed an output property, make sure you announce the name of the changed property to the CT2 core.
                OnPropertyChanged("OutputStream1");

                //Write whatever left to the second part
                while ((bytesRead = reader.Read(buffer)) > 0)
                {
                    outputPart2.Write(buffer, 0, bytesRead);
                }

                outputPart2.Close();

                // HOWTO: After you have changed an output property, make sure you announce the name of the changed property to the CT2 core.
                OnPropertyChanged("OutputStream2");

            }


            // HOWTO: Make sure the progress bar is at maximum when your Execute() finished successfully.
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
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
            _offset = 0;
            _offsetChanged = false;
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
            if (outputPart1 != null)
            {
                outputPart1.Dispose();
                outputPart1 = null;
            }

            if (outputPart2 != null)
            {
                outputPart2.Dispose();
                outputPart2 = null;
            }

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
}