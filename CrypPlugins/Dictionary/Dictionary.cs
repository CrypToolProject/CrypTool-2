/*
   Copyright 2008 Thomas Schmid, University of Siegen

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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace Dictionary
{
    [Author("Thomas Schmid, Matthäus Wander", "thomas.schmid@CrypTool.org", "Uni Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("Dictionary.Properties.Resources", "PluginCaption", "PluginTooltip", "Dictionary/DetailedDescription/doc.xml", "Dictionary/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class CrypToolDictionary : ICrypComponent
    {
        #region private_variables

        private const string DATATYPE = "wordlists";

        private readonly CrypToolDictionarySettings settings = new CrypToolDictionarySettings();

        // dictionary name -> collection of words
        private readonly Dictionary<DataFileMetaInfo, string[]> dicValues = new Dictionary<DataFileMetaInfo, string[]>();
        private readonly Dictionary<DataFileMetaInfo, string> dicValuesOld = new Dictionary<DataFileMetaInfo, string>();

        // list of dictionaries
        private DataFileMetaInfo[] dicList;

        // Manages wordlist files
        private readonly DataManager dataMgr = new DataManager();

        // Flag to enable re-execution during play mode
        private bool allowReexecution = false;

        # endregion private_variables

        public CrypToolDictionary()
        {
            LoadFileList();
        }

        public DataFileMetaInfo CurrentDicSelection
        {
            get
            {
                if (dicList != null && settings.Dictionary >= 0 && settings.Dictionary < dicList.Length)
                {
                    return dicList[settings.Dictionary];
                }
                else
                {
                    return null;
                }
            }
        }

        [Obsolete("Use string[] output instead")]
        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", false)]
        public string OutputString
        {
            get
            {
                if (OutputList != null)
                {
                    Debug.Assert(CurrentDicSelection != null);
                    if (!dicValuesOld.ContainsKey(CurrentDicSelection))
                    {
                        dicValuesOld.Add(CurrentDicSelection, string.Join(" ", OutputList));
                    }
                    return dicValuesOld[CurrentDicSelection];
                }
                return null;
            }
            set { } // readonly
        }

        [PropertyInfo(Direction.OutputData, "OutputListCaption", "OutputListTooltip", false)]
        public string[] OutputList
        {
            get
            {
                if (CurrentDicSelection != null)
                {
                    if (dicValues.ContainsKey(CurrentDicSelection) || LoadDictionary(CurrentDicSelection))
                    {
                        return dicValues[CurrentDicSelection];
                    }
                }
                return null;
            }
            set { } // readonly
        }

        #region IPlugin Members

#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void Progress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public ISettings Settings => settings;

        public UserControl Presentation => null;

        public void PreExecution()
        {
        }

        public void Execute()
        {
            if (CurrentDicSelection == null)
            {
                GuiLogMessage("No dictionary chosen.", NotificationLevel.Error);
                return;
            }

            Progress(50, 100);
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs("OutputList"));
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs("OutputString"));
            Progress(100, 100);

            // enable re-execution after the first run
            allowReexecution = true;
        }

        public void PostExecution()
        {
            // disable re-execution when leaving play mode
            allowReexecution = false;
        }

        public void Stop()
        {
        }

        /// <summary>
        /// Loads dictionary file based on current setting.
        /// </summary>
        /// <returns>true if file has been loaded correctly</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private bool LoadDictionary(DataFileMetaInfo file)
        {
            // sanity check for multi-threading
            if (dicValues.ContainsKey(file))
            {
                return true;
            }

            try
            {
                if (file.DataFile.Exists)
                {
                    GuiLogMessage(string.Format(Properties.Resources.loading_dic_now, new object[] { file.Name }), NotificationLevel.Info);
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    using (FileStream fs = file.DataFile.OpenRead())
                    {
                        if (file.TextEncoding == null)
                        {
                            using (StreamReader sr = new StreamReader(fs))
                            {
                                dicValues.Add(file, sr.ReadToEnd().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
                            }
                        }
                        else
                        {
                            using (StreamReader sr = new StreamReader(fs, file.TextEncoding))
                            {
                                dicValues.Add(file, sr.ReadToEnd().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
                            }
                        }
                    }

                    stopWatch.Stop();
                    // This log msg is shown on init after first using this plugin, even if event subscription
                    // should not have been done yet. Results from using static LoadContent method.
                    GuiLogMessage(string.Format(Properties.Resources.finished_loading_dic, new object[] { stopWatch.Elapsed.Milliseconds }), NotificationLevel.Info);

                    return true;
                }
                else
                {
                    GuiLogMessage(string.Format(Properties.Resources.dic_file_not_found, new object[] { file.ToString() }), NotificationLevel.Error);
                }
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }

            return false;
        }

        private void setNumberOfEntires()
        {
            if (CurrentDicSelection != null)
            {
                if (dicValues.ContainsKey(CurrentDicSelection) || LoadDictionary(CurrentDicSelection))
                {
                    int n = dicValues[CurrentDicSelection].Length;
                    settings.NumberEntries = (n > 0) ? n.ToString("#,#") : n.ToString();
                }
                else
                {
                    settings.NumberEntries = "?";
                }
            }
            else
            {
                settings.NumberEntries = "?";
            }
        }

        public void Initialize()
        {
            settings.PropertyChanged += SettingsPropertyChanged; // catch settings changes
            setNumberOfEntires();
        }

        private void SettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            setNumberOfEntires();

            // if user chooses another dictionary, force re-execution
            if (allowReexecution && e.PropertyName == "Dictionary")
            {
                Execute();
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void LoadFileList()
        {
            dicList = dataMgr.LoadDirectory(DATATYPE).Values.ToArray();

            if (settings.Collection.Count > 0)
            {
                settings.Collection.Clear();
            }

            foreach (DataFileMetaInfo meta in dicList)
            {
                settings.Collection.Add(this.GetPluginStringResource(meta.Name));
            }
        }

        public void Dispose()
        {
            dicValues.Clear();
            dicValuesOld.Clear();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
