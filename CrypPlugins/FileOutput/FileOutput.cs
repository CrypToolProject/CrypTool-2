/*
   Copyright 2012 Julian Weyes, University Duisburg-Essen

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
using FileOutputWPF;
using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace FileOutput
{
    [Author("Julian Weyers", "julian.weyers@uni-due.de", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("FileOutput.Properties.Resources", "PluginCaption", "PluginTooltip",
        "FileOutput/DetailedDescription/doc.xml", "FileOutput/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class FileOutputClass : ICrypComponent
    {
        #region Private variables    

        public FileOutputSettings settings;

        #endregion Private variables

        private readonly FileOutputWPFPresentation fileOutputPresentation;

        public FileOutputClass()
        {
            settings = new FileOutputSettings();
            fileOutputPresentation = new FileOutputWPFPresentation(this);
            Presentation = fileOutputPresentation;
        }

        public string InputFile { get; set; }

        #region ICrypComponent Members

        public ISettings Settings
        {
            get => settings;
            set => settings = (FileOutputSettings)value;
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public UserControl Presentation { get; private set; }

        public void Initialize()
        {
            if (settings.SaveAndRestoreState != string.Empty && File.Exists(settings.SaveAndRestoreState))
            {
                InputFile = settings.SaveAndRestoreState;
                fileOutputPresentation.OpenFile(settings.TargetFilename);
            }
        }

        /// <summary>
        /// Close open file and save open filename to settings. Will be called when saving
        /// workspace or when deleting an element instance from workspace.
        /// </summary>
        public void Dispose()
        {
            fileOutputPresentation.CloseFileToGetFileStreamForExecution();
            fileOutputPresentation.dispose();
        }

        public void Stop()
        {

            fileOutputPresentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    fileOutputPresentation.Clear();
                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format("Exception during Clear of HexBox: {0}", ex.Message), NotificationLevel.Error);
                }
            }, DispatcherPriority.Normal);
        }

        public void PreExecution()
        {
            InputFile = null;
        }

        public void PostExecution()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Execute()
        {
            Progress(0.0, 1.0);

            if (string.IsNullOrEmpty(settings.TargetFilename))
            {
                GuiLogMessage("You have to select a target filename before using this plugin as output.",
                              NotificationLevel.Error);
                return;
            }

            if (StreamInput == null)
            {
                GuiLogMessage("Received null value for ICrypToolStream.", NotificationLevel.Warning);
                return;
            }

            Progress(0.5, 1.0);

            using (CStreamReader reader = StreamInput.CreateReader())
            {
                // If target file was selected we have to copy the input to target. 

                # region copyToTarget

                if (settings.TargetFilename != null)
                {
                    InputFile = settings.TargetFilename;
                    try
                    {
                        fileOutputPresentation.Dispatcher.Invoke(DispatcherPriority.Normal,
                        (SendOrPostCallback)
                        delegate
                        {
                            fileOutputPresentation.
                                CloseFileToGetFileStreamForExecution();
                        },
                        null);

                        FileStream fs;
                        if (!settings.Append)
                        {
                            fs = new FileStream(settings.TargetFilename, FileMode.Create);
                        }
                        else
                        {
                            fs = new FileStream(settings.TargetFilename, FileMode.Append);
                            for (int i = 0; i < settings.AppendBreaks; i++)
                            {
                                const string nl = "\n";
                                fs.Write(Encoding.ASCII.GetBytes(nl), 0, Encoding.ASCII.GetByteCount(nl));
                            }
                        }

                        byte[] byteValues = new byte[1024];
                        int byteRead;

                        long position = fs.Position;
                        GuiLogMessage("Start writing to target file now: " + settings.TargetFilename,
                                      NotificationLevel.Debug);
                        while ((byteRead = reader.Read(byteValues, 0, byteValues.Length)) != 0)
                        {
                            fs.Write(byteValues, 0, byteRead);
                            if (OnPluginProgressChanged != null && reader.Length > 0 &&
                                (int)(reader.Position * 100 / reader.Length) > position)
                            {
                                position = (int)(reader.Position * 100 / reader.Length);
                                Progress(reader.Position, reader.Length);
                            }
                        }
                        fs.Flush();
                        fs.Close();

                        GuiLogMessage("Finished writing: " + settings.TargetFilename, NotificationLevel.Debug);
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage(ex.Message, NotificationLevel.Error);
                        settings.TargetFilename = null;
                    }
                }

                # endregion copyToTarget

                fileOutputPresentation.Dispatcher.Invoke(DispatcherPriority.Normal,
                                                         (SendOrPostCallback)
                                                         delegate { fileOutputPresentation.ReopenClosedFile(); }, null);
                Progress(1.0, 1.0);
            }
        }

        #endregion



        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public void UpdateQuickWatch()
        {
            OnPropertyChanged("StreamInput");
        }

        private void Progress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public void getMessage(string message)
        {
            GuiLogMessage(message, NotificationLevel.Debug);
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        # region Properties

        [PropertyInfo(Direction.InputData, "StreamInputCaption", "StreamInputTooltip", true)]
        public ICrypToolStream StreamInput { get; set; }

        #endregion
    }
}