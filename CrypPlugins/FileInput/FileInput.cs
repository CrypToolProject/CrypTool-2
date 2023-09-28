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
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace FileInput
{
    [Author("Julian Weyers", "julian.weyers@stud.uni-duisburg-essen.de", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("FileInput.Properties.Resources", "PluginCaption", "PluginTooltip",
        "FileInput/DetailedDescription/doc.xml", "FileInput/FileInput.png")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class FileInputClass : ICrypComponent
    {
        #region Private variables

        private readonly FileInputWPFPresentation fileInputPresentation;
        private CStreamWriter cstreamWriter;
        private FileInputSettings settings;

        #endregion

        public FileInputClass()
        {
            settings = new FileInputSettings();
            settings.PropertyChanged += settings_PropertyChanged;
            fileInputPresentation = new FileInputWPFPresentation(this);
            fileInputPresentation.OnGuiLogNotificationOccured += OnGuiLogNotificationOccured;

            Presentation = fileInputPresentation;
        }

        #region ICrypComponent Members

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public void getMessage(string message)
        {
            GuiLogMessage(message, NotificationLevel.Debug);
        }

        public UserControl Presentation { get; private set; }

        public void Initialize()
        {
            fileInputPresentation.CloseFile();
            settings.SettingChanged("CloseFile", Visibility.Hidden);
            if (File.Exists(settings.OpenFilename))
            {
                fileInputPresentation.OpenFile(settings.OpenFilename);
                NotifyPropertyChange();
                settings.SettingChanged("CloseFile", Visibility.Visible);
            }
        }

        /// <summary>
        /// Close open file. Will be called when deleting an element instance from workspace.
        /// </summary>
        public void Dispose()
        {
            if (cstreamWriter != null)
            {
                cstreamWriter.Dispose();
                cstreamWriter = null;
            }
            fileInputPresentation.CloseFileToGetFileStreamForExecution();
            fileInputPresentation.dispose();
        }

        public void Stop()
        {
        }

        public void PreExecution()
        {

        }

        public void PostExecution()
        {
            fileInputPresentation.makeUnaccesAble(true);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Execute()
        {
            if (string.IsNullOrWhiteSpace(settings.OpenFilename))
            {
                GuiLogMessage("No input file selected, can't proceed", NotificationLevel.Error);
                return;
            }

            try
            {
                cstreamWriter = new CStreamWriter(settings.OpenFilename, true);
                NotifyPropertyChange();
                fileInputPresentation.makeUnaccesAble(false);
            }
            catch (FileNotFoundException)
            {
                GuiLogMessage(string.Format("File not found: '{0}'", settings.OpenFilename), NotificationLevel.Error);
            }
        }

        #endregion

        private void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "OpenFilename")
            {
                string fileName = settings.OpenFilename;

                if (File.Exists(fileName))
                {
                    fileInputPresentation.CloseFile();
                    fileInputPresentation.OpenFile(settings.OpenFilename);
                    FileSize = (int)new FileInfo(fileName).Length;
                    GuiLogMessage("Opened file: " + settings.OpenFilename, NotificationLevel.Info);
                    settings.SettingChanged("CloseFile", Visibility.Visible);
                }
                else if (e.PropertyName == "OpenFilename" && fileName == null)
                {
                    fileInputPresentation.CloseFile();
                    FileSize = 0;
                    settings.SettingChanged("CloseFile", Visibility.Collapsed);
                }
                NotifyPropertyChange();
            }

            if (e.PropertyName == "CloseFile")
            {
                fileInputPresentation.CloseFile();
                fileInputPresentation.dispose();
            }
        }

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #region methods

        public void NotifyPropertyChange()
        {
            OnPropertyChanged("StreamOutput");
            OnPropertyChanged("FileSize");
        }


        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        #endregion

        #region Properties

        [PropertyInfo(Direction.OutputData, "StreamOutputCaption", "StreamOutputTooltip", true)]
        public ICrypToolStream StreamOutput
        {
            get => cstreamWriter;
            set { } // readonly
        }

        [PropertyInfo(Direction.OutputData, "FileSizeCaption", "FileSizeTooltip")]
        public int FileSize { get; private set; }

        public ISettings Settings
        {
            get => settings;
            set => settings = (FileInputSettings)value;
        }

        #endregion
    }
}